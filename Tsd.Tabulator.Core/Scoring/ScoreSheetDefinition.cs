using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Scoring;

/// <summary>
/// Simple immutable implementation of IScoreSheetDefinition.
/// </summary>
public sealed class ScoreSheetDefinition : IScoreSheetDefinition
{
    public ScoreSheetDefinition(
        string sheetKey,
        string displayName,
        int judgeCount,
        IReadOnlyList<ScoreCriterionDefinition> criteria)
    {
        SheetKey = sheetKey ?? throw new ArgumentNullException(nameof(sheetKey));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        JudgeCount = judgeCount;
        Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
    }

    public string SheetKey { get; }
    public string DisplayName { get; }
    public int JudgeCount { get; }
    public IReadOnlyList<ScoreCriterionDefinition> Criteria { get; }
}
