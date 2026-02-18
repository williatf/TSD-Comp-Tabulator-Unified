using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Reporting;

public interface IReportLoader<T>
{
    Task<IReadOnlyList<T>> LoadAsync();
}