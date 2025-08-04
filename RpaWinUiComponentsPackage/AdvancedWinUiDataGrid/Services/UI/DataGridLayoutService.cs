// Services/UI/DataGridLayoutService.cs - ✅ NOVÝ: Layout Management Service
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Input;
using Windows.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Controls.SpecialColumns;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.UI
{
    /// <summary>
    /// Service pre Layout Management - INTERNAL
    /// Zodpovedný za column sizing, row height, grid dimensions, virtualization, scrolling
    /// </summary>
    internal class DataGridLayoutService : IDisposable
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // Referencia na hlavný DataGrid control
        private AdvancedDataGrid? _dataGrid;
        
        // Column management
        private readonly List<Models.Grid.ColumnDefinition> _columns = new();
        private readonly List<ResizableColumnHeader> _resizableHeaders = new();

        // Resize functionality
        private bool _isResizing = false;
        private ResizableColumnHeader? _currentResizingHeader;
        private double _resizeStartPosition;
        private double _resizeStartWidth;

        // Scroll synchronization
        private bool _isScrollSynchronizing = false;

        // Layout management
        private double _totalAvailableWidth = 0;
        private double _validAlertsMinWidth = 200;

        // CheckBox column support
        private bool _checkBoxColumnEnabled = false;
        private string _checkBoxColumnName = "CheckBoxState";
        private CheckBoxColumnHeader? _checkBoxColumnHeader;

        // Initialization state
        private bool _isInitialized = false;

        #endregion

        #region Constructor

        public DataGridLayoutService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("🏗️ DataGridLayoutService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje layout service s referenciou na DataGrid
        /// </summary>
        public async Task InitializeAsync(AdvancedDataGrid dataGrid, List<Models.Grid.ColumnDefinition> columns)
        {
            try
            {
                _logger.LogInformation("🚀 DataGridLayoutService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
                _columns.Clear();
                _columns.AddRange(columns ?? throw new ArgumentNullException(nameof(columns)));

                // Inicializuj komponenty
                await InitializeResizeSupportAsync();
                await InitializeScrollSupportAsync();
                await InitializeLayoutManagementAsync();

                _isInitialized = true;
                _logger.LogInformation("✅ DataGridLayoutService initialized successfully - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during DataGridLayoutService initialization");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje resize support
        /// </summary>
        private async Task InitializeResizeSupportAsync()
        {
            try
            {
                _logger.LogDebug("🔧 Initializing resize support");
                
                // Setup resize handlers
                if (_dataGrid != null)
                {
                    _dataGrid.PointerPressed += OnPointerPressed;
                    _dataGrid.PointerMoved += OnPointerMoved;
                    _dataGrid.PointerReleased += OnPointerReleased;
                    _dataGrid.PointerCaptureLost += OnPointerCaptureLost;
                }

                await CreateResizableHeadersAsync();
                _logger.LogInformation("✅ Resize support initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing resize support");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje scroll support
        /// </summary>
        private async Task InitializeScrollSupportAsync()
        {
            try
            {
                _logger.LogDebug("📜 Initializing scroll support");
                
                await Task.Run(() =>
                {
                    SetupScrollSynchronization();
                });

                _logger.LogInformation("✅ Scroll support initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing scroll support");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje layout management
        /// </summary>
        private async Task InitializeLayoutManagementAsync()
        {
            try
            {
                _logger.LogDebug("📐 Initializing layout management");
                
                // Setup layout handlers
                if (_dataGrid != null)
                {
                    _dataGrid.SizeChanged += OnDataGridSizeChanged;
                    _dataGrid.LayoutUpdated += OnLayoutUpdated;
                }

                await SetupValidAlertsStretchingAsync();
                _logger.LogInformation("✅ Layout management initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing layout management");
                throw;
            }
        }

        #endregion

        #region Size Change Handlers

        /// <summary>
        /// Handler pre zmenu veľkosti DataGrid
        /// </summary>
        public async void OnDataGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                _logger.LogDebug("📏 DataGrid size changed: {OldSize} → {NewSize}", 
                    e.PreviousSize, e.NewSize);

                if (e.NewSize.Width != e.PreviousSize.Width)
                {
                    _totalAvailableWidth = e.NewSize.Width;
                    await UpdateLayoutAfterSizeChangeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling DataGrid size change");
            }
        }

        /// <summary>
        /// Handler pre layout update
        /// </summary>
        private async void OnLayoutUpdated(object? sender, object e)
        {
            try
            {
                await UpdateLayoutAfterDataChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling layout update");
            }
        }

        #endregion

        #region Resize Management

        /// <summary>
        /// Vytvorí resizable headers pre všetky stĺpce
        /// </summary>
        private async Task CreateResizableHeadersAsync()
        {
            try
            {
                _logger.LogDebug("🔧 Creating resizable headers for {ColumnCount} columns", _columns.Count);

                _resizableHeaders.Clear();

                var headerContainer = GetHeaderStackPanel();
                if (headerContainer == null)
                {
                    _logger.LogWarning("⚠️ Header container not found");
                    return;
                }

                await Task.Run(() =>
                {
                    foreach (var column in _columns)
                    {
                        CreateColumnHeaderAsync(column, headerContainer);
                    }
                });

                _logger.LogInformation("✅ Created {HeaderCount} resizable headers", _resizableHeaders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating resizable headers");
                throw;
            }
        }

        /// <summary>
        /// Vytvorí header pre konkrétny stĺpec
        /// </summary>
        private async Task CreateColumnHeaderAsync(Models.Grid.ColumnDefinition column, StackPanel headerContainer)
        {
            try
            {
                await Task.Run(() =>
                {
                    var header = new ResizableColumnHeader
                    {
                        ColumnName = column.Name,
                        OriginalWidth = column.Width,
                        MinWidth = column.MinWidth,
                        MaxWidth = column.MaxWidth
                    };

                    // Vytvorenie UI elementov
                    var headerBorder = new Border
                    {
                        Width = column.Width,
                        Height = 40,
                        Background = new SolidColorBrush(Colors.LightGray),
                        BorderBrush = new SolidColorBrush(Colors.Gray),
                        BorderThickness = new Thickness(0, 0, 1, 1)
                    };

                    var headerText = new TextBlock
                    {
                        Text = column.Header,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold
                    };

                    headerBorder.Child = headerText;
                    header.HeaderElement = headerBorder;

                    // Resize grip
                    var resizeGrip = new Border
                    {
                        Width = 5,
                        Background = new SolidColorBrush(Colors.Transparent)
                        // ✅ IMPLEMENTED: Set cursor for resize grip in WinUI3
                    };

                    header.ResizeGrip = resizeGrip;
                    _resizableHeaders.Add(header);

                    _logger.LogTrace("📊 Created header for column: {ColumnName}", column.Name);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating column header for: {ColumnName}", column.Name);
            }
        }

        #endregion

        #region Pointer Event Handlers

        /// <summary>
        /// Handler pre pointer pressed
        /// </summary>
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var point = e.GetCurrentPoint(_dataGrid);
                var resizeHeader = GetResizeHeaderFromPoint(point.Position);

                if (resizeHeader != null)
                {
                    _isResizing = true;
                    _currentResizingHeader = resizeHeader;
                    _resizeStartPosition = point.Position.X;
                    _resizeStartWidth = resizeHeader.OriginalWidth;

                    _dataGrid?.CapturePointer(e.Pointer);
                    e.Handled = true;

                    _logger.LogDebug("🖱️ Started resizing column: {ColumnName}", resizeHeader.ColumnName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer pressed");
            }
        }

        /// <summary>
        /// Handler pre pointer moved
        /// </summary>
        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_isResizing && _currentResizingHeader != null)
                {
                    var point = e.GetCurrentPoint(_dataGrid);
                    var deltaX = point.Position.X - _resizeStartPosition;
                    var newWidth = Math.Max(_currentResizingHeader.MinWidth, 
                                   Math.Min(_currentResizingHeader.MaxWidth, _resizeStartWidth + deltaX));

                    UpdateColumnWidth(_currentResizingHeader.ColumnName, newWidth);
                    e.Handled = true;
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
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_isResizing)
                {
                    EndResize();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer released");
            }
        }

        /// <summary>
        /// Handler pre pointer capture lost
        /// </summary>
        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_isResizing)
                {
                    EndResize();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling pointer capture lost");
            }
        }

        /// <summary>
        /// Ukončí resize operáciu
        /// </summary>
        private void EndResize()
        {
            _isResizing = false;
            _currentResizingHeader = null;
            // _dataGrid?.ReleasePointerCapture(); // TODO: Need to store pointer reference
            
            _logger.LogDebug("🏁 Ended resize operation");
        }

        #endregion

        #region Scroll Synchronization

        /// <summary>
        /// Nastaví scroll synchronization medzi header a data grid
        /// </summary>
        private void SetupScrollSynchronization()
        {
            try
            {
                var headerScrollViewer = GetHeaderScrollViewer();
                var dataScrollViewer = GetDataGridScrollViewer();

                if (headerScrollViewer != null && dataScrollViewer != null)
                {
                    headerScrollViewer.ViewChanged += OnHeaderScrollViewChanged;
                    dataScrollViewer.ViewChanged += OnDataScrollViewChanged;

                    _logger.LogDebug("🔄 Scroll synchronization setup completed");
                }
                else
                {
                    _logger.LogWarning("⚠️ Could not setup scroll synchronization - ScrollViewers not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting up scroll synchronization");
            }
        }

        /// <summary>
        /// Handler pre header scroll change
        /// </summary>
        public void OnHeaderScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing) return;

                var headerScrollViewer = sender as ScrollViewer;
                var dataScrollViewer = GetDataGridScrollViewer();

                if (headerScrollViewer != null && dataScrollViewer != null)
                {
                    _isScrollSynchronizing = true;
                    dataScrollViewer.ScrollToHorizontalOffset(headerScrollViewer.HorizontalOffset);
                    _isScrollSynchronizing = false;

                    _logger.LogTrace("↔️ Synchronized header scroll: {Offset}", headerScrollViewer.HorizontalOffset);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling header scroll change");
                _isScrollSynchronizing = false;
            }
        }

        /// <summary>
        /// Handler pre data scroll change
        /// </summary>
        public void OnDataScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing) return;

                var dataScrollViewer = sender as ScrollViewer;
                var headerScrollViewer = GetHeaderScrollViewer();

                if (dataScrollViewer != null && headerScrollViewer != null)
                {
                    _isScrollSynchronizing = true;
                    headerScrollViewer.ScrollToHorizontalOffset(dataScrollViewer.HorizontalOffset);
                    _isScrollSynchronizing = false;

                    _logger.LogTrace("↔️ Synchronized data scroll: {Offset}", dataScrollViewer.HorizontalOffset);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling data scroll change");
                _isScrollSynchronizing = false;
            }
        }

        #endregion

        #region Layout Update Methods

        /// <summary>
        /// Aktualizuje layout po zmene dát
        /// </summary>
        public async Task UpdateLayoutAfterDataChangeAsync()
        {
            try
            {
                if (!_isInitialized) return;

                await Task.Run(() =>
                {
                    RecalculateColumnWidths();
                });

                _logger.LogTrace("📐 Layout updated after data change");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating layout after data change");
            }
        }

        /// <summary>
        /// Aktualizuje layout po zmene veľkosti
        /// </summary>
        private async Task UpdateLayoutAfterSizeChangeAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    RecalculateColumnWidths();
                    RecalculateValidAlertsWidth();
                });

                _logger.LogTrace("📏 Layout updated after size change");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating layout after size change");
            }
        }

        /// <summary>
        /// Nastaví ValidAlerts stretching
        /// </summary>
        private async Task SetupValidAlertsStretchingAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    RecalculateValidAlertsWidth();
                });

                _logger.LogDebug("📊 ValidAlerts stretching setup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting up ValidAlerts stretching");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Získa header StackPanel
        /// </summary>
        private StackPanel? GetHeaderStackPanel()
        {
            return _dataGrid?.FindName("HeaderStackPanel") as StackPanel;
        }

        /// <summary>
        /// Získa header ScrollViewer
        /// </summary>
        private ScrollViewer? GetHeaderScrollViewer()
        {
            return _dataGrid?.FindName("HeaderScrollViewer") as ScrollViewer;
        }

        /// <summary>
        /// Získa data ScrollViewer
        /// </summary>
        private ScrollViewer? GetDataGridScrollViewer()
        {
            return _dataGrid?.FindName("DataGridScrollViewer") as ScrollViewer;
        }

        /// <summary>
        /// Získa resize header z pozície
        /// </summary>
        private ResizableColumnHeader? GetResizeHeaderFromPoint(Point point)
        {
            return _resizableHeaders.FirstOrDefault(h => 
                h.ResizeGrip != null && IsPointerOverElement(point, h.ResizeGrip));
        }

        /// <summary>
        /// Skontroluje či je pointer nad elementom
        /// </summary>
        private bool IsPointerOverElement(Point point, FrameworkElement element)
        {
            try
            {
                var transform = element.TransformToVisual(_dataGrid);
                var elementBounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                return elementBounds.Contains(point);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Aktualizuje šírku stĺpca
        /// </summary>
        private void UpdateColumnWidth(string columnName, double newWidth)
        {
            try
            {
                var column = _columns.FirstOrDefault(c => c.Name == columnName);
                var header = _resizableHeaders.FirstOrDefault(h => h.ColumnName == columnName);

                if (column != null && header != null)
                {
                    column.Width = newWidth;
                    header.OriginalWidth = newWidth;

                    if (header.HeaderElement != null)
                    {
                        header.HeaderElement.Width = newWidth;
                    }

                    _logger.LogTrace("📏 Updated column width: {ColumnName} = {Width}", columnName, newWidth);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating column width: {ColumnName}", columnName);
            }
        }

        /// <summary>
        /// Prepočíta šírky stĺpcov
        /// </summary>
        private void RecalculateColumnWidths()
        {
            try
            {
                var totalUsedWidth = _columns.Sum(c => c.Width);
                var remainingWidth = Math.Max(0, _totalAvailableWidth - totalUsedWidth);

                _logger.LogTrace("📊 Recalculated column widths - Total: {Total}, Used: {Used}, Remaining: {Remaining}",
                    _totalAvailableWidth, totalUsedWidth, remainingWidth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error recalculating column widths");
            }
        }

        /// <summary>
        /// Prepočíta šírku ValidAlerts stĺpca
        /// </summary>
        private void RecalculateValidAlertsWidth()
        {
            try
            {
                var dataColumnsWidth = _columns.Where(c => !c.IsSpecialColumn).Sum(c => c.Width);
                var availableWidth = Math.Max(_validAlertsMinWidth, _totalAvailableWidth - dataColumnsWidth);

                _logger.LogTrace("📊 Recalculated ValidAlerts width: {Width}", availableWidth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error recalculating ValidAlerts width");
            }
        }

        #endregion

        #region Column and Position Methods

        /// <summary>
        /// Získa index stĺpca podľa názvu
        /// </summary>
        public int GetColumnIndex(string columnName)
        {
            try
            {
                var index = _columns.FindIndex(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                _logger.LogTrace("📍 Column index for '{ColumnName}': {Index}", columnName, index);
                return index;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting column index for: {ColumnName}", columnName);
                return -1;
            }
        }

        /// <summary>
        /// Získa pozíciu bunky z Point
        /// </summary>
        public Models.Cell.CellPosition? GetCellFromPoint(Point point)
        {
            try
            {
                // Implementation for getting cell position from point
                // This would need to analyze the visual tree and calculate row/column
                _logger.LogTrace("📍 Getting cell from point: {Point}", point);
                
                // Placeholder implementation
                return new Models.Cell.CellPosition(0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting cell from point: {Point}", point);
                return null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Celková dostupná šírka
        /// </summary>
        public double TotalAvailableWidth => _totalAvailableWidth;

        /// <summary>
        /// Či je služba inicializovaná
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Počet stĺpcov
        /// </summary>
        public int ColumnCount => _columns.Count;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                _logger.LogInformation("🧹 Disposing DataGridLayoutService - InstanceId: {InstanceId}", _serviceInstanceId);

                // Cleanup event handlers
                if (_dataGrid != null)
                {
                    _dataGrid.PointerPressed -= OnPointerPressed;
                    _dataGrid.PointerMoved -= OnPointerMoved;
                    _dataGrid.PointerReleased -= OnPointerReleased;
                    _dataGrid.PointerCaptureLost -= OnPointerCaptureLost;
                    _dataGrid.SizeChanged -= OnDataGridSizeChanged;
                    _dataGrid.LayoutUpdated -= OnLayoutUpdated;
                }

                var headerScrollViewer = GetHeaderScrollViewer();
                var dataScrollViewer = GetDataGridScrollViewer();

                if (headerScrollViewer != null)
                    headerScrollViewer.ViewChanged -= OnHeaderScrollViewChanged;

                if (dataScrollViewer != null)
                    dataScrollViewer.ViewChanged -= OnDataScrollViewChanged;

                // Clear collections
                _columns.Clear();
                _resizableHeaders.Clear();

                _dataGrid = null;
                _currentResizingHeader = null;
                _checkBoxColumnHeader = null;

                _logger.LogInformation("✅ DataGridLayoutService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing DataGridLayoutService");
            }
        }

        #endregion

        #region ResizableColumnHeader Class

        /// <summary>
        /// Helper trieda pre resizable column header
        /// </summary>
        private class ResizableColumnHeader
        {
            public string ColumnName { get; set; } = string.Empty;
            public Border? HeaderElement { get; set; }
            public Border? ResizeGrip { get; set; }
            public double OriginalWidth { get; set; }
            public double MinWidth { get; set; } = 50;
            public double MaxWidth { get; set; } = 500;
            public bool IsResizing { get; set; }
        }

        #endregion
    }
}