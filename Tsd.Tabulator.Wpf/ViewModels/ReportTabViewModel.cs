using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Services;

namespace Tsd.Tabulator.Wpf.ViewModels;

public sealed class ReportTabViewModel<T> : Screen, IReportTab
    where T : AwardEntryBase
{
    private readonly IReportLoader<T> _loader;
    private readonly IClassConfigService _classConfigService;
    private readonly string _eventDbPath;
    public ReportSchema Schema { get; }
    public ObservableCollection<object> Buckets { get; } = new();
    public override string DisplayName { get; set; }

    public ReportTabViewModel(
        IReportLoader<T> loader,
        IClassConfigService classConfigService,
        string eventDbPath,
        ReportSchema schema,
        string displayName)
    {
        _loader = loader;
        _classConfigService = classConfigService;
        _eventDbPath = eventDbPath;
        Schema = schema;
        DisplayName = displayName;
    }

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {

        var entries = await _loader.LoadAsync();

        // Load class definitions for the current event
        var classDefs = (await _classConfigService
            .GetClassDefinitionsAsync(_eventDbPath))
            .Where(cd => cd.IsActive)
            .ToList();

        Buckets.Clear();

        // Group by bucket, using your real bucket ordering logic
        var buckets = classDefs
            .GroupBy(cd => cd.Bucket)
            .OrderBy(g => ClassOrder.GetBucketOrder(g.Key))
            .Select(bucketGroup =>
            {
                // Build class groups inside each bucket
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
                    .Where(cg => cg.Items.Any())   // remove empty classes
                    .ToList();

                return new BucketGroup(bucketGroup.Key, classes);
            })
            .Where(bg => bg.Classes.Any())       // remove empty buckets
            .ToList();

        foreach (var b in buckets)
            Buckets.Add(b);

        NotifyOfPropertyChange(() => Buckets);
    }
}