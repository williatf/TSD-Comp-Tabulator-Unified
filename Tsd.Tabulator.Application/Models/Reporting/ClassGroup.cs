// ClassGroup.cs
using System.Collections.Generic;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Models.Reporting;

public sealed class ClassGroup<T>
{
    public string ClassKey { get; }
    public string DisplayName { get; }
    public int SortOrder { get; }
    public string Bucket { get; }
    public IReadOnlyList<T> Items { get; }
    public IReadOnlyList<ReportColumn> Columns { get; }

    public ClassGroup(
        string classKey,
        string displayName,
        int sortOrder,
        string bucket,
        IReadOnlyList<T> items,
        IReadOnlyList<ReportColumn> columns)
    {
        ClassKey = classKey;
        DisplayName = displayName;
        SortOrder = sortOrder;
        Bucket = bucket;
        Items = items;
        Columns = columns;
    }
}