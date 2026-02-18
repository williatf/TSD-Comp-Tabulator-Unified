namespace Tsd.Tabulator.Core.Reporting;

public sealed class ClassGroup
{
    public string ClassKey { get; }
    public string DisplayName { get; }
    public int SortOrder { get; }
    public List<object> Items { get; }

    public ClassGroup(string classKey, string displayName, int sortOrder, List<object> items)
    {
        ClassKey = classKey;
        DisplayName = displayName;
        SortOrder = sortOrder;
        Items = items;
    }
}