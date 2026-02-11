using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Interfaces;

/// <summary>
/// Service that can suggest an appropriate score sheet for a routine.
/// This is a future automation hook - not currently used.
/// </summary>
public interface IScoreSheetSuggestionService
{
    /// <summary>
    /// Suggests a sheet key for the given routine, or null if no suggestion.
    /// </summary>
    /// <param name="routine">The routine to score</param>
    /// <returns>Suggested sheet key, or null if no suggestion</returns>
    string? SuggestSheetKey(RoutineRow routine);
}
