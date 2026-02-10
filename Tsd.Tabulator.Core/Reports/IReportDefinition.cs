using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports;

/// <summary>
/// Defines a report that can be generated.
/// </summary>
public interface IReportDefinition
{
    /// <summary>
    /// Gets the unique identifier for this report.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name for this report.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Creates a new instance of the report tab view model.
    /// </summary>
    object CreateViewModel();
}
