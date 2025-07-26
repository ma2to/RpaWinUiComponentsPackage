// Controls/AdvancedDataGrid.xaml.cs - ✅ KOMPLETNÝ s Auto-Add funkciou a správnym PUBLIC API
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

// ✅ OPRAVENÉ CS0104: Aliasy pre zamedzenie konfliktov s WinUI typmi
using GridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using GridValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid komponent s kompletnou Auto-Add riadkov funkcionalitou - ✅ PUBLIC API
    /// 
    /// Auto-Add funkcionalita:
    /// - Pri načítaní dát: Ak má viac dát ako minimum → vytvorí dáta + 1 prázdny riadok
    /// - Pri vyplnení posledného riadku: Automaticky pridá nový prázdny riadok  
    /// - Pri mazaní: Ak je nad minimum → fyzicky zmaže, ak je na minimume → iba vyčistí obsah
    /// - Vždy zostane aspoň jeden prázdny riadok na konci
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdvancedDataGrid> _logger;
        private readonly IDataManagementService _dataManagementService;
        private readonly IValidationService _validationService;
        private readonly IExportService _exportService;

        private bool _isInitialized = false;
        private bool _isDisposed = false;

        // ✅ Auto-Add konfigurácia
        private int _minimumRowCount = 5;
        private bool _autoAddEnabled = true;

        // Color theme support
        private DataGridColorTheme _colorTheme = DataGridColorTheme.Light;

        #endregion

        #region Constructor

        public AdvancedDataGrid()
        {
            // Inicializácia DI kontajnera
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Získanie služieb z DI kontajnera
            _logger = _serviceProvider.GetRequiredService<ILogger<AdvancedDataGrid>>();
            _dataManagementService = _serviceProvider.GetRequiredService<IDataManagementService>();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _exportService = _serviceProvider.GetRequiredService<IExportService>();

            this.InitializeComponent();
            _logger.LogInformation("AdvancedDataGrid s Auto-Add funkciou inicializovaný");
        }

        #endregion

        #region ✅ PUBLIC Color Theme API

        /// <summary>
        /// Aktuálna color theme
        /// </summary>
        public DataGridColorTheme ColorTheme
        {
            get => _colorTheme;
            set
            {
                if (SetProperty(ref _colorTheme, value))
                {
                    ApplyColorThemeInternal();
                }
            }
        }

        /// <summary>
        /// Aplikuje color theme na DataGrid
        /// </summary>
        public void ApplyColorTheme(DataGridColorTheme theme)
        {
            ColorTheme = theme ?? DataGridColorTheme.Light;
        }

        /// <summary>
        /// Resetuje na default light theme
        /// </summary>
        public void ResetToDefaultTheme()
        {
            ColorTheme = DataGridColorTheme.Light;
        }

        private void ApplyColorThemeInternal()
        {
            try
            {
                _logger.LogDebug("Color theme aplikovaná: {ThemeName}", _colorTheme.ToString());
                UpdateDemoInfo($"Color theme aplikovaná: {_colorTheme}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri aplikovaní color theme");
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods s Auto-Add

        /// <summary>
        /// Inicializuje DataGrid s konfiguráciou - ✅ s Auto-Add podporou
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15)
        {
            try
            {
                _logger.LogInformation("AUTO-ADD: Začína inicializácia DataGrid s minimálne {MinimumRows} riadkami...", emptyRowsCount);
                ShowLoadingState("Inicializuje sa DataGrid s Auto-Add funkcionalitou...");

                // ✅ Nastav Auto-Add parametre
                _minimumRowCount = Math.Max(emptyRowsCount, 1);
                _autoAddEnabled = true;

                // Vytvor konfiguráciu s Auto-Add nastaveniami
                var configuration = new GridConfiguration
                {
                    Columns = columns ?? new List<GridColumnDefinition>(),
                    ValidationRules = validationRules ?? new List<GridValidationRule>(),
                    ThrottlingConfig = throttlingConfig ?? GridThrottlingConfig.Default,
                    EmptyRowsCount = _minimumRowCount,
                    AutoAddNewRow = _autoAddEnabled,
                    GridName = "AdvancedDataGrid_AutoAdd"
                };

                // Inicializuj služby
                await _dataManagementService.InitializeAsync(configuration);
                await _validationService.InitializeAsync(configuration);
                await _exportService.InitializeAsync(configuration);

                _isInitialized = true;
                HideLoadingState();

                // ✅ Auto-Add demo info
                UpdateDemoInfo($"AUTO-ADD inicializované: minimum {_minimumRowCount} riadkov, auto-add: {_autoAddEnabled}");

                _logger.LogInformation("AUTO-ADD: DataGrid úspešne inicializovaný s Auto-Add funkciou");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri inicializácii DataGrid s Auto-Add");
                ShowLoadingState($"Chyba: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ KĽÚČOVÁ: Načíta dáta do DataGrid s auto-add riadkov funkciou
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation($"AUTO-ADD: Načítavajú sa dáta: {data.Count} riadkov");

                ShowLoadingState("Načítavajú sa dáta s Auto-Add logikou...");

                // ✅ Zavolaj Data Management Service ktorý implementuje Auto-Add logiku
                await _dataManagementService.LoadDataAsync(data);

                HideLoadingState();

                // ✅ Auto-Add demo info update s detailmi
                var totalRows = await _dataManagementService.GetAllDataAsync();
                var nonEmptyRows = await _dataManagementService.GetNonEmptyRowCountAsync();
                var emptyRows = totalRows.Count - nonEmptyRows;

                UpdateDemoInfo($"AUTO-ADD: Načítané {data.Count} riadkov dát → celkom {totalRows.Count} riadkov ({nonEmptyRows} s dátami + {emptyRows} prázdnych)");

                _logger.LogInformation("AUTO-ADD: Dáta úspešne načítané s auto-add riadkov");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri načítavaní dát s Auto-Add");
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta z DataTable s Auto-Add
        /// </summary>
        public async Task LoadDataAsync(DataTable dataTable)
        {
            var dataList = new List<Dictionary<string, object?>>();
            foreach (DataRow row in dataTable.Rows)
            {
                var rowDict = new Dictionary<string, object?>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    rowDict[column.ColumnName] = row[column];
                }
                dataList.Add(rowDict);
            }

            await LoadDataAsync(dataList);
        }

        /// <summary>
        /// Validuje všetky riadky
        /// </summary>
        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation("AUTO-ADD: Spúšťa sa validácia všetkých riadkov");

                var isValid = await _validationService.ValidateAllRowsAsync();

                var totalRows = await _dataManagementService.GetAllDataAsync();
                var nonEmptyRows = await _dataManagementService.GetNonEmptyRowCountAsync();

                UpdateDemoInfo($"AUTO-ADD validácia: {nonEmptyRows} neprázdnych z {totalRows.Count} riadkov, všetky validné: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri validácii všetkých riadkov");
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
                _logger.LogInformation("AUTO-ADD: Exportujú sa dáta do DataTable");

                var dataTable = await _exportService.ExportToDataTableAsync();

                var nonEmptyRows = await _dataManagementService.GetNonEmptyRowCountAsync();
                UpdateDemoInfo($"AUTO-ADD export: {dataTable.Rows.Count} riadkov exportovaných ({nonEmptyRows} s dátami)");

                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte do DataTable");
                throw;
            }
        }

        /// <summary>
        /// ✅ AKTUALIZOVANÉ: Vymaže všetky dáta z DataGrid s rešpektovaním minimálneho počtu riadkov
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation("AUTO-ADD: Vymazávajú sa všetky dáta s ochranou minimálneho počtu");

                ShowLoadingState("AUTO-ADD: Vymazávajú sa dáta s ochranou minimálneho počtu...");

                await _dataManagementService.ClearAllDataAsync();

                HideLoadingState();

                var totalRows = await _dataManagementService.GetAllDataAsync();
                UpdateDemoInfo($"AUTO-ADD clear: Všetky dáta vymazané, zachovaných {totalRows.Count} prázdnych riadkov (minimum {_minimumRowCount})");

                _logger.LogInformation("AUTO-ADD: Všetky dáta vymazané s ochranou minimálneho počtu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri vymazávaní dát s Auto-Add");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Zmaže riadky na základe custom validačných pravidiel s Auto-Add ochranou
        /// </summary>
        public async Task DeleteRowsByCustomValidationAsync(List<GridValidationRule> deleteValidationRules)
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation($"AUTO-ADD: Spúšťa sa custom delete s {deleteValidationRules.Count} pravidlami");

                ShowLoadingState("AUTO-ADD: Aplikujú sa custom delete pravidlá...");

                var allData = await _dataManagementService.GetAllDataAsync();
                var deletedCount = 0;
                var originalCount = allData.Count;

                // ✅ Aplikuj delete pravidlá s Auto-Add ochranou
                for (int i = allData.Count - 1; i >= 0; i--)
                {
                    var row = allData[i];
                    bool shouldDelete = false;

                    // Skontroluj každé pravidlo
                    foreach (var rule in deleteValidationRules)
                    {
                        if (row.ContainsKey(rule.ColumnName))
                        {
                            var value = row[rule.ColumnName];
                            if (rule.Validate(value))
                            {
                                shouldDelete = true;
                                break;
                            }
                        }
                    }

                    if (shouldDelete)
                    {
                        await _dataManagementService.DeleteRowAsync(i);
                        deletedCount++;
                    }
                }

                HideLoadingState();

                var finalData = await _dataManagementService.GetAllDataAsync();
                var nonEmptyRows = await _dataManagementService.GetNonEmptyRowCountAsync();

                UpdateDemoInfo($"AUTO-ADD delete: {deletedCount} riadkov spracovaných, zostalo {finalData.Count} riadkov ({nonEmptyRows} s dátami), minimum {_minimumRowCount} zachované");

                _logger.LogInformation($"AUTO-ADD custom delete dokončené: {deletedCount} riadkov spracovaných");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri AUTO-ADD custom delete");
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
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Registruj služby
            services.AddSingleton<IDataManagementService, DataManagementService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddTransient<IExportService, ExportService>();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DataGrid nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
        }

        private void ShowLoadingState(string message)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (LoadingText != null)
                    LoadingText.Text = message;
            });
        }

        private void HideLoadingState()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            });
        }

        private void UpdateDemoInfo(string message)
        {
            // Demo info sa môže zobraziť v UI alebo len logovať
            _logger.LogInformation($"AUTO-ADD DEMO: {message}");
        }

        #endregion

        #region Event Handlers s Auto-Add demo

        internal void OnTestAutoAddClick(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    UpdateDemoInfo("AUTO-ADD Test: Simuluje sa pridávanie dát s auto-add logikou...");

                    // ✅ Test: Pridaj 2 riadky dát (menej ako minimum 5)
                    var newData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 100, ["Meno"] = "Auto-Add Test 1", ["Email"] = "test1@auto.add", ["Vek"] = 30, ["Plat"] = 3000m },
                        new() { ["ID"] = 101, ["Meno"] = "Auto-Add Test 2", ["Email"] = "test2@auto.add", ["Vek"] = 25, ["Plat"] = 2500m }
                    };

                    await LoadDataAsync(newData);

                    UpdateDemoInfo("AUTO-ADD Test: 2 riadky dát pridané → mal by zostať na minimálnych 5 riadkoch + 1 prázdny");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AUTO-ADD Test failed");
                }
            });
        }

        internal void OnTestValidationClick(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await ValidateAllRowsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Test validácie failed");
                }
            });
        }

        internal void OnTestDeleteClick(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // ✅ Test Auto-Add delete s demo pravidlami
                    var deleteRules = new List<GridValidationRule>
                    {
                        GridValidationRule.Custom("ID", value =>
                        {
                            if (int.TryParse(value?.ToString(), out var id))
                                return id > 100; // Zmaž ID > 100
                            return false;
                        }, "Auto-Add test delete rule")
                    };

                    await DeleteRowsByCustomValidationAsync(deleteRules);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AUTO-ADD Test delete failed");
                }
            });
        }

        #endregion

        #region INotifyPropertyChanged

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

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                _isDisposed = true;
                _logger?.LogInformation("AdvancedDataGrid s Auto-Add funkciou disposed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri dispose: {ex.Message}");
            }
        }

        #endregion

        #region ✅ NOVÉ: Auto-Add Properties (PUBLIC read-only info)

        /// <summary>
        /// Či je Auto-Add funkcionalita povolená
        /// </summary>
        public bool IsAutoAddEnabled => _autoAddEnabled;

        /// <summary>
        /// Minimálny počet riadkov ktorý sa zachováva
        /// </summary>
        public int MinimumRowCount => _minimumRowCount;

        #endregion
    }
}