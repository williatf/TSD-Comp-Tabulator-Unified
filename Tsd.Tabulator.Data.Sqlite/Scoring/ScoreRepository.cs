using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reports.Raw;
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
            }, transaction: tx);
        }

        tx.Commit();
    }

    public async Task<IReadOnlyList<RoutineRow>> GetAllRoutinesAsync()
    {
        using var conn = _factory.OpenConnection();
        var rows = await conn.QueryAsync<RoutineRow>(
            @"SELECT RoutineId, 
                     '' AS StartTime,
                     ProgramNumber, 
                     EntryTypeRaw AS EntryType,
                     '' AS Category,
                     Class, 
                     ParticipantsRaw AS Participants,
                     StudioName, 
                     RoutineTitle
              FROM Routine
              ORDER BY ProgramNumber");
        return rows.AsList();
    }

    public async Task<string?> GetLastSheetKeyAsync(string routineId)
    {
        using var conn = _factory.OpenConnection();
        var result = await conn.QuerySingleOrDefaultAsync<string?>(
            "SELECT LastSheetKey FROM RoutineScoreStatus WHERE RoutineId = @RoutineId",
            new { RoutineId = routineId });
        return result;
    }

    public async Task SetLastSheetKeyAsync(string routineId, string sheetKey)
    {
        using var conn = _factory.OpenConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO RoutineScoreStatus (RoutineId, LastSheetKey, IsScored)
            VALUES (@RoutineId, @SheetKey, 1)
            ON CONFLICT(RoutineId) DO UPDATE SET
                LastSheetKey = excluded.LastSheetKey,
                IsScored = excluded.IsScored
        ", new { RoutineId = routineId, SheetKey = sheetKey });
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

    public async Task<IReadOnlyList<ScoredRoutineRow>> GetScoredRoutinesAsync(string entryType)
    {
        using var conn = _factory.OpenConnection();

        var sql = @"
        SELECT
            r.RoutineId,
            r.ProgramNumber,
            r.Class,
            r.ParticipantsRaw AS Participants,
            r.StudioName,
            r.RoutineTitle,
            rss.LastSheetKey
        FROM Routine r
        JOIN RoutineScoreStatus rss ON r.RoutineId = rss.RoutineId
        WHERE r.EntryTypeRaw LIKE @entryType
          AND rss.IsScored = 1
          AND rss.LastSheetKey IS NOT NULL;
    ";

        var results = await conn.QueryAsync<ScoredRoutineRow>(sql, new { entryType });
        return results.AsList();
    }

    public async Task<IReadOnlyList<ScoreCellRow>> GetScoreCellsAsync(string routineId, string sheetKey)
    {
        using var conn = _factory.OpenConnection();

        var sql = @"
        SELECT JudgeIndex, Value
        FROM RoutineScoreCell
        WHERE RoutineId = @routineId
          AND SheetKey = @sheetKey;
    ";

        var results = await conn.QueryAsync<ScoreCellRow>(sql, new { routineId, sheetKey });
        return results.AsList();
    }

}
