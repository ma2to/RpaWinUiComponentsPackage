// Controls/AdvancedDataGrid.xaml.cs - ✅ ROZŠÍRENÉ LOGOVANIE pre debugging
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
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.Common.SharedUtilities.Extensions;
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
    /// ✅ ROZŠÍRENÉ LOGOVANIE - Detailné logy pre debugging a monitoring
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

        // ✅ ROZŠÍRENÉ LOGOVANIE: NEZÁVISLÉ LOGOVANIE s ILogger abstractions
        private readonly ILogger _logger;
        private readonly string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];

        // Performance tracking s rozšírenými metrikami
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private readonly Dictionary<string, double> _operationDurations = new();

        // Realtime validácia fields
        private readonly Dictionary<string, System.Threading.Timer> _validationTimers = new();
        private GridThrottlingConfig? _throttlingConfig;
        private readonly object _validationLock = new object();

        // ✅ Column resize functionality
        private readonly List<ResizableColumnHeader> _resizableHeaders = new();
        private bool _isResizing = false;
        private ResizableColumnHeader? _currentResizingHeader;
        private double _resizeStartPosition;
        private double _resizeStartWidth;

        // ✅ Scroll synchronization
        private bool _isScrollSynchronizing = false;

        // ✅ Layout management
        private double _totalAvailableWidth = 0;
        private double _validAlertsMinWidth = 200;

        // ✅ NOVÉ: UI State tracking pre detailné logovanie
        private readonly Dictionary<string, object?> _uiStateSnapshot = new();
        private int _totalCellsRendered = 0;
        private int _totalValidationErrors = 0;
        private DateTime _lastDataUpdate = DateTime.MinValue;

        #endregion

        #region ✅ ROZŠÍRENÉ: Constructors s kompletným logovaním inicializácie

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

                // ✅ ROZŠÍRENÉ: Detailná inicializácia s logovaním každého kroku
                InitializeDependencyInjection();
                InitializeResizeSupport();
                InitializeScrollSupport();
                InitializeLayoutManagement();
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
                _logger.LogError(ex, "❌ CRITICAL CONSTRUCTOR ERROR - Instance: {ComponentInstanceId}, " +
                    "LoggerType: {LoggerType}", _componentInstanceId, _logger.GetType().Name);
                throw;
            }
        }

        /// <summary>
        /// ObservableCollection pre UI binding - PUBLIC pre x:Bind
        /// </summary>
        public ObservableCollection<DataRowViewModel> DisplayRows => _displayRows;

        #endregion

        #region ✅ ROZŠÍRENÉ: PUBLIC API Methods s kompletným logovaním a metrics

        /// <summary>
        /// InitializeAsync s realtime validáciou - PUBLIC API
        /// ✅ ROZŠÍRENÉ LOGOVANIE: Detailné sledovanie každého kroku inicializácie
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
                _logger.LogInformation("🚀 InitializeAsync START - Instance: {ComponentInstanceId}, " +
                    "Columns: {ColumnCount}, Rules: {RuleCount}, EmptyRows: {EmptyRows}, HasColors: {HasColors}",
                    _componentInstanceId, columns?.Count ?? 0, validationRules?.Count ?? 0, emptyRowsCount,
                    colorConfig?.HasAnyCustomColors ?? false);

                StartOperation("InitializeAsync");
                IncrementOperationCounter("InitializeAsync");

                if (columns == null || columns.Count == 0)
                {
                    _logger.LogError("❌ InitializeAsync: Columns parameter is null or empty - Instance: {ComponentInstanceId}",
                        _componentInstanceId);
                    throw new ArgumentException("Columns parameter cannot be null or empty", nameof(columns));
                }

                // ✅ ROZŠÍRENÉ: Detailné logovanie štruktúry stĺpcov
                LogColumnStructure(columns);

                // ✅ ROZŠÍRENÉ: Detailné logovanie validačných pravidiel
                LogValidationRules(validationRules);

                // Store throttling config pre realtime validáciu
                _throttlingConfig = throttlingConfig?.Clone() ?? GridThrottlingConfig.Default;
                _logger.LogDebug("⚙️ Throttling config stored - ValidationDebounce: {ValidationMs}ms, " +
                    "UIUpdate: {UIMs}ms, Search: {SearchMs}ms, RealtimeValidation: {RealtimeEnabled}",
                    _throttlingConfig.ValidationDebounceMs, _throttlingConfig.UIUpdateDebounceMs,
                    _throttlingConfig.SearchDebounceMs, _throttlingConfig.EnableRealtimeValidation);

                // Store configuration
                _columns.Clear();
                _columns.AddRange(columns);
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;
                _individualColorConfig = colorConfig?.Clone();

                // ✅ ROZŠÍRENÉ: Detailné logovanie color config
                LogColorConfiguration(colorConfig);

                // Initialize services
                await InitializeServicesAsync(columns, validationRules ?? new List<GridValidationRule>(), _throttlingConfig, emptyRowsCount);

                // ✅ UI setup s resize, scroll a stretch funkcionalitou
                ApplyIndividualColorsToUI();
                InitializeSearchSortZebra();
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
        /// ExportToDataTableAsync s detailným logovaním exportu
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync()
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

                var result = await _exportService.ExportToDataTableAsync();
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

        #region ✅ ROZŠÍRENÉ: Detailed Logging Helper Methods

        /// <summary>
        /// Loguje informácie o systéme pri inicializácii
        /// </summary>
        private void LogSystemInfo()
        {
            try
            {
                var osInfo = Environment.OSVersion;
                var processorCount = Environment.ProcessorCount;
                var workingSet = Environment.WorkingSet;

                _logger.LogDebug("🖥️ System Info - OS: {OSVersion}, Processors: {ProcessorCount}, " +
                    "WorkingSet: {WorkingSet} bytes", osInfo, processorCount, workingSet);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log system info");
            }
        }

        /// <summary>
        /// Loguje detailnú štruktúru stĺpcov
        /// </summary>
        private void LogColumnStructure(List<GridColumnDefinition> columns)
        {
            try
            {
                _logger.LogDebug("📋 Column Structure Analysis - TotalColumns: {TotalCount}", columns.Count);

                var columnDetails = columns.Select((col, index) => new
                {
                    Index = index,
                    Name = col.Name,
                    Type = col.DataType.Name,
                    Width = col.Width,
                    MinWidth = col.MinWidth,
                    IsVisible = col.IsVisible,
                    IsEditable = col.IsEditable,
                    IsSpecial = col.IsSpecialColumn
                }).ToList();

                foreach (var col in columnDetails)
                {
                    _logger.LogDebug("📊 Column[{Index}]: {Name} ({Type}) - Width: {Width}, " +
                        "Visible: {IsVisible}, Editable: {IsEditable}, Special: {IsSpecial}",
                        col.Index, col.Name, col.Type, col.Width, col.IsVisible, col.IsEditable, col.IsSpecial);
                }

                var specialColumns = columnDetails.Where(c => c.IsSpecial).ToList();
                var editableColumns = columnDetails.Where(c => c.IsEditable).ToList();
                var totalWidth = columnDetails.Where(c => c.IsVisible).Sum(c => c.Width);

                _logger.LogInformation("📊 Column Summary - Total: {Total}, Special: {Special}, " +
                    "Editable: {Editable}, TotalWidth: {TotalWidth}px",
                    columns.Count, specialColumns.Count, editableColumns.Count, totalWidth);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log column structure");
            }
        }

        /// <summary>
        /// Loguje validačné pravidlá
        /// </summary>
        private void LogValidationRules(List<GridValidationRule>? validationRules)
        {
            try
            {
                if (validationRules == null || !validationRules.Any())
                {
                    _logger.LogDebug("📋 No validation rules provided");
                    return;
                }

                _logger.LogDebug("🔍 Validation Rules Analysis - TotalRules: {TotalCount}", validationRules.Count);

                var rulesByColumn = validationRules.GroupBy(r => r.ColumnName).ToList();
                foreach (var group in rulesByColumn)
                {
                    var rules = group.ToList();
                    var ruleTypes = string.Join(", ", rules.Select(r => r.Type.ToString()));
                    _logger.LogDebug("🔍 Column '{Column}': {RuleCount} rules ({RuleTypes})",
                        group.Key, rules.Count, ruleTypes);
                }

                var ruleTypeCounts = validationRules.GroupBy(r => r.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                _logger.LogInformation("🔍 Validation Summary - TotalRules: {Total}, " +
                    "RuleDistribution: {RuleDistribution}",
                    validationRules.Count, string.Join(", ", ruleTypeCounts.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log validation rules");
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
                    _logger.LogDebug("🎨 No custom color configuration provided - using defaults");
                    return;
                }

                _logger.LogDebug("🎨 Color Configuration - HasCustomColors: {HasColors}, " +
                    "CustomColorCount: {CustomCount}, ZebraEnabled: {ZebraEnabled}",
                    colorConfig.HasAnyCustomColors, colorConfig.CustomColorsCount, colorConfig.IsZebraRowsEnabled);

                if (colorConfig.HasAnyCustomColors)
                {
                    var colorDetails = new List<string>();
                    if (colorConfig.CellBackgroundColor.HasValue) colorDetails.Add("CellBackground");
                    if (colorConfig.HeaderBackgroundColor.HasValue) colorDetails.Add("HeaderBackground");
                    if (colorConfig.ValidationErrorColor.HasValue) colorDetails.Add("ValidationError");
                    if (colorConfig.AlternateRowColor.HasValue) colorDetails.Add("ZebraRows");

                    _logger.LogDebug("🎨 Custom Colors: {CustomColors}", string.Join(", ", colorDetails));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log color configuration");
            }
        }

        /// <summary>
        /// Analyza prichádzajúcich dát s detailným logovaním
        /// </summary>
        private async Task LogDataAnalysis(List<Dictionary<string, object?>> data)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (!data.Any())
                    {
                        _logger.LogDebug("📊 Data Analysis - Empty dataset provided");
                        return;
                    }

                    _logger.LogDebug("📊 Data Analysis START - RowCount: {RowCount}", data.Count);

                    // Analýza štruktúry dát
                    var firstRow = data.First();
                    var columnNames = firstRow.Keys.ToList();

                    _logger.LogDebug("📊 Data Structure - Columns: {ColumnNames}",
                        string.Join(", ", columnNames));

                    // Sample dát
                    var sampleData = firstRow.Take(5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    _logger.LogDebug("📊 Sample Data: {SampleData}",
                        string.Join(", ", sampleData.Select(kvp => $"{kvp.Key}='{kvp.Value}'")));

                    // Analýza typov hodnôt
                    var typeAnalysis = new Dictionary<string, Dictionary<string, int>>();
                    foreach (var row in data.Take(10)) // Analyzuj prvých 10 riadkov
                    {
                        foreach (var kvp in row)
                        {
                            if (!typeAnalysis.ContainsKey(kvp.Key))
                                typeAnalysis[kvp.Key] = new Dictionary<string, int>();

                            var typeName = kvp.Value?.GetType().Name ?? "null";
                            if (!typeAnalysis[kvp.Key].ContainsKey(typeName))
                                typeAnalysis[kvp.Key][typeName] = 0;

                            typeAnalysis[kvp.Key][typeName]++;
                        }
                    }

                    foreach (var column in typeAnalysis)
                    {
                        var typeDistribution = string.Join(", ",
                            column.Value.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                        _logger.LogDebug("📊 Column '{Column}' types: {TypeDistribution}",
                            column.Key, typeDistribution);
                    }

                    // Kontrola na prázdne/null hodnoty
                    var nullCounts = new Dictionary<string, int>();
                    foreach (var row in data)
                    {
                        foreach (var kvp in row)
                        {
                            if (kvp.Value == null || string.IsNullOrWhiteSpace(kvp.Value.ToString()))
                            {
                                if (!nullCounts.ContainsKey(kvp.Key))
                                    nullCounts[kvp.Key] = 0;
                                nullCounts[kvp.Key]++;
                            }
                        }
                    }

                    if (nullCounts.Any())
                    {
                        _logger.LogDebug("📊 Null/Empty Values: {NullCounts}",
                            string.Join(", ", nullCounts.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
                    }

                    _logger.LogInformation("📊 Data Analysis COMPLETED - Rows: {RowCount}, " +
                        "Columns: {ColumnCount}, NullColumns: {NullColumnCount}",
                        data.Count, columnNames.Count, nullCounts.Count);
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not complete data analysis");
            }
        }

        /// <summary>
        /// Loguje súhrn načítania dát
        /// </summary>
        private void LogDataLoadSummary(List<Dictionary<string, object?>> data, double duration)
        {
            try
            {
                var dataRowCount = data?.Count ?? 0;
                var emptyRowCount = _displayRows.Count - dataRowCount;
                var avgTimePerRow = dataRowCount > 0 ? duration / dataRowCount : 0;

                _logger.LogInformation("📊 LoadData Summary - DataRows: {DataRows}, EmptyRows: {EmptyRows}, " +
                    "TotalDisplayRows: {TotalRows}, Duration: {Duration}ms, AvgTimePerRow: {AvgTime:F2}ms",
                    dataRowCount, emptyRowCount, _displayRows.Count, duration, avgTimePerRow);

                // Memory info
                var memoryBefore = GC.GetTotalMemory(false);
                GC.Collect();
                var memoryAfter = GC.GetTotalMemory(true);
                var memoryUsed = Math.Max(0, memoryBefore - memoryAfter);

                _logger.LogDebug("🧠 Memory Impact - Before: {MemoryBefore} bytes, " +
                    "After GC: {MemoryAfter} bytes, Freed: {MemoryFreed} bytes",
                    memoryBefore, memoryAfter, memoryUsed);
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
                var validRowCount = _displayRows.Count(r => r.Cells.All(c => c.IsValid));
                var invalidRowCount = _displayRows.Count - validRowCount;
                var totalCells = _displayRows.Sum(r => r.Cells.Count);
                var invalidCells = _displayRows.SelectMany(r => r.Cells).Count(c => !c.IsValid);

                _totalValidationErrors = invalidCells;

                _logger.LogInformation("🔍 Validation Results - Overall: {Result}, Duration: {Duration}ms, " +
                    "ValidRows: {ValidRows}, InvalidRows: {InvalidRows}, InvalidCells: {InvalidCells}/{TotalCells}",
                    result ? "PASS" : "FAIL", duration, validRowCount, invalidRowCount, invalidCells, totalCells);

                if (invalidCells > 0)
                {
                    // Log top validation errors
                    var errorSummary = _displayRows
                        .SelectMany(r => r.Cells)
                        .Where(c => !c.IsValid && !string.IsNullOrEmpty(c.ValidationErrors))
                        .GroupBy(c => c.ValidationErrors)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => $"{g.Key}({g.Count()})")
                        .ToList();

                    if (errorSummary.Any())
                    {
                        _logger.LogWarning("⚠️ Top Validation Errors: {ErrorSummary}",
                            string.Join(", ", errorSummary));
                    }
                }
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
                var totalCells = result.Rows.Count * result.Columns.Count;
                var avgTimePerRow = result.Rows.Count > 0 ? duration / result.Rows.Count : 0;
                var estimatedSizeKB = (totalCells * 50) / 1024; // Rough estimate

                _logger.LogInformation("📤 Export Results - Rows: {Rows}, Columns: {Columns}, " +
                    "TotalCells: {TotalCells}, Duration: {Duration}ms, AvgTimePerRow: {AvgTime:F2}ms, " +
                    "EstimatedSize: {EstimatedSize}KB",
                    result.Rows.Count, result.Columns.Count, totalCells, duration, avgTimePerRow, estimatedSizeKB);

                // Column info
                var columnInfo = result.Columns.Cast<DataColumn>()
                    .Select(c => $"{c.ColumnName}({c.DataType.Name})")
                    .ToList();

                _logger.LogDebug("📤 Exported Columns: {ColumnInfo}", string.Join(", ", columnInfo));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log export results");
            }
        }

        /// <summary>
        /// Loguje stav komponenty
        /// </summary>
        private void LogComponentState(string context)
        {
            try
            {
                var state = new
                {
                    IsInitialized = _isInitialized,
                    ColumnCount = _columns.Count,
                    DisplayRowCount = _displayRows.Count,
                    IsResizing = _isResizing,
                    AutoAddEnabled = _autoAddEnabled,
                    TotalWidth = _totalAvailableWidth,
                    HasValidationErrors = _totalValidationErrors > 0,
                    LastDataUpdate = _lastDataUpdate,
                    TotalOperations = _operationCounters.Sum(kvp => kvp.Value)
                };

                _logger.LogDebug("🔍 Component State [{Context}] - {ComponentState}",
                    context, state);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log component state for context: {Context}", context);
            }
        }

        /// <summary>
        /// Loguje súhrn inicializácie
        /// </summary>
        private void LogInitializationSummary(double duration)
        {
            try
            {
                var enabledFeatures = GetEnabledFeatures();
                var memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024; // MB

                _logger.LogInformation("✅ Initialization Summary - Duration: {Duration}ms, " +
                    "EnabledFeatures: {Features}, MemoryUsage: {Memory}MB, " +
                    "TotalColumns: {Columns}, MinRows: {MinRows}",
                    duration, enabledFeatures, memoryUsage, _columns.Count, _unifiedRowCount);

                var performanceMetrics = new
                {
                    InitializationTime = duration,
                    ColumnsPerSecond = _columns.Count / (duration / 1000),
                    ComponentsInitialized = GetInitializedComponentsCount(),
                    MemoryFootprintMB = memoryUsage
                };

                _logger.LogDebug("📊 Performance Metrics: {Metrics}", performanceMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log initialization summary");
            }
        }

        #endregion

        #region ✅ ROZŠÍRENÉ: Performance Tracking Methods

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
        /// Získa počet inicializovaných komponentov
        /// </summary>
        private int GetInitializedComponentsCount()
        {
            var count = 0;
            if (_dataManagementService != null) count++;
            if (_validationService != null) count++;
            if (_exportService != null) count++;
            if (_searchAndSortService != null) count++;
            return count;
        }

        /// <summary>
        /// Získa celkový počet validačných pravidiel
        /// </summary>
        private int GetTotalValidationRulesCount()
        {
            try
            {
                return _validationService?.ValidationErrors?.Sum(kvp => kvp.Value.Count) ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Existing methods (skrátené pre úsporu miesta - ponechané iba signatures)

        // ✅ Column Resize Implementation
        private void InitializeResizeSupport() { /* implementation exists */ }
        private async Task CreateResizableHeadersAsync() { /* implementation exists */ }
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e) { /* implementation exists */ }
        private void OnPointerMoved(object sender, PointerRoutedEventArgs e) { /* implementation exists */ }
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e) { /* implementation exists */ }
        private void OnPointerCaptureLost(object sender, PointerEventArgs e) { /* implementation exists */ }

        // ✅ ValidAlerts Stretching Implementation  
        private void SetupValidAlertsStretching() { /* implementation exists */ }
        private async Task RecalculateValidAlertsWidthAsync() { /* implementation exists */ }

        // ✅ Scroll Support Implementation
        private void InitializeScrollSupport() { /* implementation exists */ }
        private void SetupScrollSynchronization() { /* implementation exists */ }
        private void OnDataScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) { /* implementation exists */ }
        private void OnHeaderScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) { /* implementation exists */ }

        // ✅ Layout Management
        private void InitializeLayoutManagement() { /* implementation exists */ }
        private async Task UpdateLayoutAfterDataChangeAsync() { /* implementation exists */ }

        // ✅ Data Display Implementation
        private async Task RefreshDataDisplayAsync() { /* implementation exists */ }
        private async Task UpdateDisplayRowsWithRealtimeValidationAsync() { /* implementation exists */ }
        private async Task UpdateUIVisibilityAsync() { /* implementation exists */ }

        // ✅ Helper Methods
        private void EnsureInitialized() { /* implementation exists */ }
        private void InitializeDependencyInjection() { /* implementation exists */ }
        private void ApplyIndividualColorsToUI() { /* implementation exists */ }
        private void InitializeSearchSortZebra() { /* implementation exists */ }
        private async Task CreateInitialEmptyRowsAsync() { /* implementation exists */ }
        private async Task InitializeServicesAsync(List<GridColumnDefinition> columns, List<GridValidationRule> rules, GridThrottlingConfig throttling, int emptyRows) { /* implementation exists */ }

        // ✅ Event Handlers
        public void OnHideValidationOverlayClick(object sender, RoutedEventArgs e) { /* implementation exists */ }

        // ✅ Performance Tracking (existing)
        private void StartOperation(string operationName)
        {
            _operationStartTimes[operationName] = DateTime.UtcNow;
            _logger.LogTrace("⏱️ Operation START: {OperationName}", operationName);
        }

        private double EndOperation(string operationName)
        {
            if (_operationStartTimes.TryGetValue(operationName, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _operationStartTimes.Remove(operationName);
                var roundedDuration = Math.Round(duration, 2);

                // Store duration for analytics
                _operationDurations[operationName] = roundedDuration;

                _logger.LogTrace("⏱️ Operation END: {OperationName} - {Duration}ms", operationName, roundedDuration);
                return roundedDuration;
            }
            return 0;
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

        // ✅ XAML element access properties
        private StackPanel? HeaderStackPanel => this.FindName("HeaderStackPanel") as StackPanel;
        private ScrollViewer? HeaderScrollViewer => this.FindName("HeaderScrollViewer") as ScrollViewer;
        private ScrollViewer? DataGridScrollViewer => this.FindName("DataGridScrollViewer") as ScrollViewer;
        private ItemsControl? DataRowsContainer => this.FindName("DataRowsContainer") as ItemsControl;
        private Grid? MainContentGrid => this.FindName("MainContentGrid") as Grid;
        private Border? LoadingOverlay => this.FindName("LoadingOverlay") as Border;

        #endregion

        #region INotifyPropertyChanged & IDisposable

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
                _logger.LogInformation("🧹 AdvancedDataGrid DISPOSE START - Instance: {InstanceId}, " +
                    "TotalOperations: {TotalOps}, LastUpdate: {LastUpdate}",
                    _componentInstanceId, _operationCounters.Sum(kvp => kvp.Value), _lastDataUpdate);

                // Log final performance summary
                LogFinalPerformanceSummary();

                // Dispose resources
                DisposeResources();

                _isDisposed = true;
                _logger.LogInformation("✅ AdvancedDataGrid DISPOSED successfully - Instance: {InstanceId}",
                    _componentInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during dispose - Instance: {InstanceId}", _componentInstanceId);
            }
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error clearing collections");
            }
        }

        #endregion

        #region Additional Helper Classes (existing)

        // Keep existing helper classes unchanged
        internal class TypedLoggerWrapper<T> : ILogger<T> { /* existing implementation */ }
        internal class ResizableColumnHeader { /* existing implementation */ }
        public class DataRowViewModel : INotifyPropertyChanged { /* existing implementation */ }
        public class CellViewModel : INotifyPropertyChanged { /* existing implementation */ }

        #endregion
    }
}