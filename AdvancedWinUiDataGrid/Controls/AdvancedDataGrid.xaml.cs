// Controls/AdvancedDataGrid.xaml.cs - OPRAVENÉ warningy
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// AdvancedDataGrid - profesionálny WinUI3 komponent pre dynamické tabuľky.
    /// Verejné API pre používateľov balíka.
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, IDisposable
    {
        #region Private fields

        private readonly ServiceCollection _services;
        private ServiceProvider? _serviceProvider;
        private IDataManagementService? _dataService;
        private IValidationService? _validationService;
        private ICopyPasteService? _copyPasteService;
        private IExportService? _exportService;
        private ILoggingService? _logger;
        private INavigationService? _navigationService;

        private GridConfiguration? _configuration;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // UI Collections
        private readonly ObservableCollection<ColumnDefinition> _headerColumns = new();
        private readonly ObservableCollection<ObservableCollection<CellData>> _dataRows = new();

        // Navigation & Selection - OPRAVENÉ: Odstránený nepoužívaný _currentCell
        private readonly HashSet<(int row, int col)> _selectedCells = new();

        // Throttling & Performance
        private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        #endregion

        #region Constructor

        /// <summary>
        /// Vytvorí novú inštanciu AdvancedDataGrid komponentu.
        /// </summary>
        public AdvancedDataGrid()
        {
            this.InitializeComponent();

            // Inicializovať DI kontajner
            _services = new ServiceCollection();
            ConfigureDependencyInjection();

            // Nastaviť UI bindings
            HeaderRepeater.ItemsSource = _headerColumns;
            DataRowsRepeater.ItemsSource = _dataRows;

            // Keyboard handling
            this.KeyDown += OnKeyDown;
            this.Loaded += OnLoaded;

            // Scroll synchronization
            DataScrollViewer.ViewChanged += OnDataScrollViewChanged;
        }

        #endregion

        #region Verejné API - Inicializácia

        /// <summary>
        /// Inicializuje DataGrid s konfiguráciou stĺpcov, validačných pravidiel a nastavení.
        /// </summary>
        /// <param name="columns">Definície stĺpcov</param>
        /// <param name="validationRules">Validačné pravidlá</param>
        /// <param name="throttlingConfig">Konfigurácia throttling-u</param>
        /// <param name="emptyRowsCount">Počet prázdnych riadkov na vytvorenie</param>
        /// <returns>Task pre asynchrónnu inicializáciu</returns>
        public async Task InitializeAsync(
            List<ColumnDefinition> columns,
            List<ValidationRule> validationRules,
            ThrottlingConfig? throttlingConfig = null,
            int emptyRowsCount = 10)
        {
            await _initializationSemaphore.WaitAsync();
            try
            {
                if (_isInitialized)
                    throw new InvalidOperationException("DataGrid je už inicializovaný");

                await ShowLoadingAsync("Inicializuje sa DataGrid...");

                _logger?.LogInformation("Začína inicializácia AdvancedDataGrid");

                // Validovať vstupné parametre
                if (columns == null || !columns.Any())
                    throw new ArgumentException("Musí byť definovaný aspoň jeden stĺpec");

                // Vytvoriť konfiguráciu gridu
                _configuration = new GridConfiguration(
                    columns,
                    validationRules,
                    throttlingConfig ?? ThrottlingConfig.Default,
                    emptyRowsCount);

                // Inicializovať services - OPRAVENÉ: Null check pre _serviceProvider
                _serviceProvider = _services.BuildServiceProvider();
                if (_serviceProvider == null)
                    throw new InvalidOperationException("Nepodarilo sa vytvoriť ServiceProvider");

                await InitializeServicesAsync();

                // Nastaviť UI
                await SetupUIAsync();

                // Vytvoriť prázdne riadky
                await CreateEmptyRowsAsync(emptyRowsCount);

                _isInitialized = true;
                _logger?.LogInformation("AdvancedDataGrid úspešne inicializovaný");

                await HideLoadingAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri inicializácii AdvancedDataGrid");
                await HideLoadingAsync();
                throw;
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        /// <summary>
        /// Určuje či je DataGrid inicializovaný.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Verejné API - Dátové operácie

        /// <summary>
        /// Načíta dáta z Dictionary kolekcie.
        /// </summary>
        /// <param name="data">Kolekcia dát</param>
        /// <returns>Task pre asynchrónne načítanie</returns>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            EnsureInitialized();

            try
            {
                await ShowLoadingAsync("Načítavajú sa dáta...");
                await _dataService!.LoadDataAsync(data);
                await RefreshUIAsync();
                _logger?.LogInformation("Načítaných {Count} riadkov dát", data?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri načítaní dát");
                throw;
            }
            finally
            {
                await HideLoadingAsync();
            }
        }

        /// <summary>
        /// Načíta dáta z DataTable.
        /// </summary>
        /// <param name="dataTable">DataTable s dátami</param>
        /// <returns>Task pre asynchrónne načítanie</returns>
        public async Task LoadDataAsync(DataTable dataTable)
        {
            EnsureInitialized();

            try
            {
                await ShowLoadingAsync("Načítavajú sa dáta z DataTable...");
                await _dataService!.LoadDataAsync(dataTable);
                await RefreshUIAsync();
                _logger?.LogInformation("Načítaných {Count} riadkov z DataTable", dataTable?.Rows.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri načítaní dát z DataTable");
                throw;
            }
            finally
            {
                await HideLoadingAsync();
            }
        }

        /// <summary>
        /// Vymaže všetky dáta z DataGrid a uvoľní zdroje.
        /// </summary>
        /// <returns>Task pre asynchrónne vymazanie</returns>
        public async Task ClearAllDataAsync()
        {
            EnsureInitialized();

            try
            {
                await ShowLoadingAsync("Vymazávajú sa dáta...");

                // Vyčistiť services
                await _dataService!.ClearAllDataAsync();

                // Vyčistiť UI kolekcie
                _dataRows.Clear();
                _selectedCells.Clear();

                _logger?.LogInformation("Všetky dáta vymazané z DataGrid");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri vymazávaní dát");
                throw;
            }
            finally
            {
                await HideLoadingAsync();
            }
        }

        #endregion

        #region Verejné API - Validácia

        /// <summary>
        /// Validuje všetky neprázdne riadky v DataGrid.
        /// </summary>
        /// <returns>True ak sú všetky dáta validné</returns>
        public async Task<bool> ValidateAllRowsAsync()
        {
            EnsureInitialized();

            try
            {
                await ShowLoadingAsync("Validujú sa dáta...");

                var allRowsData = _dataService!.GetAllRowsData();
                var result = await _validationService!.ValidateAllNonEmptyRowsAsync(allRowsData);

                // Aktualizovať UI s výsledkami validácie
                await RefreshValidationUIAsync(result);

                _logger?.LogInformation("Validácia dokončená: {ValidRows}/{TotalRows} validných riadkov",
                    result.ValidRowsCount, result.TotalRowsCount);

                return result.IsValid;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri validácii dát");
                throw;
            }
            finally
            {
                await HideLoadingAsync();
            }
        }

        #endregion

        #region Verejné API - Export

        /// <summary>
        /// Exportuje všetky dáta do DataTable.
        /// Zahŕňa ValidAlerts stĺpec, ale nezahŕňa DeleteRows stĺpec.
        /// </summary>
        /// <returns>DataTable s dátami</returns>
        public async Task<DataTable> ExportToDataTableAsync()
        {
            EnsureInitialized();

            try
            {
                await ShowLoadingAsync("Exportujú sa dáta...");

                var result = await _exportService!.ExportToDataTableAsync(includeEmptyRows: false);

                _logger?.LogInformation("Export do DataTable dokončený: {RowCount} riadkov", result.Rows.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri exporte do DataTable");
                throw;
            }
            finally
            {
                await HideLoadingAsync();
            }
        }

        #endregion

        #region Private - Inicializácia services

        /// <summary>
        /// Konfiguruje Dependency Injection kontajner.
        /// </summary>
        private void ConfigureDependencyInjection()
        {
            // Zaregistrovať services
            _services.AddSingleton<IValidationService, ValidationService>();
            _services.AddSingleton<IDataManagementService, DataManagementService>();
            _services.AddSingleton<ICopyPasteService, CopyPasteService>();
            _services.AddTransient<IExportService, ExportService>();
            _services.AddSingleton<INavigationService, NavigationService>();

            // Logging abstrakcia - OPRAVENÉ: Null-safe
            _services.AddSingleton<ILoggingService>(provider =>
            {
                var logger = provider.GetService<ILogger<AdvancedDataGrid>>();
                return new LoggingServiceAdapter(logger);
            });
        }

        /// <summary>
        /// Inicializuje všetky services.
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            // OPRAVENÉ: Null-safe získanie services s explicit null check
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceProvider nie je inicializovaný");

            _logger = _serviceProvider.GetRequiredService<ILoggingService>();
            _dataService = _serviceProvider.GetRequiredService<IDataManagementService>();
            _validationService = _serviceProvider.GetRequiredService<IValidationService>();
            _copyPasteService = _serviceProvider.GetRequiredService<ICopyPasteService>();
            _exportService = _serviceProvider.GetRequiredService<IExportService>();
            _navigationService = _serviceProvider.GetRequiredService<INavigationService>();

            // Inicializovať každý service s konfiguráciou
            await _dataService.InitializeAsync(_configuration!);
            await _validationService.InitializeAsync(_configuration!);
            await _copyPasteService.InitializeAsync(_configuration!);
            await _exportService.InitializeAsync(_configuration!);
            await _navigationService.InitializeAsync(_configuration!, this);

            // OPRAVENÉ: Set cross-dependencies
            if (_copyPasteService is CopyPasteService copyPasteImpl)
            {
                copyPasteImpl.SetDataService(_dataService);
            }

            if (_exportService is ExportService exportImpl)
            {
                exportImpl.SetDataService(_dataService);
            }

            // Pripojiť event handlery
            _dataService.DataChanged += OnDataChanged;
            _validationService.CellValidationChanged += OnCellValidationChanged;
        }

        #endregion

        #region Private - UI management

        /// <summary>
        /// Nastaví UI na základe konfigurácie.
        /// </summary>
        private async Task SetupUIAsync()
        {
            // Nastaviť hlavičky stĺpcov
            _headerColumns.Clear();
            foreach (var column in _configuration!.Columns)
            {
                _headerColumns.Add(column);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Vytvorí prázdne riadky.
        /// </summary>
        private async Task CreateEmptyRowsAsync(int rowCount)
        {
            await _dataService!.CreateEmptyRowsAsync(rowCount);
            await RefreshUIAsync();
        }

        /// <summary>
        /// Obnoví UI s aktuálnymi dátami.
        /// </summary>
        private async Task RefreshUIAsync()
        {
            _dataRows.Clear();

            var allRowsData = _dataService!.GetAllRowsData();
            foreach (var rowData in allRowsData)
            {
                var rowCollection = new ObservableCollection<CellData>(rowData);
                _dataRows.Add(rowCollection);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Aktualizuje UI s výsledkami validácie.
        /// </summary>
        private async Task RefreshValidationUIAsync(GridValidationResult validationResult)
        {
            // Aktualizovať validation stav v UI
            foreach (var rowResult in validationResult.RowResults)
            {
                var rowIndex = rowResult.Key;
                var rowValidation = rowResult.Value;

                if (rowIndex < _dataRows.Count)
                {
                    var uiRow = _dataRows[rowIndex];

                    // Aktualizovať ValidAlerts stĺpec
                    var validAlertsCell = uiRow.FirstOrDefault(c => c.ColumnDefinition.IsValidationColumn);
                    if (validAlertsCell != null)
                    {
                        validAlertsCell.ErrorMessage = rowValidation.FormattedErrorMessage;
                    }

                    // Aktualizovať jednotlivé bunky
                    foreach (var cellResult in rowValidation.CellResults)
                    {
                        var columnIndex = cellResult.Key;
                        var cellValidation = cellResult.Value;

                        if (columnIndex < uiRow.Count)
                        {
                            var uiCell = uiRow[columnIndex];
                            uiCell.IsValid = cellValidation.IsValid;
                            if (!cellValidation.IsValid)
                            {
                                uiCell.ErrorMessage = cellValidation.ErrorMessage;
                            }
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Private - Loading UI

        private async Task ShowLoadingAsync(string message)
        {
            await this.DispatcherQueue.EnqueueAsync(() =>
            {
                LoadingText.Text = message;
                LoadingOverlay.Visibility = Visibility.Visible;
            });
        }

        private async Task HideLoadingAsync()
        {
            await this.DispatcherQueue.EnqueueAsync(() =>
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            });
        }

        #endregion

        #region Event handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Dodatočná inicializácia po načítaní UI
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Delegovať na NavigationService
            _navigationService?.HandleKeyDown(e);
        }

        private void OnDataScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            // Synchronizovať scroll header-u s dátami
            if (!e.IsIntermediate)
            {
                HeaderScrollViewer.ScrollToHorizontalOffset(DataScrollViewer.HorizontalOffset);
            }
        }

        private void OnDeleteRowClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int rowIndex)
            {
                // Vymazať obsah riadku (nie fyzicky riadok)
                _ = Task.Run(async () => await _dataService!.DeleteRowContentAsync(rowIndex));
            }
        }

        private void OnDataChanged(object? sender, DataChangedEventArgs e)
        {
            // Spracovať zmenu dát
            _ = Task.Run(async () => await RefreshUIAsync());
        }

        private void OnCellValidationChanged(object? sender, CellValidationChangedEventArgs e)
        {
            // Aktualizovať UI validáciu pre konkrétnu bunku
        }

        #endregion

        #region Helper methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DataGrid nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Dispose services
                _dataService?.Dispose();
                _validationService?.Dispose();
                _copyPasteService?.Dispose();
                _exportService?.Dispose();
                _navigationService?.Dispose();
                _logger?.Dispose();

                // Dispose DI container
                _serviceProvider?.Dispose();

                // Dispose other resources
                _configuration?.Dispose();
                _initializationSemaphore?.Dispose();

                _disposed = true;
            }
            catch (Exception ex)
            {
                // Log chybu ak je možné
                System.Diagnostics.Debug.WriteLine($"Chyba pri dispose: {ex.Message}");
            }
        }

        #endregion
    }

    #region Extension methods pre DispatcherQueue

    internal static class DispatcherQueueExtensions
    {
        public static async Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            dispatcher.TryEnqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }
    }

    #endregion
}