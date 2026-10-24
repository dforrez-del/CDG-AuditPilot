using CDG.AuditPilot.Infrastructure;
using Xunit;
namespace CDG.AuditPilot.Tests;
public sealed class WorkspaceTests : IDisposable
{
 private readonly string folder = Path.Combine(Path.GetTempPath(), "AuditPilotTests", Guid.NewGuid().ToString("N"));
 private string Database => Path.Combine(folder, "test.db");
 [Fact]
 public async Task ClientAndMissionSurviveReopening()
 {
  var first = new AuditWorkspace(Database); await first.InitializeAsync();
  var client = await first.CreateClientAsync("Entreprise test", "");
  var mission = await first.CreateMissionAsync(client.Id, "audit-001", "Organisation", "Flux documentaires");
  var second = new AuditWorkspace(Database); await second.InitializeAsync();
  Assert.Equal(client.Id, Assert.Single(await second.GetClientsAsync()).Id);
  var restored = Assert.Single(await second.GetMissionsAsync(client.Id));
  Assert.Equal(mission.Id, restored.Id); Assert.Equal("AUDIT-001", restored.Reference);
 }
 [Fact]
 public async Task MissionsAreIsolatedByClient()
 {
  var store = new AuditWorkspace(Database); await store.InitializeAsync();
  var a = await store.CreateClientAsync("A", ""); var b = await store.CreateClientAsync("B", "");
  await store.CreateMissionAsync(a.Id, "A-1", "Audit A", "");
  Assert.Empty(await store.GetMissionsAsync(b.Id));
 }
 [Fact]
 public async Task DuplicateReferencesAreRejectedWithoutExtraMission()
 {
  var store = new AuditWorkspace(Database); await store.InitializeAsync();
  var client = await store.CreateClientAsync("A", "");
  await store.CreateMissionAsync(client.Id, "A-1", "Audit A", "");
  await Assert.ThrowsAsync<ArgumentException>(() => store.CreateMissionAsync(client.Id, " a-1 ", "Autre", ""));
  Assert.Single(await store.GetMissionsAsync(client.Id));
 }
 [Fact]
 public async Task InvalidInputDoesNotWriteData()
 {
  var store = new AuditWorkspace(Database); await store.InitializeAsync();
  await Assert.ThrowsAsync<ArgumentException>(() => store.CreateClientAsync(" ", ""));
  await Assert.ThrowsAsync<ArgumentException>(() => store.CreateClientAsync("A", "invalide"));
  await Assert.ThrowsAsync<ArgumentException>(() => store.CreateMissionAsync(Guid.NewGuid(), "A", "Mission", ""));
  Assert.Empty(await store.GetClientsAsync());
 }
 public void Dispose()
 {
  Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
  if (Directory.Exists(folder)) Directory.Delete(folder, true);
 }
}
