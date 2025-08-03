// Services/Interfaces/IBackgroundValidationService.cs - Interface for background validation service
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre background validation service
    /// </summary>
    internal interface IBackgroundValidationService
    {
        /// <summary>
        /// Event pre dokončenie background validácie
        /// </summary>
        event EventHandler<BackgroundValidationCompletedEventArgs>? ValidationCompleted;

        /// <summary>
        /// Event pre zmenu stavu validácie (started/completed/cancelled)
        /// </summary>
        event EventHandler<BackgroundValidationStateChangedEventArgs>? ValidationStateChanged;

        /// <summary>
        /// Inicializácia background validation service
        /// </summary>
        Task InitializeAsync(BackgroundValidationConfiguration configuration);

        /// <summary>
        /// Spustí background validáciu pre bunku
        /// </summary>
        Task<string> StartCellValidationAsync(
            string columnName, 
            object? value, 
            Dictionary<string, object?> rowData, 
            int rowIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Spustí background validáciu pre celý riadok
        /// </summary>
        Task<string> StartRowValidationAsync(
            Dictionary<string, object?> rowData, 
            int rowIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Zruší všetky prebiehajúce validácie pre bunku
        /// </summary>
        Task CancelCellValidationAsync(string columnName, int rowIndex);

        /// <summary>
        /// Zruší všetky prebiehajúce validácie pre riadok
        /// </summary>
        Task CancelRowValidationAsync(int rowIndex);

        /// <summary>
        /// Zruší všetky prebiehajúce validácie
        /// </summary>
        Task CancelAllValidationsAsync();

        /// <summary>
        /// Získa výsledok validácie pre bunku
        /// </summary>
        BackgroundValidationResult? GetCellValidationResult(string columnName, int rowIndex);

        /// <summary>
        /// Získa všetky výsledky validácie pre riadok
        /// </summary>
        Dictionary<string, BackgroundValidationResult> GetRowValidationResults(int rowIndex);

        /// <summary>
        /// Kontroluje či prebieha validácia pre bunku
        /// </summary>
        bool IsValidationRunning(string columnName, int rowIndex);

        /// <summary>
        /// Kontroluje či prebieha validácia pre riadok
        /// </summary>
        bool IsRowValidationRunning(int rowIndex);

        /// <summary>
        /// Získa počet prebiehajúcich validácií
        /// </summary>
        int GetRunningValidationsCount();

        /// <summary>
        /// Vyčistí cache validácií
        /// </summary>
        Task ClearValidationCacheAsync();

        /// <summary>
        /// Aktualizuje konfiguráciu za behu
        /// </summary>
        Task UpdateConfigurationAsync(BackgroundValidationConfiguration configuration);

        /// <summary>
        /// Pridá nové background validation rule
        /// </summary>
        Task AddBackgroundRuleAsync(BackgroundValidationRule rule);

        /// <summary>
        /// Odstráni background validation rule
        /// </summary>
        Task RemoveBackgroundRuleAsync(string columnName);

        /// <summary>
        /// Získa diagnostické informácie
        /// </summary>
        BackgroundValidationDiagnostics GetDiagnostics();
    }

    /// <summary>
    /// Event args pre dokončenie background validácie
    /// </summary>
    public class BackgroundValidationCompletedEventArgs : EventArgs
    {
        public string ColumnName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string ValidationId { get; set; } = string.Empty;
        public BackgroundValidationResult Result { get; set; } = new();
        public double DurationMs { get; set; }
    }

    /// <summary>
    /// Event args pre zmenu stavu validácie
    /// </summary>
    public class BackgroundValidationStateChangedEventArgs : EventArgs
    {
        public string ColumnName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string ValidationId { get; set; } = string.Empty;
        public BackgroundValidationState State { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Stav background validácie
    /// </summary>
    public enum BackgroundValidationState
    {
        Starting,
        Running,
        Completed,
        Cancelled,
        Failed,
        TimedOut
    }

    /// <summary>
    /// Diagnostické informácie pre background validácie
    /// </summary>
    public class BackgroundValidationDiagnostics
    {
        public int TotalValidationsStarted { get; set; }
        public int TotalValidationsCompleted { get; set; }
        public int TotalValidationsCancelled { get; set; }
        public int TotalValidationsFailed { get; set; }
        public int TotalValidationsTimedOut { get; set; }
        public int CurrentlyRunning { get; set; }
        public double AverageValidationTimeMs { get; set; }
        public int CacheHitCount { get; set; }
        public int CacheMissCount { get; set; }
        public DateTime LastResetTime { get; set; } = DateTime.Now;
    }
}