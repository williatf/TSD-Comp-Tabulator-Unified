using Tsd.Tabulator.Core.Reporting;
using Caliburn.Micro;

namespace Tsd.Tabulator.Wpf.Reporting;

public interface IReportScheme
{
    string DisplayName { get; }
    IScreen CreateTab();
}