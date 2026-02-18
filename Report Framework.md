# TSD Reporting Framework

## A Complete Architectural Overview

This document describes the metadata-driven reporting framework used in the TSD Tabulator application. The system supports multiple competition types (Solo, Duet, Trio, Ensemble, etc.) through a generic, extensible, type-safe architecture that cleanly separates data, logic, and UI.

The framework is composed of:

* Core data models
* Report services
* Loader adapters
* Report schemes
* A dynamic tab factory
* A generic UI tab
* Dependency injection wiring

Together, these components form a scalable reporting pipeline that automatically discovers and displays all available report types.

---

# 1. Core Concepts

## 1.1 AwardEntryBase

All report rows inherit from a shared base class:

```csharp
public abstract record AwardEntryBase
{
    public string ClassKey { get; init; } = string.Empty;
    public int Place { get; init; }
    public decimal FinalScore { get; init; }
    public int ProgramNumber { get; init; }
}
```

Each report type (`SoloAwardEntry`, `DuetAwardEntry`, etc.) extends this base.
This allows the UI to treat all report entries generically.

---

# 2. Data Layer (Core)

## 2.1 Report Services

Each report type has a service responsible for:

* Querying the SQLite database
* Applying scoring, tie-breaking, and dedupe rules
* Grouping entries by class
* Returning a typed `...AwardReport` object

Example interface:

```csharp
public interface ISoloAwardReportService
{
    Task<SoloAwardReport> GenerateReportAsync();
}
```

### Why services use a DI handler

Report services require:

* The current event DB path
* A fresh repository
* Class configuration

These dependencies are event-specific, so services are registered using `RegisterHandler` to ensure correctness.

---

# 3. Application Layer

## 3.1 IReportLoader<T>

A generic interface that loads flat lists of entries for the UI:

```csharp
public interface IReportLoader<T>
{
    Task<IReadOnlyList<T>> LoadAsync();
}
```

Each report type has a loader adapter that:

* Calls the report service
* Flattens grouped results
* Returns a list of `T` entries

This keeps the UI generic and reusable.

---

## 3.2 IReportScheme

A report scheme describes the shape and metadata of a report:

* Display name
* Competition type
* Column definitions
* Sorting rules
* A factory method to create the UI tab

```csharp
public interface IReportScheme
{
    string DisplayName { get; }
    CompetitionType CompetitionType { get; }
    ReportSchema Schema { get; }
    IScreen CreateTab();
}
```

Each report type (Solo, Duet, etc.) has its own scheme.

---

# 4. UI Layer (WPF + Caliburn.Micro)

## 4.1 ReportTabViewModel<T>

A generic tab that:

* Loads data via `IReportLoader<T>`
* Groups entries by `ClassKey`
* Exposes rows to the view
* Supports refresh

```csharp
public class ReportTabViewModel<T> : Screen
    where T : AwardEntryBase
```

This is the heart of the generic UI.

---

## 4.2 ReportTabView

A single XAML view that works for all report types.

It binds to:

* Columns (from the scheme)
* Rows (from the loader)

No report-specific XAML exists.

---

# 5. Dependency Injection (SimpleContainer)

## 5.1 Score Repository

Registered per request, tied to the current event:

```csharp
_container.RegisterHandler(typeof(IScoreRepository), null, c =>
{
    var shell = c.GetInstance<ShellViewModel>();
    var factory = new SqliteConnectionFactory(shell.CurrentDbPath!);
    return new ScoreRepository(factory);
});
```

---

## 5.2 Report Services

Registered using handlers to inject event-specific dependencies:

```csharp
_container.RegisterHandler(typeof(ISoloAwardReportService), null, c =>
{
    var shell = c.GetInstance<ShellViewModel>();
    var repo = c.GetInstance<IScoreRepository>();
    var classConfig = c.GetInstance<IClassConfigService>();

    return new SoloAwardReportService(repo, classConfig, shell.CurrentDbPath!);
});
```

---

## 5.3 Loader Adapters

Registered as singletons:

```csharp
_container.Singleton<IReportLoader<SoloAwardEntry>, SoloAwardsLoaderAdapter>();
```

---

## 5.4 Report Schemes

Registered under the interface for automatic discovery:

```csharp
_container.Singleton<IReportScheme, SoloAwardsScheme>();
_container.Singleton<IReportScheme, DuetAwardsScheme>();
```

This enables dynamic tab creation.

---

# 6. The Tab Factory

The tab factory dynamically discovers all registered report schemes and creates tabs for them.

```csharp
public sealed class ReportTabFactory
{
    private readonly SimpleContainer _container;

    public ReportTabFactory(SimpleContainer container)
    {
        _container = container;
    }

    public IReadOnlyList<IScreen> CreateAllTabs()
    {
        return _container.GetAllInstances<IReportScheme>()
                         .Select(s => s.CreateTab())
                         .ToList();
    }
}
```

### Benefits

* No hard-coded report list
* Adding a new report = register a new scheme
* UI updates automatically
* Supports competition-type filtering

---

# 7. ReportViewModel (UI Entry Point)

This view model hosts all report tabs:

```csharp
public sealed class ReportViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly ReportTabFactory _factory;
    private readonly ShellViewModel _shell;

    public ReportViewModel(ShellViewModel shell, ReportTabFactory factory)
    {
        _shell = shell;
        _factory = factory;
        DisplayName = "Reports";
    }

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        Items.Clear();

        foreach (var tab in _factory.CreateAllTabs())
            Items.Add(tab);

        await ActivateItemAsync(Items.FirstOrDefault(), cancellationToken);
    }
}
```

---

# 8. ShellViewModel Navigation

```csharp
public void ShowReportsView()
{
    var vm = IoC.Get<ReportViewModel>();
    ActivateItemAsync(vm);
}
```

---

# 9. Adding a New Report Type (Example: Trio)

To add a new report:

1. Create `TrioAwardEntry`
2. Create `TrioAwardReportService`
3. Create `TrioAwardsLoaderAdapter`
4. Create `TrioAwardsScheme`

Register:

```csharp
_container.RegisterHandler(typeof(ITrioAwardReportService), null, ...);
_container.Singleton<IReportLoader<TrioAwardEntry>, TrioAwardsLoaderAdapter>();
_container.Singleton<IReportScheme, TrioAwardsScheme>();
```

The UI picks it up automatically.

---

# 10. Summary

The TSD reporting framework is:

* Generic — one UI for all reports
* Metadata-driven — schemes define everything
* Extensible — add new reports in minutes
* Competition-type aware
* DI-powered
* Dynamic — tabs discovered automatically
* Cleanly layered

This architecture is designed for long-term maintainability and easy expansion as new report types are added.
