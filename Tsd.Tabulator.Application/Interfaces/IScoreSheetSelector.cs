using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Interfaces;

/// <summary>
/// Determines which score sheets are available for a given competition type and routine.
/// </summary>
public interface IScoreSheetSelector
{
    /// <summary>
    /// Gets the available score sheets and default sheet key for the given context.
    /// </summary>
    /// <param name="compType">The competition type</param>
    /// <param name="routine">The routine being scored (can be null for initial setup)</param>
    /// <returns>List of available definitions and the default sheet key</returns>
    (IReadOnlyList<IScoreSheetDefinition> AvailableSheets, string DefaultSheetKey) GetSheets(
        CompetitionType compType,
        RoutineRow? routine);
}
