using Caliburn.Micro;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class DuetAwardsScheme : IReportScheme
{
    private readonly IReportLoader<DuetAwardEntry> _loader;
    private readonly IClassConfigService _classConfigService;
    private readonly ShellViewModel _shell;

    public string DisplayName => "Duet Awards";
    public ReportSchema Schema { get; }

    public DuetAwardsScheme(
        IReportLoader<DuetAwardEntry> loader,
        IClassConfigService classConfigService,
        ShellViewModel shell)
    {
        _loader = loader;
        _classConfigService = classConfigService;
        _shell = shell;

        Schema = new ReportSchema
        {
            Columns =
            [
                new("PlaceName", nameof(DuetAwardEntry.PlaceName), 100),
                new("Place", nameof(DuetAwardEntry.Place), 40),
                new("Score", nameof(DuetAwardEntry.FinalScore), 60),
                new("Entry ID", nameof(DuetAwardEntry.ProgramNumber), 60),
                new("Participants", nameof(DuetAwardEntry.Participants), 200),
                new("Studio", nameof(DuetAwardEntry.StudioName), 200),
                new("Routine Title", nameof(DuetAwardEntry.RoutineTitle), 300)
            ]
        };
    }

    public IScreen CreateTab()
    {
        return new ReportTabViewModel<DuetAwardEntry>(
            _loader,
            _classConfigService,
            _shell.CurrentDbPath!,
            Schema,
            DisplayName);
    }
}