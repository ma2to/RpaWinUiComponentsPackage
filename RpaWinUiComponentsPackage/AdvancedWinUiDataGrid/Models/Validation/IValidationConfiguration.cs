// Models/Validation/IValidationConfiguration.cs - ✅ PUBLIC API pre validation konfiguráciu
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// ✅ PUBLIC API interface pre validation konfiguráciu
    /// </summary>
    public interface IValidationConfiguration
    {
        /// <summary>
        /// Názov konfigurácie
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Popis konfigurácie
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Či je konfigurácia povolená
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Počet validation rules
        /// </summary>
        int RulesCount { get; }
    }

    /// <summary>
    /// ✅ PUBLIC API builder pre validation konfiguráciu
    /// </summary>
    public class ValidationConfigurationBuilder
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isEnabled = true;
        private readonly List<Action<ValidationRuleSet>> _ruleConfigurators = new();

        /// <summary>
        /// Nastaví názov konfigurácie
        /// </summary>
        public ValidationConfigurationBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Nastaví popis konfigurácie
        /// </summary>
        public ValidationConfigurationBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        /// <summary>
        /// Nastaví či je konfigurácia povolená
        /// </summary>
        public ValidationConfigurationBuilder WithEnabled(bool enabled)
        {
            _isEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Pridá required field validáciu
        /// </summary>
        public ValidationConfigurationBuilder AddRequiredField(string columnName, string errorMessage = "")
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"required_{columnName}",
                    TargetColumns = new List<string> { columnName },
                    ErrorMessage = string.IsNullOrEmpty(errorMessage) ? $"{columnName} is required" : errorMessage,
                    Severity = ValidationSeverity.Error,
                    ValidationFunction = context => 
                    {
                        var value = context.Value;
                        return !string.IsNullOrWhiteSpace(value?.ToString()) 
                            ? ValidationResult.Success() 
                            : ValidationResult.Failure($"{context.ColumnName} is required");
                    },
                    Priority = 100
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá range validáciu
        /// </summary>
        public ValidationConfigurationBuilder AddRange(string columnName, double min, double max, string errorMessage = "")
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"range_{columnName}",
                    TargetColumns = new List<string> { columnName },
                    ErrorMessage = string.IsNullOrEmpty(errorMessage) ? 
                        $"{columnName} must be between {min} and {max}" : errorMessage,
                    Severity = ValidationSeverity.Error,
                    ValidationFunction = context =>
                    {
                        var value = context.Value;
                        if (double.TryParse(value?.ToString(), out var numValue))
                        {
                            return numValue >= min && numValue <= max
                                ? ValidationResult.Success()
                                : ValidationResult.Failure($"{context.ColumnName} must be between {min} and {max}");
                        }
                        return ValidationResult.Success(); // Skip validation for non-numeric values
                    },
                    Priority = 90
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá regex validáciu
        /// </summary>
        public ValidationConfigurationBuilder AddRegex(string columnName, string pattern, string errorMessage = "")
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"regex_{columnName}",
                    TargetColumns = new List<string> { columnName },
                    ErrorMessage = string.IsNullOrEmpty(errorMessage) ? 
                        $"{columnName} format is invalid" : errorMessage,
                    Severity = ValidationSeverity.Error,
                    ValidationFunction = context =>
                    {
                        var value = context.Value;
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                            return ValidationResult.Success(); // Skip validation for empty values
                        
                        return System.Text.RegularExpressions.Regex.IsMatch(value.ToString()!, pattern)
                            ? ValidationResult.Success()
                            : ValidationResult.Failure($"{context.ColumnName} format is invalid");
                    },
                    Priority = 80
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá basic custom validation function (simple bool validation)
        /// </summary>
        public ValidationConfigurationBuilder AddCustomValidation(string columnName, Func<object?, bool> validationFunction, string errorMessage, int priority = 70)
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"custom_{columnName}_{Guid.NewGuid():N}",
                    TargetColumns = new List<string> { columnName },
                    ErrorMessage = errorMessage,
                    Severity = ValidationSeverity.Error,
                    ValidationFunction = context =>
                    {
                        return validationFunction(context.Value)
                            ? ValidationResult.Success()
                            : ValidationResult.Failure(errorMessage);
                    },
                    Priority = priority
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá cross-cell validation v rámci riadku (stĺpec podmienený iným stĺpcom v tom istom riadku)
        /// </summary>
        public ValidationConfigurationBuilder AddRowValidation(
            string targetColumn, 
            Func<Dictionary<string, object?>, bool> rowValidator, 
            string errorMessage, 
            int priority = 60)
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"row_validation_{targetColumn}_{Guid.NewGuid():N}",
                    TargetColumns = new List<string> { targetColumn },
                    ErrorMessage = errorMessage,
                    Severity = ValidationSeverity.Error,
                    CrossCellValidator = (context, allRowData) =>
                    {
                        // Reconstruct current row data from context
                        var currentRowData = new Dictionary<string, object?>();
                        if (context.RowIndex < allRowData.Count)
                        {
                            var contextRow = allRowData[context.RowIndex];
                            currentRowData = contextRow.RowData ?? new Dictionary<string, object?>();
                        }
                        
                        return rowValidator(currentRowData)
                            ? ValidationResult.Success()
                            : ValidationResult.Failure(errorMessage);
                    },
                    Priority = priority
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá unique constraint pre stĺpec (hodnota musí byť jedinečná cez všetky riadky)
        /// </summary>
        public ValidationConfigurationBuilder AddUniqueConstraint(
            string columnName, 
            string errorMessage = "", 
            int priority = 80)
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"unique_{columnName}_{Guid.NewGuid():N}",
                    TargetColumns = new List<string> { columnName },
                    ErrorMessage = string.IsNullOrEmpty(errorMessage) ? 
                        $"{columnName} must be unique" : errorMessage,
                    Severity = ValidationSeverity.Error,
                    CrossCellValidator = (context, allRowData) =>
                    {
                        var currentValue = context.Value;
                        if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
                            return ValidationResult.Success(); // Skip validation for empty values

                        // Check for duplicates in other rows
                        for (int i = 0; i < allRowData.Count; i++)
                        {
                            if (i == context.RowIndex) continue; // Skip current row
                            
                            var otherRowData = allRowData[i].RowData;
                            if (otherRowData?.TryGetValue(columnName, out var otherValue) == true)
                            {
                                if (Equals(currentValue, otherValue))
                                {
                                    return ValidationResult.Failure(errorMessage);
                                }
                            }
                        }

                        return ValidationResult.Success();
                    },
                    Priority = priority
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá composite unique constraint (kombinácia stĺpcov musí byť jedinečná)
        /// </summary>
        public ValidationConfigurationBuilder AddCompositeUniqueConstraint(
            List<string> columnNames, 
            string errorMessage = "", 
            int priority = 75)
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"composite_unique_{string.Join("_", columnNames)}_{Guid.NewGuid():N}",
                    TargetColumns = columnNames,
                    ErrorMessage = string.IsNullOrEmpty(errorMessage) ? 
                        $"Combination of {string.Join(", ", columnNames)} must be unique" : errorMessage,
                    Severity = ValidationSeverity.Error,
                    CrossCellValidator = (context, allRowData) =>
                    {
                        // Get current row values for all columns in constraint
                        var currentRowData = allRowData[context.RowIndex].RowData;
                        var currentComposite = columnNames
                            .Select(col => currentRowData?.TryGetValue(col, out var val) == true ? val : null)
                            .Cast<object?>()
                            .ToList();

                        // Skip validation if any composite value is null/empty
                        if (currentComposite.Any(v => v == null || string.IsNullOrWhiteSpace(v?.ToString())))
                            return ValidationResult.Success();

                        // Check for duplicates in other rows
                        for (int i = 0; i < allRowData.Count; i++)
                        {
                            if (i == context.RowIndex) continue; // Skip current row
                            
                            var otherRowData = allRowData[i].RowData;
                            var otherComposite = columnNames
                                .Select(col => otherRowData?.TryGetValue(col, out var val) == true ? val : null)
                                .Cast<object?>()
                                .ToList();

                            if (CompositeValuesEqual(currentComposite, otherComposite))
                            {
                                return ValidationResult.Failure(errorMessage);
                            }
                        }

                        return ValidationResult.Success();
                    },
                    Priority = priority
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Pridá custom cross-row validation (hodnota podmienená inými riadkami)
        /// </summary>
        public ValidationConfigurationBuilder AddCrossRowCustomValidation(
            string targetColumn,
            Func<Dictionary<string, object?>, List<Dictionary<string, object?>>, bool> validator,
            string errorMessage,
            int priority = 50)
        {
            _ruleConfigurators.Add(ruleSet =>
            {
                var rule = new AdvancedValidationRule
                {
                    Id = $"cross_row_custom_{targetColumn}_{Guid.NewGuid():N}",
                    TargetColumns = new List<string> { targetColumn },
                    ErrorMessage = errorMessage,
                    Severity = ValidationSeverity.Error,
                    CrossCellValidator = (context, allRowData) =>
                    {
                        var currentRowData = allRowData[context.RowIndex].RowData ?? new Dictionary<string, object?>();
                        var allRowsData = allRowData.Select(ctx => ctx.RowData ?? new Dictionary<string, object?>()).ToList();
                        
                        return validator(currentRowData, allRowsData)
                            ? ValidationResult.Success()
                            : ValidationResult.Failure(errorMessage);
                    },
                    Priority = priority
                };
                ruleSet.AddRule(rule);
            });
            return this;
        }

        /// <summary>
        /// Helper method pre porovnanie composite values
        /// </summary>
        private bool CompositeValuesEqual(List<object?> values1, List<object?> values2)
        {
            if (values1.Count != values2.Count) return false;

            for (int i = 0; i < values1.Count; i++)
            {
                if (!Equals(values1[i], values2[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// ✅ INTERNAL: Vytvorí ValidationRuleSet z konfigurácie
        /// </summary>
        internal ValidationRuleSet BuildInternal()
        {
            var ruleSet = new ValidationRuleSet
            {
                Name = _name,
                Description = _description,
                IsEnabled = _isEnabled
            };

            foreach (var configurator in _ruleConfigurators)
            {
                configurator(ruleSet);
            }

            return ruleSet;
        }

        /// <summary>
        /// ✅ PUBLIC: Vytvorí validation konfiguráciu
        /// </summary>
        public IValidationConfiguration Build()
        {
            var internalRuleSet = BuildInternal();
            return new PublicValidationConfiguration(internalRuleSet);
        }
    }

    /// <summary>
    /// ✅ PUBLIC API wrapper pre ValidationRuleSet
    /// </summary>
    internal class PublicValidationConfiguration : IValidationConfiguration
    {
        private readonly ValidationRuleSet _internalRuleSet;

        internal PublicValidationConfiguration(ValidationRuleSet internalRuleSet)
        {
            _internalRuleSet = internalRuleSet;
        }

        public string Name => _internalRuleSet.Name;
        public string Description => _internalRuleSet.Description;
        public bool IsEnabled => _internalRuleSet.IsEnabled;
        public int RulesCount => _internalRuleSet.Rules.Count;

        /// <summary>
        /// ✅ INTERNAL: Získa internal ValidationRuleSet
        /// </summary>
        internal ValidationRuleSet GetInternalRuleSet() => _internalRuleSet;
    }

    /// <summary>
    /// ✅ PUBLIC API factory pre validation konfigurácie
    /// </summary>
    public static class ValidationConfigurationFactory
    {
        /// <summary>
        /// Vytvorí nový validation configuration builder
        /// </summary>
        public static ValidationConfigurationBuilder Create(string name)
        {
            return new ValidationConfigurationBuilder().WithName(name);
        }

        /// <summary>
        /// Vytvorí základnú validation konfiguráciu
        /// </summary>
        public static IValidationConfiguration CreateBasic(string name = "BasicValidation")
        {
            return Create(name)
                .WithDescription("Basic validation rules")
                .Build();
        }

        /// <summary>
        /// Vytvorí validation konfiguráciu pre formuláre
        /// </summary>
        public static IValidationConfiguration CreateFormValidation(string name = "FormValidation")
        {
            return Create(name)
                .WithDescription("Standard form validation rules")
                .Build();
        }
    }
}