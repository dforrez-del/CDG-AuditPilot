using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace CDG.AuditPilot.Application;
public sealed record AuditQuestion(string Id, string Label, string Type, bool Required, string[] Options);
public sealed record AuditSection(string Title, AuditQuestion[] Questions);
public sealed record AuditQuestionnaire(int SchemaVersion, string Title, AuditSection[] Sections);
public sealed record AnalysisDraft(DateTime ConsentedUtc, string Endpoint, string Model, string RequestJson, string Response, string Status = "Proposition non validée");
public sealed record InterviewSession(Guid Id, Guid MissionId, AuditQuestionnaire Questionnaire, Dictionary<string, string> Answers, List<AnalysisDraft> Analyses)
{
 [JsonIgnore]
 public string Title => Questionnaire.Title;
}
public interface IQuestionnaireStore
{
 Task<IReadOnlyList<InterviewSession>> ListAsync(Guid missionId);
 Task SaveAsync(InterviewSession session);
}
public static class QuestionnaireFormat
{
 public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };
 private static readonly HashSet<string> Types = ["text", "longText", "yesNo", "singleChoice", "multipleChoice", "scale0to5", "scale0to10", "number", "date"];
 public static AuditQuestionnaire Parse(string json)
 {
  if (json.Length > 1_000_000) throw new ArgumentException("Questionnaire trop volumineux (limite : 1 million de caractères).");
  AuditQuestionnaire q;
  try { q = JsonSerializer.Deserialize<AuditQuestionnaire>(json, Json) ?? throw new JsonException(); }
  catch (JsonException) { throw new ArgumentException("JSON invalide ou structure non prise en charge. Utilisez le modèle AuditPilot schemaVersion 1."); }
  if (q.SchemaVersion != 1 || string.IsNullOrWhiteSpace(q.Title) || q.Title.Length > 200 || q.Sections.Length is < 1 or > 50)
   throw new ArgumentException("Version, titre ou nombre de sections invalide.");
  var ids = new HashSet<string>();
  foreach (var section in q.Sections)
  {
   if (section is null || string.IsNullOrWhiteSpace(section.Title) || section.Questions is null || section.Questions.Length == 0) throw new ArgumentException("Chaque section doit contenir un titre et des questions.");
   foreach (var item in section.Questions)
   {
    if (item is null || string.IsNullOrWhiteSpace(item.Id) || item.Id.Length > 80 || !ids.Add(item.Id) || string.IsNullOrWhiteSpace(item.Label) || item.Label.Length > 4000 || !Types.Contains(item.Type) || item.Options is null)
     throw new ArgumentException("Question invalide : identifiant unique, libellé et type pris en charge obligatoires.");
    if (item.Options.Any(string.IsNullOrWhiteSpace) || item.Options.Distinct().Count() != item.Options.Length || item.Options.Length > 100 || item.Options.Any(x => x.Contains(';')))
     throw new ArgumentException($"Options invalides pour {item.Id}.");
    if ((item.Type is "singleChoice" or "multipleChoice") != (item.Options.Length > 0)) throw new ArgumentException($"Options incohérentes pour {item.Id}.");
   }
  }
  if (ids.Count > 500) throw new ArgumentException("Maximum 500 questions par questionnaire.");
  return q;
 }
 public static string? ValidateAnswer(AuditQuestion q, string answer)
 {
  if (answer.Length > 10000) return $"{q.Id} : réponse trop longue.";
  if (string.IsNullOrWhiteSpace(answer)) return null; // Brouillon incomplet autorisé, explicitement transmis comme manquant.
  bool ok = q.Type switch
  {
   "yesNo" => answer is "Oui" or "Non",
   "singleChoice" => q.Options.Contains(answer),
   "multipleChoice" => answer.Split(';', StringSplitOptions.TrimEntries).All(q.Options.Contains),
   "scale0to5" => int.TryParse(answer, out var n) && n is >= 0 and <= 5,
   "scale0to10" => int.TryParse(answer, out var n) && n is >= 0 and <= 10,
   "number" => decimal.TryParse(answer, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"), out _),
   "date" => DateOnly.TryParseExact(answer, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
   _ => true
  };
  return ok ? null : $"{q.Id} : réponse incompatible avec le type {q.Type}.";
 }
 public static void ValidateSession(InterviewSession session)
 {
  Parse(JsonSerializer.Serialize(session.Questionnaire, Json));
  var questions = session.Questionnaire.Sections.SelectMany(s => s.Questions).ToDictionary(q => q.Id);
  foreach (var (id, value) in session.Answers)
  {
   if (!questions.TryGetValue(id, out var q)) throw new ArgumentException("Réponse sans question associée.");
   if (ValidateAnswer(q, value) is string error) throw new ArgumentException(error);
  }
 }
}
