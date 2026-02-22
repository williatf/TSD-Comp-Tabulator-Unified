using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

/// <summary>
/// Represents a grouping of Ensemble award results by Class
/// (e.g., Middle School, High School, Junior, Elementary, J.V.).
/// This is the second level in the Ensemble hierarchy:
///
/// Bucket → Class → Type → Items
///
/// Each <see cref="EnsembleClassGroup"/> contains the list of
/// <see cref="EnsembleTypeGroup"/> objects belonging to this class.
/// </summary>
public sealed record EnsembleClassGroup
{
    public string ClassKey { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int SortOrder { get; init; }


    /// <summary>
    /// The collection of Ensemble type groups under this class.
    /// </summary>
    public IReadOnlyList<EnsembleTypeGroup> Types { get; init; } =
        new List<EnsembleTypeGroup>();

    /// <summary>
    /// Creates a new <see cref="EnsembleClassGroup"/> with the specified
    /// class label and list of type groups.
    /// </summary>
    public EnsembleClassGroup(
        string classKey,
        string displayName,
        int sortOrder,
        IReadOnlyList<EnsembleTypeGroup> types)
    {
        ClassKey = classKey;
        DisplayName = displayName;
        SortOrder = sortOrder;
        Types = types;
    }

}