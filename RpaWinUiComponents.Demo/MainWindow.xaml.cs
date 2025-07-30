// Controls/AdvancedDataGrid.xaml.cs - ✅ ROZŠÍRENÉ LOGOVANIE pre troubleshooting
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions; // ✅ IBA Abstractions
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.Logger;  // ✅ LoggerComponent integrácia
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
// ✅ Aliasy pre zamedzenie konfliktov s WinUI typmi
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid s KOMPLETNOU LoggerComponent integráciou + ROZŠÍRENÝM LOGOVANÍM - ✅ PUBLIC API
    /// Balík je nezávislý na logging systéme - používa iba Abstractions + LoggerComponent
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

        // ✅ AUTO-ADD: Unified row count
        private int _unifiedRowCount = 15;
        private bool _autoAddEnabled = true;

        // ✅ Individual colors namiesto themes
        private DataGridColorConfig? _individualColorConfig;

        // ✅ SearchAndSortService s PUBLIC SortDirection typom
        private SearchAndSortService? _searchAndSortService;

        // ✅ Interné dáta pre AUTO-ADD a UI binding
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();
        private readonly ObservableCollection<DataRowViewModel> _displayRows = new();

        // ✅ Search & Sort state tracking s PUBLIC SortDirection typom
        private readonly Dictionary<string, string> _columnSearchFilters = new();

        // ✅ LoggerComponent integrácia
        private LoggerComponent? _integratedLogger;
        private bool _loggerIntegrationEnabled = false;
        private string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];

        // ✅ NOVÉ: Performance tracking pre debug
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();

        #endregion

        #region ✅ Constructor s ROZŠÍRENÝM error handling a logovaním

        public AdvancedDataGrid()
        {
            try
            {
                LogDebug("🔧 AdvancedDataGrid Constructor START - Instance: {InstanceId}", _componentInstanceId);
                LogDebug("📊 Constructor - Memory before: {Memory} MB", GC.GetTotalMemory(false) / 1024 / 1024);

                StartOperation("Constructor");

                // ✅ XAML loading s detailným logovaním
                TryInitializeXaml();

                if (!_xamlLoadFailed)
                {
                    LogDebug("✅ Constructor - XAML úspešne načítané");
                    InitializeDependencyInjection();
                    LogInfo("✅ Constructor - Kompletne inicializovaný s LoggerComponent support");
                    UpdateUIVisibility();
                }
                else
                {
                    LogWarn("⚠️ Constructor - XAML loading zlyhal, vytváram fallback UI");
                    CreateSimpleFallbackUI();
                }

                // ✅ UI binding
                DataContext = this;

                LogDebug("📊 Constructor - Memory after: {Memory} MB", GC.GetTotalMemory(false) / 1024 / 1024);
                LogInfo("✅ Constructor COMPLETED - Instance: {InstanceId}, Duration: {Duration}ms",
                    _componentInstanceId, EndOperation("Constructor"));
            }
            catch (Exception ex)
            {
                LogError("❌ CRITICAL CONSTRUCTOR ERROR: {Error}", ex.Message);
                LogError("❌ Constructor Exception Details: {Exception}", ex);
                CreateSimpleFallbackUI();
            }
        }

        /// <summary>
        /// ✅ ObservableCollection pre UI binding - PUBLIC pre x:Bind
        /// </summary>
        public ObservableCollection<DataRowViewModel> DisplayRows => _displayRows;

        /// <summary>
        /// ✅ ROZŠÍRENÉ XAML loading s detailným logovaním
        /// </summary>
        private void TryInitializeXaml()
        {
            try
            {
                LogDebug("🔧 XAML Loading START - checking InitializeComponent()...");
                StartOperation("XamlLoading");

                this.InitializeComponent();
                _xamlLoadFailed = false;

                LogDebug("✅ XAML Loading - InitializeComponent() SUCCESS");

                // ✅ Okamžite validuj UI elementy
                ValidateUIElementsAfterXaml();

                LogDebug("✅ XAML Loading COMPLETED - Duration: {Duration}ms", EndOperation("XamlLoading"));
            }
            catch (Exception xamlEx)
            {
                EndOperation("XamlLoading");
                LogError("❌ XAML LOADING FAILED: {Error}", xamlEx.Message);
                LogError("❌ XAML Exception Details: {Exception}", xamlEx);
                LogError("❌ XAML StackTrace: {StackTrace}", xamlEx.StackTrace);
                _xamlLoadFailed = true;
            }
        }

        /// <summary>
        /// ✅ Validácia UI elementov s detailným reportingom
        /// </summary>
        private void ValidateUIElementsAfterXaml()
        {
            try
            {
                LogDebug("🔍 UI Validation START - checking required elements...");

                var mainContent = this.FindName("MainContentGrid");
                var loadingOverlay = this.FindName("LoadingOverlay");
                var headerRepeater = this.FindName("HeaderRepeater");
                var dataRowsRepeater = this.FindName("DataRowsRepeater");

                var validationResults = new Dictionary<string, bool>
                {
                    ["MainContentGrid"] = mainContent != null,
                    ["LoadingOverlay"] = loadingOverlay != null,
                    ["HeaderRepeater"] = headerRepeater != null,
                    ["DataRowsRepeater"] = dataRowsRepeater != null
                };

                foreach (var result in validationResults)
                {
                    LogDebug("🔍 UI Element {ElementName}: {Status}", result.Key, result.Value ? "FOUND" : "MISSING");
                }

                var allElementsFound = validationResults.Values.All(v => v);

                if (!allElementsFound)
                {
                    var missingElements = validationResults.Where(r => !r.Value).Select(r => r.Key);
                    LogWarn("❌ UI Validation FAILED - Missing elements: {MissingElements}", string.Join(", ", missingElements));
                    _xamlLoadFailed = true;
                }
                else
                {
                    LogDebug("✅ UI Validation SUCCESS - All required elements found");
                }
            }
            catch (Exception ex)
            {
                LogError("⚠️ UI Validation ERROR: {Error}", ex.Message);
                LogError("⚠️ UI Validation Exception: {Exception}", ex);
                _xamlLoadFailed = true;
            }
        }

        #endregion

        #region ✅ LoggerComponent Integration s ROZŠÍRENÝM logovaním

        /// <summary>
        /// ✅ Nastaví LoggerComponent pre integráciu s DataGrid - PUBLIC API
        /// </summary>
        public void SetIntegratedLogger(LoggerComponent? loggerComponent, bool enableIntegration = true)
        {
            try
            {
                LogDebug("🔗 SetIntegratedLogger START - Component: {HasComponent}, Enable: {Enable}",
                    loggerComponent != null, enableIntegration);

                _integratedLogger = loggerComponent;
                _loggerIntegrationEnabled = enableIntegration && loggerComponent != null;

                if (_loggerIntegrationEnabled && loggerComponent != null)
                {
                    LogInfo("🔗 LoggerComponent integration ENABLED for DataGrid instance [{InstanceId}] - Logger: {LoggerType}, File: {LogFile}",
                        _componentInstanceId, loggerComponent.ExternalLoggerType, loggerComponent.CurrentLogFile);

                    // Test loggeru
                    _ = Task.Run(async () =>
                    {
                        var testResult = await loggerComponent.TestLoggingAsync();
                        LogDebug("🧪 LoggerComponent test result: {TestResult}", testResult ? "SUCCESS" : "FAILED");
                    });
                }
                else
                {
                    LogInfo("🔗 LoggerComponent integration DISABLED for DataGrid instance [{InstanceId}]", _componentInstanceId);
                }
            }
            catch (Exception ex)
            {
                LogError("❌ SetIntegratedLogger ERROR: {Error}", ex.Message);
                LogError("❌ SetIntegratedLogger Exception: {Exception}", ex);
            }
        }

        /// <summary>
        /// ✅ PRIVATE: Async logovanie s fallback na Debug.WriteLine + detailným error handling
        /// </summary>
        private async Task LogAsync(string message, string logLevel = "INFO")
        {
            try
            {
                var timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] [DataGrid-{_componentInstanceId}] {message}";

                if (_loggerIntegrationEnabled && _integratedLogger != null)
                {
                    await _integratedLogger.LogAsync(timestampedMessage, logLevel);
                }
                else
                {
                    // Fallback na System.Diagnostics.Debug
                    System.Diagnostics.Debug.WriteLine($"[{logLevel}] {timestampedMessage}");
                }
            }
            catch (Exception ex)
            {
                // Aj keď logovanie zlyhal, pokračujeme - ale zaznamename to
                System.Diagnostics.Debug.WriteLine($"⚠️ Logging failed: {ex.Message} | Original message: {message}");
            }
        }

        /// <summary>
        /// ✅ SYNC logovanie pre prípady kde nemôžeme await
        /// </summary>
        private void LogSync(string message, string logLevel = "INFO")
        {
            _ = Task.Run(async () => await LogAsync(message, logLevel));
        }

        // ✅ Helper metódy pre rôzne log levels s template podporou - RELEASE MODE READY
        private void LogTrace(string message, params object[] args) => LogSync(FormatMessage(message, args), "TRACE");
        private void LogDebug(string message, params object[] args) => LogSync(FormatMessage(message, args), "DEBUG");
        private void LogInfo(string message, params object[] args) => LogSync(FormatMessage(message, args), "INFO");
        private void LogWarn(string message, params object[] args) => LogSync(FormatMessage(message, args), "WARN");
        private void LogError(string message, params object[] args) => LogSync(FormatMessage(message, args), "ERROR");

        private static string FormatMessage(string template, params object[] args)
        {
            try
            {
                return args.Length > 0 ? string.Format(template.Replace("{", "{{").Replace("}", "}}").Replace("{{", "{").Replace("}}", "}"), args) : template;
            }
            catch
            {
                return $"{template} [ARGS: {string.Join(", ", args)}]";
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods s KOMPLETNÝM logovaním a error handling

        /// <summary>
        /// ✅ InitializeAsync s LoggerComponent parameter + KOMPLETNÉ logovanie - PUBLIC API
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule>? validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15,
            DataGridColorConfig? colorConfig = null,
            LoggerComponent? loggerComponent = null)
        {
            try
            {
                LogInfo("🚀 InitializeAsync START - Columns: {ColumnCount}, Rules: {RuleCount}, EmptyRows: {EmptyRows}, LoggerComponent: {HasLogger}",
                    columns?.Count ?? 0, validationRules?.Count ?? 0, emptyRowsCount, loggerComponent != null);

                StartOperation("InitializeAsync");

                // ✅ Validácia vstupných parametrov
                if (columns == null || columns.Count == 0)
                {
                    var error = "Columns parameter cannot be null or empty";
                    LogError("❌ InitializeAsync VALIDATION ERROR: {Error}", error);
                    throw new ArgumentException(error, nameof(columns));
                }

                // ✅ LoggerComponent integrácia
                if (loggerComponent != null)
                {
                    LogDebug("🔗 InitializeAsync - Setting up LoggerComponent integration...");
                    SetIntegratedLogger(loggerComponent, true);
                    LogInfo("🔗 LoggerComponent integration configured: {DiagnosticInfo}", loggerComponent.GetDiagnosticInfo());

                    // ✅ Rekonfiguruj services s externým logger
                    ReconfigureServicesWithExternalLogger(loggerComponent.ExternalLogger);
                }

                // ✅ Uloženie konfigurácie
                LogDebug("📝 InitializeAsync - Storing configuration...");
                _columns.Clear();
                _columns.AddRange(columns);
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;
                _individualColorConfig = colorConfig?.Clone();

                LogDebug("📝 Configuration stored - Columns: {ColumnCount}, UnifiedRowCount: {RowCount}, AutoAdd: {AutoAdd}, Colors: {HasColors}",
                    _columns.Count, _unifiedRowCount, _autoAddEnabled, _individualColorConfig != null);

                // ✅ Inicializácia services
                await InitializeServicesAsync(columns, validationRules ?? new List<GridValidationRule>(), throttlingConfig, emptyRowsCount);

                // ✅ UI setup
                if (!_xamlLoadFailed)
                {
                    LogDebug("🎨 InitializeAsync - Setting up UI...");
                    ApplyIndividualColorsToUI();
                    InitializeSearchSortZebra();
                    await CreateInitialEmptyRowsAsync();
                }
                else
                {
                    LogWarn("🎨 InitializeAsync - UI setup skipped due to XAML errors");
                }

                _isInitialized = true;

                var duration = EndOperation("InitializeAsync");
                LogInfo("✅ InitializeAsync COMPLETED successfully - Duration: {Duration}ms, Instance: {InstanceId}",
                    duration, _componentInstanceId);

                // ✅ Update UI visibility
                UpdateUIVisibility();
            }
            catch (Exception ex)
            {
                EndOperation("InitializeAsync");
                LogError("❌ CRITICAL ERROR during InitializeAsync: {Error}", ex.Message);
                LogError("❌ InitializeAsync Exception Details: {Exception}", ex);
                LogError("❌ InitializeAsync StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// ✅ LoadDataAsync s ROZŠÍRENÝM logovaním a UI update
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                LogInfo("📊 LoadDataAsync START - Rows: {RowCount}", data?.Count ?? 0);
                StartOperation("LoadDataAsync");

                if (data == null)
                {
                    LogWarn("⚠️ LoadDataAsync - data parameter is null, creating empty list");
                    data = new List<Dictionary<string, object?>>();
                }

                LogDebug("📊 LoadDataAsync - Validating data structure...");
                ValidateDataStructure(data);

                EnsureInitialized();

                if (_dataManagementService == null)
                {
                    var error = "DataManagementService is not available";
                    LogError("❌ LoadDataAsync ERROR: {Error}", error);
                    throw new InvalidOperationException(error);
                }

                LogDebug("🔄 LoadDataAsync - Calling DataManagementService.LoadDataAsync...");
                await _dataManagementService.LoadDataAsync(data);

                LogDebug("🎨 LoadDataAsync - Updating UI display rows...");
                await UpdateDisplayRowsAsync();

                var duration = EndOperation("LoadDataAsync");
                LogInfo("✅ LoadDataAsync COMPLETED - Rows: {RowCount}, Duration: {Duration}ms", data.Count, duration);
            }
            catch (Exception ex)
            {
                EndOperation("LoadDataAsync");
                LogError("❌ ERROR in LoadDataAsync: {Error}", ex.Message);
                LogError("❌ LoadDataAsync Exception: {Exception}", ex);
                LogError("❌ LoadDataAsync StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// ✅ ValidateAllRowsAsync s detailným logovaním
        /// </summary>
        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                LogInfo("🔍 ValidateAllRowsAsync START");
                StartOperation("ValidateAllRowsAsync");

                EnsureInitialized();

                if (_validationService == null)
                {
                    LogError("❌ ValidationService not available");
                    return false;
                }

                LogDebug("🔄 ValidateAllRowsAsync - Calling ValidationService...");
                var isValid = await _validationService.ValidateAllRowsAsync();

                var duration = EndOperation("ValidateAllRowsAsync");
                LogInfo("✅ ValidateAllRowsAsync COMPLETED - Result: {Result}, Duration: {Duration}ms",
                    isValid ? "ALL VALID" : "ERRORS FOUND", duration);

                if (!isValid)
                {
                    LogWarn("⚠️ Validation errors found - ErrorCount: {ErrorCount}",
                        _validationService.TotalValidationErrorCount);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                EndOperation("ValidateAllRowsAsync");
                LogError("❌ ERROR in ValidateAllRowsAsync: {Error}", ex.Message);
                LogError("❌ ValidateAllRowsAsync Exception: {Exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// ✅ ExportToDataTableAsync s detailným logovaním
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                LogInfo("📤 ExportToDataTableAsync START");
                StartOperation("ExportToDataTableAsync");

                EnsureInitialized();

                if (_exportService == null)
                {
                    LogError("❌ ExportService not available");
                    return new DataTable();
                }

                LogDebug("🔄 ExportToDataTableAsync - Calling ExportService...");
                var dataTable = await _exportService.ExportToDataTableAsync();

                var duration = EndOperation("ExportToDataTableAsync");
                LogInfo("✅ ExportToDataTableAsync COMPLETED - Rows: {RowCount}, Columns: {ColumnCount}, Duration: {Duration}ms",
                    dataTable.Rows.Count, dataTable.Columns.Count, duration);

                return dataTable;
            }
            catch (Exception ex)
            {
                EndOperation("ExportToDataTableAsync");
                LogError("❌ ERROR in ExportToDataTableAsync: {Error}", ex.Message);
                LogError("❌ ExportToDataTableAsync Exception: {Exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// ✅ ClearAllDataAsync s logovaním
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            try
            {
                LogInfo("🗑️ ClearAllDataAsync START");
                StartOperation("ClearAllDataAsync");

                EnsureInitialized();

                if (_dataManagementService == null)
                {
                    LogError("❌ DataManagementService not available");
                    return;
                }

                LogDebug("🔄 ClearAllDataAsync - Calling DataManagementService...");
                await _dataManagementService.ClearAllDataAsync();

                LogDebug("🎨 ClearAllDataAsync - Updating UI display rows...");
                await UpdateDisplayRowsAsync();

                var duration = EndOperation("ClearAllDataAsync");
                LogInfo("✅ ClearAllDataAsync COMPLETED - Duration: {Duration}ms", duration);
            }
            catch (Exception ex)
            {
                EndOperation("ClearAllDataAsync");
                LogError("❌ ERROR in ClearAllDataAsync: {Error}", ex.Message);
                LogError("❌ ClearAllDataAsync Exception: {Exception}", ex);
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Performance tracking metódy

        private void StartOperation(string operationName)
        {
            _operationStartTimes[operationName] = DateTime.UtcNow;
        }

        private double EndOperation(string operationName)
        {
            if (_operationStartTimes.TryGetValue(operationName, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _operationStartTimes.Remove(operationName);
                return Math.Round(duration, 2);
            }
            return 0;
        }

        #endregion

        #region ✅ NOVÉ: Data validation helper

        private void ValidateDataStructure(List<Dictionary<string, object?>> data)
        {
            try
            {
                LogDebug("🔍 ValidateDataStructure START - Rows: {RowCount}", data.Count);

                if (data.Count == 0)
                {
                    LogDebug("ℹ️ ValidateDataStructure - Empty data set");
                    return;
                }

                // Skontroluj štruktúru prvého riadku
                var firstRow = data[0];
                var rowColumns = firstRow.Keys.ToList();

                LogDebug("🔍 ValidateDataStructure - First row columns: {Columns}", string.Join(", ", rowColumns));

                // Skontroluj či sa stĺpce zhodujú s definíciou
                var definedColumns = _columns.Select(c => c.Name).ToList();
                var missingColumns = definedColumns.Except(rowColumns).ToList();
                var extraColumns = rowColumns.Except(definedColumns).ToList();

                if (missingColumns.Any())
                {
                    LogWarn("⚠️ ValidateDataStructure - Missing columns in data: {MissingColumns}", string.Join(", ", missingColumns));
                }

                if (extraColumns.Any())
                {
                    LogWarn("⚠️ ValidateDataStructure - Extra columns in data: {ExtraColumns}", string.Join(", ", extraColumns));
                }

                LogDebug("✅ ValidateDataStructure COMPLETED");
            }
            catch (Exception ex)
            {
                LogError("❌ ValidateDataStructure ERROR: {Error}", ex.Message);
            }
        }

        #endregion

        #region ✅ UI Update Methods s rozšíreným logovaním

        /// <summary>
        /// ✅ Aktualizuje ObservableCollection pre UI binding s detailným logovaním
        /// </summary>
        private async Task UpdateDisplayRowsAsync()
        {
            try
            {
                LogDebug("🎨 UpdateDisplayRowsAsync START - Current rows: {CurrentRows}", _displayRows.Count);
                StartOperation("UpdateDisplayRowsAsync");

                if (_dataManagementService == null)
                {
                    LogWarn("⚠️ UpdateDisplayRowsAsync - DataManagementService not available");
                    return;
                }

                var allData = await _dataManagementService.GetAllDataAsync();
                LogDebug("🎨 UpdateDisplayRowsAsync - Retrieved {DataRows} rows from service", allData.Count);

                // Update UI na main thread
                this.DispatcherQueue?.TryEnqueue(() =>
                {
                    try
                    {
                        LogDebug("🎨 UpdateDisplayRowsAsync - Updating UI on main thread...");

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

                            _displayRows.Add(viewModel);
                        }

                        var duration = EndOperation("UpdateDisplayRowsAsync");
                        LogDebug("✅ UpdateDisplayRowsAsync COMPLETED - UI updated: {DisplayRows} rows, Duration: {Duration}ms",
                            _displayRows.Count, duration);
                    }
                    catch (Exception uiEx)
                    {
                        EndOperation("UpdateDisplayRowsAsync");
                        LogError("❌ UpdateDisplayRowsAsync UI ERROR: {Error}", uiEx.Message);
                        LogError("❌ UpdateDisplayRowsAsync UI Exception: {Exception}", uiEx);
                    }
                });
            }
            catch (Exception ex)
            {
                EndOperation("UpdateDisplayRowsAsync");
                LogError("❌ ERROR in UpdateDisplayRowsAsync: {Error}", ex.Message);
                LogError("❌ UpdateDisplayRowsAsync Exception: {Exception}", ex);
            }
        }

        #endregion

        #region ✅ Helper Methods s rozšíreným logovaním

        private void InitializeDependencyInjection()
        {
            try
            {
                LogDebug("🔧 InitializeDependencyInjection START");
                StartOperation("InitializeDependencyInjection");

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                _dataManagementService = _serviceProvider.GetService<IDataManagementService>();
                _validationService = _serviceProvider.GetService<IValidationService>();
                _exportService = _serviceProvider.GetService<IExportService>();

                // ✅ SearchAndSortService bez logger parametra
                _searchAndSortService = new SearchAndSortService();

                var duration = EndOperation("InitializeDependencyInjection");

                LogDebug("✅ Services initialized - DataManagement: {HasDataService}, Validation: {HasValidationService}, Export: {HasExportService}, SearchSort: {HasSearchService}",
                    _dataManagementService != null, _validationService != null, _exportService != null, _searchAndSortService != null);

                LogInfo("✅ Dependency Injection initialized successfully - Duration: {Duration}ms", duration);
            }
            catch (Exception ex)
            {
                EndOperation("InitializeDependencyInjection");
                LogError("⚠️ DI initialization ERROR: {Error}", ex.Message);
                LogError("⚠️ DI Exception: {Exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// ✅ ConfigureServices s detailným logovaním - NEZÁVISLÝ na logging systéme
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            try
            {
                LogDebug("🔧 ConfigureServices START - Setting up services with NullLogger fallback...");

                // ✅ KĽÚČOVÉ: Balík je nezávislý na logging systéme
                // Služby dostanú NullLogger.Instance ako fallback ak nie je poskytnutý externý logger

                services.AddSingleton<IDataManagementService>(provider =>
                {
                    var logger = NullLogger<DataManagementService>.Instance;
                    return new DataManagementService(logger);
                });

                services.AddSingleton<IValidationService>(provider =>
                {
                    var logger = NullLogger<ValidationService>.Instance;
                    return new ValidationService(logger);
                });

                services.AddTransient<IExportService>(provider =>
                {
                    var logger = NullLogger<ExportService>.Instance;
                    var dataService = provider.GetRequiredService<IDataManagementService>();
                    return new ExportService(logger, dataService);
                });

                services.AddTransient<ICopyPasteService>(provider =>
                {
                    var logger = NullLogger<CopyPasteService>.Instance;
                    return new CopyPasteService(logger);
                });

                services.AddTransient<INavigationService>(provider =>
                {
                    var logger = NullLogger<NavigationService>.Instance;
                    return new NavigationService(logger);
                });

                LogDebug("✅ ConfigureServices COMPLETED - All services configured with NullLogger fallback");
            }
            catch (Exception ex)
            {
                LogError("⚠️ ConfigureServices ERROR: {Error}", ex.Message);
                LogError("⚠️ ConfigureServices Exception: {Exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// ✅ Rekonfigurácia services s externým logger + logovanie
        /// </summary>
        private void ReconfigureServicesWithExternalLogger(ILogger externalLogger)
        {
            try
            {
                LogDebug("🔄 ReconfigureServicesWithExternalLogger START - Logger: {LoggerType}", externalLogger.GetType().Name);
                StartOperation("ReconfigureServices");

                var services = new ServiceCollection();

                // Registruj services s externým logger namiesto NullLogger
                services.AddSingleton<IDataManagementService>(provider =>
                {
                    var typedLogger = new LoggerAdapter<DataManagementService>(externalLogger);
                    return new DataManagementService(typedLogger);
                });

                services.AddSingleton<IValidationService>(provider =>
                {
                    var typedLogger = new LoggerAdapter<ValidationService>(externalLogger);
                    return new ValidationService(typedLogger);
                });

                services.AddTransient<IExportService>(provider =>
                {
                    var typedLogger = new LoggerAdapter<ExportService>(externalLogger);
                    var dataService = provider.GetRequiredService<IDataManagementService>();
                    return new ExportService(typedLogger, dataService);
                });

                // Rebuild service provider s novým logger
                _serviceProvider?.Dispose();
                _serviceProvider = services.BuildServiceProvider();

                // Znovu získaj services s novým logger
                _dataManagementService = _serviceProvider.GetService<IDataManagementService>();
                _validationService = _serviceProvider.GetService<IValidationService>();
                _exportService = _serviceProvider.GetService<IExportService>();

                var duration = EndOperation("ReconfigureServices");
                LogInfo("✅ Services reconfigured with external logger - Duration: {Duration}ms", duration);
            }
            catch (Exception ex)
            {
                EndOperation("ReconfigureServices");
                LogError("⚠️ ReconfigureServicesWithExternalLogger ERROR: {Error}", ex.Message);
                LogError("⚠️ ReconfigureServices Exception: {Exception}", ex);
            }
        }

        // ✅ Helper adapter pre ILogger<T> z generic ILogger
        internal class LoggerAdapter<T> : ILogger<T>
        {
            private readonly ILogger _baseLogger;

            public LoggerAdapter(ILogger baseLogger)
            {
                _baseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
                => _baseLogger.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel)
                => _baseLogger.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => _baseLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                var errorMsg = "DataGrid is not initialized. Call InitializeAsync() first.";
                LogError("❌ EnsureInitialized FAILED: {Error}", errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
        }

        #endregion

        #region ✅ FALLBACK UI Creation s error handling

        /// <summary>
        /// ✅ Jednoduchší fallback UI s detailným logovaním
        /// </summary>
        private void CreateSimpleFallbackUI()
        {
            try
            {
                LogInfo("🔧 CreateSimpleFallbackUI START - Creating fallback UI due to XAML errors");

                var mainGrid = new Grid();

                var fallbackText = new TextBlock
                {
                    Text = "⚠️ AdvancedDataGrid - XAML Fallback Mode\n(LoggerComponent integrated)\n\nXAML loading failed, but component is functional.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                };

                mainGrid.Children.Add(fallbackText);
                this.Content = mainGrid;

                LogInfo("✅ CreateSimpleFallbackUI COMPLETED - Fallback UI created successfully");
            }
            catch (Exception fallbackEx)
            {
                LogError("❌ CreateSimpleFallbackUI FAILED: {Error}", fallbackEx.Message);
                LogError("❌ Fallback Exception: {Exception}", fallbackEx);

                // ✅ Posledná záchrana - iba TextBlock
                try
                {
                    this.Content = new TextBlock
                    {
                        Text = "AdvancedDataGrid - Critical Error\n(LoggerComponent available)",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    };
                    LogInfo("✅ Emergency fallback UI created");
                }
                catch (Exception criticalEx)
                {
                    LogError("❌ CRITICAL: Even emergency fallback failed: {Error}", criticalEx.Message);
                }
            }
        }

        #endregion

        #region ✅ UI Helper Methods s rozšíreným error handling

        private void UpdateUIVisibility()
        {
            if (_xamlLoadFailed)
            {
                LogDebug("⚠️ UpdateUIVisibility SKIPPED - XAML error detected");
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    LogDebug("🔄 UpdateUIVisibility START - Initialized: {IsInitialized}", _isInitialized);

                    var mainContentGrid = this.FindName("MainContentGrid") as FrameworkElement;
                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;

                    if (mainContentGrid != null)
                    {
                        var newVisibility = _isInitialized ? Visibility.Visible : Visibility.Collapsed;
                        mainContentGrid.Visibility = newVisibility;
                        LogDebug("✅ MainContentGrid visibility: {Visibility}", newVisibility);
                    }
                    else
                    {
                        LogWarn("⚠️ MainContentGrid not found in UpdateUIVisibility");
                    }

                    if (loadingOverlay != null)
                    {
                        var newVisibility = _isInitialized ? Visibility.Collapsed : Visibility.Visible;
                        loadingOverlay.Visibility = newVisibility;
                        LogDebug("✅ LoadingOverlay visibility: {Visibility}", newVisibility);
                    }
                    else
                    {
                        LogWarn("⚠️ LoadingOverlay not found in UpdateUIVisibility");
                    }

                    LogDebug("✅ UpdateUIVisibility COMPLETED");
                }
                catch (Exception ex)
                {
                    LogError("⚠️ UpdateUIVisibility ERROR: {Error}", ex.Message);
                    LogError("⚠️ UpdateUIVisibility Exception: {Exception}", ex);
                }
            });
        }

        private void ShowLoadingState(string message)
        {
            if (_xamlLoadFailed)
            {
                LogDebug("⚠️ ShowLoadingState SKIPPED - XAML error, Message: {Message}", message);
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    LogDebug("📺 ShowLoadingState START - Message: {Message}", message);

                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;
                    var loadingText = this.FindName("LoadingText") as TextBlock;

                    if (loadingOverlay != null)
                    {
                        loadingOverlay.Visibility = Visibility.Visible;
                        LogDebug("✅ LoadingOverlay shown");
                    }

                    if (loadingText != null)
                    {
                        loadingText.Text = message;
                        LogDebug("✅ LoadingText updated: {Message}", message);
                    }

                    LogDebug("✅ ShowLoadingState COMPLETED");
                }
                catch (Exception ex)
                {
                    LogError("⚠️ ShowLoadingState ERROR: {Error}", ex.Message);
                }
            });
        }

        private void HideLoadingState()
        {
            if (_xamlLoadFailed)
            {
                LogDebug("⚠️ HideLoadingState SKIPPED - XAML error");
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    LogDebug("📺 HideLoadingState START");

                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;
                    if (loadingOverlay != null)
                    {
                        loadingOverlay.Visibility = Visibility.Collapsed;
                        LogDebug("✅ LoadingOverlay hidden");
                    }

                    LogDebug("✅ HideLoadingState COMPLETED");
                }
                catch (Exception ex)
                {
                    LogError("⚠️ HideLoadingState ERROR: {Error}", ex.Message);
                }
            });
        }

        #endregion

        #region ✅ Diagnostic Properties s rozšíreným info

        public bool IsXamlLoaded => !_xamlLoadFailed;

        public string DiagnosticInfo =>
            $"AdvancedDataGrid[{_componentInstanceId}]: " +
            $"Initialized={_isInitialized}, " +
            $"XAML={!_xamlLoadFailed}, " +
            $"Logger={_loggerIntegrationEnabled}, " +
            $"Columns={_columns.Count}, " +
            $"Rows={_displayRows.Count}, " +
            $"AutoAdd={_autoAddEnabled}";

        /// <summary>
        /// ✅ LoggerComponent integration status s rozšírenými detailmi
        /// </summary>
        public string GetLoggerIntegrationStatus()
        {
            if (!_loggerIntegrationEnabled || _integratedLogger == null)
                return $"LoggerComponent: DISABLED for DataGrid [{_componentInstanceId}]";

            return $"LoggerComponent: ENABLED for DataGrid [{_componentInstanceId}] " +
                   $"(Logger: {_integratedLogger.ExternalLoggerType}, " +
                   $"File: {Path.GetFileName(_integratedLogger.CurrentLogFile)}, " +
                   $"Size: {_integratedLogger.CurrentFileSizeMB:F2}MB, " +
                   $"Rotation: {_integratedLogger.IsRotationEnabled})";
        }

        /// <summary>
        /// ✅ NOVÉ: Získa performance report
        /// </summary>
        public string GetPerformanceReport()
        {
            var activeOperations = _operationStartTimes.Count;
            var memoryMB = GC.GetTotalMemory(false) / 1024 / 1024;

            return $"Performance[{_componentInstanceId}]: " +
                   $"ActiveOps={activeOperations}, " +
                   $"Memory={memoryMB}MB, " +
                   $"Gen0={GC.CollectionCount(0)}, " +
                   $"Gen1={GC.CollectionCount(1)}, " +
                   $"Gen2={GC.CollectionCount(2)}";
        }

        #endregion

        #region ✅ Skeleton implementácie s logovaním

        public async Task LoadDataAsync(DataTable dataTable)
        {
            LogInfo("📊 LoadDataAsync(DataTable) START - Rows: {RowCount}", dataTable?.Rows.Count ?? 0);

            if (dataTable == null)
            {
                LogWarn("⚠️ DataTable parameter is null");
                return;
            }

            // Convert DataTable to List<Dictionary>
            var dataList = new List<Dictionary<string, object?>>();

            foreach (System.Data.DataRow row in dataTable.Rows)
            {
                var rowDict = new Dictionary<string, object?>();
                foreach (System.Data.DataColumn column in dataTable.Columns)
                {
                    rowDict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                }
                dataList.Add(rowDict);
            }

            LogDebug("📊 DataTable converted to List<Dictionary> - Rows: {RowCount}", dataList.Count);
            await LoadDataAsync(dataList);
        }

        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        // ✅ Skeleton metódy s logovaním
        private void ApplyIndividualColorsToUI()
        {
            LogDebug("🎨 ApplyIndividualColorsToUI called - HasColors: {HasColors}", _individualColorConfig != null);
        }

        private void InitializeSearchSortZebra()
        {
            LogDebug("🔍 InitializeSearchSortZebra called - SearchSort available: {Available}", _searchAndSortService != null);
        }

        private async Task CreateInitialEmptyRowsAsync()
        {
            LogDebug("🔥 CreateInitialEmptyRowsAsync START - Creating {RowCount} initial rows", _unifiedRowCount);
            await Task.CompletedTask;
            LogDebug("✅ CreateInitialEmptyRowsAsync COMPLETED");
        }

        private async Task InitializeServicesAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount)
        {
            LogDebug("🔧 InitializeServicesAsync START - Columns: {ColumnCount}, Rules: {RuleCount}",
                columns.Count, validationRules.Count);
            await Task.CompletedTask;
            LogDebug("✅ InitializeServicesAsync COMPLETED");
        }

        #endregion

        #region INotifyPropertyChanged & IDisposable s rozšíreným logovaním

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
                LogInfo("🧹 AdvancedDataGrid DISPOSE START - Instance: {InstanceId}", _componentInstanceId);

                _searchAndSortService?.Dispose();

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                // ✅ LoggerComponent nie je owned by DataGrid
                _integratedLogger = null;
                _loggerIntegrationEnabled = false;

                // Clean up tracking
                _operationStartTimes.Clear();

                _isDisposed = true;
                LogInfo("✅ AdvancedDataGrid DISPOSED successfully - Instance: {InstanceId}", _componentInstanceId);
            }
            catch (Exception ex)
            {
                LogError("❌ Error during dispose: {Error}", ex.Message);
            }
        }

        #endregion
    }

    /// <summary>
    /// ✅ ViewModel pre zobrazenie riadku v UI - s logovaním
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
                    cells.Add(new CellViewModel
                    {
                        ColumnName = column.Name,
                        Value = value,
                        DisplayValue = value?.ToString() ?? "",
                        Header = column.Header ?? column.Name,
                        Width = column.Width
                    });
                }
                return cells;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// ✅ ViewModel pre zobrazenie bunky
    /// </summary>
    public class CellViewModel
    {
        public string ColumnName { get; set; } = "";
        public object? Value { get; set; }
        public string DisplayValue { get; set; } = "";
        public string Header { get; set; } = "";
        public double Width { get; set; } = 150;
    }
}