# Contributing Guidelines

## Overview
This project organizes source files into logical folders. Follow these rules when adding or moving code so builds, view?locators, and XAML partial classes remain consistent.

## Folder & namespace conventions (MANDATORY)
- Models go into `Models` and use namespace `Tsd.Tabulator.Wpf.Models`.
- Views go into `Views` and use namespace `Tsd.Tabulator.Wpf.Views`.
- ViewModels go into `ViewModels` and use namespace `Tsd.Tabulator.Wpf.ViewModels`.
- Other shared code stays in the root namespace `Tsd.Tabulator.Wpf` or appropriate subnamespace.

Always keep the namespace in the source file in sync with the folder path. For example:
- `Views/ShellView.xaml` -> `x:Class="Tsd.Tabulator.Wpf.Views.ShellView"`
- `Views/ShellView.xaml.cs` -> `namespace Tsd.Tabulator.Wpf.Views;`
- `ViewModels/ShellViewModel.cs` -> `namespace Tsd.Tabulator.Wpf.ViewModels;`

## XAML rules for Views
- The `x:Class` attribute in the `.xaml` must match the fully qualified namespace + class name of the code?behind partial class.
- The code?behind partial class must declare the same namespace and class name, and the same base type (e.g., `global::System.Windows.Controls.Ribbon.RibbonWindow` for ribbon windows).
- When moving a view, update both the `.xaml` `x:Class` and the `.xaml.cs` namespace declaration.

Example header for a moved view:
```xml
<!-- Views/ShellView.xaml -->
<ribbon:RibbonWindow x:Class="Tsd.Tabulator.Wpf.Views.ShellView"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  xmlns:ribbon="clr-namespace:System.Windows.Controls.Ribbon;assembly=System.Windows.Controls.Ribbon">
  ...
</ribbon:RibbonWindow>
```
And code?behind:
```csharp
// Views/ShellView.xaml.cs
namespace Tsd.Tabulator.Wpf.Views;

public partial class ShellView : global::System.Windows.Controls.Ribbon.RibbonWindow
{
    public ShellView() => InitializeComponent();
}
```

## Caliburn.Micro view conventions
- The project uses Caliburn.Micro's naming conventions. If you move Views/ViewModels into folders, ensure the `ViewLocator` mapping supports the folder structure. By default, this project maps `ViewModels` -> `Views`. If you introduce additional folders, update `CaliburnConfig` accordingly.

Example (already present) in `CaliburnConfig`:
```csharp
ViewLocator.NameTransformer.AddRule(
  "ViewModels",
  "Views"
);
```
If you create deeper folders (e.g., `ViewModels/Dialogs` and `Views/Dialogs`) add matching rules.

## Project file / build settings
- Visual Studio will normally update the `.csproj` automatically when moving files in Solution Explorer. Ensure XAML files remain `Build Action: Page` and code?behind remains `Build Action: Compile`.
- Images and resources must keep their `Build Action` (Resource) if referenced by pack URIs.

If you prefer to edit the `.csproj` manually, add folder patterns (not required but allowed):
```xml
<ItemGroup>
  <Page Include="Views\**\*.xaml" />
  <Compile Include="Views\**\*.xaml.cs" />
  <Compile Include="ViewModels\**\*.cs" />
  <Compile Include="Models\**\*.cs" />
</ItemGroup>
```

## Steps to move files safely (recommended)
1. In Visual Studio Solution Explorer, create folders `Views`, `ViewModels`, `Models` under the project.
2. Drag files into the matching folder. VS will update the project and prompt to rename namespaces — accept the refactor.
3. For each moved `.xaml` view:
   - Open `.xaml` and update `x:Class` to the new fully qualified name.
   - Open `.xaml.cs` and update the `namespace` to match.
   - Ensure the code?behind class still uses the same base type as the XAML root element.
4. Update any DI registrations (e.g., Bootstrapper) and code references that used the old namespace.
5. Clean and rebuild the solution.

## PR checklist
- [ ] Namespaces match folder structure
- [ ] XAML `x:Class` + code?behind namespaces match
- [ ] Caliburn conventions updated if needed
- [ ] Project builds and views load at runtime

## Rationale
Consistent folder and namespace organization improves discoverability, helps tooling (refactors, find usages), and keeps Caliburn.Micro view resolutions predictable.