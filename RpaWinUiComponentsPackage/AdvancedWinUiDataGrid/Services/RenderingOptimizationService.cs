// Services/RenderingOptimizationService.cs - ✅ NOVÉ: Rendering Pipeline Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Rendering Pipeline Optimization Service - GPU acceleration, virtualization, render batching
    /// </summary>
    internal class RenderingOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Configuration
        private RenderingOptimizationConfiguration _config;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // ✅ NOVÉ: Render pipeline management
        private readonly ConcurrentDictionary<string, RenderBatch> _renderBatches = new();
        private readonly ConcurrentDictionary<string, VirtualizedElement> _virtualizedElements = new();
        private readonly ConcurrentDictionary<string, RenderCacheEntry> _renderCache = new();
        private readonly Timer _renderBatchTimer;
        private readonly Timer _cacheCleanupTimer;

        // ✅ NOVÉ: Performance monitoring
        private long _totalRenderOperations = 0;
        private long _totalFramesRendered = 0;
        private long _cacheHits = 0;
        private long _cacheMisses = 0;
        private readonly Stopwatch _frameTimeStopwatch = new();
        private readonly Queue<double> _recentFrameTimes = new();

        // ✅ NOVÉ: Virtualization tracking
        private Rect _currentViewport = Rect.Empty;
        private readonly ConcurrentDictionary<int, VirtualizedRow> _virtualizedRows = new();
        private int _currentRenderGeneration = 0;

        // ✅ NOVÉ: GPU acceleration support
        private bool _isGpuAccelerationAvailable = false;

        // ✅ NOVÉ: Level-of-detail management
        private readonly ConcurrentDictionary<string, LodLevel> _lodLevels = new();

        #endregion

        #region Constructor

        public RenderingOptimizationService(ILogger? logger = null, RenderingOptimizationConfiguration? config = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? RenderingOptimizationConfiguration.Default;
            _config.Validate();

            _renderBatchTimer = new Timer(ProcessRenderBatches, null, _config.RenderBatchTimeoutMs, _config.RenderBatchTimeoutMs);
            _cacheCleanupTimer = new Timer(CleanupRenderCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

            InitializeGpuAcceleration();

            _logger.LogInformation("🎨 RenderingOptimizationService initialized - InstanceId: {InstanceId}, Config: {Config}",
                _serviceInstanceId, _config.ToString());
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Service je inicializovaný
        /// </summary>
        public bool IsInitialized => !_isDisposed;

        /// <summary>
        /// Počet aktívnych render batches
        /// </summary>
        public int ActiveRenderBatchesCount => _renderBatches.Count;

        /// <summary>
        /// Počet virtualizovaných elementov
        /// </summary>
        public int VirtualizedElementsCount => _virtualizedElements.Count;

        /// <summary>
        /// Cache hit ratio
        /// </summary>
        public double CacheHitRatio => (_cacheHits + _cacheMisses) > 0 ? (double)_cacheHits / (_cacheHits + _cacheMisses) : 0;

        /// <summary>
        /// Average frame time (ms)
        /// </summary>
        public double AverageFrameTime => _recentFrameTimes.Count > 0 ? _recentFrameTimes.Average() : 0;

        /// <summary>
        /// Current FPS
        /// </summary>
        public double CurrentFps => AverageFrameTime > 0 ? 1000.0 / AverageFrameTime : 0;

        #endregion

        #region Render Batching

        /// <summary>
        /// ✅ NOVÉ: Queue render operation for batching
        /// </summary>
        public void QueueRenderOperation(string elementId, RenderOperationType operationType, FrameworkElement element, Rect bounds)
        {
            if (!_config.IsEnabled || !_config.EnableRenderBatching)
            {
                // Direct rendering without batching
                ProcessSingleRenderOperation(elementId, operationType, element, bounds);
                return;
            }

            var batchKey = GetBatchKey(operationType, bounds);
            var batch = _renderBatches.GetOrAdd(batchKey, _ => new RenderBatch
            {
                BatchId = Guid.NewGuid().ToString("N")[..8],
                OperationType = operationType,
                CreatedTime = DateTime.UtcNow,
                Operations = new ConcurrentQueue<RenderOperation>()
            });

            var operation = new RenderOperation
            {
                ElementId = elementId,
                Element = element,
                Bounds = bounds,
                QueuedTime = DateTime.UtcNow,
                Priority = GetRenderPriority(operationType, bounds)
            };

            batch.Operations.Enqueue(operation);

            _logger.LogTrace("🎨 Queued render operation - Element: {ElementId}, Type: {Type}, Batch: {BatchId}",
                elementId, operationType, batch.BatchId);

            // Process batch immediately if it's full
            if (batch.Operations.Count >= _config.MaxRenderBatchSize)
            {
                ProcessRenderBatch(batch);
                _renderBatches.TryRemove(batchKey, out _);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Process single render operation
        /// </summary>
        private void ProcessSingleRenderOperation(string elementId, RenderOperationType operationType, FrameworkElement element, Rect bounds)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Check render cache first
                var cacheKey = GetRenderCacheKey(elementId, operationType, bounds);
                if (_config.EnableRenderCaching && _renderCache.TryGetValue(cacheKey, out var cachedEntry))
                {
                    if (IsRenderCacheValid(cachedEntry))
                    {
                        ApplyCachedRender(element, cachedEntry);
                        Interlocked.Increment(ref _cacheHits);
                        _logger.LogTrace("✅ Applied cached render - Element: {ElementId}, Type: {Type}",
                            elementId, operationType);
                        return;
                    }
                    else
                    {
                        _renderCache.TryRemove(cacheKey, out _);
                    }
                }

                Interlocked.Increment(ref _cacheMisses);

                // Perform actual rendering
                PerformRenderOperation(element, operationType, bounds);

                // Cache result if beneficial
                if (_config.EnableRenderCaching && ShouldCacheRender(operationType, bounds))
                {
                    CacheRenderResult(cacheKey, element, operationType, bounds);
                }

                Interlocked.Increment(ref _totalRenderOperations);
                
                stopwatch.Stop();
                UpdateFrameTime(stopwatch.Elapsed.TotalMilliseconds);

                _logger.LogTrace("🎨 Processed render operation - Element: {ElementId}, Type: {Type}, Duration: {Duration}ms",
                    elementId, operationType, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing render operation - Element: {ElementId}, Type: {Type}",
                    elementId, operationType);
            }
        }

        /// <summary>
        /// Process render batches on timer
        /// </summary>
        private void ProcessRenderBatches(object? state)
        {
            if (_isDisposed || _renderBatches.IsEmpty) return;

            try
            {
                var batchesToProcess = _renderBatches.ToList();
                
                foreach (var kvp in batchesToProcess)
                {
                    if (_renderBatches.TryRemove(kvp.Key, out var batch))
                    {
                        _ = Task.Run(() => ProcessRenderBatch(batch));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing render batches");
            }
        }

        /// <summary>
        /// Process individual render batch
        /// </summary>
        private void ProcessRenderBatch(RenderBatch batch)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var operations = new List<RenderOperation>();

                // Drain all operations from the batch
                while (batch.Operations.TryDequeue(out var operation))
                {
                    operations.Add(operation);
                }

                if (operations.Count == 0) return;

                _logger.LogDebug("🎨 Processing render batch - BatchId: {BatchId}, Operations: {Count}",
                    batch.BatchId, operations.Count);

                // Sort by priority and render
                var sortedOperations = operations.OrderByDescending(op => op.Priority).ToList();

                foreach (var operation in sortedOperations)
                {
                    ProcessSingleRenderOperation(operation.ElementId, batch.OperationType, operation.Element, operation.Bounds);
                }

                stopwatch.Stop();
                _logger.LogDebug("✅ Render batch completed - BatchId: {BatchId}, Duration: {Duration}ms",
                    batch.BatchId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing render batch - BatchId: {BatchId}", batch.BatchId);
            }
        }

        #endregion

        #region Virtualization

        /// <summary>
        /// ✅ NOVÉ: Update viewport for virtualization
        /// </summary>
        public void UpdateViewport(Rect viewport)
        {
            if (!_config.IsEnabled || !_config.EnableVirtualization) return;

            _currentViewport = viewport;
            _currentRenderGeneration++;

            _logger.LogTrace("🔍 Viewport updated - X: {X}, Y: {Y}, W: {Width}, H: {Height}",
                viewport.X, viewport.Y, viewport.Width, viewport.Height);

            // Update virtualized elements visibility
            _ = Task.Run(UpdateVirtualizedElementsVisibility);
        }

        /// <summary>
        /// ✅ NOVÉ: Register element for virtualization
        /// </summary>
        public void RegisterVirtualizedElement(string elementId, FrameworkElement element, Rect bounds, int dataIndex)
        {
            if (!_config.IsEnabled || !_config.EnableVirtualization) return;

            var virtualizedElement = new VirtualizedElement
            {
                ElementId = elementId,
                Element = element,
                Bounds = bounds,
                DataIndex = dataIndex,
                IsVisible = IsElementInViewport(bounds),
                LastAccessTime = DateTime.UtcNow,
                RenderGeneration = _currentRenderGeneration
            };

            _virtualizedElements[elementId] = virtualizedElement;

            _logger.LogTrace("📋 Registered virtualized element - Element: {ElementId}, Visible: {IsVisible}",
                elementId, virtualizedElement.IsVisible);

            // Update element visibility
            UpdateElementVisibility(virtualizedElement);
        }

        /// <summary>
        /// Update virtualized elements visibility based on viewport
        /// </summary>
        private async Task UpdateVirtualizedElementsVisibility()
        {
            try
            {
                var elementsToUpdate = _virtualizedElements.Values
                    .Where(ve => ve.RenderGeneration != _currentRenderGeneration)
                    .ToList();

                var tasks = elementsToUpdate
                    .Take(_config.MaxConcurrentRenderingTasks)
                    .Select(async ve =>
                    {
                        var wasVisible = ve.IsVisible;
                        ve.IsVisible = IsElementInViewport(ve.Bounds);
                        ve.RenderGeneration = _currentRenderGeneration;

                        if (wasVisible != ve.IsVisible)
                        {
                            await UpdateElementVisibilityAsync(ve);
                        }
                    });

                await Task.WhenAll(tasks);

                _logger.LogTrace("🔄 Updated {Count} virtualized elements visibility", elementsToUpdate.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating virtualized elements visibility");
            }
        }

        /// <summary>
        /// Update element visibility
        /// </summary>
        private void UpdateElementVisibility(VirtualizedElement virtualizedElement)
        {
            try
            {
                if (virtualizedElement.Element == null) return;

                if (virtualizedElement.IsVisible)
                {
                    // Make element visible
                    virtualizedElement.Element.Visibility = Visibility.Visible;
                    
                    if (_config.EnableLazyLoading)
                    {
                        // Trigger lazy loading if needed
                        TriggerLazyLoading(virtualizedElement);
                    }
                }
                else
                {
                    // Hide element to save resources
                    virtualizedElement.Element.Visibility = Visibility.Collapsed;
                }

                virtualizedElement.LastAccessTime = DateTime.UtcNow;

                _logger.LogTrace("👁️ Updated element visibility - Element: {ElementId}, Visible: {IsVisible}",
                    virtualizedElement.ElementId, virtualizedElement.IsVisible);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating element visibility - Element: {ElementId}",
                    virtualizedElement.ElementId);
            }
        }

        /// <summary>
        /// Update element visibility async
        /// </summary>
        private async Task UpdateElementVisibilityAsync(VirtualizedElement virtualizedElement)
        {
            await Task.Run(() => UpdateElementVisibility(virtualizedElement));
        }

        #endregion

        #region Level of Detail (LOD)

        /// <summary>
        /// ✅ NOVÉ: Calculate LOD level based on distance/scale
        /// </summary>
        public LodLevel CalculateLodLevel(Rect elementBounds, double viewportScale = 1.0)
        {
            if (!_config.IsEnabled || !_config.EnableLevelOfDetail)
                return LodLevel.High;

            var elementSize = Math.Max(elementBounds.Width, elementBounds.Height) * viewportScale;

            for (int i = 0; i < _config.LodDistanceThresholds.Length; i++)
            {
                if (elementSize >= _config.LodDistanceThresholds[i])
                {
                    return (LodLevel)Math.Min(i, 3); // Max LOD level is 3 (Ultra)
                }
            }

            return LodLevel.Low;
        }

        /// <summary>
        /// ✅ NOVÉ: Apply LOD rendering
        /// </summary>
        public void ApplyLodRendering(string elementId, FrameworkElement element, LodLevel lodLevel)
        {
            if (!_config.IsEnabled || !_config.EnableLevelOfDetail) return;

            try
            {
                _lodLevels[elementId] = lodLevel;

                switch (lodLevel)
                {
                    case LodLevel.Low:
                        // Simplified rendering
                        ApplyLowDetailRendering(element);
                        break;
                    case LodLevel.Medium:
                        // Standard rendering
                        ApplyMediumDetailRendering(element);
                        break;
                    case LodLevel.High:
                        // Full detail rendering
                        ApplyHighDetailRendering(element);
                        break;
                    case LodLevel.Ultra:
                        // Enhanced rendering with effects
                        ApplyUltraDetailRendering(element);
                        break;
                }

                _logger.LogTrace("🎯 Applied LOD rendering - Element: {ElementId}, Level: {LodLevel}",
                    elementId, lodLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error applying LOD rendering - Element: {ElementId}", elementId);
            }
        }

        #endregion

        #region GPU Acceleration

        /// <summary>
        /// Initialize GPU acceleration
        /// </summary>
        private void InitializeGpuAcceleration()
        {
            try
            {
                if (!_config.EnableGpuAcceleration) return;

                // Check if GPU acceleration is available (WinUI3 compatibility)
                _isGpuAccelerationAvailable = _config.EnableGpuAcceleration; // Assume available if requested

                if (_isGpuAccelerationAvailable)
                {
                    _logger.LogInformation("🚀 GPU acceleration enabled and assumed available");
                }
                else
                {
                    _logger.LogWarning("⚠️ GPU acceleration not requested, using software rendering");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing GPU acceleration");
                _isGpuAccelerationAvailable = false;
            }
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// ✅ NOVÉ: Get rendering performance metrics
        /// </summary>
        public RenderingPerformanceMetrics GetPerformanceMetrics()
        {
            return new RenderingPerformanceMetrics
            {
                TotalRenderOperations = _totalRenderOperations,
                TotalFramesRendered = _totalFramesRendered,
                CacheHitRatio = CacheHitRatio,
                AverageFrameTime = AverageFrameTime,
                CurrentFps = CurrentFps,
                ActiveRenderBatchesCount = ActiveRenderBatchesCount,
                VirtualizedElementsCount = VirtualizedElementsCount,
                RenderCacheSize = _renderCache.Count,
                IsGpuAccelerationEnabled = _isGpuAccelerationAvailable,
                CurrentRenderGeneration = _currentRenderGeneration
            };
        }

        /// <summary>
        /// Update frame time tracking
        /// </summary>
        private void UpdateFrameTime(double frameTimeMs)
        {
            if (!_config.EnableRenderStatistics) return;

            lock (_recentFrameTimes)
            {
                _recentFrameTimes.Enqueue(frameTimeMs);
                
                // Keep only recent frame times (last 60 frames)
                while (_recentFrameTimes.Count > 60)
                {
                    _recentFrameTimes.Dequeue();
                }
            }

            Interlocked.Increment(ref _totalFramesRendered);
        }

        #endregion

        #region Configuration & Management

        /// <summary>
        /// ✅ NOVÉ: Update configuration
        /// </summary>
        public void UpdateConfiguration(RenderingOptimizationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                
                // Update timer intervals
                _renderBatchTimer.Change(_config.RenderBatchTimeoutMs, _config.RenderBatchTimeoutMs);

                _logger.LogInformation("⚙️ RenderingOptimizationService configuration updated - Config: {Config}",
                    _config.ToString());
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Force render cache cleanup
        /// </summary>
        public void ClearRenderCache()
        {
            var count = _renderCache.Count;
            _renderCache.Clear();
            
            _logger.LogInformation("🧹 Render cache cleared - Entries removed: {Count}", count);
        }

        #endregion

        #region Private Helper Methods

        private string GetBatchKey(RenderOperationType operationType, Rect bounds)
        {
            // Group operations by type and approximate location
            var regionX = (int)(bounds.X / 100) * 100;
            var regionY = (int)(bounds.Y / 100) * 100;
            return $"{operationType}_{regionX}_{regionY}";
        }

        private int GetRenderPriority(RenderOperationType operationType, Rect bounds)
        {
            var priority = (int)_config.AsyncRenderingPriority * 100;
            
            // Higher priority for visible elements
            if (IsElementInViewport(bounds))
                priority += 50;
            
            // Different priorities for different operations
            switch (operationType)
            {
                case RenderOperationType.Draw:
                    priority += 30;
                    break;
                case RenderOperationType.Layout:
                    priority += 20;
                    break;
                case RenderOperationType.Composite:
                    priority += 10;
                    break;
            }

            return priority;
        }

        private bool IsElementInViewport(Rect elementBounds)
        {
            if (_currentViewport.IsEmpty) return true;

            var expandedViewport = new Rect(
                _currentViewport.X - _config.ViewportCullingMargin,
                _currentViewport.Y - _config.ViewportCullingMargin,
                _currentViewport.Width + (_config.ViewportCullingMargin * 2),
                _currentViewport.Height + (_config.ViewportCullingMargin * 2));

            // WinUI3 compatible intersection check
            return !(expandedViewport.X + expandedViewport.Width < elementBounds.X ||
                    elementBounds.X + elementBounds.Width < expandedViewport.X ||
                    expandedViewport.Y + expandedViewport.Height < elementBounds.Y ||
                    elementBounds.Y + elementBounds.Height < expandedViewport.Y);
        }

        private string GetRenderCacheKey(string elementId, RenderOperationType operationType, Rect bounds)
        {
            return $"{elementId}_{operationType}_{bounds.GetHashCode()}";
        }

        private bool IsRenderCacheValid(RenderCacheEntry entry)
        {
            return DateTime.UtcNow - entry.CreatedTime < TimeSpan.FromMinutes(5);
        }

        private bool ShouldCacheRender(RenderOperationType operationType, Rect bounds)
        {
            // Cache larger elements and complex operations
            return operationType == RenderOperationType.Composite || 
                   (bounds.Width * bounds.Height) > 10000;
        }

        private void PerformRenderOperation(FrameworkElement element, RenderOperationType operationType, Rect bounds)
        {
            // Actual rendering logic would go here
            // This is a placeholder for the complex rendering operations
        }

        private void ApplyCachedRender(FrameworkElement element, RenderCacheEntry entry)
        {
            // Apply cached render result
        }

        private void CacheRenderResult(string cacheKey, FrameworkElement element, RenderOperationType operationType, Rect bounds)
        {
            if (_renderCache.Count >= _config.MaxRenderCacheSizeMb * 100) // Rough estimate
                return;

            var entry = new RenderCacheEntry
            {
                CacheKey = cacheKey,
                EntryType = RenderCacheEntryType.Composite,
                CreatedTime = DateTime.UtcNow,
                LastAccessTime = DateTime.UtcNow,
                SizeEstimate = (int)(bounds.Width * bounds.Height)
            };

            _renderCache[cacheKey] = entry;
        }

        private void TriggerLazyLoading(VirtualizedElement element)
        {
            // Lazy loading logic
        }

        private void ApplyLowDetailRendering(FrameworkElement element)
        {
            // Simplified rendering for distant/small elements
        }

        private void ApplyMediumDetailRendering(FrameworkElement element)
        {
            // Standard rendering
        }

        private void ApplyHighDetailRendering(FrameworkElement element)
        {
            // Full detail rendering
        }

        private void ApplyUltraDetailRendering(FrameworkElement element)
        {
            // Enhanced rendering with effects
        }

        private void CleanupRenderCache(object? state)
        {
            if (_isDisposed) return;

            try
            {
                var expiredKeys = _renderCache
                    .Where(kvp => DateTime.UtcNow - kvp.Value.LastAccessTime > TimeSpan.FromMinutes(10))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _renderCache.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogTrace("🧹 Cleaned up {Count} expired render cache entries", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cleaning up render cache");
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            try
            {
                _renderBatchTimer?.Dispose();
                _cacheCleanupTimer?.Dispose();

                _renderBatches.Clear();
                _virtualizedElements.Clear();
                _renderCache.Clear();
                _virtualizedRows.Clear();
                _lodLevels.Clear();

                _logger.LogInformation("🧹 RenderingOptimizationService disposed - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing RenderingOptimizationService");
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Render batch
    /// </summary>
    internal class RenderBatch
    {
        public string BatchId { get; set; } = string.Empty;
        public RenderOperationType OperationType { get; set; }
        public DateTime CreatedTime { get; set; }
        public ConcurrentQueue<RenderOperation> Operations { get; set; } = new();
    }

    /// <summary>
    /// Render operation
    /// </summary>
    internal class RenderOperation
    {
        public string ElementId { get; set; } = string.Empty;
        public FrameworkElement Element { get; set; } = null!;
        public Rect Bounds { get; set; }
        public DateTime QueuedTime { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Virtualized element
    /// </summary>
    internal class VirtualizedElement
    {
        public string ElementId { get; set; } = string.Empty;
        public FrameworkElement? Element { get; set; }
        public Rect Bounds { get; set; }
        public int DataIndex { get; set; }
        public bool IsVisible { get; set; }
        public DateTime LastAccessTime { get; set; }
        public int RenderGeneration { get; set; }
    }

    /// <summary>
    /// Virtualized row
    /// </summary>
    internal class VirtualizedRow
    {
        public int RowIndex { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
        public DateTime LastAccessTime { get; set; }
    }

    /// <summary>
    /// Render cache entry
    /// </summary>
    internal class RenderCacheEntry
    {
        public string CacheKey { get; set; } = string.Empty;
        public RenderCacheEntryType EntryType { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public int SizeEstimate { get; set; }
        public object? CachedData { get; set; }
    }

    /// <summary>
    /// LOD level
    /// </summary>
    public enum LodLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }

    /// <summary>
    /// Rendering performance metrics
    /// </summary>
    public class RenderingPerformanceMetrics
    {
        public long TotalRenderOperations { get; set; }
        public long TotalFramesRendered { get; set; }
        public double CacheHitRatio { get; set; }
        public double AverageFrameTime { get; set; }
        public double CurrentFps { get; set; }
        public int ActiveRenderBatchesCount { get; set; }
        public int VirtualizedElementsCount { get; set; }
        public int RenderCacheSize { get; set; }
        public bool IsGpuAccelerationEnabled { get; set; }
        public int CurrentRenderGeneration { get; set; }
    }

    #endregion
}