// Services/UIThreadOptimizationService.cs - ✅ NOVÉ: UI Thread Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
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
    /// ✅ NOVÉ: UI Thread Optimization Service - throttled updates, merging, time budgeting
    /// </summary>
    internal class UIThreadOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Update batching and merging
        private readonly ConcurrentDictionary<string, UIUpdateRequest> _pendingUpdates = new();
        private readonly Dictionary<UIUpdatePriority, Queue<UIUpdateRequest>> _priorityQueues = new();
        private readonly Timer _batchUpdateTimer;
        private readonly Timer _realtimeUpdateTimer;

        // ✅ NOVÉ: Performance tracking
        private readonly Stopwatch _frameStopwatch = new();
        private readonly List<double> _frameTimings = new();
        private double _averageFrameTime = 0;
        private int _processedUpdatesCount = 0;
        private int _mergedUpdatesCount = 0;
        private int _droppedUpdatesCount = 0;

        // ✅ NOVÉ: Configuration
        private UIThreadOptimizationConfiguration _config;
        private volatile bool _isProcessingUpdates = false;
        private volatile UIUpdateMode _currentMode = UIUpdateMode.Realtime;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri zmene update mode
        /// </summary>
        public event EventHandler<UIUpdateModeChangedEventArgs>? UpdateModeChanged;

        /// <summary>
        /// Event vyvolaný pri performance warning
        /// </summary>
        public event EventHandler<UIPerformanceWarningEventArgs>? PerformanceWarning;

        #endregion

        #region Constructor & Initialization

        public UIThreadOptimizationService(
            DispatcherQueue? dispatcherQueue = null,
            UIThreadOptimizationConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _dispatcherQueue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread();
            _config = config ?? UIThreadOptimizationConfiguration.Default;

            // Initialize priority queues
            foreach (UIUpdatePriority priority in Enum.GetValues<UIUpdatePriority>())
            {
                _priorityQueues[priority] = new Queue<UIUpdateRequest>();
            }

            // Setup timers
            _realtimeUpdateTimer = new Timer(ProcessRealtimeUpdates, null, 
                _config.RealtimeUpdateIntervalMs, _config.RealtimeUpdateIntervalMs);
            
            _batchUpdateTimer = new Timer(ProcessBatchUpdates, null, 
                _config.BatchUpdateIntervalMs, _config.BatchUpdateIntervalMs);

            _logger.LogDebug("🚀 UIThreadOptimizationService initialized - Mode: {Mode}, " +
                "RealtimeInterval: {RealtimeMs}ms, BatchInterval: {BatchMs}ms",
                _currentMode, _config.RealtimeUpdateIntervalMs, _config.BatchUpdateIntervalMs);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ✅ NOVÉ: Schedules UI update with priority and merging support
        /// </summary>
        public void ScheduleUIUpdate(UIUpdateRequest updateRequest)
        {
            try
            {
                if (_isDisposed || updateRequest == null)
                    return;

                var updateKey = GenerateUpdateKey(updateRequest);
                
                // ✅ NOVÉ: Merge updates for same UI element
                if (_pendingUpdates.TryGetValue(updateKey, out var existingUpdate))
                {
                    var mergedUpdate = MergeUpdates(existingUpdate, updateRequest);
                    _pendingUpdates[updateKey] = mergedUpdate;
                    _mergedUpdatesCount++;
                    
                    _logger.LogTrace("🔄 UI update merged - Key: {UpdateKey}, Priority: {Priority}",
                        updateKey, updateRequest.Priority);
                }
                else
                {
                    _pendingUpdates[updateKey] = updateRequest;
                    
                    lock (_lockObject)
                    {
                        _priorityQueues[updateRequest.Priority].Enqueue(updateRequest);
                    }

                    _logger.LogTrace("📝 UI update scheduled - Key: {UpdateKey}, Priority: {Priority}",
                        updateKey, updateRequest.Priority);
                }

                // ✅ NOVÉ: Auto-switch to batch mode if too many updates
                if (_pendingUpdates.Count > _config.BatchModeThreshold && _currentMode == UIUpdateMode.Realtime)
                {
                    SwitchUpdateMode(UIUpdateMode.Batch, "High update volume detected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error scheduling UI update");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Force immediate processing of high priority updates
        /// </summary>
        public async Task ProcessHighPriorityUpdatesAsync()
        {
            try
            {
                if (_isDisposed || _isProcessingUpdates)
                    return;

                _isProcessingUpdates = true;
                _frameStopwatch.Restart();

                var highPriorityUpdates = ExtractHighPriorityUpdates();
                if (highPriorityUpdates.Any())
                {
                    await ProcessUpdateBatch(highPriorityUpdates, _config.MaxFrameTimeMs);
                    _logger.LogDebug("⚡ Processed {Count} high priority updates",
                        highPriorityUpdates.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing high priority updates");
            }
            finally
            {
                _isProcessingUpdates = false;
                _frameStopwatch.Stop();
                UpdateFrameMetrics(_frameStopwatch.Elapsed.TotalMilliseconds);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Switch between realtime and batch update modes
        /// </summary>
        public void SwitchUpdateMode(UIUpdateMode newMode, string reason = "")
        {
            if (newMode == _currentMode)
                return;

            var oldMode = _currentMode;
            _currentMode = newMode;

            // Adjust timer intervals based on mode
            if (newMode == UIUpdateMode.Realtime)
            {
                _realtimeUpdateTimer.Change(_config.RealtimeUpdateIntervalMs, _config.RealtimeUpdateIntervalMs);
                _batchUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                _realtimeUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _batchUpdateTimer.Change(_config.BatchUpdateIntervalMs, _config.BatchUpdateIntervalMs);
            }

            _logger.LogInformation("🔄 UI update mode changed: {OldMode} → {NewMode} - Reason: {Reason}",
                oldMode, newMode, reason);

            UpdateModeChanged?.Invoke(this, new UIUpdateModeChangedEventArgs(oldMode, newMode, reason));
        }

        /// <summary>
        /// ✅ NOVÉ: Clear all pending updates (emergency measure)
        /// </summary>
        public void ClearPendingUpdates()
        {
            try
            {
                var pendingCount = _pendingUpdates.Count;
                _pendingUpdates.Clear();
                
                lock (_lockObject)
                {
                    foreach (var queue in _priorityQueues.Values)
                    {
                        queue.Clear();
                    }
                }

                _droppedUpdatesCount += pendingCount;
                _logger.LogWarning("🧹 Cleared {Count} pending UI updates", pendingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing pending updates");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ✅ NOVÉ: Process realtime updates (60 FPS)
        /// </summary>
        private async void ProcessRealtimeUpdates(object? state)
        {
            if (_isDisposed || _isProcessingUpdates || _currentMode != UIUpdateMode.Realtime)
                return;

            try
            {
                _isProcessingUpdates = true;
                _frameStopwatch.Restart();

                var updates = ExtractUpdatesForProcessing(_config.RealtimeUpdatesPerFrame);
                if (updates.Any())
                {
                    await ProcessUpdateBatch(updates, _config.MaxFrameTimeMs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in realtime update processing");
            }
            finally
            {
                _isProcessingUpdates = false;
                _frameStopwatch.Stop();
                UpdateFrameMetrics(_frameStopwatch.Elapsed.TotalMilliseconds);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Process batch updates (10 FPS)
        /// </summary>
        private async void ProcessBatchUpdates(object? state)
        {
            if (_isDisposed || _isProcessingUpdates || _currentMode != UIUpdateMode.Batch)
                return;

            try
            {
                _isProcessingUpdates = true;
                _frameStopwatch.Restart();

                var updates = ExtractUpdatesForProcessing(_config.BatchUpdatesPerFrame);
                if (updates.Any())
                {
                    await ProcessUpdateBatch(updates, _config.MaxBatchFrameTimeMs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in batch update processing");
            }
            finally
            {
                _isProcessingUpdates = false;
                _frameStopwatch.Stop();
                UpdateFrameMetrics(_frameStopwatch.Elapsed.TotalMilliseconds);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Extract updates for processing based on priority
        /// </summary>
        private List<UIUpdateRequest> ExtractUpdatesForProcessing(int maxUpdates)
        {
            var updates = new List<UIUpdateRequest>();

            lock (_lockObject)
            {
                // Process by priority: Critical -> High -> Medium -> Low
                foreach (var priority in new[] { UIUpdatePriority.Critical, UIUpdatePriority.High, 
                    UIUpdatePriority.Medium, UIUpdatePriority.Low })
                {
                    var queue = _priorityQueues[priority];
                    while (queue.Count > 0 && updates.Count < maxUpdates)
                    {
                        var update = queue.Dequeue();
                        var key = GenerateUpdateKey(update);
                        
                        // Remove from pending updates
                        _pendingUpdates.TryRemove(key, out _);
                        updates.Add(update);
                    }
                }
            }

            return updates;
        }

        /// <summary>
        /// ✅ NOVÉ: Extract only high priority updates for immediate processing
        /// </summary>
        private List<UIUpdateRequest> ExtractHighPriorityUpdates()
        {
            var updates = new List<UIUpdateRequest>();

            lock (_lockObject)
            {
                // Process only Critical and High priority
                foreach (var priority in new[] { UIUpdatePriority.Critical, UIUpdatePriority.High })
                {
                    var queue = _priorityQueues[priority];
                    while (queue.Count > 0)
                    {
                        var update = queue.Dequeue();
                        var key = GenerateUpdateKey(update);
                        
                        _pendingUpdates.TryRemove(key, out _);
                        updates.Add(update);
                    }
                }
            }

            return updates;
        }

        /// <summary>
        /// ✅ NOVÉ: Process batch of UI updates with time budgeting
        /// </summary>
        private async Task ProcessUpdateBatch(List<UIUpdateRequest> updates, double maxTimeMs)
        {
            if (!updates.Any())
                return;

            var batchStopwatch = Stopwatch.StartNew();
            var processedCount = 0;

            try
            {
                foreach (var update in updates)
                {
                    // ✅ NOVÉ: Time budgeting - check if we have time for more updates
                    if (batchStopwatch.Elapsed.TotalMilliseconds > maxTimeMs)
                    {
                        _logger.LogTrace("⏱️ Frame time budget exceeded - Processed: {Processed}/{Total}",
                            processedCount, updates.Count);
                        
                        // Re-queue remaining updates
                        for (int i = processedCount; i < updates.Count; i++)
                        {
                            ScheduleUIUpdate(updates[i]);
                        }
                        break;
                    }

                    await ProcessSingleUpdate(update);
                    processedCount++;
                }

                _processedUpdatesCount += processedCount;

                _logger.LogTrace("✅ Processed {Count} UI updates in {Time:F2}ms",
                    processedCount, batchStopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing UI update batch");
            }
            finally
            {
                batchStopwatch.Stop();
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Process single UI update on dispatcher thread
        /// </summary>
        private async Task ProcessSingleUpdate(UIUpdateRequest update)
        {
            try
            {
                if (_dispatcherQueue.HasThreadAccess)
                {
                    update.UpdateAction?.Invoke();
                }
                else
                {
                    var tcs = new TaskCompletionSource<bool>();
                    
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            update.UpdateAction?.Invoke();
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    });

                    await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error processing UI update for {ElementId}", 
                    update.ElementId ?? "unknown");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Generate unique key for update merging
        /// </summary>
        private string GenerateUpdateKey(UIUpdateRequest update)
        {
            return $"{update.ElementId}_{update.UpdateType}";
        }

        /// <summary>
        /// ✅ NOVÉ: Merge two updates for same element
        /// </summary>
        private UIUpdateRequest MergeUpdates(UIUpdateRequest existing, UIUpdateRequest newUpdate)
        {
            // Use higher priority
            var priority = (UIUpdatePriority)Math.Max((int)existing.Priority, (int)newUpdate.Priority);
            
            // Use newer timestamp
            var timestamp = Math.Max(existing.Timestamp, newUpdate.Timestamp);
            
            // Chain update actions
            Action? mergedAction = null;
            if (existing.UpdateAction != null && newUpdate.UpdateAction != null)
            {
                mergedAction = () =>
                {
                    existing.UpdateAction();
                    newUpdate.UpdateAction();
                };
            }
            else
            {
                mergedAction = newUpdate.UpdateAction ?? existing.UpdateAction;
            }

            return new UIUpdateRequest
            {
                ElementId = newUpdate.ElementId,
                UpdateType = newUpdate.UpdateType,
                Priority = priority,
                UpdateAction = mergedAction,
                Timestamp = timestamp,
                Data = newUpdate.Data ?? existing.Data
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Update frame timing metrics
        /// </summary>
        private void UpdateFrameMetrics(double frameTimeMs)
        {
            _frameTimings.Add(frameTimeMs);
            
            // Keep only last 100 measurements
            if (_frameTimings.Count > 100)
            {
                _frameTimings.RemoveAt(0);
            }

            _averageFrameTime = _frameTimings.Average();

            // ✅ NOVÉ: Performance warning detection
            if (frameTimeMs > _config.PerformanceWarningThresholdMs)
            {
                var warning = new UIPerformanceWarningEventArgs(
                    frameTimeMs,
                    _averageFrameTime,
                    _pendingUpdates.Count,
                    $"Frame time {frameTimeMs:F2}ms exceeds threshold {_config.PerformanceWarningThresholdMs}ms"
                );

                PerformanceWarning?.Invoke(this, warning);
                
                _logger.LogWarning("⚠️ UI Performance Warning - Frame: {FrameTime:F2}ms, " +
                    "Average: {AvgTime:F2}ms, Pending: {PendingCount}",
                    frameTimeMs, _averageFrameTime, _pendingUpdates.Count);
            }

            if (_config.EnableDiagnostics)
            {
                _logger.LogTrace("📊 Frame metrics - Time: {FrameTime:F2}ms, " +
                    "Average: {AvgTime:F2}ms, Processed: {ProcessedCount}, Merged: {MergedCount}",
                    frameTimeMs, _averageFrameTime, _processedUpdatesCount, _mergedUpdatesCount);
            }
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Aktuálny update mode
        /// </summary>
        public UIUpdateMode CurrentMode => _currentMode;

        /// <summary>
        /// Počet pending updates
        /// </summary>
        public int PendingUpdatesCount => _pendingUpdates.Count;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(UIThreadOptimizationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ UI thread optimization configuration updated");
            }
        }

        /// <summary>
        /// Získa štatistiky UI thread optimization
        /// </summary>
        public UIThreadOptimizationStats GetStats()
        {
            return new UIThreadOptimizationStats
            {
                CurrentMode = _currentMode,
                PendingUpdatesCount = _pendingUpdates.Count,
                ProcessedUpdatesCount = _processedUpdatesCount,
                MergedUpdatesCount = _mergedUpdatesCount,
                DroppedUpdatesCount = _droppedUpdatesCount,
                AverageFrameTime = _averageFrameTime,
                RecentFrameTimes = new List<double>(_frameTimings)
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
                    _realtimeUpdateTimer?.Dispose();
                    _batchUpdateTimer?.Dispose();
                    
                    ClearPendingUpdates();

                    _logger.LogDebug("🔄 UIThreadOptimizationService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during UIThreadOptimizationService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// ✅ NOVÉ: UI update mode enumeration
    /// </summary>
    public enum UIUpdateMode
    {
        Realtime,    // 60 FPS updates
        Batch        // 10 FPS updates
    }

    /// <summary>
    /// ✅ NOVÉ: UI update priority levels
    /// </summary>
    public enum UIUpdatePriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// ✅ NOVÉ: UI update request
    /// </summary>
    public class UIUpdateRequest
    {
        public string? ElementId { get; set; }
        public string UpdateType { get; set; } = string.Empty;
        public UIUpdatePriority Priority { get; set; } = UIUpdatePriority.Medium;
        public Action? UpdateAction { get; set; }
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public Dictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for update mode changes
    /// </summary>
    public class UIUpdateModeChangedEventArgs : EventArgs
    {
        public UIUpdateMode OldMode { get; }
        public UIUpdateMode NewMode { get; }
        public string Reason { get; }

        public UIUpdateModeChangedEventArgs(UIUpdateMode oldMode, UIUpdateMode newMode, string reason)
        {
            OldMode = oldMode;
            NewMode = newMode;
            Reason = reason;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for performance warnings
    /// </summary>
    public class UIPerformanceWarningEventArgs : EventArgs
    {
        public double FrameTime { get; }
        public double AverageFrameTime { get; }
        public int PendingUpdatesCount { get; }
        public string Message { get; }

        public UIPerformanceWarningEventArgs(double frameTime, double averageFrameTime, 
            int pendingUpdatesCount, string message)
        {
            FrameTime = frameTime;
            AverageFrameTime = averageFrameTime;
            PendingUpdatesCount = pendingUpdatesCount;
            Message = message;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: UI thread optimization statistics
    /// </summary>
    public class UIThreadOptimizationStats
    {
        public UIUpdateMode CurrentMode { get; set; }
        public int PendingUpdatesCount { get; set; }
        public int ProcessedUpdatesCount { get; set; }
        public int MergedUpdatesCount { get; set; }
        public int DroppedUpdatesCount { get; set; }
        public double AverageFrameTime { get; set; }
        public List<double> RecentFrameTimes { get; set; } = new();

        public override string ToString()
        {
            var fps = AverageFrameTime > 0 ? 1000.0 / AverageFrameTime : 0;
            return $"UIThreadOptimization: Mode={CurrentMode}, " +
                   $"Pending={PendingUpdatesCount}, Processed={ProcessedUpdatesCount}, " +
                   $"Merged={MergedUpdatesCount}, Dropped={DroppedUpdatesCount}, " +
                   $"AvgFrame={AverageFrameTime:F2}ms ({fps:F1}FPS)";
        }
    }

    #endregion
}