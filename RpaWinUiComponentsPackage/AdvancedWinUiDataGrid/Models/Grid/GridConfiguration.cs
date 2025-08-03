// Models/GridConfiguration.cs
using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid
{
    /// <summary>
    /// Konfigurácia pre DataGrid komponent
    /// </summary>
    internal class GridConfiguration
    {
        /// <summary>
        /// Definície stĺpcov
        /// </summary>
        public List<ColumnDefinition> Columns { get; set; } = new();

        /// <summary>
        /// Validačné pravidlá
        /// </summary>
        public List<Models.Validation.ValidationRule> ValidationRules { get; set; } = new();

        /// <summary>
        /// Konfigurácia throttling-u
        /// </summary>
        public ThrottlingConfig ThrottlingConfig { get; set; } = ThrottlingConfig.Default;

        /// <summary>
        /// Počet prázdnych riadkov pri inicializácii
        /// </summary>
        public int EmptyRowsCount { get; set; } = 15;

        /// <summary>
        /// Či je povolená multi-selection buniek
        /// </summary>
        public bool AllowMultiSelection { get; set; } = true;

        /// <summary>
        /// Či je povolené copy/paste
        /// </summary>
        public bool AllowCopyPaste { get; set; } = true;

        /// <summary>
        /// Či je povolené mazanie riadkov
        /// </summary>
        public bool AllowRowDeletion { get; set; } = true;

        /// <summary>
        /// Či sa majú validácie spúšťať realtime
        /// </summary>
        public bool EnableRealtimeValidation { get; set; } = true;

        /// <summary>
        /// Maximálny počet riadkov (0 = neobmedzené)
        /// </summary>
        public int MaxRows { get; set; } = 0;

        /// <summary>
        /// Minimálny počet riadkov
        /// </summary>
        public int MinRows { get; set; } = 0;

        /// <summary>
        /// Názov gridu (pre logovanie a debugging)
        /// </summary>
        public string GridName { get; set; } = "AdvancedDataGrid";

        /// <summary>
        /// Či sa má automaticky pridávať nový riadok keď je posledný vyplnený
        /// </summary>
        public bool AutoAddNewRow { get; set; } = true;

        /// <summary>
        /// Konfigurácia pre background validácie
        /// </summary>
        public BackgroundValidationConfiguration? BackgroundValidationConfig { get; set; }

        /// <summary>
        /// Povoliť batch validácie (inteligentný switching medzi realtime a batch)
        /// </summary>
        public bool EnableBatchValidation { get; set; } = false;

        /// <summary>
        /// ✅ NOVÉ: Povoliť virtual scrolling pre veľké datasety (1000+ riadkov)
        /// </summary>
        public bool EnableVirtualScrolling { get; set; } = true;

        /// <summary>
        /// ✅ NOVÉ: Počet viditeľných riadkov pre virtual scrolling (default: 50)
        /// </summary>
        public int VirtualScrollingVisibleRows { get; set; } = 50;

        /// <summary>
        /// ✅ NOVÉ: Buffer size pre virtual scrolling (navyše riadky mimo viewportu)
        /// </summary>
        public int VirtualScrollingBufferSize { get; set; } = 10;

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (Columns == null || Columns.Count == 0)
                throw new InvalidOperationException("Konfigurácia musí obsahovať aspoň jeden stĺpec");

            if (EmptyRowsCount < 0)
                throw new ArgumentException("EmptyRowsCount nemôže byť záporný");

            if (MaxRows > 0 && MinRows > MaxRows)
                throw new ArgumentException("MinRows nemôže byť väčší ako MaxRows");

            if (string.IsNullOrWhiteSpace(GridName))
                throw new ArgumentException("GridName nemôže byť prázdny");

            // Validácia stĺpcov
            var columnNames = new HashSet<string>();
            foreach (var column in Columns)
            {
                if (string.IsNullOrWhiteSpace(column.Name))
                    throw new ArgumentException("Názov stĺpca nemôže byť prázdny");

                if (!columnNames.Add(column.Name))
                    throw new ArgumentException($"Duplicitný názov stĺpca: {column.Name}");

                column.Validate();
            }

            // Validácia validačných pravidiel
            foreach (var rule in ValidationRules)
            {
                if (string.IsNullOrWhiteSpace(rule.ColumnName))
                    throw new ArgumentException("ValidationRule musí mať definovaný ColumnName");

                if (!columnNames.Contains(rule.ColumnName))
                    throw new ArgumentException($"ValidationRule odkazuje na neexistujúci stĺpec: {rule.ColumnName}");
            }

            ThrottlingConfig?.Validate();

            // Validácia background validation konfigurácie
            if (BackgroundValidationConfig != null)
            {
                foreach (var bgRule in BackgroundValidationConfig.BackgroundRules)
                {
                    if (!columnNames.Contains(bgRule.ColumnName))
                        throw new ArgumentException($"Background validation rule odkazuje na neexistujúci stĺpec: {bgRule.ColumnName}");
                }
            }
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public GridConfiguration Clone()
        {
            return new GridConfiguration
            {
                Columns = new List<ColumnDefinition>(Columns.Select(c => c.Clone())),
                ValidationRules = new List<Models.Validation.ValidationRule>(ValidationRules),
                ThrottlingConfig = ThrottlingConfig.Clone(),
                EmptyRowsCount = EmptyRowsCount,
                AllowMultiSelection = AllowMultiSelection,
                AllowCopyPaste = AllowCopyPaste,
                AllowRowDeletion = AllowRowDeletion,
                EnableRealtimeValidation = EnableRealtimeValidation,
                MaxRows = MaxRows,
                MinRows = MinRows,
                GridName = GridName,
                AutoAddNewRow = AutoAddNewRow,
                BackgroundValidationConfig = BackgroundValidationConfig,
                EnableBatchValidation = EnableBatchValidation,
                EnableVirtualScrolling = EnableVirtualScrolling,
                VirtualScrollingVisibleRows = VirtualScrollingVisibleRows,
                VirtualScrollingBufferSize = VirtualScrollingBufferSize
            };
        }
    }
}