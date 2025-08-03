// Core/DataGridConfiguration.cs - ✅ NOVÝ: Centrálna konfigurácia pre DataGrid
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core
{
    /// <summary>
    /// Centrálna konfigurácia pre AdvancedDataGrid komponent - INTERNAL
    /// Obsahuje všetky nastavenia potrebné pre inicializáciu a správu DataGrid-u
    /// </summary>
    internal class DataGridConfiguration
    {
        #region Core Configuration

        /// <summary>
        /// Definície stĺpcov
        /// </summary>
        public List<Models.Grid.ColumnDefinition> Columns { get; set; } = new();

        /// <summary>
        /// Počet prázdnych riadkov na vytvorenie
        /// </summary>
        public int EmptyRowsCount { get; set; } = 10;

        /// <summary>
        /// Validačné pravidlá
        /// </summary>
        public List<ValidationRule> ValidationRules { get; set; } = new();

        /// <summary>
        /// Konfigurácia farieb
        /// </summary>
        public DataGridColorConfig? ColorConfig { get; set; }

        /// <summary>
        /// Throttling konfigurácia
        /// </summary>
        public ThrottlingConfig? ThrottlingConfig { get; set; }

        #endregion

        #region Feature Flags

        /// <summary>
        /// Povoliť virtual scrolling
        /// </summary>
        public bool EnableVirtualScrolling { get; set; } = true;

        /// <summary>
        /// Povoliť zebra rows
        /// </summary>
        public bool EnableZebraRows { get; set; } = true;

        /// <summary>
        /// Povoliť resize stĺpcov
        /// </summary>
        public bool EnableColumnResize { get; set; } = true;

        /// <summary>
        /// Povoliť sorting
        /// </summary>
        public bool EnableSorting { get; set; } = true;

        /// <summary>
        /// Povoliť search
        /// </summary>
        public bool EnableSearch { get; set; } = true;

        /// <summary>
        /// Povoliť copy/paste operácie
        /// </summary>
        public bool EnableCopyPaste { get; set; } = true;

        /// <summary>
        /// Povoliť export/import
        /// </summary>
        public bool EnableExportImport { get; set; } = true;

        /// <summary>
        /// Povoliť checkbox column
        /// </summary>
        public bool EnableCheckBoxColumn { get; set; } = false;

        /// <summary>
        /// Povoliť auto-add funkčnosť
        /// </summary>
        public bool AutoAddEnabled { get; set; } = true;

        /// <summary>
        /// Unifikovaný počet riadkov
        /// </summary>
        public int UnifiedRowCount { get; set; } = 15;

        /// <summary>
        /// Povoliť CheckBox stĺpec (alias pre EnableCheckBoxColumn)
        /// </summary>
        public bool CheckBoxColumnEnabled 
        { 
            get => EnableCheckBoxColumn; 
            set => EnableCheckBoxColumn = value; 
        }

        /// <summary>
        /// Minimálna šírka ValidAlerts stĺpca
        /// </summary>
        public double ValidAlertsMinWidth { get; set; } = 200;

        /// <summary>
        /// Povoliť background processing
        /// </summary>
        public bool EnableBackgroundProcessing { get; set; } = true;

        /// <summary>
        /// Konfigurácia pre background validácie
        /// </summary>
        public BackgroundValidationConfiguration? BackgroundValidationConfig { get; set; }

        #endregion

        #region Performance Settings

        /// <summary>
        /// Maximum počet riadkov pre virtual scrolling
        /// </summary>
        public int VirtualScrollingThreshold { get; set; } = 100;

        /// <summary>
        /// Batch size pre background operácie
        /// </summary>
        public int BackgroundProcessingBatchSize { get; set; } = 50;

        /// <summary>
        /// Timeout pre async operácie (v ms)
        /// </summary>
        public int AsyncOperationTimeout { get; set; } = 30000;

        #endregion

        #region Factory Methods

        /// <summary>
        /// Vytvorí základnú konfiguráciu
        /// </summary>
        public static DataGridConfiguration CreateDefault()
        {
            return new DataGridConfiguration
            {
                EmptyRowsCount = 10,
                EnableVirtualScrolling = true,
                EnableZebraRows = true,
                EnableColumnResize = true,
                EnableSorting = true,
                EnableSearch = true,
                EnableCopyPaste = true,
                EnableExportImport = true,
                EnableBackgroundProcessing = true,
                VirtualScrollingThreshold = 100,
                BackgroundProcessingBatchSize = 50,
                AsyncOperationTimeout = 30000
            };
        }

        /// <summary>
        /// Vytvorí vysoko výkonnú konfiguráciu pre veľké datasety
        /// </summary>
        public static DataGridConfiguration CreateHighPerformance()
        {
            return new DataGridConfiguration
            {
                EmptyRowsCount = 20,
                EnableVirtualScrolling = true,
                EnableZebraRows = false,  // Disable pre lepší výkon
                EnableColumnResize = true,
                EnableSorting = true,
                EnableSearch = true,
                EnableCopyPaste = true,
                EnableExportImport = true,
                EnableBackgroundProcessing = true,
                VirtualScrollingThreshold = 50,  // Nižší threshold
                BackgroundProcessingBatchSize = 100,  // Väčší batch
                AsyncOperationTimeout = 60000  // Dlhší timeout
            };
        }

        /// <summary>
        /// Vytvorí minimálnu konfiguráciu pre jednoduché použitie
        /// </summary>
        public static DataGridConfiguration CreateMinimal()
        {
            return new DataGridConfiguration
            {
                EmptyRowsCount = 5,
                EnableVirtualScrolling = false,
                EnableZebraRows = true,
                EnableColumnResize = false,
                EnableSorting = false,
                EnableSearch = false,
                EnableCopyPaste = false,
                EnableExportImport = false,
                EnableBackgroundProcessing = false,
                VirtualScrollingThreshold = 1000,
                BackgroundProcessingBatchSize = 25,
                AsyncOperationTimeout = 15000
            };
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (EmptyRowsCount < 0)
                errors.Add("EmptyRowsCount cannot be negative");

            if (VirtualScrollingThreshold <= 0)
                errors.Add("VirtualScrollingThreshold must be positive");

            if (BackgroundProcessingBatchSize <= 0)
                errors.Add("BackgroundProcessingBatchSize must be positive");

            if (AsyncOperationTimeout <= 0)
                errors.Add("AsyncOperationTimeout must be positive");

            return errors.Count == 0;
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie o konfigurácii
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var enabledFeatures = 0;
            if (EnableVirtualScrolling) enabledFeatures++;
            if (EnableZebraRows) enabledFeatures++;
            if (EnableColumnResize) enabledFeatures++;
            if (EnableSorting) enabledFeatures++;
            if (EnableSearch) enabledFeatures++;
            if (EnableCopyPaste) enabledFeatures++;
            if (EnableExportImport) enabledFeatures++;
            if (EnableCheckBoxColumn) enabledFeatures++;
            if (EnableBackgroundProcessing) enabledFeatures++;

            return $"DataGridConfiguration - Columns: {Columns.Count}, " +
                   $"EmptyRows: {EmptyRowsCount}, EnabledFeatures: {enabledFeatures}/9, " +
                   $"VirtualThreshold: {VirtualScrollingThreshold}";
        }

        #endregion
    }
}