using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CDG.AuditPilot.Infrastructure.Migrations;
[DbContext(typeof(AuditDbContext))]
[Migration("202610220001_InitialSchema")]
public sealed class InitialSchema : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.Sql("""
  CREATE TABLE Clients (Id TEXT NOT NULL PRIMARY KEY, Name TEXT NOT NULL CHECK(length(trim(Name)) BETWEEN 1 AND 200), Email TEXT NOT NULL, CreatedUtc TEXT NOT NULL);
  CREATE TABLE Missions (Id TEXT NOT NULL PRIMARY KEY, ClientId TEXT NOT NULL, Reference TEXT NOT NULL CHECK(length(Reference) BETWEEN 1 AND 80), Title TEXT NOT NULL CHECK(length(trim(Title)) BETWEEN 1 AND 200), Scope TEXT NOT NULL, Status INTEGER NOT NULL CHECK(Status BETWEEN 0 AND 5), CreatedUtc TEXT NOT NULL, FOREIGN KEY(ClientId) REFERENCES Clients(Id) ON DELETE RESTRICT);
  CREATE UNIQUE INDEX IX_Missions_Reference ON Missions(Reference);
  CREATE INDEX IX_Missions_ClientId ON Missions(ClientId);
  CREATE TABLE AuditEvents (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, TimestampUtc TEXT NOT NULL, Operation TEXT NOT NULL, EntityId TEXT NOT NULL);
  """);
 }
 protected override void Down(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.Sql("DROP TABLE AuditEvents; DROP TABLE Missions; DROP TABLE Clients;");
 }
}
