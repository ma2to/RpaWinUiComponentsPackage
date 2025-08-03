// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ KOMPLETNE OPRAVENÝ
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

// ✅ OPRAVENÉ: Správne using direktívy pre multi-component package
#if !NO_PACKAGE
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.Logger; // ✅ OPRAVENÉ: Správny namespace pre LoggerComponent
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.DataGridColorConfig;
// ✅ NOVÉ: Background validation classes
using BackgroundValidationConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation.BackgroundValidationConfiguration;
using BackgroundValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation.BackgroundValidationRule;
using BackgroundValidationResult = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation.BackgroundValidationResult;
using SortDirection = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search.SortDirection;
#endif

namespace RpaWinUiComponentsPackage.Demo
{
    /// <summary>
    /// ✅ OPRAVENÉ: MainWindow s kompletnou LoggerComponent integráciou
    /// </summary>
    public sealed partial class MainWindow : Window // ✅ OPRAVENÉ: Window base class pre FindName support
    {
        private bool _packageAvailable = false;
        private bool _isInitialized = false;

        // ✅ OPRAVENÉ: Správne typy s LoggerComponent integráciou
#if !NO_PACKAGE
        private AdvancedDataGrid? _actualDataGrid;
        private LoggerComponent? _logger; // ✅ OPRAVENÉ: Správny typ namiesto alias
#endif

        public MainWindow()
        {
            try
            {
                this.InitializeComponent();
                System.Diagnostics.Debug.WriteLine("✅ MainWindow InitializeComponent úspešný");

                // ✅ NOVÉ: LoggerComponent setup hneď v konštruktore
                InitializeLoggerEarly();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ InitializeComponent chyba: {ex.Message}");
                LogError($"Constructor initialization failed: {ex.Message}");
            }

            // ✅ OPRAVENÉ: Správny event handler typ
            this.Activated += OnWindowActivated;
        }

