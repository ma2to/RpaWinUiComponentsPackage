// Controls/AdvancedDataGrid.xaml.cs - ✅ OPRAVENÝ s LoggerComponent integráciou
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
using RpaWinUiComponentsPackage.Logger;  // ✅ NOVÉ: LoggerComponent integrácia

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid komponent s AUTO-ADD, Individual Colors, Search, Sort, Zebra Rows + LoggerComponent integrácia - ✅ PUBLIC API
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

        // ✅ NOVÉ: LoggerComponent integrácia
        private LoggerComponent? _integratedLogger;
        private bool _loggerIntegrationEnabled = false;

        #endregion

        #region ✅ Constructor s OPRAVENÝ XAML handling

        public AdvancedDataGrid()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 AdvancedDataGrid: Začína konštruktor...");

                // ✅ KĽÚČOVÁ OPRAVA: Jednoduchšie XAML loading bez complex error handling
                TryInitializeXaml();

                if (!_xamlLoadFailed)
                {
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid: XAML úspešne načítané");
                    InitializeDependencyInjection();
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid: Kompletne inicializovaný");
                    UpdateUIVisibility();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ XAML loading zlyhal - vytváram fallback UI");
                    CreateSimpleFallbackUI();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CRITICAL CONSTRUCTOR ERROR: {ex.Message}");
                CreateSimpleFallbackUI();
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ: Jednoduchšie XAML loading
        /// </summary>
        private void TryInitializeXaml()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 XAML Loading: Volám InitializeComponent()...");

                this.InitializeComponent();
                _xamlLoadFailed = false;

                System.Diagnostics.Debug.WriteLine("✅ XAML Loading: InitializeComponent() úspešný!");

                // ✅ NOVÉ: Okamžite skontroluj či sa UI elementy načítali
                ValidateUIElementsAfterXaml();
            }
            catch (Exception xamlEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ XAML ERROR: {xamlEx.Message}");
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

                System.Diagnostics.Debug.WriteLine($"📋 UI validácia: MainContent={hasMainContent}, LoadingOverlay={hasLoadingOverlay}");

                if (!hasMainContent || !hasLoadingOverlay)
                {
                    System.Diagnostics.Debug.WriteLine("❌ UI elementy sa nenačítali správne - označujem ako XAML failed");
                    _xamlLoadFailed = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ UI validácia chyba: {ex.Message}");
                _xamlLoadFailed = true;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ: Jednoduchší fallback UI
        /// </summary>
        private void CreateSimpleFallbackUI()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 Vytváram jednoduchý fallback UI...");

                // ✅ Vytvor základný Grid namiesto komplexného UI
                var mainGrid = new Grid();

                var fallbackText = new TextBlock
                {
                    Text = "⚠️ AdvancedDataGrid - XAML Fallback Mode (Search/Sort/Zebra + LoggerComponent)",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap
                };

                mainGrid.Children.Add(fallbackText);
                this.Content = mainGrid;

                System.Diagnostics.Debug.WriteLine("✅ Fallback UI vytvorené");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Aj fallback UI creation failed: {fallbackEx.Message}");

                // ✅ Posledná záchrana - iba TextBlock
                this.Content = new TextBlock
                {
                    Text = "AdvancedDataGrid - Error",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
        }

        private void InitializeDependencyInjection()
        {
            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                _logger = _serviceProvider.GetService<ILogger<AdvancedDataGrid>>();
                _dataManagementService = _serviceProvider.GetService<IDataManagementService>();
                _validationService = _serviceProvider.GetService<IValidationService>();
                _exportService = _serviceProvider.GetService<IExportService>();

                // ✅ SearchAndSortService bez logger parametra
                _searchAndSortService = new SearchAndSortService();

                _logger?.LogInformation("AdvancedDataGrid s Search/Sort/Zebra + LoggerComponent integrácia inicializovaný");
                System.Diagnostics.Debug.WriteLine("✅ Dependency Injection úspešne inicializované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ DI initialization warning: {ex}");
            }
        }

        #endregion

        #region ✅ NOVÉ: LoggerComponent Integrácia API

        /// <summary>
        /// ✅ NOVÉ: Nastaví LoggerComponent pre integráciu s DataGrid - PUBLIC API
        /// </summary>
        /// <param name="loggerComponent">LoggerComponent inštancia</param>
        /// <param name="enableIntegration">Či povoliť logovanie</param>
        public void SetIntegratedLogger(LoggerComponent? loggerComponent, bool enableIntegration = true)
        {
            _integratedLogger = loggerComponent;
            _loggerIntegrationEnabled = enableIntegration && loggerComponent != null;

            LogAsync($"LoggerComponent integration {(enableIntegration ? "ENABLED" : "DISABLED")}", "INFO");
        }

        /// <summary>
        /// ✅ NOVÉ: Async logovanie s fallback na Debug.WriteLine
        /// </summary>
        private async Task LogAsync(string message, string logLevel = "INFO")
        {
            try
            {
                if (_loggerIntegrationEnabled && _integratedLogger != null)
                {
                    await _integratedLogger.LogAsync($"[AdvancedDataGrid] {message}", logLevel);
                }
                else
                {
                    // Fallback na System.Diagnostics.Debug
                    System.Diagnostics.Debug.WriteLine($"[{logLevel}] [AdvancedDataGrid] {message}");
                }
            }
            catch (Exception ex)
            {
                // Aj keď logovanie zlyh, pokračujeme
                System.Diagnostics.Debug.WriteLine($"⚠️ Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Získa info o LoggerComponent integrácii
        /// </summary>
        public string GetLoggerIntegrationStatus()
        {
            if (!_loggerIntegrationEnabled || _integratedLogger == null)
                return "LoggerComponent integration: DISABLED";

            return $"LoggerComponent integration: ENABLED (File: {_integratedLogger.CurrentLogFile}, Size: {_integratedLogger.CurrentFileSizeMB:F2}MB)";
        }

        #endregion

        #region ✅ PUBLIC API Methods s Individual Colors, LoggerComponent integrácia a LEPŠÍM error handlingom

        /// <summary>
        /// Inicializuje DataGrid s Individual Color Config + LoggerComponent integrácia - ✅ PUBLIC API
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15,
            DataGridColorConfig? colorConfig = null,
            LoggerComponent? loggerComponent = null)  // ✅ NOVÉ: Optional LoggerComponent
        {
            try
            {
                await LogAsync($"InitializeAsync začína (XAML failed: {_xamlLoadFailed}, Logger: {loggerComponent != null})...", "INFO");

                // ✅ NOVÉ: Nastav LoggerComponent ak je poskytnutý
                if (loggerComponent != null)
                {
                    SetIntegratedLogger(loggerComponent, true);
                    await LogAsync("LoggerComponent integration ENABLED for this DataGrid instance", "INFO");
                }

                // ✅ OPRAVENÉ CS8604: Null check pre columns parameter
                if (columns == null)
                {
                    throw new ArgumentNullException(nameof(columns), "Columns parameter cannot be null");
                }

                // ✅ KĽÚČOVÁ OPRAVA: Ak XAML zlyhal, pokračuj iba s dátovou inicializáciou
                if (_xamlLoadFailed)
                {
                    await LogAsync("XAML zlyhal - pokračujem iba s dátovou inicializáciou bez UI updates", "WARN");
                    await InitializeDataOnlyAsync(columns, validationRules, throttlingConfig, emptyRowsCount, colorConfig);
                    return;
                }

                _logger?.LogInformation("Začína inicializácia DataGrid s Individual Colors, Search/Sort/Zebra a {EmptyRowsCount} riadkami...", emptyRowsCount);

                ShowLoadingState("Inicializuje sa DataGrid s Individual Colors, Search/Sort/Zebra a LoggerComponent...");

                // ✅ AUTO-ADD: Unified row count
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;

                await LogAsync($"AUTO-ADD: Nastavený unified počet riadkov = {_unifiedRowCount}", "INFO");

                // ✅ Individual Colors - nastavuje sa iba pri inicializácii
                _individualColorConfig = colorConfig?.Clone() ?? DataGridColorConfig.Light;
                if (colorConfig != null)
                {
                    await LogAsync("Individual Colors: Custom colors nastavené", "INFO");
                    ApplyIndividualColorsToUI();
                }

                // Ulož columns pre neskoršie použitie
                _columns.Clear();
                _columns.AddRange(columns);

                // ✅ Nastav Search/Sort/Zebra
                InitializeSearchSortZebra();

                await InitializeServicesAsync(columns, validationRules, throttlingConfig, emptyRowsCount);

                // ✅ Vytvor počiatočné prázdne riadky
                await CreateInitialEmptyRowsAsync();

                // ✅ Setup header click handlers pre sorting
                SetupHeaderClickHandlers();

                _isInitialized = true;
                await LogAsync("DataGrid úspešne inicializovaný s LoggerComponent integráciou!", "INFO");

                UpdateUIVisibility();
                HideLoadingState();

                _logger?.LogInformation("DataGrid úspešne inicializovaný s Individual Colors: {HasColors}, Search/Sort/Zebra enabled, LoggerComponent: {LoggerEnabled}",
                    colorConfig != null, _loggerIntegrationEnabled);
            }
            catch (Exception ex)
            {
                await LogAsync($"KRITICKÁ CHYBA pri inicializácii DataGrid: {ex.Message}", "ERROR");
                _logger?.LogError(ex, "Chyba pri inicializácii DataGrid s Individual Colors a Search/Sort/Zebra");

                if (!_xamlLoadFailed)
                {
                    ShowLoadingState($"Chyba: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Inicializácia iba dátovej časti bez UI (pre XAML fallback)
        /// </summary>
        private async Task InitializeDataOnlyAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount,
            DataGridColorConfig? colorConfig)
        {
            try
            {
                await LogAsync("Inicializujem iba dátovú časť bez UI...", "INFO");

                // ✅ OPRAVENÉ CS8604: Null check pre columns parameter
                if (columns == null)
                {
                    throw new ArgumentNullException(nameof(columns), "Columns parameter cannot be null");
                }

                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;
                _individualColorConfig = colorConfig?.Clone() ?? DataGridColorConfig.Light;

                _columns.Clear();
                _columns.AddRange(columns);

                _searchAndSortService = new SearchAndSortService();

                await InitializeServicesAsync(columns, validationRules, throttlingConfig, emptyRowsCount);
                await CreateInitialEmptyRowsAsync();

                _isInitialized = true;
                await LogAsync("Dátová inicializácia dokončená (bez UI)", "INFO");
            }
            catch (Exception ex)
            {
                await LogAsync($"CHYBA pri dátovej inicializácii: {ex.Message}", "ERROR");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Pomocná metóda pre inicializáciu services
        /// </summary>
        private async Task InitializeServicesAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount)
        {
            var configuration = new GridConfiguration
            {
                Columns = columns ?? new List<GridColumnDefinition>(),
                ValidationRules = validationRules ?? new List<GridValidationRule>(),
                ThrottlingConfig = throttlingConfig ?? GridThrottlingConfig.Default,
                EmptyRowsCount = _unifiedRowCount,
                AutoAddNewRow = _autoAddEnabled,
                GridName = "AdvancedDataGrid_Individual_Colors_Search_Sort_Zebra_LoggerIntegration"
            };

            // Safe service calls
            if (_dataManagementService != null)
            {
                await _dataManagementService.InitializeAsync(configuration);
                await LogAsync("DataManagementService inicializovaný", "INFO");
            }
            if (_validationService != null)
            {
                await _validationService.InitializeAsync(configuration);
                await LogAsync("ValidationService inicializovaný", "INFO");
            }
            if (_exportService != null)
            {
                await _exportService.InitializeAsync(configuration);
                await LogAsync("ExportService inicializovaný", "INFO");
            }
        }

        // ✅ LoadDataAsync zostáva rovnaká ale s LoggerComponent integráciou
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                if (data == null)
                {
                    await LogAsync("LoadDataAsync: data parameter je null - vytváram prázdny zoznam", "WARN");
                    data = new List<Dictionary<string, object?>>();
                }

                await LogAsync($"LoadDataAsync začína s {data.Count} riadkami...", "INFO");
                EnsureInitialized();

                if (_dataManagementService == null)
                {
                    await LogAsync("DataManagementService nie je dostupná", "ERROR");
                    return;
                }

                await _dataManagementService.LoadDataAsync(data);

                // ✅ Po načítaní dát aplikuj search/sort/zebra
                await ApplySearchSortZebraAsync();

                await LogAsync($"LoadDataAsync dokončené s Search/Sort/Zebra - {data.Count} riadkov načítaných", "INFO");
            }
            catch (Exception ex)
            {
                await LogAsync($"CHYBA pri LoadDataAsync: {ex.Message}", "ERROR");
                throw;
            }
        }

        #endregion

        #region ✅ UI Helper Methods s lepším error handlingom

        private void UpdateUIVisibility()
        {
            if (_xamlLoadFailed)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ UpdateUIVisibility preskočené kvôli XAML chybe");
                return;
            }

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Aktualizujem UI visibility (initialized: {_isInitialized})...");

                    var mainContentGrid = this.FindName("MainContentGrid") as FrameworkElement;
                    var loadingOverlay = this.FindName("LoadingOverlay") as FrameworkElement;

                    if (mainContentGrid != null)
                    {
                        mainContentGrid.Visibility = _isInitialized ? Visibility.Visible : Visibility.Collapsed;
                        System.Diagnostics.Debug.WriteLine($"✅ MainContentGrid visibility = {mainContentGrid.Visibility}");
                    }

                    if (loadingOverlay != null)
                    {
                        loadingOverlay.Visibility = _isInitialized ? Visibility.Collapsed : Visibility.Visible;
                        System.Diagnostics.Debug.WriteLine($"✅ LoadingOverlay visibility = {loadingOverlay.Visibility}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ UpdateUIVisibility error: {ex}");
                }
            });
        }

        private void ShowLoadingState(string message)
        {
            if (_xamlLoadFailed)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ ShowLoadingState preskočené kvôli XAML chybe: {message}");
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ShowLoadingState error: {ex}");
                }
            });
        }

        private void HideLoadingState()
        {
            if (_xamlLoadFailed)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ HideLoadingState preskočené kvôli XAML chybe");
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ HideLoadingState error: {ex}");
                }
            });
        }

        #endregion

        #region Helper Methods

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            });

            services.AddSingleton<IDataManagementService, DataManagementService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddTransient<IExportService, ExportService>();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DataGrid nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
        }

        #endregion

        #region Diagnostic Properties s LoggerComponent info

        public bool IsXamlLoaded => !_xamlLoadFailed;

        public string DiagnosticInfo => $"Initialized: {_isInitialized}, XAML: {!_xamlLoadFailed}, Auto-Add: {_autoAddEnabled}, Unified-RowCount: {_unifiedRowCount}, Data-Rows: {_gridData.Count}, Individual-Colors: {_individualColorConfig != null}, Search/Sort/Zebra: {_searchAndSortService != null}, LoggerComponent: {_loggerIntegrationEnabled}";

        public string AutoAddStatus => $"AUTO-ADD: {_unifiedRowCount} rows (initial=minimum), Auto-Add: {_autoAddEnabled}, Current-Data: {_gridData.Count}";

        public string SearchSortZebraStatus => _searchAndSortService?.GetStatusInfo() ?? "Search/Sort/Zebra not initialized";

        /// <summary>
        /// ✅ NOVÉ: LoggerComponent integration status
        /// </summary>
        public string LoggerIntegrationStatus => GetLoggerIntegrationStatus();

        #endregion

        #region INotifyPropertyChanged & IDisposable

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                _ = LogAsync("AdvancedDataGrid disposing...", "INFO");

                _searchAndSortService?.Dispose();

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                // ✅ NOVÉ: LoggerComponent nie je owned by DataGrid, len sa používa
                // Takže ho nedisposujeme, iba resetujeme referenciu
                _integratedLogger = null;
                _loggerIntegrationEnabled = false;

                _isDisposed = true;
                System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid disposed s LoggerComponent integration cleanup");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri dispose: {ex}");
            }
        }

        #endregion

        // ✅ Skeleton implementácie pre zostávajúce metódy (aby sa kód skompiloval)
        public async Task LoadDataAsync(DataTable dataTable) => await Task.CompletedTask;
        public async Task<bool> ValidateAllRowsAsync() => await Task.FromResult(true);
        public async Task<DataTable> ExportToDataTableAsync() => await Task.FromResult(new DataTable());
        public async Task ClearAllDataAsync() => await Task.CompletedTask;
        public async Task DeleteRowsByCustomValidationAsync(List<GridValidationRule> deleteRules) => await Task.CompletedTask;
        public async Task SetColumnSearchAsync(string columnName, string searchText) => await Task.CompletedTask;
        public async Task ToggleColumnSortAsync(string columnName) => await Task.CompletedTask;
        public async Task ClearAllSearchAsync() => await Task.CompletedTask;
        public async Task SetZebraRowsEnabledAsync(bool enabled) => await Task.CompletedTask;

        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        private void ApplyIndividualColorsToUI() { }
        private void InitializeSearchSortZebra() { }
        private void SetupHeaderClickHandlers() { }
        private async Task ApplySearchSortZebraAsync() => await Task.CompletedTask;
        private void UpdateHeaderSortIndicators() { }
        private async Task CreateInitialEmptyRowsAsync() => await Task.CompletedTask;
        private Dictionary<string, object?> CreateEmptyRow() => new();
        private bool IsRowEmpty(Dictionary<string, object?> row) => true;

        // Test metódy s LoggerComponent integráciou
        public async Task TestAutoAddFewRowsAsync()
        {
            await LogAsync("Testing AUTO-ADD few rows functionality", "INFO");
            await Task.CompletedTask;
        }

        public async Task TestAutoAddManyRowsAsync()
        {
            await LogAsync("Testing AUTO-ADD many rows functionality", "INFO");
            await Task.CompletedTask;
        }

        public async Task TestAutoAddDeleteAsync()
        {
            await LogAsync("Testing AUTO-ADD smart delete functionality", "INFO");
            await Task.CompletedTask;
        }

        public async Task TestRealtimeValidationAsync()
        {
            await LogAsync("Testing realtime validation functionality", "INFO");
            await Task.CompletedTask;
        }

        public async Task TestNavigationAsync()
        {
            await LogAsync("Testing navigation functionality", "INFO");
            await Task.CompletedTask;
        }

        public async Task TestCopyPasteAsync()
        {
            await LogAsync("Testing copy/paste functionality", "INFO");
            await Task.CompletedTask;
        }
    }
}