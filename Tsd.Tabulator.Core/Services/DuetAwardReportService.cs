using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Services;

public sealed class DuetAwardReportService : IDuetAwardReportService
{
    private readonly IScoreRepository _repository;
    private readonly IClassConfigService _classConfig;
    private readonly string _eventDbPath;
    private const int MaxEntriesPerGroup = 12;

    public DuetAwardReportService(
        IScoreRepository repository,
        IClassConfigService classConfigService,
        string eventDbPath)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _classConfig = classConfigService ?? throw new ArgumentNullException(nameof(classConfigService));
        _eventDbPath = eventDbPath ?? throw new ArgumentNullException(nameof(eventDbPath));
    }

    public async Task<DuetAwardReport> GenerateReportAsync()
    {
        var candidates = await LoadCandidatesAsync();

        // Resolve class keys
        var enriched = new List<(DuetAwardCandidate Candidate, string? ClassKey)>();
        foreach (var c in candidates)
        {
            var key = await _classConfig.ResolveClassKeyAsync(c.Class, _eventDbPath);
            enriched.Add((c, key));
        }

        // Load class definitions
        var defs = (await _classConfig.GetClassDefinitionsAsync(_eventDbPath)).ToList();

        // Bucket ordering helper
        static int BucketPriority(string? bucket) =>
            bucket?.Equals("studio", StringComparison.OrdinalIgnoreCase) == true ? 1 :
            bucket?.Equals("school", StringComparison.OrdinalIgnoreCase) == true ? 2 : 99;

        // Build groups
        var groups = enriched
            .GroupBy(x => x.ClassKey ?? x.Candidate.Class?.Trim() ?? string.Empty)
            .Select(g =>
            {
                var key = g.Key;
                var def = defs.FirstOrDefault(d =>
                    string.Equals(d.ClassKey, key, StringComparison.OrdinalIgnoreCase));

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
            .OrderBy(g => BucketPriority(g.Bucket))
            .ThenBy(g => g.SortOrder)
            .ThenBy(g => g.DisplayName)
            .ToList();

        var groupsResult = new List<DuetAwardGroup>();
        foreach (var g in groups)
        {
            groupsResult.Add(await CreateGroup(
                g.Bucket,
                g.ClassKey,
                g.Candidates,
                _eventDbPath));
        }

        return new DuetAwardReport(groupsResult);
    }

    private async Task<IReadOnlyList<DuetAwardCandidate>> LoadCandidatesAsync()
    {
        var routines = await _repository.GetScoredRoutinesAsync("%Duet%");

        var candidates = new List<DuetAwardCandidate>();

        foreach (var r in routines)
        {
            var cells = await _repository.GetScoreCellsAsync(r.RoutineId, r.LastSheetKey);

            var judgeTotals = cells
                .GroupBy(c => c.JudgeIndex)
                .Select(g => g.Sum(c => c.Value))
                .ToList();

            var finalScore = judgeTotals.Average();

            candidates.Add(new DuetAwardCandidate
            {
                Class = r.Class,
                Participants = r.Participants,
                ProgramNumber = r.ProgramNumber,
                StudioName = r.StudioName,
                RoutineTitle = r.RoutineTitle,
                FinalScore = (double)finalScore
            });
        }

        return candidates;
    }

    private static async Task<DuetAwardGroup> CreateGroup(
        string bucket,
        string classKey,
        IEnumerable<DuetAwardCandidate> candidates,
        string eventDbPath)
    {
        var sorted = candidates.OrderByDescending(c => c.FinalScore).ToList();
        var entries = new List<DuetAwardEntry>();
        int currentPlace = 1;
        double? previousScore = null;
        int countAtCurrentScore = 0;
        int totalEntriesAdded = 0;

        foreach (var candidate in sorted)
        {
            if (totalEntriesAdded >= MaxEntriesPerGroup &&
                (!previousScore.HasValue || Math.Abs(candidate.FinalScore - previousScore.Value) >= 0.0001))
                break;

            if (previousScore.HasValue &&
                Math.Abs(candidate.FinalScore - previousScore.Value) < 0.0001)
            {
                countAtCurrentScore++;
            }
            else
            {
                if (previousScore.HasValue)
                    currentPlace += countAtCurrentScore;

                countAtCurrentScore = 1;
                previousScore = candidate.FinalScore;
            }

            entries.Add(new DuetAwardEntry(
                currentPlace,
                candidate.FinalScore,
                candidate.ProgramNumber,
                candidate.Participants,
                candidate.StudioName,
                candidate.RoutineTitle,
                classKey
            ));

            totalEntriesAdded++;
        }

        return new DuetAwardGroup(bucket, classKey, entries);
    }
}