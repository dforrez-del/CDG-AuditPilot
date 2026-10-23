using CDG.AuditPilot.Application;
using Xunit;
namespace CDG.AuditPilot.Tests;
public sealed class StructuredAnalysisTests {
 const string Valid="""{"summary":"Déclarations à confirmer","findings":[{"text":"Contrôle à formaliser","sources":["Q1"]}],"recommendations":[],"followUpQuestions":["Quel justificatif ?"],"reportDraft":"Projet"}""";
 [Fact]public void ReferencesAreChecked(){Assert.Contains("Q1",StructuredAnalysis.Parse(Valid,["Q1"]).ToReviewText());Assert.Throws<InvalidDataException>(()=>StructuredAnalysis.Parse(Valid,["Q2"]));}
 [Fact]public void UnknownSchemaIsRejected()=>Assert.Throws<InvalidDataException>(()=>StructuredAnalysis.Parse("{\"conclusion\":\"Tout va bien\"}",["Q1"]));
 [Fact]public void MissingSourceIsExplicitlyUnverified(){var result=StructuredAnalysis.Parse(Valid.Replace("[\"Q1\"]","[]"),["Q1"]);Assert.Contains("source absente",result.ToReviewText());}
}
