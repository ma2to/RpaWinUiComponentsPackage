// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ OPRAVENÝ XAML loading issue
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

// ✅ Conditional import s alias pre vyriešenie konfliktov
#if !NO_PACKAGE
using RpaWinUiComponents.AdvancedWinUiDataGrid;
using GridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using GridValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;
using GridDataGridColorConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.DataGridColorConfig;
#endif

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        private bool _packageAvailable = false;
        private bool _isInitialized = false; // ✅ NOVÉ: Prevent multiple initialization

        // ✅ Reference na skutočný DataGrid ak je dostupný
#if !NO_PACKAGE
        private AdvancedDataGrid? _actualDataGrid;
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

            // ✅ OPRAVENÉ: Použiť Activated s lepším timing-om
            this.Activated += OnWindowActivated;
        }

        private async void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            // ✅ Spusti inicializáciu iba pri prvej aktivácii a ak ešte nie je inicializované
            if (e.WindowActivationState != WindowActivationState.Deactivated && !_isInitialized)
            {
                this.Activated -= OnWindowActivated; // Odpoj handler aby sa spustil iba raz
                _isInitialized = true;

                // ✅ KĽÚČOVÁ OPRAVA: Pridaj malé oneskorenie aby sa XAML stihol úplne načítať
                await Task.Delay(100);

                // ✅ NOVÉ: Skontroluj či sú UI elementy pripravené
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

        // ✅ NOVÁ metóda: Počká kým sa UI elementy nenačítajú
        private async Task<bool> WaitForUIElementsAsync()
        {
            const int maxAttempts = 10;
            const int delayMs = 50;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Skontroluj či sú kľúčové UI elementy pripravené
                if (LoadingPanel != null &&
                    DataGridControl != null &&
                    StatusTextBlock != null &&
                    InitStatusText != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ UI elementy pripravené po {attempt + 1} pokusoch");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"⏳ UI elementy nie sú pripravené, pokus {attempt + 1}/{maxAttempts}");
                await Task.Delay(delayMs);
            }

            System.Diagnostics.Debug.WriteLine("❌ UI elementy sa nepodarilo načítať ani po 10 pokusoch");
            return false;
        }

        private async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Inicializuje sa demo aplikácia...");

                UpdateStatus("Kontroluje sa package dostupnosť...", "📦 Package check...");
                await Task.Delay(300); // ✅ Mierne zvýšené oneskorenie

                // ✅ Skontroluj dostupnosť package
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
                // ✅ OPRAVENÉ: Bezpečnejšie vytvorenie inštancie s try-catch
                System.Diagnostics.Debug.WriteLine("🔍 Pokúšam sa vytvoriť AdvancedDataGrid inštanciu...");

                _actualDataGrid = new AdvancedDataGrid();

                // ✅ NOVÉ: Počkaj chvíľu aby sa DataGrid stihol inicializovať
                await Task.Delay(100);

                _packageAvailable = true;
                System.Diagnostics.Debug.WriteLine("✅ Package je dostupný a DataGrid vytvorený");
#else
                _packageAvailable = false;
                System.Diagnostics.Debug.WriteLine("⚠️ Package nie je dostupný (NO_PACKAGE definované)");
#endif
            }
            catch (Exception ex)
            {
                _packageAvailable = false;
                System.Diagnostics.Debug.WriteLine($"❌ Package nie je dostupný: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }

            await Task.CompletedTask;
        }

        private async Task InitializeWithPackageAsync()
        {
            try
            {
                UpdateStatus("Package je dostupný!", "Inicializuje sa DataGrid...");

#if !NO_PACKAGE
                if (_actualDataGrid != null && DataGridControl != null)
                {
                    System.Diagnostics.Debug.WriteLine("🔧 Nastavujem DataGrid do ContentControl...");

                    // ✅ Nastav skutočný DataGrid ako obsah ContentControl
                    DataGridControl.Content = _actualDataGrid;

                    // ✅ Pridaj malý delay pred inicializáciou DataGrid
                    await Task.Delay(200);

                    System.Diagnostics.Debug.WriteLine("🔧 Inicializujem DataGrid s demo dátami...");

                    // ✅ Inicializuj DataGrid s demo dátami - OPRAVENÉ typy
                    var columns = new List<GridColumnDefinition>
                    {
                        new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                        new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                        new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                        new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                        new("Plat", typeof(decimal)) { MinWidth = 120, Width = 120, Header = "💰 Plat" },
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
                        AlternateRowColor = Color.FromArgb(20, 0, 120, 215),
                        ValidationErrorColor = Microsoft.UI.Colors.Red
                    };

                    System.Diagnostics.Debug.WriteLine("🔧 Volám InitializeAsync na DataGrid...");
                    await _actualDataGrid.InitializeAsync(columns, rules, GridThrottlingConfig.Default, 15, colors);
                    System.Diagnostics.Debug.WriteLine("✅ DataGrid InitializeAsync dokončené");

                    // ✅ Načítaj demo dáta
                    var demoData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 1, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 28, ["Plat"] = 2500m },
                        new() { ["ID"] = 2, ["Meno"] = "Peter Svoboda", ["Email"] = "peter@company.sk", ["Vek"] = 34, ["Plat"] = 3200m },
                        new() { ["ID"] = 3, ["Meno"] = "Eva Krásna", ["Email"] = "eva@firma.sk", ["Vek"] = 26, ["Plat"] = 2800m }
                    };

                    System.Diagnostics.Debug.WriteLine("🔧 Načítavam demo dáta...");
                    await _actualDataGrid.LoadDataAsync(demoData);
                    System.Diagnostics.Debug.WriteLine("✅ Demo dáta načítané");
                }
#endif

                CompleteInitialization();
            }
            catch (Exception ex)
            {
                ShowError($"DataGrid inicializácia zlyhala: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InitializeWithPackageAsync chyba: {ex}");
            }
        }

        private void CompleteInitialization()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎉 Dokončujem inicializáciu...");

                // ✅ Skry loading a zobraz DataGrid
                if (LoadingPanel != null)
                {
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    System.Diagnostics.Debug.WriteLine("✅ LoadingPanel skrytý");
                }

                if (DataGridControl != null)
                {
                    DataGridControl.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine("✅ DataGridControl zobrazený");
                }

                UpdateStatus("✅ Demo je pripravené!", "🎉 Package je funkčný!");

                if (InitStatusText != null)
                {
                    InitStatusText.Text = "✅ Package je funkčný!";
                    System.Diagnostics.Debug.WriteLine("✅ InitStatusText aktualizovaný");
                }

                System.Diagnostics.Debug.WriteLine("🎉 Inicializácia úspešne dokončená!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CompleteInitialization chyba: {ex}");
                ShowError($"Dokončenie inicializácie zlyhalo: {ex.Message}");
            }
        }

        private void ShowPackageUnavailable()
        {
            if (LoadingPanel != null)
                LoadingPanel.Visibility = Visibility.Collapsed;

            if (NoPackagePanel != null)
                NoPackagePanel.Visibility = Visibility.Visible;

            UpdateStatus("⚠️ Package nie je dostupný", "Skontrolujte inštaláciu balíka");

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

        #region ✅ Event Handlers - Safe implementations

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable)
            {
                ShowError("Package nie je dostupný");
                return;
            }

