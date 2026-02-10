using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Views;

public partial class DataView : UserControl
{
    public DataView() => InitializeComponent();

    private void ScoreEntryBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Move to next control (same as Tab)
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            ((UIElement)sender).MoveFocus(request);
            e.Handled = true;
        }
    }
}
