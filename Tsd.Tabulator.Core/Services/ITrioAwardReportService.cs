using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Services;

/// <summary>
/// Service for generating trio award reports with ranking and tie-breaking logic.
/// </summary>
public interface ITrioAwardReportService
{
    /// <summary>
    /// Generates a complete trio award report with proper ranking including tie handling.
    /// Rankings are done within each (Bucket, Class) group.
    /// Ties result in shared places with subsequent places skipped.
    /// </summary>
    Task<TrioAwardReport> GenerateReportAsync();
}