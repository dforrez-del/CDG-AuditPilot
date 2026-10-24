using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CDG.AuditPilot.Application;
using Microsoft.Win32;
namespace CDG.AuditPilot.Desktop;

public sealed class QuestionnaireLibraryWindow : Window
{
    private readonly string folder = Path.Combine(App.DataDirectory, "Questionnaires");
    private readonly ListBox library = new() { MinWidth = 190 };
    private readonly DataGrid sections = new() { AutoGenerateColumns = false, CanUserAddRows = false, MinHeight = 100, MaxHeight = 160 };
    private readonly DataGrid questions = new() { AutoGenerateColumns = false, CanUserAddRows = false, MinHeight = 200, RowHeight = double.NaN };
    private readonly TextBox title = new();
    private readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 0) };
    private QuestionnaireDraft draft = new();
    private string baseline = "";
    private bool loading;
    private sealed record LibraryEntry(FileInfo File, string Name);
    private LibraryEntry? selected;
    private static readonly KeyValuePair<string,string>[] Types = [new("text","Texte court"),new("longText","Texte long"),new("yesNo","Oui / Non"),new("singleChoice","Choix unique"),new("multipleChoice","Choix multiples"),new("scale0to5","Note de 0 à 5"),new("scale0to10","Note de 0 à 10"),new("number","Nombre"),new("date","Date")];

    public AuditQuestionnaire? SelectedQuestionnaire { get; private set; }
    public QuestionnaireLibraryWindow(bool chooseForInterview = false)
    {
        Title = "Bibliothèque de questionnaires"; Width = 1200; Height = 820; MinWidth = 900; MinHeight = 650;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Directory.CreateDirectory(folder);
        var root = new DockPanel { Margin = new Thickness(20) }; Content = root;
        DockPanel.SetDock(status, Dock.Bottom); root.Children.Add(status);
        var bar = new WrapPanel(); DockPanel.SetDock(bar, Dock.Top); root.Children.Add(bar);
        Add(bar,"Nouveau",New); Add(bar,"Importer JSON",Import); Add(bar,"Enregistrer une version",SaveVersion);
        Add(bar,"Exporter JSON",Export); Add(bar,"Modèles initiaux",Templates);
        if(chooseForInterview)Add(bar,"Enregistrer et utiliser",()=>{SaveVersion();SelectedQuestionnaire=draft.Build();DialogResult=true;});
        var layout = new Grid(); layout.ColumnDefinitions.Add(new() { Width = new GridLength(220) }); layout.ColumnDefinitions.Add(new()); root.Children.Add(layout);
        var versionLabel=new FrameworkElementFactory(typeof(TextBlock));versionLabel.SetBinding(TextBlock.TextProperty,new Binding("Name"));versionLabel.SetValue(TextBlock.TextWrappingProperty,TextWrapping.Wrap);versionLabel.SetValue(TextBlock.MarginProperty,new Thickness(4,8,4,8));
        library.ItemTemplate=new DataTemplate{VisualTree=versionLabel};
        library.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty,ScrollBarVisibility.Disabled);
        var itemStyle=new Style(typeof(ListBoxItem));itemStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty,HorizontalAlignment.Stretch));library.ItemContainerStyle=itemStyle;
        library.Margin = new Thickness(0,12,16,0); layout.Children.Add(library);
        var body = new DockPanel { Margin = new Thickness(0,12,0,0) }; Grid.SetColumn(body,1); layout.Children.Add(body);
        var top = new StackPanel(); DockPanel.SetDock(top,Dock.Top); body.Children.Add(top);
        top.Children.Add(new TextBlock { Text="Titre du questionnaire", FontWeight=FontWeights.SemiBold }); top.Children.Add(title);
        top.Children.Add(new TextBlock { Text="Sections — sélectionnez une section pour modifier ses questions", Margin=new Thickness(0,12,0,6) });
        sections.Columns.Add(TextColumn("Intitulé", "Title", 1, true)); top.Children.Add(sections);
        var sectionBar=new WrapPanel(); top.Children.Add(sectionBar);
        Add(sectionBar,"Ajouter une section",()=>{Commit();var s=new SectionDraft();s.Questions.Add(new());draft.Sections.Add(s);sections.SelectedItem=s;});
        Add(sectionBar,"Supprimer la section",()=>{if(sections.SelectedItem is SectionDraft s&&Confirm("Supprimer cette section et ses questions du brouillon ?")){draft.Sections.Remove(s);sections.SelectedItem=draft.Sections.FirstOrDefault();}});
        Add(sectionBar,"↑ Section",()=>MoveSection(-1));Add(sectionBar,"↓ Section",()=>MoveSection(1));
        top.Children.Add(new TextBlock { Text="Questions — pour les choix, saisissez une option par ligne (Entrée dans la cellule).", TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,10,0,6) });
        var questionBar=new WrapPanel(); top.Children.Add(questionBar);
        Add(questionBar,"Ajouter une question",()=>{Commit();if(sections.SelectedItem is SectionDraft s){var q=new QuestionDraft();s.Questions.Add(q);questions.SelectedItem=q;questions.ScrollIntoView(q);}});
        Add(questionBar,"Supprimer la question",()=>{if(sections.SelectedItem is SectionDraft s&&questions.SelectedItem is QuestionDraft q&&Confirm("Supprimer cette question du brouillon ?"))s.Questions.Remove(q);});
        Add(questionBar,"↑ Question",()=>MoveQuestion(-1));Add(questionBar,"↓ Question",()=>MoveQuestion(1));
        questions.Columns.Add(TextColumn("Question","Label",270));
        questions.Columns.Add(new DataGridComboBoxColumn{Header="Réponse attendue",ItemsSource=Types,SelectedValuePath="Key",DisplayMemberPath="Value",SelectedValueBinding=new Binding("Type"){UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},Width=150});
        questions.Columns.Add(new DataGridCheckBoxColumn{Header="Obligatoire",Binding=new Binding("Required"),Width=90});
        var options=TextColumn("Choix (une option par ligne)","OptionsText",230);
        var editStyle=new Style(typeof(TextBox)); editStyle.Setters.Add(new Setter(TextBox.AcceptsReturnProperty,true)); editStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty,TextWrapping.Wrap)); options.EditingElementStyle=editStyle; questions.Columns.Add(options);
        body.Children.Add(questions);
        sections.SelectionChanged+=(_,_)=>{questions.ItemsSource=(sections.SelectedItem as SectionDraft)?.Questions;};
        library.SelectionChanged+=(_,_)=>
        {
            if(loading||library.SelectedItem is not LibraryEntry entry)return;
            if(!MayDiscard()){loading=true;library.SelectedItem=selected;loading=false;return;}
            try{Load(Read(entry.File.FullName));selected=entry;status.Text="Version chargée. Enregistrer crée un nouveau fichier ; les entretiens existants restent inchangés.";}
            catch(Exception ex){ShowError(ex);loading=true;library.SelectedItem=selected;loading=false;}
        };
        Closing+=(_,e)=>{if(!MayDiscard())e.Cancel=true;};
        Refresh();NewDraft();
    }
    private static DataGridTextColumn TextColumn(string caption,string property,double width,bool star=false)
    {
        var style=new Style(typeof(TextBlock));style.Setters.Add(new Setter(TextBlock.TextWrappingProperty,TextWrapping.Wrap));style.Setters.Add(new Setter(TextBlock.MarginProperty,new Thickness(6)));
        return new(){Header=caption,Binding=new Binding(property){UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged},Width=new DataGridLength(width,star?DataGridLengthUnitType.Star:DataGridLengthUnitType.Pixel),ElementStyle=style};
    }
    private void Add(Panel panel,string label,Action action){var button=new Button{Content=label};button.Click+=(_,_)=>{try{action();}catch(Exception ex){ShowError(ex);}};panel.Children.Add(button);}
    private void ShowError(Exception ex){App.WriteTechnicalError(ex);MessageBox.Show(this,ex is ArgumentException?ex.Message:"Lecture ou enregistrement impossible. Vérifiez le fichier et les droits d’accès.","Questionnaire");}
    private bool Confirm(string message)=>MessageBox.Show(this,message,"Questionnaire",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes;
    private void Commit(){questions.CommitEdit(DataGridEditingUnit.Cell,true);questions.CommitEdit(DataGridEditingUnit.Row,true);sections.CommitEdit(DataGridEditingUnit.Cell,true);sections.CommitEdit(DataGridEditingUnit.Row,true);draft.Title=title.Text;}
    private string Fingerprint(){Commit();return JsonSerializer.Serialize(draft);}
    private bool MayDiscard()=>Fingerprint()==baseline||Confirm("Abandonner les modifications non enregistrées de ce questionnaire ?");
    private void Load(AuditQuestionnaire questionnaire){draft=QuestionnaireDraft.From(questionnaire);title.Text=draft.Title;sections.ItemsSource=draft.Sections;sections.SelectedItem=draft.Sections.FirstOrDefault();baseline=Fingerprint();}
    private void NewDraft(){Load(new(1,"Nouveau questionnaire",[new("Diagnostic",[new("Q-"+Guid.NewGuid().ToString("N"),"Nouvelle question","text",false,[])])]));selected=null;loading=true;library.SelectedItem=null;loading=false;status.Text="Modifiez les cellules, puis enregistrez une version. Les identifiants des questions existantes sont conservés.";}
    private void New(){if(MayDiscard())NewDraft();}
    private static AuditQuestionnaire Read(string path){if(new FileInfo(path).Length>1_000_000)throw new ArgumentException("1 Mo maximum.");return QuestionnaireFormat.Parse(File.ReadAllText(path));}
    private void Import(){if(!MayDiscard())return;var dialog=new OpenFileDialog{Filter="Questionnaire JSON|*.json"};if(dialog.ShowDialog(this)!=true)return;var q=Read(dialog.FileName);Load(q);selected=null;loading=true;library.SelectedItem=null;loading=false;baseline="";status.Text="Import prêt à modifier. Enregistrez une version pour l’ajouter à la bibliothèque.";}
    private void Refresh()=>library.ItemsSource=new DirectoryInfo(folder).GetFiles("*.json").OrderByDescending(f=>f.CreationTimeUtc).Select(f=>
    {string name;try{name=Read(f.FullName).Title;}catch{name="Fichier à vérifier";}return new LibraryEntry(f,name+"\n"+f.CreationTime.ToString("dd/MM/yyyy HH:mm:ss.fff"));}).ToArray();
    private void SaveVersion(){Commit();var q=draft.Build();var path=Path.Combine(folder,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")+".json");WriteNew(path,q);baseline=Fingerprint();loading=true;Refresh();selected=library.Items.Cast<LibraryEntry>().Single(f=>f.File.FullName==path);library.SelectedItem=selected;loading=false;status.Text="Nouvelle version enregistrée — "+q.Title+". Les versions précédentes sont conservées.";}
    private static void WriteNew(string path,AuditQuestionnaire q)
    {
        var bytes=JsonSerializer.SerializeToUtf8Bytes(q,QuestionnaireFormat.Json);
        if(bytes.Length>1_000_000)throw new ArgumentException("Questionnaire trop volumineux (1 Mo maximum).");
        var temporary=path+".tmp";
        try{using(var stream=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))stream.Write(bytes);File.Move(temporary,path);}
        finally{if(File.Exists(temporary))File.Delete(temporary);}
    }
    private void Export(){Commit();var q=draft.Build();var d=new SaveFileDialog{Filter="Questionnaire JSON|*.json",FileName="questionnaire.json"};if(d.ShowDialog(this)==true)File.WriteAllText(d.FileName,JsonSerializer.Serialize(q,QuestionnaireFormat.Json));}
    private void MoveSection(int offset){Commit();if(sections.SelectedItem is not SectionDraft s)return;var i=draft.Sections.IndexOf(s);if(i+offset>=0&&i+offset<draft.Sections.Count)draft.Sections.Move(i,i+offset);}
    private void MoveQuestion(int offset){Commit();if(sections.SelectedItem is not SectionDraft s||questions.SelectedItem is not QuestionDraft q)return;var i=s.Questions.IndexOf(q);if(i+offset>=0&&i+offset<s.Questions.Count)s.Questions.Move(i,i+offset);}
    private void Templates(){foreach(var name in new[]{"Organisation et processus","Documents et messageries","Systèmes d’information","Flux et contrôle interne"})WriteNew(Path.Combine(folder,Guid.NewGuid().ToString("N")+".json"),new(1,name,[new("Diagnostic",[new("Q1","Décrivez le processus et les intervenants.","longText",true,[]),new("Q2","Quels contrôles et justificatifs sont conservés ?","longText",true,[]),new("Q3","Quels suivis de stocks ou fichiers Excel interviennent ?","longText",false,[]),new("Q4","Quelles difficultés et améliorations identifiez-vous ?","longText",false,[])])]));loading=true;var previous=selected?.File.FullName;Refresh();selected=library.Items.Cast<LibraryEntry>().FirstOrDefault(x=>x.File.FullName==previous);library.SelectedItem=selected;loading=false;status.Text="Quatre modèles ajoutés à la bibliothèque.";}
    internal void SmokeEditAndSave(){draft.Sections[0].Questions[0].Label="Qui valide le contrôle ?";title.Text="Questionnaire conçu visuellement";SaveVersion();var first=selected!.File.FullName;draft.Sections[0].Questions[0].Label="Qui valide et conserve la preuve ?";SaveVersion();if(Read(first).Sections[0].Questions[0].Label!="Qui valide le contrôle ?"||Read(selected!.File.FullName).Sections[0].Questions[0].Label!="Qui valide et conserve la preuve ?")throw new InvalidOperationException("Versions de bibliothèque altérées.");questions.Items.Refresh();}
}
