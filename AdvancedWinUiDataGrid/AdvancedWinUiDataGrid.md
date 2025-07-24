📋 Obsah

Účel balíka
Kľúčové funkcie
Inštalácia a použitie
NOVÁ funkcionalita
API referencia
Architektúra balíka
Príklady použitia
Technické detaily


🎯 Účel balíka
RpaWinUiComponents.AdvancedWinUiDataGrid je vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie, ktorý poskytuje pokročilé funkcie pre správu a vizualizáciu dát v moderných desktop aplikáciách.
Prečo tento komponent?

🎨 Dynamické stĺpce - Generovanie stĺpcov za behu podľa dátovej štruktúry
⚡ Realtime validácie - Okamžité validácie s throttling optimalizáciou
📋 Excel kompatibilita - Plná podpora Copy/Paste s Excel aplikáciami
🗑️ Inteligentné mazanie - Custom validačné pravidlá pre mazanie riadkov
🎨 Color themes - Predpripravené a custom farby pre rôzne použitia
⚙️ Performance - Optimalizované pre veľké datasety s virtualizáciou


⭐ Kľúčové funkcie
🆕 NOVÁ FUNKCIONALITA: DeleteRowsByCustomValidationAsync
Hlavnou novinkou je možnosť mazať riadky na základe vlastných validačných pravidiel:
csharp// Definuj pravidlá pre mazanie riadkov
var deleteRules = new List<ValidationRule>
{
    // Zmaž riadky kde plat > 10000
    ValidationRule.Custom("Plat", value =>
        decimal.TryParse(value?.ToString(), out var plat) && plat > 10000m,
        "Vysoký plat - riadok zmazaný"),
        
    // Zmaž riadky kde vek > 50  
    ValidationRule.Custom("Vek", value =>
        int.TryParse(value?.ToString(), out var vek) && vek > 50,
        "Vysoký vek - riadok zmazaný"),
        
    // Zmaž riadky s prázdnym emailom
    ValidationRule.Custom("Email", value =>
        string.IsNullOrWhiteSpace(value?.ToString()),
        "Prázdny email - riadok zmazaný")
};

// Aplikuj delete pravidlá
await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);
🎨 Color Theme API
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
🔍 Realtime Validácie
Validácie sa spúšťajú ihneď pri písaní s optimalizovaným throttling mechanizmom:

Throttling: 300ms default (konfigurovateľné)
Inteligentné: Validuje iba neprázdne riadky
Vizuálne: Červené orámovanie nevalidných buniek
Performance: Optimalizované pre veľké množstvo dát

🧭 Pokročilá Navigácia
Kompletná podpora klávesových skratiek pre efektívnu prácu:
KlávesaAkciaTabĎalšia bunka + potvrdenie zmienShift+TabPredchádzajúca bunka + potvrdenieEnterBunka o riadok nižšie + potvrdenieShift+EnterNový riadok v bunke (multiline)EscZrušenie zmien + výskok z bunkyCtrl+C/V/XCopy/Paste/Cut s Excel kompatibilitou
📊 Špeciálne Stĺpce
DeleteRows stĺpec

Automaticky sa vytvorí pri názve "DeleteRows" v definícii stĺpcov
Zobrazuje ikonku krížika pre mazanie riadkov
Inteligentné mazanie s kompaktovaním

ValidAlerts stĺpec

Vždy prítomný na konci tabuľky
Zobrazuje zoznam validačných chýb pre daný riadok
Formát: "StĺpecNázov: Chybová správa; InýStĺpec: Ďalšia chyba"


🚀 Inštalácia a použitie
1. Pridanie do projektu
xml<!-- V .csproj súbore -->
<ItemGroup>
    <ProjectReference Include="path\to\AdvancedWinUiDataGrid\AdvancedWinUiDataGrid.csproj" />
</ItemGroup>
2. Použitie v XAML
xml<Window xmlns:grid="using:RpaWinUiComponents.AdvancedWinUiDataGrid">
    <Grid>
        <grid:AdvancedDataGrid x:Name="DataGridControl"/>
    </Grid>
