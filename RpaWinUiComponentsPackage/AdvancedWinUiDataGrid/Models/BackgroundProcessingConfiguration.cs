// Models/BackgroundProcessingConfiguration.cs - ✅ NOVÝ: Background Processing Configuration
using System;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Konfigurácia pre background processing operations - INTERNAL
    /// </summary>
    internal class BackgroundProcessingConfiguration
    {
        #region Properties

        /// <summary>
        /// Povolenie async data loading
        /// </summary>
        public bool EnableAsyncDataLoading { get; set; } = true;

        /// <summary>
        /// Timeout pre async operácie (ms)
        /// </summary>
        public int AsyncOperationTimeout { get; set; } = 30000; // 30 sekúnd

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
        /// Povolenie cancellation support
        /// </summary>
        public bool EnableCancellation { get; set; } = true;

        /// <summary>
        /// Povolenie parallel data loading
        /// </summary>
        public bool EnableParallelLoading { get; set; } = false;

        /// <summary>
        /// Maximálny počet concurrent tasks
        /// </summary>
        public int MaxConcurrentTasks { get; set; } = Environment.ProcessorCount;

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

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia pre malé datasety
        /// </summary>
        public static BackgroundProcessingConfiguration Basic => new()
        {
            EnableAsyncDataLoading = true,
            AsyncOperationTimeout = 15000,
            DataLoadingBatchSize = 500,
            ProgressReportingInterval = 1000,
            ShowProgressBar = true,
            EnableCancellation = true,
            EnableParallelLoading = false,
            EnableDataStreaming = false
        };

        /// <summary>
        /// Optimalizovaná konfigurácia pre stredné datasety
        /// </summary>
        public static BackgroundProcessingConfiguration Optimized => new()
        {
            EnableAsyncDataLoading = true,
            AsyncOperationTimeout = 30000,
            DataLoadingBatchSize = 1000,
            ProgressReportingInterval = 500,
            ShowProgressBar = true,
            EnableCancellation = true,
            EnableParallelLoading = true,
            MaxConcurrentTasks = Math.Max(2, Environment.ProcessorCount / 2),
            EnableDataStreaming = false
        };

        /// <summary>
        /// High-performance konfigurácia pre veľké datasety
        /// </summary>
        public static BackgroundProcessingConfiguration HighPerformance => new()
        {
            EnableAsyncDataLoading = true,
            AsyncOperationTimeout = 60000,
            DataLoadingBatchSize = 2000,
            ProgressReportingInterval = 250,
            ShowProgressBar = true,
            EnableCancellation = true,
            EnableParallelLoading = true,
            MaxConcurrentTasks = Environment.ProcessorCount,
            EnableDataStreaming = true,
            StreamingThreshold = 5000,
            BackgroundThreadPriority = System.Threading.ThreadPriority.Normal
        };

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (AsyncOperationTimeout < 5000) AsyncOperationTimeout = 5000;
            if (DataLoadingBatchSize < 100) DataLoadingBatchSize = 100;
            if (ProgressReportingInterval < 100) ProgressReportingInterval = 100;
            if (MaxConcurrentTasks < 1) MaxConcurrentTasks = 1;
            if (MaxConcurrentTasks > Environment.ProcessorCount * 2) 
                MaxConcurrentTasks = Environment.ProcessorCount * 2;
            if (StreamingThreshold < 1000) StreamingThreshold = 1000;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public BackgroundProcessingConfiguration Clone()
        {
            return new BackgroundProcessingConfiguration
            {
                EnableAsyncDataLoading = EnableAsyncDataLoading,
                AsyncOperationTimeout = AsyncOperationTimeout,
                DataLoadingBatchSize = DataLoadingBatchSize,
                ProgressReportingInterval = ProgressReportingInterval,
                ShowProgressBar = ShowProgressBar,
                EnableCancellation = EnableCancellation,
                EnableParallelLoading = EnableParallelLoading,
                MaxConcurrentTasks = MaxConcurrentTasks,
                EnableDataStreaming = EnableDataStreaming,
                StreamingThreshold = StreamingThreshold,
                BackgroundThreadPriority = BackgroundThreadPriority
            };
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
}