using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class TrioAwardsLoaderAdapter : IReportLoader<TrioAwardEntry>
{
    private readonly ITrioAwardReportService _service;

    public TrioAwardsLoaderAdapter(ITrioAwardReportService service)
    {
        _service = service;
    }

    public async Task<IReadOnlyList<TrioAwardEntry>> LoadAsync()
    {
        var report = await _service.GenerateReportAsync();

        return report.Groups
                     .SelectMany(g => g.Entries)
                     .ToList();
    }
}