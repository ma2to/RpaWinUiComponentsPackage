// Controls/AdvancedDataGrid.xaml.cs - ✅ OPRAVENÝ s Auto-Add funkciou
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ✅ OPRAVENÉ CS0104: Aliasy pre zamedzenie konfliktov s WinUI typmi
using GridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using GridValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;
using GridConfiguration = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.GridConfiguration;
using RowDataModel = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.RowDataModel;
using CellDataModel = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.CellDataModel;

using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Hlavný AdvancedDataGrid komponent pre WinUI3 - ✅ PUBLIC API
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly IValidationService _validationService;
        private readonly IDataManagementService _dataManagementService;
        private readonly ICopyPasteService _copyPasteService;
        private readonly IExportService _exportService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<AdvancedDataGrid> _logger;

        private GridConfiguration? _configuration;

        // ✅ OPRAVENÉ CS0053: INTERNAL properties pre x:Bind (ale internal, nie public!)
        private readonly ObservableCollection<GridColumnDefinition> _headerColumns = new();
        private readonly ObservableCollection<RowDataModel> _dataRows = new();

        // Color theme support
        private DataGridColorTheme _colorTheme = DataGridColorTheme.Light;
        private readonly Dictionary<string, DispatcherTimer> _realtimeValidationTimers = new();

        // Realtime validation support  
        private readonly Dictionary<string, object?> _lastValidatedValues = new();
        private readonly Dictionary<string, bool> _cellEditingStates = new();

        private bool _isInitialized = false;
        private bool _isDisposed = false;

        #endregion

        #region Constructor

        public AdvancedDataGrid()
        {
            // Inicializácia DI kontajnera
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Získanie služieb z DI kontajnera
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _dataManagementService = _serviceProvider.GetRequiredService<IDataManagementService>();
            _copyPasteService = _serviceProvider.GetRequiredService<ICopyPasteService>();
            _exportService = _serviceProvider.GetRequiredService<IExportService>();
            _navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<AdvancedDataGrid>>();

            this.InitializeComponent();
            _logger.LogInformation("AdvancedDataGrid inicializovaný");
        }

        #endregion

        #region ✅ OPRAVENÉ CS0053: INTERNAL Properties pre x:Bind (read-only access)

        /// <summary>
        /// Header stĺpce pre XAML binding - INTERNAL pre x:Bind
        /// </summary>
        internal ObservableCollection<GridColumnDefinition> HeaderColumns => _headerColumns;

        /// <summary>
        /// Dátové riadky pre XAML binding - INTERNAL pre x:Bind
        /// </summary>
        internal ObservableCollection<RowDataModel> DataRows => _dataRows;

        #endregion

        #region ✅ PUBLIC Color Theme API

        /// <summary>
        /// Aktuálna color theme. Setter automaticky aplikuje tému.
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

        /// <summary>
        /// Internes aplikovanie color theme
        /// </summary>
        private void ApplyColorThemeInternal()
        {
            try
            {
                // Theme sa aplikuje cez binding v XAML - len notifikujeme o zmene
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    // Trigger property changed pre všetky color bindings
                    OnPropertyChanged(nameof(ColorTheme));
                });

                _logger.LogDebug("Color theme aplikovaná: {ThemeName}", _colorTheme.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri aplikovaní color theme");
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods

        /// <summary>
        /// Inicializuje DataGrid s konfiguráciou
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15)
        {
            try
            {
                _logger.LogInformation("Začína inicializácia DataGrid...");
                ShowLoadingState("Inicializuje sa DataGrid...");

                // Vytvorenie konfigurácie
                _configuration = new GridConfiguration
                {
                    Columns = columns ?? new List<GridColumnDefinition>(),
                    ValidationRules = validationRules ?? new List<GridValidationRule>(),
                    ThrottlingConfig = throttlingConfig ?? GridThrottlingConfig.Default,
                    EmptyRowsCount = emptyRowsCount
                };

                // Inicializácia služieb
                await _validationService.InitializeAsync(_configuration);
                await _dataManagementService.InitializeAsync(_configuration);
                await _exportService.InitializeAsync(_configuration);

                // Generovanie headers
                GenerateHeaders();

                // Vytvorenie prázdnych riadkov
                await CreateEmptyRowsAsync(emptyRowsCount);

                _isInitialized = true;
                HideLoadingState();

                _logger.LogInformation("DataGrid úspešne inicializovaný");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri inicializácii DataGrid");
                ShowLoadingState($"Chyba: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Načíta dáta do DataGrid s auto-add riadkov funkciou
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation($"Načítavajú sa dáta: {data.Count} riadkov s auto-add funkciou");

                ShowLoadingState("Načítavajú sa dáta...");

                // ✅ NOVÁ FUNKCIONALITA: Data management service automaticky zabezpečí kapacitu
                await _dataManagementService.LoadDataAsync(data);

                await RefreshGridDataAsync();

                HideLoadingState();
                _logger.LogInformation("Dáta úspešne načítané s auto-add riadkov");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri načítavaní dát");
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
                EnsureInitialized();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri načítavaní dát z DataTable");
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
                _logger.LogInformation("Spúšťa sa validácia všetkých riadkov");

                bool hasErrors = false;

                // Validuj všetky riadky
                foreach (var row in _dataRows.ToList())
                {
                    if (IsRowEmpty(row)) continue;

                    var rowDict = new Dictionary<string, object?>();
                    foreach (var cell in row.Cells)
                    {
                        rowDict[cell.ColumnName] = cell.Value;
                    }

                    var errors = await _validationService.ValidateRowAsync(rowDict);
                    if (errors.Any())
                    {
                        hasErrors = true;
                    }
                }

                _logger.LogInformation("Validácia všetkých riadkov dokončená: {HasErrors}", hasErrors ? "našli sa chyby" : "všetko v poriadku");
                return !hasErrors;
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
                _logger.LogInformation("Exportujú sa dáta do DataTable");

                var result = await _exportService.ExportToDataTableAsync();
                _logger.LogInformation($"Export dokončený: {result.Rows.Count} riadkov");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte do DataTable");
                throw;
            }
        }

        /// <summary>
        /// ✅ UPRAVENÉ: Vymaže všetky dáta z DataGrid s rešpektovaním minimálneho počtu riadkov
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation("Vymazávajú sa všetky dáta");

                ShowLoadingState("Vymazávajú sa dáta...");

                await _dataManagementService.ClearAllDataAsync();

                // ✅ NOVÁ FUNKCIONALITA: Refresh UI po vymazaní - zachová minimum riadkov
                await RefreshGridDataAsync();

                HideLoadingState();
                _logger.LogInformation("Všetky dáta vymazané s zachovaním minimálneho počtu riadkov");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri vymazávaní dát");
                throw;
            }
        }

        /// <summary>
        /// Zmaže riadky na základe custom validačných pravidiel
        /// </summary>
        public async Task DeleteRowsByCustomValidationAsync(List<GridValidationRule> deleteValidationRules)
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation($"Spúšťa sa mazanie riadkov podľa {deleteValidationRules.Count} custom validačných pravidiel");

                ShowLoadingState("Vyhodnocujú sa pravidlá pre mazanie riadkov...");

                int deletedCount = 0;
                var rowsToDelete = new List<RowDataModel>();

                // Prejdi všetky riadky
                foreach (var row in _dataRows.ToList())
                {
                    // Preskač prázdne riadky
                    if (IsRowEmpty(row))
                        continue;

                    // Kontrola či riadok splňuje niektoré z delete pravidiel
                    bool shouldDelete = false;

                    foreach (var rule in deleteValidationRules)
                    {
                        var cellData = row.Cells.FirstOrDefault(c => c.ColumnName == rule.ColumnName);
                        if (cellData != null)
                        {
                            // Ak pravidlo vráti TRUE, riadok sa zmaže
                            if (rule.Validate(cellData.Value))
                            {
                                shouldDelete = true;
                                _logger.LogDebug($"Riadok {row.RowIndex} bude zmazaný - splnil pravidlo: {rule.ColumnName}");
                                break;
                            }
                        }
                    }

                    if (shouldDelete)
                    {
                        rowsToDelete.Add(row);
                    }
                }

                // ✅ NOVÁ FUNKCIONALITA: Inteligentné mazanie cez DataManagementService
                foreach (var rowToDelete in rowsToDelete)
                {
                    await _dataManagementService.DeleteRowAsync(rowToDelete.RowIndex);
                    deletedCount++;
                }

                // ✅ NOVÁ FUNKCIONALITA: Refresh UI po mazaní
                await RefreshGridDataAsync();

                HideLoadingState();
                _logger.LogInformation($"Úspešne zmazaných {deletedCount} riadkov podľa custom validačných pravidiel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri mazaní riadkov podľa custom validácie");
                HideLoadingState();
                throw;
            }
        }

        #endregion

        #region ✅ NOVÁ FUNKCIONALITA: Auto-Add Riadkov Helper Methods

        /// <summary>
        /// ✅ NOVÉ: Automaticky skontroluje a pridá nový prázdny riadok ak je potrebný
        /// </summary>
        public async Task CheckAndAddNewRowIfNeededAsync()
        {
            try
            {
                if (!_isInitialized) return;

                var lastRow = _dataRows.LastOrDefault();
                if (lastRow != null && !IsRowEmpty(lastRow))
                {
                    _logger.LogDebug("Posledný riadok je vyplnený - pridávam nový prázdny riadok");

                    // Pridaj prázdny riadok cez DataManagementService
                    await _dataManagementService.AddRowAsync(null);

                    // Refresh UI
                    await RefreshGridDataAsync();

                    _logger.LogInformation($"Automaticky pridaný nový prázdny riadok na index {_dataRows.Count - 1}");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri automatickom pridávaní nového riadku");
            }
        }

        #endregion

        #region ✅ INTERNAL Helper Methods

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Core services
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<IDataManagementService, DataManagementService>();
            services.AddSingleton<ICopyPasteService, CopyPasteService>();
            services.AddTransient<IExportService, ExportService>();
            services.AddSingleton<INavigationService, NavigationService>();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DataGrid nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
        }

        private void GenerateHeaders()
        {
            _headerColumns.Clear();

            if (_configuration?.Columns == null) return;

            foreach (var column in _configuration.Columns)
            {
                _headerColumns.Add(column);
            }
        }

        private async Task CreateEmptyRowsAsync(int count)
        {
            _dataRows.Clear();

            for (int i = 0; i < count; i++)
            {
                var row = CreateEmptyRow(i);
                _dataRows.Add(row);
            }

            await Task.CompletedTask;
        }

        private RowDataModel CreateEmptyRow(int index)
        {
            var row = new RowDataModel { RowIndex = index };

            if (_configuration?.Columns != null)
            {
                foreach (var column in _configuration.Columns)
                {
                    var cell = new CellDataModel
                    {
                        ColumnName = column.Name,
                        Value = null,
                        DataType = column.DataType,
                        IsValid = true,
                        ValidationErrors = string.Empty,
                        RowIndex = index
                    };
                    row.Cells.Add(cell);
                }

                // Pridaj ValidAlerts bunku
                row.Cells.Add(new CellDataModel
                {
                    ColumnName = "ValidAlerts",
                    Value = string.Empty,
                    DataType = typeof(string),
                    IsValid = true,
                    ValidationErrors = string.Empty,
                    RowIndex = index
                });
            }

            return row;
        }

        /// <summary>
        /// ✅ AKTUALIZOVANÉ: RefreshGridDataAsync - synchronizuje s DataManagementService
        /// </summary>
        private async Task RefreshGridDataAsync()
        {
            try
            {
                var data = await _dataManagementService.GetAllDataAsync();

                // Vyčisti existujúce riadky
                _dataRows.Clear();

                // Vytvor nové riadky na základe dát z DataManagementService
                for (int i = 0; i < data.Count; i++)
                {
                    var rowData = data[i];
                    var rowModel = CreateEmptyRow(i);

                    // Naplň dáta do buniek
                    foreach (var cell in rowModel.Cells)
                    {
                        if (rowData.ContainsKey(cell.ColumnName))
                        {
                            cell.Value = rowData[cell.ColumnName];
                        }
                    }

                    _dataRows.Add(rowModel);
                }

                await RefreshValidationStateAsync();

                _logger.LogDebug($"Grid UI refreshnutý: {_dataRows.Count} riadkov");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri refresh grid data");
                throw;
            }
        }

        private async Task RefreshValidationStateAsync()
        {
            foreach (var row in _dataRows)
            {
                await ValidateRowAsync(row);
            }
        }

        private async Task ValidateRowAsync(RowDataModel row)
        {
            if (IsRowEmpty(row))
            {
                // Prázdny riadok - vyčisti validačné chyby
                foreach (var cell in row.Cells)
                {
                    cell.IsValid = true;
                    cell.ValidationErrors = string.Empty;
                }
                return;
            }

            var validationErrors = new List<string>();

            foreach (var cell in row.Cells.Where(c => c.ColumnName != "ValidAlerts"))
            {
                var cellErrors = await _validationService.ValidateCellAsync(cell.ColumnName, cell.Value);
                if (cellErrors.Any())
                {
                    cell.IsValid = false;
                    cell.ValidationErrors = string.Join("; ", cellErrors);
                    validationErrors.AddRange(cellErrors);
                }
                else
                {
                    cell.IsValid = true;
                    cell.ValidationErrors = string.Empty;
                }
            }

            // Aktualizuj ValidAlerts bunku
            var validAlertsCell = row.Cells.FirstOrDefault(c => c.ColumnName == "ValidAlerts");
            if (validAlertsCell != null)
            {
                validAlertsCell.Value = string.Join("; ", validationErrors);
                validAlertsCell.IsValid = !validationErrors.Any();
            }
        }

        private bool IsRowEmpty(RowDataModel row)
        {
            return row.Cells
                .Where(c => c.ColumnName != "DeleteRows" && c.ColumnName != "ValidAlerts")
                .All(c => c.Value == null || string.IsNullOrWhiteSpace(c.Value?.ToString()));
        }

        private void ShowLoadingState(string message)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // Loading logic - implementované v XAML
            });
        }

        private void HideLoadingState()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // Hide loading logic - implementované v XAML
            });
        }

        #endregion

        #region Event Handlers

        internal async void OnCellKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                await _navigationService.HandleKeyDownAsync(sender, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri spracovaní KeyDown event");
            }
        }

        internal async void OnCellLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox && textBox.DataContext is CellDataModel cell)
                {
                    await ValidateRowAsync(_dataRows.FirstOrDefault(r => r.Cells.Contains(cell))!);

                    // ✅ NOVÁ FUNKCIONALITA: Kontrola či treba pridať nový riadok
                    await CheckAndAddNewRowIfNeededAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri spracovaní LostFocus event");
            }
        }

        internal void OnCellGotFocus(object sender, RoutedEventArgs e)
        {
            // Môže sa použiť pre copy/paste selection logic
        }

        internal void OnCellTextChanged(object sender, TextChangedEventArgs e)
        {
            // Realtime validácia pri zmene textu
            if (sender is TextBox textBox && textBox.DataContext is CellDataModel cell)
            {
                // Throttle validation
                var key = $"{cell.RowIndex}_{cell.ColumnName}";

                if (_realtimeValidationTimers.ContainsKey(key))
                {
                    _realtimeValidationTimers[key].Stop();
                }
                else
                {
                    _realtimeValidationTimers[key] = new DispatcherTimer();
                }

                _realtimeValidationTimers[key].Interval = TimeSpan.FromMilliseconds(300);
                _realtimeValidationTimers[key].Tick += async (s, args) =>
                {
                    _realtimeValidationTimers[key].Stop();
                    cell.Value = textBox.Text;

                    // Aktualizuj hodnotu v DataManagementService
                    await _dataManagementService.SetCellValueAsync(cell.RowIndex, cell.ColumnName, textBox.Text);

                    await ValidateRowAsync(_dataRows.FirstOrDefault(r => r.Cells.Contains(cell))!);

                    // ✅ NOVÁ FUNKCIONALITA: Kontrola či treba pridať nový riadok
                    await CheckAndAddNewRowIfNeededAsync();
                };
                _realtimeValidationTimers[key].Start();
            }
        }

        internal async void OnDeleteRowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is RowDataModel row)
                {
                    // ✅ NOVÁ FUNKCIONALITA: Inteligentné mazanie cez DataManagementService
                    await _dataManagementService.DeleteRowAsync(row.RowIndex);
                    await RefreshGridDataAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri mazaní riadku");
            }
        }

        internal void OnDataScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Synchronizácia header scroll s data scroll - implementované v XAML
        }

        internal void OnSelectionCanvasPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Multi-selection logic
        }

        internal void OnSelectionCanvasPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Multi-selection logic
        }

        internal void OnSelectionCanvasPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // Multi-selection logic
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
                // Cleanup timers
                foreach (var timer in _realtimeValidationTimers.Values)
                {
                    timer?.Stop();
                }
                _realtimeValidationTimers.Clear();

                if (_serviceProvider is IDisposable disposableProvider)
                    disposableProvider.Dispose();

                _headerColumns.Clear();
                _dataRows.Clear();

                _isDisposed = true;
                _logger?.LogInformation("AdvancedDataGrid disposed");
            }
            catch (Exception ex)
            {
                // Log but don't throw during disposal
                System.Diagnostics.Debug.WriteLine($"Chyba pri dispose: {ex.Message}");
            }
        }

        #endregion
    }
}