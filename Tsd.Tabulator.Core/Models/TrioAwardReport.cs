using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a complete trio award report with all groups.
/// </summary>
public class TrioAwardReport
{
    public TrioAwardReport(IReadOnlyList<TrioAwardGroup> groups)
    {
        Groups = groups;
    }

    public IReadOnlyList<TrioAwardGroup> Groups { get; }
}
