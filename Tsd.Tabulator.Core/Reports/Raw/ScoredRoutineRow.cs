using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports.Raw;

public sealed record ScoredRoutineRow
{
    public string RoutineId { get; init; } = string.Empty;
    public int ProgramNumber { get; init; }
    public string Class { get; init; } = string.Empty;
    public string EntryType { get; init; } = string.Empty;   // ← REQUIRED for Ensemble
    public string Participants { get; init; } = string.Empty;
    public string StudioName { get; init; } = string.Empty;
    public string RoutineTitle { get; init; } = string.Empty;
    public string LastSheetKey { get; init; } = string.Empty;
}