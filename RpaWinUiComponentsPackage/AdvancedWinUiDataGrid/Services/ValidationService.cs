// Services/ValidationService.cs - ✅ NEZÁVISLÝ s ILogger<T>
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia validačnej služby pre DataGrid - ✅ NEZÁVISLÝ s ILogger<ValidationService>  
    /// </summary>
    internal class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;
        private readonly Dictionary<string, List<ValidationRule>> _validationRules = new();
        private readonly Dictionary<string, List<string>> _validationErrors = new();
        private readonly ThrottleHelper _throttleHelper;

        private GridConfiguration? _configuration;
        private bool _isInitialized = false;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _throttleHelper = new ThrottleHelper();

            _logger.LogDebug("🔧 ValidationService created with logger: {LoggerType}", logger.GetType().Name);
        }

        public Task InitializeAsync(GridConfiguration configuration)
        {
            try
            {
                _logger.LogInformation("🚀 ValidationService.InitializeAsync START - Rules: {RuleCount}",
                    configuration?.ValidationRules?.Count ?? 0);

                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                _validationRules.Clear();
                _validationErrors.Clear();

                // Registruj všetky validačné pravidlá
                foreach (var rule in _configuration.ValidationRules)
                {
                    AddValidationRuleInternal(rule);
                    _logger.LogDebug("📋 Registered validation rule: {ColumnName} - {RuleType} - '{Message}'",
                        rule.ColumnName, rule.Type, rule.ErrorMessage);
                }

                // Nastav throttling ak je povolený
                if (_configuration.ThrottlingConfig.EnableValidationThrottling)
                {
                    _throttleHelper.SetDebounceTime(_configuration.ThrottlingConfig.ValidationDebounceMs);
                    _logger.LogDebug("⚙️ Validation throttling enabled - Debounce: {DebounceMs}ms",
                        _configuration.ThrottlingConfig.ValidationDebounceMs);
                }
                else
                {
                    _logger.LogDebug("⚙️ Validation throttling disabled");
                }

                _isInitialized = true;
                _logger.LogInformation("✅ ValidationService initialized - {RuleCount} rules, {ColumnCount} columns",
                    _configuration.ValidationRules.Count, _validationRules.Keys.Count);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during ValidationService initialization");
                throw;
            }
        }

        public async Task<List<string>> ValidateCellAsync(string columnName, object? value)
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogWarning("⚠️ ValidateCellAsync: Empty column name provided");
                    return new List<string>();
                }

                _logger.LogDebug("🔍 ValidateCellAsync START - Column: {ColumnName}, Value: '{Value}' (Type: {ValueType})",
                    columnName, value, value?.GetType().Name ?? "null");

                var errors = new List<string>();

                if (!_validationRules.ContainsKey(columnName))
                {
                    _logger.LogTrace("📋 No validation rules for column: {ColumnName}", columnName);
                    return errors;
                }

                var rules = _validationRules[columnName];
                _logger.LogDebug("🔍 Found {RuleCount} validation rules for column: {ColumnName}", rules.Count, columnName);

                if (_configuration!.ThrottlingConfig.EnableValidationThrottling)
                {
                    errors = await _throttleHelper.ThrottleAsync(
                        $"ValidateCell_{columnName}",
                        () => ValidateCellInternal(columnName, value, rules)
                    );
                    _logger.LogDebug("⏱️ Throttled validation completed for {ColumnName}", columnName);
                }
                else
                {
                    errors = await Task.Run(() => ValidateCellInternal(columnName, value, rules));
                    _logger.LogDebug("⚡ Direct validation completed for {ColumnName}", columnName);
                }

                // Uložiť alebo vymazať chyby
                var errorKey = $"{columnName}";
                if (errors.Any())
                {
                    _validationErrors[errorKey] = errors;
                    _logger.LogWarning("⚠️ Validation FAILED for {ColumnName}: {ErrorCount} errors - {Errors}",
                        columnName, errors.Count, string.Join("; ", errors));
                }
                else
                {
                    _validationErrors.Remove(errorKey);
                    _logger.LogDebug("✅ Validation PASSED for {ColumnName}", columnName);
                }

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateCellAsync - Column: {ColumnName}", columnName);
                return new List<string> { $"Chyba validácie: {ex.Message}" };
            }
        }

        public async Task<List<string>> ValidateRowAsync(Dictionary<string, object?> rowData)
        {
            try
            {
                EnsureInitialized();

                if (rowData == null)
                {
                    _logger.LogWarning("⚠️ ValidateRowAsync: Null row data provided");
                    return new List<string>();
                }

                _logger.LogDebug("🔍 ValidateRowAsync START - Row with {CellCount} cells", rowData.Count);

                var allErrors = new List<string>();

                var isRowEmpty = IsRowEmpty(rowData);
                if (isRowEmpty)
                {
                    _logger.LogDebug("📄 Row is empty - skipping validation");
                    return allErrors;
                }

                // Log sample row data pre debugging
                var sampleData = string.Join(", ", rowData.Take(3).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                _logger.LogDebug("📊 Validating row data sample: {SampleData}...", sampleData);

                var validationTasks = new List<Task<List<string>>>();

                // Vytvor validation tasks pre každú bunku
                foreach (var kvp in rowData)
                {
                    var columnName = kvp.Key;
                    var value = kvp.Value;

                    if (IsSpecialColumn(columnName))
                    {
                        _logger.LogTrace("⏭️ Skipping special column: {ColumnName}", columnName);
                        continue;
                    }

                    validationTasks.Add(ValidateCellAsync(columnName, value));
                }

                // Počkaj na všetky validácie
                var results = await Task.WhenAll(validationTasks);

                // Agreguj všetky chyby
                foreach (var errors in results)
                {
                    allErrors.AddRange(errors);
                }

                if (allErrors.Any())
                {
                    _logger.LogWarning("⚠️ Row validation FAILED - {ErrorCount} total errors: {Errors}",
                        allErrors.Count, string.Join("; ", allErrors));
                }
                else
                {
                    _logger.LogDebug("✅ Row validation PASSED - All cells valid");
                }

                return allErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateRowAsync");
                return new List<string> { $"Chyba validácie riadku: {ex.Message}" };
            }
        }

        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation("🔍 ValidateAllRowsAsync START - Clearing existing errors");

                _validationErrors.Clear();
                var hasErrors = false;

                // TODO: Implementácia validácie všetkých riadkov cez DataManagementService
                // Zatiaľ iba placeholder logika
                _logger.LogDebug("📋 ValidateAllRows: Implementation pending - returning success");

                await Task.CompletedTask;

                _logger.LogInformation("✅ ValidateAllRowsAsync COMPLETED - Result: {HasErrors}",
                    hasErrors ? "ERRORS FOUND" : "ALL VALID");
                return !hasErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateAllRowsAsync");
                throw;
            }
        }

        public Task AddValidationRuleAsync(ValidationRule rule)
        {
            try
            {
                if (rule == null)
                {
                    _logger.LogError("❌ AddValidationRuleAsync: Null rule provided");
                    throw new ArgumentNullException(nameof(rule));
                }

                _logger.LogInformation("➕ Adding validation rule - Column: {ColumnName}, Type: {RuleType}, Message: '{Message}'",
                    rule.ColumnName, rule.Type, rule.ErrorMessage);

                AddValidationRuleInternal(rule);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddValidationRuleAsync");
                throw;
            }
        }

        public Task RemoveValidationRuleAsync(string columnName, ValidationType type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogError("❌ RemoveValidationRuleAsync: Empty column name");
                    throw new ArgumentException("ColumnName nemôže byť prázdny", nameof(columnName));
                }

                _logger.LogInformation("🗑️ Removing validation rule - Column: {ColumnName}, Type: {RuleType}",
                    columnName, type);

                if (_validationRules.ContainsKey(columnName))
                {
                    var rules = _validationRules[columnName];
                    var removedCount = rules.RemoveAll(r => r.Type == type);

                    if (removedCount > 0)
                    {
                        _logger.LogDebug("✅ Removed {Count} validation rules of type {Type} for column {ColumnName}",
                            removedCount, type, columnName);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No validation rules of type {Type} found for column {ColumnName}",
                            type, columnName);
                    }

                    if (!rules.Any())
                    {
                        _validationRules.Remove(columnName);
                        _logger.LogDebug("🧹 Removed empty rule collection for column {ColumnName}", columnName);
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ No validation rules found for column {ColumnName}", columnName);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in RemoveValidationRuleAsync");
                throw;
            }
        }

        public List<ValidationRule> GetValidationRules(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                _logger.LogWarning("⚠️ GetValidationRules: Empty column name");
                return new List<ValidationRule>();
            }

            var rules = _validationRules.ContainsKey(columnName)
                ? new List<ValidationRule>(_validationRules[columnName])
                : new List<ValidationRule>();

            _logger.LogDebug("📋 GetValidationRules for {ColumnName}: {RuleCount} rules", columnName, rules.Count);
            return rules;
        }

        public Task ClearAllValidationErrorsAsync()
        {
            try
            {
                var errorCount = _validationErrors.Sum(kvp => kvp.Value.Count);
                _validationErrors.Clear();

                _logger.LogInformation("🧹 ClearAllValidationErrorsAsync - Cleared {ErrorCount} errors", errorCount);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearAllValidationErrorsAsync");
                throw;
            }
        }

        #region Private Helper Methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                _logger.LogError("❌ ValidationService not initialized - call InitializeAsync() first");
                throw new InvalidOperationException("ValidationService nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
            }
        }

        private void AddValidationRuleInternal(ValidationRule rule)
        {
            if (!_validationRules.ContainsKey(rule.ColumnName))
            {
                _validationRules[rule.ColumnName] = new List<ValidationRule>();
                _logger.LogDebug("📋 Created new rule collection for column: {ColumnName}", rule.ColumnName);
            }

            _validationRules[rule.ColumnName].Add(rule);
            _logger.LogDebug("✅ Added validation rule: {ColumnName} - {RuleType}", rule.ColumnName, rule.Type);
        }

        private List<string> ValidateCellInternal(string columnName, object? value, List<ValidationRule> rules)
        {
            var errors = new List<string>();
            var startTime = DateTime.UtcNow;

            _logger.LogTrace("🔍 ValidateCellInternal START - {ColumnName}: '{Value}' against {RuleCount} rules",
                columnName, value, rules.Count);

            foreach (var rule in rules)
            {
                try
                {
                    var ruleStartTime = DateTime.UtcNow;
                    var isValid = rule.Validate(value);
                    var ruleDuration = (DateTime.UtcNow - ruleStartTime).TotalMilliseconds;

                    if (!isValid)
                    {
                        var errorMessage = $"{columnName}: {rule.ErrorMessage}";
                        errors.Add(errorMessage);
                        _logger.LogDebug("❌ Rule FAILED: {RuleType} for {ColumnName} - '{ErrorMessage}' ({Duration:F1}ms)",
                            rule.Type, columnName, rule.ErrorMessage, ruleDuration);
                    }
                    else
                    {
                        _logger.LogTrace("✅ Rule PASSED: {RuleType} for {ColumnName} ({Duration:F1}ms)",
                            rule.Type, columnName, ruleDuration);
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"{columnName}: Chyba validácie - {ex.Message}";
                    errors.Add(errorMessage);
                    _logger.LogError(ex, "❌ Exception in validation rule {RuleType} for column {ColumnName}",
                        rule.Type, columnName);
                }
            }

            var totalDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogTrace("🔍 ValidateCellInternal COMPLETED - {ColumnName}: {ErrorCount} errors in {Duration:F1}ms",
                columnName, errors.Count, totalDuration);

            return errors;
        }

        private bool IsRowEmpty(Dictionary<string, object?> rowData)
        {
            foreach (var kvp in rowData)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                if (IsSpecialColumn(columnName))
                    continue;

                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSpecialColumn(string columnName)
        {
            var isSpecial = columnName == "DeleteRows" || columnName == "ValidAlerts";
            if (isSpecial)
            {
                _logger.LogTrace("🔍 Special column detected: {ColumnName}", columnName);
            }
            return isSpecial;
        }

        #endregion

        #region Public Properties s logovaním

        public IReadOnlyDictionary<string, List<string>> ValidationErrors => _validationErrors;

        public bool HasValidationErrors(string columnName)
        {
            var hasErrors = _validationErrors.ContainsKey(columnName) && _validationErrors[columnName].Any();
            _logger.LogTrace("🔍 HasValidationErrors for {ColumnName}: {HasErrors}", columnName, hasErrors);
            return hasErrors;
        }

        public List<string> GetValidationErrors(string columnName)
        {
            var errors = _validationErrors.ContainsKey(columnName)
                ? new List<string>(_validationErrors[columnName])
                : new List<string>();

            _logger.LogTrace("📋 GetValidationErrors for {ColumnName}: {ErrorCount} errors", columnName, errors.Count);
            return errors;
        }

        public bool HasAnyValidationErrors
        {
            get
            {
                var hasErrors = _validationErrors.Any(kvp => kvp.Value.Any());
                if (hasErrors)
                {
                    var totalErrors = _validationErrors.Sum(kvp => kvp.Value.Count);
                    _logger.LogTrace("⚠️ HasAnyValidationErrors: TRUE - {TotalErrors} total errors", totalErrors);
                }
                return hasErrors;
            }
        }

        public int TotalValidationErrorCount
        {
            get
            {
                var count = _validationErrors.Sum(kvp => kvp.Value.Count);
                _logger.LogTrace("📊 TotalValidationErrorCount: {Count}", count);
                return count;
            }
        }

        /// <summary>
        /// Diagnostické informácie o ValidationService
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var ruleCount = _validationRules.Sum(kvp => kvp.Value.Count);
            var errorCount = TotalValidationErrorCount;
            var columnCount = _validationRules.Keys.Count;

            return $"ValidationService: {ruleCount} rules on {columnCount} columns, {errorCount} current errors, " +
                   $"Throttling: {_configuration?.ThrottlingConfig.EnableValidationThrottling ?? false}";
        }

        #endregion
    }
}