using Tsd.Tabulator.Core.Reporting;
using Caliburn.Micro;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed record ReportHeader(
    string? Title,
    string? Subtitle,
    string? Notes
);
public interface IReportScheme
{
    string DisplayName { get; }
    IScreen CreateTab();
    public ReportHeader Header { get; init; }
}