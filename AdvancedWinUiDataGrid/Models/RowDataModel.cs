// Models/RowDataModel.cs - ✅ NOVÝ SÚBOR
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Model pre riadok v DataGrid - INTERNAL MODEL
    /// </summary>
    public class RowDataModel : INotifyPropertyChanged
    {
        private int _rowIndex;
        private bool _isSelected;
        private string _validationErrors = string.Empty;

        /// <summary>
        /// Index riadku v gridu
        /// </summary>
        public int RowIndex
        {
            get => _rowIndex;
            set => SetProperty(ref _rowIndex, value);
        }

        /// <summary>
        /// Kolekcia buniek v riadku
        /// </summary>
        public ObservableCollection<CellDataModel> Cells { get; set; } = new();

        /// <summary>
        /// Či je riadok označený/selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Validačné chyby pre celý riadok
        /// </summary>
        public string ValidationErrors
        {
            get => _validationErrors;
            set => SetProperty(ref _validationErrors, value);
        }

        /// <summary>
        /// Či má riadok validačné chyby
        /// </summary>
        public bool HasValidationErrors => !string.IsNullOrEmpty(ValidationErrors);

        /// <summary>
        /// Či je celý riadok validný
        /// </summary>
        public bool IsValid => !HasValidationErrors;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        /// <summary>
        /// Získa bunku podľa názvu stĺpca
        /// </summary>
        public CellDataModel? GetCell(string columnName)
        {
            foreach (var cell in Cells)
            {
                if (cell.ColumnName == columnName)
                    return cell;
            }
            return null;
        }

        /// <summary>
        /// Nastaví hodnotu bunky
        /// </summary>
        public void SetCellValue(string columnName, object? value)
        {
            var cell = GetCell(columnName);
            if (cell != null)
            {
                cell.Value = value;
            }
        }

        /// <summary>
        /// Získa hodnotu bunky
        /// </summary>
        public object? GetCellValue(string columnName)
        {
            return GetCell(columnName)?.Value;
        }

        /// <summary>
        /// Kontroluje či je riadok úplne prázdny (ignoruje špeciálne stĺpce)
        /// </summary>
        public bool IsEmpty()
        {
            foreach (var cell in Cells)
            {
                // Ignoruj špeciálne stĺpce
                if (cell.ColumnName == "DeleteRows" || cell.ColumnName == "ValidAlerts")
                    continue;

                if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString()))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Vyčisti všetky dáta v riadku
        /// </summary>
        public void ClearData()
        {
            foreach (var cell in Cells)
            {
                if (cell.ColumnName != "ValidAlerts") // ValidAlerts sa vyčisti osobne
                {
                    cell.Value = null;
                    cell.IsValid = true;
                    cell.ValidationErrors = string.Empty;
                }
            }

            ValidationErrors = string.Empty;
        }

        public override string ToString()
        {
            var cellCount = Cells.Count;
            var validCells = Cells.Count(c => c.IsValid);
            return $"Row {RowIndex}: {cellCount} cells ({validCells} valid)";
        }
    }
}