// Utilities/BoolToVisibilityConverter.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
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

    /// <summary>
    /// Konverter pre validáciu - false → červený brush, true → default brush.
    /// </summary>
    internal class BoolToValidationBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isValid)
            {
                if (isValid)
                {
                    // Transparent pre validné bunky
                    return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
                else
                {
                    // Červená pre nevalidné bunky
                    return new SolidColorBrush(Microsoft.UI.Colors.Red);
                }
            }

            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Konverter pre validáciu - false → hrubší border, true → štandardný border.
    /// </summary>
    internal class BoolToValidationThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isValid)
            {
                return isValid ? new Thickness(0) : new Thickness(2);
            }

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}