        /// <summary>
        /// ✅ NOVÉ: Skorá inicializácia loggera pre zachytenie všetkých operácií
        /// </summary>
        private void InitializeLoggerEarly()
        {
#if !NO_PACKAGE
            try
            {
                var tempDir = System.IO.Path.GetTempPath();
                var logFileName = $"RpaDemo_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
                _logger = new LoggerComponent(tempDir, logFileName, 10); // 10MB max size

                if (_logger != null)
                {
                    _ = Task.Run(async () => await _logger.LogAsync("🚀 Demo aplikácia spustená - LoggerComponent early initialization", "INFO"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Early logger init failed: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// ✅ OPRAVENÉ: Správny event handler pre WindowActivated
        /// </summary>
        private async void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            try
            {
                if (e.WindowActivationState != Microsoft.UI.Xaml.WindowActivationState.Deactivated && !_isInitialized)
                {
                    this.Activated -= OnWindowActivated;
                    _isInitialized = true;

                    await LogAsync("MainWindow activated - starting initialization", "INFO");

                    await Task.Delay(100);
                    if (await WaitForUIElementsAsync())
                    {
                        await InitializeAsync();
                    }
                    else
                    {
                        await LogAsync("UI elements not ready after waiting", "ERROR");
                        ShowError("UI elementy nie sú pripravené");
                    }
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"WindowActivated error: {ex.Message}", "ERROR");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: UI elements waiting s detailným logovaním
        /// </summary>
        private async Task<bool> WaitForUIElementsAsync()
        {
            const int maxAttempts = 10;
            const int delayMs = 50;

            await LogAsync($"Waiting for UI elements - max {maxAttempts} attempts", "DEBUG");

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    // ✅ OPRAVENÉ: Správne použitie FindName (Window base class)
                    var loadingPanel = this.FindName("LoadingPanel");
                    var dataGridControl = this.FindName("DataGridControl");
                    var statusTextBlock = this.FindName("StatusTextBlock");
                    var initStatusText = this.FindName("InitStatusText");

                    if (loadingPanel != null && dataGridControl != null &&
                        statusTextBlock != null && initStatusText != null)
                    {
                        await LogAsync($"UI elements ready after {attempt + 1} attempts", "DEBUG");
                        return true;
                    }

                    await LogAsync($"UI check attempt {attempt + 1} - some elements missing", "DEBUG");
                }
                catch (Exception ex)
                {
                    await LogAsync($"UI check attempt {attempt + 1} failed: {ex.Message}", "WARN");
                }

                await Task.Delay(delayMs);
            }

            await LogAsync("UI elements not ready after maximum attempts", "ERROR");
            return false;
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Hlavná inicializácia s kompletným logovaním
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                await LogAsync("🚀 Starting multi-component package demo initialization", "INFO");

                UpdateStatus("Kontroluje sa multi-component package...", "📦 Package check...");
                await Task.Delay(300);

                await CheckPackageAvailabilityAsync();

                if (_packageAvailable)
                {
                    await LogAsync("Package available - proceeding with full initialization", "INFO");
                    await InitializeWithPackageAsync();
                }
                else
                {
                    await LogAsync("Package not available - showing fallback UI", "WARN");
                    ShowPackageUnavailable();
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"Critical initialization error: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
                ShowError($"Inicializácia zlyhala: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Package availability check s detailným logovaním
        /// </summary>
        private async Task CheckPackageAvailabilityAsync()
        {
            try
            {
#if !NO_PACKAGE
                await LogAsync("🔍 Testing multi-component package availability...", "DEBUG");

                // ✅ Test AdvancedDataGrid komponentu
                _actualDataGrid = new AdvancedDataGrid();
                await LogAsync("AdvancedDataGrid component created successfully", "DEBUG");

                // ✅ Test LoggerComponent (už máme z early init)
                if (_logger == null)
                {
                    var tempDir = System.IO.Path.GetTempPath();
                    var logFileName = $"RpaDemo_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
                    _logger = new LoggerComponent(tempDir, logFileName, 10);
                }

                if (_logger != null)
                {
                    await _logger.LogAsync("🚀 Package availability test - both components working", "INFO");
                }

                await Task.Delay(100);
                _packageAvailable = true;
                await LogAsync("✅ Multi-component package is available and functional", "INFO");
#else
                _packageAvailable = false;
                await LogAsync("⚠️ Multi-component package not available (NO_PACKAGE flag)", "WARN");
#endif
            }
            catch (Exception ex)
            {
                _packageAvailable = false;
                await LogAsync($"❌ Package availability test failed: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
            }
        }

        /// <summary>
        /// ✅ KĽÚČOVÁ METÓDA: Inicializácia s LoggerComponent integráciou
        /// </summary>
        private async Task InitializeWithPackageAsync()
        {
            try
            {
                UpdateStatus("Multi-component package je dostupný!", "Inicializuje sa DataGrid + Logger...");

#if !NO_PACKAGE
                if (_actualDataGrid != null && _logger != null)
                {
                    await LogAsync("🔧 Setting up DataGrid content in UI", "DEBUG");

                    try
                    {
                        // ✅ OPRAVENÉ: Správne použitie FindName
                        var dataGridControl = this.FindName("DataGridControl") as ContentControl;
                        if (dataGridControl != null)
                        {
                            dataGridControl.Content = _actualDataGrid;
                            await LogAsync("DataGrid content set successfully", "DEBUG");
                        }
                        else
                        {
                            await LogAsync("DataGridControl not found in UI", "ERROR");
                        }
                    }
                    catch (Exception ex)
                    {
                        await LogAsync($"DataGrid content setting failed: {ex.Message}", "ERROR");
                    }

                    await Task.Delay(200);

                    // ✅ KĽÚČOVÁ INTEGRÁCIA: LoggerComponent -> AdvancedDataGrid
                    await _logger.LogAsync("🔧 Initializing AdvancedDataGrid with integrated LoggerComponent", "INFO");

                    // ✅ Demo konfigurácia s rozšíreným logovaním + Background Validation
                    var columns = new List<GridColumnDefinition>
                    {
                        new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                        new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                        new("Email", typeof(string)) { MinWidth = 200, Width = 250, Header = "📧 Email" },
                        new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                        new("TaxNumber", typeof(string)) { MinWidth = 150, Width = 180, Header = "🏢 DIČ" },
                        new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
                    };

                    await LogAsync($"Created {columns.Count} column definitions with background validation support", "DEBUG");

                    // ✅ Realtime validation rules (zostávajú zachované)
                    var rules = new List<GridValidationRule>
                    {
                        GridValidationRule.Required("Meno", "Meno je povinné"),
                        GridValidationRule.Email("Email", "Neplatný email formát"),
                        GridValidationRule.Range("Vek", 18, 100, "Vek musí byť 18-100"),
                        GridValidationRule.Pattern("TaxNumber", @"^\d{8,10}$", "DIČ musí mať 8-10 číslic")
                    };

                    await LogAsync($"Created {rules.Count} realtime validation rules", "DEBUG");

                    // ✅ NOVÉ: Background validation konfigurácia
                    var backgroundConfig = new BackgroundValidationConfiguration
                    {
                        IsEnabled = true,
                        MaxConcurrentValidations = 2,
                        DefaultTimeoutMs = 8000,
                        ValidationDelayMs = 1500,
                        ShowProgressIndicator = true,
                        AutoTriggerOnValueChange = true,
                        TriggerOnCellLostFocus = true,
                        UseValidationCache = true,
                        ValidationCacheMinutes = 3
                    };

                    // ✅ DEMO: Email uniqueness validation (simuluje databázový dotaz)
                    backgroundConfig.BackgroundRules.Add(
                        BackgroundValidationRule.DatabaseValidation(
                            "Email",
                            "Check email uniqueness in database",
                            async (value, rowData, cancellationToken) =>
                            {
                                var email = value?.ToString();
                                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                                    return BackgroundValidationResult.Success();

                                // Simulácia databázového dotazu (2s delay)
                                await Task.Delay(2000, cancellationToken);
                                
                                // Demo logika - niektore emaily sú "duplikáty"
                                var duplicateEmails = new[] { "test@duplicate.sk", "admin@duplicate.sk", "duplicate@test.sk" };
                                var isDuplicate = Array.Exists(duplicateEmails, e => e.Equals(email, StringComparison.OrdinalIgnoreCase));
                                
                                return isDuplicate 
                                    ? BackgroundValidationResult.Error($"Email '{email}' už existuje v databáze")
                                    : BackgroundValidationResult.Success($"Email '{email}' je unikátny");
                            },
                            priority: 1,
                            timeoutMs: 5000));

                    // ✅ DEMO: Tax number validation (simuluje API volanie)
                    backgroundConfig.BackgroundRules.Add(
                        BackgroundValidationRule.ApiValidation(
                            "TaxNumber",
                            "Validate tax number via government API",
                            async (value, rowData, cancellationToken) =>
                            {
                                var taxNumber = value?.ToString();
                                if (string.IsNullOrEmpty(taxNumber))
                                    return BackgroundValidationResult.Success();

                                // Simulácia API volania (3s delay)
                                await Task.Delay(3000, cancellationToken);
                                
                                // Demo logika - niektoré DIČ sú "platné"
                                var validTaxNumbers = new[] { "12345678", "87654321", "11111111", "22222222" };
                                var isValid = Array.Exists(validTaxNumbers, t => t == taxNumber);
                                
                                return isValid
                                    ? BackgroundValidationResult.Success($"DIČ {taxNumber} je platné")
                                    : BackgroundValidationResult.Warning($"DIČ {taxNumber} nebolo nájdené v registri");
                            },
                            priority: 2,
                            timeoutMs: 8000));

                    await LogAsync($"Created background validation config with {backgroundConfig.BackgroundRules.Count} background rules", "INFO");

                    var colors = new GridDataGridColorConfig
                    {
                        CellBackgroundColor = Microsoft.UI.Colors.White,
                        AlternateRowColor = Color.FromArgb(20, 0, 120, 215)
                    };

                    await LogAsync("Created color configuration with zebra rows", "DEBUG");

                    // ✅ NOVÉ: InitializeWithBackgroundValidationAsync s LoggerComponent parametrom
                    await LogAsync("🎯 Calling DataGrid.InitializeWithBackgroundValidationAsync with LoggerComponent integration", "INFO");

                    await _actualDataGrid.InitializeWithBackgroundValidationAsync(
                        columns,
                        rules,
                        backgroundConfig,
                        GridThrottlingConfig.Default,
                        15,
                        colors,
                        _logger.ExternalLogger  // ✅ Nezávislé komponenty - posielam logger do background validation
                    );

                    await _logger.LogAsync("✅ AdvancedDataGrid successfully initialized with LoggerComponent integration", "INFO");

                    // ✅ Demo dáta s background validation testami
                    var demoData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 1, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 28, ["TaxNumber"] = "12345678" },
                        new() { ["ID"] = 2, ["Meno"] = "Peter Svoboda", ["Email"] = "test@duplicate.sk", ["Vek"] = 34, ["TaxNumber"] = "99999999" },
                        new() { ["ID"] = 3, ["Meno"] = "Eva Krásna", ["Email"] = "eva@test.sk", ["Vek"] = 26, ["TaxNumber"] = "87654321" },
                        new() { ["ID"] = 4, ["Meno"] = "Ján Testovací", ["Email"] = "jan@unique.sk", ["Vek"] = 45, ["TaxNumber"] = "11111111" }
                    };

                    await LogAsync("📝 Demo data includes background validation test cases:", "INFO");
                    await LogAsync("  - Email 'test@duplicate.sk' (should trigger duplicate error after 2s)", "INFO");
                    await LogAsync("  - TaxNumber '99999999' (should trigger API warning after 3s)", "INFO");
                    await LogAsync("  - TaxNumber '12345678' (should be valid)", "INFO");

                    await LogAsync($"Loading {demoData.Count} demo data rows", "INFO");
                    await _actualDataGrid.LoadDataAsync(demoData);
                    await _logger.LogAsync($"📊 Demo data loaded: {demoData.Count} rows with complete LoggerComponent integration!", "INFO");

                    // ✅ NOVÉ: Background validation demo info
                    await LogAsync("🎯 Background validation is now active!", "INFO");
                    await LogAsync($"  - Background validation enabled: {_actualDataGrid.IsBackgroundValidationEnabled()}", "INFO");
                    await LogAsync($"  - Background rules count: {_actualDataGrid.GetBackgroundValidationRulesCount()}", "INFO");
                    await LogAsync("", "INFO");
                    await LogAsync("📝 To test background validation:", "INFO");
                    await LogAsync("  1. Edit Email field and enter 'test@duplicate.sk' (will show error after 2s)", "INFO");
                    await LogAsync("  2. Edit TaxNumber field and enter '99999999' (will show warning after 3s)", "INFO");
                    await LogAsync("  3. Valid TaxNumbers: 12345678, 87654321, 11111111, 22222222", "INFO");
                    await LogAsync("  4. Background validations run automatically after value changes", "INFO");
                    
                    // ✅ DEMO: Testovanie všetkých PUBLIC API metód
                    await Task.Delay(2000); // Počkaj na inicializáciu
                    await DemoAllPublicApiMethodsAsync();
                }
#endif

                CompleteInitialization();
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ CRITICAL ERROR: Multi-component initialization failed: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
                ShowError($"Multi-component inicializácia zlyhala: {ex.Message}");

#if !NO_PACKAGE
                if (_logger != null)
                {
                    await _logger.LogAsync($"❌ CRITICAL ERROR: Multi-component initialization failed: {ex.Message}", "ERROR");
                }
#endif
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Complete initialization s logovaním
        /// </summary>
        private void CompleteInitialization()
        {
            try
            {
                // ✅ OPRAVENÉ: Správne použitie FindName
                try
                {
                    var loadingPanel = this.FindName("LoadingPanel") as FrameworkElement;
                    if (loadingPanel != null)
                    {
                        loadingPanel.Visibility = Visibility.Collapsed;
                        _ = LogAsync("Loading panel hidden", "DEBUG");
                    }
                }
                catch (Exception ex)
                {
                    _ = LogAsync($"Error hiding loading panel: {ex.Message}", "WARN");
                }

                try
                {
                    var dataGridControl = this.FindName("DataGridControl") as FrameworkElement;
                    if (dataGridControl != null)
                    {
                        dataGridControl.Visibility = Visibility.Visible;
                        _ = LogAsync("DataGrid control made visible", "DEBUG");
                    }
                }
                catch (Exception ex)
                {
                    _ = LogAsync($"Error showing DataGrid: {ex.Message}", "WARN");
                }

                UpdateStatus("✅ Multi-component demo pripravené!", "🎉 Package funguje s LoggerComponent integration!");

                try
                {
                    var initStatusText = this.FindName("InitStatusText") as TextBlock;
                    if (initStatusText != null)
                    {
                        initStatusText.Text = "✅ Multi-component package funkčný s LoggerComponent!";
                        _ = LogAsync("Status text updated", "DEBUG");
                    }
                }
                catch (Exception ex)
                {
                    _ = LogAsync($"Error updating status text: {ex.Message}", "WARN");
                }

                System.Diagnostics.Debug.WriteLine("🎉 Multi-component inicializácia úspešne dokončená!");

#if !NO_PACKAGE
                Task.Run(async () =>
                {
                    if (_logger != null)
                    {
                        await _logger.LogAsync("🎉 Demo aplikácia úspešne spustená s kompletnou LoggerComponent integráciou", "INFO");
                    }
                });
#endif
            }
            catch (Exception ex)
            {
                _ = LogAsync($"CompleteInitialization error: {ex.Message}", "ERROR");
                ShowError($"Dokončenie inicializácie zlyhalo: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Show package unavailable s logovaním
        /// </summary>
        private void ShowPackageUnavailable()
        {
            try
            {
                var loadingPanel = this.FindName("LoadingPanel") as FrameworkElement;
                if (loadingPanel != null)
                    loadingPanel.Visibility = Visibility.Collapsed;

                var noPackagePanel = this.FindName("NoPackagePanel") as FrameworkElement;
                if (noPackagePanel != null)
                    noPackagePanel.Visibility = Visibility.Visible;

                UpdateStatus("⚠️ Multi-component package nie je dostupný", "Skontrolujte inštaláciu");

                var initStatusText = this.FindName("InitStatusText") as TextBlock;
                if (initStatusText != null)
                    initStatusText.Text = "⚠️ Package Reference chyba";

                _ = LogAsync("Package unavailable UI shown", "INFO");
            }
            catch (Exception ex)
            {
                _ = LogAsync($"ShowPackageUnavailable error: {ex.Message}", "ERROR");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Update status s logovaním
        /// </summary>
        private void UpdateStatus(string detailText, string statusText)
        {
            try
            {
                try
                {
                    var loadingDetailText = this.FindName("LoadingDetailText") as TextBlock;
                    if (loadingDetailText != null)
                        loadingDetailText.Text = detailText;
                }
                catch (Exception ex)
                {
                    _ = LogAsync($"Error updating detail text: {ex.Message}", "DEBUG");
                }

                try
                {
                    var statusTextBlock = this.FindName("StatusTextBlock") as TextBlock;
                    if (statusTextBlock != null)
                        statusTextBlock.Text = statusText;
                }
                catch (Exception ex)
                {
                    _ = LogAsync($"Error updating status text: {ex.Message}", "DEBUG");
                }

                _ = LogAsync($"Status updated: {detailText} | {statusText}", "DEBUG");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ UpdateStatus chyba: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ ROZŠÍRENÉ: Show error s logovaním
        /// </summary>
        private void ShowError(string errorMessage)
        {
            UpdateStatus($"❌ {errorMessage}", "Chyba aplikácie");

            try
            {
                var initStatusText = this.FindName("InitStatusText") as TextBlock;
                if (initStatusText != null)
                    initStatusText.Text = "❌ Chyba";
            }
            catch { }

            System.Diagnostics.Debug.WriteLine($"❌ Error: {errorMessage}");

            _ = LogAsync($"Application error: {errorMessage}", "ERROR");
        }

        #region ✅ NOVÉ: Unified Logging Methods

        /// <summary>
        /// ✅ NOVÉ: Unified async logging method
        /// </summary>
        private async Task LogAsync(string message, string logLevel = "INFO")
        {
            try
            {
                var timestampedMessage = $"[MainWindow] {message}";

#if !NO_PACKAGE
                if (_logger != null)
                {
                    await _logger.LogAsync(timestampedMessage, logLevel);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[{logLevel}] {timestampedMessage}");
                }
#else
                System.Diagnostics.Debug.WriteLine($"[{logLevel}] {timestampedMessage}");
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Synchronous logging for non-async contexts
        /// </summary>
        private void LogSync(string message, string logLevel = "INFO")
        {
            _ = Task.Run(async () => await LogAsync(message, logLevel));
        }

        /// <summary>
        /// ✅ NOVÉ: Helper logging methods
        /// </summary>
        private void LogInfo(string message) => LogSync(message, "INFO");
        private void LogError(string message) => LogSync(message, "ERROR");
        private void LogDebug(string message) => LogSync(message, "DEBUG");

        #endregion

        #region ✅ NOVÉ: Event Handlers s LoggerComponent integráciou

        /// <summary>
        /// ✅ NOVÉ: Load sample data s logovaním
        /// </summary>
        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable)
            {
                await LogAsync("Load sample data clicked but package not available", "WARN");
                return;
            }

#if !NO_PACKAGE
            try
            {
                await LogAsync("Loading sample data triggered by user", "INFO");

                if (_actualDataGrid != null)
                {
                    var sampleData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 101, ["Meno"] = "Nový User", ["Email"] = "novy@test.sk", ["Vek"] = 25 },
                        new() { ["ID"] = 102, ["Meno"] = "Test User", ["Email"] = "test@demo.sk", ["Vek"] = 30 }
                    };

                    await _actualDataGrid.LoadDataAsync(sampleData);
                    UpdateStatus("Sample data načítané", "✅ Data load úspešný");

                    await LogAsync($"Sample data loaded successfully: {sampleData.Count} rows", "INFO");
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"Load sample data error: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
                ShowError($"Load data chyba: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// ✅ NOVÉ: Validate all s logovaním
        /// </summary>
        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                await LogAsync("Validate all triggered by user", "INFO");

                if (_actualDataGrid != null)
                {
                    var isValid = await _actualDataGrid.ValidateAllRowsAsync();
                    var statusText = isValid ? "✅ Všetky dáta sú validné" : "❌ Našli sa validačné chyby";

                    UpdateStatus(statusText, isValid ? "Validácia OK" : "Validačné chyby");
                    await LogAsync($"Validation completed: {(isValid ? "ALL VALID" : "ERRORS FOUND")}", "INFO");
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"Validate all error: {ex.Message}", "ERROR");
                ShowError($"Validácia chyba: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// ✅ NOVÉ: Clear all data s logovaním
        /// </summary>
        private async void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                await LogAsync("Clear all data triggered by user", "INFO");

                if (_actualDataGrid != null)
                {
                    await _actualDataGrid.ClearAllDataAsync();
                    UpdateStatus("Všetky dáta vymazané", "✅ Clear successful");
                    await LogAsync("All data cleared successfully", "INFO");
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"Clear data error: {ex.Message}", "ERROR");
                ShowError($"Clear data chyba: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// ✅ NOVÉ: Export data s logovaním
        /// </summary>
        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                await LogAsync("Export data triggered by user", "INFO");

                if (_actualDataGrid != null)
                {
                    var dataTable = await _actualDataGrid.ExportToDataTableAsync();
                    UpdateStatus($"Dáta exportované: {dataTable.Rows.Count} riadkov", "✅ Export successful");
                    await LogAsync($"Data exported successfully: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns", "INFO");
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"Export data error: {ex.Message}", "ERROR");
                ShowError($"Export chyba: {ex.Message}");
            }
#endif
        }

        #region ✅ NOVÉ EVENT HANDLERY: Kompletné testovanie PUBLIC API

        // ================== BACKGROUND VALIDATION TESTING ==================

        /// <summary>
        /// Test email duplicate background validation
        /// </summary>
        private async void OnTestEmailDuplicateClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🔍 Testing email duplicate background validation", "INFO");
                
                // Pridaj nový riadok s duplicitným emailom
                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 999, ["Meno"] = "Test Duplicate", ["Email"] = "test@duplicate.sk", ["Vek"] = 30, ["TaxNumber"] = "12345678" }
                };
                
                var currentData = await _actualDataGrid.GetAllDataAsync();
                currentData.AddRange(testData);
                await _actualDataGrid.LoadDataAsync(currentData);
                
                await LogAsync("✅ Added row with duplicate email - background validation should trigger in 2s", "INFO");
                UpdateStatus("Test duplicate email pridaný", "Background validation sa spustí za 2s");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Test email duplicate error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Test tax number background validation
        /// </summary>
        private async void OnTestTaxNumberClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🏢 Testing tax number background validation", "INFO");
                
                // Pridaj riadky s rôznymi tax numbers
                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 998, ["Meno"] = "Valid Tax", ["Email"] = "valid@tax.sk", ["Vek"] = 35, ["TaxNumber"] = "87654321" },
                    new() { ["ID"] = 997, ["Meno"] = "Invalid Tax", ["Email"] = "invalid@tax.sk", ["Vek"] = 40, ["TaxNumber"] = "99999999" }
                };
                
                var currentData = await _actualDataGrid.GetAllDataAsync();
                currentData.AddRange(testData);
                await _actualDataGrid.LoadDataAsync(currentData);
                
                await LogAsync("✅ Added rows with valid/invalid tax numbers - API validation triggers in 3s", "INFO");
                UpdateStatus("Test tax numbers pridané", "API validation sa spustí za 3s");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Test tax number error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Add new background validation rule
        /// </summary>
        private async void OnAddBgValidationRuleClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("➕ Adding new background validation rule", "INFO");
                
                var newRule = BackgroundValidationRule.ComplexBusinessRule(
                    "Vek",
                    "Senior citizen validation",
                    async (value, rowData, cancellationToken) =>
                    {
                        await Task.Delay(1500, cancellationToken);
                        var age = Convert.ToInt32(value ?? 0);
                        return age > 65 
                            ? BackgroundValidationResult.Warning("Senior citizen - special benefits may apply")
                            : BackgroundValidationResult.Success();
                    },
                    priority: 3,
                    timeoutMs: 4000);
                
                await _actualDataGrid.AddBackgroundValidationRuleAsync(newRule);
                
                var newCount = _actualDataGrid.GetBackgroundValidationRulesCount();
                await LogAsync($"✅ Background validation rule added - total rules: {newCount}", "INFO");
                UpdateStatus($"BG validation rule pridané", $"Celkom {newCount} pravidiel");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Add BG validation rule error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Get background validation diagnostics
        /// </summary>
        private async void OnGetBgDiagnosticsClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📊 Getting background validation diagnostics", "INFO");
                
                var diagnostics = _actualDataGrid.GetBackgroundValidationDiagnostics();
                var isEnabled = _actualDataGrid.IsBackgroundValidationEnabled();
                var rulesCount = _actualDataGrid.GetBackgroundValidationRulesCount();
                
                await LogAsync("🔍 Background Validation Diagnostics:", "INFO");
                await LogAsync($"  Enabled: {isEnabled}", "INFO");
                await LogAsync($"  Rules Count: {rulesCount}", "INFO");
                await LogAsync($"  Currently Running: {diagnostics?.CurrentlyRunning ?? 0}", "INFO");
                await LogAsync($"  Total Started: {diagnostics?.TotalValidationsStarted ?? 0}", "INFO");
                await LogAsync($"  Total Completed: {diagnostics?.TotalValidationsCompleted ?? 0}", "INFO");
                await LogAsync($"  Average Duration: {diagnostics?.AverageValidationTimeMs ?? 0:F1}ms", "INFO");
                
                UpdateStatus("BG validation diagnostics", $"Running: {diagnostics?.CurrentlyRunning ?? 0}");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Get BG diagnostics error: {ex.Message}", "ERROR");
            }
#endif
        }

        // ================== SEARCH & FILTER TESTING ==================

        /// <summary>
        /// Search for 'Anna' in Name column
        /// </summary>
        private async void OnSearchNameAnnaClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🔍 Searching for 'Anna' in Meno column", "INFO");
                await _actualDataGrid.SetColumnSearchAsync("Meno", "Anna");
                UpdateStatus("Search: Anna v Meno stĺpci", "✅ Search applied");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Search Anna error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Search for 'test' in Email column
        /// </summary>
        private async void OnSearchEmailTestClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📧 Searching for 'test' in Email column", "INFO");
                await _actualDataGrid.SetColumnSearchAsync("Email", "test");
                UpdateStatus("Search: test v Email stĺpci", "✅ Search applied");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Search Email error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Clear all search filters
        /// </summary>
        private async void OnClearSearchClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🗑️ Clearing all search filters", "INFO");
                await _actualDataGrid.ClearAllSearchFiltersAsync();
                UpdateStatus("Všetky search filtre vymazané", "✅ Search cleared");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Clear search error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Sort by Age column
        /// </summary>
        private async void OnSortByAgeClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🔢 Sorting by Vek column (ascending)", "INFO");
                await _actualDataGrid.SortByColumnAsync("Vek", SortDirection.Ascending);
                UpdateStatus("Sorted by Vek (ascending)", "✅ Sort applied");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Sort by age error: {ex.Message}", "ERROR");
            }
#endif
        }

        // ================== CHECKBOX OPERATIONS TESTING ==================

        /// <summary>
        /// Check all rows
        /// </summary>
        private async void OnCheckAllRowsClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("☑️ Checking all rows", "INFO");
                _actualDataGrid.CheckAllRows();
                var checkedCount = _actualDataGrid.GetCheckedRowsCount();
                await LogAsync($"✅ All rows checked - total: {checkedCount}", "INFO");
                UpdateStatus($"Všetky riadky označené", $"Checked: {checkedCount}");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Check all rows error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Uncheck all rows
        /// </summary>
        private async void OnUncheckAllRowsClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("☐ Unchecking all rows", "INFO");
                _actualDataGrid.UncheckAllRows();
                var checkedCount = _actualDataGrid.GetCheckedRowsCount();
                await LogAsync($"✅ All rows unchecked - remaining: {checkedCount}", "INFO");
                UpdateStatus($"Všetky riadky odznačené", $"Checked: {checkedCount}");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Uncheck all rows error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Get checked rows count
        /// </summary>
        private async void OnGetCheckedCountClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                var checkedCount = _actualDataGrid.GetCheckedRowsCount();
                var checkedIndices = _actualDataGrid.GetCheckedRowIndices();
                
                await LogAsync($"📊 Checked rows count: {checkedCount}", "INFO");
                await LogAsync($"📊 Checked indices: [{string.Join(", ", checkedIndices)}]", "INFO");
                UpdateStatus($"Checked rows: {checkedCount}", $"Indices: {string.Join(",", checkedIndices)}");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Get checked count error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Delete checked rows
        /// </summary>
        private async void OnDeleteCheckedClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                var beforeCount = _actualDataGrid.GetCheckedRowsCount();
                await LogAsync($"🗑️ Deleting {beforeCount} checked rows", "INFO");
                
                await _actualDataGrid.DeleteAllCheckedRowsAsync();
                
                var afterCount = _actualDataGrid.GetCheckedRowsCount();
                await LogAsync($"✅ Deleted checked rows - remaining checked: {afterCount}", "INFO");
                UpdateStatus($"Checked rows deleted", $"Deleted: {beforeCount}, Remaining: {afterCount}");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Delete checked rows error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Export only checked rows
        /// </summary>
        private async void OnExportCheckedClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📤 Exporting only checked rows", "INFO");
                var dataTable = await _actualDataGrid.ExportCheckedRowsOnlyAsync(includeValidAlerts: false);
                await LogAsync($"✅ Exported checked rows: {dataTable.Rows.Count} rows", "INFO");
                UpdateStatus($"Checked rows exported", $"Exported: {dataTable.Rows.Count} rows");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Export checked rows error: {ex.Message}", "ERROR");
            }
#endif
        }

        // ================== NAVIGATION TESTING ==================

        /// <summary>
        /// Move to cell [0,0]
        /// </summary>
        private async void OnMoveToCell00Click(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🎯 Moving to cell [0,0]", "INFO");
                await _actualDataGrid.MoveToCellAsync(0, 0);
                UpdateStatus("Moved to cell [0,0]", "✅ Navigation successful");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Move to cell error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Move to next cell
        /// </summary>
        private async void OnMoveToNextCellClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("➡️ Moving to next cell", "INFO");
                await _actualDataGrid.MoveToNextCellAsync(0, 1);  // From [0,1] to next
                UpdateStatus("Moved to next cell", "✅ Navigation successful");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Move to next cell error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Move to previous cell
        /// </summary>
        private async void OnMoveToPrevCellClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("⬅️ Moving to previous cell", "INFO");
                await _actualDataGrid.MoveToPreviousCellAsync(0, 2);  // From [0,2] to previous
                UpdateStatus("Moved to previous cell", "✅ Navigation successful");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Move to previous cell error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Select all cells
        /// </summary>
        private async void OnSelectAllCellsClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("🎯 Selecting all cells", "INFO");
                await _actualDataGrid.SelectAllCellsAsync();
                UpdateStatus("All cells selected", "✅ Selection successful");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Select all cells error: {ex.Message}", "ERROR");
            }
#endif
        }

        // ================== IMPORT/EXPORT TESTING ==================

        /// <summary>
        /// Export to CSV file
        /// </summary>
        private async void OnExportCsvClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📄 Exporting to CSV file", "INFO");
                var result = await _actualDataGrid.ExportToCsvFileAsync("demo_export.csv", includeHeaders: true);
                await LogAsync($"✅ CSV export result: {result}", "INFO");
                UpdateStatus("CSV export completed", result);
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ CSV export error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Export to JSON file
        /// </summary>
        private async void OnExportJsonClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📄 Exporting to JSON file", "INFO");
                var result = await _actualDataGrid.ExportToJsonFileAsync("demo_export.json");
                await LogAsync($"✅ JSON export result: {result}", "INFO");
                UpdateStatus("JSON export completed", result);
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ JSON export error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Import from CSV file
        /// </summary>
        private async void OnImportCsvClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📥 Importing from CSV file (if exists)", "INFO");
                var result = await _actualDataGrid.ImportFromCsvAsync("demo_export.csv", 
                    includeHeaders: true, validateOnImport: true);
                await LogAsync($"✅ CSV import result: Success={result.IsSuccessful}, Rows={result.SuccessfullyImportedRows}", "INFO");
                UpdateStatus($"CSV import: {result.SuccessfullyImportedRows} rows", 
                    result.IsSuccessful ? "✅ Success" : "❌ Failed");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ CSV import error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Get export history
        /// </summary>
        private async void OnGetExportHistoryClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📊 Getting export history", "INFO");
                var history = _actualDataGrid.GetExportHistory();
                await LogAsync($"📊 Export history entries: {history.Count}", "INFO");
                foreach (var entry in history.Take(5)) // Show first 5
                {
                    await LogAsync($"  {entry.Key}: {entry.Value}", "INFO");
                }
                UpdateStatus($"Export history: {history.Count} entries", "✅ Retrieved");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Get export history error: {ex.Message}", "ERROR");
            }
#endif
        }

        // ================== LOGGERCOMPONENT TESTING ==================

        /// <summary>
        /// Log info message
        /// </summary>
        private async void OnLogInfoClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await _logger.Info($"ℹ️ Test info message at {DateTime.Now:HH:mm:ss}");
                await LogAsync("ℹ️ Info message logged via LoggerComponent", "INFO");
                UpdateStatus("Info message logged", "✅ Logged");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Log info error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        private async void OnLogWarningClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await _logger.Warning($"⚠️ Test warning message at {DateTime.Now:HH:mm:ss}");
                await LogAsync("⚠️ Warning message logged via LoggerComponent", "INFO");
                UpdateStatus("Warning message logged", "✅ Logged");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Log warning error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Log error message
        /// </summary>
        private async void OnLogErrorClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await _logger.Error($"❌ Test error message at {DateTime.Now:HH:mm:ss}");
                await LogAsync("❌ Error message logged via LoggerComponent", "INFO");
                UpdateStatus("Error message logged", "✅ Logged");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Log error error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Get logger diagnostics
        /// </summary>
        private async void OnGetLoggerDiagnosticsClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📊 LoggerComponent Diagnostics:", "INFO");
                await LogAsync($"  Current log file: {_logger.CurrentLogFile}", "INFO");
                await LogAsync($"  File size: {_logger.CurrentFileSizeMB:F2} MB", "INFO");
                await LogAsync($"  Rotation enabled: {_logger.IsRotationEnabled}", "INFO");
                await LogAsync($"  Rotation file count: {_logger.RotationFileCount}", "INFO");
                await LogAsync($"  External logger type: {_logger.ExternalLoggerType}", "INFO");
                
                UpdateStatus("Logger diagnostics retrieved", $"File: {_logger.CurrentFileSizeMB:F2}MB");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Get logger diagnostics error: {ex.Message}", "ERROR");
            }
#endif
        }

        // ================== ADVANCED TESTING ==================

        /// <summary>
        /// Load large dataset (100 rows)
        /// </summary>
        private async void OnLoadLargeDatasetClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("📊 Loading large dataset (100 rows)", "INFO");
                
                var largeData = new List<Dictionary<string, object?>>();
                var random = new Random();
                var names = new[] { "Anna", "Peter", "Eva", "Ján", "Mária", "Tomáš", "Zuzana", "Michal", "Katarína", "Lukáš" };
                var domains = new[] { "test.sk", "example.com", "sample.org", "demo.net", "trial.sk" };
                
                for (int i = 1; i <= 100; i++)
                {
                    var name = names[random.Next(names.Length)] + $" {i}";
                    var email = $"{name.ToLower().Replace(" ", ".")}@{domains[random.Next(domains.Length)]}";
                    var age = random.Next(18, 70);
                    var taxNumber = random.Next(10000000, 99999999).ToString();
                    
                    largeData.Add(new Dictionary<string, object?>
                    {
                        ["ID"] = i,
                        ["Meno"] = name,
                        ["Email"] = email,
                        ["Vek"] = age,
                        ["TaxNumber"] = taxNumber
                    });
                }
                
                await _actualDataGrid.LoadDataAsync(largeData);
                await LogAsync($"✅ Large dataset loaded: {largeData.Count} rows", "INFO");
                UpdateStatus($"Large dataset loaded: {largeData.Count} rows", "✅ Performance test ready");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Load large dataset error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Test all validation functionality
        /// </summary>
        private async void OnTestAllValidationClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("✅ Testing all validation functionality", "INFO");
                
                // Test realtime validation
                var isValid = await _actualDataGrid.ValidateAllRowsAsync();
                await LogAsync($"  ValidateAllRowsAsync: {isValid}", "INFO");
                
                var areAllValid = await _actualDataGrid.AreAllNonEmptyRowsValidAsync();
                await LogAsync($"  AreAllNonEmptyRowsValidAsync: {areAllValid}", "INFO");
                
                // Test background validation
                var bgEnabled = _actualDataGrid.IsBackgroundValidationEnabled();
                var bgRulesCount = _actualDataGrid.GetBackgroundValidationRulesCount();
                await LogAsync($"  Background validation enabled: {bgEnabled}", "INFO");
                await LogAsync($"  Background rules count: {bgRulesCount}", "INFO");
                
                UpdateStatus("All validations tested", $"RT: {isValid}, BG: {bgEnabled}");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Test all validation error: {ex.Message}", "ERROR");
            }
#endif
        }

