// Controls/AdvancedDataGrid.Core.cs - ✅ CORE partial class - fields, properties, constructors
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    /// AdvancedDataGrid CORE partial class - shared infrastructure, fields, properties, constructors
    /// </summary>
    public sealed partial class AdvancedDataGrid
    {
        #region Private Fields - Core Infrastructure

        // ✅ CORE: DataGridController - centrálny koordinátor services
        private Core.DataGridController? _controller;
        private Core.DataGridConfiguration? _configuration;

        // ✅ CORE: Service instances pre modularitu
        private Services.UI.DataGridCoreService? _coreService;
        private Services.UI.DataGridLayoutService? _layoutService;
        private Services.UI.DataGridEventService? _eventService;
        private Services.Operations.DataGridBindingService? _bindingService;
        
        private IServiceProvider? _serviceProvider;
        private IDataManagementService? _dataManagementService;
        private IValidationService? _validationService;
        private IExportService? _exportService;

        // ✅ CORE: State management
        private bool _isInitialized = false;
        private bool _isDisposed = false;

        // ✅ CORE: Auto-Add fields
        private int _unifiedRowCount = 15;
        private bool _autoAddEnabled = true;

        // ✅ CORE: Individual colors namiesto themes
        private DataGridColorConfig? _individualColorConfig;

        // ✅ CORE: Service instances
        private SearchAndSortService? _searchAndSortService;
        private NavigationService? _navigationService;
        private RowHeightAnimationService? _rowHeightAnimationService;
        private CopyPasteService? _copyPasteService;
        private VirtualScrollingService? _virtualScrollingService;
        private BatchValidationService? _batchValidationService;
        private AdvancedSearchService? _advancedSearchService;

        // ✅ CORE: State management objects
        private readonly CellSelectionState _cellSelectionState = new();
        private readonly DragSelectionState _dragSelectionState = new();
        private Models.Validation.ValidationRuleSet? _advancedValidationRules;

        // ✅ CORE: Internal data pre AUTO-ADD a UI binding
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();
        private readonly ObservableCollection<DataRowViewModel> _displayRows = new();

        // ✅ CORE: Search & Sort state tracking
        private readonly Dictionary<string, string> _columnSearchFilters = new();

        // ✅ CORE: ROZŠÍRENÉ LOGOVANIE - NEZÁVISLÉ LOGOVANIE s ILogger abstractions
        private readonly ILogger _logger;
        private readonly string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];

        // ✅ NOVÉ: Performance metrics tracking
        private long _lastValidationDurationMs = 0;

        // ✅ CORE: Performance tracking s rozšírenými metrikami
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private readonly Dictionary<string, double> _operationDurations = new();

        // ✅ CORE: Realtime validácia fields
        private readonly Dictionary<string, System.Threading.Timer> _validationTimers = new();
        private GridThrottlingConfig? _throttlingConfig;
        private readonly object _validationLock = new object();

        // ✅ CORE: Column resize functionality
        private readonly List<ResizableColumnHeader> _resizableHeaders = new();
        private bool _isResizing = false;
        private ResizableColumnHeader? _currentResizingHeader;
        private double _resizeStartPosition;
        private double _resizeStartWidth;

        // ✅ CORE: Scroll synchronization
        private bool _isScrollSynchronizing = false;

        // ✅ CORE: Layout management
        private double _totalAvailableWidth = 0;
        private double _validAlertsMinWidth = 200;

        // ✅ CORE: UI State tracking pre detailné logovanie
        private readonly Dictionary<string, object?> _uiStateSnapshot = new();
        private int _totalCellsRendered = 0;
        private int _totalValidationErrors = 0;
        private DateTime _lastDataUpdate = DateTime.MinValue;

        // ✅ CORE: CheckBox Column Support
        private bool _checkBoxColumnEnabled = false;
        private string _checkBoxColumnName = "CheckBoxState";
        private readonly Dictionary<int, bool> _checkBoxStates = new();
        private CheckBoxColumnHeader? _checkBoxColumnHeader = null;
        private readonly CheckBoxStateManager _checkBoxStateManager;

        #endregion

        #region Core Properties - Protected Access for Partial Classes

        /// <summary>
        /// Protected access to logger for all partial classes
        /// </summary>
        protected ILogger Logger => _logger;

        /// <summary>
        /// Protected access to component instance ID
        /// </summary>
        protected string ComponentInstanceId => _componentInstanceId;

        /// <summary>
        /// Protected access to initialization state
        /// </summary>
        protected bool IsInitialized => _isInitialized;

        /// <summary>
        /// Protected access to disposal state
        /// </summary>
        protected bool IsDisposed => _isDisposed;

        /// <summary>
        /// Protected access to grid data
        /// </summary>
        protected List<Dictionary<string, object?>> GridData => _gridData;

        /// <summary>
        /// Protected access to columns
        /// </summary>
        protected List<GridColumnDefinition> Columns => _columns;

        /// <summary>
        /// Protected access to display rows
        /// </summary>
        protected ObservableCollection<DataRowViewModel> InternalDisplayRows => _displayRows;

        /// <summary>
        /// Internal access to cell selection state
        /// </summary>
        internal CellSelectionState CellSelectionState => _cellSelectionState;

        /// <summary>
        /// Internal access to drag selection state
        /// </summary>
        internal DragSelectionState DragSelectionState => _dragSelectionState;

        #endregion

        #region Core Constructors

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
                // ✅ ROZŠÍRENÉ: Použije poskytnutý logger alebo NullLogger
                _logger = logger ?? NullLogger.Instance;

                _logger.LogInformation("🔧 AdvancedDataGrid Constructor START - Instance: {ComponentInstanceId}, LoggerType: {LoggerType}",
                    _componentInstanceId, _logger.GetType().Name);
                StartOperation("Constructor");

                // ✅ ROZŠÍRENÉ: Log system info
                LogSystemInfo();

                // ✅ KRITICKÉ: InitializeComponent pre UserControl - automaticky generované z XAML
                this.InitializeComponent();
                _logger.LogDebug("✅ Constructor - XAML successfully loaded");

                // ✅ NOVÉ: Inicializuj CheckBoxStateManager
                _checkBoxStateManager = new CheckBoxStateManager(_logger, _componentInstanceId);

                // ✅ NOVÉ: Inicializácia cez services (async)
                _ = Task.Run(async () => await InitializeServicesAsync());
                InitializeController(); // ✅ NOVÉ: Inicializácia DataGridController
                InitializeDependencyInjection();
                InitializePerformanceTracking();

                _logger.LogInformation("✅ Constructor - Complete initialization with resize, scroll, stretch");

                this.DataContext = this;
                var duration = EndOperation("Constructor");

                // ✅ ROZŠÍRENÉ: Detailný súhrn inicializácie
                _logger.LogInformation("✅ Constructor COMPLETED - Instance: {ComponentInstanceId}, Duration: {Duration}ms, " +
                    "Features: Resize+Scroll+Stretch+Zebra+Validation, LoggerActive: {LoggerActive}",
                    _componentInstanceId, duration, !(_logger is NullLogger));

                LogComponentState("Constructor-End");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ CRITICAL CONSTRUCTOR ERROR - Instance: {ComponentInstanceId}, " +
                    "LoggerType: {LoggerType}", _componentInstanceId, _logger?.GetType().Name ?? "Unknown");
                throw;
            }
        }

        /// <summary>
        /// ObservableCollection pre UI binding - PUBLIC pre x:Bind
        /// </summary>
        public ObservableCollection<DataRowViewModel> DisplayRows => _displayRows;

        #endregion

        #region Core Performance Tracking - Protected Methods

        /// <summary>
        /// Internal method to start operation tracking
        /// </summary>
        internal string StartOperation(string operationName)
        {
            try
            {
                _operationStartTimes[operationName] = DateTime.UtcNow;
                _operationCounters[operationName] = _operationCounters.GetValueOrDefault(operationName, 0) + 1;
                
                _logger.LogTrace("⏱️ Operation START - {Operation} (#{Count})", operationName, _operationCounters[operationName]);
                return operationName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error starting operation tracking - {Operation}", operationName);
                return operationName;
            }
        }

        /// <summary>
        /// Internal method to end operation tracking
        /// </summary>
        internal double EndOperation(string operationName)
        {
            try
            {
                if (_operationStartTimes.TryGetValue(operationName, out var startTime))
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _operationDurations[operationName] = duration;
                    
                    _logger.LogTrace("✅ Operation END - {Operation}, Duration: {Duration}ms", operationName, duration);
                    return duration;
                }
                else
                {
                    _logger.LogWarning("⚠️ Operation end without start - {Operation}", operationName);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error ending operation tracking - {Operation}", operationName);
                return 0;
            }
        }

        /// <summary>
        /// Internal method to log system information
        /// </summary>
        internal void LogSystemInfo()
        {
            try
            {
                _logger.LogDebug("🖥️ System Info - OS: {OS}, .NET: {Runtime}, Memory: {Memory}MB, " +
                    "Culture: {Culture}, UI Thread: {UIThread}",
                    Environment.OSVersion.VersionString,
                    Environment.Version,
                    GC.GetTotalMemory(false) / 1024 / 1024,
                    System.Globalization.CultureInfo.CurrentUICulture.Name,
                    Environment.CurrentManagedThreadId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error logging system info");
            }
        }

        /// <summary>
        /// Internal method to log component state
        /// </summary>
        internal void LogComponentState(string checkpoint)
        {
            try
            {
                _logger.LogDebug("📊 Component State [{Checkpoint}] - Instance: {ComponentInstanceId}, " +
                    "Initialized: {Initialized}, Rows: {RowCount}, Columns: {ColumnCount}, " +
                    "CellsRendered: {CellsRendered}, ValidationErrors: {ValidationErrors}",
                    checkpoint, _componentInstanceId, _isInitialized, _displayRows.Count, _columns.Count,
                    _totalCellsRendered, _totalValidationErrors);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error logging component state - {Checkpoint}", checkpoint);
            }
        }

        #endregion

        #region Core Properties - Public API

        /// <summary>
        /// Public property for unified row count
        /// </summary>
        public int UnifiedRowCount
        {
            get => _unifiedRowCount;
            set
            {
                if (_unifiedRowCount != value)
                {
                    _unifiedRowCount = value;
                    OnPropertyChanged();
                    _logger.LogDebug("📝 UnifiedRowCount changed to {Value}", value);
                }
            }
        }

        /// <summary>
        /// Public property for auto-add enabled state
        /// </summary>
        public bool AutoAddEnabled
        {
            get => _autoAddEnabled;
            set
            {
                if (_autoAddEnabled != value)
                {
                    _autoAddEnabled = value;
                    OnPropertyChanged();
                    _logger.LogDebug("📝 AutoAddEnabled changed to {Value}", value);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// PropertyChanged event pre data binding
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Internal method to raise PropertyChanged event
        /// </summary>
        internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                _logger.LogTrace("🔔 PropertyChanged - {PropertyName}", propertyName ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error raising PropertyChanged - {PropertyName}", propertyName ?? "Unknown");
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose method - cleanup resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                _logger.LogInformation("🧹 AdvancedDataGrid.Dispose START - Instance: {ComponentInstanceId}", _componentInstanceId);
                StartOperation("Dispose");

                // Dispose all services
                DisposeServices();

                // Cleanup validation timers
                DisposeValidationTimers();

                // Clear collections
                _gridData.Clear();
                _columns.Clear();
                _displayRows.Clear();
                _columnSearchFilters.Clear();
                _operationStartTimes.Clear();
                _operationCounters.Clear();
                _operationDurations.Clear();
                _uiStateSnapshot.Clear();
                _checkBoxStates.Clear();

                _isDisposed = true;
                var duration = EndOperation("Dispose");

                _logger.LogInformation("✅ AdvancedDataGrid.Dispose COMPLETED - Instance: {ComponentInstanceId}, Duration: {Duration}ms", 
                    _componentInstanceId, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during dispose - Instance: {ComponentInstanceId}", _componentInstanceId);
            }
        }

        /// <summary>
        /// Internal method to dispose services
        /// </summary>
        internal void DisposeServices()
        {
            try
            {
                _searchAndSortService?.Dispose();
                // _navigationService?.Dispose(); // TODO: NavigationService does not implement IDisposable yet
                _rowHeightAnimationService?.Dispose();
                // _copyPasteService?.Dispose(); // TODO: CopyPasteService does not implement IDisposable yet  
                _virtualScrollingService?.Dispose();
                _batchValidationService?.Dispose();
                _advancedSearchService?.Dispose();
                // _controller?.Dispose(); // TODO: DataGridController does not implement IDisposable yet

                _logger.LogDebug("✅ All services disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing services");
            }
        }

        /// <summary>
        /// Internal method to dispose validation timers
        /// </summary>
        internal void DisposeValidationTimers()
        {
            try
            {
                foreach (var timer in _validationTimers.Values)
                {
                    timer?.Dispose();
                }
                _validationTimers.Clear();

                _logger.LogDebug("✅ All validation timers disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing validation timers");
            }
        }

        #endregion
    }
}