</Window>
3. Inicializácia v kóde
csharppublic async Task InitializeDataGrid()
{
    // Definícia stĺpcov
    var columns = new List<ColumnDefinition>
    {
        new("ID", typeof(int)) { Header = "🔢 ID", MinWidth = 80 },
        new("Name", typeof(string)) { Header = "👤 Meno", MinWidth = 150 },
        new("Email", typeof(string)) { Header = "📧 Email", MinWidth = 200 },
        new("Age", typeof(int)) { Header = "🎂 Vek", MinWidth = 80 },
        new("Salary", typeof(decimal)) { Header = "💰 Plat", MinWidth = 120 },
        new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
    };

    // Validačné pravidlá
    var validationRules = new List<ValidationRule>
    {
        ValidationRule.Required("Name", "Meno je povinné"),
        ValidationRule.Email("Email", "Neplatný email formát"),
        ValidationRule.Range("Age", 18, 100, "Vek musí byť 18-100"),
        ValidationRule.Range("Salary", 500, 50000, "Plat musí byť 500-50000")
    };

    // Inicializácia s realtime validáciami
    await DataGridControl.InitializeAsync(
        columns, 
        validationRules, 
        ThrottlingConfig.Default, 
        emptyRowsCount: 15
    );

    // Načítanie dát
    var data = new List<Dictionary<string, object?>>
    {
        new() { ["ID"] = 1, ["Name"] = "Ján Novák", ["Email"] = "jan@example.com", 
                 ["Age"] = 30, ["Salary"] = 2500.00m }
    };
    
    await DataGridControl.LoadDataAsync(data);
}

🔧 API Referencia
Základné Operácie
csharp// Inicializácia
Task InitializeAsync(List<ColumnDefinition> columns, 
                    List<ValidationRule> validationRules, 
                    ThrottlingConfig throttling, 
                    int emptyRowsCount = 15);

// Načítanie dát
Task LoadDataAsync(List<Dictionary<string, object?>> data);
Task LoadDataAsync(DataTable dataTable);

// Validácia
Task<bool> ValidateAllRowsAsync();

// Export
Task<DataTable> ExportToDataTableAsync();

// Vymazanie
Task ClearAllDataAsync();

// 🆕 NOVÁ METÓDA: Custom delete
Task DeleteRowsByCustomValidationAsync(List<ValidationRule> deleteRules);
Konfigurácia Stĺpcov
csharppublic class ColumnDefinition
{
    public string Name { get; set; }
    public Type DataType { get; set; }
    public string? Header { get; set; }
    public double Width { get; set; } = 150;
    public double MinWidth { get; set; } = 50;
    public double MaxWidth { get; set; } = 0;  // 0 = neobmedzené
    public bool IsVisible { get; set; } = true;
    public bool IsEditable { get; set; } = true;
    public object? DefaultValue { get; set; }
    public string? DisplayFormat { get; set; }
}
Validačné Pravidlá
csharp// Predpripravené validácie
ValidationRule.Required(columnName, errorMessage);
ValidationRule.Email(columnName, errorMessage);
ValidationRule.Range(columnName, min, max, errorMessage);
ValidationRule.MinLength(columnName, minLength, errorMessage);
ValidationRule.MaxLength(columnName, maxLength, errorMessage);
ValidationRule.Pattern(columnName, regexPattern, errorMessage);

// Custom validácie
ValidationRule.Custom(columnName, validationFunction, errorMessage);
Throttling Konfigurácia
csharp// Predpripravené konfigurácie
ThrottlingConfig.Default;        // 300ms validácie
ThrottlingConfig.Fast;           // 150ms validácie  
ThrottlingConfig.Slow;           // 500ms validácie
ThrottlingConfig.PerformanceCritical; // 100ms validácie
ThrottlingConfig.NoThrottling;   // Okamžité

// Custom konfigurácia
var config = new ThrottlingConfig
{
    ValidationDebounceMs = 200,
    UIUpdateDebounceMs = 50,
    BatchSize = 100,
    EnableValidationThrottling = true,
    EnableRealtimeValidation = true
};

🏗️ Architektúra balíka
Štruktúra Projektu
AdvancedWinUiDataGrid/
├── Controls/                    # UI komponenty
│   ├── AdvancedDataGrid.xaml    # Hlavný UserControl
│   ├── DataGridCell.xaml        # Bunka tabuľky
│   └── SpecialColumns/          # Špeciálne stĺpce
├── Models/                      # Dátové modely (PUBLIC API)
│   ├── ColumnDefinition.cs      # Definícia stĺpca
│   ├── ValidationRule.cs        # Validačné pravidlá
│   ├── ThrottlingConfig.cs      # Throttling nastavenia
│   └── DataGridColorTheme.cs    # Color themes
├── Services/                    # Business logika (INTERNAL)
│   ├── Interfaces/              # Service rozhrania
│   ├── ValidationService.cs     # Validačné služby
│   ├── DataManagementService.cs # Správa dát
│   ├── CopyPasteService.cs      # Excel copy/paste
│   ├── ExportService.cs         # Export funkcionalita
│   └── NavigationService.cs     # Klávesová navigácia
├── Utilities/                   # Helper triedy (INTERNAL)
└── Extensions/                  # Extension metódy (INTERNAL)
Dependency Injection
Balík používa Microsoft.Extensions.DependencyInjection pre čistú architektúru:
csharpservices.AddSingleton<IValidationService, ValidationService>();
services.AddSingleton<IDataManagementService, DataManagementService>();
services.AddSingleton<ICopyPasteService, CopyPasteService>();
services.AddTransient<IExportService, ExportService>();
services.AddSingleton<INavigationService, NavigationService>();
Design Principles

