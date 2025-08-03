// Controls/SpecialColumns/CheckBoxColumn.xaml.cs - ✅ NOVÝ: CheckBox Column Component  
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Controls.SpecialColumns
{
    /// <summary>
    /// CheckBox column pre označovanie riadkov - ✅ NOVÝ COMPONENT
    /// </summary>
    public sealed partial class CheckBoxColumn : UserControl
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        // Row identifikácia
        private int _rowIndex = -1;
        private string _rowId = string.Empty;
        
        // Parent reference pre komunikáciu
        private AdvancedDataGrid? _parentGrid;
        
        // State tracking
        private bool _isInitialized = false;
        private bool _suppressEvents = false;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri zmene checkbox stavu
        /// </summary>
        public event EventHandler<CheckBoxStateChangedEventArgs>? CheckBoxStateChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Aktuálny stav checkboxu
        /// </summary>
        public bool IsChecked
        {
            get => CellCheckBox.IsChecked == true;
            set
            {
                _suppressEvents = true;
                CellCheckBox.IsChecked = value;
                _suppressEvents = false;
            }
        }

        /// <summary>
        /// Index riadku
        /// </summary>
        public int RowIndex
        {
            get => _rowIndex;
            set => _rowIndex = value;
        }

        /// <summary>
        /// Unique ID riadku
        /// </summary>
        public string RowId
        {
            get => _rowId;
            set => _rowId = value;
        }

        /// <summary>
        /// Parent grid reference
        /// </summary>
        public AdvancedDataGrid? ParentGrid
        {
            get => _parentGrid;
            set => _parentGrid = value;
        }

        #endregion

        #region Constructor

        public CheckBoxColumn()
        {
            _logger = NullLogger.Instance;
            this.InitializeComponent();
            
            _logger.LogDebug("☑️ CheckBoxColumn created - InstanceId: {InstanceId}", _componentInstanceId);
            
            // Inicializácia
            InitializeCheckBoxColumn();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje CheckBox column
        /// </summary>
        private void InitializeCheckBoxColumn()
        {
            try
            {
                _logger.LogDebug("☑️ InitializeCheckBoxColumn START - InstanceId: {InstanceId}", _componentInstanceId);
                
                // Nastavenie štýlov
                SetDefaultStyles();
                
                // Nastavenie accessibility
                SetAccessibilityProperties();
                
                _isInitialized = true;
                
                _logger.LogDebug("✅ CheckBoxColumn initialized - InstanceId: {InstanceId}", _componentInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeCheckBoxColumn - InstanceId: {InstanceId}", _componentInstanceId);
            }
        }

        /// <summary>
        /// Nastavuje predvolené štýly
        /// </summary>
        private void SetDefaultStyles()
        {
            // CheckBox styling
            CellCheckBox.MinWidth = 20;
            CellCheckBox.MinHeight = 20;
            
            // Container styling
            CheckBoxContainer.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        /// <summary>
        /// Nastavuje accessibility properties
        /// </summary>
        private void SetAccessibilityProperties()
        {
            // Accessibility
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(CellCheckBox, "Row selection checkbox");
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetHelpText(CellCheckBox, 
                "Check to select this row for operations like delete or export");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler pre checkbox Checked event
        /// </summary>
        private void OnCellCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents) return;
            
            try
            {
                _logger.LogDebug("☑️ CheckBox CHECKED - RowIndex: {RowIndex}, RowId: {RowId}, InstanceId: {InstanceId}",
                    _rowIndex, _rowId, _componentInstanceId);
                
                // Notify parent grid
                OnCheckBoxStateChanged(true);
                
                // Update data in parent grid
                UpdateParentGridData(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellCheckBoxChecked - RowIndex: {RowIndex}, InstanceId: {InstanceId}",
                    _rowIndex, _componentInstanceId);
            }
        }

        /// <summary>
        /// Handler pre checkbox Unchecked event
        /// </summary>
        private void OnCellCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents) return;
            
            try
            {
                _logger.LogDebug("☐ CheckBox UNCHECKED - RowIndex: {RowIndex}, RowId: {RowId}, InstanceId: {InstanceId}",
                    _rowIndex, _rowId, _componentInstanceId);
                
                // Notify parent grid
                OnCheckBoxStateChanged(false);
                
                // Update data in parent grid
                UpdateParentGridData(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellCheckBoxUnchecked - RowIndex: {RowIndex}, InstanceId: {InstanceId}",
                    _rowIndex, _componentInstanceId);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Nastavuje checkbox state bez triggerovanie eventov
        /// </summary>
        public void SetCheckStateQuietly(bool isChecked)
        {
            try
            {
                _suppressEvents = true;
                CellCheckBox.IsChecked = isChecked;
                _suppressEvents = false;
                
                _logger.LogTrace("☑️ SetCheckStateQuietly - RowIndex: {RowIndex}, IsChecked: {IsChecked}",
                    _rowIndex, isChecked);
            }
            catch (Exception ex)
            {
                _suppressEvents = false;
                _logger.LogError(ex, "❌ ERROR in SetCheckStateQuietly - RowIndex: {RowIndex}", _rowIndex);
            }
        }

        /// <summary>
        /// Toggluje checkbox state
        /// </summary>
        public void ToggleCheckState()
        {
            try
            {
                var newState = !IsChecked;
                CellCheckBox.IsChecked = newState;
                
                _logger.LogDebug("🔄 ToggleCheckState - RowIndex: {RowIndex}, NewState: {NewState}",
                    _rowIndex, newState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ToggleCheckState - RowIndex: {RowIndex}", _rowIndex);
            }
        }

        /// <summary>
        /// Resetuje checkbox na unchecked
        /// </summary>
        public void ResetCheckState()
        {
            SetCheckStateQuietly(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Vyvolá CheckBoxStateChanged event
        /// </summary>
        private void OnCheckBoxStateChanged(bool isChecked)
        {
            var args = new CheckBoxStateChangedEventArgs
            {
                RowIndex = _rowIndex,
                RowId = _rowId,
                IsChecked = isChecked,
                Timestamp = DateTime.UtcNow
            };
            
            CheckBoxStateChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Aktualizuje dáta v parent grid
        /// </summary>
        private void UpdateParentGridData(bool isChecked)
        {
            try
            {
                if (_parentGrid != null && _rowIndex >= 0)
                {
                    _parentGrid.UpdateCheckBoxState(_rowIndex, isChecked);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateParentGridData - RowIndex: {RowIndex}", _rowIndex);
            }
        }

        #endregion
    }

    /// <summary>
    /// Event args pre CheckBox state zmeny
    /// </summary>
    public class CheckBoxStateChangedEventArgs : EventArgs
    {
        public int RowIndex { get; set; } = -1;
        public string RowId { get; set; } = string.Empty;
        public bool IsChecked { get; set; } = false;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}