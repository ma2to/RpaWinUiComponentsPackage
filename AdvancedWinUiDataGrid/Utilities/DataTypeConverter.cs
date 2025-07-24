// Utilities/DataTypeConverter.cs
using System;
using System.Globalization;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre konverziu dátových typov
    /// </summary>
    public static class DataTypeConverter
    {
        /// <summary>
        /// Konvertuje hodnotu na zadaný typ
        /// </summary>
        public static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
                return GetDefaultValue(targetType);

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            // Handle Nullable types
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType)!;
                var convertedValue = ConvertValue(value, underlyingType);
                return convertedValue;
            }

            try
            {
                // Special handling for specific types
                if (targetType == typeof(string))
                    return value.ToString();

                if (targetType == typeof(bool))
                    return ConvertToBool(value);

                if (targetType == typeof(DateTime))
                    return ConvertToDateTime(value);

                if (targetType == typeof(DateOnly))
                    return ConvertToDateOnly(value);

                if (targetType == typeof(TimeOnly))
                    return ConvertToTimeOnly(value);

                if (targetType.IsEnum)
                    return ConvertToEnum(value, targetType);

                // Use Convert.ChangeType for numeric types and others
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private static object? GetDefaultValue(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        private static bool ConvertToBool(object value)
        {
            if (value is bool boolValue)
                return boolValue;

            var stringValue = value.ToString()?.ToLowerInvariant();
            return stringValue switch
            {
                "true" or "1" or "yes" or "ano" or "y" or "a" => true,
                "false" or "0" or "no" or "nie" or "n" => false,
                _ => bool.Parse(stringValue!)
            };
        }

        private static DateTime ConvertToDateTime(object value)
        {
            if (value is DateTime dateTime)
                return dateTime;

            if (value is DateOnly dateOnly)
                return dateOnly.ToDateTime(TimeOnly.MinValue);

            return DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture);
        }

        private static DateOnly ConvertToDateOnly(object value)
        {
            if (value is DateOnly dateOnly)
                return dateOnly;

            if (value is DateTime dateTime)
                return DateOnly.FromDateTime(dateTime);

            return DateOnly.Parse(value.ToString()!, CultureInfo.InvariantCulture);
        }

        private static TimeOnly ConvertToTimeOnly(object value)
        {
            if (value is TimeOnly timeOnly)
                return timeOnly;

            if (value is DateTime dateTime)
                return TimeOnly.FromDateTime(dateTime);

            return TimeOnly.Parse(value.ToString()!, CultureInfo.InvariantCulture);
        }

        private static object ConvertToEnum(object value, Type enumType)
        {
            if (value.GetType() == enumType)
                return value;

            if (value is string stringValue)
                return Enum.Parse(enumType, stringValue, true);

            if (value is int or long or short or byte)
                return Enum.ToObject(enumType, value);

            return Enum.Parse(enumType, value.ToString()!, true);
        }
    }
}