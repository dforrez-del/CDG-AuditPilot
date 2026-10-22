using CDG.AuditPilot.Application;
using CDG.AuditPilot.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CDG.AuditPilot.Infrastructure;
public sealed class AuditWorkspace : IAuditWorkspace
{
 private readonly DbContextOptions<AuditDbContext> options;
 public AuditWorkspace(string databasePath)
 {
  var path = Path.GetFullPath(databasePath);
  Directory.CreateDirectory(Path.GetDirectoryName(path)!);
  options = new DbContextOptionsBuilder<AuditDbContext>().UseSqlite(new SqliteConnectionStringBuilder { DataSource = path, ForeignKeys = true }.ToString()).Options;
 }
 public async Task InitializeAsync(CancellationToken ct = default)
 {
  await using var db = new AuditDbContext(options);
  await db.Database.MigrateAsync(ct);
 }
 public async Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken ct = default)
 {
  await using var db = new AuditDbContext(options);
  return await db.Clients.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
 }
 public async Task<IReadOnlyList<Mission>> GetMissionsAsync(Guid clientId, CancellationToken ct = default)
 {
  await using var db = new AuditDbContext(options);
  return await db.Missions.AsNoTracking().Where(x => x.ClientId == clientId).OrderBy(x => x.Reference).ToListAsync(ct);
 }
 public async Task<Client> CreateClientAsync(string name, string email, CancellationToken ct = default)
 {
  name = Required(name, "La raison sociale", 200);
  email = email.Trim();
  if (email.Length > 254 || (email.Length > 0 && !System.Net.Mail.MailAddress.TryCreate(email, out _)))
   throw new ArgumentException("L’adresse e-mail est invalide.");
  var client = new Client { Name = name, Email = email };
  await using var db = new AuditDbContext(options);
  db.Clients.Add(client);
  db.AuditEvents.Add(new AuditEvent { Operation = "Client.Created", EntityId = client.Id });
  await db.SaveChangesAsync(ct);
  return client;
 }
 public async Task<Mission> CreateMissionAsync(Guid clientId, string reference, string title, string scope, CancellationToken ct = default)
 {
  reference = Required(reference, "La référence", 80).ToUpperInvariant();
  title = Required(title, "L’intitulé", 200);
  if (scope.Length > 10000) throw new ArgumentException("Le périmètre dépasse 10 000 caractères.");
  await using var db = new AuditDbContext(options);
  if (!await db.Clients.AnyAsync(x => x.Id == clientId, ct)) throw new ArgumentException("Sélectionnez un client existant.");
  if (await db.Missions.AnyAsync(x => x.Reference == reference, ct)) throw new ArgumentException("Cette référence de mission existe déjà.");
  var mission = new Mission { ClientId = clientId, Reference = reference, Title = title, Scope = scope.Trim() };
  db.Missions.Add(mission);
  db.AuditEvents.Add(new AuditEvent { Operation = "Mission.Created", EntityId = mission.Id });
  try { await db.SaveChangesAsync(ct); }
  catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
  { throw new ArgumentException("Cette référence de mission existe déjà.", ex); }
  return mission;
 }
 private static string Required(string value, string label, int max)
 {
  value = value.Trim();
  if (value.Length == 0 || value.Length > max) throw new ArgumentException($"{label} doit contenir entre 1 et {max} caractères.");
  return value;
 }
}
