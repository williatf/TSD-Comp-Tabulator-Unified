using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Services;

/// <summary>
/// Generates duet award reports using the event DB's class snapshot for ordering and bucket resolution.
/// </summary>
public sealed class DuetAwardReportService : IDuetAwardReportService
{
    private readonly IScoreRepository _repository;
    private readonly IClassConfigService _classConfig;
    private readonly string _eventDbPath;
    private const int MaxEntriesPerGroup = 12;

    public DuetAwardReportService(IScoreRepository repository, IClassConfigService classConfigService, string eventDbPath)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _classConfig = classConfigService ?? throw new ArgumentNullException(nameof(classConfigService));
        _eventDbPath = eventDbPath ?? throw new ArgumentNullException(nameof(eventDbPath));
    }

    public async Task<DuetAwardReport> GenerateReportAsync()
    {
        var candidates = (await _repository.GetDuetAwardsCandidatesAsync()).ToList();

        // Resolve class keys for each candidate using the event snapshot
        var enriched = new List<(DuetAwardCandidate Candidate, string? ClassKey)>();
        foreach (var c in candidates)
        {
            var key = await _classConfig.ResolveClassKeyAsync(c.Class, _eventDbPath);
            enriched.Add((c, key));
        }

        // Gather class definitions for ordering
        var defs = (await _classConfig.GetClassDefinitionsAsync(_eventDbPath)).ToList();

        // Helper for bucket ordering: Studio first, then School
        static int BucketPriority(string? bucket) =>
            bucket?.Equals("studio", StringComparison.OrdinalIgnoreCase) == true ? 1 :
            bucket?.Equals("school", StringComparison.OrdinalIgnoreCase) == true ? 2 : 99;

        // Build groups keyed by resolved class key or by literal class text if unresolved
        var groups = enriched
            .GroupBy(x => x.ClassKey ?? x.Candidate.Class?.Trim() ?? string.Empty)
            .Select(g =>
            {
                var key = g.Key;
                // find definition if exists
                var def = defs.FirstOrDefault(d => string.Equals(d.ClassKey, key, StringComparison.OrdinalIgnoreCase));
                var bucket = def?.Bucket ?? string.Empty;
                var displayName = def?.DisplayName ?? key;
                var sortOrder = def?.SortOrder ?? 1000;
                var candidatesInGroup = g.Select(x => x.Candidate).ToList();
                return new
                {
                    ClassKey = key,
                    DisplayName = displayName,
                    Bucket = bucket,
                    SortOrder = sortOrder,
                    Candidates = candidatesInGroup
                };
            })
            .OrderBy(g => BucketPriority(g.Bucket))   // Studio first, then School
            .ThenBy(g => g.SortOrder)                 // class SortOrder from ClassDefinitions
            .ThenBy(g => g.DisplayName)
            .ToList();

        var groupsResult = new List<DuetAwardGroup>();
        foreach (var g in groups)
        {
            groupsResult.Add(CreateGroup(g.Bucket, g.DisplayName, g.Candidates));
        }

        return new DuetAwardReport(groupsResult);
    }

    private static DuetAwardGroup CreateGroup(string bucket, string @class, IEnumerable<DuetAwardCandidate> candidates)
    {
        // Sort by FinalScore descending
        var sorted = candidates.OrderByDescending(c => c.FinalScore).ToList();

        var entries = new List<DuetAwardEntry>();
        int currentPlace = 1;
        double? previousScore = null;
        int countAtCurrentScore = 0;
        int totalEntriesAdded = 0;

        foreach (var candidate in sorted)
        {
            // Stop after top 12 entries (but include ties at 12th place)
            if (totalEntriesAdded >= MaxEntriesPerGroup &&
                (!previousScore.HasValue || Math.Abs(candidate.FinalScore - previousScore.Value) >= 0.0001))
            {
                break;
            }

            // Check if this is a tie with previous score
            if (previousScore.HasValue && Math.Abs(candidate.FinalScore - previousScore.Value) < 0.0001)
            {
                // Tie - same place
                countAtCurrentScore++;
            }
            else
            {
                // New score - advance place by number of entries at previous score
                if (previousScore.HasValue)
                {
                    currentPlace += countAtCurrentScore;
                }
                countAtCurrentScore = 1;
                previousScore = candidate.FinalScore;
            }

            entries.Add(new DuetAwardEntry(
                currentPlace,
                candidate.FinalScore,
                candidate.ProgramNumber,
                candidate.Participants,
                candidate.StudioName,
                candidate.RoutineTitle
            ));

            totalEntriesAdded++;
        }

        return new DuetAwardGroup(bucket, @class, entries);
    }
}
