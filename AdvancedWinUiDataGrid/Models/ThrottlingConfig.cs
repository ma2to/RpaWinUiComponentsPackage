// Models/ThrottlingConfig.cs
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Konfigurácia pre throttling operácií v DataGrid
    /// </summary>
    public class ThrottlingConfig
    {
        /// <summary>
        /// Debounce delay pre validácie v ms (default 300ms)
        /// </summary>
        public int ValidationDebounceMs { get; set; } = 300;

        /// <summary>
        /// Debounce delay pre UI updates v ms (default 100ms)
        /// </summary>
        public int UIUpdateDebounceMs { get; set; } = 100;

        /// <summary>
        /// Debounce delay pre search/filter operácie v ms (default 500ms)
        /// </summary>
        public int SearchDebounceMs { get; set; } = 500;

        /// <summary>
        /// Batch size pre bulk operácie (default 50)
        /// </summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>
        /// Maximálny počet batch operácií za sekundu (default 10)
        /// </summary>
        public int MaxBatchesPerSecond { get; set; } = 10;

        /// <summary>
        /// Či je povolený throttling pre validácie
        /// </summary>
        public bool EnableValidationThrottling { get; set; } = true;

        /// <summary>
        /// Či je povolený throttling pre UI updates
        /// </summary>
        public bool EnableUIThrottling { get; set; } = true;

        /// <summary>
        /// Či je povolený throttling pre search operácie
        /// </summary>
        public bool EnableSearchThrottling { get; set; } = true;

        /// <summary>
        /// Timeout pre async operácie v ms (default 5000ms)
        /// </summary>
        public int AsyncOperationTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Či sa majú throttle operácie v background thread
        /// </summary>
        public bool UseBackgroundProcessing { get; set; } = true;

        /// <summary>
        /// Priorita thread pre background operácie
        /// </summary>
        public System.Threading.ThreadPriority BackgroundThreadPriority { get; set; } = System.Threading.ThreadPriority.BelowNormal;

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static ThrottlingConfig Default => new ThrottlingConfig();

        /// <summary>
        /// Konfigurácia pre rýchle operácie (menšie delays)
        /// </summary>
        public static ThrottlingConfig Fast => new ThrottlingConfig
        {
            ValidationDebounceMs = 150,
            UIUpdateDebounceMs = 50,
            SearchDebounceMs = 250,
            BatchSize = 100,
            MaxBatchesPerSecond = 20
        };

        /// <summary>
        /// Konfigurácia pre pomalé operácie (väčšie delays)
        /// </summary>
        public static ThrottlingConfig Slow => new ThrottlingConfig
        {
            ValidationDebounceMs = 500,
            UIUpdateDebounceMs = 200,
            SearchDebounceMs = 800,
            BatchSize = 25,
            MaxBatchesPerSecond = 5
        };

        /// <summary>
        /// Konfigurácia pre performance kritické aplikácie
        /// </summary>
        public static ThrottlingConfig PerformanceCritical => new ThrottlingConfig
        {
            ValidationDebounceMs = 100,
            UIUpdateDebounceMs = 30,
            SearchDebounceMs = 200,
            BatchSize = 200,
            MaxBatchesPerSecond = 50,
            UseBackgroundProcessing = true,
            BackgroundThreadPriority = System.Threading.ThreadPriority.Normal
        };

        /// <summary>
        /// Konfigurácia bez throttling (immediate operácie)
        /// </summary>
        public static ThrottlingConfig NoThrottling => new ThrottlingConfig
        {
            ValidationDebounceMs = 0,
            UIUpdateDebounceMs = 0,
            SearchDebounceMs = 0,
            EnableValidationThrottling = false,
            EnableUIThrottling = false,
            EnableSearchThrottling = false,
            BatchSize = 1000,
            MaxBatchesPerSecond = 1000
        };

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (ValidationDebounceMs < 0)
                throw new ArgumentException("ValidationDebounceMs nemôže byť záporný");

            if (UIUpdateDebounceMs < 0)
                throw new ArgumentException("UIUpdateDebounceMs nemôže byť záporný");

            if (SearchDebounceMs < 0)
                throw new ArgumentException("SearchDebounceMs nemôže byť záporný");

            if (BatchSize <= 0)
                throw new ArgumentException("BatchSize musí byť väčší ako 0");

            if (MaxBatchesPerSecond <= 0)
                throw new ArgumentException("MaxBatchesPerSecond musí byť väčší ako 0");

            if (AsyncOperationTimeoutMs <= 0)
                throw new ArgumentException("AsyncOperationTimeoutMs musí byť väčší ako 0");
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public ThrottlingConfig Clone()
        {
            return new ThrottlingConfig
            {
                ValidationDebounceMs = ValidationDebounceMs,
                UIUpdateDebounceMs = UIUpdateDebounceMs,
                SearchDebounceMs = SearchDebounceMs,
                BatchSize = BatchSize,
                MaxBatchesPerSecond = MaxBatchesPerSecond,
                EnableValidationThrottling = EnableValidationThrottling,
                EnableUIThrottling = EnableUIThrottling,
                EnableSearchThrottling = EnableSearchThrottling,
                AsyncOperationTimeoutMs = AsyncOperationTimeoutMs,
                UseBackgroundProcessing = UseBackgroundProcessing,
                BackgroundThreadPriority = BackgroundThreadPriority
            };
        }

        /// <summary>
        /// Kombinuje túto konfiguráciu s inou (táto má prioritu)
        /// </summary>
        public ThrottlingConfig MergeWith(ThrottlingConfig other)
        {
            if (other == null) return Clone();

            var merged = Clone();

            // Použije menšie delays (rýchlejšie operácie)
            merged.ValidationDebounceMs = Math.Min(ValidationDebounceMs, other.ValidationDebounceMs);
            merged.UIUpdateDebounceMs = Math.Min(UIUpdateDebounceMs, other.UIUpdateDebounceMs);
            merged.SearchDebounceMs = Math.Min(SearchDebounceMs, other.SearchDebounceMs);

            // Použije väčší batch size (efektívnejšie operácie)
            merged.BatchSize = Math.Max(BatchSize, other.BatchSize);
            merged.MaxBatchesPerSecond = Math.Max(MaxBatchesPerSecond, other.MaxBatchesPerSecond);

            return merged;
        }

        public override string ToString()
        {
            return $"ThrottlingConfig(Validation:{ValidationDebounceMs}ms, UI:{UIUpdateDebounceMs}ms, Search:{SearchDebounceMs}ms, Batch:{BatchSize})";
        }
    }
}