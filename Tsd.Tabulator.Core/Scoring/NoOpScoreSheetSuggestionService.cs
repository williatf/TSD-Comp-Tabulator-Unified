using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Scoring;

/// <summary>
/// Default implementation that never suggests a sheet (manual selection only).
/// </summary>
public sealed class NoOpScoreSheetSuggestionService : IScoreSheetSuggestionService
{
    public string? SuggestSheetKey(RoutineRow routine) => null;
}
