using Caliburn.Micro;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Reports.d_Ensemble;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class EnsembleAwardsScheme : IReportScheme
{
    private readonly IReportLoader<EnsembleAwardEntry> _loader;
    private readonly IClassConfigService _classConfigService;
    private readonly ShellViewModel _shell;
    private readonly IEnsembleAwardReportService _ensembleAwardReportService;

    public string DisplayName => "Ensembles";
    public ReportSchema Schema { get; }

    public ReportHeader Header { get; init; } = new(
        Title: "Ensemble Awards",
        Subtitle: "This is a subtitle.  It's optional.",
        Notes: "Awards for classes with 5+ entries. Sorted by score, then by place."
    );

    public EnsembleAwardsScheme(
        IReportLoader<EnsembleAwardEntry> loader,
        IClassConfigService classConfigService,
        ShellViewModel shell,
        IEnsembleAwardReportService ensembleAwardReportService)
    {
        _loader = loader;
        _classConfigService = classConfigService;
        _shell = shell;
        _ensembleAwardReportService = ensembleAwardReportService;

        Schema = new ReportSchema
        {
            Columns =
            [
                new("PlaceName", nameof(EnsembleAwardEntry.PlaceName), 100),
                new("Place", nameof(EnsembleAwardEntry.Place), 40),
                new("Score", nameof(EnsembleAwardEntry.FinalScore), 60),
                new("Entry ID", nameof(EnsembleAwardEntry.ProgramNumber), 60),
                new("Studio", nameof(EnsembleAwardEntry.StudioName), 200),
                new("Routine Title", nameof(EnsembleAwardEntry.RoutineTitle), 300)
            ],
            HierarchyDepth = 3
        };
        _ensembleAwardReportService = ensembleAwardReportService;
    }

    public IScreen CreateTab()
    {
        return new ReportTabViewModel<EnsembleAwardEntry>(
            this,
            _loader,
            _classConfigService,
            _shell.CurrentDbPath!,
            Schema,
            DisplayName,
            _ensembleAwardReportService);
    }
}