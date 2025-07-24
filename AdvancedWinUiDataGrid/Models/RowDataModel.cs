// Models/RowDataModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Model pre riadok v DataGrid
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
    }

    /// <summary>
    /// Model pre bunku v DataGrid
    /// </summary>
    public class CellDataModel : INotifyPropertyChanged
    {
        private string _columnName = string.Empty;
        private object? _value;
        private Type _dataType = typeof(string);
        private bool _isValid = true;
        private string _validationErrors = string.Empty;
        private bool _isSelected;
        private bool _isEditing;
        private object? _originalValue;

        /// <summary>
        /// Názov stĺpca ku ktorému bunka patrí
        /// </summary>
        public string ColumnName
        {
            get => _columnName;
            set => SetProperty(ref _columnName, value);
        }

        /// <summary>
        /// Hodnota bunky
        /// </summary>
        public object? Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    OnPropertyChanged(nameof(DisplayValue));
                }
            }
        }

        /// <summary>
        /// Pôvodná hodnota (pred editáciou)
        /// </summary>
        public object? OriginalValue
        {
            get => _originalValue;
            set => SetProperty(ref _originalValue, value);
        }

        /// <summary>
        /// Hodnota pre zobrazenie (string reprezentácia)
        /// </summary>
        public string DisplayValue => Value?.ToString() ?? string.Empty;

        /// <summary>
        /// Dátový typ bunky
        /// </summary>
        public Type DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value);
        }

        /// <summary>
        /// Či je bunka validná
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        /// <summary>
        /// Validačné chyby bunky
        /// </summary>
        public string ValidationErrors
        {
            get => _validationErrors;
            set => SetProperty(ref _validationErrors, value);
        }

        /// <summary>
        /// Či je bunka označená/selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Či sa bunka práve edituje
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    if (value)
                    {
                        // Začiatok editácie - ulož pôvodnú hodnotu
                        OriginalValue = Value;
                    }
                }
            }
        }

        /// <summary>
        /// Či má bunka validačné chyby
        /// </summary>
        public bool HasValidationErrors => !string.IsNullOrEmpty(ValidationErrors);

        /// <summary>
        /// Či je hodnota prázdna/null
        /// </summary>
        public bool IsEmpty => Value == null || string.IsNullOrWhiteSpace(Value.ToString());

        /// <summary>
        /// Či sa hodnota zmenila od posledného uloženia
        /// </summary>
        public bool IsModified => !Equals(Value, OriginalValue);

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
        /// Začne editáciu bunky
        /// </summary>
        public void StartEditing()
        {
            IsEditing = true;
        }

        /// <summary>
        /// Dokončí editáciu bunky (potvrdi zmeny)
        /// </summary>
        public void FinishEditing()
        {
            IsEditing = false;
            OriginalValue = Value; // Potvrď zmeny
        }

        /// <summary>
        /// Zruší editáciu bunky (vráti pôvodnú hodnotu)
        /// </summary>
        public void CancelEditing()
        {
            if (IsEditing)
            {
                Value = OriginalValue; // Vráť pôvodnú hodnotu
                IsEditing = false;
            }
        }

        /// <summary>
        /// Konvertuje hodnotu na zadaný typ
        /// </summary>
        public T? GetValueAs<T>()
        {
            if (Value == null) return default(T);

            try
            {
                if (Value is T directCast)
                    return directCast;

                return (T)Convert.ChangeType(Value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Nastaví hodnotu s typovou kontrolou
        /// </summary>
        public bool TrySetValue(object? newValue)
        {
            try
            {
                if (newValue == null)
                {
                    Value = null;
                    return true;
                }

                // Pokus o konverziu na správny typ
                var convertedValue = Convert.ChangeType(newValue, DataType);
                Value = convertedValue;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vyčisti bunku (vráti na default hodnoty)
        /// </summary>
        public void Clear()
        {
            Value = null;
            OriginalValue = null;
            IsValid = true;
            ValidationErrors = string.Empty;
            IsSelected = false;
            IsEditing = false;
        }

        public override string ToString()
        {
            return $"{ColumnName}: {DisplayValue} (Valid: {IsValid})";
        }
    }
}