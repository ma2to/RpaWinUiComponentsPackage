// Models/AdvancedValidationRule.cs - ✅ INTERNAL Advanced validation rules
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// Advanced validation rule s cross-cell dependencies a business logic - INTERNAL
    /// </summary>
    internal class AdvancedValidationRule
    {
        #region Properties

        /// <summary>
        /// Unique identifier pre pravidlo
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
        /// Severity level validácie
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

        #region Validation Functions

        /// <summary>
        /// Condition function - kedy sa má validácia spustiť
        /// </summary>
        public Func<ValidationContext, bool>? WhenCondition { get; set; }

        /// <summary>
        /// Main validation function
        /// </summary>
        public Func<ValidationContext, ValidationResult>? ValidationFunction { get; set; }

        /// <summary>
        /// Async validation function
        /// </summary>
        public Func<ValidationContext, Task<ValidationResult>>? AsyncValidationFunction { get; set; }

        /// <summary>
        /// Cross-cell dependency validator
        /// </summary>
        public Func<ValidationContext, List<ValidationContext>, ValidationResult>? CrossCellValidator { get; set; }

        #endregion

        #region Builder Methods

        /// <summary>
        /// Vytvorí nové pravidlo s podmienkou When
        /// </summary>
        public static AdvancedValidationRuleBuilder Create(string name)
        {
            return new AdvancedValidationRuleBuilder(name);
        }

        /// <summary>
        /// Vytvorí cross-cell dependency pravidlo
        /// </summary>
        public static AdvancedValidationRuleBuilder CreateCrossCellRule(string name)
        {
            return new AdvancedValidationRuleBuilder(name)
                .WithCrossCellValidation();
        }

        /// <summary>
        /// Vytvorí async validation pravidlo
        /// </summary>
        public static AdvancedValidationRuleBuilder CreateAsyncRule(string name)
        {
            return new AdvancedValidationRuleBuilder(name)
                .WithAsyncValidation();
        }

        #endregion

        #region Execution Methods

        /// <summary>
        /// Vykoná validáciu synchronne
        /// </summary>
        public ValidationResult Execute(ValidationContext context, List<ValidationContext>? allRowData = null)
        {
            try
            {
                if (!IsEnabled)
                    return ValidationResult.Success();

                // Check condition first
                if (WhenCondition != null && !WhenCondition(context))
                    return ValidationResult.Success();

                // Execute appropriate validation
                if (CrossCellValidator != null && allRowData != null)
                {
                    return CrossCellValidator(context, allRowData);
                }

                if (ValidationFunction != null)
                {
                    return ValidationFunction(context);
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"Validation error in rule '{Name}': {ex.Message}");
            }
        }

        /// <summary>
        /// Vykoná validáciu async
        /// </summary>
        public async Task<ValidationResult> ExecuteAsync(ValidationContext context, List<ValidationContext>? allRowData = null)
        {
            try
            {
                if (!IsEnabled)
                    return ValidationResult.Success();

                // Check condition first
                if (WhenCondition != null && !WhenCondition(context))
                    return ValidationResult.Success();

                // Execute async validation if available
                if (AsyncValidationFunction != null)
                {
                    return await AsyncValidationFunction(context);
                }

                // Fallback to sync validation
                return Execute(context, allRowData);
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"Async validation error in rule '{Name}': {ex.Message}");
            }
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie o pravidle
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"Rule '{Name}' (ID: {Id}) - Priority: {Priority}, " +
                   $"Enabled: {IsEnabled}, Severity: {Severity}, " +
                   $"Targets: [{string.Join(", ", TargetColumns)}], " +
                   $"Dependencies: [{string.Join(", ", DependencyColumns)}]";
        }

        #endregion
    }

    /// <summary>
    /// Validation severity levels - INTERNAL
    /// </summary>
    public enum ValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }

    /// <summary>
    /// Validation result - INTERNAL
    /// </summary>
    internal class ValidationResult
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
        public static ValidationResult Failure(string message) => new() 
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
    }

    /// <summary>
    /// Validation context s prístupom k cell values a row data - INTERNAL
    /// </summary>
    internal class ValidationContext
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public object? CurrentValue { get; set; }
        public object? OriginalValue { get; set; }
        
        /// <summary>
        /// Alias pre CurrentValue pre backward compatibility
        /// </summary>
        public object? Value => CurrentValue;
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