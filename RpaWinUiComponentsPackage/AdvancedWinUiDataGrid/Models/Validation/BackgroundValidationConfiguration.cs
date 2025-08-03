// Models/Validation/BackgroundValidationConfiguration.cs - Configuration for background validation
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// Konfigurácia pre background validácie
    /// </summary>
    public class BackgroundValidationConfiguration
    {
        /// <summary>
        /// Či je background validácia povolená
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Maximálny počet súčasne bežiacich validácií
        /// </summary>
        public int MaxConcurrentValidations { get; set; } = 3;

        /// <summary>
        /// Predvolený timeout pre validácie v milisekundách
        /// </summary>
        public int DefaultTimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Delay pred spustením background validácie po zmene hodnoty (ms)
        /// </summary>
        public int ValidationDelayMs { get; set; } = 1000;

        /// <summary>
        /// Či zobraziť progress indikátor počas validácie
        /// </summary>
        public bool ShowProgressIndicator { get; set; } = true;

        /// <summary>
        /// Či spustiť background validáciu automaticky pri zmene hodnoty
        /// </summary>
        public bool AutoTriggerOnValueChange { get; set; } = true;

        /// <summary>
        /// Či spustiť background validáciu pri stratení fokusu bunky
        /// </summary>
        public bool TriggerOnCellLostFocus { get; set; } = true;

        /// <summary>
        /// Či zrušiť prebiehajúce validácie pri novej zmene hodnoty
        /// </summary>
        public bool CancelPreviousValidation { get; set; } = true;

        /// <summary>
        /// Maximálny vek cache výsledkov validácie v minútach
        /// </summary>
        public int ValidationCacheMinutes { get; set; } = 5;

        /// <summary>
        /// Či využiť cache pre duplicitné validácie
        /// </summary>
        public bool UseValidationCache { get; set; } = true;

        /// <summary>
        /// Background validačné pravidlá
        /// </summary>
        public List<BackgroundValidationRule> BackgroundRules { get; set; } = new();

        /// <summary>
        /// Predvolená konfigurácia - konzervatívna
        /// </summary>
        public static BackgroundValidationConfiguration Default => new()
        {
            IsEnabled = true,
            MaxConcurrentValidations = 2,
            DefaultTimeoutMs = 8000,
            ValidationDelayMs = 1500,
            ShowProgressIndicator = true,
            AutoTriggerOnValueChange = true,
            TriggerOnCellLostFocus = true,
            CancelPreviousValidation = true,
            UseValidationCache = true,
            ValidationCacheMinutes = 5
        };

        /// <summary>
        /// Rýchla konfigurácia - viac súčasných validácií
        /// </summary>
        public static BackgroundValidationConfiguration Fast => new()
        {
            IsEnabled = true,
            MaxConcurrentValidations = 5,
            DefaultTimeoutMs = 5000,
            ValidationDelayMs = 800,
            ShowProgressIndicator = true,
            AutoTriggerOnValueChange = true,
            TriggerOnCellLostFocus = true,
            CancelPreviousValidation = true,
            UseValidationCache = true,
            ValidationCacheMinutes = 3
        };

        /// <summary>
        /// Pomalá konfigurácia - konzervativnejšia
        /// </summary>
        public static BackgroundValidationConfiguration Conservative => new()
        {
            IsEnabled = true,
            MaxConcurrentValidations = 1,
            DefaultTimeoutMs = 15000,
            ValidationDelayMs = 2500,
            ShowProgressIndicator = true,
            AutoTriggerOnValueChange = false,
            TriggerOnCellLostFocus = true,
            CancelPreviousValidation = true,
            UseValidationCache = true,
            ValidationCacheMinutes = 10
        };

        /// <summary>
        /// Konfigurácia bez cache - pre vždy aktuálne validácie
        /// </summary>
        public static BackgroundValidationConfiguration NoCaching => new()
        {
            IsEnabled = true,
            MaxConcurrentValidations = 3,
            DefaultTimeoutMs = 10000,
            ValidationDelayMs = 1000,
            ShowProgressIndicator = true,
            AutoTriggerOnValueChange = true,
            TriggerOnCellLostFocus = true,
            CancelPreviousValidation = true,
            UseValidationCache = false,
            ValidationCacheMinutes = 0
        };
    }
}