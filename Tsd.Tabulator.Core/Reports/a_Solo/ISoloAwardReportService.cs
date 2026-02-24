using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports.a_Solo;

/// <summary>
/// Service for generating solo award reports with ranking and tie-breaking logic.
/// </summary>
public interface ISoloAwardReportService
{
    /// <summary>
    /// Generates a complete solo award report with proper ranking including tie handling.
    /// Rankings are done within each (Bucket, Class) group.
    /// Ties result in shared places with subsequent places skipped.
    /// </summary>
    Task<SoloAwardReport> GenerateReportAsync();
}