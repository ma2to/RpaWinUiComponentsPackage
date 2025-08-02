// Controls/DataGridCell.xaml.cs - ✅ ENHANCED s komplexným error logging
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Reprezentuje jednu bunku v DataGrid
    /// </summary>
    public sealed partial class DataGridCell : UserControl, INotifyPropertyChanged
    {
        #region Private Fields

        private object? _value;
        private bool _isValid = true;
        private string _validationErrors = string.Empty;
        private bool _isSelected = false;
        private bool _isEditing = false;
        private object? _originalValue;
        private Type _dataType = typeof(string);
        private string _columnName = string.Empty;

        // ✅ NOVÉ: Logging support s nezávislou implementáciou
        private readonly ILogger _logger;
        private readonly string _cellInstanceId = Guid.NewGuid().ToString("N")[..8];
        private int _editOperationCount = 0;
        private int _valueChangeCount = 0;
        private int _validationErrorCount = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Vytvorí DataGridCell bez loggingu (NullLogger) - DEFAULT konštruktor
        /// </summary>
        public DataGridCell() : this(null)
        {
        }

        /// <summary>
        /// Vytvorí DataGridCell s voliteľným loggerom
        /// </summary>
        /// <param name="logger">ILogger pre logovanie (null = žiadne logovanie)</param>
        public DataGridCell(ILogger? logger)
        {
            try
            {
                _logger = logger ?? NullLogger.Instance;
                
                _logger.LogDebug("🔧 DataGridCell Constructor START - InstanceId: {InstanceId}, LoggerType: {LoggerType}",
                    _cellInstanceId, _logger.GetType().Name);

                this.InitializeComponent();
                this.DataContext = this;

                _logger.LogDebug("✅ DataGridCell Constructor COMPLETED - InstanceId: {InstanceId}",
                    _cellInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during DataGridCell construction - InstanceId: {InstanceId}",
                    _cellInstanceId);
                throw;
            }
        }

        #endregion

        #region Properties

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
                    UpdateCellDisplay();
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
            set
            {
                if (SetProperty(ref _isValid, value))
                {
                    UpdateValidationVisual();
                }
            }
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
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    UpdateSelectionVisual();
                }
            }
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
                        StartEditing();
                    }
                    else
                    {
                        FinishEditing();
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

        #endregion

        #region Events

        /// <summary>
        /// Event ktorý sa spustí pri zmene hodnoty bunky
        /// </summary>
        public event EventHandler<CellValueChangedEventArgs>? ValueChanged;

        /// <summary>
        /// Event ktorý sa spustí pri začatí editácie
        /// </summary>
        public event EventHandler? EditingStarted;

        /// <summary>
        /// Event ktorý sa spustí pri dokončení editácie
        /// </summary>
        public event EventHandler? EditingFinished;

        /// <summary>
        /// Event ktorý sa spustí pri zrušení editácie
        /// </summary>
        public event EventHandler? EditingCancelled;

        #endregion

        #region Private Methods

        private void UpdateCellDisplay()
        {
            if (CellTextBox != null && !IsEditing)
            {
                CellTextBox.Text = DisplayValue;
            }
        }

        private void UpdateValidationVisual()
        {
            if (CellBorder != null)
            {
                if (IsValid)
                {
                    CellBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    CellBorder.BorderThickness = new Thickness(0, 0, 1, 1);
                }
                else
                {
                    CellBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    CellBorder.BorderThickness = new Thickness(2);
                }
            }
        }

        private void UpdateSelectionVisual()
        {
            if (CellBorder != null)
            {
                if (IsSelected)
                {
                    CellBorder.Background = Application.Current.Resources["SystemAccentColorLight2"] as SolidColorBrush
                                          ?? new SolidColorBrush(Microsoft.UI.Colors.LightBlue);
                }
                else
                {
                    CellBorder.Background = Application.Current.Resources["LayerFillColorDefaultBrush"] as SolidColorBrush
                                          ?? new SolidColorBrush(Microsoft.UI.Colors.White);
                }
            }
        }

        private void StartEditing()
        {
            if (CellTextBox != null)
            {
                OriginalValue = Value;
                CellTextBox.Text = DisplayValue;
                CellTextBox.Focus(FocusState.Programmatic);
                CellTextBox.SelectAll();

                EditingStarted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void FinishEditing()
        {
            if (CellTextBox != null)
            {
                var newValue = CellTextBox.Text;
                if (TryConvertValue(newValue, out var convertedValue))
                {
                    Value = convertedValue;
                    OriginalValue = convertedValue;
                }

                EditingFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CancelEditing()
        {
            if (CellTextBox != null)
            {
                Value = OriginalValue;
                CellTextBox.Text = DisplayValue;

                EditingCancelled?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool TryConvertValue(object? inputValue, out object? convertedValue)
        {
            convertedValue = null;

            try
            {
                _logger.LogTrace("🔄 TryConvertValue START - InstanceId: {InstanceId}, Column: {ColumnName}, " +
                    "InputValue: '{InputValue}', InputType: {InputType}, TargetType: {TargetType}",
                    _cellInstanceId, ColumnName, inputValue, inputValue?.GetType().Name ?? "null", DataType.Name);

                if (inputValue == null)
                {
                    convertedValue = GetDefaultValueForType(DataType);
                    _logger.LogTrace("✅ Null input converted to default value: '{DefaultValue}'", convertedValue);
                    return true;
                }

                if (DataType.IsAssignableFrom(inputValue.GetType()))
                {
                    convertedValue = inputValue;
                    _logger.LogTrace("✅ Direct assignment - no conversion needed");
                    return true;
                }

                // Špeciálne konverzie
                if (DataType == typeof(string))
                {
                    convertedValue = inputValue.ToString();
                    _logger.LogTrace("✅ Converted to string: '{StringValue}'", convertedValue);
                    return true;
                }

                if (DataType == typeof(int) || DataType == typeof(int?))
                {
                    if (int.TryParse(inputValue.ToString(), out int intValue))
                    {
                        convertedValue = intValue;
                        _logger.LogTrace("✅ Converted to int: {IntValue}", intValue);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Failed to parse int from '{InputValue}'", inputValue);
                    }
                }

                if (DataType == typeof(decimal) || DataType == typeof(decimal?))
                {
                    if (decimal.TryParse(inputValue.ToString(), out decimal decValue))
                    {
                        convertedValue = decValue;
                        _logger.LogTrace("✅ Converted to decimal: {DecimalValue}", decValue);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Failed to parse decimal from '{InputValue}'", inputValue);
                    }
                }

                // Fallback konverzia
                convertedValue = Convert.ChangeType(inputValue, DataType);
                _logger.LogTrace("✅ Fallback conversion successful: '{ConvertedValue}'", convertedValue);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Type conversion FAILED - Column: {ColumnName}, " +
                    "InputValue: '{InputValue}', TargetType: {TargetType}",
                    ColumnName, inputValue, DataType.Name);
                return false;
            }
        }

        private static object? GetDefaultValueForType(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        #endregion

        #region Event Handlers

        private void OnCellKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("⌨️ CellKeyDown START - InstanceId: {InstanceId}, Key: {Key}, " +
                    "IsEditing: {IsEditing}, Column: {ColumnName}",
                    _cellInstanceId, e.Key, IsEditing, ColumnName);

                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Tab:
                        e.Handled = true;
                        IsEditing = false;
                        _logger.LogDebug("⌨️ Tab key - ending edit mode, Column: {ColumnName}", ColumnName);
                        // TODO: Navigate to next cell
                        break;

                    case Windows.System.VirtualKey.Enter:
                        e.Handled = true;
                        if (IsShiftPressed())
                        {
                            _logger.LogDebug("⌨️ Shift+Enter - inserting new line, Column: {ColumnName}", ColumnName);
                            InsertNewLine();
                        }
                        else
                        {
                            _logger.LogDebug("⌨️ Enter - ending edit mode, Column: {ColumnName}", ColumnName);
                            IsEditing = false;
                            // TODO: Navigate to cell below
                        }
                        break;

                    case Windows.System.VirtualKey.Escape:
                        e.Handled = true;
                        _logger.LogDebug("⌨️ Escape - canceling edit, Column: {ColumnName}", ColumnName);
                        IsEditing = false;
                        CancelEditing();
                        break;
                }

                _logger.LogTrace("✅ CellKeyDown COMPLETED - Key: {Key}, FinalEditState: {IsEditing}",
                    e.Key, IsEditing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellKeyDown - InstanceId: {InstanceId}, Key: {Key}",
                    _cellInstanceId, e.Key);
            }
        }

        private void OnCellLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("🎯 CellLostFocus - InstanceId: {InstanceId}, Column: {ColumnName}, " +
                    "WasEditing: {WasEditing}", _cellInstanceId, ColumnName, IsEditing);

                if (IsEditing)
                {
                    _editOperationCount++;
                    IsEditing = false;
                    _logger.LogDebug("📝 Edit mode ended on focus loss - Column: {ColumnName}, " +
                        "TotalEditOps: {EditOps}", ColumnName, _editOperationCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellLostFocus - InstanceId: {InstanceId}",
                    _cellInstanceId);
            }
        }

        private void OnCellGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("🎯 CellGotFocus - InstanceId: {InstanceId}, Column: {ColumnName}, " +
                    "CurrentValue: '{Value}'", _cellInstanceId, ColumnName, DisplayValue);

                IsEditing = true;
                _logger.LogDebug("📝 Edit mode started - Column: {ColumnName}", ColumnName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellGotFocus - InstanceId: {InstanceId}",
                    _cellInstanceId);
            }
        }

        private void OnCellTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (IsEditing && sender is TextBox textBox)
                {
                    var oldValue = OriginalValue;
                    var newValue = textBox.Text;
                    _valueChangeCount++;

                    _logger.LogTrace("📝 CellTextChanged - InstanceId: {InstanceId}, Column: {ColumnName}, " +
                        "OldValue: '{OldValue}' → NewValue: '{NewValue}', ChangeCount: {ChangeCount}",
                        _cellInstanceId, ColumnName, oldValue, newValue, _valueChangeCount);

                    ValueChanged?.Invoke(this, new CellValueChangedEventArgs(oldValue, newValue));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellTextChanged - InstanceId: {InstanceId}",
                    _cellInstanceId);
            }
        }

        private void InsertNewLine()
        {
            if (CellTextBox != null)
            {
                var currentPosition = CellTextBox.SelectionStart;
                var currentText = CellTextBox.Text;

                var newText = currentText.Insert(currentPosition, Environment.NewLine);
                CellTextBox.Text = newText;
                CellTextBox.SelectionStart = currentPosition + Environment.NewLine.Length;
            }
        }

        private static bool IsShiftPressed()
        {
            var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
            return (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Začne editáciu bunky
        /// </summary>
        public void StartEditingMode()
        {
            IsEditing = true;
        }

        /// <summary>
        /// Dokončí editáciu bunky (potvrdi zmeny)
        /// </summary>
        public void FinishEditingMode()
        {
            IsEditing = false;
        }

        /// <summary>
        /// Zruší editáciu bunky (vráti pôvodnú hodnotu)
        /// </summary>
        public void CancelEditingMode()
        {
            if (IsEditing)
            {
                CancelEditing();
                IsEditing = false;
            }
        }

        /// <summary>
        /// Vyčisti bunku (vráti na default hodnoty)
        /// </summary>
        public void Clear()
        {
            Value = GetDefaultValueForType(DataType);
            OriginalValue = Value;
            IsValid = true;
            ValidationErrors = string.Empty;
            IsSelected = false;
            IsEditing = false;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region ToString Override

        public override string ToString()
        {
            return $"Cell[{ColumnName}] = '{DisplayValue}' (Valid: {IsValid})";
        }

        #endregion
    }

    /// <summary>
    /// Event args pre ValueChanged event
    /// </summary>
    public class CellValueChangedEventArgs : EventArgs
    {
        public object? OldValue { get; }
        public object? NewValue { get; }

        public CellValueChangedEventArgs(object? oldValue, object? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}