using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Core.Reports.b_Duet;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class DuetAwardsLoaderAdapter : IReportLoader<DuetAwardEntry>
{
    private readonly IDuetAwardReportService _service;

    public DuetAwardsLoaderAdapter(IDuetAwardReportService service)
    {
        _service = service;
    }

    public async Task<IReadOnlyList<DuetAwardEntry>> LoadAsync()
    {
        var report = await _service.GenerateReportAsync();

        return report.Groups
                     .SelectMany(g => g.Entries)
                     .ToList();
    }
}