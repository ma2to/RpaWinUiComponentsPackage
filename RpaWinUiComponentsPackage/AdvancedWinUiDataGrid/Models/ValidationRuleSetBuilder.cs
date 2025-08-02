// Models/ValidationRuleSetBuilder.cs - ✅ INTERNAL Builder pre ValidationRuleSet
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Fluent builder pre ValidationRuleSet - INTERNAL
    /// </summary>
    internal class ValidationRuleSetBuilder
    {
        private readonly ValidationRuleSet _ruleSet;

        internal ValidationRuleSetBuilder(string name)
        {
            _ruleSet = new ValidationRuleSet
            {
                Name = name
            };
        }

        #region Basic Configuration

        /// <summary>
        /// Nastaví popis rule set
        /// </summary>
        public ValidationRuleSetBuilder WithDescription(string description)
        {
            _ruleSet.Description = description;
            return this;
        }

        /// <summary>
        /// Nastaví execution mode
        /// </summary>
        public ValidationRuleSetBuilder WithExecutionMode(ValidationExecutionMode mode)
        {
            _ruleSet.ExecutionMode = mode;
            return this;
        }

        /// <summary>
        /// Nastaví throttling config
        /// </summary>
        public ValidationRuleSetBuilder WithThrottling(int debounceMs = 300, int maxConcurrent = 5)
        {
            _ruleSet.ThrottlingConfig = new ValidationThrottlingConfig
            {
                DebounceMs = debounceMs,
                MaxConcurrentValidations = maxConcurrent,
                EnableAsyncValidation = true,
                EnableBatchValidation = true
            };
            return this;
        }

        /// <summary>
        /// Vypne async validation
        /// </summary>
        public ValidationRuleSetBuilder DisableAsyncValidation()
        {
            _ruleSet.ThrottlingConfig.EnableAsyncValidation = false;
            return this;
        }

        /// <summary>
        /// Vypne batch validation
        /// </summary>
        public ValidationRuleSetBuilder DisableBatchValidation()
        {
            _ruleSet.ThrottlingConfig.EnableBatchValidation = false;
            return this;
        }

        #endregion

        #region Rule Management

        /// <summary>
        /// Pridá pravidlo do setu
        /// </summary>
        public ValidationRuleSetBuilder AddRule(AdvancedValidationRule rule)
        {
            _ruleSet.AddRule(rule);
            return this;
        }

        /// <summary>
        /// Pridá viacero pravidiel
        /// </summary>
        public ValidationRuleSetBuilder AddRules(params AdvancedValidationRule[] rules)
        {
            foreach (var rule in rules)
            {
                _ruleSet.AddRule(rule);
            }
            return this;
        }

        /// <summary>
        /// Pridá pravidlá z kolekcie
        /// </summary>
        public ValidationRuleSetBuilder AddRules(IEnumerable<AdvancedValidationRule> rules)
        {
            foreach (var rule in rules)
            {
                _ruleSet.AddRule(rule);
            }
            return this;
        }

        #endregion

        #region Quick Rule Builders

        /// <summary>
        /// Pridá required field validation
        /// </summary>
        public ValidationRuleSetBuilder AddRequired(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                var rule = AdvancedValidationRule.Create($"Required_{columnName}")
                    .ForColumns(columnName)
                    .ThenRequired(columnName)
                    .WithSeverity(ValidationSeverity.Error)
                    .Build();

                _ruleSet.AddRule(rule);
            }
            return this;
        }

        /// <summary>
        /// Pridá email validation
        /// </summary>
        public ValidationRuleSetBuilder AddEmailValidation(string columnName, bool required = false)
        {
            var builder = AdvancedValidationRule.Create($"Email_{columnName}")
                .ForColumns(columnName)
                .ThenValidEmail(columnName)
                .WithSeverity(ValidationSeverity.Error);

            if (required)
            {
                builder = builder.When(ctx => ctx.HasValue(columnName));
            }

            _ruleSet.AddRule(builder.Build());
            return this;
        }

        /// <summary>
        /// Pridá phone validation
        /// </summary>
        public ValidationRuleSetBuilder AddPhoneValidation(string columnName, bool required = false)
        {
            var builder = AdvancedValidationRule.Create($"Phone_{columnName}")
                .ForColumns(columnName)
                .ThenValidPhoneNumber(columnName)
                .WithSeverity(ValidationSeverity.Error);

            if (required)
            {
                builder = builder.When(ctx => ctx.HasValue(columnName));
            }

            _ruleSet.AddRule(builder.Build());
            return this;
        }

        /// <summary>
        /// Pridá range validation
        /// </summary>
        public ValidationRuleSetBuilder AddRangeValidation(string columnName, IComparable minValue, IComparable maxValue)
        {
            var rule = AdvancedValidationRule.Create($"Range_{columnName}")
                .ForColumns(columnName)
                .ThenInRange(columnName, minValue, maxValue)
                .WithSeverity(ValidationSeverity.Error)
                .Build();

            _ruleSet.AddRule(rule);
            return this;
        }

        /// <summary>
        /// Pridá unique validation
        /// </summary>
        public ValidationRuleSetBuilder AddUniqueValidation(string columnName)
        {
            var rule = AdvancedValidationRule.CreateCrossCellRule($"Unique_{columnName}")
                .ForColumns(columnName)
                .ThenUniqueInColumn(columnName)
                .WithSeverity(ValidationSeverity.Error)
                .Build();

            _ruleSet.AddRule(rule);
            return this;
        }

        /// <summary>
        /// Pridá date range validation
        /// </summary>
        public ValidationRuleSetBuilder AddDateRangeValidation(string startColumn, string endColumn)
        {
            var rule = AdvancedValidationRule.Create($"DateRange_{startColumn}_{endColumn}")
                .ForColumns(startColumn, endColumn)
                .DependsOn(startColumn, endColumn)
                .ThenStartBeforeEnd(startColumn, endColumn)
                .WithSeverity(ValidationSeverity.Error)
                .Build();

            _ruleSet.AddRule(rule);
            return this;
        }

        /// <summary>
        /// Pridá conditional required validation
        /// </summary>
        public ValidationRuleSetBuilder AddConditionalRequired(string targetColumn, string conditionColumn, object conditionValue)
        {
            var rule = AdvancedValidationRule.Create($"ConditionalRequired_{targetColumn}")
                .ForColumns(targetColumn)
                .DependsOn(conditionColumn)
                .WhenColumnEquals(conditionColumn, conditionValue)
                .ThenRequired(targetColumn)
                .WithSeverity(ValidationSeverity.Error)
                .WithMessage($"{targetColumn} is required when {conditionColumn} is '{conditionValue}'")
                .Build();

            _ruleSet.AddRule(rule);
            return this;
        }

        #endregion

        #region Common Business Rules

        /// <summary>
        /// Pridá employee validation rules
        /// </summary>
        public ValidationRuleSetBuilder AddEmployeeValidationRules()
        {
            return AddRequired("FirstName", "LastName", "Email")
                .AddEmailValidation("Email", true)
                .AddUniqueValidation("Email")
                .AddRangeValidation("Salary", 0, 999999)
                .AddConditionalRequired("ManagerId", "EmployeeType", "Employee")
                .AddRule(
                    AdvancedValidationRule.Create("ManagerSalaryCheck")
                        .ForColumns("Salary")
                        .DependsOn("ManagerId", "EmployeeType")
                        .When(ctx => ctx.GetStringValue("EmployeeType") == "Employee" && ctx.HasValue("ManagerId"))
                        .ThenValidateAcrossCells((ctx, allData) =>
                        {
                            var employeeSalary = Convert.ToDecimal(ctx.CurrentValue ?? 0);
                            var managerId = ctx.GetStringValue("ManagerId");
                            
                            var manager = allData.FirstOrDefault(d => 
                                d.GetStringValue("EmployeeId") == managerId);
                            
                            if (manager != null)
                            {
                                var managerSalary = Convert.ToDecimal(manager.GetValue("Salary") ?? 0);
                                if (employeeSalary > managerSalary)
                                {
                                    return ValidationResult.Error("Employee salary cannot exceed manager's salary");
                                }
                            }
                            
                            return ValidationResult.Success();
                        })
                        .WithSeverity(ValidationSeverity.Warning)
                        .Build()
                );
        }

        /// <summary>
        /// Pridá project validation rules
        /// </summary>
        public ValidationRuleSetBuilder AddProjectValidationRules()
        {
            return AddRequired("ProjectName", "StartDate")
                .AddDateRangeValidation("StartDate", "EndDate")
                .AddUniqueValidation("ProjectCode")
                .AddConditionalRequired("EndDate", "Status", "Completed")
                .AddConditionalRequired("ActualHours", "Status", "Completed");
        }

        #endregion

        #region Build

        /// <summary>
        /// Vytvorí finálny ValidationRuleSet
        /// </summary>
        public ValidationRuleSet Build()
        {
            if (string.IsNullOrEmpty(_ruleSet.Name))
                throw new InvalidOperationException("RuleSet name is required");

            return _ruleSet;
        }

        /// <summary>
        /// Implicit conversion to ValidationRuleSet
        /// </summary>
        public static implicit operator ValidationRuleSet(ValidationRuleSetBuilder builder)
        {
            return builder.Build();
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí basic validation rule set
        /// </summary>
        public static ValidationRuleSet CreateBasicRuleSet(string name, params string[] requiredFields)
        {
            return ValidationRuleSet.Create(name)
                .AddRequired(requiredFields)
                .WithExecutionMode(ValidationExecutionMode.ProcessAll)
                .WithThrottling(300, 5)
                .Build();
        }

        /// <summary>
        /// Vytvorí employee data rule set
        /// </summary>
        public static ValidationRuleSet CreateEmployeeRuleSet()
        {
            return ValidationRuleSet.Create("EmployeeValidation")
                .WithDescription("Validation rules for employee data")
                .AddEmployeeValidationRules()
                .WithExecutionMode(ValidationExecutionMode.ProcessAll)
                .WithThrottling(500, 3)
                .Build();
        }

        /// <summary>
        /// Vytvorí project data rule set
        /// </summary>
        public static ValidationRuleSet CreateProjectRuleSet()
        {
            return ValidationRuleSet.Create("ProjectValidation")
                .WithDescription("Validation rules for project data")
                .AddProjectValidationRules()
                .WithExecutionMode(ValidationExecutionMode.ProcessAll)
                .WithThrottling(300, 5)
                .Build();
        }

        #endregion
    }
}