using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Scoring;

/// <summary>
/// Default implementation of score sheet selection logic.
/// </summary>
public sealed class DefaultScoreSheetSelector : IScoreSheetSelector
{
    public (IReadOnlyList<IScoreSheetDefinition> AvailableSheets, string DefaultSheetKey) GetSheets(
        CompetitionType compType,
        RoutineRow? routine)
    {
        return compType switch
        {
            CompetitionType.OKState => GetOkStateSheets(routine),
            CompetitionType.USASF => GetUsasfSheets(routine),
            CompetitionType.TSDance => GetTSDanceSheets(routine),
            _ => GetTSDanceSheets(routine)
        };
    }

    private (IReadOnlyList<IScoreSheetDefinition>, string) GetOkStateSheets(RoutineRow? routine)
    {
        // OKState uses Standard and Technical sheets
        var sheets = new List<IScoreSheetDefinition>
        {
            ScoreSheetDefinitions.Standard,
            ScoreSheetDefinitions.GameDay,
            ScoreSheetDefinitions.SpiritRally,
        };

        // Default to Standard for OKState
        return (sheets, ScoreSheetDefinitions.Standard.SheetKey);
    }

    private (IReadOnlyList<IScoreSheetDefinition>, string) GetUsasfSheets(RoutineRow? routine)
    {
        // USASF might have different sheets based on category/class
        var sheets = new List<IScoreSheetDefinition>
        {
            ScoreSheetDefinitions.Standard,
            ScoreSheetDefinitions.GameDay,
            ScoreSheetDefinitions.SpiritRally,
        };

        return (sheets, ScoreSheetDefinitions.Standard.SheetKey);
    }

    private (IReadOnlyList<IScoreSheetDefinition>, string) GetTSDanceSheets(RoutineRow? routine)
    {
        // Base competition type shows all sheets
        var sheets = new List<IScoreSheetDefinition>
        {
            ScoreSheetDefinitions.Trendsetters,
            ScoreSheetDefinitions.GameDay
        };

        return (sheets, sheets.FirstOrDefault()?.SheetKey ?? string.Empty);
    }
}
