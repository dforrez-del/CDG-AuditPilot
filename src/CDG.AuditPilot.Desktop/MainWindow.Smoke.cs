using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Infrastructure;
using CDG.AuditPilot.Reporting;
namespace CDG.AuditPilot.Desktop;
public partial class MainWindow {
 internal async Task RunSmokeAsync(){
  ClientName.Text="Client fictif de recette";AddClient_Click(this,new RoutedEventArgs());ClientsList.SelectedItem=data.Clients.Last();
  MissionTitle.Text="Audit stocks et documents";AddMission_Click(this,new RoutedEventArgs());MissionsList.SelectedItem=data.Missions.Last();if(mission is null||client is null)throw new InvalidOperationException("Sélection client/mission perdue.");
  InterviewPerson.Text="Responsable logistique";InterviewNotes.Text="Les écarts sont suivis dans Excel.";AddInterview_Click(this,new RoutedEventArgs());FindingText.Text="Contrôle documentaire à formaliser";FindingEvidence.Text="preuve absente";AddFinding_Click(this,new RoutedEventArgs());ActionText.Text="Formaliser le rapprochement";ActionOwner.Text="Responsable";AddAction_Click(this,new RoutedEventArgs());
  var q=new AuditQuestionnaire(1,"Questionnaire test",[new("Stocks",[new("Q1","Qui valide ?","text",true,[])])]);var store=new QuestionnaireStore(Path.Combine(App.DataDirectory,"auditpilot.db"));await store.SaveAsync(new(Guid.NewGuid(),mission.Id,q,new(){["Q1"]="Responsable"},[]));
  var library=new QuestionnaireLibraryWindow{Owner=this};library.Show();library.SmokeEditAndSave();library.UpdateLayout();await Task.Delay(150);var libraryBitmap=new RenderTargetBitmap((int)library.ActualWidth,(int)library.ActualHeight,96,96,PixelFormats.Pbgra32);libraryBitmap.Render(library);var libraryEncoder=new PngBitmapEncoder();libraryEncoder.Frames.Add(BitmapFrame.Create(libraryBitmap));using(var capture=File.Create(Path.Combine(App.DataDirectory,"smoke-library.png")))libraryEncoder.Save(capture);library.Close();
  var window=new QuestionnaireWindow(mission,store){Owner=this};window.Show();await Task.Delay(500);window.Close();
  var flow=new FlowWindow(mission){Owner=this};flow.Show();flow.SmokeSave();flow.Close();
  var lines=ReportExporter.Content(data,mission.Id,new(),await store.ListAsync(mission.Id));var images=Directory.GetFiles(Path.Combine(App.DataDirectory,"Missions",mission.Id.ToString("N"),"Flows"),"*.png");ReportExporter.Word(Path.Combine(App.DataDirectory,"smoke-report.docx"),lines,images);ReportExporter.Pdf(Path.Combine(App.DataDirectory,"smoke-report.pdf"),lines,images);ReportExporter.Excel(Path.Combine(App.DataDirectory,"smoke-actions.xlsx"),data.Actions);
  for(int i=0;i<Tabs.Items.Count;i++){Tabs.SelectedIndex=i;UpdateLayout();await Task.Delay(100);var bitmap=new RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(this);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(Path.Combine(App.DataDirectory,$"smoke-tab-{i+1}.png"));encoder.Save(stream);}
  if(MissionCount.Text!="1"||ClientCount.Text!="1"||ReviewCount.Text!="1"||ActionCount.Text!="1"||!ActiveMission.Text.Contains(mission.Title))throw new InvalidOperationException("Tableau de bord incohérent.");
  Tabs.SelectedIndex=7;UpdateLayout();if(SettingsNavigation.Background==Brushes.Transparent)throw new InvalidOperationException("Paramètres non signalés dans la navigation.");
  Tabs.SelectedIndex=6;UpdateLayout();if(PageTitle.Text!="Mon activité d’audit"||FindingsGrid.AutoGenerateColumns)throw new InvalidOperationException("Navigation ou colonnes métier incorrectes.");
  var loaded=new CaseworkStore(Path.Combine(App.DataDirectory,"auditpilot.db")).Load();if(loaded.Findings.Count!=1||loaded.Actions.Count!=1)throw new InvalidOperationException("Persistance incomplète.");File.WriteAllText(Path.Combine(App.DataDirectory,"smoke-result.txt"),"PASS: WPF startup, client/mission selection, interview, finding, action, questionnaire, diagram PNG/SVG, DOCX/PDF/XLSX and persistence.");
 }
}
