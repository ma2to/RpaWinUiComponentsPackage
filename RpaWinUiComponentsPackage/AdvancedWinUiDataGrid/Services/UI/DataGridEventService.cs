// Services/UI/DataGridEventService.cs - ✅ NOVÝ: Event Handling Service
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Input;
using Windows.Foundation;
using Windows.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;
using CellPosition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell.CellPosition;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.UI
{
    /// <summary>
    /// Service pre Event Handling - INTERNAL
    /// Zodpovedný za mouse/keyboard events, selection handling, drag & drop, context menu
    /// </summary>
    internal class DataGridEventService : IDisposable
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // Referencia na hlavný DataGrid control
        private AdvancedDataGrid? _dataGrid;

        // Selection management
        private readonly List<CellPosition> _selectedCells = new();
        private CellPosition? _currentCell;
        private CellPosition? _anchorCell;
        private bool _isSelecting = false;
        private bool _isMultiSelecting = false;

        // Keyboard navigation
        private bool _isNavigating = false;
        private DateTime _lastKeyPressTime = DateTime.MinValue;
        private const int KeyRepeatDelay = 100; // ms

        // Mouse operations
        private bool _isMouseDown = false;
        private Point _mouseDownPosition;
        private DateTime _mouseDownTime;
        private const int DoubleClickThreshold = 500; // ms

        // Drag & Drop
        private bool _isDragging = false;
        private CellPosition? _dragStartCell;
        private Point _dragStartPosition;
        private const double DragThreshold = 5.0; // pixels

        // Context menu
        private MenuFlyout? _contextMenu;
        private CellPosition? _contextMenuCell;

        // Selection rectangle
        private Border? _selectionRectangle;
        private bool _isShowingSelectionRectangle = false;

        // Focus management
        private bool _hasFocus = false;
        private CellPosition? _focusedCell;

        // Initialization state
        private bool _isInitialized = false;

        #endregion

        #region Events

        /// <summary>
        /// Event pre zmenu selection
        /// </summary>
        internal event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// Event pre cell click
        /// </summary>
        public event EventHandler<CellClickEventArgs>? CellClicked;

        /// <summary>
        /// Event pre cell double click
        /// </summary>
        public event EventHandler<CellClickEventArgs>? CellDoubleClicked;

        /// <summary>
        /// Event pre keyboard navigation
        /// </summary>
        public event EventHandler<NavigationEventArgs>? NavigationRequested;

        /// <summary>
        /// Event pre začiatok editácie
        /// </summary>
        public event EventHandler<CellEditEventArgs>? EditStartRequested;

        #endregion

        #region Constructor

        public DataGridEventService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("🏗️ DataGridEventService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje event service s referenciou na DataGrid
        /// </summary>
        public async Task InitializeAsync(AdvancedDataGrid dataGrid)
        {
            try
            {
                _logger.LogInformation("🚀 DataGridEventService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));

                // Registruj event handlery
                await RegisterEventHandlersAsync();
                await InitializeSelectionRectangleAsync();
                await CreateContextMenuAsync();

                _isInitialized = true;
                _logger.LogInformation("✅ DataGridEventService initialized successfully - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during DataGridEventService initialization");
                throw;
            }
        }

        /// <summary>
        /// Registruje všetky event handlery
        /// </summary>
        private async Task RegisterEventHandlersAsync()
        {
            try
            {
                if (_dataGrid == null) return;

                await Task.Run(() =>
                {
                    // Mouse events
                    _dataGrid.PointerPressed += OnPointerPressed;
                    _dataGrid.PointerMoved += OnPointerMoved;
                    _dataGrid.PointerReleased += OnPointerReleased;
                    _dataGrid.PointerEntered += OnPointerEntered;
                    _dataGrid.PointerExited += OnPointerExited;
                    _dataGrid.RightTapped += OnRightTapped;

                    // Keyboard events
                    _dataGrid.KeyDown += OnKeyDown;
                    _dataGrid.KeyUp += OnKeyUp;
                    _dataGrid.PreviewKeyDown += OnPreviewKeyDown;

                    // Focus events
                    _dataGrid.GotFocus += OnGotFocus;
                    _dataGrid.LostFocus += OnLostFocus;

                    _logger.LogDebug("📝 Registered all event handlers");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error registering event handlers");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje selection rectangle
        /// </summary>
        private async Task InitializeSelectionRectangleAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _selectionRectangle = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.Blue),
                        BorderThickness = new Thickness(2),
                        Background = new SolidColorBrush(Colors.LightBlue) { Opacity = 0.3 },
                        Visibility = Visibility.Collapsed,
                        IsHitTestVisible = false
                    };

                    _logger.LogDebug("📐 Selection rectangle initialized");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing selection rectangle");
            }
        }

        /// <summary>
        /// Vytvorí context menu
        /// </summary>
        private async Task CreateContextMenuAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _contextMenu = new MenuFlyout();

                    // Copy
                    var copyItem = new MenuFlyoutItem { Text = "Copy" };
                    copyItem.Click += OnContextMenuCopyClick;
                    _contextMenu.Items.Add(copyItem);

                    // Paste
                    var pasteItem = new MenuFlyoutItem { Text = "Paste" };
                    pasteItem.Click += OnContextMenuPasteClick;
                    _contextMenu.Items.Add(pasteItem);

                    // Separator
                    _contextMenu.Items.Add(new MenuFlyoutSeparator());

                    // Select All
                    var selectAllItem = new MenuFlyoutItem { Text = "Select All" };
                    selectAllItem.Click += OnContextMenuSelectAllClick;
                    _contextMenu.Items.Add(selectAllItem);

                    _logger.LogDebug("📋 Context menu created");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating context menu");
            }
        }

        #endregion

        #region Mouse Event Handlers

        /// <summary>
        /// Handler pre pointer pressed
        /// </summary>
        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var point = e.GetCurrentPoint(_dataGrid);
                _mouseDownPosition = point.Position;
                _mouseDownTime = DateTime.Now;
                _isMouseDown = true;

                var cellPosition = await GetCellPositionFromPointAsync(point.Position);
                if (cellPosition != null)
                {
                    await HandleCellClickAsync(cellPosition, e);
                }

                _logger.LogTrace("🖱️ Pointer pressed at: {Position}", point.Position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer pressed");
            }
        }

        /// <summary>
        /// Handler pre pointer moved
        /// </summary>
        private async void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (!_isMouseDown) return;

                var point = e.GetCurrentPoint(_dataGrid);
                var distance = CalculateDistance(_mouseDownPosition, point.Position);

                if (distance > DragThreshold && !_isDragging)
                {
                    await StartDragOperationAsync(point.Position);
                }
                else if (_isDragging)
                {
                    await UpdateDragOperationAsync(point.Position);
                }
                else if (_isSelecting)
                {
                    await UpdateSelectionAsync(point.Position);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer moved");
            }
        }

        /// <summary>
        /// Handler pre pointer released
        /// </summary>
        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var clickDuration = DateTime.Now - _mouseDownTime;
                var point = e.GetCurrentPoint(_dataGrid);

                if (_isDragging)
                {
                    await EndDragOperationAsync(point.Position);
                }
                else if (_isSelecting)
                {
                    await EndSelectionAsync();
                }
                else if (clickDuration.TotalMilliseconds < DoubleClickThreshold)
                {
                    await HandlePotentialDoubleClickAsync(point.Position);
                }

                _isMouseDown = false;
                _logger.LogTrace("🖱️ Pointer released");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer released");
            }
        }

        /// <summary>
        /// Handler pre pointer entered
        /// </summary>
        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                _logger.LogTrace("🖱️ Pointer entered DataGrid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer entered");
            }
        }

        /// <summary>
        /// Handler pre pointer exited
        /// </summary>
        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                _logger.LogTrace("🖱️ Pointer exited DataGrid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer exited");
            }
        }

        /// <summary>
        /// Handler pre right tap (context menu)
        /// </summary>
        private async void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                var point = new Point(0, 0); // RightTappedRoutedEventArgs doesn't have GetCurrentPoint in WinUI3
                var cellPosition = await GetCellPositionFromPointAsync(point);

                if (cellPosition != null && _contextMenu != null)
                {
                    _contextMenuCell = cellPosition;
                    _contextMenu.ShowAt(_dataGrid, point);
                    
                    _logger.LogDebug("📋 Context menu shown at cell: {Cell}", cellPosition);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling right tap");
            }
        }

        #endregion

        #region Keyboard Event Handlers

        /// <summary>
        /// Handler pre key down
        /// </summary>
        private async void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                if ((now - _lastKeyPressTime).TotalMilliseconds < KeyRepeatDelay)
                {
                    e.Handled = true;
                    return;
                }
                _lastKeyPressTime = now;

                await HandleKeyboardNavigationAsync(e.Key, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling key down: {Key}", e.Key);
            }
        }

        /// <summary>
        /// Handler pre key up
        /// </summary>
        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                _isNavigating = false;
                _logger.LogTrace("⌨️ Key up: {Key}", e.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling key up: {Key}", e.Key);
            }
        }

        /// <summary>
        /// Handler pre preview key down
        /// </summary>
        private async void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                // Handle special keys that need preview handling
                switch (e.Key)
                {
                    case VirtualKey.Tab:
                        await HandleTabNavigationAsync(e);
                        break;
                    case VirtualKey.Enter:
                        await HandleEnterKeyAsync(e);
                        break;
                    case VirtualKey.Escape:
                        await HandleEscapeKeyAsync(e);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling preview key down: {Key}", e.Key);
            }
        }

        #endregion

        #region Focus Event Handlers

        /// <summary>
        /// Handler pre got focus
        /// </summary>
        private async void OnGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _hasFocus = true;
                await UpdateFocusVisualsAsync(true);
                _logger.LogDebug("🎯 DataGrid got focus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling got focus");
            }
        }

        /// <summary>
        /// Handler pre lost focus
        /// </summary>
        private async void OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _hasFocus = false;
                await UpdateFocusVisualsAsync(false);
                _logger.LogDebug("🎯 DataGrid lost focus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling lost focus");
            }
        }

        #endregion

        #region Cell Interaction Methods

        /// <summary>
        /// Spracuje klik na bunku
        /// </summary>
        private async Task HandleCellClickAsync(CellPosition cellPosition, PointerRoutedEventArgs e)
        {
            try
            {
                var point = e.GetCurrentPoint(_dataGrid);
                var modifiers = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
                var isCtrlPressed = modifiers.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
                modifiers = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                var isShiftPressed = modifiers.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                if (isShiftPressed && _anchorCell != null)
                {
                    await SelectRangeAsync(_anchorCell, cellPosition);
                }
                else if (isCtrlPressed)
                {
                    await ToggleCellSelectionAsync(cellPosition);
                }
                else
                {
                    await SelectSingleCellAsync(cellPosition);
                }

                // Fire event
                CellClicked?.Invoke(this, new CellClickEventArgs(cellPosition));

                _logger.LogDebug("🖱️ Cell clicked: {Cell}", cellPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling cell click: {Cell}", cellPosition);
            }
        }

        /// <summary>
        /// Spracuje potenciálny double click
        /// </summary>
        private async Task HandlePotentialDoubleClickAsync(Point position)
        {
            try
            {
                var cellPosition = await GetCellPositionFromPointAsync(position);
                if (cellPosition != null)
                {
                    // Fire double click event
                    CellDoubleClicked?.Invoke(this, new CellClickEventArgs(cellPosition));
                    
                    // Start editing if applicable
                    EditStartRequested?.Invoke(this, new CellEditEventArgs(cellPosition));

                    _logger.LogDebug("🖱️ Cell double clicked: {Cell}", cellPosition);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling potential double click");
            }
        }

        #endregion

        #region Selection Management

        /// <summary>
        /// Vyberie jednu bunku
        /// </summary>
        private async Task SelectSingleCellAsync(CellPosition cellPosition)
        {
            try
            {
                var previousSelection = _selectedCells.ToList();
                
                _selectedCells.Clear();
                _selectedCells.Add(cellPosition);
                _currentCell = cellPosition;
                _anchorCell = cellPosition;

                await UpdateSelectionVisualsAsync();
                FireSelectionChangedEvent(previousSelection, _selectedCells.ToList());

                _logger.LogDebug("📍 Selected single cell: {Cell}", cellPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error selecting single cell: {Cell}", cellPosition);
            }
        }

        /// <summary>
        /// Toggle selection bunky
        /// </summary>
        private async Task ToggleCellSelectionAsync(CellPosition cellPosition)
        {
            try
            {
                var previousSelection = _selectedCells.ToList();

                if (_selectedCells.Contains(cellPosition))
                {
                    _selectedCells.Remove(cellPosition);
                }
                else
                {
                    _selectedCells.Add(cellPosition);
                }

                _currentCell = cellPosition;
                await UpdateSelectionVisualsAsync();
                FireSelectionChangedEvent(previousSelection, _selectedCells.ToList());

                _logger.LogDebug("🔀 Toggled cell selection: {Cell}", cellPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error toggling cell selection: {Cell}", cellPosition);
            }
        }

        /// <summary>
        /// Vyberie rozsah buniek
        /// </summary>
        private async Task SelectRangeAsync(CellPosition startCell, CellPosition endCell)
        {
            try
            {
                var previousSelection = _selectedCells.ToList();
                _selectedCells.Clear();

                var minRow = Math.Min(startCell.Row, endCell.Row);
                var maxRow = Math.Max(startCell.Row, endCell.Row);
                var minCol = Math.Min(startCell.Column, endCell.Column);
                var maxCol = Math.Max(startCell.Column, endCell.Column);

                for (int row = minRow; row <= maxRow; row++)
                {
                    for (int col = minCol; col <= maxCol; col++)
                    {
                        _selectedCells.Add(new CellPosition(row, col));
                    }
                }

                _currentCell = endCell;
                await UpdateSelectionVisualsAsync();
                FireSelectionChangedEvent(previousSelection, _selectedCells.ToList());

                _logger.LogDebug("📐 Selected range: {StartCell} to {EndCell} ({Count} cells)", 
                    startCell, endCell, _selectedCells.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error selecting range: {StartCell} to {EndCell}", startCell, endCell);
            }
        }

        /// <summary>
        /// Aktualizuje vizuály selection
        /// </summary>
        private async Task UpdateSelectionVisualsAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Implementation for updating selection visuals
                    // This would update the visual state of selected cells
                    _logger.LogTrace("🎨 Updated selection visuals for {Count} cells", _selectedCells.Count);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating selection visuals");
            }
        }

        #endregion

        #region Keyboard Navigation

        /// <summary>
        /// Spracuje keyboard navigation
        /// </summary>
        private async Task HandleKeyboardNavigationAsync(VirtualKey key, KeyRoutedEventArgs e)
        {
            try
            {
                if (_currentCell == null) return;

                CellPosition? newCell = null;

                switch (key)
                {
                    case VirtualKey.Up:
                        newCell = new CellPosition(_currentCell.Row - 1, _currentCell.Column);
                        break;
                    case VirtualKey.Down:
                        newCell = new CellPosition(_currentCell.Row + 1, _currentCell.Column);
                        break;
                    case VirtualKey.Left:
                        newCell = new CellPosition(_currentCell.Row, _currentCell.Column - 1);
                        break;
                    case VirtualKey.Right:
                        newCell = new CellPosition(_currentCell.Row, _currentCell.Column + 1);
                        break;
                    case VirtualKey.Home:
                        newCell = new CellPosition(_currentCell.Row, 0);
                        break;
                    case VirtualKey.End:
                        newCell = new CellPosition(_currentCell.Row, GetLastColumnIndex());
                        break;
                    case VirtualKey.PageUp:
                        newCell = new CellPosition(Math.Max(0, _currentCell.Row - 10), _currentCell.Column);
                        break;
                    case VirtualKey.PageDown:
                        newCell = new CellPosition(_currentCell.Row + 10, _currentCell.Column);
                        break;
                }

                if (newCell != null && await IsValidCellPositionAsync(newCell))
                {
                    await NavigateToCellAsync(newCell, e);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling keyboard navigation: {Key}", key);
            }
        }

        /// <summary>
        /// Naviguje na bunku
        /// </summary>
        private async Task NavigateToCellAsync(CellPosition cellPosition, KeyRoutedEventArgs e)
        {
            try
            {
                var isShiftPressed = false; // e.KeyStatus.IsShiftPressed not available in WinUI3

                if (isShiftPressed && _anchorCell != null)
                {
                    await SelectRangeAsync(_anchorCell, cellPosition);
                }
                else
                {
                    await SelectSingleCellAsync(cellPosition);
                }

                // Fire navigation event
                NavigationRequested?.Invoke(this, new NavigationEventArgs(cellPosition));

                _logger.LogDebug("🧭 Navigated to cell: {Cell}", cellPosition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error navigating to cell: {Cell}", cellPosition);
            }
        }

        /// <summary>
        /// Spracuje Tab navigáciu
        /// </summary>
        private async Task HandleTabNavigationAsync(KeyRoutedEventArgs e)
        {
            try
            {
                if (_currentCell == null) return;

                var isShiftPressed = false; // e.KeyStatus.IsShiftPressed not available in WinUI3
                var newColumn = isShiftPressed ? _currentCell.Column - 1 : _currentCell.Column + 1;

                CellPosition? newCell = null;
                if (newColumn < 0)
                {
                    // Previous row, last column
                    newCell = new CellPosition(_currentCell.Row - 1, GetLastColumnIndex());
                }
                else if (newColumn > GetLastColumnIndex())
                {
                    // Next row, first column
                    newCell = new CellPosition(_currentCell.Row + 1, 0);
                }
                else
                {
                    newCell = new CellPosition(_currentCell.Row, newColumn);
                }

                if (newCell != null && await IsValidCellPositionAsync(newCell))
                {
                    await SelectSingleCellAsync(newCell);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling Tab navigation");
            }
        }

        /// <summary>
        /// Spracuje Enter key
        /// </summary>
        private async Task HandleEnterKeyAsync(KeyRoutedEventArgs e)
        {
            try
            {
                if (_currentCell != null)
                {
                    EditStartRequested?.Invoke(this, new CellEditEventArgs(_currentCell));
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling Enter key");
            }
        }

        /// <summary>
        /// Spracuje Escape key
        /// </summary>
        private async Task HandleEscapeKeyAsync(KeyRoutedEventArgs e)
        {
            try
            {
                await ClearSelectionAsync();
                e.Handled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling Escape key");
            }
        }

        #endregion

        #region Drag & Drop

        /// <summary>
        /// Začne drag operáciu
        /// </summary>
        private async Task StartDragOperationAsync(Point position)
        {
            try
            {
                _isDragging = true;
                _dragStartPosition = position;
                _dragStartCell = await GetCellPositionFromPointAsync(position);

                _logger.LogDebug("📦 Started drag operation from: {Cell}", _dragStartCell);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error starting drag operation");
            }
        }

        /// <summary>
        /// Aktualizuje drag operáciu
        /// </summary>
        private async Task UpdateDragOperationAsync(Point position)
        {
            try
            {
                var currentCell = await GetCellPositionFromPointAsync(position);
                // Implementation for updating drag visuals
                _logger.LogTrace("📦 Updating drag operation to: {Cell}", currentCell);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating drag operation");
            }
        }

        /// <summary>
        /// Ukončí drag operáciu
        /// </summary>
        private async Task EndDragOperationAsync(Point position)
        {
            try
            {
                var endCell = await GetCellPositionFromPointAsync(position);
                
                // Implementation for handling drag completion
                
                _isDragging = false;
                _dragStartCell = null;

                _logger.LogDebug("📦 Ended drag operation at: {Cell}", endCell);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ending drag operation");
            }
        }

        #endregion

        #region Context Menu Handlers

        /// <summary>
        /// Handler pre context menu Copy
        /// </summary>
        private async void OnContextMenuCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedCells.Any())
                {
                    // Implementation for copy operation
                    _logger.LogDebug("📋 Copy operation requested for {Count} cells", _selectedCells.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling context menu copy");
            }
        }

        /// <summary>
        /// Handler pre context menu Paste
        /// </summary>
        private async void OnContextMenuPasteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_contextMenuCell != null)
                {
                    // Implementation for paste operation
                    _logger.LogDebug("📋 Paste operation requested at: {Cell}", _contextMenuCell);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling context menu paste");
            }
        }

        /// <summary>
        /// Handler pre context menu Select All
        /// </summary>
        private async void OnContextMenuSelectAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await SelectAllCellsAsync();
                _logger.LogDebug("📋 Select All operation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling context menu select all");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Získa pozíciu bunky z Point
        /// </summary>
        private async Task<CellPosition?> GetCellPositionFromPointAsync(Point point)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // Implementation for getting cell position from point
                    // This would analyze the visual tree and calculate row/column
                    return new CellPosition(0, 0); // Placeholder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting cell position from point");
                return null;
            }
        }

        /// <summary>
        /// Skontroluje či je pozícia bunky validná
        /// </summary>
        private async Task<bool> IsValidCellPositionAsync(CellPosition position)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // Implementation for validating cell position
                    return position.Row >= 0 && position.Column >= 0; // Placeholder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating cell position: {Position}", position);
                return false;
            }
        }

        /// <summary>
        /// Vypočíta vzdialenosť medzi dvoma bodmi
        /// </summary>
        private double CalculateDistance(Point point1, Point point2)
        {
            var deltaX = point2.X - point1.X;
            var deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// Získa index posledného stĺpca
        /// </summary>
        private int GetLastColumnIndex()
        {
            // Implementation for getting last column index
            return 10; // Placeholder
        }

        /// <summary>
        /// Aktualizuje focus vizuály
        /// </summary>
        private async Task UpdateFocusVisualsAsync(bool hasFocus)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Implementation for updating focus visuals
                    _logger.LogTrace("🎯 Updated focus visuals: {HasFocus}", hasFocus);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating focus visuals");
            }
        }

        /// <summary>
        /// Vymaže selection
        /// </summary>
        private async Task ClearSelectionAsync()
        {
            try
            {
                var previousSelection = _selectedCells.ToList();
                _selectedCells.Clear();
                _currentCell = null;
                _anchorCell = null;

                await UpdateSelectionVisualsAsync();
                FireSelectionChangedEvent(previousSelection, new List<CellPosition>());

                _logger.LogDebug("🧹 Cleared selection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing selection");
            }
        }

        /// <summary>
        /// Vyberie všetky bunky
        /// </summary>
        private async Task SelectAllCellsAsync()
        {
            try
            {
                var previousSelection = _selectedCells.ToList();
                _selectedCells.Clear();

                // Implementation for selecting all cells
                // This would add all valid cell positions to _selectedCells

                await UpdateSelectionVisualsAsync();
                FireSelectionChangedEvent(previousSelection, _selectedCells.ToList());

                _logger.LogDebug("📋 Selected all cells");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error selecting all cells");
            }
        }

        /// <summary>
        /// Aktualizuje selection po mouse move
        /// </summary>
        private async Task UpdateSelectionAsync(Point position)
        {
            try
            {
                var currentCell = await GetCellPositionFromPointAsync(position);
                if (currentCell != null && _anchorCell != null)
                {
                    await SelectRangeAsync(_anchorCell, currentCell);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating selection");
            }
        }

        /// <summary>
        /// Ukončí selection operáciu
        /// </summary>
        private async Task EndSelectionAsync()
        {
            try
            {
                _isSelecting = false;
                await Task.CompletedTask;
                _logger.LogDebug("🏁 Ended selection operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ending selection");
            }
        }

        /// <summary>
        /// Vyvolá SelectionChanged event
        /// </summary>
        private void FireSelectionChangedEvent(List<CellPosition> oldSelection, List<CellPosition> newSelection)
        {
            try
            {
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldSelection, newSelection));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error firing selection changed event");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Aktuálne vybrané bunky
        /// </summary>
        public IReadOnlyList<CellPosition> SelectedCells => _selectedCells.AsReadOnly();

        /// <summary>
        /// Aktuálna bunka
        /// </summary>
        public CellPosition? CurrentCell => _currentCell;

        /// <summary>
        /// Či má DataGrid focus
        /// </summary>
        public bool HasFocus => _hasFocus;

        /// <summary>
        /// Či je inicializovaný
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                _logger.LogInformation("🧹 Disposing DataGridEventService - InstanceId: {InstanceId}", _serviceInstanceId);

                // Cleanup event handlers
                if (_dataGrid != null)
                {
                    _dataGrid.PointerPressed -= OnPointerPressed;
                    _dataGrid.PointerMoved -= OnPointerMoved;
                    _dataGrid.PointerReleased -= OnPointerReleased;
                    _dataGrid.PointerEntered -= OnPointerEntered;
                    _dataGrid.PointerExited -= OnPointerExited;
                    _dataGrid.RightTapped -= OnRightTapped;
                    _dataGrid.KeyDown -= OnKeyDown;
                    _dataGrid.KeyUp -= OnKeyUp;
                    _dataGrid.PreviewKeyDown -= OnPreviewKeyDown;
                    _dataGrid.GotFocus -= OnGotFocus;
                    _dataGrid.LostFocus -= OnLostFocus;
                }

                // Clear collections
                _selectedCells.Clear();

                // Clear references
                _dataGrid = null;
                _currentCell = null;
                _anchorCell = null;
                _dragStartCell = null;
                _contextMenuCell = null;
                _focusedCell = null;
                _selectionRectangle = null;
                _contextMenu = null;

                _logger.LogInformation("✅ DataGridEventService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing DataGridEventService");
            }
        }

        #endregion
    }

    #region Event Args Classes

    /// <summary>
    /// Event args pre zmenu selection
    /// </summary>
    internal class SelectionChangedEventArgs : EventArgs
    {
        public IReadOnlyList<CellPosition> OldSelection { get; }
        public IReadOnlyList<CellPosition> NewSelection { get; }

        public SelectionChangedEventArgs(IReadOnlyList<CellPosition> oldSelection, IReadOnlyList<CellPosition> newSelection)
        {
            OldSelection = oldSelection;
            NewSelection = newSelection;
        }
    }

    /// <summary>
    /// Event args pre cell click
    /// </summary>
    internal class CellClickEventArgs : EventArgs
    {
        public CellPosition CellPosition { get; }

        public CellClickEventArgs(CellPosition cellPosition)
        {
            CellPosition = cellPosition;
        }
    }

    /// <summary>
    /// Event args pre navigation
    /// </summary>
    internal class NavigationEventArgs : EventArgs
    {
        public CellPosition TargetPosition { get; }

        public NavigationEventArgs(CellPosition targetPosition)
        {
            TargetPosition = targetPosition;
        }
    }

    /// <summary>
    /// Event args pre cell edit
    /// </summary>
    internal class CellEditEventArgs : EventArgs
    {
        public CellPosition CellPosition { get; }

        public CellEditEventArgs(CellPosition cellPosition)
        {
            CellPosition = cellPosition;
        }
    }

    #endregion
}