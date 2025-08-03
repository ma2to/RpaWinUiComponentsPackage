// Models/Validation/AdaptiveValidationConfiguration.cs - ✅ NOVÉ: Adaptive Validation Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre adaptive validation service
    /// </summary>
    public class AdaptiveValidationConfiguration
    {
        #region Properties

        /// <summary>
        /// Povoliť adaptive mode switching
        /// </summary>
        public bool EnableAdaptiveMode { get; set; } = true;

        /// <summary>
        /// Povoliť validation caching
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Povoliť diagnostické informácie
        /// </summary>
        public bool EnableDiagnostics { get; set; } = false;

        /// <summary>
        /// Maximálna veľkosť validation cache
        /// </summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>
        /// Expiračný čas cache entries v ms
        /// </summary>
        public int CacheExpirationMs { get; set; } = 300000; // 5 minút

        /// <summary>
        /// Threshold pre high frequency validation (validácie za minútu)
        /// </summary>
        public int HighFrequencyThreshold { get; set; } = 10;

        /// <summary>
        /// Threshold pre low frequency validation (validácie za minútu)
        /// </summary>
        public int LowFrequencyThreshold { get; set; } = 2;

        /// <summary>
        /// Timeout pre realtime mode v ms (ak niet activity, switch na batch)
        /// </summary>
        public int RealtimeModeTimeoutMs { get; set; } = 5000; // 5 sekúnd

        /// <summary>
        /// Timeout pre batch mode v ms (ak nie je bulk operation, switch na realtime)
        /// </summary>
        public int BatchModeTimeoutMs { get; set; } = 30000; // 30 sekúnd

        /// <summary>
        /// Threshold pre automatické prepnutie na batch mode (počet buniek)
        /// </summary>
        public int BatchModeThreshold { get; set; } = 5;

        /// <summary>
        /// Veľkosť batch-a pri batch validácii
        /// </summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>
        /// Maximálny počet entries pre frequency tracking
        /// </summary>
        public int MaxFrequencyTrackingEntries { get; set; } = 500;

        /// <summary>
        /// Throttling delay pre realtime validation v ms
        /// </summary>
        public int RealtimeThrottleMs { get; set; } = 100;

        /// <summary>
        /// Maximálny parallelism pre batch validation
        /// </summary>
        public int MaxBatchConcurrency { get; set; } = Environment.ProcessorCount;

        #endregion

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia pre malé aplikácie
        /// </summary>
        public static AdaptiveValidationConfiguration Basic => new()
        {
            EnableAdaptiveMode = false,
            EnableCaching = false,
            MaxCacheSize = 100,
            CacheExpirationMs = 60000, // 1 minúta
            HighFrequencyThreshold = 5,
            LowFrequencyThreshold = 1,
            RealtimeModeTimeoutMs = 10000, // 10 sekúnd
            BatchModeThreshold = 10,
            BatchSize = 20,
            RealtimeThrottleMs = 200,
            MaxBatchConcurrency = 2
        };

        /// <summary>
        /// Optimalizovaná konfigurácia pre stredné aplikácie
        /// </summary>
        public static AdaptiveValidationConfiguration Optimized => new()
        {
            EnableAdaptiveMode = true,
            EnableCaching = true,
            MaxCacheSize = 500,
            CacheExpirationMs = 180000, // 3 minúty
            HighFrequencyThreshold = 8,
            LowFrequencyThreshold = 2,
            RealtimeModeTimeoutMs = 5000,
            BatchModeThreshold = 5,
            BatchSize = 30,
            RealtimeThrottleMs = 100,
            MaxBatchConcurrency = Math.Max(2, Environment.ProcessorCount / 2)
        };

        /// <summary>
        /// Pokročilá konfigurácia pre enterprise aplikácie
        /// </summary>
        public static AdaptiveValidationConfiguration Advanced => new()
        {
            EnableAdaptiveMode = true,
            EnableCaching = true,
            EnableDiagnostics = true,
            MaxCacheSize = 2000,
            CacheExpirationMs = 300000, // 5 minút
            HighFrequencyThreshold = 15,
            LowFrequencyThreshold = 3,
            RealtimeModeTimeoutMs = 3000,
            BatchModeThreshold = 3,
            BatchSize = 50,
            MaxFrequencyTrackingEntries = 1000,
            RealtimeThrottleMs = 50,
            MaxBatchConcurrency = Environment.ProcessorCount
        };

        /// <summary>
        /// High-performance konfigurácia pre veľké datasety
        /// </summary>
        public static AdaptiveValidationConfiguration HighPerformance => new()
        {
            EnableAdaptiveMode = true,
            EnableCaching = true,
            EnableDiagnostics = true,
            MaxCacheSize = 5000,
            CacheExpirationMs = 600000, // 10 minút
            HighFrequencyThreshold = 20,
            LowFrequencyThreshold = 5,
            RealtimeModeTimeoutMs = 1000, // Agresívne prepínanie
            BatchModeThreshold = 2, // Skoré prepnutie na batch
            BatchSize = 100,
            MaxFrequencyTrackingEntries = 2000,
            RealtimeThrottleMs = 25, // Agresívny throttling
            MaxBatchConcurrency = Environment.ProcessorCount * 2
        };

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static AdaptiveValidationConfiguration Default => Optimized;

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (MaxCacheSize < 10) MaxCacheSize = 10;
            if (MaxCacheSize > 10000) MaxCacheSize = 10000;
            
            if (CacheExpirationMs < 1000) CacheExpirationMs = 1000;
            if (CacheExpirationMs > 3600000) CacheExpirationMs = 3600000; // Max 1 hodina
            
            if (HighFrequencyThreshold < 1) HighFrequencyThreshold = 1;
            if (LowFrequencyThreshold < 0) LowFrequencyThreshold = 0;
            if (HighFrequencyThreshold <= LowFrequencyThreshold) 
                HighFrequencyThreshold = LowFrequencyThreshold + 1;
            
            if (RealtimeModeTimeoutMs < 100) RealtimeModeTimeoutMs = 100;
            if (BatchModeTimeoutMs < 1000) BatchModeTimeoutMs = 1000;
            
            if (BatchModeThreshold < 1) BatchModeThreshold = 1;
            if (BatchSize < 1) BatchSize = 1;
            if (BatchSize > 1000) BatchSize = 1000;
            
            if (MaxFrequencyTrackingEntries < 10) MaxFrequencyTrackingEntries = 10;
            if (RealtimeThrottleMs < 1) RealtimeThrottleMs = 1;
            if (RealtimeThrottleMs > 1000) RealtimeThrottleMs = 1000;
            
            if (MaxBatchConcurrency < 1) MaxBatchConcurrency = 1;
            if (MaxBatchConcurrency > Environment.ProcessorCount * 4)
                MaxBatchConcurrency = Environment.ProcessorCount * 4;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public AdaptiveValidationConfiguration Clone()
        {
            return new AdaptiveValidationConfiguration
            {
                EnableAdaptiveMode = EnableAdaptiveMode,
                EnableCaching = EnableCaching,
                EnableDiagnostics = EnableDiagnostics,
                MaxCacheSize = MaxCacheSize,
                CacheExpirationMs = CacheExpirationMs,
                HighFrequencyThreshold = HighFrequencyThreshold,
                LowFrequencyThreshold = LowFrequencyThreshold,
                RealtimeModeTimeoutMs = RealtimeModeTimeoutMs,
                BatchModeTimeoutMs = BatchModeTimeoutMs,
                BatchModeThreshold = BatchModeThreshold,
                BatchSize = BatchSize,
                MaxFrequencyTrackingEntries = MaxFrequencyTrackingEntries,
                RealtimeThrottleMs = RealtimeThrottleMs,
                MaxBatchConcurrency = MaxBatchConcurrency
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe očakávanej záťaže
        /// </summary>
        public AdaptiveValidationConfiguration OptimizeForWorkload(ValidationWorkloadType workload)
        {
            var optimized = Clone();

            switch (workload)
            {
                case ValidationWorkloadType.LightEditing:
                    // Optimalizácia pre light editing - favor realtime
                    optimized.EnableAdaptiveMode = false; // Stick to realtime
                    optimized.RealtimeThrottleMs = 200;
                    optimized.MaxCacheSize = 200;
                    optimized.BatchModeThreshold = 20; // Vysoký threshold
                    break;

                case ValidationWorkloadType.HeavyEditing:
                    // Optimalizácia pre heavy editing - adaptive switching
                    optimized.EnableAdaptiveMode = true;
                    optimized.HighFrequencyThreshold = 5; // Nižší threshold
                    optimized.RealtimeModeTimeoutMs = 2000; // Faster switching
                    optimized.BatchModeThreshold = 3;
                    break;

                case ValidationWorkloadType.BulkOperations:
                    // Optimalizácia pre bulk operations - favor batch
                    optimized.BatchModeThreshold = 1; // Immediate batch mode
                    optimized.BatchSize = 100; // Väčšie batches
                    optimized.MaxBatchConcurrency = Environment.ProcessorCount;
                    optimized.RealtimeModeTimeoutMs = 500; // Quick switch to batch
                    break;

                case ValidationWorkloadType.Mixed:
                    // Balanced configuration - default
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
            return $"AdaptiveValidation: " +
                   $"AdaptiveMode={EnableAdaptiveMode}, " +
                   $"Caching={EnableCaching}(Size={MaxCacheSize}), " +
                   $"Thresholds: HF={HighFrequencyThreshold}/LF={LowFrequencyThreshold}, " +
                   $"Timeouts: RT={RealtimeModeTimeoutMs}ms/Batch={BatchModeTimeoutMs}ms, " +
                   $"BatchThreshold={BatchModeThreshold}, " +
                   $"Throttle={RealtimeThrottleMs}ms, " +
                   $"Concurrency={MaxBatchConcurrency}";
        }

        /// <summary>
        /// ✅ NOVÉ: Vypočíta očakávané memory footprint
        /// </summary>
        public long EstimateMemoryFootprint()
        {
            // Odhad memory usage pre cache entries
            const int avgCacheEntrySize = 200; // bytes (cacheKey + ValidationResult)
            const int avgFrequencyEntrySize = 50; // bytes (string key + int + DateTime)
            
            var cacheMemory = MaxCacheSize * avgCacheEntrySize;
            var frequencyMemory = MaxFrequencyTrackingEntries * avgFrequencyEntrySize;
            
            return cacheMemory + frequencyMemory;
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÉ: Typ workload-u pre optimalizáciu
    /// </summary>
    public enum ValidationWorkloadType
    {
        LightEditing,    // Občasné editovanie jednotlivých buniek
        HeavyEditing,    // Časté editovanie s vysokou frekvenciou
        BulkOperations,  // Import, paste, bulk updates
        Mixed           // Kombinované workload
    }
}