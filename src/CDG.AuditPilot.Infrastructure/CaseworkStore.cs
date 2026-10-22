using System.Text.Json;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CDG.AuditPilot.Infrastructure;
public sealed class CaseworkStore(string databasePath)
{
 private AuditDbContext Open()=>new(new DbContextOptionsBuilder<AuditDbContext>().UseSqlite(new SqliteConnectionStringBuilder{DataSource=databasePath,ForeignKeys=true}.ToString()).Options);
 public AuditData Load() {
  using var db=Open();
  using var connection=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=databasePath}.ToString());connection.Open();
  using var cmd=connection.CreateCommand();cmd.CommandText="SELECT Payload FROM WorkspaceStates WHERE Id=1";
  var raw=cmd.ExecuteScalar() as string;
  var data=raw is null ? new AuditData():JsonSerializer.Deserialize<AuditData>(raw)??throw new InvalidDataException("Dossier illisible.");
  data.Clients=db.Clients.AsNoTracking().ToList();data.Missions=db.Missions.AsNoTracking().ToList();return data;
 }
 public void Save(AuditData data) {
  data.Validate();using var db=Open();using var tx=db.Database.BeginTransaction();
  foreach(var c in data.Clients){if(db.Clients.Find(c.Id) is { } old)db.Entry(old).CurrentValues.SetValues(c);else db.Clients.Add(c);}
  foreach(var m in data.Missions){if(string.IsNullOrEmpty(m.Reference))m.Reference="M-"+m.Id.ToString("N");if(db.Missions.Find(m.Id) is {} old)db.Entry(old).CurrentValues.SetValues(m);else db.Missions.Add(m);}
  db.AuditEvents.Add(new AuditEvent{Operation="Workspace.Saved"});db.SaveChanges();
  var revision=data.Revision;data.Revision++;
  try {
   string json=JsonSerializer.Serialize(data);
   int count=db.Database.ExecuteSqlInterpolated($"UPDATE WorkspaceStates SET Payload={json}, Revision={data.Revision} WHERE Id=1 AND Revision={revision}");
   if(count==0){if(revision!=0)throw new InvalidOperationException("Le dossier a été modifié par une autre session.");db.Database.ExecuteSqlInterpolated($"INSERT INTO WorkspaceStates(Id,Revision,Payload) VALUES(1,{data.Revision},{json})");}
   tx.Commit();
  }catch {data.Revision=revision;throw;}
 }
 public void ImportLegacy(string legacyPath) {
  if(!File.Exists(legacyPath))return;
  using var db=Open();using var c=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=databasePath}.ToString());c.Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT COUNT(*) FROM WorkspaceStates";if((long)cmd.ExecuteScalar()!>0)return;
  var data=JsonSerializer.Deserialize<AuditData>(File.ReadAllText(legacyPath))??throw new InvalidDataException("Ancien dossier illisible.");
  // Conserver aussi les dossiers déjà créés dans la première branche SQLite.
  data.Clients.AddRange(db.Clients.AsNoTracking().Where(x=>!data.Clients.Select(y=>y.Id).Contains(x.Id)).ToList());
  data.Missions.AddRange(db.Missions.AsNoTracking().Where(x=>!data.Missions.Select(y=>y.Id).Contains(x.Id)).ToList());
  File.Copy(legacyPath,legacyPath+".before-sqlite",true);Save(data);
 }
}
