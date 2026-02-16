namespace Tsd.Tabulator.Application.Models.Reporting;

public sealed record ReportSortDescriptor(string ColumnId, SortDirection Direction);

public enum SortDirection { Ascending, Descending }