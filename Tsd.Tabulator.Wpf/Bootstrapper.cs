using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Caliburn.Micro;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Application.Interfaces.Reporting;
using Tsd.Tabulator.Application.Services;
using Tsd.Tabulator.Wpf.Reports;
using Tsd.Tabulator.Wpf.Reports.Schemes;
using Tsd.Tabulator.Application.Reports;
using Tsd.Tabulator.Application.Reports.Loaders;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite.Scoring;
using Tsd.Tabulator.Wpf.ViewModels;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Wpf.Helpers;

namespace Tsd.Tabulator.Wpf;

[SupportedOSPlatform("windows")]
public sealed class Bootstrapper : BootstrapperBase
{
    private readonly SimpleContainer _container = new();

    public Bootstrapper() => Initialize();

    protected override void Configure()
    {
        // Ensure Caliburn naming rules are applied before view resolution
        CaliburnConfig.Configure();

        // Caliburn.Micro infrastructure
        _container.Singleton<IWindowManager, WindowManager>();
        _container.Singleton<IEventAggregator, EventAggregator>();
        
        // Core services
        _container.Singleton<IFingerprintService, FingerprintService>();
        _container.Singleton<IClassConfigService, ClassConfigService>();
        
        // Shell (singleton - the event context)
        _container.Singleton<ShellViewModel>();

        // Event context
        _container.Singleton<IEventContext, EventContext>();

        // === NEW: Report scheme infrastructure ===
        _container.RegisterHandler(typeof(IReportSchemeProvider), null, c =>
        {
            // Get ALL registered scheme objects (concrete classes)
            var schemes = c.GetAllInstances(typeof(object))
                           .Where(o => o is IReportSchemeUiBase)   // see below
                           .ToList();

            return new ReportSchemeProvider(schemes);
        });

        // Register all IReportScheme implementations
        _container.Singleton<object, SoloAwardsReportScheme>();
        // _container.Singleton<IReportScheme, DuetAwardsReportScheme>(); // when created

        // Add factory registration
        _container.Singleton<IScoreRepositoryFactory, ScoreRepositoryFactory>();

        // Register loaders as singletons
        _container.Singleton<IReportDataLoader<SoloAwardCandidate>, SoloAwardsDataLoader>();
        //_container.Singleton<IReportDataLoader<DuetAwardCandidate>, DuetAwardsDataLoader>();

        _container.Singleton<ReportDataLoaderRegistry>();
        var registry = _container.GetInstance<ReportDataLoaderRegistry>();
        registry.Initialize(_container);

        _container.Singleton<ReportsViewModel>();

        LogManager.GetLog = type => new DebugLog(type);

    }

    protected override object GetInstance(Type service, string key)
        => _container.GetInstance(service, key)
           ?? throw new InvalidOperationException($"Could not locate {service.FullName}");

    protected override IEnumerable<object> GetAllInstances(Type service)
        => _container.GetAllInstances(service);

    protected override void BuildUp(object instance)
        => _container.BuildUp(instance);

    protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
    {
        DisplayRootViewForAsync<ShellViewModel>().GetAwaiter().GetResult();
    }

    public class DebugLog : ILog
    {
        private readonly Type _type;

        public DebugLog(Type type)
        {
            _type = type;
        }

        public void Error(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {_type.Name}: {exception}");
        }

        public void Info(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine($"[INFO] {_type.Name}: {string.Format(format, args)}");
        }

        public void Warn(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine($"[WARN] {_type.Name}: {string.Format(format, args)}");
        }
    }
}
