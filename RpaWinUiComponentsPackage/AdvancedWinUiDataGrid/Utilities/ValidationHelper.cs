// AdvancedWinUiDataGrid/Utilities/ValidationHelper.cs - ✅ NOVÝ SÚBOR
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre validačné operácie - INTERNAL
    /// </summary>
    internal static class ValidationHelper
    {
        /// <summary>
        /// Validuje hodnotu proti zoznamu pravidiel
        /// </summary>
        public static async Task<List<string>> ValidateValueAsync(
            object? value,
            List<ValidationRule> rules,
            ILogger? logger = null)
        {
            var errors = new List<string>();

            if (rules == null || !rules.Any())
            {
                logger?.LogTrace("🔍 ValidationHelper: No rules to validate against");
                return errors;
            }

            logger?.LogTrace("🔍 ValidationHelper: Validating value '{Value}' against {RuleCount} rules",
                value, rules.Count);

            await Task.Run(() =>
            {
                foreach (var rule in rules)
                {
                    try
                    {
                        if (!rule.Validate(value))
                        {
                            errors.Add(rule.ErrorMessage);
                            logger?.LogDebug("❌ ValidationHelper: Rule failed - {ErrorMessage}", rule.ErrorMessage);
                        }
                        else
                        {
                            logger?.LogTrace("✅ ValidationHelper: Rule passed - {RuleType}", rule.Type);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Validation error: {ex.Message}";
                        errors.Add(errorMessage);
                        logger?.LogError(ex, "❌ ValidationHelper: Exception in validation rule {RuleType}", rule.Type);
                    }
                }
            });

            logger?.LogDebug("🔍 ValidationHelper: Validation completed - {ErrorCount} errors", errors.Count);
            return errors;
        }

        /// <summary>
        /// Validuje celý riadok dát
        /// </summary>
        public static async Task<Dictionary<string, List<string>>> ValidateRowAsync(
            Dictionary<string, object?> rowData,
            Dictionary<string, List<ValidationRule>> columnRules,
            ILogger? logger = null)
        {
            var allErrors = new Dictionary<string, List<string>>();

            if (rowData == null || !rowData.Any())
            {
                logger?.LogTrace("🔍 ValidationHelper: Empty row data");
                return allErrors;
            }

            logger?.LogDebug("🔍 ValidationHelper: Validating row with {CellCount} cells", rowData.Count);

            var validationTasks = new List<Task<KeyValuePair<string, List<string>>>>();

            foreach (var kvp in rowData)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                if (columnRules.TryGetValue(columnName, out var rules))
                {
                    validationTasks.Add(ValidateColumnValueAsync(columnName, value, rules, logger));
                }
            }

            var results = await Task.WhenAll(validationTasks);

            foreach (var result in results)
            {
                if (result.Value.Any())
                {
                    allErrors[result.Key] = result.Value;
                }
            }

            logger?.LogDebug("🔍 ValidationHelper: Row validation completed - {ErrorColumns}/{TotalColumns} columns with errors",
                allErrors.Count, rowData.Count);

            return allErrors;
        }

        /// <summary>
        /// Validuje hodnotu pre konkrétny stĺpec
        /// </summary>
        private static async Task<KeyValuePair<string, List<string>>> ValidateColumnValueAsync(
            string columnName,
            object? value,
            List<ValidationRule> rules,
            ILogger? logger)
        {
            var errors = await ValidateValueAsync(value, rules, logger);
            return new KeyValuePair<string, List<string>>(columnName, errors);
        }

        /// <summary>
        /// Vytvorí validačné pravidlá pre bežné use cases
        /// </summary>
        public static class CommonValidationRules
        {
            /// <summary>
            /// Email validácia s rozšíreným regex
            /// </summary>
            public static ValidationRule Email(string columnName, string? errorMessage = null)
            {
                var message = errorMessage ?? "Neplatný email formát";
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

                return ValidationRule.Custom(columnName, value =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true; // Prazdne je OK ak nie je Required

                    return emailRegex.IsMatch(value.ToString()!);
                }, message);
            }

            /// <summary>
            /// Slovenské PSČ validácia
            /// </summary>
            public static ValidationRule SlovakPostalCode(string columnName, string? errorMessage = null)
            {
                var message = errorMessage ?? "Neplatné slovenské PSČ (formát: 12345)";
                var postalCodeRegex = new Regex(@"^\d{5}$", RegexOptions.Compiled);

                return ValidationRule.Custom(columnName, value =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true;

                    return postalCodeRegex.IsMatch(value.ToString()!);
                }, message);
            }

            /// <summary>
            /// Slovenské telefónne číslo
            /// </summary>
            public static ValidationRule SlovakPhoneNumber(string columnName, string? errorMessage = null)
            {
                var message = errorMessage ?? "Neplatné slovenské telefónne číslo";
                var phoneRegex = new Regex(@"^(\+421|00421|0)[1-9]\d{8}$", RegexOptions.Compiled);

                return ValidationRule.Custom(columnName, value =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true;

                    var cleanPhone = value.ToString()!.Replace(" ", "").Replace("-", "");
                    return phoneRegex.IsMatch(cleanPhone);
                }, message);
            }

            /// <summary>
            /// Pozitívne číslo validácia
            /// </summary>
            public static ValidationRule PositiveNumber(string columnName, bool allowZero = false, string? errorMessage = null)
            {
                var message = errorMessage ?? (allowZero ? "Číslo musí byť nezáporné" : "Číslo musí byť kladné");

                return ValidationRule.Custom(columnName, value =>
                {
                    if (value == null) return true;

                    if (decimal.TryParse(value.ToString(), out var decimalValue))
                        return allowZero ? decimalValue >= 0 : decimalValue > 0;

                    if (double.TryParse(value.ToString(), out var doubleValue))
                        return allowZero ? doubleValue >= 0 : doubleValue > 0;

                    if (int.TryParse(value.ToString(), out var intValue))
                        return allowZero ? intValue >= 0 : intValue > 0;

                    return false;
                }, message);
            }

            /// <summary>
            /// Dátum v budúcnosti
            /// </summary>
            public static ValidationRule FutureDate(string columnName, string? errorMessage = null)
            {
                var message = errorMessage ?? "Dátum musí byť v budúcnosti";

                return ValidationRule.Custom(columnName, value =>
                {
                    if (value == null) return true;

                    if (value is DateTime dateTime)
                        return dateTime.Date > DateTime.Today;

                    if (DateTime.TryParse(value.ToString(), out var parsedDate))
                        return parsedDate.Date > DateTime.Today;

                    return false;
                }, message);
            }

            /// <summary>
            /// Vek na základe dátumu narodenia
            /// </summary>
            public static ValidationRule Age(string columnName, int minAge, int maxAge, string? errorMessage = null)
            {
                var message = errorMessage ?? $"Vek musí byť medzi {minAge} a {maxAge} rokmi";

                return ValidationRule.Custom(columnName, value =>
                {
                    if (value == null) return true;

                    DateTime birthDate;
                    if (value is DateTime dt)
                        birthDate = dt;
                    else if (!DateTime.TryParse(value.ToString(), out birthDate))
                        return false;

                    var age = DateTime.Today.Year - birthDate.Year;
                    if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

                    return age >= minAge && age <= maxAge;
                }, message);
            }
        }

        /// <summary>
        /// Získa súhrn validačných chýb
        /// </summary>
        public static string GetErrorSummary(Dictionary<string, List<string>> errors)
        {
            if (!errors.Any()) return string.Empty;

            var summary = new List<string>();
            foreach (var kvp in errors)
            {
                var columnErrors = string.Join("; ", kvp.Value);
                summary.Add($"{kvp.Key}: {columnErrors}");
            }

            return string.Join(" | ", summary);
        }

        /// <summary>
        /// Kontroluje či má hodnota nejaké chyby
        /// </summary>
        public static bool HasErrors(Dictionary<string, List<string>> errors)
        {
            return errors.Any(kvp => kvp.Value.Any());
        }

        /// <summary>
        /// Získa celkový počet chýb
        /// </summary>
        public static int GetTotalErrorCount(Dictionary<string, List<string>> errors)
        {
            return errors.Sum(kvp => kvp.Value.Count);
        }
    }
}