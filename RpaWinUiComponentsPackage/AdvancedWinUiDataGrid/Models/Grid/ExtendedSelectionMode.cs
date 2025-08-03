// Models/ExtendedSelectionMode.cs - ✅ NOVÝ: Extended Selection Modes Support
using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid
{
    /// <summary>
    /// Typy extended selection modes - INTERNAL
    /// </summary>
    internal enum ExtendedSelectionMode
    {
        /// <summary>
        /// Základný single cell selection
        /// </summary>
        SingleCell,

        /// <summary>
        /// Multi-cell selection s Ctrl+Click
        /// </summary>
        MultiCell,

        /// <summary>
        /// Range selection s Shift+Click (rozsah buniek)
        /// </summary>
        RangeSelection,

        /// <summary>
        /// Multi-range selection - viacero rozsahov s Ctrl+Click
        /// </summary>
        MultiRangeSelection,

        /// <summary>
        /// Row header selection - klik na číslo riadku označí celý riadok
        /// </summary>
        RowHeaderSelection,

        /// <summary>
        /// Column header selection - klik na header označí celý stĺpec
        /// </summary>
        ColumnHeaderSelection,

        /// <summary>
        /// Block selection - označenie obdĺžnikového bloku buniek
        /// </summary>
        BlockSelection
    }

    /// <summary>
    /// Konfigurácia pre extended selection modes - INTERNAL
    /// </summary>
    internal class ExtendedSelectionConfiguration
    {
        #region Properties

        /// <summary>
        /// Povolené selection modes
        /// </summary>
        public HashSet<ExtendedSelectionMode> EnabledModes { get; set; } = new();

        /// <summary>
        /// Maximálny počet súčasne označených buniek (0 = neobmedzené)
        /// </summary>
        public int MaxSelectedCells { get; set; } = 0;

        /// <summary>
        /// Maximálny počet súčasne označených rozsahov (0 = neobmedzené)
        /// </summary>
        public int MaxSelectedRanges { get; set; } = 0;

        /// <summary>
        /// Povolenie označovania cez mouse drag
        /// </summary>
        public bool EnableMouseDragSelection { get; set; } = true;

        /// <summary>
        /// Povolenie označovania pomocou klávesnice
        /// </summary>
        public bool EnableKeyboardSelection { get; set; } = true;

        /// <summary>
        /// Zobrazenie visual indicators pre selection
        /// </summary>
        public bool ShowSelectionIndicators { get; set; } = true;

        /// <summary>
        /// Farba pozadia pre označené bunky
        /// </summary>
        public string SelectionBackgroundColor { get; set; } = "#3399FF";

        /// <summary>
        /// Farba pozadia pre označené ranges
        /// </summary>
        public string RangeSelectionBackgroundColor { get; set; } = "#66CCFF";

        /// <summary>
        /// Farba pozadia pre označené riadky
        /// </summary>
        public string RowSelectionBackgroundColor { get; set; } = "#E6F3FF";

        /// <summary>
        /// Farba pozadia pre označené stĺpce
        /// </summary>
        public string ColumnSelectionBackgroundColor { get; set; } = "#F0F8FF";

        #endregion

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia - single a multi cell selection
        /// </summary>
        public static ExtendedSelectionConfiguration Basic => new()
        {
            EnabledModes = new HashSet<ExtendedSelectionMode>
            {
                ExtendedSelectionMode.SingleCell,
                ExtendedSelectionMode.MultiCell
            },
            MaxSelectedCells = 100,
            EnableMouseDragSelection = true,
            EnableKeyboardSelection = true,
            ShowSelectionIndicators = true
        };

        /// <summary>
        /// Pokročilá konfigurácia s range selection
        /// </summary>
        public static ExtendedSelectionConfiguration Advanced => new()
        {
            EnabledModes = new HashSet<ExtendedSelectionMode>
            {
                ExtendedSelectionMode.SingleCell,
                ExtendedSelectionMode.MultiCell,
                ExtendedSelectionMode.RangeSelection,
                ExtendedSelectionMode.MultiRangeSelection
            },
            MaxSelectedCells = 500,
            MaxSelectedRanges = 10,
            EnableMouseDragSelection = true,
            EnableKeyboardSelection = true,
            ShowSelectionIndicators = true
        };

        /// <summary>
        /// Kompletná konfigurácia so všetkými selection modes
        /// </summary>
        public static ExtendedSelectionConfiguration Complete => new()
        {
            EnabledModes = new HashSet<ExtendedSelectionMode>
            {
                ExtendedSelectionMode.SingleCell,
                ExtendedSelectionMode.MultiCell,
                ExtendedSelectionMode.RangeSelection,
                ExtendedSelectionMode.MultiRangeSelection,
                ExtendedSelectionMode.RowHeaderSelection,
                ExtendedSelectionMode.ColumnHeaderSelection,
                ExtendedSelectionMode.BlockSelection
            },
            MaxSelectedCells = 0, // Neobmedzené
            MaxSelectedRanges = 0, // Neobmedzené
            EnableMouseDragSelection = true,
            EnableKeyboardSelection = true,
            ShowSelectionIndicators = true
        };

        #endregion

        #region Methods

        /// <summary>
        /// Skontroluje či je daný selection mode povolený
        /// </summary>
        public bool IsModeEnabled(ExtendedSelectionMode mode)
        {
            return EnabledModes.Contains(mode);
        }

        /// <summary>
        /// Pridá selection mode do povolených
        /// </summary>
        public void EnableMode(ExtendedSelectionMode mode)
        {
            EnabledModes.Add(mode);
        }

        /// <summary>
        /// Odstráni selection mode z povolených
        /// </summary>
        public void DisableMode(ExtendedSelectionMode mode)
        {
            EnabledModes.Remove(mode);
        }

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (MaxSelectedCells < 0) MaxSelectedCells = 0;
            if (MaxSelectedRanges < 0) MaxSelectedRanges = 0;
            
            if (string.IsNullOrEmpty(SelectionBackgroundColor))
                SelectionBackgroundColor = "#3399FF";
                
            if (string.IsNullOrEmpty(RangeSelectionBackgroundColor))
                RangeSelectionBackgroundColor = "#66CCFF";
                
            if (string.IsNullOrEmpty(RowSelectionBackgroundColor))
                RowSelectionBackgroundColor = "#E6F3FF";
                
            if (string.IsNullOrEmpty(ColumnSelectionBackgroundColor))
                ColumnSelectionBackgroundColor = "#F0F8FF";
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public ExtendedSelectionConfiguration Clone()
        {
            return new ExtendedSelectionConfiguration
            {
                EnabledModes = new HashSet<ExtendedSelectionMode>(EnabledModes),
                MaxSelectedCells = MaxSelectedCells,
                MaxSelectedRanges = MaxSelectedRanges,
                EnableMouseDragSelection = EnableMouseDragSelection,
                EnableKeyboardSelection = EnableKeyboardSelection,
                ShowSelectionIndicators = ShowSelectionIndicators,
                SelectionBackgroundColor = SelectionBackgroundColor,
                RangeSelectionBackgroundColor = RangeSelectionBackgroundColor,
                RowSelectionBackgroundColor = RowSelectionBackgroundColor,
                ColumnSelectionBackgroundColor = ColumnSelectionBackgroundColor
            };
        }

        #endregion
    }

    /// <summary>
    /// Selection state pre extended selection modes - INTERNAL
    /// </summary>
    internal class ExtendedSelectionState
    {
        #region Properties

        /// <summary>
        /// Aktuálne označené bunky (row, column)
        /// </summary>
        public HashSet<(int Row, int Column)> SelectedCells { get; set; } = new();

        /// <summary>
        /// Aktuálne označené rozsahy
        /// </summary>
        public List<CellRange> SelectedRanges { get; set; } = new();

        /// <summary>
        /// Označené celé riadky
        /// </summary>
        public HashSet<int> SelectedRows { get; set; } = new();

        /// <summary>
        /// Označené celé stĺpce
        /// </summary>
        public HashSet<int> SelectedColumns { get; set; } = new();

        /// <summary>
        /// Anchor point pre range selection (začiatočná bunka)
        /// </summary>
        public (int Row, int Column)? AnchorCell { get; set; }

        /// <summary>
        /// Aktuálny selection mode
        /// </summary>
        public ExtendedSelectionMode CurrentMode { get; set; } = ExtendedSelectionMode.SingleCell;

        /// <summary>
        /// Počet celkovo označených buniek
        /// </summary>
        public int TotalSelectedCells => 
            SelectedCells.Count + 
            SelectedRanges.Sum(r => r.GetCellCount()) +
            SelectedRows.Count * (ColumnCount ?? 0) +
            SelectedColumns.Count * (RowCount ?? 0);

        /// <summary>
        /// Celkový počet riadkov (pre výpočet selected cells)
        /// </summary>
        public int? RowCount { get; set; }

        /// <summary>
        /// Celkový počet stĺpcov (pre výpočet selected cells)
        /// </summary>
        public int? ColumnCount { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Vyčistí všetky selections
        /// </summary>
        public void Clear()
        {
            SelectedCells.Clear();
            SelectedRanges.Clear();
            SelectedRows.Clear();
            SelectedColumns.Clear();
            AnchorCell = null;
        }

        /// <summary>
        /// Skontroluje či je bunka označená
        /// </summary>
        public bool IsCellSelected(int row, int column)
        {
            // Priamo označená bunka
            if (SelectedCells.Contains((row, column)))
                return true;

            // Bunka v označenom range
            if (SelectedRanges.Any(r => r.ContainsCell(row, column)))
                return true;

            // Bunka v označenom riadku
            if (SelectedRows.Contains(row))
                return true;

            // Bunka v označenom stĺpci
            if (SelectedColumns.Contains(column))
                return true;

            return false;
        }

        /// <summary>
        /// Pridá bunku do selection
        /// </summary>
        public void AddCell(int row, int column)
        {
            SelectedCells.Add((row, column));
        }

        /// <summary>
        /// Odstráni bunku zo selection
        /// </summary>
        public void RemoveCell(int row, int column)
        {
            SelectedCells.Remove((row, column));
        }

        /// <summary>
        /// Pridá range do selection
        /// </summary>
        public void AddRange(CellRange range)
        {
            SelectedRanges.Add(range);
        }

        /// <summary>
        /// Označí celý riadok
        /// </summary>
        public void SelectRow(int row)
        {
            SelectedRows.Add(row);
        }

        /// <summary>
        /// Označí celý stĺpec
        /// </summary>
        public void SelectColumn(int column)
        {
            SelectedColumns.Add(column);
        }

        /// <summary>
        /// Získa všetky označené bunky ako jednotlivé pozície
        /// </summary>
        public List<(int Row, int Column)> GetAllSelectedCells()
        {
            var result = new List<(int Row, int Column)>(SelectedCells);

            // Pridaj bunky z ranges
            foreach (var range in SelectedRanges)
            {
                for (int row = range.StartRow; row <= range.EndRow; row++)
                {
                    for (int col = range.StartColumn; col <= range.EndColumn; col++)
                    {
                        result.Add((row, col));
                    }
                }
            }

            // Pridaj bunky z označených riadkov
            if (ColumnCount.HasValue)
            {
                foreach (var row in SelectedRows)
                {
                    for (int col = 0; col < ColumnCount.Value; col++)
                    {
                        result.Add((row, col));
                    }
                }
            }

            // Pridaj bunky z označených stĺpcov
            if (RowCount.HasValue)
            {
                foreach (var col in SelectedColumns)
                {
                    for (int row = 0; row < RowCount.Value; row++)
                    {
                        result.Add((row, col));
                    }
                }
            }

            return result.Distinct().ToList();
        }

        #endregion
    }
}

