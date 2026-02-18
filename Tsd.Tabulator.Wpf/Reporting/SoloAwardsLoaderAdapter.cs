using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class SoloAwardsLoaderAdapter : IReportLoader<SoloAwardEntry>
{
    private readonly ISoloAwardReportService _service;

    public SoloAwardsLoaderAdapter(ISoloAwardReportService service)
    {
        _service = service;
    }

    public async Task<IReadOnlyList<SoloAwardEntry>> LoadAsync()
    {
        var report = await _service.GenerateReportAsync();

        return report.Groups
                     .SelectMany(g => g.Entries)
                     .ToList();
    }
}