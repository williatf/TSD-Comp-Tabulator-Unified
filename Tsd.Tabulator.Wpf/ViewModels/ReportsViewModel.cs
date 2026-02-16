using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Application.Interfaces.Reporting;
using Tsd.Tabulator.Application.Reports;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Wpf.Reports;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class ReportsViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly ShellViewModel _shell;
    private readonly IReportSchemeProvider _schemeProvider;
    private readonly IEventContext _eventContext;
    private readonly ReportDataLoaderRegistry _registry;

    private bool _isRefreshing;

    public ReportsViewModel(
        ShellViewModel shell,
        IReportSchemeProvider schemeProvider,
        IEventContext eventContext,
        ReportDataLoaderRegistry registry)
    {
        _shell = shell;
        _schemeProvider = schemeProvider;
        _eventContext = eventContext;
        _registry = registry;

        DisplayName = "Reports";
    }

    public bool HasEventLoaded => _shell.HasEventLoaded;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanRefresh));
        }
    }

    public bool CanRefresh => !IsRefreshing && HasEventLoaded;

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        if (!HasEventLoaded)
        {
            Items.Clear();
            return;
        }

        await BuildTabsAsync();
        await RefreshAsync();
    }

    // Store tabs as IReportTab
    //public new List<IReportTab> Items { get; } = new();

    private async Task BuildTabsAsync()
    {
        var compType = _eventContext.CompetitionType;
        var schemes = _schemeProvider.GetSchemesFor(compType).ToList();

        Items.Clear();

        foreach (IReportSchemeUiBase scheme in schemes)
        {
            // 1. Extract the generic type parameter T from the scheme
            var t = scheme.DataType;

            // 2. Resolve the loader for that T
            var loader = _registry.GetLoader(t);

            // 3. Create the tab dynamically
            var tab = scheme.CreateTabDynamic(loader, _eventContext);

            Items.Add((IScreen)tab);
        }
    }

    /// <summary>
    /// Refreshes the active report tab.
    /// </summary>
    public async Task RefreshAsync()
    {
        if (!HasEventLoaded)
            return;

        IsRefreshing = true;

        try
        {
            if (ActiveItem is IReportTab activeTab)
                await activeTab.RefreshAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Report refresh failed: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    public new async void Refresh()
    {
        await RefreshAsync();
    }

    protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        foreach (var item in Items.ToList())
            await DeactivateItemAsync(item, close, cancellationToken);

        await base.OnDeactivateAsync(close, cancellationToken);
    }
}