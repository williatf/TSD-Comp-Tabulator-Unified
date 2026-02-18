using System.Collections.Generic;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reports.Raw;

namespace Tsd.Tabulator.Core.Services;

public interface IScoreRepository
{
    Task<IReadOnlyList<RoutineScoreCellRow>> GetCellsAsync(string routineId, string sheetKey);
    Task SaveCellsAsync(string routineId, string sheetKey, IEnumerable<RoutineScoreCellRow> cells);
    Task<IReadOnlyList<RoutineRow>> GetAllRoutinesAsync();
    
    /// <summary>
    /// Gets the last used sheet key for a routine, or null if not set.
    /// </summary>
    Task<string?> GetLastSheetKeyAsync(string routineId);
    
    /// <summary>
    /// Updates the last used sheet key for a routine.
    /// </summary>
    Task SetLastSheetKeyAsync(string routineId, string sheetKey);
    
    /// <summary>
    /// Deletes all scores for a routine except the specified sheet.
    /// Used to enforce single-sheet scoring.
    /// </summary>
    Task DeleteScoresExceptSheetAsync(string routineId, string keepSheetKey);
    
    /// <summary>
    /// Deletes all scores for all routines from all score sheets.
    /// </summary>
    Task ClearAllScoresAsync();
    
    /// <summary>
    /// Gets solo award candidates with computed final scores.
    /// Filters for EntryType containing 'Solo', splits into School/Studio buckets,
    /// and keeps only the best-scoring routine per participant per bucket.
    /// </summary>
    Task<IReadOnlyList<SoloAwardCandidate>> GetSoloAwardsCandidatesAsync();
    
    /// <summary>
    /// Gets duet award candidates with computed final scores.
    /// Filters for EntryType containing 'Duet', splits into School/Studio buckets,
    /// and keeps only the best-scoring routine per participant per bucket.
    /// </summary>
    Task<IReadOnlyList<DuetAwardCandidate>> GetDuetAwardsCandidatesAsync();

    /// <summary>
    /// Retrieves all routines of the specified entry type that have been fully scored.
    /// Returns raw routine metadata including class, participants, program number,
    /// studio name, title, and the LastSheetKey needed to load score cells.
    /// This method performs no scoring, ranking, or award logic.
    /// </summary>
    Task<IReadOnlyList<ScoredRoutineRow>> GetScoredRoutinesAsync(string entryType);

    /// <summary>
    /// Retrieves all score cells for a specific routine and score sheet.
    /// Each row represents a single judge’s value for a single scoring criterion.
    /// Used by award report services to compute judge totals and final scores.
    /// </summary>
    Task<IReadOnlyList<ScoreCellRow>> GetScoreCellsAsync(string routineId, string sheetKey);
}