// Models/AdvancedValidationRuleBuilder.cs - ✅ INTERNAL Builder pre advanced validation rules
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Fluent builder pre advanced validation rules - INTERNAL
    /// </summary>
    internal class AdvancedValidationRuleBuilder
    {
        private readonly AdvancedValidationRule _rule;

        internal AdvancedValidationRuleBuilder(string name)
        {
            _rule = new AdvancedValidationRule
            {
                Name = name
            };
        }

        #region Basic Configuration

        /// <summary>
        /// Nastaví popis pravidla
        /// </summary>
        public AdvancedValidationRuleBuilder WithDescription(string description)
        {
            _rule.Description = description;
            return this;
        }

        /// <summary>
        /// Nastaví error message
        /// </summary>
        public AdvancedValidationRuleBuilder WithMessage(string message)
        {
            _rule.ErrorMessage = message;
            return this;
        }

        /// <summary>
        /// Nastaví severity level
        /// </summary>
        public AdvancedValidationRuleBuilder WithSeverity(ValidationSeverity severity)
        {
            _rule.Severity = severity;
            return this;
        }

        /// <summary>
        /// Nastaví priority
        /// </summary>
        public AdvancedValidationRuleBuilder WithPriority(int priority)
        {
            _rule.Priority = priority;
            return this;
        }

        /// <summary>
        /// Určí target columns
        /// </summary>
        public AdvancedValidationRuleBuilder ForColumns(params string[] columns)
        {
            _rule.TargetColumns.AddRange(columns);
            return this;
        }

        /// <summary>
        /// Určí dependency columns
        /// </summary>
        public AdvancedValidationRuleBuilder DependsOn(params string[] columns)
        {
            _rule.DependencyColumns.AddRange(columns);
            return this;
        }

        #endregion

        #region Conditional Logic

        /// <summary>
        /// Pridá When condition
        /// </summary>
        public AdvancedValidationRuleBuilder When(Func<ValidationContext, bool> condition)
        {
            _rule.WhenCondition = condition;
            return this;
        }

        /// <summary>
        /// When s jednoduchým column value check
        /// </summary>
        public AdvancedValidationRuleBuilder WhenColumnEquals(string columnName, object? value)
        {
            return When(ctx => 
            {
                var colValue = ctx.GetValue(columnName);
                return object.Equals(colValue, value);
            });
        }

        /// <summary>
        /// When s string comparison
        /// </summary>
        public AdvancedValidationRuleBuilder WhenColumnContains(string columnName, string value)
        {
            return When(ctx => 
            {
                var colValue = ctx.GetStringValue(columnName);
                return colValue.Contains(value, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>
        /// When má column hodnotu
        /// </summary>
        public AdvancedValidationRuleBuilder WhenColumnHasValue(string columnName)
        {
            return When(ctx => ctx.HasValue(columnName));
        }

        /// <summary>
        /// When je column prázdny
        /// </summary>
        public AdvancedValidationRuleBuilder WhenColumnIsEmpty(string columnName)
        {
            return When(ctx => !ctx.HasValue(columnName));
        }

        #endregion

        #region Validation Logic

        /// <summary>
        /// Pridá main validation function
        /// </summary>
        public AdvancedValidationRuleBuilder Then(Func<ValidationContext, ValidationResult> validator)
        {
            _rule.ValidationFunction = validator;
            return this;
        }

        /// <summary>
        /// Then s simple boolean check
        /// </summary>
        public AdvancedValidationRuleBuilder Then(Func<ValidationContext, bool> validator)
        {
            return Then(ctx => 
            {
                var isValid = validator(ctx);
                return isValid 
                    ? ValidationResult.Success() 
                    : new ValidationResult 
                    { 
                        IsValid = false, 
                        Severity = _rule.Severity,
                        Message = _rule.ErrorMessage 
                    };
            });
        }

        /// <summary>
        /// Then s required field check
        /// </summary>
        public AdvancedValidationRuleBuilder ThenRequired(string columnName)
        {
            return Then(ctx => ctx.HasValue(columnName))
                .WithMessage($"{columnName} is required");
        }

        /// <summary>
        /// Then s multiple required fields
        /// </summary>
        public AdvancedValidationRuleBuilder ThenRequiredFields(params string[] columnNames)
        {
            return Then(ctx => 
            {
                var missingFields = columnNames.Where(col => !ctx.HasValue(col)).ToList();
                if (missingFields.Any())
                {
                    return ValidationResult.Error($"Required fields missing: {string.Join(", ", missingFields)}");
                }
                return ValidationResult.Success();
            });
        }

        /// <summary>
        /// Then s range validation
        /// </summary>
        public AdvancedValidationRuleBuilder ThenInRange(string columnName, IComparable minValue, IComparable maxValue)
        {
            return Then(ctx => 
            {
                var value = ctx.GetValue(columnName);
                if (value is IComparable comparable)
                {
                    return comparable.CompareTo(minValue) >= 0 && comparable.CompareTo(maxValue) <= 0;
                }
                return false;
            })
            .WithMessage($"{columnName} must be between {minValue} and {maxValue}");
        }

        #endregion

        #region Cross-Cell Validation

        /// <summary>
        /// Označí pravidlo ako cross-cell validation
        /// </summary>
        public AdvancedValidationRuleBuilder WithCrossCellValidation()
        {
            // Marker pre cross-cell validation
            return this;
        }

        /// <summary>
        /// Cross-cell validator
        /// </summary>
        public AdvancedValidationRuleBuilder ThenValidateAcrossCells(
            Func<ValidationContext, List<ValidationContext>, ValidationResult> validator)
        {
            _rule.CrossCellValidator = validator;
            return this;
        }

        /// <summary>
        /// Date range validation - začiatok musí byť pred koncom
        /// </summary>
        public AdvancedValidationRuleBuilder ThenStartBeforeEnd(string startColumn, string endColumn)
        {
            return Then(ctx => 
            {
                var startValue = ctx.GetValue(startColumn);
                var endValue = ctx.GetValue(endColumn);

                if (startValue is DateTime start && endValue is DateTime end)
                {
                    return start < end;
                }

                // Try parse as strings
                if (DateTime.TryParse(startValue?.ToString(), out start) && 
                    DateTime.TryParse(endValue?.ToString(), out end))
                {
                    return start < end;
                }

                return true; // Skip validation ak nie sú dates
            })
            .WithMessage($"{startColumn} must be before {endColumn}");
        }

        /// <summary>
        /// Unique value across column
        /// </summary>
        public AdvancedValidationRuleBuilder ThenUniqueInColumn(string columnName)
        {
            return ThenValidateAcrossCells((ctx, allData) => 
            {
                var currentValue = ctx.GetValue(columnName);
                if (currentValue == null) return ValidationResult.Success();

                var duplicates = allData
                    .Where(other => other.RowIndex != ctx.RowIndex)
                    .Where(other => object.Equals(other.GetValue(columnName), currentValue))
                    .ToList();

                if (duplicates.Any())
                {
                    return ValidationResult.Error($"Value '{currentValue}' in {columnName} must be unique");
                }

                return ValidationResult.Success();
            });
        }

        #endregion

        #region Async Validation

        /// <summary>
        /// Označí pravidlo ako async validation
        /// </summary>
        public AdvancedValidationRuleBuilder WithAsyncValidation()
        {
            // Marker pre async validation
            return this;
        }

        /// <summary>
        /// Async validation function
        /// </summary>
        public AdvancedValidationRuleBuilder ThenAsync(Func<ValidationContext, Task<ValidationResult>> asyncValidator)
        {
            _rule.AsyncValidationFunction = asyncValidator;
            return this;
        }

        /// <summary>
        /// Async boolean validation
        /// </summary>
        public AdvancedValidationRuleBuilder ThenAsync(Func<ValidationContext, Task<bool>> asyncValidator)
        {
            return ThenAsync(async ctx => 
            {
                var isValid = await asyncValidator(ctx);
                return isValid 
                    ? ValidationResult.Success() 
                    : new ValidationResult 
                    { 
                        IsValid = false, 
                        Severity = _rule.Severity,
                        Message = _rule.ErrorMessage 
                    };
            });
        }

        #endregion

        #region Common Business Rules

        /// <summary>
        /// Email format validation
        /// </summary>
        public AdvancedValidationRuleBuilder ThenValidEmail(string columnName)
        {
            return Then(ctx => 
            {
                var email = ctx.GetStringValue(columnName);
                if (string.IsNullOrEmpty(email)) return ValidationResult.Success();

                var isValid = System.Text.RegularExpressions.Regex.IsMatch(
                    email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                return isValid 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error($"Invalid email format in {columnName}");
            });
        }

        /// <summary>
        /// Phone number validation
        /// </summary>
        public AdvancedValidationRuleBuilder ThenValidPhoneNumber(string columnName)
        {
            return Then(ctx => 
            {
                var phone = ctx.GetStringValue(columnName);
                if (string.IsNullOrEmpty(phone)) return ValidationResult.Success();

                var isValid = System.Text.RegularExpressions.Regex.IsMatch(
                    phone, @"^[\+]?[1-9][\d]{0,15}$");

                return isValid 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error($"Invalid phone number format in {columnName}");
            });
        }

        #endregion

        #region Build

        /// <summary>
        /// Vytvorí finálne pravidlo
        /// </summary>
        public AdvancedValidationRule Build()
        {
            // Validate builder state
            if (string.IsNullOrEmpty(_rule.Name))
                throw new InvalidOperationException("Rule name is required");

            if (_rule.ValidationFunction == null && 
                _rule.AsyncValidationFunction == null && 
                _rule.CrossCellValidator == null)
            {
                throw new InvalidOperationException("At least one validation function must be specified");
            }

            return _rule;
        }

        /// <summary>
        /// Implicit conversion to AdvancedValidationRule
        /// </summary>
        public static implicit operator AdvancedValidationRule(AdvancedValidationRuleBuilder builder)
        {
            return builder.Build();
        }

        #endregion
    }
}