using System.ComponentModel;
using System.Windows;
namespace CDG.AuditPilot.Desktop;
public partial class MainWindow : Window
{
 private readonly MainViewModel vm;
 public MainWindow(MainViewModel viewModel)
 {
  InitializeComponent(); vm = viewModel; DataContext = vm;
  Closing += OnClosing;
 }
 private void Questionnaire_Click(object sender, RoutedEventArgs e)
 {
  if (vm.SelectedMission is null || vm.SelectedMission.ClientId != vm.SelectedClient?.Id) { MessageBox.Show(this, "Sélectionnez une mission dans le tableau."); return; }
  new QuestionnaireWindow(vm.SelectedMission, new CDG.AuditPilot.Infrastructure.QuestionnaireStore(System.IO.Path.Combine(App.DataDirectory, "auditpilot.db"))) { Owner = this }.ShowDialog();
 }
 private void Flows_Click(object sender, RoutedEventArgs e)
 {
  if (vm.SelectedMission is null || vm.SelectedMission.ClientId != vm.SelectedClient?.Id) { MessageBox.Show(this, "Sélectionnez une mission dans le tableau."); return; }
  new FlowWindow(vm.SelectedMission) { Owner = this }.ShowDialog();
 }
 private void OnClosing(object? sender, CancelEventArgs e)
 {
  if (vm.IsBusy) { e.Cancel = true; return; }
  if (vm.HasDraft && MessageBox.Show("Une saisie n’est pas enregistrée. Fermer et abandonner cette saisie ?", "CDG AuditPilot", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
   e.Cancel = true;
 }
}
