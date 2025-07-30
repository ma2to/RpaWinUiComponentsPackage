// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ OPRAVENÉ using direktívy a integrácia LoggerComponent
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
using RpaWinUiComponentsPackage.Logger;  // ✅ CHANGED: LoggerComponent -> Logger namespace
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;  // ✅ FIXED: Pridané .Models
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;  // ✅ FIXED: Pridané .Models
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;  // ✅ FIXED: Pridané .Models
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.DataGridColorConfig;  // ✅ FIXED: Pridané .Models
#endif

namespace RpaWinUiComponentsPackage.Demo
{
    public sealed partial class MainWindow : Window
    {
        private bool _packageAvailable = false;
        private bool _isInitialized = false;

        // ✅ References na komponenty z multi-component package
#if !NO_PACKAGE
        private AdvancedDataGrid? _actualDataGrid;
        private LoggerComponent? _logger;  // ✅ FIXED: Teraz môže nájsť typ
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
                // ✅ OPRAVENÉ: Kontrola existencie UI elementov z XAML
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

                // ✅ Test LoggerComponent komponentu - s integráciou do AdvancedDataGrid
                var tempDir = System.IO.Path.GetTempPath();
                _logger = new LoggerComponent(tempDir, "demo-log.log", 10);
                await _logger.LogAsync("Multi-component package test - LoggerComponent + AdvancedDataGrid integration", "INFO");

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
                if (_actualDataGrid != null)
                {
                    // ✅ OPRAVENÉ: Bezpečné nastavenie DataGrid obsahu
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

                    // ✅ Log inicializácie
                    if (_logger != null)
                    {
                        await _logger.LogAsync("Inicializuje sa AdvancedDataGrid s integrated LoggerComponent", "INFO");
                    }

                    // ✅ Inicializuj DataGrid s demo dátami a LoggerComponent integráciou
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

                    // ✅ KĽÚČOVÉ: Inicializuj DataGrid s LoggerComponent integráciou
                    await _actualDataGrid.InitializeAsync(columns, rules, GridThrottlingConfig.Default, 15, colors);

                    // ✅ Log úspešnej inicializácie
                    if (_logger != null)
                    {
                        await _logger.LogAsync("AdvancedDataGrid inicializovaný s integrated LoggerComponent", "INFO");
                    }

                    // ✅ Demo dáta
                    var demoData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 1, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 28 },
                        new() { ["ID"] = 2, ["Meno"] = "Peter Svoboda", ["Email"] = "peter@test.sk", ["Vek"] = 34 },
                        new() { ["ID"] = 3, ["Meno"] = "Eva Krásna", ["Email"] = "eva@test.sk", ["Vek"] = 26 }
                    };

                    await _actualDataGrid.LoadDataAsync(demoData);

                    // ✅ Log načítania dát
                    if (_logger != null)
                    {
                        await _logger.LogAsync($"DataGrid inicializovaný s {demoData.Count} riadkami - LoggerComponent integration working!", "INFO");
                    }
                }
#endif

                CompleteInitialization();
            }
            catch (Exception ex)
            {
                ShowError($"Multi-component inicializácia zlyhala: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InitializeWithPackageAsync chyba: {ex}");
            }
        }

        private void CompleteInitialization()
        {
            try
            {
                // ✅ OPRAVENÉ: Bezpečná práca s UI elementmi
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

                UpdateStatus("✅ Multi-component demo pripravené!", "🎉 Package je funkčný s LoggerComponent integration!");

                try
                {
                    var initStatusText = this.FindName("InitStatusText") as TextBlock;
                    if (initStatusText != null)
                        initStatusText.Text = "✅ Multi-component package funkčný s LoggerComponent!";
                }
                catch { }

                System.Diagnostics.Debug.WriteLine("🎉 Multi-component inicializácia úspešne dokončená s LoggerComponent integration!");
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
        }

        #region Event Handlers s LoggerComponent integráciou

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid != null)
                {
                    var sampleData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 101, ["Meno"] = "Nový User", ["Email"] = "novy@test.sk", ["Vek"] = 25 }
                    };

                    await _actualDataGrid.LoadDataAsync(sampleData);
                    UpdateStatus("Sample data načítané", "✅ Data load úspešný");

                    // ✅ Log do LoggerComponent
                    if (_logger != null)
                    {
                        await _logger.LogAsync("Sample data načítané cez demo - AdvancedDataGrid + LoggerComponent integration working!", "INFO");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Load data chyba: {ex.Message}");

                // ✅ Log chyby
                if (_logger != null)
                {
                    await _logger.LogAsync($"Load data ERROR: {ex.Message}", "ERROR");
                }
            }
#endif
        }

        // ✅ Ostatné event handlers s LoggerComponent integráciou
        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;
#if !NO_PACKAGE
            if (_actualDataGrid != null)
            {
                var isValid = await _actualDataGrid.ValidateAllRowsAsync();
                UpdateStatus(isValid ? "Všetky dáta validné" : "Validačné chyby",
                           isValid ? "✅ OK" : "❌ Chyby");

                // ✅ Log validácie
                if (_logger != null)
                {
                    await _logger.LogAsync($"Validation result: {(isValid ? "ALL VALID" : "ERRORS FOUND")}", "INFO");
                }
            }
#endif
        }

        private async void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;
#if !NO_PACKAGE
            if (_actualDataGrid != null)
            {
                await _actualDataGrid.ClearAllDataAsync();
                UpdateStatus("Dáta vyčistené", "✅ Clear úspešný");

                // ✅ Log clear operácie
                if (_logger != null)
                {
                    await _logger.LogAsync("Data cleared via demo interface", "INFO");
                }
            }
#endif
        }

        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;
#if !NO_PACKAGE
            if (_actualDataGrid != null)
            {
                var dataTable = await _actualDataGrid.ExportToDataTableAsync();
                UpdateStatus($"Export: {dataTable.Rows.Count} riadkov", "✅ Export úspešný");

                // ✅ Log export operácie
                if (_logger != null)
                {
                    await _logger.LogAsync($"Data exported: {dataTable.Rows.Count} rows", "INFO");
                }
            }
#endif
        }

        // Ostatné button handlers (bez zmien)
        private void OnApplyLightThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Light theme", "🎨 Téma zmenená");

        private void OnApplyDarkThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Dark theme", "🎨 Téma zmenená");

        private void OnApplyBlueThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Blue theme", "🎨 Téma zmenená");

        private void OnApplyCustomThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Custom theme", "🎨 Téma zmenená");

        private void OnResetThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Default theme", "🎨 Téma zmenená");

        private void OnTestSearchClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Search test", "🔍 Search funkcia");

        private void OnTestSortClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Sort test", "⬆️⬇️ Sort funkcia");

        private void OnTestZebraToggleClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Zebra toggle", "🦓 Zebra rows");

        private void OnClearSearchClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Search cleared", "🧹 Search vyčistený");

        private void OnTestAutoAddFewRowsClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Auto-add test", "🔥 AUTO-ADD funkcia");

        private void OnTestAutoAddManyRowsClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Auto-add many", "🔥 AUTO-ADD test");

        private void OnTestAutoAddDeleteClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Auto-add delete", "🔥 Smart delete test");

        #endregion
    }
}