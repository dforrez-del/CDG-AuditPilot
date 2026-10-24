using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CDG.AuditPilot.Infrastructure.Migrations;
[DbContext(typeof(AuditDbContext))]
[Migration("20261022170000_WorkspaceRevisions")]
public sealed class WorkspaceRevisions:Migration {
 protected override void Up(MigrationBuilder b)=>b.Sql("CREATE TABLE WorkspaceRevisions (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, SavedUtc TEXT NOT NULL, Actor TEXT NOT NULL, Payload TEXT NOT NULL);");
 protected override void Down(MigrationBuilder b)=>b.Sql("DROP TABLE WorkspaceRevisions;");
}
