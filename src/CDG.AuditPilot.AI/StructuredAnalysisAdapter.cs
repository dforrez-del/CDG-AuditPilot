using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CDG.AuditPilot.Application;
namespace CDG.AuditPilot.AI;
public static class StructuredAnalysisAdapter {
 public static PreparedAnalysis PrepareStructured(this CompatibleChatProvider provider,InterviewSession session,string endpoint,string model,bool maskContactDetails){
  string Mask(string value)=>maskContactDetails?Regex.Replace(Regex.Replace(value,@"[\w.+'%-]+@[\w.-]+\.[A-Za-z]{2,}","[COURRIEL]"),@"(?<!\d)(?:\+33|0)[1-9](?:[ .-]?\d{2}){4}(?!\d)","[TÉLÉPHONE]"):value;
  var source=session with{Answers=session.Answers.ToDictionary(p=>p.Key,p=>Mask(p.Value)),Comments=session.Comments.ToDictionary(p=>p.Key,p=>Mask(p.Value)),Observations=Mask(session.Observations)};
  var request=provider.Prepare(source,endpoint,model);var payload=JsonNode.Parse(request.Json)!;
  payload["response_format"]=new JsonObject{["type"]="json_object"};
  payload["messages"]![0]!["content"]=payload["messages"]![0]!["content"]!.GetValue<string>()+" Réponds uniquement par un objet JSON sans balises Markdown, avec exactement ces propriétés : summary (texte), findings (tableau de {text: texte, sources: tableau d’identifiants de questions}), recommendations (même structure), followUpQuestions (tableau de textes), reportDraft (texte). N’utilise que les identifiants des questions fournies. Sans source, utilise un tableau sources vide et indique à vérifier. Une source est une déclaration, pas une preuve vérifiée.";
  return request with{Json=payload.ToJsonString(QuestionnaireFormat.Json)};
 }
 public static async Task<AnalysisDraft> AnalyzeStructuredAsync(this CompatibleChatProvider provider,PreparedAnalysis request,InterviewSession session,string key,bool consent,CancellationToken ct){var draft=await provider.AnalyzeAsync(request,key,consent,ct);var structured=StructuredAnalysis.Parse(draft.Response,session.Questionnaire.Sections.SelectMany(s=>s.Questions).Select(q=>q.Id));return draft with{Response=structured.ToReviewText()};}
}
