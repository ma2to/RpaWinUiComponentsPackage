﻿🚀 RpaWinUiComponents.AdvancedWinUiDataGrid
Profesionálny WinUI3 DataGrid komponent s pokročilými funkciami pre dátové aplikácie

🎯 Účel balíka
Tento balík poskytuje vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie s podporou:

✅ Dynamického generovania stĺpcov
✅ Realtime validácií s throttling
✅ Copy/Paste Excel funkcionalite
✅ Custom validačných pravidiel pre mazanie riadkov ⭐ NOVÉ
✅ Color Theme API s predpripravenými témami ⭐ NOVÉ
✅ Auto-Add riadkov funkcionalita ⭐ NAJNOVŠIE
✅ Optimalizácie pre veľké datasety
✅ Profesionálneho clean PUBLIC API


📦 Inštalácia
Package Manager Console
bashInstall-Package RpaWinUiComponents.AdvancedWinUiDataGrid
.NET CLI
bashdotnet add package RpaWinUiComponents.AdvancedWinUiDataGrid
PackageReference
xml<PackageReference Include="RpaWinUiComponents.AdvancedWinUiDataGrid" Version="1.0.2" />

🚀 Quick Start
1. Základné použitie v XAML
xml<!-- MainWindow.xaml -->
<Window x:Class="YourApp.MainWindow"
        xmlns:grid="using:RpaWinUiComponents.AdvancedWinUiDataGrid">
    
    <Grid>
        <grid:AdvancedDataGrid x:Name="DataGridControl"/>
    </Grid>
</Window>
2. Inicializácia v C#
csharp// MainWindow.xaml.cs
using RpaWinUiComponents.AdvancedWinUiDataGrid;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        InitializeDataGrid();
    }

    private async void InitializeDataGrid()
    {
        // 1. Definícia stĺpcov
        var columns = new List<ColumnDefinition>
        {
            new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
            new("Name", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Name" },
            new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
            new("DeleteRows", typeof(string)) { Width = 40 } // Špeciálny delete stĺpec
        };

        // 2. Validačné pravidlá
        var validationRules = new List<ValidationRule>
        {
            ValidationRule.Required("Name", "Name is required"),
            ValidationRule.Email("Email", "Invalid email format")
        };

        // 3. Inicializácia
        await DataGridControl.InitializeAsync(columns, validationRules, ThrottlingConfig.Default, 15);

        // 4. Načítanie dát
        var data = new List<Dictionary<string, object?>>
        {
            new() { ["ID"] = 1, ["Name"] = "John Doe", ["Email"] = "john@example.com" }
        };
        await DataGridControl.LoadDataAsync(data);
    }
}

⭐ NAJNOVŠIE FUNKCIE
🎨 Color Theme API ⭐ NOVÉ
Predpripravené témy a možnosť vytvorenia vlastných:
csharp// Predpripravené témy
DataGridControl.ApplyColorTheme(DataGridColorTheme.Light);    // Svetlá
DataGridControl.ApplyColorTheme(DataGridColorTheme.Dark);     // Tmavá  
DataGridControl.ApplyColorTheme(DataGridColorTheme.Blue);     // Modrá
DataGridControl.ApplyColorTheme(DataGridColorTheme.Green);    // Zelená

// Custom téma pomocou Builder pattern
var customTheme = DataGridColorThemeBuilder.Create()
    .WithCellBackground(Colors.LightYellow)
    .WithCellBorder(Colors.Orange)
    .WithHeaderBackground(Colors.Orange)
    .WithValidationError(Colors.DarkRed)
    .Build();

DataGridControl.ApplyColorTheme(customTheme);

// Reset na default
DataGridControl.ResetToDefaultTheme();
⚡ Auto-Add Riadkov ⭐ NAJNOVŠIE
Automatické pridávanie riadkov pri práci s dátami:
csharp// Ak načítaš 12 riadkov dát, automaticky sa vytvorí 13. prázdny riadok
var data = GenerateData(12); // 12 riadkov
await DataGridControl.LoadDataAsync(data);
// Výsledok: 13 riadkov (12 s dátami + 1 prázdny)

