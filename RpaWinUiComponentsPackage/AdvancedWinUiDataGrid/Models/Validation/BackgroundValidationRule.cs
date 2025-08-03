// Models/Validation/BackgroundValidationRule.cs - Background validation rule
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// Delegate pre async background validačnú funkciu
    /// </summary>
    /// <param name="value">Hodnota na validáciu</param>
    /// <param name="rowData">Kompletné dáta riadku pre cross-cell validácie</param>
    /// <param name="cancellationToken">Token pre zrušenie operácie</param>
    /// <returns>Výsledok validácie</returns>
    public delegate Task<BackgroundValidationResult> BackgroundValidationFunction(
        object? value, 
        Dictionary<string, object?> rowData, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Definícia validačného pravidla pre background validáciu
    /// </summary>
    public class BackgroundValidationRule
    {
        /// <summary>
        /// Názov stĺpca na ktorý sa validácia aplikuje
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Popis validačného pravidla
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Priorita validácie (1 = najvyššia, 10 = najnižšia)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Timeout pre validáciu v milisekundách
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Background validačná funkcia
        /// </summary>
        public BackgroundValidationFunction? ValidationFunction { get; set; }

        /// <summary>
        /// Či je validácia povolená
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Vytvorí pravidlo pre databázovú validáciu
        /// </summary>
        public static BackgroundValidationRule DatabaseValidation(
            string columnName, 
            string description,
            BackgroundValidationFunction validationFunction,
            int priority = 5,
            int timeoutMs = 10000)
        {
            return new BackgroundValidationRule
            {
                ColumnName = columnName,
                Description = description,
                ValidationFunction = validationFunction,
                Priority = priority,
                TimeoutMs = timeoutMs
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre API validáciu
        /// </summary>
        public static BackgroundValidationRule ApiValidation(
            string columnName, 
            string description,
            BackgroundValidationFunction validationFunction,
            int priority = 3,
            int timeoutMs = 15000)
        {
            return new BackgroundValidationRule
            {
                ColumnName = columnName,
                Description = description,
                ValidationFunction = validationFunction,
                Priority = priority,
                TimeoutMs = timeoutMs
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre komplexnú business validáciu
        /// </summary>
        public static BackgroundValidationRule ComplexBusinessRule(
            string columnName, 
            string description,
            BackgroundValidationFunction validationFunction,
            int priority = 4,
            int timeoutMs = 8000)
        {
            return new BackgroundValidationRule
            {
                ColumnName = columnName,
                Description = description,
                ValidationFunction = validationFunction,
                Priority = priority,
                TimeoutMs = timeoutMs
            };
        }
    }
}