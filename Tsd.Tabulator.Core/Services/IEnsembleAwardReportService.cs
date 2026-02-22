using System.Threading.Tasks;
using Tsd.Tabulator.Core.Reports.d_Ensemble;

namespace Tsd.Tabulator.Core.Services;

/// <summary>
/// Service for generating ensemble award reports with ranking and tie-breaking logic.
/// </summary>
public interface IEnsembleAwardReportService
{
    /// <summary>
    /// Generates a complete ensemble award report with proper ranking including tie handling.
    /// Rankings are done within each (Bucket, Level, Type) group.
    /// Ties result in shared places with subsequent places skipped.
    /// </summary>
    Task<EnsembleAwardReport> GenerateReportAsync();
}
