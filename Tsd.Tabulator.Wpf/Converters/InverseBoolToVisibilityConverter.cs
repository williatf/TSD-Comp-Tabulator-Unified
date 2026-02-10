using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tsd.Tabulator.Wpf;

/// <summary>
/// Converts bool to Visibility with inverse logic (true = Collapsed, false = Visible).
/// </summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}