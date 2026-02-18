using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Caliburn.Micro;
using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite.Scoring;
using Tsd.Tabulator.Wpf.Reports;
using Tsd.Tabulator.Wpf.ViewModels;
using Tsd.Tabulator.Wpf.ViewModels.Reports;
using Tsd.Tabulator.Wpf.Reporting;

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
        _container.RegisterHandler(typeof(IScoreRepository), null, c =>
        {
            var shell = c.GetInstance<ShellViewModel>();
            if (!shell.HasEventLoaded)
                throw new InvalidOperationException("No event is currently open.");

            var factory = new SqliteConnectionFactory(shell.CurrentDbPath!);
            return new ScoreRepository(factory);
        });

        // Class config service (manages master-config and event snapshots)
        _container.Singleton<IClassConfigService, ClassConfigService>();
        
        // Shell (singleton - the event context)
        _container.Singleton<ShellViewModel>();
        
        // Report configuration (singleton)
        _container.Singleton<ReportConfiguration>();
        
        // Report infrastructure
        _container.RegisterHandler(typeof(IReportCatalog), null, c => 
        {
            var definitions = c.GetAllInstances(typeof(IReportDefinition))
                .Cast<IReportDefinition>();
            return new ReportCatalog(definitions);
        });

        _container.Singleton<ReportTabFactory>();


        // Register all report definitions (without keys so GetAllInstances can find them)
        // TODO: Remove these manual registrations once we have a more dynamic way to discover report definitions
        //_container.Singleton<IReportDefinition, SoloAwardsReportDefinition>();
        //_container.Singleton<IReportDefinition, DuetsAwardsReportDefinition>();


        // Solo Awards report dependencies
        _container.Singleton<IReportScheme, SoloAwardsScheme>();
        _container.Singleton<IReportLoader<SoloAwardEntry>, SoloAwardsLoaderAdapter>();
        _container.PerRequest<ReportTabViewModel<SoloAwardEntry>>();
        _container.PerRequest<SoloAwardsReportTabViewModel>();

        // Duet Awards report dependencies
        _container.Singleton<IReportScheme, DuetAwardsScheme>();
        _container.Singleton<IReportLoader<DuetAwardEntry>, DuetAwardsLoaderAdapter>();
        _container.PerRequest<DuetsAwardsReportTabViewModel>();

        // Report ViewModels (per-request)
        _container.PerRequest<ReportsViewModel>();
        _container.PerRequest<ReportViewModel>();

        // Report services (per-request to get fresh repository)
        _container.RegisterHandler(typeof(ISoloAwardReportService), null, c =>
        {
            //var shell = c.GetInstance(typeof(ShellViewModel), null) as ShellViewModel;
            var shell = c.GetInstance<ShellViewModel>();
            if (!shell.HasEventLoaded)
                throw new InvalidOperationException("No event is currently open.");

            //var factory = new SqliteConnectionFactory(shell.CurrentDbPath!);
            var scoreRepo = c.GetInstance<IScoreRepository>();

            //var classConfig = c.GetInstance(typeof(IClassConfigService), null) as IClassConfigService
            //?? throw new InvalidOperationException("IClassConfigService not registered.");
            var classConfig = c.GetInstance<IClassConfigService>();

            return new SoloAwardReportService(scoreRepo, classConfig, shell.CurrentDbPath!);
        });

        _container.RegisterHandler(typeof(IDuetAwardReportService), null, c =>
        {
            //var shell = c.GetInstance(typeof(ShellViewModel), null) as ShellViewModel;
            var shell = c.GetInstance<ShellViewModel>();
            if (!shell.HasEventLoaded)
                throw new InvalidOperationException("No event is currently open.");

            //var factory = new SqliteConnectionFactory(shell.CurrentDbPath!);
            var scoreRepo = c.GetInstance<IScoreRepository>();

            //var classConfig = c.GetInstance(typeof(IClassConfigService), null) as IClassConfigService
                              //?? throw new InvalidOperationException("IClassConfigService not registered.");
            var classConfig = c.GetInstance<IClassConfigService>();

            return new DuetAwardReportService(scoreRepo, classConfig, shell.CurrentDbPath!);
        });

        // Dialogs
        _container.PerRequest<NewEventDialogViewModel>();

        // Register ConfigViewModel for view resolution if needed (optional)
        _container.PerRequest<ConfigViewModel>();
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
}
