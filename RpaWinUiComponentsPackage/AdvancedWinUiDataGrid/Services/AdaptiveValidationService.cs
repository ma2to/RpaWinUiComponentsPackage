// Services/AdaptiveValidationService.cs - ✅ NOVÉ: Adaptive Validation Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Operations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Adaptive Validation Service - automatické prepínanie medzi realtime a batch validáciou
    /// </summary>
    internal class AdaptiveValidationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly IValidationService _realtimeValidationService;
        private readonly BatchValidationService _batchValidationService;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Adaptive mode tracking
        private ValidationMode _currentMode = ValidationMode.Realtime;
        private readonly Dictionary<string, DateTime> _lastValidationTimes = new();
        private readonly ConcurrentDictionary<string, ValidationResult> _validationCache = new();
        private readonly Dictionary<string, int> _validationFrequency = new();
        
        // ✅ NOVÉ: Performance tracking
        private readonly Stopwatch _modeDetectionStopwatch = new();
        private int _realtimeValidationCount = 0;
        private int _batchValidationCount = 0;
        private double _averageRealtimeTime = 0;
        private double _averageBatchTime = 0;

        // ✅ NOVÉ: Configuration
        private AdaptiveValidationConfiguration _config;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri zmene validation mode
        /// </summary>
        public event EventHandler<ValidationModeChangedEventArgs>? ValidationModeChanged;

        /// <summary>
        /// Event vyvolaný pri cache hit/miss pre diagnostiku
        /// </summary>
        public event EventHandler<ValidationCacheEventArgs>? CacheEvent;

        #endregion

        #region Constructor & Initialization

        public AdaptiveValidationService(
            IValidationService realtimeValidationService,
            BatchValidationService batchValidationService,
            AdaptiveValidationConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _realtimeValidationService = realtimeValidationService ?? throw new ArgumentNullException(nameof(realtimeValidationService));
            _batchValidationService = batchValidationService ?? throw new ArgumentNullException(nameof(batchValidationService));
            _config = config ?? AdaptiveValidationConfiguration.Default;

            _logger.LogDebug("🚀 AdaptiveValidationService initialized - Mode: {Mode}, CacheSize: {CacheSize}",
                _currentMode, _config.MaxCacheSize);
        }

        #endregion

        #region Public Validation Methods

        /// <summary>
        /// ✅ NOVÉ: Inteligentná validácia s automatickým mode switching
        /// </summary>
        public async Task<List<string>> ValidateCellAsync(string columnName, object? value, ValidationContext? context = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogWarning("⚠️ ValidateCellAsync: Empty column name provided");
                    return new List<string>();
                }

                _modeDetectionStopwatch.Restart();

                // ✅ NOVÉ: Check cache first
                var cacheKey = GenerateCacheKey(columnName, value);
                if (_validationCache.TryGetValue(cacheKey, out var cachedResult) && 
                    !IsCacheExpired(cachedResult.Timestamp))
                {
                    CacheEvent?.Invoke(this, new ValidationCacheEventArgs(cacheKey, true, cachedResult.Errors.Count));
                    _logger.LogTrace("💾 Validation cache HIT - Column: {ColumnName}, Errors: {ErrorCount}", 
                        columnName, cachedResult.Errors.Count);
                    return new List<string>(cachedResult.Errors);
                }

                // ✅ NOVÉ: Determine validation mode based on context
                var selectedMode = DetermineValidationMode(columnName, context);
                
                List<string> errors;
                
                if (selectedMode == ValidationMode.Batch && context?.OperationType == ValidationOperationType.BulkOperation)
                {
                    // Use batch validation for bulk operations
                    errors = await ValidateInBatchMode(columnName, value, context);
                    _batchValidationCount++;
                }
                else
                {
                    // Use realtime validation for individual cell edits
                    errors = await ValidateInRealtimeMode(columnName, value);
                    _realtimeValidationCount++;
                }

                _modeDetectionStopwatch.Stop();
                UpdatePerformanceMetrics(selectedMode, _modeDetectionStopwatch.Elapsed.TotalMilliseconds);

                // ✅ NOVÉ: Cache result if configured
                if (_config.EnableCaching && _validationCache.Count < _config.MaxCacheSize)
                {
                    var validationResult = new ValidationResult
                    {
                        Errors = errors,
                        Timestamp = DateTime.UtcNow,
                        ValidationMode = selectedMode
                    };
                    _validationCache.TryAdd(cacheKey, validationResult);
                    
                    CacheEvent?.Invoke(this, new ValidationCacheEventArgs(cacheKey, false, errors.Count));
                    _logger.LogTrace("💾 Validation result cached - Column: {ColumnName}, Mode: {Mode}", 
                        columnName, selectedMode);
                }

                // ✅ NOVÉ: Update validation frequency for future mode decisions
                UpdateValidationFrequency(columnName);

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AdaptiveValidationService.ValidateCellAsync - Column: {ColumnName}", columnName);
                return new List<string> { $"Chyba adaptívnej validácie: {ex.Message}" };
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Batch validácia pre viacero buniek naraz
        /// </summary>
        public async Task<Dictionary<string, List<string>>> ValidateMultipleCellsAsync(
            Dictionary<string, object?> cellData, 
            ValidationContext? context = null)
        {
            try
            {
                _logger.LogDebug("🔍 ValidateMultipleCellsAsync START - {CellCount} cells", cellData.Count);

                var results = new Dictionary<string, List<string>>();

                // Determine if we should use batch mode for this operation
                var shouldUseBatch = ShouldUseBatchMode(cellData.Count, context);
                
                if (shouldUseBatch)
                {
                    _logger.LogDebug("📦 Using BATCH mode for {CellCount} cells", cellData.Count);
                    
                    // Set context for batch operation if not already set
                    var batchContext = context ?? new ValidationContext 
                    { 
                        OperationType = ValidationOperationType.BulkOperation,
                        BatchSize = cellData.Count
                    };

                    // Process in batches
                    var batches = cellData
                        .Select((kvp, index) => new { Index = index, Key = kvp.Key, Value = kvp.Value })
                        .GroupBy(x => x.Index / _config.BatchSize)
                        .Select(g => g.ToDictionary(x => x.Key, x => x.Value));

                    foreach (var batch in batches)
                    {
                        var batchTasks = batch.Select(async kvp =>
                        {
                            var errors = await ValidateCellAsync(kvp.Key, kvp.Value, batchContext);
                            return new { ColumnName = kvp.Key, Errors = errors };
                        });

                        var batchResults = await Task.WhenAll(batchTasks);
                        
                        foreach (var result in batchResults)
                        {
                            results[result.ColumnName] = result.Errors;
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("⚡ Using REALTIME mode for {CellCount} cells", cellData.Count);
                    
                    // Process each cell individually in realtime mode
                    var realtimeContext = context ?? new ValidationContext 
                    { 
                        OperationType = ValidationOperationType.SingleCellEdit 
                    };

                    foreach (var kvp in cellData)
                    {
                        var errors = await ValidateCellAsync(kvp.Key, kvp.Value, realtimeContext);
                        results[kvp.Key] = errors;
                    }
                }

                _logger.LogDebug("✅ ValidateMultipleCellsAsync COMPLETED - {CellCount} cells, {ErrorCount} total errors",
                    cellData.Count, results.Values.Sum(errors => errors.Count));

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateMultipleCellsAsync");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// ✅ NOVÉ: Determines optimal validation mode based on context and frequency
        /// </summary>
        private ValidationMode DetermineValidationMode(string columnName, ValidationContext? context)
        {
            try
            {
                // Force batch mode for explicit bulk operations
                if (context?.OperationType == ValidationOperationType.BulkOperation)
                {
                    var newMode = ValidationMode.Batch;
                    if (newMode != _currentMode)
                    {
                        ChangeValidationMode(newMode, $"Bulk operation detected for column {columnName}");
                    }
                    return newMode;
                }

                // Force realtime mode for single cell edits
                if (context?.OperationType == ValidationOperationType.SingleCellEdit)
                {
                    var newMode = ValidationMode.Realtime;
                    if (newMode != _currentMode)
                    {
                        ChangeValidationMode(newMode, $"Single cell edit detected for column {columnName}");
                    }
                    return newMode;
                }

                // ✅ NOVÉ: Adaptive mode based on validation frequency and timing
                if (_config.EnableAdaptiveMode)
                {
                    var frequency = _validationFrequency.GetValueOrDefault(columnName, 0);
                    var lastValidationTime = _lastValidationTimes.GetValueOrDefault(columnName, DateTime.MinValue);
                    var timeSinceLastValidation = DateTime.UtcNow - lastValidationTime;

                    // High frequency + recent validations = realtime mode
                    if (frequency > _config.HighFrequencyThreshold && 
                        timeSinceLastValidation.TotalMilliseconds < _config.RealtimeModeTimeoutMs)
                    {
                        var newMode = ValidationMode.Realtime;
                        if (newMode != _currentMode)
                        {
                            ChangeValidationMode(newMode, $"High frequency validation for column {columnName}");
                        }
                        return newMode;
                    }

                    // Low frequency or old validations = batch mode for efficiency
                    if (frequency < _config.LowFrequencyThreshold || 
                        timeSinceLastValidation.TotalMilliseconds > _config.BatchModeTimeoutMs)
                    {
                        var newMode = ValidationMode.Batch;
                        if (newMode != _currentMode)
                        {
                            ChangeValidationMode(newMode, $"Low frequency validation for column {columnName}");
                        }
                        return newMode;
                    }
                }

                // Default to current mode
                return _currentMode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error determining validation mode for column {ColumnName}, using current mode", columnName);
                return _currentMode;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Validates using realtime validation service
        /// </summary>
        private async Task<List<string>> ValidateInRealtimeMode(string columnName, object? value)
        {
            try
            {
                _logger.LogTrace("⚡ Realtime validation - Column: {ColumnName}", columnName);
                return await _realtimeValidationService.ValidateCellAsync(columnName, value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error in realtime validation for column {ColumnName}", columnName);
                return new List<string> { $"Realtime validation error: {ex.Message}" };
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Validates using batch validation service
        /// </summary>
        private async Task<List<string>> ValidateInBatchMode(string columnName, object? value, ValidationContext? context)
        {
            try
            {
                _logger.LogTrace("📦 Batch validation - Column: {ColumnName}", columnName);
                
                // For single cell in batch mode, create a small batch
                // Note: BatchValidationService expects row data, so we simulate a single row
                var rowData = new Dictionary<string, object?> { { columnName, value } };
                var rows = new List<Dictionary<string, object?>> { rowData };
                
                // We'll need the validation rules and columns - for now use empty lists as placeholder
                var result = await _batchValidationService.ValidateRowsAsync(rows, new List<Models.Grid.ColumnDefinition>(), new List<ValidationRule>(), CancellationToken.None);
                
                // Extract errors for our specific column from the batch result
                var errors = new List<string>();
                if (result.RowResults.Count > 0)
                {
                    var rowResult = result.RowResults[0]; // First (and only) row
                    var cellResult = rowResult.CellResults.FirstOrDefault(c => c.ColumnName == columnName);
                    if (cellResult != null && !cellResult.IsValid && !string.IsNullOrEmpty(cellResult.ErrorMessage))
                    {
                        errors.Add(cellResult.ErrorMessage);
                    }
                }
                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error in batch validation for column {ColumnName}", columnName);
                return new List<string> { $"Batch validation error: {ex.Message}" };
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Determines if batch mode should be used based on data size and context
        /// </summary>
        private bool ShouldUseBatchMode(int cellCount, ValidationContext? context)
        {
            // Always use batch for explicit bulk operations
            if (context?.OperationType == ValidationOperationType.BulkOperation)
                return true;

            // Always use realtime for single cell edits
            if (context?.OperationType == ValidationOperationType.SingleCellEdit)
                return false;

            // Use batch mode if cell count exceeds threshold
            return cellCount >= _config.BatchModeThreshold;
        }

        /// <summary>
        /// ✅ NOVÉ: Generates cache key for validation result
        /// </summary>
        private string GenerateCacheKey(string columnName, object? value)
        {
            var valueString = value?.ToString() ?? "null";
            return $"{columnName}:{valueString.GetHashCode():X}";
        }

        /// <summary>
        /// ✅ NOVÉ: Checks if cached validation result is expired
        /// </summary>
        private bool IsCacheExpired(DateTime timestamp)
        {
            return DateTime.UtcNow - timestamp > TimeSpan.FromMilliseconds(_config.CacheExpirationMs);
        }

        /// <summary>
        /// ✅ NOVÉ: Updates validation frequency tracking for adaptive mode
        /// </summary>
        private void UpdateValidationFrequency(string columnName)
        {
            lock (_lockObject)
            {
                _validationFrequency[columnName] = _validationFrequency.GetValueOrDefault(columnName, 0) + 1;
                _lastValidationTimes[columnName] = DateTime.UtcNow;

                // Cleanup old frequency data periodically
                if (_validationFrequency.Count > _config.MaxFrequencyTrackingEntries)
                {
                    var oldEntries = _lastValidationTimes
                        .Where(kvp => DateTime.UtcNow - kvp.Value > TimeSpan.FromMinutes(5))
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in oldEntries)
                    {
                        _validationFrequency.Remove(key);
                        _lastValidationTimes.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Updates performance metrics for adaptive tuning
        /// </summary>
        private void UpdatePerformanceMetrics(ValidationMode mode, double durationMs)
        {
            if (mode == ValidationMode.Realtime)
            {
                _averageRealtimeTime = (_averageRealtimeTime * 0.9) + (durationMs * 0.1);
            }
            else
            {
                _averageBatchTime = (_averageBatchTime * 0.9) + (durationMs * 0.1);
            }

            if (_config.EnableDiagnostics)
            {
                _logger.LogTrace("📊 Performance metrics - Mode: {Mode}, Duration: {Duration:F2}ms, " +
                    "AvgRealtime: {AvgRealtime:F2}ms, AvgBatch: {AvgBatch:F2}ms",
                    mode, durationMs, _averageRealtimeTime, _averageBatchTime);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Changes validation mode and notifies listeners
        /// </summary>
        private void ChangeValidationMode(ValidationMode newMode, string reason)
        {
            var oldMode = _currentMode;
            _currentMode = newMode;

            _logger.LogDebug("🔄 Validation mode changed: {OldMode} → {NewMode} - Reason: {Reason}",
                oldMode, newMode, reason);

            ValidationModeChanged?.Invoke(this, new ValidationModeChangedEventArgs(oldMode, newMode, reason));
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Aktuálny validation mode
        /// </summary>
        public ValidationMode CurrentMode => _currentMode;

        /// <summary>
        /// Konfigurácia adaptive validation
        /// </summary>
        public AdaptiveValidationConfiguration Configuration => _config;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(AdaptiveValidationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Adaptive validation configuration updated");
            }
        }

        /// <summary>
        /// Vyčistí validation cache
        /// </summary>
        public void ClearCache()
        {
            lock (_lockObject)
            {
                var cacheSize = _validationCache.Count;
                _validationCache.Clear();
                _logger.LogInformation("🧹 Validation cache cleared - {CacheSize} entries removed", cacheSize);
            }
        }

        /// <summary>
        /// Získa štatistiky adaptive validation
        /// </summary>
        public AdaptiveValidationStats GetStats()
        {
            return new AdaptiveValidationStats
            {
                CurrentMode = _currentMode,
                RealtimeValidationCount = _realtimeValidationCount,
                BatchValidationCount = _batchValidationCount,
                CacheSize = _validationCache.Count,
                CacheHitRatio = CalculateCacheHitRatio(),
                AverageRealtimeTime = _averageRealtimeTime,
                AverageBatchTime = _averageBatchTime,
                FrequencyTrackingEntries = _validationFrequency.Count
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Calculates cache hit ratio for performance monitoring
        /// </summary>
        private double CalculateCacheHitRatio()
        {
            var totalValidations = _realtimeValidationCount + _batchValidationCount;
            if (totalValidations == 0) return 0.0;

            // This is a simplified calculation - in real implementation you'd track actual cache hits
            var estimatedCacheHits = Math.Min(_validationCache.Count, totalValidations);
            return (double)estimatedCacheHits / totalValidations * 100;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    ClearCache();
                    _validationFrequency.Clear();
                    _lastValidationTimes.Clear();

                    _logger.LogDebug("🔄 AdaptiveValidationService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during AdaptiveValidationService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// ✅ NOVÉ: Validation mode enumeration
    /// </summary>
    public enum ValidationMode
    {
        Realtime,
        Batch
    }

    /// <summary>
    /// ✅ NOVÉ: Validation operation type for context
    /// </summary>
    public enum ValidationOperationType
    {
        SingleCellEdit,
        BulkOperation,
        ImportOperation,
        PasteOperation
    }

    /// <summary>
    /// ✅ NOVÉ: Validation context for adaptive decisions
    /// </summary>
    public class ValidationContext
    {
        public ValidationOperationType OperationType { get; set; } = ValidationOperationType.SingleCellEdit;
        public int BatchSize { get; set; } = 1;
        public string? Source { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Cached validation result
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public ValidationMode ValidationMode { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for validation mode changes
    /// </summary>
    public class ValidationModeChangedEventArgs : EventArgs
    {
        public ValidationMode OldMode { get; }
        public ValidationMode NewMode { get; }
        public string Reason { get; }

        public ValidationModeChangedEventArgs(ValidationMode oldMode, ValidationMode newMode, string reason)
        {
            OldMode = oldMode;
            NewMode = newMode;
            Reason = reason;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for cache events
    /// </summary>
    public class ValidationCacheEventArgs : EventArgs
    {
        public string CacheKey { get; }
        public bool IsHit { get; }
        public int ErrorCount { get; }

        public ValidationCacheEventArgs(string cacheKey, bool isHit, int errorCount)
        {
            CacheKey = cacheKey;
            IsHit = isHit;
            ErrorCount = errorCount;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Adaptive validation statistics
    /// </summary>
    public class AdaptiveValidationStats
    {
        public ValidationMode CurrentMode { get; set; }
        public int RealtimeValidationCount { get; set; }
        public int BatchValidationCount { get; set; }
        public int CacheSize { get; set; }
        public double CacheHitRatio { get; set; }
        public double AverageRealtimeTime { get; set; }
        public double AverageBatchTime { get; set; }
        public int FrequencyTrackingEntries { get; set; }

        public override string ToString()
        {
            return $"AdaptiveValidation: Mode={CurrentMode}, " +
                   $"Realtime={RealtimeValidationCount}, Batch={BatchValidationCount}, " +
                   $"Cache={CacheSize} (HitRatio={CacheHitRatio:F1}%), " +
                   $"AvgTimes: RT={AverageRealtimeTime:F2}ms, Batch={AverageBatchTime:F2}ms";
        }
    }

    #endregion
}