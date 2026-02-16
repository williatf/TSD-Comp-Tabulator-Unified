using System.Collections.Generic;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Application.Interfaces.Reporting;
using Tsd.Tabulator.Application.Models.Reporting;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Wpf.Reports;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reports.Schemes;

public sealed class SoloAwardsReportScheme : IReportScheme<SoloAwardCandidate>, IReportSchemeUi<SoloAwardCandidate>
{
    public string ReportId => "SoloAwards";
    public string DisplayName => "Solo Awards";

    public IReadOnlyList<ReportColumn> Columns { get; } = new List<ReportColumn>
    {
        new("Rank", "Rank", 0),
        new("Program #", "ProgramNumber", 1),
        new("Studio", "StudioName", 2),
        new("Participant", "Participants", 3),
        new("Avg Score", "FinalScore", 4)
    };

    public IReadOnlyList<ReportGroupDescriptor> Groups { get; } = new List<ReportGroupDescriptor>
    {
        new("Bucket"),
        new("Class")
    };

    public IReadOnlyList<ReportSortDescriptor> DefaultSort { get; } = new List<ReportSortDescriptor>
    {
        new("FinalScore", SortDirection.Descending)
    };

    public int? LimitTopN => 10;

    public IReadOnlyList<CompetitionType> SupportedTypes { get; } = new[] { CompetitionType.TSDance, CompetitionType.USASF };

    public IReportTab CreateTab(IReportDataLoader<SoloAwardCandidate> loader, IEventContext context)
    {
        return new ReportTabViewModel<SoloAwardCandidate>(DisplayName, loader, context);
    }
}