Clean API: Iba potrebné triedy sú public
SOLID principles: Separation of concerns
Interface-based: Všetky služby majú rozhrania
Resource cleanup: IDisposable, memory management
Performance first: Optimalizácie na všetkých úrovniach


💼 Príklady použitia
1. Employee Management System
csharppublic class EmployeeManagementExample
{
    private AdvancedDataGrid employeeGrid;

    public async Task SetupEmployeeGrid()
    {
        var columns = new List<ColumnDefinition>
        {
            new("ID", typeof(int)) { Header = "👤 Employee ID", MinWidth = 80 },
            new("FirstName", typeof(string)) { Header = "📝 First Name", MinWidth = 120 },
            new("LastName", typeof(string)) { Header = "📝 Last Name", MinWidth = 120 },
            new("Email", typeof(string)) { Header = "📧 Email", MinWidth = 200 },
            new("Department", typeof(string)) { Header = "🏢 Department", MinWidth = 150 },
            new("Salary", typeof(decimal)) { Header = "💰 Salary", MinWidth = 120 },
            new("HireDate", typeof(DateTime)) { Header = "📅 Hire Date", MinWidth = 120 },
            new("IsActive", typeof(bool)) { Header = "✅ Active", MinWidth = 80 },
            new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
        };

        var validationRules = new List<ValidationRule>
        {
            ValidationRule.Required("FirstName", "First name is required"),
            ValidationRule.Required("LastName", "Last name is required"),
            ValidationRule.Email("Email", "Invalid email format"),
            ValidationRule.Required("Department", "Department is required"),
            ValidationRule.Range("Salary", 25000m, 300000m, "Salary must be 25k-300k"),
            
            // Custom validácia pre hire date
            ValidationRule.Custom("HireDate", value =>
            {
                if (DateTime.TryParse(value?.ToString(), out var date))
                    return date <= DateTime.Now && date >= DateTime.Now.AddYears(-40);
                return false;
            }, "Hire date must be within last 40 years")
        };

        await employeeGrid.InitializeAsync(columns, validationRules, 
                                         ThrottlingConfig.PerformanceCritical, 20);

        // 🆕 HR cleanup pomocou custom delete
        var cleanupRules = new List<ValidationRule>
        {
            // Odstráň neaktívnych zamestnancov
            ValidationRule.Custom("IsActive", value =>
                bool.TryParse(value?.ToString(), out var isActive) && !isActive,
                "Inactive employee removed"),

            // Odstráň zamestnancov s neplatným emailom
            ValidationRule.Custom("Email", value =>
            {
                var email = value?.ToString() ?? "";
                return !string.IsNullOrWhiteSpace(email) && !email.Contains("@");
            }, "Invalid email removed")
        };

        await employeeGrid.DeleteRowsByCustomValidationAsync(cleanupRules);
    }
}
2. Financial Data Analysis
csharppublic class FinancialDataExample  
{
    public async Task SetupFinancialAnalysis()
    {
        var columns = new List<ColumnDefinition>
        {
            new("TransactionID", typeof(string)) { Header = "🏷️ Transaction ID", MinWidth = 120 },
            new("Date", typeof(DateTime)) { Header = "📅 Date", MinWidth = 100 },
            new("Amount", typeof(decimal)) { Header = "💰 Amount", MinWidth = 100, DisplayFormat = "C2" },
            new("Category", typeof(string)) { Header = "📂 Category", MinWidth = 120 },
            new("Description", typeof(string)) { Header = "📝 Description", MinWidth = 200 },
            new("DeleteRows", typeof(string)) { Width = 40 }
        };

        var validationRules = new List<ValidationRule>
        {
            ValidationRule.Required("TransactionID", "Transaction ID required"),
            ValidationRule.Range("Amount", -1000000m, 1000000m, "Amount out of range"),
            ValidationRule.Required("Category", "Category required")
        };

        var dataGrid = new AdvancedDataGrid();
        await dataGrid.InitializeAsync(columns, validationRules, ThrottlingConfig.Default, 25);

        // 🆕 Financial cleanup rules
        var financialCleanupRules = new List<ValidationRule>
        {
            // Odstráň mikrotransakcie (< 1€)
            ValidationRule.Custom("Amount", value =>
            {
                if (decimal.TryParse(value?.ToString(), out var amount))
                    return Math.Abs(amount) < 1.0m;
                return false;
            }, "Micro-transaction removed"),

            // Odstráň test transakcie
            ValidationRule.Custom("Description", value =>
            {
                var desc = value?.ToString() ?? "";
                return desc.Contains("test", StringComparison.OrdinalIgnoreCase);
            }, "Test transaction removed")
        };

        await dataGrid.DeleteRowsByCustomValidationAsync(financialCleanupRules);
    }
}

