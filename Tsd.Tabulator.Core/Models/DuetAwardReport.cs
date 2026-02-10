using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a complete duet award report with all groups.
/// </summary>
public class DuetAwardReport
{
    public DuetAwardReport(IReadOnlyList<DuetAwardGroup> groups)
    {
        Groups = groups;
    }

    public IReadOnlyList<DuetAwardGroup> Groups { get; }
}
