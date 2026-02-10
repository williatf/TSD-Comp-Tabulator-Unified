using System.Data;
using Dapper;

namespace Tsd.Tabulator.Data.Sqlite;

public sealed class SchemaInitializer
{
    private readonly ISqliteConnectionFactory _factory;

    public SchemaInitializer(ISqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public void EnsureSchema()
    {
        using var conn = _factory.OpenConnection();

        conn.Execute(@"
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS DbInfo (
                SchemaVersion INTEGER NOT NULL,
                AppliedAtUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                Notes TEXT NULL,
                CONSTRAINT PK_DbInfo PRIMARY KEY (SchemaVersion)
            );

            CREATE TABLE IF NOT EXISTS EventState (
                EventStateId INTEGER NOT NULL CONSTRAINT PK_EventState PRIMARY KEY CHECK (EventStateId = 1),
                EventName TEXT NULL,
                EventDateLocal TEXT NULL,
                IsProgramLocked INTEGER NOT NULL DEFAULT 0 CHECK (IsProgramLocked IN (0,1)),
                ProgramLockedAtUtc TEXT NULL
            );
            INSERT OR IGNORE INTO EventState(EventStateId) VALUES (1);

            CREATE TABLE IF NOT EXISTS AppSettings (
                ConfigKey TEXT PRIMARY KEY NOT NULL,
                ConfigValue TEXT NULL,
                UpdatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
            );

            CREATE TABLE IF NOT EXISTS Routine (
                RoutineId TEXT PRIMARY KEY NOT NULL,
                ProgramNumber INTEGER NOT NULL,
                StartTimeText TEXT NULL,
                EntryTypeRaw TEXT NOT NULL,
                Category TEXT NULL,
                Class TEXT NULL,
                StudioName TEXT NULL,
                RoutineTitle TEXT NOT NULL,
                ParticipantsRaw TEXT NULL,
                Fingerprint TEXT NOT NULL,
                FingerprintVersion INTEGER NOT NULL DEFAULT 1,
                IsInactive INTEGER NOT NULL DEFAULT 0 CHECK (IsInactive IN (0,1)),
                CreatedAtUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                UpdatedAtUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                CONSTRAINT PK_Routine PRIMARY KEY (RoutineId)
            );

            CREATE UNIQUE INDEX IF NOT EXISTS UX_Routine_Fingerprint ON Routine(Fingerprint);
            CREATE UNIQUE INDEX IF NOT EXISTS UX_Routine_ProgramNumber ON Routine(ProgramNumber);

            CREATE TABLE IF NOT EXISTS Participant (
                ParticipantId TEXT NOT NULL,
                DisplayName TEXT NOT NULL,
                NormalizedName TEXT NOT NULL,
                CONSTRAINT PK_Participant PRIMARY KEY (ParticipantId)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS UX_Participant_NormalizedName ON Participant(NormalizedName);

            CREATE TABLE IF NOT EXISTS RoutineParticipant (
                RoutineId TEXT NOT NULL,
                ParticipantId TEXT NOT NULL,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                CONSTRAINT PK_RoutineParticipant PRIMARY KEY (RoutineId, ParticipantId),
                CONSTRAINT FK_RP_Routine FOREIGN KEY (RoutineId) REFERENCES Routine(RoutineId) ON DELETE CASCADE,
                CONSTRAINT FK_RP_Participant FOREIGN KEY (ParticipantId) REFERENCES Participant(ParticipantId) ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS ClassDefinitions (
                ClassKey TEXT PRIMARY KEY NOT NULL,
                DisplayName TEXT NOT NULL,
                Bucket TEXT NOT NULL,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
                UpdatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
            );

            CREATE TABLE IF NOT EXISTS ClassAliases (
                Alias TEXT PRIMARY KEY NOT NULL,
                ClassKey TEXT NOT NULL,
                UpdatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                CONSTRAINT FK_CA_Class FOREIGN KEY (ClassKey) REFERENCES ClassDefinitions(ClassKey) ON DELETE RESTRICT
            );

            /*
             * Scoring schema additions
             * - RoutineScoreSet: one row per routine+sheet (unique)
             * - RoutineScoreCell: per-judge, per-criterion value cells
             * - RoutineScoreStatus: optional quick flag indicating whether any scores exist for a routine
             *
             * Note: Routine table referenced (singular) since existing schema uses `Routine`.
             */

            CREATE TABLE IF NOT EXISTS RoutineScoreSet (
                ScoreSetId     INTEGER PRIMARY KEY AUTOINCREMENT,
                RoutineId      TEXT    NOT NULL,
                SheetKey       TEXT    NOT NULL,
                CreatedUtc     TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                UpdatedUtc     TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                CONSTRAINT UX_RoutineScoreSet_Unique UNIQUE(RoutineId, SheetKey),
                CONSTRAINT FK_RSS_Routine FOREIGN KEY(RoutineId) REFERENCES Routine(RoutineId) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS RoutineScoreCell (
                RoutineId      TEXT    NOT NULL,
                SheetKey       TEXT    NOT NULL,
                JudgeIndex     INTEGER NOT NULL,
                CriterionKey   TEXT    NOT NULL,
                Value          REAL    NULL,
                UpdatedUtc     TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                CONSTRAINT PK_RoutineScoreCell PRIMARY KEY (RoutineId, SheetKey, JudgeIndex, CriterionKey),
                CONSTRAINT FK_RSC_Routine FOREIGN KEY(RoutineId) REFERENCES Routine(RoutineId) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS RoutineScoreStatus (
                RoutineId      TEXT    PRIMARY KEY,
                IsScored       INTEGER NOT NULL DEFAULT 0,
                UpdatedUtc     TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                CONSTRAINT FK_RSSS_Routine FOREIGN KEY(RoutineId) REFERENCES Routine(RoutineId) ON DELETE CASCADE
            );

            INSERT OR IGNORE INTO DbInfo(SchemaVersion, Notes)
            VALUES (3, 'Add LastSheetkey to table RoutineScoreStatus');
        ");

        // Migration: Add LastSheetKey if table exists but column doesn't
        var hasColumn = conn.ExecuteScalar<int>(@"
            SELECT COUNT(*) FROM pragma_table_info('RoutineScoreStatus') 
            WHERE name='LastSheetKey'
        ");

        if (hasColumn == 0)
        {
            conn.Execute("ALTER TABLE RoutineScoreStatus ADD COLUMN LastSheetKey TEXT NULL;");
        }
    }
}
