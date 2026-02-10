using Caliburn.Micro;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Scoring;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Wpf.ViewModels.Scoring;

public sealed class ScoreSheetTabVM : Screen
{
    private readonly IScoreRepository _scoreRepo;
    private readonly IScoreSheetDefinition _definition;
    private bool _isInitializing;

    public string SheetKey => _definition.SheetKey;
    public string Header => _definition.DisplayName;
    public string? CurrentRoutineId { get; private set; }
    
    // Includes Total as last column
    public List<ScoreColumnVM> Columns { get; }
    
    // Group headers (empty if no grouping)
    public List<GroupHeaderVM> GroupHeaders { get; }
    
    // True if any criterion has a group
    public bool ShowGroupHeaders { get; }
    
    public ObservableCollection<JudgeRowVM> Judges { get; } = new();

    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            NotifyOfPropertyChange(() => IsDirty);
        }
    }

    public ScoreSheetTabVM(IScoreRepository scoreRepo, IScoreSheetDefinition definition)
    {
        _scoreRepo = scoreRepo;
        _definition = definition;

        // Sort criteria by Order
        var orderedCriteria = _definition.Criteria.OrderBy(c => c.Order).ToList();

        // Build columns from definition criteria
        Columns = orderedCriteria
            .Select(c => new ScoreColumnVM(c.Key, c.DisplayName, c.MaxScore, isTotal: false))
            .ToList();

        // Add a Total column at the end for display convenience
        Columns.Add(new ScoreColumnVM("TOTAL", "Total", null, isTotal: true));

        // Compute group headers
        ShowGroupHeaders = orderedCriteria.Any(c => c.Group != null);
        GroupHeaders = ComputeGroupHeaders(orderedCriteria);

        // Initialize judges from definition
        for (int i = 1; i <= _definition.JudgeCount; i++)
        {
            Judges.Add(new JudgeRowVM(i, $"Judge {i}", Columns, OnCellChanged));
        }

        Recalc();
    }

    private List<GroupHeaderVM> ComputeGroupHeaders(IReadOnlyList<ScoreCriterionDefinition> orderedCriteria)
    {
        var headers = new List<GroupHeaderVM>();
        if (!ShowGroupHeaders)
            return headers;

        int currentIndex = 0;
        int i = 0;

        while (i < orderedCriteria.Count)
        {
            var criterion = orderedCriteria[i];
            
            if (criterion.Group == null)
            {
                // No group - skip this column
                i++;
                currentIndex++;
                continue;
            }

            // Find consecutive criteria with same group
            string groupName = criterion.Group;
            int startIndex = currentIndex;
            int span = 0;

            while (i < orderedCriteria.Count && orderedCriteria[i].Group == groupName)
            {
                span++;
                i++;
            }

            headers.Add(new GroupHeaderVM(groupName, startIndex, span));
            currentIndex += span;
        }

        return headers;
    }

    private void OnCellChanged()
    {
        Recalc();
        if (!_isInitializing)
            IsDirty = true;
    }

    public async Task LoadRoutineAsync(string routineId)
    {
        _isInitializing = true;
        CurrentRoutineId = routineId;

        // Clear all cells
        foreach (var judge in Judges)
            judge.Clear();

        var cells = await _scoreRepo.GetCellsAsync(routineId, SheetKey);

        // Apply values to matching cells
        foreach (var cell in cells)
        {
            var judge = Judges.FirstOrDefault(j => j.JudgeIndex == cell.JudgeIndex);
            if (judge != null && judge.CellsByKey.TryGetValue(cell.CriterionKey, out var scoreCell))
            {
                scoreCell.Text = cell.Value?.ToString() ?? "";
            }
        }

        _isInitializing = false;
        IsDirty = false;
        Recalc();
    }

    public bool HasValidationErrors()
    {
        return Judges.Any(j => j.Cells.Any(c => c.HasErrors));
    }

    public ScoreCellVM? GetFirstInvalidCell()
    {
        foreach (var judge in Judges)
        {
            var invalidCell = judge.Cells.FirstOrDefault(c => c.HasErrors);
            if (invalidCell != null)
                return invalidCell;
        }
        return null;
    }

    public async Task<bool> SaveAsync()
    {
        if (CurrentRoutineId == null)
            return false;

        if (HasValidationErrors())
            return false;

        var rows = Judges
            .SelectMany(j => j.CellsByKey.Values.Select(c =>
                new RoutineScoreCellRow(CurrentRoutineId, SheetKey, j.JudgeIndex, c.Key, (double?)c.Value)))
            .ToList();

        // Delete scores from all other sheets for this routine
        await _scoreRepo.DeleteScoresExceptSheetAsync(CurrentRoutineId, SheetKey);
        
        // Save scores for this sheet
        await _scoreRepo.SaveCellsAsync(CurrentRoutineId, SheetKey, rows);
        
        // Update LastSheetKey for this routine
        await _scoreRepo.SetLastSheetKeyAsync(CurrentRoutineId, SheetKey);
        
        IsDirty = false;
        return true;
    }

    public void ClearAll()
    {
        foreach (var j in Judges) j.Clear();
        IsDirty = false;
        Recalc();
    }

    private void Recalc()
    {
        if (_isInitializing) return;
        
        GrandTotal = Judges.Sum(j => j.Total);
        AvgScore = Judges.Count == 0 ? 0m : (GrandTotal / Judges.Count);
    }

    private decimal _grandTotal;
    public decimal GrandTotal
    {
        get => _grandTotal;
        private set
        {
            if (_grandTotal == value) return;
            _grandTotal = value;
            NotifyOfPropertyChange(() => GrandTotal);
        }
    }

    private decimal _avgScore;
    public decimal AvgScore
    {
        get => _avgScore;
        private set
        {
            if (_avgScore == value) return;
            _avgScore = value;
            NotifyOfPropertyChange(() => AvgScore);
        }
    }
}
