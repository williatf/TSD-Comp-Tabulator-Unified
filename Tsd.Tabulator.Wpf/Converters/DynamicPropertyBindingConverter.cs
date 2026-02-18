using System;
using System.Globalization;
using System.Windows.Data;

namespace Tsd.Tabulator.Wpf.Converters
{
    public sealed class DynamicPropertyBindingConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var row = values[0];
            var propertyName = values[1] as string;

            if (row == null || propertyName == null)
                return string.Empty;

            var prop = row.GetType().GetProperty(propertyName);

            var value = prop?.GetValue(row);

            //convert everything to string for display, or return empty if null
            if (value is double d)
                return d.ToString("0.00");

            if (value is int or long)
                return value.ToString();

            return value?.ToString() ?? "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}