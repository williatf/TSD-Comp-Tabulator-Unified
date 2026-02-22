namespace Tsd.Tabulator.Core.Reporting;

public sealed record ReportColumn(
    string Header,
    string BindingPath,
    double Width = double.NaN
);

public sealed class ReportSchema
{
    public required IReadOnlyList<ReportColumn> Columns { get; init; }

    /// <summary>
    /// The number of grouping levels in this report.
    /// 2 = Bucket → Class → Items (Solo/Duet/Trio)
    /// 3 = Bucket → Class → Type → Items (Ensemble)
    /// </summary>
    public int HierarchyDepth { get; init; } = 2; // default for existing reports
}