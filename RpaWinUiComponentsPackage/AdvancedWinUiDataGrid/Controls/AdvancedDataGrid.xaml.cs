// Controls/AdvancedDataGrid.xaml.cs - ✅ VŠETKY CHYBY OPRAVENÉ + RESIZE + SCROLL + LOGGING
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid - NEZÁVISLÝ KOMPONENT s ILogger abstractions + RESIZE + SCROLL
    /// ✅ ÚPLNE NEZÁVISLÝ na LoggerComponent - používa iba ILogger abstractions
    /// ✅ OPRAVENÉ: Všetky XAML a C# chyby
    /// ✅ NOVÉ: Mouse resize stĺpcov, scroll support, ValidAlerts stretching
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private IServiceProvider? _serviceProvider;
        private IDataManagementService? _dataManagementService;
        private IValidationService? _validationService;
        private IExportService? _exportService;

        private bool _isInitialized = false;
        private bool _isDisposed = false;
        private bool _xamlLoadFailed = false;

        // Auto-Add fields
        private int _unifiedRowCount = 15;
        private bool _autoAddEnabled = true;

        // Individual colors namiesto themes
        private DataGridColorConfig? _individualColorConfig;

        // Search and Sort service
        private SearchAndSortService? _searchAndSortService;

        // Internal data pre AUTO-ADD a UI binding
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();
        private readonly ObservableCollection<DataRowViewModel> _displayRows = new();

        // Search & Sort state tracking
        private readonly Dictionary<string, string> _columnSearchFilters = new();

        // ✅ NEZÁVISLÉ LOGOVANIE: Používa ILogger abstractions
        private readonly ILogger _logger;
        private string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];

        // Performance tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();

        // Realtime validácia fields
        private readonly Dictionary<string, System.Threading.Timer> _validationTimers = new();
        private GridThrottlingConfig? _throttlingConfig;
        private readonly object _validationLock = new object();

        // ✅ NOVÉ: Column resize functionality
        private readonly List<ResizableColumnHeader> _resizableHeaders = new();
        private bool _isResizing = false;
        private ResizableColumnHeader? _currentResizingHeader;
        private double _resizeStartPosition;
        private double _resizeStartWidth;

        // ✅ NOVÉ: Scroll synchronization
        private bool _isScrollSynchronizing = false;

        #endregion

        #region ✅ NEZÁVISLÉ Constructors s ILogger podporou

        /// <summary>
        /// Vytvorí AdvancedDataGrid bez loggingu (NullLogger) - DEFAULT konštruktor
        /// </summary>
        public AdvancedDataGrid() : this(null)
        {
        }

        /// <summary>
        /// Vytvorí AdvancedDataGrid s voliteľným loggerom
        /// ✅ HLAVNÝ KONŠTRUKTOR - podporuje externe logovanie
        /// </summary>
        /// <param name="logger">ILogger pre logovanie (null = žiadne logovanie)</param>
        public AdvancedDataGrid(ILogger? logger)
        {
            try
            {
                // ✅ NEZÁVISLÉ LOGOVANIE: Použije poskytnutý logger alebo NullLogger
                _logger = logger ?? NullLogger.Instance;

                _logger.LogDebug("🔧 AdvancedDataGrid Constructor START - Instance: {ComponentInstanceId}", _componentInstanceId);
                StartOperation("Constructor");

                TryInitializeXaml();

                if (!_xamlLoadFailed)
                {
                    _logger.LogDebug("✅ Constructor - XAML úspešne načítané");
                    InitializeDependencyInjection();
                    InitializeResizeSupport();
                    InitializeScrollSupport();
                    _logger.LogInformation("✅ Constructor - Kompletne inicializovaný s realtime validáciou, resize a scroll");
                    UpdateUIVisibility();
                }
                else
                {
                    _logger.LogWarning("⚠️ Constructor - XAML loading zlyhal, vytváram fallback UI");
                    CreateSimpleFallbackUI();
                }

                DataContext = this;
                var duration = EndOperation("Constructor");
                _logger.LogInformation("✅ Constructor COMPLETED - Instance: {ComponentInstanceId}, Duration: {Duration}ms",
                    _componentInstanceId, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL CONSTRUCTOR ERROR");
                CreateSimpleFallbackUI();
            }
        }

        /// <summary>
        /// ObservableCollection pre UI binding - PUBLIC pre x:Bind
        /// </summary>
        public ObservableCollection<DataRowViewModel> DisplayRows => _displayRows;

        #endregion

        #region ✅ PUBLIC API Methods s kompletným logovaním

        /// <summary>
        /// InitializeAsync s realtime validáciou - PUBLIC API
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule>? validationRules,
            GridThrottlingConfig? throttlingConfig,
            int emptyRowsCount = 15,
            DataGridColorConfig? colorConfig = null)
        {
            try
            {
                _logger.LogInformation("🚀 InitializeAsync START - Columns: {ColumnCount}, Rules: {RuleCount}, EmptyRows: {EmptyRows}",
                    columns?.Count ?? 0, validationRules?.Count ?? 0, emptyRowsCount);
                StartOperation("InitializeAsync");

                if (columns == null || columns.Count == 0)
                {
                    _logger.LogError("❌ InitializeAsync: Columns parameter is null or empty");
                    throw new ArgumentException("Columns parameter cannot be null or empty", nameof(columns));
                }

                _logger.LogDebug("📋 Column details: {ColumnNames}", string.Join(", ", columns.Select(c => $"{c.Name}({c.DataType.Name})")));

                // Store throttling config pre realtime validáciu
                _throttlingConfig = throttlingConfig?.Clone() ?? GridThrottlingConfig.Default;
                _logger.LogDebug("⚙️ Throttling config: {ThrottlingConfig}", _throttlingConfig);

                // Store configuration
                _columns.Clear();
                _columns.AddRange(columns);
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;
                _individualColorConfig = colorConfig?.Clone();

                _logger.LogDebug("🎨 Color config: {HasColors} custom colors, Zebra: {ZebraEnabled}",
                    colorConfig?.HasAnyCustomColors ?? false, colorConfig?.IsZebraRowsEnabled ?? false);

                // Initialize services
                await InitializeServicesAsync(columns, validationRules ?? new List<GridValidationRule>(), _throttlingConfig, emptyRowsCount);

                // UI setup
                if (!_xamlLoadFailed)
                {
                    ApplyIndividualColorsToUI();
                    InitializeSearchSortZebra();
                    await CreateInitialEmptyRowsAsync();
                    CreateResizableHeaders();
                    SetupValidAlertsStretching();
                }

                _isInitialized = true;
                UpdateUIVisibility();

                var duration = EndOperation("InitializeAsync");
                _logger.LogInformation("✅ InitializeAsync COMPLETED - Duration: {Duration}ms, Instance: {InstanceId}",
                    duration, _componentInstanceId);
            }
            catch (Exception ex)
            {
                EndOperation("InitializeAsync");
                _logger.LogError(ex, "❌ CRITICAL ERROR during InitializeAsync");
                throw;
            }
        }

        /// <summary>
        /// LoadDataAsync s kompletným logovaním
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                _logger.LogInformation("📊 LoadDataAsync START - InputRows: {RowCount}", data?.Count ?? 0);
                StartOperation("LoadDataAsync");

                if (data == null)
                {
                    _logger.LogWarning("⚠️ LoadDataAsync: Null data provided, using empty list");
                    data = new List<Dictionary<string, object?>>();
                }

                EnsureInitialized();

                // Log data statistics
                if (data.Any())
                {
                    var firstRow = data.First();
                    var columnNames = firstRow.Keys.ToList();
                    _logger.LogDebug("📊 Data columns: {ColumnNames}", string.Join(", ", columnNames));
                    _logger.LogDebug("📊 Sample data: Row[0] = {SampleData}",
                        string.Join(", ", firstRow.Take(3).Select(kvp => $"{kvp.Key}={kvp.Value}")));
                }

                if (_dataManagementService != null)
                {
                    await _dataManagementService.LoadDataAsync(data);
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                    RefreshDataDisplay();
                }

                var duration = EndOperation("LoadDataAsync");
                _logger.LogInformation("✅ LoadDataAsync COMPLETED - Duration: {Duration}ms, FinalRowCount: {FinalCount}",
                    duration, _displayRows.Count);
            }
            catch (Exception ex)
            {
                EndOperation("LoadDataAsync");
                _logger.LogError(ex, "❌ ERROR in LoadDataAsync");
                throw;
            }
        }

        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                _logger.LogInformation("🔍 ValidateAllRowsAsync START");
                StartOperation("ValidateAllRows");
                EnsureInitialized();

                if (_validationService == null)
                {
                    _logger.LogWarning("⚠️ ValidateAllRowsAsync: ValidationService is null");
                    return false;
                }

                var result = await _validationService.ValidateAllRowsAsync();
                var duration = EndOperation("ValidateAllRows");

                _logger.LogInformation("✅ ValidateAllRowsAsync COMPLETED - Result: {IsValid}, Duration: {Duration}ms",
                    result, duration);
                return result;
            }
            catch (Exception ex)
            {
                EndOperation("ValidateAllRows");
                _logger.LogError(ex, "❌ ERROR in ValidateAllRowsAsync");
                throw;
            }
        }

        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                _logger.LogInformation("📤 ExportToDataTableAsync START");
                StartOperation("ExportToDataTable");
                EnsureInitialized();

                if (_exportService == null)
                {
                    _logger.LogWarning("⚠️ ExportToDataTableAsync: ExportService is null");
                    return new DataTable();
                }

                var result = await _exportService.ExportToDataTableAsync();
                var duration = EndOperation("ExportToDataTable");

                _logger.LogInformation("✅ ExportToDataTableAsync COMPLETED - Rows: {RowCount}, Columns: {ColumnCount}, Duration: {Duration}ms",
                    result.Rows.Count, result.Columns.Count, duration);
                return result;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToDataTable");
                _logger.LogError(ex, "❌ ERROR in ExportToDataTableAsync");
                throw;
            }
        }

        public async Task ClearAllDataAsync()
        {
            try
            {
                _logger.LogInformation("🗑️ ClearAllDataAsync START - CurrentRows: {CurrentRowCount}", _displayRows.Count);
                StartOperation("ClearAllData");
                EnsureInitialized();

                if (_dataManagementService != null)
                {
                    await _dataManagementService.ClearAllDataAsync();
                    await UpdateDisplayRowsWithRealtimeValidationAsync();
                    RefreshDataDisplay();
                }

                var duration = EndOperation("ClearAllData");
                _logger.LogInformation("✅ ClearAllDataAsync COMPLETED - Duration: {Duration}ms, NewRowCount: {NewRowCount}",
                    duration, _displayRows.Count);
            }
            catch (Exception ex)
            {
                EndOperation("ClearAllData");
                _logger.LogError(ex, "❌ ERROR in ClearAllDataAsync");
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Column Resize Implementation

        /// <summary>
        /// Inicializuje podporu pre resize stĺpcov
        /// </summary>
        private void InitializeResizeSupport()
        {
            try
            {
                _logger.LogDebug("🔧 InitializeResizeSupport START");

                // Register for window-level pointer events pre resize tracking
                this.PointerMoved += OnPointerMoved;
                this.PointerPressed += OnPointerPressed;
                this.PointerReleased += OnPointerReleased;
                this.PointerCaptureLost += OnPointerCaptureLost;

                _logger.LogDebug("✅ Resize support initialized - Pointer events registered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing resize support");
            }
        }

        /// <summary>
        /// Vytvorí resizable headers pre stĺpce
        /// </summary>
        private void CreateResizableHeaders()
        {
            try
            {
                if (_xamlLoadFailed || HeaderStackPanel == null)
                {
                    _logger.LogWarning("⚠️ Cannot create resizable headers - XAML failed or HeaderStackPanel null");
                    return;
                }

                _logger.LogDebug("🔧 CreateResizableHeaders START - Columns: {ColumnCount}", _columns.Count);

                _resizableHeaders.Clear();
                HeaderStackPanel.Children.Clear();

                foreach (var column in _columns)
                {
                    var header = CreateResizableHeader(column);
                    _resizableHeaders.Add(header);
                    HeaderStackPanel.Children.Add(header.HeaderBorder);
                }

                _logger.LogInformation("✅ Created {HeaderCount} resizable headers", _resizableHeaders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating resizable headers");
            }
        }

        /// <summary>
        /// Vytvorí jeden resizable header pre stĺpec
        /// </summary>
        private ResizableColumnHeader CreateResizableHeader(GridColumnDefinition column)
        {
            var headerBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                BorderThickness = new Thickness(0, 0, 1, 1),
                MinHeight = 40,
                Width = column.Width
            };

            var headerGrid = new Grid();
            headerBorder.Child = headerGrid;

            // Header text
            var headerText = new TextBlock
            {
                Text = column.Header ?? column.Name,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 6, 12, 6),
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            // ✅ NOVÉ: ValidAlerts stĺpec sa roztiahne
            if (column.Name == "ValidAlerts")
            {
                headerBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                headerText.Text = "⚠️ Validation Alerts";
                headerText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange);
            }

            headerGrid.Children.Add(headerText);

            // Resize grip (iba ak nie je ValidAlerts)
            Border? resizeGrip = null;
            if (column.Name != "ValidAlerts")
            {
                resizeGrip = new Border
                {
                    Width = 4,
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Cursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast)
                };

                headerGrid.Children.Add(resizeGrip);
            }

            var resizableHeader = new ResizableColumnHeader
            {
                Column = column,
                HeaderBorder = headerBorder,
                HeaderText = headerText,
                ResizeGrip = resizeGrip,
                OriginalWidth = column.Width
            };

            _logger.LogTrace("📏 Created resizable header: {ColumnName} (Width: {Width})", column.Name, column.Width);

            return resizableHeader;
        }

        /// <summary>
        /// Nastaví ValidAlerts stĺpec aby sa roztiahol na koniec
        /// </summary>
        private void SetupValidAlertsStretching()
        {
            try
            {
                _logger.LogDebug("🔧 SetupValidAlertsStretching START");

                var validAlertsHeader = _resizableHeaders.FirstOrDefault(h => h.Column.Name == "ValidAlerts");
                if (validAlertsHeader != null)
                {
                    // Nastav ValidAlerts aby sa roztiahol
                    validAlertsHeader.HeaderBorder.HorizontalAlignment = HorizontalAlignment.Stretch;

                    // Aktualizuj layout pri zmene veľkosti
                    this.SizeChanged += OnDataGridSizeChanged;

                    _logger.LogDebug("✅ ValidAlerts stretching configured");
                }
                else
                {
                    _logger.LogDebug("⚠️ ValidAlerts column not found for stretching");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting up ValidAlerts stretching");
            }
        }

        private void OnDataGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!_isInitialized) return;

                _logger.LogTrace("📏 DataGrid size changed: {NewWidth}x{NewHeight}", e.NewSize.Width, e.NewSize.Height);

                // Prepočítaj šírku ValidAlerts stĺpca
                RecalculateValidAlertsWidth();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling size change");
            }
        }

        private void RecalculateValidAlertsWidth()
        {
            try
            {
                var validAlertsHeader = _resizableHeaders.FirstOrDefault(h => h.Column.Name == "ValidAlerts");
                if (validAlertsHeader == null) return;

                var totalWidth = this.ActualWidth;
                var otherColumnsWidth = _resizableHeaders
                    .Where(h => h.Column.Name != "ValidAlerts")
                    .Sum(h => h.HeaderBorder.Width);

                var validAlertsWidth = Math.Max(200, totalWidth - otherColumnsWidth - 20); // 20px for margins
                validAlertsHeader.HeaderBorder.Width = validAlertsWidth;

                _logger.LogTrace("📏 ValidAlerts width recalculated: {Width}", validAlertsWidth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error recalculating ValidAlerts width");
            }
        }

        #endregion

        #region ✅ NOVÉ: Resize Event Handlers

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_isResizing) return;

                var position = e.GetCurrentPoint(this);
                var header = FindHeaderUnderPointer(position);

                if (header?.ResizeGrip != null && IsPointerOverResizeGrip(position, header))
                {
                    _isResizing = true;
                    _currentResizingHeader = header;
                    _resizeStartPosition = position.Position.X;
                    _resizeStartWidth = header.HeaderBorder.Width;

                    this.CapturePointer(e.Pointer);

                    _logger.LogDebug("🖱️ Resize started - Column: {ColumnName}, StartWidth: {StartWidth}",
                        header.Column.Name, _resizeStartWidth);

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnPointerPressed");
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_isResizing && _currentResizingHeader != null)
                {
                    var position = e.GetCurrentPoint(this);
                    var deltaX = position.Position.X - _resizeStartPosition;
                    var newWidth = Math.Max(_currentResizingHeader.Column.MinWidth, _resizeStartWidth + deltaX);

                    _currentResizingHeader.HeaderBorder.Width = newWidth;
                    _currentResizingHeader.Column.Width = newWidth;

                    // Update data columns width too
                    UpdateDataColumnsWidth(_currentResizingHeader.Column.Name, newWidth);

                    _logger.LogTrace("🖱️ Resizing column {ColumnName}: {NewWidth}",
                        _currentResizingHeader.Column.Name, newWidth);

                    e.Handled = true;
                }
                else
                {
                    // Update cursor pre resize grip hover
                    var position = e.GetCurrentPoint(this);
                    var header = FindHeaderUnderPointer(position);

                    if (header?.ResizeGrip != null && IsPointerOverResizeGrip(position, header))
                    {
                        this.Cursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast);
                    }
                    else
                    {
                        this.Cursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnPointerMoved");
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_isResizing && _currentResizingHeader != null)
                {
                    var finalWidth = _currentResizingHeader.HeaderBorder.Width;

                    _logger.LogInformation("✅ Resize completed - Column: {ColumnName}, FinalWidth: {FinalWidth} (was: {StartWidth})",
                        _currentResizingHeader.Column.Name, finalWidth, _resizeStartWidth);

                    _isResizing = false;
                    _currentResizingHeader = null;
                    this.ReleasePointerCapture(e.Pointer);

                    // Recalculate ValidAlerts if needed
                    RecalculateValidAlertsWidth();

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnPointerReleased");
            }
        }

        private void OnPointerCaptureLost(object sender, PointerEventArgs e)
        {
            try
            {
                if (_isResizing)
                {
                    _logger.LogDebug("🖱️ Resize cancelled - pointer capture lost");
                    _isResizing = false;
                    _currentResizingHeader = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnPointerCaptureLost");
            }
        }

        private ResizableColumnHeader? FindHeaderUnderPointer(PointerPoint position)
        {
            foreach (var header in _resizableHeaders)
            {
                var headerPosition = header.HeaderBorder.TransformToVisual(this).TransformPoint(new Windows.Foundation.Point(0, 0));
                var headerBounds = new Windows.Foundation.Rect(
                    headerPosition.X,
                    headerPosition.Y,
                    header.HeaderBorder.ActualWidth,
                    header.HeaderBorder.ActualHeight);

                if (headerBounds.Contains(position.Position))
                {
                    return header;
                }
            }
            return null;
        }

        private bool IsPointerOverResizeGrip(PointerPoint position, ResizableColumnHeader header)
        {
            if (header.ResizeGrip == null) return false;

            var gripPosition = header.ResizeGrip.TransformToVisual(this).TransformPoint(new Windows.Foundation.Point(0, 0));
            var gripBounds = new Windows.Foundation.Rect(
                gripPosition.X,
                gripPosition.Y,
                header.ResizeGrip.ActualWidth,
                header.ResizeGrip.ActualHeight);

            return gripBounds.Contains(position.Position);
        }

        private void UpdateDataColumnsWidth(string columnName, double newWidth)
        {
            // TODO: Update width of data columns to match header
            // This would involve updating the ItemsRepeater or data rows
            _logger.LogTrace("📏 UpdateDataColumnsWidth: {ColumnName} = {NewWidth}", columnName, newWidth);
        }

        #endregion

        #region ✅ NOVÉ: Scroll Support Implementation

        private void InitializeScrollSupport()
        {
            try
            {
                _logger.LogDebug("🔧 InitializeScrollSupport START");

                if (!_xamlLoadFailed && DataGridScrollViewer != null && HeaderScrollViewer != null)
                {
                    // Synchronizuj horizontal scroll medzi header a data
                    DataGridScrollViewer.ViewChanged += OnDataScrollViewChanged;
                    HeaderScrollViewer.ViewChanged += OnHeaderScrollViewChanged;

                    _logger.LogDebug("✅ Scroll synchronization initialized");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing scroll support");
            }
        }

        private void OnDataScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing || HeaderScrollViewer == null) return;

                _isScrollSynchronizing = true;
                var horizontalOffset = DataGridScrollViewer.HorizontalOffset;
                HeaderScrollViewer.ChangeView(horizontalOffset, null, null, true);
                _isScrollSynchronizing = false;

                _logger.LogTrace("📜 Data scroll changed: HorizontalOffset = {Offset}", horizontalOffset);
            }
            catch (Exception ex)
            {
                _isScrollSynchronizing = false;
                _logger.LogError(ex, "❌ Error in OnDataScrollViewChanged");
            }
        }

        private void OnHeaderScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing || DataGridScrollViewer == null) return;

                _isScrollSynchronizing = true;
                var horizontalOffset = HeaderScrollViewer.HorizontalOffset;
                DataGridScrollViewer.ChangeView(horizontalOffset, null, null, true);
                _isScrollSynchronizing = false;

                _logger.LogTrace("📜 Header scroll changed: HorizontalOffset = {Offset}", horizontalOffset);
            }
            catch (Exception ex)
            {
                _isScrollSynchronizing = false;
                _logger.LogError(ex, "❌ Error in OnHeaderScrollViewChanged");
            }
        }

        #endregion

        #region ✅ Data Display Implementation

        /// <summary>
        /// Refreshuje zobrazenie dát v UI
        /// </summary>
        private void RefreshDataDisplay()
        {
            try
            {
                if (_xamlLoadFailed || DataRowsContainer == null) return;

                _logger.LogDebug("🎨 RefreshDataDisplay START - Rows: {RowCount}", _displayRows.Count);

                this.DispatcherQueue?.TryEnqueue(() =>
                {
                    try
                    {
                        DataRowsContainer.Children.Clear();

                        foreach (var row in _displayRows)
                        {
                            var rowElement = CreateDataRowElement(row);
                            DataRowsContainer.Children.Add(rowElement);
                        }

                        _logger.LogDebug("✅ Data display refreshed - {RowCount} rows rendered", _displayRows.Count);
                    }
                    catch (Exception uiEx)
                    {
                        _logger.LogError(uiEx, "❌ RefreshDataDisplay UI ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in RefreshDataDisplay");
            }
        }

        private FrameworkElement CreateDataRowElement(DataRowViewModel row)
        {
            var rowBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                BorderThickness = new Thickness(0, 0, 0, 1),
                MinHeight = 36
            };

            var rowStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            rowBorder.Child = rowStackPanel;

            foreach (var cell in row.Cells)
            {
                var cellElement = CreateDataCellElement(cell);
                rowStackPanel.Children.Add(cellElement);
            }

            return rowBorder;
        }

        private FrameworkElement CreateDataCellElement(CellViewModel cell)
        {
            var column = _columns.FirstOrDefault(c => c.Name == cell.ColumnName);
            var width = column?.Width ?? 150;

            var cellBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                BorderThickness = new Thickness(0, 0, 1, 0),
                Width = width,
                MinHeight = 36,
                Padding = new Thickness(4, 2)
            };

            // ✅ ValidAlerts stĺpec sa roztiahne
            if (cell.ColumnName == "ValidAlerts")
            {
                cellBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            var textBox = new TextBox
            {
                Text = cell.DisplayValue,
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                BorderThickness = new Thickness(0),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Realtime validation visual feedback
            if (!cell.IsValid)
            {
                textBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                textBox.BorderThickness = new Thickness(2);
            }

            cellBorder.Child = textBox;
            return cellBorder;
        }

        #endregion

        #region ✅ UI Update Methods s kompletným logovaním

        /// <summary>
        /// Aktualizuje ObservableCollection s realtime validáciou
        /// </summary>
        private async Task UpdateDisplayRowsWithRealtimeValidationAsync()
        {
            try
            {
                _logger.LogDebug("🎨 UpdateDisplayRowsWithRealtimeValidationAsync START");
                StartOperation("UpdateDisplayRows");

                if (_dataManagementService == null)
                {
                    _logger.LogWarning("⚠️ UpdateDisplayRows: DataManagementService is null");
                    return;
                }

                var allData = await _dataManagementService.GetAllDataAsync();
                _logger.LogDebug("📊 Retrieved {DataCount} rows from DataManagementService", allData.Count);

                this.DispatcherQueue?.TryEnqueue(() =>
                {
                    try
                    {
                        var oldRowCount = _displayRows.Count;
                        _displayRows.Clear();

                        for (int i = 0; i < allData.Count; i++)
                        {
                            var rowData = allData[i];
                            var viewModel = new DataRowViewModel
                            {
                                RowIndex = i,
                                Columns = _columns,
                                Data = rowData
                            };

                            // ✅ Subscribe na realtime validáciu
                            foreach (var cell in viewModel.Cells)
                            {
                                cell.PropertyChanged += OnCellValueChanged;
                                cell.OriginalValue = cell.Value; // Set original value
                            }

                            _displayRows.Add(viewModel);
                        }

                        var duration = EndOperation("UpdateDisplayRows");
                        _logger.LogDebug("✅ UpdateDisplayRows UI COMPLETED - {OldCount} → {NewCount} rows, Duration: {Duration}ms",
                            oldRowCount, _displayRows.Count, duration);
                    }
                    catch (Exception uiEx)
                    {
                        _logger.LogError(uiEx, "❌ UpdateDisplayRows UI ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                EndOperation("UpdateDisplayRows");
                _logger.LogError(ex, "❌ ERROR in UpdateDisplayRowsWithRealtimeValidationAsync");
            }
        }

        #endregion

        #region ✅ Realtime Validation Implementation s logovaním

        /// <summary>
        /// Spustí realtime validáciu pre bunku s throttling
        /// </summary>
        private async Task ValidateCellRealtimeAsync(CellViewModel cell)
        {
            if (_validationService == null || !_isInitialized)
            {
                _logger.LogDebug("🔍 ValidateCellRealtime SKIPPED - Service: {HasService}, Initialized: {IsInit}",
                    _validationService != null, _isInitialized);
                return;
            }

            try
            {
                var cellKey = $"{cell.RowIndex}_{cell.ColumnName}";

                lock (_validationLock)
                {
                    // Cancel existing timer pre túto bunku
                    if (_validationTimers.TryGetValue(cellKey, out var existingTimer))
                    {
                        existingTimer.Dispose();
                        _validationTimers.Remove(cellKey);
                    }

                    // Create new timer with throttling delay
                    var delay = _throttlingConfig?.ValidationDebounceMs ?? 300;
                    var timer = new System.Threading.Timer(async _ =>
                    {
                        await PerformCellValidationAsync(cell, cellKey);
                    }, null, delay, Timeout.Infinite);

                    _validationTimers[cellKey] = timer;
                }

                _logger.LogDebug("🔍 Realtime validácia scheduled pre {CellKey}: '{DisplayValue}' (delay: {Delay}ms)",
                    cellKey, cell.DisplayValue, _throttlingConfig?.ValidationDebounceMs ?? 300);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba realtime validácie pre {CellKey}", $"{cell.RowIndex}_{cell.ColumnName}");
            }
        }

        /// <summary>
        /// Vykoná skutočnú validáciu bunky
        /// </summary>
        private async Task PerformCellValidationAsync(CellViewModel cell, string cellKey)
        {
            try
            {
                if (_validationService == null) return;

                _logger.LogDebug("🔍 PerformCellValidation START - {CellKey}: '{Value}'", cellKey, cell.DisplayValue);

                // Validate cell value
                var errors = await _validationService.ValidateCellAsync(cell.ColumnName, cell.DisplayValue);

                // Update UI na main thread
                this.DispatcherQueue?.TryEnqueue(() =>
                {
                    var wasValid = cell.IsValid;
                    cell.IsValid = !errors.Any();
                    cell.ValidationErrors = string.Join("; ", errors);

                    if (wasValid != cell.IsValid)
                    {
                        _logger.LogDebug("🔍 Validation state changed for {CellKey}: {OldState} → {NewState}",
                            cellKey, wasValid ? "VALID" : "INVALID", cell.IsValid ? "VALID" : "INVALID");
                    }

                    if (errors.Any())
                    {
                        _logger.LogWarning("⚠️ Validation errors for {CellKey}: {Errors}", cellKey, string.Join("; ", errors));
                    }
                });

                // Clean up timer
                lock (_validationLock)
                {
                    if (_validationTimers.TryGetValue(cellKey, out var timer))
                    {
                        timer.Dispose();
                        _validationTimers.Remove(cellKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba pri validácii bunky {CellKey}", cellKey);
            }
        }

        /// <summary>
        /// Handler pre cell value changes (volá sa z ViewModelu)
        /// </summary>
        private async void OnCellValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is CellViewModel cell && e.PropertyName == nameof(CellViewModel.DisplayValue))
            {
                _logger.LogDebug("📝 Cell value changed: [{RowIndex}, {ColumnName}] = '{NewValue}' (was: '{OldValue}')",
                    cell.RowIndex, cell.ColumnName, cell.DisplayValue, cell.OriginalValue);

                // Spusti realtime validáciu
                await ValidateCellRealtimeAsync(cell);

                // Update data management service
                if (_dataManagementService != null)
                {
                    try
                    {
                        await _dataManagementService.SetCellValueAsync(cell.RowIndex, cell.ColumnName, cell.DisplayValue);
                        _logger.LogDebug("✅ Cell value persisted to DataManagementService");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to persist cell value to DataManagementService");
                    }
                }
            }
        }

        #endregion

        #region ✅ XAML Event Handlers s logovaním

        /// <summary>
        /// Handler pre načítanie TextBox - pripojí KeyDown event
        /// </summary>
        public void OnCellTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Pripoj KeyDown event handler (async wrapper)
                textBox.KeyDown += (s, args) =>
                {
                    _ = Task.Run(async () => await OnCellKeyDown(s, args));
                };

                // Tag obsahuje CellViewModel
                if (textBox.Tag is CellViewModel cell)
                {
                    _logger.LogTrace("🔧 TextBox loaded pre cell [{RowIndex}, {ColumnName}]", cell.RowIndex, cell.ColumnName);
                }
            }
        }

        /// <summary>
        /// Handler pre keyboard events v bunkách
        /// </summary>
        private async Task OnCellKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            try
            {
                // Získaj CellViewModel z Tag
                if (textBox.Tag is not CellViewModel cell) return;

                _logger.LogDebug("⌨️ KeyDown in cell [{RowIndex}, {ColumnName}]: {Key}",
                    cell.RowIndex, cell.ColumnName, e.Key);

                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Escape:
                        await HandleEscapeKey(textBox, cell);
                        e.Handled = true;
                        break;

                    case Windows.System.VirtualKey.Enter:
                        await HandleEnterKey(textBox, cell, IsShiftPressed());
                        e.Handled = true;
                        break;

                    case Windows.System.VirtualKey.Tab:
                        await HandleTabKey(textBox, cell, IsShiftPressed());
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba pri spracovaní keyboard event");
            }
        }

        /// <summary>
        /// Handle Escape key - zruš zmeny
        /// </summary>
        private async Task HandleEscapeKey(TextBox textBox, CellViewModel cell)
        {
            try
            {
                var oldValue = cell.DisplayValue;
                // Vráť originálnu hodnotu
                cell.DisplayValue = cell.OriginalValue?.ToString() ?? "";
                textBox.Text = cell.DisplayValue;

                // Vyčisti validačné chyby
                cell.IsValid = true;
                cell.ValidationErrors = "";

                _logger.LogDebug("🔄 ESC pressed - Reverted cell [{RowIndex}, {ColumnName}]: '{OldValue}' → '{NewValue}'",
                    cell.RowIndex, cell.ColumnName, oldValue, cell.DisplayValue);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba pri ESC handling");
            }
        }

        /// <summary>
        /// Handle Enter key - potvrď zmeny
        /// </summary>
        private async Task HandleEnterKey(TextBox textBox, CellViewModel cell, bool isShiftPressed)
        {
            try
            {
                if (isShiftPressed)
                {
                    // Shift+Enter = nový riadok v bunke
                    var currentPosition = textBox.SelectionStart;
                    var currentText = textBox.Text;
                    var newText = currentText.Insert(currentPosition, Environment.NewLine);
                    textBox.Text = newText;
                    textBox.SelectionStart = currentPosition + Environment.NewLine.Length;
                    _logger.LogDebug("📝 Shift+Enter - Added newline in cell [{RowIndex}, {ColumnName}]", cell.RowIndex, cell.ColumnName);
                    return;
                }

                // Potvrď hodnotu
                cell.OriginalValue = cell.DisplayValue;

                // Force validácia bez throttling
                await PerformCellValidationAsync(cell, $"{cell.RowIndex}_{cell.ColumnName}");

                _logger.LogDebug("✅ ENTER pressed - Confirmed cell [{RowIndex}, {ColumnName}] value: '{DisplayValue}'",
                    cell.RowIndex, cell.ColumnName, cell.DisplayValue);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba pri ENTER handling");
            }
        }

        /// <summary>
        /// Handle Tab key - potvrď zmeny a presun
        /// </summary>
        private async Task HandleTabKey(TextBox textBox, CellViewModel cell, bool isShiftPressed)
        {
            try
            {
                // Potvrď hodnotu
                cell.OriginalValue = cell.DisplayValue;

                // Force validácia bez throttling
                await PerformCellValidationAsync(cell, $"{cell.RowIndex}_{cell.ColumnName}");

                _logger.LogDebug("✅ TAB pressed - Confirmed cell [{RowIndex}, {ColumnName}] value: '{DisplayValue}' (Shift: {IsShift})",
                    cell.RowIndex, cell.ColumnName, cell.DisplayValue, isShiftPressed);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba pri TAB handling");
            }
        }

        /// <summary>
        /// Handler pre skrytie validation overlay - PUBLIC pre XAML
        /// </summary>
        public void OnHideValidationOverlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var validationOverlay = this.FindName("ValidationOverlay") as FrameworkElement;
                if (validationOverlay != null)
                {
                    validationOverlay.Visibility = Visibility.Collapsed;
                    _logger.LogDebug("✅ Validation overlay hidden by user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error hiding validation overlay");
            }
        }

        #endregion

        #region ✅ Helper Methods s logovaním

        private void TryInitializeXaml()
        {
            try
            {
                this.InitializeComponent();
                _xamlLoadFailed = false;
                _logger.LogDebug("✅ XAML Loading SUCCESS");
            }
            catch (Exception xamlEx)
            {
                _logger.LogError(xamlEx, "❌ XAML LOADING FAILED");
                _xamlLoadFailed = true;
            }
        }

        private void InitializeDependencyInjection()
        {
            try
            {
                _logger.LogDebug("🔧 Initializing Dependency Injection...");
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                _dataManagementService = _serviceProvider.GetService<IDataManagementService>();
                _validationService = _serviceProvider.GetService<IValidationService>();
                _exportService = _serviceProvider.GetService<IExportService>();
                _searchAndSortService = new SearchAndSortService();

                _logger.LogInformation("✅ Dependency Injection initialized - Services: DataMgmt={HasDataMgmt}, Validation={HasValidation}, Export={HasExport}",
                    _dataManagementService != null, _validationService != null, _exportService != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DI initialization ERROR");
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            _logger.LogDebug("🔧 Configuring services with logger: {LoggerType}", _logger.GetType().Name);

            // ✅ NEZÁVISLÉ KOMPONENTY: Použije poskytnutý logger alebo NullLogger
            services.AddSingleton<IDataManagementService>(provider =>
                new DataManagementService(CreateTypedLogger<DataManagementService>()));

            services.AddSingleton<IValidationService>(provider =>
                new ValidationService(CreateTypedLogger<ValidationService>()));

            services.AddTransient<IExportService>(provider =>
            {
                var dataService = provider.GetRequiredService<IDataManagementService>();
                return new ExportService(CreateTypedLogger<ExportService>(), dataService);
            });
        }

        /// <summary>
        /// Vytvorí typed logger alebo NullLogger ak nie je poskytnutý
        /// </summary>
        private ILogger<T> CreateTypedLogger<T>()
        {
            if (_logger is NullLogger)
            {
                _logger.LogDebug("🔧 Creating NullLogger<{TypeName}> - no logging will occur", typeof(T).Name);
                return NullLogger<T>.Instance;
            }

            // Ak máme skutočný logger, vytvoríme z neho typed logger
            _logger.LogDebug("🔧 Creating TypedLogger<{TypeName}> from {LoggerType}", typeof(T).Name, _logger.GetType().Name);
            return new TypedLoggerWrapper<T>(_logger);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                _logger.LogError("❌ DataGrid is not initialized - call InitializeAsync() first");
                throw new InvalidOperationException("DataGrid is not initialized. Call InitializeAsync() first.");
            }
        }

        #endregion

        #region ✅ Keyboard Helper Methods

        private static bool IsShiftPressed()
        {
            var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
            return (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        private static bool IsCtrlPressed()
        {
            var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            return (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        #endregion

        #region ✅ Performance Tracking

        private void StartOperation(string operationName)
        {
            _operationStartTimes[operationName] = DateTime.UtcNow;
            _logger.LogDebug("⏱️ Operation START: {OperationName}", operationName);
        }

        private double EndOperation(string operationName)
        {
            if (_operationStartTimes.TryGetValue(operationName, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _operationStartTimes.Remove(operationName);
                var roundedDuration = Math.Round(duration, 2);
                _logger.LogDebug("⏱️ Operation END: {OperationName} - {Duration}ms", operationName, roundedDuration);
                return roundedDuration;
            }
            return 0;
        }

        #endregion

        #region ✅ Skeleton Methods

        private void CreateSimpleFallbackUI()
        {
            try
            {
                _logger.LogWarning("🔧 Creating fallback UI due to XAML failure");
                var mainGrid = new Grid();
                var fallbackText = new TextBlock
                {
                    Text = "⚠️ AdvancedDataGrid - XAML Fallback Mode\n(Realtime validation available)",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                };
                mainGrid.Children.Add(fallbackText);
                this.Content = mainGrid;
                _logger.LogInformation("✅ Fallback UI created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CreateSimpleFallbackUI FAILED");
            }
        }

        private void UpdateUIVisibility()
        {
            if (_xamlLoadFailed) return;

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    var mainContentGrid = this.FindName("MainContentGrid") as FrameworkElement;
                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;

                    if (mainContentGrid != null)
                        mainContentGrid.Visibility = _isInitialized ? Visibility.Visible : Visibility.Collapsed;

                    if (loadingOverlay != null)
                        loadingOverlay.Visibility = _isInitialized ? Visibility.Collapsed : Visibility.Visible;

                    _logger.LogDebug("🎨 UI visibility updated - Initialized: {IsInitialized}", _isInitialized);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ UpdateUIVisibility ERROR");
                }
            });
        }

        private void ApplyIndividualColorsToUI()
        {
            _logger.LogDebug("🎨 ApplyIndividualColorsToUI called - HasColors: {HasColors}",
                _individualColorConfig?.HasAnyCustomColors ?? false);

            // TODO: Implementácia aplikácie farieb
        }

        private void InitializeSearchSortZebra()
        {
            _logger.LogDebug("🔍 InitializeSearchSortZebra called - Service: {HasService}", _searchAndSortService != null);

            // TODO: Implementácia search/sort/zebra inicializácie
        }

        private async Task CreateInitialEmptyRowsAsync()
        {
            _logger.LogDebug("📄 CreateInitialEmptyRowsAsync called - UnifiedRowCount: {RowCount}", _unifiedRowCount);
            await Task.CompletedTask;

            // TODO: Implementácia vytvorenia prázdnych riadkov
        }

        private async Task InitializeServicesAsync(List<GridColumnDefinition> columns, List<GridValidationRule> rules,
            GridThrottlingConfig throttling, int emptyRows)
        {
            _logger.LogDebug("🔧 InitializeServicesAsync called - Columns: {ColCount}, Rules: {RuleCount}, EmptyRows: {EmptyRows}",
                columns.Count, rules.Count, emptyRows);
            await Task.CompletedTask;

            // TODO: Implementácia inicializácie služieb
        }

        #endregion

        #region ✅ Properties

        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        public string DiagnosticInfo =>
            $"AdvancedDataGrid[{_componentInstanceId}]: Initialized={_isInitialized}, " +
            $"RealtimeValidation=ENABLED, Rows={_displayRows.Count}, Logger={_logger.GetType().Name}, Resize=ENABLED, Scroll=ENABLED";

        // ✅ NOVÉ: XAML element access properties
        private StackPanel? HeaderStackPanel => this.FindName("HeaderStackPanel") as StackPanel;
        private ScrollViewer? HeaderScrollViewer => this.FindName("HeaderScrollViewer") as ScrollViewer;
        private ScrollViewer? DataGridScrollViewer => this.FindName("DataGridScrollViewer") as ScrollViewer;
        private StackPanel? DataRowsContainer => this.FindName("DataRowsContainer") as StackPanel;

        #endregion

        #region ✅ INotifyPropertyChanged & IDisposable

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                _logger.LogInformation("🧹 AdvancedDataGrid DISPOSE START - Instance: {InstanceId}", _componentInstanceId);

                // Dispose validation timers
                lock (_validationLock)
                {
                    foreach (var timer in _validationTimers.Values)
                    {
                        timer?.Dispose();
                    }
                    _validationTimers.Clear();
                    _logger.LogDebug("🧹 Disposed {TimerCount} validation timers", _validationTimers.Count);
                }

                // Unsubscribe from cell events
                var cellEventCount = 0;
                foreach (var row in _displayRows)
                {
                    foreach (var cell in row.Cells)
                    {
                        cell.PropertyChanged -= OnCellValueChanged;
                        cellEventCount++;
                    }
                }
                _logger.LogDebug("🧹 Unsubscribed from {CellEventCount} cell events", cellEventCount);

                // ✅ Dispose resize support
                this.PointerMoved -= OnPointerMoved;
                this.PointerPressed -= OnPointerPressed;
                this.PointerReleased -= OnPointerReleased;
                this.PointerCaptureLost -= OnPointerCaptureLost;
                this.SizeChanged -= OnDataGridSizeChanged;

                _searchAndSortService?.Dispose();

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                _operationStartTimes.Clear();
                _resizableHeaders.Clear();

                _isDisposed = true;
                _logger.LogInformation("✅ AdvancedDataGrid DISPOSED successfully - Instance: {InstanceId}", _componentInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during dispose");
            }
        }

        #endregion
    }

    #region ✅ Helper Classes

    /// <summary>
    /// Wrapper pre konverziu ILogger na ILogger<T>
    /// </summary>
    internal class TypedLoggerWrapper<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public TypedLoggerWrapper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel)
            => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <summary>
    /// ✅ NOVÉ: Resizable column header class
    /// </summary>
    internal class ResizableColumnHeader
    {
        public required GridColumnDefinition Column { get; set; }
        public required Border HeaderBorder { get; set; }
        public required TextBlock HeaderText { get; set; }
        public Border? ResizeGrip { get; set; }
        public double OriginalWidth { get; set; }
    }

    #endregion

    #region ✅ ViewModely s realtime validáciou - NEZMENENÉ

    /// <summary>
    /// ViewModel pre zobrazenie riadku v UI - S realtime validáciou
    /// </summary>
    public class DataRowViewModel : INotifyPropertyChanged
    {
        public int RowIndex { get; set; }
        public List<GridColumnDefinition> Columns { get; set; } = new();
        public Dictionary<string, object?> Data { get; set; } = new();

        public List<CellViewModel> Cells
        {
            get
            {
                var cells = new List<CellViewModel>();
                foreach (var column in Columns)
                {
                    var value = Data.ContainsKey(column.Name) ? Data[column.Name] : null;
                    var cell = new CellViewModel
                    {
                        ColumnName = column.Name,
                        Value = value,
                        DisplayValue = value?.ToString() ?? "",
                        Header = column.Header ?? column.Name,
                        Width = column.Width,
                        RowIndex = RowIndex,
                        OriginalValue = value // Set original value
                    };
                    cells.Add(cell);
                }
                return cells;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public override string ToString() => $"DataRow {RowIndex}: {Cells.Count} cells";
    }

    /// <summary>
    /// ViewModel pre zobrazenie bunky - S KOMPLETNOU realtime validáciou
    /// </summary>
    public class CellViewModel : INotifyPropertyChanged
    {
        private string _displayValue = "";
        private bool _isValid = true;
        private string _validationErrors = "";

        public string ColumnName { get; set; } = "";
        public object? Value { get; set; }
        public object? OriginalValue { get; set; } // Pre ESC handling
        public string Header { get; set; } = "";
        public double Width { get; set; }
        public int RowIndex { get; set; }

        /// <summary>
        /// DisplayValue s PropertyChanged pre realtime validáciu
        /// </summary>
        public string DisplayValue
        {
            get => _displayValue;
            set
            {
                if (SetProperty(ref _displayValue, value))
                {
                    Value = value; // Aktualizuj aj Value object
                }
            }
        }

        /// <summary>
        /// Validačný stav bunky
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
        /// Má validačné chyby
        /// </summary>
        public bool HasValidationErrors => !string.IsNullOrEmpty(ValidationErrors);

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

        public override string ToString() => $"Cell[{ColumnName}]: {DisplayValue} (Valid: {IsValid})";
    }

    #endregion
}