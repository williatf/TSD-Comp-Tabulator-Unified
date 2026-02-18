using Caliburn.Micro;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class SoloAwardsScheme : IReportScheme
{
    private readonly IReportLoader<SoloAwardEntry> _loader;
    private readonly IClassConfigService _classConfigService;
    private readonly ShellViewModel _shell;

    public string DisplayName => "Solo Awards";
    public ReportSchema Schema { get; }

    public SoloAwardsScheme(
        IReportLoader<SoloAwardEntry> loader,
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
                new("PlaceName", nameof(SoloAwardEntry.PlaceName), 100),
                new("Place", nameof(SoloAwardEntry.Place), 40),
                new("Score", nameof(SoloAwardEntry.FinalScore), 60),
                new("Entry ID", nameof(SoloAwardEntry.ProgramNumber), 60),
                new("Participants", nameof(SoloAwardEntry.Participants), 200),
                new("Studio", nameof(SoloAwardEntry.StudioName), 200),
                new("Routine Title", nameof(SoloAwardEntry.RoutineTitle), 300)
            ]
        };
    }

    public IScreen CreateTab()
    {
        return new ReportTabViewModel<SoloAwardEntry>(
            _loader,
            _classConfigService,
            _shell.CurrentDbPath!,
            Schema,
            DisplayName);
    }
}