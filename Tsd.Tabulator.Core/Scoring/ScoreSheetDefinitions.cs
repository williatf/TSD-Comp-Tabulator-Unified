using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Scoring;

/// <summary>
/// Registry of all available score sheet definitions.
/// </summary>
public static class ScoreSheetDefinitions
{
    /// <summary>
    /// OKState Standard scoring sheet - the original/primary sheet.
    /// </summary>
    public static IScoreSheetDefinition Standard { get; } = new ScoreSheetDefinition(
        sheetKey: "STANDARD",
        displayName: "Standard",
        judgeCount: 3,
        criteria: new[]
        {
            new ScoreCriterionDefinition("Choreography", "Choreography", 10m, order: 0),
            new ScoreCriterionDefinition("Staging", "Staging", 10m, order: 1),
            new ScoreCriterionDefinition("Elements", "Elements", 10m, order: 2),
            new ScoreCriterionDefinition("Execution", "Execution", 10m, order: 3),
            new ScoreCriterionDefinition("Movement", "Movement", 10m, order: 4),
            new ScoreCriterionDefinition("Artistry", "Artistry", 10m, order: 5),
            new ScoreCriterionDefinition("Precision", "Precision", 10m, order: 6),
            new ScoreCriterionDefinition("Performance", "Performance", 10m, order: 7),
            new ScoreCriterionDefinition("Appearance", "Appearance", 10m, order: 8),
            new ScoreCriterionDefinition("Impression", "Impression", 10m, order: 9),
        }
    );

    public static IScoreSheetDefinition GameDay { get; } = new ScoreSheetDefinition(
        sheetKey: "GAMEDAY",
        displayName: "Game Day",
        judgeCount: 3,
        criteria: new[]
        {
            // Fight Song group (3 columns)
            new ScoreCriterionDefinition(
                key: "FightSong_Effectiveness",
                displayName: "Effectiveness",
                maxScore: 10m,
                order: 0,
                group: "Fight Song",
                canonicalKey: "Effectiveness"),
            
            new ScoreCriterionDefinition(
                key: "FightSong_Creativity",
                displayName: "Creativity",
                maxScore: 10m,
                order: 1,
                group: "Fight Song",
                canonicalKey: "Creativity"),
            
            new ScoreCriterionDefinition(
                key: "FightSong_Execution",
                displayName: "Execution",
                maxScore: 10m,
                order: 2,
                group: "Fight Song",
                canonicalKey: "Execution"),

            // Spirit Raising group (3 columns)
            new ScoreCriterionDefinition(
                key: "SpiritRaising_Effectiveness",
                displayName: "Effectiveness",
                maxScore: 10m,
                order: 3,
                group: "Spirit Raising",
                canonicalKey: "Effectiveness"),
            
            new ScoreCriterionDefinition(
                key: "SpiritRaising_Creativity",
                displayName: "Creativity",
                maxScore: 10m,
                order: 4,
                group: "Spirit Raising",
                canonicalKey: "Creativity"),
            
            new ScoreCriterionDefinition(
                key: "SpiritRaising_Execution",
                displayName: "Execution",
                maxScore: 10m,
                order: 5,
                group: "Spirit Raising",
                canonicalKey: "Execution"),

            // Performance group (4 columns)
            new ScoreCriterionDefinition(
                key: "Performance_Energy",
                displayName: "Energy",
                maxScore: 10m,
                order: 6,
                group: "Performance",
                canonicalKey: "Energy"),
            
            new ScoreCriterionDefinition(
                key: "Performance_Synchronization",
                displayName: "Synchronization",
                maxScore: 10m,
                order: 7,
                group: "Performance",
                canonicalKey: "Synchronization"),
            
            new ScoreCriterionDefinition(
                key: "Performance_Skills",
                displayName: "Skills",
                maxScore: 10m,
                order: 8,
                group: "Performance",
                canonicalKey: "Skills"),
            
            new ScoreCriterionDefinition(
                key: "Performance_Overall",
                displayName: "Overall",
                maxScore: 10m,
                order: 9,
                group: "Performance",
                canonicalKey: "Overall"),
        }
    );

    /// <summary>
    /// Spirit Rally sheet with simple canonical keys.
    /// </summary>
    public static IScoreSheetDefinition SpiritRally { get; } = new ScoreSheetDefinition(
        sheetKey: "SPIRIT_RALLY",
        displayName: "Spirit Rally",
        judgeCount: 3,
        criteria: new[]
        {
            new ScoreCriterionDefinition("Movement", "Movement", 10m, order: 0, group: "Execution"),
            new ScoreCriterionDefinition("Musicality", "Musicality", 10m, order: 1, group: "Execution"),
            new ScoreCriterionDefinition("Creativity", "Creativity", 10m, order: 2, group: "Choreography"),
            new ScoreCriterionDefinition("Execution", "Execution", 10m, order: 3, group: "Choreography"),
            new ScoreCriterionDefinition("Difficulty", "Difficulty", 10m, order: 4, group: "Showmanship"),
            new ScoreCriterionDefinition("Overall", "Overall", 10m, order: 5, "Showmanship"),
        }
    );

    /// <summary>
    /// Trendsetters Dance Competition scoring sheet.
    /// </summary>
    public static IScoreSheetDefinition Trendsetters { get; } = new ScoreSheetDefinition(
        sheetKey: "TRENDSETTERS",
        displayName: "Trendsetters Dance Competition",
        judgeCount: 3,
        criteria: new[]
        {
            new ScoreCriterionDefinition("Choreography", "Choreography", 25m, order: 0),
            new ScoreCriterionDefinition("Technique", "Technique", 25m, order: 1),
            new ScoreCriterionDefinition("Execution", "Execution", 25m, order: 2),
            new ScoreCriterionDefinition("Artistry", "Artistry", 10m, order: 3),
            new ScoreCriterionDefinition("Showmanship", "Showmanship", 10m, order: 4),
            new ScoreCriterionDefinition("Appearance", "Appearance", 5m, order: 5),
        }
    );
}
