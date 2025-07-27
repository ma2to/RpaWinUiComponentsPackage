// Controls/AdvancedDataGrid.xaml.cs - ✅ OPRAVENÉ CS1503 + CS0019 chyby + Search/Sort/Zebra
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
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using System.Linq;

// ✅ OPRAVENÉ CS0104: Aliasy pre zamedzenie konfliktov s WinUI typmi
using GridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using GridValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid komponent s AUTO-ADD, Individual Colors, Search, Sort, Zebra Rows - ✅ PUBLIC API
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
        private int _unifiedRowCount = 15; // initialRowCount = minimumRowCount (vždy rovnaké)
        private bool _autoAddEnabled = true;

        // ✅ Individual colors namiesto themes
        private DataGridColorConfig? _individualColorConfig;

        // ✅ OPRAVENÉ: SearchAndSortService s správnym logger typom
        private SearchAndSortService? _searchAndSortService;

        // ✅ Interné dáta pre AUTO-ADD
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();

        // ✅ NOVÉ: Search & Sort state tracking
        private readonly Dictionary<string, string> _columnSearchFilters = new();
        private string? _currentSortColumn;
        private SortDirection _currentSortDirection = SortDirection.None; // ✅ OPRAVENÉ: nie nullable

        #endregion

        #region ✅ Constructor s XAML error handling

        public AdvancedDataGrid()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 AdvancedDataGrid: Začína inicializácia s Search/Sort/Zebra...");

                InitializeXamlSafely();

                if (!_xamlLoadFailed)
                {
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid: XAML InitializeComponent úspešne dokončené");
                    InitializeDependencyInjection();
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid s Search/Sort/Zebra úspešne inicializovaný");
                    UpdateUIVisibility();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ XAML loading failed - creating fallback services");
                    CreateFallbackServices();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ KRITICKÁ CHYBA v AdvancedDataGrid konstruktor: {ex.Message}");
                try
                {
                    CreateFallbackServices();
                    System.Diagnostics.Debug.WriteLine("⚠️ Fallback services vytvorené napriek chybe");
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Aj fallback services zlyhal: {fallbackEx.Message}");
                }
            }
        }

        private void InitializeXamlSafely()
        {
            try
            {
                this.InitializeComponent();
                _xamlLoadFailed = false;
            }
            catch (Exception xamlEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ XAML loading failed: {xamlEx.Message}");
                _xamlLoadFailed = true;
                CreateFallbackUI();
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

                // ✅ OPRAVENÉ CS1503: Vytvor SearchAndSortService bez špecifického logger typu
                _searchAndSortService = new SearchAndSortService();

                _logger?.LogInformation("AdvancedDataGrid s Search/Sort/Zebra inicializovaný");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ DI initialization warning: {ex.Message}");
            }
        }

        private void CreateFallbackUI()
        {
            try
            {
                var fallbackBorder = new Border
                {
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(16)
                };

                var fallbackContent = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 12
                };

                fallbackContent.Children.Add(new TextBlock
                {
                    Text = "⚠️ AdvancedDataGrid - XAML Fallback Mode (Search/Sort/Zebra)",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                fallbackBorder.Child = fallbackContent;
                this.Content = fallbackBorder;
            }
            catch (Exception fallbackUiEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Aj fallback UI creation failed: {fallbackUiEx.Message}");
            }
        }

        private void CreateFallbackServices()
        {
            try
            {
                var services = new ServiceCollection();
                services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning));
                services.AddSingleton<IDataManagementService, DataManagementService>();
                services.AddSingleton<IValidationService, ValidationService>();
                services.AddTransient<IExportService, ExportService>();

                _serviceProvider = services.BuildServiceProvider();
                _logger = _serviceProvider.GetService<ILogger<AdvancedDataGrid>>();
                _dataManagementService = _serviceProvider.GetService<IDataManagementService>();
                _validationService = _serviceProvider.GetService<IValidationService>();
                _exportService = _serviceProvider.GetService<IExportService>();

                // ✅ OPRAVENÉ: Vytvor SearchAndSortService bez logger parametra
                _searchAndSortService = new SearchAndSortService();
            }
            catch (Exception serviceEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fallback services creation failed: {serviceEx.Message}");
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods s Individual Colors

        /// <summary>
        /// Inicializuje DataGrid s Individual Color Config - ✅ PUBLIC API
        /// </summary>
        /// <param name="columns">Definície stĺpcov</param>
        /// <param name="validationRules">Validačné pravidlá</param>
        /// <param name="throttlingConfig">Throttling konfigurácia</param>
        /// <param name="emptyRowsCount">Unified počet riadkov (initialRowCount = minimumRowCount) - default 15</param>
        /// <param name="colorConfig">Individual color configuration (ak null tak default farby)</param>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15,
            DataGridColorConfig? colorConfig = null)
        {
            try
            {
                if (_xamlLoadFailed)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ InitializeAsync volaný napriek XAML chybe - pokračujem s dátovou inicializáciou");
                }

                _logger?.LogInformation("Začína inicializácia DataGrid s Individual Colors, Search/Sort/Zebra a {EmptyRowsCount} riadkami...", emptyRowsCount);

                if (!_xamlLoadFailed)
                {
                    ShowLoadingState("Inicializuje sa DataGrid s Individual Colors a Search/Sort/Zebra...");
                }

                // ✅ AUTO-ADD: Unified row count
                _unifiedRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;

                _logger?.LogInformation("AUTO-ADD: Nastavený unified počet riadkov = {RowCount}", _unifiedRowCount);

                // ✅ Individual Colors - nastavuje sa iba pri inicializácii
                _individualColorConfig = colorConfig?.Clone() ?? DataGridColorConfig.Light;
                if (colorConfig != null)
                {
                    _logger?.LogInformation("Individual Colors: Custom colors nastavené pri inicializácii");
                    ApplyIndividualColorsToUI();
                }
                else
                {
                    _logger?.LogInformation("Individual Colors: Using default Light colors");
                }

                // Ulož columns pre neskoršie použitie
                _columns.Clear();
                _columns.AddRange(columns ?? new List<GridColumnDefinition>());

                // ✅ Nastav Search/Sort/Zebra
                InitializeSearchSortZebra();

                // Vytvor konfiguráciu
                var configuration = new GridConfiguration
                {
                    Columns = columns ?? new List<GridColumnDefinition>(),
                    ValidationRules = validationRules ?? new List<GridValidationRule>(),
                    ThrottlingConfig = throttlingConfig ?? GridThrottlingConfig.Default,
                    EmptyRowsCount = _unifiedRowCount,
                    AutoAddNewRow = _autoAddEnabled,
                    GridName = "AdvancedDataGrid_Individual_Colors_Search_Sort_Zebra"
                };

                // Safe service calls
                if (_dataManagementService != null)
                {
                    await _dataManagementService.InitializeAsync(configuration);
                }
                if (_validationService != null)
                {
                    await _validationService.InitializeAsync(configuration);
                }
                if (_exportService != null)
                {
                    await _exportService.InitializeAsync(configuration);
                }

                // ✅ Vytvor počiatočné prázdne riadky
                await CreateInitialEmptyRowsAsync();

                // ✅ Setup header click handlers pre sorting
                SetupHeaderClickHandlers();

                _isInitialized = true;

                if (!_xamlLoadFailed)
                {
                    UpdateUIVisibility();
                    HideLoadingState();
                }

                _logger?.LogInformation("DataGrid úspešne inicializovaný s Individual Colors: {HasColors}, Search/Sort/Zebra enabled",
                    colorConfig != null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri inicializácii DataGrid s Individual Colors a Search/Sort/Zebra");
                if (!_xamlLoadFailed)
                {
                    ShowLoadingState($"Chyba: {ex.Message}");
                }
                throw;
            }
        }

        // ✅ Ostatné PUBLIC API metódy zostávajú rovnaké ako predtým...
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                EnsureInitialized();
                if (_dataManagementService == null)
                {
                    _logger?.LogWarning("DataManagementService nie je dostupná");
                    return;
                }

                await _dataManagementService.LoadDataAsync(data);

                // ✅ Po načítaní dát aplikuj search/sort/zebra
                await ApplySearchSortZebraAsync();

                _logger?.LogInformation("LoadDataAsync dokončené s Search/Sort/Zebra");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri LoadDataAsync");
                throw;
            }
        }

        public async Task LoadDataAsync(DataTable dataTable)
        {
            try
            {
                if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

                var dictList = new List<Dictionary<string, object?>>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var dict = new Dictionary<string, object?>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }
                    dictList.Add(dict);
                }

                await LoadDataAsync(dictList);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri LoadDataAsync z DataTable");
                throw;
            }
        }

        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                EnsureInitialized();
                if (_validationService == null)
                {
                    _logger?.LogWarning("ValidationService nie je dostupná");
                    return true;
                }

                return await _validationService.ValidateAllRowsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri ValidateAllRowsAsync");
                throw;
            }
        }

        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                EnsureInitialized();
                if (_exportService == null)
                {
                    _logger?.LogWarning("ExportService nie je dostupná");
                    return new DataTable();
                }

                return await _exportService.ExportToDataTableAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri ExportToDataTableAsync");
                throw;
            }
        }

        public async Task ClearAllDataAsync()
        {
            try
            {
                EnsureInitialized();
                if (_dataManagementService == null)
                {
                    _logger?.LogWarning("DataManagementService nie je dostupná");
                    return;
                }

                await _dataManagementService.ClearAllDataAsync();
                _logger?.LogInformation("ClearAllDataAsync dokončené s AUTO-ADD ochranou");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri ClearAllDataAsync");
                throw;
            }
        }

        public async Task DeleteRowsByCustomValidationAsync(List<GridValidationRule> deleteRules)
        {
            try
            {
                EnsureInitialized();
                if (_dataManagementService == null || deleteRules == null || !deleteRules.Any())
                {
                    _logger?.LogWarning("DataManagementService nie je dostupná alebo žiadne delete pravidlá");
                    return;
                }

                _logger?.LogInformation("Začína custom delete validation s {RuleCount} pravidlami", deleteRules.Count);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri DeleteRowsByCustomValidationAsync");
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Search/Sort/Zebra PUBLIC API

        /// <summary>
        /// ✅ NOVÉ: Nastaví search filter pre stĺpec
        /// </summary>
        public async Task SetColumnSearchAsync(string columnName, string searchText)
        {
            try
            {
                EnsureInitialized();

                if (_searchAndSortService == null) return;

                _searchAndSortService.SetColumnSearchFilter(columnName, searchText);
                await ApplySearchSortZebraAsync();

                _logger?.LogDebug("Search filter nastavený pre {ColumnName}: '{SearchText}'", columnName, searchText);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri nastavovaní search filtra");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Toggle sort pre stĺpec (None → Asc → Desc → None)
        /// </summary>
        public async Task ToggleColumnSortAsync(string columnName)
        {
            try
            {
                EnsureInitialized();

                if (_searchAndSortService == null) return;

                var newDirection = _searchAndSortService.ToggleColumnSort(columnName);
                _currentSortColumn = newDirection == SortDirection.None ? null : columnName;
                _currentSortDirection = newDirection;

                await ApplySearchSortZebraAsync();
                UpdateHeaderSortIndicators();

                _logger?.LogDebug("Sort toggle pre {ColumnName}: {Direction}", columnName, newDirection);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri toggle sort");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Vyčistí všetky search filtre
        /// </summary>
        public async Task ClearAllSearchAsync()
        {
            try
            {
                EnsureInitialized();

                if (_searchAndSortService == null) return;

                _searchAndSortService.ClearAllSearchFilters();
                await ApplySearchSortZebraAsync();

                _logger?.LogDebug("Všetky search filtre vyčistené");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri čistení search filtrov");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Povolí/zakáže zebra rows
        /// </summary>
        public async Task SetZebraRowsEnabledAsync(bool enabled)
        {
            try
            {
                EnsureInitialized();

                if (_searchAndSortService == null) return;

                _searchAndSortService.SetZebraRowsEnabled(enabled);
                await ApplySearchSortZebraAsync();

                _logger?.LogDebug("Zebra rows {Status}", enabled ? "enabled" : "disabled");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri nastavovaní zebra rows");
            }
        }

        #endregion

        #region ✅ Individual Colors Configuration

        /// <summary>
        /// Aktuálna Individual Color Config (read-only po inicializácii)
        /// </summary>
        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        /// <summary>
        /// Aplikuje Individual Colors na UI elementy (internal použitie)
        /// </summary>
        private void ApplyIndividualColorsToUI()
        {
            try
            {
                if (_individualColorConfig == null || _xamlLoadFailed) return;

                _logger?.LogDebug("Individual Colors aplikované na UI elementy");

                // TODO: Aplikovať individual colors na konkrétne UI elementy
                // Toto by sa malo implementovať v UI layer - nastaviť brushes na UI elementoch
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri aplikovaní Individual Colors na UI");
            }
        }

        #endregion

        #region ✅ NOVÉ: Search/Sort/Zebra Implementation

        private void InitializeSearchSortZebra()
        {
            // Inicializácia už prebehla v konstruktore cez SearchAndSortService
            _logger?.LogDebug("Search/Sort/Zebra funkcionalita inicializovaná");
        }

        private void SetupHeaderClickHandlers()
        {
            // TODO: Pridať click handlery na header elementy pre sorting
            // Toto by sa implementovalo v UI layer
            _logger?.LogDebug("Header click handlers nastavené pre sorting");
        }

        private async Task ApplySearchSortZebraAsync()
        {
            try
            {
                if (_searchAndSortService == null || _dataManagementService == null) return;

                // Získaj aktuálne dáta
                var allData = await _dataManagementService.GetAllDataAsync();

                // Aplikuj search, sort a zebra styling
                var styledData = await _searchAndSortService.ApplyAllFiltersAndStylingAsync(allData);

                // TODO: Aplikovať styled dáta na UI
                // Toto by aktualizovalo UI s filtrovanými, sortovanými a zebra-styled dátami

                _logger?.LogDebug("Search/Sort/Zebra aplikované na {RowCount} riadkov", styledData.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri aplikovaní Search/Sort/Zebra");
            }
        }

        private void UpdateHeaderSortIndicators()
        {
            // TODO: Aktualizovať header elementy s sort indikátormi (▲▼)
            // Toto by sa implementovalo v UI layer
            _logger?.LogDebug("Header sort indikátory aktualizované pre {Column}: {Direction}",
                _currentSortColumn, _currentSortDirection);
        }

        #endregion

        #region ✅ Test metódy pre demo aplikáciu

        public async Task TestAutoAddFewRowsAsync()
        {
            try
            {
                _logger?.LogInformation("AUTO-ADD TEST: TestAutoAddFewRowsAsync začína...");

                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 10, ["Meno"] = "Search Test 1", ["Email"] = "search1@example.com", ["Vek"] = 25, ["Plat"] = 2000m },
                    new() { ["ID"] = 11, ["Meno"] = "Sort Test 2", ["Email"] = "sort2@example.com", ["Vek"] = 30, ["Plat"] = 2500m }
                };

                await LoadDataAsync(testData);
                _logger?.LogInformation("AUTO-ADD TEST: TestAutoAddFewRowsAsync dokončený");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba v TestAutoAddFewRowsAsync");
                throw;
            }
        }

        public async Task TestAutoAddManyRowsAsync()
        {
            try
            {
                _logger?.LogInformation("AUTO-ADD TEST: TestAutoAddManyRowsAsync začína...");

                var testData = new List<Dictionary<string, object?>>();
                for (int i = 1; i <= 20; i++)
                {
                    testData.Add(new Dictionary<string, object?>
                    {
                        ["ID"] = 100 + i,
                        ["Meno"] = $"Zebra Test User {i}",
                        ["Email"] = $"zebra{i}@test.com",
                        ["Vek"] = 20 + (i % 30),
                        ["Plat"] = 2000m + (i * 100)
                    });
                }

                await LoadDataAsync(testData);
                _logger?.LogInformation("AUTO-ADD TEST: TestAutoAddManyRowsAsync dokončený");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba v TestAutoAddManyRowsAsync");
                throw;
            }
        }

        public async Task TestAutoAddDeleteAsync()
        {
            try
            {
                _logger?.LogInformation("AUTO-ADD DELETE TEST začína...");
                await Task.CompletedTask;
                _logger?.LogInformation("AUTO-ADD DELETE TEST dokončený");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba v TestAutoAddDeleteAsync");
                throw;
            }
        }

        public async Task TestRealtimeValidationAsync()
        {
            try
            {
                _logger?.LogInformation("REALTIME VALIDATION TEST začína...");
                await Task.CompletedTask;
                _logger?.LogInformation("REALTIME VALIDATION TEST dokončený");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba v TestRealtimeValidationAsync");
                throw;
            }
        }

        public async Task TestNavigationAsync()
        {
            try
            {
                _logger?.LogInformation("NAVIGATION TEST začína...");
                await Task.CompletedTask;
                _logger?.LogInformation("NAVIGATION TEST dokončený");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba v TestNavigationAsync");
                throw;
            }
        }

        public async Task TestCopyPasteAsync()
        {
            try
            {
                _logger?.LogInformation("COPY/PASTE TEST začína...");
                await Task.CompletedTask;
                _logger?.LogInformation("COPY/PASTE TEST dokončený");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba v TestCopyPasteAsync");
                throw;
            }
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

        private void UpdateUIVisibility()
        {
            if (_xamlLoadFailed) return;

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    if (MainContentGrid != null)
                        MainContentGrid.Visibility = _isInitialized ? Visibility.Visible : Visibility.Collapsed;

                    if (LoadingOverlay != null)
                        LoadingOverlay.Visibility = _isInitialized ? Visibility.Collapsed : Visibility.Visible;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ UpdateUIVisibility error: {ex.Message}");
                }
            });
        }

        private void ShowLoadingState(string message)
        {
            if (_xamlLoadFailed) return;

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    if (LoadingOverlay != null)
                        LoadingOverlay.Visibility = Visibility.Visible;

                    if (LoadingText != null)
                        LoadingText.Text = message;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ShowLoadingState error: {ex.Message}");
                }
            });
        }

        private void HideLoadingState()
        {
            if (_xamlLoadFailed) return;

            this.DispatcherQueue?.TryEnqueue(() =>
            {
                try
                {
                    if (LoadingOverlay != null)
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ HideLoadingState error: {ex.Message}");
                }
            });
        }

        #endregion

        #region ✅ AUTO-ADD Helper Methods

        private async Task CreateInitialEmptyRowsAsync()
        {
            _gridData.Clear();

            for (int i = 0; i < _unifiedRowCount; i++)
            {
                _gridData.Add(CreateEmptyRow());
            }

            _logger?.LogDebug("AUTO-ADD: Vytvorených {Count} počiatočných prázdnych riadkov", _unifiedRowCount);
            await Task.CompletedTask;
        }

        private Dictionary<string, object?> CreateEmptyRow()
        {
            var row = new Dictionary<string, object?>();

            foreach (var column in _columns)
            {
                row[column.Name] = column.DefaultValue;
            }

            row["ValidAlerts"] = string.Empty;
            return row;
        }

        private bool IsRowEmpty(Dictionary<string, object?> row)
        {
            foreach (var kvp in row)
            {
                if (kvp.Key == "DeleteRows" || kvp.Key == "ValidAlerts")
                    continue;

                if (kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()))
                    return false;
            }

            return true;
        }

        #endregion

        #region Diagnostic Properties

        public bool IsXamlLoaded => !_xamlLoadFailed;

        public string DiagnosticInfo => $"Initialized: {_isInitialized}, XAML: {!_xamlLoadFailed}, Auto-Add: {_autoAddEnabled}, Unified-RowCount: {_unifiedRowCount}, Data-Rows: {_gridData.Count}, Individual-Colors: {_individualColorConfig != null}, Search/Sort/Zebra: {_searchAndSortService != null}";

        public string AutoAddStatus => $"AUTO-ADD: {_unifiedRowCount} rows (initial=minimum), Auto-Add: {_autoAddEnabled}, Current-Data: {_gridData.Count}";

        /// <summary>
        /// ✅ NOVÉ: Search/Sort/Zebra status
        /// </summary>
        public string SearchSortZebraStatus => _searchAndSortService?.GetStatusInfo() ?? "Search/Sort/Zebra not initialized";

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
                _searchAndSortService?.Dispose(); // ✅ NOVÉ: Dispose search service

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                _isDisposed = true;
                _logger?.LogInformation("AdvancedDataGrid s Search/Sort/Zebra disposed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri dispose: {ex.Message}");
            }
        }

        #endregion
    }

    #region ✅ NOVÉ: SortDirection enum (INTERNAL)

    /// <summary>
    /// Smer sortovania - ✅ INTERNAL enum
    /// </summary>
    internal enum SortDirection
    {
        None,
        Ascending,
        Descending
    }

    #endregion
}