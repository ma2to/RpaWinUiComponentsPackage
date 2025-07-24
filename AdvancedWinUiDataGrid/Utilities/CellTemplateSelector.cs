// Utilities/CellTemplateSelector.cs - ✅ NOVÝ SÚBOR
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Template selector pre rôzne typy buniek v DataGrid
    /// </summary>
    public class CellTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Template pre bežné dátové bunky
        /// </summary>
        public DataTemplate? CellTemplate { get; set; }

        /// <summary>
        /// Template pre delete button bunky
        /// </summary>
        public DataTemplate? DeleteButtonTemplate { get; set; }

        /// <summary>
        /// Template pre validation alerts bunky
        /// </summary>
        public DataTemplate? ValidationAlertsTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(object item)
        {
            if (item is CellDataModel cell)
            {
                return cell.ColumnName switch
                {
                    "DeleteRows" => DeleteButtonTemplate ?? CellTemplate,
                    "ValidAlerts" => ValidationAlertsTemplate ?? CellTemplate,
                    _ => CellTemplate
                };
            }

            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }

    /// <summary>
    /// Helper trieda pre multi-selection v DataGrid
    /// </summary>
    public class CellSelectionManager
    {
        private readonly List<CellDataModel> _selectedCells = new();
        private CellDataModel? _lastSelectedCell;
        private bool _isSelecting = false;

        /// <summary>
        /// Označené bunky
        /// </summary>
        public IReadOnlyList<CellDataModel> SelectedCells => _selectedCells.AsReadOnly();

        /// <summary>
        /// Event pre zmenu označenia
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// Začne označovanie buniek
        /// </summary>
        public void StartSelection(CellDataModel cell)
        {
            _isSelecting = true;
            _selectedCells.Clear();
            _selectedCells.Add(cell);
            _lastSelectedCell = cell;

            cell.IsSelected = true;
            OnSelectionChanged();
        }

        /// <summary>
        /// Rozšíri označenie na ďalšiu bunku
        /// </summary>
        public void ExtendSelection(CellDataModel cell)
        {
            if (!_isSelecting) return;

            if (!_selectedCells.Contains(cell))
            {
                _selectedCells.Add(cell);
                cell.IsSelected = true;
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Dokončí označovanie
        /// </summary>
        public void EndSelection()
        {
            _isSelecting = false;
        }

        /// <summary>
        /// Vyčistí označenie
        /// </summary>
        public void ClearSelection()
        {
            foreach (var cell in _selectedCells)
            {
                cell.IsSelected = false;
            }

            _selectedCells.Clear();
            _lastSelectedCell = null;
            _isSelecting = false;
            OnSelectionChanged();
        }

        /// <summary>
        /// Označí rozsah buniek (pre Shift+Click)
        /// </summary>
        public void SelectRange(CellDataModel startCell, CellDataModel endCell, List<RowDataModel> allRows)
        {
            if (startCell == null || endCell == null) return;

            ClearSelection();

            var startRowIndex = startCell.RowIndex;
            var endRowIndex = endCell.RowIndex;
            var startColIndex = GetColumnIndex(startCell, allRows);
            var endColIndex = GetColumnIndex(endCell, allRows);

            // Normalizácia rozsahu
            var minRow = Math.Min(startRowIndex, endRowIndex);
            var maxRow = Math.Max(startRowIndex, endRowIndex);
            var minCol = Math.Min(startColIndex, endColIndex);
            var maxCol = Math.Max(startColIndex, endColIndex);

            // Označenie rozsahu
            for (int row = minRow; row <= maxRow; row++)
            {
                if (row < allRows.Count)
                {
                    var rowData = allRows[row];
                    for (int col = minCol; col <= maxCol; col++)
                    {
                        if (col < rowData.Cells.Count)
                        {
                            var cell = rowData.Cells[col];
                            _selectedCells.Add(cell);
                            cell.IsSelected = true;
                        }
                    }
                }
            }

            OnSelectionChanged();
        }

        /// <summary>
        /// Označí celý riadok
        /// </summary>
        public void SelectRow(RowDataModel row)
        {
            ClearSelection();

            foreach (var cell in row.Cells.Where(c => c.ColumnName != "ValidAlerts"))
            {
                _selectedCells.Add(cell);
                cell.IsSelected = true;
            }

            OnSelectionChanged();
        }

        /// <summary>
        /// Označí celý stĺpec
        /// </summary>
        public void SelectColumn(string columnName, List<RowDataModel> allRows)
        {
            ClearSelection();

            foreach (var row in allRows)
            {
                var cell = row.Cells.FirstOrDefault(c => c.ColumnName == columnName);
                if (cell != null && !cell.IsEmpty)
                {
                    _selectedCells.Add(cell);
                    cell.IsSelected = true;
                }
            }

            OnSelectionChanged();
        }

        /// <summary>
        /// Získa dáta označených buniek pre copy/paste
        /// </summary>
        public List<List<object?>> GetSelectedData()
        {
            if (!_selectedCells.Any()) return new List<List<object?>>();

            // Zoradi bunky podľa pozície
            var sortedCells = _selectedCells
                .OrderBy(c => c.RowIndex)
                .ThenBy(c => GetColumnIndex(c, null))
                .ToList();

            // Vytvorí 2D štruktúru
            var minRow = sortedCells.Min(c => c.RowIndex);
            var maxRow = sortedCells.Max(c => c.RowIndex);
            var minCol = sortedCells.Min(c => GetColumnIndex(c, null));
            var maxCol = sortedCells.Max(c => GetColumnIndex(c, null));

            var result = new List<List<object?>>();

            for (int row = minRow; row <= maxRow; row++)
            {
                var rowData = new List<object?>();
                for (int col = minCol; col <= maxCol; col++)
                {
                    var cell = sortedCells.FirstOrDefault(c =>
                        c.RowIndex == row && GetColumnIndex(c, null) == col);

                    rowData.Add(cell?.Value ?? "");
                }
                result.Add(rowData);
            }

            return result;
        }

        private int GetColumnIndex(CellDataModel cell, List<RowDataModel>? allRows)
        {
            // Pre jednoduchosť vrátime hash, v reálnej implementácii by sme
            // potrebovali skutočný column index
            return Math.Abs(cell.ColumnName.GetHashCode()) % 1000;
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(_selectedCells.Count));
        }
    }

    /// <summary>
    /// Event args pre zmenu označenia
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        public int SelectedCount { get; }

        public SelectionChangedEventArgs(int selectedCount)
        {
            SelectedCount = selectedCount;
        }
    }
}