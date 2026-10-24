using System.Text.Json;
namespace CDG.AuditPilot.Application;
public sealed record AnalysisProposal(string Text,string[] Sources);
public sealed record StructuredAnalysis(string Summary,AnalysisProposal[] Findings,AnalysisProposal[] Recommendations,string[] FollowUpQuestions,string ReportDraft) {
 public static StructuredAnalysis Parse(string json,IEnumerable<string> questionIds){
  StructuredAnalysis result;try{result=JsonSerializer.Deserialize<StructuredAnalysis>(json,QuestionnaireFormat.Json)??throw new JsonException();}catch(JsonException ex){throw new InvalidDataException("Sortie IA non conforme au schéma attendu.",ex);}
  if(result.Summary is null||result.Findings is null||result.Recommendations is null||result.FollowUpQuestions is null||result.ReportDraft is null||result.FollowUpQuestions.Any(x=>x is null||x.Length>10000))throw new InvalidDataException("Sortie IA incomplète ou invalide.");
  if(result.Summary.Length>30000||result.Findings.Length>100||result.Recommendations.Length>100||result.FollowUpQuestions.Length>100||result.ReportDraft.Length>60000)throw new InvalidDataException("Sortie IA trop volumineuse.");
  var ids=questionIds.ToHashSet();foreach(var item in result.Findings.Concat(result.Recommendations))if(item is null||string.IsNullOrWhiteSpace(item.Text)||item.Text.Length>10000||item.Sources is null||item.Sources.Any(id=>!ids.Contains(id)))throw new InvalidDataException("Source IA inconnue ou proposition invalide.");return result;
 }
 public string ToReviewText()=>"PROPOSITION IA — À VÉRIFIER\n\nSynthèse\n"+Summary+"\n\nConstats proposés\n"+string.Join("\n",Findings.Select(p=>p.Text+" ["+(p.Sources.Length==0?"à vérifier — source absente":string.Join(", ",p.Sources))+"]"))+"\n\nRecommandations\n"+string.Join("\n",Recommendations.Select(p=>p.Text+" ["+(p.Sources.Length==0?"à vérifier — source absente":string.Join(", ",p.Sources))+"]"))+"\n\nQuestions complémentaires\n"+string.Join("\n",FollowUpQuestions)+"\n\nProjet de rédaction\n"+ReportDraft;
}
