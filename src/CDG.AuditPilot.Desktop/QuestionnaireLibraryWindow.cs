using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using CDG.AuditPilot.Application;
using Microsoft.Win32;
namespace CDG.AuditPilot.Desktop;
public sealed class QuestionnaireLibraryWindow:Window {
 public QuestionnaireLibraryWindow(){Title="Bibliothèque — versions immuables de questionnaires";Width=1000;Height=720;var folder=Path.Combine(App.DataDirectory,"Questionnaires");Directory.CreateDirectory(folder);var root=new DockPanel{Margin=new Thickness(15)};var bar=new WrapPanel();DockPanel.SetDock(bar,Dock.Top);root.Children.Add(bar);var list=new ListBox{Width=260,DisplayMemberPath="Name"};DockPanel.SetDock(list,Dock.Left);root.Children.Add(list);var editor=new TextBox{AcceptsReturn=true,AcceptsTab=true,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,FontFamily=new System.Windows.Media.FontFamily("Consolas")};root.Children.Add(editor);Content=root;
  void Refresh()=>list.ItemsSource=new DirectoryInfo(folder).GetFiles("*.json").OrderByDescending(f=>f.CreationTimeUtc).ToArray();
  void Add(string title,Action action){var b=new Button{Content=title,Margin=new Thickness(4),Padding=new Thickness(8)};b.Click+=(_,_)=>{try{action();}catch(Exception ex){MessageBox.Show(ex is ArgumentException?ex.Message:"Lecture ou enregistrement impossible.");}};bar.Children.Add(b);}
  list.SelectionChanged+=(_,_)=>{if(list.SelectedItem is FileInfo f)editor.Text=File.ReadAllText(f.FullName);};
  Add("Importer",()=>{var d=new OpenFileDialog{Filter="JSON|*.json"};if(d.ShowDialog(this)!=true)return;if(new FileInfo(d.FileName).Length>1_000_000)throw new ArgumentException("1 Mo maximum.");editor.Text=JsonSerializer.Serialize(QuestionnaireFormat.Parse(File.ReadAllText(d.FileName)),QuestionnaireFormat.Json);});
  Add("Nouvelle version",()=>{var q=QuestionnaireFormat.Parse(editor.Text);var file=Path.Combine(folder,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")+".json");File.WriteAllText(file,JsonSerializer.Serialize(q,QuestionnaireFormat.Json));Refresh();});
  Add("Exporter pour entretien",()=>{var q=QuestionnaireFormat.Parse(editor.Text);var d=new SaveFileDialog{Filter="JSON|*.json",FileName="questionnaire.json"};if(d.ShowDialog(this)==true)File.WriteAllText(d.FileName,JsonSerializer.Serialize(q,QuestionnaireFormat.Json));});
  Add("Modèles initiaux",()=>{foreach(var domain in new[]{"Organisation et processus","Documents et messageries","Systèmes d’information","Flux et contrôle interne"}){var q=new AuditQuestionnaire(1,domain,[new AuditSection("Diagnostic",[new("Q1","Décrivez le processus et les intervenants.","longText",true,[]),new("Q2","Quels contrôles et justificatifs sont conservés ?","longText",true,[]),new("Q3","Quels suivis de stocks ou fichiers Excel interviennent ?","longText",false,[]),new("Q4","Quelles difficultés et améliorations identifiez-vous ?","longText",false,[])])]);File.WriteAllText(Path.Combine(folder,Guid.NewGuid().ToString("N")+".json"),JsonSerializer.Serialize(q,QuestionnaireFormat.Json));}Refresh();});Refresh();
 }
}
