// Models/RenderingOptimizationConfiguration.cs - ✅ NOVÉ: Rendering Pipeline optimization configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre Rendering Pipeline optimization
    /// </summary>
    public class RenderingOptimizationConfiguration
    {
        #region Properties

        /// <summary>
        /// Povoliť rendering optimizácie
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Povoliť virtualizáciu UI elementov
        /// </summary>
        public bool EnableVirtualization { get; set; } = true;

        /// <summary>
        /// Povoliť lazy loading UI elementov
        /// </summary>
        public bool EnableLazyLoading { get; set; } = true;

        /// <summary>
        /// Povoliť render batching
        /// </summary>
        public bool EnableRenderBatching { get; set; } = true;

        /// <summary>
        /// Povoliť GPU acceleration
        /// </summary>
        public bool EnableGpuAcceleration { get; set; } = true;

        /// <summary>
        /// Povoliť frame rate limiting
        /// </summary>
        public bool EnableFrameRateLimiting { get; set; } = true;

        /// <summary>
        /// Target frame rate (FPS)
        /// </summary>
        public int TargetFrameRate { get; set; } = 60;

        /// <summary>
        /// Max počet viditeľných riadkov pre virtualizáciu
        /// </summary>
        public int VirtualizationWindowSize { get; set; } = 100;

        /// <summary>
        /// Buffer size pre virtualizáciu (riadky nad/pod window)
        /// </summary>
        public int VirtualizationBufferSize { get; set; } = 20;

        /// <summary>
        /// Max počet UI elementov v render batch
        /// </summary>
        public int MaxRenderBatchSize { get; set; } = 50;

        /// <summary>
        /// Render batch timeout (ms)
        /// </summary>
        public int RenderBatchTimeoutMs { get; set; } = 16; // ~60 FPS

        /// <summary>
        /// Povoliť render caching
        /// </summary>
        public bool EnableRenderCaching { get; set; } = true;

        /// <summary>
        /// Max veľkosť render cache (MB)
        /// </summary>
        public int MaxRenderCacheSizeMb { get; set; } = 50;

        /// <summary>
        /// Povoliť invalidation tracking
        /// </summary>
        public bool EnableInvalidationTracking { get; set; } = true;

        /// <summary>
        /// Povoliť viewport culling
        /// </summary>
        public bool EnableViewportCulling { get; set; } = true;

        /// <summary>
        /// Viewport culling margin (pixels)
        /// </summary>
        public double ViewportCullingMargin { get; set; } = 100.0;

        /// <summary>
        /// Povoliť occlusion culling
        /// </summary>
        public bool EnableOcclusionCulling { get; set; } = false; // Experimental

        /// <summary>
        /// Min element size pre occlusion culling (pixels)
        /// </summary>
        public double MinOcclusionCullingSize { get; set; } = 5.0;

        /// <summary>
        /// Povoliť level-of-detail rendering
        /// </summary>
        public bool EnableLevelOfDetail { get; set; } = true;

        /// <summary>
        /// LOD distance thresholds
        /// </summary>
        public double[] LodDistanceThresholds { get; set; } = { 500.0, 1000.0, 2000.0 };

        /// <summary>
        /// Povoliť render statistics
        /// </summary>
        public bool EnableRenderStatistics { get; set; } = true;

        /// <summary>
        /// Povoliť debug overlays
        /// </summary>
        public bool EnableDebugOverlays { get; set; } = false;

        /// <summary>
        /// Async rendering priority
        /// </summary>
        public RenderingPriority AsyncRenderingPriority { get; set; } = RenderingPriority.Normal;

        /// <summary>
        /// Max concurrent rendering tasks
        /// </summary>
        public int MaxConcurrentRenderingTasks { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Render quality level
        /// </summary>
        public RenderQuality RenderQuality { get; set; } = RenderQuality.High;

        #endregion

        #region Default Configurations

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static RenderingOptimizationConfiguration Default => new RenderingOptimizationConfiguration();

        /// <summary>
        /// High performance konfigurácia
        /// </summary>
        public static RenderingOptimizationConfiguration HighPerformance => new RenderingOptimizationConfiguration
        {
            TargetFrameRate = 120,
            VirtualizationWindowSize = 200,
            VirtualizationBufferSize = 50,
            MaxRenderBatchSize = 100,
            RenderBatchTimeoutMs = 8, // ~120 FPS
            MaxRenderCacheSizeMb = 100,
            EnableGpuAcceleration = true,
            EnableLevelOfDetail = true,
            AsyncRenderingPriority = RenderingPriority.High,
            MaxConcurrentRenderingTasks = Environment.ProcessorCount * 2,
            RenderQuality = RenderQuality.High
        };

        /// <summary>
        /// Low-end device konfigurácia
        /// </summary>
        public static RenderingOptimizationConfiguration LowEnd => new RenderingOptimizationConfiguration
        {
            TargetFrameRate = 30,
            VirtualizationWindowSize = 50,
            VirtualizationBufferSize = 10,
            MaxRenderBatchSize = 25,
            RenderBatchTimeoutMs = 33, // ~30 FPS
            MaxRenderCacheSizeMb = 20,
            EnableGpuAcceleration = false,
            EnableOcclusionCulling = false,
            EnableLevelOfDetail = true,
            AsyncRenderingPriority = RenderingPriority.Low,
            MaxConcurrentRenderingTasks = Math.Max(1, Environment.ProcessorCount / 2),
            RenderQuality = RenderQuality.Medium
        };

        /// <summary>
        /// Debug konfigurácia
        /// </summary>
        public static RenderingOptimizationConfiguration Debug => new RenderingOptimizationConfiguration
        {
            EnableDebugOverlays = true,
            EnableRenderStatistics = true,
            TargetFrameRate = 60,
            VirtualizationWindowSize = 30,
            EnableRenderCaching = false, // Disable for debugging
            EnableInvalidationTracking = true,
            RenderQuality = RenderQuality.Low
        };

        /// <summary>
        /// Disabled konfigurácia
        /// </summary>
        public static RenderingOptimizationConfiguration Disabled => new RenderingOptimizationConfiguration
        {
            IsEnabled = false,
            EnableVirtualization = false,
            EnableLazyLoading = false,
            EnableRenderBatching = false,
            EnableGpuAcceleration = false,
            EnableFrameRateLimiting = false,
            EnableRenderCaching = false,
            EnableInvalidationTracking = false,
            EnableViewportCulling = false,
            EnableOcclusionCulling = false,
            EnableLevelOfDetail = false,
            EnableRenderStatistics = false,
            EnableDebugOverlays = false
        };

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (TargetFrameRate <= 0 || TargetFrameRate > 240)
                throw new ArgumentException("TargetFrameRate must be between 1 and 240");

            if (VirtualizationWindowSize <= 0)
                throw new ArgumentException("VirtualizationWindowSize must be greater than 0");

            if (VirtualizationBufferSize < 0)
                throw new ArgumentException("VirtualizationBufferSize cannot be negative");

            if (MaxRenderBatchSize <= 0)
                throw new ArgumentException("MaxRenderBatchSize must be greater than 0");

            if (RenderBatchTimeoutMs <= 0)
                throw new ArgumentException("RenderBatchTimeoutMs must be greater than 0");

            if (MaxRenderCacheSizeMb < 0)
                throw new ArgumentException("MaxRenderCacheSizeMb cannot be negative");

            if (ViewportCullingMargin < 0)
                throw new ArgumentException("ViewportCullingMargin cannot be negative");

            if (MinOcclusionCullingSize <= 0)
                throw new ArgumentException("MinOcclusionCullingSize must be greater than 0");

            if (MaxConcurrentRenderingTasks <= 0)
                throw new ArgumentException("MaxConcurrentRenderingTasks must be greater than 0");

            if (LodDistanceThresholds == null || LodDistanceThresholds.Length == 0)
                throw new ArgumentException("LodDistanceThresholds cannot be null or empty");

            for (int i = 1; i < LodDistanceThresholds.Length; i++)
            {
                if (LodDistanceThresholds[i] <= LodDistanceThresholds[i - 1])
                    throw new ArgumentException("LodDistanceThresholds must be in ascending order");
            }
        }

        /// <summary>
        /// Klonuje konfiguráciu
        /// </summary>
        public RenderingOptimizationConfiguration Clone()
        {
            return new RenderingOptimizationConfiguration
            {
                IsEnabled = IsEnabled,
                EnableVirtualization = EnableVirtualization,
                EnableLazyLoading = EnableLazyLoading,
                EnableRenderBatching = EnableRenderBatching,
                EnableGpuAcceleration = EnableGpuAcceleration,
                EnableFrameRateLimiting = EnableFrameRateLimiting,
                TargetFrameRate = TargetFrameRate,
                VirtualizationWindowSize = VirtualizationWindowSize,
                VirtualizationBufferSize = VirtualizationBufferSize,
                MaxRenderBatchSize = MaxRenderBatchSize,
                RenderBatchTimeoutMs = RenderBatchTimeoutMs,
                EnableRenderCaching = EnableRenderCaching,
                MaxRenderCacheSizeMb = MaxRenderCacheSizeMb,
                EnableInvalidationTracking = EnableInvalidationTracking,
                EnableViewportCulling = EnableViewportCulling,
                ViewportCullingMargin = ViewportCullingMargin,
                EnableOcclusionCulling = EnableOcclusionCulling,
                MinOcclusionCullingSize = MinOcclusionCullingSize,
                EnableLevelOfDetail = EnableLevelOfDetail,
                LodDistanceThresholds = (double[])LodDistanceThresholds.Clone(),
                EnableRenderStatistics = EnableRenderStatistics,
                EnableDebugOverlays = EnableDebugOverlays,
                AsyncRenderingPriority = AsyncRenderingPriority,
                MaxConcurrentRenderingTasks = MaxConcurrentRenderingTasks,
                RenderQuality = RenderQuality
            };
        }

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public override string ToString()
        {
            return $"RenderingOptimizationConfiguration[Enabled: {IsEnabled}, " +
                   $"Virtualization: {EnableVirtualization}, Batching: {EnableRenderBatching}, " +
                   $"TargetFPS: {TargetFrameRate}, WindowSize: {VirtualizationWindowSize}, " +
                   $"Quality: {RenderQuality}, GPU: {EnableGpuAcceleration}]";
        }

        #endregion
    }

    #region Enumerations

    /// <summary>
    /// Rendering priority
    /// </summary>
    public enum RenderingPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Render quality level
    /// </summary>
    public enum RenderQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    /// <summary>
    /// Render operation type
    /// </summary>
    public enum RenderOperationType
    {
        Layout,
        Draw,
        Composite,
        Present,
        Invalidate
    }

    /// <summary>
    /// Render cache entry type
    /// </summary>
    public enum RenderCacheEntryType
    {
        Bitmap,
        Vector,
        Text,
        Composite
    }

    #endregion
}