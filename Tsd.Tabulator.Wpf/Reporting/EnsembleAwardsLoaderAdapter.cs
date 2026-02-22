using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Reports.d_Ensemble;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Wpf.Reporting;

/// <summary>
/// Loader adapter for the Ensemble Award Report. This flattens the full
/// Bucket → Class → Type → Items hierarchy into a simple list of
/// <see cref="EnsembleAwardEntry"/> items for display in the generic
/// report tab.
/// </summary>
public sealed class EnsembleAwardsLoaderAdapter : IReportLoader<EnsembleAwardEntry>
{
    private readonly IEnsembleAwardReportService _service;

    public EnsembleAwardsLoaderAdapter(IEnsembleAwardReportService service)
    {
        _service = service;
    }

    public async Task<IReadOnlyList<EnsembleAwardEntry>> LoadAsync()
    {
        var report = await _service.GenerateReportAsync();

        return report.Buckets
            .SelectMany(b => b.Classes)   // UPDATED: Levels → Classes
            .SelectMany(c => c.Types)
            .SelectMany(t => t.Items)
            .ToList();
    }
}