using CDG.AuditPilot.Domain;
namespace CDG.AuditPilot.Application;
public record ClientContact(Guid Id,Guid ClientId,string Name,string Function,string Email,string Phone);
public record ClientSite(Guid Id,Guid ClientId,string Name,string Address);
public record RiskMethod(int ReviewThreshold=8,int HighThreshold=15,int Version=1){public string Level(int score)=>score>=HighThreshold?"Élevé":score>=ReviewThreshold?"À surveiller":"Faible";}
public record Question(Guid Id,string Section,string Text,string Type);
public record Interview(Guid Id,Guid MissionId,string Person,string Notes,DateTime CreatedUtc);
public record Finding(Guid Id,Guid MissionId,string Description,string Evidence,int Impact,int Likelihood)
{
 public int Score=>Impact*Likelihood;
 public RiskMethod Scoring {get;init;}=new();
 public string RiskLevel=>Scoring.Level(Score);
 public string Criterion {get;init;}="";
 public string Cause {get;init;}="";
 public string Consequence {get;init;}="";
 public bool Validated {get;init;}
 public string Reviewer {get;init;}="";
 public Guid[] EvidenceIds {get;init;}=[];
}
public record AuditAction(Guid Id,Guid MissionId,string Description,string Owner,string Status)
{
 public Guid[] Dependencies {get;init;}=[];
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
 public List<ClientContact> Contacts {get;set;}=[];
 public List<ClientSite> Sites {get;set;}=[];
 public RiskMethod RiskMethod {get;set;}=new();
 public long Revision {get;set;}
 public void Validate() {
  if(RiskMethod.ReviewThreshold<1||RiskMethod.HighThreshold<=RiskMethod.ReviewThreshold||RiskMethod.HighThreshold>25)throw new ArgumentException("Seuils : 1 ≤ surveillance < élevé ≤ 25.");
  if(Contacts.Any(x=>!Clients.Any(c=>c.Id==x.ClientId))||Sites.Any(x=>!Clients.Any(c=>c.Id==x.ClientId)))throw new ArgumentException("Contact ou établissement sans client.");
  foreach(var action in Actions){if(action.Dependencies.Any(id=>id==action.Id||!Actions.Any(a=>a.Id==id&&a.MissionId==action.MissionId)))throw new ArgumentException("Dépendance d’action invalide.");var seen=new HashSet<Guid>();bool Visit(Guid id){if(!seen.Add(id))return true;var a=Actions.Single(x=>x.Id==id);foreach(var dependency in a.Dependencies)if(Visit(dependency))return true;seen.Remove(id);return false;}if(Visit(action.Id))throw new ArgumentException("Cycle de dépendances d’actions interdit.");}
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
