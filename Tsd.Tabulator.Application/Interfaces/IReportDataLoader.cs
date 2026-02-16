using System.Collections.Generic;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Models.Reporting;

namespace Tsd.Tabulator.Application.Interfaces;

public interface IReportDataLoader<T>
{
    string ReportId { get; }
    Task<IReadOnlyList<BucketGroup<T>>> LoadAsync(IEventContext context);
}