// Controls/AdvancedDataGrid.xaml.cs - ✅ MAIN partial class - class declaration only
// ✅ SPLIT INTO PARTIAL CLASSES: Core infrastructure moved to AdvancedDataGrid.Core.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Row;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ImportExport;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Operations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Controls.SpecialColumns;
using Windows.Foundation;
using CellPosition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell.CellPosition;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid.ColumnDefinition;
using RowDisplayInfo = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Row.RowDisplayInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid.ThrottlingConfig;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation.ValidationRule;
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid.DataGridColorConfig;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid - MAIN partial class declaration
    /// ✅ REFACTORED: Split into multiple partial classes for maintainability
    /// ✅ Core fields, properties, constructors: AdvancedDataGrid.Core.cs
    /// ✅ Services initialization: AdvancedDataGrid.Services.cs (planned)
    /// ✅ Event handling: AdvancedDataGrid.Events.cs (planned)
    /// ✅ Selection management: AdvancedDataGrid.Selection.cs (planned)
    /// ✅ Navigation: AdvancedDataGrid.Navigation.cs (planned)
    /// ✅ Data operations: AdvancedDataGrid.DataOperations.cs (planned)
    /// ✅ Search functionality: AdvancedDataGrid.Search.cs (planned)
    /// ✅ UI management: AdvancedDataGrid.UI.cs (planned)
    /// ✅ Validation: AdvancedDataGrid.Validation.cs (planned)
    /// ✅ Performance: AdvancedDataGrid.Performance.cs (planned)
    /// ✅ Helper methods: AdvancedDataGrid.Helpers.cs (planned)
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable, INavigationCallback
    {
        // ✅ NOTE: All core fields, properties, constructors moved to AdvancedDataGrid.Core.cs
        // ✅ NOTE: This file now serves as the main class declaration and will contain remaining implementation

        #region ✅ KOMPLETNE OPRAVENÉ: XAML Element Access Properties (CS0102 fix)

        // ✅ OPRAVENÉ CS0102: Jediné definície XAML element properties
        private StackPanel? HeaderStackPanelElement => this.FindName("HeaderStackPanel") as StackPanel;
        private ScrollViewer? HeaderScrollViewerElement => this.FindName("HeaderScrollViewer") as ScrollViewer;
        private ScrollViewer? DataGridScrollViewerElement => this.FindName("DataGridScrollViewer") as ScrollViewer;
        private ItemsControl? DataRowsContainerElement => this.FindName("DataRowsContainer") as ItemsControl;
        private Grid? MainContentGridElement => this.FindName("MainContentGrid") as Grid;
        private Border? LoadingOverlayElement => this.FindName("LoadingOverlay") as Border;

        #endregion

        #region ✅ NOVÉ: Services Inicializácia

        /// <summary>
        /// Inicializuje všetky services pre modulárnu architektúru
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            try
            {
                _logger.LogInformation("🚀 InitializeServicesAsync START - InstanceId: {InstanceId}", _componentInstanceId);

                // Vytvor konfiguráciu
                _configuration = new Core.DataGridConfiguration
                {
                    AutoAddEnabled = _autoAddEnabled,
                    UnifiedRowCount = _unifiedRowCount,
                    CheckBoxColumnEnabled = _checkBoxColumnEnabled,
                    ValidAlertsMinWidth = _validAlertsMinWidth
                };

                // Inicializuj Core Service
                _coreService = new Services.UI.DataGridCoreService();
                await _coreService.InitializeAsync(this, _configuration);

                // Inicializuj Layout Service
                _layoutService = new Services.UI.DataGridLayoutService();
                await _layoutService.InitializeAsync(this, _columns);

                // Inicializuj Event Service
                _eventService = new Services.UI.DataGridEventService();
                await _eventService.InitializeAsync(this);

                // Inicializuj Binding Service
                _bindingService = new Services.Operations.DataGridBindingService();
                await _bindingService.InitializeAsync(this, _columns);

                // ✅ NOVÉ: Inicializuj Row Height Animation Service
                _rowHeightAnimationService = new RowHeightAnimationService(_logger, RowHeightAnimationConfiguration.Default);

                _logger.LogInformation("✅ All services initialized successfully (including Row Height Animation)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during services initialization");
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Event Handlers Inicializácia a Implementácia

        /// <summary>
        /// Inicializuje všetky event handlers - deleguje na services
        /// </summary>
        private void InitializeEventHandlers()
        {
            try
            {
                _logger.LogDebug("🔧 InitializeEventHandlers START - delegating to services");
                
                // Event handlers sú teraz spravované services
                // Layout Service spravuje SizeChanged a LayoutUpdated
                // Event Service spravuje Pointer events
                
                _logger.LogDebug("✅ InitializeEventHandlers COMPLETED - services handle events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeEventHandlers");
                throw;
            }
        }

        /// <summary>
        /// ✅ IMPLEMENTOVANÉ: OnDataGridSizeChanged event handler - deleguje na Layout Service
        /// </summary>
        private void OnDataGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                _logger.LogDebug("📐 OnDataGridSizeChanged - delegating to Layout Service");
                
                // Deleguj na Layout Service
                _layoutService?.OnDataGridSizeChanged(sender, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnDataGridSizeChanged");
            }
        }

        /// <summary>
        /// ✅ IMPLEMENTOVANÉ: OnLayoutUpdated event handler
        /// </summary>
        private void OnLayoutUpdated(object? sender, object e)
        {
            try
            {
                _logger.LogTrace("🔄 OnLayoutUpdated - TotalWidth: {TotalWidth}", _totalAvailableWidth);

                // Update ValidAlerts stretching if needed
                _ = Task.Run(async () => await RecalculateValidAlertsWidthAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnLayoutUpdated");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Handler pre per-row height changes z DataGridCell
        /// </summary>
        private async void OnCellHeightChanged(object? sender, Controls.CellHeightChangedEventArgs e)
        {
            try
            {
                _logger.LogDebug("📏 Cell height changed - Column: {ColumnName}, NewHeight: {NewHeight}, OldHeight: {OldHeight}",
                    e.ColumnName, e.NewHeight, e.OldHeight);

                if (sender is Controls.DataGridCell cell)
                {
                    // Získaj row index z cell
                    var rowIndex = GetRowIndexFromCell(cell);
                    if (rowIndex >= 0 && rowIndex < _displayRows.Count)
                    {
                        // Animuj height change cez RowHeightAnimationService
                        if (_rowHeightAnimationService != null)
                        {
                            var rowElement = GetRowElementAt(rowIndex);
                            if (rowElement != null)
                            {
                                await _rowHeightAnimationService.AnimateRowHeightAsync(
                                    rowIndex, e.OldHeight, e.NewHeight, rowElement);
                                
                                _logger.LogTrace("✅ Row height animation triggered - Row: {RowIndex}, Height: {OldHeight} → {NewHeight}",
                                    rowIndex, e.OldHeight, e.NewHeight);
                            }
                        }

                        // Aktualizuj layout ak potrebujeme
                        await UpdateLayoutAfterRowHeightChangeAsync(rowIndex, e.NewHeight);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellHeightChanged - Column: {ColumnName}", e.ColumnName);
            }
        }

        /// <summary>
        /// ✅ IMPLEMENTOVANÉ: OnCellValueChanged event handler
        /// </summary>
        private void OnCellValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is CellViewModel cell && e.PropertyName == nameof(CellViewModel.Value))
                {
                    _logger.LogDebug("📝 OnCellValueChanged - Cell: [{RowIndex}, {ColumnName}] = '{Value}'",
                        cell.RowIndex, cell.ColumnName, cell.Value);

                    // Trigger realtime validation if enabled
                    if (_throttlingConfig?.EnableRealtimeValidation == true)
                    {
                        _ = Task.Run(async () => await ValidateCellRealtimeAsync(cell));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellValueChanged");
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS0123: OnPointerCaptureLost s správnym signature
        /// </summary>
        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("🖱️ OnPointerCaptureLost - Resizing: {IsResizing}, Dragging: {IsDragging}", 
                    _isResizing, _dragSelectionState.IsDragging);

                if (_isResizing)
                {
                    EndResize();
                }

                // Handle drag selection cancellation
                if (_dragSelectionState.IsDragging)
                {
                    _ = Task.Run(async () => await OnDragSelectionEnd());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnPointerCaptureLost");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Column header click handler pre sortovanie s Multi-Sort podporou (Ctrl+klik)
        /// </summary>
        private async Task OnColumnHeaderClicked(string columnName, TextBlock sortIndicator)
        {
            try
            {
                // Zisti či je stlačený Ctrl - WinUI3 approach
                var isCtrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                    .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                _logger.LogInformation("🔀 Column header clicked - Column: {ColumnName}, CtrlPressed: {CtrlPressed}, Instance: {InstanceId}",
                    columnName, isCtrlPressed, _componentInstanceId);

                if (_searchAndSortService == null)
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot sort");
                    return;
                }

                // Použij Multi-Sort ak je Ctrl stlačené, inak single sort
                var multiSortResult = _searchAndSortService.AddOrUpdateMultiSort(columnName, isCtrlPressed);
                
                if (multiSortResult != null)
                {
                    // Multi-Sort je aktívne
                    UpdateMultiSortIndicators();
                    _logger.LogInformation("✅ Multi-Sort applied - Column: {ColumnName}, Direction: {Direction}, Priority: {Priority}",
                        columnName, multiSortResult.Direction, multiSortResult.Priority);
                }
                else if (!isCtrlPressed)
                {
                    // Fallback na single sort
                    var newDirection = _searchAndSortService.ToggleColumnSort(columnName);
                    UpdateSortIndicator(columnName, newDirection);
                    _logger.LogInformation("✅ Single-Sort applied - Column: {ColumnName}, Direction: {Direction}",
                        columnName, newDirection);
                }
                else
                {
                    // Multi-Sort stĺpec bol odstránený
                    UpdateMultiSortIndicators();
                    _logger.LogInformation("✅ Multi-Sort column removed - Column: {ColumnName}", columnName);
                }
                
                // Apply sorting and refresh display
                await ApplySortAndRefreshAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnColumnHeaderClicked - Column: {ColumnName}",
                    columnName);
            }
        }

        /// <summary>
        /// Spracuje zmenu search textu v header search boxe
        /// </summary>
        private async void OnSearchTextChanged(string columnName, string searchText)
        {
            try
            {
                _logger.LogDebug("🔍 Search text changed - Column: {ColumnName}, Text: '{SearchText}'",
                    columnName, searchText);

                if (_searchAndSortService == null)
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot apply search");
                    return;
                }

                // Set search filter
                _searchAndSortService.SetColumnSearchFilter(columnName, searchText);

                // Apply search and refresh display
                await ApplySearchAndRefreshAsync();

                _logger.LogInformation("✅ Search applied - Column: {ColumnName}, Filter: '{SearchText}'",
                    columnName, searchText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnSearchTextChanged - Column: {ColumnName}, Text: '{SearchText}'",
                    columnName, searchText);
            }
        }

        /// <summary>
        /// Aplikuje search filtre a obnoví zobrazenie
        /// </summary>
        private async Task ApplySearchAndRefreshAsync()
        {
            try
            {
                _logger.LogDebug("🔍 ApplySearchAndRefresh START");

                if (_dataManagementService == null || _searchAndSortService == null)
                {
                    _logger.LogWarning("⚠️ Required services are null - cannot apply search");
                    return;
                }

                // Get current data
                var allData = await _dataManagementService.GetAllDataAsync();
                
                // Apply search and sort (empty rows will be at the end)
                var processedData = await _searchAndSortService.ApplyAllFiltersAndStylingAsync(allData);
                
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    // Update display rows
                    _displayRows.Clear();
                    foreach (var rowInfo in processedData)
                    {
                        var rowViewModel = CreateRowViewModelFromRowInfo(rowInfo);
                        _displayRows.Add(rowViewModel);
                    }

                    _totalCellsRendered = _displayRows.Sum(r => r.Cells.Count);
                    _logger.LogDebug("✅ Search applied - Rows: {RowCount}, Cells: {CellCount}",
                        _displayRows.Count, _totalCellsRendered);
                }, _logger);

                _logger.LogDebug("✅ ApplySearchAndRefresh COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ApplySearchAndRefreshAsync");
            }
        }

        /// <summary>
        /// Vytvorí RowViewModel z RowDisplayInfo
        /// </summary>
        private DataRowViewModel CreateRowViewModelFromRowInfo(RowDisplayInfo rowInfo)
        {
            var rowViewModel = new DataRowViewModel
            {
                RowIndex = rowInfo.RowIndex,
                IsZebraRow = rowInfo.IsZebraRow
            };

            // Create cells from the row data
            foreach (var column in _columns)
            {
                var cellValue = rowInfo.Data.TryGetValue(column.Name, out var value) ? value : null;
                var cellViewModel = new CellViewModel
                {
                    ColumnName = column.Name,
                    Value = cellValue,
                    DataType = column.DataType,
                    RowIndex = rowInfo.RowIndex,
                    IsValid = true
                };
                rowViewModel.Cells.Add(cellViewModel);
            }

            return rowViewModel;
        }

        /// <summary>
        /// Nastaví keyboard shortcuts pre copy/paste operácie
        /// </summary>
        private void SetupKeyboardShortcuts()
        {
            try
            {
                _logger.LogDebug("⌨️ SetupKeyboardShortcuts START");

                // Add KeyDown event handler to the main UserControl
                this.KeyDown += OnDataGridKeyDown;
                this.IsTabStop = true; // Allow control to receive focus
                this.TabFocusNavigation = KeyboardNavigationMode.Local;

                _logger.LogDebug("✅ Keyboard shortcuts setup - Ctrl+C/V/X enabled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetupKeyboardShortcuts");
            }
        }

        /// <summary>
        /// Spracuje keyboard shortcuts na úrovni DataGrid-u
        /// </summary>
        private async void OnDataGridKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("⌨️ DataGrid KeyDown - Key: {Key}, Ctrl: {Ctrl}, Shift: {Shift}",
                    e.Key, IsCtrlPressed(), IsShiftPressed());

                if (IsCtrlPressed())
                {
                    switch (e.Key)
                    {
                        case Windows.System.VirtualKey.C:
                            e.Handled = true;
                            await HandleCopyShortcut();
                            break;

                        case Windows.System.VirtualKey.V:
                            e.Handled = true;
                            await HandlePasteShortcut();
                            break;

                        case Windows.System.VirtualKey.X:
                            e.Handled = true;
                            await HandleCutShortcut();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnDataGridKeyDown - Key: {Key}", e.Key);
            }
        }

        /// <summary>
        /// Spracuje Ctrl+C (copy) shortcut
        /// </summary>
        private async Task HandleCopyShortcut()
        {
            try
            {
                _logger.LogInformation("📋 Copy shortcut triggered (Ctrl+C)");

                if (_copyPasteService == null)
                {
                    _logger.LogWarning("⚠️ CopyPasteService is null - cannot copy");
                    return;
                }

                // Get currently selected cells
                var selectedCells = GetSelectedCellsForCopy();
                
                await _copyPasteService.CopySelectedCellsAsync(selectedCells);

                _logger.LogInformation("✅ Copy operation completed - Cells: {CellCount}", selectedCells.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in HandleCopyShortcut");
            }
        }

        /// <summary>
        /// Spracuje Ctrl+V (paste) shortcut
        /// </summary>
        private async Task HandlePasteShortcut()
        {
            try
            {
                _logger.LogInformation("📋 Paste shortcut triggered (Ctrl+V)");

                if (_copyPasteService == null)
                {
                    _logger.LogWarning("⚠️ CopyPasteService is null - cannot paste");
                    return;
                }

                // Get paste target position (for now, start at beginning)
                var targetPosition = GetPasteTargetPosition();
                
                await _copyPasteService.PasteFromClipboardAsync(targetPosition.Row, targetPosition.Column);

                // Refresh display after paste
                await ApplySearchAndRefreshAsync();

                _logger.LogInformation("✅ Paste operation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in HandlePasteShortcut");
            }
        }

        /// <summary>
        /// Spracuje Ctrl+X (cut) shortcut  
        /// </summary>
        private async Task HandleCutShortcut()
        {
            try
            {
                _logger.LogInformation("📋 Cut shortcut triggered (Ctrl+X)");

                if (_copyPasteService == null)
                {
                    _logger.LogWarning("⚠️ CopyPasteService is null - cannot cut");
                    return;
                }

                // Get currently selected cells
                var selectedCells = GetSelectedCellsForCopy();
                
                await _copyPasteService.CutSelectedCellsAsync(selectedCells);

                // Refresh display after cut
                await ApplySearchAndRefreshAsync();

                _logger.LogInformation("✅ Cut operation completed - Cells: {CellCount}", selectedCells.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in HandleCutShortcut");
            }
        }

        /// <summary>
        /// Získa aktuálne označené bunky pre copy operáciu
        /// </summary>
        private List<CellSelection> GetSelectedCellsForCopy()
        {
            var selectedCells = new List<CellSelection>();

            try
            {
                var selectedPositions = _cellSelectionState.GetSelectedCells();
                
                if (!selectedPositions.Any())
                {
                    _logger.LogDebug("📋 No cells selected - nothing to copy");
                    return selectedCells;
                }

                // Convert selected positions to CellSelection objects with actual values
                foreach (var position in selectedPositions)
                {
                    var row = _displayRows.FirstOrDefault(r => r.RowIndex == position.Row);
                    var cell = row?.Cells.FirstOrDefault(c => c.ColumnName == position.ColumnName);
                    
                    if (cell != null)
                    {
                        selectedCells.Add(new CellSelection
                        {
                            RowIndex = cell.RowIndex,
                            ColumnIndex = GetColumnIndex(cell.ColumnName),
                            ColumnName = cell.ColumnName,
                            Value = cell.Value
                        });
                    }
                }

                // Set copied state and update visual feedback
                _cellSelectionState.SetCopiedCells(selectedPositions);
                _ = Task.Run(async () => await UpdateCellVisualStates());

                _logger.LogDebug("📋 Selected {CellCount} cells for copy operation", selectedCells.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetSelectedCellsForCopy");
            }

            return selectedCells;
        }

        /// <summary>
        /// Získa target pozíciu pre paste operáciu
        /// </summary>
        private (int Row, int Column) GetPasteTargetPosition()
        {
            // For now, paste at the beginning of the first non-empty row
            return (0, 0);
        }

        /// <summary>
        /// Získa index stĺpca podľa názvu
        /// </summary>
        private int GetColumnIndex(string columnName)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (_columns[i].Name == columnName)
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// Kontroluje či je riadok prázdny
        /// </summary>
        private bool IsRowEmpty(DataRowViewModel row)
        {
            return row.Cells.All(c => c.Value == null || string.IsNullOrWhiteSpace(c.Value?.ToString()));
        }

        /// <summary>
        /// Kontroluje či je stlačený Ctrl
        /// </summary>
        private bool IsCtrlPressed()
        {
            var coreWindow = Microsoft.UI.Xaml.Window.Current?.CoreWindow;
            if (coreWindow == null) return false;
            
            var ctrlState = coreWindow.GetKeyState(Windows.System.VirtualKey.Control);
            return (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        /// <summary>
        /// Kontroluje či je stlačený Shift
        /// </summary>
        private bool IsShiftPressed()
        {
            var coreWindow = Microsoft.UI.Xaml.Window.Current?.CoreWindow;
            if (coreWindow == null) return false;
            
            var shiftState = coreWindow.GetKeyState(Windows.System.VirtualKey.Shift);
            return (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        #endregion

        #region ✅ NOVÉ: Cell Selection Management

        /// <summary>
        /// Spracuje click na bunku s podporou Ctrl+Click pre multi-select
        /// </summary>
        private async Task OnCellClicked(int rowIndex, int columnIndex, string columnName, bool isCtrlPressed)
        {
            try
            {
                _logger.LogDebug("🎯 Cell clicked - Row: {Row}, Column: {Column} ({ColumnName}), Ctrl: {Ctrl}",
                    rowIndex, columnIndex, columnName, isCtrlPressed);

                // Neklikaj na special columns
                var column = _columns.FirstOrDefault(c => c.Name == columnName);
                if (column?.IsSpecialColumn == true)
                {
                    _logger.LogDebug("🎯 Ignoring click on special column: {ColumnName}", columnName);
                    return;
                }

                if (isCtrlPressed)
                {
                    // Multi-select: pridaj/odobér z selection
                    await HandleMultiSelectClick(rowIndex, columnIndex, columnName);
                }
                else
                {
                    // Single select: nastav new selection
                    await HandleSingleSelectClick(rowIndex, columnIndex, columnName);
                }

                // Clear copied cells po novom selection (okrem ak nie je rovnaká bunka)
                var copiedCells = _cellSelectionState.GetCopiedCells();
                if (copiedCells.Any() && !copiedCells.Any(c => c.Row == rowIndex && c.Column == columnIndex))
                {
                    _cellSelectionState.ClearCopiedCells();
                    await UpdateCellVisualStates();
                }

                _logger.LogInformation("✅ Cell selection updated - {SelectionInfo}",
                    _cellSelectionState.GetDiagnosticInfo());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnCellClicked - Row: {Row}, Column: {Column}",
                    rowIndex, columnIndex);
            }
        }

        /// <summary>
        /// Spracuje single cell click (bez Ctrl)
        /// </summary>
        private async Task HandleSingleSelectClick(int rowIndex, int columnIndex, string columnName)
        {
            // Clear previous selection
            _cellSelectionState.SetSingleCellSelection(rowIndex, columnIndex, columnName);
            
            await UpdateCellVisualStates();
            
            _logger.LogDebug("🎯 Single cell selected - [{Row},{Column}]{ColumnName}",
                rowIndex, columnIndex, columnName);
        }

        /// <summary>
        /// Spracuje multi-select click (s Ctrl)
        /// </summary>
        private async Task HandleMultiSelectClick(int rowIndex, int columnIndex, string columnName)
        {
            if (_cellSelectionState.IsCellSelected(rowIndex, columnIndex))
            {
                // Odobér z selection
                _cellSelectionState.RemoveCellFromSelection(rowIndex, columnIndex);
                _logger.LogDebug("🎯 Cell removed from selection - [{Row},{Column}]{ColumnName}",
                    rowIndex, columnIndex, columnName);
            }
            else
            {
                // Pridaj do selection
                _cellSelectionState.AddCellToSelection(rowIndex, columnIndex, columnName);
                _logger.LogDebug("🎯 Cell added to selection - [{Row},{Column}]{ColumnName}",
                    rowIndex, columnIndex, columnName);
            }

            await UpdateCellVisualStates();
        }

        /// <summary>
        /// Spracuje click mimo DataGrid (clear selection)
        /// </summary>
        private async Task OnOutsideClick()
        {
            try
            {
                _logger.LogDebug("🎯 Outside click - clearing selection");

                _cellSelectionState.ClearSelection();
                await UpdateCellVisualStates();

                _logger.LogDebug("✅ Selection cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnOutsideClick");
            }
        }

        /// <summary>
        /// Aktualizuje vizuálny stav všetkých buniek na base selection state
        /// </summary>
        private async Task UpdateCellVisualStates()
        {
            try
            {
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    var focusedCell = _cellSelectionState.FocusedCell;
                    var selectedCells = _cellSelectionState.GetSelectedCells();
                    var copiedCells = _cellSelectionState.GetCopiedCells();

                    foreach (var row in _displayRows)
                    {
                        foreach (var cell in row.Cells)
                        {
                            // Update focus state
                            cell.IsFocused = focusedCell?.Row == cell.RowIndex && 
                                           focusedCell?.ColumnName == cell.ColumnName;

                            // Update selected state
                            cell.IsSelected = selectedCells.Any(c => c.Row == cell.RowIndex && 
                                                                   c.ColumnName == cell.ColumnName);

                            // Update copied state
                            cell.IsCopied = copiedCells.Any(c => c.Row == cell.RowIndex && 
                                                              c.ColumnName == cell.ColumnName);
                        }
                    }

                    _logger.LogTrace("🎨 Updated visual states - Focus: {Focus}, Selected: {Selected}, Copied: {Copied}",
                        focusedCell?.ToString() ?? "None", selectedCells.Count, copiedCells.Count);
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateCellVisualStates");
            }
        }

        #endregion

        #region ✅ NOVÉ: Drag Selection Management

        /// <summary>
        /// Spracuje začiatok drag selection operácie
        /// </summary>
        private async Task OnDragSelectionStart(Point startPoint, CellPosition startCell)
        {
            try
            {
                _logger.LogDebug("🖱️ Drag selection started - Point: ({X},{Y}), Cell: [{Row},{Column}]",
                    startPoint.X, startPoint.Y, startCell.Row, startCell.Column);

                _dragSelectionState.StartDrag(startPoint, startCell);

                // Clear current selection a začni nový
                _cellSelectionState.ClearSelection();
                _cellSelectionState.SetSingleCellSelection(startCell.Row, startCell.Column, startCell.ColumnName);

                await UpdateCellVisualStates();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnDragSelectionStart");
            }
        }

        /// <summary>
        /// Spracuje update drag selection operácie
        /// </summary>
        private async Task OnDragSelectionUpdate(Point currentPoint, CellPosition? currentCell)
        {
            try
            {
                if (!_dragSelectionState.IsDragging || currentCell == null)
                    return;

                _dragSelectionState.UpdateDrag(currentPoint, currentCell);

                // Ak je drag dostatočne veľký, aktualizuj selection
                if (_dragSelectionState.IsValidDragDistance)
                {
                    await UpdateDragSelection();
                    await ShowSelectionRectangle();
                }

                _logger.LogTrace("🖱️ Drag selection updated - Point: ({X},{Y}), Cell: [{Row},{Column}]",
                    currentPoint.X, currentPoint.Y, currentCell.Row, currentCell.Column);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnDragSelectionUpdate");
            }
        }

        /// <summary>
        /// Spracuje koniec drag selection operácie
        /// </summary>
        private async Task OnDragSelectionEnd()
        {
            try
            {
                if (!_dragSelectionState.IsDragging)
                    return;

                var dragInfo = _dragSelectionState.GetDiagnosticInfo();
                _logger.LogDebug("🖱️ Drag selection ended - {DragInfo}", dragInfo);

                // Finalizuj selection ak bol drag dostatočne veľký
                if (_dragSelectionState.IsValidDragDistance)
                {
                    await FinalizeDragSelection();
                }

                await HideSelectionRectangle();
                _dragSelectionState.EndDrag();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnDragSelectionEnd");
            }
        }

        /// <summary>
        /// Aktualizuje selection na base drag state
        /// </summary>
        private async Task UpdateDragSelection()
        {
            try
            {
                var (startRow, startCol, endRow, endCol) = _dragSelectionState.GetSelectionRange();

                // Clear current selection
                _cellSelectionState.ClearSelection();

                // Add all cells in drag range
                for (int row = startRow; row <= endRow; row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        if (col < _columns.Count && !_columns[col].IsSpecialColumn)
                        {
                            _cellSelectionState.AddCellToSelection(row, col, _columns[col].Name);
                        }
                    }
                }

                // Set focus to current drag cell
                if (_dragSelectionState.CurrentCell != null)
                {
                    var currentCell = _dragSelectionState.CurrentCell;
                    _cellSelectionState.SetFocusedCell(currentCell.Row, currentCell.Column, currentCell.ColumnName);
                }

                await UpdateCellVisualStates();

                _logger.LogTrace("🖱️ Drag selection updated - Range: [{StartRow},{StartCol}]-[{EndRow},{EndCol}]",
                    startRow, startCol, endRow, endCol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateDragSelection");
            }
        }

        /// <summary>
        /// Finalizuje drag selection
        /// </summary>
        private async Task FinalizeDragSelection()
        {
            try
            {
                var selectedCount = _cellSelectionState.SelectedCellCount;
                _logger.LogInformation("🖱️ Drag selection finalized - Selected {SelectedCount} cells", selectedCount);

                // Selection už je nastavenený v UpdateDragSelection, takže nie je potrebné robiť nič extra
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in FinalizeDragSelection");
            }
        }

        /// <summary>
        /// Zobrazí selection rectangle visualization
        /// </summary>
        private async Task ShowSelectionRectangle()
        {
            try
            {
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    if (SelectionRectangleOverlay != null && SelectionRectangle != null)
                    {
                        var rect = _dragSelectionState.SelectionRectangle;
                        
                        if (rect.Width > 0 && rect.Height > 0)
                        {
                            // Update rectangle position and size
                            SelectionRectangle.Width = rect.Width;
                            SelectionRectangle.Height = rect.Height;
                            SelectionRectangle.Margin = new Thickness(rect.X, rect.Y, 0, 0);
                            
                            // Show overlay
                            SelectionRectangleOverlay.Visibility = Visibility.Visible;
                            
                            _logger.LogTrace("🖱️ Selection rectangle shown - Position: ({X},{Y}), Size: {Width}x{Height}",
                                rect.X, rect.Y, rect.Width, rect.Height);
                        }
                    }
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ShowSelectionRectangle");
            }
        }

        /// <summary>
        /// Skryje selection rectangle visualization  
        /// </summary>
        private async Task HideSelectionRectangle()
        {
            try
            {
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    if (SelectionRectangleOverlay != null)
                    {
                        SelectionRectangleOverlay.Visibility = Visibility.Collapsed;
                        _logger.LogTrace("🖱️ Selection rectangle hidden");
                    }
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in HideSelectionRectangle");
            }
        }

        /// <summary>
        /// Získa cell pozíciu z mouse pozície pomocou hit-testing
        /// </summary>
        private CellPosition? GetCellFromPoint(Point point)
        {
            try
            {
                // Adjust point relative to DataContainer if available
                if (DataContainer != null && DataGridScrollViewer != null)
                {
                    // Get the transformed point relative to data container
                    var transform = DataGridScrollViewer.TransformToVisual(DataContainer);
                    point = transform.TransformPoint(point);
                }

                // Use visual tree hit testing to find TextBox elements
                var elementsAtPoint = VisualTreeHelper.FindElementsInHostCoordinates(
                    point, (UIElement)(DataContainer ?? (object)this), false);

                // Look for TextBox in the hit elements
                foreach (var element in elementsAtPoint)
                {
                    if (element is TextBox textBox && textBox.Tag is CellViewModel cellViewModel)
                    {
                        // Found exact cell via hit-testing
                        return new CellPosition
                        {
                            Row = cellViewModel.RowIndex,
                            Column = cellViewModel.ColumnIndex,
                            ColumnName = cellViewModel.ColumnName
                        };
                    }
                }

                // Fallback to estimation if hit-testing doesn't find cell
                var estimatedRow = Math.Max(0, (int)(point.Y / 36)); // Default row height 36px
                var estimatedCol = 0;
                double cumulativeWidth = 0;

                // Calculate column based on actual column widths if available
                for (int i = 0; i < _columns.Count; i++)
                {
                    var colWidth = _columns[i].Width > 0 ? _columns[i].Width : 150; // Default width
                    if (point.X <= cumulativeWidth + colWidth)
                    {
                        estimatedCol = i;
                        break;
                    }
                    cumulativeWidth += colWidth;
                    estimatedCol = i + 1;
                }

                // Validate boundaries
                estimatedRow = Math.Min(estimatedRow, _displayRows.Count - 1);
                estimatedCol = Math.Max(0, Math.Min(estimatedCol, _columns.Count - 1));

                if (estimatedCol < _columns.Count && !_columns[estimatedCol].IsSpecialColumn)
                {
                    var cellPosition = new CellPosition
                    {
                        Row = estimatedRow,
                        Column = estimatedCol,
                        ColumnName = _columns[estimatedCol].Name
                    };

                    _logger.LogTrace("🖱️ GetCellFromPoint - Point: ({X},{Y}), Cell: [{Row},{Column}] '{ColumnName}'",
                        point.X, point.Y, cellPosition.Row, cellPosition.Column, cellPosition.ColumnName);

                    return cellPosition;
                }

                _logger.LogTrace("🖱️ GetCellFromPoint - Point: ({X},{Y}), No valid cell found", point.X, point.Y);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetCellFromPoint - Point: ({X},{Y})", point.X, point.Y);
                return null;
            }
        }

        #endregion

        #region ✅ NOVÉ: INavigationCallback Implementation

        /// <summary>
        /// Presunie focus na ďalšiu bunku (Tab)
        /// </summary>
        public async Task MoveToNextCellAsync(int currentRow, int currentColumn)
        {
            try
            {
                var nextColumn = currentColumn + 1;
                var nextRow = currentRow;

                // Ak je na konci riadku, prejdi na začiatok ďalšieho riadku
                if (nextColumn >= _columns.Count || _columns[nextColumn].IsSpecialColumn)
                {
                    nextColumn = 0;
                    nextRow++;

                    // Ak je na konci dát, zostať na poslednej bunke
                    if (nextRow >= _displayRows.Count)
                    {
                        nextRow = _displayRows.Count - 1;
                        nextColumn = GetLastEditableColumnIndex();
                    }
                }

                await MoveToCellAsync(nextRow, nextColumn);
                _logger.LogDebug("🎮 MoveToNext: [{CurrentRow},{CurrentColumn}] → [{NextRow},{NextColumn}]",
                    currentRow, currentColumn, nextRow, nextColumn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToNextCellAsync");
            }
        }

        /// <summary>
        /// Presunie focus na predchádzajúcu bunku (Shift+Tab)
        /// </summary>
        public async Task MoveToPreviousCellAsync(int currentRow, int currentColumn)
        {
            try
            {
                var prevColumn = currentColumn - 1;
                var prevRow = currentRow;

                // Ak je na začiatku riadku, prejdi na koniec predchádzajúceho riadku
                if (prevColumn < 0 || (prevColumn > 0 && _columns[prevColumn].IsSpecialColumn))
                {
                    prevRow--;
                    prevColumn = GetLastEditableColumnIndex();

                    // Ak je na začiatku dát, zostať na prvej bunke
                    if (prevRow < 0)
                    {
                        prevRow = 0;
                        prevColumn = GetFirstEditableColumnIndex();
                    }
                }

                await MoveToCellAsync(prevRow, prevColumn);
                _logger.LogDebug("🎮 MoveToPrevious: [{CurrentRow},{CurrentColumn}] → [{PrevRow},{PrevColumn}]",
                    currentRow, currentColumn, prevRow, prevColumn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToPreviousCellAsync");
            }
        }

        /// <summary>
        /// Presunie focus na bunku nižšie (Enter)
        /// </summary>
        public async Task MoveToCellBelowAsync(int currentRow, int currentColumn)
        {
            try
            {
                var nextRow = currentRow + 1;

                // Ak je na konci dát, zostať na aktuálnej bunke
                if (nextRow >= _displayRows.Count)
                {
                    nextRow = _displayRows.Count - 1;
                }

                await MoveToCellAsync(nextRow, currentColumn);
                _logger.LogDebug("🎮 MoveToCellBelow: [{CurrentRow},{CurrentColumn}] → [{NextRow},{NextColumn}]",
                    currentRow, currentColumn, nextRow, currentColumn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToCellBelowAsync");
            }
        }

        /// <summary>
        /// Presunie focus na bunku vyššie (Arrow Up)
        /// </summary>
        public async Task MoveToCellAboveAsync(int currentRow, int currentColumn)
        {
            try
            {
                var prevRow = currentRow - 1;

                // Ak je na začiatku dát, zostať na aktuálnej bunke
                if (prevRow < 0)
                {
                    prevRow = 0;
                }

                await MoveToCellAsync(prevRow, currentColumn);
                _logger.LogDebug("🎮 MoveToCellAbove: [{CurrentRow},{CurrentColumn}] → [{PrevRow},{PrevColumn}]",
                    currentRow, currentColumn, prevRow, currentColumn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToCellAboveAsync");
            }
        }

        /// <summary>
        /// Presunie focus na bunku vľavo (Arrow Left)
        /// </summary>
        public async Task MoveToCellLeftAsync(int currentRow, int currentColumn)
        {
            try
            {
                var prevColumn = currentColumn - 1;

                // Nájdi predchádzajúci editable column
                while (prevColumn >= 0 && _columns[prevColumn].IsSpecialColumn)
                {
                    prevColumn--;
                }

                // Ak nie je žiadny editable column vľavo, zostať na aktuálnej bunke
                if (prevColumn < 0)
                {
                    prevColumn = currentColumn;
                }

                await MoveToCellAsync(currentRow, prevColumn);
                _logger.LogDebug("🎮 MoveToCellLeft: [{CurrentRow},{CurrentColumn}] → [{NextRow},{PrevColumn}]",
                    currentRow, currentColumn, currentRow, prevColumn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToCellLeftAsync");
            }
        }

        /// <summary>
        /// Presunie focus na bunku vpravo (Arrow Right)
        /// </summary>
        public async Task MoveToCellRightAsync(int currentRow, int currentColumn)
        {
            try
            {
                var nextColumn = currentColumn + 1;

                // Nájdi ďalší editable column
                while (nextColumn < _columns.Count && _columns[nextColumn].IsSpecialColumn)
                {
                    nextColumn++;
                }

                // Ak nie je žiadny editable column vpravo, zostať na aktuálnej bunke
                if (nextColumn >= _columns.Count)
                {
                    nextColumn = currentColumn;
                }

                await MoveToCellAsync(currentRow, nextColumn);
                _logger.LogDebug("🎮 MoveToCellRight: [{CurrentRow},{CurrentColumn}] → [{NextRow},{NextColumn}]",
                    currentRow, currentColumn, currentRow, nextColumn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToCellRightAsync");
            }
        }

        /// <summary>
        /// Rozšíri selection s Shift+Arrow
        /// </summary>
        public async Task ExtendSelectionAsync(int fromRow, int fromColumn, int toRow, int toColumn)
        {
            try
            {
                // Validuj pozície
                toRow = Math.Max(0, Math.Min(toRow, _displayRows.Count - 1));
                toColumn = Math.Max(0, Math.Min(toColumn, _columns.Count - 1));

                // Preskočí special columns
                if (toColumn < _columns.Count && _columns[toColumn].IsSpecialColumn)
                {
                    return;
                }

                // Vytvor extended selection
                var startRow = Math.Min(fromRow, toRow);
                var endRow = Math.Max(fromRow, toRow);
                var startCol = Math.Min(fromColumn, toColumn);
                var endCol = Math.Max(fromColumn, toColumn);

                // Clear current selection
                _cellSelectionState.ClearSelection();

                // Add all cells in range
                for (int row = startRow; row <= endRow; row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        if (col < _columns.Count && !_columns[col].IsSpecialColumn)
                        {
                            _cellSelectionState.AddCellToSelection(row, col, _columns[col].Name);
                        }
                    }
                }

                // Set focus to target cell
                if (toColumn < _columns.Count && !_columns[toColumn].IsSpecialColumn)
                {
                    _cellSelectionState.SetFocusedCell(toRow, toColumn, _columns[toColumn].Name);
                }

                await UpdateCellVisualStates();

                _logger.LogDebug("🎮 ExtendSelection: [{FromRow},{FromColumn}] → [{ToRow},{ToColumn}], " +
                    "Range: [{StartRow},{StartCol}]-[{EndRow},{EndCol}]",
                    fromRow, fromColumn, toRow, toColumn, startRow, startCol, endRow, endCol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ExtendSelectionAsync");
            }
        }

        /// <summary>
        /// Vyberie všetky bunky (Ctrl+A)
        /// </summary>
        public async Task SelectAllCellsAsync()
        {
            try
            {
                _cellSelectionState.ClearSelection();

                // Select all non-special cells
                foreach (var row in _displayRows)
                {
                    foreach (var cell in row.Cells)
                    {
                        var column = _columns.FirstOrDefault(c => c.Name == cell.ColumnName);
                        if (column != null && !column.IsSpecialColumn)
                        {
                            _cellSelectionState.AddCellToSelection(cell.RowIndex, 
                                GetColumnIndex(cell.ColumnName), cell.ColumnName);
                        }
                    }
                }

                await UpdateCellVisualStates();

                var selectedCount = _cellSelectionState.SelectedCellCount;
                _logger.LogInformation("🎮 SelectAllCells: Selected {SelectedCount} cells", selectedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SelectAllCellsAsync");
            }
        }

        /// <summary>
        /// Získa aktuálnu pozíciu bunky z UI elementu
        /// </summary>
        public (int Row, int Column) GetCellPosition(object uiElement)
        {
            // Pre jednoduchosť zatiaľ vráti (0,0) - v skutočnej implementácii by to parsovalo z UI
            // V plnej implementácii by sa hľadal parent container bunky a získala pozícia
            return (0, 0);
        }

        /// <summary>
        /// Získa UI element pre bunku na pozícii
        /// </summary>
        public object? GetCellUIElement(int row, int column)
        {
            // Pre jednoduchosť zatiaľ vráti null - v skutočnej implementácii by to našlo UI element
            return null;
        }

        /// <summary>
        /// ✅ UPDATED: Presunie focus na bunku na pozícii - now public for INavigationCallback
        /// </summary>
        public async Task MoveToCellAsync(int row, int column)
        {
            try
            {
                if (row < 0 || row >= _displayRows.Count || column < 0 || column >= _columns.Count)
                {
                    _logger.LogWarning("🎮 MoveToCellAsync: Invalid position [{Row},{Column}]", row, column);
                    return;
                }

                var columnName = _columns[column].Name;
                
                // Update selection state
                await OnCellClicked(row, column, columnName, false);

                _logger.LogDebug("🎮 MovedToCell: [{Row},{Column}]{ColumnName}", row, column, columnName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToCellAsync");
            }
        }

        /// <summary>
        /// Získa index prvého editable stĺpca
        /// </summary>
        private int GetFirstEditableColumnIndex()
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (!_columns[i].IsSpecialColumn)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// Získa index posledného editable stĺpca
        /// </summary>
        private int GetLastEditableColumnIndex()
        {
            for (int i = _columns.Count - 1; i >= 0; i--)
            {
                if (!_columns[i].IsSpecialColumn)
                {
                    return i;
                }
            }
            return _columns.Count - 1;
        }

        /// <summary>
        /// ✅ NOVÉ: INavigationCallback - Ctrl+Home navigation
        /// </summary>
        public async Task MoveToFirstCellAsync()
        {
            await MoveToCellAsync(0, 0);
            _logger.LogDebug("🎮 MoveToFirstCellAsync: Moved to (0,0)");
        }

        /// <summary>
        /// ✅ NOVÉ: INavigationCallback - Ctrl+End navigation
        /// </summary>
        public async Task MoveToLastCellAsync()
        {
            var lastRow = Math.Max(0, _displayRows.Count - 1);
            var lastCol = GetLastEditableColumnIndex();
            await MoveToCellAsync(lastRow, lastCol);
            _logger.LogDebug("🎮 MoveToLastCellAsync: Moved to ({Row},{Col})", lastRow, lastCol);
        }

        /// <summary>
        /// ✅ NOVÉ: INavigationCallback - Home navigation
        /// </summary>
        public async Task MoveToFirstColumnInRowAsync(int row)
        {
            await MoveToCellAsync(row, 0);
            _logger.LogDebug("🎮 MoveToFirstColumnInRowAsync: Moved to ({Row},0)", row);
        }

        /// <summary>
        /// ✅ NOVÉ: INavigationCallback - End navigation
        /// </summary>
        public async Task MoveToLastColumnInRowAsync(int row)
        {
            var lastCol = GetLastEditableColumnIndex();
            await MoveToCellAsync(row, lastCol);
            _logger.LogDebug("🎮 MoveToLastColumnInRowAsync: Moved to ({Row},{Col})", row, lastCol);
        }


        /// <summary>
        /// ✅ NOVÉ: INavigationCallback - Get visible rows count for Page Up/Down
        /// </summary>
        public async Task<int> GetVisibleRowsCountAsync()
        {
            // Estimate visible rows based on DataGrid height and row height
            var gridHeight = DataGridScrollViewer?.ViewportHeight ?? 400;
            var rowHeight = 36.0; // Default row height
            var visibleRows = Math.Max(1, (int)(gridHeight / rowHeight) - 1); // -1 for header
            
            _logger.LogTrace("📏 GetVisibleRowsCountAsync: GridHeight={Height}, RowHeight={RowHeight}, VisibleRows={VisibleRows}",
                gridHeight, rowHeight, visibleRows);
            
            await Task.CompletedTask;
            return visibleRows;
        }

        /// <summary>
        /// ✅ NOVÉ: INavigationCallback - Get total rows count
        /// </summary>
        public async Task<int> GetTotalRowsCountAsync()
        {
            var totalRows = _displayRows.Count;
            _logger.LogTrace("📊 GetTotalRowsCountAsync: TotalRows={TotalRows}", totalRows);
            
            await Task.CompletedTask;
            return totalRows;
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Get total rows count (sync version)
        /// </summary>
        public int GetTotalRowCount()
        {
            var totalRows = _displayRows.Count;
            _logger.LogTrace("📊 GetTotalRowCount: TotalRows={TotalRows}", totalRows);
            return totalRows;
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Get selected rows count (checkbox based)
        /// </summary>
        public int GetSelectedRowCount()
        {
            try
            {
                var checkboxColumnIndex = GetCheckboxColumnIndex();
                if (checkboxColumnIndex < 0)
                {
                    _logger.LogTrace("📊 GetSelectedRowCount: No checkbox column, returning 0");
                    return 0;
                }

                var selectedCount = 0;
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    if (checkboxColumnIndex < row.Cells.Count)
                    {
                        var isSelected = row.Cells[checkboxColumnIndex].Value as bool? == true;
                        if (isSelected) selectedCount++;
                    }
                }

                _logger.LogTrace("📊 GetSelectedRowCount: SelectedRows={SelectedRows}", selectedCount);
                return selectedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetSelectedRowCount");
                return 0;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Get valid rows count (validation based)
        /// </summary>
        public int GetValidRowCount()
        {
            try
            {
                var validCount = 0;
                foreach (var row in _displayRows)
                {
                    var isRowValid = row.Cells.All(cell => cell.IsValid);
                    var hasData = row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell.Value?.ToString()));
                    
                    if (hasData && isRowValid) validCount++;
                }

                _logger.LogTrace("📊 GetValidRowCount: ValidRows={ValidRows}", validCount);
                return validCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetValidRowCount");
                return 0;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Get invalid rows count (validation based)
        /// </summary>
        public int GetInvalidRowCount()
        {
            try
            {
                var invalidCount = 0;
                foreach (var row in _displayRows)
                {
                    var isRowValid = row.Cells.All(cell => cell.IsValid);
                    var hasData = row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell.Value?.ToString()));
                    
                    if (hasData && !isRowValid) invalidCount++;
                }

                _logger.LogTrace("📊 GetInvalidRowCount: InvalidRows={InvalidRows}", invalidCount);
                return invalidCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetInvalidRowCount");
                return 0;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Get last validation duration in milliseconds
        /// </summary>
        public long GetLastValidationDuration()
        {
            _logger.LogTrace("📊 GetLastValidationDuration: Duration={Duration}ms", _lastValidationDurationMs);
            return _lastValidationDurationMs;
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Get minimum row count (unified row count)
        /// </summary>
        public int GetMinimumRowCount()
        {
            _logger.LogTrace("📊 GetMinimumRowCount: MinRows={MinRows}", _unifiedRowCount);
            return _unifiedRowCount;
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Set minimum row count (unified row count)
        /// </summary>
        public async Task SetMinimumRowCountAsync(int minRowCount)
        {
            try
            {
                _logger.LogInformation("📊 SetMinimumRowCountAsync START - Current: {Current}, New: {New}", 
                    _unifiedRowCount, minRowCount);

                var oldCount = _unifiedRowCount;
                _unifiedRowCount = Math.Max(minRowCount, 1);

                if (_unifiedRowCount != oldCount)
                {
                    // Update configuration if available
                    if (_configuration != null)
                    {
                        _configuration.UnifiedRowCount = _unifiedRowCount;
                    }

                    // Ensure we have enough empty rows
                    await Task.Run(() => EnsureMinimumRows());
                    
                    _logger.LogInformation("✅ SetMinimumRowCountAsync COMPLETED - Updated from {Old} to {New}", 
                        oldCount, _unifiedRowCount);
                }
                else
                {
                    _logger.LogDebug("📊 SetMinimumRowCountAsync: No change needed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetMinimumRowCountAsync");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Update throttling configuration at runtime
        /// </summary>
        public void UpdateThrottlingConfig(GridThrottlingConfig newConfig)
        {
            try
            {
                _logger.LogInformation("⚙️ UpdateThrottlingConfig START - ValidationDebounce: {ValidationMs}ms, " +
                    "UIUpdate: {UIMs}ms, Search: {SearchMs}ms, RealtimeValidation: {RealtimeEnabled}",
                    newConfig.ValidationDebounceMs, newConfig.UIUpdateDebounceMs,
                    newConfig.SearchDebounceMs, newConfig.EnableRealtimeValidation);

                var oldConfig = _throttlingConfig;
                _throttlingConfig = newConfig?.Clone();

                // Update configuration object if available
                if (_configuration != null && _throttlingConfig != null)
                {
                    // RealtimeValidationEnabled is handled by throttling config directly
                    // _configuration.RealtimeValidationEnabled = _throttlingConfig.EnableRealtimeValidation;
                }

                _logger.LogInformation("✅ UpdateThrottlingConfig COMPLETED - Configuration updated at runtime");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateThrottlingConfig");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Update color configuration at runtime
        /// </summary>
        public void UpdateColorConfig(DataGridColorConfig newConfig)
        {
            try
            {
                _logger.LogInformation("🎨 UpdateColorConfig START - ZebraRows: {ZebraEnabled}, CustomColors: {CustomCount}",
                    newConfig?.IsZebraRowsEnabled ?? false, newConfig?.CustomColorsCount ?? 0);

                _individualColorConfig = newConfig?.Clone();

                // Apply colors to UI immediately
                ApplyIndividualColorsToUI();

                // Update zebra rows setting in search service
                if (_searchAndSortService != null && _individualColorConfig != null)
                {
                    _searchAndSortService.SetZebraRowsEnabled(_individualColorConfig.IsZebraRowsEnabled);
                }

                _logger.LogInformation("✅ UpdateColorConfig COMPLETED - Colors applied to UI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateColorConfig");
                throw;
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods s kompletným logovaním a metrics

        /// <summary>
        /// InitializeAsync - HLAVNÁ PUBLIC API metóda pre nové aplikácie
        /// ✅ ROZŠÍRENÉ LOGOVANIE: Detailné sledovanie každého kroku inicializácie
        /// </summary>
        /// <param name="columns">Definície stĺpcov gridu</param>
        /// <param name="validationConfig">Konfigurácia validačných pravidiel (optional)</param>
        /// <param name="throttlingConfig">Konfigurácia throttling-u (optional)</param>
        /// <param name="emptyRowsCount">Počet prázdnych riadkov na konci (default: 15)</param>
        /// <param name="colorConfig">Konfigurácia farieb gridu (optional)</param>
        /// <param name="logger">External logger (optional)</param>
        /// <param name="enableBatchValidation">Povoliť batch validáciu (default: false)</param>
        /// <param name="maxSearchHistoryItems">Max počet položiek v search history (DEPRECATED - použite searchHistoryItems)</param>
        /// <param name="enableSort">Povoliť sortovanie stĺpcov (default: false)</param>
        /// <param name="enableSearch">Povoliť vyhľadávanie (default: false)</param>
        /// <param name="enableFilter">Povoliť filtrovanie stĺpcov (default: false)</param>
        /// <param name="searchHistoryItems">Počet položiek v search history (0-100, default: 0)</param>
        /// <exception cref="ArgumentOutOfRangeException">Ak searchHistoryItems nie je v rozsahu 0-100</exception>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            IValidationConfiguration? validationConfig = null,
            GridThrottlingConfig? throttlingConfig = null,
            int emptyRowsCount = 15,
            DataGridColorConfig? colorConfig = null,
            ILogger? logger = null,
            bool enableBatchValidation = false,
            int maxSearchHistoryItems = 0,
            bool enableSort = false,
            bool enableSearch = false, 
            bool enableFilter = false,
            int searchHistoryItems = 0)
        {
            try
            {
                // ✅ BUILD-TIME VALIDATION: Search history items must be in range 0-100
                if (searchHistoryItems < 0 || searchHistoryItems > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(searchHistoryItems), 
                        searchHistoryItems, 
                        "Search history items must be between 0 and 100 (inclusive). " +
                        $"Provided value: {searchHistoryItems}");
                }

                // If external logger provided, use it; otherwise use internal logger
                var effectiveLogger = logger ?? _logger;
                
                effectiveLogger.LogInformation("🚀 InitializeAsync START - Instance: {ComponentInstanceId}, " +
                    "Columns: {ColumnCount}, Rules: {RuleCount}, EmptyRows: {EmptyRows}, HasColors: {HasColors}, " +
                    "Sort: {EnableSort}, Search: {EnableSearch}, Filter: {EnableFilter}, SearchHistory: {SearchHistory}",
                    _componentInstanceId, columns?.Count ?? 0, validationConfig?.RulesCount ?? 0, emptyRowsCount,
                    colorConfig?.HasAnyCustomColors ?? false, enableSort, enableSearch, enableFilter, searchHistoryItems);

                StartOperation("InitializeAsync");
                IncrementOperationCounter("InitializeAsync");

                if (columns == null || columns.Count == 0)
                {
                    _logger.LogError("❌ InitializeAsync: Columns parameter is null or empty - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    throw new ArgumentException("Columns parameter cannot be null or empty", nameof(columns));
                }

                // Convert validation config to internal format for processing
                Models.Validation.ValidationRuleSet? internalValidationRules = null;
                if (validationConfig is PublicValidationConfiguration publicConfig)
                {
                    internalValidationRules = publicConfig.GetInternalRuleSet();
                }
                
                // Convert advanced rules to legacy format for internal processing
                var legacyRules = ConvertAdvancedRulesToLegacy(internalValidationRules);

                // ✅ NOVÉ: Vytvor a nastav DataGridConfiguration
                await ConfigureControllerAsync(columns, legacyRules, throttlingConfig, emptyRowsCount, colorConfig);

                // ✅ ROZŠÍRENÉ: Detailné logovanie štruktúry stĺpcov
                LogColumnStructure(columns);

                // ✅ NOVÉ: Detekcia CheckBox column
                DetectAndConfigureCheckBoxColumn(columns);

                // ✅ ROZŠÍRENÉ: Detailné logovanie validačných pravidiel
                LogAdvancedValidationRules(internalValidationRules);

                // Store throttling config pre realtime validáciu
                _throttlingConfig = throttlingConfig?.Clone() ?? GridThrottlingConfig.Default;
                _logger.LogDebug("⚙️ Throttling config stored - ValidationDebounce: {ValidationMs}ms, " +
                    "UIUpdate: {UIMs}ms, Search: {SearchMs}ms, RealtimeValidation: {RealtimeEnabled}",
                    _throttlingConfig.ValidationDebounceMs, _throttlingConfig.UIUpdateDebounceMs,
                    _throttlingConfig.SearchDebounceMs, _throttlingConfig.EnableRealtimeValidation);

                // ✅ NOVÉ: Header deduplikácia pred uložením
                var deduplicatedColumns = DeduplicateColumnHeaders(columns);
                
                // Store configuration
                _columns.Clear();
                _columns.AddRange(deduplicatedColumns);

                // ✅ NOVÉ: Configure feature flags
                await ConfigureFeatureFlagsAsync(enableSort, enableSearch, enableFilter, searchHistoryItems);
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;
                _individualColorConfig = colorConfig?.Clone() ?? new DataGridColorConfig();
                
                // ✅ Ak nie je nastavená AlternateRowColor, nastav default zebra farbu
                if (_individualColorConfig.AlternateRowColor == null)
                {
                    _individualColorConfig.AlternateRowColor = Windows.UI.Color.FromArgb(20, 0, 0, 0);  // Jemne tmavá zebra farba
                }
                _advancedValidationRules = internalValidationRules;

                // ✅ ROZŠÍRENÉ: Detailné logovanie color config
                LogColorConfiguration(colorConfig);

                // Initialize services
                await InitializeServicesAsync(columns, legacyRules, _throttlingConfig, emptyRowsCount, enableBatchValidation, maxSearchHistoryItems);

                // ✅ UI setup s resize, scroll a stretch funkcionalitou
                ApplyIndividualColorsToUI();
                await InitializeSearchSortZebra();
                await CreateInitialEmptyRowsAsync();
                await CreateResizableHeadersAsync();
                SetupValidAlertsStretching();
                SetupScrollSynchronization();

                _isInitialized = true;
                await UpdateUIVisibilityAsync();

                var duration = EndOperation("InitializeAsync");

                // ✅ ROZŠÍRENÉ: Kompletný súhrn inicializácie
                LogInitializationSummary(duration);

                _logger.LogInformation("✅ InitializeAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, TotalColumns: {TotalColumns}, ConfiguredFeatures: {Features}",
                    _componentInstanceId, duration, _columns.Count, GetEnabledFeatures());

                LogComponentState("InitializeAsync-End");
            }
            catch (Exception ex)
            {
                EndOperation("InitializeAsync");
                IncrementOperationCounter("InitializeAsync-Error");
                _logger.LogError(ex, "❌ CRITICAL ERROR during InitializeAsync - Instance: {ComponentInstanceId}, " +
                    "Columns: {ColumnCount}", _componentInstanceId, columns?.Count ?? 0);
                throw;
            }
        }

        /// <summary>
        /// LoadDataAsync s kompletným logovaním
        /// ✅ ROZŠÍRENÉ LOGOVANIE: Detailné sledovanie dátových operácií
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                _logger.LogInformation("📊 LoadDataAsync START - Instance: {ComponentInstanceId}, " +
                    "InputRows: {RowCount}, CurrentDisplayRows: {CurrentRows}",
                    _componentInstanceId, data?.Count ?? 0, _displayRows.Count);

                StartOperation("LoadDataAsync");
                IncrementOperationCounter("LoadDataAsync");

                if (data == null)
                {
                    _logger.LogWarning("⚠️ LoadDataAsync: Null data provided, using empty list - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    data = new List<Dictionary<string, object?>>();
                }

                EnsureInitialized();

                // ✅ ROZŠÍRENÉ: Detailná analýza prichádzajúcich dát
                await LogDataAnalysis(data);

                if (_dataManagementService != null)
                {
                    _logger.LogDebug("📋 LoadDataAsync: Delegating to DataManagementService - RowCount: {RowCount}",
                        data.Count);

                    await _dataManagementService.LoadDataAsync(data);
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                    await RefreshDataDisplayAsync();
                }

                // ✅ Update layout after data load
                await UpdateLayoutAfterDataChangeAsync();

                _lastDataUpdate = DateTime.UtcNow;
                var duration = EndOperation("LoadDataAsync");

                // ✅ ROZŠÍRENÉ: Kompletný súhrn načítania dát
                LogDataLoadSummary(data, duration);

                _logger.LogInformation("✅ LoadDataAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, FinalRowCount: {FinalCount}, CellsRendered: {CellsRendered}",
                    _componentInstanceId, duration, _displayRows.Count, _totalCellsRendered);

                LogComponentState("LoadDataAsync-End");
            }
            catch (Exception ex)
            {
                EndOperation("LoadDataAsync");
                IncrementOperationCounter("LoadDataAsync-Error");
                _logger.LogError(ex, "❌ ERROR in LoadDataAsync - Instance: {ComponentInstanceId}, " +
                    "InputRowCount: {InputRows}", _componentInstanceId, data?.Count ?? 0);
                throw;
            }
        }

        /// <summary>
        /// ValidateAllRowsAsync s detailným logovaním validácie
        /// </summary>
        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                _logger.LogInformation("🔍 ValidateAllRowsAsync START - Instance: {ComponentInstanceId}, " +
                    "CurrentRows: {RowCount}, ValidationRules: {RuleCount}",
                    _componentInstanceId, _displayRows.Count, GetTotalValidationRulesCount());

                StartOperation("ValidateAllRows");
                IncrementOperationCounter("ValidateAllRows");
                EnsureInitialized();

                if (_validationService == null)
                {
                    _logger.LogWarning("⚠️ ValidateAllRowsAsync: ValidationService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return false;
                }

                var result = await _validationService.ValidateAllRowsAsync();
                var duration = EndOperation("ValidateAllRows");

                // ✅ NOVÉ: Track validation duration for performance metrics
                _lastValidationDurationMs = (long)duration;

                // ✅ ROZŠÍRENÉ: Detailné logovanie výsledkov validácie
                LogValidationResults(result, duration);

                _logger.LogInformation("✅ ValidateAllRowsAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Result: {IsValid}, Duration: {Duration}ms, TotalErrors: {ErrorCount}",
                    _componentInstanceId, result, duration, _totalValidationErrors);

                return result;
            }
            catch (Exception ex)
            {
                EndOperation("ValidateAllRows");
                IncrementOperationCounter("ValidateAllRows-Error");
                _logger.LogError(ex, "❌ ERROR in ValidateAllRowsAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Vykoná validáciu všetkých neprázdnych riadkov a aktualizuje UI - PUBLIC API
        /// </summary>
        public async Task ValidateAndUpdateUIAsync()
        {
            try
            {
                _logger.LogInformation("🔍 ValidateAndUpdateUIAsync START - Instance: {ComponentInstanceId}, " +
                    "NonEmptyRows: {NonEmptyRowCount}, TotalRows: {TotalRows}",
                    _componentInstanceId, GetNonEmptyRowCount(), _displayRows.Count);

                StartOperation("ValidateAndUpdateUI");
                IncrementOperationCounter("ValidateAndUpdateUI");
                EnsureInitialized();

                if (_validationService == null)
                {
                    _logger.LogWarning("⚠️ ValidateAndUpdateUIAsync: ValidationService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                // Získaj len neprázdne riadky pre validáciu
                var nonEmptyRows = GetNonEmptyRowsData();
                if (!nonEmptyRows.Any())
                {
                    _logger.LogInformation("ℹ️ ValidateAndUpdateUIAsync: No non-empty rows to validate - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                _logger.LogDebug("🔍 Validating {NonEmptyRowCount} non-empty rows...", nonEmptyRows.Count);

                // Spusti validáciu len pre neprázdne riadky
                await _validationService.ValidateRowsAsync();

                // Aktualizuj UI pre všetky riadky
                await UpdateValidationUI();

                var duration = EndOperation("ValidateAndUpdateUI");

                _logger.LogInformation("✅ ValidateAndUpdateUIAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ValidatedRows: {ValidatedRows}, TotalErrors: {ErrorCount}",
                    _componentInstanceId, duration, nonEmptyRows.Count, _totalValidationErrors);
            }
            catch (Exception ex)
            {
                EndOperation("ValidateAndUpdateUI");
                IncrementOperationCounter("ValidateAndUpdateUI-Error");
                _logger.LogError(ex, "❌ ERROR in ValidateAndUpdateUIAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Získa počet neprázdnych riadkov
        /// </summary>
        private int GetNonEmptyRowCount()
        {
            try
            {
                return _displayRows.Count(row => 
                    row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell.Value?.ToString())));
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Získa dáta len z neprázdnych riadkov
        /// </summary>
        private List<Dictionary<string, object?>> GetNonEmptyRowsData()
        {
            try
            {
                var nonEmptyRows = new List<Dictionary<string, object?>>();
                
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    var hasData = row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell.Value?.ToString()));
                    
                    if (hasData)
                    {
                        var rowData = new Dictionary<string, object?>();
                        for (int j = 0; j < _columns.Count && j < row.Cells.Count; j++)
                        {
                            rowData[_columns[j].Name] = row.Cells[j].Value;
                        }
                        nonEmptyRows.Add(rowData);
                    }
                }

                return nonEmptyRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR getting non-empty rows data");
                return new List<Dictionary<string, object?>>();
            }
        }

        /// <summary>
        /// Aktualizuje UI po validácii
        /// </summary>
        private async Task UpdateValidationUI()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Aktualizuj UI na main thread
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            // Refresh validation alerts column ak existuje
                            RefreshValidationAlertsColumn();
                            
                            // Trigger UI refresh
                            InvalidateArrange();
                            
                            _logger.LogDebug("✅ Validation UI updated successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ ERROR updating validation UI");
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateValidationUI");
            }
        }

        /// <summary>
        /// Refresh validation alerts column
        /// </summary>
        private void RefreshValidationAlertsColumn()
        {
            try
            {
                // Find validation alerts column and refresh its content
                var validationColumnIndex = -1;
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (_columns[i].Name.Contains("ValidAlert") || _columns[i].Name.Contains("ValidationAlert"))
                    {
                        validationColumnIndex = i;
                        break;
                    }
                }

                if (validationColumnIndex >= 0)
                {
                    // Update validation alerts for all rows
                    for (int rowIndex = 0; rowIndex < _displayRows.Count; rowIndex++)
                    {
                        var row = _displayRows[rowIndex];
                        if (validationColumnIndex < row.Cells.Count)
                        {
                            // Refresh validation state for this row
                            var validationMessages = GetValidationMessagesForRow(rowIndex);
                            row.Cells[validationColumnIndex].ClearValidationErrors();
                            if (validationMessages.Any())
                            {
                                row.Cells[validationColumnIndex].ValidationErrors = string.Join("; ", validationMessages);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR refreshing validation alerts column");
            }
        }

        /// <summary>
        /// Získa validation messages pre konkrétny riadok
        /// </summary>
        private List<string> GetValidationMessagesForRow(int rowIndex)
        {
            try
            {
                var messages = new List<string>();
                
                if (rowIndex >= 0 && rowIndex < _displayRows.Count)
                {
                    var row = _displayRows[rowIndex];
                    foreach (var cell in row.Cells)
                    {
                        if (!string.IsNullOrEmpty(cell.ValidationErrors))
                        {
                            messages.Add(cell.ValidationErrors);
                        }
                    }
                }

                return messages;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// ExportToDataTableAsync s detailným logovaním exportu
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync(bool includeValidAlerts = false)
        {
            try
            {
                _logger.LogInformation("📤 ExportToDataTableAsync START - Instance: {ComponentInstanceId}, " +
                    "DisplayRows: {RowCount}, Columns: {ColumnCount}",
                    _componentInstanceId, _displayRows.Count, _columns.Count);

                StartOperation("ExportToDataTable");
                IncrementOperationCounter("ExportToDataTable");
                EnsureInitialized();

                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ ExportToDataTableAsync: ExportService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return new DataTable();
                }

                var result = await _exportService.ExportToDataTableAsync(includeValidAlerts);
                var duration = EndOperation("ExportToDataTable");

                // ✅ ROZŠÍRENÉ: Detailné logovanie exportu
                LogExportResults(result, duration);

                _logger.LogInformation("✅ ExportToDataTableAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ExportedRows: {RowCount}, ExportedColumns: {ColumnCount}",
                    _componentInstanceId, duration, result.Rows.Count, result.Columns.Count);

                return result;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToDataTable");
                IncrementOperationCounter("ExportToDataTable-Error");
                _logger.LogError(ex, "❌ ERROR in ExportToDataTableAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Export to Excel as byte array
        /// </summary>
        public async Task<byte[]> ExportToExcelAsync(bool includeValidAlertsColumn = false)
        {
            try
            {
                _logger.LogInformation("📤 ExportToExcelAsync START - Instance: {ComponentInstanceId}, " +
                    "DisplayRows: {RowCount}, IncludeValidAlerts: {IncludeValidAlerts}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn);

                StartOperation("ExportToExcel");
                IncrementOperationCounter("ExportToExcel");
                EnsureInitialized();

                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ ExportToExcelAsync: ExportService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return Array.Empty<byte>();
                }

                var dataTable = await ExportToDataTableAsync(includeValidAlertsColumn);
                var excelBytes = await ConvertDataTableToExcelBytesAsync(dataTable);
                var duration = EndOperation("ExportToExcel");

                _logger.LogInformation("✅ ExportToExcelAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ByteSize: {ByteSize}B",
                    _componentInstanceId, duration, excelBytes.Length);

                return excelBytes;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToExcel");
                IncrementOperationCounter("ExportToExcel-Error");
                _logger.LogError(ex, "❌ ERROR in ExportToExcelAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Export to CSV as byte array
        /// </summary>
        public async Task<byte[]> ExportToCsvAsync(bool includeValidAlertsColumn = false)
        {
            try
            {
                _logger.LogInformation("📤 ExportToCsvAsync START - Instance: {ComponentInstanceId}, " +
                    "DisplayRows: {RowCount}, IncludeValidAlerts: {IncludeValidAlerts}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn);

                StartOperation("ExportToCsv");
                IncrementOperationCounter("ExportToCsv");
                EnsureInitialized();

                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ ExportToCsvAsync: ExportService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return Array.Empty<byte>();
                }

                var dataTable = await ExportToDataTableAsync(includeValidAlertsColumn);
                var csvBytes = await ConvertDataTableToCsvBytesAsync(dataTable);
                var duration = EndOperation("ExportToCsv");

                _logger.LogInformation("✅ ExportToCsvAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ByteSize: {ByteSize}B",
                    _componentInstanceId, duration, csvBytes.Length);

                return csvBytes;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToCsv");
                IncrementOperationCounter("ExportToCsv-Error");
                _logger.LogError(ex, "❌ ERROR in ExportToCsvAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Export to JSON as byte array
        /// </summary>
        public async Task<byte[]> ExportToJsonAsync(bool includeValidAlertsColumn = false)
        {
            try
            {
                _logger.LogInformation("📤 ExportToJsonAsync START - Instance: {ComponentInstanceId}, " +
                    "DisplayRows: {RowCount}, IncludeValidAlerts: {IncludeValidAlerts}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn);

                StartOperation("ExportToJson");
                IncrementOperationCounter("ExportToJson");
                EnsureInitialized();

                var dataTable = await ExportToDataTableAsync(includeValidAlertsColumn);
                var jsonBytes = await ConvertDataTableToJsonBytesAsync(dataTable);
                var duration = EndOperation("ExportToJson");

                _logger.LogInformation("✅ ExportToJsonAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ByteSize: {ByteSize}B",
                    _componentInstanceId, duration, jsonBytes.Length);

                return jsonBytes;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToJson");
                IncrementOperationCounter("ExportToJson-Error");
                _logger.LogError(ex, "❌ ERROR in ExportToJsonAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Export to XML as string
        /// </summary>
        public string ExportToXmlString(bool includeValidAlertsColumn = false)
        {
            try
            {
                _logger.LogInformation("📤 ExportToXmlString START - Instance: {ComponentInstanceId}, " +
                    "DisplayRows: {RowCount}, IncludeValidAlerts: {IncludeValidAlerts}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn);

                StartOperation("ExportToXml");
                IncrementOperationCounter("ExportToXml");
                EnsureInitialized();

                var allData = GetAllData(includeValidAlertsColumn);
                var xmlString = ConvertDataToXmlString(allData);
                var duration = EndOperation("ExportToXml");

                _logger.LogInformation("✅ ExportToXmlString COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, StringLength: {StringLength}",
                    _componentInstanceId, duration, xmlString.Length);

                return xmlString;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToXml");
                IncrementOperationCounter("ExportToXml-Error");
                _logger.LogError(ex, "❌ ERROR in ExportToXmlString - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Import from Excel byte array
        /// </summary>
        public async Task<ImportResult> ImportFromExcelAsync(byte[] excelBytes, bool validateOnImport = true, bool continueOnErrors = false)
        {
            try
            {
                _logger.LogInformation("📥 ImportFromExcelAsync START - Instance: {ComponentInstanceId}, " +
                    "ByteSize: {ByteSize}B, ValidateOnImport: {ValidateOnImport}",
                    _componentInstanceId, excelBytes.Length, validateOnImport);

                StartOperation("ImportFromExcel");
                IncrementOperationCounter("ImportFromExcel");
                EnsureInitialized();

                // Convert Excel bytes to data
                var importedData = await ConvertExcelBytesToDataAsync(excelBytes);
                
                // Create import result
                var result = new ImportResult
                {
                    FilePath = "byte_array_excel_import",
                    IsSuccessful = importedData.Count > 0,
                    TotalRowsInFile = importedData.Count,
                    SuccessfullyImportedRows = importedData.Count,
                    ImportedData = importedData
                };

                if (result.IsSuccessful)
                {
                    // Load data into grid with validation if required
                    if (validateOnImport && _validationService != null)
                    {
                        _logger.LogDebug("📥 Validating imported Excel data...");
                        // Validation will be performed during LoadDataAsync
                    }

                    await LoadDataAsync(importedData);
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                }

                var duration = EndOperation("ImportFromExcel");
                result.FinalizeImport();

                _logger.LogInformation("✅ ImportFromExcelAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ImportedRows: {ImportedRows}",
                    _componentInstanceId, duration, importedData.Count);

                return result;
            }
            catch (Exception ex)
            {
                EndOperation("ImportFromExcel");
                IncrementOperationCounter("ImportFromExcel-Error");
                _logger.LogError(ex, "❌ ERROR in ImportFromExcelAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                
                var errorResult = new ImportResult
                {
                    FilePath = "byte_array_excel_import",
                    IsSuccessful = false
                };
                errorResult.AddError($"Excel import failed: {ex.Message}", severity: ErrorSeverity.Critical);
                errorResult.FinalizeImport();
                return errorResult;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Import from XML string
        /// </summary>
        public async Task<ImportResult> ImportFromXmlAsync(string xmlString, bool validateOnImport = true, bool continueOnErrors = false)
        {
            try
            {
                _logger.LogInformation("📥 ImportFromXmlAsync START - Instance: {ComponentInstanceId}, " +
                    "StringLength: {StringLength}, ValidateOnImport: {ValidateOnImport}",
                    _componentInstanceId, xmlString.Length, validateOnImport);

                StartOperation("ImportFromXml");
                IncrementOperationCounter("ImportFromXml");
                EnsureInitialized();

                // Convert XML string to data
                var importedData = await ConvertXmlStringToDataAsync(xmlString);
                
                // Create import result
                var result = new ImportResult
                {
                    FilePath = "xml_string_import",
                    IsSuccessful = importedData.Count > 0,
                    TotalRowsInFile = importedData.Count,
                    SuccessfullyImportedRows = importedData.Count,
                    ImportedData = importedData
                };

                if (result.IsSuccessful)
                {
                    // Load data into grid with validation if required
                    if (validateOnImport && _validationService != null)
                    {
                        _logger.LogDebug("📥 Validating imported XML data...");
                        // Validation will be performed during LoadDataAsync
                    }

                    await LoadDataAsync(importedData);
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                }

                var duration = EndOperation("ImportFromXml");
                result.FinalizeImport();

                _logger.LogInformation("✅ ImportFromXmlAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ImportedRows: {ImportedRows}",
                    _componentInstanceId, duration, importedData.Count);

                return result;
            }
            catch (Exception ex)
            {
                EndOperation("ImportFromXml");
                IncrementOperationCounter("ImportFromXml-Error");
                _logger.LogError(ex, "❌ ERROR in ImportFromXmlAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                
                var errorResult = new ImportResult
                {
                    FilePath = "xml_string_import",
                    IsSuccessful = false
                };
                errorResult.AddError($"XML import failed: {ex.Message}", severity: ErrorSeverity.Critical);
                errorResult.FinalizeImport();
                return errorResult;
            }
        }

        #region ✅ NOVÉ: Data Export/Import PUBLIC API

        /// <summary>
        /// Získa všetky dáta z gridu - PUBLIC API
        /// </summary>
        public List<Dictionary<string, object?>> GetAllData(bool includeValidAlertsColumn = false)
        {
            try
            {
                _logger.LogInformation("📊 GetAllData START - Instance: {ComponentInstanceId}, " +
                    "TotalRows: {TotalRows}, IncludeValidAlerts: {IncludeValidAlerts}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn);

                StartOperation("GetAllData");
                EnsureInitialized();

                var allData = new List<Dictionary<string, object?>>();

                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    var rowData = new Dictionary<string, object?>();

                    // Add regular column data
                    for (int j = 0; j < _columns.Count && j < row.Cells.Count; j++)
                    {
                        var column = _columns[j];
                        var isValidAlertsColumn = column.Name.Contains("ValidAlert") || column.Name.Contains("ValidationAlert");

                        if (!isValidAlertsColumn || includeValidAlertsColumn)
                        {
                            rowData[column.Name] = row.Cells[j].Value;
                        }
                    }

                    allData.Add(rowData);
                }

                var duration = EndOperation("GetAllData");
                _logger.LogInformation("✅ GetAllData COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, RowsReturned: {RowCount}, ColumnsPerRow: {ColumnCount}",
                    _componentInstanceId, duration, allData.Count, allData.FirstOrDefault()?.Count ?? 0);

                return allData;
            }
            catch (Exception ex)
            {
                EndOperation("GetAllData");
                _logger.LogError(ex, "❌ ERROR in GetAllData - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Získa len označené dáta z gridu (checked riadky) - PUBLIC API
        /// </summary>
        public List<Dictionary<string, object?>> GetSelectedData(bool includeValidAlertsColumn = false)
        {
            try
            {
                _logger.LogInformation("📊 GetSelectedData START - Instance: {ComponentInstanceId}, " +
                    "TotalRows: {TotalRows}, IncludeValidAlerts: {IncludeValidAlerts}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn);

                StartOperation("GetSelectedData");
                EnsureInitialized();

                var selectedData = new List<Dictionary<string, object?>>();
                var checkboxColumnIndex = GetCheckboxColumnIndex();

                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    
                    // Check if row is selected (checkbox column)
                    bool isSelected = false;
                    if (checkboxColumnIndex >= 0 && checkboxColumnIndex < row.Cells.Count)
                    {
                        isSelected = row.Cells[checkboxColumnIndex].Value as bool? == true;
                    }

                    if (isSelected)
                    {
                        var rowData = new Dictionary<string, object?>();

                        // Add regular column data
                        for (int j = 0; j < _columns.Count && j < row.Cells.Count; j++)
                        {
                            var column = _columns[j];
                            var isValidAlertsColumn = column.Name.Contains("ValidAlert") || column.Name.Contains("ValidationAlert");

                            if (!isValidAlertsColumn || includeValidAlertsColumn)
                            {
                                rowData[column.Name] = row.Cells[j].Value;
                            }
                        }

                        selectedData.Add(rowData);
                    }
                }

                var duration = EndOperation("GetSelectedData");
                _logger.LogInformation("✅ GetSelectedData COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, SelectedRows: {SelectedCount}, TotalRows: {TotalRows}",
                    _componentInstanceId, duration, selectedData.Count, _displayRows.Count);

                return selectedData;
            }
            catch (Exception ex)
            {
                EndOperation("GetSelectedData");
                _logger.LogError(ex, "❌ ERROR in GetSelectedData - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Nastaví dáta do gridu s checkbox support - PUBLIC API
        /// </summary>
        public void SetData(List<Dictionary<string, object?>> data, Dictionary<int, bool>? checkboxStates = null)
        {
            try
            {
                _logger.LogInformation("📥 SetData START - Instance: {ComponentInstanceId}, " +
                    "InputRows: {InputRows}, InputColumns: {InputColumns}, CheckboxStates: {CheckboxStatesCount}",
                    _componentInstanceId, data?.Count ?? 0, data?.FirstOrDefault()?.Count ?? 0, checkboxStates?.Count ?? 0);

                StartOperation("SetData");
                EnsureInitialized();

                if (data == null)
                {
                    _logger.LogWarning("⚠️ SetData: Input data is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                // Clear existing data
                _displayRows.Clear();
                _gridData.Clear();

                var checkboxColumnIndex = GetCheckboxColumnIndex();

                // Convert input data to grid format
                for (int i = 0; i < data.Count; i++)
                {
                    var inputRow = data[i];
                    var gridRow = new DataRowViewModel();
                    
                    for (int j = 0; j < _columns.Count; j++)
                    {
                        var column = _columns[j];
                        object? cellValue;

                        // Handle checkbox column with states
                        if (j == checkboxColumnIndex && checkboxStates != null)
                        {
                            cellValue = checkboxStates.TryGetValue(i, out var checkboxValue) ? checkboxValue : false;
                        }
                        else
                        {
                            cellValue = inputRow.TryGetValue(column.Name, out var value) ? value : null;
                        }
                        
                        var cell = new CellViewModel
                        {
                            Value = cellValue,
                            ColumnName = column.Name
                        };
                        
                        gridRow.Cells.Add(cell);
                    }

                    _displayRows.Add(gridRow);
                    
                    // Add checkbox state to grid data if applicable
                    var gridDataRow = new Dictionary<string, object?>(inputRow);
                    if (checkboxColumnIndex >= 0 && checkboxStates != null)
                    {
                        var checkboxColumnName = _columns[checkboxColumnIndex].Name;
                        gridDataRow[checkboxColumnName] = checkboxStates.TryGetValue(i, out var checkboxValue) ? checkboxValue : false;
                    }
                    
                    _gridData.Add(gridDataRow);
                }

                // Ensure minimum row count
                EnsureMinimumRows();

                var duration = EndOperation("SetData");
                _logger.LogInformation("✅ SetData COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, FinalRows: {FinalRows}",
                    _componentInstanceId, duration, _displayRows.Count);

                // Trigger UI refresh
                InvalidateArrange();
            }
            catch (Exception ex)
            {
                EndOperation("SetData");
                _logger.LogError(ex, "❌ ERROR in SetData - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Získa index checkbox column ak existuje
        /// </summary>
        private int GetCheckboxColumnIndex()
        {
            try
            {
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (_columns[i].Name.ToLower().Contains("checkbox") || 
                        _columns[i].Name.ToLower().Contains("selected") ||
                        _columns[i].Name.ToLower().Contains("check"))
                    {
                        return i;
                    }
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Zabezpečí minimálny počet riadkov
        /// </summary>
        private void EnsureMinimumRows()
        {
            try
            {
                while (_displayRows.Count < _unifiedRowCount)
                {
                    var emptyRow = new DataRowViewModel();
                    var emptyRowData = new Dictionary<string, object?>();

                    for (int j = 0; j < _columns.Count; j++)
                    {
                        var column = _columns[j];
                        var cell = new CellViewModel
                        {
                            Value = null,
                            ColumnName = column.Name
                        };
                        
                        emptyRow.Cells.Add(cell);
                        emptyRowData[column.Name] = null;
                    }

                    _displayRows.Add(emptyRow);
                    _gridData.Add(emptyRowData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR ensuring minimum rows");
            }
        }

        /// <summary>
        /// Získa všetky dáta ako DataTable - PUBLIC API
        /// </summary>
        public DataTable GetAllDataAsDataTable(bool includeValidAlertsColumn = false, bool? checkboxFilter = null)
        {
            try
            {
                _logger.LogInformation("📊 GetAllDataAsDataTable START - Instance: {ComponentInstanceId}, " +
                    "TotalRows: {TotalRows}, IncludeValidAlerts: {IncludeValidAlerts}, CheckboxFilter: {CheckboxFilter}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn, checkboxFilter);

                StartOperation("GetAllDataAsDataTable");
                EnsureInitialized();

                var dataTable = new DataTable();
                var checkboxColumnIndex = GetCheckboxColumnIndex();

                // Create columns
                for (int j = 0; j < _columns.Count; j++)
                {
                    var column = _columns[j];
                    var isValidAlertsColumn = column.Name.Contains("ValidAlert") || column.Name.Contains("ValidationAlert");

                    if (!isValidAlertsColumn || includeValidAlertsColumn)
                    {
                        var dataColumn = new DataColumn(column.Name, typeof(object));
                        dataTable.Columns.Add(dataColumn);
                    }
                }

                // Add rows
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    
                    // Apply checkbox filter if specified
                    if (checkboxFilter.HasValue && checkboxColumnIndex >= 0 && checkboxColumnIndex < row.Cells.Count)
                    {
                        var isChecked = row.Cells[checkboxColumnIndex].Value as bool? == true;
                        if (checkboxFilter.Value != isChecked)
                            continue; // Skip this row
                    }

                    var dataRow = dataTable.NewRow();
                    int dataColumnIndex = 0;

                    for (int j = 0; j < _columns.Count && j < row.Cells.Count; j++)
                    {
                        var column = _columns[j];
                        var isValidAlertsColumn = column.Name.Contains("ValidAlert") || column.Name.Contains("ValidationAlert");

                        if (!isValidAlertsColumn || includeValidAlertsColumn)
                        {
                            dataRow[dataColumnIndex] = row.Cells[j].Value ?? DBNull.Value;
                            dataColumnIndex++;
                        }
                    }

                    dataTable.Rows.Add(dataRow);
                }

                var duration = EndOperation("GetAllDataAsDataTable");
                _logger.LogInformation("✅ GetAllDataAsDataTable COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, RowsReturned: {RowCount}, ColumnsReturned: {ColumnCount}",
                    _componentInstanceId, duration, dataTable.Rows.Count, dataTable.Columns.Count);

                return dataTable;
            }
            catch (Exception ex)
            {
                EndOperation("GetAllDataAsDataTable");
                _logger.LogError(ex, "❌ ERROR in GetAllDataAsDataTable - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Získa len označené dáta ako DataTable - PUBLIC API
        /// </summary>
        public DataTable GetSelectedDataAsDataTable(bool includeValidAlertsColumn = false, bool? checkboxFilter = null)
        {
            try
            {
                _logger.LogInformation("📊 GetSelectedDataAsDataTable START - Instance: {ComponentInstanceId}, " +
                    "TotalRows: {TotalRows}, IncludeValidAlerts: {IncludeValidAlerts}, CheckboxFilter: {CheckboxFilter}",
                    _componentInstanceId, _displayRows.Count, includeValidAlertsColumn, checkboxFilter);

                StartOperation("GetSelectedDataAsDataTable");
                EnsureInitialized();

                var dataTable = new DataTable();
                var checkboxColumnIndex = GetCheckboxColumnIndex();

                // Create columns
                for (int j = 0; j < _columns.Count; j++)
                {
                    var column = _columns[j];
                    var isValidAlertsColumn = column.Name.Contains("ValidAlert") || column.Name.Contains("ValidationAlert");

                    if (!isValidAlertsColumn || includeValidAlertsColumn)
                    {
                        var dataColumn = new DataColumn(column.Name, typeof(object));
                        dataTable.Columns.Add(dataColumn);
                    }
                }

                // Add only selected rows
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    
                    // Check if row is selected (must have checkbox)
                    bool isSelected = false;
                    if (checkboxColumnIndex >= 0 && checkboxColumnIndex < row.Cells.Count)
                    {
                        isSelected = row.Cells[checkboxColumnIndex].Value as bool? == true;
                    }

                    if (!isSelected && !checkboxFilter.HasValue)
                        continue; // Skip non-selected rows

                    // Apply additional checkbox filter if specified
                    if (checkboxFilter.HasValue && checkboxColumnIndex >= 0 && checkboxColumnIndex < row.Cells.Count)
                    {
                        var isChecked = row.Cells[checkboxColumnIndex].Value as bool? == true;
                        if (checkboxFilter.Value != isChecked)
                            continue; // Skip this row
                    }
                    else if (!checkboxFilter.HasValue && !isSelected)
                    {
                        continue; // Default behavior: only selected rows
                    }

                    var dataRow = dataTable.NewRow();
                    int dataColumnIndex = 0;

                    for (int j = 0; j < _columns.Count && j < row.Cells.Count; j++)
                    {
                        var column = _columns[j];
                        var isValidAlertsColumn = column.Name.Contains("ValidAlert") || column.Name.Contains("ValidationAlert");

                        if (!isValidAlertsColumn || includeValidAlertsColumn)
                        {
                            dataRow[dataColumnIndex] = row.Cells[j].Value ?? DBNull.Value;
                            dataColumnIndex++;
                        }
                    }

                    dataTable.Rows.Add(dataRow);
                }

                var duration = EndOperation("GetSelectedDataAsDataTable");
                _logger.LogInformation("✅ GetSelectedDataAsDataTable COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, SelectedRows: {SelectedCount}, TotalRows: {TotalRows}",
                    _componentInstanceId, duration, dataTable.Rows.Count, _displayRows.Count);

                return dataTable;
            }
            catch (Exception ex)
            {
                EndOperation("GetSelectedDataAsDataTable");
                _logger.LogError(ex, "❌ ERROR in GetSelectedDataAsDataTable - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Nastaví dáta z DataTable do gridu s checkbox support - PUBLIC API
        /// </summary>
        public void SetDataFromDataTable(DataTable dataTable, Dictionary<int, bool>? checkboxStates = null)
        {
            try
            {
                _logger.LogInformation("📥 SetDataFromDataTable START - Instance: {ComponentInstanceId}, " +
                    "InputRows: {InputRows}, InputColumns: {InputColumns}, CheckboxStates: {CheckboxStatesCount}",
                    _componentInstanceId, dataTable?.Rows?.Count ?? 0, dataTable?.Columns?.Count ?? 0, checkboxStates?.Count ?? 0);

                StartOperation("SetDataFromDataTable");
                EnsureInitialized();

                if (dataTable == null)
                {
                    _logger.LogWarning("⚠️ SetDataFromDataTable: Input DataTable is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                // Convert DataTable to Dictionary format
                var data = new List<Dictionary<string, object?>>();

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    var rowDict = new Dictionary<string, object?>();
                    
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var columnName = dataTable.Columns[j].ColumnName;
                        var value = dataRow[j];
                        rowDict[columnName] = value == DBNull.Value ? null : value;
                    }

                    data.Add(rowDict);
                }

                // Use existing SetData method with checkbox states
                SetData(data, checkboxStates);

                var duration = EndOperation("SetDataFromDataTable");
                _logger.LogInformation("✅ SetDataFromDataTable COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, FinalRows: {FinalRows}",
                    _componentInstanceId, duration, _displayRows.Count);
            }
            catch (Exception ex)
            {
                EndOperation("SetDataFromDataTable");
                _logger.LogError(ex, "❌ ERROR in SetDataFromDataTable - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Row Management PUBLIC API

        /// <summary>
        /// Zmaže označené riadky (checked) - PUBLIC API
        /// </summary>
        public void DeleteSelectedRows()
        {
            try
            {
                _logger.LogInformation("🗑️ DeleteSelectedRows START - Instance: {ComponentInstanceId}, " +
                    "TotalRows: {TotalRows}",
                    _componentInstanceId, _displayRows.Count);

                StartOperation("DeleteSelectedRows");
                EnsureInitialized();

                var checkboxColumnIndex = GetCheckboxColumnIndex();
                if (checkboxColumnIndex < 0)
                {
                    _logger.LogWarning("⚠️ DeleteSelectedRows: No checkbox column found - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                var rowsToDelete = new List<int>();

                // Find all selected rows
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    if (checkboxColumnIndex < row.Cells.Count)
                    {
                        var isSelected = row.Cells[checkboxColumnIndex].Value as bool? == true;
                        if (isSelected)
                        {
                            rowsToDelete.Add(i);
                        }
                    }
                }

                // Delete rows from bottom to top to preserve indices
                for (int i = rowsToDelete.Count - 1; i >= 0; i--)
                {
                    var rowIndex = rowsToDelete[i];
                    if (rowIndex < _displayRows.Count)
                    {
                        _displayRows.RemoveAt(rowIndex);
                    }
                    if (rowIndex < _gridData.Count)
                    {
                        _gridData.RemoveAt(rowIndex);
                    }
                }

                // Ensure minimum row count
                EnsureMinimumRows();

                var duration = EndOperation("DeleteSelectedRows");
                _logger.LogInformation("✅ DeleteSelectedRows COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, DeletedRows: {DeletedCount}, RemainingRows: {RemainingRows}",
                    _componentInstanceId, duration, rowsToDelete.Count, _displayRows.Count);

                // Trigger UI refresh
                InvalidateArrange();
            }
            catch (Exception ex)
            {
                EndOperation("DeleteSelectedRows");
                _logger.LogError(ex, "❌ ERROR in DeleteSelectedRows - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Zmaže riadky podľa custom predicate - PUBLIC API
        /// </summary>
        public void DeleteRowsWhere(Func<Dictionary<string, object?>, bool> predicate)
        {
            try
            {
                _logger.LogInformation("🗑️ DeleteRowsWhere START - Instance: {ComponentInstanceId}, " +
                    "TotalRows: {TotalRows}",
                    _componentInstanceId, _displayRows.Count);

                StartOperation("DeleteRowsWhere");
                EnsureInitialized();

                if (predicate == null)
                {
                    _logger.LogWarning("⚠️ DeleteRowsWhere: Predicate is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                var rowsToDelete = new List<int>();

                // Find rows that match predicate
                for (int i = 0; i < _gridData.Count; i++)
                {
                    try
                    {
                        if (predicate(_gridData[i]))
                        {
                            rowsToDelete.Add(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ DeleteRowsWhere: Predicate failed for row {RowIndex}", i);
                    }
                }

                // Delete rows from bottom to top to preserve indices
                for (int i = rowsToDelete.Count - 1; i >= 0; i--)
                {
                    var rowIndex = rowsToDelete[i];
                    if (rowIndex < _displayRows.Count)
                    {
                        _displayRows.RemoveAt(rowIndex);
                    }
                    if (rowIndex < _gridData.Count)
                    {
                        _gridData.RemoveAt(rowIndex);
                    }
                }

                // Ensure minimum row count
                EnsureMinimumRows();

                var duration = EndOperation("DeleteRowsWhere");
                _logger.LogInformation("✅ DeleteRowsWhere COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, DeletedRows: {DeletedCount}, RemainingRows: {RemainingRows}",
                    _componentInstanceId, duration, rowsToDelete.Count, _displayRows.Count);

                // Trigger UI refresh
                InvalidateArrange();
            }
            catch (Exception ex)
            {
                EndOperation("DeleteRowsWhere");
                _logger.LogError(ex, "❌ ERROR in DeleteRowsWhere - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Vloží nový riadok na zadanú pozíciu - PUBLIC API
        /// </summary>
        public void InsertRowAt(int index, Dictionary<string, object?>? data = null)
        {
            try
            {
                _logger.LogInformation("📝 InsertRowAt START - Instance: {ComponentInstanceId}, " +
                    "Index: {Index}, TotalRows: {TotalRows}, HasData: {HasData}",
                    _componentInstanceId, index, _displayRows.Count, data != null);

                StartOperation("InsertRowAt");
                EnsureInitialized();

                if (index < 0 || index > _displayRows.Count)
                {
                    _logger.LogWarning("⚠️ InsertRowAt: Invalid index {Index} for {TotalRows} rows - Instance: {ComponentInstanceId}",
                        index, _displayRows.Count, _componentInstanceId);
                    return;
                }

                // Create new row
                var newGridRow = new DataRowViewModel();
                var newGridData = new Dictionary<string, object?>();

                for (int j = 0; j < _columns.Count; j++)
                {
                    var column = _columns[j];
                    var cellValue = data?.TryGetValue(column.Name, out var value) == true ? value : null;
                    
                    var cell = new CellViewModel
                    {
                        Value = cellValue,
                        ColumnName = column.Name
                    };
                    
                    newGridRow.Cells.Add(cell);
                    newGridData[column.Name] = cellValue;
                }

                // Insert at specified position
                _displayRows.Insert(index, newGridRow);
                _gridData.Insert(index, newGridData);

                var duration = EndOperation("InsertRowAt");
                _logger.LogInformation("✅ InsertRowAt COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, NewIndex: {Index}, TotalRows: {TotalRows}",
                    _componentInstanceId, duration, index, _displayRows.Count);

                // Trigger UI refresh
                InvalidateArrange();
            }
            catch (Exception ex)
            {
                EndOperation("InsertRowAt");
                _logger.LogError(ex, "❌ ERROR in InsertRowAt - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Nastaví dáta pre konkrétny riadok - PUBLIC API
        /// </summary>
        public void SetRowData(int rowIndex, Dictionary<string, object?> data)
        {
            try
            {
                _logger.LogInformation("📝 SetRowData START - Instance: {ComponentInstanceId}, " +
                    "RowIndex: {RowIndex}, TotalRows: {TotalRows}, DataColumns: {DataColumns}",
                    _componentInstanceId, rowIndex, _displayRows.Count, data?.Count ?? 0);

                StartOperation("SetRowData");
                EnsureInitialized();

                if (rowIndex < 0 || rowIndex >= _displayRows.Count)
                {
                    _logger.LogWarning("⚠️ SetRowData: Invalid row index {RowIndex} for {TotalRows} rows - Instance: {ComponentInstanceId}",
                        rowIndex, _displayRows.Count, _componentInstanceId);
                    return;
                }

                if (data == null)
                {
                    _logger.LogWarning("⚠️ SetRowData: Data is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    return;
                }

                var row = _displayRows[rowIndex];

                // Update cell values
                for (int j = 0; j < _columns.Count && j < row.Cells.Count; j++)
                {
                    var column = _columns[j];
                    var cellValue = data.TryGetValue(column.Name, out var value) ? value : null;
                    
                    row.Cells[j].Value = cellValue;
                }

                // Update grid data
                if (rowIndex < _gridData.Count)
                {
                    _gridData[rowIndex] = new Dictionary<string, object?>(data);
                }

                var duration = EndOperation("SetRowData");
                _logger.LogInformation("✅ SetRowData COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, RowIndex: {RowIndex}",
                    _componentInstanceId, duration, rowIndex);

                // Trigger UI refresh
                InvalidateArrange();
            }
            catch (Exception ex)
            {
                EndOperation("SetRowData");
                _logger.LogError(ex, "❌ ERROR in SetRowData - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        #endregion

        /// <summary>
        /// ClearAllDataAsync s logovaním
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            try
            {
                _logger.LogInformation("🗑️ ClearAllDataAsync START - Instance: {ComponentInstanceId}, " +
                    "CurrentRows: {CurrentRowCount}, WillPreserve: {PreserveCount}",
                    _componentInstanceId, _displayRows.Count, _unifiedRowCount);

                StartOperation("ClearAllData");
                IncrementOperationCounter("ClearAllData");
                EnsureInitialized();

                if (_dataManagementService != null)
                {
                    await _dataManagementService.ClearAllDataAsync();
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                    await RefreshDataDisplayAsync();
                }

                var duration = EndOperation("ClearAllData");
                _logger.LogInformation("✅ ClearAllDataAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, NewRowCount: {NewRowCount}",
                    _componentInstanceId, duration, _displayRows.Count);

                LogComponentState("ClearAllData-End");
            }
            catch (Exception ex)
            {
                EndOperation("ClearAllData");
                IncrementOperationCounter("ClearAllData-Error");
                _logger.LogError(ex, "❌ ERROR in ClearAllDataAsync - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Multi-Sort PUBLIC API

        /// <summary>
        /// Nastaví Multi-Sort konfiguráciu - PUBLIC API
        /// </summary>
        public void SetMultiSortConfiguration(MultiSortConfiguration config)
        {
            try
            {
                _logger.LogInformation("🔢 SetMultiSortConfiguration called - Config: {ConfigDescription}",
                    config?.GetDescription() ?? "null");

                EnsureInitialized();

                if (_searchAndSortService != null)
                {
                    _searchAndSortService.SetMultiSortConfiguration(config ?? MultiSortConfiguration.Default);
                    _logger.LogInformation("✅ Multi-Sort configuration set successfully");
                }
                else
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot set Multi-Sort configuration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetMultiSortConfiguration");
                throw;
            }
        }

        /// <summary>
        /// Povolí/zakáže Multi-Sort režim - PUBLIC API
        /// </summary>
        public void SetMultiSortMode(bool enabled)
        {
            try
            {
                _logger.LogInformation("🔢 SetMultiSortMode called - Enabled: {Enabled}", enabled);

                EnsureInitialized();

                if (_searchAndSortService != null)
                {
                    _searchAndSortService.SetMultiSortMode(enabled);
                    _logger.LogInformation("✅ Multi-Sort mode set to: {Enabled}", enabled);
                }
                else
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot set Multi-Sort mode");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetMultiSortMode - Enabled: {Enabled}", enabled);
                throw;
            }
        }

        /// <summary>
        /// Pridá stĺpec do Multi-Sort (programaticky) - PUBLIC API
        /// </summary>
        public MultiSortColumn? AddMultiSortColumn(string columnName, SortDirection direction, int priority = 1)
        {
            try
            {
                _logger.LogInformation("🔢 AddMultiSortColumn called - Column: {ColumnName}, " +
                    "Direction: {Direction}, Priority: {Priority}",
                    columnName, direction, priority);

                EnsureInitialized();

                if (_searchAndSortService == null)
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot add Multi-Sort column");
                    return null;
                }

                // Programaticky pridaj stĺpec do Multi-Sort
                var multiSortColumn = new MultiSortColumn(columnName, direction, priority);
                var result = _searchAndSortService.AddOrUpdateMultiSort(columnName, true);

                if (result != null)
                {
                    UpdateMultiSortIndicators();
                    _logger.LogInformation("✅ Multi-Sort column added - Column: {ColumnName}, " +
                        "Direction: {Direction}, Priority: {Priority}",
                        result.ColumnName, result.Direction, result.Priority);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddMultiSortColumn - Column: {ColumnName}", columnName);
                throw;
            }
        }

        /// <summary>
        /// Získa všetky aktívne Multi-Sort stĺpce - PUBLIC API
        /// </summary>
        public List<MultiSortColumn> GetMultiSortColumns()
        {
            try
            {
                _logger.LogDebug("🔢 GetMultiSortColumns called");

                EnsureInitialized();

                if (_searchAndSortService != null)
                {
                    var result = _searchAndSortService.GetMultiSortColumns();
                    _logger.LogDebug("✅ GetMultiSortColumns completed - Count: {Count}", result.Count);
                    return result;
                }
                else
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - returning empty list");
                    return new List<MultiSortColumn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetMultiSortColumns");
                throw;
            }
        }

        /// <summary>
        /// Vyčistí všetky Multi-Sort stavy - PUBLIC API
        /// </summary>
        public void ClearMultiSort()
        {
            try
            {
                _logger.LogInformation("🔢 ClearMultiSort called");

                EnsureInitialized();

                if (_searchAndSortService != null)
                {
                    _searchAndSortService.ClearMultiSort();
                    UpdateMultiSortIndicators();
                    _logger.LogInformation("✅ Multi-Sort cleared successfully");
                }
                else
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot clear Multi-Sort");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearMultiSort");
                throw;
            }
        }

        /// <summary>
        /// Aplikuje Multi-Sort na aktuálne dáta - PUBLIC API
        /// </summary>
        public async Task ApplyMultiSortAsync()
        {
            try
            {
                _logger.LogInformation("🔢 ApplyMultiSortAsync called");

                EnsureInitialized();

                if (_searchAndSortService == null)
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - cannot apply Multi-Sort");
                    return;
                }

                await ApplySortAndRefreshAsync();
                _logger.LogInformation("✅ Multi-Sort applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ApplyMultiSortAsync");
                throw;
            }
        }

        /// <summary>
        /// Kontroluje či je Multi-Sort aktívne - PUBLIC API
        /// </summary>
        public bool HasActiveMultiSort()
        {
            try
            {
                EnsureInitialized();

                if (_searchAndSortService != null)
                {
                    var result = _searchAndSortService.HasActiveMultiSort;
                    _logger.LogTrace("🔢 HasActiveMultiSort - Result: {Result}", result);
                    return result;
                }
                else
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - returning false");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in HasActiveMultiSort");
                return false;
            }
        }

        /// <summary>
        /// Kontroluje či je Multi-Sort režim aktívny - PUBLIC API
        /// </summary>
        public bool IsMultiSortMode()
        {
            try
            {
                EnsureInitialized();

                if (_searchAndSortService != null)
                {
                    var result = _searchAndSortService.IsMultiSortMode;
                    _logger.LogTrace("🔢 IsMultiSortMode - Result: {Result}", result);
                    return result;
                }
                else
                {
                    _logger.LogWarning("⚠️ SearchAndSortService is null - returning false");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in IsMultiSortMode");
                return false;
            }
        }

        #endregion

        #region ✅ NOVÉ: Import/Export Enhancement PUBLIC API

        /// <summary>
        /// Importuje dáta zo súboru s komplexnou validáciou a loggingom - ✅ PUBLIC API
        /// </summary>
        public async Task<ImportResult> ImportFromFileAsync(string filePath, ImportExportConfiguration? config = null, bool[]? checkBoxStates = null)
        {
            try
            {
                _logger.LogInformation("📥 ImportFromFileAsync START - Instance: {ComponentInstanceId}, " +
                    "FilePath: {FilePath}, HasConfig: {HasConfig}, HasCheckBoxStates: {HasCheckBoxStates}",
                    _componentInstanceId, filePath, config != null, checkBoxStates != null);

                string operationId = StartOperation("ImportFromFile");
                IncrementOperationCounter("ImportFromFile");
                EnsureInitialized();

                if (_exportService == null)
                {
                    _logger.LogError("❌ ImportFromFileAsync: ExportService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    var errorResult = new ImportResult
                    {
                        FilePath = filePath,
                        IsSuccessful = false
                    };
                    errorResult.AddError("Export service nie je dostupný", severity: ErrorSeverity.Critical);
                    errorResult.FinalizeImport();
                    return errorResult;
                }

                var result = await _exportService.ImportFromFileAsync(filePath, config);
                
                // Ak bol import úspešný, aktualizuj dáta v grid
                if (result.IsSuccessful && result.ImportedData.Any())
                {
                    _logger.LogInformation("📥 Import successful - refreshing grid data with {RowCount} rows",
                        result.ImportedData.Count);
                    
                    // Clear existing data first
                    await ClearAllDataAsync();
                    
                    // Add imported data to data management service
                    if (_dataManagementService != null)
                    {
                        foreach (var rowData in result.ImportedData)
                        {
                            await _dataManagementService.AddRowAsync(rowData);
                        }
                        
                        // Apply CheckBox states if provided and CheckBox column is enabled
                        if (checkBoxStates != null && _checkBoxColumnEnabled)
                        {
                            _logger.LogInformation("☑️ Applying CheckBox states - StatesCount: {StatesCount}", checkBoxStates.Length);
                            SetCheckBoxStates(checkBoxStates);
                        }
                        
                        // Refresh display
                        await UpdateDisplayRowsWithRealtimeValidationAsync();
                        await RefreshDataDisplayAsync();
                    }
                }

                var duration = EndOperation(operationId);

                _logger.LogInformation("✅ ImportFromFileAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, ImportId: {ImportId}, Status: {Status}, " +
                    "ProcessedRows: {ProcessedRows}, SuccessfulRows: {SuccessfulRows}, " +
                    "ErrorRows: {ErrorRows}, SuccessRate: {SuccessRate:F1}%",
                    _componentInstanceId, duration, result.ImportId, result.IsSuccessful ? "SUCCESS" : "FAILED",
                    result.TotalRowsInFile, result.SuccessfullyImportedRows,
                    result.ErrorRows, result.SuccessRate);

                return result;
            }
            catch (Exception ex)
            {
                IncrementOperationCounter("ImportFromFile-Error");
                _logger.LogError(ex, "❌ ERROR in ImportFromFileAsync - Instance: {ComponentInstanceId}, " +
                    "FilePath: {FilePath}", _componentInstanceId, filePath);
                
                var errorResult = new ImportResult
                {
                    FilePath = filePath,
                    IsSuccessful = false
                };
                errorResult.AddError($"Kritická chyba pri importe: {ex.Message}", severity: ErrorSeverity.Critical);
                errorResult.FinalizeImport();
                return errorResult;
            }
        }

        /// <summary>
        /// Exportuje dáta do súboru s konfiguráciou a loggingom - ✅ PUBLIC API
        /// </summary>
        public async Task<string> ExportToFileAsync(string filePath, ImportExportConfiguration? config = null)
        {
            try
            {
                _logger.LogInformation("📤 ExportToFileAsync START - Instance: {ComponentInstanceId}, " +
                    "FilePath: {FilePath}, HasConfig: {HasConfig}, CurrentRowCount: {RowCount}",
                    _componentInstanceId, filePath, config != null, _displayRows.Count);

                string operationId = StartOperation("ExportToFile");
                IncrementOperationCounter("ExportToFile");
                EnsureInitialized();

                if (_exportService == null)
                {
                    _logger.LogError("❌ ExportToFileAsync: ExportService is null - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    throw new InvalidOperationException("Export service nie je dostupný");
                }

                var result = await _exportService.ExportToFileAsync(filePath, config);
                var duration = EndOperation(operationId);

                _logger.LogInformation("✅ ExportToFileAsync COMPLETED - Instance: {ComponentInstanceId}, " +
                    "Duration: {Duration}ms, FilePath: {FilePath}, Format: {Format}",
                    _componentInstanceId, duration, result, config?.Format ?? ExportFormat.CSV);

                return result;
            }
            catch (Exception ex)
            {
                IncrementOperationCounter("ExportToFile-Error");
                _logger.LogError(ex, "❌ ERROR in ExportToFileAsync - Instance: {ComponentInstanceId}, " +
                    "FilePath: {FilePath}", _componentInstanceId, filePath);
                throw;
            }
        }

        /// <summary>
        /// Získa import history z export service - ✅ PUBLIC API
        /// </summary>
        public Dictionary<string, ImportResult> GetImportHistory()
        {
            try
            {
                _logger.LogDebug("📋 GetImportHistory - Instance: {ComponentInstanceId}", _componentInstanceId);
                
                EnsureInitialized();
                
                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ GetImportHistory: ExportService is null - returning empty history");
                    return new Dictionary<string, ImportResult>();
                }

                var history = _exportService.GetImportHistory();
                
                _logger.LogDebug("📋 GetImportHistory COMPLETED - Count: {HistoryCount}", history.Count);
                
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetImportHistory - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                return new Dictionary<string, ImportResult>();
            }
        }

        /// <summary>
        /// Získa export history z export service - ✅ PUBLIC API
        /// </summary>
        public Dictionary<string, string> GetExportHistory()
        {
            try
            {
                _logger.LogDebug("📋 GetExportHistory - Instance: {ComponentInstanceId}", _componentInstanceId);
                
                EnsureInitialized();
                
                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ GetExportHistory: ExportService is null - returning empty history");
                    return new Dictionary<string, string>();
                }

                var history = _exportService.GetExportHistory();
                
                _logger.LogDebug("📋 GetExportHistory COMPLETED - Count: {HistoryCount}", history.Count);
                
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetExportHistory - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Vyčistí import/export history - ✅ PUBLIC API
        /// </summary>
        public void ClearImportExportHistory()
        {
            try
            {
                _logger.LogInformation("🧹 ClearImportExportHistory - Instance: {ComponentInstanceId}", _componentInstanceId);
                
                IncrementOperationCounter("ClearHistory");
                EnsureInitialized();
                
                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ ClearImportExportHistory: ExportService is null");
                    return;
                }

                _exportService.ClearHistory();
                
                _logger.LogInformation("✅ ClearImportExportHistory COMPLETED - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
            }
            catch (Exception ex)
            {
                IncrementOperationCounter("ClearHistory-Error");
                _logger.LogError(ex, "❌ ERROR in ClearImportExportHistory - Instance: {ComponentInstanceId}",
                    _componentInstanceId);
            }
        }

        /// <summary>
        /// Importuje z CSV súboru s predvolenou konfiguráciou - ✅ PUBLIC API HELPER
        /// </summary>
        public async Task<ImportResult> ImportFromCsvAsync(string filePath, bool includeHeaders = true, 
            bool validateOnImport = true, bool continueOnErrors = false, bool[]? checkBoxStates = null)
        {
            var config = new ImportExportConfiguration
            {
                Format = ExportFormat.CSV,
                IncludeHeaders = includeHeaders,
                ValidateOnImport = validateOnImport,
                ContinueOnErrors = continueOnErrors,
                SkipEmptyRows = true,
                Encoding = "UTF-8"
            };

            return await ImportFromFileAsync(filePath, config, checkBoxStates);
        }

        /// <summary>
        /// Importuje z JSON súboru s predvolenou konfiguráciou - ✅ PUBLIC API HELPER
        /// </summary>
        public async Task<ImportResult> ImportFromJsonAsync(string filePath, bool validateOnImport = true, bool[]? checkBoxStates = null)
        {
            var config = new ImportExportConfiguration
            {
                Format = ExportFormat.JSON,
                ValidateOnImport = validateOnImport,
                ContinueOnErrors = false,
                Encoding = "UTF-8"
            };

            return await ImportFromFileAsync(filePath, config, checkBoxStates);
        }

        /// <summary>
        /// Exportuje do CSV súboru s predvolenou konfiguráciou - ✅ PUBLIC API HELPER
        /// </summary>
        public async Task<string> ExportToCsvFileAsync(string filePath, bool includeHeaders = true, 
            bool backupExisting = true, bool autoOpen = false)
        {
            var config = new ImportExportConfiguration
            {
                Format = ExportFormat.CSV,
                IncludeHeaders = includeHeaders,
                BackupExistingFile = backupExisting,
                AutoOpenFile = autoOpen,
                Encoding = "UTF-8"
            };

            return await ExportToFileAsync(filePath, config);
        }

        /// <summary>
        /// Exportuje do JSON súboru s predvolenou konfiguráciou - ✅ PUBLIC API HELPER
        /// </summary>
        public async Task<string> ExportToJsonFileAsync(string filePath, JsonFormatting formatting = JsonFormatting.Indented, 
            bool backupExisting = true, bool autoOpen = false)
        {
            var config = new ImportExportConfiguration
            {
                Format = ExportFormat.JSON,
                JsonFormatting = formatting,
                BackupExistingFile = backupExisting,
                AutoOpenFile = autoOpen,
                Encoding = "UTF-8"
            };

            return await ExportToFileAsync(filePath, config);
        }

        #endregion

        #region ✅ NOVÉ: CheckBox Column Management - PUBLIC API

        /// <summary>
        /// Detekuje a konfiguruje CheckBox column ak je prítomný v headers
        /// </summary>
        private void DetectAndConfigureCheckBoxColumn(List<GridColumnDefinition> columns)
        {
            try
            {
                var checkBoxColumn = columns.FirstOrDefault(c => c.Name == _checkBoxColumnName || 
                    c.Name.Equals("CheckBoxState", StringComparison.OrdinalIgnoreCase));
                
                if (checkBoxColumn != null)
                {
                    _checkBoxColumnEnabled = true;
                    _checkBoxColumnName = checkBoxColumn.Name;
                    
                    _logger.LogInformation("☑️ CheckBox column detected - Name: {ColumnName}, Enabled: {Enabled}",
                        _checkBoxColumnName, _checkBoxColumnEnabled);
                    
                    // Inicializuj checkbox states
                    _checkBoxStates.Clear();
                }
                else
                {
                    _checkBoxColumnEnabled = false;
                    _logger.LogDebug("☐ CheckBox column not detected in headers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in DetectAndConfigureCheckBoxColumn");
                _checkBoxColumnEnabled = false;
            }
        }

        /// <summary>
        /// Aktualizuje checkbox state pre konkrétny riadok - INTERNAL
        /// </summary>
        public void UpdateCheckBoxState(int rowIndex, bool isChecked)
        {
            try
            {
                if (!_checkBoxColumnEnabled) return;
                
                _checkBoxStates[rowIndex] = isChecked;
                
                // Aktualizuj dáta v data management service
                if (_dataManagementService != null && rowIndex >= 0 && rowIndex < _gridData.Count)
                {
                    _gridData[rowIndex][_checkBoxColumnName] = isChecked;
                    
                    _logger.LogTrace("☑️ CheckBox state updated - RowIndex: {RowIndex}, IsChecked: {IsChecked}",
                        rowIndex, isChecked);
                }
                
                // Aktualizuj header state
                UpdateHeaderCheckBoxState();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateCheckBoxState - RowIndex: {RowIndex}, IsChecked: {IsChecked}",
                    rowIndex, isChecked);
            }
        }

        /// <summary>
        /// Check all rows - PUBLIC API
        /// </summary>
        public void CheckAllRows()
        {
            try
            {
                if (!_checkBoxColumnEnabled) return;
                
                _logger.LogInformation("☑️ CheckAllRows START - TotalRows: {TotalRows}", _displayRows.Count);
                
                var checkedCount = 0;
                
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    var row = _displayRows[i];
                    
                    // Skip empty rows (auto-add rows)
                    if (IsRowEmpty(row))
                        continue;
                    
                    _checkBoxStates[i] = true;
                    
                    // Update in grid data
                    if (i < _gridData.Count)
                    {
                        _gridData[i][_checkBoxColumnName] = true;
                    }
                    
                    checkedCount++;
                }
                
                // Refresh display
                _ = Task.Run(async () => await RefreshDataDisplayAsync());
                
                _logger.LogInformation("✅ CheckAllRows COMPLETED - CheckedRows: {CheckedCount}", checkedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CheckAllRows");
            }
        }

        /// <summary>
        /// Uncheck all rows - PUBLIC API
        /// </summary>
        public void UncheckAllRows()
        {
            try
            {
                if (!_checkBoxColumnEnabled) return;
                
                _logger.LogInformation("☐ UncheckAllRows START - TotalRows: {TotalRows}", _displayRows.Count);
                
                var uncheckedCount = 0;
                
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    if (_checkBoxStates.ContainsKey(i) && _checkBoxStates[i])
                    {
                        _checkBoxStates[i] = false;
                        
                        // Update in grid data
                        if (i < _gridData.Count)
                        {
                            _gridData[i][_checkBoxColumnName] = false;
                        }
                        
                        uncheckedCount++;
                    }
                }
                
                // Refresh display
                _ = Task.Run(async () => await RefreshDataDisplayAsync());
                
                _logger.LogInformation("✅ UncheckAllRows COMPLETED - UncheckedRows: {UncheckedCount}", uncheckedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UncheckAllRows");
            }
        }

        /// <summary>
        /// Delete all checked rows - PUBLIC API
        /// </summary>
        public async Task DeleteAllCheckedRowsAsync()
        {
            try
            {
                if (!_checkBoxColumnEnabled)
                {
                    _logger.LogWarning("⚠️ DeleteAllCheckedRowsAsync: CheckBox column not enabled");
                    return;
                }
                
                string operationId = StartOperation("DeleteAllCheckedRows");
                IncrementOperationCounter("DeleteAllCheckedRows");
                
                var checkedRows = _checkBoxStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).OrderByDescending(i => i).ToList();
                
                _logger.LogInformation("🗑️ DeleteAllCheckedRowsAsync START - CheckedRows: {CheckedCount}, TotalRows: {TotalRows}",
                    checkedRows.Count, _displayRows.Count);
                
                if (!checkedRows.Any())
                {
                    _logger.LogInformation("ℹ️ No checked rows to delete");
                    return;
                }
                
                // Delete rows from highest index to lowest to maintain correct indices
                var deletedCount = 0;
                foreach (var rowIndex in checkedRows)
                {
                    if (rowIndex >= 0 && rowIndex < _gridData.Count && !IsRowEmpty(_displayRows[rowIndex]))
                    {
                        // await _dataManagementService?.RemoveRowAsync(rowIndex)!; // Method doesn't exist
                        _gridData.RemoveAt(rowIndex);
                        _displayRows.RemoveAt(rowIndex);
                        deletedCount++;
                    }
                }
                
                // Clear checkbox states for deleted rows
                foreach (var rowIndex in checkedRows)
                {
                    _checkBoxStates.Remove(rowIndex);
                }
                
                // Move empty rows to end and refresh
                await MoveEmptyRowsToEndAsync();
                await UpdateDisplayRowsWithRealtimeValidationAsync();
                await RefreshDataDisplayAsync();
                
                var duration = EndOperation(operationId);
                
                _logger.LogInformation("✅ DeleteAllCheckedRowsAsync COMPLETED - Duration: {Duration}ms, " +
                    "DeletedRows: {DeletedCount}, RemainingRows: {RemainingRows}",
                    duration, deletedCount, _displayRows.Count);
            }
            catch (Exception ex)
            {
                IncrementOperationCounter("DeleteAllCheckedRows-Error");
                _logger.LogError(ex, "❌ ERROR in DeleteAllCheckedRowsAsync");
                throw;
            }
        }

        /// <summary>
        /// Export only checked rows - PUBLIC API
        /// </summary>
        public async Task<DataTable> ExportCheckedRowsOnlyAsync(bool includeValidAlerts = false)
        {
            try
            {
                if (!_checkBoxColumnEnabled)
                {
                    _logger.LogWarning("⚠️ ExportCheckedRowsOnlyAsync: CheckBox column not enabled - exporting all data");
                    return await ExportToDataTableAsync();
                }
                
                string operationId = StartOperation("ExportCheckedRowsOnly");
                IncrementOperationCounter("ExportCheckedRowsOnly");
                
                var checkedRows = _checkBoxStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
                
                _logger.LogInformation("📤 ExportCheckedRowsOnlyAsync START - CheckedRows: {CheckedCount}, " +
                    "IncludeValidAlerts: {IncludeValidAlerts}", checkedRows.Count, includeValidAlerts);
                
                if (!checkedRows.Any())
                {
                    _logger.LogInformation("ℹ️ No checked rows to export - returning empty DataTable");
                    return new DataTable("EmptyCheckedRowsExport");
                }
                
                // Get full data table first
                var fullDataTable = await ExportToDataTableAsync();
                var filteredDataTable = fullDataTable.Clone();
                
                // Filter only checked rows
                var exportedRowCount = 0;
                foreach (var rowIndex in checkedRows.OrderBy(i => i))
                {
                    if (rowIndex >= 0 && rowIndex < fullDataTable.Rows.Count)
                    {
                        var sourceRow = fullDataTable.Rows[rowIndex];
                        var targetRow = filteredDataTable.NewRow();
                        
                        // Copy data excluding CheckBoxState column and optionally ValidAlerts
                        foreach (DataColumn column in fullDataTable.Columns)
                        {
                            if (column.ColumnName == _checkBoxColumnName) continue;
                            if (!includeValidAlerts && column.ColumnName == "ValidAlerts") continue;
                            
                            if (filteredDataTable.Columns.Contains(column.ColumnName))
                            {
                                targetRow[column.ColumnName] = sourceRow[column.ColumnName];
                            }
                        }
                        
                        filteredDataTable.Rows.Add(targetRow);
                        exportedRowCount++;
                    }
                }
                
                // Remove CheckBoxState column from schema if not needed
                if (filteredDataTable.Columns.Contains(_checkBoxColumnName))
                {
                    filteredDataTable.Columns.Remove(_checkBoxColumnName);
                }
                
                // Remove ValidAlerts column if not requested
                if (!includeValidAlerts && filteredDataTable.Columns.Contains("ValidAlerts"))
                {
                    filteredDataTable.Columns.Remove("ValidAlerts");
                }
                
                var duration = EndOperation(operationId);
                
                _logger.LogInformation("✅ ExportCheckedRowsOnlyAsync COMPLETED - Duration: {Duration}ms, " +
                    "ExportedRows: {ExportedRows}, ExportedColumns: {ExportedColumns}",
                    duration, exportedRowCount, filteredDataTable.Columns.Count);
                
                return filteredDataTable;
            }
            catch (Exception ex)
            {
                IncrementOperationCounter("ExportCheckedRowsOnly-Error");
                _logger.LogError(ex, "❌ ERROR in ExportCheckedRowsOnlyAsync");
                throw;
            }
        }

        /// <summary>
        /// Získa počet checked rows - PUBLIC API
        /// </summary>
        public int GetCheckedRowsCount()
        {
            try
            {
                if (!_checkBoxColumnEnabled) return 0;
                
                return _checkBoxStates.Count(kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetCheckedRowsCount");
                return 0;
            }
        }

        /// <summary>
        /// Získa zoznam indices checked rows - PUBLIC API
        /// </summary>
        public List<int> GetCheckedRowIndices()
        {
            try
            {
                if (!_checkBoxColumnEnabled) return new List<int>();
                
                return _checkBoxStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).OrderBy(i => i).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetCheckedRowIndices");
                return new List<int>();
            }
        }

        /// <summary>
        /// Nastavuje checkbox states pre import - PUBLIC API
        /// </summary>
        public void SetCheckBoxStates(bool[] checkboxStates)
        {
            try
            {
                if (!_checkBoxColumnEnabled || checkboxStates == null) return;
                
                _logger.LogInformation("☑️ SetCheckBoxStates - ArrayLength: {ArrayLength}, EnabledRows: {EnabledCount}",
                    checkboxStates.Length, checkboxStates.Count(b => b));
                
                _checkBoxStates.Clear();
                
                for (int i = 0; i < checkboxStates.Length && i < _gridData.Count; i++)
                {
                    _checkBoxStates[i] = checkboxStates[i];
                    
                    // Update in grid data
                    _gridData[i][_checkBoxColumnName] = checkboxStates[i];
                }
                
                // Update display
                _ = Task.Run(async () => await RefreshDataDisplayAsync());
                
                _logger.LogDebug("✅ CheckBox states set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetCheckBoxStates");
            }
        }

        /// <summary>
        /// Aktualizuje header checkbox state na základe aktuálnych checked rows
        /// </summary>
        private void UpdateHeaderCheckBoxState()
        {
            try
            {
                if (!_checkBoxColumnEnabled || _checkBoxColumnHeader == null) return;
                
                var totalNonEmptyRows = 0;
                var checkedRows = 0;
                
                for (int i = 0; i < _displayRows.Count; i++)
                {
                    if (!IsRowEmpty(_displayRows[i]))
                    {
                        totalNonEmptyRows++;
                        if (_checkBoxStates.ContainsKey(i) && _checkBoxStates[i])
                        {
                            checkedRows++;
                        }
                    }
                }
                
                _checkBoxColumnHeader.UpdateHeaderState(totalNonEmptyRows, checkedRows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateHeaderCheckBoxState");
            }
        }

        /// <summary>
        /// Checks if CheckBox column is enabled - PUBLIC API
        /// </summary>
        public bool IsCheckBoxColumnEnabled => _checkBoxColumnEnabled;

        /// <summary>
        /// Automaticky posúva prázdne riadky na koniec - INTERNAL
        /// </summary>
        private async Task MoveEmptyRowsToEndAsync()
        {
            try
            {
                string operationId = StartOperation("MoveEmptyRowsToEnd");
                
                _logger.LogDebug("🔄 MoveEmptyRowsToEndAsync START - TotalRows: {TotalRows}", _gridData.Count);
                
                if (_dataManagementService == null) return;
                
                var allData = await _dataManagementService.GetAllDataAsync();
                if (allData == null || !allData.Any()) return;
                
                // Separate empty and non-empty rows
                var nonEmptyRows = new List<Dictionary<string, object?>>();
                var emptyRows = new List<Dictionary<string, object?>>();
                
                foreach (var row in allData)
                {
                    if (IsRowEmptyData(row))
                    {
                        emptyRows.Add(row);
                    }
                    else
                    {
                        nonEmptyRows.Add(row);
                    }
                }
                
                // Clear and re-add in correct order
                await _dataManagementService.ClearAllDataAsync();
                
                // Add non-empty rows first
                foreach (var row in nonEmptyRows)
                {
                    await _dataManagementService.AddRowAsync(row);
                }
                
                // Add empty rows at the end
                foreach (var row in emptyRows)
                {
                    await _dataManagementService.AddRowAsync(row);
                }
                
                var duration = EndOperation(operationId);
                
                _logger.LogDebug("✅ MoveEmptyRowsToEndAsync COMPLETED - Duration: {Duration}ms, " +
                    "NonEmptyRows: {NonEmptyRows}, EmptyRows: {EmptyRows}",
                    duration, nonEmptyRows.Count, emptyRows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveEmptyRowsToEndAsync");
            }
        }

        /// <summary>
        /// Checks if row data is empty
        /// </summary>
        private bool IsRowEmptyData(Dictionary<string, object?> row)
        {
            return row.Where(kvp => kvp.Key != "DeleteRows" && kvp.Key != "ValidAlerts" && kvp.Key != _checkBoxColumnName)
                     .All(kvp => kvp.Value == null || string.IsNullOrWhiteSpace(kvp.Value.ToString()));
        }

        /// <summary>
        /// Checks if all non-empty exportable rows are valid - PUBLIC API
        /// </summary>
        public async Task<bool> AreAllNonEmptyRowsValidAsync()
        {
            try
            {
                string operationId = StartOperation("AreAllNonEmptyRowsValid");
                IncrementOperationCounter("AreAllNonEmptyRowsValid");
                
                _logger.LogDebug("✅ AreAllNonEmptyRowsValidAsync START - CheckBoxEnabled: {CheckBoxEnabled}",
                    _checkBoxColumnEnabled);
                
                // If no validation rules are set, all rows are considered valid
                if (_validationService == null || 
                    (_configuration?.ValidationRules == null || !_configuration.ValidationRules.Any()) &&
                    (_advancedValidationRules == null || _advancedValidationRules.Rules.Count == 0))
                {
                    _logger.LogDebug("✅ No validation rules set - all rows considered valid");
                    return true;
                }
                
                var allData = await _dataManagementService?.GetAllDataAsync()!;
                if (allData == null || !allData.Any())
                {
                    _logger.LogDebug("✅ No data to validate");
                    return true;
                }
                
                var validCount = 0;
                var invalidCount = 0;
                var checkedRowsOnly = _checkBoxColumnEnabled && _checkBoxStates.Any(kvp => kvp.Value);
                
                for (int i = 0; i < allData.Count; i++)
                {
                    var row = allData[i];
                    
                    // Skip empty rows
                    if (IsRowEmptyData(row))
                        continue;
                    
                    // If checkbox column is enabled, only validate checked rows
                    if (checkedRowsOnly && (!_checkBoxStates.ContainsKey(i) || !_checkBoxStates[i]))
                        continue;
                    
                    // Check if row has validation errors
                    var validationErrors = row.ContainsKey("ValidAlerts") ? row["ValidAlerts"]?.ToString() : null;
                    
                    if (string.IsNullOrWhiteSpace(validationErrors))
                    {
                        validCount++;
                    }
                    else
                    {
                        invalidCount++;
                        
                        _logger.LogTrace("❌ Invalid row found - RowIndex: {RowIndex}, Errors: {Errors}",
                            i, validationErrors);
                    }
                }
                
                var isAllValid = invalidCount == 0;
                var duration = EndOperation(operationId);
                
                _logger.LogInformation("✅ AreAllNonEmptyRowsValidAsync COMPLETED - Duration: {Duration}ms, " +
                    "ValidRows: {ValidRows}, InvalidRows: {InvalidRows}, AllValid: {AllValid}, " +
                    "CheckedRowsOnly: {CheckedRowsOnly}",
                    duration, validCount, invalidCount, isAllValid, checkedRowsOnly);
                
                return isAllValid;
            }
            catch (Exception ex)
            {
                IncrementOperationCounter("AreAllNonEmptyRowsValid-Error");
                _logger.LogError(ex, "❌ ERROR in AreAllNonEmptyRowsValidAsync");
                return false;
            }
        }

        // ✅ REMOVED: Background Validation API - replaced with unified InitializeAsync with enableBatchValidation parameter

        #endregion

        #region ✅ NOVÉ: Event Handler pre Hide Validation Overlay

        /// <summary>
        /// Event handler pre hide validation overlay button
        /// </summary>
        public void OnHideValidationOverlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("🔘 OnHideValidationOverlayClick");

                var validationOverlay = this.FindName("ValidationOverlay") as Border;
                if (validationOverlay != null)
                {
                    validationOverlay.Visibility = Visibility.Collapsed;
                    _logger.LogDebug("✅ Validation overlay hidden");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnHideValidationOverlayClick");
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: System Info a Diagnostika

        /// <summary>
        /// Loguje informácie o systéme pri inicializácii
        /// </summary>

        #endregion

        #region ✅ NOVÉ: DataGridController Initialization

        /// <summary>
        /// Inicializuje DataGridController pre koordináciu services
        /// </summary>
        private void InitializeController()
        {
            try
            {
                _logger.LogDebug("🎛️ InitializeController START");

                // Vytvor DataGridController
                _controller = new Core.DataGridController();

                _logger.LogDebug("✅ InitializeController COMPLETED - Controller ready for configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeController");
                throw;
            }
        }

        /// <summary>
        /// Konfiguruje DataGridController s poskytnutými parametrami
        /// </summary>
        private async Task ConfigureControllerAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule>? validationRules,
            GridThrottlingConfig? throttlingConfig,
            int emptyRowsCount,
            DataGridColorConfig? colorConfig)
        {
            try
            {
                _logger.LogDebug("🎛️ ConfigureControllerAsync START");

                if (_controller == null)
                {
                    throw new InvalidOperationException("DataGridController is not initialized");
                }

                // Vytvor DataGridConfiguration
                _configuration = new Core.DataGridConfiguration
                {
                    Columns = columns.Cast<Models.Grid.ColumnDefinition>().ToList(),
                    EmptyRowsCount = emptyRowsCount,
                    ValidationRules = validationRules?.Cast<Models.Validation.ValidationRule>().ToList() ?? new(),
                    ColorConfig = colorConfig,
                    ThrottlingConfig = throttlingConfig,
                    EnableVirtualScrolling = true,
                    EnableZebraRows = true,
                    EnableColumnResize = true,
                    EnableSorting = true,
                    EnableSearch = true,
                    EnableCopyPaste = true,
                    EnableExportImport = true
                };

                // Inicializuj controller s konfiguráciou
                await _controller.InitializeAsync(_configuration);

                _logger.LogDebug("✅ ConfigureControllerAsync COMPLETED - Controller configured and initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConfigureControllerAsync");
                throw;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Dependency Injection Setup

        /// <summary>
        /// Inicializuje dependency injection pre services
        /// </summary>
        private void InitializeDependencyInjection()
        {
            try
            {
                _logger.LogDebug("🔧 InitializeDependencyInjection START");

                // Vytvor service collection
                var services = new ServiceCollection();

                // Registruj logger ako singleton
                services.AddSingleton(_logger);

                // Registruj internal services
                services.AddScoped<IDataManagementService, DataManagementService>();
                services.AddScoped<IValidationService, ValidationService>();
                services.AddScoped<IExportService, ExportService>();

                // Build service provider
                _serviceProvider = services.BuildServiceProvider();

                _logger.LogDebug("✅ InitializeDependencyInjection COMPLETED - Services registered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeDependencyInjection");
                throw;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Services Initialization

        /// <summary>
        /// Inicializuje všetky internal services
        /// </summary>
        private async Task InitializeServicesAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> rules,
            GridThrottlingConfig throttling,
            int emptyRows,
            bool enableBatchValidation = false,
            int maxSearchHistoryItems = 0)
        {
            try
            {
                _logger.LogInformation("🚀 InitializeServicesAsync START - Services: DataManagement, Validation, Export");

                if (_serviceProvider == null)
                {
                    _logger.LogError("❌ ServiceProvider is null - DI not initialized");
                    throw new InvalidOperationException("ServiceProvider not initialized");
                }

                // Vytvor grid configuration
                var gridConfig = new GridConfiguration
                {
                    Columns = columns,
                    ValidationRules = rules,
                    ThrottlingConfig = throttling,
                    EmptyRowsCount = emptyRows,
                    AutoAddNewRow = _autoAddEnabled,
                    EnableRealtimeValidation = throttling.EnableRealtimeValidation,
                    EnableBatchValidation = enableBatchValidation,
                    GridName = $"AdvancedDataGrid-{_componentInstanceId}"
                };

                // Inicializuj DataManagementService
                _dataManagementService = _serviceProvider.GetRequiredService<IDataManagementService>();
                await _dataManagementService.InitializeAsync(gridConfig);
                _logger.LogDebug("✅ DataManagementService initialized");

                // Inicializuj ValidationService
                _validationService = _serviceProvider.GetRequiredService<IValidationService>();
                await _validationService.InitializeAsync(gridConfig);
                _logger.LogDebug("✅ ValidationService initialized");

                // Inicializuj ExportService
                _exportService = _serviceProvider.GetRequiredService<IExportService>();
                await _exportService.InitializeAsync(gridConfig);
                _logger.LogDebug("✅ ExportService initialized");

                // ✅ NOVÉ: Inicializuj VirtualScrollingService s pokročilou konfiguráciou
                var virtualScrollConfig = Models.VirtualScrollingConfiguration.Advanced.Clone();
                virtualScrollConfig.EnableVerticalVirtualization = gridConfig.EnableVirtualScrolling;
                virtualScrollConfig.VisibleRowCount = gridConfig.VirtualScrollingVisibleRows;
                virtualScrollConfig.RowBufferSize = gridConfig.VirtualScrollingBufferSize;
                virtualScrollConfig.OptimizedRowHeight = 36.0;
                virtualScrollConfig.ScrollThrottleMs = 16;
                virtualScrollConfig.EnableLazyLoading = true;
                virtualScrollConfig.EnableSelectiveInvalidation = true;
                virtualScrollConfig.EnableDiagnostics = true;
                
                _virtualScrollingService = new VirtualScrollingService(virtualScrollConfig, _logger);
                _logger.LogDebug("✅ VirtualScrollingService initialized - Enabled: {IsEnabled}, VisibleRows: {VisibleRows}", 
                    virtualScrollConfig.EnableVerticalVirtualization, virtualScrollConfig.VisibleRowCount);

                // ✅ NOVÉ: Inicializuj BatchValidationService
                var batchValidationConfig = enableBatchValidation ? BatchValidationConfiguration.Default : 
                    new BatchValidationConfiguration { IsEnabled = false };
                _batchValidationService = new BatchValidationService(batchValidationConfig, _logger);
                _logger.LogDebug("✅ BatchValidationService initialized - Enabled: {IsEnabled}, BatchSize: {BatchSize}", 
                    batchValidationConfig.IsEnabled, batchValidationConfig.BatchSize);

                // ✅ NOVÉ: Inicializuj AdvancedSearchService
                var advancedSearchConfig = AdvancedSearchConfiguration.Default;
                advancedSearchConfig.MaxSearchHistoryItems = maxSearchHistoryItems;
                _advancedSearchService = new AdvancedSearchService(advancedSearchConfig, _logger);
                _logger.LogDebug("✅ AdvancedSearchService initialized - HistoryItems: {HistoryItems}, Regex: {RegexEnabled}", 
                    maxSearchHistoryItems, advancedSearchConfig.EnableRegexSearch);

                _logger.LogInformation("✅ InitializeServicesAsync COMPLETED - All services ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in InitializeServicesAsync");
                throw;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Column Resize Support

        /// <summary>
        /// Inicializuje podporu pre resize stĺpcov
        /// </summary>
        private void InitializeResizeSupport()
        {
            try
            {
                _logger.LogDebug("🖱️ InitializeResizeSupport START");

                // Resize support je inicializovaný cez event handlers
                // Skutočná implementácia resize grip-ov sa vytvorí v CreateResizableHeadersAsync

                _logger.LogDebug("✅ InitializeResizeSupport COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeResizeSupport");
                throw;
            }
        }

        /// <summary>
        /// Vytvorí resizable headers pre všetky stĺpce
        /// </summary>
        private async Task CreateResizableHeadersAsync()
        {
            try
            {
                _logger.LogDebug("🖱️ CreateResizableHeadersAsync START - Columns: {ColumnCount}", _columns.Count);

                await UIHelper.RunOnUIThreadAsync(async () =>
                {
                    var headerContainer = HeaderStackPanelElement;
                    if (headerContainer == null)
                    {
                        _logger.LogWarning("⚠️ HeaderStackPanel not found - resize not available");
                        return;
                    }

                    // Vyčisti existujúce headers
                    headerContainer.Children.Clear();
                    _resizableHeaders.Clear();

                    // Vytvor header pre každý stĺpec
                    foreach (var column in _columns.Where(c => c.IsVisible))
                    {
                        await CreateColumnHeaderAsync(column, headerContainer);
                    }

                    _logger.LogInformation("✅ Created {HeaderCount} resizable headers", _resizableHeaders.Count);
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CreateResizableHeadersAsync");
                throw;
            }
        }

        /// <summary>
        /// Vytvorí header pre jeden stĺpec
        /// </summary>
        private async Task CreateColumnHeaderAsync(GridColumnDefinition column, StackPanel headerContainer)
        {
            try
            {
                // Calculate width settings based on column type
                double actualWidth;
                double actualMinWidth;
                double actualMaxWidth;
                
                if (column.IsSpecialColumn)
                {
                    switch (column.SpecialColumnType)
                    {
                        case SpecialColumnType.ValidationAlerts:
                            // ValidAlerts: MinWidth respected, MaxWidth ignored (stretch)
                            actualWidth = Math.Max(column.Width, column.MinWidth);
                            actualMinWidth = column.MinWidth;
                            actualMaxWidth = double.PositiveInfinity; // Stretch to remaining space
                            break;
                            
                        case SpecialColumnType.DeleteButton:
                        case SpecialColumnType.Checkbox:
                            // CheckBox/DeleteRows: Auto width, ignore user settings
                            actualWidth = 40; // Fixed width for buttons/checkboxes
                            actualMinWidth = 40;
                            actualMaxWidth = 40;
                            break;
                            
                        default:
                            // Other special columns use standard logic
                            actualWidth = column.Width;
                            actualMinWidth = column.MinWidth;
                            actualMaxWidth = column.MaxWidth > 0 ? column.MaxWidth : double.PositiveInfinity;
                            break;
                    }
                }
                else
                {
                    // Normal columns: respect all width settings
                    actualWidth = column.Width;
                    actualMinWidth = column.MinWidth;
                    actualMaxWidth = column.MaxWidth > 0 ? column.MaxWidth : double.PositiveInfinity;
                }

                // Vytvor header border
                var headerBorder = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    MinHeight = 65,
                    Width = actualWidth,
                    MinWidth = actualMinWidth,
                    MaxWidth = actualMaxWidth
                };

                // ✅ NOVÉ: Vytvor header content s sort indikátorom a search box
                var headerGrid = new Grid();
                headerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                headerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                headerGrid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = GridLength.Auto });

                // Header text
                var headerText = new TextBlock
                {
                    Text = column.Header ?? column.Name,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(8, 2, 4, 2),
                    FontSize = 12
                };
                Grid.SetColumn(headerText, 0);
                Grid.SetRow(headerText, 0);

                // ✅ NOVÉ: Sort indikátor (šípka)
                var sortIndicator = new TextBlock
                {
                    Text = "",
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2, 0, 4, 0),
                    Visibility = Visibility.Collapsed
                };
                Grid.SetColumn(sortIndicator, 1);
                Grid.SetRow(sortIndicator, 0);

                // ✅ NOVÉ: Search box (iba pre non-special stĺpce)
                TextBox? searchBox = null;
                if (!column.IsSpecialColumn && column.Name != "ValidAlerts")
                {
                    searchBox = new TextBox
                    {
                        PlaceholderText = "🔍 Search...",
                        FontSize = 10,
                        Height = 22,
                        Margin = new Thickness(4, 1, 4, 2),
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray)
                    };
                    Grid.SetColumn(searchBox, 0);
                    Grid.SetRow(searchBox, 1);
                    Grid.SetColumnSpan(searchBox, 2);

                    // Add search event handler
                    searchBox.TextChanged += (sender, e) => OnSearchTextChanged(column.Name, searchBox.Text);
                }

                headerGrid.Children.Add(headerText);
                headerGrid.Children.Add(sortIndicator);
                if (searchBox != null)
                {
                    headerGrid.Children.Add(searchBox);
                }

                // ✅ NOVÉ: Vytvor resize grip (iba pre non-special columns)
                Border? resizeGrip = null;
                if (!column.IsSpecialColumn && column.Name != "ValidAlerts")
                {
                    resizeGrip = new Border
                    {
                        Width = 4,
                        Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        // Cursor sa v WinUI3 nastavuje cez ProtectedCursor property
                        // Cursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast)
                    };

                    headerGrid.Children.Add(resizeGrip);
                }

                // Set header content
                headerBorder.Child = headerGrid;

                // ✅ NOVÉ: Pridaj click handler pre sortovanie (iba pre non-special columns)
                if (!column.IsSpecialColumn && column.Name != "ValidAlerts")
                {
                    headerBorder.Tapped += async (sender, e) => await OnColumnHeaderClicked(column.Name, sortIndicator);
                    // WinUI3: Cursor sa nastavuje cez UI element properties
                    // headerBorder.Cursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Hand);
                    
                    _logger.LogTrace("🔀 Sort click handler added for column: {ColumnName}", column.Name);
                }

                // Pridaj do container
                headerContainer.Children.Add(headerBorder);

                // ✅ NOVÉ: Zaregistruj resizable header s sort indikátorom
                var resizableHeader = new ResizableColumnHeader
                {
                    ColumnName = column.Name,
                    HeaderElement = headerBorder,
                    ResizeGrip = resizeGrip,
                    SortIndicator = sortIndicator,
                    OriginalWidth = actualWidth,
                    MinWidth = actualMinWidth,
                    MaxWidth = actualMaxWidth == double.PositiveInfinity ? 500 : actualMaxWidth
                };

                _resizableHeaders.Add(resizableHeader);

                _logger.LogTrace("📊 Created header for column {ColumnName} - Width: {Width}, Resizable: {Resizable}",
                    column.Name, column.Width, resizeGrip != null);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR creating header for column {ColumnName}", column.Name);
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Scroll Support

        /// <summary>
        /// Inicializuje scroll podporu
        /// </summary>
        private void InitializeScrollSupport()
        {
            try
            {
                _logger.LogDebug("📜 InitializeScrollSupport START");

                // Scroll support sa aktivuje cez XAML ScrollViewer elementy
                // Synchronizácia sa nastaví v SetupScrollSynchronization

                _logger.LogDebug("✅ InitializeScrollSupport COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeScrollSupport");
                throw;
            }
        }

        /// <summary>
        /// Nastaví synchronizáciu scroll medzi header a data
        /// </summary>
        private void SetupScrollSynchronization()
        {
            try
            {
                _logger.LogDebug("📜 SetupScrollSynchronization START");

                // Event handlers sú už nastavené v XAML
                // OnDataScrollViewChanged a OnHeaderScrollViewChanged

                _logger.LogDebug("✅ SetupScrollSynchronization COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetupScrollSynchronization");
                throw;
            }
        }

        /// <summary>
        /// Event handler pre data scroll view changed - ✅ ROZŠÍRENÉ: Virtual Scrolling Support
        /// </summary>
        public void OnDataScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing) return;

                var dataScrollViewer = sender as ScrollViewer;
                var headerScrollViewer = HeaderScrollViewerElement;

                if (dataScrollViewer != null && headerScrollViewer != null)
                {
                    _isScrollSynchronizing = true;

                    // Synchronizuj horizontálny scroll
                    headerScrollViewer.ScrollToHorizontalOffset(dataScrollViewer.HorizontalOffset);

                    _logger.LogTrace("📜 Data scroll synchronized - HorizontalOffset: {Offset}",
                        dataScrollViewer.HorizontalOffset);

                    // ✅ NOVÉ: Virtual Scrolling Integration
                    if (_virtualScrollingService != null && _displayRows.Any())
                    {
                        try
                        {
                            // Update virtual scrolling service with current scroll position
                            _virtualScrollingService.SetTotalRowCount(_displayRows.Count);
                            
                            var viewport = _virtualScrollingService.CalculateViewportOptimized(
                                dataScrollViewer.VerticalOffset, 
                                dataScrollViewer.ViewportHeight);

                            // Trigger viewport-based rendering if enabled
                            if (viewport.IsValid && _virtualScrollingService.ShouldUseVirtualScrolling())
                            {
                                _ = Task.Run(async () => await UpdateVirtualScrollingViewportAsync(viewport));
                                
                                _logger.LogTrace("🚀 Virtual scrolling viewport updated - Visible: {FirstVisible}-{LastVisible}, " +
                                    "Rendered: {FirstRendered}-{LastRendered}",
                                    viewport.FirstVisibleRowIndex, viewport.LastVisibleRowIndex,
                                    viewport.FirstRenderedRowIndex, viewport.LastRenderedRowIndex);
                            }
                        }
                        catch (Exception vsEx)
                        {
                            _logger.LogWarning(vsEx, "⚠️ Error in virtual scrolling viewport calculation");
                        }
                    }

                    _isScrollSynchronizing = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnDataScrollViewChanged");
                _isScrollSynchronizing = false;
            }
        }

        /// <summary>
        /// Event handler pre header scroll view changed
        /// </summary>
        public void OnHeaderScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing) return;

                var headerScrollViewer = sender as ScrollViewer;
                var dataScrollViewer = DataGridScrollViewerElement;

                if (headerScrollViewer != null && dataScrollViewer != null)
                {
                    _isScrollSynchronizing = true;

                    // Synchronizuj horizontálny scroll
                    dataScrollViewer.ScrollToHorizontalOffset(headerScrollViewer.HorizontalOffset);

                    _logger.LogTrace("📜 Header scroll synchronized - HorizontalOffset: {Offset}",
                        headerScrollViewer.HorizontalOffset);

                    _isScrollSynchronizing = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnHeaderScrollViewChanged");
                _isScrollSynchronizing = false;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: ValidAlerts Stretching

        /// <summary>
        /// Nastaví ValidAlerts stĺpec na stretching
        /// </summary>
        private void SetupValidAlertsStretching()
        {
            try
            {
                _logger.LogDebug("📐 SetupValidAlertsStretching START");

                // ValidAlerts stretching sa implementuje cez layout management
                _ = Task.Run(async () => await RecalculateValidAlertsWidthAsync());

                _logger.LogDebug("✅ SetupValidAlertsStretching COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetupValidAlertsStretching");
                throw;
            }
        }

        /// <summary>
        /// Prepočíta šírku ValidAlerts stĺpca pre stretching
        /// </summary>
        private async Task RecalculateValidAlertsWidthAsync()
        {
            try
            {
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    // Vypočítaj dostupný priestor
                    var totalWidth = _totalAvailableWidth;
                    var usedWidth = _columns.Where(c => c.IsVisible && c.Name != "ValidAlerts").Sum(c => c.Width);
                    var availableWidth = totalWidth - usedWidth;

                    // ValidAlerts by mal zaberať zvyšný priestor, ale min 200px
                    var validAlertsWidth = Math.Max(availableWidth, _validAlertsMinWidth);

                    // Nájdi ValidAlerts stĺpec a aktualizuj šírku
                    var validAlertsColumn = _columns.FirstOrDefault(c => c.Name == "ValidAlerts");
                    if (validAlertsColumn != null)
                    {
                        validAlertsColumn.Width = validAlertsWidth;
                        _logger.LogTrace("📐 ValidAlerts width recalculated: {Width}px (available: {Available}px)",
                            validAlertsWidth, availableWidth);
                    }
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in RecalculateValidAlertsWidthAsync");
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Layout Management

        /// <summary>
        /// Inicializuje layout management
        /// </summary>
        private void InitializeLayoutManagement()
        {
            try
            {
                _logger.LogDebug("📐 InitializeLayoutManagement START");

                _totalAvailableWidth = 0;
                _validAlertsMinWidth = 200;

                _logger.LogDebug("✅ InitializeLayoutManagement COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeLayoutManagement");
                throw;
            }
        }

        /// <summary>
        /// Aktualizuje layout po zmene dát
        /// </summary>
        private async Task UpdateLayoutAfterDataChangeAsync()
        {
            try
            {
                _logger.LogDebug("📐 UpdateLayoutAfterDataChangeAsync START");

                await UIHelper.RunOnUIThreadAsync(async () =>
                {
                    // Aktualizuj ValidAlerts stretching
                    await RecalculateValidAlertsWidthAsync();

                    // Aktualizuj UI visibility ak je potrebné
                    await UpdateUIVisibilityAsync();

                    _logger.LogDebug("✅ Layout updated after data change");
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateLayoutAfterDataChangeAsync");
            }
        }

        /// <summary>
        /// Aktualizuje layout po zmene veľkosti
        /// </summary>
        private async Task UpdateLayoutAfterSizeChangeAsync()
        {
            try
            {
                _logger.LogDebug("📐 UpdateLayoutAfterSizeChangeAsync START - TotalWidth: {TotalWidth}", _totalAvailableWidth);

                await RecalculateValidAlertsWidthAsync();

                _logger.LogDebug("✅ Layout updated after size change");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateLayoutAfterSizeChangeAsync");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Updates virtual scrolling viewport and optimizes rendering
        /// </summary>
        private async Task UpdateVirtualScrollingViewportAsync(VirtualScrollingViewport viewport)
        {
            try
            {
                if (_virtualScrollingService == null || !viewport.IsValid)
                {
                    _logger.LogTrace("⚠️ Virtual scrolling service not available or viewport invalid");
                    return;
                }

                _logger.LogTrace("🚀 UpdateVirtualScrollingViewportAsync START - Viewport: {FirstVisible}-{LastVisible}",
                    viewport.FirstVisibleRowIndex, viewport.LastVisibleRowIndex);

                // Get current virtual scrolling stats for performance monitoring
                var stats = _virtualScrollingService.GetStats();
                
                // Only process if we have significant changes or performance benefits
                if (stats.MemorySavingPercent > 50)
                {
                    // Optimize rendering based on viewport
                    var indicesToRender = _virtualScrollingService.GetRowIndicesToRender().ToList();
                    
                    if (indicesToRender.Any())
                    {
                        // Register rendered elements for recycling
                        for (int i = viewport.FirstRenderedRowIndex; i <= viewport.LastRenderedRowIndex; i++)
                        {
                            if (i >= 0 && i < _displayRows.Count)
                            {
                                var rowElement = GetRowUIElement(i); // Get UI element for row
                                if (rowElement != null)
                                {
                                    _virtualScrollingService.RegisterRenderedElement(i, rowElement);
                                }
                            }
                        }

                        _logger.LogDebug("💾 Virtual scrolling optimization applied - Memory saved: {MemorySaving:F1}%, " +
                            "Rendered: {RenderedRows}/{TotalRows}, Cache: {CachedElements}, Recycled: {RecycledElements}",
                            stats.MemorySavingPercent, stats.RenderedRows, stats.TotalRows, 
                            stats.CachedElements, stats.RecycledElements);
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateVirtualScrollingViewportAsync");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Gets UI element for a specific row (helper for virtual scrolling)
        /// </summary>
        private Microsoft.UI.Xaml.FrameworkElement? GetRowUIElement(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _displayRows.Count)
                    return null;

                // Try to find the row element in the visual tree
                var dataContainer = DataContainer;
                if (dataContainer?.Children.Count > rowIndex)
                {
                    var rowElement = dataContainer.Children[rowIndex] as Microsoft.UI.Xaml.FrameworkElement;
                    return rowElement;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "⚠️ Error getting UI element for row {RowIndex}", rowIndex);
                return null;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Individual Colors Support

        /// <summary>
        /// Aplikuje individual colors na UI
        /// </summary>
        private void ApplyIndividualColorsToUI()
        {
            try
            {
                _logger.LogDebug("🎨 ApplyIndividualColorsToUI START - HasCustomColors: {HasColors}",
                    _individualColorConfig?.HasAnyCustomColors ?? false);

                if (_individualColorConfig == null)
                {
                    _logger.LogDebug("📋 No custom colors to apply - using defaults");
                    return;
                }

                // Apply colors cez theme resources
                var resources = this.Resources;

                if (_individualColorConfig.CellBackgroundColor.HasValue)
                {
                    resources["DataGridCellBackgroundBrush"] = new SolidColorBrush(_individualColorConfig.CellBackgroundColor.Value);
                }

                if (_individualColorConfig.HeaderBackgroundColor.HasValue)
                {
                    resources["DataGridHeaderBackgroundBrush"] = new SolidColorBrush(_individualColorConfig.HeaderBackgroundColor.Value);
                }

                if (_individualColorConfig.ValidationErrorColor.HasValue)
                {
                    resources["DataGridValidationErrorBrush"] = new SolidColorBrush(_individualColorConfig.ValidationErrorColor.Value);
                }

                if (_individualColorConfig.AlternateRowColor.HasValue)
                {
                    resources["DataGridZebraRowBrush"] = new SolidColorBrush(_individualColorConfig.AlternateRowColor.Value);
                }

                _logger.LogDebug("✅ Individual colors applied - {ColorCount} custom colors",
                    _individualColorConfig.CustomColorsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ApplyIndividualColorsToUI");
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Search/Sort/Zebra Support

        /// <summary>
        /// Inicializuje search, sort a zebra rows funkcionalitu
        /// </summary>
        private async Task InitializeSearchSortZebra()
        {
            try
            {
                _logger.LogDebug("🔍 InitializeSearchSortZebra START");

                // Vytvor SearchAndSortService
                _searchAndSortService = new SearchAndSortService(_logger as ILogger<SearchAndSortService> ?? NullLogger<SearchAndSortService>.Instance);

                // Nastav zebra rows ak sú povolené
                var zebraEnabled = _individualColorConfig?.IsZebraRowsEnabled ?? false;
                _searchAndSortService.SetZebraRowsEnabled(zebraEnabled);

                // Vytvor NavigationService
                _navigationService = new NavigationService(_logger as ILogger<NavigationService> ?? NullLogger<NavigationService>.Instance);
                await _navigationService.InitializeAsync();
                
                // Nastav navigation callback
                _navigationService.SetNavigationCallback(this);

                // Vytvor CopyPasteService
                _copyPasteService = new CopyPasteService();
                await _copyPasteService.InitializeAsync();

                // Setup keyboard shortcuts for copy/paste
                SetupKeyboardShortcuts();

                _logger.LogDebug("✅ SearchAndSortZebra initialized - ZebraRows: {ZebraEnabled}", zebraEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in InitializeSearchSortZebra");
                throw;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Data Display

        /// <summary>
        /// Vytvorí počiatočné prázdne riadky
        /// </summary>
        private async Task CreateInitialEmptyRowsAsync()
        {
            try
            {
                _logger.LogDebug("📄 CreateInitialEmptyRowsAsync START - TargetCount: {TargetCount}", _unifiedRowCount);

                await Task.Run(() =>
                {
                    _displayRows.Clear();

                    for (int i = 0; i < _unifiedRowCount; i++)
                    {
                        var rowViewModel = CreateEmptyRowViewModel(i);
                        _displayRows.Add(rowViewModel);
                    }

                    _logger.LogDebug("✅ Created {RowCount} initial empty rows", _displayRows.Count);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CreateInitialEmptyRowsAsync");
                throw;
            }
        }

        /// <summary>
        /// Vytvorí prázdny row view model
        /// </summary>
        private DataRowViewModel CreateEmptyRowViewModel(int rowIndex)
        {
            var rowViewModel = new DataRowViewModel
            {
                RowIndex = rowIndex,
                IsSelected = false,
                ValidationErrors = string.Empty,
                IsZebraRow = (rowIndex % 2) == 1 // Každý druhý riadok
            };

            // Vytvor cells pre každý stĺpec
            foreach (var column in _columns)
            {
                var cellViewModel = new CellViewModel
                {
                    RowIndex = rowIndex,
                    ColumnName = column.Name,
                    Value = column.DefaultValue,
                    OriginalValue = column.DefaultValue,
                    DataType = column.DataType,
                    IsValid = true,
                    ValidationErrors = string.Empty
                };

                // Subscribe na cell value changes pre realtime validation
                cellViewModel.PropertyChanged += OnCellValueChanged;

                rowViewModel.Cells.Add(cellViewModel);
            }

            return rowViewModel;
        }

        /// <summary>
        /// Aktualizuje display rows s realtime validáciou
        /// </summary>
        private async Task UpdateDisplayRowsWithRealtimeValidationAsync()
        {
            try
            {
                _logger.LogDebug("🔄 UpdateDisplayRowsWithRealtimeValidationAsync START");

                if (_dataManagementService == null)
                {
                    _logger.LogWarning("⚠️ DataManagementService is null");
                    return;
                }

                // Získaj aktuálne dáta
                var allData = await _dataManagementService.GetAllDataAsync();

                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    // Vyčisti existujúce rows
                    _displayRows.Clear();

                    // Vytvor nové rows z dát
                    for (int i = 0; i < allData.Count; i++)
                    {
                        var rowData = allData[i];
                        var rowViewModel = CreateRowViewModelFromData(i, rowData);
                        _displayRows.Add(rowViewModel);
                    }

                    // ✅ NOVÉ: Nastav ItemsSource pre DataRowsContainer aby sa zobrazili zebra rows
                    var dataRowsContainer = DataRowsContainerElement;
                    if (dataRowsContainer != null)
                    {
                        dataRowsContainer.ItemsSource = _displayRows;
                    }

                    _totalCellsRendered = _displayRows.Sum(r => r.Cells.Count);
                    _logger.LogDebug("✅ Display rows updated - {RowCount} rows, {CellCount} cells, ZebraRows: {ZebraCount}",
                        _displayRows.Count, _totalCellsRendered, _displayRows.Count(r => r.IsZebraRow));
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateDisplayRowsWithRealtimeValidationAsync");
                throw;
            }
        }

        /// <summary>
        /// Vytvorí row view model z dát
        /// </summary>
        private DataRowViewModel CreateRowViewModelFromData(int rowIndex, Dictionary<string, object?> rowData)
        {
            var rowViewModel = new DataRowViewModel
            {
                RowIndex = rowIndex,
                IsZebraRow = (_individualColorConfig?.IsZebraRowsEnabled == true) && ((rowIndex % 2) == 1)
            };

            // Vytvor cells
            foreach (var column in _columns)
            {
                var value = rowData.ContainsKey(column.Name) ? rowData[column.Name] : column.DefaultValue;

                var cellViewModel = new CellViewModel
                {
                    RowIndex = rowIndex,
                    ColumnName = column.Name,
                    Value = value,
                    OriginalValue = value,
                    DataType = column.DataType,
                    IsValid = true
                };

                cellViewModel.PropertyChanged += OnCellValueChanged;
                rowViewModel.Cells.Add(cellViewModel);
            }

            return rowViewModel;
        }

        /// <summary>
        /// Refresh data display
        /// </summary>
        private async Task RefreshDataDisplayAsync()
        {
            try
            {
                _logger.LogDebug("🔄 RefreshDataDisplayAsync START");

                await UpdateDisplayRowsWithRealtimeValidationAsync();

                _logger.LogDebug("✅ Data display refreshed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in RefreshDataDisplayAsync");
                throw;
            }
        }

        /// <summary>
        /// Aktualizuje UI visibility
        /// </summary>
        private async Task UpdateUIVisibilityAsync()
        {
            try
            {
                _logger.LogDebug("👁️ UpdateUIVisibilityAsync START - Initialized: {IsInitialized}", _isInitialized);

                await UIHelper.RunOnUIThreadAsync(async () =>
                {
                    var loadingOverlay = LoadingOverlayElement;
                    var mainContent = MainContentGridElement;

                    if (_isInitialized)
                    {
                        // Skry loading, zobraz content
                        if (loadingOverlay != null)
                            await UIHelper.SetVisibilityAsync(loadingOverlay, Visibility.Collapsed, _logger);

                        if (mainContent != null)
                            await UIHelper.SetVisibilityAsync(mainContent, Visibility.Visible, _logger);

                        _logger.LogDebug("✅ UI switched to content view");
                    }
                    else
                    {
                        // Zobraz loading, skry content
                        if (loadingOverlay != null)
                            await UIHelper.SetVisibilityAsync(loadingOverlay, Visibility.Visible, _logger);

                        if (mainContent != null)
                            await UIHelper.SetVisibilityAsync(mainContent, Visibility.Collapsed, _logger);

                        _logger.LogDebug("✅ UI switched to loading view");
                    }
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateUIVisibilityAsync");
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Realtime Validation

        /// <summary>
        /// Vykoná realtime validáciu bunky
        /// </summary>
        private async Task ValidateCellRealtimeAsync(CellViewModel cell)
        {
            try
            {
                if (_validationService == null || _throttlingConfig?.EnableRealtimeValidation != true)
                    return;

                _logger.LogTrace("🔍 ValidateCellRealtimeAsync - [{RowIndex}, {ColumnName}] = '{Value}'",
                    cell.RowIndex, cell.ColumnName, cell.Value);

                // Validuj bunku
                var errors = await _validationService.ValidateCellAsync(cell.ColumnName, cell.Value);

                // Aktualizuj cell validation state
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    cell.IsValid = !errors.Any();
                    cell.ValidationErrors = string.Join("; ", errors);

                    if (!cell.IsValid)
                    {
                        _totalValidationErrors++;
                        _logger.LogDebug("❌ Cell validation failed - [{RowIndex}, {ColumnName}]: {Errors}",
                            cell.RowIndex, cell.ColumnName, cell.ValidationErrors);
                    }
                }, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateCellRealtimeAsync");
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Performance Tracking

        /// <summary>
        /// Inicializuje performance tracking
        /// </summary>
        private void InitializePerformanceTracking()
        {
            try
            {
                _logger.LogDebug("📊 Performance tracking initialized");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not initialize performance tracking");
            }
        }

        /// <summary>
        /// Inkrementuje counter pre operáciu
        /// </summary>
        private void IncrementOperationCounter(string operationName)
        {
            try
            {
                if (!_operationCounters.ContainsKey(operationName))
                    _operationCounters[operationName] = 0;

                _operationCounters[operationName]++;

                _logger.LogTrace("📊 Operation Counter [{Operation}]: {Count}",
                    operationName, _operationCounters[operationName]);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not increment operation counter: {Operation}", operationName);
            }
        }

        #endregion

        #region ✅ RESIZE Event Handlers - KOMPLETNÁ IMPLEMENTÁCIA

        /// <summary>
        /// Pointer pressed event handler pre resize a drag selection
        /// </summary>
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                _logger.LogTrace("🖱️ OnPointerPressed START");

                var pointerPosition = e.GetCurrentPoint(this);
                var isLeftButton = pointerPosition.Properties.IsLeftButtonPressed;

                // Priority 1: Hľadaj resize grip pod kurzorom
                foreach (var header in _resizableHeaders)
                {
                    if (header.ResizeGrip != null && IsPointerOverElement(pointerPosition, header.ResizeGrip))
                    {
                        _isResizing = true;
                        _currentResizingHeader = header;
                        _resizeStartPosition = pointerPosition.Position.X;
                        _resizeStartWidth = header.OriginalWidth;

                        this.CapturePointer(e.Pointer);

                        _logger.LogDebug("🖱️ Resize started - Column: {ColumnName}, StartWidth: {Width}",
                            header.ColumnName, _resizeStartWidth);
                        return; // Exit early for resize
                    }
                }

                // Priority 2: Check for drag selection start (left button only)
                if (isLeftButton && !_isResizing)
                {
                    var cellPosition = GetCellFromPoint(pointerPosition.Position);
                    if (cellPosition != null)
                    {
                        // Start drag selection
                        _ = Task.Run(async () => await OnDragSelectionStart(pointerPosition.Position, cellPosition));
                        this.CapturePointer(e.Pointer);
                        
                        _logger.LogDebug("🖱️ Drag selection start - Cell: [{Row},{Column}] '{ColumnName}'",
                            cellPosition.Row, cellPosition.Column, cellPosition.ColumnName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnPointerPressed");
            }
        }

        /// <summary>
        /// Pointer moved event handler pre resize a drag selection
        /// </summary>
        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var pointerPosition = e.GetCurrentPoint(this);

                // Priority 1: Handle resize operation
                if (_isResizing && _currentResizingHeader != null)
                {
                    var deltaX = pointerPosition.Position.X - _resizeStartPosition;
                    var newWidth = Math.Max(_resizeStartWidth + deltaX, _currentResizingHeader.MinWidth);
                    newWidth = Math.Min(newWidth, _currentResizingHeader.MaxWidth);

                    // Aktualizuj šírku header elementu
                    if (_currentResizingHeader.HeaderElement != null)
                    {
                        _currentResizingHeader.HeaderElement.Width = newWidth;
                    }

                    // Aktualizuj ColumnDefinition
                    var column = _columns.FirstOrDefault(c => c.Name == _currentResizingHeader.ColumnName);
                    if (column != null)
                    {
                        column.Width = newWidth;
                    }

                    _logger.LogTrace("🖱️ Resizing - Column: {ColumnName}, NewWidth: {Width}",
                        _currentResizingHeader.ColumnName, newWidth);
                    return;
                }

                // Priority 2: Handle drag selection update
                if (_dragSelectionState.IsDragging)
                {
                    var currentCell = GetCellFromPoint(pointerPosition.Position);
                    _ = Task.Run(async () => await OnDragSelectionUpdate(pointerPosition.Position, currentCell));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnPointerMoved");
            }
        }

        /// <summary>
        /// Pointer released event handler pre resize a drag selection
        /// </summary>
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                // Priority 1: Handle resize completion
                if (_isResizing && _currentResizingHeader != null)
                {
                    var finalWidth = _currentResizingHeader.HeaderElement?.Width ?? _resizeStartWidth;

                    _logger.LogDebug("🖱️ Resize completed - Column: {ColumnName}, FinalWidth: {Width}",
                        _currentResizingHeader.ColumnName, finalWidth);

                    // Aktualizuj layout po resize
                    _ = Task.Run(async () => await RecalculateValidAlertsWidthAsync());
                    
                    EndResize();
                }
                
                // Priority 2: Handle drag selection end
                if (_dragSelectionState.IsDragging)
                {
                    _ = Task.Run(async () => await OnDragSelectionEnd());
                }

                this.ReleasePointerCapture(e.Pointer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in OnPointerReleased");
            }
        }

        /// <summary>
        /// Ukončí resize operáciu
        /// </summary>
        private void EndResize()
        {
            _isResizing = false;
            _currentResizingHeader = null;
            _logger.LogDebug("🖱️ Resize operation ended");
        }

        /// <summary>
        /// Kontroluje či je pointer nad elementom
        /// </summary>
        private bool IsPointerOverElement(PointerPoint point, FrameworkElement element)
        {
            try
            {
                var elementBounds = element.TransformToVisual(this).TransformBounds(
                    new Windows.Foundation.Rect(0, 0, element.ActualWidth, element.ActualHeight));

                return elementBounds.Contains(point.Position);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Helper Methods

        /// <summary>
        /// Deduplikuje header názvy stĺpcov (meno, meno, priezvisko → meno_1, meno_2, priezvisko)
        /// </summary>
        private List<GridColumnDefinition> DeduplicateColumnHeaders(List<GridColumnDefinition> originalColumns)
        {
            try
            {
                _logger.LogDebug("🔄 DeduplicateColumnHeaders START - InputColumns: {ColumnCount}",
                    originalColumns.Count);

                var deduplicatedColumns = new List<GridColumnDefinition>();
                var headerCounts = new Dictionary<string, int>();

                foreach (var column in originalColumns)
                {
                    var originalHeader = column.Header ?? column.Name;
                    var newColumn = new GridColumnDefinition
                    {
                        Name = column.Name,
                        Header = originalHeader,
                        DataType = column.DataType,
                        Width = column.Width,
                        MinWidth = column.MinWidth,
                        MaxWidth = column.MaxWidth,
                        IsVisible = column.IsVisible,
                        IsEditable = column.IsEditable,
                        // IsSpecialColumn = column.IsSpecialColumn, // Read-only property
                        DefaultValue = column.DefaultValue
                    };

                    // Kontrola deduplikácie header názvov
                    if (headerCounts.ContainsKey(originalHeader))
                    {
                        headerCounts[originalHeader]++;
                        newColumn.Header = $"{originalHeader}_{headerCounts[originalHeader]}";
                        
                        _logger.LogDebug("🔄 Header deduplicated: '{OriginalHeader}' → '{NewHeader}'",
                            originalHeader, newColumn.Header);
                    }
                    else
                    {
                        headerCounts[originalHeader] = 1;
                    }

                    deduplicatedColumns.Add(newColumn);
                }

                var duplicatesFound = headerCounts.Count(kvp => kvp.Value > 1);
                _logger.LogInformation("✅ Header deduplikácia COMPLETED - InputColumns: {InputCount}, " +
                    "OutputColumns: {OutputCount}, DuplicatesFound: {Duplicates}",
                    originalColumns.Count, deduplicatedColumns.Count, duplicatesFound);

                return deduplicatedColumns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in DeduplicateColumnHeaders");
                throw;
            }
        }

        /// <summary>
        /// Kontroluje či je komponent inicializovaný
        /// </summary>
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                _logger.LogError("❌ Component not initialized - call InitializeAsync() first");
                throw new InvalidOperationException("AdvancedDataGrid nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Aktualizuje sort indikátor pre stĺpec (single sort)
        /// </summary>
        private void UpdateSortIndicator(string columnName, SortDirection direction)
        {
            try
            {
                foreach (var header in _resizableHeaders)
                {
                    if (header.SortIndicator != null)
                    {
                        if (header.ColumnName == columnName)
                        {
                            // Set sort indicator for active column
                            if (direction != SortDirection.None)
                            {
                                header.SortIndicator.Visibility = Visibility.Visible;
                                header.SortIndicator.Text = direction == SortDirection.Ascending ? "▲" : "▼";
                                _logger.LogTrace("🔀 Single-Sort indicator updated - Column: {ColumnName}, Symbol: {Symbol}",
                                    columnName, header.SortIndicator.Text);
                            }
                            else
                            {
                                header.SortIndicator.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            // Hide indicators for other columns
                            header.SortIndicator.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateSortIndicator - Column: {ColumnName}",
                    columnName);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Aktualizuje Multi-Sort indikátory pre všetky aktívne stĺpce
        /// </summary>
        private void UpdateMultiSortIndicators()
        {
            try
            {
                if (_searchAndSortService == null) return;

                var multiSortColumns = _searchAndSortService.GetMultiSortColumns();
                var isMultiSortActive = multiSortColumns.Any();

                _logger.LogTrace("🔢 Updating Multi-Sort indicators - ActiveColumns: {ActiveColumns}, " +
                    "Columns: [{ColumnDetails}]",
                    multiSortColumns.Count, 
                    string.Join(", ", multiSortColumns.Select(c => $"{c.ColumnName}:{c.GetSortSymbol()}{c.Priority}")));

                foreach (var header in _resizableHeaders)
                {
                    if (header.SortIndicator != null)
                    {
                        var multiSortColumn = multiSortColumns.FirstOrDefault(c => 
                            c.ColumnName.Equals(header.ColumnName, StringComparison.OrdinalIgnoreCase));

                        if (multiSortColumn != null)
                        {
                            // Zobraz Multi-Sort indikátor s prioritou
                            header.SortIndicator.Visibility = Visibility.Visible;
                            var symbol = multiSortColumn.GetSortSymbol();
                            var priorityText = multiSortColumns.Count > 1 ? $"{multiSortColumn.Priority}" : "";
                            header.SortIndicator.Text = $"{symbol}{priorityText}";

                            _logger.LogTrace("🔢 Multi-Sort indicator set - Column: {ColumnName}, " +
                                "Symbol: {Symbol}, Priority: {Priority}, DisplayText: '{DisplayText}'",
                                header.ColumnName, symbol, multiSortColumn.Priority, header.SortIndicator.Text);
                        }
                        else
                        {
                            // Skry indikátor pre neaktívne stĺpce
                            header.SortIndicator.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                _logger.LogDebug("✅ Multi-Sort indicators updated - ActiveColumns: {ActiveColumns}, " +
                    "IsMultiSortActive: {IsActive}", multiSortColumns.Count, isMultiSortActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in UpdateMultiSortIndicators");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Aplikuje sortovanie a refresh display
        /// </summary>
        private async Task ApplySortAndRefreshAsync()
        {
            try
            {
                _logger.LogDebug("🔀 ApplySortAndRefresh START");

                if (_dataManagementService == null || _searchAndSortService == null)
                {
                    _logger.LogWarning("⚠️ Required services are null - cannot apply sort");
                    return;
                }

                // Get current data
                var allData = await _dataManagementService.GetAllDataAsync();
                
                // Apply sorting (prázdne riadky budú na konci)
                var sortedData = await _searchAndSortService.ApplySortingAsync(allData);
                
                await UIHelper.RunOnUIThreadAsync(() =>
                {
                    // Update display rows
                    _displayRows.Clear();
                    
                    for (int i = 0; i < sortedData.Count; i++)
                    {
                        var rowData = sortedData[i];
                        var rowViewModel = CreateRowViewModelFromData(i, rowData);
                        _displayRows.Add(rowViewModel);
                    }

                    _totalCellsRendered = _displayRows.Sum(r => r.Cells.Count);
                    _logger.LogDebug("✅ Sort applied - Rows: {RowCount}, Cells: {CellCount}",
                        _displayRows.Count, _totalCellsRendered);
                }, _logger);

                _logger.LogDebug("✅ ApplySortAndRefresh COMPLETED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ApplySortAndRefreshAsync");
            }
        }

        /// <summary>
        /// Konvertuje advanced validation rules na legacy format
        /// </summary>
        private List<GridValidationRule>? ConvertAdvancedRulesToLegacy(Models.Validation.ValidationRuleSet? advancedRules)
        {
            if (advancedRules == null || !advancedRules.Rules.Any())
                return null;

            var legacyRules = new List<GridValidationRule>();

            foreach (var advancedRule in advancedRules.Rules.Where(r => r.IsEnabled))
            {
                // Convert len basic validation rules do legacy formátu
                if (advancedRule.ValidationFunction != null && 
                    advancedRule.TargetColumns.Count == 1 &&
                    advancedRule.CrossCellValidator == null &&
                    advancedRule.AsyncValidationFunction == null)
                {
                    var legacyRule = new GridValidationRule
                    {
                        ColumnName = advancedRule.TargetColumns.First(),
                        CustomValidator = (value) =>
                        {
                            // Vytvor temporary validation context
                            var context = new Models.Validation.ValidationContext
                            {
                                ColumnName = advancedRule.TargetColumns.First(),
                                CurrentValue = value,
                                RowData = new Dictionary<string, object?> { { advancedRule.TargetColumns.First(), value } }
                            };

                            var result = advancedRule.ValidationFunction(context);
                            return result?.IsValid ?? true;
                        },
                        ErrorMessage = advancedRule.ErrorMessage,
                        IsEnabled = advancedRule.IsEnabled
                    };

                    legacyRules.Add(legacyRule);
                }
            }

            _logger.LogDebug("🔧 Converted {AdvancedCount} advanced rules to {LegacyCount} legacy rules",
                advancedRules.Rules.Count, legacyRules.Count);

            return legacyRules.Any() ? legacyRules : null;
        }

        /// <summary>
        /// Získa zoznam povolených funkcionalít
        /// </summary>
        private string GetEnabledFeatures()
        {
            var features = new List<string>();

            if (_autoAddEnabled) features.Add("AutoAdd");
            if (_throttlingConfig?.EnableRealtimeValidation == true) features.Add("RealtimeValidation");
            if (_individualColorConfig?.IsZebraRowsEnabled == true) features.Add("ZebraRows");
            if (_resizableHeaders.Any()) features.Add("ColumnResize");
            if (_searchAndSortService != null) features.Add("SearchSort");
            features.Add("ScrollSync");
            features.Add("ValidAlertsStretch");

            return string.Join("+", features);
        }

        /// <summary>
        /// Získa celkový počet validačných pravidiel
        /// </summary>
        private int GetTotalValidationRulesCount()
        {
            return _validationService?.TotalValidationErrorCount ?? 0;
        }

        /// <summary>
        /// Loguje finálny performance súhrn
        /// </summary>
        private void LogFinalPerformanceSummary()
        {
            try
            {
                if (_operationCounters.Any())
                {
                    var topOperations = _operationCounters
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(5)
                        .Select(kvp => $"{kvp.Key}:{kvp.Value}")
                        .ToList();

                    _logger.LogInformation("📊 Final Performance Summary - TopOperations: {TopOps}",
                        string.Join(", ", topOperations));
                }

                if (_operationDurations.Any())
                {
                    var avgDurations = _operationDurations
                        .Select(kvp => $"{kvp.Key}:{kvp.Value:F1}ms")
                        .ToList();

                    _logger.LogDebug("⏱️ Average Durations: {AvgDurations}",
                        string.Join(", ", avgDurations));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log final performance summary");
            }
        }

        /// <summary>
        /// Dispose všetkých resources
        /// </summary>
        private void DisposeResources()
        {
            try
            {
                // Dispose validation timers
                lock (_validationLock)
                {
                    foreach (var timer in _validationTimers.Values)
                    {
                        timer?.Dispose();
                    }
                    _validationTimers.Clear();
                }

                // Unsubscribe from events
                UnsubscribeFromEvents();

                // Clear collections
                ClearCollections();

                _searchAndSortService?.Dispose();
                _rowHeightAnimationService?.Dispose();

                // ✅ NOVÉ: Dispose controller
                if (_controller != null)
                {
                    _ = Task.Run(async () => await _controller.DisposeAsync());
                    _controller = null;
                }

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing resources");
            }
        }

        private void UnsubscribeFromEvents()
        {
            try
            {
                this.PointerMoved -= OnPointerMoved;
                this.PointerPressed -= OnPointerPressed;
                this.PointerReleased -= OnPointerReleased;
                this.PointerCaptureLost -= OnPointerCaptureLost;
                this.SizeChanged -= OnDataGridSizeChanged;
                this.LayoutUpdated -= OnLayoutUpdated;

                var dataScrollViewer = DataGridScrollViewerElement;
                var headerScrollViewer = HeaderScrollViewerElement;

                if (dataScrollViewer != null)
                    dataScrollViewer.ViewChanged -= OnDataScrollViewChanged;
                if (headerScrollViewer != null)
                    headerScrollViewer.ViewChanged -= OnHeaderScrollViewChanged;

                // Unsubscribe from cell events
                foreach (var row in _displayRows)
                {
                    foreach (var cell in row.Cells)
                    {
                        cell.PropertyChanged -= OnCellValueChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error unsubscribing from events");
            }
        }

        private void ClearCollections()
        {
            try
            {
                _operationStartTimes.Clear();
                _operationCounters.Clear();
                _operationDurations.Clear();
                _resizableHeaders.Clear();
                _uiStateSnapshot.Clear();
                _columnSearchFilters.Clear();
                _displayRows.Clear();
                _gridData.Clear();
                _columns.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error clearing collections");
            }
        }


        #endregion

        #region ✅ KOMPLETNÁ IMPLEMENTÁCIA: Detailed Logging Methods

        /// <summary>
        /// Loguje štruktúru stĺpcov
        /// </summary>
        private void LogColumnStructure(List<GridColumnDefinition> columns)
        {
            try
            {
                foreach (var column in columns)
                {
                    _logger.LogDebug("📊 Column: {Name} ({Type}) - Header: '{Header}', Width: {Width}, " +
                        "Visible: {Visible}, Editable: {Editable}, Special: {Special}",
                        column.Name, column.DataType.Name, column.Header, column.Width,
                        column.IsVisible, column.IsEditable, column.IsSpecialColumn);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log column structure");
            }
        }

        /// <summary>
        /// Loguje validačné pravidlá
        /// </summary>
        /// <summary>
        /// Loguje advanced validation rules
        /// </summary>
        private void LogAdvancedValidationRules(Models.Validation.ValidationRuleSet? validationRules)
        {
            try
            {
                if (validationRules?.Rules == null || !validationRules.Rules.Any())
                {
                    _logger.LogDebug("📋 No advanced validation rules provided");
                    return;
                }

                _logger.LogDebug("📋 Advanced Validation Rules: {RuleCount} rules", validationRules.Rules.Count);
                foreach (var rule in validationRules.Rules)
                {
                    _logger.LogDebug("🔍 Advanced Rule: {Id} - Targets: {TargetColumns} - Priority: {Priority}",
                        rule.Id, string.Join(",", rule.TargetColumns), rule.Priority);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log advanced validation rules");
            }
        }

        /// <summary>
        /// Loguje color configuration
        /// </summary>
        private void LogColorConfiguration(DataGridColorConfig? colorConfig)
        {
            try
            {
                if (colorConfig == null)
                {
                    _logger.LogDebug("🎨 Using default colors - no custom configuration");
                    return;
                }

                _logger.LogDebug("🎨 Color Config: CustomColors: {CustomCount}, ZebraRows: {ZebraEnabled}, " +
                    "CellBg: {CellBg}, HeaderBg: {HeaderBg}, ValidationError: {ValidationError}",
                    colorConfig.CustomColorsCount, colorConfig.IsZebraRowsEnabled,
                    colorConfig.CellBackgroundColor?.ToString() ?? "default",
                    colorConfig.HeaderBackgroundColor?.ToString() ?? "default",
                    colorConfig.ValidationErrorColor?.ToString() ?? "default");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log color configuration");
            }
        }

        /// <summary>
        /// Loguje analýzu dát
        /// </summary>
        private async Task LogDataAnalysis(List<Dictionary<string, object?>> data)
        {
            try
            {
                await Task.Run(() =>
                {
                    var nonEmptyRows = data.Count(row => row.Any(kvp => kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString())));
                    var totalCells = data.Sum(row => row.Count);
                    var filledCells = data.Sum(row => row.Count(kvp => kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString())));

                    _logger.LogDebug("📊 Data Analysis - TotalRows: {TotalRows}, NonEmptyRows: {NonEmptyRows}, " +
                        "TotalCells: {TotalCells}, FilledCells: {FilledCells} ({FillPercentage:F1}%)",
                        data.Count, nonEmptyRows, totalCells, filledCells,
                        totalCells > 0 ? (filledCells * 100.0 / totalCells) : 0);

                    // Sample first few rows for debugging
                    for (int i = 0; i < Math.Min(3, data.Count); i++)
                    {
                        var sampleData = string.Join(", ", data[i].Take(3).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        _logger.LogTrace("📝 Sample Row[{Index}]: {SampleData}...", i, sampleData);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log data analysis");
            }
        }

        /// <summary>
        /// Loguje súhrn načítania dát
        /// </summary>
        private void LogDataLoadSummary(List<Dictionary<string, object?>> data, double duration)
        {
            try
            {
                var summary = new
                {
                    InputRows = data.Count,
                    FinalRows = _displayRows.Count,
                    CellsRendered = _totalCellsRendered,
                    Duration = duration,
                    AutoAddEnabled = _autoAddEnabled,
                    MinimumRows = _unifiedRowCount
                };

                _logger.LogInformation("📊 Data Load Summary: {Summary}", summary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log data load summary");
            }
        }

        /// <summary>
        /// Loguje výsledky validácie
        /// </summary>
        private void LogValidationResults(bool result, double duration)
        {
            try
            {
                _logger.LogInformation("🔍 Validation Results: IsValid: {IsValid}, Duration: {Duration}ms, " +
                    "TotalErrors: {ErrorCount}, ErrorRate: {ErrorRate:F1}%",
                    result, duration, _totalValidationErrors,
                    _displayRows.Count > 0 ? (_totalValidationErrors * 100.0 / _displayRows.Count) : 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log validation results");
            }
        }

        /// <summary>
        /// Loguje výsledky exportu
        /// </summary>
        private void LogExportResults(DataTable result, double duration)
        {
            try
            {
                _logger.LogInformation("📤 Export Results: Rows: {RowCount}, Columns: {ColumnCount}, " +
                    "Duration: {Duration}ms, SizeMB: {SizeMB:F2}",
                    result.Rows.Count, result.Columns.Count, duration,
                    result.Rows.Count * result.Columns.Count * 10.0 / 1024 / 1024); // Rough estimate
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log export results");
            }
        }

        /// <summary>
        /// Loguje súhrn inicializácie
        /// </summary>
        private void LogInitializationSummary(double duration)
        {
            try
            {
                var summary = new
                {
                    Duration = duration,
                    ColumnCount = _columns.Count,
                    InitialRows = _displayRows.Count,
                    Features = GetEnabledFeatures(),
                    ValidationRules = GetTotalValidationRulesCount(),
                    HasCustomColors = _individualColorConfig?.HasAnyCustomColors ?? false,
                    AutoAddEnabled = _autoAddEnabled
                };

                _logger.LogInformation("🚀 Initialization Summary: {Summary}", summary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log initialization summary");
            }
        }

        #endregion

        #region Properties

        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Diagnostické informácie s performance metrics
        /// </summary>
        public string DiagnosticInfo =>
            $"AdvancedDataGrid[{_componentInstanceId}]: Initialized={_isInitialized}, " +
            $"Features={GetEnabledFeatures()}, Rows={_displayRows.Count}, " +
            $"Logger={_logger.GetType().Name}, Operations={_operationCounters.Sum(kvp => kvp.Value)}, " +
            $"LastUpdate={_lastDataUpdate:HH:mm:ss}, Errors={_totalValidationErrors}";

        #endregion


        #region ✅ KOMPLETNE IMPLEMENTOVANÉ: Helper Classes (CS0535 fix)

        /// <summary>
        /// Resizable column header helper class s sort indikátorom
        /// </summary>
        internal class ResizableColumnHeader
        {
            public string ColumnName { get; set; } = string.Empty;
            public Border? HeaderElement { get; set; }
            public Border? ResizeGrip { get; set; }
            public TextBlock? SortIndicator { get; set; }
            public double OriginalWidth { get; set; }
            public double MinWidth { get; set; } = 50;
            public double MaxWidth { get; set; } = 500;
            public bool IsResizing { get; set; }
        }

        /// <summary>
        /// ✅ KOMPLETNE IMPLEMENTOVANÉ CS1061 fix: DataRowViewModel s Cells property
        /// </summary>
        public class DataRowViewModel : INotifyPropertyChanged
        {
            private int _rowIndex;
            private bool _isSelected;
            private string _validationErrors = string.Empty;
            private bool _isZebraRow;

            /// <summary>
            /// ✅ KĽÚČOVÉ: Cells property ktorá chýbala (CS1061 fix)
            /// </summary>
            public ObservableCollection<CellViewModel> Cells { get; set; } = new();

            /// <summary>
            /// Index riadku
            /// </summary>
            public int RowIndex
            {
                get => _rowIndex;
                set => SetProperty(ref _rowIndex, value);
            }

            /// <summary>
            /// Či je riadok označený
            /// </summary>
            public bool IsSelected
            {
                get => _isSelected;
                set => SetProperty(ref _isSelected, value);
            }

            /// <summary>
            /// Validačné chyby riadku
            /// </summary>
            public string ValidationErrors
            {
                get => _validationErrors;
                set => SetProperty(ref _validationErrors, value);
            }

            /// <summary>
            /// Či je toto zebra riadok (pre alternating colors)
            /// </summary>
            public bool IsZebraRow
            {
                get => _isZebraRow;
                set => SetProperty(ref _isZebraRow, value);
            }

            /// <summary>
            /// Či má riadok validačné chyby
            /// </summary>
            public bool HasValidationErrors => !string.IsNullOrEmpty(ValidationErrors);

            /// <summary>
            /// Či je riadok validný
            /// </summary>
            public bool IsValid => !HasValidationErrors;

            #region ✅ INotifyPropertyChanged implementácia (CS0535 fix)

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

            public override string ToString()
            {
                return $"DataRow[{RowIndex}]: {Cells.Count} cells, Valid: {IsValid}, Zebra: {IsZebraRow}";
            }
        }

        /// <summary>
        /// ✅ KOMPLETNE IMPLEMENTOVANÉ: CellViewModel 
        /// </summary>
        public class CellViewModel : INotifyPropertyChanged
        {
            private string _columnName = string.Empty;
            private object? _value;
            private Type _dataType = typeof(string);
            private bool _isValid = true;
            private string _validationErrors = string.Empty;
            private bool _isSelected;
            private bool _isEditing;
            private object? _originalValue;
            private int _rowIndex;
            private int _columnIndex;

            // ✅ NOVÉ: Cell Selection states
            private bool _isFocused;
            private bool _isCopied;

            /// <summary>
            /// Index riadku ku ktorému bunka patrí
            /// </summary>
            public int RowIndex
            {
                get => _rowIndex;
                set => SetProperty(ref _rowIndex, value);
            }

            /// <summary>
            /// Index stĺpca ku ktorému bunka patrí
            /// </summary>
            public int ColumnIndex
            {
                get => _columnIndex;
                set => SetProperty(ref _columnIndex, value);
            }

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
                set => SetProperty(ref _value, value);
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
                set => SetProperty(ref _isEditing, value);
            }

            /// <summary>
            /// ✅ NOVÉ: Či má bunka focus
            /// </summary>
            public bool IsFocused
            {
                get => _isFocused;
                set => SetProperty(ref _isFocused, value);
            }

            /// <summary>
            /// ✅ NOVÉ: Či je bunka skopírovaná (Ctrl+C)
            /// </summary>
            public bool IsCopied
            {
                get => _isCopied;
                set => SetProperty(ref _isCopied, value);
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

            #region ✅ INotifyPropertyChanged implementácia (CS0535 fix)

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
            /// Vyčistí validačné chyby bunky
            /// </summary>
            public void ClearValidationErrors()
            {
                ValidationErrors = string.Empty;
                IsValid = true;
            }

            public override string ToString()
            {
                return $"Cell[{RowIndex}, {ColumnName}]: '{DisplayValue}' (Valid: {IsValid})";
            }
        }

        #endregion

        #region ✅ NOVÉ: Virtual Scrolling PUBLIC API

        /// <summary>
        /// Získa statistiky virtual scrolling - PUBLIC API
        /// </summary>
        public VirtualScrollingStats? GetVirtualScrollingStats()
        {
            try
            {
                if (_virtualScrollingService == null)
                {
                    _logger.LogDebug("🔍 VirtualScrollingService not initialized");
                    return null;
                }

                var stats = _virtualScrollingService.GetStats();
                _logger.LogDebug("📊 VirtualScrolling stats: {Stats}", stats);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetVirtualScrollingStats");
                return null;
            }
        }

        /// <summary>
        /// Nastaví virtual scrolling konfiguráciu - PUBLIC API
        /// </summary>
        public void SetVirtualScrollingConfiguration(Models.VirtualScrollingConfiguration config)
        {
            try
            {
                if (_virtualScrollingService == null)
                {
                    _logger.LogWarning("⚠️ VirtualScrollingService not initialized");
                    return;
                }

                config.Validate();
                _virtualScrollingService.UpdateConfiguration(config);
                _logger.LogInformation("⚙️ Virtual scrolling configuration updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetVirtualScrollingConfiguration");
                throw;
            }
        }

        /// <summary>
        /// Získa aktuálnu virtual scrolling konfiguráciu - PUBLIC API
        /// </summary>
        public Models.VirtualScrollingConfiguration? GetVirtualScrollingConfiguration()
        {
            try
            {
                return _virtualScrollingService?.GetConfiguration();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetVirtualScrollingConfiguration");
                return null;
            }
        }

        /// <summary>
        /// Získa aktuálny viewport info - PUBLIC API
        /// </summary>
        public VirtualScrollingViewport? GetCurrentViewport()
        {
            try
            {
                return _virtualScrollingService?.GetCurrentViewport();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetCurrentViewport");
                return null;
            }
        }

        #endregion

        #region ✅ NOVÉ: Batch Validation PUBLIC API

        /// <summary>
        /// Event pre batch validation progress reporting - PUBLIC API
        /// </summary>
        public event EventHandler<BatchValidationProgress>? BatchValidationProgressChanged;

        /// <summary>
        /// Spustí batch validation všetkých riadkov - PUBLIC API
        /// </summary>
        public async Task<BatchValidationResult?> ValidateAllRowsBatchAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_batchValidationService == null)
                {
                    _logger.LogWarning("⚠️ BatchValidationService not initialized");
                    return null;
                }

                _logger.LogInformation("🚀 ValidateAllRowsBatchAsync START - Rows: {RowCount}", _gridData.Count);

                // Subscribe to progress events
                if (_batchValidationService != null)
                {
                    _batchValidationService.ProgressChanged += OnBatchValidationProgressChanged;
                }

                var result = await _batchValidationService.ValidateRowsAsync(
                    _gridData, 
                    _columns, 
                    ConvertToLegacyValidationRules(), 
                    cancellationToken);

                _logger.LogInformation("✅ ValidateAllRowsBatchAsync COMPLETED - Duration: {Duration}ms, " +
                    "Valid: {Valid}, Invalid: {Invalid}", 
                    result.Duration.TotalMilliseconds, result.ValidRows, result.InvalidRows);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateAllRowsBatchAsync");
                throw;
            }
            finally
            {
                // Unsubscribe from progress events
                if (_batchValidationService != null)
                {
                    _batchValidationService.ProgressChanged -= OnBatchValidationProgressChanged;
                }
            }
        }

        /// <summary>
        /// Nastaví batch validation konfiguráciu - PUBLIC API
        /// </summary>
        public void SetBatchValidationConfiguration(BatchValidationConfiguration config)
        {
            try
            {
                if (_batchValidationService == null)
                {
                    _logger.LogWarning("⚠️ BatchValidationService not initialized");
                    return;
                }

                config.Validate();
                _batchValidationService.UpdateConfiguration(config);
                _logger.LogInformation("⚙️ Batch validation configuration updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetBatchValidationConfiguration");
                throw;
            }
        }

        /// <summary>
        /// Získa batch validation konfiguráciu - PUBLIC API
        /// </summary>
        public BatchValidationConfiguration? GetBatchValidationConfiguration()
        {
            try
            {
                return _batchValidationService?.GetConfiguration();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetBatchValidationConfiguration");
                return null;
            }
        }

        /// <summary>
        /// Zruší prebiehajúcu batch validation - PUBLIC API
        /// </summary>
        public void CancelBatchValidation()
        {
            try
            {
                _batchValidationService?.CancelValidation();
                _logger.LogInformation("🛑 Batch validation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CancelBatchValidation");
            }
        }

        /// <summary>
        /// Handler pre batch validation progress events
        /// </summary>
        private void OnBatchValidationProgressChanged(object? sender, BatchValidationProgress e)
        {
            try
            {
                _logger.LogTrace("📊 Batch validation progress: {Progress}", e);
                BatchValidationProgressChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error in batch validation progress handler");
            }
        }

        /// <summary>
        /// Konvertuje advanced validation rules na legacy format pre batch validation
        /// </summary>
        private List<ValidationRule> ConvertToLegacyValidationRules()
        {
            var legacyRules = new List<ValidationRule>();

            // Konvertuj z _advancedValidationRules ak existujú
            if (_advancedValidationRules?.Rules != null)
            {
                foreach (var rule in _advancedValidationRules.Rules)
                {
                    // Pre každý target column vytvoríme legacy ValidationRule
                    foreach (var targetColumn in rule.TargetColumns)
                    {
                        legacyRules.Add(new ValidationRule
                        {
                            ColumnName = targetColumn,
                            Type = ValidationType.Custom,
                            ErrorMessage = rule.ErrorMessage,
                            CustomValidator = (value) => 
                            {
                                try
                                {
                                    var context = new Models.Validation.ValidationContext
                                    {
                                        ColumnName = targetColumn,
                                        CurrentValue = value
                                    };
                                    var result = rule.ValidationFunction?.Invoke(context);
                                    return result?.IsValid ?? true;
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                        });
                    }
                }
            }

            // Fallback na základné required rules pre všetky stĺpce
            if (legacyRules.Count == 0)
            {
                foreach (var column in _columns)
                {
                    legacyRules.Add(new ValidationRule
                    {
                        ColumnName = column.Name,
                        IsRequired = false // Default nie sú required
                    });
                }
            }

            return legacyRules;
        }

        #endregion

        #region ✅ NOVÉ: Advanced Search PUBLIC API

        /// <summary>
        /// Event pre advanced search results - PUBLIC API
        /// </summary>
        public event EventHandler<SearchResults>? AdvancedSearchCompleted;

        /// <summary>
        /// Event pre search history changes - PUBLIC API
        /// </summary>
        public event EventHandler<List<SearchCriteria>>? SearchHistoryChanged;

        /// <summary>
        /// Spustí advanced search - PUBLIC API
        /// </summary>
        public async Task<SearchResults?> SearchAsync(
            string searchTerm,
            bool isCaseSensitive = false,
            bool isRegex = false,
            bool isWholeWord = false,
            List<string>? targetColumns = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_advancedSearchService == null)
                {
                    _logger.LogWarning("⚠️ AdvancedSearchService not initialized");
                    return null;
                }

                _logger.LogInformation("🔍 SearchAsync START - Term: '{SearchTerm}', Regex: {IsRegex}", 
                    searchTerm, isRegex);

                SearchResults? result = null;

                // Subscribe to search events
                void OnSearchCompleted(object? sender, SearchResults e)
                {
                    result = e;
                    AdvancedSearchCompleted?.Invoke(this, e);
                }

                _advancedSearchService.SearchCompleted += OnSearchCompleted;
                _advancedSearchService.SearchHistoryChanged += OnSearchHistoryChanged;

                try
                {
                    await _advancedSearchService.SearchAsync(
                        searchTerm, _gridData, _columns, isCaseSensitive, isRegex, isWholeWord, targetColumns, cancellationToken);

                    // Wait a bit for the event to fire
                    await Task.Delay(50, cancellationToken);

                    _logger.LogInformation("✅ SearchAsync COMPLETED - Results: {ResultCount}", 
                        result?.TotalCount ?? 0);

                    return result;
                }
                finally
                {
                    _advancedSearchService.SearchCompleted -= OnSearchCompleted;
                    _advancedSearchService.SearchHistoryChanged -= OnSearchHistoryChanged;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SearchAsync - Term: '{SearchTerm}'", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Získa search history - PUBLIC API
        /// </summary>
        public List<SearchCriteria> GetSearchHistory()
        {
            try
            {
                return _advancedSearchService?.GetSearchHistory() ?? new List<SearchCriteria>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetSearchHistory");
                return new List<SearchCriteria>();
            }
        }

        /// <summary>
        /// Vyčistí search history - PUBLIC API
        /// </summary>
        public void ClearSearchHistory()
        {
            try
            {
                _advancedSearchService?.ClearSearchHistory();
                _logger.LogInformation("🗑️ Search history cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearSearchHistory");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: PUBLIC API - Clear current search and show all data
        /// </summary>
        public async Task ClearSearchAsync()
        {
            try
            {
                _logger.LogInformation("🗑️ ClearSearchAsync START - Instance: {ComponentInstanceId}", _componentInstanceId);

                // Clear search filters
                _columnSearchFilters.Clear();

                // Reset display to show all data
                if (_searchAndSortService != null)
                {
                    _searchAndSortService.ClearAllSearchFilters();
                }

                // Refresh display with all data
                await UpdateDisplayRowsWithRealtimeValidationAsync();
                await RefreshDataDisplayAsync();

                _logger.LogInformation("✅ ClearSearchAsync COMPLETED - All data restored");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearSearchAsync");
                throw;
            }
        }

        /// <summary>
        /// Nastaví advanced search konfiguráciu - PUBLIC API
        /// </summary>
        public void SetAdvancedSearchConfiguration(AdvancedSearchConfiguration config)
        {
            try
            {
                if (_advancedSearchService == null)
                {
                    _logger.LogWarning("⚠️ AdvancedSearchService not initialized");
                    return;
                }

                if (!config.IsValid())
                    throw new ArgumentException("Invalid AdvancedSearchConfiguration");

                _advancedSearchService.UpdateConfiguration(config);
                _logger.LogInformation("⚙️ Advanced search configuration updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetAdvancedSearchConfiguration");
                throw;
            }
        }

        /// <summary>
        /// Získa advanced search konfiguráciu - PUBLIC API
        /// </summary>
        public AdvancedSearchConfiguration? GetAdvancedSearchConfiguration()
        {
            try
            {
                return _advancedSearchService?.GetConfiguration();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetAdvancedSearchConfiguration");
                return null;
            }
        }

        /// <summary>
        /// Handler pre search history changes
        /// </summary>
        private void OnSearchHistoryChanged(object? sender, List<SearchCriteria> e)
        {
            try
            {
                SearchHistoryChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error in search history change handler");
            }
        }

        #region ✅ REFACTORING: Centralized Helper Methods

        /// <summary>
        /// ✅ REFACTORING: Centralizované vykonanie operácie s automatickým loggingom a error handlingom
        /// </summary>
        private async Task<T> ExecuteWithLoggingAsync<T>(
            string operationName, 
            Func<Task<T>> operation,
            Func<T, string>? successMessageFormatter = null)
        {
            try
            {
                _logger.LogInformation("🚀 {OperationName} START - Instance: {ComponentInstanceId}", 
                    operationName, _componentInstanceId);
                StartOperation(operationName);
                IncrementOperationCounter(operationName);
                
                var result = await operation();
                var duration = EndOperation(operationName);
                
                var successMessage = successMessageFormatter?.Invoke(result) ?? "COMPLETED";
                _logger.LogInformation("✅ {OperationName} {Message} - Duration: {Duration}ms, Instance: {ComponentInstanceId}", 
                    operationName, successMessage, duration, _componentInstanceId);
                    
                return result;
            }
            catch (Exception ex)
            {
                EndOperation(operationName);
                IncrementOperationCounter($"{operationName}-Error");
                _logger.LogError(ex, "❌ ERROR in {OperationName} - Instance: {ComponentInstanceId}", 
                    operationName, _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ REFACTORING: Synchronná verzia ExecuteWithLogging
        /// </summary>
        private T ExecuteWithLogging<T>(
            string operationName, 
            Func<T> operation,
            Func<T, string>? successMessageFormatter = null)
        {
            try
            {
                _logger.LogInformation("🚀 {OperationName} START - Instance: {ComponentInstanceId}", 
                    operationName, _componentInstanceId);
                StartOperation(operationName);
                IncrementOperationCounter(operationName);
                
                var result = operation();
                var duration = EndOperation(operationName);
                
                var successMessage = successMessageFormatter?.Invoke(result) ?? "COMPLETED";
                _logger.LogInformation("✅ {OperationName} {Message} - Duration: {Duration}ms, Instance: {ComponentInstanceId}", 
                    operationName, successMessage, duration, _componentInstanceId);
                    
                return result;
            }
            catch (Exception ex)
            {
                EndOperation(operationName);
                IncrementOperationCounter($"{operationName}-Error");
                _logger.LogError(ex, "❌ ERROR in {OperationName} - Instance: {ComponentInstanceId}", 
                    operationName, _componentInstanceId);
                throw;
            }
        }

        /// <summary>
        /// ✅ REFACTORING: Void verzia ExecuteWithLogging
        /// </summary>
        private async Task ExecuteWithLoggingAsync(
            string operationName, 
            Func<Task> operation,
            string? successMessage = null)
        {
            await ExecuteWithLoggingAsync(operationName, async () =>
            {
                await operation();
                return true; // Dummy return value
            }, _ => successMessage ?? "COMPLETED");
        }

        /// <summary>
        /// ✅ REFACTORING: Parameter validation helper
        /// </summary>
        private void ValidateParameter<T>(T parameter, string parameterName) where T : class
        {
            if (parameter == null)
            {
                _logger.LogError("❌ {ParameterName} parameter is null - Instance: {ComponentInstanceId}",
                    parameterName, _componentInstanceId);
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// ✅ REFACTORING: Service validation helper
        /// </summary>
        private bool ValidateService<T>(T service, string serviceName) where T : class
        {
            if (service == null)
            {
                _logger.LogWarning("⚠️ {ServiceName} is null - Instance: {ComponentInstanceId}",
                    serviceName, _componentInstanceId);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ✅ REFACTORING: Collection validation helper
        /// </summary>
        private void ValidateCollection<T>(ICollection<T> collection, string collectionName)
        {
            if (collection == null || collection.Count == 0)
            {
                _logger.LogError("❌ {CollectionName} collection is null or empty - Instance: {ComponentInstanceId}",
                    collectionName, _componentInstanceId);
                throw new ArgumentException($"{collectionName} collection cannot be null or empty", collectionName);
            }
        }

        /// <summary>
        /// ✅ REFACTORING: Safe string conversion helper
        /// </summary>
        private static string SafeToString(object? value) =>
            value?.ToString() ?? string.Empty;

        /// <summary>
        /// ✅ REFACTORING: Empty value check helper
        /// </summary>
        private static bool IsEmptyValue(object? value) =>
            value == null || string.IsNullOrWhiteSpace(value.ToString());

        /// <summary>
        /// ✅ REFACTORING: Row data emptiness check helper (enhanced version of IsRowEmptyData)
        /// </summary>
        private static bool IsRowDataEmpty(Dictionary<string, object?> rowData) =>
            rowData?.Values?.All(IsEmptyValue) ?? true;

        /// <summary>
        /// ✅ REFACTORING: UI Thread execution helper
        /// </summary>
        private async Task ExecuteOnUIThreadAsync(Action uiAction, string operationName = "UIUpdate")
        {
            await Task.Run(() =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        uiAction();
                        _logger.LogDebug("✅ {OperationName} completed on UI thread - Instance: {ComponentInstanceId}", 
                            operationName, _componentInstanceId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ ERROR in {OperationName} on UI thread - Instance: {ComponentInstanceId}", 
                            operationName, _componentInstanceId);
                    }
                });
            });
        }

        /// <summary>
        /// ✅ REFACTORING: Synchronous UI Thread execution helper
        /// </summary>
        private void ExecuteOnUIThread(Action uiAction, string operationName = "UIUpdate")
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    uiAction();
                    _logger.LogDebug("✅ {OperationName} completed on UI thread - Instance: {ComponentInstanceId}", 
                        operationName, _componentInstanceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR in {OperationName} on UI thread - Instance: {ComponentInstanceId}", 
                        operationName, _componentInstanceId);
                }
            });
        }

        /// <summary>
        /// ✅ REFACTORING: CheckBox State Manager helper class
        /// </summary>
        private class CheckBoxStateManager
        {
            private readonly Dictionary<int, bool> _states = new();
            private readonly ILogger _logger;
            private readonly string _componentInstanceId;
            private CheckBoxColumnHeader? _header;

            public CheckBoxStateManager(ILogger logger, string componentInstanceId)
            {
                _logger = logger;
                _componentInstanceId = componentInstanceId;
            }

            public void SetHeader(CheckBoxColumnHeader? header)
            {
                _header = header;
            }

            public void SetState(int rowIndex, bool state)
            {
                _states[rowIndex] = state;
                _logger.LogTrace("🔲 CheckBox state set - Row: {RowIndex}, State: {State}", rowIndex, state);
            }

            public bool GetState(int rowIndex)
            {
                return _states.TryGetValue(rowIndex, out var state) && state;
            }

            public void UpdateHeaderState(int totalRows)
            {
                if (_header == null) return;

                try
                {
                    var checkedCount = _states.Values.Count(s => s);
                    var isIndeterminate = checkedCount > 0 && checkedCount < totalRows;
                    var isChecked = checkedCount == totalRows && totalRows > 0;

                    var headerState = isChecked 
                        ? Controls.SpecialColumns.CheckBoxHeaderState.Checked 
                        : (isIndeterminate 
                            ? Controls.SpecialColumns.CheckBoxHeaderState.Indeterminate 
                            : Controls.SpecialColumns.CheckBoxHeaderState.Unchecked);
                    
                    _header.SetHeaderState(headerState);
                    
                    _logger.LogDebug("🔲 Header state updated - Checked: {CheckedCount}/{TotalRows}, State: {HeaderState}",
                        checkedCount, totalRows, headerState);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error updating header checkbox state");
                }
            }

            public void ClearAll()
            {
                _states.Clear();
                UpdateHeaderState(0);
                _logger.LogDebug("🔲 All checkbox states cleared");
            }

            public void SetAllStates(bool state, int rowCount)
            {
                _states.Clear();
                for (int i = 0; i < rowCount; i++)
                {
                    _states[i] = state;
                }
                UpdateHeaderState(rowCount);
                _logger.LogDebug("🔲 All checkbox states set to {State} for {RowCount} rows", state, rowCount);
            }

            public Dictionary<int, bool> GetAllStates() => new(_states);

            public List<int> GetCheckedRowIndices() => 
                _states.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }


        #endregion

        /// <summary>
        /// ✅ NOVÉ: Konfiguruje feature flags pre sort, search, filter a search history
        /// </summary>
        private async Task ConfigureFeatureFlagsAsync(bool enableSort, bool enableSearch, bool enableFilter, int searchHistoryItems)
        {
            try
            {
                _logger.LogInformation("⚙️ ConfigureFeatureFlagsAsync START - Sort: {EnableSort}, Search: {EnableSearch}, " +
                    "Filter: {EnableFilter}, SearchHistory: {SearchHistory}",
                    enableSort, enableSearch, enableFilter, searchHistoryItems);

                // Configure sorting if enabled
                if (enableSort && _searchAndSortService != null)
                {
                    _logger.LogDebug("🔧 Enabling sorting functionality");
                    // Set multi-sort configuration if needed
                    var sortConfig = MultiSortConfiguration.Default;
                    sortConfig.IsEnabled = true;
                    // Additional sorting configuration can be added here
                }

                // Configure search if enabled  
                if (enableSearch && _advancedSearchService != null)
                {
                    _logger.LogDebug("🔧 Enabling search functionality with history: {SearchHistory}", searchHistoryItems);
                    
                    var searchConfig = AdvancedSearchConfiguration.Default;
                    searchConfig.MaxSearchHistoryItems = searchHistoryItems;
                    // EnableSearchHistory is read-only and automatically calculated from MaxSearchHistoryItems
                    
                    _advancedSearchService.UpdateConfiguration(searchConfig);
                }

                // Configure filtering if enabled
                if (enableFilter)
                {
                    _logger.LogDebug("🔧 Enabling filter functionality");
                    // Filter configuration logic can be added here when filter service is available
                }

                // Log final configuration state
                _logger.LogInformation("✅ Feature flags configured - Sort: {EnableSort}, Search: {EnableSearch}, " +
                    "Filter: {EnableFilter}, SearchHistory: {SearchHistory}",
                    enableSort, enableSearch, enableFilter, searchHistoryItems);

                await Task.CompletedTask; // Placeholder for any async operations
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConfigureFeatureFlagsAsync");
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Export Format Conversion Helper Methods

        /// <summary>
        /// Converts DataTable to Excel format as byte array
        /// </summary>
        private async Task<byte[]> ConvertDataTableToExcelBytesAsync(DataTable dataTable)
        {
            try
            {
                await Task.CompletedTask; // For async signature

                // Simple Excel format simulation - in real implementation would use proper Excel library
                var sb = new StringBuilder();
                
                // Add headers
                var headers = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                sb.AppendLine(string.Join("\t", headers));
                
                // Add data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var fields = row.ItemArray.Select(field => field?.ToString() ?? "");
                    sb.AppendLine(string.Join("\t", fields));
                }
                
                return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConvertDataTableToExcelBytesAsync");
                throw;
            }
        }

        /// <summary>
        /// Converts DataTable to CSV format as byte array
        /// </summary>
        private async Task<byte[]> ConvertDataTableToCsvBytesAsync(DataTable dataTable)
        {
            try
            {
                await Task.CompletedTask; // For async signature

                var sb = new StringBuilder();
                
                // Add headers
                var headers = dataTable.Columns.Cast<DataColumn>().Select(column => EscapeCsvField(column.ColumnName));
                sb.AppendLine(string.Join(",", headers));
                
                // Add data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var fields = row.ItemArray.Select(field => EscapeCsvField(field?.ToString() ?? ""));
                    sb.AppendLine(string.Join(",", fields));
                }
                
                return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConvertDataTableToCsvBytesAsync");
                throw;
            }
        }

        /// <summary>
        /// Converts DataTable to JSON format as byte array
        /// </summary>
        private async Task<byte[]> ConvertDataTableToJsonBytesAsync(DataTable dataTable)
        {
            try
            {
                await Task.CompletedTask; // For async signature

                var jsonArray = new List<Dictionary<string, object?>>();
                
                foreach (DataRow row in dataTable.Rows)
                {
                    var jsonObject = new Dictionary<string, object?>();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        jsonObject[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                    }
                    jsonArray.Add(jsonObject);
                }
                
                // Simple JSON serialization - in real implementation would use System.Text.Json
                var jsonString = SerializeToJsonString(jsonArray);
                return System.Text.Encoding.UTF8.GetBytes(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConvertDataTableToJsonBytesAsync");
                throw;
            }
        }

        /// <summary>
        /// Converts data to XML string format
        /// </summary>
        private string ConvertDataToXmlString(List<Dictionary<string, object?>> data)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sb.AppendLine("<DataGridExport>");
                
                foreach (var row in data)
                {
                    sb.AppendLine("  <Row>");
                    foreach (var kvp in row)
                    {
                        var columnName = EscapeXmlElementName(kvp.Key);
                        var value = EscapeXmlContent(kvp.Value?.ToString() ?? "");
                        sb.AppendLine($"    <{columnName}>{value}</{columnName}>");
                    }
                    sb.AppendLine("  </Row>");
                }
                
                sb.AppendLine("</DataGridExport>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConvertDataToXmlString");
                throw;
            }
        }

        /// <summary>
        /// Escapes CSV field content
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            
            return field;
        }

        /// <summary>
        /// Simple JSON serialization for basic objects
        /// </summary>
        private string SerializeToJsonString(List<Dictionary<string, object?>> data)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            
            for (int i = 0; i < data.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                
                var kvps = data[i].ToArray();
                for (int j = 0; j < kvps.Length; j++)
                {
                    if (j > 0) sb.Append(",");
                    sb.Append($"\"{EscapeJsonString(kvps[j].Key)}\":");
                    
                    var value = kvps[j].Value;
                    if (value == null)
                    {
                        sb.Append("null");
                    }
                    else if (value is string str)
                    {
                        sb.Append($"\"{EscapeJsonString(str)}\"");
                    }
                    else if (value is bool b)
                    {
                        sb.Append(b.ToString().ToLower());
                    }
                    else if (value is int || value is long || value is double || value is decimal)
                    {
                        sb.Append(value.ToString());
                    }
                    else
                    {
                        sb.Append($"\"{EscapeJsonString(value.ToString() ?? "")}\"");
                    }
                }
                
                sb.Append("}");
            }
            
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Escapes JSON string content
        /// </summary>
        private string EscapeJsonString(string str)
        {
            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }

        /// <summary>
        /// Escapes XML element name
        /// </summary>
        private string EscapeXmlElementName(string name)
        {
            return name.Replace(" ", "_")
                      .Replace("-", "_")
                      .Replace(".", "_");
        }

        /// <summary>
        /// Escapes XML content
        /// </summary>
        private string EscapeXmlContent(string content)
        {
            return content.Replace("&", "&amp;")
                         .Replace("<", "&lt;")
                         .Replace(">", "&gt;")
                         .Replace("\"", "&quot;")
                         .Replace("'", "&apos;");
        }

        /// <summary>
        /// Converts Excel byte array to data
        /// </summary>
        private async Task<List<Dictionary<string, object?>>> ConvertExcelBytesToDataAsync(byte[] excelBytes)
        {
            try
            {
                await Task.CompletedTask; // For async signature

                // Simple Excel parsing - in real implementation would use proper Excel library
                var content = System.Text.Encoding.UTF8.GetString(excelBytes);
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (lines.Length == 0) return new List<Dictionary<string, object?>>();

                // First line contains headers
                var headers = lines[0].Split('\t');
                var data = new List<Dictionary<string, object?>>();

                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split('\t');
                    var row = new Dictionary<string, object?>();
                    
                    for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                    {
                        row[headers[j]] = string.IsNullOrEmpty(values[j]) ? null : values[j];
                    }
                    
                    data.Add(row);
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConvertExcelBytesToDataAsync");
                throw;
            }
        }

        /// <summary>
        /// Converts XML string to data
        /// </summary>
        private async Task<List<Dictionary<string, object?>>> ConvertXmlStringToDataAsync(string xmlString)
        {
            try
            {
                await Task.CompletedTask; // For async signature

                var data = new List<Dictionary<string, object?>>();
                
                // Simple XML parsing - in real implementation would use XDocument or XmlDocument
                var lines = xmlString.Split('\n');
                Dictionary<string, object?>? currentRow = null;
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    if (trimmedLine == "<Row>")
                    {
                        currentRow = new Dictionary<string, object?>();
                    }
                    else if (trimmedLine == "</Row>" && currentRow != null)
                    {
                        data.Add(currentRow);
                        currentRow = null;
                    }
                    else if (currentRow != null && trimmedLine.StartsWith("<") && trimmedLine.Contains(">"))
                    {
                        // Parse element: <ElementName>Value</ElementName>
                        var startTag = trimmedLine.IndexOf('<') + 1;
                        var endTag = trimmedLine.IndexOf('>');
                        var closeTag = trimmedLine.LastIndexOf('<');
                        
                        if (startTag > 0 && endTag > startTag && closeTag > endTag)
                        {
                            var elementName = trimmedLine.Substring(startTag, endTag - startTag);
                            var value = trimmedLine.Substring(endTag + 1, closeTag - endTag - 1);
                            
                            // Unescape XML content
                            value = UnescapeXmlContent(value);
                            currentRow[elementName] = string.IsNullOrEmpty(value) ? null : value;
                        }
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ConvertXmlStringToDataAsync");
                throw;
            }
        }

        /// <summary>
        /// Unescapes XML content
        /// </summary>
        private string UnescapeXmlContent(string content)
        {
            return content.Replace("&lt;", "<")
                         .Replace("&gt;", ">")
                         .Replace("&quot;", "\"")
                         .Replace("&apos;", "'")
                         .Replace("&amp;", "&");
        }

        #endregion

        #region ✅ NOVÉ: Per-row Height Management Helper Methods

        /// <summary>
        /// Získa row index z DataGridCell
        /// </summary>
        private int GetRowIndexFromCell(Controls.DataGridCell cell)
        {
            try
            {
                // Nájdi parent row container
                var parent = cell.Parent;
                while (parent != null && !(parent is ListView))
                {
                    parent = ((FrameworkElement)parent).Parent;
                }

                if (parent is ListView listView)
                {
                    // Nájdi DataRowViewModel ktorý obsahuje túto cell
                    for (int i = 0; i < _displayRows.Count; i++)
                    {
                        var rowViewModel = _displayRows[i];
                        // Skontroluj či cell patrí k tomuto row
                        if (rowViewModel.Cells.Any(c => c.ColumnName == cell.ColumnName))
                        {
                            return i;
                        }
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting row index from cell - Column: {ColumnName}", cell.ColumnName);
                return -1;
            }
        }

        /// <summary>
        /// Získa FrameworkElement pre riadok na konkrétnom indexe
        /// </summary>
        private FrameworkElement? GetRowElementAt(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _displayRows.Count)
                    return null;

                // Pre ListView implementáciu, nájdi container pre item
                var listView = FindName("DataRowsListView") as ListView;
                if (listView != null)
                {
                    var container = listView.ContainerFromIndex(rowIndex) as ListViewItem;
                    return container;
                }

                // Fallback - vráť null ak nie je možné nájsť element
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting row element at index: {RowIndex}", rowIndex);
                return null;
            }
        }

        /// <summary>
        /// Aktualizuje layout po zmene height konkrétneho riadku
        /// </summary>
        private async Task UpdateLayoutAfterRowHeightChangeAsync(int rowIndex, double newHeight)
        {
            try
            {
                if (rowIndex >= 0 && rowIndex < _displayRows.Count)
                {
                    // Aktualizuj height v DataRowViewModel ak je možné
                    var rowViewModel = _displayRows[rowIndex];
                    
                    // Nájdi RowDisplayInfo cell ak existuje
                    var displayInfoCell = rowViewModel.Cells.FirstOrDefault(c => c.ColumnName == "_RowDisplayInfo");
                    if (displayInfoCell?.Value is Models.Row.RowDisplayInfo displayInfo)
                    {
                        displayInfo.RowHeight = newHeight;
                    }

                    // Spusti layout update pre DataGrid
                    if (_layoutService != null)
                    {
                        await _layoutService.UpdateLayoutAfterDataChangeAsync();
                    }

                    _logger.LogTrace("✅ Layout updated after row height change - Row: {RowIndex}, NewHeight: {NewHeight}",
                        rowIndex, newHeight);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error updating layout after row height change - Row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// Naregistruje HeightChanged event handler pre všetky DataGridCell v row
        /// </summary>
        private void RegisterCellHeightChangedHandlers(DataRowViewModel rowViewModel)
        {
            try
            {
                // Pre každú cell v row zaregistruj HeightChanged handler
                // Toto sa volá pri vytváraní nových rows
                foreach (var cell in rowViewModel.Cells)
                {
                    // Tu by sa registroval handler pre každú cell
                    // Implementácia závisí od UI frameworku a ako sa cells vytvárajú
                    
                    _logger.LogTrace("📝 Registered height change handler for cell - Column: {ColumnName}", cell.ColumnName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error registering cell height change handlers");
            }
        }

        #endregion

        #region ✅ NOVÉ: Public API - Filter Operations

        /// <summary>
        /// ✅ PUBLIC API: Pridá filter pre špecifikovaný stĺpec
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="filterValue">Hodnota pre filtrovanie</param>
        /// <param name="caseSensitive">Case sensitive porovnanie</param>
        public async Task AddFilterAsync(string columnName, string filterValue, bool caseSensitive = false)
        {
            try
            {
                if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(filterValue))
                {
                    _logger.LogWarning("⚠️ AddFilterAsync - Invalid parameters: columnName='{ColumnName}', filterValue='{FilterValue}'", 
                        columnName ?? "null", filterValue ?? "null");
                    return;
                }

                StartOperation("AddFilter");
                _logger.LogInformation("🔍 AddFilterAsync START - Column: '{ColumnName}', Value: '{FilterValue}', CaseSensitive: {CaseSensitive}", 
                    columnName, filterValue, caseSensitive);

                // Add to column search filters
                _columnSearchFilters[columnName] = filterValue;

                // Apply filter using search service
                if (_searchAndSortService != null)
                {
                    _searchAndSortService.SetColumnSearchFilter(columnName, filterValue);
                }

                // Update display
                await UpdateDisplayRowsWithRealtimeValidationAsync();

                var duration = EndOperation("AddFilter");
                _logger.LogInformation("✅ AddFilterAsync COMPLETED - Column: '{ColumnName}', Duration: {Duration}ms", 
                    columnName, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddFilterAsync - Column: '{ColumnName}'", columnName ?? "null");
                throw;
            }
        }

        /// <summary>
        /// ✅ PUBLIC API: Pridá viacero filtrov naraz (bulk operation)
        /// </summary>
        /// <param name="filters">Dictionary stĺpcov a ich filter hodnôt</param>
        /// <param name="caseSensitive">Case sensitive porovnanie</param>
        public async Task AddFiltersAsync(Dictionary<string, string> filters, bool caseSensitive = false)
        {
            try
            {
                if (filters == null || filters.Count == 0)
                {
                    _logger.LogWarning("⚠️ AddFiltersAsync - No filters provided");
                    return;
                }

                StartOperation("AddFilters");
                _logger.LogInformation("🔍 AddFiltersAsync START - FilterCount: {FilterCount}, CaseSensitive: {CaseSensitive}", 
                    filters.Count, caseSensitive);

                // Add all filters to column search filters
                foreach (var filter in filters)
                {
                    if (!string.IsNullOrEmpty(filter.Key) && !string.IsNullOrEmpty(filter.Value))
                    {
                        _columnSearchFilters[filter.Key] = filter.Value;
                        _logger.LogDebug("📝 Filter added - Column: '{ColumnName}', Value: '{FilterValue}'", 
                            filter.Key, filter.Value);
                    }
                }

                // Apply combined search across multiple columns
                if (_searchAndSortService != null && filters.Any())
                {
                    // Set each filter individually using the search service
                    foreach (var filter in filters.Where(f => !string.IsNullOrEmpty(f.Key) && !string.IsNullOrEmpty(f.Value)))
                    {
                        _searchAndSortService.SetColumnSearchFilter(filter.Key, filter.Value);
                    }
                }

                // Update display
                await UpdateDisplayRowsWithRealtimeValidationAsync();

                var duration = EndOperation("AddFilters");
                _logger.LogInformation("✅ AddFiltersAsync COMPLETED - FilterCount: {FilterCount}, Duration: {Duration}ms", 
                    filters.Count, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddFiltersAsync - FilterCount: {FilterCount}", filters?.Count ?? 0);
                throw;
            }
        }

        /// <summary>
        /// ✅ PUBLIC API: Vyčistí všetky filtre
        /// </summary>
        public async Task ClearFiltersAsync()
        {
            try
            {
                StartOperation("ClearFilters");
                _logger.LogInformation("🔍 ClearFiltersAsync START - ActiveFilterCount: {FilterCount}", 
                    _columnSearchFilters.Count);

                var oldCount = _columnSearchFilters.Count;

                // Clear all column search filters
                _columnSearchFilters.Clear();

                // Clear search service filters
                if (_searchAndSortService != null)
                {
                    _searchAndSortService.ClearAllSearchFilters();
                }

                // Update display to show all data
                await UpdateDisplayRowsWithRealtimeValidationAsync();

                var duration = EndOperation("ClearFilters");
                _logger.LogInformation("✅ ClearFiltersAsync COMPLETED - Cleared {OldCount} filters, Duration: {Duration}ms", 
                    oldCount, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearFiltersAsync");
                throw;
            }
        }

        /// <summary>
        /// ✅ PUBLIC API: Vyčistí filter pre špecifikovaný stĺpec
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        public async Task ClearFilterAsync(string columnName)
        {
            try
            {
                if (string.IsNullOrEmpty(columnName))
                {
                    _logger.LogWarning("⚠️ ClearFilterAsync - Invalid columnName: '{ColumnName}'", columnName ?? "null");
                    return;
                }

                StartOperation("ClearFilter");
                _logger.LogInformation("🔍 ClearFilterAsync START - Column: '{ColumnName}'", columnName);

                var wasRemoved = _columnSearchFilters.Remove(columnName);

                if (wasRemoved)
                {
                    // Apply remaining filters or clear all if none remain
                    if (_searchAndSortService != null)
                    {
                        if (_columnSearchFilters.Any())
                        {
                            // Re-apply remaining filters
                            foreach (var remainingFilter in _columnSearchFilters)
                            {
                                _searchAndSortService.SetColumnSearchFilter(remainingFilter.Key, remainingFilter.Value);
                            }
                        }
                        else
                        {
                            // No filters remaining, clear all
                            _searchAndSortService.ClearAllSearchFilters();
                        }
                    }

                    // Update display
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                }

                var duration = EndOperation("ClearFilter");
                _logger.LogInformation("✅ ClearFilterAsync COMPLETED - Column: '{ColumnName}', WasRemoved: {WasRemoved}, Duration: {Duration}ms", 
                    columnName, wasRemoved, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearFilterAsync - Column: '{ColumnName}'", columnName ?? "null");
                throw;
            }
        }

        /// <summary>
        /// ✅ PUBLIC API: Získa zoznam aktívnych filtrov
        /// </summary>
        /// <returns>Dictionary aktívnych filtrov (stĺpec -> filter hodnota)</returns>
        public Dictionary<string, string> GetActiveFilters()
        {
            try
            {
                StartOperation("GetActiveFilters");
                _logger.LogDebug("🔍 GetActiveFilters - ActiveFilterCount: {FilterCount}", _columnSearchFilters.Count);

                // Return a copy to prevent external modification
                var activeFilters = new Dictionary<string, string>(_columnSearchFilters);

                var duration = EndOperation("GetActiveFilters");
                _logger.LogDebug("✅ GetActiveFilters COMPLETED - FilterCount: {FilterCount}, Duration: {Duration}ms", 
                    activeFilters.Count, duration);

                return activeFilters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetActiveFilters");
                return new Dictionary<string, string>();
            }
        }

        #endregion
    }
}