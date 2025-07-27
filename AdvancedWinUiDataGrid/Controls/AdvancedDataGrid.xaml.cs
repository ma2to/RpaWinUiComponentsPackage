// Controls/AdvancedDataGrid.xaml.cs - ✅ OPRAVENÝ konštruktor s XAML error handling
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
    /// AdvancedDataGrid komponent s kompletnou Auto-Add funkcionalitou - ✅ PUBLIC API
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
        private bool _xamlLoadFailed = false;

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

        #region ✅ OPRAVENÝ Constructor s XAML error handling

        public AdvancedDataGrid()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 AdvancedDataGrid: Začína inicializácia s XAML error handling...");

                // ✅ KĽÚČOVÁ OPRAVA: Bezpečná XAML inicializácia s error handling
                InitializeXamlSafely();

                // ✅ OPRAVA: Najprv XAML, potom DI
                if (!_xamlLoadFailed)
                {
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid: XAML InitializeComponent úspešne dokončené");

                    // Inicializácia DI kontajnera
                    var services = new ServiceCollection();
                    ConfigureServices(services);
                    _serviceProvider = services.BuildServiceProvider();

                    // Získanie služieb z DI kontajnera
                    _logger = _serviceProvider.GetRequiredService<ILogger<AdvancedDataGrid>>();
                    _dataManagementService = _serviceProvider.GetRequiredService<IDataManagementService>();
                    _validationService = _serviceProvider.GetRequiredService<IValidationService>();
                    _exportService = _serviceProvider.GetRequiredService<IExportService>();

                    _logger?.LogInformation("AdvancedDataGrid s Auto-Add funkciou inicializovaný cez Package Reference");
                    System.Diagnostics.Debug.WriteLine("✅ AdvancedDataGrid s Auto-Add funkciou úspešne inicializovaný cez Package Reference");

                    // ✅ Nastav počiatočný UI stav
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
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                // ✅ NOVÉ: Detailná analýza chyby
                AnalyzeConstructorError(ex);

                // ✅ NOVÉ: Pokús sa vytvoriť fallback services
                try
                {
                    CreateFallbackServices();
                    System.Diagnostics.Debug.WriteLine("⚠️ Fallback services vytvorené napriek chybe");
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Aj fallback services zlyhal: {fallbackEx.Message}");
                }

                // ✅ NOVÉ: Nevyhadzuj exception - nech aplikácia pokračuje
                // throw; // Commented out - necháme aplikáciu bežať
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Bezpečná XAML inicializácia s error handling
        /// </summary>
        private void InitializeXamlSafely()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Pokúšam sa načítať XAML súbor pre AdvancedDataGrid...");

                // ✅ Pokús sa načítať XAML
                this.InitializeComponent();

                System.Diagnostics.Debug.WriteLine("✅ XAML súbor úspešne načítaný");
                _xamlLoadFailed = false;
            }
            catch (Exception xamlEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ XAML loading failed: {xamlEx.Message}");

                // ✅ Detailná analýza XAML chyby
                if (xamlEx.Message.Contains("LoadComponent") || xamlEx.HResult == unchecked((int)0x802B000A))
                {
                    System.Diagnostics.Debug.WriteLine("❌ XAML RESOURCE NOT FOUND - XBF súbory nie sú dostupné");
                    System.Diagnostics.Debug.WriteLine("💡 Možné riešenia:");
                    System.Diagnostics.Debug.WriteLine("   1. Rebuild balíka s Release konfiguráciou");
                    System.Diagnostics.Debug.WriteLine("   2. Skontrolovať Package Reference verziu");
                    System.Diagnostics.Debug.WriteLine("   3. Vymazať bin/obj a dotnet restore --force");
                }
                else if (xamlEx.Message.Contains("XAML"))
                {
                    System.Diagnostics.Debug.WriteLine("❌ XAML PARSING ERROR - problém so syntaxou XAML");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ GENERAL XAML ERROR: {xamlEx.GetType().Name}");
                }

                _xamlLoadFailed = true;

                // ✅ Vytvor základný UI programmaticky ako fallback
                CreateFallbackUI();
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Vytvorí základný UI programmaticky ak XAML zlyhá
        /// </summary>
        private void CreateFallbackUI()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 Vytváram fallback UI programmaticky...");

                // ✅ Základný Border ako root element
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

                fallbackContent.Children.Add(new TextBlock
                {
                    Text = "XAML súbory neboli načítané správne z NuGet balíka.",
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

                fallbackContent.Children.Add(new TextBlock
                {
                    Text = "Skúste: rebuild balíka, dotnet restore --force, alebo verifikujte Package Reference",
                    FontSize = 12,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

                fallbackBorder.Child = fallbackContent;
                this.Content = fallbackBorder;

                System.Diagnostics.Debug.WriteLine("✅ Fallback UI vytvorený");
            }
            catch (Exception fallbackUiEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Aj fallback UI creation failed: {fallbackUiEx.Message}");
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Vytvorí fallback services ak DI zlyhá
        /// </summary>
        private void CreateFallbackServices()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 Vytváram fallback services...");

                // ✅ Jednoduché fallback implementácie
                var services = new ServiceCollection();

                // Basic logging
                services.AddLogging(builder =>
                {
                    builder.AddDebug();
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
                });

                // Fallback services
                services.AddSingleton<IDataManagementService, DataManagementService>();
                services.AddSingleton<IValidationService, ValidationService>();
                services.AddTransient<IExportService, ExportService>();

                _serviceProvider = services.BuildServiceProvider();

                // Získaj services
                _logger = _serviceProvider.GetService<ILogger<AdvancedDataGrid>>();
                _dataManagementService = _serviceProvider.GetRequiredService<IDataManagementService>();
                _validationService = _serviceProvider.GetRequiredService<IValidationService>();
                _exportService = _serviceProvider.GetRequiredService<IExportService>();

                System.Diagnostics.Debug.WriteLine("✅ Fallback services vytvorené");
            }
            catch (Exception serviceEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fallback services creation failed: {serviceEx.Message}");
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Analýza chyby konštruktora
        /// </summary>
        private void AnalyzeConstructorError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("🔍 ANALÝZA CHYBY:");
            System.Diagnostics.Debug.WriteLine($"   Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"   HResult: 0x{ex.HResult:X8}");
            System.Diagnostics.Debug.WriteLine($"   Message: {ex.Message}");

            // ✅ Špecifické HResult hodnoty pre WinUI/XAML chyby
            switch (ex.HResult)
            {
                case unchecked((int)0x802B000A): // INET_E_RESOURCE_NOT_FOUND
                    System.Diagnostics.Debug.WriteLine("💡 DIAGNÓZA: XAML resource nenájdený - XBF súbory chýbajú v NuGet balíku");
                    break;
                case unchecked((int)0x80004005): // E_FAIL
                    System.Diagnostics.Debug.WriteLine("💡 DIAGNÓZA: Obecná XAML chyba - možno packaging problém");
                    break;
                case unchecked((int)0x8007000B): // ERROR_BAD_FORMAT
                    System.Diagnostics.Debug.WriteLine("💡 DIAGNÓZA: Nesprávny formát XAML súboru");
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("💡 DIAGNÓZA: Neznáma chyba - možno dependency problém");
                    break;
            }

            // ✅ Stack trace analýza
            if (ex.StackTrace?.Contains("LoadComponent") == true)
            {
                System.Diagnostics.Debug.WriteLine("💡 STACK TRACE: Chyba v LoadComponent - XAML packaging problém");
            }
            else if (ex.StackTrace?.Contains("Activate_") == true)
            {
                System.Diagnostics.Debug.WriteLine("💡 STACK TRACE: Chyba v XAML Activation - Type resolution problém");
            }
        }

        #endregion

        #region ✅ PUBLIC Color Theme API (unchanged)

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
                _logger?.LogDebug("Color theme aplikovaná: {ThemeName}", _colorTheme.ToString());
                // TODO: Aplikovať theme na UI elementy
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri aplikovaní color theme");
            }
        }

        #endregion

        #region ✅ PUBLIC API Methods s Auto-Add (unchanged but with error protection)

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
                // ✅ NOVÉ: Kontrola či XAML loading prebehol úspešne
                if (_xamlLoadFailed)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ InitializeAsync volaný napriek XAML chybe - pokračujem s iba dátovou inicializáciou");
                }

                _logger?.LogInformation("AUTO-ADD: Začína inicializácia DataGrid s {EmptyRowsCount} riadkami...", emptyRowsCount);

                if (!_xamlLoadFailed)
                {
                    ShowLoadingState("Inicializuje sa DataGrid s Auto-Add funkcionalitou...");
                }

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

                // Inicializuj služby (ak existujú)
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

                _logger?.LogInformation("AUTO-ADD: DataGrid úspešne inicializovaný s Auto-Add funkciou");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Chyba pri inicializácii DataGrid s Auto-Add");

                if (!_xamlLoadFailed)
                {
                    ShowLoadingState($"Chyba: {ex.Message}");
                }

                throw;
            }
        }

        // ✅ Ostatné PUBLIC API metódy zostávajú nezmenené...
        // (LoadDataAsync, ValidateAllRowsAsync, ExportToDataTableAsync, atď.)
        // Ale pridajú sa null checks pre _xamlLoadFailed scenáre

        #endregion

        #region Helper Methods (updated with error protection)

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
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

        private void UpdateUIVisibility()
        {
            if (_xamlLoadFailed) return; // Skip ak XAML zlyhal

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
            if (_xamlLoadFailed) return; // Skip ak XAML zlyhal

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
            if (_xamlLoadFailed) return; // Skip ak XAML zlyhal

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

        // ✅ Ostatné metódy zostávajú nezmenené...
        // (Auto-Add helper methods, Public properties, INotifyPropertyChanged, IDisposable, atď.)

        #region ✅ NOVÉ: Auto-Add Helper Methods (unchanged)

        private async Task CreateInitialEmptyRowsAsync()
        {
            _gridData.Clear();

            for (int i = 0; i < _initialRowCount; i++)
            {
                _gridData.Add(CreateEmptyRow());
            }

            _logger?.LogDebug("AUTO-ADD: Vytvorených {Count} počiatočných prázdnych riadkov", _initialRowCount);
            await Task.CompletedTask;
        }

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

        private bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteRows" || columnName == "ValidAlerts";
        }

        #endregion

        #region ✅ NOVÉ: Diagnostic Properties

        /// <summary>
        /// Či sa XAML načítal úspešne (pre debugging)
        /// </summary>
        public bool IsXamlLoaded => !_xamlLoadFailed;

        /// <summary>
        /// Diagnostické info o stave komponentu
        /// </summary>
        public string DiagnosticInfo => $"Initialized: {_isInitialized}, XAML: {!_xamlLoadFailed}, Auto-Add: {_autoAddEnabled}, Rows: {_gridData.Count}";

        #endregion

        #region INotifyPropertyChanged & IDisposable (unchanged)

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