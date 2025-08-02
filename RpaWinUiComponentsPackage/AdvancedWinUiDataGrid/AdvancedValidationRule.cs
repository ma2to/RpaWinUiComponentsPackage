// AdvancedValidationRule.cs - ✅ PUBLIC API pre advanced validation
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// PUBLIC API pre definovanie advanced validation rules - môže byť použité v aplikácii
    /// </summary>
    public class AdvancedValidationRule
    {
        #region Public Properties

        /// <summary>
        /// Unique identifier pravidla
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        /// Názov pravidla
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Popis pravidla
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Error message pri failed validation
        /// </summary>
        public string ErrorMessage { get; set; } = "Validation failed";

        /// <summary>
        /// Severity level (Info, Warning, Error, Critical)
        /// </summary>
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;

        /// <summary>
        /// Target columns pre validation
        /// </summary>
        public List<string> TargetColumns { get; set; } = new();

        /// <summary>
        /// Dependency columns - stĺpce potrebné pre validation
        /// </summary>
        public List<string> DependencyColumns { get; set; } = new();

        /// <summary>
        /// Či je pravidlo enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Priority pravidla (vyšší = skôr sa spracuje)
        /// </summary>
        public int Priority { get; set; } = 100;

        #endregion

        #region Public Validation Functions

        /// <summary>
        /// Condition function - kedy sa má validácia spustiť
        /// Funkcia dostane ValidationContext s prístupom k row data
        /// </summary>
        public Func<ValidationContext, bool>? WhenCondition { get; set; }

        /// <summary>
        /// Main validation function - dostane kontext a vráti bool
        /// </summary>
        public Func<ValidationContext, bool>? ValidationFunction { get; set; }

        /// <summary>
        /// Advanced validation function - dostane kontext a vráti ValidationResult
        /// </summary>
        public Func<ValidationContext, ValidationResult>? AdvancedValidationFunction { get; set; }

        /// <summary>
        /// Cross-cell dependency validator - dostane kontext bunky a všetky row data
        /// </summary>
        public Func<ValidationContext, List<ValidationContext>, ValidationResult>? CrossCellValidator { get; set; }

        /// <summary>
        /// Async validation function
        /// </summary>
        public Func<ValidationContext, Task<ValidationResult>>? AsyncValidationFunction { get; set; }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí required field validation
        /// </summary>
        public static AdvancedValidationRule Required(string columnName, string? customMessage = null)
        {
            return new AdvancedValidationRule
            {
                Name = $"Required_{columnName}",
                TargetColumns = new List<string> { columnName },
                ErrorMessage = customMessage ?? $"{columnName} is required",
                ValidationFunction = ctx => !string.IsNullOrWhiteSpace(ctx.GetStringValue(columnName)),
                Severity = ValidationSeverity.Error
            };
        }

        /// <summary>
        /// Vytvorí email validation
        /// </summary>
        public static AdvancedValidationRule EmailFormat(string columnName, string? customMessage = null)
        {
            return new AdvancedValidationRule
            {
                Name = $"Email_{columnName}",
                TargetColumns = new List<string> { columnName },
                ErrorMessage = customMessage ?? $"Invalid email format in {columnName}",
                ValidationFunction = ctx =>
                {
                    var email = ctx.GetStringValue(columnName);
                    if (string.IsNullOrEmpty(email)) return true; // Optional field
                    return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                },
                Severity = ValidationSeverity.Error
            };
        }

        /// <summary>
        /// Vytvorí range validation pre číselné hodnoty
        /// </summary>
        public static AdvancedValidationRule NumberRange(string columnName, decimal minValue, decimal maxValue, string? customMessage = null)
        {
            return new AdvancedValidationRule
            {
                Name = $"Range_{columnName}",
                TargetColumns = new List<string> { columnName },
                ErrorMessage = customMessage ?? $"{columnName} must be between {minValue} and {maxValue}",
                ValidationFunction = ctx =>
                {
                    var valueStr = ctx.GetStringValue(columnName);
                    if (string.IsNullOrEmpty(valueStr)) return true; // Optional field
                    
                    if (decimal.TryParse(valueStr, out var value))
                    {
                        return value >= minValue && value <= maxValue;
                    }
                    return false;
                },
                Severity = ValidationSeverity.Error
            };
        }

        /// <summary>
        /// Vytvorí conditional required validation - "ak je X = Y, tak Z musí byť vyplnené"
        /// </summary>
        public static AdvancedValidationRule ConditionalRequired(
            string targetColumn, 
            string conditionColumn, 
            object conditionValue, 
            string? customMessage = null)
        {
            return new AdvancedValidationRule
            {
                Name = $"ConditionalRequired_{targetColumn}",
                TargetColumns = new List<string> { targetColumn },
                DependencyColumns = new List<string> { conditionColumn },
                ErrorMessage = customMessage ?? $"{targetColumn} is required when {conditionColumn} is '{conditionValue}'",
                WhenCondition = ctx => object.Equals(ctx.GetValue(conditionColumn), conditionValue),
                ValidationFunction = ctx => ctx.HasValue(targetColumn),
                Severity = ValidationSeverity.Error
            };
        }

        /// <summary>
        /// Vytvorí date range validation - začiatok musí byť pred koncom
        /// </summary>
        public static AdvancedValidationRule DateRange(
            string startColumn, 
            string endColumn, 
            string? customMessage = null)
        {
            return new AdvancedValidationRule
            {
                Name = $"DateRange_{startColumn}_{endColumn}",
                TargetColumns = new List<string> { startColumn, endColumn },
                DependencyColumns = new List<string> { startColumn, endColumn },
                ErrorMessage = customMessage ?? $"{startColumn} must be before {endColumn}",
                ValidationFunction = ctx =>
                {
                    var startValue = ctx.GetValue(startColumn);
                    var endValue = ctx.GetValue(endColumn);

                    if (startValue == null || endValue == null) return true; // Optional

                    if (DateTime.TryParse(startValue.ToString(), out var start) && 
                        DateTime.TryParse(endValue.ToString(), out var end))
                    {
                        return start <= end;
                    }
                    return true;
                },
                Severity = ValidationSeverity.Error
            };
        }

        /// <summary>
        /// Vytvorí unique validation - hodnota musí byť unique v stĺpci
        /// </summary>
        public static AdvancedValidationRule Unique(string columnName, string? customMessage = null)
        {
            return new AdvancedValidationRule
            {
                Name = $"Unique_{columnName}",
                TargetColumns = new List<string> { columnName },
                ErrorMessage = customMessage ?? $"Value in {columnName} must be unique",
                CrossCellValidator = (ctx, allData) =>
                {
                    var currentValue = ctx.GetValue(columnName);
                    if (currentValue == null) return ValidationResult.Success();

                    var duplicates = allData
                        .Where(other => other.RowIndex != ctx.RowIndex)
                        .Where(other => object.Equals(other.GetValue(columnName), currentValue))
                        .ToList();

                    return duplicates.Any() 
                        ? ValidationResult.Error($"Value '{currentValue}' in {columnName} must be unique")
                        : ValidationResult.Success();
                },
                Severity = ValidationSeverity.Error
            };
        }

        /// <summary>
        /// Vytvorí custom validation rule s vlastnou funkciou
        /// </summary>
        public static AdvancedValidationRule Custom(
            string name, 
            string[] targetColumns, 
            Func<ValidationContext, bool> validationFunction,
            string errorMessage,
            ValidationSeverity severity = ValidationSeverity.Error)
        {
            return new AdvancedValidationRule
            {
                Name = name,
                TargetColumns = new List<string>(targetColumns),
                ValidationFunction = validationFunction,
                ErrorMessage = errorMessage,
                Severity = severity
            };
        }

        /// <summary>
        /// Vytvorí cross-cell custom validation rule
        /// </summary>
        public static AdvancedValidationRule CrossCellCustom(
            string name,
            string[] targetColumns,
            string[] dependencyColumns,
            Func<ValidationContext, List<ValidationContext>, ValidationResult> validator,
            ValidationSeverity severity = ValidationSeverity.Error)
        {
            return new AdvancedValidationRule
            {
                Name = name,
                TargetColumns = new List<string>(targetColumns),
                DependencyColumns = new List<string>(dependencyColumns),
                CrossCellValidator = validator,
                Severity = severity
            };
        }

        #endregion
    }

    /// <summary>
    /// Validation severity levels - PUBLIC
    /// </summary>
    public enum ValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }

    /// <summary>
    /// Validation result - PUBLIC
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> DetailedMessages { get; set; } = new();
        public Dictionary<string, object?> Metadata { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Error(string message) => new() 
        { 
            IsValid = false, 
            Severity = ValidationSeverity.Error, 
            Message = message 
        };
        public static ValidationResult Warning(string message) => new() 
        { 
            IsValid = true, 
            Severity = ValidationSeverity.Warning, 
            Message = message 
        };
        public static ValidationResult Info(string message) => new() 
        { 
            IsValid = true, 
            Severity = ValidationSeverity.Info, 
            Message = message 
        };
    }

    /// <summary>
    /// Validation context s prístupom k cell values a row data - PUBLIC
    /// </summary>
    public class ValidationContext
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public object? CurrentValue { get; set; }
        public object? OriginalValue { get; set; }
        public Dictionary<string, object?> RowData { get; set; } = new();
        public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Získa hodnotu z iného stĺpca v tom istom riadku
        /// </summary>
        public object? GetValue(string columnName)
        {
            return RowData.TryGetValue(columnName, out var value) ? value : null;
        }

        /// <summary>
        /// Získa hodnotu ako string
        /// </summary>
        public string GetStringValue(string columnName)
        {
            return GetValue(columnName)?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Skontroluje či má stĺpec hodnotu
        /// </summary>
        public bool HasValue(string columnName)
        {
            var value = GetValue(columnName);
            return value != null && !string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}