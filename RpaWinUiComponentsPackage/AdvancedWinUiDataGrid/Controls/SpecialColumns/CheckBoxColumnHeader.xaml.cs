// Controls/SpecialColumns/CheckBoxColumnHeader.xaml.cs - ✅ NOVÝ: CheckBox Column Header Component
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Controls.SpecialColumns
{
    /// <summary>
    /// Header pre CheckBox column s Check All/Uncheck All funkcionalitou - ✅ NOVÝ COMPONENT
    /// </summary>
    public sealed partial class CheckBoxColumnHeader : UserControl
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        // Parent reference
        private AdvancedDataGrid? _parentGrid;
        
        // State tracking
        private bool _isUpdatingState = false;
        private CheckBoxHeaderState _currentState = CheckBoxHeaderState.Unchecked;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri Check All operácii
        /// </summary>
        public event EventHandler<CheckAllEventArgs>? CheckAllRequested;

        /// <summary>
        /// Event vyvolaný pri Uncheck All operácii
        /// </summary>
        public event EventHandler<CheckAllEventArgs>? UncheckAllRequested;

        #endregion

        #region Properties

        /// <summary>
        /// Aktuálny stav header checkboxu
        /// </summary>
        public CheckBoxHeaderState HeaderState
        {
            get => _currentState;
            set => SetHeaderState(value);
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

        public CheckBoxColumnHeader()
        {
            _logger = NullLogger.Instance;
            this.InitializeComponent();
            
            _logger.LogDebug("☑️ CheckBoxColumnHeader created - InstanceId: {InstanceId}", _componentInstanceId);
            
            // Inicializácia
            InitializeHeader();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje header
        /// </summary>
        private void InitializeHeader()
        {
            try
            {
                _logger.LogDebug("☑️ InitializeHeader START - InstanceId: {InstanceId}", _componentInstanceId);
                
                // Nastavenie accessibility
                SetAccessibilityProperties();
                
                // Nastavenie počiatočného stavu
                SetHeaderState(CheckBoxHeaderState.Unchecked);
                
                _logger.LogDebug("✅ CheckBoxColumnHeader initialized - InstanceId: {InstanceId}", _componentInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeHeader - InstanceId: {InstanceId}", _componentInstanceId);
            }
        }

        /// <summary>
        /// Nastavuje accessibility properties
        /// </summary>
        private void SetAccessibilityProperties()
        {
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(HeaderCheckBox, "Select all rows");
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetHelpText(HeaderCheckBox, 
                "Check to select all rows, uncheck to deselect all rows");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler pre header checkbox Checked event
        /// </summary>
        private void OnHeaderCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingState) return;
            
            try
            {
                _logger.LogInformation("☑️ Header CheckBox CHECKED - Check All requested, InstanceId: {InstanceId}",
                    _componentInstanceId);
                
                _currentState = CheckBoxHeaderState.Checked;
                
                // Notify listeners
                var args = new CheckAllEventArgs
                {
                    Operation = CheckAllOperation.CheckAll,
                    Timestamp = DateTime.UtcNow
                };
                
                CheckAllRequested?.Invoke(this, args);
                
                // Notify parent grid
                if (_parentGrid != null)
                {
                    _parentGrid.CheckAllRows();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnHeaderCheckBoxChecked - InstanceId: {InstanceId}", _componentInstanceId);
            }
        }

        /// <summary>
        /// Handler pre header checkbox Unchecked event
        /// </summary>
        private void OnHeaderCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingState) return;
            
            try
            {
                _logger.LogInformation("☐ Header CheckBox UNCHECKED - Uncheck All requested, InstanceId: {InstanceId}",
                    _componentInstanceId);
                
                _currentState = CheckBoxHeaderState.Unchecked;
                
                // Notify listeners
                var args = new CheckAllEventArgs
                {
                    Operation = CheckAllOperation.UncheckAll,
                    Timestamp = DateTime.UtcNow
                };
                
                UncheckAllRequested?.Invoke(this, args);
                
                // Notify parent grid
                if (_parentGrid != null)
                {
                    _parentGrid.UncheckAllRows();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnHeaderCheckBoxUnchecked - InstanceId: {InstanceId}", _componentInstanceId);
            }
        }

        /// <summary>
        /// Handler pre header checkbox Indeterminate event
        /// </summary>
        private void OnHeaderCheckBoxIndeterminate(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingState) return;
            
            try
            {
                _logger.LogDebug("⬜ Header CheckBox INDETERMINATE - Mixed state, InstanceId: {InstanceId}",
                    _componentInstanceId);
                
                _currentState = CheckBoxHeaderState.Indeterminate;
                
                // V indeterminate stave klikneme -> check all
                _isUpdatingState = true;
                HeaderCheckBox.IsChecked = true;
                _isUpdatingState = false;
                
                // Trigger check all
                OnHeaderCheckBoxChecked(sender, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnHeaderCheckBoxIndeterminate - InstanceId: {InstanceId}", _componentInstanceId);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Nastavuje stav header checkboxu bez triggerovanie eventov
        /// </summary>
        public void SetHeaderState(CheckBoxHeaderState state)
        {
            try
            {
                _isUpdatingState = true;
                _currentState = state;
                
                switch (state)
                {
                    case CheckBoxHeaderState.Checked:
                        HeaderCheckBox.IsChecked = true;
                        break;
                    case CheckBoxHeaderState.Unchecked:
                        HeaderCheckBox.IsChecked = false;
                        break;
                    case CheckBoxHeaderState.Indeterminate:
                        HeaderCheckBox.IsChecked = null;
                        break;
                }
                
                _isUpdatingState = false;
                
                _logger.LogTrace("☑️ SetHeaderState - State: {State}, InstanceId: {InstanceId}",
                    state, _componentInstanceId);
            }
            catch (Exception ex)
            {
                _isUpdatingState = false;
                _logger.LogError(ex, "❌ ERROR in SetHeaderState - State: {State}, InstanceId: {InstanceId}",
                    state, _componentInstanceId);
            }
        }

        /// <summary>
        /// Aktualizuje header stav na základe počtu checked riadkov
        /// </summary>
        public void UpdateHeaderState(int totalRows, int checkedRows)
        {
            try
            {
                if (totalRows == 0)
                {
                    SetHeaderState(CheckBoxHeaderState.Unchecked);
                }
                else if (checkedRows == 0)
                {
                    SetHeaderState(CheckBoxHeaderState.Unchecked);
                }
                else if (checkedRows == totalRows)
                {
                    SetHeaderState(CheckBoxHeaderState.Checked);
                }
                else
                {
                    SetHeaderState(CheckBoxHeaderState.Indeterminate);
                }
                
                _logger.LogTrace("☑️ UpdateHeaderState - TotalRows: {TotalRows}, CheckedRows: {CheckedRows}, " +
                    "NewState: {NewState}", totalRows, checkedRows, _currentState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateHeaderState - TotalRows: {TotalRows}, CheckedRows: {CheckedRows}",
                    totalRows, checkedRows);
            }
        }

        #endregion
    }

    /// <summary>
    /// Stav header checkboxu
    /// </summary>
    public enum CheckBoxHeaderState
    {
        Unchecked,
        Checked,
        Indeterminate
    }

    /// <summary>
    /// Typ Check All operácie
    /// </summary>
    public enum CheckAllOperation
    {
        CheckAll,
        UncheckAll
    }

    /// <summary>
    /// Event args pre Check All operácie
    /// </summary>
    public class CheckAllEventArgs : EventArgs
    {
        public CheckAllOperation Operation { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}