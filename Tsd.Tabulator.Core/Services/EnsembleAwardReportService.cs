using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reports.d_Ensemble;
using Tsd.Tabulator.Core.Reporting;

namespace Tsd.Tabulator.Core.Services;

/// <summary>
/// Generates the Ensemble Award Report using the canonical class-definition
/// pipeline shared by Solo/Duet/Trio, but producing a deeper hierarchy:
///
///     Bucket → Class → Type → Items
///
/// The service:
///  1. Loads raw Ensemble routines and computes final scores.
///  2. Normalizes class text to canonical ClassKeys using ResolveClassKeyAsync.
///  3. Joins candidates to class definitions (bucket, display name, sort order).
///  4. Groups by Bucket → Class → Type.
///  5. Selects top-scoring entries per type and assigns places with tie handling.
///  6. Produces an EnsembleAwardReport containing the full hierarchy.
/// </summary>
public sealed class EnsembleAwardReportService : IEnsembleAwardReportService
{
    private readonly IScoreRepository _repository;
    private readonly IClassConfigService _classConfig;
    private readonly string _eventDbPath;

    private const int MaxEntriesPerGroup = 12;

    public EnsembleAwardReportService(
        IScoreRepository repository,
        IClassConfigService classConfigService,
        string eventDbPath)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _classConfig = classConfigService ?? throw new ArgumentNullException(nameof(classConfigService));
        _eventDbPath = eventDbPath ?? throw new ArgumentNullException(nameof(eventDbPath));
    }

    /// <summary>
    /// Generates the complete Ensemble Award Report by:
    ///  - Loading raw candidates
    ///  - Normalizing class keys
    ///  - Joining class definitions
    ///  - Grouping by Bucket → Class → Type
    ///  - Selecting winners and assigning places
    /// </summary>
    public async Task<EnsembleAwardReport> GenerateReportAsync()
    {
        // 1. Load raw candidates (unnormalized)
        var candidates = await LoadCandidatesAsync();

        // 2. Normalize class keys using the same mechanism as Solo/Duet/Trio
        var enriched = new List<(EnsembleAwardCandidate Candidate, string? ClassKey)>();
        foreach (var c in candidates)
        {
            var key = await _classConfig.ResolveClassKeyAsync(c.Division, _eventDbPath);
            enriched.Add((c, key));
        }

        // 3. Load class definitions for bucket, display name, sort order
        var defs = (await _classConfig.GetClassDefinitionsAsync(_eventDbPath)).ToList();

        // 4. Build canonical class groups (ClassKey → metadata → candidates)
        var classGroups = enriched
            .GroupBy(x => x.ClassKey ?? x.Candidate.Division.Trim())
            .Select(g =>
            {
                var key = g.Key;
                var def = defs.FirstOrDefault(d =>
                    string.Equals(d.ClassKey, key, StringComparison.OrdinalIgnoreCase));

                var displayName = def?.DisplayName ?? key;
                var bucket = def?.Bucket ?? string.Empty;
                var sortOrder = def?.SortOrder ?? 1000;

                return new
                {
                    ClassKey = key,
                    DisplayName = displayName,
                    Bucket = bucket,
                    SortOrder = sortOrder,
                    Candidates = g.Select(x => x.Candidate).ToList()
                };
            })
            .OrderBy(g => BucketPriority(g.Bucket))
            .ThenBy(g => g.SortOrder)
            .ThenBy(g => g.DisplayName)
            .ToList();

        // 5. Build the full hierarchy: Bucket → Class → Type → Items
        var buckets = classGroups
            .GroupBy(g => g.Bucket)
            .OrderBy(g => BucketPriority(g.Key))
            .Select(bucketGroup => new EnsembleBucketGroup(
                bucketGroup.Key,
                bucketGroup
                    .Select(classGroup =>
                    {
                        // Group candidates by Ensemble Type (Small/Medium/Large/XL)
                        var typeGroups = classGroup.Candidates
                            .GroupBy(c => c.Type)
                            .OrderBy(g => EnsembleTypePriority(g.Key))
                            .Select(typeGroup =>
                            {
                                // Select top-scoring entries
                                var winners = typeGroup
                                    .OrderByDescending(c => c.FinalScore)
                                    .ThenBy(c => c.RoutineId)
                                    .Take(MaxEntriesPerGroup)
                                    .ToList();

                                // Assign places with tie handling
                                var placed = AssignPlaces(winners);

                                // Convert to final award entries
                                var entries = placed.Select(t =>
                                    new EnsembleAwardEntry(
                                        place: t.Place,
                                        finalScore: t.Candidate.FinalScore,
                                        programNumber: t.Candidate.ProgramNumber,
                                        studioName: t.Candidate.StudioName,
                                        routineTitle: t.Candidate.RoutineTitle,
                                        classKey: classGroup.ClassKey,   // canonical
                                        type: t.Candidate.Type,
                                        division: t.Candidate.Division
                                    )).ToList();

                                return new EnsembleTypeGroup(typeGroup.Key, entries);
                            })
                            .ToList();

                        return new EnsembleClassGroup(
                            classGroup.ClassKey,
                            classGroup.DisplayName,
                            classGroup.SortOrder,
                            typeGroups
                        );
                    })
                    .ToList()
            ))
            .ToList();

        return new EnsembleAwardReport(buckets);
    }

    /// <summary>
    /// Loads raw Ensemble candidates from the score repository.
    /// Computes final scores but does NOT normalize class keys.
    /// This mirrors the Solo/Duet/Trio pattern: raw → normalized → grouped.
    /// </summary>
    private async Task<IReadOnlyList<EnsembleAwardCandidate>> LoadCandidatesAsync()
    {
        var routines = await _repository.GetScoredRoutinesAsync("%Ensemble%");
        var candidates = new List<EnsembleAwardCandidate>();

        foreach (var r in routines)
        {
            if (string.IsNullOrWhiteSpace(r.LastSheetKey))
                continue;

            var scoreCells = await _repository.GetScoreCellsAsync(r.RoutineId, r.LastSheetKey);
            var finalScore = scoreCells.Sum(s => s.Value);

            candidates.Add(new EnsembleAwardCandidate
            {
                RoutineId = r.RoutineId,
                Division = r.Class,                 // raw class text
                Type = r.EntryType,
                ProgramNumber = r.ProgramNumber,
                StudioName = r.StudioName,
                RoutineTitle = r.RoutineTitle,
                FinalScore = (double)finalScore
            });
        }

        return candidates;
    }

    /// <summary>
    /// Assigns places to candidates, handling ties by:
    ///  - Sharing the same place number for tied scores
    ///  - Skipping the appropriate number of places after ties
    /// </summary>
    private static List<(EnsembleAwardCandidate Candidate, int Place)> AssignPlaces(List<EnsembleAwardCandidate> candidates)
    {
        var result = new List<(EnsembleAwardCandidate, int)>();
        int place = 1;
        int skip = 1;
        double? lastScore = null;

        foreach (var c in candidates)
        {
            if (lastScore != null && c.FinalScore != lastScore)
            {
                place += skip;
                skip = 1;
            }
            else if (lastScore != null)
            {
                skip++;
            }

            result.Add((c, place));
            lastScore = c.FinalScore;
        }

        return result;
    }

    /// <summary>
    /// Converts a program number string to a long, returning 0 if invalid.
    /// </summary>
    private static long ParseProgramNumber(string? value)
    {
        if (long.TryParse(value, out var number))
            return number;

        return 0;
    }

    /// <summary>
    /// Extracts Ensemble size/type from the entry type string.
    /// </summary>
    private static string ExtractEnsembleType(string entryType)
    {
        if (entryType.Contains("Small", StringComparison.OrdinalIgnoreCase)) return "Small";
        if (entryType.Contains("Medium", StringComparison.OrdinalIgnoreCase)) return "Medium";
        if (entryType.Contains("Large", StringComparison.OrdinalIgnoreCase)) return "Large";
        if (entryType.Contains("XL", StringComparison.OrdinalIgnoreCase)) return "XL";
        return "Other";
    }

    /// <summary>
    /// Orders buckets consistently: Studio → School → Select School → Elite School.
    /// </summary>
    private static int BucketPriority(string? bucket) =>
        bucket?.Equals("studio", StringComparison.OrdinalIgnoreCase) == true ? 1 :
        bucket?.Equals("school", StringComparison.OrdinalIgnoreCase) == true ? 2 :
        bucket?.Equals("select school", StringComparison.OrdinalIgnoreCase) == true ? 3 :
        bucket?.Equals("elite school", StringComparison.OrdinalIgnoreCase) == true ? 4 :
        99;

    private static readonly (string Match, string Level)[] LevelRules =
    {
        ("Middle School", "Middle School"),
        ("High School", "High School"),
        ("Classic High School", "High School"),
        ("Select High School", "High School"),
        ("J.V.", "J.V."),
        ("Junior", "Junior"),
        ("Elementary", "Elementary"),
        ("Senior", "Senior"),
        ("Studio Junior", "Junior"),
        ("Studio Elementary", "Elementary"),
        ("Studio Senior", "Senior"),
    };

    private static int EnsembleTypePriority(string entryTypeRaw)
    {
        if (entryTypeRaw.Contains("Small", StringComparison.OrdinalIgnoreCase)) return 1;
        if (entryTypeRaw.Contains("Medium", StringComparison.OrdinalIgnoreCase)) return 2;
        if (entryTypeRaw.Contains("Large", StringComparison.OrdinalIgnoreCase)) return 3;
        if (entryTypeRaw.Contains("XL", StringComparison.OrdinalIgnoreCase)) return 4;
        return 99;
    }
    /// <summary>
    /// Extracts a level grouping (Middle School, High School, etc.)
    /// from a class display name.
    /// </summary>
    private static string ExtractLevel(string displayName)
    {
        foreach (var (match, level) in LevelRules)
        {
            if (displayName.StartsWith(match, StringComparison.OrdinalIgnoreCase))
                return level;
        }

        // Fallback: first two words
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0]} {parts[1]}";

        return displayName;
    }
}