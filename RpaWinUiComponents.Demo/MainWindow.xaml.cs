// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ OPRAVENÝ s Auto-Add volaniami
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

// ✅ KĽÚČOVÉ: Import iba PUBLIC API tried z balíka
using RpaWinUiComponents.AdvancedWinUiDataGrid;

// ✅ EXPLICITNÉ importy PUBLIC tried pre zamedzenie konfliktov
using PublicColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using PublicValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using PublicThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;
using PublicDataGridColorTheme = RpaWinUiComponents.AdvancedWinUiDataGrid.DataGridColorTheme;
using PublicDataGridColorThemeBuilder = RpaWinUiComponents.AdvancedWinUiDataGrid.DataGridColorThemeBuilder;

// ✅ Windows.UI.Color pre farby
using Windows.UI;

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        private bool _isInitialized = false;

        public MainWindow()
        {
            this.InitializeComponent();

            // ✅ Bezpečná inicializácia cez DispatcherQueue
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(500); // Počkaj na UI load
                await InitializeComponentAsync();
            });
        }

        private async Task InitializeComponentAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 ŠTART inicializácie Demo aplikácie s AUTO-ADD funkcionalitou...");

                UpdateLoadingState("Inicializuje sa balík v1.0.6...", "Načítava sa z Package Reference s AUTO-ADD funkciou...");
                await Task.Delay(300);

                // ✅ OVERENIE dostupnosti komponentu
                if (DataGridControl == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ KRITICKÁ CHYBA: DataGridControl je NULL!");
                    ShowError("DataGridControl komponent nie je dostupný");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✅ DataGridControl komponent je dostupný");

                // ✅ KROK 1: Definícia stĺpcov pomocou PUBLIC API
                var columns = new List<PublicColumnDefinition>
                {
                    new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                    new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                    new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                    new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                    new("Plat", typeof(decimal)) { MinWidth = 100, Width = 120, Header = "💰 Plat" },
                    new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" } // Špeciálny delete stĺpec
                };

                // ✅ KROK 2: Validačné pravidlá pomocou PUBLIC API
                var validationRules = new List<PublicValidationRule>
                {
                    PublicValidationRule.Required("Meno", "Meno je povinné"),
                    PublicValidationRule.Email("Email", "Neplatný email formát"),
                    PublicValidationRule.Range("Vek", 18, 100, "Vek musí byť 18-100"),
                    PublicValidationRule.Range("Plat", 500, 50000, "Plat musí byť 500-50000")
                };

                // ✅ KROK 3: Throttling konfigurácia pomocou PUBLIC API
                var throttlingConfig = PublicThrottlingConfig.Default;

                // ✅ KROK 4: Inicializácia komponentu s minimálne 5 riadkami pre AUTO-ADD demo
                UpdateLoadingState("Inicializuje sa DataGrid s AUTO-ADD...", "Volám InitializeAsync s minimálne 5 riadkami...");
                await Task.Delay(200);

                System.Diagnostics.Debug.WriteLine("🔧 Volám InitializeAsync s PUBLIC API (AUTO-ADD minimum 5 riadkov)...");
                await DataGridControl.InitializeAsync(columns, validationRules, throttlingConfig, 5); // ✅ AUTO-ADD: minimum 5 riadkov
                System.Diagnostics.Debug.WriteLine("✅ InitializeAsync dokončené úspešne s AUTO-ADD");

                // ✅ KROK 5: Načítanie testových dát s AUTO-ADD demonstráciou
                UpdateLoadingState("Načítavajú sa AUTO-ADD demo dáta...", "Pripravujú sa záznamy pre auto-add test...");
                await Task.Delay(200);

                // ✅ Štartovanie dáta: Načítaj 3 riadky (menej ako minimum 5) - mal by zostať na 5 + 1 prázdny = 6 celkom
                var initialData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 1, ["Meno"] = "Ján Novák", ["Email"] = "jan@example.com", ["Vek"] = 30, ["Plat"] = 2500.00m },
                    new() { ["ID"] = 2, ["Meno"] = "Mária Svoboda", ["Email"] = "maria@company.sk", ["Vek"] = 28, ["Plat"] = 3200.00m },
                    new() { ["ID"] = 3, ["Meno"] = "Peter Kováč", ["Email"] = "peter@firma.sk", ["Vek"] = 35, ["Plat"] = 4500.00m }
                };

                System.Diagnostics.Debug.WriteLine("📊 Načítavam 3 riadky štartovacích dát cez PUBLIC API (AUTO-ADD test)...");
                await DataGridControl.LoadDataAsync(initialData);
                System.Diagnostics.Debug.WriteLine("✅ AUTO-ADD test dáta načítané - mal by byť 6 riadkov celkom (5 minimum + 1 prázdny)");

                // ✅ KROK 6: Dokončenie inicializácie
                CompleteInitialization();

                System.Diagnostics.Debug.WriteLine("🎉 Demo aplikácia ÚSPEŠNE inicializovaná s AUTO-ADD funkciou!");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ KRITICKÁ CHYBA pri inicializácii: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                ShowError($"Chyba pri inicializácii: {ex.Message}");
            }
        }

        #region UI Helper metódy

        private void UpdateLoadingState(string detailText, string statusText)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingDetailText != null)
                    LoadingDetailText.Text = detailText;

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = statusText;
            });
        }

        private void CompleteInitialization()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;

                if (DataGridControl != null)
                    DataGridControl.Visibility = Visibility.Visible;

                if (InitStatusText != null)
                {
                    InitStatusText.Text = "✅ AUTO-ADD Pripravené!";
                    InitStatusText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 128, 0)); // Green
                }

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "🔥 AUTO-ADD je aktívne! Vyplň posledný riadok → automaticky sa pridá nový prázdny! 🎉";
                }
            });
        }

        private void ShowError(string errorMessage)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingDetailText != null)
                    LoadingDetailText.Text = $"❌ {errorMessage}";

                if (InitStatusText != null)
                {
                    InitStatusText.Text = "❌ Chyba";
                    InitStatusText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); // Red
                }

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba: {errorMessage}";
            });
        }

        #endregion

        #region ✅ NOVÉ: Auto-Add Demo Button Handlers

        /// <summary>
        /// Test metóda pre auto-add s malým počtom riadkov (menej ako minimum)
        /// </summary>
        private async void OnTestAutoAddFewRowsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔥 AUTO-ADD TEST: Volám TestAutoAddFewRowsAsync...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "AUTO-ADD Test: Načítavajú sa 3 riadky (menej ako minimum 5)...";

                await DataGridControl.TestAutoAddFewRowsAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🔥 AUTO-ADD: Test niekoľkých riadkov dokončený!";

                System.Diagnostics.Debug.WriteLine("✅ AUTO-ADD TEST 'few rows' dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AUTO-ADD Test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri AUTO-ADD teste: {ex.Message}";
            }
        }

        private async void OnTestAutoAddManyRowsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔥 AUTO-ADD TEST: Volám TestAutoAddManyRowsAsync...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "AUTO-ADD Test: Načítavajú sa 20 riadkov (viac ako minimum 5)...";

                await DataGridControl.TestAutoAddManyRowsAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🔥 AUTO-ADD: Test množstva riadkov dokončený → vytvorených 21 riadkov (20 + 1 prázdny)";

                System.Diagnostics.Debug.WriteLine("✅ AUTO-ADD TEST 'many rows' dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AUTO-ADD Test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri AUTO-ADD teste: {ex.Message}";
            }
        }

        private async void OnTestAutoAddDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔥 AUTO-ADD DELETE TEST: Volám TestAutoAddDeleteAsync...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "AUTO-ADD Test: Testuje sa inteligentné mazanie s ochranou minimálneho počtu...";

                await DataGridControl.TestAutoAddDeleteAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🔥 AUTO-ADD Delete: Inteligentné mazanie dokončené - minimum riadkov zachované!";

                System.Diagnostics.Debug.WriteLine("✅ AUTO-ADD DELETE TEST dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AUTO-ADD Delete test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri AUTO-ADD delete teste: {ex.Message}";
            }
        }

        #endregion

        #region ✅ OPRAVENÉ: Test Button Handlers

        private async void OnTestRealtimeValidationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("⚡ REALTIME VALIDATION TEST...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Testujú sa realtime validácie...";

                await DataGridControl.TestRealtimeValidationAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "⚡ Realtime validation test dokončený - pozri červené orámovanie nevalidných buniek!";

                System.Diagnostics.Debug.WriteLine("✅ REALTIME VALIDATION TEST dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Realtime validation test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri realtime teste: {ex.Message}";
            }
        }

        private async void OnTestNavigationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🧭 NAVIGATION TEST...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Navigácia: Použite Tab/Enter/Esc/Shift+Enter v bunkách pre testovanie navigácie";

                await DataGridControl.TestNavigationAsync();

                System.Diagnostics.Debug.WriteLine("✅ NAVIGATION TEST dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Navigation test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri navigation teste: {ex.Message}";
            }
        }

        private async void OnTestCopyPasteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📋 COPY/PASTE TEST...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Copy/Paste: Použite Ctrl+C/V/X pre testovanie copy/paste funkcionalita";

                await DataGridControl.TestCopyPasteAsync();

                System.Diagnostics.Debug.WriteLine("✅ COPY/PASTE TEST dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Copy/paste test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri copy/paste teste: {ex.Message}";
            }
        }

        #endregion

        #region Color Theme Button Handlers - PUBLIC API

        private void OnApplyLightThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Aplikujem Light Theme cez PUBLIC API...");

                DataGridControl.ApplyColorTheme(PublicDataGridColorTheme.Light);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Light theme aplikovaná cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Light theme úspešne aplikovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Light theme: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Light theme: {ex.Message}";
            }
        }

        private void OnApplyDarkThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Aplikujem Dark Theme cez PUBLIC API...");

                DataGridControl.ApplyColorTheme(PublicDataGridColorTheme.Dark);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Dark theme aplikovaná cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Dark theme úspešne aplikovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Dark theme: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Dark theme: {ex.Message}";
            }
        }

        private void OnApplyBlueThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Aplikujem Blue Theme cez PUBLIC API...");

                DataGridControl.ApplyColorTheme(PublicDataGridColorTheme.Blue);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Blue theme aplikovaná cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Blue theme úspešne aplikovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Blue theme: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Blue theme: {ex.Message}";
            }
        }

        private void OnApplyCustomThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Vytváram Custom Theme cez PUBLIC API...");

                // ✅ Custom theme pomocou PUBLIC DataGridColorThemeBuilder
                var customTheme = PublicDataGridColorThemeBuilder.Create()
                    .WithCellBackground(Color.FromArgb(255, 255, 255, 224)) // LightYellow
                    .WithCellBorder(Color.FromArgb(255, 255, 165, 0))       // Orange
                    .WithCellText(Color.FromArgb(255, 0, 0, 139))           // DarkBlue
                    .WithHeaderBackground(Color.FromArgb(255, 255, 165, 0)) // Orange
                    .WithHeaderText(Color.FromArgb(255, 255, 255, 255))     // White
                    .WithValidationError(Color.FromArgb(255, 139, 0, 0))    // DarkRed
                    .WithSelection(Color.FromArgb(100, 255, 165, 0))        // Orange alpha
                    .WithEditingCell(Color.FromArgb(50, 255, 215, 0))       // Gold alpha
                    .Build();

                DataGridControl.ApplyColorTheme(customTheme);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Custom Orange theme vytvorená a aplikovaná cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Custom theme úspešne vytvorená a aplikovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Custom theme: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Custom theme: {ex.Message}";
            }
        }

        private void OnResetThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Resetujem na default theme...");

                DataGridControl.ResetToDefaultTheme();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🔄 Reset na default Light theme cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Theme úspešne resetovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri reset theme: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri reset theme: {ex.Message}";
            }
        }

        #endregion

        #region Standard Button Event Handlers - PUBLIC API

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📊 Načítavam ukážkové dáta cez PUBLIC API s AUTO-ADD...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Načítavajú sa ukážkové dáta s AUTO-ADD...";

                // ✅ Načítaj 6 riadkov ukážkových dát
                var sampleData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 101, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 25, ["Plat"] = 3000m },
                    new() { ["ID"] = 102, ["Meno"] = "Milan Svoboda", ["Email"] = "milan@company.sk", ["Vek"] = 32, ["Plat"] = 4500m },
                    new() { ["ID"] = 103, ["Meno"] = "Eva Krásna", ["Email"] = "eva@firma.sk", ["Vek"] = 28, ["Plat"] = 3800m },
                    new() { ["ID"] = 104, ["Meno"] = "Tomáš Novák", ["Email"] = "tomas@example.sk", ["Vek"] = 35, ["Plat"] = 5200m },
                    new() { ["ID"] = 105, ["Meno"] = "Lenka Malá", ["Email"] = "lenka@test.sk", ["Vek"] = 29, ["Plat"] = 3600m },
                    new() { ["ID"] = 106, ["Meno"] = "Michal Veľký", ["Email"] = "michal@firma.sk", ["Vek"] = 31, ["Plat"] = 4100m }
                };

                await DataGridControl.LoadDataAsync(sampleData);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "✨ AUTO-ADD: 6 riadkov ukážkových dát načítané → celkom 7 riadkov (6 + 1 prázdny)";

                System.Diagnostics.Debug.WriteLine("✅ AUTO-ADD ukážkové dáta úspešne načítané");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri načítavaní: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba: {ex.Message}";
            }
        }

        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("✅ Validujem všetky dáta cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Validujú sa dáta s AUTO-ADD...";

                var isValid = await DataGridControl.ValidateAllRowsAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = isValid ? "✅ Všetky dáta sú validné (AUTO-ADD)" : "❌ Nájdené validačné chyby - pozri červené orámovanie";

                System.Diagnostics.Debug.WriteLine($"✅ Validácia dokončená s AUTO-ADD: Všetky validné = {isValid}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri validácii: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri validácii: {ex.Message}";
            }
        }

        private async void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🗑️ Vymazávam všetky dáta cez PUBLIC API s AUTO-ADD ochranou...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Vymazávajú sa dáta s AUTO-ADD ochranou...";

                await DataGridControl.ClearAllDataAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "✨ AUTO-ADD: Všetky dáta vymazané, zostalo minimum 5 prázdnych riadkov";

                System.Diagnostics.Debug.WriteLine("✅ Dáta úspešne vymazané s AUTO-ADD ochranou");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri vymazávaní: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba: {ex.Message}";
            }
        }

        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📤 Exportujem dáta cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Exportujú sa dáta z AUTO-ADD...";

                var exportedData = await DataGridControl.ExportToDataTableAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"📤 Export dokončený: {exportedData.Rows.Count} riadkov (vrátane prázdnych z AUTO-ADD)";

                System.Diagnostics.Debug.WriteLine($"✅ Export úspešný s AUTO-ADD: {exportedData.Rows.Count} riadkov");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri exporte: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri exporte: {ex.Message}";
            }
        }

        // ✅ Custom Delete Validation cez PUBLIC API
        private async void OnDeleteByCustomValidationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 Custom delete validation s AUTO-ADD ochranou cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Aplikujú sa custom delete pravidlá s AUTO-ADD ochranou...";

                // ✅ Definuj custom validačné pravidlá pre mazanie pomocou PUBLIC API
                var deleteValidationRules = new List<PublicValidationRule>
                {
                    // Zmaž riadky kde plat > 10000
                    PublicValidationRule.Custom("Plat", value =>
                    {
                        if (value == null) return false;
                        if (decimal.TryParse(value.ToString(), out var plat))
                        {
                            return plat > 10000m; // TRUE = zmaž riadok
                        }
                        return false;
                    }, "Vysoký plat - riadok zmazaný s AUTO-ADD ochranou"),

                    // Zmaž riadky kde vek > 50
                    PublicValidationRule.Custom("Vek", value =>
                    {
                        if (value == null) return false;
                        if (int.TryParse(value.ToString(), out var vek))
                        {
                            return vek > 50; // TRUE = zmaž riadok
                        }
                        return false;
                    }, "Vysoký vek - riadok zmazaný s AUTO-ADD ochranou")
                };

                // ✅ Zavolaj NOVÚ metódu cez PUBLIC API
                await DataGridControl.DeleteRowsByCustomValidationAsync(deleteValidationRules);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "✨ AUTO-ADD delete: Custom pravidlá aplikované, minimum riadkov zachované";

                System.Diagnostics.Debug.WriteLine("✅ Custom delete s AUTO-ADD ochranou úspešne dokončené");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri custom delete: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri custom delete: {ex.Message}";
            }
        }

        private async void OnAdvancedDeleteExamplesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("⚡ Pokročilé delete príklady s AUTO-ADD ochranou cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Spúšťajú sa pokročilé delete pravidlá s AUTO-ADD ochranou...";

                var advancedDeleteRules = new List<PublicValidationRule>
                {
                    // Zmaž riadky s prázdnym emailom
                    PublicValidationRule.Custom("Email", value =>
                    {
                        var email = value?.ToString() ?? "";
                        return string.IsNullOrWhiteSpace(email);
                    }, "Prázdny email - riadok zmazaný s AUTO-ADD ochranou"),

                    // Zmaž riadky kde ID je párne
                    PublicValidationRule.Custom("ID", value =>
                    {
                        if (value == null) return false;
                        if (int.TryParse(value.ToString(), out var id))
                        {
                            return id % 2 == 0; // Párne ID
                        }
                        return false;
                    }, "Párne ID - riadok zmazaný s AUTO-ADD ochranou")
                };

                await DataGridControl.DeleteRowsByCustomValidationAsync(advancedDeleteRules);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "✨ AUTO-ADD: Pokročilé delete pravidlá aplikované, minimum zachované";

                System.Diagnostics.Debug.WriteLine("✅ Pokročilé delete s AUTO-ADD ochranou úspešne dokončené");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri advanced delete: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri advanced delete: {ex.Message}";
            }
        }

        #endregion
    }
}