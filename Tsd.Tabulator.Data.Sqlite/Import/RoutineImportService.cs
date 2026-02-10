using Dapper;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Data.Sqlite.Import;

public sealed class RoutineImportService(
    ISqliteConnectionFactory factory,
    IFingerprintService fingerprint)
{
    private readonly ISqliteConnectionFactory _factory = factory;
    private readonly IFingerprintService _fp = fingerprint;

    public int ImportFromCsv(string csvPath)
    {
        var rows = CsvRoutineParser.ParseFile(csvPath);

        using var conn = _factory.OpenConnection();
        using var tx = conn.BeginTransaction();

        // Is program locked?
        var isLocked = conn.ExecuteScalar<long>(
            "SELECT IsProgramLocked FROM EventState WHERE EventStateId=1;",
            transaction: tx) == 1;

        int imported = 0;

        foreach (var r in rows)
        {
            var participantsNorm = _fp.SplitAndNormalizeParticipants(r.Participants);
            var fingerprint = _fp.ComputeRoutineFingerprint(
                r.StudioName, r.RoutineTitle, r.EntryType, r.Category, r.Class, participantsNorm);

            var existingRoutineId = conn.ExecuteScalar<string?>(
                "SELECT RoutineId FROM Routine WHERE Fingerprint = @Fingerprint;",
                new { Fingerprint = fingerprint },
                transaction: tx);

            string routineId;
            if (existingRoutineId is null)
            {
                routineId = Guid.NewGuid().ToString("N");

                conn.Execute("""
                    INSERT INTO Routine
                    (RoutineId, ProgramNumber, StartTimeText, EntryTypeRaw, Category, Class, StudioName, RoutineTitle,
                     ParticipantsRaw, Fingerprint, FingerprintVersion, IsInactive)
                    VALUES
                    (@RoutineId, @ProgramNumber, @StartTimeText, @EntryTypeRaw, @Category, @Class, @StudioName, @RoutineTitle,
                     @ParticipantsRaw, @Fingerprint, 1, 0);
                """, new
                {
                    RoutineId = routineId,
                    ProgramNumber = r.ProgramNumber,
                    StartTimeText = r.StartTime,
                    EntryTypeRaw = r.EntryType,
                    Category = r.Category,
                    Class = r.Class,
                    StudioName = r.StudioName,
                    RoutineTitle = r.RoutineTitle,
                    ParticipantsRaw = r.Participants,
                    Fingerprint = fingerprint
                }, transaction: tx);
            }
            else
            {
                routineId = existingRoutineId;

                // Update descriptive fields; update program number only if not locked
                conn.Execute("""
                    UPDATE Routine
                    SET StartTimeText=@StartTimeText,
                        EntryTypeRaw=@EntryTypeRaw,
                        Category=@Category,
                        Class=@Class,
                        StudioName=@StudioName,
                        RoutineTitle=@RoutineTitle,
                        ParticipantsRaw=@ParticipantsRaw,
                        UpdatedAtUtc=(strftime('%Y-%m-%dT%H:%M:%fZ','now'))
                    WHERE RoutineId=@RoutineId;
                """, new
                {
                    RoutineId = routineId,
                    StartTimeText = r.StartTime,
                    EntryTypeRaw = r.EntryType,
                    Category = r.Category,
                    Class = r.Class,
                    StudioName = r.StudioName,
                    RoutineTitle = r.RoutineTitle,
                    ParticipantsRaw = r.Participants
                }, transaction: tx);

                if (!isLocked)
                {
                    conn.Execute("""
                        UPDATE Routine SET ProgramNumber=@ProgramNumber
                        WHERE RoutineId=@RoutineId;
                    """, new { RoutineId = routineId, ProgramNumber = r.ProgramNumber }, transaction: tx);
                }
            }

            // Participants upsert + re-link
            conn.Execute("DELETE FROM RoutineParticipant WHERE RoutineId=@RoutineId;",
                new { RoutineId = routineId }, transaction: tx);

            int sort = 0;
            foreach (var pn in participantsNorm)
            {
                // upsert participant by NormalizedName
                var pid = conn.ExecuteScalar<string?>(
                    "SELECT ParticipantId FROM Participant WHERE NormalizedName=@N;",
                    new { N = pn }, transaction: tx);

                if (pid is null)
                {
                    pid = Guid.NewGuid().ToString("N");
                    conn.Execute("""
                        INSERT INTO Participant(ParticipantId, DisplayName, NormalizedName)
                        VALUES (@ParticipantId, @DisplayName, @NormalizedName);
                    """, new { ParticipantId = pid, DisplayName = pn, NormalizedName = pn }, transaction: tx);
                }

                conn.Execute("""
                    INSERT INTO RoutineParticipant(RoutineId, ParticipantId, SortOrder)
                    VALUES (@RoutineId, @ParticipantId, @SortOrder);
                """, new { RoutineId = routineId, ParticipantId = pid, SortOrder = sort++ }, transaction: tx);
            }

            imported++;
        }

        tx.Commit();
        return imported;
    }

    public IReadOnlyList<RoutineRow> LoadRoutinesForGrid()
    {
        using var conn = _factory.OpenConnection();

        // Simple projection for now (participants raw)
        var list = conn.Query<RoutineRow>("""
            SELECT
                StartTimeText AS StartTime,
                ProgramNumber,
                EntryTypeRaw AS EntryType,
                COALESCE(Category,'') AS Category,
                COALESCE(Class,'') AS Class,
                COALESCE(ParticipantsRaw,'') AS Participants,
                COALESCE(StudioName,'') AS StudioName,
                RoutineTitle
            FROM Routine
            WHERE IsInactive = 0
            ORDER BY ProgramNumber;
        """).ToList();

        return list;
    }
}
