// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ KOMPLETNE OPRAVENÝ - PUBLIC API only
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
                System.Diagnostics.Debug.WriteLine("🚀 ŠTART inicializácie Demo aplikácie s PUBLIC API balíka...");

                UpdateLoadingState("Inicializuje sa balík...", "Načítava sa z Package Reference...");
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

                // ✅ KROK 4: Inicializácia komponentu
                UpdateLoadingState("Inicializuje sa DataGrid...", "Volám InitializeAsync...");
                await Task.Delay(200);

                System.Diagnostics.Debug.WriteLine("🔧 Volám InitializeAsync s PUBLIC API...");
                await DataGridControl.InitializeAsync(columns, validationRules, throttlingConfig, 15);
                System.Diagnostics.Debug.WriteLine("✅ InitializeAsync dokončené úspešne");

                // ✅ KROK 5: Načítanie testových dát
                UpdateLoadingState("Načítavajú sa testové dáta...", "Pripravujú sa ukážkové záznamy...");
                await Task.Delay(200);

                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 1, ["Meno"] = "Ján Novák", ["Email"] = "jan@example.com", ["Vek"] = 30, ["Plat"] = 2500.00m },
                    new() { ["ID"] = 2, ["Meno"] = "Mária Svoboda", ["Email"] = "maria@company.sk", ["Vek"] = 28, ["Plat"] = 3200.00m },
                    new() { ["ID"] = 3, ["Meno"] = "Peter Kováč", ["Email"] = "peter@firma.sk", ["Vek"] = 35, ["Plat"] = 4500.00m },
                    new() { ["ID"] = 4, ["Meno"] = "", ["Email"] = "invalid-email", ["Vek"] = 15, ["Plat"] = 200.00m }, // Nevalidný pre test
                    new() { ["ID"] = 5, ["Meno"] = "Test User", ["Email"] = "test@example.com", ["Vek"] = 150, ["Plat"] = 50000.00m }, // Nevalidný pre test
                    new() { ["ID"] = 6, ["Meno"] = "High Salary", ["Email"] = "high@salary.com", ["Vek"] = 45, ["Plat"] = 15000.00m }, // Pre delete test
                    new() { ["ID"] = 7, ["Meno"] = "Senior Dev", ["Email"] = "senior@dev.com", ["Vek"] = 55, ["Plat"] = 12000.00m } // Pre delete test
                };

                System.Diagnostics.Debug.WriteLine("📊 Načítavam testové dáta cez PUBLIC API...");
                await DataGridControl.LoadDataAsync(testData);
                System.Diagnostics.Debug.WriteLine("✅ Testové dáta načítané úspešne");

                // ✅ KROK 6: Dokončenie inicializácie
                CompleteInitialization();

                System.Diagnostics.Debug.WriteLine("🎉 Demo aplikácia ÚSPEŠNE inicializovaná s Package Reference!");

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
                    InitStatusText.Text = "✅ Pripravené";
                    InitStatusText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 128, 0)); // Green
                }

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "DataGrid úspešne načítaný z Package Reference v1.0.2";
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

        #region Color Theme Button Handlers - PUBLIC API

        private void OnApplyLightThemeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Aplikujem Light Theme cez PUBLIC API...");

                DataGridControl.ApplyColorTheme(PublicDataGridColorTheme.Light);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Light theme aplikovaná cez PUBLIC API";

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
                    StatusTextBlock.Text = "Dark theme aplikovaná cez PUBLIC API";

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
                    StatusTextBlock.Text = "Blue theme aplikovaná cez PUBLIC API";

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
                    StatusTextBlock.Text = "Custom Orange theme vytvorená a aplikovaná cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Custom theme úspešne vytvorená a aplikovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri Custom theme: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri Custom theme: {ex.Message}";
            }
        }

        #endregion

        #region Button Event Handlers - PUBLIC API

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📊 Načítavam ukážkové dáta cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Načítavajú sa ukážkové dáta...";

                var sampleData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 101, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk", ["Vek"] = 25, ["Plat"] = 3000m },
                    new() { ["ID"] = 102, ["Meno"] = "Milan Svoboda", ["Email"] = "milan@company.sk", ["Vek"] = 32, ["Plat"] = 4500m },
                    new() { ["ID"] = 103, ["Meno"] = "Eva Krásna", ["Email"] = "eva@firma.sk", ["Vek"] = 28, ["Plat"] = 3800m }
                };

                await DataGridControl.LoadDataAsync(sampleData);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Ukážkové dáta načítané cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Ukážkové dáta úspešne načítané");
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
                    StatusTextBlock.Text = "Validujú sa dáta...";

                var isValid = await DataGridControl.ValidateAllRowsAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = isValid ? "Všetky dáta sú validné" : "Nájdené validačné chyby";

                System.Diagnostics.Debug.WriteLine($"✅ Validácia dokončená: Všetky validné = {isValid}");
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
                System.Diagnostics.Debug.WriteLine("🗑️ Vymazávam všetky dáta cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Vymazávajú sa dáta...";

                await DataGridControl.ClearAllDataAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Všetky dáta vymazané cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ Dáta úspešne vymazané");
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
                    StatusTextBlock.Text = "Exportujú sa dáta...";

                var exportedData = await DataGridControl.ExportToDataTableAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Export dokončený: {exportedData.Rows.Count} riadkov cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine($"✅ Export úspešný: {exportedData.Rows.Count} riadkov");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri exporte: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri exporte: {ex.Message}";
            }
        }

        // ✅ NOVÁ FUNKCIONALITA: Custom Delete Validation cez PUBLIC API
        private async void OnDeleteByCustomValidationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎯 NOVÁ FUNKCIA: Custom delete validation cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Aplikujú sa custom delete pravidlá...";

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
                    }, "Vysoký plat - riadok zmazaný"),

                    // Zmaž riadky kde vek > 50
                    PublicValidationRule.Custom("Vek", value =>
                    {
                        if (value == null) return false;
                        if (int.TryParse(value.ToString(), out var vek))
                        {
                            return vek > 50; // TRUE = zmaž riadok
                        }
                        return false;
                    }, "Vysoký vek - riadok zmazaný")
                };

                // ✅ Zavolaj NOVÚ metódu cez PUBLIC API
                await DataGridControl.DeleteRowsByCustomValidationAsync(deleteValidationRules);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Custom delete pravidlá aplikované cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ NOVÁ FUNKCIA úspešne dokončená");
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
                System.Diagnostics.Debug.WriteLine("⚡ DEMO: Pokročilé delete príklady cez PUBLIC API...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Spúšťajú sa pokročilé delete pravidlá...";

                var advancedDeleteRules = new List<PublicValidationRule>
                {
                    // Zmaž riadky s prázdnym emailom
                    PublicValidationRule.Custom("Email", value =>
                    {
                        var email = value?.ToString() ?? "";
                        return string.IsNullOrWhiteSpace(email);
                    }, "Prázdny email - riadok zmazaný"),

                    // Zmaž riadky kde ID je párne
                    PublicValidationRule.Custom("ID", value =>
                    {
                        if (value == null) return false;
                        if (int.TryParse(value.ToString(), out var id))
                        {
                            return id % 2 == 0; // Párne ID
                        }
                        return false;
                    }, "Párne ID - riadok zmazaný")
                };

                await DataGridControl.DeleteRowsByCustomValidationAsync(advancedDeleteRules);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Pokročilé delete pravidlá aplikované cez PUBLIC API";

                System.Diagnostics.Debug.WriteLine("✅ DEMO úspešne dokončené");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri advanced delete: {ex.Message}");
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri advanced delete: {ex.Message}";
            }
        }

        #endregion

        #region Testing Button Handlers

        private async void OnTestRealtimeValidationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("⚡ TEST: Realtime validácie...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "💡 TIP: Začni písať do buniek - validácie sa spúšťajú realtime!";

                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["Meno"] = "", ["Email"] = "invalid", ["Vek"] = 15, ["Plat"] = 100.00m }, // Nevalidné
                    new() { ["Meno"] = "Test", ["Email"] = "valid@email.com", ["Vek"] = 25, ["Plat"] = 3000.00m } // Validné
                };

                await DataGridControl.LoadDataAsync(testData);

                System.Diagnostics.Debug.WriteLine("✅ Realtime validation test dáta načítané");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Test failed: {ex.Message}");
            }
        }

        private void OnTestNavigationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🧭 TEST: Navigation...");

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "💡 TIP: Skús Tab, Enter, Esc, Shift+Enter v bunkách";
                }

                System.Diagnostics.Debug.WriteLine("✅ Navigation test ready");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Navigation test failed: {ex.Message}");
            }
        }

        private void OnTestCopyPasteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📋 TEST: Copy/Paste...");

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "💡 TIP: Skús Ctrl+C/Ctrl+V/Ctrl+X pre copy/paste";
                }

                System.Diagnostics.Debug.WriteLine("✅ Copy/Paste test ready");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Copy/Paste test failed: {ex.Message}");
            }
        }

        #endregion
    }
}