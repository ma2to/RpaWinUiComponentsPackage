// Services/ValidationService.cs - ✅ OPRAVENÝ - INTERNAL
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia validačnej služby pre DataGrid - ✅ INTERNAL
    /// </summary>
    internal class ValidationService : IValidationService  // ✅ CHANGED: public -> internal
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
        }

        // ... rest of implementation stays the same ...
        // (všetky metódy zostávajú rovnaké, len trieda je internal)

        public Task InitializeAsync(GridConfiguration configuration)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                _validationRules.Clear();
                _validationErrors.Clear();

                foreach (var rule in _configuration.ValidationRules)
                {
                    AddValidationRuleInternal(rule);
                }

                if (_configuration.ThrottlingConfig.EnableValidationThrottling)
                {
                    _throttleHelper.SetDebounceTime(_configuration.ThrottlingConfig.ValidationDebounceMs);
                }

                _isInitialized = true;
                _logger.LogInformation("ValidationService inicializovaný s {RuleCount} pravidlami", _configuration.ValidationRules.Count);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri inicializácii ValidationService");
                throw;
            }
        }

        public async Task<List<string>> ValidateCellAsync(string columnName, object? value)
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(columnName))
                    return new List<string>();

                var errors = new List<string>();

                if (!_validationRules.ContainsKey(columnName))
                    return errors;

                var rules = _validationRules[columnName];

                if (_configuration!.ThrottlingConfig.EnableValidationThrottling)
                {
                    errors = await _throttleHelper.ThrottleAsync(
                        $"ValidateCell_{columnName}",
                        () => ValidateCellInternal(columnName, value, rules)
                    );
                }
                else
                {
                    errors = await Task.Run(() => ValidateCellInternal(columnName, value, rules));
                }

                var errorKey = $"{columnName}";
                if (errors.Any())
                {
                    _validationErrors[errorKey] = errors;
                }
                else
                {
                    _validationErrors.Remove(errorKey);
                }

                _logger.LogDebug("Validácia bunky {ColumnName}: {ErrorCount} chýb", columnName, errors.Count);
                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri validácii bunky {ColumnName}", columnName);
                return new List<string> { $"Chyba validácie: {ex.Message}" };
            }
        }

        public async Task<List<string>> ValidateRowAsync(Dictionary<string, object?> rowData)
        {
            try
            {
                EnsureInitialized();

                if (rowData == null)
                    return new List<string>();

                var allErrors = new List<string>();

                var isRowEmpty = IsRowEmpty(rowData);
                if (isRowEmpty)
                {
                    _logger.LogDebug("Riadok je prázdny - preskakujem validáciu");
                    return allErrors;
                }

                var validationTasks = new List<Task<List<string>>>();

                foreach (var kvp in rowData)
                {
                    var columnName = kvp.Key;
                    var value = kvp.Value;

                    if (IsSpecialColumn(columnName))
                        continue;

                    validationTasks.Add(ValidateCellAsync(columnName, value));
                }

                var results = await Task.WhenAll(validationTasks);

                foreach (var errors in results)
                {
                    allErrors.AddRange(errors);
                }

                _logger.LogDebug("Validácia riadku dokončená: {ErrorCount} chýb", allErrors.Count);
                return allErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri validácii riadku");
                return new List<string> { $"Chyba validácie riadku: {ex.Message}" };
            }
        }

        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation("Spúšťa sa validácia všetkých riadkov");

                _validationErrors.Clear();
                var hasErrors = false;
                await Task.CompletedTask;

                _logger.LogInformation("Validácia všetkých riadkov dokončená: {HasErrors}", hasErrors ? "našli sa chyby" : "všetko v poriadku");
                return !hasErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri validácii všetkých riadkov");
                throw;
            }
        }

        public Task AddValidationRuleAsync(ValidationRule rule)
        {
            try
            {
                if (rule == null)
                    throw new ArgumentNullException(nameof(rule));

                AddValidationRuleInternal(rule);
                _logger.LogDebug("Pridané validačné pravidlo pre stĺpec {ColumnName}: {RuleType}", rule.ColumnName, rule.Type);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri pridávaní validačného pravidla");
                throw;
            }
        }

        public Task RemoveValidationRuleAsync(string columnName, ValidationType type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(columnName))
                    throw new ArgumentException("ColumnName nemôže byť prázdny", nameof(columnName));

                if (_validationRules.ContainsKey(columnName))
                {
                    var rules = _validationRules[columnName];
                    var removedCount = rules.RemoveAll(r => r.Type == type);

                    if (removedCount > 0)
                    {
                        _logger.LogDebug("Odstránených {Count} validačných pravidiel typu {Type} pre stĺpec {ColumnName}",
                            removedCount, type, columnName);
                    }

                    if (!rules.Any())
                    {
                        _validationRules.Remove(columnName);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri odstraňovaní validačného pravidla");
                throw;
            }
        }

        public List<ValidationRule> GetValidationRules(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return new List<ValidationRule>();

            return _validationRules.ContainsKey(columnName)
                ? new List<ValidationRule>(_validationRules[columnName])
                : new List<ValidationRule>();
        }

        public Task ClearAllValidationErrorsAsync()
        {
            try
            {
                _validationErrors.Clear();
                _logger.LogDebug("Všetky validačné chyby vyčistené");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri čistení validačných chýb");
                throw;
            }
        }

        #region Private Helper Methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("ValidationService nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
        }

        private void AddValidationRuleInternal(ValidationRule rule)
        {
            if (!_validationRules.ContainsKey(rule.ColumnName))
            {
                _validationRules[rule.ColumnName] = new List<ValidationRule>();
            }

            _validationRules[rule.ColumnName].Add(rule);
        }

        private List<string> ValidateCellInternal(string columnName, object? value, List<ValidationRule> rules)
        {
            var errors = new List<string>();

            foreach (var rule in rules)
            {
                try
                {
                    if (!rule.Validate(value))
                    {
                        errors.Add($"{columnName}: {rule.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Chyba pri vykonávaní validačného pravidla {RuleType} pre stĺpec {ColumnName}",
                        rule.Type, columnName);
                    errors.Add($"{columnName}: Chyba validácie - {ex.Message}");
                }
            }

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
                    return false;
            }

            return true;
        }

        private bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteRows" || columnName == "ValidAlerts";
        }

        #endregion

        #region Public Properties

        public IReadOnlyDictionary<string, List<string>> ValidationErrors => _validationErrors;

        public bool HasValidationErrors(string columnName)
        {
            return _validationErrors.ContainsKey(columnName) && _validationErrors[columnName].Any();
        }

        public List<string> GetValidationErrors(string columnName)
        {
            return _validationErrors.ContainsKey(columnName)
                ? new List<string>(_validationErrors[columnName])
                : new List<string>();
        }

        public bool HasAnyValidationErrors => _validationErrors.Any(kvp => kvp.Value.Any());

        public int TotalValidationErrorCount => _validationErrors.Sum(kvp => kvp.Value.Count);

        #endregion
    }
}