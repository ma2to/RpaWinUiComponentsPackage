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

            // ✅ KĽÚČOVÁ Auto-Add logika:
            // Ak má viac dát ako minimum → vytvorí data.Count + 1 prázdny riadok
            // Ak má menej dát ako minimum → vytvorí minimum riadkov + 1 prázdny riadok
            var requiredDataRows = data.Count;
            var totalRowsNeeded = Math.Max(requiredDataRows + 1, _minimumRowCount + 1); // +1 pre prázdny riadok

            // Pridaj prázdne riadky až do požadovaného počtu
            while (_gridData.Count < totalRowsNeeded)
            {
                _gridData.Add(CreateEmptyRow());
            }

            // ✅ Volaj data management service
            await _dataManagementService.LoadDataAsync(_gridData);

            _logger.LogDebug("AUTO-ADD: Načítané {DataCount} riadkov dát, celkom {TotalCount} riadkov (vrátane {EmptyCount} prázdnych)",
                data.Count, _gridData.Count, totalRowsNeeded - data.Count);
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
                _logger.LogDebug("AUTO-ADD: Pridaný nový prázdny riadok na koniec (celkom: {TotalRows})", _gridData.Count);
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Metóda volaná pri editácii bunky - zabezpeč auto-add nových riadkov
        /// </summary>
        public void OnCellValueChanged(int rowIndex, string columnName, object? newValue)
        {
            try
            {
                if (!_autoAddEnabled || IsSpecialColumn(columnName))
                    return;

                // Ak editujem posledný riadok a nie je už prázdny
                if (rowIndex == _gridData.Count - 1)
                {
                    var lastRow = _gridData[rowIndex];
                    if (!IsRowEmpty(lastRow))
                    {
                        // Pridaj nový prázdny riadok
                        _gridData.Add(CreateEmptyRow());
                        _logger.LogDebug("AUTO-ADD: Vyplnený posledný riadok → pridaný nový prázdny (celkom: {TotalRows})", _gridData.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri AUTO-ADD OnCellValueChanged");
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

        /// <summary>
        /// Skontroluje či je stĺpec špeciálny (neráta sa do Auto-Add logiky)
        /// </summary>
        private bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteRows" || columnName == "ValidAlerts";
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

        #region ✅ NOVÉ: Public Test Methods pre Demo aplikáciu

        /// <summary>
        /// Test metóda pre auto-add s malým počtom riadkov (menej ako minimum)
        /// </summary>
        public async Task TestAutoAddFewRowsAsync()
        {
            try
            {
                _logger.LogInformation("AUTO-ADD TEST: TestAutoAddFewRowsAsync začína...");

                // Načítaj 3 riadky (menej ako minimum 5)
                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 201, ["Meno"] = "Test User 1", ["Email"] = "test1@auto.add", ["Vek"] = 25, ["Plat"] = 2500m },
                    new() { ["ID"] = 202, ["Meno"] = "Test User 2", ["Email"] = "test2@auto.add", ["Vek"] = 30, ["Plat"] = 3000m },
                    new() { ["ID"] = 203, ["Meno"] = "Test User 3", ["Email"] = "test3@auto.add", ["Vek"] = 35, ["Plat"] = 3500m }
                };

                await LoadDataAsync(testData);

                // Malo by byť: minimálne riadky (5) + 1 prázdny = 6 riadkov
                _logger.LogInformation("AUTO-ADD TEST: Načítané {DataCount} riadky, výsledok: {TotalCount} riadkov",
                    testData.Count, CurrentRowCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba v TestAutoAddFewRowsAsync");
                throw;
            }
        }

        /// <summary>
        /// Test metóda pre auto-add s veľkým počtom riadkov (viac ako minimum)
        /// </summary>
        public async Task TestAutoAddManyRowsAsync()
        {
            try
            {
                _logger.LogInformation("AUTO-ADD TEST: TestAutoAddManyRowsAsync začína...");

                // Načítaj 20 riadkov (viac ako minimum 5)
                var testData = new List<Dictionary<string, object?>>();
                for (int i = 1; i <= 20; i++)
                {
                    testData.Add(new Dictionary<string, object?>
                    {
                        ["ID"] = 300 + i,
                        ["Meno"] = $"Bulk User {i}",
                        ["Email"] = $"bulk{i}@auto.add",
                        ["Vek"] = 20 + (i % 40),
                        ["Plat"] = 2000m + (i * 100)
                    });
                }

                await LoadDataAsync(testData);

                // Malo by byť: 20 dátových riadkov + 1 prázdny = 21 riadkov
                _logger.LogInformation("AUTO-ADD TEST: Načítané {DataCount} riadky, výsledok: {TotalCount} riadkov",
                    testData.Count, CurrentRowCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba v TestAutoAddManyRowsAsync");
                throw;
            }
        }

        /// <summary>
        /// Test metóda pre auto-add inteligentné mazanie
        /// </summary>
        public async Task TestAutoAddDeleteAsync()
        {
            try
            {
                _logger.LogInformation("AUTO-ADD DELETE TEST: TestAutoAddDeleteAsync začína...");

                // Najprv načítaj dáta
                await TestAutoAddFewRowsAsync();

                // Test custom delete pravidiel s auto-add ochranou
                var deleteRules = new List<GridValidationRule>
                {
                    GridValidationRule.Custom("Vek", value =>
                    {
                        if (int.TryParse(value?.ToString(), out var age))
                            return age < 30; // Zmaž mladších ako 30
                        return false;
                    }, "Too young - deleted by auto-add test")
                };

                await DeleteRowsByCustomValidationAsync(deleteRules);

                _logger.LogInformation("AUTO-ADD DELETE TEST: Po delete operácii: {TotalCount} riadkov", CurrentRowCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba v TestAutoAddDeleteAsync");
                throw;
            }
        }

        /// <summary>
        /// Test metóda pre realtime validácie
        /// </summary>
        public async Task TestRealtimeValidationAsync()
        {
            try
            {
                _logger.LogInformation("REALTIME VALIDATION TEST: TestRealtimeValidationAsync začína...");

                // Načítaj dáta s validačnými chybami
                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 401, ["Meno"] = "", ["Email"] = "invalid-email", ["Vek"] = 150, ["Plat"] = -1000m },
                    new() { ["ID"] = 402, ["Meno"] = "X", ["Email"] = "", ["Vek"] = 5, ["Plat"] = 999999m },
                    new() { ["ID"] = 403, ["Meno"] = "Valid User", ["Email"] = "valid@test.com", ["Vek"] = 25, ["Plat"] = 3000m }
                };

                await LoadDataAsync(testData);

                // Spustiť validáciu
                var isValid = await ValidateAllRowsAsync();

                _logger.LogInformation("REALTIME VALIDATION TEST: Validácia dokončená, všetko validné: {IsValid}", isValid);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba v TestRealtimeValidationAsync");
                throw;
            }
        }

        /// <summary>
        /// Test metóda pre navigáciu (Tab/Enter/Esc)
        /// </summary>
        public async Task TestNavigationAsync()
        {
            try
            {
                _logger.LogInformation("NAVIGATION TEST: TestNavigationAsync začína...");

                // Jednoducho načítaj dáta pre navigáciu
                await TestAutoAddFewRowsAsync();

                _logger.LogInformation("NAVIGATION TEST: Dáta pripravené pre navigačný test - použite Tab/Enter/Esc v bunkách");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba v TestNavigationAsync");
                throw;
            }
        }

        /// <summary>
        /// Test metóda pre copy/paste funkcionalitu
        /// </summary>
        public async Task TestCopyPasteAsync()
        {
            try
            {
                _logger.LogInformation("COPY/PASTE TEST: TestCopyPasteAsync začína...");

                // Načítaj dáta pre copy/paste test
                await TestAutoAddFewRowsAsync();

                _logger.LogInformation("COPY/PASTE TEST: Dáta pripravené pre copy/paste test - použite Ctrl+C/V/X");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba v TestCopyPasteAsync");
                throw;
            }
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
    }
}