using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using CDG.AuditPilot.AI;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using Microsoft.Win32;
namespace CDG.AuditPilot.Desktop;
public partial class QuestionnaireWindow : Window
{
 private readonly QuestionnaireViewModel vm = new();
 private readonly IQuestionnaireStore store;
 private readonly Mission mission;
 private InterviewSession? current;
 private bool dirty;
 private bool changing;
 private readonly System.Windows.Threading.DispatcherTimer autosave=new(){Interval=TimeSpan.FromSeconds(3)};
 private void MetadataChanged(object sender,TextChangedEventArgs e){if(current is not null&&!changing)dirty=true;}

 public QuestionnaireWindow(Mission mission, IQuestionnaireStore store)
 {
  InitializeComponent(); this.mission = mission; this.store = store; DataContext = vm;
  Title = $"{mission.Reference} — Questionnaires et analyse";
  ApiKey.Password=SecretStore.Load("secrets");
  vm.PropertyChanged+=(_,e)=>{if(e.PropertyName==nameof(vm.Result)&&current is not null&&!changing)dirty=true;};
  autosave.Tick+=async (_,_)=>{if(!dirty||vm.Busy||current is null)return;vm.Busy=true;try{await SaveCurrentAsync(Snapshot());vm.Status="Sauvegarde automatique effectuée.";}catch(Exception ex){Error(ex);}finally{vm.Busy=false;}};autosave.Start();
  Loaded += async (_, _) => { try { foreach (var s in await Task.Run(()=>store.ListAsync(mission.Id))) vm.Sessions.Add(s); } catch (Exception ex) { Error(ex); } };
  Closing += (_, e) => { if (vm.Busy || !MayDiscard()) e.Cancel = true; };
  Closed += (_, _) => {autosave.Stop();ApiKey.Clear();};
 }
 private bool MayDiscard() => !dirty || MessageBox.Show(this, "Abandonner les modifications non enregistrées ?", "Questionnaire", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
 private InterviewSession Snapshot()
 {
  if (current is null) throw new ArgumentException("Sélectionnez un questionnaire.");
  var analyses=current.Analyses.ToList();
  if(analyses.Count>0&&analyses[^1].Response!=vm.Result)analyses[^1]=analyses[^1] with{Response=vm.Result,Status="Proposition révisée manuellement — non validée"};
  var snapshot = current with {Interviewee=Interviewee.Text,Observations=Observations.Text,Comments=vm.Answers.ToDictionary(x=>x.Question.Id,x=>x.Comment),NotApplicable=vm.Answers.Where(x=>x.NotApplicable).Select(x=>x.Question.Id).ToHashSet(), Answers = vm.Answers.ToDictionary(x => x.Question.Id, x => x.NotApplicable?"":x.Value.Trim()), Analyses = analyses };
  QuestionnaireFormat.ValidateSession(snapshot); return snapshot;
 }
 private void Session_Changed(object sender, SelectionChangedEventArgs e)
 {
  if (changing) return;
  if (!MayDiscard()) { changing = true; SessionList.SelectedItem = current; changing = false; return; }
  current = SessionList.SelectedItem as InterviewSession; vm.Answers.Clear(); dirty = false;
  if (current is null) return;
  changing=true;Interviewee.Text=current.Interviewee;Observations.Text=current.Observations;
  foreach (var section in current.Questionnaire.Sections)
   foreach (var question in section.Questions)
   {
    var answer = new QuestionAnswerViewModel(question, section.Title, current.Answers.GetValueOrDefault(question.Id, ""));
    answer.Comment=current.Comments.GetValueOrDefault(question.Id,"");answer.NotApplicable=current.NotApplicable.Contains(question.Id);
    answer.PropertyChanged += (_, _) => dirty = true; vm.Answers.Add(answer);
   }
  vm.Result = current.Analyses.LastOrDefault()?.Response ?? "";changing=false;dirty=false;
 }
 private async void Import_Click(object sender, RoutedEventArgs e)
 {
  if (!MayDiscard()) return;
  var dialog = new OpenFileDialog { Filter = "Questionnaire AuditPilot (*.json)|*.json" };
  if (dialog.ShowDialog(this) != true) return;
  vm.Busy = true;
  try
  {
   if (new FileInfo(dialog.FileName).Length > 1_000_000) throw new ArgumentException("Fichier trop volumineux (1 Mo maximum).");
   var questionnaire = QuestionnaireFormat.Parse(await File.ReadAllTextAsync(dialog.FileName));
   var session = new InterviewSession(Guid.NewGuid(), mission.Id, questionnaire, new(), []);
   await Task.Run(()=>store.SaveAsync(session));
   dirty = false; vm.Sessions.Add(session); SessionList.SelectedItem = session;
   vm.Status = "Questionnaire importé et conservé dans la mission.";
  }
  catch (Exception ex) { Error(ex); }
  finally { vm.Busy = false; }
 }
 private async Task SaveCurrentAsync(InterviewSession snapshot)
 {
  await Task.Run(()=>store.SaveAsync(snapshot));
  var index = vm.Sessions.IndexOf(current!); changing = true;
  if (index >= 0) vm.Sessions[index] = snapshot;
  current = snapshot; SessionList.SelectedItem = snapshot; changing = false; dirty = false;
 }
 private async void Save_Click(object sender, RoutedEventArgs e)
 {
  vm.Busy = true;
  try { await SaveCurrentAsync(Snapshot()); vm.Status = "Réponses enregistrées. Les versions précédentes sont conservées."; }
  catch (Exception ex) { Error(ex); }
  finally { vm.Busy = false; }
 }
 private async void Analyze_Click(object sender, RoutedEventArgs e)
 {
  vm.Busy = true;
  try
  {
   var snapshot = Snapshot();
   if (string.IsNullOrWhiteSpace(ApiKey.Password)) throw new ArgumentException("Saisissez votre clé API.");
   using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(120) };
   var provider = new CompatibleChatProvider(http);
   var prepared = provider.Prepare(snapshot, vm.Endpoint, vm.Model);
   if (!ConfirmTransmission(prepared)) { vm.Status = "Envoi annulé. Aucune donnée transmise."; return; }
   await SaveCurrentAsync(snapshot);
   vm.Status = "Analyse en cours… délai maximal de 120 secondes.";
   using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));
   var result = await provider.AnalyzeAsync(prepared, ApiKey.Password, true, timeout.Token);
   snapshot.Analyses.Add(result); vm.Result = result.Response; dirty = true;
   await SaveCurrentAsync(snapshot); vm.Status = "Proposition IA enregistrée avec les sources transmises, le fournisseur, le modèle et la date d’autorisation.";
  }
  catch (Exception ex) { Error(ex); }
  finally { vm.Busy = false; }
 }
 private bool ConfirmTransmission(PreparedAnalysis request)
 {
  var window = new Window { Owner = this, Title = "Autoriser la transmission à l’IA", Width = 850, Height = 650, WindowStartupLocation = WindowStartupLocation.CenterOwner };
  var panel = new DockPanel { Margin = new Thickness(16) };
  var buttons = new StackPanel { Orientation = Orientation.Horizontal };
  var cancel = new Button { Content = "Annuler", IsCancel = true, Margin = new Thickness(8) };
  var send = new Button { Content = "J’autorise l’envoi de ces données", Margin = new Thickness(8) };
  send.Click += (_, _) => window.DialogResult = true; buttons.Children.Add(cancel); buttons.Children.Add(send);
  DockPanel.SetDock(buttons, Dock.Bottom); panel.Children.Add(buttons);
  var label = new TextBlock { Text = $"Destination : {request.Endpoint}\nModèle : {request.Model}\nContenu exact ci-dessous. La clé est transmise séparément pour l’authentification. Cet appel peut être facturé par votre fournisseur.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,12) };
  DockPanel.SetDock(label, Dock.Top); panel.Children.Add(label);
  panel.Children.Add(new TextBox { Text = request.Json, IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
  window.Content = panel; return window.ShowDialog() == true;
 }
 private void Error(Exception ex)
 {
  App.WriteTechnicalError(ex);
  vm.Status = ex is ArgumentException or InvalidDataException ? ex.Message : ex is OperationCanceledException ? "Délai dépassé ou appel annulé. Aucun nouvel essai automatique." : ex is HttpRequestException ? "Échec de connexion API. Vérifiez l’URL, le modèle, la clé et le quota. Aucun nouvel essai automatique." : "Opération impossible. Vérifiez le fichier et l’accès au dossier local. Une éventuelle réponse IA reste visible à l’écran.";
 }
}
