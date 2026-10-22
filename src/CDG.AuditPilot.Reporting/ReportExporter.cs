using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W=DocumentFormat.OpenXml.Wordprocessing;
using S=DocumentFormat.OpenXml.Spreadsheet;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
namespace CDG.AuditPilot.Reporting;
public sealed record ReportBrand(string Organization="Cap Digital Gestion",string Title="Rapport d’audit organisationnel",string Footer="Confidentiel — diffusion limitée au client");
public static class ReportExporter {
 public static List<string> Content(AuditData data,Guid missionId,ReportBrand brand,IReadOnlyList<InterviewSession>? sessions=null){
  var m=data.Missions.Single(x=>x.Id==missionId);var c=data.Clients.Single(x=>x.Id==m.ClientId);
  var lines=new List<string>{brand.Organization,brand.Title,c.Name+" — "+m.Title,"Référence : "+m.Reference,"État : "+m.Status,"Édition UTC : "+DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),"Périmètre",m.Scope,"Entretiens"};
  foreach(var i in data.Interviews.Where(x=>x.MissionId==missionId)){lines.Add(i.Person+" — "+i.CreatedUtc.ToString("yyyy-MM-dd"));lines.Add(i.Notes);}
  foreach(var session in sessions??[]){lines.Add(session.Title);foreach(var section in session.Questionnaire.Sections){lines.Add(section.Title);foreach(var question in section.Questions){lines.Add(question.Label);lines.Add(session.Answers.GetValueOrDefault(question.Id,"Non renseigné"));}}}
  lines.Add("Constats et risques");foreach(var f in data.Findings.Where(x=>x.MissionId==missionId)){
   lines.Add(f.Description);lines.Add("Critère : "+f.Criterion);lines.Add("Cause présumée : "+f.Cause);lines.Add("Conséquence : "+f.Consequence);lines.Add($"Impact {f.Impact}/5 × probabilité {f.Likelihood}/5 = {f.Score}/25");lines.Add($"Niveau : {f.RiskLevel} — méthode v{f.Scoring.Version}, seuils {f.Scoring.ReviewThreshold}/{f.Scoring.HighThreshold}");lines.Add(f.Validated?"Validé par "+f.Reviewer:"BROUILLON — non validé");lines.Add("Preuves : "+f.Evidence);
   foreach(var id in f.EvidenceIds){var e=data.Evidence.Single(x=>x.Id==id);lines.Add(e.OriginalFileName+" — SHA-256 "+e.Sha256);}}
  lines.Add("Plan d’actions");foreach(var a in data.Actions.Where(x=>x.MissionId==missionId)){lines.Add(a.Description);lines.Add($"Responsable : {a.Owner} | Priorité : {a.Priority} | Échéance : {a.DueDate:yyyy-MM-dd} | État : {a.Status}");lines.Add($"Charge : {a.EstimatedHours} h | Coût estimé : {a.EstimatedCost} EUR | Efficacité : {a.Effectiveness}");}
  lines.Add(brand.Footer);return lines;
 }
 public static void Word(string path,IReadOnlyList<string> lines,IReadOnlyList<string>? images=null){
  using var doc=WordprocessingDocument.Create(path,WordprocessingDocumentType.Document);var main=doc.AddMainDocumentPart();var body=new W.Body();main.Document=new W.Document(body);
  for(int i=0;i<lines.Count;i++)body.Append(new W.Paragraph(new W.ParagraphProperties(new W.SpacingBetweenLines{After="120"}),new W.Run(new W.RunProperties(new W.RunFonts{Ascii="Calibri",HighAnsi="Calibri"},new W.FontSize{Val=i<2?"32":"22"}),new W.Text(lines[i]){Space=SpaceProcessingModeValues.Preserve})));
  uint id=1;foreach(var file in images??[]){var part=main.AddImagePart(ImagePartType.Png);using(var stream=File.OpenRead(file))part.FeedData(stream);var rel=main.GetIdOfPart(part);
   using var img=XImage.FromFile(file);long cx=5486400,cy=(long)(cx*img.PointHeight/img.PointWidth);if(cy>7500000){cx=(long)(cx*7500000.0/cy);cy=7500000;}
   var xml=$"<w:drawing xmlns:w='http://schemas.openxmlformats.org/wordprocessingml/2006/main' xmlns:wp='http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing' xmlns:a='http://schemas.openxmlformats.org/drawingml/2006/main' xmlns:pic='http://schemas.openxmlformats.org/drawingml/2006/picture' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><wp:inline><wp:extent cx='{cx}' cy='{cy}'/><wp:docPr id='{id++}' name='Schéma'/><a:graphic><a:graphicData uri='http://schemas.openxmlformats.org/drawingml/2006/picture'><pic:pic><pic:nvPicPr><pic:cNvPr id='0' name='Schéma'/><pic:cNvPicPr/></pic:nvPicPr><pic:blipFill><a:blip r:embed='{rel}'/><a:stretch><a:fillRect/></a:stretch></pic:blipFill><pic:spPr><a:xfrm><a:off x='0' y='0'/><a:ext cx='{cx}' cy='{cy}'/></a:xfrm><a:prstGeom prst='rect'><a:avLst/></a:prstGeom></pic:spPr></pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing>";
   body.Append(new W.Paragraph(new W.Run(new W.Break{Type=W.BreakValues.Page})),new W.Paragraph(new W.Run(new W.Drawing(xml))));
  }
  body.Append(new W.SectionProperties(new W.PageSize{Width=11906,Height=16838},new W.PageMargin{Top=1134,Bottom=1134,Left=1134,Right=1134,Header=567,Footer=567,Gutter=0}));main.Document.Save();
 }
 public static void Excel(string path,IEnumerable<AuditAction> actions){
  using var doc=SpreadsheetDocument.Create(path,SpreadsheetDocumentType.Workbook);var main=doc.AddWorkbookPart();main.Workbook=new S.Workbook();var part=main.AddNewPart<WorksheetPart>();var rows=new S.SheetData();
  void Row(params string[] values){var row=new S.Row();foreach(var value in values)row.Append(new S.Cell{DataType=S.CellValues.InlineString,InlineString=new S.InlineString(new S.Text(value){Space=SpaceProcessingModeValues.Preserve})});rows.Append(row);}
  Row("Action","Responsable","Priorité","Échéance","Statut","Charge (h)","Coût estimé (EUR)","Contrôle efficacité");
  foreach(var a in actions){Row(a.Description,a.Owner,a.Priority,a.DueDate?.ToString("yyyy-MM-dd")??"",a.Status,a.EstimatedHours.ToString(CultureInfo.InvariantCulture),a.EstimatedCost.ToString(CultureInfo.InvariantCulture),a.Effectiveness);var cells=rows.LastChild!.Elements<S.Cell>().ToArray();foreach(int i in new[]{5,6}){var value=cells[i].InlineString!.InnerText;cells[i].InlineString=null;cells[i].DataType=S.CellValues.Number;cells[i].CellValue=new S.CellValue(value);}}
  part.Worksheet=new S.Worksheet(new S.Columns(new S.Column{Min=1,Max=1,Width=65,CustomWidth=true},new S.Column{Min=2,Max=8,Width=24,CustomWidth=true}),rows,new S.AutoFilter{Reference="A1:H"+rows.ChildElements.Count});var sheets=main.Workbook.AppendChild(new S.Sheets());sheets.Append(new S.Sheet{Id=main.GetIdOfPart(part),SheetId=1,Name="Plan d’actions"});main.Workbook.Save();
 }
 public static void Pdf(string path,IReadOnlyList<string> lines,IReadOnlyList<string>? images=null){
  if(GlobalFontSettings.FontResolver is null)GlobalFontSettings.FontResolver=new LocalFontResolver();
  using var doc=new PdfDocument();XGraphics? g=null;double y=0;int page=0;var font=new XFont("AuditPilot",10);
  void NewPage(){g?.Dispose();var p=doc.AddPage();p.Size=PdfSharp.PageSize.A4;g=XGraphics.FromPdfPage(p);y=52;g.DrawString("CDG AuditPilot · "+(++page),font,XBrushes.Gray,new XPoint(48,810));}
  NewPage();foreach(var text in lines){foreach(var paragraph in text.Replace("\r","").Split('\n')){string line="";foreach(var word in paragraph.Split(' ')){string candidate=line.Length==0?word:line+" "+word;if(g!.MeasureString(candidate,font).Width>495&&line.Length>0){if(y>775)NewPage();g!.DrawString(line,font,XBrushes.Black,new XPoint(48,y));y+=15;line=word;}else line=candidate;
    while(g!.MeasureString(line,font).Width>495){int n=line.Length-1;while(n>1&&g.MeasureString(line[..n],font).Width>495)n--;if(y>775)NewPage();g!.DrawString(line[..n],font,XBrushes.Black,new XPoint(48,y));y+=15;line=line[n..];}}
   if(y>775)NewPage();g!.DrawString(line,font,XBrushes.Black,new XPoint(48,y));y+=17;}y+=5;}
  foreach(var file in images??[]){NewPage();using var image=XImage.FromFile(file);double scale=Math.Min(495/image.PointWidth,700/image.PointHeight);g!.DrawImage(image,48,52,image.PointWidth*scale,image.PointHeight*scale);}g?.Dispose();doc.Save(path);
 }
 private sealed class LocalFontResolver:IFontResolver {
  public FontResolverInfo ResolveTypeface(string familyName,bool bold,bool italic)=>new("AuditPilot");
  public byte[] GetFont(string faceName){var path=OperatingSystem.IsWindows()?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"arial.ttf"):"/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";return File.ReadAllBytes(path);}
 }
}
