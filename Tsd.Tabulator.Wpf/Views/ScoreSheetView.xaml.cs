using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tsd.Tabulator.Wpf.ViewModels.Scoring;

namespace Tsd.Tabulator.Wpf.Views;

public partial class ScoreSheetView : UserControl
{
    public ScoreSheetView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ScoreSheetTabVM vm && vm.ShowGroupHeaders)
        {
            BuildGroupHeaders(vm);
        }
    }

    private void BuildGroupHeaders(ScoreSheetTabVM vm)
    {
        var grid = GroupHeaderColumnsGrid;
        grid.ColumnDefinitions.Clear();
        grid.Children.Clear();

        // Create one ColumnDefinition per score column (excluding Total) with fixed width to match TextBoxes
        // TextBox width is 90 + margins (1+1) = 92 per column
        int scoreColumnCount = vm.Columns.Count - 1; // -1 for Total
        for (int i = 0; i < scoreColumnCount; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92) });
        }

        // Add group header TextBlocks
        foreach (var header in vm.GroupHeaders)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var textBlock = new TextBlock
            {
                Text = header.Text,
                Style = (Style)FindResource("GroupHeaderStyle")
            };

            border.Child = textBlock;
            Grid.SetColumn(border, header.StartIndex);
            Grid.SetColumnSpan(border, header.Span);

            grid.Children.Add(border);
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Tab)
        {
            var textBox = (TextBox)sender;
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            request.Wrapped = true;
            textBox.MoveFocus(request);
            e.Handled = true;
        }
    }

    private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        textBox.SelectAll();
    }

    private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var textBox = (TextBox)sender;
        if (!textBox.IsKeyboardFocusWithin)
        {
            textBox.Focus();
            e.Handled = true;
        }
    }
}
