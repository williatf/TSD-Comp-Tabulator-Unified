using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Core.Models;
using Xunit;

namespace Tsd.Tabulator.Data.Sqlite.Tests;

public class ClassConfigServiceTests
{
    [Fact]
    public async Task Seed_and_Upsert_and_GetUnmapped_behave_as_expected()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "TsdTabulatorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);

        var masterPath = Path.Combine(tmpDir, "master.db");
        var eventPath = Path.Combine(tmpDir, "event.db");

        // Ensure empty event DB exists and has schema
        var factory = new SqliteConnectionStringBuilder { DataSource = eventPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        using (var conn = new SqliteConnection(factory))
        {
            conn.Open();
            // create Routine table to allow GetUnmappedClassesAsync to query it
            conn.Execute(@"
                PRAGMA foreign_keys = ON;
                CREATE TABLE IF NOT EXISTS Routine (
                    RoutineId TEXT PRIMARY KEY NOT NULL,
                    Class TEXT NULL
                );
            ");
        }

        var svc = new ClassConfigService(masterPath);

        // Upsert a global class definition
        var def = new ClassDefinition
        {
            ClassKey = "JUNIOR",
            DisplayName = "Junior",
            Bucket = "Studio",
            SortOrder = 10,
            IsActive = true,
            UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        await svc.UpsertClassDefinitionAsync(def, eventPath, saveGlobally: true);

        // Upsert an alias into event only
        await svc.UpsertAliasAsync("Jr", "JUNIOR", eventPath, saveGlobally: false);

        // Seed a fresh event from master - create a new event DB and seed
        var event2 = Path.Combine(tmpDir, "event2.db");
        // create minimal Routine table
        var cs2 = new SqliteConnectionStringBuilder { DataSource = event2, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        using (var c2 = new SqliteConnection(cs2))
        {
            c2.Open();
            c2.Execute("CREATE TABLE IF NOT EXISTS Routine (RoutineId TEXT PRIMARY KEY NOT NULL, Class TEXT NULL);");
        }

        // Since we wrote def to master via saveGlobally above, seeding should copy it into event2
        await svc.SeedEventFromGlobalAsync(event2);
        var defs = (await svc.GetClassDefinitionsAsync(event2)).ToList();
        Assert.Contains(defs, d => d.ClassKey == "JUNIOR");

        // Test GetUnmappedClassesAsync: insert a routine with class text that is not mapped
        using (var c = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = event2 }.ToString()))
        {
            c.Open();
            c.Execute("INSERT INTO Routine(RoutineId, Class) VALUES (@id, @c)", new { id = Guid.NewGuid().ToString(), c = "UnmappedClass" });
        }

        var unmapped = (await svc.GetUnmappedClassesAsync(event2)).ToList();
        Assert.Contains("UnmappedClass", unmapped);

        // Clean up
        Directory.Delete(tmpDir, recursive: true);
    }
}