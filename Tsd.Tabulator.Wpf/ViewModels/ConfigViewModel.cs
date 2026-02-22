using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Core.Scoring;
using Tsd.Tabulator.Data.Sqlite;
using System.Windows;
using Tsd.Tabulator.Data.Sqlite.Scoring;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class ConfigViewModel : Screen
{
    private readonly ShellViewModel _shell;
    private readonly ScoreRepository _scoreRepo;
    private readonly IClassConfigService _classConfigService;

    public ObservableCollection<ClassDefinition> ClassDefinitions { get; } = new();
    public ObservableCollection<ClassAlias> ClassAliases { get; } = new();
    public ObservableCollection<string> UnmappedClasses { get; } = new();
    public ObservableCollection<CompetitionTypeOption> AvailableCompetitionTypes { get; } = new();

    private bool _saveGlobally = true;
    public bool SaveGlobally
    {
        get => _saveGlobally;
        set
        {
            _saveGlobally = value;
            NotifyOfPropertyChange(() => SaveGlobally);
        }
    }

    private CompetitionTypeOption? _selectedCompetitionType;
    public CompetitionTypeOption? SelectedCompetitionType
    {
        get => _selectedCompetitionType;
        set
        {
            if (_selectedCompetitionType != value)
            {
                _selectedCompetitionType = value;
                NotifyOfPropertyChange(() => SelectedCompetitionType);
                // Notify shell of the actual enum value change
                if (value != null)
                {
                    _shell.CurrentCompetitionType = value.Value;
                    // Save to database immediately - properly await it
                    Task.Run(async () => await SaveCompetitionTypeAsync()).Wait();
                }
            }
        }
    }

    private async Task SaveCompetitionTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(_shell.CurrentDbPath))
            return;
            
        var factory = new SqliteConnectionFactory(_shell.CurrentDbPath);
        using var conn = factory.OpenConnection();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO AppSettings (ConfigKey, ConfigValue)
            VALUES ('CompetitionType', @value)";
        cmd.Parameters.AddWithValue("@value", ((int)_shell.CurrentCompetitionType).ToString());
        
        await cmd.ExecuteNonQueryAsync();
    }

    // Update the SelectedUnmapped property to notify the Can method
    private string? _selectedUnmapped;
    public string? SelectedUnmapped
    {
        get => _selectedUnmapped;
        set
        {
            _selectedUnmapped = value;
            NotifyOfPropertyChange(() => SelectedUnmapped);
            NotifyOfPropertyChange(() => CanCreateDefinitionFromUnmapped);
        }
    }

    private ClassDefinition? _selectedDefinition;
    public ClassDefinition? SelectedDefinition
    {
        get => _selectedDefinition;
        set
        {
            _selectedDefinition = value;
            NotifyOfPropertyChange(() => SelectedDefinition);
            NotifyOfPropertyChange(() => CanSaveDefinition);
            NotifyOfPropertyChange(() => CanDeleteDefinition);
        }
    }

    private ClassAlias? _selectedAlias;
    public ClassAlias? SelectedAlias
    {
        get => _selectedAlias;
        set
        {
            _selectedAlias = value;
            NotifyOfPropertyChange(() => SelectedAlias);
            NotifyOfPropertyChange(() => CanSaveAlias);
            NotifyOfPropertyChange(() => CanDeleteAlias);
        }
    }

    public bool CanSaveDefinition => SelectedDefinition != null;
    public bool CanSaveAlias => SelectedAlias != null;
    public bool CanCreateDefinitionFromUnmapped => !string.IsNullOrWhiteSpace(SelectedUnmapped);
    public bool CanDeleteDefinition => SelectedDefinition != null;
    public bool CanDeleteAlias => SelectedAlias != null;

    public string CurrentEventFolder => _shell.CurrentEventFolder ?? "No event loaded";
    public string HasEventLoaded => _shell.HasEventLoaded ? "Yes" : "No";
    public string CurrentDbPath => _shell.CurrentDbPath ?? "N/A";

    public ConfigViewModel(ShellViewModel shell, ScoreRepository scoreRepo, IClassConfigService classConfigService)
    {
        _shell = shell ?? throw new ArgumentNullException(nameof(shell));
        _scoreRepo = scoreRepo ?? throw new ArgumentNullException(nameof(scoreRepo));
        _classConfigService = classConfigService ?? throw new ArgumentNullException(nameof(classConfigService));

        DisplayName = "Class Configuration";
        
        // Initialize competition types
        InitializeCompetitionTypes();
    }

    private void InitializeCompetitionTypes()
    {
        // Populate from the actual enum
        AvailableCompetitionTypes.Add(new CompetitionTypeOption 
        { 
            DisplayName = "TSDance", 
            Value = Core.Models.CompetitionType.TSDance 
        });
        AvailableCompetitionTypes.Add(new CompetitionTypeOption 
        { 
            DisplayName = "USASF", 
            Value = Core.Models.CompetitionType.USASF 
        });
        AvailableCompetitionTypes.Add(new CompetitionTypeOption 
        { 
            DisplayName = "Oklahoma State", 
            Value = Core.Models.CompetitionType.OKState 
        });
        
        // Don't set default here - let LoadAsync() do it
    }

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken = default)
    {
        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        var dbPath = _shell.CurrentDbPath;
        if (string.IsNullOrWhiteSpace(dbPath)) return;

        ClassDefinitions.Clear();
        ClassAliases.Clear();
        UnmappedClasses.Clear();

        var defs = await _classConfigService.GetClassDefinitionsAsync(dbPath);
        foreach (var d in defs) ClassDefinitions.Add(d);

        var aliases = await _classConfigService.GetAliasesAsync(dbPath);
        foreach (var a in aliases) ClassAliases.Add(a);

        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
        
        // Load the saved competition type from shell/configuration
        var currentType = _shell.CurrentCompetitionType;
        SelectedCompetitionType = AvailableCompetitionTypes.FirstOrDefault(x => x.Value == currentType) 
                                  ?? AvailableCompetitionTypes.First();
    }

    public async Task AddDefinition()
    {
        var newKey = $"class_{Guid.NewGuid():N}";
        var def = new ClassDefinition
        {
            ClassKey = newKey,
            DisplayName = "New Class",
            Bucket = "Studio",
            SortOrder = 1000,
            IsActive = true,
            UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        ClassDefinitions.Add(def);
        SelectedDefinition = def;
        // Don't save yet - let user edit first, then auto-save will handle it
    }

    public async Task SaveDefinition()
    {
        if (SelectedDefinition == null) return;
        
        var dbPath = _shell.CurrentDbPath!;
        SelectedDefinition.UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        
        await _classConfigService.UpsertClassDefinitionAsync(SelectedDefinition, dbPath, SaveGlobally);
        
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
    }

    public async Task SaveAllDefinitions()
    {
        var dbPath = _shell.CurrentDbPath!;
        
        foreach (var def in ClassDefinitions)
        {
            def.UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            await _classConfigService.UpsertClassDefinitionAsync(def, dbPath, SaveGlobally);
        }
        
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
    }

    public async Task DeleteDefinition()
    {
        if (SelectedDefinition == null) return;
        
        var result = MessageBox.Show(
            $"Delete definition '{SelectedDefinition.DisplayName}' (key: {SelectedDefinition.ClassKey})?\n\n" +
            $"This will also delete all associated aliases.\n" +
            $"Routines using this class will become unmapped.",
            "Confirm Delete Definition",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result != MessageBoxResult.Yes) return;
        
        var dbPath = _shell.CurrentDbPath!;
        var classKeyToDelete = SelectedDefinition.ClassKey;
        
        // Delete from database (cascades to aliases)
        await _classConfigService.DeleteClassDefinitionAsync(classKeyToDelete, dbPath, SaveGlobally);
        
        // Remove from UI
        ClassDefinitions.Remove(SelectedDefinition);
        
        // Remove all aliases that referenced this definition
        var aliasesToRemove = ClassAliases.Where(a => a.ClassKey == classKeyToDelete).ToList();
        foreach (var alias in aliasesToRemove)
        {
            ClassAliases.Remove(alias);
        }
        
        // Refresh unmapped classes (previously mapped routines will now appear)
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
        
        SelectedDefinition = null;
    }

    public async Task AddAlias()
    {
        if (SelectedDefinition == null) return;
        var alias = new ClassAlias
        {
            Alias = $"alias_{Guid.NewGuid():N}",
            ClassKey = SelectedDefinition.ClassKey,
            UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        ClassAliases.Add(alias);
        SelectedAlias = alias;
        // Don't save yet - let user edit first, then auto-save will handle it
    }

    public async Task SaveAlias()
    {
        if (SelectedAlias == null) return;
        
        var dbPath = _shell.CurrentDbPath!;
        SelectedAlias.UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        await _classConfigService.UpsertAliasAsync(SelectedAlias.Alias, SelectedAlias.ClassKey, dbPath, SaveGlobally);
        
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
    }

    public async Task SaveAllAliases()
    {
        var dbPath = _shell.CurrentDbPath!;
        
        foreach (var alias in ClassAliases)
        {
            alias.UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            await _classConfigService.UpsertAliasAsync(alias.Alias, alias.ClassKey, dbPath, SaveGlobally);
        }
        
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
    }

    // New: Handle auto-save when row editing ends
    public async Task OnDefinitionRowEditEnding(ClassDefinition def)
    {
        if (def == null) return;
        
        var dbPath = _shell.CurrentDbPath;
        if (string.IsNullOrWhiteSpace(dbPath)) return;
        
        def.UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        await _classConfigService.UpsertClassDefinitionAsync(def, dbPath, SaveGlobally);
    }

    // New: Handle auto-save when alias row editing ends
    public async Task OnAliasRowEditEnding(ClassAlias alias)
    {
        if (alias == null) return;
        
        var dbPath = _shell.CurrentDbPath;
        if (string.IsNullOrWhiteSpace(dbPath)) return;
        
        alias.UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        await _classConfigService.UpsertAliasAsync(alias.Alias, alias.ClassKey, dbPath, SaveGlobally);
    }

    // New: Save all pending changes when navigating away
    protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        // Save all pending changes before leaving the view
        await SaveAllDefinitions();
        await SaveAllAliases();
        
        await base.OnDeactivateAsync(close, cancellationToken);
    }

    private static System.Windows.DependencyObject? FindName(System.Windows.DependencyObject parent, string name)
    {
        if (parent is System.Windows.FrameworkElement fe && fe.Name == name)
            return fe;
        
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            var result = FindName(child, name);
            if (result != null) return result;
        }
        return null;
    }

    public async Task MapSelectedUnmappedToSelectedDefinition()
    {
        if (SelectedUnmapped == null || SelectedDefinition == null) return;
        var dbPath = _shell.CurrentDbPath!;
        await _classConfigService.UpsertAliasAsync(SelectedUnmapped, SelectedDefinition.ClassKey, dbPath, SaveGlobally);
        
        // Don't reload everything - just refresh unmapped classes and aliases
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
        
        ClassAliases.Clear();
        var aliases = await _classConfigService.GetAliasesAsync(dbPath);
        foreach (var a in aliases) ClassAliases.Add(a);
    }

    // Add this new method after MapSelectedUnmappedToSelectedDefinition()
    public async Task CreateDefinitionFromUnmapped()
    {
        if (string.IsNullOrWhiteSpace(SelectedUnmapped)) return;
        
        var dbPath = _shell.CurrentDbPath!;
        
        // Create a normalized ClassKey from the unmapped text
        var classKey = SelectedUnmapped.Trim().ToLowerInvariant().Replace(" ", "_");
        
        // Check if this ClassKey already exists
        if (ClassDefinitions.Any(d => d.ClassKey.Equals(classKey, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(
                $"A definition with key '{classKey}' already exists.",
                "Duplicate Definition",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        
        // Create the definition using the unmapped text as the display name
        var newDef = new ClassDefinition
        {
            ClassKey = classKey,
            DisplayName = SelectedUnmapped.Trim(),
            Bucket = "Studio", // Default bucket - user can edit
            SortOrder = 1000,  // Default sort order - user can edit
            IsActive = true,
            UpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        
        // Save the definition
        await _classConfigService.UpsertClassDefinitionAsync(newDef, dbPath, SaveGlobally);
        
        // Add to collection and select it
        ClassDefinitions.Add(newDef);
        SelectedDefinition = newDef;
        
        // Automatically create an alias mapping the original unmapped text to the new ClassKey
        await _classConfigService.UpsertAliasAsync(SelectedUnmapped, classKey, dbPath, SaveGlobally);
        
        // Refresh unmapped classes (this one should disappear)
        UnmappedClasses.Clear();
        var unmapped = await _classConfigService.GetUnmappedClassesAsync(dbPath);
        foreach (var u in unmapped) UnmappedClasses.Add(u);
        
        // Refresh aliases to show the new one
        ClassAliases.Clear();
        var aliases = await _classConfigService.GetAliasesAsync(dbPath);
        foreach (var a in aliases) ClassAliases.Add(a);
        
        MessageBox.Show(
            $"Created definition '{newDef.DisplayName}' with key '{classKey}'.\n\n" +
            $"An alias has been created automatically.\n" +
            $"You can now edit the Bucket and SortOrder in the Class Definitions grid.",
            "Definition Created",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public async Task GenerateRandomScores()
    {
        var result = MessageBox.Show(
            "Generate random test scores for all routines?\n\nThis will overwrite any existing scores.",
            "Generate Random Scores",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            // Get the first available score sheet for the current competition type
            var selector = new DefaultScoreSheetSelector();
            var (availableSheets, _) = selector.GetSheets(_shell.CurrentCompetitionType, null);
            
            if (!availableSheets.Any())
            {
                MessageBox.Show("No score sheets available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sheet = availableSheets.First();

            // Get all routines
            var routines = await _scoreRepo.GetAllRoutinesAsync();
            if (!routines.Any())
            {
                MessageBox.Show("No routines found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var random = new Random();

            foreach (var routine in routines)
            {
                // Generate random scores for all judges and criteria
                var cells = new List<RoutineScoreCellRow>();
                for (int judgeIndex = 1; judgeIndex <= sheet.JudgeCount; judgeIndex++)
                {
                    foreach (var criterion in sheet.Criteria)
                    {
                        // Generate random score between 70% and 100% of max score
                        var minScore = criterion.MaxScore * 0.7m;
                        var randomValue = minScore + ((criterion.MaxScore - minScore) * (decimal)random.NextDouble());
                        
                        // Round to 1 decimal place
                        var value = Math.Round(randomValue, 1);

                        cells.Add(new RoutineScoreCellRow(
                            routine.RoutineId,
                            sheet.SheetKey,
                            judgeIndex,
                            criterion.Key,
                            (double)value
                        ));
                    }
                }

                // Save the scores
                await _scoreRepo.SaveCellsAsync(routine.RoutineId, sheet.SheetKey, cells);

                // Remove scores from any other sheets for this routine (overwrite behavior)
                await _scoreRepo.DeleteScoresExceptSheetAsync(routine.RoutineId, sheet.SheetKey);

                // Mark routine as scored and set the last sheet key so it is included in scored routines
                await _scoreRepo.SetLastSheetKeyAsync(routine.RoutineId, sheet.SheetKey);
            }

            MessageBox.Show(
                $"Generated random scores for {routines.Count} routines using '{sheet.DisplayName}' score sheet.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadAsync(); // Refresh the view
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error generating random scores: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public async Task ClearAllScores()
    {
        var result = MessageBox.Show(
            "Delete ALL scores for ALL routines?\n\nThis action cannot be undone!",
            "Clear All Scores",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        // Double confirmation
        var confirm = MessageBox.Show(
            "Are you ABSOLUTELY SURE?\n\nAll score data will be permanently deleted.",
            "Confirm Clear All Scores",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            await _scoreRepo.ClearAllScoresAsync();
            
            MessageBox.Show(
                "All scores have been deleted.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadAsync(); // Refresh the view
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error clearing scores: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public Task Close() => TryCloseAsync();
}

// Helper class for UI binding - wraps the enum for display purposes
public class CompetitionTypeOption
{
    public string DisplayName { get; set; } = string.Empty;
    public Core.Models.CompetitionType Value { get; set; }
}
