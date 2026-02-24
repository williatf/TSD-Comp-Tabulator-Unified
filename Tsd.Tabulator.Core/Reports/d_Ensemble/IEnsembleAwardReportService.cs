using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

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
