using System.IO;
using System.Windows;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CDG.AuditPilot.Desktop;
public partial class App : System.Windows.Application
{
 private ServiceProvider? services;
 public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CapDigitalGestion", "AuditPilot");
 protected override async void OnStartup(StartupEventArgs e)
 {
  base.OnStartup(e);
  ShutdownMode = ShutdownMode.OnExplicitShutdown;
  try
  {
   var collection = new ServiceCollection();
   collection.AddSingleton<IAuditWorkspace>(new AuditWorkspace(Path.Combine(DataDirectory, "auditpilot.db")));
   collection.AddSingleton<MainViewModel>();
   collection.AddSingleton<MainWindow>();
   services = collection.BuildServiceProvider();
   await Task.Run(() => services.GetRequiredService<IAuditWorkspace>().InitializeAsync());
   var vm = services.GetRequiredService<MainViewModel>();
   await vm.LoadAsync();
   MainWindow = services.GetRequiredService<MainWindow>();
   ShutdownMode = ShutdownMode.OnMainWindowClose;
   MainWindow.Show();
  }
  catch (Exception ex)
  {
   WriteTechnicalError(ex);
   MessageBox.Show("Impossible d’ouvrir le dossier local. Vérifiez l’espace disque et les droits d’accès. Les données existantes n’ont pas été supprimées.", "CDG AuditPilot", MessageBoxButton.OK, MessageBoxImage.Error);
   Shutdown(1);
  }
 }
 public static void WriteTechnicalError(Exception ex)
 {
  try
  {
   Directory.CreateDirectory(DataDirectory);
   var file = Path.Combine(DataDirectory, "technical.log");
   if (File.Exists(file) && new FileInfo(file).Length > 1048576) File.Move(file, file + ".previous", true);
   File.AppendAllText(file, $"{DateTime.UtcNow:O} {ex.GetType().Name} HResult={ex.HResult}\n");
  }
  catch (IOException) { }
  catch (UnauthorizedAccessException) { }
 }
 protected override void OnExit(ExitEventArgs e) { services?.Dispose(); base.OnExit(e); }
}
