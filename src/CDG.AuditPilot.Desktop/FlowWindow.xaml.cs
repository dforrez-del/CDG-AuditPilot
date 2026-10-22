using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using Microsoft.Win32;
namespace CDG.AuditPilot.Desktop;
public partial class FlowWindow : Window
{
 private readonly ObservableCollection<FlowNode> nodes = [];
 private readonly ObservableCollection<FlowEdge> edges = [];
 private readonly string folder;
 private bool dirty;
 private bool loading;
 private BitmapSource? importedImage;
 public FlowWindow(Mission mission)
 {
  InitializeComponent(); Title = $"{mission.Reference} — Schémas de flux";
  folder = System.IO.Path.Combine(App.DataDirectory, "Missions", mission.Id.ToString("N"), "Flows");
  Directory.CreateDirectory(folder); NodesGrid.ItemsSource = nodes; EdgesGrid.ItemsSource = edges;
  nodes.CollectionChanged += (_,_)=>dirty=true; edges.CollectionChanged += (_,_)=>dirty=true;
  Refresh(); dirty=false;
  Closing += (_,e)=> { if (!MayDiscard()) e.Cancel=true; };
 }
 internal void SmokeSave(){New_Click(this,new RoutedEventArgs());Save_Click(this,new RoutedEventArgs());if(dirty)throw new InvalidOperationException("Sauvegarde du schéma impossible.");}
 private bool MayDiscard() => !dirty || MessageBox.Show(this,"Abandonner le schéma non enregistré ?","Schémas",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes;
 private void Changed(object sender, TextChangedEventArgs e) { if (!loading) dirty=true; }
 private void GridChanged(object sender, DataGridCellEditEndingEventArgs e) => dirty=true;
 private void GridKey(object sender, System.Windows.Input.KeyEventArgs e) { if(e.Key==System.Windows.Input.Key.Delete) dirty=true; }
 private void Refresh() { loading=true; Saved.ItemsSource=Directory.GetFiles(folder).Where(f=>f.EndsWith(".json")||f.EndsWith(".png")).Select(System.IO.Path.GetFileName).OrderDescending().ToArray(); loading=false; }
 private FlowDiagram Current()
 {
  NodesGrid.CommitEdit(DataGridEditingUnit.Cell,true); NodesGrid.CommitEdit(DataGridEditingUnit.Row,true);
  EdgesGrid.CommitEdit(DataGridEditingUnit.Cell,true); EdgesGrid.CommitEdit(DataGridEditingUnit.Row,true);
  var diagram=new FlowDiagram(1,FlowTitle.Text.Trim(),nodes.ToArray(),edges.ToArray()); FlowDiagram.Validate(diagram); return diagram;
 }
 private void LoadFlow(FlowDiagram flow)
 {
  loading=true; importedImage=null; FlowTitle.Text=flow.Title; nodes.Clear(); edges.Clear();
  foreach(var n in flow.Nodes) nodes.Add(n); foreach(var edge in flow.Edges) edges.Add(edge);
  NodesGrid.IsEnabled=EdgesGrid.IsEnabled=true; Render(flow); loading=false;
 }
 private void New_Click(object sender,RoutedEventArgs e)
 {
  if(!MayDiscard()) return;
  LoadFlow(new(1,"Nouveau processus",[new FlowNode { Id="depart", Label="Début du processus" }],[])); dirty=true;
 }
 private void Generate_Click(object sender,RoutedEventArgs e) { try { if(importedImage is null) Render(Current()); else ShowImage(importedImage); Status.Text="Schéma généré. Ouvrez l’onglet Schéma."; } catch(Exception ex){ Error(ex); } }
 private void Render(FlowDiagram flow)
 {
  var layout=flow.Layout(); Drawing.Children.Clear(); Drawing.Width=1100; Drawing.Height=layout.Values.Max(p=>p.Y)+160;
  foreach(var edge in flow.Edges)
  {
   var from=layout[edge.From]; var to=layout[edge.To];
   var a=new Point(from.X+125,from.Y+100); var b=new Point(to.X+125,to.Y);
   if(b.Y<=a.Y) { a=new(from.X+250,from.Y+50); b=new(to.X+250,to.Y+50); }
   var line=new Polyline { Stroke=Brushes.SlateGray, StrokeThickness=2, Points=[a,new Point(a.X+ (b.Y<=a.Y ? 35:0),(a.Y+b.Y)/2),new Point(b.X+ (b.Y<=a.Y ? 35:0),(a.Y+b.Y)/2),b] }; Drawing.Children.Add(line);
   var prev=line.Points[^2]; var direction=b-prev; if(direction.Length>0) direction.Normalize(); var side=new Vector(-direction.Y,direction.X);
   Drawing.Children.Add(new Polygon { Fill=Brushes.SlateGray, Points=[b,b-direction*12+side*5,b-direction*12-side*5] });
   var text=new TextBlock { Text=edge.Label, Background=Brushes.White, MaxWidth=220, TextWrapping=TextWrapping.Wrap }; Canvas.SetLeft(text,(a.X+b.X)/2+8); Canvas.SetTop(text,(a.Y+b.Y)/2); Drawing.Children.Add(text);
  }
  foreach(var node in flow.Nodes)
  {
   var p=layout[node.Id];
   var box=new Border { Width=250, Height=100, CornerRadius=new CornerRadius(8), Background=Brushes.AliceBlue, BorderBrush=Brushes.SteelBlue, BorderThickness=new Thickness(2), Padding=new Thickness(8), Child=new TextBlock { Text=node.Label+"\n"+node.Service, ToolTip=node.Label+"\n"+node.Service, MaxHeight=80, FontSize=12, TextTrimming=TextTrimming.CharacterEllipsis, TextWrapping=TextWrapping.Wrap, VerticalAlignment=VerticalAlignment.Center, TextAlignment=TextAlignment.Center } };
   Canvas.SetLeft(box,p.X);Canvas.SetTop(box,p.Y);Drawing.Children.Add(box);
  }
 }
 private void ShowImage(BitmapSource image)
 {
  Drawing.Children.Clear();Drawing.Width=image.PixelWidth;Drawing.Height=image.PixelHeight;
  Drawing.Children.Add(new Image { Source=image,Width=image.PixelWidth,Height=image.PixelHeight });
 }
 private void Read(string file)
 {
  if(new FileInfo(file).Length>10_000_000) throw new ArgumentException("Fichier trop volumineux (10 Mo maximum).");
  if(file.EndsWith(".json",StringComparison.OrdinalIgnoreCase)) LoadFlow(FlowDiagram.Parse(File.ReadAllText(file)));
  else
  {
   using var stream=File.OpenRead(file);var decoder=BitmapDecoder.Create(stream,BitmapCreateOptions.DelayCreation,BitmapCacheOption.OnLoad);var image=decoder.Frames[0];
   if((long)image.PixelWidth*image.PixelHeight>20_000_000)throw new ArgumentException("Image trop grande (20 millions de pixels maximum).");
   image.Freeze();importedImage=image;nodes.Clear();edges.Clear();NodesGrid.IsEnabled=EdgesGrid.IsEnabled=false;
   loading=true;FlowTitle.Text=System.IO.Path.GetFileNameWithoutExtension(file);loading=false;ShowImage(image);
  }
 }
 private void Import_Click(object sender,RoutedEventArgs e)
 {
  if(!MayDiscard())return;
  var d=new OpenFileDialog{Filter="Schémas (*.json;*.png;*.jpg;*.jpeg)|*.json;*.png;*.jpg;*.jpeg"};if(d.ShowDialog(this)!=true)return;
  try{Read(d.FileName);dirty=true;Status.Text="Schéma importé. Enregistrez-le dans la mission pour le conserver.";}catch(Exception ex){Error(ex);}
 }
 private void Saved_Changed(object sender,SelectionChangedEventArgs e)
 {
  if(loading||Saved.SelectedItem is not string name)return;
  if(!MayDiscard()) { loading=true;Saved.SelectedItem=null;loading=false;return; }
  try{Read(System.IO.Path.Combine(folder,name));dirty=false;}catch(Exception ex){Error(ex);}
 }
 private void Save_Click(object sender,RoutedEventArgs e)
 {
  try
  {
   var stem=System.IO.Path.Combine(folder,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N"));
   if(importedImage is not null)WritePng(stem+".png",importedImage);
   else { var content=JsonSerializer.Serialize(Current(),QuestionnaireFormat.Json);File.WriteAllText(stem+".tmp",content);File.Move(stem+".tmp",stem+".json"); }
   if(importedImage is null){Render(Current());Drawing.Measure(new Size(Drawing.Width,Drawing.Height));Drawing.Arrange(new Rect(0,0,Drawing.Width,Drawing.Height));Drawing.UpdateLayout();var bmp=new RenderTargetBitmap((int)Drawing.Width,(int)Drawing.Height,96,96,PixelFormats.Pbgra32);bmp.Render(Drawing);WritePng(stem+".png",bmp);File.WriteAllText(stem+".svg",FlowSvg.Render(Current()));}
   dirty=false;Refresh();Status.Text="Nouvelle version du schéma conservée dans la mission.";
  }catch(Exception ex){Error(ex);}
 }
 private static void WritePng(string path,BitmapSource image)
 {
  var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));
  var temp=path+".tmp";using(var stream=File.Create(temp))encoder.Save(stream);File.Move(temp,path,true);
 }
 private void Export_Click(object sender,RoutedEventArgs e)
 {
  try
  {
   if(importedImage is null)Render(Current());
   var d=new SaveFileDialog{Filter="Image PNG|*.png|Schéma SVG (étapes modifiables)|*.svg",FileName="schema-flux.png"};if(d.ShowDialog(this)!=true)return;
   if(d.FilterIndex==2){if(importedImage is not null)throw new ArgumentException("L’export SVG nécessite un schéma structuré.");File.WriteAllText(d.FileName,FlowSvg.Render(Current()));}
   else if(importedImage is not null)WritePng(d.FileName,importedImage);
   else { Drawing.Measure(new Size(Drawing.Width,Drawing.Height));Drawing.Arrange(new Rect(0,0,Drawing.Width,Drawing.Height));Drawing.UpdateLayout();var bmp=new RenderTargetBitmap((int)Drawing.Width,(int)Drawing.Height,96,96,PixelFormats.Pbgra32);bmp.Render(Drawing);WritePng(d.FileName,bmp); }
   Status.Text="Image PNG exportée.";
  }catch(Exception ex){Error(ex);}
 }
 private void Error(Exception ex) { App.WriteTechnicalError(ex);Status.Text=ex is ArgumentException ? ex.Message : "Impossible de lire ou d’enregistrer ce schéma. Vérifiez le format et les droits d’accès."; }
}
