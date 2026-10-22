using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CDG.AuditPilot.Infrastructure.Migrations;
[DbContext(typeof(AuditDbContext))]
[Migration("20261022104900_InterviewDocuments")]
public sealed class InterviewDocuments : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
 CREATE TABLE InterviewDocuments (Id TEXT NOT NULL PRIMARY KEY, MissionId TEXT NOT NULL, SessionId TEXT NOT NULL, SavedUtc TEXT NOT NULL, Payload TEXT NOT NULL, FOREIGN KEY(MissionId) REFERENCES Missions(Id) ON DELETE RESTRICT);
 CREATE INDEX IX_InterviewDocuments_MissionId_SessionId ON InterviewDocuments(MissionId, SessionId);
 """);
 protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE InterviewDocuments;");
}
