using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Reports;

namespace Tsd.Tabulator.Wpf.ViewModels;

/// <summary>
/// Host ViewModel for tabbed reports. Auto-loads report data when activated.
/// </summary>
public sealed class ReportsViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly ShellViewModel _shell;
    private readonly IReportCatalog _reportCatalog;
    private readonly ReportConfiguration _reportConfig;
    private bool _isRefreshing;

    public ReportsViewModel(ShellViewModel shell, IReportCatalog reportCatalog, ReportConfiguration reportConfig)
    {
        _shell = shell;
        _reportCatalog = reportCatalog;
        _reportConfig = reportConfig;
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
        // Get ordered list of report IDs for current competition type
        var reportIds = _reportConfig.GetReportsFor(_shell.CurrentCompetitionType);
        
        System.Diagnostics.Debug.WriteLine($"Building tabs for {_shell.CurrentCompetitionType}, found {reportIds.Count} report IDs: {string.Join(", ", reportIds)}");
        
        // Resolve report definitions in the specified order
        var reportDefs = reportIds
            .Select(id => 
            {
                try 
                { 
                    var report = _reportCatalog.GetReport(id);
                    System.Diagnostics.Debug.WriteLine($"  Found report: {id} -> {report.DisplayName}");
                    return report; 
                }
                catch (Exception ex)
                { 
                    System.Diagnostics.Debug.WriteLine($"  Failed to load report '{id}': {ex.Message}");
                    return null; // Skip reports that aren't registered
                }
            })
            .Where(r => r != null)
            .Select(r => r!) // Use null-forgiving operator to assert non-null
            .ToList();

        System.Diagnostics.Debug.WriteLine($"Successfully loaded {reportDefs.Count} report definitions");

        // If tabs already match, don't rebuild
        if (Items.Count == reportDefs.Count && 
            Items.Zip(reportDefs, (item, def) => item.DisplayName == def.DisplayName).All(x => x))
        {
            System.Diagnostics.Debug.WriteLine("Tabs already match, skipping rebuild");
            return; // Tabs already built
        }

        // Clear and rebuild
        Items.Clear();
        
        foreach (var reportDef in reportDefs)
        {
            try
            {
                var tab = reportDef.CreateViewModel() as IScreen;
                if (tab != null)
                {
                    Items.Add(tab);
                    System.Diagnostics.Debug.WriteLine($"  Added tab: {reportDef.DisplayName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  Warning: CreateViewModel for '{reportDef.DisplayName}' did not return IScreen");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  Failed to create tab for '{reportDef.DisplayName}': {ex.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine($"Final tab count: {Items.Count}");

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
            var activeTab = ActiveItem as IReportTab;
            if (activeTab != null)
            {
                await activeTab.RefreshAsync();
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Report refresh failed: {ex.Message}");
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
        // Deactivate all tabs to clean up resources
        foreach (var item in Items.ToList())
        {
            await DeactivateItemAsync(item, close, cancellationToken);
        }

        await base.OnDeactivateAsync(close, cancellationToken);
    }
}
