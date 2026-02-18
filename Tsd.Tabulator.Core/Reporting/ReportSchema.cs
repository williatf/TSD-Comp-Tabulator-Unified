
namespace Tsd.Tabulator.Core.Reporting;

public sealed record ReportColumn(
    string Header,
    string BindingPath,
    double Width = double.NaN
);

public sealed class ReportSchema
{
    public required IReadOnlyList<ReportColumn> Columns { get; init; }
}