using CDG.AuditPilot.Domain;
namespace CDG.AuditPilot.Application;
public record Question(Guid Id,string Section,string Text,string Type);
public record Interview(Guid Id,Guid MissionId,string Person,string Notes,DateTime CreatedUtc);
public record Finding(Guid Id,Guid MissionId,string Description,string Evidence,int Impact,int Likelihood)
{
 public int Score=>Impact*Likelihood;
 public string Criterion {get;init;}="";
 public string Cause {get;init;}="";
 public string Consequence {get;init;}="";
 public bool Validated {get;init;}
 public string Reviewer {get;init;}="";
 public Guid[] EvidenceIds {get;init;}=[];
}
public record AuditAction(Guid Id,Guid MissionId,string Description,string Owner,string Status)
{
 public DateTime? DueDate {get;init;}
 public string Priority {get;init;}="Normale";
 public string Effectiveness {get;init;}="";
 public decimal EstimatedHours {get;init;}
 public decimal EstimatedCost {get;init;}
}
public sealed record Evidence(Guid Id,Guid MissionId,string OriginalFileName,string Sha256,long SizeBytes,DateTimeOffset ImportedAtUtc,string Source,string StoredRelativePath);
public class AuditData {
 public List<Client> Clients {get;set;}=[];
 public List<Mission> Missions {get;set;}=[];
 public List<Question> Questions {get;set;}=[];
 public List<Interview> Interviews {get;set;}=[];
 public List<Finding> Findings {get;set;}=[];
 public List<AuditAction> Actions {get;set;}=[];
 public List<Evidence> Evidence {get;set;}=[];
 public long Revision {get;set;}
 public void Validate() {
  var ids=Missions.Select(x=>x.Id).ToHashSet();
  if(Missions.Any(m=>!Clients.Any(c=>c.Id==m.ClientId)))throw new ArgumentException("Client de mission absent.");
  if(Interviews.Any(x=>!ids.Contains(x.MissionId))||Findings.Any(x=>!ids.Contains(x.MissionId))||Actions.Any(x=>!ids.Contains(x.MissionId))||Evidence.Any(x=>!ids.Contains(x.MissionId)))throw new ArgumentException("Mission absente.");
  foreach(var f in Findings){
   if(f.Impact is <1 or >5 || f.Likelihood is <1 or >5)throw new ArgumentException("Cotation attendue de 1 à 5.");
   if(f.EvidenceIds.Any(id=>!Evidence.Any(e=>e.Id==id&&e.MissionId==f.MissionId)))throw new ArgumentException("Preuve absente ou d’une autre mission.");
   if(f.Validated && (string.IsNullOrWhiteSpace(f.Reviewer)|| (f.EvidenceIds.Length==0&&string.IsNullOrWhiteSpace(f.Evidence))))throw new ArgumentException("Validation : responsable et preuve ou mention preuve absente obligatoires.");
  }
 }
}
