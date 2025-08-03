// Services/Operations/BackgroundValidationService.cs - Background validation service implementation
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Operations
{
    /// <summary>
    /// Implementácia background validation service
    /// </summary>
    internal class BackgroundValidationService : IBackgroundValidationService
    {
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _validationSemaphore;
        private readonly ConcurrentDictionary<string, BackgroundValidationRule> _backgroundRules = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningValidations = new();
        private readonly ConcurrentDictionary<string, BackgroundValidationResult> _validationResults = new();
        private readonly ConcurrentDictionary<string, DateTime> _validationCache = new();
        private readonly BackgroundValidationDiagnostics _diagnostics = new();

        private BackgroundValidationConfiguration _configuration = BackgroundValidationConfiguration.Default;
        private bool _isInitialized = false;

        public event EventHandler<BackgroundValidationCompletedEventArgs>? ValidationCompleted;
        public event EventHandler<BackgroundValidationStateChangedEventArgs>? ValidationStateChanged;

        public BackgroundValidationService(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
            _validationSemaphore = new SemaphoreSlim(3, 3); // Default 3 concurrent validations
            
            _logger.LogDebug("🔧 BackgroundValidationService created with logger: {LoggerType}", 
                logger?.GetType().Name ?? "NullLogger");
        }

        public Task InitializeAsync(BackgroundValidationConfiguration configuration)
        {
            try
            {
                _logger.LogInformation("🚀 BackgroundValidationService.InitializeAsync START - Enabled: {IsEnabled}, Rules: {RuleCount}",
                    configuration.IsEnabled, configuration.BackgroundRules.Count);

                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                
                // Update semaphore with new concurrent limit
                _validationSemaphore.Release(_validationSemaphore.CurrentCount);
                for (int i = 0; i < _configuration.MaxConcurrentValidations; i++)
                {
                    _validationSemaphore.Wait(0);
                }

                // Register background rules
                _backgroundRules.Clear();
                foreach (var rule in _configuration.BackgroundRules)
                {
                    var key = $"{rule.ColumnName}";
                    _backgroundRules.TryAdd(key, rule);
                    _logger.LogDebug("📝 Registered background rule: {ColumnName} - {Description}", 
                        rule.ColumnName, rule.Description);
                }

                _isInitialized = true;
                _logger.LogInformation("✅ BackgroundValidationService initialized successfully - Rules: {RuleCount}", 
                    _backgroundRules.Count);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in BackgroundValidationService.InitializeAsync");
                throw;
            }
        }

        public async Task<string> StartCellValidationAsync(
            string columnName, 
            object? value, 
            Dictionary<string, object?> rowData, 
            int rowIndex,
            CancellationToken cancellationToken = default)
        {
            if (!_isInitialized || !_configuration.IsEnabled)
            {
                return string.Empty;
            }

            try
            {
                var validationId = Guid.NewGuid().ToString("N")[..8];
                var cacheKey = GenerateCacheKey(columnName, value, rowIndex);

                _logger.LogDebug("🔍 Starting background validation - Column: {ColumnName}, Row: {RowIndex}, ID: {ValidationId}",
                    columnName, rowIndex, validationId);

                // Check cache first
                if (_configuration.UseValidationCache && IsValidationCached(cacheKey))
                {
                    _diagnostics.CacheHitCount++;
                    _logger.LogDebug("💾 Cache hit for validation: {CacheKey}", cacheKey);
                    return validationId;
                }

                _diagnostics.CacheMissCount++;

                // Cancel previous validation for this cell if configured
                if (_configuration.CancelPreviousValidation)
                {
                    await CancelCellValidationAsync(columnName, rowIndex);
                }

                // Find background rule for this column
                if (!_backgroundRules.TryGetValue(columnName, out var rule) || !rule.IsEnabled)
                {
                    _logger.LogDebug("⚠️ No background rule found for column: {ColumnName}", columnName);
                    return string.Empty;
                }

                // Create cancellation token source
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var validationKey = $"{columnName}_{rowIndex}";
                _runningValidations.TryAdd(validationKey, cts);

                // Fire state changed event
                ValidationStateChanged?.Invoke(this, new BackgroundValidationStateChangedEventArgs
                {
                    ColumnName = columnName,
                    RowIndex = rowIndex,
                    ValidationId = validationId,
                    State = BackgroundValidationState.Starting
                });

                // Start validation task
                _ = Task.Run(async () =>
                {
                    await ExecuteValidationAsync(validationId, columnName, value, rowData, rowIndex, rule, cts.Token);
                }, cts.Token);

                _diagnostics.TotalValidationsStarted++;
                return validationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in StartCellValidationAsync - Column: {ColumnName}, Row: {RowIndex}",
                    columnName, rowIndex);
                return string.Empty;
            }
        }

        public async Task<string> StartRowValidationAsync(
            Dictionary<string, object?> rowData, 
            int rowIndex,
            CancellationToken cancellationToken = default)
        {
            if (!_isInitialized || !_configuration.IsEnabled)
            {
                return string.Empty;
            }

            try
            {
                _logger.LogDebug("🔍 Starting row background validation - Row: {RowIndex}", rowIndex);

                var validationTasks = new List<Task<string>>();

                // Start validation for each column that has background rules
                foreach (var rule in _backgroundRules.Values)
                {
                    if (rowData.ContainsKey(rule.ColumnName))
                    {
                        var task = StartCellValidationAsync(
                            rule.ColumnName, 
                            rowData[rule.ColumnName], 
                            rowData, 
                            rowIndex, 
                            cancellationToken);
                        validationTasks.Add(task);
                    }
                }

                await Task.WhenAll(validationTasks);
                
                var validationId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogDebug("✅ Row background validation started - Row: {RowIndex}, ID: {ValidationId}", 
                    rowIndex, validationId);

                return validationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in StartRowValidationAsync - Row: {RowIndex}", rowIndex);
                return string.Empty;
            }
        }

        private async Task ExecuteValidationAsync(
            string validationId,
            string columnName, 
            object? value, 
            Dictionary<string, object?> rowData, 
            int rowIndex,
            BackgroundValidationRule rule,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var validationKey = $"{columnName}_{rowIndex}";

            try
            {
                ValidationStateChanged?.Invoke(this, new BackgroundValidationStateChangedEventArgs
                {
                    ColumnName = columnName,
                    RowIndex = rowIndex,
                    ValidationId = validationId,
                    State = BackgroundValidationState.Running
                });

                await _validationSemaphore.WaitAsync(cancellationToken);

                try
                {
                    _logger.LogDebug("⚡ Executing background validation - Column: {ColumnName}, Rule: {Description}",
                        columnName, rule.Description);

                    // Apply timeout
                    using var timeoutCts = new CancellationTokenSource(rule.TimeoutMs);
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken, timeoutCts.Token);

                    // Execute validation function
                    var result = rule.ValidationFunction != null 
                        ? await rule.ValidationFunction(value, rowData, combinedCts.Token)
                        : BackgroundValidationResult.Success();

                    stopwatch.Stop();
                    result.DurationMs = stopwatch.Elapsed.TotalMilliseconds;

                    // Store result
                    var resultKey = $"{columnName}_{rowIndex}";
                    _validationResults.AddOrUpdate(resultKey, result, (key, oldValue) => result);

                    // Update cache
                    if (_configuration.UseValidationCache)
                    {
                        var cacheKey = GenerateCacheKey(columnName, value, rowIndex);
                        _validationCache.AddOrUpdate(cacheKey, DateTime.Now, (key, oldValue) => DateTime.Now);
                    }

                    _logger.LogDebug("✅ Background validation completed - Column: {ColumnName}, Valid: {IsValid}, Duration: {Duration}ms",
                        columnName, result.IsValid, result.DurationMs);

                    // Fire completion event
                    ValidationCompleted?.Invoke(this, new BackgroundValidationCompletedEventArgs
                    {
                        ColumnName = columnName,
                        RowIndex = rowIndex,
                        ValidationId = validationId,
                        Result = result,
                        DurationMs = result.DurationMs
                    });

                    ValidationStateChanged?.Invoke(this, new BackgroundValidationStateChangedEventArgs
                    {
                        ColumnName = columnName,
                        RowIndex = rowIndex,
                        ValidationId = validationId,
                        State = BackgroundValidationState.Completed
                    });

                    _diagnostics.TotalValidationsCompleted++;
                    UpdateAverageValidationTime(result.DurationMs);
                }
                finally
                {
                    _validationSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("🚫 Background validation cancelled - Column: {ColumnName}, Row: {RowIndex}",
                    columnName, rowIndex);

                ValidationStateChanged?.Invoke(this, new BackgroundValidationStateChangedEventArgs
                {
                    ColumnName = columnName,
                    RowIndex = rowIndex,
                    ValidationId = validationId,
                    State = BackgroundValidationState.Cancelled
                });

                _diagnostics.TotalValidationsCancelled++;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Background validation failed - Column: {ColumnName}, Row: {RowIndex}",
                    columnName, rowIndex);

                var errorResult = BackgroundValidationResult.Error($"Background validation error: {ex.Message}");
                errorResult.DurationMs = stopwatch.Elapsed.TotalMilliseconds;

                var resultKey = $"{columnName}_{rowIndex}";
                _validationResults.AddOrUpdate(resultKey, errorResult, (key, oldValue) => errorResult);

                ValidationStateChanged?.Invoke(this, new BackgroundValidationStateChangedEventArgs
                {
                    ColumnName = columnName,
                    RowIndex = rowIndex,
                    ValidationId = validationId,
                    State = BackgroundValidationState.Failed,
                    Message = ex.Message
                });

                _diagnostics.TotalValidationsFailed++;
            }
            finally
            {
                _runningValidations.TryRemove(validationKey, out _);
            }
        }

        public async Task CancelCellValidationAsync(string columnName, int rowIndex)
        {
            var validationKey = $"{columnName}_{rowIndex}";
            if (_runningValidations.TryRemove(validationKey, out var cts))
            {
                cts.Cancel();
                _logger.LogDebug("🚫 Cancelled background validation - Column: {ColumnName}, Row: {RowIndex}",
                    columnName, rowIndex);
            }
        }

        public async Task CancelRowValidationAsync(int rowIndex)
        {
            var keysToCancel = _runningValidations.Keys
                .Where(key => key.EndsWith($"_{rowIndex}"))
                .ToList();

            foreach (var key in keysToCancel)
            {
                if (_runningValidations.TryRemove(key, out var cts))
                {
                    cts.Cancel();
                }
            }

            _logger.LogDebug("🚫 Cancelled row background validations - Row: {RowIndex}, Count: {Count}",
                rowIndex, keysToCancel.Count);
        }

        public async Task CancelAllValidationsAsync()
        {
            var allKeys = _runningValidations.Keys.ToList();
            foreach (var key in allKeys)
            {
                if (_runningValidations.TryRemove(key, out var cts))
                {
                    cts.Cancel();
                }
            }

            _logger.LogDebug("🚫 Cancelled all background validations - Count: {Count}", allKeys.Count);
        }

        public BackgroundValidationResult? GetCellValidationResult(string columnName, int rowIndex)
        {
            var resultKey = $"{columnName}_{rowIndex}";
            return _validationResults.TryGetValue(resultKey, out var result) ? result : null;
        }

        public Dictionary<string, BackgroundValidationResult> GetRowValidationResults(int rowIndex)
        {
            return _validationResults
                .Where(kvp => kvp.Key.EndsWith($"_{rowIndex}"))
                .ToDictionary(
                    kvp => kvp.Key.Split('_')[0], // Extract column name
                    kvp => kvp.Value
                );
        }

        public bool IsValidationRunning(string columnName, int rowIndex)
        {
            var validationKey = $"{columnName}_{rowIndex}";
            return _runningValidations.ContainsKey(validationKey);
        }

        public bool IsRowValidationRunning(int rowIndex)
        {
            return _runningValidations.Keys.Any(key => key.EndsWith($"_{rowIndex}"));
        }

        public int GetRunningValidationsCount()
        {
            return _runningValidations.Count;
        }

        public async Task ClearValidationCacheAsync()
        {
            _validationCache.Clear();
            _validationResults.Clear();
            _logger.LogDebug("🧹 Background validation cache cleared");
        }

        public async Task UpdateConfigurationAsync(BackgroundValidationConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger.LogDebug("🔧 Background validation configuration updated");
        }

        public async Task AddBackgroundRuleAsync(BackgroundValidationRule rule)
        {
            var key = rule.ColumnName;
            _backgroundRules.AddOrUpdate(key, rule, (k, v) => rule);
            _logger.LogDebug("➕ Added background validation rule: {ColumnName}", rule.ColumnName);
        }

        public async Task RemoveBackgroundRuleAsync(string columnName)
        {
            if (_backgroundRules.TryRemove(columnName, out _))
            {
                _logger.LogDebug("➖ Removed background validation rule: {ColumnName}", columnName);
            }
        }

        public BackgroundValidationDiagnostics GetDiagnostics()
        {
            _diagnostics.CurrentlyRunning = _runningValidations.Count;
            return _diagnostics;
        }

        private string GenerateCacheKey(string columnName, object? value, int rowIndex)
        {
            var valueHash = value?.GetHashCode() ?? 0;
            return $"{columnName}_{valueHash}_{rowIndex}";
        }

        private bool IsValidationCached(string cacheKey)
        {
            if (!_validationCache.TryGetValue(cacheKey, out var cacheTime))
                return false;

            var ageMinutes = (DateTime.Now - cacheTime).TotalMinutes;
            return ageMinutes < _configuration.ValidationCacheMinutes;
        }

        private void UpdateAverageValidationTime(double durationMs)
        {
            var total = _diagnostics.TotalValidationsCompleted;
            if (total == 1)
            {
                _diagnostics.AverageValidationTimeMs = durationMs;
            }
            else
            {
                _diagnostics.AverageValidationTimeMs = 
                    (_diagnostics.AverageValidationTimeMs * (total - 1) + durationMs) / total;
            }
        }
    }
}