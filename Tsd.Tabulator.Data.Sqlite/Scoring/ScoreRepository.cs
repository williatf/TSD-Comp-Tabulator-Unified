using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Data.Sqlite.Scoring;

public sealed class ScoreRepository : IScoreRepository
{
    private readonly ISqliteConnectionFactory _factory;

    public ScoreRepository(ISqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<RoutineScoreCellRow>> GetCellsAsync(string routineId, string sheetKey)
    {
        using var conn = _factory.OpenConnection();
        var rows = await conn.QueryAsync<RoutineScoreCellRow>(
            @"SELECT RoutineId, SheetKey, JudgeIndex, CriterionKey, Value
              FROM RoutineScoreCell
              WHERE RoutineId = @RoutineId AND SheetKey = @SheetKey",
            new { RoutineId = routineId, SheetKey = sheetKey });
        return rows.AsList();
    }

    public async Task SaveCellsAsync(string routineId, string sheetKey, IEnumerable<RoutineScoreCellRow> cells)
    {
        using var conn = _factory.OpenConnection();
        using var tx = conn.BeginTransaction();

        const string upsert = @"
            INSERT INTO RoutineScoreCell (RoutineId, SheetKey, JudgeIndex, CriterionKey, Value, UpdatedUtc)
            VALUES (@RoutineId, @SheetKey, @JudgeIndex, @CriterionKey, @Value, strftime('%Y-%m-%dT%H:%M:%fZ','now'))
            ON CONFLICT(RoutineId, SheetKey, JudgeIndex, CriterionKey)
            DO UPDATE SET
                Value = excluded.Value,
                UpdatedUtc = excluded.UpdatedUtc;
        ";

        foreach (var cell in cells)
        {
            await conn.ExecuteAsync(upsert, new
            {
                RoutineId = cell.RoutineId,
                SheetKey = cell.SheetKey,
                JudgeIndex = cell.JudgeIndex,
                CriterionKey = cell.CriterionKey,
                Value = cell.Value
            }, tx);
        }

        // Upsert RoutineScoreStatus (IsScored=1, LastSheetKey)
        await conn.ExecuteAsync(@"
            INSERT INTO RoutineScoreStatus (RoutineId, IsScored, LastSheetKey, UpdatedUtc)
            VALUES (@RoutineId, 1, @LastSheetKey, strftime('%Y-%m-%dT%H:%M:%fZ','now'))
            ON CONFLICT(RoutineId) DO UPDATE SET
                IsScored = 1,
                LastSheetKey = excluded.LastSheetKey,
                UpdatedUtc = excluded.UpdatedUtc;
        ", new { RoutineId = routineId, LastSheetKey = sheetKey }, tx);

        tx.Commit();
    }

    public async Task<IReadOnlyList<RoutineRow>> GetAllRoutinesAsync()
    {
        using var conn = _factory.OpenConnection();
        var routines = await conn.QueryAsync<RoutineRow>(
            @"SELECT RoutineId, StartTimeText as StartTime, ProgramNumber, EntryTypeRaw AS EntryType, Category, Class, ParticipantsRaw as Participants, StudioName, RoutineTitle
              FROM Routine");
        return routines.AsList();
    }

    public async Task<string?> GetLastSheetKeyAsync(string routineId)
    {
        using var conn = _factory.OpenConnection();
        var result = await conn.QueryFirstOrDefaultAsync<string?>(
            @"SELECT LastSheetKey FROM RoutineScoreStatus WHERE RoutineId = @RoutineId",
            new { RoutineId = routineId });
        return result;
    }

    public async Task SetLastSheetKeyAsync(string routineId, string sheetKey)
    {
        using var conn = _factory.OpenConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO RoutineScoreStatus (RoutineId, LastSheetKey, UpdatedUtc)
            VALUES (@RoutineId, @LastSheetKey, strftime('%Y-%m-%dT%H:%M:%fZ','now'))
            ON CONFLICT(RoutineId) DO UPDATE SET
                LastSheetKey = excluded.LastSheetKey,
                UpdatedUtc = excluded.UpdatedUtc;
        ", new { RoutineId = routineId, LastSheetKey = sheetKey });
    }

    public async Task DeleteScoresExceptSheetAsync(string routineId, string keepSheetKey)
    {
        using var conn = _factory.OpenConnection();
        await conn.ExecuteAsync(@"
            DELETE FROM RoutineScoreCell 
            WHERE RoutineId = @RoutineId AND SheetKey != @KeepSheetKey
        ", new { RoutineId = routineId, KeepSheetKey = keepSheetKey });
    }

    public async Task ClearAllScoresAsync()
    {
        using var conn = _factory.OpenConnection();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync("DELETE FROM RoutineScoreCell", transaction: tx);
        await conn.ExecuteAsync("DELETE FROM RoutineScoreStatus", transaction: tx);

        tx.Commit();
    }

    public async Task<IReadOnlyList<SoloAwardCandidate>> GetSoloAwardsCandidatesAsync()
    {
        using var conn = _factory.OpenConnection();
        
        // Complex query that:
        // 1. Filters for Solo entries
        // 2. Computes FinalScore using LastSheetKey
        // 3. Assigns bucket based on Class
        // 4. De-duplicates participants keeping best score per bucket
        var results = await conn.QueryAsync<SoloAwardCandidate>(@"
            WITH ScoredRoutines AS (
                -- Get all solo routines with their scores
                SELECT 
                    r.RoutineId,
                    r.ProgramNumber,
                    r.EntryTypeRaw,
                    r.Class,
                    r.ParticipantsRaw AS Participants,
                    r.StudioName,
                    r.RoutineTitle,
                    rss.LastSheetKey,
                    -- Determine bucket
                    CASE 
                        WHEN r.Class LIKE '%School%' THEN 'School'
                        WHEN r.Class LIKE '%Studio%' THEN 'Studio'
                        ELSE NULL
                    END AS Bucket
                FROM Routine r
                INNER JOIN RoutineScoreStatus rss ON r.RoutineId = rss.RoutineId
                WHERE r.EntryTypeRaw LIKE '%Solo%'
                    AND rss.LastSheetKey IS NOT NULL
                    AND rss.IsScored = 1
            ),
            JudgeTotals AS (
                -- Compute sum of all criteria per judge per routine
                SELECT 
                    sr.RoutineId,
                    sr.Bucket,
                    sr.Class,
                    sr.Participants,
                    sr.ProgramNumber,
                    sr.StudioName,
                    sr.RoutineTitle,
                    rsc.JudgeIndex,
                    SUM(COALESCE(rsc.Value, 0)) AS JudgeTotal
                FROM ScoredRoutines sr
                INNER JOIN RoutineScoreCell rsc 
                    ON sr.RoutineId = rsc.RoutineId 
                    AND sr.LastSheetKey = rsc.SheetKey
                GROUP BY sr.RoutineId, sr.Bucket, sr.Class, sr.Participants, 
                         sr.ProgramNumber, sr.StudioName, sr.RoutineTitle, rsc.JudgeIndex
            ),
            RoutineScores AS (
                -- Average judge totals to get final score
                SELECT 
                    RoutineId,
                    Bucket,
                    Class,
                    Participants,
                    ProgramNumber,
                    StudioName,
                    RoutineTitle,
                    AVG(JudgeTotal) AS FinalScore
                FROM JudgeTotals
                WHERE Bucket IS NOT NULL  -- Exclude routines that don't match School or Studio
                GROUP BY RoutineId, Bucket, Class, Participants, ProgramNumber, StudioName, RoutineTitle
            ),
            RankedRoutines AS (
                -- Rank routines per participant per bucket (best score = rank 1)
                SELECT 
                    *,
                    ROW_NUMBER() OVER (
                        PARTITION BY Bucket, Participants 
                        ORDER BY FinalScore DESC
                    ) AS RoutineRank
                FROM RoutineScores
            )
            -- Keep only the best routine per participant per bucket
            SELECT 
                Bucket,
                Class,
                Participants,
                ProgramNumber,
                StudioName,
                RoutineTitle,
                FinalScore
            FROM RankedRoutines
            WHERE RoutineRank = 1
            ORDER BY Bucket, Class, FinalScore DESC
        ");
        
        return results.AsList();
    }
}
