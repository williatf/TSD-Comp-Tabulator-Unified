using Caliburn.Micro;
using System;
using Tsd.Tabulator.Application.Interfaces;

public sealed class ReportDataLoaderRegistry
{
    private SimpleContainer _container = null!;

    // Parameterless constructor required by Caliburn
    public ReportDataLoaderRegistry()
    {
    }

    // Called manually from Bootstrapper to inject the container
    public void Initialize(SimpleContainer container)
    {
        _container = container;
    }

    public IReportDataLoader<T> GetLoader<T>()
    {
        return _container.GetInstance<IReportDataLoader<T>>();
    }

    public object GetLoader(Type t)
    {
        var method = typeof(ReportDataLoaderRegistry)
            .GetMethod(nameof(GetLoader), Type.EmptyTypes)!
            .MakeGenericMethod(t);

        return method.Invoke(this, null)!;
    }
}