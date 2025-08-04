// Models/BackgroundProcessingConfiguration.cs - ✅ ENHANCED: Background Processing Configuration
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ ENHANCED: Konfigurácia pre background processing operations - INTERNAL
    /// </summary>
    internal class BackgroundProcessingConfiguration : INotifyPropertyChanged, ICloneable
    {
        #region Private Fields

        private int _maxConcurrentTasks = Environment.ProcessorCount;
        private int _maxQueueSize = 1000;
        private int _defaultTaskTimeoutMs = 30000;
        private int _performanceWarningThresholdMs = 5000;
        private bool _enableTaskPrioritization = true;
        private bool _enableTaskCancellation = true;
        private bool _enablePerformanceMonitoring = true;
        private BackgroundProcessingLevel _performanceLevel = BackgroundProcessingLevel.Optimized;

        #endregion

        #region Original Properties (pre backward compatibility)

        /// <summary>
        /// Povolenie async data loading
        /// </summary>
        public bool EnableAsyncDataLoading { get; set; } = true;

        /// <summary>
        /// Timeout pre async operácie (ms) - alias pre DefaultTaskTimeoutMs
        /// </summary>
        public int AsyncOperationTimeout 
        { 
            get => DefaultTaskTimeoutMs; 
            set => DefaultTaskTimeoutMs = value; 
        }

        /// <summary>
        /// Veľkosť batch-u pre data loading
        /// </summary>
        public int DataLoadingBatchSize { get; set; } = 1000;

        /// <summary>
        /// Interval pre progress reporting (ms)
        /// </summary>
        public int ProgressReportingInterval { get; set; } = 500;

        /// <summary>
        /// Povolenie progress bar zobrazenia
        /// </summary>
        public bool ShowProgressBar { get; set; } = true;

        /// <summary>
        /// Povolenie cancellation support - alias pre EnableTaskCancellation
        /// </summary>
        public bool EnableCancellation 
        { 
            get => EnableTaskCancellation; 
            set => EnableTaskCancellation = value; 
        }

        /// <summary>
        /// Povolenie parallel data loading
        /// </summary>
        public bool EnableParallelLoading { get; set; } = false;

        /// <summary>
        /// Povolenie data streaming pre veľké datasety
        /// </summary>
        public bool EnableDataStreaming { get; set; } = false;

        /// <summary>
        /// Prahová hodnota pre streaming (počet riadkov)
        /// </summary>
        public int StreamingThreshold { get; set; } = 10000;

        /// <summary>
        /// Priorita background thread-ov
        /// </summary>
        public System.Threading.ThreadPriority BackgroundThreadPriority { get; set; } = 
            System.Threading.ThreadPriority.BelowNormal;

        #endregion

        #region New Enhanced Properties

        /// <summary>
        /// ✅ NOVÉ: Maximálny počet súbežne spracovaných taskov
        /// </summary>
        public int MaxConcurrentTasks
        {
            get => _maxConcurrentTasks;
            set
            {
                if (value != _maxConcurrentTasks && value > 0 && value <= 16)
                {
                    _maxConcurrentTasks = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Maximálna veľkosť queue pre background tasky
        /// </summary>
        public int MaxQueueSize
        {
            get => _maxQueueSize;
            set
            {
                if (value != _maxQueueSize && value > 0)
                {
                    _maxQueueSize = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Predvolený timeout pre background tasky (ms)
        /// </summary>
        public int DefaultTaskTimeoutMs
        {
            get => _defaultTaskTimeoutMs;
            set
            {
                if (value != _defaultTaskTimeoutMs && value > 0)
                {
                    _defaultTaskTimeoutMs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Prah pre performance warning (ms)
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
        /// ✅ NOVÉ: Zapína/vypína task prioritization
        /// </summary>
        public bool EnableTaskPrioritization
        {
            get => _enableTaskPrioritization;
            set
            {
                if (value != _enableTaskPrioritization)
                {
                    _enableTaskPrioritization = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Zapína/vypína task cancellation
        /// </summary>
        public bool EnableTaskCancellation
        {
            get => _enableTaskCancellation;
            set
            {
                if (value != _enableTaskCancellation)
                {
                    _enableTaskCancellation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Zapína/vypína performance monitoring
        /// </summary>
        public bool EnablePerformanceMonitoring
        {
            get => _enablePerformanceMonitoring;
            set
            {
                if (value != _enablePerformanceMonitoring)
                {
                    _enablePerformanceMonitoring = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Úroveň background processing performance
        /// </summary>
        public BackgroundProcessingLevel PerformanceLevel
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
        /// ✅ ENHANCED: Predvolená konfigurácia
        /// </summary>
        public static BackgroundProcessingConfiguration Default => new()
        {
            MaxConcurrentTasks = 4,
            MaxQueueSize = 1000,
            DefaultTaskTimeoutMs = 30000,
            PerformanceWarningThresholdMs = 5000,
            EnableTaskPrioritization = true,
            EnableTaskCancellation = true,
            EnablePerformanceMonitoring = true,
            PerformanceLevel = BackgroundProcessingLevel.Optimized,
            // Original properties
            EnableAsyncDataLoading = true,
            DataLoadingBatchSize = 1000,
            ProgressReportingInterval = 500,
            ShowProgressBar = true,
            EnableParallelLoading = true,
            EnableDataStreaming = false
        };

        /// <summary>
        /// ✅ ENHANCED: Základná konfigurácia - minimálne funkcie
        /// </summary>
        public static BackgroundProcessingConfiguration Basic => new()
        {
            MaxConcurrentTasks = 2,
            MaxQueueSize = 100,
            DefaultTaskTimeoutMs = 15000,
            PerformanceWarningThresholdMs = 3000,
            EnableTaskPrioritization = false,
            EnableTaskCancellation = true,
            EnablePerformanceMonitoring = false,
            PerformanceLevel = BackgroundProcessingLevel.Basic,
            // Original properties
            EnableAsyncDataLoading = true,
            DataLoadingBatchSize = 500,
            ProgressReportingInterval = 1000,
            ShowProgressBar = true,
            EnableParallelLoading = false,
            EnableDataStreaming = false
        };

        /// <summary>
        /// ✅ ENHANCED: Optimalizovaná konfigurácia pre stredné datasety
        /// </summary>
        public static BackgroundProcessingConfiguration Optimized => new()
        {
            MaxConcurrentTasks = Math.Max(2, Environment.ProcessorCount / 2),
            MaxQueueSize = 1000,
            DefaultTaskTimeoutMs = 30000,
            PerformanceWarningThresholdMs = 5000,
            EnableTaskPrioritization = true,
            EnableTaskCancellation = true,
            EnablePerformanceMonitoring = true,
            PerformanceLevel = BackgroundProcessingLevel.Optimized,
            // Original properties
            EnableAsyncDataLoading = true,
            DataLoadingBatchSize = 1000,
            ProgressReportingInterval = 500,
            ShowProgressBar = true,
            EnableParallelLoading = true,
            EnableDataStreaming = false
        };

        /// <summary>
        /// ✅ ENHANCED: Pokročilá konfigurácia - všetky funkcie aktívne
        /// </summary>
        public static BackgroundProcessingConfiguration Advanced => new()
        {
            MaxConcurrentTasks = 8,
            MaxQueueSize = 5000,
            DefaultTaskTimeoutMs = 60000,
            PerformanceWarningThresholdMs = 10000,
            EnableTaskPrioritization = true,
            EnableTaskCancellation = true,
            EnablePerformanceMonitoring = true,
            PerformanceLevel = BackgroundProcessingLevel.Advanced,
            // Original properties
            EnableAsyncDataLoading = true,
            DataLoadingBatchSize = 2000,
            ProgressReportingInterval = 250,
            ShowProgressBar = true,
            EnableParallelLoading = true,
            EnableDataStreaming = true,
            StreamingThreshold = 5000,
            BackgroundThreadPriority = System.Threading.ThreadPriority.Normal
        };

        /// <summary>
        /// ✅ NOVÉ: High performance konfigurácia - maximálny výkon
        /// </summary>
        public static BackgroundProcessingConfiguration HighPerformance => new()
        {
            MaxConcurrentTasks = 12,
            MaxQueueSize = 10000,
            DefaultTaskTimeoutMs = 120000,
            PerformanceWarningThresholdMs = 15000,
            EnableTaskPrioritization = true,
            EnableTaskCancellation = true,
            EnablePerformanceMonitoring = true,
            PerformanceLevel = BackgroundProcessingLevel.HighPerformance,
            // Original properties
            EnableAsyncDataLoading = true,
            DataLoadingBatchSize = 5000,
            ProgressReportingInterval = 100,
            ShowProgressBar = true,
            EnableParallelLoading = true,
            EnableDataStreaming = true,
            StreamingThreshold = 2000,
            BackgroundThreadPriority = System.Threading.ThreadPriority.Normal
        };

        #endregion

        #region Methods

        /// <summary>
        /// ✅ ENHANCED: Aplikuje nastavenia pre danú performance level
        /// </summary>
        private void ApplyPerformanceLevel(BackgroundProcessingLevel level)
        {
            switch (level)
            {
                case BackgroundProcessingLevel.Basic:
                    MaxConcurrentTasks = Math.Min(MaxConcurrentTasks, 2);
                    MaxQueueSize = Math.Min(MaxQueueSize, 100);
                    DefaultTaskTimeoutMs = Math.Min(DefaultTaskTimeoutMs, 15000);
                    break;

                case BackgroundProcessingLevel.Optimized:
                    MaxConcurrentTasks = Math.Min(MaxConcurrentTasks, 4);
                    MaxQueueSize = Math.Min(MaxQueueSize, 1000);
                    break;

                case BackgroundProcessingLevel.Advanced:
                    MaxConcurrentTasks = Math.Min(MaxConcurrentTasks, 8);
                    MaxQueueSize = Math.Min(MaxQueueSize, 5000);
                    break;

                case BackgroundProcessingLevel.HighPerformance:
                    // Žiadne obmedzenia pre high performance
                    break;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu pre dostupné CPU cores
        /// </summary>
        public void OptimizeForCpuCores()
        {
            var coreCount = Environment.ProcessorCount;
            
            MaxConcurrentTasks = PerformanceLevel switch
            {
                BackgroundProcessingLevel.Basic => Math.Max(1, Math.Min(2, coreCount / 2)),
                BackgroundProcessingLevel.Optimized => Math.Max(2, Math.Min(4, coreCount)),
                BackgroundProcessingLevel.Advanced => Math.Max(4, Math.Min(8, coreCount * 2)),
                BackgroundProcessingLevel.HighPerformance => Math.Max(6, Math.Min(16, coreCount * 3)),
                _ => 4
            };
            
            OnPropertyChanged(nameof(MaxConcurrentTasks));
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu pre pamäť
        /// </summary>
        public void OptimizeForMemory(long availableMemoryMB)
        {
            if (availableMemoryMB < 512) // Nízka pamäť
            {
                MaxQueueSize = Math.Min(MaxQueueSize, 50);
                MaxConcurrentTasks = Math.Min(MaxConcurrentTasks, 2);
            }
            else if (availableMemoryMB < 2048) // Stredná pamäť
            {
                MaxQueueSize = Math.Min(MaxQueueSize, 500);
                MaxConcurrentTasks = Math.Min(MaxConcurrentTasks, 4);
            }
            // Vysoká pamäť - žiadne obmedzenia
            
            OnPropertyChanged(nameof(MaxQueueSize));
            OnPropertyChanged(nameof(MaxConcurrentTasks));
        }

        /// <summary>
        /// ✅ NOVÉ: Odhaduje performance impact konfigurácie
        /// </summary>
        public BackgroundProcessingPerformanceEstimate EstimatePerformance()
        {
            var cpuUsage = MaxConcurrentTasks switch
            {
                <= 2 => BackgroundProcessingPerformanceLevel.Low,
                <= 4 => BackgroundProcessingPerformanceLevel.Medium,
                <= 8 => BackgroundProcessingPerformanceLevel.High,
                _ => BackgroundProcessingPerformanceLevel.VeryHigh
            };

            var memoryUsage = MaxQueueSize switch
            {
                <= 100 => BackgroundProcessingPerformanceLevel.Low,
                <= 1000 => BackgroundProcessingPerformanceLevel.Medium,
                <= 5000 => BackgroundProcessingPerformanceLevel.High,
                _ => BackgroundProcessingPerformanceLevel.VeryHigh
            };

            var throughput = (MaxConcurrentTasks * MaxQueueSize) switch
            {
                <= 200 => BackgroundProcessingPerformanceLevel.Low,
                <= 4000 => BackgroundProcessingPerformanceLevel.Medium,
                <= 40000 => BackgroundProcessingPerformanceLevel.High,
                _ => BackgroundProcessingPerformanceLevel.VeryHigh
            };

            return new BackgroundProcessingPerformanceEstimate
            {
                CpuUsage = cpuUsage,
                MemoryUsage = memoryUsage,
                ExpectedThroughput = throughput,
                OverallRating = (BackgroundProcessingPerformanceLevel)Math.Max((int)cpuUsage, Math.Max((int)memoryUsage, (int)throughput))
            };
        }

        /// <summary>
        /// ✅ ENHANCED: Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            // New validation
            if (MaxConcurrentTasks <= 0 || MaxConcurrentTasks > 16)
                throw new InvalidOperationException($"MaxConcurrentTasks must be between 1 and 16, got: {MaxConcurrentTasks}");

            if (MaxQueueSize <= 0)
                throw new InvalidOperationException($"MaxQueueSize must be positive, got: {MaxQueueSize}");

            if (DefaultTaskTimeoutMs <= 0)
                throw new InvalidOperationException($"DefaultTaskTimeoutMs must be positive, got: {DefaultTaskTimeoutMs}");

            if (PerformanceWarningThresholdMs <= 0)
                throw new InvalidOperationException($"PerformanceWarningThresholdMs must be positive, got: {PerformanceWarningThresholdMs}");

            // Original validation
            if (DataLoadingBatchSize < 100) DataLoadingBatchSize = 100;
            if (ProgressReportingInterval < 100) ProgressReportingInterval = 100;
            if (StreamingThreshold < 1000) StreamingThreshold = 1000;
        }

        /// <summary>
        /// ✅ ENHANCED: Vytvorí kópiu konfigurácie
        /// </summary>
        public BackgroundProcessingConfiguration Clone()
        {
            return new BackgroundProcessingConfiguration
            {
                // New properties
                MaxConcurrentTasks = MaxConcurrentTasks,
                MaxQueueSize = MaxQueueSize,
                DefaultTaskTimeoutMs = DefaultTaskTimeoutMs,
                PerformanceWarningThresholdMs = PerformanceWarningThresholdMs,
                EnableTaskPrioritization = EnableTaskPrioritization,
                EnableTaskCancellation = EnableTaskCancellation,
                EnablePerformanceMonitoring = EnablePerformanceMonitoring,
                PerformanceLevel = PerformanceLevel,
                // Original properties
                EnableAsyncDataLoading = EnableAsyncDataLoading,
                DataLoadingBatchSize = DataLoadingBatchSize,
                ProgressReportingInterval = ProgressReportingInterval,
                ShowProgressBar = ShowProgressBar,
                EnableParallelLoading = EnableParallelLoading,
                EnableDataStreaming = EnableDataStreaming,
                StreamingThreshold = StreamingThreshold,
                BackgroundThreadPriority = BackgroundThreadPriority
            };
        }

        object ICloneable.Clone() => Clone();

        public override string ToString()
        {
            return $"BackgroundProcessing: Workers={MaxConcurrentTasks}, Queue={MaxQueueSize}, " +
                   $"Timeout={DefaultTaskTimeoutMs}ms, Level={PerformanceLevel}";
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
    /// Progress info pre async operations - INTERNAL
    /// </summary>
    internal class AsyncLoadingProgress
    {
        /// <summary>
        /// Percentage completed (0-100)
        /// </summary>
        public double PercentageCompleted { get; set; }

        /// <summary>
        /// Počet spracovaných položiek
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// Celkový počet položiek
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Aktuálny status message
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp posledného update
        /// </summary>
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Odhadovaný čas dokončenia
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }

        /// <summary>
        /// Či je operácia zrušená
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Či došlo k chybe
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message ak došlo k chybe
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result pre async data loading operations - INTERNAL
    /// </summary>
    internal class AsyncLoadingResult<T>
    {
        /// <summary>
        /// Či bola operácia úspešná
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Načítané dáta
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Počet načítaných položiek
        /// </summary>
        public int LoadedItemsCount { get; set; }

        /// <summary>
        /// Čas trvania operácie
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Error message pri neúspešnej operácii
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Exception ak došlo k chybe
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Či bola operácia zrušená
        /// </summary>
        public bool WasCancelled { get; set; }

        /// <summary>
        /// Metadata o operácii
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Vytvorí úspešný result
        /// </summary>
        public static AsyncLoadingResult<T> Success(T data, int itemsCount, TimeSpan duration)
        {
            return new AsyncLoadingResult<T>
            {
                IsSuccessful = true,
                Data = data,
                LoadedItemsCount = itemsCount,
                Duration = duration
            };
        }

        /// <summary>
        /// Vytvorí error result
        /// </summary>
        public static AsyncLoadingResult<T> Error(string errorMessage, Exception? exception = null)
        {
            return new AsyncLoadingResult<T>
            {
                IsSuccessful = false,
                ErrorMessage = errorMessage,
                Exception = exception
            };
        }

        /// <summary>
        /// Vytvorí cancelled result
        /// </summary>
        public static AsyncLoadingResult<T> Cancelled()
        {
            return new AsyncLoadingResult<T>
            {
                IsSuccessful = false,
                WasCancelled = true,
                ErrorMessage = "Operation was cancelled"
            };
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Úrovne background processing výkonu
    /// </summary>
    internal enum BackgroundProcessingLevel
    {
        /// <summary>
        /// Základná úroveň - minimálne funkcie, nízky výkon
        /// </summary>
        Basic = 0,

        /// <summary>
        /// Optimalizovaná úroveň - vyvážený výkon a spotreba
        /// </summary>
        Optimized = 1,

        /// <summary>
        /// Pokročilá úroveň - vysoký výkon, viac funkcií
        /// </summary>
        Advanced = 2,

        /// <summary>
        /// Maximálny výkon - všetky funkcie aktívne
        /// </summary>
        HighPerformance = 3
    }

    /// <summary>
    /// ✅ NOVÉ: Performance levels pre odhad
    /// </summary>
    internal enum BackgroundProcessingPerformanceLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        VeryHigh = 3
    }

    /// <summary>
    /// ✅ NOVÉ: Odhad performance impact
    /// </summary>
    internal class BackgroundProcessingPerformanceEstimate
    {
        public BackgroundProcessingPerformanceLevel CpuUsage { get; set; }
        public BackgroundProcessingPerformanceLevel MemoryUsage { get; set; }
        public BackgroundProcessingPerformanceLevel ExpectedThroughput { get; set; }
        public BackgroundProcessingPerformanceLevel OverallRating { get; set; }

        public override string ToString()
        {
            return $"CPU: {CpuUsage}, Memory: {MemoryUsage}, Throughput: {ExpectedThroughput}, Overall: {OverallRating}";
        }
    }
}