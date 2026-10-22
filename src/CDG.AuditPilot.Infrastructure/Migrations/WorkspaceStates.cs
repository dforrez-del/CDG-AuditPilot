using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CDG.AuditPilot.Infrastructure.Migrations;
[DbContext(typeof(AuditDbContext))]
[Migration("20261022160000_WorkspaceStates")]
public sealed class WorkspaceStates:Migration {
 protected override void Up(MigrationBuilder b)=>b.Sql("CREATE TABLE WorkspaceStates (Id INTEGER NOT NULL PRIMARY KEY CHECK(Id=1), Revision INTEGER NOT NULL, Payload TEXT NOT NULL);");
 protected override void Down(MigrationBuilder b)=>b.Sql("DROP TABLE WorkspaceStates;");
}
