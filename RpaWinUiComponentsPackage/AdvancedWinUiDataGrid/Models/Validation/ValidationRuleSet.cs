// Models/ValidationRuleSet.cs - ✅ INTERNAL Validation rules management
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// Sada validation rules s management funkciami - INTERNAL
    /// </summary>
    internal class ValidationRuleSet
    {
        #region Properties

        /// <summary>
        /// Názov rule set
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Popis rule set
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Všetky pravidlá v sete
        /// </summary>
        public List<AdvancedValidationRule> Rules { get; private set; } = new();

        /// <summary>
        /// Či je rule set enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Throttling config pre validáciu
        /// </summary>
        public ValidationThrottlingConfig ThrottlingConfig { get; set; } = new();

        /// <summary>
        /// Execution mode
        /// </summary>
        public ValidationExecutionMode ExecutionMode { get; set; } = ValidationExecutionMode.StopOnFirstError;

        /// <summary>
        /// Či má rule set nejaké pravidlá
        /// </summary>
        public bool HasRules => Rules.Count > 0;

        #endregion

        #region Builder Methods

        /// <summary>
        /// Vytvorí nový ValidationRuleSet
        /// </summary>
        public static ValidationRuleSetBuilder Create(string name)
        {
            return new ValidationRuleSetBuilder(name);
        }

        #endregion

        #region Rule Management

        /// <summary>
        /// Pridá pravidlo do setu
        /// </summary>
        public ValidationRuleSet AddRule(AdvancedValidationRule rule)
        {
            if (rule != null && !Rules.Any(r => r.Id == rule.Id))
            {
                Rules.Add(rule);
                Rules.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority desc
            }
            return this;
        }

        /// <summary>
        /// Odstráni pravidlo zo setu
        /// </summary>
        public ValidationRuleSet RemoveRule(string ruleId)
        {
            Rules.RemoveAll(r => r.Id == ruleId);
            return this;
        }

        /// <summary>
        /// Získa pravidlo podľa ID
        /// </summary>
        public AdvancedValidationRule? GetRule(string ruleId)
        {
            return Rules.FirstOrDefault(r => r.Id == ruleId);
        }

        /// <summary>
        /// Získa pravidlá pre konkrétny stĺpec
        /// </summary>
        public List<AdvancedValidationRule> GetRulesForColumn(string columnName)
        {
            return Rules
                .Where(r => r.IsEnabled)
                .Where(r => r.TargetColumns.Contains(columnName) || r.TargetColumns.Count == 0)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }

        /// <summary>
        /// Získa cross-cell pravidlá
        /// </summary>
        public List<AdvancedValidationRule> GetCrossCellRules()
        {
            return Rules
                .Where(r => r.IsEnabled)
                .Where(r => r.CrossCellValidator != null)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }

        /// <summary>
        /// Získa async pravidlá
        /// </summary>
        public List<AdvancedValidationRule> GetAsyncRules()
        {
            return Rules
                .Where(r => r.IsEnabled)
                .Where(r => r.AsyncValidationFunction != null)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }

        #endregion

        #region Validation Execution

        /// <summary>
        /// Vykoná validáciu pre konkrétnu bunku
        /// </summary>
        public ValidationResult ValidateCell(ValidationContext context, List<ValidationContext>? allRowData = null)
        {
            var results = new List<ValidationResult>();
            var applicableRules = GetRulesForColumn(context.ColumnName);

            foreach (var rule in applicableRules)
            {
                var result = rule.Execute(context, allRowData);
                results.Add(result);

                // Stop on first error if configured
                if (ExecutionMode == ValidationExecutionMode.StopOnFirstError && !result.IsValid)
                {
                    break;
                }
            }

            return CombineResults(results);
        }

        /// <summary>
        /// Vykoná async validáciu pre konkrétnu bunku
        /// </summary>
        public async Task<ValidationResult> ValidateCellAsync(ValidationContext context, List<ValidationContext>? allRowData = null)
        {
            var results = new List<ValidationResult>();
            var applicableRules = GetRulesForColumn(context.ColumnName);

            foreach (var rule in applicableRules)
            {
                var result = await rule.ExecuteAsync(context, allRowData);
                results.Add(result);

                // Stop on first error if configured
                if (ExecutionMode == ValidationExecutionMode.StopOnFirstError && !result.IsValid)
                {
                    break;
                }
            }

            return CombineResults(results);
        }

        /// <summary>
        /// Vykoná cross-cell validáciu pre celý riadok
        /// </summary>
        public ValidationResult ValidateRow(List<ValidationContext> rowData, List<ValidationContext> allData)
        {
            var results = new List<ValidationResult>();
            var crossCellRules = GetCrossCellRules();

            foreach (var rule in crossCellRules)
            {
                foreach (var cellContext in rowData)
                {
                    if (rule.TargetColumns.Count == 0 || rule.TargetColumns.Contains(cellContext.ColumnName))
                    {
                        var result = rule.Execute(cellContext, allData);
                        results.Add(result);

                        if (ExecutionMode == ValidationExecutionMode.StopOnFirstError && !result.IsValid)
                        {
                            return result;
                        }
                    }
                }
            }

            return CombineResults(results);
        }

        /// <summary>
        /// Batch validation pre multiple cells
        /// </summary>
        public async Task<Dictionary<string, ValidationResult>> ValidateBatch(
            List<ValidationContext> contexts, 
            List<ValidationContext>? allData = null)
        {
            var results = new Dictionary<string, ValidationResult>();
            var tasks = new List<Task>();

            foreach (var context in contexts)
            {
                var cellKey = $"{context.RowIndex}_{context.ColumnName}";
                
                tasks.Add(Task.Run(async () =>
                {
                    var result = await ValidateCellAsync(context, allData);
                    lock (results)
                    {
                        results[cellKey] = result;
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Kombinuje viacero validation results
        /// </summary>
        private ValidationResult CombineResults(List<ValidationResult> results)
        {
            if (!results.Any())
                return ValidationResult.Success();

            var combinedResult = new ValidationResult
            {
                IsValid = results.All(r => r.IsValid),
                Severity = results.Where(r => !r.IsValid).DefaultIfEmpty().Max(r => r?.Severity) ?? ValidationSeverity.Info
            };

            var errorMessages = results
                .Where(r => !r.IsValid || r.Severity == ValidationSeverity.Warning)
                .Select(r => r.Message)
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();

            if (errorMessages.Any())
            {
                combinedResult.Message = string.Join("; ", errorMessages);
                combinedResult.DetailedMessages.AddRange(errorMessages);
            }

            return combinedResult;
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Získa diagnostic info o rule set
        /// </summary>
        public ValidationRuleSetDiagnostics GetDiagnostics()
        {
            return new ValidationRuleSetDiagnostics
            {
                Name = Name,
                TotalRules = Rules.Count,
                EnabledRules = Rules.Count(r => r.IsEnabled),
                DisabledRules = Rules.Count(r => !r.IsEnabled),
                CrossCellRules = Rules.Count(r => r.CrossCellValidator != null),
                AsyncRules = Rules.Count(r => r.AsyncValidationFunction != null),
                RulesByPriority = Rules
                    .GroupBy(r => r.Priority)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RulesBySeverity = Rules
                    .GroupBy(r => r.Severity)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        #endregion
    }

    /// <summary>
    /// Execution mode pre validation - INTERNAL
    /// </summary>
    internal enum ValidationExecutionMode
    {
        /// <summary>
        /// Zastaví sa na prvej chybe
        /// </summary>
        StopOnFirstError,
        
        /// <summary>
        /// Spracuje všetky pravidlá
        /// </summary>
        ProcessAll,
        
        /// <summary>
        /// Spracuje len warning a info pravidlá po chybe
        /// </summary>
        ContinueWithWarnings
    }

    /// <summary>
    /// Throttling config pre validation - INTERNAL
    /// </summary>
    internal class ValidationThrottlingConfig
    {
        public int DebounceMs { get; set; } = 300;
        public int MaxConcurrentValidations { get; set; } = 5;
        public bool EnableAsyncValidation { get; set; } = true;
        public bool EnableBatchValidation { get; set; } = true;
    }

    /// <summary>
    /// Diagnostické informácie o rule set - INTERNAL
    /// </summary>
    internal class ValidationRuleSetDiagnostics
    {
        public string Name { get; set; } = string.Empty;
        public int TotalRules { get; set; }
        public int EnabledRules { get; set; }
        public int DisabledRules { get; set; }
        public int CrossCellRules { get; set; }
        public int AsyncRules { get; set; }
        public Dictionary<int, int> RulesByPriority { get; set; } = new();
        public Dictionary<ValidationSeverity, int> RulesBySeverity { get; set; } = new();
    }
}