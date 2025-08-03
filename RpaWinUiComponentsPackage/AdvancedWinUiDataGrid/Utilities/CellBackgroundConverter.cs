// Utilities/CellBackgroundConverter.cs - ✅ INTERNAL converter pre cell background farby
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Converter pre cell background farby na base selection state - INTERNAL
    /// </summary>
    internal class CellBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is not AdvancedDataGrid.CellViewModel cell) 
                    return new SolidColorBrush(Colors.White);

                // Získaj color config z parameter
                var colorConfig = parameter as DataGridColorConfig;
                var defaultColor = colorConfig?.ResolvedCellBackgroundColor ?? Colors.White;

                // Priority: Copied > Focused > Selected > Validation Error > Zebra > Default
                if (cell.IsCopied)
                {
                    return new SolidColorBrush(colorConfig?.ResolvedCopiedCellColor ?? 
                        Windows.UI.Color.FromArgb(60, 34, 139, 34));
                }

                if (cell.IsFocused)
                {
                    return new SolidColorBrush(colorConfig?.ResolvedFocusedCellColor ?? 
                        Windows.UI.Color.FromArgb(80, 0, 120, 215));
                }

                if (cell.IsSelected)
                {
                    return new SolidColorBrush(colorConfig?.ResolvedSelectionColor ?? 
                        Windows.UI.Color.FromArgb(100, 0, 120, 215));
                }

                // Validation error background (ale zachováme border)
                if (!cell.IsValid && cell.HasValidationErrors)
                {
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(30, 255, 0, 0)); // Light red background
                }

                // Check if this is zebra row
                // Potrebujeme access k parent row info - zatiaľ jednoduché riešenie
                return new SolidColorBrush(defaultColor);
            }
            catch
            {
                return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter pre cell border farby a thickness na base validation state - INTERNAL
    /// </summary>
    internal class CellBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is not AdvancedDataGrid.CellViewModel cell) 
                    return new SolidColorBrush(Colors.LightGray);

                var colorConfig = parameter as DataGridColorConfig;

                // Validation error má červený border
                if (!cell.IsValid && cell.HasValidationErrors)
                {
                    return new SolidColorBrush(colorConfig?.ResolvedValidationErrorBorderColor ?? Colors.Red);
                }

                // Focused cell má modrý border
                if (cell.IsFocused)
                {
                    return new SolidColorBrush(colorConfig?.ResolvedFocusedCellColor ?? 
                        Windows.UI.Color.FromArgb(255, 0, 120, 215));
                }

                // Default border
                return new SolidColorBrush(colorConfig?.ResolvedCellBorderColor ?? Colors.LightGray);
            }
            catch
            {
                return new SolidColorBrush(Colors.LightGray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter pre cell border thickness na base validation state - INTERNAL
    /// </summary>
    internal class CellBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is not AdvancedDataGrid.CellViewModel cell) 
                    return new Thickness(1);

                // Validation error má hrubší border
                if (!cell.IsValid && cell.HasValidationErrors)
                {
                    return new Thickness(2);
                }

                // Focused cell má hrubší border
                if (cell.IsFocused)
                {
                    return new Thickness(2);
                }

                // Default thickness
                return new Thickness(1);
            }
            catch
            {
                return new Thickness(1);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}