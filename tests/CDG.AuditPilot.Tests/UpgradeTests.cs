using CDG.AuditPilot.Domain;
using CDG.AuditPilot.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
namespace CDG.AuditPilot.Tests;
public sealed class UpgradeTests
{
 [Fact]
 public async Task PreviousDatabaseCanBeUpgradedWithoutLosingClients()
 {
  var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")+".db");
  try
  {
   var options=new DbContextOptionsBuilder<AuditDbContext>().UseSqlite($"Data Source={path}").Options;
   await using(var db=new AuditDbContext(options))
   {
    // Reconstituer le schéma et l’historique exacts de la première version.
    var initial = new CDG.AuditPilot.Infrastructure.Migrations.InitialSchema();
    foreach (var command in db.GetService<IMigrationsSqlGenerator>().Generate(initial.UpOperations))
     await db.Database.ExecuteSqlRawAsync(command.CommandText);
    await db.Database.ExecuteSqlRawAsync(db.GetService<IHistoryRepository>().GetCreateScript());
    await db.Database.ExecuteSqlRawAsync(db.GetService<IHistoryRepository>().GetInsertScript(new HistoryRow("202610220001_InitialSchema", "10.0.0")));
    db.Clients.Add(new Client{Name="Client conservé"});await db.SaveChangesAsync();
   }
   var workspace=new AuditWorkspace(path);await workspace.InitializeAsync();
   Assert.Equal("Client conservé",Assert.Single(await workspace.GetClientsAsync()).Name);
   Assert.Empty(await new QuestionnaireStore(path).ListAsync(Guid.NewGuid()));
  }
  finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();if(File.Exists(path))File.Delete(path); }
 }
}
