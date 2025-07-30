// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ OPRAVENÉ LoggerComponent using
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;

// ✅ OPRAVENÉ: Správne using direktívy pre multi-component package
#if !NO_PACKAGE
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
// ✅ KĽÚČOVÁ OPRAVA: Plne kvalifikovaný using pre LoggerComponent
using LoggerComp = RpaWinUiComponentsPackage.Logger.LoggerComponent;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.DataGridColorConfig;
#endif

namespace RpaWinUiComponentsPackage.Demo
{
    public sealed partial class MainWindow : Window
    {
        private bool _packageAvailable = false;
        private bool _isInitialized = false;

        // ✅ OPRAVENÉ: Použitie alias LoggerComp namiesto LoggerComponent
#if !NO_PACKAGE
        private AdvancedDataGrid? _actualDataGrid;
        private LoggerComp? _logger;  // ✅ FIXED: Alias namiesto priameho LoggerComponent
#endif

        public MainWindow()
        {
            try
            {
                this.InitializeComponent();
                System.Diagnostics.Debug.WriteLine("✅ MainWindow InitializeComponent úspešný");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ InitializeComponent chyba: {ex.Message}");
            }

            this.Activated += OnWindowActivated;
        }

        private async void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != WindowActivationState.Deactivated && !_isInitialized)
            {
                this.Activated -= OnWindowActivated;
                _isInitialized = true;

                await Task.Delay(100);
                if (await WaitForUIElementsAsync())
                {
                    await InitializeAsync();
                }
                else
                {
                    ShowError("UI elementy nie sú pripravené");
                }
            }
        }

        private async Task<bool> WaitForUIElementsAsync()
        {
            const int maxAttempts = 10;
            const int delayMs = 50;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    var loadingPanel = this.FindName("LoadingPanel");
                    var dataGridControl = this.FindName("DataGridControl");
                    var statusTextBlock = this.FindName("StatusTextBlock");
                    var initStatusText = this.FindName("InitStatusText");

                    if (loadingPanel != null && dataGridControl != null &&
                        statusTextBlock != null && initStatusText != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ UI elementy pripravené po {attempt + 1} pokusoch");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ UI check attempt {attempt + 1} failed: {ex.Message}");
                }

                await Task.Delay(delayMs);
            }

            return false;
        }

        private async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Inicializuje sa multi-component package demo...");

                UpdateStatus("Kontroluje sa multi-component package...", "📦 Package check...");
                await Task.Delay(300);

                await CheckPackageAvailabilityAsync();

                if (_packageAvailable)
                {
                    await InitializeWithPackageAsync();
                }
                else
                {
                    ShowPackageUnavailable();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Inicializácia zlyhala: {ex.Message}");
            }
        }

        private async Task CheckPackageAvailabilityAsync()
        {
            try
            {
#if !NO_PACKAGE
                System.Diagnostics.Debug.WriteLine("🔍 Pokúšam sa vytvoriť multi-component inštancie...");

                // ✅ Test AdvancedDataGrid komponentu
                _actualDataGrid = new AdvancedDataGrid();

                // ✅ OPRAVENÉ: Vytvorenie LoggerComponent s použitím alias
                var tempDir = System.IO.Path.GetTempPath();
                var logFileName = $"RpaDemo_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
                _logger = new LoggerComp(tempDir, logFileName, 10); // 10MB max size

                if (_logger != null)
                {
                    await _logger.LogAsync("🚀 Multi-component package test started - AdvancedDataGrid + LoggerComponent integration", "INFO");
                }

                await Task.Delay(100);
                _packageAvailable = true;
                System.Diagnostics.Debug.WriteLine("✅ Multi-component package je dostupný");
#else
                _packageAvailable = false;
                System.Diagnostics.Debug.WriteLine("⚠️ Multi-component package nie je dostupný (NO_PACKAGE)");
#endif
            }
            catch (Exception ex)
            {
                _packageAvailable = false;
                System.Diagnostics.Debug.WriteLine($"❌ Multi-component package chyba: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        private async Task InitializeWithPackageAsync()
        {
            try
            {
                UpdateStatus("Multi-component package je dostupný!", "Inicializuje sa DataGrid + Logger...");

#if !NO_PACKAGE
                if (_actualDataGrid != null && _logger != null)
                {
                    try
                    {
                        var dataGridControl = this.FindName("DataGridControl") as ContentControl;
                        if (dataGridControl != null)
                        {
                            dataGridControl.Content = _actualDataGrid;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ DataGrid content setting failed: {ex.Message}");
                    }

                    await Task.Delay(200);

                    // ✅ KĽÚČOVÁ INTEGRÁCIA: Pošli LoggerComponent do AdvancedDataGrid
                    await _logger.LogAsync("🔧 Inicializuje sa AdvancedDataGrid s integrated LoggerComponent", "INFO");

                    // ✅ Demo konfigurácia s LoggerComponent integráciou
                    var columns = new List<GridColumnDefinition>
                    {
                        new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                        new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                        new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                        new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                        new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
                    };

                    var rules = new List<GridValidationRule>
                    {
                        GridValidationRule.Required("Meno", "Meno je povinné"),
                        GridValidationRule.Email("Email", "Neplatný email formát"),
                        GridValidationRule.Range("Vek", 18, 100, "Vek musí byť 18-100")
                    };

                    var colors = new GridDataGridColorConfig
                    {
                        CellBackgroundColor = Microsoft.UI.Colors.White,
                        AlternateRowColor = Color.FromArgb(20, 0, 120, 215)
                    };

                    // ✅ KĽÚČOVÉ: Inicializuj DataGrid S LoggerComponent integráciou (5 parametrov)
                    await _actualDataGrid.InitializeAsync(
                        columns,
                        rules,
                        GridThrottlingConfig.Default,
                        15,
                        colors
                    // Poznámka: LoggerComponent sa pošle cez SetIntegratedLogger metódu
                    );

                    // ✅ NOVÉ: Nastavenie LoggerComponent cez dedikovanú metódu
                    _actualDataGrid.SetIntegratedLogger(_logger, true);

                    await _logger.LogAsync("✅ AdvancedDataGrid úspešne inicializovaný s LoggerComponent integráciou", "INFO");

                    // ✅ Demo dáta s logovaním
                    var demoData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 1, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 28 },
                        new() { ["ID"] = 2, ["Meno"] = "Peter Svoboda", ["Email"] = "peter@test.sk", ["Vek"] = 34 },
                        new() { ["ID"] = 3, ["Meno"] = "Eva Krásna", ["Email"] = "eva@test.sk", ["Vek"] = 26 }
                    };

                    await _actualDataGrid.LoadDataAsync(demoData);
                    await _logger.LogAsync($"📊 Demo dáta načítané: {demoData.Count} riadkov s LoggerComponent integration working!", "INFO");
                }
#endif

                CompleteInitialization();
            }
            catch (Exception ex)
            {
                ShowError($"Multi-component inicializácia zlyhala: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InitializeWithPackageAsync chyba: {ex}");

#if !NO_PACKAGE
                if (_logger != null)
                {
                    await _logger.LogAsync($"❌ CRITICAL ERROR: Multi-component initialization failed: {ex.Message}", "ERROR");
                }
#endif
            }
        }

        private void CompleteInitialization()
        {
            try
            {
                try
                {
                    var loadingPanel = this.FindName("LoadingPanel") as FrameworkElement;
                    if (loadingPanel != null)
                        loadingPanel.Visibility = Visibility.Collapsed;
                }
                catch { }

                try
                {
                    var dataGridControl = this.FindName("DataGridControl") as FrameworkElement;
                    if (dataGridControl != null)
                        dataGridControl.Visibility = Visibility.Visible;
                }
                catch { }

                UpdateStatus("✅ Multi-component demo pripravené!", "🎉 Package funguje s LoggerComponent integration!");

                try
                {
                    var initStatusText = this.FindName("InitStatusText") as TextBlock;
                    if (initStatusText != null)
                        initStatusText.Text = "✅ Multi-component package funkčný s LoggerComponent!";
                }
                catch { }

                System.Diagnostics.Debug.WriteLine("🎉 Multi-component inicializácia úspešne dokončená s LoggerComponent integration!");

#if !NO_PACKAGE
                Task.Run(async () =>
                {
                    if (_logger != null)
                    {
                        await _logger.LogAsync("🎉 Demo aplikácia úspešne spustená s LoggerComponent integráciou", "INFO");
                    }
                });
#endif
            }
            catch (Exception ex)
            {
                ShowError($"Dokončenie inicializácie zlyhalo: {ex.Message}");
            }
        }

        private void ShowPackageUnavailable()
        {
            try
            {
                var loadingPanel = this.FindName("LoadingPanel") as FrameworkElement;
                if (loadingPanel != null)
                    loadingPanel.Visibility = Visibility.Collapsed;
            }
            catch { }

            try
            {
                var noPackagePanel = this.FindName("NoPackagePanel") as FrameworkElement;
                if (noPackagePanel != null)
                    noPackagePanel.Visibility = Visibility.Visible;
            }
            catch { }

            UpdateStatus("⚠️ Multi-component package nie je dostupný", "Skontrolujte inštaláciu");

            try
            {
                var initStatusText = this.FindName("InitStatusText") as TextBlock;
                if (initStatusText != null)
                    initStatusText.Text = "⚠️ Package Reference chyba";
            }
            catch { }
        }

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
                catch { }

                try
                {
                    var statusTextBlock = this.FindName("StatusTextBlock") as TextBlock;
                    if (statusTextBlock != null)
                        statusTextBlock.Text = statusText;
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ UpdateStatus chyba: {ex.Message}");
            }
        }

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

#if !NO_PACKAGE
            Task.Run(async () =>
            {
                if (_logger != null)
                {
                    await _logger.LogAsync($"❌ DEMO ERROR: {errorMessage}", "ERROR");
                }
            });
#endif
        }

        // Zvyšok event handlerov zostáva rovnaký...
        #region Event Handlers s LoggerComponent integráciou (skrátené kvôli limitu)

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid != null && _logger != null)
                {
                    var sampleData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 101, ["Meno"] = "Nový User", ["Email"] = "novy@test.sk", ["Vek"] = 25 }
                    };

                    await _actualDataGrid.LoadDataAsync(sampleData);
                    UpdateStatus("Sample data načítané", "✅ Data load úspešný");

                    await _logger.LogAsync("📊 Sample data načítané cez demo UI - AdvancedDataGrid + LoggerComponent working!", "INFO");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Load data chyba: {ex.Message}");

                if (_logger != null)
                {
                    await _logger.LogAsync($"❌ Load data ERROR: {ex.Message}", "ERROR");
                }
            }
#endif
        }

        // Ostatné event handlery... (skrátené kvôli limitu znakov)

        #endregion
    }
}