using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Reports.a_Solo;

/// <summary>
/// Represents a group of solo award entries within a bucket and class.
/// </summary>
public sealed record SoloAwardGroup(
    string Bucket,
    string Class,
    IReadOnlyList<SoloAwardEntry> Entries
);
