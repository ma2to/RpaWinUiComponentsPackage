// Services/VirtualScrollingService.cs - ✅ NOVÉ: Virtual Scrolling Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using AdvancedVirtualScrollingConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.VirtualScrollingConfiguration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ ROZŠÍRENÉ: Advanced Virtual Scrolling Service pre optimalizáciu výkonu
    /// </summary>
    public class VirtualScrollingService
    {
        private readonly ILogger _logger;
        private AdvancedVirtualScrollingConfiguration _config;
        private VirtualScrollingViewport _viewport;
        private int _totalRowCount;
        private readonly object _lockObject = new();
        
        // ✅ NOVÉ: Advanced features
        private readonly Dictionary<int, Microsoft.UI.Xaml.FrameworkElement> _renderedElements = new();
        private readonly Queue<Microsoft.UI.Xaml.FrameworkElement> _recycledElements = new();
        private readonly System.Diagnostics.Stopwatch _renderStopwatch = new();
        private readonly System.Threading.Timer _scrollThrottleTimer;
        private volatile bool _isScrolling = false;
        private int _renderCallCount = 0;
        private double _averageRenderTime = 0;

        public VirtualScrollingService(AdvancedVirtualScrollingConfiguration? config = null, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? AdvancedVirtualScrollingConfiguration.Advanced;
            _viewport = new VirtualScrollingViewport();
            
            // ✅ NOVÉ: Scroll throttling timer
            _scrollThrottleTimer = new System.Threading.Timer(OnScrollThrottleElapsed, null,
                System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            
            _logger.LogDebug("🚀 Advanced VirtualScrollingService initialized - Config: {ConfigInfo}", 
                _config.GetDiagnosticInfo());
        }

        /// <summary>
        /// Aktualizuje konfiguráciu virtual scrolling
        /// </summary>
        public void UpdateConfiguration(AdvancedVirtualScrollingConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Virtual scrolling configuration updated");
            }
        }

        /// <summary>
        /// Nastaví celkový počet riadkov
        /// </summary>
        public void SetTotalRowCount(int rowCount)
        {
            lock (_lockObject)
            {
                _totalRowCount = rowCount;
                _viewport.TotalRowCount = rowCount;
                _viewport.TotalHeight = rowCount * _config.OptimizedRowHeight;
                
                _logger.LogDebug("📊 Total row count set to {RowCount}, total height: {TotalHeight}px", 
                    rowCount, _viewport.TotalHeight);
            }
        }

        /// <summary>
        /// Či sa má použiť virtual scrolling pre daný počet riadkov
        /// </summary>
        public bool ShouldUseVirtualScrolling()
        {
            return _config.EnableVerticalVirtualization && _totalRowCount >= _config.MinRowsForActivation;
        }

        /// <summary>
        /// Vypočíta viewport na základe scroll pozície
        /// </summary>
        public VirtualScrollingViewport CalculateViewport(double scrollPosition, double viewportHeight)
        {
            lock (_lockObject)
            {
                try
                {
                    _logger.LogTrace("🔍 Calculating viewport - ScrollPos: {ScrollPos}, ViewportHeight: {ViewportHeight}", 
                        scrollPosition, viewportHeight);

                    if (!ShouldUseVirtualScrolling())
                    {
                        // Vráti viewport s všetkými riadkami
                        return new VirtualScrollingViewport
                        {
                            FirstVisibleRowIndex = 0,
                            LastVisibleRowIndex = Math.Max(0, _totalRowCount - 1),
                            FirstRenderedRowIndex = 0,
                            LastRenderedRowIndex = Math.Max(0, _totalRowCount - 1),
                            ScrollPosition = scrollPosition,
                            ViewportHeight = viewportHeight,
                            TotalHeight = _viewport.TotalHeight,
                            TotalRowCount = _totalRowCount
                        };
                    }

                    // Vypočítaj viditeľné riadky
                    int firstVisibleRow = Math.Max(0, (int)(scrollPosition / _config.OptimizedRowHeight));
                    int visibleRowCount = Math.Min(_config.VisibleRowCount, 
                        (int)Math.Ceiling(viewportHeight / _config.OptimizedRowHeight) + 1);
                    int lastVisibleRow = Math.Min(_totalRowCount - 1, firstVisibleRow + visibleRowCount - 1);

                    // Vypočítaj renderované riadky s bufferom
                    int firstRenderedRow = Math.Max(0, firstVisibleRow - _config.RowBufferSize);
                    int lastRenderedRow = Math.Min(_totalRowCount - 1, lastVisibleRow + _config.RowBufferSize);

                    var newViewport = new VirtualScrollingViewport
                    {
                        FirstVisibleRowIndex = firstVisibleRow,
                        LastVisibleRowIndex = lastVisibleRow,
                        FirstRenderedRowIndex = firstRenderedRow,
                        LastRenderedRowIndex = lastRenderedRow,
                        ScrollPosition = scrollPosition,
                        ViewportHeight = viewportHeight,
                        TotalHeight = _viewport.TotalHeight,
                        TotalRowCount = _totalRowCount
                    };

                    _logger.LogTrace("✅ Viewport calculated - Visible: {FirstVisible}-{LastVisible}, " +
                        "Rendered: {FirstRendered}-{LastRendered}", 
                        firstVisibleRow, lastVisibleRow, firstRenderedRow, lastRenderedRow);

                    _viewport = newViewport;
                    return newViewport;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error calculating viewport");
                    throw;
                }
            }
        }

        /// <summary>
        /// Vráti aktuálny viewport
        /// </summary>
        public VirtualScrollingViewport GetCurrentViewport()
        {
            lock (_lockObject)
            {
                return new VirtualScrollingViewport
                {
                    FirstVisibleRowIndex = _viewport.FirstVisibleRowIndex,
                    LastVisibleRowIndex = _viewport.LastVisibleRowIndex,
                    FirstRenderedRowIndex = _viewport.FirstRenderedRowIndex,
                    LastRenderedRowIndex = _viewport.LastRenderedRowIndex,
                    ScrollPosition = _viewport.ScrollPosition,
                    ViewportHeight = _viewport.ViewportHeight,
                    TotalHeight = _viewport.TotalHeight,
                    TotalRowCount = _viewport.TotalRowCount
                };
            }
        }

        /// <summary>
        /// Vráti zoznam indexov riadkov ktoré sa majú renderovať
        /// </summary>
        public IEnumerable<int> GetRowIndicesToRender()
        {
            var viewport = GetCurrentViewport();
            if (!viewport.IsValid) yield break;

            for (int i = viewport.FirstRenderedRowIndex; i <= viewport.LastRenderedRowIndex; i++)
            {
                if (i >= 0 && i < _totalRowCount)
                    yield return i;
            }
        }

        /// <summary>
        /// Vypočíta offset Y pozíciu pre renderovaný container
        /// </summary>
        public double CalculateContainerOffsetY()
        {
            var viewport = GetCurrentViewport();
            return viewport.FirstRenderedRowIndex * _config.OptimizedRowHeight;
        }

        /// <summary>
        /// Vypočíta celkovú výšku virtual scroll container-a
        /// </summary>
        public double CalculateTotalContainerHeight()
        {
            return _totalRowCount * _config.OptimizedRowHeight;
        }

        /// <summary>
        /// Vráti konfiguráciu
        /// </summary>
        public AdvancedVirtualScrollingConfiguration GetConfiguration()
        {
            lock (_lockObject)
            {
                return _config.Clone();
            }
        }

        /// <summary>
        /// Statistiky virtual scrolling-u
        /// </summary>
        public VirtualScrollingStats GetStats()
        {
            var viewport = GetCurrentViewport();
            return new VirtualScrollingStats
            {
                TotalRows = _totalRowCount,
                RenderedRows = viewport.RenderedRowCount,
                MemorySavingPercent = _totalRowCount > 0 ? 
                    (1.0 - (double)viewport.RenderedRowCount / _totalRowCount) * 100 : 0,
                IsVirtualizationActive = ShouldUseVirtualScrolling(),
                ViewportInfo = viewport,
                // ✅ NOVÉ: Pokročilé štatistiky
                CachedElements = _renderedElements.Count,
                RecycledElements = _recycledElements.Count,
                AverageRenderTime = _averageRenderTime,
                RenderCallCount = _renderCallCount
            };
        }

        // ✅ NOVÉ: Advanced Methods

        /// <summary>
        /// Aktualizuje viewport s throttling optimalizáciou
        /// </summary>
        public VirtualScrollingViewport CalculateViewportOptimized(double scrollPosition, double viewportHeight)
        {
            lock (_lockObject)
            {
                // Throttling pre scroll events
                if (_isScrolling && _config.ScrollThrottleMs > 0)
                {
                    _scrollThrottleTimer.Change(_config.ScrollThrottleMs, System.Threading.Timeout.Infinite);
                    return _viewport; // Return cached viewport
                }

                _isScrolling = true;
                _renderStopwatch.Restart();

                var viewport = CalculateViewport(scrollPosition, viewportHeight);

                _renderStopwatch.Stop();
                UpdatePerformanceMetrics();

                _isScrolling = false;
                return viewport;
            }
        }

        /// <summary>
        /// Registeruje renderovaný UI element pre recykláciu
        /// </summary>
        public void RegisterRenderedElement(int rowIndex, Microsoft.UI.Xaml.FrameworkElement element)
        {
            try
            {
                if (_config.EnableLazyLoading)
                {
                    // Recykluj staré elementy mimo rendered range
                    var viewport = GetCurrentViewport();
                    var elementsToRecycle = new List<int>();

                    foreach (var kvp in _renderedElements)
                    {
                        if (kvp.Key < viewport.FirstRenderedRowIndex || kvp.Key > viewport.LastRenderedRowIndex)
                        {
                            elementsToRecycle.Add(kvp.Key);
                        }
                    }

                    foreach (var index in elementsToRecycle.Take(10)) // Limituj počet
                    {
                        if (_renderedElements.TryGetValue(index, out var oldElement))
                        {
                            _renderedElements.Remove(index);
                            if (_recycledElements.Count < _config.MaxCachedUIElements)
                            {
                                _recycledElements.Enqueue(oldElement);
                            }
                        }
                    }
                }

                _renderedElements[rowIndex] = element;

                _logger.LogTrace("📝 Element registered - Row: {RowIndex}, Cached: {CachedCount}, " +
                    "Recycled: {RecycledCount}", rowIndex, _renderedElements.Count, _recycledElements.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error registering rendered element - Row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// Získa recyklovaný UI element ak je dostupný
        /// </summary>
        public Microsoft.UI.Xaml.FrameworkElement? GetRecycledElement()
        {
            if (_config.EnableLazyLoading && _recycledElements.Count > 0)
            {
                var element = _recycledElements.Dequeue();
                _logger.LogTrace("♻️ Element recycled - Remaining: {RemainingCount}", _recycledElements.Count);
                return element;
            }
            return null;
        }

        /// <summary>
        /// Vyčistí cache a recyklované elementy
        /// </summary>
        public void ClearElementCache()
        {
            try
            {
                _logger.LogDebug("🧹 Clearing virtual scrolling element cache...");

                var cachedCount = _renderedElements.Count;
                var recycledCount = _recycledElements.Count;

                _renderedElements.Clear();
                _recycledElements.Clear();

                _logger.LogInformation("✅ Element cache cleared - Cached: {CachedCount}, " +
                    "Recycled: {RecycledCount}", cachedCount, recycledCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR clearing element cache");
            }
        }

        /// <summary>
        /// Aktualizuje performance metrics
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            _renderCallCount++;
            var currentRenderTime = _renderStopwatch.Elapsed.TotalMilliseconds;

            // Exponential moving average
            _averageRenderTime = (_averageRenderTime * 0.9) + (currentRenderTime * 0.1);

            if (_config.EnableDiagnostics)
            {
                _logger.LogTrace("📊 Render metrics updated - Call: {CallCount}, " +
                    "Current: {CurrentTime:F2}ms, Average: {AverageTime:F2}ms",
                    _renderCallCount, currentRenderTime, _averageRenderTime);
            }
        }

        /// <summary>
        /// Scroll throttle timer handler
        /// </summary>
        private void OnScrollThrottleElapsed(object? state)
        {
            _isScrolling = false;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _scrollThrottleTimer?.Dispose();
                ClearElementCache();

                _logger.LogDebug("🔄 VirtualScrollingService disposed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during VirtualScrollingService disposal");
            }
        }
    }

    /// <summary>
    /// ✅ ROZŠÍRENÉ: Advanced Virtual Scrolling Statistics
    /// </summary>
    public class VirtualScrollingStats
    {
        public int TotalRows { get; set; }
        public int RenderedRows { get; set; }
        public double MemorySavingPercent { get; set; }
        public bool IsVirtualizationActive { get; set; }
        public VirtualScrollingViewport? ViewportInfo { get; set; }
        
        // ✅ NOVÉ: Advanced metrics
        public int CachedElements { get; set; }
        public int RecycledElements { get; set; }
        public double AverageRenderTime { get; set; }
        public int RenderCallCount { get; set; }

        public override string ToString()
        {
            return $"VirtualScrolling: {RenderedRows}/{TotalRows} rows rendered " +
                   $"({MemorySavingPercent:F1}% memory saved), " +
                   $"Cache: {CachedElements}, Recycled: {RecycledElements}, " +
                   $"AvgRender: {AverageRenderTime:F2}ms, Calls: {RenderCallCount}";
        }
    }
}