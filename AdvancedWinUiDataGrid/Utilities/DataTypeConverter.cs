// Utilities/DataTypeConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Utility trieda pre konverziu dátových typov v DataGrid komponente.
    /// Poskytuje robustné konverzie medzi rôznymi typmi s error handling.
    /// </summary>
    internal static class DataTypeConverter
    {
        #region Základné konverzie

        /// <summary>
        /// Pokúsi sa konvertovať hodnotu na cieľový typ.
        /// </summary>
        /// <param name="value">Hodnota na konverziu</param>
        /// <param name="targetType">Cieľový typ</param>
        /// <param name="convertedValue">Výsledná konvertovaná hodnota</param>
        /// <returns>True ak konverzia bola úspešná</returns>
        public static bool TryConvert(object? value, Type targetType, out object? convertedValue)
        {
            convertedValue = null;

            try
            {
                // Null handling
                if (value == null)
                {
                    convertedValue = GetDefaultValue(targetType);
                    return true;
                }

                // Ak je už správny typ
                if (targetType.IsAssignableFrom(value.GetType()))
                {
                    convertedValue = value;
                    return true;
                }

                // Handle nullable types
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null)
                {
                    return TryConvert(value, underlyingType, out convertedValue);
                }

                // String konverzie
                if (targetType == typeof(string))
                {
                    convertedValue = value.ToString();
                    return true;
                }

                var stringValue = value.ToString() ?? string.Empty;

                // Číselné typy
                if (TryConvertNumeric(stringValue, targetType, out convertedValue))
                    return true;

                // DateTime typy
                if (TryConvertDateTime(stringValue, targetType, out convertedValue))
                    return true;

                // Boolean typ
                if (TryConvertBoolean(stringValue, targetType, out convertedValue))
                    return true;

                // Enum typy
                if (TryConvertEnum(stringValue, targetType, out convertedValue))
                    return true;

                // Fallback - použiť System.Convert
                convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                convertedValue = GetDefaultValue(targetType);
                return false;
            }
        }

        /// <summary>
        /// Konvertuje hodnotu na cieľový typ alebo vráti default hodnotu.
        /// </summary>
        /// <param name="value">Hodnota na konverziu</param>
        /// <param name="targetType">Cieľový typ</param>
        /// <returns>Konvertovaná hodnota alebo default pre daný typ</returns>
        public static object? ConvertOrDefault(object? value, Type targetType)
        {
            return TryConvert(value, targetType, out var result) ? result : GetDefaultValue(targetType);
        }

        #endregion

        #region Špecializované konverzie

        /// <summary>
        /// Pokúsi sa konvertovať string na číselný typ.
        /// </summary>
        private static bool TryConvertNumeric(string value, Type targetType, out object? convertedValue)
        {
            convertedValue = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                convertedValue = GetDefaultValue(targetType);
                return true;
            }

            // Int typy
            if (targetType == typeof(int))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                {
                    convertedValue = intValue;
                    return true;
                }
                // Skúsiť parsovať ako decimal a potom cast na int
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decValue))
                {
                    convertedValue = (int)Math.Round(decValue);
                    return true;
                }
            }

            if (targetType == typeof(long))
            {
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longValue))
                {
                    convertedValue = longValue;
                    return true;
                }
            }

            // Decimal typy
            if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalValue))
                {
                    convertedValue = decimalValue;
                    return true;
                }
            }

            if (targetType == typeof(double))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
                {
                    convertedValue = doubleValue;
                    return true;
                }
            }

            if (targetType == typeof(float))
            {
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                {
                    convertedValue = floatValue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Pokúsi sa konvertovať string na DateTime typ.
        /// </summary>
        private static bool TryConvertDateTime(string value, Type targetType, out object? convertedValue)
        {
            convertedValue = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                convertedValue = GetDefaultValue(targetType);
                return true;
            }

            if (targetType == typeof(DateTime))
            {
                // Skúsiť rôzne formáty
                var formats = new[]
                {
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd",
                    "dd.MM.yyyy",
                    "dd/MM/yyyy",
                    "MM/dd/yyyy",
                    "yyyy/MM/dd"
                };

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        convertedValue = dateTime;
                        return true;
                    }
                }

                // Fallback na general parsing
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime generalDateTime))
                {
                    convertedValue = generalDateTime;
                    return true;
                }
            }

            if (targetType == typeof(DateOnly))
            {
                if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOnly))
                {
                    convertedValue = dateOnly;
                    return true;
                }
            }

            if (targetType == typeof(TimeOnly))
            {
                if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly timeOnly))
                {
                    convertedValue = timeOnly;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Pokúsi sa konvertovať string na boolean typ.
        /// </summary>
        private static bool TryConvertBoolean(string value, Type targetType, out object? convertedValue)
        {
            convertedValue = null;

            if (targetType != typeof(bool))
                return false;

            if (string.IsNullOrWhiteSpace(value))
            {
                convertedValue = false;
                return true;
            }

            // Štandardné boolean hodnoty
            if (bool.TryParse(value, out bool boolValue))
            {
                convertedValue = boolValue;
                return true;
            }

            // Rozšírené boolean hodnoty
            var lowerValue = value.ToLowerInvariant().Trim();
            var trueValues = new[] { "1", "yes", "y", "ano", "áno", "true", "on" };
            var falseValues = new[] { "0", "no", "n", "nie", "false", "off" };

            if (Array.Exists(trueValues, v => v == lowerValue))
            {
                convertedValue = true;
                return true;
            }

            if (Array.Exists(falseValues, v => v == lowerValue))
            {
                convertedValue = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pokúsi sa konvertovať string na enum typ.
        /// </summary>
        private static bool TryConvertEnum(string value, Type targetType, out object? convertedValue)
        {
            convertedValue = null;

            if (!targetType.IsEnum)
                return false;

            if (string.IsNullOrWhiteSpace(value))
            {
                convertedValue = GetDefaultValue(targetType);
                return true;
            }

            try
            {
                // Skúsiť parsovať ako string
                if (Enum.TryParse(targetType, value, true, out var enumValue))
                {
                    convertedValue = enumValue;
                    return true;
                }

                // Skúsiť parsovať ako číslo
                if (int.TryParse(value, out int intValue) && Enum.IsDefined(targetType, intValue))
                {
                    convertedValue = Enum.ToObject(targetType, intValue);
                    return true;
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return false;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Vráti default hodnotu pre daný typ.
        /// </summary>
        /// <param name="type">Typ</param>
        /// <returns>Default hodnota</returns>
        public static object? GetDefaultValue(Type type)
        {
            if (type == null)
                return null;

            // Handle nullable types
            if (Nullable.GetUnderlyingType(type) != null)
                return null;

            // Value types
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            // Reference types
            return null;
        }

        /// <summary>
        /// Určuje či je typ číselný.
        /// </summary>
        /// <param name="type">Typ na kontrolu</param>
        /// <returns>True ak je typ číselný</returns>
        public static bool IsNumericType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType == typeof(int) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType == typeof(double) ||
                   underlyingType == typeof(float) ||
                   underlyingType == typeof(short) ||
                   underlyingType == typeof(byte) ||
                   underlyingType == typeof(uint) ||
                   underlyingType == typeof(ulong) ||
                   underlyingType == typeof(ushort) ||
                   underlyingType == typeof(sbyte);
        }

        /// <summary>
        /// Určuje či je typ date/time typ.
        /// </summary>
        /// <param name="type">Typ na kontrolu</param>
        /// <returns>True ak je typ date/time</returns>
        public static bool IsDateTimeType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType == typeof(DateTime) ||
                   underlyingType == typeof(DateOnly) ||
                   underlyingType == typeof(TimeOnly) ||
                   underlyingType == typeof(DateTimeOffset);
        }

        /// <summary>
        /// Formátuje hodnotu pre zobrazenie v UI.
        /// </summary>
        /// <param name="value">Hodnota na formátovanie</param>
        /// <param name="type">Typ hodnoty</param>
        /// <returns>Formátovaný string</returns>
        public static string FormatForDisplay(object? value, Type type)
        {
            if (value == null)
                return string.Empty;

            try
            {
                return value switch
                {
                    DateTime dt => dt.ToString("dd.MM.yyyy HH:mm"),
                    DateOnly date => date.ToString("dd.MM.yyyy"),
                    TimeOnly time => time.ToString("HH:mm:ss"),
                    decimal dec => dec.ToString("N2", CultureInfo.CurrentCulture),
                    double dbl => dbl.ToString("N2", CultureInfo.CurrentCulture),
                    float flt => flt.ToString("N2", CultureInfo.CurrentCulture),
                    bool bl => bl ? "Áno" : "Nie",
                    _ => value.ToString() ?? string.Empty
                };
            }
            catch
            {
                return value.ToString() ?? string.Empty;
            }
        }

        #endregion

        #region Type registry

        /// <summary>
        /// Registry podporovaných typov s ich default konvertermi.
        /// </summary>
        public static readonly Dictionary<Type, Func<string, object?>> TypeConverters = new()
        {
            { typeof(string), value => value },
            { typeof(int), value => int.TryParse(value, out var result) ? result : 0 },
            { typeof(long), value => long.TryParse(value, out var result) ? result : 0L },
            { typeof(decimal), value => decimal.TryParse(value, out var result) ? result : 0m },
            { typeof(double), value => double.TryParse(value, out var result) ? result : 0.0 },
            { typeof(float), value => float.TryParse(value, out var result) ? result : 0f },
            { typeof(bool), value => bool.TryParse(value, out var result) ? result : false },
            { typeof(DateTime), value => DateTime.TryParse(value, out var result) ? result : DateTime.MinValue }
        };

        #endregion
    }
}