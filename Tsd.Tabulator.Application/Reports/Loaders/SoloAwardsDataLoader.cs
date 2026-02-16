using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Application.Interfaces.Reporting;
using Tsd.Tabulator.Application.Models.Reporting;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reports;

namespace Tsd.Tabulator.Application.Reports.Loaders;

public sealed class SoloAwardsDataLoader
    : IReportDataLoader<SoloAwardCandidate>
{
    private readonly IScoreRepositoryFactory _scoreRepoFactory;
    private readonly IReportScheme<SoloAwardCandidate> _scheme;

    public SoloAwardsDataLoader(
        IScoreRepositoryFactory scoreRepoFactory,
        IReportScheme<SoloAwardCandidate> scheme)
    {
        _scoreRepoFactory = scoreRepoFactory;
        _scheme = scheme;
    }

    public string ReportId => "SoloAwards";

    public async Task<IReadOnlyList<BucketGroup<SoloAwardCandidate>>> LoadAsync(IEventContext context)
    {
        var repo = _scoreRepoFactory.Create();
        var candidates = await repo.GetSoloAwardsCandidatesAsync();

        // Build ClassGroups<T>
        var classGroups =
            candidates
                .GroupBy(c => c.Bucket)
                .SelectMany(bucketGroup =>
                    bucketGroup.GroupBy(c => c.Class)
                        .Select(classGroup =>
                        {
                            var sorted = classGroup
                                .OrderByDescending(c => c.FinalScore)
                                .ToList();

                            var limited = _scheme.LimitTopN.HasValue
                                ? sorted.Take(_scheme.LimitTopN.Value).ToList()
                                : sorted;

                            return new ClassGroup<SoloAwardCandidate>(
                                classKey: classGroup.Key,
                                displayName: classGroup.Key,
                                sortOrder: 0,
                                bucket: bucketGroup.Key,
                                items: limited,
                                columns: _scheme.Columns
                            );
                        })
                )
                .ToList();

        // Build BucketGroups<T>
        var bucketGroups =
            classGroups
                .GroupBy(cg => cg.Bucket)
                .Select(bg => new BucketGroup<SoloAwardCandidate>(
                    bucket: bg.Key,
                    classes: bg.ToList()
                ))
                .ToList();

        return bucketGroups;
    }
}