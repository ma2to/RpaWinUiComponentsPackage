// Services/CrossRowValidationService.cs - ✅ NOVÉ: Cross-row Validation Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Service pre cross-row validation (unique constraints, dependencies)
    /// </summary>
    public class CrossRowValidationService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly List<CrossRowValidationRule> _rules = new();
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        public CrossRowValidationService(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _logger.LogDebug("🚀 CrossRowValidationService initialized");
        }

        /// <summary>
        /// Event pre cross-row validation results
        /// </summary>
        public event EventHandler<CrossRowValidationResults>? ValidationCompleted;

        /// <summary>
        /// Pridá cross-row validation rule
        /// </summary>
        public void AddRule(CrossRowValidationRule rule)
        {
            lock (_lockObject)
            {
                try
                {
                    rule.Validate();
                    _rules.Add(rule);
                    _logger.LogDebug("✅ Added cross-row validation rule: {Rule}", rule);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error adding cross-row validation rule");
                    throw;
                }
            }
        }

        /// <summary>
        /// Odstráni cross-row validation rule
        /// </summary>
        public void RemoveRule(CrossRowValidationRule rule)
        {
            lock (_lockObject)
            {
                _rules.Remove(rule);
                _logger.LogDebug("🗑️ Removed cross-row validation rule: {Rule}", rule);
            }
        }

        /// <summary>
        /// Vyčistí všetky rules
        /// </summary>
        public void ClearRules()
        {
            lock (_lockObject)
            {
                _rules.Clear();
                _logger.LogDebug("🗑️ Cleared all cross-row validation rules");
            }
        }

        /// <summary>
        /// Spustí cross-row validation pre všetky riadky
        /// </summary>
        public async Task<CrossRowValidationResults> ValidateAllRowsAsync(
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new CrossRowValidationResults();

            try
            {
                _logger.LogInformation("🔍 Cross-row validation START - Rows: {RowCount}, Rules: {RuleCount}",
                    data.Count, _rules.Count);

                if (!_rules.Any() || !data.Any())
                {
                    _logger.LogDebug("⏭️ Cross-row validation skipped - no rules or data");
                    return results;
                }

                // Validuj každý riadok
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var rowResult = await ValidateRowAsync(rowIndex, data, columns, cancellationToken);
                    if (!rowResult.IsValid)
                    {
                        results.RowResults.Add(rowResult);
                        results.TotalErrors++;
                    }
                }

                stopwatch.Stop();
                results.Duration = stopwatch.Elapsed;

                _logger.LogInformation("✅ Cross-row validation COMPLETED - Duration: {Duration}ms, " +
                    "Errors: {ErrorCount}", results.Duration.TotalMilliseconds, results.TotalErrors);

                ValidationCompleted?.Invoke(this, results);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in cross-row validation");
                stopwatch.Stop();
                results.Duration = stopwatch.Elapsed;
                results.HasErrors = true;
                return results;
            }
        }

        /// <summary>
        /// Validuje jeden riadok proti všetkým ostatným
        /// </summary>
        public async Task<CrossRowValidationRowResult> ValidateRowAsync(
            int rowIndex,
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            CancellationToken cancellationToken = default)
        {
            await Task.Yield(); // Make it async

            var rowResult = new CrossRowValidationRowResult
            {
                RowIndex = rowIndex,
                IsValid = true
            };

            try
            {
                if (rowIndex < 0 || rowIndex >= data.Count)
                {
                    rowResult.IsValid = false;
                    rowResult.ErrorMessage = "Invalid row index";
                    return rowResult;
                }

                var context = new CrossRowValidationContext
                {
                    CurrentRowIndex = rowIndex,
                    CurrentRowData = data[rowIndex],
                    AllRowsData = data
                };

                // Spustí všetky rules pre tento riadok
                foreach (var rule in _rules.Where(r => r.IsEnabled))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var ruleResult = await ValidateRuleAsync(rule, context, columns, cancellationToken);
                    
                    if (!ruleResult.IsValid)
                    {
                        rowResult.IsValid = false;
                        rowResult.RuleResults.Add(ruleResult);
                        
                        if (string.IsNullOrEmpty(rowResult.ErrorMessage))
                            rowResult.ErrorMessage = ruleResult.ErrorMessage;
                        else
                            rowResult.ErrorMessage += "; " + ruleResult.ErrorMessage;
                    }
                }

                return rowResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR validating row {RowIndex}", rowIndex);
                rowResult.IsValid = false;
                rowResult.ErrorMessage = $"Validation error: {ex.Message}";
                return rowResult;
            }
        }

        /// <summary>
        /// Validuje jedno pravidlo
        /// </summary>
        private async Task<CrossRowValidationResult> ValidateRuleAsync(
            CrossRowValidationRule rule,
            CrossRowValidationContext context,
            List<Models.Grid.ColumnDefinition> columns,
            CancellationToken cancellationToken)
        {
            await Task.Yield();

            try
            {
                _logger.LogTrace("🔍 Validating rule {Rule} for row {RowIndex}", 
                    rule.ValidationType, context.CurrentRowIndex);

                return rule.ValidationType switch
                {
                    CrossRowValidationType.UniqueConstraint => ValidateUniqueConstraint(rule, context),
                    CrossRowValidationType.CompositeUniqueConstraint => ValidateCompositeUniqueConstraint(rule, context),
                    CrossRowValidationType.DependencyConstraint => ValidateDependencyConstraint(rule, context),
                    CrossRowValidationType.HierarchicalConstraint => ValidateHierarchicalConstraint(rule, context),
                    CrossRowValidationType.Custom => ValidateCustomRule(rule, context),
                    _ => CrossRowValidationResult.Failure($"Unknown validation type: {rule.ValidationType}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR validating rule {Rule}", rule.ValidationType);
                return CrossRowValidationResult.Failure($"Rule validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validuje unique constraint
        /// </summary>
        private CrossRowValidationResult ValidateUniqueConstraint(
            CrossRowValidationRule rule, 
            CrossRowValidationContext context)
        {
            var currentValue = context.GetCurrentValue<object>(rule.ColumnName);
            
            // Skip validation for null/empty values
            if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
                return CrossRowValidationResult.Success();

            var otherValues = context.GetOtherValues<object>(rule.ColumnName);
            var duplicates = otherValues
                .Select((value, index) => new { Value = value, Index = index })
                .Where(item => Equals(item.Value, currentValue))
                .Select(item => item.Index)
                .ToList();

            if (duplicates.Any())
            {
                return CrossRowValidationResult.Failure(
                    rule.ErrorMessage,
                    rule.Severity,
                    duplicates.ToArray());
            }

            return CrossRowValidationResult.Success();
        }

        /// <summary>
        /// Validuje composite unique constraint
        /// </summary>
        private CrossRowValidationResult ValidateCompositeUniqueConstraint(
            CrossRowValidationRule rule, 
            CrossRowValidationContext context)
        {
            var currentComposite = rule.ComparisonColumns
                .Select(col => context.GetCurrentValue<object>(col))
                .ToList();

            // Skip validation ak ktorýkoľvek z composite values je null/empty
            if (currentComposite.Any(v => v == null || string.IsNullOrWhiteSpace(v?.ToString())))
                return CrossRowValidationResult.Success();

            var conflictingRows = new List<int>();

            for (int i = 0; i < context.OtherRowsData.Count; i++)
            {
                var otherRow = context.OtherRowsData[i];
                var otherComposite = rule.ComparisonColumns
                    .Select(col => otherRow.ContainsKey(col) ? otherRow[col] : null)
                    .ToList();

                if (CompositeValuesEqual(currentComposite, otherComposite))
                {
                    conflictingRows.Add(i);
                }
            }

            if (conflictingRows.Any())
            {
                return CrossRowValidationResult.Failure(
                    rule.ErrorMessage,
                    rule.Severity,
                    conflictingRows.ToArray());
            }

            return CrossRowValidationResult.Success();
        }

        /// <summary>
        /// Validuje dependency constraint
        /// </summary>
        private CrossRowValidationResult ValidateDependencyConstraint(
            CrossRowValidationRule rule, 
            CrossRowValidationContext context)
        {
            // Basic dependency validation - implementácia závisí od konkrétnych requirements
            var currentValue = context.GetCurrentValue<object>(rule.ColumnName);
            var dependentColumn = rule.ComparisonColumns.FirstOrDefault();
            
            if (string.IsNullOrEmpty(dependentColumn))
                return CrossRowValidationResult.Success();

            var dependentValue = context.GetCurrentValue<object>(dependentColumn);

            // Základná logika: ak dependent value je null/empty, current value musí byť tiež null/empty
            if (dependentValue == null || string.IsNullOrWhiteSpace(dependentValue.ToString()))
            {
                if (currentValue != null && !string.IsNullOrWhiteSpace(currentValue.ToString()))
                {
                    return CrossRowValidationResult.Failure(
                        rule.ErrorMessage,
                        rule.Severity,
                        context.CurrentRowIndex);
                }
            }

            return CrossRowValidationResult.Success();
        }

        /// <summary>
        /// Validuje hierarchical constraint
        /// </summary>
        private CrossRowValidationResult ValidateHierarchicalConstraint(
            CrossRowValidationRule rule, 
            CrossRowValidationContext context)
        {
            // Placeholder pre hierarchical validation
            // Implementácia závisí od konkrétnych hierarchical requirements
            return CrossRowValidationResult.Success();
        }

        /// <summary>
        /// Validuje custom rule
        /// </summary>
        private CrossRowValidationResult ValidateCustomRule(
            CrossRowValidationRule rule, 
            CrossRowValidationContext context)
        {
            if (rule.CustomValidationFunction == null)
                return CrossRowValidationResult.Failure("Custom validation function not provided");

            try
            {
                return rule.CustomValidationFunction(context.CurrentRowData, context.AllRowsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in custom validation function");
                return CrossRowValidationResult.Failure($"Custom validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Porovná composite values
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
        /// Získa aktuálne rules
        /// </summary>
        public List<CrossRowValidationRule> GetRules()
        {
            lock (_lockObject)
            {
                return new List<CrossRowValidationRule>(_rules);
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _rules.Clear();
            _isDisposed = true;

            _logger.LogDebug("🗑️ CrossRowValidationService disposed");
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Výsledky cross-row validation
    /// </summary>
    public class CrossRowValidationResults
    {
        /// <summary>
        /// Výsledky pre jednotlivé riadky (len tie s chybami)
        /// </summary>
        public List<CrossRowValidationRowResult> RowResults { get; set; } = new();

        /// <summary>
        /// Celkový počet chýb
        /// </summary>
        public int TotalErrors { get; set; }

        /// <summary>
        /// Trvanie validácie
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Či mali place nejaké chyby
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// Či sú všetky riadky validné
        /// </summary>
        public bool IsValid => TotalErrors == 0 && !HasErrors;

        public override string ToString()
        {
            return $"CrossRowValidation: {TotalErrors} errors in {RowResults.Count} rows ({Duration.TotalMilliseconds:F1}ms)";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Výsledok validácie pre jeden riadok
    /// </summary>
    public class CrossRowValidationRowResult
    {
        /// <summary>
        /// Index riadku
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Či je riadok validný
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Výsledky jednotlivých rules
        /// </summary>
        public List<CrossRowValidationResult> RuleResults { get; set; } = new();

        public override string ToString()
        {
            return $"Row {RowIndex}: {(IsValid ? "Valid" : $"Invalid - {ErrorMessage}")}";
        }
    }
}