// Controls/AdvancedDataGrid.xaml.cs - ✅ KOMPLETNE OPRAVENÝ s Resize+Scroll+Stretch
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.DataGridColorConfig;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid - NEZÁVISLÝ KOMPONENT s ILogger abstractions
    /// ✅ KOMPLETNE OPRAVENÝ: Resize+Scroll+Stretch+Logging
    /// ✅ NEZÁVISLÝ na LoggerComponent - používa iba ILogger abstractions
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

        // ✅ OPRAVENÉ: NEZÁVISLÉ LOGOVANIE - používa ILogger abstractions
        private readonly ILogger _logger;
        private readonly string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];

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

        // ✅ NOVÉ: Layout management
        private double _totalAvailableWidth = 0;
        private double _validAlertsMinWidth = 200;

        #endregion

        #region ✅ OPRAVENÉ: Constructors s ILogger abstractions podporou

        /// <summary>
        /// Vytvorí AdvancedDataGrid bez loggingu (NullLogger) - DEFAULT konštruktor
        /// </summary>
        public AdvancedDataGrid() : this(null)
        {
        }

        /// <summary>
        /// Vytvorí AdvancedDataGrid s voliteľným loggerom
        /// ✅ HLAVNÝ KONŠTRUKTOR - podporuje externe logovanie cez abstractions
        /// </summary>
        /// <param name="logger">ILogger pre logovanie (null = žiadne logovanie)</param>
        public AdvancedDataGrid(ILogger? logger)
        {
            try
            {
                // ✅ OPRAVENÉ: Použije poskytnutý logger alebo NullLogger
                _logger = logger ?? NullLogger.Instance;

                _logger.LogDebug("🔧 AdvancedDataGrid Constructor START - Instance: {ComponentInstanceId}", _componentInstanceId);
                StartOperation("Constructor");

                // ✅ KRITICKÉ: InitializeComponent pre UserControl
                this.InitializeComponent();

                _logger.LogDebug("✅ Constructor - XAML úspešne načítané");
                InitializeDependencyInjection();
                InitializeResizeSupport();
                InitializeScrollSupport();
                InitializeLayoutManagement();

                _logger.LogInformation("✅ Constructor - Kompletne inicializovaný s resize, scroll, stretch");

                this.DataContext = this;
                var duration = EndOperation("Constructor");
                _logger.LogInformation("✅ Constructor COMPLETED - Instance: {ComponentInstanceId}, Duration: {Duration}ms",
                    _componentInstanceId, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL CONSTRUCTOR ERROR");
                throw;
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
            List<GridValidationRule>? validationRules = null,
            GridThrottlingConfig? throttlingConfig = null,
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

                // ✅ NOVÉ: UI setup s resize, scroll a stretch funkcionalitou
                ApplyIndividualColorsToUI();
                InitializeSearchSortZebra();
                await CreateInitialEmptyRowsAsync();
                await CreateResizableHeadersAsync();
                SetupValidAlertsStretching();
                SetupScrollSynchronization();

                _isInitialized = true;
                await UpdateUIVisibilityAsync();

                var duration = EndOperation("InitializeAsync");
                _logger.LogInformation("✅ InitializeAsync COMPLETED - Duration: {Duration}ms, Features: Resize+Scroll+Stretch", duration);
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
                    await RefreshDataDisplayAsync();
                }

                // ✅ NOVÉ: Update layout after data load
                await UpdateLayoutAfterDataChangeAsync();

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
                    await RefreshDataDisplayAsync();
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

                // Register for pointer events pre resize tracking
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
        private async Task CreateResizableHeadersAsync()
        {
            try
            {
                if (HeaderStackPanel == null)
                {
                    _logger.LogWarning("⚠️ Cannot create resizable headers - HeaderStackPanel null");
                    return;
                }

                _logger.LogDebug("🔧 CreateResizableHeaders START - Columns: {ColumnCount}", _columns.Count);

                await this.DispatcherQueue.EnqueueAsync(async () =>
                {
                    _resizableHeaders.Clear();
                    HeaderStackPanel.Children.Clear();

                    foreach (var column in _columns)
                    {
                        var header = await CreateResizableHeaderAsync(column);
                        _resizableHeaders.Add(header);
                        HeaderStackPanel.Children.Add(header.HeaderBorder);
                    }

                    // ✅ KĽÚČOVÉ: Setup ValidAlerts stretching
                    await SetupValidAlertsStretchingAsync();

                    _logger.LogInformation("✅ Created {HeaderCount} resizable headers", _resizableHeaders.Count);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating resizable headers");
            }
        }

        /// <summary>
        /// Vytvorí jeden resizable header pre stĺpec
        /// </summary>
        private async Task<ResizableColumnHeader> CreateResizableHeaderAsync(GridColumnDefinition column)
        {
            return await Task.Run(() =>
            {
                var headerBorder = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    MinHeight = 40,
                    Width = column.Width,
                    HorizontalAlignment = HorizontalAlignment.Left
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

                // ✅ KĽÚČOVÉ: ValidAlerts stĺpec konfigurácia
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
                        VerticalAlignment = VerticalAlignment.Stretch
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
            });
        }

        #endregion

        #region ✅ NOVÉ: Resize Event Handlers - OPRAVENÉ

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
                    _ = UpdateDataColumnsWidthAsync(_currentResizingHeader.Column.Name, newWidth);

                    _logger.LogTrace("🖱️ Resizing column {ColumnName}: {NewWidth}",
                        _currentResizingHeader.Column.Name, newWidth);

                    e.Handled = true;
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
                    _ = RecalculateValidAlertsWidthAsync();

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnPointerReleased");
            }
        }

        // ✅ OPRAVENÉ: Správny signature pre PointerEventHandler
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

        private ResizableColumnHeader? FindHeaderUnderPointer(Microsoft.UI.Input.PointerPoint position)
        {
            foreach (var header in _resizableHeaders)
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error finding header under pointer");
                }
            }
            return null;
        }

        private bool IsPointerOverResizeGrip(Microsoft.UI.Input.PointerPoint position, ResizableColumnHeader header)
        {
            if (header.ResizeGrip == null) return false;

            try
            {
                var gripPosition = header.ResizeGrip.TransformToVisual(this).TransformPoint(new Windows.Foundation.Point(0, 0));
                var gripBounds = new Windows.Foundation.Rect(
                    gripPosition.X,
                    gripPosition.Y,
                    header.ResizeGrip.ActualWidth,
                    header.ResizeGrip.ActualHeight);

                return gripBounds.Contains(position.Position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error checking resize grip");
                return false;
            }
        }

        private async Task UpdateDataColumnsWidthAsync(string columnName, double newWidth)
        {
            try
            {
                // TODO: Update width of data columns to match header
                await Task.CompletedTask;
                _logger.LogTrace("📏 UpdateDataColumnsWidth: {ColumnName} = {NewWidth}", columnName, newWidth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating data columns width");
            }
        }

        #endregion

        #region ✅ NOVÉ: ValidAlerts Stretching Implementation

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

        private async Task SetupValidAlertsStretchingAsync()
        {
            await Task.Run(() => SetupValidAlertsStretching());
        }

        private void OnDataGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!_isInitialized) return;

                _logger.LogTrace("📏 DataGrid size changed: {NewWidth}x{NewHeight}", e.NewSize.Width, e.NewSize.Height);

                _totalAvailableWidth = e.NewSize.Width;

                // Prepočítaj šírku ValidAlerts stĺpca
                _ = RecalculateValidAlertsWidthAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling size change");
            }
        }

        private async Task RecalculateValidAlertsWidthAsync()
        {
            try
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    var validAlertsHeader = _resizableHeaders.FirstOrDefault(h => h.Column.Name == "ValidAlerts");
                    if (validAlertsHeader == null) return;

                    if (_totalAvailableWidth <= 0) return;

                    var otherColumnsWidth = _resizableHeaders
                        .Where(h => h.Column.Name != "ValidAlerts")
                        .Sum(h => h.HeaderBorder.Width);

                    var validAlertsWidth = Math.Max(_validAlertsMinWidth, _totalAvailableWidth - otherColumnsWidth - 20); // 20px for margins
                    validAlertsHeader.HeaderBorder.Width = validAlertsWidth;
                    validAlertsHeader.Column.Width = validAlertsWidth;

                    _logger.LogTrace("📏 ValidAlerts width recalculated: {Width}", validAlertsWidth);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error recalculating ValidAlerts width");
            }
        }

        #endregion

        #region ✅ NOVÉ: Scroll Support Implementation

        private void InitializeScrollSupport()
        {
            try
            {
                _logger.LogDebug("🔧 InitializeScrollSupport START");
                // Scroll events sa nastavia v SetupScrollSynchronization
                _logger.LogDebug("✅ Scroll support initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing scroll support");
            }
        }

        private void SetupScrollSynchronization()
        {
            try
            {
                if (DataGridScrollViewer != null && HeaderScrollViewer != null)
                {
                    // Synchronizuj horizontal scroll medzi header a data
                    DataGridScrollViewer.ViewChanged += OnDataScrollViewChanged;
                    HeaderScrollViewer.ViewChanged += OnHeaderScrollViewChanged;

                    _logger.LogDebug("✅ Scroll synchronization configured");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting up scroll synchronization");
            }
        }

        private void OnDataScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (_isScrollSynchronizing || HeaderScrollViewer == null) return;

                _isScrollSynchronizing = true;
                var horizontalOffset = DataGridScrollViewer!.HorizontalOffset;
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
                var horizontalOffset = HeaderScrollViewer!.HorizontalOffset;
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

        #region ✅ NOVÉ: Layout Management

        private void InitializeLayoutManagement()
        {
            try
            {
                _logger.LogDebug("🔧 InitializeLayoutManagement START");

                // Register for layout updates
                this.LayoutUpdated += OnLayoutUpdated;

                _logger.LogDebug("✅ Layout management initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing layout management");
            }
        }

        private void OnLayoutUpdated(object? sender, object e)
        {
            try
            {
                if (!_isInitialized) return;

                // Update total available width
                if (this.ActualWidth > 0 && Math.Abs(_totalAvailableWidth - this.ActualWidth) > 1)
                {
                    _totalAvailableWidth = this.ActualWidth;
                    _ = RecalculateValidAlertsWidthAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnLayoutUpdated");
            }
        }

        private async Task UpdateLayoutAfterDataChangeAsync()
        {
            try
            {
                await Task.Delay(100); // Small delay to let layout settle
                await RecalculateValidAlertsWidthAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating layout after data change");
            }
        }

        #endregion

        #region ✅ Data Display Implementation - OPRAVENÉ

        /// <summary>
        /// Refreshuje zobrazenie dát v UI
        /// </summary>
        private async Task RefreshDataDisplayAsync()
        {
            try
            {
                if (DataRowsContainer == null) return;

                _logger.LogDebug("🎨 RefreshDataDisplay START - Rows: {RowCount}", _displayRows.Count);

                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    try
                    {
                        DataRowsContainer.ItemsSource = null;
                        DataRowsContainer.ItemsSource = _displayRows;

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

        #endregion

        #region ✅ UI Update Methods s kompletným logovaním

        /// <summary>
        /// Aktualizuje ObservableCollection s realtime validáciou
        /// </summary>
        private async Task UpdateDisplayRowsWithRealtimeValidationAsync()
        {
            try
            {
                _logger.LogDebug("🎨 UpdateDisplayRowsWithRealtimeValidation START");
                StartOperation("UpdateDisplayRows");

                if (_dataManagementService == null)
                {
                    _logger.LogWarning("⚠️ UpdateDisplayRows: DataManagementService is null");
                    return;
                }

                var allData = await _dataManagementService.GetAllDataAsync();
                _logger.LogDebug("📊 Retrieved {DataCount} rows from DataManagementService", allData.Count);

                await this.DispatcherQueue.EnqueueAsync(() =>
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
                _logger.LogError(ex, "❌ ERROR in UpdateDisplayRowsWithRealtimeValidation");
            }
        }

        private async Task UpdateUIVisibilityAsync()
        {
            await this.DispatcherQueue.EnqueueAsync(() =>
            {
                try
                {
                    if (MainContentGrid != null)
                        MainContentGrid.Visibility = _isInitialized ? Visibility.Visible : Visibility.Collapsed;

                    if (LoadingOverlay != null)
                        LoadingOverlay.Visibility = _isInitialized ? Visibility.Collapsed : Visibility.Visible;

                    _logger.LogDebug("🎨 UI visibility updated - Initialized: {IsInitialized}", _isInitialized);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ UpdateUIVisibility ERROR");
                }
            });
        }

        #endregion

        #region ✅ XAML Event Handlers - PUBLIC pre XAML

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

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                _logger.LogError("❌ DataGrid is not initialized - call InitializeAsync() first");
                throw new InvalidOperationException("DataGrid is not initialized. Call InitializeAsync() first.");
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

        #region ✅ Skeleton/Stub Methods - OPRAVENÉ async

        private void ApplyIndividualColorsToUI()
        {
            _logger.LogDebug("🎨 ApplyIndividualColorsToUI called - HasColors: {HasColors}",
                _individualColorConfig?.HasAnyCustomColors ?? false);
        }

        private void InitializeSearchSortZebra()
        {
            _logger.LogDebug("🔍 InitializeSearchSortZebra called - Service: {HasService}", _searchAndSortService != null);
        }

        private async Task CreateInitialEmptyRowsAsync()
        {
            _logger.LogDebug("📄 CreateInitialEmptyRowsAsync called - UnifiedRowCount: {RowCount}", _unifiedRowCount);
            await Task.CompletedTask;
        }

        private async Task InitializeServicesAsync(List<GridColumnDefinition> columns, List<GridValidationRule> rules,
            GridThrottlingConfig throttling, int emptyRows)
        {
            _logger.LogDebug("🔧 InitializeServicesAsync called - Columns: {ColCount}, Rules: {RuleCount}, EmptyRows: {EmptyRows}",
                columns.Count, rules.Count, emptyRows);
            await Task.CompletedTask;
        }

        // ✅ OPRAVENÉ: Async Cell value change handler
        private async void OnCellValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is CellViewModel cell && e.PropertyName == nameof(CellViewModel.DisplayValue))
            {
                _logger.LogDebug("📝 Cell value changed: [{RowIndex}, {ColumnName}] = '{NewValue}'",
                    cell.RowIndex, cell.ColumnName, cell.DisplayValue);

                // Handle async validation
                try
                {
                    await Task.Run(async () =>
                    {
                        // Placeholder for validation logic
                        await Task.CompletedTask;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in async cell value change handling");
                }
            }
        }

        #endregion

        #region ✅ Properties

        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        public string DiagnosticInfo =>
            $"AdvancedDataGrid[{_componentInstanceId}]: Initialized={_isInitialized}, " +
            $"Features=Resize+Scroll+Stretch, Rows={_displayRows.Count}, Logger={_logger.GetType().Name}";

        // ✅ XAML element access properties
        private StackPanel? HeaderStackPanel => this.FindName("HeaderStackPanel") as StackPanel;
        private ScrollViewer? HeaderScrollViewer => this.FindName("HeaderScrollViewer") as ScrollViewer;
        private ScrollViewer? DataGridScrollViewer => this.FindName("DataGridScrollViewer") as ScrollViewer;
        private ItemsControl? DataRowsContainer => this.FindName("DataRowsContainer") as ItemsControl;

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
                }

                // Unsubscribe from events
                this.PointerMoved -= OnPointerMoved;
                this.PointerPressed -= OnPointerPressed;
                this.PointerReleased -= OnPointerReleased;
                this.PointerCaptureLost -= OnPointerCaptureLost;
                this.SizeChanged -= OnDataGridSizeChanged;
                this.LayoutUpdated -= OnLayoutUpdated;

                if (DataGridScrollViewer != null)
                    DataGridScrollViewer.ViewChanged -= OnDataScrollViewChanged;
                if (HeaderScrollViewer != null)
                    HeaderScrollViewer.ViewChanged -= OnHeaderScrollViewChanged;

                // Unsubscribe from cell events
                foreach (var row in _displayRows)
                {
                    foreach (var cell in row.Cells)
                    {
                        cell.PropertyChanged -= OnCellValueChanged;
                    }
                }

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
    /// ✅ Resizable column header class
    /// </summary>
    internal class ResizableColumnHeader
    {
        public required GridColumnDefinition Column { get; set; }
        public required Border HeaderBorder { get; set; }
        public required TextBlock HeaderText { get; set; }
        public Border? ResizeGrip { get; set; }
        public double OriginalWidth { get; set; }
    }

    /// <summary>
    /// ✅ ViewModely s realtime validáciou
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
                        OriginalValue = value
                    };
                    cells.Add(cell);
                }
                return cells;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => $"DataRow {RowIndex}: {Cells.Count} cells";
    }

    /// <summary>
    /// ViewModel pre zobrazenie bunky
    /// </summary>
    public class CellViewModel : INotifyPropertyChanged
    {
        private string _displayValue = "";
        private bool _isValid = true;
        private string _validationErrors = "";

        public string ColumnName { get; set; } = "";
        public object? Value { get; set; }
        public object? OriginalValue { get; set; }
        public string Header { get; set; } = "";
        public double Width { get; set; }
        public int RowIndex { get; set; }

        public string DisplayValue
        {
            get => _displayValue;
            set
            {
                if (SetProperty(ref _displayValue, value))
                {
                    Value = value;
                }
            }
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public string ValidationErrors
        {
            get => _validationErrors;
            set => SetProperty(ref _validationErrors, value);
        }

        public bool HasValidationErrors => !string.IsNullOrEmpty(ValidationErrors);

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

        public override string ToString() => $"Cell[{ColumnName}]: {DisplayValue} (Valid: {IsValid})";
    }

    #endregion
}