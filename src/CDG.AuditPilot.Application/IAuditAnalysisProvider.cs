namespace CDG.AuditPilot.Application;
public sealed record PreparedAnalysis(Uri Endpoint, string Model, string Json);
public interface IAuditAnalysisProvider
{
 PreparedAnalysis Prepare(InterviewSession session, string endpoint, string model);
 Task<AnalysisDraft> AnalyzeAsync(PreparedAnalysis request, string apiKey, bool authorized, CancellationToken ct);
}
