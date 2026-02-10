using System;
using System.Collections.Generic;
using System.Linq;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Reports;

/// <summary>
/// Defines which reports are available for each competition type and their display order.
/// </summary>
public sealed class ReportConfiguration
{
    private readonly Dictionary<CompetitionType, List<string>> _reportsByCompetitionType;

    public ReportConfiguration()
    {
        _reportsByCompetitionType = new Dictionary<CompetitionType, List<string>>
        {
            // Trendsetter Dance competitions
            [CompetitionType.TSDance] = new List<string>
            {
                "SoloAwards",
                "DuetAwards",
                // Add more TSDance-specific reports here in display order
            },

            // USASF competitions
            [CompetitionType.USASF] = new List<string>
            {
                "SoloAwards",
                // Add USASF-specific reports here
            },

            // Oklahoma State competitions
            [CompetitionType.OKState] = new List<string>
            {
                "SoloAwards",
                // Add OKState-specific reports here
            }
        };
    }

    /// <summary>
    /// Gets the ordered list of report IDs for the specified competition type.
    /// </summary>
    public IReadOnlyList<string> GetReportsFor(CompetitionType competitionType)
    {
        if (_reportsByCompetitionType.TryGetValue(competitionType, out var reports))
        {
            return reports;
        }

        // Fallback to TSDance if competition type not configured
        return _reportsByCompetitionType[CompetitionType.TSDance];
    }
}