// Keď vyplníš posledný riadok, automaticky sa pridá nový prázdny
// Keď mažeš riadky:
// - Ak je počet riadkov > minimum z inicializácie → fyzicky sa zmaže riadok
// - Ak je počet riadkov = minimum → iba sa vyčistí obsah riadku
Kľúčové vlastnosti:

✅ Vždy zostane aspoň jeden prázdny riadok na konci
✅ Rešpektuje minimálny počet riadkov z inicializácie
✅ Automaticky sa pridávajú riadky pri vyplňovaní
✅ Inteligentné mazanie s ochranou minimálneho počtu

🎯 Custom Delete Validation ⭐ ROZŠÍRENÉ
Mazanie riadkov na základe vlastných validačných pravidiel:
csharp// Definuj custom validačné pravidlá pre mazanie
var deleteRules = new List<ValidationRule>
{
    // Zmaž riadky kde plat > 10000
    ValidationRule.Custom("Salary", value =>
    {
        if (decimal.TryParse(value?.ToString(), out var salary))
            return salary > 10000; // TRUE = zmaž riadok
        return false;
    }, "High salary - row deleted"),

    // Zmaž riadky kde vek > 65
    ValidationRule.Custom("Age", value =>
    {
        if (int.TryParse(value?.ToString(), out var age))
            return age > 65; // TRUE = zmaž riadok
        return false;
    }, "Retirement age - row deleted"),

    // Zmaž riadky kde email je prázdny
    ValidationRule.Custom("Email", value =>
    {
        var email = value?.ToString() ?? "";
        return string.IsNullOrWhiteSpace(email); // TRUE = zmaž riadok
    }, "Empty email - row deleted")
};

// Aplikuj delete pravidlá
await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);

🔧 Kompletné PUBLIC API
Inicializácia
csharpTask InitializeAsync(
    List<ColumnDefinition> columns,
    List<ValidationRule> validationRules,
    ThrottlingConfig throttling,
    int emptyRowsCount = 15
);
Dátové operácie
csharp// Načítanie dát (s auto-add riadkov)
Task LoadDataAsync(List<Dictionary<string, object?>> data);
Task LoadDataAsync(DataTable dataTable);

// Export dát  
Task<DataTable> ExportToDataTableAsync();

// Mazanie dát
Task ClearAllDataAsync(); // Zachováva minimum riadkov
Task DeleteRowsByCustomValidationAsync(List<ValidationRule> deleteRules);
Color Theme API
csharp// Aplikovanie tém
void ApplyColorTheme(DataGridColorTheme theme);
void ResetToDefaultTheme();

// Property pre binding
DataGridColorTheme ColorTheme { get; set; }
Validácie
csharp// Validácia všetkých riadkov
Task<bool> ValidateAllRowsAsync();

📊 Konfigurácia Stĺpcov
csharpvar columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) 
    { 
        MinWidth = 60, 
        Width = 80, 
        Header = "🔢 ID",
        IsEditable = false  // Read-only stĺpec
    },
    
    new("Name", typeof(string)) 
    { 
        MinWidth = 120, 
        Width = 150, 
        Header = "👤 Name",
        DefaultValue = "New User"
    },
    
    new("Email", typeof(string)) 
    { 
        MinWidth = 200, 
        Width = 200, 
        Header = "📧 Email"
    },
    
    new("Salary", typeof(decimal)) 
    { 
        Width = 120, 
        Header = "💰 Salary",
        DisplayFormat = "C2" // Currency formát
    },
    
    // Špeciálne stĺpce
    new("DeleteRows", typeof(string)) { Width = 40 }, // Delete button
    // ValidAlerts sa pridáva automaticky
};

