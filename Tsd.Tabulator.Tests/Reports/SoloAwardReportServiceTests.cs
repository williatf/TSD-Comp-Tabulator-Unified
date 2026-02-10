using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Services;
using Xunit;

namespace Tsd.Tabulator.Tests.Reports;

public sealed class SoloAwardReportServiceTests
{
    [Fact]
    public async Task GenerateReportAsync_WithTiedScores_AssignsSharedPlaceAndSkipsNext()
    {
        // Arrange
        var mockRepo = new MockScoreRepository(new List<SoloAwardCandidate>
        {
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Alice", ProgramNumber = 101, StudioName = "Dance Co", RoutineTitle = "Routine A", FinalScore = 95.5 },
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Bob", ProgramNumber = 102, StudioName = "Dance Co", RoutineTitle = "Routine B", FinalScore = 95.5 },    // Tied for 1st
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Charlie", ProgramNumber = 103, StudioName = "Dance Co", RoutineTitle = "Routine C", FinalScore = 92.0 } // Should be 3rd
        });
        var mockClassConfig = new MockClassConfigService();
        var service = new SoloAwardReportService(mockRepo, mockClassConfig, "test.db");

        // Act
        var report = await service.GenerateReportAsync();

        // Assert
        var group = report.Groups.Single();
        Assert.Equal(3, group.Entries.Count);
        
        Assert.Equal(1, group.Entries[0].Place);
        Assert.Equal(95.5, group.Entries[0].FinalScore);
        
        Assert.Equal(1, group.Entries[1].Place); // Tied for 1st
        Assert.Equal(95.5, group.Entries[1].FinalScore);
        
        Assert.Equal(3, group.Entries[2].Place); // 3rd (skipped 2nd)
        Assert.Equal(92.0, group.Entries[2].FinalScore);
    }

    [Fact]
    public async Task GenerateReportAsync_GroupsByBucketAndClass()
    {
        // Arrange
        var mockRepo = new MockScoreRepository(new List<SoloAwardCandidate>
        {
            new() { Bucket = "School", Class = "Teen School", Participants = "Alice", ProgramNumber = 101, StudioName = "School A", RoutineTitle = "Routine 1", FinalScore = 90.0 },
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Bob", ProgramNumber = 102, StudioName = "Studio B", RoutineTitle = "Routine 2", FinalScore = 85.0 },
            new() { Bucket = "School", Class = "Junior School", Participants = "Charlie", ProgramNumber = 103, StudioName = "School C", RoutineTitle = "Routine 3", FinalScore = 88.0 }
        });
        var mockClassConfig = new MockClassConfigService();
        var service = new SoloAwardReportService(mockRepo, mockClassConfig, "test.db");

        // Act
        var report = await service.GenerateReportAsync();

        // Assert
        Assert.Equal(3, report.Groups.Count);
        
        // Verify buckets and classes
        Assert.Contains(report.Groups, g => g.Bucket == "School" && g.Class == "Junior School");
        Assert.Contains(report.Groups, g => g.Bucket == "School" && g.Class == "Teen School");
        Assert.Contains(report.Groups, g => g.Bucket == "Studio" && g.Class == "Teen Studio");
    }

