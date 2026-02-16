namespace Tsd.Tabulator.Application.Models.Reporting;

public sealed record ReportColumn(
    string Header,
    string BindingPath,
    int DisplayIndex = 0,
    string? StringFormat = null);