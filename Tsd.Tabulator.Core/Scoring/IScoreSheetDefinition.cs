using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Scoring;

/// <summary>
/// Defines the structure of a score sheet including criteria, judges, and metadata.
/// </summary>
public interface IScoreSheetDefinition
{
    /// <summary>Unique key for persistence (e.g., "OKSTATE_STANDARD")</summary>
    string SheetKey { get; }
    
    /// <summary>Display name for the tab header (e.g., "Standard")</summary>
    string DisplayName { get; }
    
    /// <summary>Number of judges for this sheet</summary>
    int JudgeCount { get; }
    
    /// <summary>Ordered list of scoring criteria</summary>
    IReadOnlyList<ScoreCriterionDefinition> Criteria { get; }
}
