namespace CDG.AuditPilot.Domain;
public sealed class InterviewDocument
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public Guid MissionId { get; set; }
 public Guid SessionId { get; set; }
 public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
 public string Payload { get; set; } = "";
}
