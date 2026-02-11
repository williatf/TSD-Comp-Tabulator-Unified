using System.Collections.Generic;

namespace Tsd.Tabulator.Application.Interfaces;

/// <summary>
/// Provides access to available report definitions.
/// </summary>
public interface IReportCatalog
{
    /// <summary>
    /// Gets all available report definitions.
    /// </summary>
    IEnumerable<IReportDefinition> GetAllReports();

    /// <summary>
    /// Gets a report definition by its identifier.
    /// </summary>
    IReportDefinition GetReport(string reportId);
}
