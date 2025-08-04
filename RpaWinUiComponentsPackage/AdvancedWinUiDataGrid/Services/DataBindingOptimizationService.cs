// Services/DataBindingOptimizationService.cs - ✅ NOVÉ: Data Binding Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Data Binding Optimization Service - change tracking, throttling, bulk operations
    /// </summary>
    internal class DataBindingOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Change tracking system
        private readonly ConcurrentDictionary<string, ChangeTrackingInfo> _trackedObjects = new();
        private readonly ConcurrentQueue<PropertyChangeInfo> _pendingChanges = new();
        private readonly Timer _changeProcessingTimer;

        // ✅ NOVÉ: Bulk operations batching
        private readonly ConcurrentDictionary<string, BulkOperationBatch> _bulkBatches = new();
        private readonly Timer _bulkProcessingTimer;

        // ✅ NOVÉ: Property change throttling
        private readonly ConcurrentDictionary<string, ThrottleInfo> _throttledProperties = new();
        private readonly Timer _throttleTimer;

        // ✅ NOVÉ: Performance monitoring
        private int _totalChangeNotifications = 0;
        private int _throttledNotifications = 0;
        private int _bulkOperationsProcessed = 0;
        private readonly List<double> _changeProcessingTimes = new();

        // ✅ NOVÉ: Configuration
        private DataBindingOptimizationConfiguration _config;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri batch completion
        /// </summary>
        public event EventHandler<BatchCompletedEventArgs>? BatchCompleted;

        /// <summary>
        /// Event vyvolaný pri performance warning
        /// </summary>
        public event EventHandler<DataBindingPerformanceWarningEventArgs>? PerformanceWarning;

        /// <summary>
        /// Event vyvolaný pri property change
        /// </summary>
        public event EventHandler<OptimizedPropertyChangedEventArgs>? OptimizedPropertyChanged;

        #endregion

        #region Constructor & Initialization

        public DataBindingOptimizationService(
            DataBindingOptimizationConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? DataBindingOptimizationConfiguration.Default;

            // Setup timers
            _changeProcessingTimer = new Timer(ProcessPendingChanges, null,
                _config.ChangeProcessingIntervalMs, _config.ChangeProcessingIntervalMs);

            _bulkProcessingTimer = new Timer(ProcessBulkOperations, null,
                _config.BulkProcessingIntervalMs, _config.BulkProcessingIntervalMs);

            _throttleTimer = new Timer(ProcessThrottledChanges, null,
                _config.ThrottleIntervalMs, _config.ThrottleIntervalMs);

            _logger.LogDebug("🚀 DataBindingOptimizationService initialized - " +
                "ChangeTracking: {ChangeTracking}, Throttling: {Throttling}, BulkOps: {BulkOps}",
                _config.EnableChangeTracking, _config.EnablePropertyThrottling, _config.EnableBulkOperations);
        }

        #endregion

        #region Change Tracking

        /// <summary>
        /// ✅ NOVÉ: Register object for change tracking optimization
        /// </summary>
        public void RegisterForChangeTracking<T>(T obj, string objectId) where T : INotifyPropertyChanged
        {
            if (!_config.EnableChangeTracking || obj == null || string.IsNullOrEmpty(objectId))
                return;

            try
            {
                var trackingInfo = new ChangeTrackingInfo
                {
                    ObjectId = objectId,
                    ObjectReference = new WeakReference(obj),
                    PropertyValues = new ConcurrentDictionary<string, object?>(),
                    LastChangeTime = DateTime.UtcNow
                };

                // Capture initial property values
                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanRead && p.GetGetMethod()?.IsPublic == true);

                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(obj);
                        trackingInfo.PropertyValues[prop.Name] = value;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogTrace("⚠️ Error capturing initial value for {Property}: {Error}",
                            prop.Name, ex.Message);
                    }
                }

                _trackedObjects[objectId] = trackingInfo;

                // Subscribe to property changes
                obj.PropertyChanged += (sender, e) => OnTrackedObjectPropertyChanged(objectId, e);

                _logger.LogTrace("📝 Registered object for change tracking - ID: {ObjectId}, Properties: {PropertyCount}",
                    objectId, trackingInfo.PropertyValues.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error registering object for change tracking - ID: {ObjectId}", objectId);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Unregister object from change tracking
        /// </summary>
        public void UnregisterFromChangeTracking(string objectId)
        {
            if (_trackedObjects.TryRemove(objectId, out var trackingInfo))
            {
                _logger.LogTrace("🗑️ Unregistered object from change tracking - ID: {ObjectId}", objectId);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Handle property change from tracked object
        /// </summary>
        private void OnTrackedObjectPropertyChanged(string objectId, PropertyChangedEventArgs e)
        {
            if (!_config.EnableChangeTracking || string.IsNullOrEmpty(e.PropertyName))
                return;

            try
            {
                if (!_trackedObjects.TryGetValue(objectId, out var trackingInfo))
                    return;

                var obj = trackingInfo.ObjectReference.Target;
                if (obj == null)
                {
                    // Object was garbage collected
                    _trackedObjects.TryRemove(objectId, out _);
                    return;
                }

                // Get current property value
                var property = obj.GetType().GetProperty(e.PropertyName);
                if (property == null || !property.CanRead)
                    return;

                var newValue = property.GetValue(obj);
                var oldValue = trackingInfo.PropertyValues.TryGetValue(e.PropertyName, out var oldVal) ? oldVal : null;

                // Check if value actually changed
                if (Equals(oldValue, newValue))
                {
                    _logger.LogTrace("🚫 Ignored redundant property change - {ObjectId}.{Property}",
                        objectId, e.PropertyName);
                    return;
                }

                // Update tracked value
                trackingInfo.PropertyValues[e.PropertyName] = newValue;
                trackingInfo.LastChangeTime = DateTime.UtcNow;
                trackingInfo.ChangeCount++;

                // Queue change for processing
                var changeInfo = new PropertyChangeInfo
                {
                    ObjectId = objectId,
                    PropertyName = e.PropertyName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Timestamp = DateTime.UtcNow
                };

                _pendingChanges.Enqueue(changeInfo);
                _totalChangeNotifications++;

                _logger.LogTrace("📋 Queued property change - {ObjectId}.{Property}: {OldValue} → {NewValue}",
                    objectId, e.PropertyName, oldValue, newValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling property change - {ObjectId}.{Property}",
                    objectId, e.PropertyName);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get differential changes for object
        /// </summary>
        public Dictionary<string, object?> GetDifferentialChanges(string objectId)
        {
            var changes = new Dictionary<string, object?>();

            if (!_trackedObjects.TryGetValue(objectId, out var trackingInfo))
                return changes;

            var obj = trackingInfo.ObjectReference.Target;
            if (obj == null)
                return changes;

            var properties = obj.GetType().GetProperties()
                .Where(p => p.CanRead && p.GetGetMethod()?.IsPublic == true);

            foreach (var prop in properties)
            {
                try
                {
                    var currentValue = prop.GetValue(obj);
                    var trackedValue = trackingInfo.PropertyValues.TryGetValue(prop.Name, out var tracked) ? tracked : null;

                    if (!Equals(currentValue, trackedValue))
                    {
                        changes[prop.Name] = currentValue;
                        trackingInfo.PropertyValues[prop.Name] = currentValue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogTrace("⚠️ Error checking differential change for {Property}: {Error}",
                        prop.Name, ex.Message);
                }
            }

            if (changes.Count > 0)
            {
                _logger.LogDebug("🔄 Differential changes detected - {ObjectId}: {ChangeCount} properties",
                    objectId, changes.Count);
            }

            return changes;
        }

        #endregion

        #region Property Change Throttling

        /// <summary>
        /// ✅ NOVÉ: Register property for throttling
        /// </summary>
        public void RegisterPropertyThrottling(string objectId, string propertyName, int throttleMs = 0)
        {
            if (!_config.EnablePropertyThrottling || string.IsNullOrEmpty(objectId) || string.IsNullOrEmpty(propertyName))
                return;

            var key = $"{objectId}.{propertyName}";
            var throttleInfo = new ThrottleInfo
            {
                Key = key,
                ObjectId = objectId,
                PropertyName = propertyName,
                ThrottleMs = throttleMs > 0 ? throttleMs : _config.DefaultThrottleMs,
                LastNotification = DateTime.MinValue,
                PendingValue = null,
                IsPending = false
            };

            _throttledProperties[key] = throttleInfo;

            _logger.LogTrace("⏱️ Registered property throttling - {Key}, Throttle: {ThrottleMs}ms",
                key, throttleInfo.ThrottleMs);
        }

        /// <summary>
        /// ✅ NOVÉ: Process throttled property change
        /// </summary>
        public bool ProcessThrottledPropertyChange(string objectId, string propertyName, object? newValue)
        {
            if (!_config.EnablePropertyThrottling)
                return true; // No throttling - process immediately

            var key = $"{objectId}.{propertyName}";
            if (!_throttledProperties.TryGetValue(key, out var throttleInfo))
                return true; // Not throttled - process immediately

            var now = DateTime.UtcNow;
            var timeSinceLastNotification = now - throttleInfo.LastNotification;

            if (timeSinceLastNotification.TotalMilliseconds >= throttleInfo.ThrottleMs)
            {
                // Throttle period expired - process immediately
                throttleInfo.LastNotification = now;
                throttleInfo.IsPending = false;
                return true;
            }

            // Still in throttle period - queue for later
            throttleInfo.PendingValue = newValue;
            throttleInfo.IsPending = true;
            _throttledNotifications++;

            _logger.LogTrace("⏳ Throttled property change - {Key}, Remaining: {RemainingMs}ms",
                key, throttleInfo.ThrottleMs - timeSinceLastNotification.TotalMilliseconds);

            return false;
        }

        /// <summary>
        /// ✅ NOVÉ: Process throttled changes timer callback
        /// </summary>
        private void ProcessThrottledChanges(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                var now = DateTime.UtcNow;
                var processedCount = 0;

                foreach (var kvp in _throttledProperties)
                {
                    var throttleInfo = kvp.Value;
                    if (!throttleInfo.IsPending)
                        continue;

                    var timeSinceLastNotification = now - throttleInfo.LastNotification;
                    if (timeSinceLastNotification.TotalMilliseconds >= throttleInfo.ThrottleMs)
                    {
                        // Process pending change
                        var args = new OptimizedPropertyChangedEventArgs(throttleInfo.ObjectId,
                            throttleInfo.PropertyName, throttleInfo.PendingValue);

                        OptimizedPropertyChanged?.Invoke(this, args);

                        throttleInfo.LastNotification = now;
                        throttleInfo.IsPending = false;
                        throttleInfo.PendingValue = null;
                        processedCount++;
                    }
                }

                if (processedCount > 0)
                {
                    _logger.LogTrace("⏱️ Processed {Count} throttled property changes", processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing throttled changes");
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// ✅ NOVÉ: Start bulk operation batch
        /// </summary>
        public string StartBulkOperation(string operationType, string? description = null)
        {
            if (!_config.EnableBulkOperations)
                return string.Empty;

            var batchId = Guid.NewGuid().ToString("N")[..8];
            var batch = new BulkOperationBatch
            {
                BatchId = batchId,
                OperationType = operationType,
                Description = description ?? operationType,
                StartTime = DateTime.UtcNow,
                Operations = new List<BulkOperationInfo>()
            };

            _bulkBatches[batchId] = batch;

            _logger.LogDebug("📦 Started bulk operation batch - ID: {BatchId}, Type: {OperationType}",
                batchId, operationType);

            return batchId;
        }

        /// <summary>
        /// ✅ NOVÉ: Add operation to bulk batch
        /// </summary>
        public void AddToBulkOperation(string batchId, string operationName, object? data = null)
        {
            if (!_config.EnableBulkOperations || string.IsNullOrEmpty(batchId))
                return;

            if (!_bulkBatches.TryGetValue(batchId, out var batch))
            {
                _logger.LogWarning("⚠️ Bulk operation batch not found - ID: {BatchId}", batchId);
                return;
            }

            var operation = new BulkOperationInfo
            {
                OperationName = operationName,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            batch.Operations.Add(operation);

            _logger.LogTrace("➕ Added operation to bulk batch - {BatchId}: {OperationName}",
                batchId, operationName);
        }

        /// <summary>
        /// ✅ NOVÉ: Complete bulk operation batch
        /// </summary>
        public async Task<BulkOperationResult> CompleteBulkOperationAsync(string batchId)
        {
            if (!_config.EnableBulkOperations || string.IsNullOrEmpty(batchId))
                return BulkOperationResult.Empty;

            if (!_bulkBatches.TryRemove(batchId, out var batch))
            {
                _logger.LogWarning("⚠️ Bulk operation batch not found for completion - ID: {BatchId}", batchId);
                return BulkOperationResult.Empty;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                batch.EndTime = DateTime.UtcNow;
                var operationCount = batch.Operations.Count;

                // Process operations in batch
                if (_config.EnableParallelBulkProcessing && operationCount > _config.ParallelBulkThreshold)
                {
                    await ProcessBulkOperationsInParallel(batch);
                }
                else
                {
                    ProcessBulkOperationsSequentially(batch);
                }

                stopwatch.Stop();
                _bulkOperationsProcessed++;

                var result = new BulkOperationResult
                {
                    BatchId = batchId,
                    OperationType = batch.OperationType,
                    OperationCount = operationCount,
                    Duration = stopwatch.Elapsed,
                    IsSuccessful = true
                };

                _logger.LogInformation("✅ Completed bulk operation batch - ID: {BatchId}, " +
                    "Operations: {OperationCount}, Duration: {Duration}ms",
                    batchId, operationCount, stopwatch.ElapsedMilliseconds);

                BatchCompleted?.Invoke(this, new BatchCompletedEventArgs(result));

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error completing bulk operation batch - ID: {BatchId}", batchId);

                return new BulkOperationResult
                {
                    BatchId = batchId,
                    OperationType = batch.OperationType,
                    OperationCount = batch.Operations?.Count ?? 0,
                    Duration = stopwatch.Elapsed,
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Process bulk operations in parallel
        /// </summary>
        private async Task ProcessBulkOperationsInParallel(BulkOperationBatch batch)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _config.MaxParallelBulkOperations
            };

            await Task.Run(() =>
            {
                Parallel.ForEach(batch.Operations, parallelOptions, operation =>
                {
                    ProcessSingleBulkOperation(operation);
                });
            });

            _logger.LogTrace("🔄 Processed {Count} bulk operations in parallel", batch.Operations.Count);
        }

        /// <summary>
        /// ✅ NOVÉ: Process bulk operations sequentially
        /// </summary>
        private void ProcessBulkOperationsSequentially(BulkOperationBatch batch)
        {
            foreach (var operation in batch.Operations)
            {
                ProcessSingleBulkOperation(operation);
            }

            _logger.LogTrace("🔄 Processed {Count} bulk operations sequentially", batch.Operations.Count);
        }

        /// <summary>
        /// ✅ NOVÉ: Process single bulk operation
        /// </summary>
        private void ProcessSingleBulkOperation(BulkOperationInfo operation)
        {
            try
            {
                // Placeholder for actual bulk operation processing
                // This would be extended based on specific operation types
                _logger.LogTrace("⚙️ Processing bulk operation: {OperationName}", operation.OperationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing bulk operation: {OperationName}", operation.OperationName);
            }
        }

        #endregion

        #region Change Processing

        /// <summary>
        /// ✅ NOVÉ: Process pending changes timer callback
        /// </summary>
        private void ProcessPendingChanges(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                var processedCount = 0;
                var stopwatch = Stopwatch.StartNew();

                // Process up to configured batch size
                while (processedCount < _config.MaxChangesPerBatch && _pendingChanges.TryDequeue(out var change))
                {
                    ProcessSingleChange(change);
                    processedCount++;
                }

                stopwatch.Stop();

                if (processedCount > 0)
                {
                    UpdateChangeProcessingMetrics(stopwatch.ElapsedMilliseconds);
                    _logger.LogTrace("⚡ Processed {Count} pending changes in {Duration}ms",
                        processedCount, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing pending changes");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Process single change
        /// </summary>
        private void ProcessSingleChange(PropertyChangeInfo change)
        {
            try
            {
                // Check if this change should be throttled
                if (!ProcessThrottledPropertyChange(change.ObjectId, change.PropertyName, change.NewValue))
                    return; // Change was throttled

                // Raise optimized property changed event
                var args = new OptimizedPropertyChangedEventArgs(change.ObjectId, change.PropertyName, change.NewValue)
                {
                    OldValue = change.OldValue,
                    Timestamp = change.Timestamp
                };

                OptimizedPropertyChanged?.Invoke(this, args);

                _logger.LogTrace("🔄 Processed property change - {ObjectId}.{Property}",
                    change.ObjectId, change.PropertyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing single change - {ObjectId}.{Property}",
                    change.ObjectId, change.PropertyName);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Process bulk operations timer callback
        /// </summary>
        private void ProcessBulkOperations(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                var now = DateTime.UtcNow;
                var expiredBatches = new List<string>();

                // Find expired batches
                foreach (var kvp in _bulkBatches)
                {
                    var batch = kvp.Value;
                    var age = now - batch.StartTime;

                    if (age.TotalMilliseconds > _config.BulkOperationTimeoutMs)
                    {
                        expiredBatches.Add(kvp.Key);
                    }
                }

                // Process expired batches
                foreach (var batchId in expiredBatches)
                {
                    _logger.LogWarning("⏰ Auto-completing expired bulk operation batch - ID: {BatchId}", batchId);
                    _ = Task.Run(async () => await CompleteBulkOperationAsync(batchId));
                }

                if (expiredBatches.Count > 0)
                {
                    _logger.LogTrace("🧹 Auto-completed {Count} expired bulk operation batches", expiredBatches.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing bulk operations cleanup");
            }
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// ✅ NOVÉ: Update change processing metrics
        /// </summary>
        private void UpdateChangeProcessingMetrics(double durationMs)
        {
            _changeProcessingTimes.Add(durationMs);

            // Keep only last 100 measurements
            if (_changeProcessingTimes.Count > 100)
                _changeProcessingTimes.RemoveAt(0);

            // Check for performance warning
            if (durationMs > _config.PerformanceWarningThresholdMs)
            {
                var avgTime = _changeProcessingTimes.Average();
                var warning = new DataBindingPerformanceWarningEventArgs(
                    durationMs, avgTime, _pendingChanges.Count, _bulkBatches.Count,
                    $"Change processing time {durationMs:F2}ms exceeds threshold {_config.PerformanceWarningThresholdMs}ms"
                );

                PerformanceWarning?.Invoke(this, warning);

                _logger.LogWarning("⚠️ Data Binding Performance Warning - " +
                    "Processing: {ProcessingTime:F2}ms, Average: {AvgTime:F2}ms, " +
                    "Pending: {PendingCount}, Batches: {BatchCount}",
                    durationMs, avgTime, _pendingChanges.Count, _bulkBatches.Count);
            }
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Konfigurácia data binding optimization
        /// </summary>
        public DataBindingOptimizationConfiguration Configuration => _config;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(DataBindingOptimizationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Data binding optimization configuration updated");
            }
        }

        /// <summary>
        /// Získa štatistiky data binding optimization
        /// </summary>
        public DataBindingOptimizationStats GetStats()
        {
            var avgProcessingTime = _changeProcessingTimes.Count > 0 ? _changeProcessingTimes.Average() : 0;
            var throttleRatio = _totalChangeNotifications > 0 ? (double)_throttledNotifications / _totalChangeNotifications * 100 : 0;

            return new DataBindingOptimizationStats
            {
                TrackedObjectsCount = _trackedObjects.Count,
                PendingChangesCount = _pendingChanges.Count,
                TotalChangeNotifications = _totalChangeNotifications,
                ThrottledNotifications = _throttledNotifications,
                ThrottleRatio = throttleRatio,
                BulkOperationsProcessed = _bulkOperationsProcessed,
                ActiveBulkBatches = _bulkBatches.Count,
                AverageProcessingTimeMs = avgProcessingTime,
                RecentProcessingTimes = new List<double>(_changeProcessingTimes)
            };
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    _changeProcessingTimer?.Dispose();
                    _bulkProcessingTimer?.Dispose();
                    _throttleTimer?.Dispose();

                    // Clear all tracking
                    _trackedObjects.Clear();
                    _throttledProperties.Clear();

                    // Complete any remaining bulk operations
                    foreach (var batchId in _bulkBatches.Keys.ToList())
                    {
                        _ = Task.Run(async () => await CompleteBulkOperationAsync(batchId));
                    }

                    _logger.LogDebug("🔄 DataBindingOptimizationService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during DataBindingOptimizationService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// ✅ NOVÉ: Change tracking information
    /// </summary>
    internal class ChangeTrackingInfo
    {
        public string ObjectId { get; set; } = string.Empty;
        public WeakReference ObjectReference { get; set; } = null!;
        public ConcurrentDictionary<string, object?> PropertyValues { get; set; } = new();
        public DateTime LastChangeTime { get; set; }
        public int ChangeCount { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Property change information
    /// </summary>
    internal class PropertyChangeInfo
    {
        public string ObjectId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Throttle information for properties
    /// </summary>
    internal class ThrottleInfo
    {
        public string Key { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public int ThrottleMs { get; set; }
        public DateTime LastNotification { get; set; }
        public object? PendingValue { get; set; }
        public bool IsPending { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Bulk operation batch
    /// </summary>
    internal class BulkOperationBatch
    {
        public string BatchId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<BulkOperationInfo> Operations { get; set; } = new();
    }

    /// <summary>
    /// ✅ NOVÉ: Individual bulk operation
    /// </summary>
    internal class BulkOperationInfo
    {
        public string OperationName { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Bulk operation result
    /// </summary>
    public class BulkOperationResult
    {
        public string BatchId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public int OperationCount { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }

        public static BulkOperationResult Empty => new() { IsSuccessful = false };

        public override string ToString()
        {
            return $"BulkOperation: {OperationType} ({OperationCount} ops, {Duration.TotalMilliseconds:F0}ms, " +
                   $"Success: {IsSuccessful})";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Optimized property changed event args
    /// </summary>
    public class OptimizedPropertyChangedEventArgs : EventArgs
    {
        public string ObjectId { get; }
        public string PropertyName { get; }
        public object? NewValue { get; }
        public object? OldValue { get; set; }
        public DateTime Timestamp { get; set; }

        public OptimizedPropertyChangedEventArgs(string objectId, string propertyName, object? newValue)
        {
            ObjectId = objectId;
            PropertyName = propertyName;
            NewValue = newValue;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{ObjectId}.{PropertyName}: {OldValue} → {NewValue}";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Batch completed event args
    /// </summary>
    public class BatchCompletedEventArgs : EventArgs
    {
        public BulkOperationResult Result { get; }

        public BatchCompletedEventArgs(BulkOperationResult result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Performance warning event args
    /// </summary>
    public class DataBindingPerformanceWarningEventArgs : EventArgs
    {
        public double ProcessingTimeMs { get; }
        public double AverageProcessingTimeMs { get; }
        public int PendingChangesCount { get; }
        public int ActiveBatchesCount { get; }
        public string Message { get; }

        public DataBindingPerformanceWarningEventArgs(double processingTime, double averageProcessingTime,
            int pendingChangesCount, int activeBatchesCount, string message)
        {
            ProcessingTimeMs = processingTime;
            AverageProcessingTimeMs = averageProcessingTime;
            PendingChangesCount = pendingChangesCount;
            ActiveBatchesCount = activeBatchesCount;
            Message = message;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Data binding optimization statistics
    /// </summary>
    public class DataBindingOptimizationStats
    {
        public int TrackedObjectsCount { get; set; }
        public int PendingChangesCount { get; set; }
        public int TotalChangeNotifications { get; set; }
        public int ThrottledNotifications { get; set; }
        public double ThrottleRatio { get; set; }
        public int BulkOperationsProcessed { get; set; }
        public int ActiveBulkBatches { get; set; }
        public double AverageProcessingTimeMs { get; set; }
        public List<double> RecentProcessingTimes { get; set; } = new();

        public override string ToString()
        {
            return $"DataBindingOptimization: TrackedObjects={TrackedObjectsCount}, " +
                   $"PendingChanges={PendingChangesCount}, " +
                   $"Throttled={ThrottledNotifications}/{TotalChangeNotifications} ({ThrottleRatio:F1}%), " +
                   $"BulkOps={BulkOperationsProcessed}, ActiveBatches={ActiveBulkBatches}, " +
                   $"AvgProcessing={AverageProcessingTimeMs:F2}ms";
        }
    }

    #endregion
}