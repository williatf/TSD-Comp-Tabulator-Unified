namespace Tsd.Tabulator.Core.Models;

public sealed class ClassDefinition
{
    public string ClassKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string UpdatedUtc { get; set; } = string.Empty;
}