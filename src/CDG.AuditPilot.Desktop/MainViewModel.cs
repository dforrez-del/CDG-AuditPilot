using System.Collections.ObjectModel;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace CDG.AuditPilot.Desktop;
public partial class MainViewModel(IAuditWorkspace workspace) : ObservableObject
{
 public ObservableCollection<Client> Clients { get; } = [];
 public ObservableCollection<Mission> Missions { get; } = [];
 [ObservableProperty] private Client? selectedClient;
 [ObservableProperty] private string clientName = "";
 [ObservableProperty] private string clientEmail = "";
 [ObservableProperty] private string missionReference = "";
 [ObservableProperty] private string missionTitle = "";
 [ObservableProperty] private string missionScope = "";
 [ObservableProperty] private string statusMessage = "Prêt. Données conservées sur ce poste.";
 [ObservableProperty] private bool isBusy;
 public bool IsIdle => !IsBusy;
 public bool HasDraft => !string.IsNullOrWhiteSpace(ClientName + ClientEmail + MissionReference + MissionTitle + MissionScope);
 public string StoragePath => App.DataDirectory;
 partial void OnIsBusyChanged(bool value) { OnPropertyChanged(nameof(IsIdle)); }
 partial void OnSelectedClientChanged(Client? value) { _ = LoadMissionsAsync(value); }
 public async Task LoadAsync()
 {
  var clients = await Task.Run(() => workspace.GetClientsAsync());
  Clients.Clear(); foreach (var c in clients) Clients.Add(c);
  SelectedClient = Clients.FirstOrDefault();
 }
 private int missionLoadVersion;
 private async Task LoadMissionsAsync(Client? client)
 {
  var version = ++missionLoadVersion;
  try
  {
   if (client is null) { Missions.Clear(); return; }
   var missions = await Task.Run(() => workspace.GetMissionsAsync(client.Id));
   if (version != missionLoadVersion || SelectedClient?.Id != client.Id) return;
   Missions.Clear(); foreach (var m in missions) Missions.Add(m);
  }
  catch (Exception ex) { App.WriteTechnicalError(ex); StatusMessage = "Impossible de charger les missions. Réessayez en sélectionnant le client."; }
 }
 [RelayCommand]
 private async Task CreateClientAsync()
 {
  if (IsBusy) return;
  IsBusy = true;
  var name = ClientName; var email = ClientEmail;
  try
  {
   var client = await Task.Run(() => workspace.CreateClientAsync(name, email));
   Clients.Add(client); SelectedClient = client;
   ClientName = ""; ClientEmail = "";
   StatusMessage = "Client enregistré.";
  }
  catch (ArgumentException ex) { StatusMessage = ex.Message; }
  catch (Exception ex) { App.WriteTechnicalError(ex); StatusMessage = "Échec de l’enregistrement. Votre saisie est conservée à l’écran."; }
  finally { IsBusy = false; }
 }
 [RelayCommand]
 private async Task CreateMissionAsync()
 {
  if (IsBusy) return;
  if (SelectedClient is null) { StatusMessage = "Sélectionnez d’abord un client."; return; }
  IsBusy = true;
  var client = SelectedClient; var reference = MissionReference; var title = MissionTitle; var scope = MissionScope;
  try
  {
   var mission = await Task.Run(() => workspace.CreateMissionAsync(client.Id, reference, title, scope));
   if (SelectedClient?.Id == client.Id) await LoadMissionsAsync(client);
   MissionReference = ""; MissionTitle = ""; MissionScope = "";
   StatusMessage = "Mission enregistrée. Vous pouvez fermer puis rouvrir l’application.";
  }
  catch (ArgumentException ex) { StatusMessage = ex.Message; }
  catch (Exception ex) { App.WriteTechnicalError(ex); StatusMessage = "Échec de l’enregistrement. Votre saisie est conservée à l’écran."; }
  finally { IsBusy = false; }
 }
}
