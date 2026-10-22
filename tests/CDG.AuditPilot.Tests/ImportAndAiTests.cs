using System.Net;
using System.Text;
using System.Text.Json;
using CDG.AuditPilot.Application;
using CDG.AuditPilot.AI;
using CDG.AuditPilot.Infrastructure;
using Xunit;
namespace CDG.AuditPilot.Tests;
public sealed class ImportAndAiTests
{
 private const string Json = """
 {"schemaVersion":1,"title":"Audit flux","sections":[{"title":"Achats","questions":[{"id":"Q1","label":"Qui valide ?","type":"text","required":true,"options":[]}]}]}
 """;
 private static InterviewSession Session() => new(Guid.NewGuid(),Guid.NewGuid(),QuestionnaireFormat.Parse(Json),new(){{"Q1","Le responsable"}},[]);
 [Fact] public void ImportPreservesQuestionIdentifiers() => Assert.Equal("Q1", QuestionnaireFormat.Parse(Json).Sections[0].Questions[0].Id);
 [Theory]
 [InlineData("\"schemaVersion\":1", "\"schemaVersion\":2")]
 [InlineData("\"type\":\"text\"", "\"type\":\"attachment\"")]
 [InlineData("\"options\":[]", "\"options\":[],\"condition\":true")]
 public void InvalidImportsAreRejected(string before,string after) => Assert.Throws<ArgumentException>(()=>QuestionnaireFormat.Parse(Json.Replace(before,after)));
 [Fact] public void DuplicateIdsAreRejected()
 {
  var q=QuestionnaireFormat.Parse(Json);var duplicate=q with {Sections=[new("Achats",[q.Sections[0].Questions[0],q.Sections[0].Questions[0]])]};
  Assert.Throws<ArgumentException>(()=>QuestionnaireFormat.Parse(JsonSerializer.Serialize(duplicate,QuestionnaireFormat.Json)));
 }
 [Fact] public void TypedAnswersAreValidated()
 {
  Assert.NotNull(QuestionnaireFormat.ValidateAnswer(new("Q","Score","scale0to5",true,[]),"8"));
  Assert.Null(QuestionnaireFormat.ValidateAnswer(new("Q","Date","date",true,[]),"2026-10-22"));
 }
 [Fact] public async Task SavedSessionsSurviveReopenAndStayInTheirMission()
 {
  var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);var db=Path.Combine(dir,"test.db");
  try
  {
   var workspace=new AuditWorkspace(db);await workspace.InitializeAsync();var c=await workspace.CreateClientAsync("Test","");
   var m=await workspace.CreateMissionAsync(c.Id,"M1","Test","");var s=Session() with {MissionId=m.Id};
   await new QuestionnaireStore(db).SaveAsync(s);s.Answers["Q1"]="Version 2";await new QuestionnaireStore(db).SaveAsync(s);
   var loaded=Assert.Single(await new QuestionnaireStore(db).ListAsync(m.Id));Assert.Equal("Version 2",loaded.Answers["Q1"]);
   Assert.Empty(await new QuestionnaireStore(db).ListAsync(Guid.NewGuid()));
  }
  finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();Directory.Delete(dir,true); }
 }
 [Fact] public async Task NoHttpCallWithoutConsent()
 {
  var fake=new FakeHandler();using var http=new HttpClient(fake);var provider=new CompatibleChatProvider(http);
  var request=provider.Prepare(Session(),"https://example.test/v1/chat/completions","test-model");
  await Assert.ThrowsAsync<InvalidOperationException>(()=>provider.AnalyzeAsync(request,"test-key",false,CancellationToken.None));Assert.Equal(0,fake.Calls);
 }
 [Fact] public async Task ApprovedPayloadIsSentExactlyAndResultIsUnvalidated()
 {
  var fake=new FakeHandler();using var http=new HttpClient(fake);var provider=new CompatibleChatProvider(http);
  var request=provider.Prepare(Session(),"https://example.test/v1/chat/completions","test-model");
  var result=await provider.AnalyzeAsync(request,"test-key",true,CancellationToken.None);
  Assert.Equal(request.Json,fake.Body);Assert.Equal(request.Json,result.RequestJson);Assert.Equal("Proposition non validée",result.Status);Assert.DoesNotContain("test-key",result.RequestJson);
 }
 [Fact] public async Task TruncatedResponsesAreRejected()
 {
  var fake=new FakeHandler{Finish="length"};using var http=new HttpClient(fake);var provider=new CompatibleChatProvider(http);
  await Assert.ThrowsAsync<InvalidDataException>(()=>provider.AnalyzeAsync(provider.Prepare(Session(),"https://example.test/v1/chat/completions","test-model"),"test-key",true,CancellationToken.None));
 }
 [Fact] public void PlainHttpIsRejected()
 {
  using var http=new HttpClient(new FakeHandler());Assert.Throws<ArgumentException>(()=>new CompatibleChatProvider(http).Prepare(Session(),"http://example.test/v1/chat/completions","test"));
 }
 private sealed class FakeHandler : HttpMessageHandler
 {
  public int Calls;public string? Body;public string Finish="stop";
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
  {
   Calls++;Body=await request.Content!.ReadAsStringAsync(ct);
   return new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new{choices=new[]{new{finish_reason=Finish,message=new{content="[Q1] Déclaration à vérifier."}}}}),Encoding.UTF8,"application/json")};
  }
 }
}
