// Models/DataBindingOptimizationConfiguration.cs - ✅ NOVÉ: Data Binding Optimization Configuration
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Configuration for Data Binding Optimization
    /// </summary>
    internal class DataBindingOptimizationConfiguration : INotifyPropertyChanged, ICloneable
    {
        #region Private Fields

        private bool _enableChangeTracking = true;
        private bool _enablePropertyThrottling = true;
        private bool _enableBulkOperations = true;
        private int _changeProcessingIntervalMs = 100;
        private int _bulkProcessingIntervalMs = 500;
        private int _throttleIntervalMs = 50;
        private int _defaultThrottleMs = 200;
        private int _maxChangesPerBatch = 100;
        private int _bulkOperationTimeoutMs = 30000;
        private int _performanceWarningThresholdMs = 1000;
        private bool _enableParallelBulkProcessing = true;
        private int _parallelBulkThreshold = 50;
        private int _maxParallelBulkOperations = 4;
        private DataBindingOptimizationLevel _performanceLevel = DataBindingOptimizationLevel.Optimized;

        #endregion

        #region Properties

        /// <summary>
        /// Zapína/vypína change tracking optimization
        /// </summary>
        public bool EnableChangeTracking
        {
            get => _enableChangeTracking;
            set
            {
                if (value != _enableChangeTracking)
                {
                    _enableChangeTracking = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Zapína/vypína property change throttling
        /// </summary>
        public bool EnablePropertyThrottling
        {
            get => _enablePropertyThrottling;
            set
            {
                if (value != _enablePropertyThrottling)
                {
                    _enablePropertyThrottling = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Zapína/vypína bulk operations batching
        /// </summary>
        public bool EnableBulkOperations
        {
            get => _enableBulkOperations;
            set
            {
                if (value != _enableBulkOperations)
                {
                    _enableBulkOperations = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Interval spracovania pending changes (ms)
        /// </summary>
        public int ChangeProcessingIntervalMs
        {
            get => _changeProcessingIntervalMs;
            set
            {
                if (value != _changeProcessingIntervalMs && value > 0)
                {
                    _changeProcessingIntervalMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Interval spracovania bulk operations (ms)
        /// </summary>
        public int BulkProcessingIntervalMs
        {
            get => _bulkProcessingIntervalMs;
            set
            {
                if (value != _bulkProcessingIntervalMs && value > 0)
                {
                    _bulkProcessingIntervalMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Interval throttle timer (ms)
        /// </summary>
        public int ThrottleIntervalMs
        {
            get => _throttleIntervalMs;
            set
            {
                if (value != _throttleIntervalMs && value > 0)
                {
                    _throttleIntervalMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Predvolený throttle delay pre properties (ms)
        /// </summary>
        public int DefaultThrottleMs
        {
            get => _defaultThrottleMs;
            set
            {
                if (value != _defaultThrottleMs && value >= 0)
                {
                    _defaultThrottleMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximálny počet changes spracovaných v jednom batchi
        /// </summary>
        public int MaxChangesPerBatch
        {
            get => _maxChangesPerBatch;
            set
            {
                if (value != _maxChangesPerBatch && value > 0)
                {
                    _maxChangesPerBatch = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Timeout pre bulk operations (ms)
        /// </summary>
        public int BulkOperationTimeoutMs
        {
            get => _bulkOperationTimeoutMs;
            set
            {
                if (value != _bulkOperationTimeoutMs && value > 0)
                {
                    _bulkOperationTimeoutMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Prah pre performance warning (ms)
        /// </summary>
        public int PerformanceWarningThresholdMs
        {
            get => _performanceWarningThresholdMs;
            set
            {
                if (value != _performanceWarningThresholdMs && value > 0)
                {
                    _performanceWarningThresholdMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Zapína/vypína parallel bulk processing
        /// </summary>
        public bool EnableParallelBulkProcessing
        {
            get => _enableParallelBulkProcessing;
            set
            {
                if (value != _enableParallelBulkProcessing)
                {
                    _enableParallelBulkProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Prah pre parallel bulk processing (počet operácií)
        /// </summary>
        public int ParallelBulkThreshold
        {
            get => _parallelBulkThreshold;
            set
            {
                if (value != _parallelBulkThreshold && value > 0)
                {
                    _parallelBulkThreshold = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximálny počet parallel bulk operations
        /// </summary>
        public int MaxParallelBulkOperations
        {
            get => _maxParallelBulkOperations;
            set
            {
                if (value != _maxParallelBulkOperations && value > 0 && value <= 16)
                {
                    _maxParallelBulkOperations = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Úroveň data binding optimization performance
        /// </summary>
        public DataBindingOptimizationLevel PerformanceLevel
        {
            get => _performanceLevel;
            set
            {
                if (value != _performanceLevel)
                {
                    _performanceLevel = value;
                    ApplyPerformanceLevel(value);
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Static Configurations

        /// <summary>
        /// Predvolená konfigurácia
        /// </summary>
        public static DataBindingOptimizationConfiguration Default => new()
        {
            EnableChangeTracking = true,
            EnablePropertyThrottling = true,
            EnableBulkOperations = true,
            ChangeProcessingIntervalMs = 100,
            BulkProcessingIntervalMs = 500,
            ThrottleIntervalMs = 50,
            DefaultThrottleMs = 200,
            MaxChangesPerBatch = 100,
            BulkOperationTimeoutMs = 30000,
            PerformanceWarningThresholdMs = 1000,
            EnableParallelBulkProcessing = true,
            ParallelBulkThreshold = 50,
            MaxParallelBulkOperations = 4,
            PerformanceLevel = DataBindingOptimizationLevel.Optimized
        };

        /// <summary>
        /// Základná konfigurácia - minimálne funkcie
        /// </summary>
        public static DataBindingOptimizationConfiguration Basic => new()
        {
            EnableChangeTracking = true,
            EnablePropertyThrottling = false,
            EnableBulkOperations = false,
            ChangeProcessingIntervalMs = 200,
            BulkProcessingIntervalMs = 1000,
            ThrottleIntervalMs = 100,
            DefaultThrottleMs = 500,
            MaxChangesPerBatch = 50,
            BulkOperationTimeoutMs = 15000,
            PerformanceWarningThresholdMs = 2000,
            EnableParallelBulkProcessing = false,
            ParallelBulkThreshold = 100,
            MaxParallelBulkOperations = 2,
            PerformanceLevel = DataBindingOptimizationLevel.Basic
        };

        /// <summary>
        /// Pokročilá konfigurácia - všetky funkcie aktívne
        /// </summary>
        public static DataBindingOptimizationConfiguration Advanced => new()
        {
            EnableChangeTracking = true,
            EnablePropertyThrottling = true,
            EnableBulkOperations = true,
            ChangeProcessingIntervalMs = 50,
            BulkProcessingIntervalMs = 250,
            ThrottleIntervalMs = 25,
            DefaultThrottleMs = 100,
            MaxChangesPerBatch = 200,
            BulkOperationTimeoutMs = 60000,
            PerformanceWarningThresholdMs = 500,
            EnableParallelBulkProcessing = true,
            ParallelBulkThreshold = 25,
            MaxParallelBulkOperations = 8,
            PerformanceLevel = DataBindingOptimizationLevel.Advanced
        };

        /// <summary>
        /// High performance konfigurácia - maximálny výkon
        /// </summary>
        public static DataBindingOptimizationConfiguration HighPerformance => new()
        {
            EnableChangeTracking = true,
            EnablePropertyThrottling = true,
            EnableBulkOperations = true,
            ChangeProcessingIntervalMs = 25,
            BulkProcessingIntervalMs = 100,
            ThrottleIntervalMs = 10,
            DefaultThrottleMs = 50,
            MaxChangesPerBatch = 500,
            BulkOperationTimeoutMs = 120000,
            PerformanceWarningThresholdMs = 250,
            EnableParallelBulkProcessing = true,
            ParallelBulkThreshold = 10,
            MaxParallelBulkOperations = 16,
            PerformanceLevel = DataBindingOptimizationLevel.HighPerformance
        };

        #endregion

        #region Methods

        /// <summary>
        /// ✅ NOVÉ: Aplikuje nastavenia pre danú performance level
        /// </summary>
        private void ApplyPerformanceLevel(DataBindingOptimizationLevel level)
        {
            switch (level)
            {
                case DataBindingOptimizationLevel.Basic:
                    EnablePropertyThrottling = false;
                    EnableBulkOperations = false;
                    ChangeProcessingIntervalMs = Math.Max(ChangeProcessingIntervalMs, 200);
                    DefaultThrottleMs = Math.Max(DefaultThrottleMs, 500);
                    MaxChangesPerBatch = Math.Min(MaxChangesPerBatch, 50);
                    break;

                case DataBindingOptimizationLevel.Optimized:
                    EnablePropertyThrottling = true;
                    EnableBulkOperations = true;
                    break;

                case DataBindingOptimizationLevel.Advanced:
                    EnablePropertyThrottling = true;
                    EnableBulkOperations = true;
                    EnableParallelBulkProcessing = true;
                    MaxParallelBulkOperations = Math.Min(MaxParallelBulkOperations, 8);
                    break;

                case DataBindingOptimizationLevel.HighPerformance:
                    EnablePropertyThrottling = true;
                    EnableBulkOperations = true;
                    EnableParallelBulkProcessing = true;
                    // Žiadne obmedzenia pre high performance
                    break;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu pre UI responsiveness
        /// </summary>
        public void OptimizeForUIResponsiveness()
        {
            switch (PerformanceLevel)
            {
                case DataBindingOptimizationLevel.Basic:
                    DefaultThrottleMs = 300;
                    ChangeProcessingIntervalMs = 150;
                    break;

                case DataBindingOptimizationLevel.Optimized:
                    DefaultThrottleMs = 200;
                    ChangeProcessingIntervalMs = 100;
                    break;

                case DataBindingOptimizationLevel.Advanced:
                    DefaultThrottleMs = 100;
                    ChangeProcessingIntervalMs = 50;
                    break;

                case DataBindingOptimizationLevel.HighPerformance:
                    DefaultThrottleMs = 50;
                    ChangeProcessingIntervalMs = 25;
                    break;
            }

            OnPropertyChanged(nameof(DefaultThrottleMs));
            OnPropertyChanged(nameof(ChangeProcessingIntervalMs));
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu pre throughput
        /// </summary>
        public void OptimizeForThroughput()
        {
            switch (PerformanceLevel)
            {
                case DataBindingOptimizationLevel.Basic:
                    MaxChangesPerBatch = 25;
                    BulkProcessingIntervalMs = 1000;
                    break;

                case DataBindingOptimizationLevel.Optimized:
                    MaxChangesPerBatch = 100;
                    BulkProcessingIntervalMs = 500;
                    break;

                case DataBindingOptimizationLevel.Advanced:
                    MaxChangesPerBatch = 200;
                    BulkProcessingIntervalMs = 250;
                    EnableParallelBulkProcessing = true;
                    break;

                case DataBindingOptimizationLevel.HighPerformance:
                    MaxChangesPerBatch = 500;
                    BulkProcessingIntervalMs = 100;
                    EnableParallelBulkProcessing = true;
                    MaxParallelBulkOperations = Environment.ProcessorCount;
                    break;
            }

            OnPropertyChanged(nameof(MaxChangesPerBatch));
            OnPropertyChanged(nameof(BulkProcessingIntervalMs));
            OnPropertyChanged(nameof(MaxParallelBulkOperations));
        }

        /// <summary>
        /// ✅ NOVÉ: Odhaduje performance impact konfigurácie
        /// </summary>
        public DataBindingPerformanceEstimate EstimatePerformance()
        {
            var processingOverhead = CalculateProcessingOverhead();
            var memoryUsage = CalculateMemoryUsage();
            var throughputLevel = CalculateThroughputLevel();

            return new DataBindingPerformanceEstimate
            {
                ProcessingOverhead = processingOverhead,
                MemoryUsage = memoryUsage,
                ExpectedThroughput = throughputLevel,
                OverallRating = (DataBindingPerformanceLevel)Math.Max((int)processingOverhead, Math.Max((int)memoryUsage, (int)throughputLevel))
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Calculate processing overhead level
        /// </summary>
        private DataBindingPerformanceLevel CalculateProcessingOverhead()
        {
            var score = 0;

            if (EnableChangeTracking) score += 1;
            if (EnablePropertyThrottling) score += 1;
            if (EnableBulkOperations) score += 1;
            if (EnableParallelBulkProcessing) score += 1;

            // Lower intervals = higher overhead
            if (ChangeProcessingIntervalMs < 50) score += 2;
            else if (ChangeProcessingIntervalMs < 100) score += 1;

            if (ThrottleIntervalMs < 25) score += 2;
            else if (ThrottleIntervalMs < 50) score += 1;

            return score switch
            {
                <= 2 => DataBindingPerformanceLevel.Low,
                <= 4 => DataBindingPerformanceLevel.Medium,
                <= 6 => DataBindingPerformanceLevel.High,
                _ => DataBindingPerformanceLevel.VeryHigh
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Calculate memory usage level
        /// </summary>
        private DataBindingPerformanceLevel CalculateMemoryUsage()
        {
            var score = 0;

            if (EnableChangeTracking) score += 2; // Stores property snapshots
            if (EnablePropertyThrottling) score += 1; // Throttle info storage
            if (EnableBulkOperations) score += 1; // Batch storage

            if (MaxChangesPerBatch > 200) score += 2;
            else if (MaxChangesPerBatch > 100) score += 1;

            if (MaxParallelBulkOperations > 8) score += 1;

            return score switch
            {
                <= 2 => DataBindingPerformanceLevel.Low,
                <= 4 => DataBindingPerformanceLevel.Medium,
                <= 6 => DataBindingPerformanceLevel.High,
                _ => DataBindingPerformanceLevel.VeryHigh
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Calculate throughput level
        /// </summary>
        private DataBindingPerformanceLevel CalculateThroughputLevel()
        {
            var score = 0;

            if (EnableBulkOperations) score += 2;
            if (EnableParallelBulkProcessing) score += 2;
            if (EnablePropertyThrottling) score += 1; // Reduces redundant processing

            // Higher batch size = higher throughput
            if (MaxChangesPerBatch > 200) score += 2;
            else if (MaxChangesPerBatch > 100) score += 1;

            // More parallel operations = higher throughput
            if (MaxParallelBulkOperations > 8) score += 2;
            else if (MaxParallelBulkOperations > 4) score += 1;

            return score switch
            {
                <= 2 => DataBindingPerformanceLevel.Low,
                <= 4 => DataBindingPerformanceLevel.Medium,
                <= 7 => DataBindingPerformanceLevel.High,
                _ => DataBindingPerformanceLevel.VeryHigh
            };
        }

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (ChangeProcessingIntervalMs <= 0)
                throw new InvalidOperationException($"ChangeProcessingIntervalMs must be positive, got: {ChangeProcessingIntervalMs}");

            if (BulkProcessingIntervalMs <= 0)
                throw new InvalidOperationException($"BulkProcessingIntervalMs must be positive, got: {BulkProcessingIntervalMs}");

            if (ThrottleIntervalMs <= 0)
                throw new InvalidOperationException($"ThrottleIntervalMs must be positive, got: {ThrottleIntervalMs}");

            if (DefaultThrottleMs < 0)
                throw new InvalidOperationException($"DefaultThrottleMs must be non-negative, got: {DefaultThrottleMs}");

            if (MaxChangesPerBatch <= 0)
                throw new InvalidOperationException($"MaxChangesPerBatch must be positive, got: {MaxChangesPerBatch}");

            if (BulkOperationTimeoutMs <= 0)
                throw new InvalidOperationException($"BulkOperationTimeoutMs must be positive, got: {BulkOperationTimeoutMs}");

            if (PerformanceWarningThresholdMs <= 0)
                throw new InvalidOperationException($"PerformanceWarningThresholdMs must be positive, got: {PerformanceWarningThresholdMs}");

            if (ParallelBulkThreshold <= 0)
                throw new InvalidOperationException($"ParallelBulkThreshold must be positive, got: {ParallelBulkThreshold}");

            if (MaxParallelBulkOperations <= 0 || MaxParallelBulkOperations > 16)
                throw new InvalidOperationException($"MaxParallelBulkOperations must be between 1 and 16, got: {MaxParallelBulkOperations}");
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public DataBindingOptimizationConfiguration Clone()
        {
            return new DataBindingOptimizationConfiguration
            {
                EnableChangeTracking = EnableChangeTracking,
                EnablePropertyThrottling = EnablePropertyThrottling,
                EnableBulkOperations = EnableBulkOperations,
                ChangeProcessingIntervalMs = ChangeProcessingIntervalMs,
                BulkProcessingIntervalMs = BulkProcessingIntervalMs,
                ThrottleIntervalMs = ThrottleIntervalMs,
                DefaultThrottleMs = DefaultThrottleMs,
                MaxChangesPerBatch = MaxChangesPerBatch,
                BulkOperationTimeoutMs = BulkOperationTimeoutMs,
                PerformanceWarningThresholdMs = PerformanceWarningThresholdMs,
                EnableParallelBulkProcessing = EnableParallelBulkProcessing,
                ParallelBulkThreshold = ParallelBulkThreshold,
                MaxParallelBulkOperations = MaxParallelBulkOperations,
                PerformanceLevel = PerformanceLevel
            };
        }

        object ICloneable.Clone() => Clone();

        public override string ToString()
        {
            return $"DataBindingOptimization: ChangeTracking={EnableChangeTracking}, " +
                   $"Throttling={EnablePropertyThrottling}, BulkOps={EnableBulkOperations}, " +
                   $"BatchSize={MaxChangesPerBatch}, Level={PerformanceLevel}";
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÉ: Úrovne data binding optimization výkonu
    /// </summary>
    internal enum DataBindingOptimizationLevel
    {
        /// <summary>
        /// Základná úroveň - len change tracking
        /// </summary>
        Basic = 0,

        /// <summary>
        /// Optimalizovaná úroveň - change tracking + throttling + bulk ops
        /// </summary>
        Optimized = 1,

        /// <summary>
        /// Pokročilá úroveň - všetky funkcie + parallel processing
        /// </summary>
        Advanced = 2,

        /// <summary>
        /// Maximálny výkon - všetky funkcie s najagresívnejšími nastaveniami
        /// </summary>
        HighPerformance = 3
    }

    /// <summary>
    /// ✅ NOVÉ: Performance levels pre odhad
    /// </summary>
    internal enum DataBindingPerformanceLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        VeryHigh = 3
    }

    /// <summary>
    /// ✅ NOVÉ: Odhad performance impact pre data binding
    /// </summary>
    internal class DataBindingPerformanceEstimate
    {
        public DataBindingPerformanceLevel ProcessingOverhead { get; set; }
        public DataBindingPerformanceLevel MemoryUsage { get; set; }
        public DataBindingPerformanceLevel ExpectedThroughput { get; set; }
        public DataBindingPerformanceLevel OverallRating { get; set; }

        public override string ToString()
        {
            return $"Processing: {ProcessingOverhead}, Memory: {MemoryUsage}, " +
                   $"Throughput: {ExpectedThroughput}, Overall: {OverallRating}";
        }
    }
}