✅ Validačné Pravidlá
Predpripravené validácie
csharpvar validationRules = new List<ValidationRule>
{
    // Základné validácie
    ValidationRule.Required("Name", "Name is required"),
    ValidationRule.Email("Email", "Invalid email format"),
    ValidationRule.Range("Age", 18, 100, "Age must be 18-100"),
    ValidationRule.MinLength("Name", 3, "Name too short"),
    ValidationRule.MaxLength("Name", 50, "Name too long"),
    ValidationRule.Pattern("Phone", @"^\d{10}$", "Invalid phone format")
};
Custom validácie
csharpvar customRules = new List<ValidationRule>
{
    // Jednoduchá custom validácia
    ValidationRule.Custom("Username", value =>
    {
        var username = value?.ToString() ?? "";
        return username.Length >= 3 && !username.Contains(" ");
    }, "Username must be 3+ chars without spaces"),
    
    // Zložitejšia custom validácia
    ValidationRule.Custom("Password", value =>
    {
        var password = value?.ToString() ?? "";
        return password.Length >= 8 && 
               password.Any(char.IsUpper) && 
               password.Any(char.IsLower) && 
               password.Any(char.IsDigit);
    }, "Password must be 8+ chars with upper, lower, and digit")
};

⚙️ Throttling Konfigurácia
csharp// Predpripravené konfigurácie
var config = ThrottlingConfig.Default;        // 300ms validácie
var config = ThrottlingConfig.Fast;           // 150ms validácie  
var config = ThrottlingConfig.Slow;           // 500ms validácie
var config = ThrottlingConfig.PerformanceCritical; // 100ms validácie
var config = ThrottlingConfig.NoThrottling;   // Immediate

// Custom konfigurácia
var customConfig = new ThrottlingConfig
{
    ValidationDebounceMs = 200,
    UIUpdateDebounceMs = 50,
    BatchSize = 100,
    EnableValidationThrottling = true,
    EnableRealtimeValidation = true
};

🖱️ Navigácia a Ovládanie
Klávesové skratky
KlávesaAkciaTabĎalšia bunka + potvrdenie zmienShift+TabPredchádzajúca bunka + potvrdenieEnterBunka o riadok nižšie + potvrdenieEscZrušenie zmien + výskok z bunkyShift+EnterNový riadok v bunke (multiline)Ctrl+CKopírovanie označených buniekCtrl+VVloženie z clipboarduCtrl+XVystrihávacie označených buniek
Excel funkcionalita

✅ Copy/Paste medzi Excel a DataGrid
✅ Zachovanie formátovania
✅ Multiline text support
✅ Automatické parsovanie typov


🎨 Špeciálne Stĺpce
DeleteRows stĺpec
csharp// Automaticky sa vytvorí ak pridáš stĺpec s názvom "DeleteRows"
new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }

Zobrazuje ikonku krížika
NOVÉ: Inteligentné mazanie - fyzicky zmaže riadok ak je nad minimum, inak len vyčistí obsah
Automaticky kompaktuje riadky

ValidAlerts stĺpec

Automaticky sa pridáva na koniec tabuľky
Zobrazuje validačné chyby pre daný riadok
Formát: "ColumnName: Error message; OtherColumn: Other error"


🔍 Validačný Systém
Realtime validácie

Validácia sa spúšťa pri každej zmene (throttling 300ms default)
Validuje sa iba na riadkoch ktoré NIE SÚ úplne prázdne
Červené orámovanie nevalidných buniek
Žiadne tooltips - len vizuálna indikácia

Riadok je považovaný za prázdny ak:
csharp// Všetky bunky (okrem DeleteRows a ValidAlerts) sú null alebo prázdne
bool isEmpty = row.Cells
    .Where(c => c.ColumnName != "DeleteRows" && c.ColumnName != "ValidAlerts")
    .All(c => c.Value == null || string.IsNullOrWhiteSpace(c.Value?.ToString()));

📊 Export Funkcionalita
csharp// Export všetkých dát (bez DeleteRows, s ValidAlerts)
DataTable allData = await DataGridControl.ExportToDataTableAsync();

// Memory management
await DataGridControl.ClearAllDataAsync(); // Fyzicky vymaže dáta z pamäte
DataGridControl.Dispose(); // IDisposable implementácia

