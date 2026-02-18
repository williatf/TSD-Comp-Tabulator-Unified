using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports.Raw;
public sealed record ScoredRoutineRow
{
    public string RoutineId { get; init; } = "";
    public int ProgramNumber { get; init; }
    public string Class { get; init; } = "";
    public string Participants { get; init; } = "";
    public string StudioName { get; init; } = "";
    public string RoutineTitle { get; init; } = "";
    public string LastSheetKey { get; init; } = "";
}