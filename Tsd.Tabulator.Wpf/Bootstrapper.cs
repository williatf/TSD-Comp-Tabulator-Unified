using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Tsd.Tabulator.Core.Reports;
using Tsd.Tabulator.Core.Services;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite.Scoring;
using Tsd.Tabulator.Wpf.Reports;
using Tsd.Tabulator.Wpf.ViewModels;
using Tsd.Tabulator.Wpf.ViewModels.Reports;

namespace Tsd.Tabulator.Wpf;

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
        
        // Register all report definitions
        _container.RegisterSingleton(typeof(IReportDefinition), "SoloAwards", typeof(SoloAwardsReportDefinition));
        
        // Report ViewModels (per-request)
        _container.PerRequest<ReportsViewModel>();
        _container.PerRequest<SoloAwardsReportTabViewModel>();
        
        // Report services (per-request to get fresh repository)
        _container.RegisterHandler(typeof(ISoloAwardReportService), null, c =>
        {
            var shell = c.GetInstance(typeof(ShellViewModel), null) as ShellViewModel;
            if (shell == null || !shell.HasEventLoaded)
                throw new InvalidOperationException("No event is currently open.");
            
            var factory = new SqliteConnectionFactory(shell.CurrentDbPath!);
            var scoreRepo = new ScoreRepository(factory);

            var classConfig = c.GetInstance(typeof(IClassConfigService), null) as IClassConfigService
                              ?? throw new InvalidOperationException("IClassConfigService not registered.");

            return new SoloAwardReportService(scoreRepo, classConfig, shell.CurrentDbPath!);
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
