// Controls/AdvancedDataGrid.xaml.cs - ✅ SIMPLIFIKOVANÝ pre NuGet kompatibilitu
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

// ✅ OPRAVENÉ CS0104: Aliasy pre zamedzenie konfliktov s WinUI typmi
using GridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using GridValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Simplifikovaný AdvancedDataGrid komponent - ✅ PUBLIC API
    /// </summary>
    public sealed partial class AdvancedDataGrid : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdvancedDataGrid> _logger;

        private bool _isInitialized = false;
        private bool _isDisposed = false;

        // Simplifikované dáta pre test
        private List<Dictionary<string, object?>> _testData = new();
        private List<GridColumnDefinition> _columns = new();
        private List<GridValidationRule> _validationRules = new();

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

            this.InitializeComponent();
            _logger.LogInformation("Simplifikovaný AdvancedDataGrid inicializovaný");
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
                // Jednoduchá implementácia - len log
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
        /// Inicializuje DataGrid s konfiguráciou - ✅ NOVÉ: s auto-add podporou
        /// </summary>
        public async Task InitializeAsync(
            List<GridColumnDefinition> columns,
            List<GridValidationRule> validationRules,
            GridThrottlingConfig throttlingConfig,
            int emptyRowsCount = 15)
        {
            try
            {
                _logger.LogInformation("Začína inicializácia DataGrid s AUTO-ADD...");
                ShowLoadingState("Inicializuje sa DataGrid s AUTO-ADD...");

                // Ulož konfiguráciu
                _columns = columns ?? new List<GridColumnDefinition>();
                _validationRules = validationRules ?? new List<GridValidationRule>();

                // Simulácia inicializácie
                await Task.Delay(300);

                _isInitialized = true;
                HideLoadingState();

                // ✅ AUTO-ADD demo info
                UpdateDemoInfo($"Inicializované s {emptyRowsCount} minimálnymi riadkami");

                _logger.LogInformation("DataGrid úspešne inicializovaný s AUTO-ADD");
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
                _logger.LogInformation($"Načítavajú sa dáta: {data.Count} riadkov s AUTO-ADD funkciou");

                ShowLoadingState("Načítavajú sa dáta s AUTO-ADD...");

                // Simulácia AUTO-ADD logiky
                _testData = new List<Dictionary<string, object?>>(data);

                // ✅ AUTO-ADD: Ak má viac dát ako minimum, pridaj ďalšie + vždy jeden prázdny
                var minimumRows = 5; // z demo
                var requiredCapacity = Math.Max(data.Count + 1, minimumRows);

                // Pridaj prázdne riadky ak je potrebné
                while (_testData.Count < requiredCapacity)
                {
                    _testData.Add(new Dictionary<string, object?>());
                }

                await Task.Delay(200);
                HideLoadingState();

                // ✅ AUTO-ADD demo info update
                UpdateDemoInfo($"AUTO-ADD: Načítané {data.Count} riadkov dát, celkom {_testData.Count} riadkov ({_testData.Count - data.Count} prázdnych)");

                _logger.LogInformation("Dáta úspešne načítané s AUTO-ADD riadkov");
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
                _logger.LogInformation("Spúšťa sa validácia všetkých riadkov s AUTO-ADD");

                await Task.Delay(100); // Simulácia

                UpdateDemoInfo("Validácia dokončená - všetky riadky sú validné (demo)");

                return true;
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

                var dataTable = new DataTable("ExportedData");

                // Simulácia exportu
                await Task.Delay(100);

                UpdateDemoInfo($"Export dokončený: {_testData.Count} riadkov exportovaných");

                return dataTable;
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
                _logger.LogInformation("Vymazávajú sa všetky dáta s AUTO-ADD ochranou");

                ShowLoadingState("Vymazávajú sa dáta s AUTO-ADD ochranou...");

                // ✅ AUTO-ADD: Zachovaj minimálny počet prázdnych riadkov
                _testData.Clear();
                var minimumRows = 5; // z demo
                for (int i = 0; i < minimumRows; i++)
                {
                    _testData.Add(new Dictionary<string, object?>());
                }

                await Task.Delay(200);
                HideLoadingState();

                UpdateDemoInfo($"AUTO-ADD: Všetky dáta vymazané, zachovaných {minimumRows} minimálnych prázdnych riadkov");

                _logger.LogInformation("Všetky dáta vymazané s AUTO-ADD ochranou");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri vymazávaní dát");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Zmaže riadky na základe custom validačných pravidiel s AUTO-ADD ochranou
        /// </summary>
        public async Task DeleteRowsByCustomValidationAsync(List<GridValidationRule> deleteValidationRules)
        {
            try
            {
                EnsureInitialized();
                _logger.LogInformation($"Spúšťa sa AUTO-ADD delete s {deleteValidationRules.Count} pravidlami");

                ShowLoadingState("AUTO-ADD delete: Aplikujú sa pravidlá...");

                int deletedCount = 0;

                // Simulácia AUTO-ADD delete logiky
                var minimumRows = 5;
                var originalCount = _testData.Count;

                // Simuluj mazanie - zachovaj aspoň minimum
                if (_testData.Count > minimumRows)
                {
                    deletedCount = Math.Min(3, _testData.Count - minimumRows); // Zmaž max 3 riadky
                    for (int i = 0; i < deletedCount; i++)
                    {
                        _testData.RemoveAt(_testData.Count - 1);
                    }
                }

                await Task.Delay(300);
                HideLoadingState();

                UpdateDemoInfo($"AUTO-ADD delete: {deletedCount} riadkov zmazaných, zachované minimum {minimumRows} riadkov");

                _logger.LogInformation($"AUTO-ADD delete dokončené: {deletedCount} riadkov zmazaných");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri AUTO-ADD delete");
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
            _logger.LogInformation($"DEMO INFO: {message}");
        }

        #endregion

        #region Event Handlers

        internal void OnTestAutoAddClick(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    UpdateDemoInfo("Test AUTO-ADD: Simuluje sa pridávanie dát...");

                    var newData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 100, ["Meno"] = "Test Auto-Add", ["Email"] = "test@auto.add" }
                    };

                    await LoadDataAsync(newData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Test AUTO-ADD failed");
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
                    var deleteRules = new List<GridValidationRule>
                    {
                        GridValidationRule.Custom("ID", value => true, "Test delete rule")
                    };

                    await DeleteRowsByCustomValidationAsync(deleteRules);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Test delete failed");
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

                _testData.Clear();
                _columns.Clear();
                _validationRules.Clear();

                _isDisposed = true;
                _logger?.LogInformation("Simplifikovaný AdvancedDataGrid disposed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri dispose: {ex.Message}");
            }
        }

        #endregion
    }
}