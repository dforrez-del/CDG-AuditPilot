using CDG.AuditPilot.Domain;
using Microsoft.EntityFrameworkCore;
namespace CDG.AuditPilot.Infrastructure;
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
 public DbSet<InterviewDocument> InterviewDocuments => Set<InterviewDocument>();
 public DbSet<Client> Clients => Set<Client>();
 public DbSet<Mission> Missions => Set<Mission>();
 public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
 protected override void OnModelCreating(ModelBuilder model)
 {
  model.Entity<InterviewDocument>().HasOne<Mission>().WithMany().HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<InterviewDocument>().HasIndex(x => new { x.MissionId, x.SessionId });
  model.Entity<Client>().Property(x => x.Name).IsRequired().HasMaxLength(200);
  model.Entity<Mission>().HasIndex(x => x.Reference).IsUnique();
  model.Entity<Mission>().HasOne<Client>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
  model.Entity<Mission>().Property(x => x.Reference).HasMaxLength(80).IsRequired();
  model.Entity<Mission>().Property(x => x.Title).HasMaxLength(200).IsRequired();
 }
}
