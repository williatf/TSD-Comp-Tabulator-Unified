using System.Threading.Tasks;

namespace Tsd.Tabulator.Application.Interfaces;

/// <summary>
/// Represents a tab that displays a report.
/// </summary>
public interface IReportTab
{
    /// <summary>
    /// Gets or sets the display name for the report tab.
    /// </summary>
    string DisplayName { get; set; }

    /// <summary>
    /// Generates the report asynchronously.
    /// </summary>
    Task GenerateReportAsync();

    /// <summary>
    /// Refreshes the active report tab.
    /// Called automatically on activation or manually via Refresh button.
    /// </summary>
    Task RefreshAsync();
}
