// Models/UIThreadOptimizationConfiguration.cs - ✅ NOVÉ: UI Thread Optimization Configuration
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre UI thread optimization service
    /// </summary>
    public class UIThreadOptimizationConfiguration
    {
        #region Properties

        /// <summary>
        /// Povoliť diagnostické informácie
        /// </summary>
        public bool EnableDiagnostics { get; set; } = false;

        /// <summary>
        /// Interval pre realtime updates v ms (60 FPS = 16.67ms)
        /// </summary>
        public int RealtimeUpdateIntervalMs { get; set; } = 16;

        /// <summary>
        /// Interval pre batch updates v ms (10 FPS = 100ms)
        /// </summary>
        public int BatchUpdateIntervalMs { get; set; } = 100;

        /// <summary>
        /// Maximálny čas frame-u v ms pre realtime mode
        /// </summary>
        public double MaxFrameTimeMs { get; set; } = 8.0;

        /// <summary>
        /// Maximálny čas frame-u v ms pre batch mode
        /// </summary>
        public double MaxBatchFrameTimeMs { get; set; } = 30.0;

        /// <summary>
        /// Počet updates na frame v realtime mode
        /// </summary>
        public int RealtimeUpdatesPerFrame { get; set; } = 5;

        /// <summary>
        /// Počet updates na frame v batch mode
        /// </summary>
        public int BatchUpdatesPerFrame { get; set; } = 20;

        /// <summary>
        /// Threshold pre automatické prepnutie na batch mode
        /// </summary>
        public int BatchModeThreshold { get; set; } = 50;

        /// <summary>
        /// Threshold pre performance warning v ms
        /// </summary>
        public double PerformanceWarningThresholdMs { get; set; } = 16.0;

        /// <summary>
        /// Maximálny počet pending updates pred drop-om
        /// </summary>
        public int MaxPendingUpdates { get; set; } = 1000;

        /// <summary>
        /// Povoliť automatic mode switching
        /// </summary>
        public bool EnableAutomaticModeSwitching { get; set; } = true;

        /// <summary>
        /// Povoliť update merging
        /// </summary>
        public bool EnableUpdateMerging { get; set; } = true;

        /// <summary>
        /// Povoliť time budgeting
        /// </summary>
        public bool EnableTimeBudgeting { get; set; } = true;

        /// <summary>
        /// Povoliť priority-based processing
        /// </summary>
        public bool EnablePriorityProcessing { get; set; } = true;

        #endregion

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia pre malé aplikácie
        /// </summary>
        public static UIThreadOptimizationConfiguration Basic => new()
        {
            EnableDiagnostics = false,
            RealtimeUpdateIntervalMs = 33, // 30 FPS
            BatchUpdateIntervalMs = 200,   // 5 FPS
            MaxFrameTimeMs = 16.0,
            MaxBatchFrameTimeMs = 50.0,
            RealtimeUpdatesPerFrame = 3,
            BatchUpdatesPerFrame = 10,
            BatchModeThreshold = 100,
            PerformanceWarningThresholdMs = 33.0,
            MaxPendingUpdates = 500,
            EnableAutomaticModeSwitching = false,
            EnableUpdateMerging = true,
            EnableTimeBudgeting = false,
            EnablePriorityProcessing = true
        };

        /// <summary>
        /// Optimalizovaná konfigurácia pre stredné aplikácie
        /// </summary>
        public static UIThreadOptimizationConfiguration Optimized => new()
        {
            EnableDiagnostics = false,
            RealtimeUpdateIntervalMs = 16, // 60 FPS
            BatchUpdateIntervalMs = 100,   // 10 FPS
            MaxFrameTimeMs = 8.0,
            MaxBatchFrameTimeMs = 30.0,
            RealtimeUpdatesPerFrame = 5,
            BatchUpdatesPerFrame = 15,
            BatchModeThreshold = 50,
            PerformanceWarningThresholdMs = 16.0,
            MaxPendingUpdates = 750,
            EnableAutomaticModeSwitching = true,
            EnableUpdateMerging = true,
            EnableTimeBudgeting = true,
            EnablePriorityProcessing = true
        };

        /// <summary>
        /// Pokročilá konfigurácia pre enterprise aplikácie
        /// </summary>
        public static UIThreadOptimizationConfiguration Advanced => new()
        {
            EnableDiagnostics = true,
            RealtimeUpdateIntervalMs = 8,  // 120 FPS
            BatchUpdateIntervalMs = 50,    // 20 FPS
            MaxFrameTimeMs = 6.0,
            MaxBatchFrameTimeMs = 20.0,
            RealtimeUpdatesPerFrame = 8,
            BatchUpdatesPerFrame = 25,
            BatchModeThreshold = 30,
            PerformanceWarningThresholdMs = 12.0,
            MaxPendingUpdates = 1000,
            EnableAutomaticModeSwitching = true,
            EnableUpdateMerging = true,
            EnableTimeBudgeting = true,
            EnablePriorityProcessing = true
        };

        /// <summary>
        /// High-performance konfigurácia pre high-end aplikácie
        /// </summary>
        public static UIThreadOptimizationConfiguration HighPerformance => new()
        {
            EnableDiagnostics = true,
            RealtimeUpdateIntervalMs = 4,  // 240 FPS (high refresh rate displays)
            BatchUpdateIntervalMs = 25,    // 40 FPS
            MaxFrameTimeMs = 4.0,
            MaxBatchFrameTimeMs = 12.0,
            RealtimeUpdatesPerFrame = 12,
            BatchUpdatesPerFrame = 40,
            BatchModeThreshold = 20,
            PerformanceWarningThresholdMs = 8.0,
            MaxPendingUpdates = 2000,
            EnableAutomaticModeSwitching = true,
            EnableUpdateMerging = true,
            EnableTimeBudgeting = true,
            EnablePriorityProcessing = true
        };

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static UIThreadOptimizationConfiguration Default => Optimized;

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (RealtimeUpdateIntervalMs < 1) RealtimeUpdateIntervalMs = 1;
            if (RealtimeUpdateIntervalMs > 100) RealtimeUpdateIntervalMs = 100;
            
            if (BatchUpdateIntervalMs < 10) BatchUpdateIntervalMs = 10;
            if (BatchUpdateIntervalMs > 1000) BatchUpdateIntervalMs = 1000;
            
            if (MaxFrameTimeMs < 1.0) MaxFrameTimeMs = 1.0;
            if (MaxFrameTimeMs > 50.0) MaxFrameTimeMs = 50.0;
            
            if (MaxBatchFrameTimeMs < MaxFrameTimeMs) MaxBatchFrameTimeMs = MaxFrameTimeMs * 2;
            if (MaxBatchFrameTimeMs > 200.0) MaxBatchFrameTimeMs = 200.0;
            
            if (RealtimeUpdatesPerFrame < 1) RealtimeUpdatesPerFrame = 1;
            if (RealtimeUpdatesPerFrame > 50) RealtimeUpdatesPerFrame = 50;
            
            if (BatchUpdatesPerFrame < 1) BatchUpdatesPerFrame = 1;
            if (BatchUpdatesPerFrame > 100) BatchUpdatesPerFrame = 100;
            
            if (BatchModeThreshold < 5) BatchModeThreshold = 5;
            if (BatchModeThreshold > 500) BatchModeThreshold = 500;
            
            if (PerformanceWarningThresholdMs < 1.0) PerformanceWarningThresholdMs = 1.0;
            if (PerformanceWarningThresholdMs > 100.0) PerformanceWarningThresholdMs = 100.0;
            
            if (MaxPendingUpdates < 10) MaxPendingUpdates = 10;
            if (MaxPendingUpdates > 10000) MaxPendingUpdates = 10000;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public UIThreadOptimizationConfiguration Clone()
        {
            return new UIThreadOptimizationConfiguration
            {
                EnableDiagnostics = EnableDiagnostics,
                RealtimeUpdateIntervalMs = RealtimeUpdateIntervalMs,
                BatchUpdateIntervalMs = BatchUpdateIntervalMs,
                MaxFrameTimeMs = MaxFrameTimeMs,
                MaxBatchFrameTimeMs = MaxBatchFrameTimeMs,
                RealtimeUpdatesPerFrame = RealtimeUpdatesPerFrame,
                BatchUpdatesPerFrame = BatchUpdatesPerFrame,
                BatchModeThreshold = BatchModeThreshold,
                PerformanceWarningThresholdMs = PerformanceWarningThresholdMs,
                MaxPendingUpdates = MaxPendingUpdates,
                EnableAutomaticModeSwitching = EnableAutomaticModeSwitching,
                EnableUpdateMerging = EnableUpdateMerging,
                EnableTimeBudgeting = EnableTimeBudgeting,
                EnablePriorityProcessing = EnablePriorityProcessing
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe target FPS
        /// </summary>
        public UIThreadOptimizationConfiguration OptimizeForTargetFPS(int targetFPS)
        {
            var optimized = Clone();

            var targetIntervalMs = Math.Max(1, 1000 / targetFPS);
            
            optimized.RealtimeUpdateIntervalMs = targetIntervalMs;
            optimized.BatchUpdateIntervalMs = Math.Max(targetIntervalMs * 2, 50);
            
            // Adjust frame time budget based on target FPS
            optimized.MaxFrameTimeMs = Math.Min(targetIntervalMs * 0.5, 8.0);
            optimized.MaxBatchFrameTimeMs = Math.Min(optimized.BatchUpdateIntervalMs * 0.3, 30.0);
            
            // Adjust updates per frame based on performance target
            if (targetFPS >= 120)
            {
                optimized.RealtimeUpdatesPerFrame = Math.Min(12, optimized.RealtimeUpdatesPerFrame);
                optimized.BatchUpdatesPerFrame = Math.Min(40, optimized.BatchUpdatesPerFrame);
            }
            else if (targetFPS >= 60)
            {
                optimized.RealtimeUpdatesPerFrame = Math.Min(8, optimized.RealtimeUpdatesPerFrame);
                optimized.BatchUpdatesPerFrame = Math.Min(25, optimized.BatchUpdatesPerFrame);
            }
            else
            {
                optimized.RealtimeUpdatesPerFrame = Math.Min(5, optimized.RealtimeUpdatesPerFrame);
                optimized.BatchUpdatesPerFrame = Math.Min(15, optimized.BatchUpdatesPerFrame);
            }

            optimized.Validate();
            return optimized;
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe device performance
        /// </summary>
        public UIThreadOptimizationConfiguration OptimizeForDevice(DevicePerformanceLevel deviceLevel)
        {
            var optimized = Clone();

            switch (deviceLevel)
            {
                case DevicePerformanceLevel.Low:
                    // Conservative settings for low-end devices
                    optimized.RealtimeUpdateIntervalMs = 33; // 30 FPS
                    optimized.BatchUpdateIntervalMs = 200;   // 5 FPS
                    optimized.MaxFrameTimeMs = 16.0;
                    optimized.RealtimeUpdatesPerFrame = 3;
                    optimized.BatchUpdatesPerFrame = 8;
                    optimized.BatchModeThreshold = 20;
                    optimized.EnableAutomaticModeSwitching = true; // Aggressive switching
                    break;

                case DevicePerformanceLevel.Medium:
                    // Balanced settings
                    optimized.RealtimeUpdateIntervalMs = 16; // 60 FPS
                    optimized.BatchUpdateIntervalMs = 100;   // 10 FPS
                    optimized.MaxFrameTimeMs = 8.0;
                    optimized.RealtimeUpdatesPerFrame = 5;
                    optimized.BatchUpdatesPerFrame = 15;
                    optimized.BatchModeThreshold = 40;
                    break;

                case DevicePerformanceLevel.High:
                    // Aggressive settings for high-end devices
                    optimized.RealtimeUpdateIntervalMs = 8;  // 120 FPS
                    optimized.BatchUpdateIntervalMs = 50;    // 20 FPS
                    optimized.MaxFrameTimeMs = 6.0;
                    optimized.RealtimeUpdatesPerFrame = 10;
                    optimized.BatchUpdatesPerFrame = 30;
                    optimized.BatchModeThreshold = 80;
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
            var realtimeFPS = 1000.0 / RealtimeUpdateIntervalMs;
            var batchFPS = 1000.0 / BatchUpdateIntervalMs;
            
            return $"UIThreadOptimization: " +
                   $"Realtime={realtimeFPS:F1}FPS({RealtimeUpdatesPerFrame}updates), " +
                   $"Batch={batchFPS:F1}FPS({BatchUpdatesPerFrame}updates), " +
                   $"FrameBudget: RT={MaxFrameTimeMs}ms/Batch={MaxBatchFrameTimeMs}ms, " +
                   $"BatchThreshold={BatchModeThreshold}, " +
                   $"Features: AutoSwitch={EnableAutomaticModeSwitching}, " +
                   $"Merge={EnableUpdateMerging}, TimeBudget={EnableTimeBudgeting}, " +
                   $"Priority={EnablePriorityProcessing}";
        }

        /// <summary>
        /// ✅ NOVÉ: Vypočíta očakávané performance metriky
        /// </summary>
        public UIPerformanceEstimate EstimatePerformance()
        {
            var realtimeFPS = 1000.0 / RealtimeUpdateIntervalMs;
            var batchFPS = 1000.0 / BatchUpdateIntervalMs;
            
            var maxRealtimeUpdatesPerSec = realtimeFPS * RealtimeUpdatesPerFrame;
            var maxBatchUpdatesPerSec = batchFPS * BatchUpdatesPerFrame;
            
            return new UIPerformanceEstimate
            {
                RealtimeFPS = realtimeFPS,
                BatchFPS = batchFPS,
                MaxRealtimeUpdatesPerSecond = (int)maxRealtimeUpdatesPerSec,
                MaxBatchUpdatesPerSecond = (int)maxBatchUpdatesPerSec,
                EstimatedFrameBudgetUtilization = (MaxFrameTimeMs / RealtimeUpdateIntervalMs) * 100,
                RecommendedForDeviceLevel = GetRecommendedDeviceLevel()
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Získa odporúčanú úroveň zariadenia pre túto konfiguráciu
        /// </summary>
        private DevicePerformanceLevel GetRecommendedDeviceLevel()
        {
            var realtimeFPS = 1000.0 / RealtimeUpdateIntervalMs;
            
            if (realtimeFPS >= 100) return DevicePerformanceLevel.High;
            if (realtimeFPS >= 50) return DevicePerformanceLevel.Medium;
            return DevicePerformanceLevel.Low;
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÉ: Úroveň výkonu zariadenia
    /// </summary>
    public enum DevicePerformanceLevel
    {
        Low,      // Nízky výkon - conservative settings
        Medium,   // Stredný výkon - balanced settings
        High      // Vysoký výkon - aggressive settings
    }

    /// <summary>
    /// ✅ NOVÉ: Odhad výkonu konfigurácie
    /// </summary>
    public class UIPerformanceEstimate
    {
        public double RealtimeFPS { get; set; }
        public double BatchFPS { get; set; }
        public int MaxRealtimeUpdatesPerSecond { get; set; }
        public int MaxBatchUpdatesPerSecond { get; set; }
        public double EstimatedFrameBudgetUtilization { get; set; }
        public DevicePerformanceLevel RecommendedForDeviceLevel { get; set; }

        public override string ToString()
        {
            return $"Performance Estimate: " +
                   $"RT={RealtimeFPS:F1}FPS({MaxRealtimeUpdatesPerSecond}ups), " +
                   $"Batch={BatchFPS:F1}FPS({MaxBatchUpdatesPerSecond}ups), " +
                   $"BudgetUsage={EstimatedFrameBudgetUtilization:F1}%, " +
                   $"RecommendedFor={RecommendedForDeviceLevel}";
        }
    }
}