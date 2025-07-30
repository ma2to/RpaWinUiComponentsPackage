// Models/CellData.cs
using System;
using System.ComponentModel;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Reprezentuje dáta a stav jednej bunky v DataGrid.
    /// Obsahuje hodnotu, validačný stav, pozíciu a metadata.
    /// INTERNAL - nie je súčasťou verejného API.
    /// </summary>
    internal class CellData : INotifyPropertyChanged, IDisposable
    {
        #region Private fields

        private object? _value;
        private bool _isValid = true;
        private string? _errorMessage;
        private bool _hasChanges = false;
        private bool _isSelected = false;
        private bool _isEditing = false;
        private object? _originalValue;
        private bool _disposed = false;

        #endregion

        #region Konštruktory

        /// <summary>
        /// Vytvorí novú bunku s určenou pozíciou a definíciou stĺpca.
        /// </summary>
        public CellData(int rowIndex, int columnIndex, ColumnDefinition columnDefinition)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ColumnDefinition = columnDefinition ?? throw new ArgumentNullException(nameof(columnDefinition));

            // Nastaviť default hodnotu podľa typu
            _value = GetDefaultValueForType(columnDefinition.DataType);
            _originalValue = _value;
        }

        #endregion

        #region Pozícia a metadata

        /// <summary>
        /// Index riadku v tabuľke (0-based).
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// Index stĺpca v tabuľke (0-based).
        /// </summary>
        public int ColumnIndex { get; }

        /// <summary>
        /// Definícia stĺpca ktorý táto bunka reprezentuje.
        /// </summary>
        public ColumnDefinition ColumnDefinition { get; }

        /// <summary>
        /// Názov stĺpca (skratka pre ColumnDefinition.Name).
        /// </summary>
        public string ColumnName => ColumnDefinition.Name;

        /// <summary>
        /// Dátový typ stĺpca (skratka pre ColumnDefinition.DataType).
        /// </summary>
        public Type DataType => ColumnDefinition.DataType;

        #endregion

        #region Hodnota a zmeny

        /// <summary>
        /// Aktuálna hodnota bunky.
        /// </summary>
        public object? Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    _hasChanges = !Equals(_originalValue, value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasChanges));
                    OnPropertyChanged(nameof(DisplayValue));
                }
            }
        }

        /// <summary>
        /// Pôvodná hodnota pred zmenami.
        /// </summary>
        public object? OriginalValue => _originalValue;

        /// <summary>
        /// Určuje či má bunka nepotvrdené zmeny.
        /// </summary>
        public bool HasChanges
        {
            get => _hasChanges;
            private set
            {
                if (_hasChanges != value)
                {
                    _hasChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Hodnota formátovaná pre zobrazenie v UI.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                if (_value == null) return string.Empty;

                // Špeciálne formátovanie pre rôzne typy
                return _value switch
                {
                    DateTime dt => dt.ToString("dd.MM.yyyy HH:mm"),
                    decimal dec => dec.ToString("N2"),
                    double dbl => dbl.ToString("N2"),
                    float flt => flt.ToString("N2"),
                    bool bl => bl ? "Áno" : "Nie",
                    _ => _value.ToString() ?? string.Empty
                };
            }
        }

        #endregion

        #region Validácia

        /// <summary>
        /// Určuje či je hodnota bunky validná.
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Chybová správa pre nevalidnú hodnotu.
        /// </summary>
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Určuje či má bunka chybové správy.
        /// </summary>
        public bool HasErrors => !string.IsNullOrEmpty(_errorMessage);

        #endregion

        #region UI stav

        /// <summary>
        /// Určuje či je bunka momentálne vybraná.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Určuje či sa bunka momentálne edituje.
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Určuje či je bunka iba na čítanie.
        /// </summary>
        public bool IsReadOnly => ColumnDefinition.IsReadOnly;

        /// <summary>
        /// Určuje či je bunka prázdna (null alebo prázdny string).
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (_value == null) return true;
                if (_value is string str) return string.IsNullOrWhiteSpace(str);
                return false;
            }
        }

        #endregion

        #region Operácie

        /// <summary>
        /// Potvrdí zmeny v bunke (nastaví OriginalValue na aktuálnu Value).
        /// </summary>
        public void CommitChanges()
        {
            _originalValue = _value;
            HasChanges = false;
        }

        /// <summary>
        /// Zruší zmeny a vráti pôvodnú hodnotu.
        /// </summary>
        public void RevertChanges()
        {
            Value = _originalValue;
            HasChanges = false;
        }

        /// <summary>
        /// Vymaže hodnotu bunky (nastaví na default pre daný typ).
        /// </summary>
        public void Clear()
        {
            Value = GetDefaultValueForType(DataType);
        }

        /// <summary>
        /// Nastaví hodnotu a označí ju ako commit-nutú (bez HasChanges).
        /// </summary>
        public void SetValueWithoutChange(object? value)
        {
            _value = value;
            _originalValue = value;
            _hasChanges = false;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(OriginalValue));
            OnPropertyChanged(nameof(HasChanges));
            OnPropertyChanged(nameof(DisplayValue));
        }

        #endregion

        #region Konverzia typov

        /// <summary>
        /// Pokúsi sa konvertovať hodnotu na cieľový typ.
        /// </summary>
        public bool TryConvertValue(object? inputValue, out object? convertedValue)
        {
            convertedValue = null;

            try
            {
                if (inputValue == null)
                {
                    convertedValue = GetDefaultValueForType(DataType);
                    return true;
                }

                if (DataType.IsAssignableFrom(inputValue.GetType()))
                {
                    convertedValue = inputValue;
                    return true;
                }

                // Špeciálne konverzie
                if (DataType == typeof(string))
                {
                    convertedValue = inputValue.ToString();
                    return true;
                }

                if (DataType == typeof(int) || DataType == typeof(int?))
                {
                    if (int.TryParse(inputValue.ToString(), out int intValue))
                    {
                        convertedValue = intValue;
                        return true;
                    }
                }

                if (DataType == typeof(decimal) || DataType == typeof(decimal?))
                {
                    if (decimal.TryParse(inputValue.ToString(), out decimal decValue))
                    {
                        convertedValue = decValue;
                        return true;
                    }
                }

                if (DataType == typeof(DateTime) || DataType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(inputValue.ToString(), out DateTime dateValue))
                    {
                        convertedValue = dateValue;
                        return true;
                    }
                }

                if (DataType == typeof(bool) || DataType == typeof(bool?))
                {
                    if (bool.TryParse(inputValue.ToString(), out bool boolValue))
                    {
                        convertedValue = boolValue;
                        return true;
                    }
                }

                // Fallback konverzia
                convertedValue = Convert.ChangeType(inputValue, DataType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vráti default hodnotu pre daný typ.
        /// </summary>
        private static object? GetDefaultValueForType(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Vyčistiť event handlery
                PropertyChanged = null;

                // Vyčistiť referencie
                _value = null;
                _originalValue = null;
                _errorMessage = null;

                _disposed = true;
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Cell[{RowIndex},{ColumnIndex}] {ColumnName}='{DisplayValue}' (Valid: {IsValid})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is CellData other)
            {
                return RowIndex == other.RowIndex &&
                       ColumnIndex == other.ColumnIndex &&
                       ColumnName.Equals(other.ColumnName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RowIndex, ColumnIndex, ColumnName.ToLowerInvariant());
        }

        #endregion
    }
}