// Controls/AdvancedDataGrid.xaml.cs - ✅ KOMPLETNÁ LoggerComponent integrácia
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

// ✅ OPRAVENÉ CS0104: Aliasy pre zamedzenie konfliktov s WinUI typmi
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.Logger;  // ✅ LoggerComponent integrácia

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid s KOMPLETNOU LoggerComponent integráciou - ✅ PUBLIC API
    /// Loguje všetky UI operácie, chyby, validácie, dátové operácie, search/sort/zebra
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private IServiceProvider? _serviceProvider;
        private ILogger<AdvancedDataGrid>? _logger;
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

        // ✅ Interné dáta pre AUTO-ADD
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();

        // ✅ Search & Sort state tracking s PUBLIC SortDirection typom
        private readonly Dictionary<string, string> _columnSearchFilters = new();
        private string? _currentSortColumn;
        private SortDirection _currentSortDirection = SortDirection.None;

        // ✅ KĽÚČOVÁ NOVINKA: LoggerComponent integrácia
        private LoggerComponent? _integratedLogger;
        private bool _loggerIntegrationEnabled = false;
        private string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];

        #endregion

        #region ✅ Constructor s OPRAVENÝ XAML handling a LoggerComponent

        public AdvancedDataGrid()
        {
            try
            {
                LogDebug("🔧 AdvancedDataGrid: Začína konštruktor...");

                // ✅ KĽÚČOVÁ OPRAVA: Jednoduchšie XAML loading bez complex error handling
                TryInitializeXaml();

                if (!_xamlLoadFailed)
                {
                    LogDebug("✅ AdvancedDataGrid: XAML úspešne načítané");
                    InitializeDependencyInjection();
                    LogInfo("✅ AdvancedDataGrid: Kompletne inicializovaný s LoggerComponent support");
                    UpdateUIVisibility();
                }
                else
                {
                    LogWarn("⚠️ XAML loading zlyhal - vytváram fallback UI");
                    CreateSimpleFallbackUI();
                }
            }
            catch (Exception ex)
            {
                LogError($"❌ CRITICAL CONSTRUCTOR ERROR: {ex.Message}", ex);
                CreateSimpleFallbackUI();
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ: Jednoduchšie XAML loading s logovaním
        /// </summary>
        private void TryInitializeXaml()
        {
            try
            {
                LogDebug("🔧 XAML Loading: Volám InitializeComponent()...");

                this.InitializeComponent();
                _xamlLoadFailed = false;

                LogInfo("✅ XAML Loading: InitializeComponent() úspešný!");

                // ✅ NOVÉ: Okamžite skontroluj či sa UI elementy načítali
                ValidateUIElementsAfterXaml();
            }
            catch (Exception xamlEx)
            {
                LogError($"❌ XAML ERROR: {xamlEx.Message}", xamlEx);
                _xamlLoadFailed = true;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Overí či sa UI elementy načítali správne po XAML
        /// </summary>
        private void ValidateUIElementsAfterXaml()
        {
            try
            {
                var hasMainContent = this.FindName("MainContentGrid") != null;
                var hasLoadingOverlay = this.FindName("LoadingOverlay") != null;

                LogDebug($"📋 UI validácia: MainContent={hasMainContent}, LoadingOverlay={hasLoadingOverlay}");

                if (!hasMainContent || !hasLoadingOverlay)
                {
                    LogWarn("❌ UI elementy sa nenačítali správne - označujem ako XAML failed");
                    _xamlLoadFailed = true;
                }
            }
            catch (Exception ex)
            {
                LogError($"⚠️ UI validácia chyba: {ex.Message}", ex);
                _xamlLoadFailed = true;
            }
        }

        #endregion

        #region ✅ KĽÚČOVÁ NOVINKA: LoggerComponent Integration API

        /// <summary>
        /// ✅ NOVÉ: Nastaví LoggerComponent pre integráciu s DataGrid - PUBLIC API
        /// </summary>
        /// <param name="loggerComponent">LoggerComponent inštancia</param>
        /// <param name="enableIntegration">Či povoliť logovanie</param>
        public void SetIntegratedLogger(LoggerComponent? loggerComponent, bool enableIntegration = true)
        {
            _integratedLogger = loggerComponent;
            _loggerIntegrationEnabled = enableIntegration && loggerComponent != null;

            LogInfo($"LoggerComponent integration {(enableIntegration ? "ENABLED" : "DISABLED")} for DataGrid instance [{_componentInstanceId}]");
        }

        /// <summary>
        /// ✅ PRIVATE: Async logovanie s fallback na Debug.WriteLine
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
                // Aj keď logovanie zlyhal, pokračujeme
                System.Diagnostics.Debug.WriteLine($"⚠️ Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ SYNC logovanie pre prípady kde nemôžeme await
        /// </summary>
        private void LogSync(string message, string logLevel = "INFO")
        {
            _ = Task.Run(async () => await LogAsync(message, logLevel));
        }

        // ✅ Helper metódy pre rôzne log levels
        private void LogDebug(string message) => LogSync(message, "DEBUG");
        private void LogInfo(string message) => LogSync(message, "INFO");
        private void LogWarn(string message) => LogSync(message, "WARN");
        private void LogError(string message, Exception? ex = null)
        {
            var errorMessage = ex != null ? $"{message} | Exception: {ex}" : message;
            LogSync(errorMessage, "ERROR");
        }

        #endregion

        #region ✅ PUBLIC API Methods s Individual Colors, LoggerComponent integrácia a KOMPLETNÝM logovaním

        /// <summary>
        /// ✅ OPRAVENÉ CS1501: InitializeAsync s LoggerComponent parameter - 6 argumentov
        /// Inicializuje DataGrid s Individual Color Config + LoggerComponent integrácia - ✅ PUBLIC API
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15,
            DataGridColorConfig? colorConfig = null,
            LoggerComponent? loggerComponent = null)  // ✅ OPRAVENÉ: 6. parameter pre LoggerComponent
        {
            try
            {
                await LogAsync($"🚀 InitializeAsync begins (XAML failed: {_xamlLoadFailed}, Logger: {loggerComponent != null}, Columns: {columns?.Count ?? 0}, EmptyRows: {emptyRowsCount})", "INFO");

                // ✅ KĽÚČOVÁ INTEGRÁCIA: Nastav LoggerComponent ak je poskytnutý
                if (loggerComponent != null)
                {
                    SetIntegratedLogger(loggerComponent, true);
                    await LogAsync("🔗 LoggerComponent integration ENABLED for this DataGrid instance", "INFO");
                }

                // ✅ OPRAVENÉ CS8604: Null check pre columns parameter
                if (columns == null)
                {
                    await LogAsync("❌ InitializeAsync: columns parameter is NULL", "ERROR");
                    throw new ArgumentNullException(nameof(columns), "Columns parameter cannot be null");
                }

                await LogAsync($"📊 InitializeAsync: Processing {columns.Count} columns, {validationRules?.Count ?? 0} validation rules", "INFO");

                // ✅ KĽÚČOVÁ OPRAVA: Ak XAML zlyhal, pokračuj iba s dátovou inicializáciou
                if (_xamlLoadFailed)
                {
                    await LogAsync("⚠️ XAML failed - continuing with data-only initialization without UI updates", "WARN");
                    await InitializeDataOnlyAsync(columns, validationRules, throttlingConfig, emptyRowsCount, colorConfig);
                    return;
                }

                _logger?.LogInformation("Začína inicializácia DataGrid s Individual Colors, Search/Sort/Zebra a {EmptyRowsCount} riadkami...", emptyRowsCount);

                ShowLoadingState("Inicializuje sa DataGrid s Individual Colors, Search/Sort/Zebra a LoggerComponent...");

                // ✅ AUTO-ADD: Unified row count
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;

                await LogAsync($"🔥 AUTO-ADD: Nastavený unified počet riadkov = {_unifiedRowCount}, auto-add enabled: {_autoAddEnabled}", "INFO");

                // ✅ Individual Colors - nastavuje sa iba pri inicializácii
                _individualColorConfig = colorConfig?.Clone() ?? DataGridColorConfig.Light;
                if (colorConfig != null)
                {
                    await LogAsync($"🎨 Individual Colors: Custom colors nastavené ({colorConfig.CustomColorsCount} custom colors)", "INFO");
                    ApplyIndividualColorsToUI();
                }
                else
                {
                    await LogAsync("🎨 Individual Colors: Using default Light colors", "INFO");
                }

                // Ulož columns pre neskoršie použitie
                _columns.Clear();
                _columns.AddRange(columns);
                await LogAsync($"📋 Columns stored: {string.Join(", ", columns.Select(c => $"{c.Name}({c.DataType.Name})"))} ", "DEBUG");

                // ✅ Nastav Search/Sort/Zebra
                InitializeSearchSortZebra();

                await InitializeServicesAsync(columns, validationRules, throttlingConfig, emptyRowsCount);

                // ✅ Vytvor počiatočné prázdne riadky
                await CreateInitialEmptyRowsAsync();

                // ✅ Setup header click handlers pre sorting
                SetupHeaderClickHandlers();

                _isInitialized = true;
                await LogAsync("🎉 DataGrid successfully initialized with LoggerComponent integration!", "INFO");

                UpdateUIVisibility();
                HideLoadingState();

                _logger?.LogInformation("DataGrid úspešne inicializovaný s Individual Colors: {HasColors}, Search/Sort/Zebra enabled, LoggerComponent: {LoggerEnabled}",
                    colorConfig != null, _loggerIntegrationEnabled);
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ CRITICAL ERROR during DataGrid initialization: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
                _logger?.LogError(ex, "Chyba pri inicializácii DataGrid s Individual Colors a Search/Sort/Zebra");

                if (!_xamlLoadFailed)
                {
                    ShowLoadingState($"Chyba: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// ✅ LoadDataAsync s rozšíreným logovaním
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                if (data == null)
                {
                    await LogAsync("⚠️ LoadDataAsync: data parameter is null - creating empty list", "WARN");
                    data = new List<Dictionary<string, object?>>();
                }

                await LogAsync($"📊 LoadDataAsync begins with {data.Count} rows...", "INFO");
                EnsureInitialized();

                if (_dataManagementService == null)
                {
                    await LogAsync("❌ DataManagementService is not available", "ERROR");
                    return;
                }

                await _dataManagementService.LoadDataAsync(data);

                // ✅ Po načítaní dát aplikuj search/sort/zebra
                await ApplySearchSortZebraAsync();

                await LogAsync($"✅ LoadDataAsync completed with Search/Sort/Zebra - {data.Count} rows loaded successfully", "INFO");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ ERROR in LoadDataAsync: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
                throw;
            }
        }

        /// <summary>
        /// ✅ ValidateAllRowsAsync s logovaním
        /// </summary>
        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                await LogAsync("🔍 ValidateAllRowsAsync begins...", "INFO");
                EnsureInitialized();

                if (_validationService == null)
                {
                    await LogAsync("❌ ValidationService not available", "ERROR");
                    return false;
                }

                var isValid = await _validationService.ValidateAllRowsAsync();
                await LogAsync($"✅ ValidateAllRowsAsync completed - result: {(isValid ? "ALL VALID" : "ERRORS FOUND")}", "INFO");

                return isValid;
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ ERROR in ValidateAllRowsAsync: {ex.Message}", "ERROR");
                throw;
            }
        }

        /// <summary>
        /// ✅ ExportToDataTableAsync s logovaním
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                await LogAsync("📤 ExportToDataTableAsync begins...", "INFO");
                EnsureInitialized();

                if (_exportService == null)
                {
                    await LogAsync("❌ ExportService not available", "ERROR");
                    return new DataTable();
                }

                var dataTable = await _exportService.ExportToDataTableAsync();
                await LogAsync($"✅ ExportToDataTableAsync completed - exported {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns", "INFO");

                return dataTable;
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ ERROR in ExportToDataTableAsync: {ex.Message}", "ERROR");
                throw;
            }
        }

        #endregion

        #region ✅ Helper Methods s logovaním

        private void InitializeDependencyInjection()
        {
            try
            {
                LogDebug("🔧 Inicializujem Dependency Injection...");

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                _logger = _serviceProvider.GetService<ILogger<AdvancedDataGrid>>();
                _dataManagementService = _serviceProvider.GetService<IDataManagementService>();
                _validationService = _serviceProvider.GetService<IValidationService>();
                _exportService = _serviceProvider.GetService<IExportService>();

                // ✅ SearchAndSortService bez logger parametra
                _searchAndSortService = new SearchAndSortService();

                _logger?.LogInformation("AdvancedDataGrid s LoggerComponent integrácia inicializovaný");
                LogInfo("✅ Dependency Injection úspešne inicializované s LoggerComponent");
            }
            catch (Exception ex)
            {
                LogError($"⚠️ DI initialization warning: {ex.Message}", ex);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            LogDebug("🔧 Configuring services...");

            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            });

            services.AddSingleton<IDataManagementService, DataManagementService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddTransient<IExportService, ExportService>();

            LogDebug("✅ Services configured");
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                var errorMsg = "DataGrid is not initialized. Call InitializeAsync() first.";
                LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
        }

        #endregion

        #region ✅ FALLBACK UI Creation

        /// <summary>
        /// ✅ OPRAVENÉ: Jednoduchší fallback UI s logovaním
        /// </summary>
        private void CreateSimpleFallbackUI()
        {
            try
            {
                LogInfo("🔧 Vytváram jednoduchý fallback UI...");

                // ✅ Vytvor základný Grid namiesto komplexného UI
                var mainGrid = new Grid();

                var fallbackText = new TextBlock
                {
                    Text = "⚠️ AdvancedDataGrid - XAML Fallback Mode (LoggerComponent integrated)",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap
                };

                mainGrid.Children.Add(fallbackText);
                this.Content = mainGrid;

                LogInfo("✅ Fallback UI vytvorené s LoggerComponent support");
            }
            catch (Exception fallbackEx)
            {
                LogError($"❌ Aj fallback UI creation failed: {fallbackEx.Message}", fallbackEx);

                // ✅ Posledná záchrana - iba TextBlock
                this.Content = new TextBlock
                {
                    Text = "AdvancedDataGrid - Error (LoggerComponent available)",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
        }

        #endregion

        #region ✅ UI Helper Methods s lepším error handlingom a logovaním

        private void UpdateUIVisibility()
        {
            if (_xamlLoadFailed)
            {
                LogDebug("⚠️ UpdateUIVisibility skipped due to XAML error");
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    LogDebug($"🔄 Updating UI visibility (initialized: {_isInitialized})...");

                    var mainContentGrid = this.FindName("MainContentGrid") as FrameworkElement;
                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;

                    if (mainContentGrid != null)
                    {
                        mainContentGrid.Visibility = _isInitialized ? Visibility.Visible : Visibility.Collapsed;
                        LogDebug($"✅ MainContentGrid visibility = {mainContentGrid.Visibility}");
                    }

                    if (loadingOverlay != null)
                    {
                        loadingOverlay.Visibility = _isInitialized ? Visibility.Collapsed : Visibility.Visible;
                        LogDebug($"✅ LoadingOverlay visibility = {loadingOverlay.Visibility}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"⚠️ UpdateUIVisibility error: {ex.Message}", ex);
                }
            });
        }

        private void ShowLoadingState(string message)
        {
            if (_xamlLoadFailed)
            {
                LogDebug($"⚠️ ShowLoadingState skipped due to XAML error: {message}");
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;
                    var loadingText = this.FindName("LoadingText") as TextBlock;

                    if (loadingOverlay != null)
                    {
                        loadingOverlay.Visibility = Visibility.Visible;
                    }

                    if (loadingText != null)
                    {
                        loadingText.Text = message;
                    }

                    LogDebug($"📺 Loading state shown: {message}");
                }
                catch (Exception ex)
                {
                    LogError($"⚠️ ShowLoadingState error: {ex.Message}", ex);
                }
            });
        }

        private void HideLoadingState()
        {
            if (_xamlLoadFailed)
            {
                LogDebug("⚠️ HideLoadingState skipped due to XAML error");
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;
                    if (loadingOverlay != null)
                    {
                        loadingOverlay.Visibility = Visibility.Collapsed;
                    }

                    LogDebug("📺 Loading state hidden");
                }
                catch (Exception ex)
                {
                    LogError($"⚠️ HideLoadingState error: {ex.Message}", ex);
                }
            });
        }

        #endregion

        #region ✅ Diagnostic Properties s LoggerComponent info

        public bool IsXamlLoaded => !_xamlLoadFailed;

        public string DiagnosticInfo => $"Initialized: {_isInitialized}, XAML: {!_xamlLoadFailed}, LoggerComponent: {_loggerIntegrationEnabled}, Instance: {_componentInstanceId}";

        /// <summary>
        /// ✅ LoggerComponent integration status s rozšírenými detailmi
        /// </summary>
        public string GetLoggerIntegrationStatus()
        {
            if (!_loggerIntegrationEnabled || _integratedLogger == null)
                return $"LoggerComponent integration: DISABLED for DataGrid [{_componentInstanceId}]";

            return $"LoggerComponent integration: ENABLED for DataGrid [{_componentInstanceId}] (File: {_integratedLogger.CurrentLogFile}, Size: {_integratedLogger.CurrentFileSizeMB:F2}MB)";
        }

        #endregion

        #region ✅ Skeleton implementácie s logovaním (aby sa kód skompiloval)

        public async Task LoadDataAsync(DataTable dataTable)
        {
            await LogAsync($"📊 LoadDataAsync(DataTable) with {dataTable?.Rows.Count ?? 0} rows", "INFO");
            await Task.CompletedTask;
        }

        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        // ✅ Skeleton metódy s logovaním
        private void ApplyIndividualColorsToUI()
        {
            LogDebug("🎨 ApplyIndividualColorsToUI called");
        }

        private void InitializeSearchSortZebra()
        {
            LogDebug("🔍 InitializeSearchSortZebra called");
        }

        private void SetupHeaderClickHandlers()
        {
            LogDebug("🖱️ SetupHeaderClickHandlers called");
        }

        private async Task ApplySearchSortZebraAsync()
        {
            LogDebug("🔍 ApplySearchSortZebraAsync called");
            await Task.CompletedTask;
        }

        private async Task CreateInitialEmptyRowsAsync()
        {
            await LogAsync($"🔥 CreateInitialEmptyRowsAsync: creating {_unifiedRowCount} initial rows", "DEBUG");
            await Task.CompletedTask;
        }

        private async Task InitializeDataOnlyAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount,
            DataGridColorConfig? colorConfig)
        {
            await LogAsync("🔧 Initializing data-only without UI...", "INFO");
            await Task.CompletedTask;
        }

        private async Task InitializeServicesAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount)
        {
            await LogAsync("🔧 Initializing services...", "DEBUG");
            await Task.CompletedTask;
        }

        #endregion

        #region INotifyPropertyChanged & IDisposable s logovaním

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
                LogInfo($"🧹 AdvancedDataGrid [{_componentInstanceId}] disposing...");

                _searchAndSortService?.Dispose();

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                // ✅ LoggerComponent nie je owned by DataGrid, len sa používa
                // Takže ho nedisposujeme, iba resetujeme referenciu
                _integratedLogger = null;
                _loggerIntegrationEnabled = false;

                _isDisposed = true;
                LogInfo($"✅ AdvancedDataGrid [{_componentInstanceId}] disposed with LoggerComponent integration cleanup");
            }
            catch (Exception ex)
            {
                LogError($"❌ Error during dispose: {ex.Message}", ex);
            }
        }

        #endregion
    }
}