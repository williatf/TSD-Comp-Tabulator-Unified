using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Application.Interfaces;

namespace Tsd.Tabulator.Data.Sqlite;

public sealed class ClassConfigService : IClassConfigService
{
    private readonly string _masterPath;

    /// <summary>
    /// Default constructor - uses %AppData%\TsdTabulator\master-config.db
    /// </summary>
    public ClassConfigService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TsdTabulator", "master-config.db"))
    {
    }

    /// <summary>
    /// Constructor allowing an explicit master path (useful for unit tests).
    /// </summary>
    public ClassConfigService(string masterPath)
    {
        var dir = Path.GetDirectoryName(masterPath) ?? Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))!;
        Directory.CreateDirectory(dir);
        _masterPath = masterPath;
        EnsureMasterSchema();
    }

    private void EnsureMasterSchema()
    {
        using var conn = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _masterPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString());
        conn.Open();

        conn.Execute("""
            PRAGMA foreign_keys = ON;

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
        """);
    }

    private static string EventCs(string eventDbPath) =>
        new SqliteConnectionStringBuilder { DataSource = eventDbPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();

    private void EnsureEventSchema(string eventDbPath)
    {
        using var conn = new SqliteConnection(EventCs(eventDbPath));
        conn.Open();

        conn.Execute("""
            PRAGMA foreign_keys = ON;

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
        """);
    }

    public async Task SeedEventFromGlobalAsync(string eventDbPath)
    {
        if (string.IsNullOrWhiteSpace(eventDbPath))
            throw new ArgumentNullException(nameof(eventDbPath));

        EnsureEventSchema(eventDbPath);

        using var master = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _masterPath }.ToString());
        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await master.OpenAsync();
        await ev.OpenAsync();

        var defs = await master.QueryAsync<ClassDefinition>("SELECT ClassKey, DisplayName, Bucket, SortOrder, IsActive, UpdatedUtc FROM ClassDefinitions");
        var aliases = await master.QueryAsync<ClassAlias>("SELECT Alias, ClassKey, UpdatedUtc FROM ClassAliases");

        using var tx = ev.BeginTransaction();
        // Merge master definitions into event DB (insert or update)
        foreach (var d in defs)
        {
            await ev.ExecuteAsync(@"
                INSERT INTO ClassDefinitions(ClassKey, DisplayName, Bucket, SortOrder, IsActive, UpdatedUtc)
                VALUES (@ClassKey, @DisplayName, @Bucket, @SortOrder, @IsActive, @UpdatedUtc)
                ON CONFLICT(ClassKey) DO UPDATE SET
                    DisplayName=excluded.DisplayName,
                    Bucket=excluded.Bucket,
                    SortOrder=excluded.SortOrder,
                    IsActive=excluded.IsActive,
                    UpdatedUtc=excluded.UpdatedUtc
            ", d, transaction: tx);
        }

        // Merge master aliases into event DB (insert or update)
        foreach (var a in aliases)
        {
            await ev.ExecuteAsync(@"
                INSERT INTO ClassAliases(Alias, ClassKey, UpdatedUtc)
                VALUES (@Alias, @ClassKey, @UpdatedUtc)
                ON CONFLICT(Alias) DO UPDATE SET
                    ClassKey=excluded.ClassKey,
                    UpdatedUtc=excluded.UpdatedUtc
            ", a, transaction: tx);
        }

        tx.Commit();
    }

    public async Task UpsertClassDefinitionAsync(ClassDefinition def, string eventDbPath, bool saveGlobally = true)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        EnsureEventSchema(eventDbPath);

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        await ev.ExecuteAsync(@"
            INSERT INTO ClassDefinitions(ClassKey, DisplayName, Bucket, SortOrder, IsActive, UpdatedUtc)
            VALUES (@ClassKey, @DisplayName, @Bucket, @SortOrder, @IsActive, COALESCE(NULLIF(@UpdatedUtc,''), (strftime('%Y-%m-%dT%H:%M:%fZ','now'))))
            ON CONFLICT(ClassKey) DO UPDATE SET
                DisplayName=excluded.DisplayName,
                Bucket=excluded.Bucket,
                SortOrder=excluded.SortOrder,
                IsActive=excluded.IsActive,
                UpdatedUtc=excluded.UpdatedUtc
        ", def);

        if (saveGlobally)
        {
            using var master = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _masterPath }.ToString());
            await master.OpenAsync();
            await master.ExecuteAsync(@"
                INSERT INTO ClassDefinitions(ClassKey, DisplayName, Bucket, SortOrder, IsActive, UpdatedUtc)
                VALUES (@ClassKey, @DisplayName, @Bucket, @SortOrder, @IsActive, COALESCE(NULLIF(@UpdatedUtc,''), (strftime('%Y-%m-%dT%H:%M:%fZ','now'))))
                ON CONFLICT(ClassKey) DO UPDATE SET
                    DisplayName=excluded.DisplayName,
                    Bucket=excluded.Bucket,
                    SortOrder=excluded.SortOrder,
                    IsActive=excluded.IsActive,
                    UpdatedUtc=excluded.UpdatedUtc
            ", def);
        }
    }

    public async Task UpsertAliasAsync(string alias, string classKey, string eventDbPath, bool saveGlobally = true)
    {
        if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));
        if (string.IsNullOrWhiteSpace(classKey)) throw new ArgumentNullException(nameof(classKey));
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        EnsureEventSchema(eventDbPath);

        var param = new { Alias = alias.Trim(), ClassKey = classKey.Trim(), UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") };

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        await ev.ExecuteAsync(@"
            INSERT INTO ClassAliases(Alias, ClassKey, UpdatedUtc)
            VALUES (@Alias, @ClassKey, @UpdatedUtc)
            ON CONFLICT(Alias) DO UPDATE SET
                ClassKey=excluded.ClassKey,
                UpdatedUtc=excluded.UpdatedUtc
        ", param);

        if (saveGlobally)
        {
            using var master = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _masterPath }.ToString());
            await master.OpenAsync();
            await master.ExecuteAsync(@"
                INSERT INTO ClassAliases(Alias, ClassKey, UpdatedUtc)
                VALUES (@Alias, @ClassKey, @UpdatedUtc)
                ON CONFLICT(Alias) DO UPDATE SET
                    ClassKey=excluded.ClassKey,
                    UpdatedUtc=excluded.UpdatedUtc
            ", param);
        }
    }

    public async Task DeleteClassDefinitionAsync(string classKey, string eventDbPath, bool deleteGlobally = true)
    {
        if (string.IsNullOrWhiteSpace(classKey)) throw new ArgumentNullException(nameof(classKey));
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        // Delete aliases first due to foreign key constraint (ON DELETE RESTRICT)
        await ev.ExecuteAsync("DELETE FROM ClassAliases WHERE ClassKey = @ClassKey", new { ClassKey = classKey });
        
        // Then delete the definition
        await ev.ExecuteAsync("DELETE FROM ClassDefinitions WHERE ClassKey = @ClassKey", new { ClassKey = classKey });

        if (deleteGlobally)
        {
            using var master = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _masterPath }.ToString());
            await master.OpenAsync();
            
            // Delete aliases first
            await master.ExecuteAsync("DELETE FROM ClassAliases WHERE ClassKey = @ClassKey", new { ClassKey = classKey });
            
            // Then delete the definition
            await master.ExecuteAsync("DELETE FROM ClassDefinitions WHERE ClassKey = @ClassKey", new { ClassKey = classKey });
        }
    }

    public async Task DeleteAliasAsync(string alias, string eventDbPath, bool deleteGlobally = true)
    {
        if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();
        
        await ev.ExecuteAsync("DELETE FROM ClassAliases WHERE Alias = @Alias", new { Alias = alias });

        if (deleteGlobally)
        {
            using var master = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _masterPath }.ToString());
            await master.OpenAsync();
            
            await master.ExecuteAsync("DELETE FROM ClassAliases WHERE Alias = @Alias", new { Alias = alias });
        }
    }

    public async Task<IEnumerable<string>> GetUnmappedClassesAsync(string eventDbPath)
    {
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        var result = await ev.QueryAsync<string>(@"
            SELECT DISTINCT trim(Class) AS ClassText
            FROM Routine
            WHERE Class IS NOT NULL AND trim(Class) <> ''
              AND NOT EXISTS (SELECT 1 FROM ClassAliases WHERE Alias = trim(Class))
              AND NOT EXISTS (SELECT 1 FROM ClassDefinitions WHERE ClassKey = trim(Class));
        ");
        return result.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
    }

    public async Task<IEnumerable<ClassDefinition>> GetClassDefinitionsAsync(string eventDbPath)
    {
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        var defs = await ev.QueryAsync<ClassDefinition>(@"
            SELECT ClassKey, DisplayName, Bucket, SortOrder, IsActive, UpdatedUtc
            FROM ClassDefinitions
            ORDER BY SortOrder, DisplayName
        ");
        return defs.ToArray();
    }

    public async Task<IEnumerable<ClassAlias>> GetAliasesAsync(string eventDbPath)
    {
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        var aliases = await ev.QueryAsync<ClassAlias>(@"
            SELECT Alias, ClassKey, UpdatedUtc
            FROM ClassAliases
            ORDER BY Alias
        ");
        return aliases.ToArray();
    }

    public async Task<string?> ResolveClassKeyAsync(string? classText, string eventDbPath)
    {
        if (string.IsNullOrWhiteSpace(eventDbPath)) throw new ArgumentNullException(nameof(eventDbPath));
        if (string.IsNullOrWhiteSpace(classText)) return null;

        var trimmed = classText.Trim();

        using var ev = new SqliteConnection(EventCs(eventDbPath));
        await ev.OpenAsync();

        // Check alias first
        var aliasMatch = await ev.QuerySingleOrDefaultAsync<string?>(@"
            SELECT ClassKey FROM ClassAliases WHERE Alias = @Alias
        ", new { Alias = trimmed });

        if (!string.IsNullOrWhiteSpace(aliasMatch)) return aliasMatch;

        // Check if classText already exactly matches a ClassKey
        var defMatch = await ev.QuerySingleOrDefaultAsync<string?>(@"
            SELECT ClassKey FROM ClassDefinitions WHERE ClassKey = @Key
        ", new { Key = trimmed });

        if (!string.IsNullOrWhiteSpace(defMatch)) return defMatch;

        return null;
    }
}