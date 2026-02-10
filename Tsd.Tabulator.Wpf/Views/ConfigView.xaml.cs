using System.Windows.Controls;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Views;

public partial class ConfigView : UserControl
{
    public ConfigView()
    {
        InitializeComponent();
    }

    private async void ClassDefinitionsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is ClassDefinition definition && DataContext is ConfigViewModel vm)
            {
                // Defer the save until after the row edit completes
                // We intentionally don't await this to allow the DataGrid to complete its edit cycle
                _ = Dispatcher.InvokeAsync(async () =>
                {
                    await vm.OnDefinitionRowEditEnding(definition);
                });
            }
        }
    }

    private async void ClassAliasesGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is ClassAlias alias && DataContext is ConfigViewModel vm)
            {
                // Defer the save until after the row edit completes
                // We intentionally don't await this to allow the DataGrid to complete its edit cycle
                _ = Dispatcher.InvokeAsync(async () =>
                {
                    await vm.OnAliasRowEditEnding(alias);
                });
            }
        }
    }
}