    [Fact]
    public async Task GenerateReportAsync_RanksWithinEachGroup()
    {
        // Arrange
        var mockRepo = new MockScoreRepository(new List<SoloAwardCandidate>
        {
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Alice", ProgramNumber = 101, StudioName = "Studio A", RoutineTitle = "Routine 1", FinalScore = 95.0 },
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Bob", ProgramNumber = 102, StudioName = "Studio B", RoutineTitle = "Routine 2", FinalScore = 90.0 },
            new() { Bucket = "Studio", Class = "Teen Studio", Participants = "Charlie", ProgramNumber = 103, StudioName = "Studio C", RoutineTitle = "Routine 3", FinalScore = 92.0 }
        });
        var mockClassConfig = new MockClassConfigService();
        var service = new SoloAwardReportService(mockRepo, mockClassConfig, "test.db");

        // Act
        var report = await service.GenerateReportAsync();

        // Assert
        var group = report.Groups.Single();
        Assert.Equal(3, group.Entries.Count);
        
        // Should be sorted by score descending
        Assert.Equal(1, group.Entries[0].Place);
        Assert.Equal(95.0, group.Entries[0].FinalScore);
        
        Assert.Equal(2, group.Entries[1].Place);
        Assert.Equal(92.0, group.Entries[1].FinalScore);
        
        Assert.Equal(3, group.Entries[2].Place);
        Assert.Equal(90.0, group.Entries[2].FinalScore);
    }

    // Mock repository for testing
    private sealed class MockScoreRepository : IScoreRepository
    {
        private readonly List<SoloAwardCandidate> _candidates;

        public MockScoreRepository(List<SoloAwardCandidate> candidates)
        {
            _candidates = candidates;
        }

        public Task<IReadOnlyList<SoloAwardCandidate>> GetSoloAwardsCandidatesAsync()
        {
            return Task.FromResult<IReadOnlyList<SoloAwardCandidate>>(_candidates);
        }

        public Task<IReadOnlyList<DuetAwardCandidate>> GetDuetAwardsCandidatesAsync()
        {
            // Return empty list for mock - duet tests would be in a separate test class
            return Task.FromResult<IReadOnlyList<DuetAwardCandidate>>(
                new List<DuetAwardCandidate>().AsReadOnly()
            );
        }

        // Other methods not needed for these tests
        public Task<IReadOnlyList<RoutineScoreCellRow>> GetCellsAsync(string routineId, string sheetKey) => throw new System.NotImplementedException();
        public Task SaveCellsAsync(string routineId, string sheetKey, IEnumerable<RoutineScoreCellRow> cells) => throw new System.NotImplementedException();
        public Task<IReadOnlyList<RoutineRow>> GetAllRoutinesAsync() => throw new System.NotImplementedException();
        public Task<string?> GetLastSheetKeyAsync(string routineId) => throw new System.NotImplementedException();
        public Task SetLastSheetKeyAsync(string routineId, string sheetKey) => throw new System.NotImplementedException();
        public Task DeleteScoresExceptSheetAsync(string routineId, string keepSheetKey) => throw new System.NotImplementedException();
        public Task ClearAllScoresAsync() => throw new System.NotImplementedException();
    }

    // Mock class config service for testing
    private sealed class MockClassConfigService : IClassConfigService
    {
        public Task SeedEventFromGlobalAsync(string eventDbPath) => Task.CompletedTask;

        public Task UpsertClassDefinitionAsync(ClassDefinition def, string eventDbPath, bool saveGlobally = true) => Task.CompletedTask;

        public Task UpsertAliasAsync(string alias, string classKey, string eventDbPath, bool saveGlobally = true) => Task.CompletedTask;

        public Task DeleteClassDefinitionAsync(string classKey, string eventDbPath, bool deleteGlobally = true) => Task.CompletedTask;

        public Task DeleteAliasAsync(string alias, string eventDbPath, bool deleteGlobally = true) => Task.CompletedTask;

        public Task<IEnumerable<string>> GetUnmappedClassesAsync(string eventDbPath) => 
            Task.FromResult(Enumerable.Empty<string>());

        public Task<IEnumerable<ClassDefinition>> GetClassDefinitionsAsync(string eventDbPath) =>
            Task.FromResult(Enumerable.Empty<ClassDefinition>());

        public Task<IEnumerable<ClassAlias>> GetAliasesAsync(string eventDbPath) =>
            Task.FromResult(Enumerable.Empty<ClassAlias>());

        public Task<string?> ResolveClassKeyAsync(string? classText, string eventDbPath) =>
            Task.FromResult(classText);
    }
}
