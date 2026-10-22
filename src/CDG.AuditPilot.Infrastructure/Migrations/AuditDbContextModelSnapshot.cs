using CDG.AuditPilot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
namespace CDG.AuditPilot.Infrastructure.Migrations;
[DbContext(typeof(AuditDbContext))]
public sealed class AuditDbContextModelSnapshot : ModelSnapshot
{
 protected override void BuildModel(ModelBuilder model)
 {
  model.HasAnnotation("ProductVersion", "10.0.0");
  model.Entity<InterviewDocument>(e => { e.Property(x=>x.Id); e.HasKey(x=>x.Id); e.Property(x=>x.MissionId); e.Property(x=>x.SessionId); e.Property(x=>x.SavedUtc); e.Property(x=>x.Payload).IsRequired(); e.HasIndex(x=>new {x.MissionId,x.SessionId}); e.HasOne<Mission>().WithMany().HasForeignKey(x=>x.MissionId).OnDelete(DeleteBehavior.Restrict); e.ToTable("InterviewDocuments"); });
  model.Entity<Client>(e => { e.Property(x=>x.Id); e.HasKey(x=>x.Id); e.Property(x=>x.Name).IsRequired().HasMaxLength(200); e.Property(x=>x.Email).IsRequired(); e.Property(x=>x.CreatedUtc); e.ToTable("Clients"); });
  model.Entity<Mission>(e => { e.Property(x=>x.Id); e.HasKey(x=>x.Id); e.Property(x=>x.ClientId); e.Property(x=>x.Reference).IsRequired().HasMaxLength(80); e.Property(x=>x.Title).IsRequired().HasMaxLength(200); e.Property(x=>x.Scope).IsRequired(); e.Property(x=>x.Status); e.Property(x=>x.CreatedUtc); e.HasIndex(x=>x.Reference).IsUnique(); e.HasIndex(x=>x.ClientId); e.HasOne<Client>().WithMany().HasForeignKey(x=>x.ClientId).OnDelete(DeleteBehavior.Restrict); e.ToTable("Missions"); });
  model.Entity<AuditEvent>(e => { e.Property(x=>x.Id).ValueGeneratedOnAdd(); e.HasKey(x=>x.Id); e.Property(x=>x.TimestampUtc); e.Property(x=>x.Operation).IsRequired(); e.Property(x=>x.EntityId); e.ToTable("AuditEvents"); });
 }
}
