using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Interfaces;

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