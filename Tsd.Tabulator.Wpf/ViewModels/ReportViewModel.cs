using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Wpf.Reporting;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class ReportViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly ShellViewModel _shell;

    public bool HasEventLoaded => _shell.HasEventLoaded;

    public ReportViewModel(ShellViewModel shell)
    {
        _shell = shell;
        DisplayName = "Reports";
    }

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        Items.Clear();

        var schemes = IoC.GetAll<IReportScheme>().ToList();

        foreach (var scheme in schemes)
            Items.Add(scheme.CreateTab());

        await ActivateItemAsync(Items.First(), cancellationToken);
    }

    public async Task RefreshAsync()
    {
        if (ActiveItem is IReportTab tab)
            await tab.RefreshAsync();
    }

    public new async void Refresh()
    {
        await RefreshAsync();
    }
}