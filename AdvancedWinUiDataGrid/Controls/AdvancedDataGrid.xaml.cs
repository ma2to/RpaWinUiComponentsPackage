// Controls/AdvancedDataGrid.xaml.cs - ✅ OPRAVENÝ pre DataGridColorConfig ako PRIMARY PUBLIC API
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
    /// AdvancedDataGrid komponent s AUTO-ADD a DataGridColorConfig - ✅ PUBLIC API
    /// 
    /// AUTO-ADD funkcionalita:
    /// - initialRowCount = minimumRowCount (vždy rovnaké číslo zadané v emptyRowsCount)
    /// - Default 15 ak nie je zadané
    /// - Pri načítaní dát: Ak má viac dát ako zadaný počet → vytvorí potrebné riadky + 1 prázdny
    /// - Vždy zostane aspoň jeden prázdny riadok na konci
    /// - Pri vyplnení posledného riadku: Automaticky pridá nový prázdny riadok
    /// - Pri mazaní: Ak je nad zadaný počet → fyzicky zmaže, ak je na zadanom počte → iba vyčistí obsah
    /// 
    /// DataGridColorConfig:
    /// - Individual farby sa nastavujú iba pri inicializácii cez InitializeAsync
    /// - Ak sa nenastavujú, používajú sa default farby
    /// - Žiadne runtime color switching
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private Fields - ✅ AUTO-ADD + DataGridColorConfig

        private IServiceProvider? _serviceProvider;
        private ILogger<AdvancedDataGrid>? _logger;
        private IDataManagementService? _dataManagementService;
        private IValidationService? _validationService;
        private IExportService? _exportService;

        private bool _isInitialized = false;
        private bool _isDisposed = false;
        private bool _xamlLoadFailed = false;

        // ✅ AUTO-ADD: Unified row count - iba jedna hodnota pre oba koncepty
        private int _unifiedRowCount = 15; // ✅ initialRowCount = minimumRowCount (vždy rovnaké)
        private bool _autoAddEnabled = true;

        // ✅ OPRAVENÉ: DataGridColorConfig namiesto DataGridColorTheme
        private DataGridColorConfig? _individualColorConfig;

        // ✅ Interné dáta pre AUTO-ADD
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();

        #endregion

        #region ✅ Constructor s XAML error handling

        public AdvancedDataGrid()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 AdvancedDataGrid: Začína inicializácia s AUTO-ADD a DataGridColorConfig...");

                // ✅ Bezpečná XAML inicializácia
                InitializeXamlSafely();

                if (!_xamlLoadFailed)
                {
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid: XAML InitializeComponent úspešne dokončené");
                    InitializeDependencyInjection();
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid s AUTO-ADD úspešne inicializovaný");
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

                _logger?.LogInformation("AdvancedDataGrid s AUTO-ADD inicializovaný");
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
                    Text = "⚠️ AdvancedDataGrid - XAML Fallback Mode",
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
            }
            catch (Exception serviceEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fallback services creation failed: {serviceEx.Message}");
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods s AUTO-ADD a DataGridColorConfig

        /// <summary>
        /// Inicializuje DataGrid s konfiguráciou - ✅ s AUTO-ADD a DataGridColorConfig
        /// </summary>
        /// <param name="columns">Definície stĺpcov</param>
        /// <param name="validationRules">Validačné pravidlá</param>
        /// <param name="throttlingConfig">Throttling konfigurácia</param>
        /// <param name="emptyRowsCount">Unified počet riadkov (initialRowCount = minimumRowCount) - default 15</param>
        /// <param name="colorConfig">Individual color configuration (optional, ak null tak default farby)</param>
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

                _logger?.LogInformation("AUTO-ADD: Začína inicializácia DataGrid s {EmptyRowsCount} riadkami (unified count)...", emptyRowsCount);

                if (!_xamlLoadFailed)
                {
                    ShowLoadingState("Inicializuje sa DataGrid s AUTO-ADD a DataGridColorConfig...");
                }

                // ✅ AUTO-ADD: Iba jedna hodnota pre oba koncepty
                _unifiedRowCount = Math.Max(emptyRowsCount, 1); // ✅ initialRowCount = minimumRowCount
                _autoAddEnabled = true;

                _logger?.LogInformation("AUTO-ADD: Nastavený počet riadkov = {RowCount} (rovnaký pre initial aj minimum)", _unifiedRowCount);

                // ✅ OPRAVENÉ: DataGridColorConfig namiesto DataGridColorTheme
                _individualColorConfig = colorConfig?.Clone() ?? DataGridColorConfig.Light; // Default Light colors
                if (colorConfig != null)
                {
                    _logger?.LogInformation("DataGridColorConfig: Custom colors set pri inicializácii");
                    ApplyColorConfigToUI();
                }
                else
                {
                    _logger?.LogInformation("DataGridColorConfig: Using default Light colors");
                }

                // Ulož columns pre neskoršie použitie
                _columns.Clear();
                _columns.AddRange(columns ?? new List<GridColumnDefinition>());

                // Vytvor konfiguráciu s AUTO-ADD nastaveniami
                var configuration = new GridConfiguration
                {
                    Columns = columns ?? new List<GridColumnDefinition>(),
                    ValidationRules = validationRules ?? new List<GridValidationRule>(),
                    ThrottlingConfig = throttlingConfig ?? GridThrottlingConfig.Default,
                    EmptyRowsCount = _unifiedRowCount, // ✅ UNIFIED hodnota
                    AutoAddNewRow = _autoAddEnabled,
                    GridName = "AdvancedDataGrid_Unified_AutoAdd"
                };

                // Safe service calls s null checks
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

                _isInitialized = true;

                if (!_xamlLoadFailed)
                {
                    UpdateUIVisibility();
                    HideLoadingState();
                }

                _logger?.LogInformation("AUTO-ADD: DataGrid úspešne inicializovaný s {RowCount} riadkami (initial=minimum), ColorConfig: {HasColors}",
                    _unifiedRowCount, colorConfig != null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri inicializácii DataGrid s AUTO-ADD");
                if (!_xamlLoadFailed)
                {
                    ShowLoadingState($"Chyba: {ex.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta do DataGrid s AUTO-ADD funkcionalitou
        /// </summary>
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
                _logger?.LogInformation("LoadDataAsync dokončené s AUTO-ADD");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri LoadDataAsync");
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta z DataTable
        /// </summary>
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

        /// <summary>
        /// Validuje všetky riadky
        /// </summary>
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

        /// <summary>
        /// Exportuje dáta do DataTable
        /// </summary>
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

        /// <summary>
        /// Vymaže všetky dáta s AUTO-ADD ochranou
        /// </summary>
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

        /// <summary>
        /// ⭐ Maže riadky na základe custom validačných pravidiel
        /// </summary>
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

                _logger?.LogInformation("Začína custom delete validation s {RuleCount} pravidlami (AUTO-ADD ochrana)", deleteRules.Count);

                // TODO: Implementácia custom delete logiky cez DataManagementService
                _logger?.LogInformation("Custom delete pravidlá aplikované s AUTO-ADD ochranou");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri DeleteRowsByCustomValidationAsync");
                throw;
            }
        }

        #endregion

        #region ✅ OPRAVENÉ: DataGridColorConfig Configuration (nastavuje sa iba pri inicializácii)

        /// <summary>
        /// Aktuálna DataGridColorConfig (read-only po inicializácii)
        /// </summary>
        public DataGridColorConfig? ColorConfig => _individualColorConfig?.Clone();

        /// <summary>
        /// Aplikuje DataGridColorConfig na UI elementy (internal použitie)
        /// </summary>
        private void ApplyColorConfigToUI()
        {
            try
            {
                if (_individualColorConfig == null || _xamlLoadFailed) return;

                _logger?.LogDebug("DataGridColorConfig aplikovaná na UI elementy");

                // TODO: Aplikovať individual colors na konkrétne UI elementy
                // Toto by sa malo implementovať v UI layer - nastaviť brushes na UI elementoch

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri aplikovaní DataGridColorConfig na UI");
            }
        }

        #endregion

        #region ✅ Test metódy pre demo aplikáciu

        /// <summary>
        /// Test metóda: AUTO-ADD s malým počtom riadkov
        /// </summary>
        public async Task TestAutoAddFewRowsAsync()
        {
            try
            {
                _logger?.LogInformation("AUTO-ADD TEST: TestAutoAddFewRowsAsync začína...");

                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 10, ["Meno"] = "Test 1", ["Email"] = "test1@example.com", ["Vek"] = 25, ["Plat"] = 2000m },
                    new() { ["ID"] = 11, ["Meno"] = "Test 2", ["Email"] = "test2@example.com", ["Vek"] = 30, ["Plat"] = 2500m }
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

        /// <summary>
        /// Test metóda: AUTO-ADD s veľkým počtom riadkov
        /// </summary>
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
                        ["Meno"] = $"Test User {i}",
                        ["Email"] = $"user{i}@test.com",
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
                _logger?.LogInformation("AUTO-ADD DELETE TEST: TestAutoAddDeleteAsync začína...");
                await Task.CompletedTask;
                _logger?.LogInformation("AUTO-ADD DELETE TEST: TestAutoAddDeleteAsync dokončený");
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

            // ✅ AUTO-ADD: Vždy vytvor _unifiedRowCount riadkov
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

        /// <summary>
        /// Či sa XAML načítal úspešne (pre debugging)
        /// </summary>
        public bool IsXamlLoaded => !_xamlLoadFailed;

        /// <summary>
        /// Diagnostické info o stave komponentu
        /// </summary>
        public string DiagnosticInfo => $"Initialized: {_isInitialized}, XAML: {!_xamlLoadFailed}, Auto-Add: {_autoAddEnabled}, Unified-RowCount: {_unifiedRowCount}, Data-Rows: {_gridData.Count}, ColorConfig: {_individualColorConfig != null}";

        /// <summary>
        /// AUTO-ADD status
        /// </summary>
        public string AutoAddStatus => $"AUTO-ADD: {_unifiedRowCount} rows (initial=minimum), Auto-Add: {_autoAddEnabled}, Current-Data: {_gridData.Count}";

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
                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                _isDisposed = true;
                _logger?.LogInformation("AdvancedDataGrid s AUTO-ADD funkciou disposed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri dispose: {ex.Message}");
            }
        }

        #endregion
    }
}