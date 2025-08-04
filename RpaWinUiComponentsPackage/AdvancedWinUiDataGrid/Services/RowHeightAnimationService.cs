// Services/RowHeightAnimationService.cs - ✅ NOVÉ: Row Height Animation Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
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
    /// ✅ NOVÉ: Row Height Animation Service - smooth transitions pri rozšírení riadkov
    /// </summary>
    internal class RowHeightAnimationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Animation system
        private RowHeightAnimationConfiguration _config;
        private readonly ConcurrentDictionary<int, RowHeightAnimation> _activeAnimations = new();
        private readonly ConcurrentDictionary<int, double> _rowHeights = new();
        private readonly Timer _debounceTimer;
        private readonly ConcurrentQueue<RowHeightChangeRequest> _pendingRequests = new();

        // ✅ NOVÉ: Performance monitoring
        private long _totalAnimations = 0;
        private long _totalAnimationTime = 0;
        private readonly Stopwatch _performanceStopwatch = new();

        #endregion

        #region Constructor

        public RowHeightAnimationService(ILogger? logger = null, RowHeightAnimationConfiguration? config = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? RowHeightAnimationConfiguration.Default;
            
            _debounceTimer = new Timer(ProcessPendingRequests, null, Timeout.Infinite, Timeout.Infinite);
            
            _logger.LogInformation("🎬 RowHeightAnimationService initialized - Enabled: {IsEnabled}, Duration: {Duration}ms",
                _config.IsEnabled, _config.AnimationDurationMs);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ✅ NOVÉ: Animate row height change
        /// </summary>
        public async Task AnimateRowHeightAsync(int rowIndex, double fromHeight, double toHeight, FrameworkElement rowElement)
        {
            if (!_config.IsEnabled || _isDisposed)
                return;

            var heightDiff = Math.Abs(toHeight - fromHeight);
            
            // Check animation thresholds
            if (heightDiff < _config.MinAnimationThreshold || heightDiff > _config.MaxAnimationThreshold)
            {
                // Direct height change without animation
                rowElement.Height = toHeight;
                _rowHeights[rowIndex] = toHeight;
                return;
            }

            // Check concurrent animation limit
            if (_activeAnimations.Count >= _config.MaxConcurrentAnimations)
            {
                _logger.LogWarning("🚫 Max concurrent animations reached ({MaxCount}), skipping animation for row {RowIndex}",
                    _config.MaxConcurrentAnimations, rowIndex);
                rowElement.Height = toHeight;
                _rowHeights[rowIndex] = toHeight;
                return;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                var animation = new RowHeightAnimation
                {
                    RowIndex = rowIndex,
                    FromHeight = fromHeight,
                    ToHeight = toHeight,
                    StartTime = DateTime.UtcNow,
                    Element = rowElement
                };

                _activeAnimations[rowIndex] = animation;

                _logger.LogTrace("🎬 Starting row height animation - Row: {RowIndex}, From: {FromHeight}, To: {ToHeight}",
                    rowIndex, fromHeight, toHeight);

                // Create and start animation
                var doubleAnimation = new DoubleAnimation
                {
                    From = fromHeight,
                    To = toHeight,
                    Duration = TimeSpan.FromMilliseconds(_config.AnimationDurationMs),
                    EasingFunction = CreateEasingFunction(_config.EasingFunction)
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(doubleAnimation);
                Storyboard.SetTarget(doubleAnimation, rowElement);
                Storyboard.SetTargetProperty(doubleAnimation, "Height");

                var tcs = new TaskCompletionSource<bool>();

                storyboard.Completed += (s, e) =>
                {
                    try
                    {
                        stopwatch.Stop();
                        _rowHeights[rowIndex] = toHeight;
                        _activeAnimations.TryRemove(rowIndex, out _);

                        if (_config.EnablePerformanceMonitoring)
                        {
                            _totalAnimations++;
                            _totalAnimationTime += stopwatch.ElapsedMilliseconds;
                        }

                        _logger.LogTrace("✅ Row height animation completed - Row: {RowIndex}, Duration: {Duration}ms",
                            rowIndex, stopwatch.ElapsedMilliseconds);

                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error completing row height animation - Row: {RowIndex}", rowIndex);
                        tcs.SetException(ex);
                    }
                };

                storyboard.Begin();
                await tcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error animating row height - Row: {RowIndex}", rowIndex);
                
                // Fallback to direct height change
                rowElement.Height = toHeight;
                _rowHeights[rowIndex] = toHeight;
                _activeAnimations.TryRemove(rowIndex, out _);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Auto-size row based on content with animation
        /// </summary>
        public async Task AutoSizeRowAsync(int rowIndex, FrameworkElement rowElement, string content)
        {
            if (!_config.EnableAutoSizing || _isDisposed)
                return;

            try
            {
                var currentHeight = _rowHeights.GetValueOrDefault(rowIndex, rowElement.ActualHeight);
                var newHeight = CalculateContentHeight(content, rowElement.ActualWidth);

                if (Math.Abs(newHeight - currentHeight) > 1.0) // Threshold for height change
                {
                    await AnimateRowHeightAsync(rowIndex, currentHeight, newHeight, rowElement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error auto-sizing row - Row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Request row height change with debouncing
        /// </summary>
        public void RequestRowHeightChange(int rowIndex, double newHeight, FrameworkElement rowElement)
        {
            if (!_config.IsEnabled || _isDisposed)
                return;

            var request = new RowHeightChangeRequest
            {
                RowIndex = rowIndex,
                NewHeight = newHeight,
                Element = rowElement,
                RequestTime = DateTime.UtcNow
            };

            _pendingRequests.Enqueue(request);

            // Reset debounce timer
            _debounceTimer.Change(_config.AutoSizingDebounceMs, Timeout.Infinite);
        }

        /// <summary>
        /// ✅ NOVÉ: Get current row height
        /// </summary>
        public double GetRowHeight(int rowIndex)
        {
            return _rowHeights.GetValueOrDefault(rowIndex, _config.MinAnimationThreshold);
        }

        /// <summary>
        /// ✅ NOVÉ: Cancel animation for specific row
        /// </summary>
        public void CancelRowAnimation(int rowIndex)
        {
            if (_activeAnimations.TryRemove(rowIndex, out var animation))
            {
                _logger.LogDebug("🛑 Cancelled row height animation - Row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Cancel all animations
        /// </summary>
        public void CancelAllAnimations()
        {
            var count = _activeAnimations.Count;
            _activeAnimations.Clear();
            
            if (count > 0)
            {
                _logger.LogInformation("🛑 Cancelled {Count} row height animations", count);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Update configuration
        /// </summary>
        public void UpdateConfiguration(RowHeightAnimationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                
                _logger.LogInformation("⚙️ Row height animation configuration updated - Enabled: {IsEnabled}, Duration: {Duration}ms",
                    _config.IsEnabled, _config.AnimationDurationMs);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get performance metrics
        /// </summary>
        public RowHeightAnimationMetrics GetPerformanceMetrics()
        {
            return new RowHeightAnimationMetrics
            {
                TotalAnimations = _totalAnimations,
                AverageAnimationTimeMs = _totalAnimations > 0 ? (double)_totalAnimationTime / _totalAnimations : 0,
                ActiveAnimationsCount = _activeAnimations.Count,
                PendingRequestsCount = _pendingRequests.Count,
                ConfigurationName = _config.GetType().Name
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create easing function based on configuration
        /// </summary>
        private EasingFunctionBase? CreateEasingFunction(RowHeightEasingFunction easingType)
        {
            return easingType switch
            {
                RowHeightEasingFunction.Linear => null,
                RowHeightEasingFunction.EaseIn => new QuadraticEase { EasingMode = EasingMode.EaseIn },
                RowHeightEasingFunction.EaseOut => new QuadraticEase { EasingMode = EasingMode.EaseOut },
                RowHeightEasingFunction.EaseInOut => new QuadraticEase { EasingMode = EasingMode.EaseInOut },
                RowHeightEasingFunction.BounceIn => new BounceEase { EasingMode = EasingMode.EaseIn },
                RowHeightEasingFunction.BounceOut => new BounceEase { EasingMode = EasingMode.EaseOut },
                RowHeightEasingFunction.ElasticIn => new ElasticEase { EasingMode = EasingMode.EaseIn },
                RowHeightEasingFunction.ElasticOut => new ElasticEase { EasingMode = EasingMode.EaseOut },
                _ => new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
        }

        /// <summary>
        /// Calculate content height based on text and available width
        /// </summary>
        private double CalculateContentHeight(string content, double availableWidth)
        {
            // Simple height calculation - can be enhanced with proper text measurement
            var baseHeight = 36.0; // Default row height
            var linesEstimate = Math.Ceiling(content.Length / (availableWidth / 8.0)); // Rough estimate
            return Math.Max(baseHeight, Math.Min(baseHeight * linesEstimate, _config.MaxAnimationThreshold));
        }

        /// <summary>
        /// Process pending row height change requests
        /// </summary>
        private async void ProcessPendingRequests(object? state)
        {
            if (_isDisposed)
                return;

            var processedRequests = new List<RowHeightChangeRequest>();

            // Group requests by row index (take latest for each row)
            var latestRequestsPerRow = new Dictionary<int, RowHeightChangeRequest>();

            while (_pendingRequests.TryDequeue(out var request))
            {
                latestRequestsPerRow[request.RowIndex] = request;
            }

            // Process latest requests
            foreach (var kvp in latestRequestsPerRow)
            {
                try
                {
                    var request = kvp.Value;
                    var currentHeight = GetRowHeight(request.RowIndex);
                    
                    await AnimateRowHeightAsync(request.RowIndex, currentHeight, request.NewHeight, request.Element);
                    processedRequests.Add(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing row height change request - Row: {RowIndex}", kvp.Key);
                }
            }

            if (processedRequests.Count > 0)
            {
                _logger.LogTrace("🔄 Processed {Count} row height change requests", processedRequests.Count);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                _debounceTimer?.Dispose();
                CancelAllAnimations();
                _activeAnimations.Clear();
                _rowHeights.Clear();

                while (_pendingRequests.TryDequeue(out _)) { }

                _logger.LogInformation("🧹 RowHeightAnimationService disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing RowHeightAnimationService");
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Row height animation data
    /// </summary>
    internal class RowHeightAnimation
    {
        public int RowIndex { get; set; }
        public double FromHeight { get; set; }
        public double ToHeight { get; set; }
        public DateTime StartTime { get; set; }
        public FrameworkElement Element { get; set; } = null!;
    }

    /// <summary>
    /// Row height change request
    /// </summary>
    internal class RowHeightChangeRequest
    {
        public int RowIndex { get; set; }
        public double NewHeight { get; set; }
        public FrameworkElement Element { get; set; } = null!;
        public DateTime RequestTime { get; set; }
    }

    /// <summary>
    /// Row height animation performance metrics
    /// </summary>
    public class RowHeightAnimationMetrics
    {
        public long TotalAnimations { get; set; }
        public double AverageAnimationTimeMs { get; set; }
        public int ActiveAnimationsCount { get; set; }
        public int PendingRequestsCount { get; set; }
        public string ConfigurationName { get; set; } = string.Empty;
    }

    #endregion
}