using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a group of solo award entries within a bucket and class.
/// </summary>
public sealed record SoloAwardGroup(
    string Bucket,
    string Class,
    IReadOnlyList<SoloAwardEntry> Entries
);
