using Caliburn.Micro;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class TrioAwardsScheme : IReportScheme
{
    private readonly IReportLoader<TrioAwardEntry> _loader;
    private readonly IClassConfigService _classConfigService;
    private readonly ShellViewModel _shell;

    public string DisplayName => "Trio Awards";
    public ReportSchema Schema { get; }

    public ReportHeader Header { get; init; } = new(
        Title: "Trio Awards",
        Subtitle: null,
        Notes: "Awards for trio classes with 5+ entries. Sorted by score, then by place."
    );

    public TrioAwardsScheme(
        IReportLoader<TrioAwardEntry> loader,
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
                new("PlaceName", nameof(TrioAwardEntry.PlaceName), 100),
                new("Place", nameof(TrioAwardEntry.Place), 40),
                new("Score", nameof(TrioAwardEntry.FinalScore), 60),
                new("Entry ID", nameof(TrioAwardEntry.ProgramNumber), 60),
                new("Participants", nameof(TrioAwardEntry.Participants), 200),
                new("Studio", nameof(TrioAwardEntry.StudioName), 200),
                new("Routine Title", nameof(TrioAwardEntry.RoutineTitle), 300)
            ]
        };
    }

    public IScreen CreateTab()
    {
        return new ReportTabViewModel<TrioAwardEntry>(
            this,
            _loader,
            _classConfigService,
            _shell.CurrentDbPath!,
            Schema,
            DisplayName);
    }
}