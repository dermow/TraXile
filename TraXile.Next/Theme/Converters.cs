using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TraXile.Next
{
    /// <summary>
    /// Two-way binding of an enum property to RadioButton.IsChecked:
    /// IsChecked="{Binding Range, Converter={StaticResource EnumEq}, ConverterParameter=Week}"
    /// </summary>
    public sealed class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null && parameter != null &&
            string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b && b && parameter != null
                ? Enum.Parse(targetType, parameter.ToString(), ignoreCase: true)
                : Binding.DoNothing;
    }

    /// <summary>true -> Collapsed, false -> Visible.</summary>
    public sealed class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
