using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CDG.AuditPilot.Desktop.Models;

public sealed record AuditEvidence(
    Guid Id,
    Guid MissionId,
    string OriginalFileName,
    string Sha256,
    long SizeBytes,
    DateTimeOffset ImportedAtUtc,
    string Source,
    string StoredRelativePath);

public static class AuditEvidenceHasher
{
    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
