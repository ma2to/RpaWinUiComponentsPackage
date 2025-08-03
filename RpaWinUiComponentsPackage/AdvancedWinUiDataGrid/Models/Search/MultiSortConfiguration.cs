// Models/MultiSortConfiguration.cs - ✅ PUBLIC configuration pre Multi-Sort
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search
{
    /// <summary>
    /// Konfigurácia pre Multi-Sort funkcionalitu - ✅ PUBLIC API
    /// </summary>
    public class MultiSortConfiguration
    {
        /// <summary>
        /// Maximálny počet súčasne sortovaných stĺpcov (default: 5)
        /// </summary>
        public int MaxSortColumns { get; set; } = 5;

        /// <summary>
        /// Povoliť Multi-Sort (default: true)
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Zobrazovať priority čísla v UI (default: true)
        /// </summary>
        public bool ShowPriorityNumbers { get; set; } = true;

        /// <summary>
        /// Klávesa pre Multi-Sort toggle (default: Ctrl)
        /// </summary>
        public MultiSortModifierKey ModifierKey { get; set; } = MultiSortModifierKey.Ctrl;

        /// <summary>
        /// Automaticky vyčistiť staré sortovanie pri pridávaní nového (default: false)
        /// </summary>
        public bool AutoClearOldSorts { get; set; } = false;

        /// <summary>
        /// Typ sort priority assignment (Sequential = 1,2,3... | Timestamp = podľa času)
        /// </summary>
        public SortPriorityMode PriorityMode { get; set; } = SortPriorityMode.Sequential;

        /// <summary>
        /// Predvolené konfigurácie
        /// </summary>
        public static MultiSortConfiguration Default => new();

        public static MultiSortConfiguration Limited => new()
        {
            MaxSortColumns = 3,
            ShowPriorityNumbers = true,
            AutoClearOldSorts = false
        };

        public static MultiSortConfiguration Advanced => new()
        {
            MaxSortColumns = 10,
            ShowPriorityNumbers = true,
            ModifierKey = MultiSortModifierKey.Ctrl,
            PriorityMode = SortPriorityMode.Timestamp
        };

        public static MultiSortConfiguration Disabled => new()
        {
            IsEnabled = false,
            MaxSortColumns = 1
        };

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public bool IsValid()
        {
            return MaxSortColumns > 0 && MaxSortColumns <= 20;
        }

        /// <summary>
        /// Získa popis konfigurácie
        /// </summary>
        public string GetDescription()
        {
            if (!IsEnabled) return "Multi-Sort Disabled";

            return $"Multi-Sort: Max {MaxSortColumns} columns, " +
                   $"Modifier: {ModifierKey}, " +
                   $"Priority: {(ShowPriorityNumbers ? "Visible" : "Hidden")}, " +
                   $"Mode: {PriorityMode}";
        }

        public override string ToString()
        {
            return GetDescription();
        }
    }

    /// <summary>
    /// Klávesové modifikátory pre Multi-Sort
    /// </summary>
    public enum MultiSortModifierKey
    {
        Ctrl,
        Shift,
        Alt,
        None
    }

    /// <summary>
    /// Režim priority assignment pre Multi-Sort
    /// </summary>
    public enum SortPriorityMode
    {
        /// <summary>
        /// Sekvenčné číslovanie (1, 2, 3...)
        /// </summary>
        Sequential,

        /// <summary>
        /// Podľa časového razítka pridania
        /// </summary>
        Timestamp
    }
}