#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid != null)
                {
                    var sampleData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 101, ["Meno"] = "Nový Používateľ", ["Email"] = "novy@test.sk", ["Vek"] = 25, ["Plat"] = 2000m },
                        new() { ["ID"] = 102, ["Meno"] = "Test User", ["Email"] = "test@company.sk", ["Vek"] = 30, ["Plat"] = 2500m }
                    };

                    await _actualDataGrid.LoadDataAsync(sampleData);
                    UpdateStatus("Sample data načítané", "✅ Data load úspešný");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Load data chyba: {ex.Message}");
            }
#endif
        }

        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid != null)
                {
                    var isValid = await _actualDataGrid.ValidateAllRowsAsync();
                    UpdateStatus(isValid ? "Všetky dáta sú validné" : "Našli sa validačné chyby",
                               isValid ? "✅ Validácia OK" : "❌ Validačné chyby");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Validácia chyba: {ex.Message}");
            }
#endif
        }

        private async void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid != null)
                {
                    await _actualDataGrid.ClearAllDataAsync();
                    UpdateStatus("Dáta vyčistené", "✅ Clear úspešný");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Clear data chyba: {ex.Message}");
            }
#endif
        }

        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            if (!_packageAvailable) return;

#if !NO_PACKAGE
            try
            {
                if (_actualDataGrid != null)
                {
                    var dataTable = await _actualDataGrid.ExportToDataTableAsync();
                    UpdateStatus($"Export: {dataTable.Rows.Count} riadkov", "✅ Export úspešný");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Export chyba: {ex.Message}");
            }
#endif
        }

        // ✅ Ostatné button handlers - jednoduché implementácie
        private void OnApplyLightThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Light theme", "🎨 Téma zmenená");

        private void OnApplyDarkThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Dark theme", "🎨 Téma zmenená");

        private void OnApplyBlueThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Blue theme", "🎨 Téma zmenená");

        private void OnApplyCustomThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Custom theme", "🎨 Téma zmenená");

        private void OnResetThemeClick(object sender, RoutedEventArgs e) =>
            UpdateStatus("Default theme", "🎨 Téma resetovaná");

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