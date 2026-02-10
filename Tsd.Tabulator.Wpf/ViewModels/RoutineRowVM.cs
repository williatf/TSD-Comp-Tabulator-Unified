using Caliburn.Micro;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class RoutineRowVM : PropertyChangedBase
{
    public string RoutineId { get; set; }
    public string? ProgramNumber { get; set; }
    public string? EntryType { get; set; }
    public string? Category { get; set; }
    public string? Class { get; set; }
    public string? StudioName { get; set; }
    public string? RoutineTitle { get; set; }
    
    private bool _isScored;
    public bool IsScored
    {
        get => _isScored;
        set
        {
            if (_isScored == value) return;
            _isScored = value;
            NotifyOfPropertyChange(() => IsScored);
        }
    }

    public RoutineRowVM(RoutineRow routineRow)
    {
        RoutineId = routineRow.RoutineId;
        ProgramNumber = routineRow.ProgramNumber.ToString();
        EntryType = routineRow.EntryType;
        Category = routineRow.Category;
        Class = routineRow.Class;
        StudioName = routineRow.StudioName;
        RoutineTitle = routineRow.RoutineTitle;
    }
}
