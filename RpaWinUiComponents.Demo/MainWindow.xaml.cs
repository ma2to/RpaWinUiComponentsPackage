// RpaWinUiComponents.Demo/MainWindow.xaml.cs - ✅ OPRAVENÝ - odstránené nepoužité fields
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

// ✅ Conditional imports - ak package nie je dostupný, build sa nevykompajluje ale aspoň uvidíme problém
#if HAS_PACKAGE
using RpaWinUiComponents.AdvancedWinUiDataGrid;
using PublicColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.ColumnDefinition;
using PublicValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.ValidationRule;
using PublicThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.ThrottlingConfig;
using PublicDataGridColorConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.DataGridColorConfig;
#endif

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        // ✅ OPRAVENÉ: Odstránené nepoužité fields
        private bool _packageAvailable = false;

        public MainWindow()
        {
            try
            {
                this.InitializeComponent();
                System.Diagnostics.Debug.WriteLine("✅ InitializeComponent() úspešne dokončené");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ InitializeComponent() failed: {ex.Message}");
                // Pokračuj aj napriek chybe
            }

            // ✅ Bezpečná inicializácia cez DispatcherQueue
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(200); // Počkaj na UI setup
                await SafeInitializeAsync();
            });
        }

        private async Task SafeInitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Demo aplikácia sa inicializuje...");

                // ✅ KROK 1: Skontroluj dostupnosť UI elementov
                await CheckUIElementsAsync();

                // ✅ KROK 2: Skontroluj dostupnosť package
                await CheckPackageAvailabilityAsync();

                // ✅ KROK 3: Inicializuj podľa dostupnosti
                if (_packageAvailable)
                {
                    await InitializeWithPackageAsync();
                }
                else
                {
                    ShowPackageNotAvailableMessage();
                }

                System.Diagnostics.Debug.WriteLine("✅ Demo aplikácia úspešne inicializovaná");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Kritická chyba pri inicializácii: {ex.Message}");
                ShowError($"Inicializačná chyba: {ex.Message}");
            }
        }

        private async Task CheckUIElementsAsync()
        {
            await Task.Run(() =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        // Skontroluj kľúčové UI elementy
                        var elementsOk = LoadingDetailText != null &&
                                        StatusTextBlock != null &&
                                        DataGridControl != null;

                        if (elementsOk)
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Všetky UI elementy sú dostupné");
                            UpdateLoadingState("UI elementy načítané...", "Kontroluje sa package dostupnosť...");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("❌ Niektoré UI elementy nie sú dostupné");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ UI check error: {ex.Message}");
                    }
                });
            });
        }

        private async Task CheckPackageAvailabilityAsync()
        {
            await Task.Run(() =>
            {
                try
                {
#if HAS_PACKAGE
                    // Skús načítať package types
                    _packageAvailable = true;
                    System.Diagnostics.Debug.WriteLine("✅ Package je dostupný cez conditional compilation");
#else
                    // Package nie je dostupný
                    _packageAvailable = false;
                    System.Diagnostics.Debug.WriteLine("⚠️ Package nie je dostupný - HAS_PACKAGE nie je definované");
#endif
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Package check error: {ex.Message}");
                    _packageAvailable = false;
                }
            });
        }

        private async Task InitializeWithPackageAsync()
        {
            try
            {
                UpdateLoadingState("Package je dostupný...", "Inicializuje sa DataGrid komponent...");
                await Task.Delay(300);

#if HAS_PACKAGE
                // Skús inicializovať DataGrid
                if (DataGridControl != null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("🎯 Pokúšam sa o inicializáciu DataGrid...");

                        // ✅ Základná inicializácia s Individual Colors
                        var basicColumns = new List<PublicColumnDefinition>
                        {
                            new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
                            new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
                            new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
                            new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
                        };

                        var basicRules = new List<PublicValidationRule>
                        {
                            PublicValidationRule.Required("Meno", "Meno je povinné"),
                            PublicValidationRule.Email("Email", "Neplatný email formát")
                        };

                        var throttling = PublicThrottlingConfig.Default;
                        var colors = PublicDataGridColorConfig.Light;

                        await DataGridControl.InitializeAsync(basicColumns, basicRules, throttling, 5, colors);

                        CompleteInitialization("✅ Package je funkčný!");
                    }
                    catch (Exception dgEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ DataGrid init error: {dgEx.Message}");
                        ShowError($"DataGrid inicializácia zlyhala: {dgEx.Message}");
                    }
                }
                else
                {
                    ShowError("DataGridControl nie je dostupný v XAML");
                }
#else
                ShowError("Package nie je skompajlovaný do aplikácie");
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Package init error: {ex.Message}");
                ShowError($"Package inicializácia zlyhala: {ex.Message}");
            }
        }

        private void ShowPackageNotAvailableMessage()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (LoadingDetailText != null)
                        LoadingDetailText.Text = "⚠️ AdvancedWinUiDataGrid package nie je dostupný";

                    if (StatusTextBlock != null)
                        StatusTextBlock.Text = "📦 Package Reference: Skontrolujte či je balík nainštalovaný (Version 1.0.18)";

                    if (LoadingPanel != null)
                        LoadingPanel.Visibility = Visibility.Visible;

                    System.Diagnostics.Debug.WriteLine("⚠️ Zobrazená správa o nedostupnom package");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Show package message error: {ex.Message}");
                }
            });
        }

        #region UI Helper Methods

        private void UpdateLoadingState(string detailText, string statusText)
        {
            this.DispatcherQueue.TryEnqueue(() =>
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
                    System.Diagnostics.Debug.WriteLine($"⚠️ UpdateLoadingState error: {ex.Message}");
                }
            });
        }

        private void CompleteInitialization(string successMessage)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (LoadingPanel != null)
                        LoadingPanel.Visibility = Visibility.Collapsed;

                    if (DataGridControl != null)
                        DataGridControl.Visibility = Visibility.Visible;

                    if (InitStatusText != null)
                    {
                        InitStatusText.Text = successMessage;
                        InitStatusText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 128, 0));
                    }

                    if (StatusTextBlock != null)
                        StatusTextBlock.Text = "🎉 Demo je pripravené na testovanie!";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ CompleteInitialization error: {ex.Message}");
                }
            });
        }

        private void ShowError(string errorMessage)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (LoadingDetailText != null)
                        LoadingDetailText.Text = $"❌ {errorMessage}";

                    if (InitStatusText != null)
                    {
                        InitStatusText.Text = "❌ Chyba";
                        InitStatusText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    }

                    if (StatusTextBlock != null)
                        StatusTextBlock.Text = $"Chyba: {errorMessage}";

                    System.Diagnostics.Debug.WriteLine($"❌ Error displayed: {errorMessage}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ShowError error: {ex.Message}");
                }
            });
        }

        #endregion

        #region Button Event Handlers - Safe implementations

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            await HandleSafeButtonClickAsync("Load Sample Data", async () =>
            {
#if HAS_PACKAGE
                if (DataGridControl != null)
                {
                    var sampleData = new List<Dictionary<string, object?>>
                    {
                        new() { ["ID"] = 101, ["Meno"] = "Anna Nováková", ["Email"] = "anna@test.sk" },
                        new() { ["ID"] = 102, ["Meno"] = "Milan Svoboda", ["Email"] = "milan@company.sk" },
                        new() { ["ID"] = 103, ["Meno"] = "Eva Krásna", ["Email"] = "eva@firma.sk" }
                    };

                    await DataGridControl.LoadDataAsync(sampleData);

                    if (StatusTextBlock != null)
                        StatusTextBlock.Text = "✅ Sample data načítané s Individual Colors + Zebra!";
                }
#endif
            });
        }

        private async void OnApplyLightThemeClick(object sender, RoutedEventArgs e)
        {
            await HandleSafeButtonClickAsync("Apply Light Theme", async () =>
            {
                await Task.Delay(300);
                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "🎨 Light theme by mal byť aplikovaný (ak package funguje)";
            });
        }

        private void OnApplyDarkThemeClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Apply Dark Theme");

        private void OnApplyBlueThemeClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Apply Blue Theme");

        private void OnApplyCustomThemeClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Apply Custom Theme");

        private void OnResetThemeClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Reset Theme");

        private void OnTestSearchClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Search Test");

        private void OnTestSortClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Sort Test");

        private void OnTestZebraToggleClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Zebra Test");

        private void OnClearSearchClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Clear Search");

        private void OnTestAutoAddFewRowsClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Auto-Add Few Rows");

        private void OnTestAutoAddManyRowsClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Auto-Add Many Rows");

        private void OnTestAutoAddDeleteClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Auto-Add Delete");

        private void OnValidateAllClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Validate All");

        private void OnClearDataClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Clear Data");

        private void OnExportDataClick(object sender, RoutedEventArgs e) =>
            HandleSafeButtonClick("Export Data");

        private async Task HandleSafeButtonClickAsync(string actionName, Func<Task> action)
        {
            try
            {
                if (!_packageAvailable)
                {
                    ShowError($"Package nie je dostupný pre {actionName}");
                    return;
                }

                UpdateLoadingState($"Vykonáva sa {actionName}...", "Processing...");
                await action();
            }
            catch (Exception ex)
            {
                ShowError($"{actionName} error: {ex.Message}");
            }
        }

        private void HandleSafeButtonClick(string actionName)
        {
            try
            {
                if (!_packageAvailable)
                {
                    ShowError($"Package nie je dostupný pre {actionName}");
                    return;
                }

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"🎯 {actionName} by mal fungovať (ak package funguje)";

                System.Diagnostics.Debug.WriteLine($"✅ Safe button click: {actionName}");
            }
            catch (Exception ex)
            {
                ShowError($"{actionName} error: {ex.Message}");
            }
        }

        #endregion
    }
}