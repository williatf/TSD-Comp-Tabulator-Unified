using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite.Scoring;
using Xunit;

namespace Tsd.Tabulator.Tests.Reports;

/// <summary>
/// Integration tests for solo award repository queries.
/// Tests participant de-duplication logic using actual SQLite database.
/// </summary>
public sealed class SoloAwardRepositoryTests
{
    [Fact]
    public async Task GetSoloAwardsCandidatesAsync_ParticipantWithMultipleRoutines_KeepsBestScorePerBucket()
    {
        // Arrange
        var factory = new InMemorySqliteConnectionFactory();
        var schemaInit = new SchemaInitializer(factory);
        schemaInit.EnsureSchema();
        
        var repo = new ScoreRepository(factory);
        
        // Create test data using direct SQL
        using (var conn = factory.OpenConnection())
        {
            // Insert routines for same participant "Alice" in Studio bucket
            await Dapper.SqlMapper.ExecuteAsync(conn, @"
                INSERT INTO Routine (RoutineId, ProgramNumber, EntryTypeRaw, Class, ParticipantsRaw, StudioName, RoutineTitle, Fingerprint)
                VALUES 
                    ('R1', 101, 'Solo', 'Teen Studio', 'Alice', 'Studio A', 'Routine 1', 'FP1'),
                    ('R2', 102, 'Solo', 'Teen Studio', 'Alice', 'Studio A', 'Routine 2', 'FP2'),
                    ('R3', 103, 'Solo', 'Junior School', 'Alice', 'School B', 'Routine 3', 'FP3')
            ");
            
            // Mark as scored with sheet keys
            await Dapper.SqlMapper.ExecuteAsync(conn, @"
                INSERT INTO RoutineScoreStatus (RoutineId, IsScored, LastSheetKey)
                VALUES 
                    ('R1', 1, 'Sheet1'),
                    ('R2', 1, 'Sheet1'),
                    ('R3', 1, 'Sheet1')
            ");
            
            // Insert scores - R2 has highest score in Studio bucket
            // R1: Judge totals = 90+92 = 182/2 = 91.0
            await Dapper.SqlMapper.ExecuteAsync(conn, @"
                INSERT INTO RoutineScoreCell (RoutineId, SheetKey, JudgeIndex, CriterionKey, Value)
                VALUES 
                    ('R1', 'Sheet1', 1, 'Tech', 45.0),
                    ('R1', 'Sheet1', 1, 'Art', 45.0),
                    ('R1', 'Sheet1', 2, 'Tech', 46.0),
                    ('R1', 'Sheet1', 2, 'Art', 46.0)
            ");
            
            // R2: Judge totals = 95+96 = 191/2 = 95.5
            await Dapper.SqlMapper.ExecuteAsync(conn, @"
                INSERT INTO RoutineScoreCell (RoutineId, SheetKey, JudgeIndex, CriterionKey, Value)
                VALUES 
                    ('R2', 'Sheet1', 1, 'Tech', 47.5),
                    ('R2', 'Sheet1', 1, 'Art', 47.5),
                    ('R2', 'Sheet1', 2, 'Tech', 48.0),
                    ('R2', 'Sheet1', 2, 'Art', 48.0)
            ");
            
            // R3 (different bucket): Judge totals = 88+89 = 177/2 = 88.5
            await Dapper.SqlMapper.ExecuteAsync(conn, @"
                INSERT INTO RoutineScoreCell (RoutineId, SheetKey, JudgeIndex, CriterionKey, Value)
                VALUES 
                    ('R3', 'Sheet1', 1, 'Tech', 44.0),
                    ('R3', 'Sheet1', 1, 'Art', 44.0),
                    ('R3', 'Sheet1', 2, 'Tech', 44.5),
                    ('R3', 'Sheet1', 2, 'Art', 44.5)
            ");
        }

        // Act
        var candidates = await repo.GetSoloAwardsCandidatesAsync();

        // Assert
        Assert.Equal(2, candidates.Count); // One per bucket
        
        var studioBucket = candidates.Where(c => c.Bucket == "Studio").ToList();
        var schoolBucket = candidates.Where(c => c.Bucket == "School").ToList();
        
        Assert.Single(studioBucket);
        Assert.Equal(102L, studioBucket[0].ProgramNumber); // R2 (best in Studio)
        Assert.Equal(95.5, studioBucket[0].FinalScore, precision: 2);
        
        Assert.Single(schoolBucket);
        Assert.Equal(103L, schoolBucket[0].ProgramNumber); // R3 (only one in School)
        Assert.Equal(88.5, schoolBucket[0].FinalScore, precision: 2);
    }
}
