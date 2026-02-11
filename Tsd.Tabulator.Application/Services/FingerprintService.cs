using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Tsd.Tabulator.Application.Interfaces;

namespace Tsd.Tabulator.Application.Services;

public sealed class FingerprintService : IFingerprintService
{
    private static readonly Regex Ws = new(@"\s+", RegexOptions.Compiled);

    public string NormalizeName(string name)
    {
        var s = (name ?? "").Trim();
        s = Ws.Replace(s, " ");
        return s.ToUpperInvariant();
    }

    public IReadOnlyList<string> SplitAndNormalizeParticipants(string participantsRaw)
    {
        if (string.IsNullOrWhiteSpace(participantsRaw))
            return Array.Empty<string>();

        // CSV field is already a single string; participants inside are comma-separated
        var parts = participantsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeName)
            .Where(x => x.Length > 0)
            .OrderBy(x => x)
            .ToList();

        return parts;
    }

    public string ComputeRoutineFingerprint(
        string studioName,
        string routineTitle,
        string entryType,
        string? category,
        string? @class,
        IReadOnlyList<string> participantsNormalized)
    {
        // IMPORTANT: does NOT include ProgramNumber / EntryID
        var key = string.Join("|", new[]
        {
            NormalizeName(studioName),
            NormalizeName(routineTitle),
            NormalizeName(entryType),
            NormalizeName(category ?? ""),
            NormalizeName(@class ?? ""),
            string.Join(",", participantsNormalized)
        });

        var bytes = Encoding.UTF8.GetBytes(key);
        var hash = SHA1.HashData(bytes);
        return Convert.ToHexString(hash); // e.g. "A1B2..."
    }
}
