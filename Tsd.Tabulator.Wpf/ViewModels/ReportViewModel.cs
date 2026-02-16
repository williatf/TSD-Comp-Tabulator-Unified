using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Reports;

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
        if (!HasEventLoaded)
        {
            Items.Clear();
            return;
        }

        // TODO: build tabs (solos, duets, trios, ensembles, officers, teams, special, etc.)
        // For now we’ll leave this empty and wire the host first.
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