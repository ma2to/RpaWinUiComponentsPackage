// Models/CellSelectionState.cs - ✅ INTERNAL Cell Selection management
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell
{
    /// <summary>
    /// Internal model pre spravovanie cell selection state - INTERNAL
    /// </summary>
    internal class CellSelectionState
    {
        #region Private Fields

        private readonly HashSet<CellPosition> _selectedCells = new();
        private readonly HashSet<CellPosition> _copiedCells = new();
        private CellPosition? _focusedCell;
        private readonly object _lockObject = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Aktuálne focused bunka
        /// </summary>
        public CellPosition? FocusedCell
        {
            get
            {
                lock (_lockObject)
                {
                    return _focusedCell;
                }
            }
        }

        /// <summary>
        /// Počet vybraných buniek
        /// </summary>
        public int SelectedCellCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _selectedCells.Count;
                }
            }
        }

        /// <summary>
        /// Počet skopírovaných buniek
        /// </summary>
        public int CopiedCellCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _copiedCells.Count;
                }
            }
        }

        /// <summary>
        /// Či má bunka focus
        /// </summary>
        public bool IsCellFocused(int rowIndex, int columnIndex)
        {
            lock (_lockObject)
            {
                return _focusedCell?.Row == rowIndex && _focusedCell?.Column == columnIndex;
            }
        }

        /// <summary>
        /// Či je bunka vybraná
        /// </summary>
        public bool IsCellSelected(int rowIndex, int columnIndex)
        {
            lock (_lockObject)
            {
                return _selectedCells.Any(c => c.Row == rowIndex && c.Column == columnIndex);
            }
        }

        /// <summary>
        /// Či je bunka skopírovaná
        /// </summary>
        public bool IsCellCopied(int rowIndex, int columnIndex)
        {
            lock (_lockObject)
            {
                return _copiedCells.Any(c => c.Row == rowIndex && c.Column == columnIndex);
            }
        }

        #endregion

        #region Focus Management

        /// <summary>
        /// Nastav focus na bunku
        /// </summary>
        public void SetFocusedCell(int rowIndex, int columnIndex, string columnName)
        {
            lock (_lockObject)
            {
                _focusedCell = new CellPosition
                {
                    Row = rowIndex,
                    Column = columnIndex,
                    ColumnName = columnName
                };

                // Ak nie je focused bunka v selected, pridaj ju
                if (!IsCellSelected(rowIndex, columnIndex))
                {
                    _selectedCells.Clear(); // Clear previous selection unless Ctrl is pressed
                    _selectedCells.Add(_focusedCell);
                }
            }
        }

        /// <summary>
        /// Zruš focus
        /// </summary>
        public void ClearFocus()
        {
            lock (_lockObject)
            {
                _focusedCell = null;
            }
        }

        #endregion

        #region Selection Management

        /// <summary>
        /// Pridaj bunku do selection (pre Ctrl+Click)
        /// </summary>
        public void AddCellToSelection(int rowIndex, int columnIndex, string columnName)
        {
            lock (_lockObject)
            {
                var cellPos = new CellPosition
                {
                    Row = rowIndex,
                    Column = columnIndex,
                    ColumnName = columnName
                };

                _selectedCells.Add(cellPos);
            }
        }

        /// <summary>
        /// Odober bunku zo selection
        /// </summary>
        public void RemoveCellFromSelection(int rowIndex, int columnIndex)
        {
            lock (_lockObject)
            {
                var cellToRemove = _selectedCells.FirstOrDefault(c => c.Row == rowIndex && c.Column == columnIndex);
                if (cellToRemove != null)
                {
                    _selectedCells.Remove(cellToRemove);
                }
            }
        }

        /// <summary>
        /// Nastav single cell selection (bez Ctrl)
        /// </summary>
        public void SetSingleCellSelection(int rowIndex, int columnIndex, string columnName)
        {
            lock (_lockObject)
            {
                _selectedCells.Clear();
                _selectedCells.Add(new CellPosition
                {
                    Row = rowIndex,
                    Column = columnIndex,
                    ColumnName = columnName
                });

                // Nastav aj focus
                _focusedCell = _selectedCells.First();
            }
        }

        /// <summary>
        /// Zruš celý selection
        /// </summary>
        public void ClearSelection()
        {
            lock (_lockObject)
            {
                _selectedCells.Clear();
                _focusedCell = null;
            }
        }

        /// <summary>
        /// Získaj všetky vybrané bunky
        /// </summary>
        public List<CellPosition> GetSelectedCells()
        {
            lock (_lockObject)
            {
                return _selectedCells.ToList();
            }
        }

        #endregion

        #region Copy State Management

        /// <summary>
        /// Nastav copied cells (po Ctrl+C)
        /// </summary>
        public void SetCopiedCells(List<CellPosition> cells)
        {
            lock (_lockObject)
            {
                _copiedCells.Clear();
                foreach (var cell in cells)
                {
                    _copiedCells.Add(cell);
                }
            }
        }

        /// <summary>
        /// Zruš copied state
        /// </summary>
        public void ClearCopiedCells()
        {
            lock (_lockObject)
            {
                _copiedCells.Clear();
            }
        }

        /// <summary>
        /// Získaj všetky skopírované bunky
        /// </summary>
        public List<CellPosition> GetCopiedCells()
        {
            lock (_lockObject)
            {
                return _copiedCells.ToList();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Diagnostické info
        /// </summary>
        public string GetDiagnosticInfo()
        {
            lock (_lockObject)
            {
                return $"Focus: {_focusedCell?.ToString() ?? "None"}, " +
                       $"Selected: {_selectedCells.Count}, " +
                       $"Copied: {_copiedCells.Count}";
            }
        }

        #endregion
    }

}