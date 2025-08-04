// Services/MemoryManagementService.cs - ✅ NOVÉ: Memory Management Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Row;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Memory Management Service - object pooling, weak references, garbage collection optimization
    /// </summary>
    internal class MemoryManagementService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Object pooling
        private readonly ConcurrentQueue<DataGridCell> _cellPool = new();
        private readonly ConcurrentQueue<RowDataModel> _rowPool = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _genericObjectPools = new();

        // ✅ NOVÉ: Weak reference cache management
        private readonly List<WeakReference> _weakReferences = new();
        private readonly Timer _cleanupTimer;
        private readonly Timer _gcMonitorTimer;

        // ✅ NOVÉ: Memory monitoring
        private long _lastTotalMemory = 0;
        private int _poolHitCount = 0;
        private int _poolMissCount = 0;
        private int _gcCollectionCount = 0;
        private readonly List<double> _memoryUsageHistory = new();

        // ✅ NOVÉ: Configuration
        private MemoryManagementConfiguration _config;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri memory pressure warning
        /// </summary>
        public event EventHandler<MemoryPressureEventArgs>? MemoryPressureWarning;

        /// <summary>
        /// Event vyvolaný pri GC collection
        /// </summary>
        public event EventHandler<GCCollectionEventArgs>? GCCollectionDetected;

        #endregion

        #region Constructor & Initialization

        public MemoryManagementService(
            MemoryManagementConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? MemoryManagementConfiguration.Default;

            // Setup cleanup timer
            _cleanupTimer = new Timer(PerformCleanup, null, 
                _config.CleanupIntervalMs, _config.CleanupIntervalMs);

            // Setup GC monitoring timer
            _gcMonitorTimer = new Timer(MonitorGarbageCollection, null,
                _config.GCMonitoringIntervalMs, _config.GCMonitoringIntervalMs);

            _logger.LogDebug("🚀 MemoryManagementService initialized - " +
                "ObjectPooling: {ObjectPooling}, WeakReferences: {WeakReferences}, " +
                "AutoGC: {AutoGC}", 
                _config.EnableObjectPooling, _config.EnableWeakReferences, _config.EnableAutomaticGC);
        }

        #endregion

        #region Object Pool Management

        /// <summary>
        /// ✅ NOVÉ: Get DataGridCell from pool or create new
        /// </summary>
        public DataGridCell GetDataGridCell()
        {
            if (!_config.EnableObjectPooling)
            {
                _poolMissCount++;
                return new DataGridCell();
            }

            if (_cellPool.TryDequeue(out var cell))
            {
                _poolHitCount++;
                ResetDataGridCell(cell);
                _logger.LogTrace("♻️ DataGridCell retrieved from pool - Pool size: {PoolSize}", _cellPool.Count);
                return cell;
            }

            _poolMissCount++;
            var newCell = new DataGridCell();
            _logger.LogTrace("🆕 New DataGridCell created - Pool empty");
            return newCell;
        }

        /// <summary>
        /// ✅ NOVÉ: Return DataGridCell to pool
        /// </summary>
        public void ReturnDataGridCell(DataGridCell cell)
        {
            if (!_config.EnableObjectPooling || cell == null)
                return;

            if (_cellPool.Count < _config.MaxPoolSize)
            {
                // Clean cell before returning to pool
                CleanDataGridCell(cell);
                _cellPool.Enqueue(cell);
                _logger.LogTrace("♻️ DataGridCell returned to pool - Pool size: {PoolSize}", _cellPool.Count);
            }
            else
            {
                _logger.LogTrace("🗑️ DataGridCell discarded - Pool full ({MaxSize})", _config.MaxPoolSize);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get RowDataModel from pool or create new
        /// </summary>
        public RowDataModel GetRowDataModel()
        {
            if (!_config.EnableObjectPooling)
            {
                _poolMissCount++;
                return new RowDataModel();
            }

            if (_rowPool.TryDequeue(out var row))
            {
                _poolHitCount++;
                ResetRowDataModel(row);
                _logger.LogTrace("♻️ RowDataModel retrieved from pool - Pool size: {PoolSize}", _rowPool.Count);
                return row;
            }

            _poolMissCount++;
            var newRow = new RowDataModel();
            _logger.LogTrace("🆕 New RowDataModel created - Pool empty");
            return newRow;
        }

        /// <summary>
        /// ✅ NOVÉ: Return RowDataModel to pool
        /// </summary>
        public void ReturnRowDataModel(RowDataModel row)
        {
            if (!_config.EnableObjectPooling || row == null)
                return;

            if (_rowPool.Count < _config.MaxPoolSize)
            {
                // Clean row before returning to pool
                CleanRowDataModel(row);
                _rowPool.Enqueue(row);
                _logger.LogTrace("♻️ RowDataModel returned to pool - Pool size: {PoolSize}", _rowPool.Count);
            }
            else
            {
                _logger.LogTrace("🗑️ RowDataModel discarded - Pool full ({MaxSize})", _config.MaxPoolSize);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Generic object pool support
        /// </summary>
        public T GetPooledObject<T>() where T : class, new()
        {
            if (!_config.EnableObjectPooling)
            {
                _poolMissCount++;
                return new T();
            }

            var type = typeof(T);
            if (_genericObjectPools.TryGetValue(type, out var pool) && pool.TryDequeue(out var obj))
            {
                _poolHitCount++;
                _logger.LogTrace("♻️ {Type} retrieved from generic pool - Pool size: {PoolSize}", 
                    type.Name, pool.Count);
                return (T)obj;
            }

            _poolMissCount++;
            var newObj = new T();
            _logger.LogTrace("🆕 New {Type} created - Generic pool empty", type.Name);
            return newObj;
        }

        /// <summary>
        /// ✅ NOVÉ: Return object to generic pool
        /// </summary>
        public void ReturnPooledObject<T>(T obj) where T : class
        {
            if (!_config.EnableObjectPooling || obj == null)
                return;

            var type = typeof(T);
            var pool = _genericObjectPools.GetOrAdd(type, _ => new ConcurrentQueue<object>());

            if (pool.Count < _config.MaxPoolSize)
            {
                pool.Enqueue(obj);
                _logger.LogTrace("♻️ {Type} returned to generic pool - Pool size: {PoolSize}", 
                    type.Name, pool.Count);
            }
            else
            {
                _logger.LogTrace("🗑️ {Type} discarded - Generic pool full ({MaxSize})", 
                    type.Name, _config.MaxPoolSize);
            }
        }

        #endregion

        #region Weak Reference Management

        /// <summary>
        /// ✅ NOVÉ: Add object to weak reference tracking
        /// </summary>
        public WeakReference AddWeakReference<T>(T target) where T : class
        {
            if (!_config.EnableWeakReferences || target == null)
                return new WeakReference(target);

            lock (_lockObject)
            {
                var weakRef = new WeakReference(target);
                _weakReferences.Add(weakRef);
                
                _logger.LogTrace("🔗 Weak reference added for {Type} - Total refs: {RefCount}", 
                    typeof(T).Name, _weakReferences.Count);
                
                return weakRef;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Clean up dead weak references
        /// </summary>
        public int CleanupWeakReferences()
        {
            if (!_config.EnableWeakReferences)
                return 0;

            lock (_lockObject)
            {
                var initialCount = _weakReferences.Count;
                _weakReferences.RemoveAll(wr => !wr.IsAlive);
                var removedCount = initialCount - _weakReferences.Count;

                if (removedCount > 0)
                {
                    _logger.LogDebug("🧹 Cleaned up {RemovedCount} dead weak references - " +
                        "Remaining: {RemainingCount}", removedCount, _weakReferences.Count);
                }

                return removedCount;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get live weak references count
        /// </summary>
        public int GetLiveWeakReferencesCount()
        {
            if (!_config.EnableWeakReferences)
                return 0;

            lock (_lockObject)
            {
                return _weakReferences.Count(wr => wr.IsAlive);
            }
        }

        #endregion

        #region Memory Monitoring & GC Management

        /// <summary>
        /// ✅ NOVÉ: Force garbage collection if needed
        /// </summary>
        public void ForceGarbageCollectionIfNeeded()
        {
            if (!_config.EnableAutomaticGC)
                return;

            var currentMemory = GC.GetTotalMemory(false);
            var memoryIncrease = currentMemory - _lastTotalMemory;
            var memoryThreshold = _config.GCTriggerThresholdBytes;

            if (memoryIncrease > memoryThreshold)
            {
                _logger.LogInformation("🗑️ Triggering garbage collection - " +
                    "Memory increase: {MemoryIncrease:N0} bytes (threshold: {Threshold:N0})", 
                    memoryIncrease, memoryThreshold);

                var beforeGC = GC.GetTotalMemory(false);
                var stopwatch = Stopwatch.StartNew();
                
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                stopwatch.Stop();
                var afterGC = GC.GetTotalMemory(false);
                var freedMemory = beforeGC - afterGC;

                _gcCollectionCount++;
                _logger.LogInformation("✅ Garbage collection completed - " +
                    "Duration: {Duration}ms, Freed: {FreedMemory:N0} bytes", 
                    stopwatch.ElapsedMilliseconds, freedMemory);

                GCCollectionDetected?.Invoke(this, new GCCollectionEventArgs(
                    beforeGC, afterGC, freedMemory, stopwatch.ElapsedMilliseconds));
            }

            _lastTotalMemory = currentMemory;
        }

        /// <summary>
        /// ✅ NOVÉ: Monitor memory usage and detect pressure
        /// </summary>
        public void MonitorMemoryUsage()
        {
            try
            {
                var currentMemory = GC.GetTotalMemory(false);
                var memoryMB = currentMemory / (1024.0 * 1024.0);
                
                _memoryUsageHistory.Add(memoryMB);
                
                // Keep only last 100 measurements
                if (_memoryUsageHistory.Count > 100)
                {
                    _memoryUsageHistory.RemoveAt(0);
                }

                // Calculate average memory usage
                var avgMemoryMB = _memoryUsageHistory.Average();
                var memoryGrowthRate = _memoryUsageHistory.Count > 1 
                    ? (memoryMB - _memoryUsageHistory[^2]) / _memoryUsageHistory[^2] * 100 
                    : 0;

                // Check for memory pressure
                if (memoryMB > _config.MemoryWarningThresholdMB)
                {
                    var warning = new MemoryPressureEventArgs(
                        currentMemory,
                        (long)(avgMemoryMB * 1024 * 1024),
                        memoryGrowthRate,
                        $"Memory usage {memoryMB:F1}MB exceeds threshold {_config.MemoryWarningThresholdMB}MB"
                    );

                    MemoryPressureWarning?.Invoke(this, warning);
                    
                    _logger.LogWarning("⚠️ Memory Pressure Warning - " +
                        "Current: {CurrentMB:F1}MB, Average: {AvgMB:F1}MB, Growth: {Growth:F1}%",
                        memoryMB, avgMemoryMB, memoryGrowthRate);

                    // Trigger emergency cleanup if enabled
                    if (_config.EnableEmergencyCleanup && memoryMB > _config.EmergencyCleanupThresholdMB)
                    {
                        PerformEmergencyCleanup();
                    }
                }

                if (_config.EnableDiagnostics)
                {
                    _logger.LogTrace("📊 Memory metrics - Current: {CurrentMB:F1}MB, " +
                        "Average: {AvgMB:F1}MB, Growth: {Growth:F1}%, Pool hits: {PoolHits}",
                        memoryMB, avgMemoryMB, memoryGrowthRate, _poolHitCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error monitoring memory usage");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// ✅ NOVÉ: Reset DataGridCell to default state
        /// </summary>
        private void ResetDataGridCell(DataGridCell cell)
        {
            try
            {
                // Reset properties to default values
                cell.Content = null;
                cell.DataContext = null;
                cell.Tag = null;
                
                // Clear any event handlers
                // Note: In real implementation, you'd need to properly unhook events
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error resetting DataGridCell");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Clean DataGridCell before returning to pool
        /// </summary>
        private void CleanDataGridCell(DataGridCell cell)
        {
            try
            {
                // Clear content and bindings
                cell.Content = null;
                cell.DataContext = null;
                cell.Tag = null;
                
                // Reset visual state
                cell.ClearValue(DataGridCell.BackgroundProperty);
                cell.ClearValue(DataGridCell.ForegroundProperty);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error cleaning DataGridCell");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Reset RowDataModel to default state
        /// </summary>
        private void ResetRowDataModel(RowDataModel row)
        {
            try
            {
                row.Cells?.Clear();
                row.RowIndex = -1;
                row.IsSelected = false;
                row.ValidationErrors = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error resetting RowDataModel");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Clean RowDataModel before returning to pool
        /// </summary>
        private void CleanRowDataModel(RowDataModel row)
        {
            try
            {
                row.Cells?.Clear();
                row.ValidationErrors = string.Empty;
                row.RowIndex = -1;
                row.IsSelected = false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error cleaning RowDataModel");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Periodic cleanup routine
        /// </summary>
        private async void PerformCleanup(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                _logger.LogTrace("🧹 Starting periodic memory cleanup");

                // Cleanup weak references
                var removedRefs = CleanupWeakReferences();
                
                // Monitor memory usage
                MonitorMemoryUsage();
                
                // Force GC if needed
                ForceGarbageCollectionIfNeeded();

                // Cleanup oversized pools
                CleanupOversizedPools();

                _logger.LogTrace("✅ Periodic cleanup completed - Removed refs: {RemovedRefs}", removedRefs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during periodic cleanup");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Monitor garbage collection activity
        /// </summary>
        private void MonitorGarbageCollection(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                var gen0 = GC.CollectionCount(0);
                var gen1 = GC.CollectionCount(1);
                var gen2 = GC.CollectionCount(2);

                if (_config.EnableDiagnostics)
                {
                    _logger.LogTrace("📊 GC Collections - Gen0: {Gen0}, Gen1: {Gen1}, Gen2: {Gen2}", 
                        gen0, gen1, gen2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error monitoring garbage collection");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Emergency cleanup when memory pressure is critical
        /// </summary>
        private void PerformEmergencyCleanup()
        {
            try
            {
                _logger.LogWarning("🚨 Performing emergency memory cleanup");

                // Clear all object pools
                _cellPool.Clear();
                _rowPool.Clear();
                
                foreach (var pool in _genericObjectPools.Values)
                {
                    pool.Clear();
                }

                // Force aggressive garbage collection
                GC.Collect(2, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true);

                // Cleanup weak references
                CleanupWeakReferences();

                _logger.LogWarning("⚠️ Emergency cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during emergency cleanup");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Cleanup pools that exceed maximum size
        /// </summary>
        private void CleanupOversizedPools()
        {
            try
            {
                // Trim cell pool if oversized
                while (_cellPool.Count > _config.MaxPoolSize)
                {
                    _cellPool.TryDequeue(out _);
                }

                // Trim row pool if oversized
                while (_rowPool.Count > _config.MaxPoolSize)
                {
                    _rowPool.TryDequeue(out _);
                }

                // Trim generic pools if oversized
                foreach (var pool in _genericObjectPools.Values)
                {
                    while (pool.Count > _config.MaxPoolSize)
                    {
                        pool.TryDequeue(out _);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error cleaning up oversized pools");
            }
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Konfigurácia memory management
        /// </summary>
        public MemoryManagementConfiguration Configuration => _config;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(MemoryManagementConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Memory management configuration updated");
            }
        }

        /// <summary>
        /// Získa štatistiky memory management
        /// </summary>
        public MemoryManagementStats GetStats()
        {
            var currentMemory = GC.GetTotalMemory(false);
            var avgMemory = _memoryUsageHistory.Count > 0 ? _memoryUsageHistory.Average() * 1024 * 1024 : 0;
            
            return new MemoryManagementStats
            {
                CurrentMemoryBytes = currentMemory,
                AverageMemoryBytes = (long)avgMemory,
                CellPoolSize = _cellPool.Count,
                RowPoolSize = _rowPool.Count,
                GenericPoolsCount = _genericObjectPools.Count,
                WeakReferencesCount = GetLiveWeakReferencesCount(),
                PoolHitCount = _poolHitCount,
                PoolMissCount = _poolMissCount,
                GCCollectionCount = _gcCollectionCount,
                PoolHitRatio = CalculatePoolHitRatio()
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Calculate pool hit ratio for performance monitoring
        /// </summary>
        private double CalculatePoolHitRatio()
        {
            var totalRequests = _poolHitCount + _poolMissCount;
            return totalRequests > 0 ? (double)_poolHitCount / totalRequests * 100 : 0;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    _cleanupTimer?.Dispose();
                    _gcMonitorTimer?.Dispose();
                    
                    // Clear all pools
                    _cellPool.Clear();
                    _rowPool.Clear();
                    _genericObjectPools.Clear();
                    
                    // Clear weak references
                    lock (_lockObject)
                    {
                        _weakReferences.Clear();
                    }

                    _logger.LogDebug("🔄 MemoryManagementService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during MemoryManagementService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// ✅ NOVÉ: Event args for memory pressure warnings
    /// </summary>
    public class MemoryPressureEventArgs : EventArgs
    {
        public long CurrentMemoryBytes { get; }
        public long AverageMemoryBytes { get; }
        public double GrowthRatePercent { get; }
        public string Message { get; }

        public MemoryPressureEventArgs(long currentMemory, long averageMemory, 
            double growthRate, string message)
        {
            CurrentMemoryBytes = currentMemory;
            AverageMemoryBytes = averageMemory;
            GrowthRatePercent = growthRate;
            Message = message;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for GC collection events
    /// </summary>
    public class GCCollectionEventArgs : EventArgs
    {
        public long MemoryBeforeBytes { get; }
        public long MemoryAfterBytes { get; }
        public long FreedMemoryBytes { get; }
        public long DurationMs { get; }

        public GCCollectionEventArgs(long memoryBefore, long memoryAfter, 
            long freedMemory, long duration)
        {
            MemoryBeforeBytes = memoryBefore;
            MemoryAfterBytes = memoryAfter;
            FreedMemoryBytes = freedMemory;
            DurationMs = duration;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Memory management statistics
    /// </summary>
    public class MemoryManagementStats
    {
        public long CurrentMemoryBytes { get; set; }
        public long AverageMemoryBytes { get; set; }
        public int CellPoolSize { get; set; }
        public int RowPoolSize { get; set; }
        public int GenericPoolsCount { get; set; }
        public int WeakReferencesCount { get; set; }
        public int PoolHitCount { get; set; }
        public int PoolMissCount { get; set; }
        public int GCCollectionCount { get; set; }
        public double PoolHitRatio { get; set; }

        public override string ToString()
        {
            var currentMB = CurrentMemoryBytes / (1024.0 * 1024.0);
            var avgMB = AverageMemoryBytes / (1024.0 * 1024.0);
            
            return $"MemoryManagement: Current={currentMB:F1}MB, Avg={avgMB:F1}MB, " +
                   $"Pools: Cell={CellPoolSize}, Row={RowPoolSize}, Generic={GenericPoolsCount}, " +
                   $"WeakRefs={WeakReferencesCount}, PoolHit={PoolHitRatio:F1}% ({PoolHitCount}/{PoolMissCount}), " +
                   $"GC={GCCollectionCount}";
        }
    }

    #endregion
}