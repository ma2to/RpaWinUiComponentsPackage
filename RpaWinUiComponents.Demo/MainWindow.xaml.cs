// RpaWinUiComponents.Demo/MainWindow.xaml.cs - AKTUALIZOVANÉ
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

// ✅ KĽÚČOVÁ OPRAVA CS0234: Import PUBLIC API typov z PROJECT REFERENCE
using RpaWinUiComponents.AdvancedWinUiDataGrid;

// ✅ EXPLICITNÉ IMPORTY pre zamedzenie konfliktov
using PublicColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using PublicValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using PublicThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        private bool _isInitialized = false;

        public MainWindow()
        {
            this.InitializeComponent();

            // OPRAVA: Inicializácia cez DispatcherQueue na bezpečné načasovanie
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(500); // Počkáme aby sa UI úplne načítalo
                await InitializeComponentAsync();
            });
        }

        private async Task InitializeComponentAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 ŠTART inicializácie MainWindow s PROJECT REFERENCE...");

                UpdateLoadingState("Inicializuje sa komponent...", "Pripravuje sa DataGrid...");
                await Task.Delay(200);

                // KROK 1: NAJPRV inicializácia komponentu s konfiguráciou
                System.Diagnostics.Debug.WriteLine("🔧 Spúšťam inicializáciu komponentu...");

                if (DataGridControl == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ CHYBA: DataGridControl je NULL!");
                    ShowError("DataGridControl nie je dostupný");
                    return;
                }

                // KROK 2: ✅ OPRAVENÉ CS0234 - Používame PUBLIC typy z PROJECT REFERENCE
                var columns = new List<PublicColumnDefinition>
                {
                    new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                    new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                    new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                    new("Vek", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Vek" },
                    new("Plat", typeof(decimal)) { MinWidth = 100, Width = 120, Header = "💰 Plat" },
                    new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" } // Delete button stĺpec
                };

                var validationRules = new List<PublicValidationRule>
                {
                    PublicValidationRule.Required("Meno", "Meno je povinné"),
                    PublicValidationRule.Email("Email", "Neplatný email formát"),
                    PublicValidationRule.Range("Vek", 18, 100, "Vek musí byť 18-100"),
                    PublicValidationRule.Range("Plat", 500, 50000, "Plat musí byť 500-50000")
                };

                // KROK 3: ✅ OPRAVENÉ CS0234 - Používame PUBLIC typ pre throttling
                var throttlingConfig = PublicThrottlingConfig.Default;

                // KROK 4: KĽÚČOVÁ OPRAVA - InitializeAsync s PUBLIC typmi
                UpdateLoadingState("Inicializuje sa DataGrid komponent...", "Pripájajú sa služby...");
                await Task.Delay(300);

                System.Diagnostics.Debug.WriteLine("🔧 Volám InitializeAsync s PUBLIC typmi z PROJECT REFERENCE...");
                await DataGridControl.InitializeAsync(columns, validationRules, throttlingConfig, 15);
                System.Diagnostics.Debug.WriteLine("✅ InitializeAsync dokončené");

                // KROK 5: Teraz môžeme načítať dáta
                UpdateLoadingState("Načítavajú sa testové dáta...", "Pripravujú sa ukážkové záznamy...");
                await Task.Delay(200);

                var testData = new List<Dictionary<string, object?>>
                {
                    new() { ["ID"] = 1, ["Meno"] = "Ján Novák", ["Email"] = "jan@example.com", ["Vek"] = 30, ["Plat"] = 2500.00m },
                    new() { ["ID"] = 2, ["Meno"] = "Mária Svoboda", ["Email"] = "maria@company.sk", ["Vek"] = 28, ["Plat"] = 3200.00m },
                    new() { ["ID"] = 3, ["Meno"] = "Peter Kováč", ["Email"] = "peter@firma.sk", ["Vek"] = 35, ["Plat"] = 4500.00m },
                    new() { ["ID"] = 4, ["Meno"] = "", ["Email"] = "invalid-email", ["Vek"] = 15, ["Plat"] = 200.00m }, // Nevalidný
                    new() { ["ID"] = 5, ["Meno"] = "Test User", ["Email"] = "test@example.com", ["Vek"] = 150, ["Plat"] = 50000.00m }, // Nevalidný
                    new() { ["ID"] = 6, ["Meno"] = "High Salary", ["Email"] = "high@salary.com", ["Vek"] = 45, ["Plat"] = 15000.00m }, // Pre delete test
                    new() { ["ID"] = 7, ["Meno"] = "Senior Dev", ["Email"] = "senior@dev.com", ["Vek"] = 55, ["Plat"] = 12000.00m } // Pre delete test
                };

                System.Diagnostics.Debug.WriteLine("📊 Načítavam testové dáta...");
                await DataGridControl.LoadDataAsync(testData);
                System.Diagnostics.Debug.WriteLine("✅ Dáta načítané");

                // KROK 6: Dokončenie inicializácie
                CompleteInitialization();

                System.Diagnostics.Debug.WriteLine("🎉 Inicializácia ÚSPEŠNE dokončená s PROJECT REFERENCE!");

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
                    InitStatusText.Text = " - Pripravené";
                    InitStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                }

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "DataGrid pripravený a inicializovaný úspešne s PROJECT REFERENCE";
                }
            });
        }

        private void ShowError(string errorMessage)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingDetailText != null)
                    LoadingDetailText.Text = errorMessage;

                if (InitStatusText != null)
                {
                    InitStatusText.Text = " - Chyba";
                    InitStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                }

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba: {errorMessage}";
            });
        }

        #endregion

        #region Button Event Handlers - ✅ OPRAVENÉ A ROZŠÍRENÉ

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 TEST: Načítavanie ukážkových dát...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Načítavajú sa ukážkové dáta...";

                // Jednoduché testové dáta
                var sampleData = new List<Dictionary<string, object?>>
                {
                    new() { ["Meno"] = "Test Osoba", ["Email"] = "test@test.com", ["Vek"] = 25, ["Plat"] = 3000m },
                    new() { ["Meno"] = "Druhá Osoba", ["Email"] = "druha@test.com", ["Vek"] = 30, ["Plat"] = 4000m }
                };

                await DataGridControl.LoadDataAsync(sampleData);

                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = "Ukážkové dáta načítané";
                }

                System.Diagnostics.Debug.WriteLine("✅ TEST úspešný: Ukážkové dáta načítané");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TEST neúspešný: {ex.Message}");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba: {ex.Message}";
            }
        }

        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 TEST: Validácia všetkých riadkov...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Validujú sa dáta...";

                var isValid = await DataGridControl.ValidateAllRowsAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = isValid ? "Všetky dáta sú validné" : "Nájdené validačné chyby";

                System.Diagnostics.Debug.WriteLine($"✅ TEST dokončený: Všetky validné = {isValid}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TEST neúspešný: {ex.Message}");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri validácii: {ex.Message}";
            }
        }

        private async void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 TEST: Vymazávanie dát...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Vymazávajú sa dáta...";

                await DataGridControl.ClearAllDataAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Dáta vymazané";

                System.Diagnostics.Debug.WriteLine("✅ TEST úspešný: Dáta vymazané");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TEST neúspešný: {ex.Message}");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba: {ex.Message}";
            }
        }

        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 TEST: Export dát...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Exportujú sa dáta...";

                var exportedData = await DataGridControl.ExportToDataTableAsync();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Export dokončený: {exportedData.Rows.Count} riadkov";

                System.Diagnostics.Debug.WriteLine($"✅ TEST úspešný: Exportovaných {exportedData.Rows.Count} riadkov");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ TEST neúspešný: {ex.Message}");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri exporte: {ex.Message}";
            }
        }

        // ✅ NOVÁ METÓDA: Delete rows by custom validation
        private async void OnDeleteByCustomValidationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 NOVÁ FUNKCIA: Mazanie riadkov podľa custom validácie...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Aplikujú sa custom validačné pravidlá pre mazanie...";

                // Definuj custom validačné pravidlá pre mazanie
                // Ak pravidlo vráti TRUE, riadok sa ZMAŽE
                var deleteValidationRules = new List<PublicValidationRule>
                {
                    // Zmaž riadky kde plat je vyšší ako 10000
                    PublicValidationRule.Custom("Plat", value =>
                    {
                        if (value == null) return false;
                        if (decimal.TryParse(value.ToString(), out var plat))
                        {
                            return plat > 10000m; // TRUE = zmaž riadok
                        }
                        return false;
                    }, "Vysoký plat - riadok zmazaný"),

                    // Zmaž riadky kde vek je vyšší ako 50
                    PublicValidationRule.Custom("Vek", value =>
                    {
                        if (value == null) return false;
                        if (int.TryParse(value.ToString(), out var vek))
                        {
                            return vek > 50; // TRUE = zmaž riadok
                        }
                        return false;
                    }, "Vysoký vek - riadok zmazaný"),

                    // Zmaž riadky kde meno obsahuje "Test"
                    PublicValidationRule.Custom("Meno", value =>
                    {
                        if (value == null) return false;
                        var meno = value.ToString() ?? "";
                        return meno.Contains("Test", StringComparison.OrdinalIgnoreCase); // TRUE = zmaž riadok
                    }, "Test meno - riadok zmazaný")
                };

                // Zavolaj novú metódu na mazanie riadkov
                await DataGridControl.DeleteRowsByCustomValidationAsync(deleteValidationRules);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Custom validačné pravidlá aplikované - ovplyvnené riadky zmazané";

                System.Diagnostics.Debug.WriteLine("✅ NOVÁ FUNKCIA úspešná: Riadky zmazané podľa custom validácie");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ NOVÁ FUNKCIA neúspešná: {ex.Message}");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri custom validácii: {ex.Message}";
            }
        }

        // ✅ DEMO METÓDA: Ukážka rôznych custom validácií pre mazanie
        private async void OnAdvancedDeleteExamplesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 DEMO: Pokročilé príklady custom validácie pre mazanie...");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Spúšťajú sa pokročilé delete pravidlá...";

                // Príklad zložitejších custom validácií
                var advancedDeleteRules = new List<PublicValidationRule>
                {
                    // Zmaž riadky s prázdnym emailom ALE len ak majú vyplnené meno
                    PublicValidationRule.Custom("Email", value =>
                    {
                        var email = value?.ToString() ?? "";
                        return string.IsNullOrWhiteSpace(email); // TRUE ak je email prázdny
                    }, "Prázdny email - riadok zmazaný"),

                    // Zmaž riadky kde email nie je validný (neobsahuje @)
                    PublicValidationRule.Custom("Email", value =>
                    {
                        var email = value?.ToString() ?? "";
                        return !string.IsNullOrWhiteSpace(email) && !email.Contains("@"); // TRUE ak email neobsahuje @
                    }, "Nevalidný email - riadok zmazaný"),

                    // Zmaž riadky kde ID je párne
                    PublicValidationRule.Custom("ID", value =>
                    {
                        if (value == null) return false;
                        if (int.TryParse(value.ToString(), out var id))
                        {
                            return id % 2 == 0; // TRUE ak je ID párne
                        }
                        return false;
                    }, "Párne ID - riadok zmazaný")
                };

                await DataGridControl.DeleteRowsByCustomValidationAsync(advancedDeleteRules);

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Pokročilé delete pravidlá aplikované";

                System.Diagnostics.Debug.WriteLine("✅ DEMO úspešné: Pokročilé delete pravidlá aplikované");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DEMO neúspešné: {ex.Message}");

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Chyba pri pokročilých pravidlách: {ex.Message}";
            }
        }

        #endregion
    }
}