        /// <summary>
        /// Stress test - multiple operations
        /// </summary>
        private async void OnStressTestClick(object sender, RoutedEventArgs e)
        {
#if !NO_PACKAGE
            try
            {
                await LogAsync("⚡ Starting stress test", "INFO");
                
                // Test 1: Load data
                var testData = new List<Dictionary<string, object?>>();
                for (int i = 1; i <= 20; i++)
                {
                    testData.Add(new Dictionary<string, object?>
                    {
                        ["ID"] = i,
                        ["Meno"] = $"Stress Test {i}",
                        ["Email"] = $"stress{i}@test.sk",
                        ["Vek"] = 20 + i,
                        ["TaxNumber"] = (10000000 + i).ToString()
                    });
                }
                await _actualDataGrid.LoadDataAsync(testData);
                await LogAsync("  ✅ Data loaded", "INFO");
                
                // Test 2: Validation
                await _actualDataGrid.ValidateAllRowsAsync();
                await LogAsync("  ✅ Validation completed", "INFO");
                
                // Test 3: Search
                await _actualDataGrid.SetColumnSearchAsync("Meno", "Stress");
                await Task.Delay(500);
                await _actualDataGrid.ClearAllSearchFiltersAsync();
                await LogAsync("  ✅ Search operations completed", "INFO");
                
                // Test 4: Checkbox operations
                _actualDataGrid.CheckAllRows();
                await Task.Delay(200);
                _actualDataGrid.UncheckAllRows();
                await LogAsync("  ✅ Checkbox operations completed", "INFO");
                
                // Test 5: Export
                await _actualDataGrid.ExportToDataTableAsync();
                await LogAsync("  ✅ Export completed", "INFO");
                
                await LogAsync("🎉 Stress test completed successfully!", "INFO");
                UpdateStatus("Stress test completed", "✅ All operations successful");
            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Stress test error: {ex.Message}", "ERROR");
            }
#endif
        }

