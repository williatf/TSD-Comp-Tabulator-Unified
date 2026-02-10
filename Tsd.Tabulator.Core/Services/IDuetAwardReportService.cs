using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Services;

/// <summary>
/// Service for generating duet award reports with ranking and tie-breaking logic.
/// </summary>
public interface IDuetAwardReportService
{
    /// <summary>
    /// Generates a complete duet award report with proper ranking including tie handling.
    /// Rankings are done within each (Bucket, Class) group.
    /// Ties result in shared places with subsequent places skipped.
    /// </summary>
    Task<DuetAwardReport> GenerateReportAsync();
}