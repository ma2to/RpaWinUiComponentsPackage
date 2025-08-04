// Models/CachingOptimizationConfiguration.cs - ✅ NOVÉ: Caching Optimization Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Caching Optimization Configuration - multi-level cache settings
    /// </summary>
    internal class CachingOptimizationConfiguration
    {
        #region Properties

        /// <summary>
        /// Enable/disable caching optimization
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Enable L1 memory cache
        /// </summary>
        public bool EnableL1MemoryCache { get; set; } = true;

        /// <summary>
        /// Enable L2 weak reference cache
        /// </summary>
        public bool EnableL2WeakReferenceCache { get; set; } = true;

        /// <summary>
        /// Enable L3 disk cache
        /// </summary>
        public bool EnableL3DiskCache { get; set; } = false;

        /// <summary>
        /// L1 cache size limit (number of items)
        /// </summary>
        public int L1CacheSize { get; set; } = 1000;

        /// <summary>
        /// L2 cache size limit (number of items)
        /// </summary>
        public int L2CacheSize { get; set; } = 5000;

        /// <summary>
        /// Cache expiration time in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Enable performance monitoring
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = true;

        /// <summary>
        /// Cache cleanup interval in minutes
        /// </summary>
        public int CleanupIntervalMinutes { get; set; } = 10;

        /// <summary>
        /// Maintenance interval in milliseconds
        /// </summary>
        public int MaintenanceIntervalMs { get; set; } = 60000;

        /// <summary>
        /// Disk cache cleanup interval in milliseconds
        /// </summary>
        public int DiskCacheCleanupIntervalMs { get; set; } = 300000;

        /// <summary>
        /// Max L1 item size in bytes
        /// </summary>
        public int MaxL1ItemSizeBytes { get; set; } = 1048576; // 1MB

        /// <summary>
        /// Max L1 cache items
        /// </summary>
        public int MaxL1CacheItems { get; set; } = 1000;

        /// <summary>
        /// Max L3 total size in bytes
        /// </summary>
        public long MaxL3TotalSizeBytes { get; set; } = 104857600; // 100MB

        /// <summary>
        /// Max L3 item size in bytes
        /// </summary>
        public int MaxL3ItemSizeBytes { get; set; } = 10485760; // 10MB

        /// <summary>
        /// Performance warning threshold in milliseconds
        /// </summary>
        public int PerformanceWarningThresholdMs { get; set; } = 1000;

        #endregion

        #region Methods

        /// <summary>
        /// Validate configuration
        /// </summary>
        public void Validate()
        {
            if (L1CacheSize <= 0)
                throw new ArgumentException("L1CacheSize must be greater than 0");
            if (L2CacheSize <= 0)
                throw new ArgumentException("L2CacheSize must be greater than 0");
            if (ExpirationMinutes <= 0)
                throw new ArgumentException("ExpirationMinutes must be greater than 0");
        }

        /// <summary>
        /// Clone configuration
        /// </summary>
        public CachingOptimizationConfiguration Clone()
        {
            return new CachingOptimizationConfiguration
            {
                IsEnabled = IsEnabled,
                EnableL1MemoryCache = EnableL1MemoryCache,
                EnableL2WeakReferenceCache = EnableL2WeakReferenceCache,
                EnableL3DiskCache = EnableL3DiskCache,
                L1CacheSize = L1CacheSize,
                L2CacheSize = L2CacheSize,
                ExpirationMinutes = ExpirationMinutes,
                EnablePerformanceMonitoring = EnablePerformanceMonitoring,
                CleanupIntervalMinutes = CleanupIntervalMinutes,
                MaintenanceIntervalMs = MaintenanceIntervalMs,
                DiskCacheCleanupIntervalMs = DiskCacheCleanupIntervalMs,
                MaxL1ItemSizeBytes = MaxL1ItemSizeBytes,
                MaxL1CacheItems = MaxL1CacheItems,
                MaxL3TotalSizeBytes = MaxL3TotalSizeBytes,
                PerformanceWarningThresholdMs = PerformanceWarningThresholdMs
            };
        }

        #endregion

        #region Predefined Configurations

        /// <summary>
        /// Default caching configuration
        /// </summary>
        public static CachingOptimizationConfiguration Default => Optimized;

        /// <summary>
        /// Basic caching configuration
        /// </summary>
        public static CachingOptimizationConfiguration Basic => new()
        {
            IsEnabled = true,
            EnableL1MemoryCache = true,
            EnableL2WeakReferenceCache = false,
            EnableL3DiskCache = false,
            L1CacheSize = 500,
            ExpirationMinutes = 30,
            EnablePerformanceMonitoring = false,
            CleanupIntervalMinutes = 15
        };

        /// <summary>
        /// Optimized caching configuration
        /// </summary>
        public static CachingOptimizationConfiguration Optimized => new()
        {
            IsEnabled = true,
            EnableL1MemoryCache = true,
            EnableL2WeakReferenceCache = true,
            EnableL3DiskCache = false,
            L1CacheSize = 1000,
            L2CacheSize = 5000,
            ExpirationMinutes = 60,
            EnablePerformanceMonitoring = true,
            CleanupIntervalMinutes = 10
        };

        /// <summary>
        /// Advanced caching configuration with disk cache
        /// </summary>
        public static CachingOptimizationConfiguration Advanced => new()
        {
            IsEnabled = true,
            EnableL1MemoryCache = true,
            EnableL2WeakReferenceCache = true,
            EnableL3DiskCache = true,
            L1CacheSize = 2000,
            L2CacheSize = 10000,
            ExpirationMinutes = 120,
            EnablePerformanceMonitoring = true,
            CleanupIntervalMinutes = 5
        };

        /// <summary>
        /// High performance caching configuration
        /// </summary>
        public static CachingOptimizationConfiguration HighPerformance => new()
        {
            IsEnabled = true,
            EnableL1MemoryCache = true,
            EnableL2WeakReferenceCache = true,
            EnableL3DiskCache = true,
            L1CacheSize = 5000,
            L2CacheSize = 20000,
            ExpirationMinutes = 240,
            EnablePerformanceMonitoring = false, // Disable for max performance
            CleanupIntervalMinutes = 2
        };

        /// <summary>
        /// Disabled caching configuration
        /// </summary>
        public static CachingOptimizationConfiguration Disabled => new()
        {
            IsEnabled = false,
            EnableL1MemoryCache = false,
            EnableL2WeakReferenceCache = false,
            EnableL3DiskCache = false,
            EnablePerformanceMonitoring = false
        };

        #endregion
    }
}