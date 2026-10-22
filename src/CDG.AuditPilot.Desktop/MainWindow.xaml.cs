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
 private void OnClosing(object? sender, CancelEventArgs e)
 {
  if (vm.IsBusy) { e.Cancel = true; return; }
  if (vm.HasDraft && MessageBox.Show("Une saisie n’est pas enregistrée. Fermer et abandonner cette saisie ?", "CDG AuditPilot", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
   e.Cancel = true;
 }
}
