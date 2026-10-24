using System.Text.Json;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CDG.AuditPilot.Infrastructure;
public sealed class QuestionnaireStore(string databasePath) : IQuestionnaireStore
{
 private AuditDbContext Open() => new(new DbContextOptionsBuilder<AuditDbContext>().UseSqlite(new SqliteConnectionStringBuilder { DataSource = databasePath, ForeignKeys = true }.ToString()).Options);
 public async Task<IReadOnlyList<InterviewSession>> ListAsync(Guid missionId)
 {
  await using var db = Open();
  var rows = await db.InterviewDocuments.AsNoTracking().Where(x => x.MissionId == missionId).ToListAsync();
  return rows.GroupBy(x => x.SessionId).Select(g => JsonSerializer.Deserialize<InterviewSession>(g.OrderByDescending(x => x.SavedUtc).First().Payload, QuestionnaireFormat.Json)!).OrderBy(x=>x.Title).ToList();
 }
 public async Task SaveAsync(InterviewSession session)
 {
  QuestionnaireFormat.ValidateSession(session);
  await using var db = Open();
  if (await db.InterviewDocuments.AnyAsync(x => x.SessionId == session.Id && x.MissionId != session.MissionId)) throw new ArgumentException("Session associée à une autre mission.");
  var previous=await db.InterviewDocuments.AsNoTracking().Where(x=>x.SessionId==session.Id).ToListAsync();
  if(previous.Count>0){var original=JsonSerializer.Deserialize<InterviewSession>(previous[0].Payload,QuestionnaireFormat.Json)!;if(JsonSerializer.Serialize(original.Questionnaire,QuestionnaireFormat.Json)!=JsonSerializer.Serialize(session.Questionnaire,QuestionnaireFormat.Json))throw new ArgumentException("La version du questionnaire d’un entretien est figée. Créez un nouvel entretien.");}
  db.InterviewDocuments.Add(new InterviewDocument { MissionId = session.MissionId, SessionId = session.Id, Payload = JsonSerializer.Serialize(session, QuestionnaireFormat.Json) });
  db.AuditEvents.Add(new AuditEvent { EntityId = session.Id, Operation = "Interview.SnapshotSaved" });
  await db.SaveChangesAsync();
 }
}
