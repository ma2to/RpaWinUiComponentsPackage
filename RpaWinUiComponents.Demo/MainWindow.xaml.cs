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

                    // ✅ Demo konfigurácia s rozšíreným logovaním
                    var columns = new List<GridColumnDefinition>
                    {
                        new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                        new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                        new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                        new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                        new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
                    };

                    await LogAsync($"Created {columns.Count} column definitions", "DEBUG");

                    var rules = new List<GridValidationRule>
                    {
                        GridValidationRule.Required("Meno", "Meno je povinné"),
                        GridValidationRule.Email("Email", "Neplatný email formát"),
                        GridValidationRule.Range("Vek", 18, 100, "Vek musí byť 18-100")
                    };

                    await LogAsync($"Created {rules.Count} validation rules", "DEBUG");

                    var colors = new GridDataGridColorConfig
                    {
                        CellBackgroundColor = Microsoft.UI.Colors.White,
                        AlternateRowColor = Color.FromArgb(20, 0, 120, 215)
                    };

                    await LogAsync("Created color configuration with zebra rows", "DEBUG");

                    // ✅ KĽÚČOVÉ: InitializeAsync s LoggerComponent parametrom (6 parametrov)
                    await LogAsync("🎯 Calling DataGrid.InitializeAsync with LoggerComponent integration", "INFO");

                    await _actualDataGrid.InitializeAsync(
                        columns,
                        rules,
                        GridThrottlingConfig.Default,
                        15,
                        colors,
                        _logger  // ✅ OPRAVENÉ: LoggerComponent parameter
                    );

                    await _logger.LogAsync("✅ AdvancedDataGrid successfully initialized with LoggerComponent integration", "INFO");

                    // ✅ Demo dáta s detailným logovaním
                    var demoData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 1, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 28 },
                        new() { ["ID"] = 2, ["Meno"] = "Peter Svoboda", ["Email"] = "peter@test.sk", ["Vek"] = 34 },
                        new() { ["ID"] = 3, ["Meno"] = "Eva Krásna", ["Email"] = "eva@test.sk", ["Vek"] = 26 }
                    };

                    await LogAsync($"Loading {demoData.Count} demo data rows", "INFO");
                    await _actualDataGrid.LoadDataAsync(demoData);
                    await _logger.LogAsync($"📊 Demo data loaded: {demoData.Count} rows with complete LoggerComponent integration!", "INFO");
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

        #endregion
    }
}