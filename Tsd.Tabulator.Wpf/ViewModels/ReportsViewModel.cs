using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Wpf.Reports;

namespace Tsd.Tabulator.Wpf.ViewModels;

/// <summary>
/// Host ViewModel for tabbed reports. Auto-loads report data when activated.
/// </summary>
public sealed class ReportsViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly ShellViewModel _shell;
    private readonly IReportCatalog _reportCatalog;
    private bool _isRefreshing;

    public ReportsViewModel(ShellViewModel shell, IReportCatalog reportCatalog)
    {
        _shell = shell;
        _reportCatalog = reportCatalog;
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
            // Clear any existing tabs
            Items.Clear();
            return;
        }

        // Build tabs from catalog based on current competition type
        await BuildTabsAsync();
        
        // Auto-load data for the active tab
        await RefreshAsync();
    }

    private async Task BuildTabsAsync()
    {
        // Get available reports for current competition type
        var reportDefs = _reportCatalog.GetAllReports()
            .Where(r => r is SoloAwardsReportDefinition soloReport && soloReport.IsAvailableFor(_shell.CurrentCompetitionType))
            .ToList();

        // If tabs already match, don't rebuild
        if (Items.Count == reportDefs.Count && 
            Items.Zip(reportDefs, (item, def) => item.DisplayName == def.DisplayName).All(x => x))
        {
            return; // Tabs already built
        }

        // Clear and rebuild
        Items.Clear();
        
        foreach (var reportDef in reportDefs)
        {
            var tab = reportDef.CreateViewModel() as IScreen;
            if (tab != null)
            {
                Items.Add(tab);
            }
        }

        // Activate first tab
        if (Items.Any())
        {
            await ActivateItemAsync(Items.First(), CancellationToken.None);
        }
    }

    /// <summary>
    /// Refreshes the active report tab.
    /// Called automatically on activation or manually via Refresh button.
    /// </summary>
    public async Task RefreshAsync()
    {
        if (!HasEventLoaded)
            return;

        IsRefreshing = true;

        try
        {
            // Refresh only the active tab
            if (ActiveItem is IReportTab activeTab)
            {
                await activeTab.RefreshAsync();
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Manual refresh triggered by button click.
    /// </summary>
    public new async void Refresh()
    {
        await RefreshAsync();
    }

    protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        // Optionally clear tabs when leaving Reports view to save memory
        if (close)
        {
            Items.Clear();
        }
        
        await base.OnDeactivateAsync(close, cancellationToken);
    }
}
