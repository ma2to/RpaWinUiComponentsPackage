// ValidationRuleSet.cs - ✅ PUBLIC API pre validation rule management
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// PUBLIC API pre management validation rules - môže byť použité v aplikácii
    /// </summary>
    public class ValidationRuleSet
    {
        #region Public Properties

        /// <summary>
        /// Názov rule set
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Popis rule set
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Všetky pravidlá v sete (READ-ONLY)
        /// </summary>
        public IReadOnlyList<AdvancedValidationRule> Rules => _rules.AsReadOnly();

        /// <summary>
        /// Či je rule set enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Execution mode
        /// </summary>
        public ValidationExecutionMode ExecutionMode { get; set; } = ValidationExecutionMode.StopOnFirstError;

        /// <summary>
        /// Debounce time v milisekundách pre real-time validation
        /// </summary>
        public int DebounceMs { get; set; } = 300;

        /// <summary>
        /// Maximum concurrent async validations
        /// </summary>
        public int MaxConcurrentValidations { get; set; } = 5;

        /// <summary>
        /// Či je povolená async validation
        /// </summary>
        public bool EnableAsyncValidation { get; set; } = true;

        #endregion

        #region Private Fields

        private readonly List<AdvancedValidationRule> _rules = new();

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí nový ValidationRuleSet
        /// </summary>
        public static ValidationRuleSet Create(string name)
        {
            return new ValidationRuleSet
            {
                Name = name
            };
        }

        /// <summary>
        /// Vytvorí basic rule set s required fields
        /// </summary>
        public static ValidationRuleSet CreateBasicRuleSet(string name, params string[] requiredFields)
        {
            var ruleSet = new ValidationRuleSet { Name = name };
            
            foreach (var field in requiredFields)
            {
                ruleSet.AddRule(AdvancedValidationRule.Required(field));
            }
            
            return ruleSet;
        }

        /// <summary>
        /// Vytvorí employee validation rule set
        /// </summary>
        public static ValidationRuleSet CreateEmployeeRuleSet()
        {
            return new ValidationRuleSet
            {
                Name = "EmployeeValidation",
                Description = "Validation rules for employee data",
                ExecutionMode = ValidationExecutionMode.ProcessAll
            }
            .AddRule(AdvancedValidationRule.Required("FirstName", "First name is required"))
            .AddRule(AdvancedValidationRule.Required("LastName", "Last name is required"))
            .AddRule(AdvancedValidationRule.Required("Email", "Email is required"))
            .AddRule(AdvancedValidationRule.EmailFormat("Email"))
            .AddRule(AdvancedValidationRule.Unique("Email", "Email must be unique"))
            .AddRule(AdvancedValidationRule.NumberRange("Salary", 0, 999999, "Salary must be between 0 and 999,999"))
            .AddRule(AdvancedValidationRule.ConditionalRequired("ManagerId", "EmployeeType", "Employee", 
                "Manager ID is required for employees"))
            .AddRule(AdvancedValidationRule.CrossCellCustom(
                "ManagerSalaryCheck",
                new[] { "Salary" },
                new[] { "ManagerId", "EmployeeType" },
                (ctx, allData) =>
                {
                    // Employee salary nemôže byť vyšší ako manager salary
                    if (ctx.GetStringValue("EmployeeType") != "Employee" || !ctx.HasValue("ManagerId"))
                        return ValidationResult.Success();

                    var employeeSalary = Convert.ToDecimal(ctx.CurrentValue ?? 0);
                    var managerId = ctx.GetStringValue("ManagerId");
                    
                    var manager = allData.FirstOrDefault(d => 
                        d.GetStringValue("EmployeeId") == managerId);
                    
                    if (manager != null)
                    {
                        var managerSalary = Convert.ToDecimal(manager.GetValue("Salary") ?? 0);
                        if (employeeSalary > managerSalary)
                        {
                            return ValidationResult.Warning("Employee salary should not exceed manager's salary");
                        }
                    }
                    
                    return ValidationResult.Success();
                },
                ValidationSeverity.Warning));
        }

        /// <summary>
        /// Vytvorí project validation rule set
        /// </summary>
        public static ValidationRuleSet CreateProjectRuleSet()
        {
            return new ValidationRuleSet
            {
                Name = "ProjectValidation",
                Description = "Validation rules for project data",
                ExecutionMode = ValidationExecutionMode.ProcessAll
            }
            .AddRule(AdvancedValidationRule.Required("ProjectName", "Project name is required"))
            .AddRule(AdvancedValidationRule.Required("StartDate", "Start date is required"))
            .AddRule(AdvancedValidationRule.DateRange("StartDate", "EndDate", "Start date must be before end date"))
            .AddRule(AdvancedValidationRule.Unique("ProjectCode", "Project code must be unique"))
            .AddRule(AdvancedValidationRule.ConditionalRequired("EndDate", "Status", "Completed", 
                "End date is required for completed projects"))
            .AddRule(AdvancedValidationRule.ConditionalRequired("ActualHours", "Status", "Completed",
                "Actual hours are required for completed projects"));
        }

        #endregion

        #region Rule Management

        /// <summary>
        /// Pridá pravidlo do setu
        /// </summary>
        public ValidationRuleSet AddRule(AdvancedValidationRule rule)
        {
            if (rule != null && !_rules.Any(r => r.Id == rule.Id))
            {
                _rules.Add(rule);
                _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority desc
            }
            return this;
        }

        /// <summary>
        /// Pridá viacero pravidiel
        /// </summary>
        public ValidationRuleSet AddRules(params AdvancedValidationRule[] rules)
        {
            foreach (var rule in rules)
            {
                AddRule(rule);
            }
            return this;
        }

        /// <summary>
        /// Pridá pravidlá z kolekcie
        /// </summary>
        public ValidationRuleSet AddRules(IEnumerable<AdvancedValidationRule> rules)
        {
            foreach (var rule in rules)
            {
                AddRule(rule);
            }
            return this;
        }

        /// <summary>
        /// Odstráni pravidlo zo setu
        /// </summary>
        public ValidationRuleSet RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.Id == ruleId);
            return this;
        }

        /// <summary>
        /// Získa pravidlo podľa ID
        /// </summary>
        public AdvancedValidationRule? GetRule(string ruleId)
        {
            return _rules.FirstOrDefault(r => r.Id == ruleId);
        }

        /// <summary>
        /// Získa pravidlá pre konkrétny stĺpec
        /// </summary>
        public List<AdvancedValidationRule> GetRulesForColumn(string columnName)
        {
            return _rules
                .Where(r => r.IsEnabled)
                .Where(r => r.TargetColumns.Contains(columnName) || r.TargetColumns.Count == 0)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }

        /// <summary>
        /// Nastaví execution mode
        /// </summary>
        public ValidationRuleSet WithExecutionMode(ValidationExecutionMode mode)
        {
            ExecutionMode = mode;
            return this;
        }

        /// <summary>
        /// Nastaví throttling config
        /// </summary>
        public ValidationRuleSet WithThrottling(int debounceMs, int maxConcurrent = 5)
        {
            DebounceMs = debounceMs;
            MaxConcurrentValidations = maxConcurrent;
            return this;
        }

        /// <summary>
        /// Vypne async validation
        /// </summary>
        public ValidationRuleSet DisableAsyncValidation()
        {
            EnableAsyncValidation = false;
            return this;
        }

        #endregion

        #region Quick Rule Helpers

        /// <summary>
        /// Pridá required field validation
        /// </summary>
        public ValidationRuleSet AddRequired(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                AddRule(AdvancedValidationRule.Required(columnName));
            }
            return this;
        }

        /// <summary>
        /// Pridá email validation
        /// </summary>
        public ValidationRuleSet AddEmailValidation(string columnName, bool required = false)
        {
            AddRule(AdvancedValidationRule.EmailFormat(columnName));
            if (required)
            {
                AddRule(AdvancedValidationRule.Required(columnName));
            }
            return this;
        }

        /// <summary>
        /// Pridá unique validation
        /// </summary>
        public ValidationRuleSet AddUniqueValidation(string columnName)
        {
            AddRule(AdvancedValidationRule.Unique(columnName));
            return this;
        }

        /// <summary>
        /// Pridá range validation
        /// </summary>
        public ValidationRuleSet AddRangeValidation(string columnName, decimal minValue, decimal maxValue)
        {
            AddRule(AdvancedValidationRule.NumberRange(columnName, minValue, maxValue));
            return this;
        }

        /// <summary>
        /// Pridá conditional required validation
        /// </summary>
        public ValidationRuleSet AddConditionalRequired(string targetColumn, string conditionColumn, object conditionValue)
        {
            AddRule(AdvancedValidationRule.ConditionalRequired(targetColumn, conditionColumn, conditionValue));
            return this;
        }

        /// <summary>
        /// Pridá date range validation
        /// </summary>
        public ValidationRuleSet AddDateRangeValidation(string startColumn, string endColumn)
        {
            AddRule(AdvancedValidationRule.DateRange(startColumn, endColumn));
            return this;
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Získa diagnostic info o rule set
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var totalRules = _rules.Count;
            var enabledRules = _rules.Count(r => r.IsEnabled);
            var crossCellRules = _rules.Count(r => r.CrossCellValidator != null);
            var asyncRules = _rules.Count(r => r.AsyncValidationFunction != null);

            return $"RuleSet '{Name}' - Total: {totalRules}, Enabled: {enabledRules}, " +
                   $"CrossCell: {crossCellRules}, Async: {asyncRules}, Mode: {ExecutionMode}";
        }

        #endregion
    }

    /// <summary>
    /// Execution mode pre validation - PUBLIC
    /// </summary>
    public enum ValidationExecutionMode
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
}