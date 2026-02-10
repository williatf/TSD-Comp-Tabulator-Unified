using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Scoring;

/// <summary>
/// Defines a single scoring criterion/column.
/// </summary>
public sealed record ScoreCriterionDefinition
{
    public ScoreCriterionDefinition(
        string key,
        string displayName,
        decimal maxScore,
        int order = 0,
        string? group = null,
        string? canonicalKey = null)
    {
        Key = key;
        DisplayName = displayName;
        MaxScore = maxScore;
        Order = order;
        Group = group;
        CanonicalKey = canonicalKey ?? key; // Default to Key if not specified
    }

    /// <summary>Unique key for persistence (e.g., "FightSong_Effectiveness")</summary>
    public string Key { get; }
    
    /// <summary>Header text (e.g., "Effectiveness")</summary>
    public string DisplayName { get; }
    
    /// <summary>Maximum allowed value (e.g., 10m)</summary>
    public decimal MaxScore { get; }
    
    /// <summary>Display order (lower values first)</summary>
    public int Order { get; }
    
    /// <summary>Optional group header (e.g., "Fight Song"). Null if not grouped.</summary>
    public string? Group { get; }
    
    /// <summary>Canonical key for reporting (e.g., "Effectiveness"). Defaults to Key.</summary>
    public string CanonicalKey { get; }
}
