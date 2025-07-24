// Extensions/ValidationExtensions.cs - ✅ OPRAVENÝ
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Extension metódy pre ValidationRule a validačné operácie.
    /// Poskytuje pomocné metódy pre tvorbu a správu validačných pravidiel.
    /// </summary>
    internal static class ValidationExtensions
    {
        #region Collection extensions

        /// <summary>
        /// Vráti validačné pravidlá pre konkrétny stĺpec.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <param name="columnName">Názov stĺpca</param>
        /// <returns>Filtrované pravidlá pre stĺpec</returns>
        public static IEnumerable<ValidationRule> ForColumn(this IEnumerable<ValidationRule> rules, string columnName)
        {
            return rules.Where(r => r.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vráti validačné pravidlá určitého typu.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <param name="ruleType">Typ pravidla</param>
        /// <returns>Filtrované pravidlá podľa typu</returns>
        public static IEnumerable<ValidationRule> OfType(this IEnumerable<ValidationRule> rules, ValidationType ruleType)
        {
            return rules.Where(r => r.Type == ruleType);
        }

        /// <summary>
        /// Vráti iba required pravidlá.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <returns>Required pravidlá</returns>
        public static IEnumerable<ValidationRule> RequiredOnly(this IEnumerable<ValidationRule> rules)
        {
            return rules.OfType(ValidationType.Required);
        }

        /// <summary>
        /// Vráti iba custom pravidlá.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <returns>Custom pravidlá</returns>
        public static IEnumerable<ValidationRule> CustomOnly(this IEnumerable<ValidationRule> rules)
        {
            return rules.OfType(ValidationType.Custom);
        }

        /// <summary>
        /// Skontroluje či má stĺpec nejaké validačné pravidlá.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <param name="columnName">Názov stĺpca</param>
        /// <returns>True ak má pravidlá</returns>
        public static bool HasRulesFor(this IEnumerable<ValidationRule> rules, string columnName)
        {
            return rules.Any(r => r.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vráti všetky stĺpce ktoré majú validačné pravidlá.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <returns>Zoznam názvov stĺpcov</returns>
        public static List<string> GetValidatedColumns(this IEnumerable<ValidationRule> rules)
        {
            return rules.Select(r => r.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        #endregion

        #region Factory extensions pre ValidationRule

        /// <summary>
        /// Vytvorí ValidationRule pre slovenské PSČ.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre PSČ</returns>
        public static ValidationRule SlovakPostalCode(string columnName, string? errorMessage = null)
        {
            var message = errorMessage ?? "Neplatné slovenské PSČ (formát: 12345)";
            var postalCodeRegex = new Regex(@"^\d{5}$", RegexOptions.Compiled);

            return ValidationRule.Custom(columnName, value =>
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return true; // PSČ nie je povinné ak nie je definované Required

                return postalCodeRegex.IsMatch(value.ToString()!);
            }, message);
        }

        /// <summary>
        /// Vytvorí ValidationRule pre slovenské telefónne číslo.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre telefón</returns>
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
        /// Vytvorí ValidationRule pre URL adresu.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="requireHttps">Vyžadovať HTTPS (default: false)</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre URL</returns>
        public static ValidationRule Url(string columnName, bool requireHttps = false, string? errorMessage = null)
        {
            var message = errorMessage ?? (requireHttps ? "Neplatná HTTPS URL adresa" : "Neplatná URL adresa");

            return ValidationRule.Custom(columnName, value =>
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return true;

                if (Uri.TryCreate(value.ToString(), UriKind.Absolute, out var uri))
                {
                    if (requireHttps)
                        return uri.Scheme == Uri.UriSchemeHttps;

                    return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
                }

                return false;
            }, message);
        }

        /// <summary>
        /// Vytvorí ValidationRule pre dátum v budúcnosti.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre budúci dátum</returns>
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
        /// Vytvorí ValidationRule pre dátum v minulosti.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre minulý dátum</returns>
        public static ValidationRule PastDate(string columnName, string? errorMessage = null)
        {
            var message = errorMessage ?? "Dátum musí byť v minulosti";

            return ValidationRule.Custom(columnName, value =>
            {
                if (value == null) return true;

                if (value is DateTime dateTime)
                    return dateTime.Date < DateTime.Today;

                if (DateTime.TryParse(value.ToString(), out var parsedDate))
                    return parsedDate.Date < DateTime.Today;

                return false;
            }, message);
        }

        /// <summary>
        /// Vytvorí ValidationRule pre vek (založený na dátume narodenia).
        /// </summary>
        /// <param name="columnName">Názov stĺpca s dátumom narodenia</param>
        /// <param name="minAge">Minimálny vek</param>
        /// <param name="maxAge">Maximálny vek</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre vek</returns>
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

        /// <summary>
        /// Vytvorí ValidationRule pre pozitívne číslo.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="allowZero">Povoliť nulu (default: false)</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre pozitívne číslo</returns>
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
        /// Vytvorí ValidationRule pre celočíselné hodnoty.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="errorMessage">Chybová správa (optional)</param>
        /// <returns>ValidationRule pre integer</returns>
        public static ValidationRule Integer(string columnName, string? errorMessage = null)
        {
            var message = errorMessage ?? "Hodnota musí byť celé číslo";

            return ValidationRule.Custom(columnName, value =>
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return true;

                return int.TryParse(value.ToString(), out _);
            }, message);
        }

        /// <summary>
        /// Vytvorí ValidationRule pre regulárny výraz.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="pattern">Regex pattern</param>
        /// <param name="errorMessage">Chybová správa</param>
        /// <param name="options">Regex options (default: None)</param>
        /// <returns>ValidationRule pre regex</returns>
        public static ValidationRule Regex(string columnName, string pattern, string errorMessage, RegexOptions options = RegexOptions.None)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern, options | RegexOptions.Compiled);

            return ValidationRule.Custom(columnName, value =>
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return true;

                return regex.IsMatch(value.ToString()!);
            }, errorMessage);
        }

        #endregion

        #region Validation helpers

        /// <summary>
        /// Vytvorí súhrn validačných pravidiel pre stĺpec.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <param name="columnName">Názov stĺpca</param>
        /// <returns>Textový súhrn pravidiel</returns>
        public static string GetValidationSummary(this IEnumerable<ValidationRule> rules, string columnName)
        {
            var columnRules = rules.ForColumn(columnName).ToList();
            if (!columnRules.Any())
                return "Žiadne validačné pravidlá";

            var summary = new List<string>();

            if (columnRules.Any(r => r.Type == ValidationType.Required))
                summary.Add("Povinné");

            foreach (var rule in columnRules.Where(r => r.Type != ValidationType.Required))
            {
                summary.Add(rule.Type.ToString());
            }

            return string.Join(", ", summary);
        }

        /// <summary>
        /// Skontroluje či má kolekcia konflikty validačných pravidiel.
        /// </summary>
        /// <param name="rules">Kolekcia validačných pravidiel</param>
        /// <returns>Zoznam konfliktov</returns>
        public static List<string> GetValidationConflicts(this IEnumerable<ValidationRule> rules)
        {
            var conflicts = new List<string>();
            var rulesList = rules.ToList();

            // Skontrolovať duplikáty pre každý stĺpec
            var groupedByColumn = rulesList.GroupBy(r => r.ColumnName.ToLowerInvariant());

            foreach (var columnGroup in groupedByColumn)
            {
                var columnRules = columnGroup.ToList();
                var duplicateTypes = columnRules.GroupBy(r => r.Type)
                                              .Where(g => g.Count() > 1)
                                              .Select(g => g.Key);

                foreach (var duplicateType in duplicateTypes)
                {
                    conflicts.Add($"Stĺpec '{columnGroup.Key}' má viacero {duplicateType} pravidiel");
                }
            }

            return conflicts;
        }

        #endregion

        #region Performance extensions

        /// <summary>
        /// Vykoná rýchlu validáciu hodnoty proti pravidlám.
        /// </summary>
        /// <param name="rules">Validačné pravidlá</param>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="value">Hodnota na validáciu</param>
        /// <returns>True ak je validná, inak false</returns>
        public static bool FastValidate(this IEnumerable<ValidationRule> rules, string columnName, object? value)
        {
            var columnRules = rules.ForColumn(columnName);

            foreach (var rule in columnRules)
            {
                if (!rule.Validate(value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Vykoná batch validáciu pre viac hodnôt naraz.
        /// </summary>
        /// <param name="rules">Validačné pravidlá</param>
        /// <param name="columnValues">Slovník stĺpcov a hodnôt</param>
        /// <returns>Slovník výsledkov validácie</returns>
        public static Dictionary<string, bool> BatchValidate(
            this IEnumerable<ValidationRule> rules,
            Dictionary<string, object?> columnValues)
        {
            var results = new Dictionary<string, bool>();

            foreach (var kvp in columnValues)
            {
                results[kvp.Key] = rules.FastValidate(kvp.Key, kvp.Value);
            }

            return results;
        }

        #endregion
    }
}