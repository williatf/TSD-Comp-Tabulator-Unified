using System.ComponentModel;

namespace Tsd.Tabulator.Wpf.ViewModels.Scoring;

public sealed class JudgeRowVM : INotifyPropertyChanged
{
    private readonly Action _onAnyCellChanged;
    private readonly List<ScoreColumnVM> _columns;
    private bool _suppressParentCallback;

    public JudgeRowVM(int judgeIndex, string label, IReadOnlyList<ScoreColumnVM> columns, Action onAnyCellChanged)
    {
        JudgeIndex = judgeIndex;
        Label = label;
        _onAnyCellChanged = onAnyCellChanged;
        _columns = columns.ToList();

        // prevent parent Recalc from executing while we're still constructing this child
        _suppressParentCallback = true;

        Cells = _columns
            .Where(c => !c.IsTotal)
            .Select(c => new ScoreCellVM(c.Key, c.Max ?? 0m, Recalc))
            .ToList();

        CellsByKey = Cells.ToDictionary(c => c.Key);

        RebuildRowCells();
        Recalc();

        // allow parent notifications after construction completes
        _suppressParentCallback = false;
    }

    public int JudgeIndex { get; }

    public string Label { get; }

    // Only score cells (no total)
    public List<ScoreCellVM> Cells { get; }

    // Dictionary lookup by criterion key
    public Dictionary<string, ScoreCellVM> CellsByKey { get; }

    // One entry per displayed column (score cells + total placeholder)
    public List<RowCellVM> RowCells { get; private set; } = new();

    private decimal _total;
    public decimal Total
    {
        get => _total;
        private set
        {
            if (_total == value) return;
            _total = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));

            // Keep total cell in sync
            var totalCell = RowCells.FirstOrDefault(c => c.IsTotal);
            if (totalCell is not null)
                totalCell.TotalValue = _total;
        }
    }

    private void RebuildRowCells()
    {
        var list = new List<RowCellVM>();

        foreach (var col in _columns)
        {
            if (col.IsTotal)
            {
                list.Add(RowCellVM.ForTotal());
            }
            else
            {
                var cell = Cells.First(c => c.Key == col.Key);
                list.Add(RowCellVM.ForScore(cell));
            }
        }

        RowCells = list;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RowCells)));
    }

    private void Recalc()
    {
        Total = Cells.Sum(c => c.Value ?? 0m);
        if (!_suppressParentCallback)
            _onAnyCellChanged?.Invoke();
    }

    public void Clear()
    {
        foreach (var c in Cells) c.Text = "";
        Recalc();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class RowCellVM : INotifyPropertyChanged
{
    private RowCellVM() { }

    public bool IsTotal { get; private set; }

    // For score cells
    public ScoreCellVM? ScoreCell { get; private set; }

    // For total cells
    private decimal _totalValue;
    public decimal TotalValue
    {
        get => _totalValue;
        set
        {
            if (_totalValue == value) return;
            _totalValue = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValue)));
        }
    }

    public static RowCellVM ForScore(ScoreCellVM cell) => new RowCellVM { IsTotal = false, ScoreCell = cell };
    public static RowCellVM ForTotal() => new RowCellVM { IsTotal = true };

    public event PropertyChangedEventHandler? PropertyChanged;
}
