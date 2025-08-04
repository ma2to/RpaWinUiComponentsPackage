// Models/MemoryManagementConfiguration.cs - ✅ NOVÉ: Memory Management Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre memory management service
    /// </summary>
    public class MemoryManagementConfiguration
    {
        #region Properties

        /// <summary>
        /// Povoliť diagnostické informácie
        /// </summary>
        public bool EnableDiagnostics { get; set; } = false;

        /// <summary>
        /// Povoliť object pooling
        /// </summary>
        public bool EnableObjectPooling { get; set; } = true;

        /// <summary>
        /// Povoliť weak references management
        /// </summary>
        public bool EnableWeakReferences { get; set; } = true;

        /// <summary>
        /// Povoliť automatické garbage collection
        /// </summary>
        public bool EnableAutomaticGC { get; set; } = true;

        /// <summary>
        /// Povoliť emergency cleanup pri kritickej pamäti
        /// </summary>
        public bool EnableEmergencyCleanup { get; set; } = true;

        /// <summary>
        /// Maximálna veľkosť object pool
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Interval čistenia v ms
        /// </summary>
        public int CleanupIntervalMs { get; set; } = 30000; // 30 seconds

        /// <summary>
        /// Interval monitorovania GC v ms
        /// </summary>
        public int GCMonitoringIntervalMs { get; set; } = 10000; // 10 seconds

        /// <summary>
        /// Threshold pre spustenie GC v bytes
        /// </summary>
        public long GCTriggerThresholdBytes { get; set; } = 50 * 1024 * 1024; // 50 MB

        /// <summary>
        /// Memory warning threshold v MB
        /// </summary>
        public double MemoryWarningThresholdMB { get; set; } = 500.0; // 500 MB

        /// <summary>
        /// Emergency cleanup threshold v MB
        /// </summary>
        public double EmergencyCleanupThresholdMB { get; set; } = 1000.0; // 1 GB

        /// <summary>
        /// Preallocation size pre object pools
        /// </summary>
        public int PreallocationSize { get; set; } = 10;

        /// <summary>
        /// Povoliť memory compaction
        /// </summary>
        public bool EnableMemoryCompaction { get; set; } = true;

        /// <summary>
        /// Minimum interval medzi GC calls v ms
        /// </summary>
        public int MinGCIntervalMs { get; set; } = 5000; // 5 seconds

        #endregion

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia pre malé aplikácie
        /// </summary>
        public static MemoryManagementConfiguration Basic => new()
        {
            EnableDiagnostics = false,
            EnableObjectPooling = true,
            EnableWeakReferences = false,
            EnableAutomaticGC = false,
            EnableEmergencyCleanup = false,
            MaxPoolSize = 50,
            CleanupIntervalMs = 60000, // 1 minute
            GCMonitoringIntervalMs = 30000, // 30 seconds
            GCTriggerThresholdBytes = 100 * 1024 * 1024, // 100 MB
            MemoryWarningThresholdMB = 1000.0, // 1 GB
            EmergencyCleanupThresholdMB = 2000.0, // 2 GB
            PreallocationSize = 5,
            EnableMemoryCompaction = false,
            MinGCIntervalMs = 10000 // 10 seconds
        };

        /// <summary>
        /// Optimalizovaná konfigurácia pre stredné aplikácie
        /// </summary>
        public static MemoryManagementConfiguration Optimized => new()
        {
            EnableDiagnostics = false,
            EnableObjectPooling = true,
            EnableWeakReferences = true,
            EnableAutomaticGC = true,
            EnableEmergencyCleanup = true,
            MaxPoolSize = 100,
            CleanupIntervalMs = 30000, // 30 seconds
            GCMonitoringIntervalMs = 10000, // 10 seconds
            GCTriggerThresholdBytes = 50 * 1024 * 1024, // 50 MB
            MemoryWarningThresholdMB = 500.0, // 500 MB
            EmergencyCleanupThresholdMB = 1000.0, // 1 GB
            PreallocationSize = 10,
            EnableMemoryCompaction = true,
            MinGCIntervalMs = 5000 // 5 seconds
        };

        /// <summary>
        /// Pokročilá konfigurácia pre enterprise aplikácie
        /// </summary>
        public static MemoryManagementConfiguration Advanced => new()
        {
            EnableDiagnostics = true,
            EnableObjectPooling = true,
            EnableWeakReferences = true,
            EnableAutomaticGC = true,
            EnableEmergencyCleanup = true,
            MaxPoolSize = 200,
            CleanupIntervalMs = 15000, // 15 seconds
            GCMonitoringIntervalMs = 5000, // 5 seconds
            GCTriggerThresholdBytes = 25 * 1024 * 1024, // 25 MB
            MemoryWarningThresholdMB = 300.0, // 300 MB
            EmergencyCleanupThresholdMB = 600.0, // 600 MB
            PreallocationSize = 20,
            EnableMemoryCompaction = true,
            MinGCIntervalMs = 2000 // 2 seconds
        };

        /// <summary>
        /// High-performance konfigurácia pre high-end aplikácie
        /// </summary>
        public static MemoryManagementConfiguration HighPerformance => new()
        {
            EnableDiagnostics = true,
            EnableObjectPooling = true,
            EnableWeakReferences = true,
            EnableAutomaticGC = true,
            EnableEmergencyCleanup = true,
            MaxPoolSize = 500,
            CleanupIntervalMs = 10000, // 10 seconds
            GCMonitoringIntervalMs = 2000, // 2 seconds
            GCTriggerThresholdBytes = 10 * 1024 * 1024, // 10 MB
            MemoryWarningThresholdMB = 200.0, // 200 MB
            EmergencyCleanupThresholdMB = 400.0, // 400 MB
            PreallocationSize = 50,
            EnableMemoryCompaction = true,
            MinGCIntervalMs = 1000 // 1 second
        };

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static MemoryManagementConfiguration Default => Optimized;

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (MaxPoolSize < 10) MaxPoolSize = 10;
            if (MaxPoolSize > 1000) MaxPoolSize = 1000;
            
            if (CleanupIntervalMs < 5000) CleanupIntervalMs = 5000;
            if (CleanupIntervalMs > 300000) CleanupIntervalMs = 300000;
            
            if (GCMonitoringIntervalMs < 1000) GCMonitoringIntervalMs = 1000;
            if (GCMonitoringIntervalMs > 60000) GCMonitoringIntervalMs = 60000;
            
            if (GCTriggerThresholdBytes < 10 * 1024 * 1024) GCTriggerThresholdBytes = 10 * 1024 * 1024; // 10 MB min
            if (GCTriggerThresholdBytes > 500 * 1024 * 1024) GCTriggerThresholdBytes = 500 * 1024 * 1024; // 500 MB max
            
            if (MemoryWarningThresholdMB < 100.0) MemoryWarningThresholdMB = 100.0;
            if (MemoryWarningThresholdMB > 4000.0) MemoryWarningThresholdMB = 4000.0;
            
            if (EmergencyCleanupThresholdMB < MemoryWarningThresholdMB) 
                EmergencyCleanupThresholdMB = MemoryWarningThresholdMB * 2;
            if (EmergencyCleanupThresholdMB > 8000.0) EmergencyCleanupThresholdMB = 8000.0;
            
            if (PreallocationSize < 0) PreallocationSize = 0;
            if (PreallocationSize > MaxPoolSize / 2) PreallocationSize = MaxPoolSize / 2;
            
            if (MinGCIntervalMs < 1000) MinGCIntervalMs = 1000;
            if (MinGCIntervalMs > 30000) MinGCIntervalMs = 30000;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public MemoryManagementConfiguration Clone()
        {
            return new MemoryManagementConfiguration
            {
                EnableDiagnostics = EnableDiagnostics,
                EnableObjectPooling = EnableObjectPooling,
                EnableWeakReferences = EnableWeakReferences,
                EnableAutomaticGC = EnableAutomaticGC,
                EnableEmergencyCleanup = EnableEmergencyCleanup,
                MaxPoolSize = MaxPoolSize,
                CleanupIntervalMs = CleanupIntervalMs,
                GCMonitoringIntervalMs = GCMonitoringIntervalMs,
                GCTriggerThresholdBytes = GCTriggerThresholdBytes,
                MemoryWarningThresholdMB = MemoryWarningThresholdMB,
                EmergencyCleanupThresholdMB = EmergencyCleanupThresholdMB,
                PreallocationSize = PreallocationSize,
                EnableMemoryCompaction = EnableMemoryCompaction,
                MinGCIntervalMs = MinGCIntervalMs
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe dostupnej pamäte
        /// </summary>
        public MemoryManagementConfiguration OptimizeForAvailableMemory(long availableMemoryBytes)
        {
            var optimized = Clone();
            var availableMemoryMB = availableMemoryBytes / (1024.0 * 1024.0);

            if (availableMemoryMB < 1000) // Less than 1GB
            {
                // Conservative settings for low memory systems
                optimized.MaxPoolSize = Math.Min(50, optimized.MaxPoolSize);
                optimized.GCTriggerThresholdBytes = 10 * 1024 * 1024; // 10 MB
                optimized.MemoryWarningThresholdMB = Math.Min(200.0, availableMemoryMB * 0.6);
                optimized.EmergencyCleanupThresholdMB = Math.Min(400.0, availableMemoryMB * 0.8);
                optimized.EnableAutomaticGC = true;
                optimized.EnableEmergencyCleanup = true;
                optimized.CleanupIntervalMs = 15000; // More frequent cleanup
            }
            else if (availableMemoryMB < 4000) // 1-4GB
            {
                // Balanced settings
                optimized.MaxPoolSize = Math.Min(100, optimized.MaxPoolSize);
                optimized.GCTriggerThresholdBytes = 25 * 1024 * 1024; // 25 MB
                optimized.MemoryWarningThresholdMB = Math.Min(500.0, availableMemoryMB * 0.4);
                optimized.EmergencyCleanupThresholdMB = Math.Min(1000.0, availableMemoryMB * 0.6);
            }
            else // 4GB+
            {
                // Aggressive settings for high memory systems
                optimized.MaxPoolSize = Math.Min(200, optimized.MaxPoolSize);
                optimized.GCTriggerThresholdBytes = 50 * 1024 * 1024; // 50 MB
                optimized.MemoryWarningThresholdMB = Math.Min(1000.0, availableMemoryMB * 0.3);
                optimized.EmergencyCleanupThresholdMB = Math.Min(2000.0, availableMemoryMB * 0.5);
                optimized.CleanupIntervalMs = 30000; // Less frequent cleanup
            }

            optimized.Validate();
            return optimized;
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe performance profile
        /// </summary>
        public MemoryManagementConfiguration OptimizeForPerformanceProfile(MemoryPerformanceProfile profile)
        {
            var optimized = Clone();

            switch (profile)
            {
                case MemoryPerformanceProfile.Conservative:
                    // Minimize memory usage, accept slower performance
                    optimized.MaxPoolSize = 25;
                    optimized.EnableObjectPooling = true;
                    optimized.EnableWeakReferences = true;
                    optimized.EnableAutomaticGC = true;
                    optimized.EnableEmergencyCleanup = true;
                    optimized.CleanupIntervalMs = 10000; // Frequent cleanup
                    optimized.GCTriggerThresholdBytes = 5 * 1024 * 1024; // 5 MB
                    break;

                case MemoryPerformanceProfile.Balanced:
                    // Default balanced settings - already good
                    break;

                case MemoryPerformanceProfile.Aggressive:
                    // Maximize performance, use more memory
                    optimized.MaxPoolSize = 500;
                    optimized.EnableObjectPooling = true;
                    optimized.EnableWeakReferences = false; // Skip weak ref overhead
                    optimized.EnableAutomaticGC = false; // Manual GC control
                    optimized.EnableEmergencyCleanup = true;
                    optimized.CleanupIntervalMs = 60000; // Less frequent cleanup
                    optimized.GCTriggerThresholdBytes = 100 * 1024 * 1024; // 100 MB
                    break;
            }

            optimized.Validate();
            return optimized;
        }

        /// <summary>
        /// ✅ NOVÉ: Získa diagnostické informácie o konfigurácii
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var gcThresholdMB = GCTriggerThresholdBytes / (1024.0 * 1024.0);
            
            return $"MemoryManagement: " +
                   $"ObjectPooling={EnableObjectPooling}(MaxSize={MaxPoolSize}), " +
                   $"WeakRefs={EnableWeakReferences}, AutoGC={EnableAutomaticGC}, " +
                   $"Cleanup: {CleanupIntervalMs}ms, GCTrigger: {gcThresholdMB:F1}MB, " +
                   $"Warnings: {MemoryWarningThresholdMB:F0}MB/{EmergencyCleanupThresholdMB:F0}MB, " +
                   $"Features: Emergency={EnableEmergencyCleanup}, Compaction={EnableMemoryCompaction}";
        }

        /// <summary>
        /// ✅ NOVÉ: Vypočíta očakávané memory savings
        /// </summary>
        public MemoryOptimizationEstimate EstimateMemoryOptimization()
        {
            var poolingSavings = EnableObjectPooling ? 40.0 : 0.0; // 40% savings from pooling
            var weakRefSavings = EnableWeakReferences ? 15.0 : 0.0; // 15% savings from weak refs
            var gcOptimizationSavings = EnableAutomaticGC ? 20.0 : 0.0; // 20% savings from optimized GC
            
            var totalSavings = Math.Min(75.0, poolingSavings + weakRefSavings + gcOptimizationSavings);
            
            return new MemoryOptimizationEstimate
            {
                ObjectPoolingSavingsPercent = poolingSavings,
                WeakReferencesSavingsPercent = weakRefSavings,
                GCOptimizationSavingsPercent = gcOptimizationSavings,
                TotalMemorySavingsPercent = totalSavings,
                EstimatedPoolHitRatio = EnableObjectPooling ? 85.0 : 0.0,
                RecommendedForDatasetSize = GetRecommendedDatasetSize()
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Získa odporúčanú veľkosť datasetu pre túto konfiguráciu
        /// </summary>
        private DatasetSize GetRecommendedDatasetSize()
        {
            if (MaxPoolSize >= 200 && EnableObjectPooling && EnableWeakReferences)
                return DatasetSize.VeryLarge; // 100k+ rows
            
            if (MaxPoolSize >= 100 && EnableObjectPooling)
                return DatasetSize.Large; // 10k+ rows
            
            if (MaxPoolSize >= 50)
                return DatasetSize.Medium; // 1k+ rows
                
            return DatasetSize.Small; // <1k rows
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÉ: Memory performance profile
    /// </summary>
    public enum MemoryPerformanceProfile
    {
        Conservative,  // Minimal memory usage, slower performance
        Balanced,      // Balance between memory and performance
        Aggressive     // Maximum performance, more memory usage
    }

    /// <summary>
    /// ✅ NOVÉ: Odporúčaná veľkosť datasetu
    /// </summary>
    public enum DatasetSize
    {
        Small,      // <1k rows
        Medium,     // 1k-10k rows
        Large,      // 10k-100k rows
        VeryLarge   // 100k+ rows
    }

    /// <summary>
    /// ✅ NOVÉ: Odhad memory optimization
    /// </summary>
    public class MemoryOptimizationEstimate
    {
        public double ObjectPoolingSavingsPercent { get; set; }
        public double WeakReferencesSavingsPercent { get; set; }
        public double GCOptimizationSavingsPercent { get; set; }
        public double TotalMemorySavingsPercent { get; set; }
        public double EstimatedPoolHitRatio { get; set; }
        public DatasetSize RecommendedForDatasetSize { get; set; }

        public override string ToString()
        {
            return $"Memory Optimization Estimate: " +
                   $"Pooling={ObjectPoolingSavingsPercent:F1}%, WeakRefs={WeakReferencesSavingsPercent:F1}%, " +
                   $"GC={GCOptimizationSavingsPercent:F1}%, Total={TotalMemorySavingsPercent:F1}%, " +
                   $"PoolHit={EstimatedPoolHitRatio:F1}%, RecommendedFor={RecommendedForDatasetSize}";
        }
    }
}