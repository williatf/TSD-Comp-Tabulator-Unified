using System.Collections.Generic;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

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
}