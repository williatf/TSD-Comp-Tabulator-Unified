using Caliburn.Micro;
using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Wpf.ViewModels.Reports;

namespace Tsd.Tabulator.Wpf.Reports;

/// <summary>
/// Report definition for Duet Awards.
/// </summary>
public sealed class DuetsAwardsReportDefinition : IReportDefinition
{
    public string Id => "DuetAwards";
    public string DisplayName => "Duet Awards";

    public object CreateViewModel()
    {
        // Let IoC handle dependency injection
        return IoC.Get<DuetsAwardsReportTabViewModel>();
    }
}