// Utilities/BoolToVisibilityConverter.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Konverter pre konverziu bool hodnôt na Visibility enum.
    /// Používa sa v XAML pre podmienené zobrazovanie elementov.
    /// </summary>
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }

    /// <summary>
    /// Inverzný BoolToVisibilityConverter - true → Collapsed, false → Visible.
    /// </summary>
    internal class InvertedBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }

            return true;
        }
    }
}