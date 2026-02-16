using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Application.Interfaces.Reporting;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reports;

public interface IReportSchemeUi<T> : IReportScheme<T>, IReportSchemeUiBase
{
    IReportTab CreateTab(IReportDataLoader<T> loader, IEventContext context);

    Type IReportSchemeUiBase.DataType => typeof(T);

    IReportTab IReportSchemeUiBase.CreateTabDynamic(object loader, IEventContext context)
    {
        return CreateTab((IReportDataLoader<T>)loader, context);
    }
}