using CDG.AuditPilot.Domain;
namespace CDG.AuditPilot.Application;
public interface IAuditWorkspace
{
 Task InitializeAsync(CancellationToken ct = default);
 Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken ct = default);
 Task<IReadOnlyList<Mission>> GetMissionsAsync(Guid clientId, CancellationToken ct = default);
 Task<Client> CreateClientAsync(string name, string email, CancellationToken ct = default);
 Task<Mission> CreateMissionAsync(Guid clientId, string reference, string title, string scope, CancellationToken ct = default);
}
