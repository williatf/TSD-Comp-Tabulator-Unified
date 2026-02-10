using Caliburn.Micro;

namespace Tsd.Tabulator.Wpf;

public static class CaliburnConfig
{
    public static void Configure()
    {
        // Map top-level ViewModels -> Views
        ViewLocator.NameTransformer.AddRule(
            "ViewModels",
            "Views"
        );

        // Also map nested folders, e.g. ViewModels.Dialogs -> Views.Dialogs
        ViewLocator.NameTransformer.AddRule(
            "ViewModels.",
            "Views."
        );
    }
}
