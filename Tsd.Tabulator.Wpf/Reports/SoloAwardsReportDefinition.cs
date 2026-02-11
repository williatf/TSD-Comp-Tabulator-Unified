using Caliburn.Micro;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Wpf.ViewModels.Reports;

namespace Tsd.Tabulator.Wpf.Reports;

/// <summary>
/// Report definition for Solo Awards.
/// </summary>
public sealed class SoloAwardsReportDefinition : IReportDefinition
{
    public string Id => "SoloAwards";
    public string DisplayName => "Solo Awards";

    public object CreateViewModel()
    {
        // Let IoC handle dependency injection
        return IoC.Get<SoloAwardsReportTabViewModel>();
    }
}