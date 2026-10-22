namespace CDG.AuditPilot.Domain;
public enum MissionStatus { Brouillon, Planifiee, EnCours, EnAttenteValidation, Terminee, Archivee }
public sealed class Client
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public string Name { get; set; } = "";
 public string Email { get; set; } = "";
 public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
public sealed class Mission
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public Guid ClientId { get; set; }
 public string Reference { get; set; } = "";
 public string Title { get; set; } = "";
 public string Scope { get; set; } = "";
 public MissionStatus Status { get; set; }
 public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
public sealed class AuditEvent
{
 public long Id { get; set; }
 public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
 public string Operation { get; set; } = "";
 public Guid EntityId { get; set; }
}
