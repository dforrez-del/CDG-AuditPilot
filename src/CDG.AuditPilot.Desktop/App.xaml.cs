using System.IO;
using System.Windows;
using CDG.AuditPilot.Infrastructure;
namespace CDG.AuditPilot.Desktop;
public partial class App:System.Windows.Application {
 private System.Threading.Mutex? mutex;
 public static string DataDirectory=>Environment.GetCommandLineArgs().Contains("--smoke-test")&&!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUDITPILOT_TEST_ROOT"))?Environment.GetEnvironmentVariable("AUDITPILOT_TEST_ROOT")!:Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CapDigitalGestion","AuditPilot");
 protected override void OnStartup(StartupEventArgs e){base.OnStartup(e);DispatcherUnhandledException+=(_,args)=>{WriteTechnicalError(args.Exception);MessageBox.Show("Opération non enregistrée. Vérifiez les données saisies et l’accès au dossier local.");args.Handled=true;};ShutdownMode=ShutdownMode.OnExplicitShutdown;
  mutex=new System.Threading.Mutex(true,"Local\\CDG-AuditPilot-"+Environment.UserName,out bool first);
  if(!first){MessageBox.Show("AuditPilot est déjà ouvert pour ce compte Windows.");Shutdown();return;}
  try{Directory.CreateDirectory(DataDirectory);new AuditWorkspace(Path.Combine(DataDirectory,"auditpilot.db")).InitializeAsync().GetAwaiter().GetResult();new CaseworkStore(Path.Combine(DataDirectory,"auditpilot.db")).ImportLegacy(Path.Combine(DataDirectory,"data.json"));MainWindow=new MainWindow();ShutdownMode=ShutdownMode.OnMainWindowClose;if(e.Args.Contains("--smoke-test")){MainWindow.Loaded+=async (_,_)=>{try{await ((MainWindow)MainWindow).RunSmokeAsync();Shutdown(0);}catch(Exception ex){File.WriteAllText(Path.Combine(DataDirectory,"smoke-error.txt"),ex.ToString());Shutdown(2);}};}MainWindow.Show();}
  catch(Exception ex){WriteTechnicalError(ex);MessageBox.Show("Ouverture impossible. Les données sont conservées. Consultez technical.log et vérifiez les droits et l’espace disque.");Shutdown(1);}
 }
 public static void WriteTechnicalError(Exception ex){try{Directory.CreateDirectory(DataDirectory);var file=Path.Combine(DataDirectory,"technical.log");if(File.Exists(file)&&new FileInfo(file).Length>1048576)File.Move(file,file+".previous",true);File.AppendAllText(file,$"{DateTime.UtcNow:O} {ex.GetType().Name} HResult={ex.HResult}\n");}catch(IOException){}catch(UnauthorizedAccessException){}}
 protected override void OnExit(ExitEventArgs e){mutex?.Dispose();base.OnExit(e);}
}
