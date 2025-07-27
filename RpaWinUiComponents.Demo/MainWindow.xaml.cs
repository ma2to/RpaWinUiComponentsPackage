// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ OPRAVENÝ pre DataGridColorConfig API + Search/Sort
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
using PublicDataGridColorConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.DataGridColorConfig;
using PublicSortDirection = RpaWinUiComponents.AdvancedWinUiDataGrid.SortDirection;

// ✅ Windows.UI.Color pre farby
using Windows.UI;

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        private bool _isInitialized = false;

        // ✅ NOVÉ: Store pre základnú konfiguráciu (pre reinicializáciu s inými farbami)
        private List<PublicColumnDefinition> _baseColumns = new();
        private List<PublicValidationRule> _baseValidationRules = new();
        private PublicThrottlingConfig _baseThrottlingConfig = PublicThrottlingConfig.Default;
        private int _baseRowCount = 5;

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
                System.Diagnostics.Debug.WriteLine("🚀 ŠTART inicializácie Demo aplikácie s AUTO-ADD, DataGridColorConfig a Search/Sort...");

                UpdateLoadingState("Inicializuje sa balík v1.0.16...", "Načítava sa z Package Reference s AUTO-ADD, DataGridColorConfig a Search/Sort...");
                await Task.Delay(300);

                // ✅ OVERENIE dostupnosti komponentu
                if (DataGridControl == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ KRITICKÁ CHYBA: DataGridControl je NULL!");
                    ShowError("DataGridControl komponent nie je dostupný");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✅ DataGridControl komponent je dostupný");

                // ✅ KROK 1: Definícia základnej konfigurácie (store pre reinicializáciu)
                _baseColumns = new List<PublicColumnDefinition>
                {
                    new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                    new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                    new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                    new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                    new("Plat", typeof(decimal)) { MinWidth = 100, Width = 120, Header = "💰 Plat" },
                    new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
                };

                _baseValidationRules = new List<PublicValidationRule>
                {
                    PublicValidationRule.Required("Meno", "Meno je povinné"),
                    PublicValidationRule.Email("Email", "Neplatný email formát"),
                    PublicValidationRule.Range("Vek", 18, 100, "Vek musí byť 18-100"),
                    PublicValidationRule.Range("Plat", 500, 50000, "Plat musí byť 500-50000")
                };

                _baseThrottlingConfig = PublicThrottlingConfig.Default;
                _baseRowCount = 5;

                // ✅ KROK 2: Inicializácia s default colors
                await InitializeDataGridWithColorConfig(null, "default colors");

                // ✅ KROK 3: Načítanie testových dát s AUTO-ADD demonstráciou
                UpdateLoadingState("Načítavajú sa AUTO-ADD demo dáta...", "Pripravujú sa záznamy pre auto-add test...");
                await Task.Delay(200);

                var initialData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 1, ["Meno"] = "Ján Novák", ["Email"] = "jan@example.com", ["Vek"] = 30, ["Plat"] = 2500.00m },
                    new() { ["ID"] = 2, ["Meno"] = "Mária Svoboda", ["Email"] = "maria@company.sk", ["Vek"] = 28, ["Plat"] = 3200.00m },
                    new() { ["ID"] = 3, ["Meno"] = "Peter Kováč", ["Email"] = "peter@firma.sk", ["Vek"] = 35, ["Plat"] = 4500.00m }
                };

                System.Diagnostics.Debug.WriteLine("📊 Načítavam 3 riadky štartovacích dát cez PUBLIC API (AUTO-ADD test)...");
                await DataGridControl.LoadDataAsync(initialData);
                System.Diagnostics.Debug.WriteLine("✅ AUTO-ADD test dáta načítané - mal by byť 6 riadkov celkom (5 minimum + 1 prázdny)");

                // ✅ KROK 4: Dokončenie inicializácie
                CompleteInitialization();

                System.Diagnostics.Debug.WriteLine("🎉 Demo aplikácia ÚSPEŠNE inicializovaná s AUTO-ADD, DataGridColorConfig a Search/Sort!");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ KRITICKÁ CHYBA pri inicializácii: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                ShowError($"Chyba pri inicializácii: {ex.Message}");
            }
        }

        #region ✅ OPRAVENÉ: DataGridColorConfig Helper Metódy

        /// <summary>
        /// Inicializuje DataGrid s určitými farbami
        /// </summary>
        private async Task InitializeDataGridWithColorConfig(PublicDataGridColorConfig? colorConfig, string colorDescription)
        {
            try
            {
                UpdateLoadingState($"Inicializuje sa DataGrid s {colorDescription}...", "Volám InitializeAsync s DataGridColorConfig...");
                await Task.Delay(200);

                System.Diagnostics.Debug.WriteLine($"🔧 Volám InitializeAsync s {colorDescription}...");
                await DataGridControl.InitializeAsync(_baseColumns, _baseValidationRules, _baseThrottlingConfig, _baseRowCount, colorConfig);
                System.Diagnostics.Debug.WriteLine($"✅ InitializeAsync dokončené s {colorDescription}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri InitializeAsync s {colorDescription}: {ex.Message}");
                throw;
            }
        }

        #endregion

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
                    InitStatusText.Text = "✅ AUTO-ADD + DataGridColorConfig + Search/Sort Pripravené!";
                    InitStatusText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 128, 0)); // Green
                }

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "🔥 AUTO-ADD je aktívne! DataGridColorConfig nastavené! Search/Sort ready! Vyplň posledný riadok → automaticky sa pridá nový! 🎉";
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

        #region ✅ OPRAVENÉ: DataGridColorConfig Button Handlers (cez reinicializáciu)

        private async void OnApplyLightThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Reinicializujem s Light DataGridColorConfig...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Reinicializuje sa s Light DataGridColorConfig...";

                // ✅ Light colors pomocou DataGridColorConfig.Light
                var lightConfig = PublicDataGridColorConfig.Light;
                await InitializeDataGridWithColorConfig(lightConfig, "Light DataGridColorConfig");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Light DataGridColorConfig aplikované cez reinicializáciu";

                System.Diagnostics.Debug.WriteLine("✅ Light DataGridColorConfig úspešne aplikované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Light colors: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Light colors: {ex.Message}";
            }
        }

        private async void OnApplyDarkThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Reinicializujem s Dark DataGridColorConfig...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Reinicializuje sa s Dark DataGridColorConfig...";

                // ✅ Dark colors pomocou DataGridColorConfig.Dark
                var darkConfig = PublicDataGridColorConfig.Dark;
                await InitializeDataGridWithColorConfig(darkConfig, "Dark DataGridColorConfig");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Dark DataGridColorConfig aplikované cez reinicializáciu";

                System.Diagnostics.Debug.WriteLine("✅ Dark DataGridColorConfig úspešne aplikované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Dark colors: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Dark colors: {ex.Message}";
            }
        }

        private async void OnApplyBlueThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Reinicializujem s Blue DataGridColorConfig...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Reinicializuje sa s Blue DataGridColorConfig...";

                // ✅ Blue colors pomocou DataGridColorConfig.Blue
                var blueConfig = PublicDataGridColorConfig.Blue;
                await InitializeDataGridWithColorConfig(blueConfig, "Blue DataGridColorConfig");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Blue DataGridColorConfig aplikované cez reinicializáciu";

                System.Diagnostics.Debug.WriteLine("✅ Blue DataGridColorConfig úspešne aplikované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Blue colors: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Blue colors: {ex.Message}";
            }
        }

        private async void OnApplyCustomThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Reinicializujem s Custom DataGridColorConfig...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Reinicializuje sa s Custom DataGridColorConfig...";

                // ✅ Custom colors pomocou DataGridColorConfig (vytvor custom inštanciu)
                var customConfig = new PublicDataGridColorConfig
                {
                    CellBackgroundColor = Color.FromArgb(255, 255, 255, 224), // LightYellow
                    CellBorderColor = Color.FromArgb(255, 255, 165, 0),       // Orange
                    CellTextColor = Color.FromArgb(255, 0, 0, 139),           // DarkBlue
                    HeaderBackgroundColor = Color.FromArgb(255, 255, 165, 0), // Orange
                    HeaderTextColor = Color.FromArgb(255, 255, 255, 255),     // White
                    ValidationErrorColor = Color.FromArgb(255, 139, 0, 0),    // DarkRed
                    SelectionColor = Color.FromArgb(100, 255, 165, 0),        // Orange alpha
                    EditingCellColor = Color.FromArgb(50, 255, 215, 0),       // Gold alpha
                    AlternateRowColor = Color.FromArgb(30, 255, 200, 0)       // ✅ NOVÉ: Zebra effect
                };

                await InitializeDataGridWithColorConfig(customConfig, "Custom Orange DataGridColorConfig s Zebra");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Custom Orange DataGridColorConfig s Zebra aplikované!";

                System.Diagnostics.Debug.WriteLine("✅ Custom DataGridColorConfig s Zebra úspešne aplikované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Custom colors: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Custom colors: {ex.Message}";
            }
        }

        private async void OnResetThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Resetujem na default DataGridColorConfig...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Resetuje sa na default DataGridColorConfig...";

                // ✅ Reset na default (null = použije default colors)
                await InitializeDataGridWithColorConfig(null, "default DataGridColorConfig");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🔄 Reset na default DataGridColorConfig cez reinicializáciu";

                System.Diagnostics.Debug.WriteLine("✅ DataGridColorConfig úspešne resetované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri reset colors: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri reset colors: {ex.Message}";
            }
        }

        #endregion

        #region ✅ NOVÉ: Search & Sort Button Handlers

        private async void OnTestSearchClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 SEARCH TEST: Testuje sa search funkcionalita...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🔍 Search Test: Nastavujú sa filtre...";

                // Demo search filters
                DataGridControl.SetColumnSearchFilter("Meno", "Test");
                DataGridControl.SetColumnSearchFilter("Email", "@test");

                var hasActiveFilters = DataGridControl.HasActiveSearchFilters;
                var searchStatus = DataGridControl.GetSearchSortStatus();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"🔍 Search filtre nastavené! Active: {hasActiveFilters}, Status: {searchStatus}";

                System.Diagnostics.Debug.WriteLine($"✅ Search Test dokončený - Active filters: {hasActiveFilters}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Search test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri search teste: {ex.Message}";
            }
        }

        private async void OnTestSortClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📊 SORT TEST: Testuje sa sort funkcionalita...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "📊 Sort Test: Toggleuje sa sort na 'Meno'...";

                // Demo sort operations
                var direction1 = DataGridControl.ToggleColumnSort("Meno"); // None → Ascending
                await Task.Delay(500);

                var direction2 = DataGridControl.ToggleColumnSort("Meno"); // Ascending → Descending
                await Task.Delay(500);

                var direction3 = DataGridControl.ToggleColumnSort("Meno"); // Descending → None

                var hasActiveSort = DataGridControl.HasActiveSort;
                var sortStatus = DataGridControl.GetSearchSortStatus();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"📊 Sort Test: {direction1} → {direction2} → {direction3}, Active: {hasActiveSort}";

                System.Diagnostics.Debug.WriteLine($"✅ Sort Test dokončený - Directions: {direction1} → {direction2} → {direction3}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Sort test failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri sort teste: {ex.Message}";
            }
        }

        private async void OnClearSearchSortClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🧹 CLEAR SEARCH/SORT: Čistia sa všetky filtre a sort...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🧹 Čistia sa search filtre a sort stavy...";

                DataGridControl.ClearAllSearchFilters();
                DataGridControl.ClearAllSorts();

                var hasActiveFilters = DataGridControl.HasActiveSearchFilters;
                var hasActiveSort = DataGridControl.HasActiveSort;
                var status = DataGridControl.GetSearchSortStatus();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"🧹 Všetko vyčistené! Filters: {hasActiveFilters}, Sort: {hasActiveSort}, Status: {status}";

                System.Diagnostics.Debug.WriteLine("✅ Search/Sort vyčistené");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Clear search/sort failed: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri clear search/sort: {ex.Message}";
            }
        }

        #endregion

        #region ✅ AUTO-ADD Demo Button Handlers

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

        #region ✅ Test Button Handlers

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

        #region Standard Button Event Handlers - PUBLIC API

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📊 Načítavam ukážkové dáta cez PUBLIC API s AUTO-ADD...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Načítavajú sa ukážkové dáta s AUTO-ADD...";

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

        private async void OnDeleteByCustomValidationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 Custom delete validation s AUTO-ADD ochranou cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Aplikujú sa custom delete pravidlá s AUTO-ADD ochranou...";

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