using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tsd.Tabulator.Core.Configuration;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Scoring;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Wpf.ViewModels.Scoring;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class DataViewModel : Screen
{
    private readonly IScoreRepository _scoreRepo;
    private readonly IScoreSheetSelector _selector;
    private readonly IScoreSheetSuggestionService _suggestionService;
    private readonly CompetitionType _competitionType;
    private bool _isUpdatingSelection; // Prevent re-entrancy during programmatic changes

    public ObservableCollection<RoutineRowVM> Routines { get; } = new();
    
    public RoutineRowVM? SelectedRoutine
    {
        get => _selectedRoutine;
        set
        {
            if (_isUpdatingSelection) return;
            _ = HandleRoutineSelectionChangeAsync(value);
        }
    }
    private RoutineRowVM? _selectedRoutine;

    private ScoreSheetTabVM? _selectedTab;
    public ScoreSheetTabVM? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (_isUpdatingSelection) return;
            _ = HandleTabSelectionChangeAsync(value);
        }
    }

    // Routine header properties (bind to UI)
    public string? EntryId { get; private set; }
    public string? EntryType { get; private set; }
    public string? Category { get; private set; }
    public string? Class { get; private set; }
    public string? StudioName { get; private set; }
    public string? RoutineTitle { get; private set; }

    public DataViewModel(IScoreRepository scoreRepo, CompetitionType competitionType)
        : this(scoreRepo, new DefaultScoreSheetSelector(), new NoOpScoreSheetSuggestionService(), competitionType)
    {
    }

    public DataViewModel(
        IScoreRepository scoreRepo,
        IScoreSheetSelector selector,
        IScoreSheetSuggestionService suggestionService,
        CompetitionType competitionType)
    {
        _scoreRepo = scoreRepo;
        _selector = selector;
        _suggestionService = suggestionService;
        _competitionType = competitionType;

        // Get available sheets based on competition type
        var (availableSheets, defaultSheetKey) = _selector.GetSheets(_competitionType, null);

        // Create tabs from filtered sheets
        Tabs = new ObservableCollection<ScoreSheetTabVM>(
            availableSheets.Select(def => new ScoreSheetTabVM(_scoreRepo, def))
        );
        
        // Select first tab by default
        if (Tabs.Any())
        {
            _isUpdatingSelection = true;
            _selectedTab = Tabs.First();
            NotifyOfPropertyChange(() => SelectedTab);
            _isUpdatingSelection = false;
        }

        tb_IsEnabled = true;
        DisplayName = "Record Scores";
    }

    public ObservableCollection<ScoreSheetTabVM> Tabs { get; }

    private bool _tb_IsEnabled;
    public bool tb_IsEnabled
    {
        get => _tb_IsEnabled;
        set { _tb_IsEnabled = value; NotifyOfPropertyChange(); }
    }

    public async Task LoadRoutinesAsync()
    {
        var routines = await _scoreRepo.GetAllRoutinesAsync();
        Routines.Clear();
        foreach (var r in routines)
            Routines.Add(new RoutineRowVM(r));
    }

    private async Task HandleRoutineSelectionChangeAsync(RoutineRowVM? newRoutine)
    {
        // Check if current sheet is dirty - warn user about unsaved changes
        if (SelectedTab?.IsDirty == true)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Switching routines will discard them.\n\nContinue?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                // Cancel the selection change
                _isUpdatingSelection = true;
                NotifyOfPropertyChange(() => SelectedRoutine); // Refresh binding to revert
                _isUpdatingSelection = false;
                return;
            }
        }

        // Proceed with routine change
        _isUpdatingSelection = true;
        _selectedRoutine = newRoutine;
        NotifyOfPropertyChange(() => SelectedRoutine);
        _isUpdatingSelection = false;

        UpdateRoutineHeader();

        if (newRoutine == null)
        {
            // No routine selected - clear current sheet
            if (SelectedTab != null)
            {
                SelectedTab.ClearAll();
            }
            return;
        }

        // Determine which sheet to use for this routine
        await SelectSheetForRoutineAsync(newRoutine);
    }

    private async Task SelectSheetForRoutineAsync(RoutineRowVM routine)
    {
        // Get last-used sheet for this routine
        var lastSheetKey = await _scoreRepo.GetLastSheetKeyAsync(routine.RoutineId);

        string? targetSheetKey = null;

        // Priority 1: LastSheetKey from persistence
        if (lastSheetKey != null && Tabs.Any(t => t.SheetKey == lastSheetKey))
        {
            targetSheetKey = lastSheetKey;
        }
        // Priority 2: Auto-suggestion (if enabled and last sheet is null)
        else if (AppSettings.EnableAutoSuggestSheet && lastSheetKey == null)
        {
            var routineData = CreateRoutineRow(routine);
            var suggested = _suggestionService.SuggestSheetKey(routineData);
            if (suggested != null && Tabs.Any(t => t.SheetKey == suggested))
            {
                targetSheetKey = suggested;
            }
        }

        // If we have a target sheet different from current, switch to it
        if (targetSheetKey != null)
        {
            var targetTab = Tabs.FirstOrDefault(t => t.SheetKey == targetSheetKey);
            if (targetTab != null && targetTab != SelectedTab)
            {
                _isUpdatingSelection = true;
                _selectedTab = targetTab;
                NotifyOfPropertyChange(() => SelectedTab);
                _isUpdatingSelection = false;
            }
        }

        // Load scores for the selected sheet
        if (SelectedTab != null)
        {
            await SelectedTab.LoadRoutineAsync(routine.RoutineId);
        }
    }

    private async Task HandleTabSelectionChangeAsync(ScoreSheetTabVM? newTab)
    {
        if (newTab == _selectedTab)
            return;

        // Check if current sheet is dirty - warn user about unsaved changes
        if (SelectedTab?.IsDirty == true)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Switching sheets will discard them.\n\nContinue?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                // Cancel the tab change
                _isUpdatingSelection = true;
                NotifyOfPropertyChange(() => SelectedTab); // Refresh binding to revert
                _isUpdatingSelection = false;
                return;
            }
        }

        // Proceed with tab change
        _isUpdatingSelection = true;
        _selectedTab = newTab;
        NotifyOfPropertyChange(() => SelectedTab);
        _isUpdatingSelection = false;

        // Load scores for the new sheet
        if (newTab != null && SelectedRoutine != null)
        {
            await newTab.LoadRoutineAsync(SelectedRoutine.RoutineId);
        }
    }

    private void UpdateRoutineHeader()
    {
        EntryId = SelectedRoutine?.ProgramNumber;
        EntryType = SelectedRoutine?.EntryType;
        Category = SelectedRoutine?.Category;
        Class = SelectedRoutine?.Class;
        StudioName = SelectedRoutine?.StudioName;
        RoutineTitle = SelectedRoutine?.RoutineTitle;
        NotifyOfPropertyChange(nameof(EntryId));
        NotifyOfPropertyChange(nameof(EntryType));
        NotifyOfPropertyChange(nameof(Category));
        NotifyOfPropertyChange(nameof(Class));
        NotifyOfPropertyChange(nameof(StudioName));
        NotifyOfPropertyChange(nameof(RoutineTitle));
    }

    private RoutineRow CreateRoutineRow(RoutineRowVM vm)
    {
        return new RoutineRow(
            vm.RoutineId,
            string.Empty,
            long.TryParse(vm.ProgramNumber, out var pn) ? pn : 0,
            vm.EntryType ?? string.Empty,
            vm.Category ?? string.Empty,
            vm.Class ?? string.Empty,
            string.Empty,
            vm.StudioName ?? string.Empty,
            vm.RoutineTitle ?? string.Empty
        );
    }

    public async void SubmitScores()
    {
        if (SelectedRoutine == null || SelectedTab == null)
            return;

        if (SelectedTab.HasValidationErrors())
        {
            MessageBox.Show(
                "Cannot save: validation errors exist.\n\nPlease fix the highlighted cells first.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var saved = await SelectedTab.SaveAsync();
        if (!saved)
            return;
        
        // Mark routine as scored in UI
        SelectedRoutine.IsScored = true;
        
        // Move to next routine
        var currentIndex = Routines.IndexOf(SelectedRoutine);
        if (currentIndex >= 0 && currentIndex < Routines.Count - 1)
        {
            SelectedRoutine = Routines[currentIndex + 1];
        }
    }

    public void Submit()
    {
        SubmitScores();
    }

    public void Cancel()
    {
        SelectedTab?.ClearAll();
    }
}
