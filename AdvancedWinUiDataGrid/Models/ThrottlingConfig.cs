// Models/ThrottlingConfig.cs
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Konfigurácia pre throttling a performance optimalizácie DataGrid komponentu.
    /// Obsahuje nastavenia pre debounce časovače, batch operácie a optimalizácie.
    /// </summary>
    public class ThrottlingConfig
    {
        #region Konštruktory

        /// <summary>
        /// Vytvorí novú ThrottlingConfig s predvolenými hodnotami.
        /// </summary>
        public ThrottlingConfig()
        {
            ValidationDebounceMs = 300;
            UIUpdateDebounceMs = 100;
            BatchSize = 50;
            EnableValidationThrottling = true;
            EnableUIThrottling = true;
            MaxConcurrentValidations = 3;
            VirtualizationThreshold = 1000;
            LazyLoadingThreshold = 2000;
        }

        #endregion

        #region Základné throttling nastavenia

        /// <summary>
        /// Debounce timeout pre validácie v milisekundách.
        /// Default: 300ms - optimálne pre používateľa ktorý píše rýchlo.
        /// </summary>
        public int ValidationDebounceMs { get; set; }

        /// <summary>
        /// Debounce timeout pre UI updates v milisekundách.
        /// Default: 100ms - rýchla odozva UI.
        /// </summary>
        public int UIUpdateDebounceMs { get; set; }

        /// <summary>
        /// Povoliť throttling validácií.
        /// Default: true - odporúčané pre výkon.
        /// </summary>
        public bool EnableValidationThrottling { get; set; }

        /// <summary>
        /// Povoliť throttling UI updates.
        /// Default: true - odporúčané pre plynulé UI.
        /// </summary>
        public bool EnableUIThrottling { get; set; }

        #endregion

        #region Batch operácie

        /// <summary>
        /// Počet položiek spracovaných v jednom batch-u.
        /// Default: 50 - vyvážené medzi výkonom a odozvou.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Časový limit pre batch operácie v milisekundách.
        /// Default: 16ms (~60 FPS) - zabránenie blokovania UI.
        /// </summary>
        public int BatchTimeoutMs { get; set; } = 16;

        /// <summary>
        /// Povoliť batch spracovanie.
        /// Default: true - výrazne zlepšuje výkon pri veľkých datasetoch.
        /// </summary>
        public bool EnableBatchProcessing { get; set; } = true;

        #endregion

        #region Pokročilé optimalizácie

        /// <summary>
        /// Maximálny počet súčasne bežiacich validácií.
        /// Default: 3 - zabránenie preťaženia CPU.
        /// </summary>
        public int MaxConcurrentValidations { get; set; }

        /// <summary>
        /// Prah počtu riadkov pre zapnutie virtualizácie.
        /// Default: 1000 riadkov - nad týmto limitom sa zapne virtualizácia.
        /// </summary>
        public int VirtualizationThreshold { get; set; }

        /// <summary>
        /// Prah počtu riadkov pre zapnutie lazy loading.
        /// Default: 2000 riadkov - nad týmto limitom sa zapne lazy loading.
        /// </summary>
        public int LazyLoadingThreshold { get; set; }

        /// <summary>
        /// Povoliť preemptive validáciu na pozadí.
        /// Default: false - môže byť užitočné pre veľké datasety.
        /// </summary>
        public bool EnableBackgroundValidation { get; set; } = false;

        #endregion

        #region Memory management

        /// <summary>
        /// Interval pre cleanup nepoužívaných zdrojov v milisekundách.
        /// Default: 30000ms (30 sekúnd).
        /// </summary>
        public int ResourceCleanupIntervalMs { get; set; } = 30000;

        /// <summary>
        /// Povoliť automatický cleanup zdrojov.
        /// Default: true - odporúčané pre dlhodobo bežiace aplikácie.
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// Maximálna veľkosť cache pre UI elementy.
        /// Default: 500 - vyvážené medzi pamäťou a výkonom.
        /// </summary>
        public int UIElementCacheSize { get; set; } = 500;

        #endregion

        #region Predvolené konfigurácie

        /// <summary>
        /// Predvolená konfigurácia pre všeobecné použitie.
        /// Vyvážené medzi výkonom a responsiveness.
        /// </summary>
        public static ThrottlingConfig Default => new ThrottlingConfig();

        /// <summary>
        /// Konfigurácia optimalizovaná pre vysoký výkon.
        /// Agresívnejšie throttling, väčšie batch-e.
        /// </summary>
        public static ThrottlingConfig HighPerformance => new ThrottlingConfig
        {
            ValidationDebounceMs = 500,
            UIUpdateDebounceMs = 150,
            BatchSize = 100,
            MaxConcurrentValidations = 2,
            VirtualizationThreshold = 500,
            LazyLoadingThreshold = 1000,
            EnableBackgroundValidation = true
        };

        /// <summary>
        /// Konfigurácia optimalizovaná pre responsiveness.
        /// Menšie throttling, menšie batch-e, rychlejšia odozva.
        /// </summary>
        public static ThrottlingConfig HighResponsiveness => new ThrottlingConfig
        {
            ValidationDebounceMs = 150,
            UIUpdateDebounceMs = 50,
            BatchSize = 25,
            MaxConcurrentValidations = 5,
            VirtualizationThreshold = 2000,
            LazyLoadingThreshold = 5000,
            EnableBackgroundValidation = false
        };

        /// <summary>
        /// Konfigurácia pre veľké datasety (10k+ riadkov).
        /// Maximálne optimalizácie pre výkon.
        /// </summary>
        public static ThrottlingConfig LargeDataset => new ThrottlingConfig
        {
            ValidationDebounceMs = 1000,
            UIUpdateDebounceMs = 200,
            BatchSize = 200,
            MaxConcurrentValidations = 1,
            VirtualizationThreshold = 100,
            LazyLoadingThreshold = 500,
            EnableBackgroundValidation = true,
            ResourceCleanupIntervalMs = 10000
        };

        /// <summary>
        /// Konfigurácia pre debugging - vypnuté všetky optimalizácie.
        /// Iba pre development a ladenie.
        /// </summary>
        public static ThrottlingConfig Debug => new ThrottlingConfig
        {
            ValidationDebounceMs = 0,
            UIUpdateDebounceMs = 0,
            BatchSize = 1,
            EnableValidationThrottling = false,
            EnableUIThrottling = false,
            EnableBatchProcessing = false,
            MaxConcurrentValidations = 1,
            EnableBackgroundValidation = false,
            EnableAutoCleanup = false
        };

        #endregion

        #region Validácia konfigurácie

        /// <summary>
        /// Skontroluje či je konfigurácia validná a opraví neplatné hodnoty.
        /// </summary>
        public void ValidateAndFix()
        {
            // Throttling timeouts
            ValidationDebounceMs = Math.Max(0, ValidationDebounceMs);
            UIUpdateDebounceMs = Math.Max(0, UIUpdateDebounceMs);
            BatchTimeoutMs = Math.Max(1, BatchTimeoutMs);

            // Batch size
            BatchSize = Math.Max(1, BatchSize);
            BatchSize = Math.Min(1000, BatchSize); // Maximum 1000 per batch

            // Concurrency
            MaxConcurrentValidations = Math.Max(1, MaxConcurrentValidations);
            MaxConcurrentValidations = Math.Min(10, MaxConcurrentValidations); // Maximum 10

            // Thresholds
            VirtualizationThreshold = Math.Max(0, VirtualizationThreshold);
            LazyLoadingThreshold = Math.Max(0, LazyLoadingThreshold);

            // Cleanup
            ResourceCleanupIntervalMs = Math.Max(1000, ResourceCleanupIntervalMs); // Minimum 1 second
            UIElementCacheSize = Math.Max(10, UIElementCacheSize);
            UIElementCacheSize = Math.Min(2000, UIElementCacheSize); // Maximum 2000
        }

        /// <summary>
        /// Vráti zoznam varovaní o potenciálne problematických nastaveniach.
        /// </summary>
        public List<string> GetConfigurationWarnings()
        {
            var warnings = new List<string>();

            if (ValidationDebounceMs < 100)
                warnings.Add("ValidationDebounceMs < 100ms môže spôsobiť vysoké CPU utilizácie");

            if (BatchSize > 200)
                warnings.Add("BatchSize > 200 môže spôsobiť blokovanie UI");

            if (MaxConcurrentValidations > 5)
                warnings.Add("MaxConcurrentValidations > 5 môže preťažiť CPU");

            if (!EnableValidationThrottling && !EnableUIThrottling)
                warnings.Add("Vypnuté throttling môže spôsobiť performance problémy");

            if (VirtualizationThreshold > 5000)
                warnings.Add("VirtualizationThreshold > 5000 môže spôsobiť pomalé UI");

            return warnings;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"ThrottlingConfig: Validation={ValidationDebounceMs}ms, UI={UIUpdateDebounceMs}ms, Batch={BatchSize}";
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie.
        /// </summary>
        public ThrottlingConfig Clone()
        {
            return new ThrottlingConfig
            {
                ValidationDebounceMs = ValidationDebounceMs,
                UIUpdateDebounceMs = UIUpdateDebounceMs,
                BatchSize = BatchSize,
                EnableValidationThrottling = EnableValidationThrottling,
                EnableUIThrottling = EnableUIThrottling,
                MaxConcurrentValidations = MaxConcurrentValidations,
                VirtualizationThreshold = VirtualizationThreshold,
                LazyLoadingThreshold = LazyLoadingThreshold,
                EnableBackgroundValidation = EnableBackgroundValidation,
                BatchTimeoutMs = BatchTimeoutMs,
                EnableBatchProcessing = EnableBatchProcessing,
                ResourceCleanupIntervalMs = ResourceCleanupIntervalMs,
                EnableAutoCleanup = EnableAutoCleanup,
                UIElementCacheSize = UIElementCacheSize
            };
        }

        #endregion
    }
}