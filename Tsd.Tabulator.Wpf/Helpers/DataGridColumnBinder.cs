using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Tsd.Tabulator.Application.Models.Reporting;

namespace Tsd.Tabulator.Wpf.Helpers;

public static class DataGridColumnBinder
{
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.RegisterAttached(
            "Columns",
            typeof(IReadOnlyList<ReportColumn>),
            typeof(DataGridColumnBinder),
            new PropertyMetadata(null, OnColumnsChanged));

    public static IReadOnlyList<ReportColumn>? GetColumns(DependencyObject obj)
        => (IReadOnlyList<ReportColumn>?)obj.GetValue(ColumnsProperty);

    public static void SetColumns(DependencyObject obj, IReadOnlyList<ReportColumn>? value)
        => obj.SetValue(ColumnsProperty, value);

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
            return;

        dataGrid.Columns.Clear();

        if (e.NewValue is not IReadOnlyList<ReportColumn> columns)
            return;

        foreach (var col in columns.OrderBy(c => c.DisplayIndex))
        {
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = col.Header,
                Binding = new Binding(col.BindingPath)
            });
        }
    }
}