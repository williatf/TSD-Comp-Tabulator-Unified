using System.Collections.Generic;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Application.Models.Reporting;

namespace Tsd.Tabulator.Application.Interfaces.Reporting;

public interface IReportScheme<T>
{
    string ReportId { get; }
    string DisplayName { get; }
    IReadOnlyList<ReportColumn> Columns { get; }
    IReadOnlyList<ReportGroupDescriptor> Groups { get; }
    IReadOnlyList<ReportSortDescriptor> DefaultSort { get; }
    int? LimitTopN { get; }
    IReadOnlyList<CompetitionType> SupportedTypes { get; }
}