using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Application.Models.Reporting;
using Tsd.Tabulator.Core.Reports;

namespace Tsd.Tabulator.Wpf.ViewModels;

public interface IReportTab : IScreen
{
    new string DisplayName { get; }
    Task RefreshAsync();
}
public sealed class ReportTabViewModel<T> : Screen, IReportTab
{
    public IReadOnlyList<BucketGroup<T>> Buckets { get; private set; } = new List<BucketGroup<T>>();

    private readonly IReportDataLoader<T> _loader;
    private readonly IEventContext _context;

    public ReportTabViewModel(string displayName, IReportDataLoader<T> loader, IEventContext context)
    {
        DisplayName = displayName;
        _loader = loader;
        _context = context;
    }

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        if (Buckets == null || Buckets.Count == 0)
            await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        var result = await _loader.LoadAsync(_context);
        Buckets = result.ToList();
        NotifyOfPropertyChange(() => Buckets);
    }
}