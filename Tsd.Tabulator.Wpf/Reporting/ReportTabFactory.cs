using Caliburn.Micro;

namespace Tsd.Tabulator.Wpf.Reporting;

public sealed class ReportTabFactory
{
    private readonly SimpleContainer _container;

    public ReportTabFactory(SimpleContainer container)
    {
        _container = container;
    }

    public IReadOnlyList<IScreen> CreateAllTabs()
    {
        // Discover all registered report schemes
        var schemes = _container.GetAllInstances<IReportScheme>();

        // Create a tab for each scheme
        return schemes
            .Select(s => s.CreateTab())
            .ToList();
    }
}