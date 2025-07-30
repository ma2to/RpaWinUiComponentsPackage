// Models/ValidationRule.cs - ✅ KOMPLETNE OPRAVENÝ
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Definícia validačného pravidla pre stĺpec v DataGrid
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// Delegate pre custom validačnú funkciu
        /// </summary>
        /// <param name="value">Hodnota na validáciu</param>
        /// <returns>True ak je hodnota validná, False ak nie</returns>
        public delegate bool ValidationFunction(object? value);

        /// <summary>
        /// Názov stĺpca na ktorý sa validácia aplikuje
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Typ validácie
        /// </summary>
        public ValidationType Type { get; set; }

        /// <summary>
        /// Custom validačná funkcia (pre Type = Custom)
        /// </summary>
        public ValidationFunction? CustomValidator { get; set; }

        /// <summary>
        /// Chybová správa pri neúspešnej validácii
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Minimálna hodnota (pre Range validácie)
        /// </summary>
        public object? MinValue { get; set; }

        /// <summary>
        /// Maximálna hodnota (pre Range validácie)
        /// </summary>
        public object? MaxValue { get; set; }

        /// <summary>
        /// Minimálna dĺžka (pre string validácie)
        /// </summary>
        public int? MinLengthValue { get; set; }

        /// <summary>
        /// Maximálna dĺžka (pre string validácie)
        /// </summary>
        public int? MaxLengthValue { get; set; }

        /// <summary>
        /// Regex pattern (pre Pattern validácie)
        /// </summary>
        public string? PatternValue { get; set; }

        /// <summary>
        /// Či je validácia povinná
        /// </summary>
        public bool IsRequired { get; set; }

        // ✅ OPRAVENÉ: Konštruktory
        public ValidationRule() { }

        public ValidationRule(string columnName, ValidationType type, string errorMessage)
        {
            ColumnName = columnName;
            Type = type;
            ErrorMessage = errorMessage;
        }

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí custom validačné pravidlo
        /// </summary>
        public static ValidationRule Custom(string columnName, ValidationFunction validator, string errorMessage)
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.Custom,
                CustomValidator = validator,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Vytvorí Required validačné pravidlo
        /// </summary>
        public static ValidationRule Required(string columnName, string errorMessage = "Pole je povinné")
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.Required,
                ErrorMessage = errorMessage,
                IsRequired = true
            };
        }

        /// <summary>
        /// Vytvorí Email validačné pravidlo
        /// </summary>
        public static ValidationRule Email(string columnName, string errorMessage = "Neplatný email formát")
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.Email,
                ErrorMessage = errorMessage,
                PatternValue = @"^[^\s@]+@[^\s@]+\.[^\s@]+$"
            };
        }

        /// <summary>
        /// Vytvorí Range validačné pravidlo pre čísla
        /// </summary>
        public static ValidationRule Range(string columnName, object minValue, object maxValue, string errorMessage)
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.Range,
                MinValue = minValue,
                MaxValue = maxValue,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Vytvorí MinLength validačné pravidlo
        /// </summary>
        public static ValidationRule MinLength(string columnName, int minLength, string errorMessage)
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.MinLength,
                MinLengthValue = minLength,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Vytvorí MaxLength validačné pravidlo
        /// </summary>
        public static ValidationRule MaxLength(string columnName, int maxLength, string errorMessage)
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.MaxLength,
                MaxLengthValue = maxLength,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Vytvorí Pattern validačné pravidlo (regex)
        /// </summary>
        public static ValidationRule Pattern(string columnName, string pattern, string errorMessage)
        {
            return new ValidationRule
            {
                ColumnName = columnName,
                Type = ValidationType.Pattern,
                PatternValue = pattern,
                ErrorMessage = errorMessage
            };
        }

        #endregion

        /// <summary>
        /// Validuje hodnotu podľa tohto pravidla
        /// </summary>
        public bool Validate(object? value)
        {
            return Type switch
            {
                ValidationType.Required => ValidateRequired(value),
                ValidationType.Email => ValidateEmail(value),
                ValidationType.Range => ValidateRange(value),
                ValidationType.MinLength => ValidateMinLength(value),
                ValidationType.MaxLength => ValidateMaxLength(value),
                ValidationType.Pattern => ValidatePattern(value),
                ValidationType.Custom => ValidateCustom(value),
                _ => true
            };
        }

        #region Private Validation Methods

        private bool ValidateRequired(object? value)
        {
            return value != null && !string.IsNullOrWhiteSpace(value.ToString());
        }

        private bool ValidateEmail(object? value)
        {
            if (value == null) return !IsRequired;
            var emailStr = value.ToString();
            if (string.IsNullOrWhiteSpace(emailStr)) return !IsRequired;

            try
            {
                var addr = new System.Net.Mail.MailAddress(emailStr);
                return addr.Address == emailStr;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateRange(object? value)
        {
            if (value == null) return !IsRequired;
            if (MinValue == null || MaxValue == null) return true;

            try
            {
                var numericValue = Convert.ToDecimal(value);
                var minDecimal = Convert.ToDecimal(MinValue);
                var maxDecimal = Convert.ToDecimal(MaxValue);
                return numericValue >= minDecimal && numericValue <= maxDecimal;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateMinLength(object? value)
        {
            if (value == null) return !IsRequired;
            var str = value.ToString();
            if (string.IsNullOrEmpty(str)) return !IsRequired;
            return MinLengthValue == null || str.Length >= MinLengthValue.Value;
        }

        private bool ValidateMaxLength(object? value)
        {
            if (value == null) return !IsRequired;
            var str = value.ToString() ?? "";
            return MaxLengthValue == null || str.Length <= MaxLengthValue.Value;
        }

        private bool ValidatePattern(object? value)
        {
            if (value == null) return !IsRequired;
            var str = value.ToString();
            if (string.IsNullOrEmpty(str)) return !IsRequired;
            if (string.IsNullOrEmpty(PatternValue)) return true;

            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(str, PatternValue);
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateCustom(object? value)
        {
            return CustomValidator?.Invoke(value) ?? true;
        }

        #endregion
    }

    /// <summary>
    /// Typ validácie
    /// </summary>
    public enum ValidationType
    {
        Required,
        Email,
        Range,
        MinLength,
        MaxLength,
        Pattern,
        Custom
    }
}