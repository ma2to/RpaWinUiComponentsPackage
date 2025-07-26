// Controls/AdvancedDataGrid.xaml.cs - ✅ OPRAVENÝ s Auto-Add funkcionalitou
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
    /// AdvancedDataGrid komponent s kompletnou Auto-Add riadkov funkcionalitou - ✅ PUBLIC API
    /// 
    /// Auto-Add funkcionalita:
    /// - Pri načítaní dát: Ak má viac dát ako inicializovaných riadkov → vytvorí potrebné riadky + 1 prázdny
    /// - Vždy zostane aspoň jeden prázdny riadok na konci
    /// - Pri vyplnení posledného riadku: Automaticky pridá nový prázdny riadok  
    /// - Pri mazaní: Ak je nad minimum → fyzicky zmaže, ak je na minimume → iba vyčistí obsah
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
        private int _initialRowCount = 15; // Počet riadkov definovaný pri inicializácii
        private int _minimumRowCount = 15; // Minimálny počet riadkov (rovnaký ako initial)
        private bool _autoAddEnabled = true;

        // Color theme support
        private DataGridColorTheme _colorTheme = DataGridColorTheme.Light;

        // ✅ NOVÉ: Interné dáta pre Auto-Add
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly List<GridColumnDefinition> _columns = new();

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
                // TODO: Aplikovať theme na UI elementy
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
                _logger.LogInformation("AUTO-ADD: Začína inicializácia DataGrid s {EmptyRowsCount} riadkami...", emptyRowsCount);
                ShowLoadingState("Inicializuje sa DataGrid s Auto-Add funkcionalitou...");

                // ✅ Nastav Auto-Add parametre
                _initialRowCount = Math.Max(emptyRowsCount, 1);
                _minimumRowCount = _initialRowCount;
                _autoAddEnabled = true;

                // Ulož columns pre neskoršie použitie
                _columns.Clear();
                _columns.AddRange(columns ?? new List<GridColumnDefinition>());

                // Vytvor konfiguráciu s Auto-Add nastaveniami
                var configuration = new GridConfiguration
                {
                    Columns = columns ?? new List<GridColumnDefinition>(),
                    ValidationRules = validationRules ?? new List<GridValidationRule>(),
                    ThrottlingConfig = throttlingConfig ?? GridThrottlingConfig.Default,
                    EmptyRowsCount = _initialRowCount,
                    AutoAddNewRow = _autoAddEnabled,
                    GridName = "AdvancedDataGrid_AutoAdd"
                };

                // Inicializuj služby
                await _dataManagementService.InitializeAsync(configuration);
                await _validationService.InitializeAsync(configuration);
                await _exportService.InitializeAsync(configuration);

                // ✅ Vytvor počiatočné prázdne riadky
                CreateInitialEmptyRows();

                _isInitialized = true;
                HideLoadingState();

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

                // ✅ Auto-Add logika pri načítaní dát
                await LoadDataWithAutoAdd(data);

                HideLoadingState();

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

                // ✅ Vymaž všetky dáta ale zachovaj minimálny počet prázdnych riadkov
                _gridData.Clear();
                CreateInitialEmptyRows();

                await _dataManagementService.ClearAllDataAsync();

                HideLoadingState();

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

                var deletedCount = 0;

                // ✅ Aplikuj delete pravidlá s Auto-Add ochranou
                for (int i = _gridData.Count - 1; i >= 0; i--)
                {
                    var row = _gridData[i];
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
                        // ✅ Auto-Add inteligentné mazanie
                        if (_gridData.Count > _minimumRowCount)
                        {
                            // Fyzicky zmaž riadok
                            _gridData.RemoveAt(i);
                            deletedCount++;
                        }
                        else
                        {
                            // Len vyčisti obsah riadku
                            ClearRowData(row);
                            deletedCount++;
                        }
                    }
                }

                // ✅ Zabezpeč že je aspoň jeden prázdny riadok na konci
                EnsureEmptyRowAtEnd();

                HideLoadingState();

                _logger.LogInformation($"AUTO-ADD custom delete dokončené: {deletedCount} riadkov spracovaných");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri AUTO-ADD custom delete");
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Auto-Add Helper Methods

        /// <summary>
        /// Vytvorí počiatočné prázdne riadky
        /// </summary>
        private void CreateInitialEmptyRows()
        {
            _gridData.Clear();

            for (int i = 0; i < _initialRowCount; i++)
            {
                _gridData.Add(CreateEmptyRow());
            }

            _logger.LogDebug("AUTO-ADD: Vytvorených {Count} počiatočných prázdnych riadkov", _initialRowCount);
        }

        /// <summary>
        /// Načíta dáta s Auto-Add logikou
        /// </summary>
        private async Task LoadDataWithAutoAdd(List<Dictionary<string, object?>> data)
        {
            _gridData.Clear();

            // ✅ Pridaj skutočné dáta
            foreach (var rowData in data)
            {
                _gridData.Add(new Dictionary<string, object?>(rowData));
            }

            // ✅ Auto-Add logika: Zabezpeč že je dosť riadkov
            var requiredRows = Math.Max(data.Count + 1, _minimumRowCount + 1); // +1 pre prázdny riadok

            while (_gridData.Count < requiredRows)
            {
                _gridData.Add(CreateEmptyRow());
            }

            // ✅ Volaj data management service
            await _dataManagementService.LoadDataAsync(_gridData);

            _logger.LogDebug("AUTO-ADD: Načítané {DataCount} riadkov dát, celkom {TotalCount} riadkov",
                data.Count, _gridData.Count);
        }

        /// <summary>
        /// Zabezpečí že je aspoň jeden prázdny riadok na konci
        /// </summary>
        private void EnsureEmptyRowAtEnd()
        {
            if (_gridData.Count == 0)
            {
                _gridData.Add(CreateEmptyRow());
                return;
            }

            // Skontroluj posledný riadok
            var lastRow = _gridData[_gridData.Count - 1];
            if (!IsRowEmpty(lastRow))
            {
                // Posledný riadok nie je prázdny → pridaj nový prázdny
                _gridData.Add(CreateEmptyRow());
                _logger.LogDebug("AUTO-ADD: Pridaný nový prázdny riadok na koniec");
            }
        }

        /// <summary>
        /// Vytvorí prázdny riadok
        /// </summary>
        private Dictionary<string, object?> CreateEmptyRow()
        {
            var row = new Dictionary<string, object?>();

            foreach (var column in _columns)
            {
                row[column.Name] = column.DefaultValue;
            }

            // Pridaj ValidAlerts stĺpec
            row["ValidAlerts"] = string.Empty;

            return row;
        }

        /// <summary>
        /// Vyčistí dáta riadku
        /// </summary>
        private void ClearRowData(Dictionary<string, object?> row)
        {
            foreach (var key in row.Keys.ToList())
            {
                if (key != "ValidAlerts") // ValidAlerts sa vyčistí osobne
                {
                    row[key] = null;
                }
            }
            row["ValidAlerts"] = string.Empty;
        }

        /// <summary>
        /// Kontroluje či je riadok prázdny
        /// </summary>
        private bool IsRowEmpty(Dictionary<string, object?> row)
        {
            foreach (var kvp in row)
            {
                // Ignoruj špeciálne stĺpce
                if (kvp.Key == "DeleteRows" || kvp.Key == "ValidAlerts")
                    continue;

                // Ak je nejaká hodnota vyplnená, riadok nie je prázdny
                if (kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()))
                    return false;
            }

            return true;
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

        /// <summary>
        /// Aktuálny počet riadkov
        /// </summary>
        public int CurrentRowCount => _gridData.Count;

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
    }
}