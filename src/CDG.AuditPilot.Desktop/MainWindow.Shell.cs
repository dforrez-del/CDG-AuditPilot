using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CDG.AuditPilot.Domain;
using CDG.AuditPilot.Infrastructure;
namespace CDG.AuditPilot.Desktop;
public partial class MainWindow
{
    private bool shellReady;
    private sealed record MissionCard(Guid Id, string Title, string Client);
    private void Navigate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && int.TryParse(button.Tag?.ToString(), out var index)) Tabs.SelectedIndex = index;
    }
    private void UpdatePage()
    {
        string[] titles = ["Clients et missions", "Préparer vos entretiens", "Notes d’entretien", "Constats et risques", "Votre plan d’actions", "Analyse et restitution", "Mon activité d’audit", "Paramètres et sauvegardes"];
        string[] subtitles = ["Cadrez votre intervention et retrouvez les dossiers de vos clients.", "Une bibliothèque de questionnaires pour guider vos échanges.", "Conservez les informations recueillies et leur contexte.", "Reliez vos observations aux preuves et validez leur cotation.", "Organisez les recommandations, les responsables et les échéances.", "Préparez vos livrables et sollicitez l’IA avec votre consentement.", "Une vue d’ensemble pour préparer votre prochaine intervention.", "Personnalisez vos documents et protégez votre travail."];
        var index = Tabs.SelectedIndex;
        if (index < 0 || index >= titles.Length) return;
        PageTitle.Text = titles[index]; PageSubtitle.Text = subtitles[index];
        foreach (var button in Navigation.Children.OfType<Button>().Append(SettingsNavigation))
        {
            bool active = button.Tag?.ToString() == index.ToString();
            button.Background = active ? new SolidColorBrush(Color.FromRgb(39,59,83)) : Brushes.Transparent;
            button.BorderBrush = active ? new SolidColorBrush(Color.FromRgb(38,186,171)) : Brushes.Transparent;
            button.BorderThickness = new Thickness(3,0,0,0);
        }
        ContextActions.Children.Clear();
        void Add(string label, Action action, bool needsMission = false)
        {
            var button = Button(ContextActions, label, action);
            button.Margin = new Thickness(0,0,8,6);
            button.IsEnabled = !needsMission || mission != null;
            if (!button.IsEnabled) button.ToolTip = "Choisissez d’abord une mission dans Clients et missions.";
        }
        void Flows() { if (mission != null) new FlowWindow(mission) { Owner = this }.ShowDialog(); }
        switch (index)
        {
            case 0: Add("Fiche client et établissements", EditClient); Add("Détails de la mission", EditMission, true); break;
            case 1: Add("Bibliothèque de questionnaires", () => new QuestionnaireLibraryWindow { Owner = this }.ShowDialog()); break;
            case 2: Add("Entretiens guidés", OpenInterview, true); Add("Preuves documentaires", EvidenceDialog, true); break;
            case 3: Add("Réviser le constat sélectionné", EditFinding, true); Add("Preuves documentaires", EvidenceDialog, true); Add("Cartographie des flux", Flows, true); break;
            case 4: Add("Modifier l’action sélectionnée", EditAction, true); Add("Exporter Word, PDF ou Excel", ExportDocuments, true); break;
            case 5: Add("Exporter Word, PDF ou Excel", ExportDocuments, true); Add("Cartographie des flux", Flows, true); Add("Exporter le dossier de mission", ExportMission, true); break;
            case 7: Add("Clé IA et identité des rapports", SettingsDialog); Add("Méthode de cotation", EditRiskMethod); Add("Sauvegarder", Backup); Add("Restaurer", Restore); Add("Sauvegardes automatiques", RestoreAuto); Add("Historique", Journal); break;
        }
    }
    private void UpdateDashboard()
    {
        MissionCount.Text = data.Missions.Count(x => x.Status is not MissionStatus.Terminee and not MissionStatus.Archivee).ToString();
        ClientCount.Text = data.Clients.Count.ToString();
        ReviewCount.Text = data.Findings.Count(x => !x.Validated).ToString();
        var open = data.Actions.Where(x => x.Status is not "Terminée" and not "Abandonnée").ToArray();
        ActionCount.Text = open.Length.ToString();
        ActiveMission.Text = mission != null ? $"{client?.Name}  /  {mission.Title}" : client != null ? client.Name + " — choisissez une mission" : "Choisissez un client puis une mission";
        RecentMissions.ItemsSource = data.Missions.OrderByDescending(x => x.CreatedUtc).Take(6).Select(x => new MissionCard(x.Id,x.Title,data.Clients.FirstOrDefault(c => c.Id == x.ClientId)?.Name ?? "")).ToArray();
        MissionEmpty.Visibility = data.Missions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        int overdue = open.Count(x => x.DueDate.HasValue && x.DueDate.Value.Date < DateTime.Today);
        AttentionText.Text = $"{data.Findings.Count(x => !x.Validated)} constat(s) en attente de validation.\n{overdue} action(s) avec une échéance dépassée.";
        int validated = data.Findings.Count(x => x.Validated);
        ReviewProgress.Value = data.Findings.Count == 0 ? 0 : 100.0 * validated / data.Findings.Count;
        ProgressLabel.Text = data.Findings.Count == 0 ? "La progression apparaîtra avec vos premiers constats." : $"{validated} / {data.Findings.Count} constats validés — tous dossiers";
        if (shellReady) UpdatePage();
    }
    private void OpenInterview()
    {
        if (mission == null) { Tabs.SelectedIndex = 0; StatusText.Text = "Choisissez un client puis une mission pour ouvrir les entretiens."; return; }
        new QuestionnaireWindow(mission, new QuestionnaireStore(Path.Combine(App.DataDirectory,"auditpilot.db"))) { Owner = this }.ShowDialog();
    }
    private void OpenInterview_Click(object sender, RoutedEventArgs e) => OpenInterview();
    private void Search_Click(object sender, RoutedEventArgs e) => Search();
    private void Help_Click(object sender, RoutedEventArgs e) => MessageBox.Show(this,"1. Créez un client et sa mission.\n2. Préparez un questionnaire et conduisez les entretiens.\n3. Ajoutez les preuves et validez les constats.\n4. Organisez les actions.\n5. Exportez les rapports.\n\nLe dossier actif reste visible en haut de chaque écran. Les données sont enregistrées localement. L’IA exige une connexion et votre consentement.","Votre parcours dans AuditPilot");
    private void RecentMission_Click(object sender, MouseButtonEventArgs e)
    {
        if (RecentMissions.SelectedItem is not MissionCard card) return;
        mission = data.Missions.Single(x => x.Id == card.Id);
        client = data.Clients.Single(x => x.Id == mission.ClientId);
        Refresh(); Tabs.SelectedIndex = 1;
    }
}
