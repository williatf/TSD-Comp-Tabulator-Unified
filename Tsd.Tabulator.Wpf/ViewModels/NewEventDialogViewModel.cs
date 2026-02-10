using Caliburn.Micro;
using Microsoft.Win32;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class NewEventDialogViewModel : Screen
{
    public string Title => "Create New Event";

    private string? _eventFolderName;
    public string? EventFolderName
    {
        get => _eventFolderName;
        set
        {
            _eventFolderName = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanCreate));
        }
    }

    private string? _csvPath;
    public string? CsvPath
    {
        get => _csvPath;
        set
        {
            _csvPath = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanCreate));
        }
    }

    public bool CanCreate =>
        !string.IsNullOrWhiteSpace(EventFolderName) &&
        !string.IsNullOrWhiteSpace(CsvPath);

    public void BrowseCsv()
    {
        var ofd = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Select registration CSV"
        };
        if (ofd.ShowDialog() == true)
            CsvPath = ofd.FileName;
    }

    public void Create() => TryCloseAsync(true);
    public void Cancel() => TryCloseAsync(false);
}
