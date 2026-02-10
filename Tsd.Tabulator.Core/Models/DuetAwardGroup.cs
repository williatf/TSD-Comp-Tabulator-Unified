using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a group of duet award entries within a bucket and class.
/// </summary>
public sealed record DuetAwardGroup(
    string Bucket,
    string Class,
    IReadOnlyList<DuetAwardEntry> Entries
);
