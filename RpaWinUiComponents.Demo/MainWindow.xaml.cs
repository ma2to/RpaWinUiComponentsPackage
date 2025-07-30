// RpaWinUiComponents.Demo/MainWindow.xaml.cs - MULTI-COMPONENT PACKAGE DEMO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;


// ✅ MULTI-COMPONENT PACKAGE IMPORTS
#if !NO_PACKAGE
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.LoggerComponent;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ColumnDefinition;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ThrottlingConfig;
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.DataGridColorConfig;
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
        private LoggerComponent? _logger;
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
                if (LoadingPanel != null &&
                    DataGridControl != null &&
                    StatusTextBlock != null &&
                    InitStatusText != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ UI elementy pripravené po {attempt + 1} pokusoch");
                    return true;
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

                // ✅ Test LoggerComponent komponentu
                var tempDir = System.IO.Path.GetTempPath();
                _logger = new LoggerComponent(tempDir, "demo-log.log", 10);
                await _logger.LogAsync("Multi-component package test", "INFO");

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
                if (_actualDataGrid != null && DataGridControl != null)
                {
                    DataGridControl.Content = _actualDataGrid;
                    await Task.Delay(200);

                    // ✅ Log inicializácie
                    if (_logger != null)
                    {
                        await _logger.LogAsync("Inicializuje sa AdvancedDataGrid", "INFO");
                    }

                    // ✅ Inicializuj DataGrid s demo dátami
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

                    await _actualDataGrid.InitializeAsync(columns, rules, GridThrottlingConfig.Default, 15, colors);

                    // ✅ Demo dáta
                    var demoData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 1, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 28 },
                        new() { ["ID"] = 2, ["Meno"] = "Peter Svoboda", ["Email"] = "peter@test.sk", ["Vek"] = 34 },
                        new() { ["ID"] = 3, ["Meno"] = "Eva Krásna", ["Email"] = "eva@test.sk", ["Vek"] = 26 }
                    };

                    await _actualDataGrid.LoadDataAsync(demoData);

                    // ✅ Log úspešnej inicializácie
                    if (_logger != null)
                    {
                        await _logger.LogAsync($"DataGrid inicializovaný s {demoData.Count} riadkami", "INFO");
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
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;

                if (DataGridControl != null)
                    DataGridControl.Visibility = Visibility.Visible;

                UpdateStatus("✅ Multi-component demo pripravené!", "🎉 Package je funkčný!");

                if (InitStatusText != null)
                    InitStatusText.Text = "✅ Multi-component package funkčný!";

                System.Diagnostics.Debug.WriteLine("🎉 Multi-component inicializácia úspešne dokončená!");
            }
            catch (Exception ex)
            {
                ShowError($"Dokončenie inicializácie zlyhalo: {ex.Message}");
            }
        }

        private void ShowPackageUnavailable()
        {
            if (LoadingPanel != null)
                LoadingPanel.Visibility = Visibility.Collapsed;

            if (NoPackagePanel != null)
                NoPackagePanel.Visibility = Visibility.Visible;

            UpdateStatus("⚠️ Multi-component package nie je dostupný", "Skontrolujte inštaláciu");

            if (InitStatusText != null)
                InitStatusText.Text = "⚠️ Package Reference chyba";
        }

        private void UpdateStatus(string detailText, string statusText)
        {
            try
            {
                if (LoadingDetailText != null)
                    LoadingDetailText.Text = detailText;

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = statusText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ UpdateStatus chyba: {ex.Message}");
            }
        }

        private void ShowError(string errorMessage)
        {
            UpdateStatus($"❌ {errorMessage}", "Chyba aplikácie");

            if (InitStatusText != null)
                InitStatusText.Text = "❌ Chyba";

            System.Diagnostics.Debug.WriteLine($"❌ Error: {errorMessage}");
        }

        #region Event Handlers

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
                        await _logger.LogAsync("Sample data načítané cez demo", "INFO");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Load data chyba: {ex.Message}");
            }
#endif
        }

        // ✅ Ostatné event handlers rovnaké ako predtým...
        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;
#if !NO_PACKAGE
            if (_actualDataGrid != null)
            {
                var isValid = await _actualDataGrid.ValidateAllRowsAsync();
                UpdateStatus(isValid ? "Všetky dáta validné" : "Validačné chyby",
                           isValid ? "✅ OK" : "❌ Chyby");
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
            }
#endif
        }

        // Ostatné button handlers
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