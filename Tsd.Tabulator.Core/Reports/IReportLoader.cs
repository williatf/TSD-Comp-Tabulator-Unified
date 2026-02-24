using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Reports;

public interface IReportLoader<T>
{
    Task<IReadOnlyList<T>> LoadAsync();
}