// Utilities/Converters.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Konverter pre bool na Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
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
    /// Inverted konverter pre bool na Visibility
    /// </summary>
    public class InvertedBoolToVisibilityConverter : IValueConverter
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
    /// Konverter pre validáciu na farbu brushu (červená pre nevalidné)
    /// </summary>
    public class BoolToValidationBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isValid)
            {
                if (isValid)
                {
                    // Validné - normálne orámovanie
                    return new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    // Nevalidné - červené orámovanie
                    return new SolidColorBrush(Colors.Red);
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Konverter pre validáciu na hrúbku orámovani (hrubšie pre nevalidné)
    /// </summary>
    public class BoolToValidationThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isValid)
            {
                if (isValid)
                {
                    // Validné - tenké orámovanie
                    return new Thickness(0, 0, 1, 1);
                }
                else
                {
                    // Nevalidné - hrubé červené orámovanie
                    return new Thickness(2);
                }
            }
            return new Thickness(0, 0, 1, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}