        #endregion

        /// <summary>
        /// ✅ KOMPLETNÉ DEMO: Testovanie všetkých PUBLIC API metód komponentov
        /// </summary>
        private async Task DemoAllPublicApiMethodsAsync()
        {
#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid == null || _logger == null) return;

                await LogAsync("🧪 === DEMO VŠETKÝCH PUBLIC API METÓD ===", "INFO");
                await Task.Delay(500);

                // ✅ 1. ZÁKLADNÉ API - Dátové operácie
                await LogAsync("📊 Testing basic data operations...", "INFO");
                
                var currentData = await _actualDataGrid.GetAllDataAsync();
                await LogAsync($"  GetAllDataAsync: {currentData.Count} rows", "INFO");
                
                var dataTable = await _actualDataGrid.ExportToDataTableAsync();
                await LogAsync($"  ExportToDataTableAsync: {dataTable.Rows.Count} rows", "INFO");

                // ✅ 2. VALIDATION API
                await LogAsync("✅ Testing validation operations...", "INFO");
                
                var isValid = await _actualDataGrid.ValidateAllRowsAsync();
                await LogAsync($"  ValidateAllRowsAsync: {isValid}", "INFO");
                
                var areAllValid = await _actualDataGrid.AreAllNonEmptyRowsValidAsync();
                await LogAsync($"  AreAllNonEmptyRowsValidAsync: {areAllValid}", "INFO");

                // ✅ 3. SEARCH & SORT API
                await LogAsync("🔍 Testing search and sort operations...", "INFO");
                
                await _actualDataGrid.SetColumnSearchAsync("Meno", "Anna");
                await LogAsync("  SetColumnSearchAsync: searching 'Anna' in Meno column", "INFO");
                await Task.Delay(1000);
                
                await _actualDataGrid.ClearAllSearchFiltersAsync();
                await LogAsync("  ClearAllSearchFiltersAsync: cleared all filters", "INFO");

                // ✅ 4. CHECKBOX OPERATIONS API
                await LogAsync("☑️ Testing checkbox operations...", "INFO");
                
                var checkedCount = _actualDataGrid.GetCheckedRowsCount();
                await LogAsync($"  GetCheckedRowsCount: {checkedCount}", "INFO");
                
                _actualDataGrid.CheckAllRows();
                await LogAsync("  CheckAllRows: all rows checked", "INFO");
                await Task.Delay(500);
                
                var newCheckedCount = _actualDataGrid.GetCheckedRowsCount();
                await LogAsync($"  GetCheckedRowsCount after CheckAll: {newCheckedCount}", "INFO");
                
                _actualDataGrid.UncheckAllRows();
                await LogAsync("  UncheckAllRows: all rows unchecked", "INFO");

                // ✅ 5. BACKGROUND VALIDATION API
                await LogAsync("⏳ Testing background validation API...", "INFO");
                
                var bgEnabled = _actualDataGrid.IsBackgroundValidationEnabled();
                await LogAsync($"  IsBackgroundValidationEnabled: {bgEnabled}", "INFO");
                
                var bgRulesCount = _actualDataGrid.GetBackgroundValidationRulesCount();
                await LogAsync($"  GetBackgroundValidationRulesCount: {bgRulesCount}", "INFO");
                
                var bgDiagnostics = _actualDataGrid.GetBackgroundValidationDiagnostics();
                await LogAsync($"  GetBackgroundValidationDiagnostics: Running={bgDiagnostics?.CurrentlyRunning}, Completed={bgDiagnostics?.TotalValidationsCompleted}", "INFO");

                // ✅ 6. PŘIDÁNÍ NOVÉHO BACKGROUND VALIDATION RULE
                await LogAsync("➕ Testing add background validation rule...", "INFO");
                
                var newBgRule = BackgroundValidationRule.ComplexBusinessRule(
                    "Vek",
                    "Age validation rule",
                    async (value, rowData, cancellationToken) =>
                    {
                        await Task.Delay(1000, cancellationToken);
                        var age = Convert.ToInt32(value ?? 0);
                        return age > 65 
                            ? BackgroundValidationResult.Warning("Senior citizen - special considerations may apply")
                            : BackgroundValidationResult.Success();
                    },
                    priority: 5,
                    timeoutMs: 3000);
                
                await _actualDataGrid.AddBackgroundValidationRuleAsync(newBgRule);
                await LogAsync("  AddBackgroundValidationRuleAsync: added age validation rule", "INFO");
                
                var newBgRulesCount = _actualDataGrid.GetBackgroundValidationRulesCount();
                await LogAsync($"  GetBackgroundValidationRulesCount after add: {newBgRulesCount}", "INFO");

                // ✅ 7. COPY/PASTE API
                await LogAsync("📋 Testing copy/paste operations...", "INFO");
                
                await _actualDataGrid.SelectAllCellsAsync();
                await LogAsync("  SelectAllCellsAsync: all cells selected", "INFO");
                await Task.Delay(500);

                // ✅ 8. NAVIGATION API
                await LogAsync("🧭 Testing navigation operations...", "INFO");
                
                await _actualDataGrid.MoveToCellAsync(0, 1);
                await LogAsync("  MoveToCellAsync: moved to cell [0,1]", "INFO");
                await Task.Delay(300);
                
                await _actualDataGrid.MoveToNextCellAsync(0, 1);
                await LogAsync("  MoveToNextCellAsync: moved to next cell", "INFO");
                await Task.Delay(300);

                // ✅ 9. EXPORT API
                await LogAsync("📤 Testing export operations...", "INFO");
                
                var csvResult = await _actualDataGrid.ExportToCsvFileAsync("demo_export.csv");
                await LogAsync($"  ExportToCsvFileAsync: {csvResult}", "INFO");
                
                var jsonResult = await _actualDataGrid.ExportToJsonFileAsync("demo_export.json");
                await LogAsync($"  ExportToJsonFileAsync: {jsonResult}", "INFO");

                // ✅ 10. LOGGERCOMPONENT API
                await LogAsync("📝 Testing LoggerComponent API...", "INFO");
                
                await _logger.Info("Demo info message from LoggerComponent");
                await LogAsync($"  LoggerComponent.Info: message logged", "INFO");
                
                await _logger.Warning("Demo warning message");
                await LogAsync($"  LoggerComponent.Warning: warning logged", "INFO");
                
                await _logger.Debug("Demo debug message");
                await LogAsync($"  LoggerComponent.Debug: debug logged", "INFO");
                
                await _logger.Error("Demo error message");
                await LogAsync($"  LoggerComponent.Error: error logged", "INFO");

                // ✅ LOGGERCOMPONENT DIAGNOSTICS
                await LogAsync($"📊 LoggerComponent diagnostics:", "INFO");
                await LogAsync($"  Current log file: {_logger.CurrentLogFile}", "INFO");
                await LogAsync($"  File size: {_logger.CurrentFileSizeMB:F2} MB", "INFO");
                await LogAsync($"  Rotation enabled: {_logger.IsRotationEnabled}", "INFO");
                await LogAsync($"  External logger type: {_logger.ExternalLoggerType}", "INFO");

                await LogAsync("", "INFO");
                await LogAsync("🎉 === VŠETKY PUBLIC API METÓDY OTESTOVANÉ ===", "INFO");
                await LogAsync("✅ Demo completed successfully! Komponenty sú plne funkčné a nezávislé.", "INFO");

            }
            catch (Exception ex)
            {
                await LogAsync($"❌ Demo API methods error: {ex.Message}", "ERROR");
            }
#endif
        }

        #endregion
    }
}