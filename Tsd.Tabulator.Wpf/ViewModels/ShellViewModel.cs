using Caliburn.Micro;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite.Import;
using Tsd.Tabulator.Data.Sqlite.Scoring;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
{
    internal readonly IFingerprintService _fingerprints;
    private readonly IWindowManager _windowManager;
    private readonly IClassConfigService _classConfigService;
    private readonly IEventContext _eventContext;
    
    public string? CurrentEventFolder { get; private set; }
    public string? CurrentDbPath { get; private set; }
    
    // Competition type for current event - default to OKState
    private CompetitionType _currentCompetitionType = CompetitionType.OKState;
    public CompetitionType CurrentCompetitionType
    {
        get => _currentCompetitionType;
        set
        {
            if (_currentCompetitionType == value) return;
            _currentCompetitionType = value;
            NotifyOfPropertyChange(() => CurrentCompetitionType);
            _eventContext.UpdateCompetitionType(value); // Use the method instead
        }
    }

    public bool HasEventLoaded => !string.IsNullOrWhiteSpace(CurrentDbPath);

    public ShellViewModel(IWindowManager windowManager, IFingerprintService fingerprints, IClassConfigService classConfigService, IEventContext eventContext)
    {
        _fingerprints = fingerprints;
        _windowManager = windowManager;
        _classConfigService = classConfigService;
        _eventContext = eventContext;
    }

    protected override async Task OnInitializedAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializedAsync(cancellationToken);
        
        // Try to auto-load last event on startup
        await TryLoadLastEventAsync();
    }

    private async Task TryLoadLastEventAsync()
    {
        var lastPath = Properties.Settings.Default.LastEventPath;
        
        if (string.IsNullOrWhiteSpace(lastPath) || !File.Exists(lastPath))
            return;

        try
        {
            await LoadEventAsync(lastPath);
        }
        catch
        {
            // If loading fails, just start with no event loaded
            Properties.Settings.Default.LastEventPath = null;
            Properties.Settings.Default.Save();
        }
    }

    private async Task LoadEventAsync(string dbPath)
    {
        var factory = new SqliteConnectionFactory(dbPath);
        new SchemaInitializer(factory).EnsureSchema();

        // Seed event DB from global master-config
        try
        {
            await _classConfigService.SeedEventFromGlobalAsync(dbPath);
        }
        catch
        {
            // Non-fatal: if seeding fails, allow event to load but log/ignore
        }

        CurrentDbPath = dbPath;
        _eventContext.UpdateEventDbPath(dbPath); // Use the method instead
        CurrentEventFolder = Path.GetDirectoryName(dbPath);

        // Save this as the last opened event
        Properties.Settings.Default.LastEventPath = dbPath;
        Properties.Settings.Default.Save();

        // Load the competition type from the database before creating views
        await LoadCompetitionTypeAsync(factory);

        // Notify that event properties changed
        NotifyOfPropertyChange(() => HasEventLoaded);
        NotifyOfPropertyChange(() => CurrentEventFolder);
        NotifyOfPropertyChange(() => CurrentDbPath);

        var scoreRepo = new ScoreRepository(factory);
        var dataVM = new DataViewModel(scoreRepo, CurrentCompetitionType);
        await dataVM.LoadRoutinesAsync();
        await ActivateItemAsync(dataVM, default);
    }

    private async Task LoadCompetitionTypeAsync(SqliteConnectionFactory factory)
    {
        // Load competition type from database settings
        using var conn = factory.OpenConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ConfigValue FROM AppSettings WHERE ConfigKey = 'CompetitionType' LIMIT 1";
        
        var result = await cmd.ExecuteScalarAsync();
        if (result != null && Enum.TryParse<CompetitionType>(result.ToString(), out var competitionType))
        {
            CurrentCompetitionType = competitionType;
        }
        else
        {
            // If no setting found, default to TSDance and save it
            CurrentCompetitionType = CompetitionType.TSDance;
            await SaveCompetitionTypeAsync(factory);
        }

    }

    private async Task SaveCompetitionTypeAsync(SqliteConnectionFactory factory)
    {
        using var conn = factory.OpenConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO AppSettings (ConfigKey, ConfigValue)
            VALUES ('CompetitionType', @value)";
        cmd.Parameters.AddWithValue("@value", CurrentCompetitionType.ToString());
        
        await cmd.ExecuteNonQueryAsync();
    }

    public void CloseApp() => System.Windows.Application.Current.Shutdown();

    public void ShowReportsView()
    {
        if (!HasEventLoaded)
        {
            MessageBox.Show("No event database is loaded yet.");
            return;
        }
        
        // Create ReportsViewModel - it will get ShellViewModel via IoC
        var reportsVM = IoC.Get<ReportsViewModel>();
        ActivateItemAsync(reportsVM, default);
    }

    public async void ShowDataView()
    {
        if (string.IsNullOrWhiteSpace(CurrentDbPath))
        {
            MessageBox.Show("No event database is loaded yet.");
            return;
        }
        var scoreRepo = new ScoreRepository(new SqliteConnectionFactory(CurrentDbPath));
        var dataVM = new DataViewModel(scoreRepo, CurrentCompetitionType);
        await dataVM.LoadRoutinesAsync();
        await ActivateItemAsync(dataVM, default);
    }

    public void ShowConfigView()
    {
        if (string.IsNullOrWhiteSpace(CurrentDbPath))
        {
            MessageBox.Show("No event database is loaded yet.");
            return;
        }
        var scoreRepo = new ScoreRepository(new SqliteConnectionFactory(CurrentDbPath));
        // pass class config service so the config UI can operate on event + master
        ActivateItemAsync(new ConfigViewModel(this, scoreRepo, _classConfigService), default);
    }

    public async void LoadNewContest()
    {
        Directory.CreateDirectory(EventsRoot);

        var dlg = IoC.Get<NewEventDialogViewModel>();
        dlg.EventFolderName = $"Event_{DateTime.Now:yyyyMMdd}";
        dlg.CsvPath = null;

        var ok = await _windowManager.ShowDialogAsync(dlg);
        if (ok != true) return;

        var folderName = SanitizeFolderName(dlg.EventFolderName!);
        var eventFolder = Path.Combine(EventsRoot, folderName);

        if (Directory.Exists(eventFolder))
        {
            MessageBox.Show($"Event folder already exists:\n{eventFolder}\n\nChoose a different name or use Open Event.");
            return;
        }

        Directory.CreateDirectory(eventFolder);

        var dbPath = Path.Combine(eventFolder, "event.sqlite");

        var factory = new SqliteConnectionFactory(dbPath);
        new SchemaInitializer(factory).EnsureSchema();

        var importer = new RoutineImportService(factory, _fingerprints);
        importer.ImportFromCsv(dlg.CsvPath!);

        // Seed event DB from global master-config
        try
        {
            await _classConfigService.SeedEventFromGlobalAsync(dbPath);
        }
        catch
        {
            // ignore seed errors for now
        }

        await LoadEventAsync(dbPath);
    }

    public void ExportDB()
    {
        if (string.IsNullOrWhiteSpace(CurrentDbPath) || !File.Exists(CurrentDbPath))
        {
            MessageBox.Show("No event database is loaded yet.");
            return;
        }

        var sfd = new SaveFileDialog
        {
            FileName = "event",
            DefaultExt = ".sqlite",
            Filter = "SQLite DB (*.sqlite)|*.sqlite|All files (*.*)|*.*"
        };

        if (sfd.ShowDialog() == true)
        {
            File.Copy(CurrentDbPath, sfd.FileName, overwrite: true);
        }
    }

    private static string EventsRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                     "TSD Tabulator", "Events");

    private static string SanitizeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Event" : cleaned;
    }

    public async void OpenContest()
    {
        Directory.CreateDirectory(EventsRoot);

        var ofd = new OpenFileDialog
        {
            Title = "Open event database",
            Filter = "SQLite DB (*.sqlite)|*.sqlite|All files (*.*)|*.*",
            InitialDirectory = EventsRoot
        };

        if (ofd.ShowDialog() != true)
            return;

        await LoadEventAsync(ofd.FileName);
    }
}
