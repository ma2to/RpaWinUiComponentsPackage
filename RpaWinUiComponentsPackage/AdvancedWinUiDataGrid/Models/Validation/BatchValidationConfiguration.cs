// Models/Validation/BatchValidationConfiguration.cs - ✅ NOVÉ: Batch Validation Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre batch validation engine
    /// </summary>
    public class BatchValidationConfiguration
    {
        /// <summary>
        /// Či je batch validation povolené
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Maximálny počet riadkov pre parallel processing
        /// </summary>
        public int MaxParallelRows { get; set; } = Environment.ProcessorCount * 50;

        /// <summary>
        /// Batch size pre processing (koľko riadkov spracovať v jednom batch-i)
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Timeout pre jeden batch (ms)
        /// </summary>
        public int BatchTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Či sa má reportovať progress
        /// </summary>
        public bool EnableProgressReporting { get; set; } = true;

        /// <summary>
        /// Interval pre progress reporting (ms)
        /// </summary>
        public int ProgressReportingIntervalMs { get; set; } = 100;

        /// <summary>
        /// Maximálny počet concurrent tasks
        /// </summary>
        public int MaxConcurrency { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Či sa má použiť cancellation token
        /// </summary>
        public bool EnableCancellation { get; set; } = true;

        /// <summary>
        /// Priorita validácie (Normal, High, Background)
        /// </summary>
        public ValidationPriority Priority { get; set; } = ValidationPriority.Normal;

        /// <summary>
        /// Či sa má použiť memory optimization pre veľké datasety
        /// </summary>
        public bool EnableMemoryOptimization { get; set; } = true;

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (BatchSize <= 0)
                throw new ArgumentException("BatchSize musí byť > 0");

            if (MaxParallelRows <= 0)
                throw new ArgumentException("MaxParallelRows musí byť > 0");

            if (BatchTimeoutMs <= 0)
                throw new ArgumentException("BatchTimeoutMs musí byť > 0");

            if (ProgressReportingIntervalMs <= 0)
                throw new ArgumentException("ProgressReportingIntervalMs musí byť > 0");

            if (MaxConcurrency <= 0)
                throw new ArgumentException("MaxConcurrency musí byť > 0");

            if (MaxConcurrency > Environment.ProcessorCount * 2)
                throw new ArgumentException($"MaxConcurrency nemôže byť väčšia ako {Environment.ProcessorCount * 2}");
        }

        /// <summary>
        /// Vytvorí default konfiguráciu
        /// </summary>
        public static BatchValidationConfiguration Default => new()
        {
            IsEnabled = true,
            MaxParallelRows = Environment.ProcessorCount * 50,
            BatchSize = 100,
            BatchTimeoutMs = 5000,
            EnableProgressReporting = true,
            ProgressReportingIntervalMs = 100,
            MaxConcurrency = Environment.ProcessorCount,
            EnableCancellation = true,
            Priority = ValidationPriority.Normal,
            EnableMemoryOptimization = true
        };

        /// <summary>
        /// Vytvorí high performance konfiguráciu
        /// </summary>
        public static BatchValidationConfiguration HighPerformance => new()
        {
            IsEnabled = true,
            MaxParallelRows = Environment.ProcessorCount * 100,
            BatchSize = 200,
            BatchTimeoutMs = 10000,
            EnableProgressReporting = false, // Vypnuté pre max performance
            ProgressReportingIntervalMs = 500,
            MaxConcurrency = Environment.ProcessorCount * 2,
            EnableCancellation = false, // Vypnuté pre max performance
            Priority = ValidationPriority.High,
            EnableMemoryOptimization = true
        };

        /// <summary>
        /// Vytvorí background konfiguráciu
        /// </summary>
        public static BatchValidationConfiguration Background => new()
        {
            IsEnabled = true,
            MaxParallelRows = Environment.ProcessorCount * 25,
            BatchSize = 50,
            BatchTimeoutMs = 15000,
            EnableProgressReporting = true,
            ProgressReportingIntervalMs = 200,
            MaxConcurrency = Math.Max(1, Environment.ProcessorCount / 2),
            EnableCancellation = true,
            Priority = ValidationPriority.Background,
            EnableMemoryOptimization = true
        };

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public BatchValidationConfiguration Clone()
        {
            return new BatchValidationConfiguration
            {
                IsEnabled = IsEnabled,
                MaxParallelRows = MaxParallelRows,
                BatchSize = BatchSize,
                BatchTimeoutMs = BatchTimeoutMs,
                EnableProgressReporting = EnableProgressReporting,
                ProgressReportingIntervalMs = ProgressReportingIntervalMs,
                MaxConcurrency = MaxConcurrency,
                EnableCancellation = EnableCancellation,
                Priority = Priority,
                EnableMemoryOptimization = EnableMemoryOptimization
            };
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Priorita batch validation
    /// </summary>
    public enum ValidationPriority
    {
        Background = 0,
        Normal = 1,
        High = 2
    }

    /// <summary>
    /// ✅ NOVÉ: Progress info pre batch validation
    /// </summary>
    public class BatchValidationProgress
    {
        /// <summary>
        /// Celkový počet riadkov na validáciu
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Počet spracovaných riadkov
        /// </summary>
        public int ProcessedRows { get; set; }

        /// <summary>
        /// Počet validných riadkov
        /// </summary>
        public int ValidRows { get; set; }

        /// <summary>
        /// Počet invalidných riadkov
        /// </summary>
        public int InvalidRows { get; set; }

        /// <summary>
        /// Percento dokončenia (0-100)
        /// </summary>
        public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;

        /// <summary>
        /// Aktuálny batch index
        /// </summary>
        public int CurrentBatch { get; set; }

        /// <summary>
        /// Celkový počet batch-ov
        /// </summary>
        public int TotalBatches { get; set; }

        /// <summary>
        /// Čas začiatku validácie
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Odhadovaný čas dokončenia
        /// </summary>
        public DateTime? EstimatedCompletion { get; set; }

        /// <summary>
        /// Či je validácia dokončená
        /// </summary>
        public bool IsCompleted => ProcessedRows >= TotalRows;

        /// <summary>
        /// Rýchlosť spracovania (riadky/sekundu)
        /// </summary>
        public double ProcessingRate
        {
            get
            {
                var elapsed = DateTime.Now - StartTime;
                return elapsed.TotalSeconds > 0 ? ProcessedRows / elapsed.TotalSeconds : 0;
            }
        }

        public override string ToString()
        {
            return $"Batch Validation: {ProcessedRows}/{TotalRows} rows ({PercentComplete:F1}%) - " +
                   $"Valid: {ValidRows}, Invalid: {InvalidRows}, Rate: {ProcessingRate:F1} rows/sec";
        }
    }
}