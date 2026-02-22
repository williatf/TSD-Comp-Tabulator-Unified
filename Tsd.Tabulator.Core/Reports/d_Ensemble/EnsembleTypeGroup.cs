using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

/// <summary>
/// Represents a grouping of Ensemble award entries by Ensemble Type
/// (e.g., Small, Medium, Large, XL). This is the third level in the
/// Ensemble hierarchy:
///
/// Bucket → Class → Type → Items
///
/// Each <see cref="EnsembleTypeGroup"/> contains the list of
/// <see cref="EnsembleAwardEntry"/> items that belong to this type.
/// </summary>
public sealed record EnsembleTypeGroup
{
    /// <summary>
    /// The Ensemble type (Small, Medium, Large, XL).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The award entries (winners) belonging to this Ensemble type.
    /// </summary>
    public IReadOnlyList<EnsembleAwardEntry> Items { get; init; } =
        new List<EnsembleAwardEntry>();

    /// <summary>
    /// Creates a new <see cref="EnsembleTypeGroup"/> with the specified
    /// type label and list of award entries.
    /// </summary>
    public EnsembleTypeGroup(string type, IReadOnlyList<EnsembleAwardEntry> items)
    {
        Type = type;
        Items = items;
    }
}