🛠️ Performance Optimalizácie
Automatické optimalizácie

Virtualizácia UI - iba viditeľné bunky v DOM
Lazy loading - dáta sa načítavajú postupne
Throttling validácií - debounce 300ms default
Batch operations - 50 items per batch default
Memory pooling - reuse objektov
Background validation - non-critical validácie

Dependency Injection
csharp// Balík používa Microsoft.Extensions.DependencyInjection
services.AddSingleton<IValidationService, ValidationService>();
services.AddSingleton<IDataManagementService, DataManagementService>();
services.AddSingleton<ICopyPasteService, CopyPasteService>();
services.AddTransient<IExportService, ExportService>();

🔒 Accessibility & Security
PUBLIC vs INTERNAL API
Iba tieto triedy sú PUBLIC:

✅ AdvancedDataGrid (hlavný komponent)
✅ ColumnDefinition
✅ ValidationRule
✅ ThrottlingConfig
✅ DataGridColorTheme + DataGridColorThemeBuilder

Všetko ostatné je INTERNAL - čisté API bez zbytočných tried.

🐛 Troubleshooting
Časté problémy
Q: DataGrid sa nezobrazuje
csharp// A: Skontroluj či je zavolaná InitializeAsync
await DataGridControl.InitializeAsync(columns, rules, throttling, 15);
Q: Validácie nefungujú
csharp// A: Skontroluj či sú definované validačné pravidlá pre správne názvy stĺpcov
var rules = new List<ValidationRule>
{
    ValidationRule.Required("Name", "Name is required") // "Name" musí existovať v columns
};
Q: Package Reference nefunguje
bash# A: Skús force restore
dotnet restore --force
# alebo vymaž bin/obj adresáre a rebuild
Q: Performance problémy s veľkými datasetmi
csharp// A: Použij PerformanceCritical throttling
var throttling = ThrottlingConfig.PerformanceCritical;
await DataGridControl.InitializeAsync(columns, rules, throttling, 15);

📈 Changelog
v1.0.2 (2025-01-xx) ⭐ NAJNOVŠIE

✅ NOVÉ: Auto-Add riadkov funkcionalita
✅ NOVÉ: Color Theme API s predpripravenými témami
✅ NOVÉ: Inteligentné mazanie riadkov s ochranou minimálneho počtu
✅ OPRAVENÉ: CS0053 accessibility chyby
✅ OPRAVENÉ: MSBuild targets pre správny Package Reference
✅ VYLEPŠENÉ: Custom delete validation s pokročilejšou logikou
✅ VYLEPŠENÉ: Memory management a performance

v1.0.0 (2025-01-xx)

✅ Initial release
✅ Dynamic column generation
✅ Realtime validations
✅ Copy/Paste Excel functionality
✅ Custom delete validation
✅ Professional clean API


💼 Príklady Použitia
Employee Management
csharpvar columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) { Header = "👤 ID", MinWidth = 60 },
    new("FirstName", typeof(string)) { Header = "📝 First Name", MinWidth = 120 },
    new("LastName", typeof(string)) { Header = "📝 Last Name", MinWidth = 120 },
    new("Email", typeof(string)) { Header = "📧 Email", MinWidth = 200 },
    new("Department", typeof(string)) { Header = "🏢 Department", MinWidth = 150 },
    new("Salary", typeof(decimal)) { Header = "💰 Salary", MinWidth = 120 },
    new("DeleteRows", typeof(string)) { Width = 40 }
};

// Color theme pre HR systém
DataGridControl.ApplyColorTheme(DataGridColorTheme.Blue);

// Custom delete pre ukončených zamestnancov
var deleteRules = new List<ValidationRule>
{
    ValidationRule.Custom("Salary", value =>
    {
        if (decimal.TryParse(value?.ToString(), out var salary))
            return salary < 15000m; // Zmaž nízko platených (možno neaktívni)
        return false;
    }, "Below minimum wage - removed")
};

await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);
