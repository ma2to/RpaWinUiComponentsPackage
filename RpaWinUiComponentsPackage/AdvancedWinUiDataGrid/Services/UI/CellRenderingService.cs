// Services/UI/CellRenderingService.cs - ✅ NOVÝ: Service pre rendering buniek
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.UI
{
    /// <summary>
    /// Service pre rendering DataGrid buniek - INTERNAL
    /// Zodpovedný za cell background, borders, text formatting, selection visual states
    /// </summary>
    internal class CellRenderingService
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        private DataGridColorConfig? _colorConfig;
        private bool _enableZebraRows = true;
        private bool _isInitialized = false;

        #endregion

        #region Constructor

        public CellRenderingService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("🎨 CellRenderingService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Service je inicializovaný
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Aktuálna color konfigurácia
        /// </summary>
        public DataGridColorConfig? ColorConfig => _colorConfig;

        /// <summary>
        /// Zebra rows sú povolené
        /// </summary>
        public bool EnableZebraRows
        {
            get => _enableZebraRows;
            set
            {
                if (_enableZebraRows != value)
                {
                    _enableZebraRows = value;
                    _logger.LogDebug("🦓 Zebra rows toggled: {Enabled}", value);
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje service s color konfiguráciou
        /// </summary>
        public async Task InitializeAsync(DataGridColorConfig? colorConfig = null, bool enableZebraRows = true)
        {
            try
            {
                _logger.LogInformation("🎨 CellRenderingService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _colorConfig = colorConfig ?? DataGridColorConfig.CreateDefault();
                _enableZebraRows = enableZebraRows;

                _isInitialized = true;

                _logger.LogInformation("✅ CellRenderingService INITIALIZED - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CellRenderingService.InitializeAsync - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
                throw;
            }
        }

        #endregion

        #region Cell Background Rendering

        /// <summary>
        /// Získa background brush pre bunku na základe jej stavu
        /// </summary>
        public SolidColorBrush GetCellBackgroundBrush(CellState cellState, int rowIndex = -1)
        {
            try
            {
                var defaultColor = _colorConfig?.ResolvedCellBackgroundColor ?? Colors.White;

                // Priority: Copied > Focused > Selected > Validation Error > Zebra > Default
                if (cellState.IsCopied)
                {
                    var copiedColor = _colorConfig?.ResolvedCopiedCellColor ?? 
                        Windows.UI.Color.FromArgb(60, 34, 139, 34);
                    return new SolidColorBrush(copiedColor);
                }

                if (cellState.IsFocused)
                {
                    var focusedColor = _colorConfig?.ResolvedFocusedCellColor ?? 
                        Windows.UI.Color.FromArgb(80, 0, 120, 215);
                    return new SolidColorBrush(focusedColor);
                }

                if (cellState.IsSelected)
                {
                    var selectedColor = _colorConfig?.ResolvedSelectionColor ?? 
                        Windows.UI.Color.FromArgb(100, 0, 120, 215);
                    return new SolidColorBrush(selectedColor);
                }

                // Validation error background (light red)
                if (!cellState.IsValid && cellState.HasValidationErrors)
                {
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(30, 255, 0, 0));
                }

                // Zebra rows
                if (_enableZebraRows && rowIndex >= 0 && rowIndex % 2 == 1)
                {
                    var zebraColor = _colorConfig?.ResolvedZebraRowColor ?? 
                        Windows.UI.Color.FromArgb(20, 128, 128, 128);
                    return new SolidColorBrush(zebraColor);
                }

                return new SolidColorBrush(defaultColor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting cell background brush");
                return new SolidColorBrush(Colors.White);
            }
        }

        /// <summary>
        /// Získa border brush pre bunku na základe jej stavu
        /// </summary>
        public SolidColorBrush GetCellBorderBrush(CellState cellState)
        {
            try
            {
                // Validation error má červený border
                if (!cellState.IsValid && cellState.HasValidationErrors)
                {
                    var errorColor = _colorConfig?.ResolvedValidationErrorBorderColor ?? Colors.Red;
                    return new SolidColorBrush(errorColor);
                }

                // Focused cell má modrý border
                if (cellState.IsFocused)
                {
                    var focusedColor = _colorConfig?.ResolvedFocusedCellColor ?? 
                        Windows.UI.Color.FromArgb(255, 0, 120, 215);
                    return new SolidColorBrush(focusedColor);
                }

                // Default border
                var defaultColor = _colorConfig?.ResolvedCellBorderColor ?? Colors.LightGray;
                return new SolidColorBrush(defaultColor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting cell border brush");
                return new SolidColorBrush(Colors.LightGray);
            }
        }

        /// <summary>
        /// Získa border thickness pre bunku na základe jej stavu
        /// </summary>
        public Microsoft.UI.Xaml.Thickness GetCellBorderThickness(CellState cellState)
        {
            try
            {
                // Validation error alebo focused cell má hrubší border
                if ((!cellState.IsValid && cellState.HasValidationErrors) || cellState.IsFocused)
                {
                    return new Microsoft.UI.Xaml.Thickness(2);
                }

                // Default thickness
                return new Microsoft.UI.Xaml.Thickness(1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting cell border thickness");
                return new Microsoft.UI.Xaml.Thickness(1);
            }
        }

        #endregion

        #region Text Formatting

        /// <summary>
        /// Získa text color pre bunku na základe jej stavu
        /// </summary>
        public SolidColorBrush GetCellTextBrush(CellState cellState)
        {
            try
            {
                // Validation error text
                if (!cellState.IsValid && cellState.HasValidationErrors)
                {
                    return new SolidColorBrush(Colors.DarkRed);
                }

                // Focused/Selected text
                if (cellState.IsFocused || cellState.IsSelected)
                {
                    var textColor = _colorConfig?.ResolvedFocusedTextColor ?? Colors.Black;
                    return new SolidColorBrush(textColor);
                }

                // Default text color
                var defaultColor = _colorConfig?.ResolvedTextColor ?? Colors.Black;
                return new SolidColorBrush(defaultColor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting cell text brush");
                return new SolidColorBrush(Colors.Black);
            }
        }

        /// <summary>
        /// Formátuje text v bunke podľa data type
        /// </summary>
        public string FormatCellText(object? value, Type dataType, string? customFormat = null)
        {
            try
            {
                if (value == null) return string.Empty;

                // Custom format má prioritu
                if (!string.IsNullOrEmpty(customFormat))
                {
                    if (value is IFormattable formattable)
                        return formattable.ToString(customFormat, null);
                }

                // Type-specific formatting
                return dataType.Name switch
                {
                    nameof(DateTime) when value is DateTime dt => dt.ToString("dd.MM.yyyy HH:mm"),
                    nameof(DateOnly) when value is DateOnly d => d.ToString("dd.MM.yyyy"),
                    nameof(TimeOnly) when value is TimeOnly t => t.ToString("HH:mm:ss"),
                    nameof(Decimal) when value is decimal dec => dec.ToString("N2"),
                    nameof(Double) when value is double dbl => dbl.ToString("N2"),
                    nameof(Single) when value is float flt => flt.ToString("N2"),
                    nameof(Boolean) when value is bool b => b ? "Áno" : "Nie",
                    _ => value.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error formatting cell text - Value: {Value}, Type: {Type}", 
                    value, dataType.Name);
                return value?.ToString() ?? string.Empty;
            }
        }

        #endregion

        #region Configuration Updates

        /// <summary>
        /// Aktualizuje color konfiguráciu
        /// </summary>
        public void UpdateColorConfiguration(DataGridColorConfig colorConfig)
        {
            _colorConfig = colorConfig ?? throw new ArgumentNullException(nameof(colorConfig));
            _logger.LogDebug("🎨 Color configuration updated");
        }

        /// <summary>
        /// Resetuje color konfiguráciu na default
        /// </summary>
        public void ResetColorConfiguration()
        {
            _colorConfig = DataGridColorConfig.CreateDefault();
            _logger.LogDebug("🔄 Color configuration reset to default");
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Pripraví rendering data pre batch buniek
        /// </summary>
        public async Task<Dictionary<string, object>> PrepareBatchRenderingDataAsync(
            List<CellState> cellStates, 
            int startRowIndex = 0)
        {
            try
            {
                _logger.LogDebug("🎨 Preparing batch rendering data - Cells: {Count}, StartRow: {StartRow}", 
                    cellStates.Count, startRowIndex);

                var renderingData = new Dictionary<string, object>();

                for (int i = 0; i < cellStates.Count; i++)
                {
                    var cellState = cellStates[i];
                    var rowIndex = startRowIndex + i;
                    var cellKey = $"cell_{rowIndex}";

                    renderingData[cellKey] = new
                    {
                        BackgroundBrush = GetCellBackgroundBrush(cellState, rowIndex),
                        BorderBrush = GetCellBorderBrush(cellState),
                        BorderThickness = GetCellBorderThickness(cellState),
                        TextBrush = GetCellTextBrush(cellState)
                    };
                }

                await Task.CompletedTask;
                return renderingData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error preparing batch rendering data");
                return new Dictionary<string, object>();
            }
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var colorConfigType = _colorConfig?.GetType().Name ?? "None";
            return $"CellRenderingService[{_serviceInstanceId}] - " +
                   $"Initialized: {_isInitialized}, ZebraRows: {_enableZebraRows}, ColorConfig: {colorConfigType}";
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Stav bunky pre rendering
    /// </summary>
    internal class CellState
    {
        public bool IsSelected { get; set; }
        public bool IsFocused { get; set; }
        public bool IsCopied { get; set; }
        public bool IsValid { get; set; } = true;
        public bool HasValidationErrors { get; set; }
        public object? Value { get; set; }
        public string? FormattedValue { get; set; }
    }

    #endregion
}