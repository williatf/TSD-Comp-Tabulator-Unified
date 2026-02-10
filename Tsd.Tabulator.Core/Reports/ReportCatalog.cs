using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsd.Tabulator.Core.Reports;

/// <summary>
/// Default implementation of IReportCatalog that provides access to available report definitions.
/// </summary>
public sealed class ReportCatalog : IReportCatalog
{
    private readonly Dictionary<string, IReportDefinition> _reportsByIdIndex;
    private readonly IEnumerable<IReportDefinition> _reports;

    public ReportCatalog(IEnumerable<IReportDefinition> reports)
    {
        _reports = reports ?? throw new ArgumentNullException(nameof(reports));
        _reportsByIdIndex = _reports.ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<IReportDefinition> GetAllReports() => _reports;

    public IReportDefinition GetReport(string reportId)
    {
        if (string.IsNullOrWhiteSpace(reportId))
            throw new ArgumentException("Report ID cannot be null or empty.", nameof(reportId));

        if (_reportsByIdIndex.TryGetValue(reportId, out var report))
            return report;

        throw new InvalidOperationException($"Report with ID '{reportId}' not found.");
    }
}
