using CDG.AuditPilot.Application;
using CDG.AuditPilot.Reporting;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using Xunit;
namespace CDG.AuditPilot.Tests;
public sealed class ReportTests {
 [Fact] public void WordIsValidEditableOpenXml(){var path=Path.GetTempFileName();try{ReportExporter.Word(path,["Cap Digital Gestion","Audit — contrôle des flux","Écart constaté : facture absente."]);using var doc=WordprocessingDocument.Open(path,false);Assert.Empty(new OpenXmlValidator().Validate(doc));Assert.Contains("facture absente",doc.MainDocumentPart!.Document.InnerText);}finally{File.Delete(path);}}
 [Fact] public void ExcelPreservesFormulaLikeTextWithoutExecution(){var path=Path.GetTempFileName();try{ReportExporter.Excel(path,[new(Guid.NewGuid(),Guid.NewGuid(),"=HYPERLINK(\"https://example.test\")","David","En cours"){EstimatedCost=125.5m}]);using var doc=SpreadsheetDocument.Open(path,false);Assert.Empty(new OpenXmlValidator().Validate(doc));Assert.Empty(doc.WorkbookPart!.WorksheetParts.Single().Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.CellFormula>());}finally{File.Delete(path);}}
 [Fact] public void PdfPaginatesLongFrenchContent(){var path=Path.GetTempFileName();try{ReportExporter.Pdf(path,Enumerable.Repeat("Évaluation des procédures — traçabilité et contrôle des pièces justificatives. "+new string('A',180),100).ToArray());using var pdf=PdfSharp.Pdf.IO.PdfReader.Open(path,PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);Assert.True(pdf.PageCount>2);}finally{File.Delete(path);}}
 [Fact] public void SvgEscapesUntrustedLabels(){var flow=new FlowDiagram(1,"Flux",[new(){Id="a",Label="<script>alert(1)</script>"},new(){Id="b",Label="Validation"}],[new(){From="a",To="b",Label="Oui & non"}]);var xml=System.Xml.Linq.XDocument.Parse(FlowSvg.Render(flow));Assert.Empty(xml.Descendants().Where(x=>x.Name.LocalName=="script"));Assert.Contains("&lt;script&gt;",xml.ToString());}
}