⚙️ Technické detaily
Performance Optimalizácie

Virtualizácia UI: Iba viditeľné bunky v DOM
Lazy loading: Postupné načítavanie dát
Throttling validácií: Debounce 300ms default
Batch operations: 50 items per batch default
Memory pooling: Reuse objektov pre lepšiu performance
Background validation: Non-critical validácie v pozadí

Memory Management
csharp// Automatické cleanup
await DataGridControl.ClearAllDataAsync(); // Fyzicky vymaže dáta z pamäte

// Manual cleanup
DataGridControl.Dispose(); // IDisposable implementácia

// Resource optimization
- Weak references pre event handlery
- Automatic GC pri veľkých operáciách
- Memory leak prevention
- Efficient object disposal
Podporované Dátové Typy

Základné: string, int, long, decimal, double, float
Dátum/čas: DateTime, DateOnly, TimeOnly
Logické: bool
Nullable: Všetky základné typy s ?
Enum: Všetky enum typy
Custom: Objekty cez ToString() a convertery

Validačný Systém
Princípy validácie:

Validácia sa spúšťa iba na neprázdnych riadkoch
Riadok je prázdny ak všetky bunky (okrem špeciálnych stĺpcov) sú null/prázdne
Špeciálne stĺpce (DeleteRows, ValidAlerts) sa nezapočítavajú do prázdnosti
Realtime validácia s throttling optimalizáciou
Červené orámovanie nevalidných buniek (bez tooltipov)

Formát chybových správ:
"NázovStĺpca: Chybová správa; InýStĺpec: Ďalšia chyba"
Excel Kompatibilita
csharp// Copy/Paste mechanizmus
- Copy (Ctrl+C): Multi-select buniek → Excel TSV formát
- Paste (Ctrl+V): Excel clipboard → automatické parsovanie
- Cut (Ctrl+X): Copy + vymazanie zdrojových buniek

// Overflow handling
- Dáta presahujúce posledný stĺpec sa ignorujú
- Automatické vytvorenie nových riadkov pri nedostatku
- Zachovanie formátovania a multiline textu

🔍 Troubleshooting
Časté Problémy
Q: DataGrid sa nezobrazuje
csharp// A: Skontroluj inicializáciu
await DataGridControl.InitializeAsync(columns, rules, throttling, 15);
Q: Validácie nefungujú
csharp// A: Skontroluj názvy stĺpcov v pravidlách
ValidationRule.Required("Name", "Name is required") // "Name" musí existovať v columns
Q: Performance problémy
csharp// A: Použij PerformanceCritical throttling
var throttling = ThrottlingConfig.PerformanceCritical;
Q: Copy/Paste nefunguje
xml<!-- A: Pridaj do Package.appxmanifest -->
<Capability Name="clipboardRead" />
Debug Tipy

Zapni logovanie pre detailné informácie
Skontroluj Browser Developer Tools pre XAML chyby
Použij Performance Profiler pre memory leaks
Testuj s malými datasetmi najprv


📈 Roadmap
Plánované Funkcie (v2.0)

🔍 Search/Filter - Pokročilé vyhľadávanie a filtrovanie
📊 Sorting - Klikateľné stĺpce pre sorting
📱 Responsive - Adaptívny design pre rôzne veľkosti




Development Guidelines

Dodržujte C# coding standards
Pridajte unit testy pre nové funkcie
Aktualizujte dokumentáciu
Používajte meaningful commit messages





🎉 Záver
RpaWinUiComponents.AdvancedWinUiDataGrid predstavuje moderné riešenie pre dátové aplikácie vo WinUI3. S pokročilými funkciami ako custom delete validácie, color themes, realtime validácie a Excel kompatibilita poskytuje všetko potrebné pre profesionálne desktopové aplikácie.
Hlavné prínosy:

⚡ Výkonnosť - Optimalizované pre veľké datasety
🎨 Flexibilita - Prispôsobiteľné pre rôzne použitia
🔧 Jednoduchos℘ - Clean API a jasná dokumentácia
🚀 Inovácie - Najnovšie funkcie a best practices