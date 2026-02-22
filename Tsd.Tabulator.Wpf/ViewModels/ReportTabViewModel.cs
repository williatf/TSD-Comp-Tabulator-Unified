using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Reports.d_Ensemble;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Wpf.Reporting;

namespace Tsd.Tabulator.Wpf.ViewModels;

/// <summary>
/// ViewModel for a single report tab.  
/// 
/// Responsibilities:
///  • Load report data via an injected IReportLoader<T>.  
///  • For Solo/Duet/Trio: build a 2‑level hierarchy (Bucket → Class → Items).  
///  • For Ensemble: delegate hierarchy construction to the Ensemble report service,
///    which produces a 3‑level hierarchy (Bucket → Class → Type → Items).  
///  • Expose the resulting bucket groups to the View for templated rendering.
/// </summary>
public sealed class ReportTabViewModel<T> : Screen, IReportTab
    where T : AwardEntryBase
{
    private readonly IReportLoader<T> _loader;
    private readonly IClassConfigService _classConfigService;
    private readonly string _eventDbPath;
    private readonly IEnsembleAwardReportService? _ensembleService;
    private readonly IReportScheme _scheme;

    /// <summary>
    /// The report schema describing hierarchy depth and metadata.
    /// </summary>
    public ReportSchema Schema { get; }

    public ReportHeader Header => _scheme.Header;

    public string? Title => Header.Title;
    public string? Subtitle => Header.Subtitle;
    public string? Notes => Header.Notes;

    /// <summary>
    /// The bucket groups displayed in the UI.  
    /// For Solo/Duet/Trio: BucketGroup.  
    /// For Ensemble: EnsembleBucketGroup.
    /// </summary>
    public ObservableCollection<object> Buckets { get; } = new();

    public override string DisplayName { get; set; }

    public ReportTabViewModel(
        IReportScheme scheme,
        IReportLoader<T> loader,
        IClassConfigService classConfigService,
        string eventDbPath,
        ReportSchema schema,
        string displayName,
        IEnsembleAwardReportService? ensembleService = null)
    {
        _scheme = scheme;
        _loader = loader;
        _classConfigService = classConfigService;
        _eventDbPath = eventDbPath;
        Schema = schema;
        DisplayName = displayName;
        _ensembleService = ensembleService;
    }

    /// <summary>
    /// Called when the tab is activated.  
    /// Loads and builds the report hierarchy.
    /// </summary>
    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        await RefreshAsync();
    }

    /// <summary>
    /// Refreshes the report data and rebuilds the bucket hierarchy.  
    /// Delegates Ensemble hierarchy construction to the Ensemble report service.
    /// </summary>
    public async Task RefreshAsync()
    {
        Buckets.Clear();

        // If this is an Ensemble report, the service already builds the full hierarchy.
        if (Schema.HierarchyDepth == 3)
        {
            if (_ensembleService is null)
            {
                Debug.WriteLine("Ensemble report requested but no Ensemble service was provided.");
                return;
            }

            var report = await _ensembleService.GenerateReportAsync();

            Buckets.Clear();
            foreach (var bucket in report.Buckets)
                Buckets.Add(bucket);

            NotifyOfPropertyChange(() => Buckets);
            return;
        }

        // Otherwise: build the standard 2‑level hierarchy (Solo/Duet/Trio)
        var entries = await _loader.LoadAsync();

        var classDefs = (await _classConfigService
            .GetClassDefinitionsAsync(_eventDbPath))
            .Where(cd => cd.IsActive)
            .ToList();

        var standardBuckets = classDefs
            .GroupBy(cd => cd.Bucket)
            .OrderBy(g => ClassOrder.GetBucketOrder(g.Key))
            .Select(bucketGroup =>
            {
                var classes = bucketGroup
                    .OrderBy(cd => cd.SortOrder)
                    .Select(cd =>
                    {
                        var items = entries
                            .Where(e => e.ClassKey == cd.ClassKey)
                            .Cast<object>()
                            .ToList();

                        return new ClassGroup(
                            classKey: cd.ClassKey,
                            displayName: cd.DisplayName,
                            sortOrder: cd.SortOrder,
                            items: items
                        );
                    })
                    .Where(cg => cg.Items.Any())
                    .ToList();

                return new BucketGroup(bucketGroup.Key, classes);
            })
            .Where(bg => bg.Classes.Any())
            .ToList();

        foreach (var b in standardBuckets)
            Buckets.Add(b);

        NotifyOfPropertyChange(() => Buckets);
    }
}