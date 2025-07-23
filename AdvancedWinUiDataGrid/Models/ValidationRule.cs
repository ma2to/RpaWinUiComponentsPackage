// Models/ValidationRule.cs
using System;
using System.Text.RegularExpressions;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Definícia validačného pravidla pre bunky v DataGrid.
    /// Podporuje predvolené aj custom validácie.
    /// </summary>
    public class ValidationRule
    {
        #region Delegates

        /// <summary>
        /// Delegate pre custom validačnú funkciu.
        /// </summary>
        /// <param name="value">Hodnota na validáciu</param>
        /// <returns>True ak je hodnota validná</returns>
        public delegate bool ValidationFunction(object? value);

        #endregion

        #region Konštruktory

        /// <summary>
        /// Privátny konštruktor - používajte factory metódy.
        /// </summary>
        private ValidationRule(string columnName, ValidationFunction validationFunc, string errorMessage, ValidationRuleType ruleType)
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            ValidationFunc = validationFunc ?? throw new ArgumentNullException(nameof(validationFunc));
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            RuleType = ruleType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Názov stĺpca na ktorý sa pravidlo aplikuje.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Funkcia ktorá vykoná validáciu.
        /// </summary>
        public ValidationFunction ValidationFunc { get; }

        /// <summary>
        /// Chybová správa zobrazená pri nevalidnej hodnote.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Typ validačného pravidla.
        /// </summary>
        public ValidationRuleType RuleType { get; }

        /// <summary>
        /// Priorita pravidla (vyššie číslo = vyššia priorita).
        /// </summary>
        public int Priority { get; set; } = 0;

        #endregion

        #region Validácia

        /// <summary>
        /// Vykoná validáciu na danej hodnote.
        /// </summary>
        /// <param name="value">Hodnota na validáciu</param>
        /// <returns>True ak je hodnota validná</returns>
        public bool Validate(object? value)
        {
            try
            {
                return ValidationFunc(value);
            }
            catch
            {
                // Ak nastane chyba pri validácii, považujeme hodnotu za nevalidnú
                return false;
            }
        }

        /// <summary>
        /// Vráti formátovanú chybovú správu s názvom stĺpca.
        /// </summary>
        public string GetFormattedErrorMessage()
        {
            return $"{ColumnName}: {ErrorMessage}";
        }

        #endregion

        #region Factory Methods - Required

        /// <summary>
        /// Vytvorí pravidlo pre povinné pole.
        /// </summary>
        public static ValidationRule Required(string columnName, string? errorMessage = null)
        {
            var message = errorMessage ?? "Pole je povinné";

            return new ValidationRule(
                columnName,
                value => !IsNullOrEmpty(value),
                message,
                ValidationRuleType.Required
            );
        }

        #endregion

        #region Factory Methods - Email

        /// <summary>
        /// Vytvorí pravidlo pre validáciu email adresy.
        /// </summary>
        public static ValidationRule Email(string columnName, string? errorMessage = null)
        {
            var message = errorMessage ?? "Neplatný email formát";
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

            return new ValidationRule(
                columnName,
                value => {
                    if (IsNullOrEmpty(value)) return true; // Email nie je povinný ak nie je definované Required
                    return emailRegex.IsMatch(value.ToString()!);
                },
                message,
                ValidationRuleType.Email
            );
        }

        #endregion

        #region Factory Methods - Range

        /// <summary>
        /// Vytvorí pravidlo pre číselný rozsah (int).
        /// </summary>
        public static ValidationRule Range(string columnName, int min, int max, string? errorMessage = null)
        {
            var message = errorMessage ?? $"Hodnota musí byť medzi {min} a {max}";

            return new ValidationRule(
                columnName,
                value => {
                    if (IsNullOrEmpty(value)) return true; // Range nie je povinný ak nie je definované Required

                    if (int.TryParse(value.ToString(), out int intValue))
                    {
                        return intValue >= min && intValue <= max;
                    }
                    return false;
                },
                message,
                ValidationRuleType.Range
            );
        }

        /// <summary>
        /// Vytvorí pravidlo pre číselný rozsah (decimal).
        /// </summary>
        public static ValidationRule Range(string columnName, decimal min, decimal max, string? errorMessage = null)
        {
            var message = errorMessage ?? $"Hodnota musí byť medzi {min} a {max}";

            return new ValidationRule(
                columnName,
                value => {
                    if (IsNullOrEmpty(value)) return true;

                    if (decimal.TryParse(value.ToString(), out decimal decimalValue))
                    {
                        return decimalValue >= min && decimalValue <= max;
                    }
                    return false;
                },
                message,
                ValidationRuleType.Range
            );
        }

        /// <summary>
        /// Vytvorí pravidlo pre číselný rozsah (double).
        /// </summary>
        public static ValidationRule Range(string columnName, double min, double max, string? errorMessage = null)
        {
            var message = errorMessage ?? $"Hodnota musí byť medzi {min} a {max}";

            return new ValidationRule(
                columnName,
                value => {
                    if (IsNullOrEmpty(value)) return true;

                    if (double.TryParse(value.ToString(), out double doubleValue))
                    {
                        return doubleValue >= min && doubleValue <= max;
                    }
                    return false;
                },
                message,
                ValidationRuleType.Range
            );
        }

        #endregion

        #region Factory Methods - MinLength/MaxLength

        /// <summary>
        /// Vytvorí pravidlo pre minimálnu dĺžku textu.
        /// </summary>
        public static ValidationRule MinLength(string columnName, int minLength, string? errorMessage = null)
        {
            var message = errorMessage ?? $"Minimálna dĺžka je {minLength} znakov";

            return new ValidationRule(
                columnName,
                value => {
                    if (IsNullOrEmpty(value)) return true;
                    return value.ToString()!.Length >= minLength;
                },
                message,
                ValidationRuleType.MinLength
            );
        }

        /// <summary>
        /// Vytvorí pravidlo pre maximálnu dĺžku textu.
        /// </summary>
        public static ValidationRule MaxLength(string columnName, int maxLength, string? errorMessage = null)
        {
            var message = errorMessage ?? $"Maximálna dĺžka je {maxLength} znakov";

            return new ValidationRule(
                columnName,
                value => {
                    if (IsNullOrEmpty(value)) return true;
                    return value.ToString()!.Length <= maxLength;
                },
                message,
                ValidationRuleType.MaxLength
            );
        }

        #endregion

        #region Factory Methods - Custom

        /// <summary>
        /// Vytvorí custom validačné pravidlo.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="validationFunc">Custom validačná funkcia</param>
        /// <param name="errorMessage">Chybová správa</param>
        /// <returns>ValidationRule s custom logikou</returns>
        public static ValidationRule Custom(string columnName, ValidationFunction validationFunc, string errorMessage)
        {
            return new ValidationRule(columnName, validationFunc, errorMessage, ValidationRuleType.Custom);
        }

        /// <summary>
        /// Vytvorí custom validačné pravidlo s Func&lt;object?, bool&gt;.
        /// </summary>
        public static ValidationRule Custom(string columnName, Func<object?, bool> validationFunc, string errorMessage)
        {
            return new ValidationRule(columnName, value => validationFunc(value), errorMessage, ValidationRuleType.Custom);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Skontroluje či je hodnota null alebo prázdna.
        /// </summary>
        private static bool IsNullOrEmpty(object? value)
        {
            if (value == null) return true;
            if (value is string str) return string.IsNullOrWhiteSpace(str);
            return false;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"ValidationRule for '{ColumnName}' ({RuleType}): {ErrorMessage}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is ValidationRule other)
            {
                return ColumnName.Equals(other.ColumnName, StringComparison.OrdinalIgnoreCase)
                    && RuleType == other.RuleType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ColumnName.ToLowerInvariant(), RuleType);
        }

        #endregion
    }

    /// <summary>
    /// Typ validačného pravidla.
    /// </summary>
    public enum ValidationRuleType
    {
        /// <summary>Povinné pole</summary>
        Required,

        /// <summary>Email validácia</summary>
        Email,

        /// <summary>Číselný rozsah</summary>
        Range,

        /// <summary>Minimálna dĺžka textu</summary>
        MinLength,

        /// <summary>Maximálna dĺžka textu</summary>
        MaxLength,

        /// <summary>Custom validácia</summary>
        Custom
    }
}