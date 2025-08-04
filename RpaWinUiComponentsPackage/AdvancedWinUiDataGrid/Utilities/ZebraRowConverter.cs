// Utilities/ZebraRowConverter.cs - ✅ NOVÝ Zebra Rows converter
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using Windows.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Converter pre Zebra rows effect - ✅ PUBLIC (potrebné pre XAML binding)
    /// </summary>
    public class ZebraRowConverter : IValueConverter
    {
        /// <summary>
        /// Default farba pre párne riadky (zebra effect)
        /// </summary>
        public SolidColorBrush? AlternateRowBrush { get; set; }

        /// <summary>
        /// Default farba pre nepárne riadky
        /// </summary>
        public SolidColorBrush? NormalRowBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                // Očakáva sa že value je bool - true pre zebra row, false pre normal row
                if (value is bool isZebraRow)
                {
                    if (isZebraRow)
                    {
                        // Použij AlternateRowBrush ak je nastavený, inak default zebra farbu
                        if (AlternateRowBrush != null)
                        {
                            return AlternateRowBrush;
                        }
                        else
                        {
                            // Default zebra farba - jemne tmavšia
                            return new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
                        }
                    }
                    else
                    {
                        // Použij NormalRowBrush ak je nastavený, inak transparent
                        return NormalRowBrush ?? new SolidColorBrush(Colors.Transparent);
                    }
                }

                // Parameter môže obsahovať custom farbu
                if (parameter is Color customColor)
                {
                    return new SolidColorBrush(customColor);
                }

                if (parameter is string colorString && TryParseColor(colorString, out var parsedColor))
                {
                    return new SolidColorBrush(parsedColor);
                }

                // Fallback na transparent
                return new SolidColorBrush(Colors.Transparent);
            }
            catch
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("ZebraRowConverter podporuje iba one-way binding");
        }

        /// <summary>
        /// Pokúsi sa parsovať string na Color
        /// </summary>
        private bool TryParseColor(string colorString, out Color color)
        {
            color = Colors.Transparent;

            try
            {
                // Podporuje #RRGGBB a #AARRGGBB formáty
                if (colorString.StartsWith("#") && colorString.Length >= 7)
                {
                    var hex = colorString.Substring(1);

                    if (hex.Length == 6) // RGB
                    {
                        var r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        var g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        var b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        color = Color.FromArgb(255, r, g, b);
                        return true;
                    }
                    else if (hex.Length == 8) // ARGB
                    {
                        var a = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        var r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        var g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        var b = System.Convert.ToByte(hex.Substring(6, 2), 16);
                        color = Color.FromArgb(a, r, g, b);
                        return true;
                    }
                }

                // Predefinované farby
                color = colorString.ToLowerInvariant() switch
                {
                    "red" => Colors.Red,
                    "green" => Colors.Green,
                    "blue" => Colors.Blue,
                    "yellow" => Colors.Yellow,
                    "orange" => Colors.Orange,
                    "purple" => Colors.Purple,
                    "gray" or "grey" => Colors.Gray,
                    "lightgray" or "lightgrey" => Colors.LightGray,
                    "darkgray" or "darkgrey" => Colors.DarkGray,
                    "white" => Colors.White,
                    "black" => Colors.Black,
                    "transparent" => Colors.Transparent,
                    _ => Colors.Transparent
                };

                return color != Colors.Transparent || colorString.ToLowerInvariant() == "transparent";
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Converter pre row index na bool (pre určenie zebra effect) - ✅ PUBLIC
    /// </summary>
    public class RowIndexToZebraConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is int rowIndex)
                {
                    // Párne indexy (0, 2, 4...) sú zebra rows
                    return rowIndex % 2 == 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-converter pre kombináciu zebra effect s custom color - ✅ PUBLIC
    /// </summary>
    public class ZebraRowMultiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                // value môže byť ZebraRowDisplayInfo alebo jednoduchý bool
                if (value is bool isZebra)
                {
                    if (parameter is Color customZebraColor && isZebra)
                    {
                        return new SolidColorBrush(customZebraColor);
                    }

                    // Default zebra colors
                    return isZebra
                        ? new SolidColorBrush(Color.FromArgb(20, 0, 0, 0))  // Jemne tmavšie
                        : new SolidColorBrush(Colors.Transparent);
                }

                return new SolidColorBrush(Colors.Transparent);
            }
            catch
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}