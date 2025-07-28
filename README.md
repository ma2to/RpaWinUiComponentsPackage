Účel balíka
Tento balík poskytuje vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie s podporou:
✅ Dynamického generovania stĺpcov - definuj stĺpce programmaticky
✅ AUTO-ADD riadkov - automaticky pridáva prázdne riadky pri práci s dátami
✅ Individual Colors - nastav jednotlivé farby namiesto celých tém
✅ Search & Filter - vyhľadávanie v jednotlivých stĺpcoch
✅ Header Click Sorting - kliknutím na header sort vzostupne/zostupne
✅ Zebra Rows - striedavé farby riadkov pre lepšiu čitateľnosť
✅ Realtime validácie s throttling pre výkon
✅ Copy/Paste Excel funkcionalitu
✅ Custom validačné pravidlá pre mazanie riadkov
✅ Clean PUBLIC API - iba 5 verejných tried

📦 Inštalácia
bash# Package Manager Console
Install-Package RpaWinUiComponents.AdvancedWinUiDataGrid

# .NET CLI
dotnet add package RpaWinUiComponents.AdvancedWinUiDataGrid
xml<!-- PackageReference -->
<PackageReference Include="RpaWinUiComponents.AdvancedWinUiDataGrid" Version="1.0.16" />

🚀 Quick Start
1. XAML Setup
xml<!-- MainWindow.xaml -->
<Window x:Class="YourApp.MainWindow"
        xmlns:grid="using:RpaWinUiComponents.AdvancedWinUiDataGrid">
    
    <Grid>
        <grid:AdvancedDataGrid x:Name="DataGridControl"/>
    </Grid>
</Window>
2. Základná inicializácia
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
            new("Age", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Age" },
            new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
        };

        // 2. Validačné pravidlá
        var rules = new List<ValidationRule>
        {
            ValidationRule.Required("Name", "Name is required"),
            ValidationRule.Email("Email", "Invalid email format"),
            ValidationRule.Range("Age", 18, 100, "Age must be 18-100")
        };

        // 3. Individual Colors (voliteľné)
        var colors = new DataGridColorConfig
        {
            CellBackgroundColor = Colors.White,
            AlternateRowColor = Color.FromArgb(20, 0, 120, 215), // Zebra effect
            ValidationErrorColor = Colors.Red
        };

        // 4. Inicializácia s AUTO-ADD (5 minimálnych riadkov)
        await DataGridControl.InitializeAsync(columns, rules, 
            ThrottlingConfig.Default, emptyRowsCount: 5, colors);

        // 5. Načítanie dát
        var data = new List<Dictionary<string, object?>>
        {
            new() { ["ID"] = 1, ["Name"] = "John", ["Email"] = "john@test.com", ["Age"] = 30 }
        };
        await DataGridControl.LoadDataAsync(data);
    }
}

🔒 PUBLIC API (Clean Interface)
Iba tieto triedy sú verejné:
TriedaÚčelAdvancedDataGridHlavný komponentColumnDefinitionDefinícia stĺpcaValidationRuleValidačné pravidláThrottlingConfigVýkonové nastaveniaDataGridColorConfigIndividual farby
Všetko ostatné je INTERNAL pre čistý API.

⚡ AUTO-ADD Funkcionalita
Automatické pridávanie a mazanie riadkov:
csharp// Inicializácia s 5 minimálnymi riadkami
await DataGridControl.InitializeAsync(columns, rules, throttling, emptyRowsCount: 5);

// ✅ Ak načítaš 3 riadky dát → bude 5 riadkov (3 s dátami + 2 prázdne)
// ✅ Ak načítaš 10 riadkov dát → bude 11 riadkov (10 s dátami + 1 prázdny)
// ✅ Vyplníš posledný prázdny → automaticky sa pridá nový prázdny
// ✅ Mažeš riadky: nad minimum = fyzicky zmaže, na minimum = len vyčistí obsah
Kľúčové vlastności:

Vždy zostane aspoň 1 prázdny riadok na konci
Rešpektuje minimálny počet z inicializácie
Inteligentné mazanie s ochranou minima


🎨 Individual Colors System
Nastavovanie jednotlivých farieb namiesto celých tém:
csharp// Predpripravené konfigurácie
var lightColors = DataGridColorConfig.Light;
var darkColors = DataGridColorConfig.Dark;
var blueColors = DataGridColorConfig.Blue;

// Custom individual colors
var customColors = new DataGridColorConfig
{
    CellBackgroundColor = Colors.LightYellow,
    CellBorderColor = Colors.Orange,
    HeaderBackgroundColor = Colors.DarkBlue,
    HeaderTextColor = Colors.White,
    ValidationErrorColor = Colors.Red,
    AlternateRowColor = Color.FromArgb(30, 255, 165, 0), // Zebra rows
    SelectionColor = Color.FromArgb(100, 0, 120, 215),
    EditingCellColor = Color.FromArgb(50, 255, 215, 0)
};

// Aplikovanie pri inicializácii
await DataGridControl.InitializeAsync(columns, rules, throttling, 15, customColors);
Available Color Properties:

CellBackgroundColor - pozadie bunky
CellBorderColor - okraj bunky
CellTextColor - text v bunke
HeaderBackgroundColor - pozadie header-u
HeaderTextColor - text header-u
ValidationErrorColor - červené orámovanie chýb
SelectionColor - označené bunky
AlternateRowColor - zebra rows effect
HoverColor - hover nad bunkou
EditingCellColor - bunka v editácii


🔍 Search & Sort & Zebra Rows
Search v stĺpcoch
csharp// Nastavenie search filtra pre stĺpec
await DataGridControl.SetColumnSearchAsync("Name", "John");

// Vyčistenie všetkých search filtrov
await DataGridControl.ClearAllSearchAsync();
Header Click Sorting
csharp// Toggle sort pri kliknutí na header (None → Asc → Desc → None)
await DataGridControl.ToggleColumnSortAsync("Age");

// Prázdne riadky sú vždy na konci (neorderujú sa)
Zebra Rows Effect
csharp// Povolenie/zakázanie zebra rows
await DataGridControl.SetZebraRowsEnabledAsync(true);

// Zebra farba sa nastavuje cez DataGridColorConfig.AlternateRowColor
var config = new DataGridColorConfig 
{ 
    AlternateRowColor = Color.FromArgb(20, 0, 120, 215) 
};
Zebra logika:

Iba neprázdne riadky majú zebra effect
Každý druhý neprázdny riadok má alternatívnu farbu
Prázdne riadky na konci majú normálnu farbu


✅ Validačné Systém
Predpripravené validácie
csharpvar rules = new List<ValidationRule>
{
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
    ValidationRule.Custom("Username", value =>
    {
        var username = value?.ToString() ?? "";
        return username.Length >= 3 && !username.Contains(" ");
    }, "Username must be 3+ chars without spaces")
};
Realtime validations

Validácia sa spúšťa pri každej zmene (throttling 300ms default)
Červené orámovanie nevalidných buniek
Validuje sa iba na neprázdnych riadkoch
Žiadne tooltips - len vizuálna indikácia


📊 Konfigurácia Stĺpcov
csharpvar columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) 
    { 
        MinWidth = 60, 
        Width = 80, 
        Header = "🔢 ID",
        IsEditable = false  // Read-only
    },
    
    new("Name", typeof(string)) 
    { 
        MinWidth = 120, 
        Width = 150, 
        Header = "👤 Name",
        DefaultValue = "New User"
    },
    
    new("Salary", typeof(decimal)) 
    { 
        Width = 120, 
        Header = "💰 Salary",
        DisplayFormat = "C2" // Currency format
    },
    
    // Špeciálne stĺpce
    new("DeleteRows", typeof(string)) { Width = 40 }, // Delete button
    // ValidAlerts sa pridáva automaticky
};

⚙️ Throttling & Performance
csharp// Predpripravené konfigurácie
var config = ThrottlingConfig.Default;        // 300ms validácie
var config = ThrottlingConfig.Fast;           // 150ms validácie
var config = ThrottlingConfig.PerformanceCritical; // 100ms validácie

// Custom konfigurácia
var customThrottling = new ThrottlingConfig
{
    ValidationDebounceMs = 200,
    UIUpdateDebounceMs = 50,
    SearchDebounceMs = 500,
    EnableRealtimeValidation = true
};

🖱️ Navigácia & Ovládanie
KlávesaAkciaTabĎalšia bunka + potvrdenie zmienShift+TabPredchádzajúca bunka + potvrdenieEnterBunka o riadok nižšie + potvrdenieEscZrušenie zmien + výskok z bunkyShift+EnterNový riadok v bunke (multiline)Ctrl+CKopírovanie označených buniekCtrl+VVloženie z clipboarduCtrl+XVystrihávanie označených buniek

🎨 Špeciálne Stĺpce
DeleteRows stĺpec
csharp// Automaticky sa vytvorí ak pridáš stĺpec s názvom "DeleteRows"
new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }

Zobrazuje ikonku krížika
Inteligentné mazanie - fyzicky zmaže ak je nad minimum, inak len vyčistí obsah
Automaticky kompaktuje riadky

ValidAlerts stĺpec

Automaticky sa pridáva na koniec tabuľky
Zobrazuje validačné chyby pre daný riadok
Formát: "ColumnName: Error message; OtherColumn: Other error"


📊 Dátové Operácie
csharp// Načítanie dát s AUTO-ADD
await DataGridControl.LoadDataAsync(dataList);
await DataGridControl.LoadDataAsync(dataTable);

// Export dát (bez DeleteRows, s ValidAlerts)
DataTable exported = await DataGridControl.ExportToDataTableAsync();

// Validácia
bool isValid = await DataGridControl.ValidateAllRowsAsync();

// Mazanie
await DataGridControl.ClearAllDataAsync(); // Zachováva minimum riadkov

// Custom delete pravidlá
var deleteRules = new List<ValidationRule>
{
    ValidationRule.Custom("Salary", value =>
        decimal.TryParse(value?.ToString(), out var salary) && salary > 10000, 
        "High salary - deleted")
};
await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);

🛠️ Architektúra Projektu
RpaWinUiComponentsPackage.sln
├── AdvancedWinUiDataGrid/           # 📦 BALÍK PROJECT
│   ├── Controls/                    # UI komponenty
│   │   ├── AdvancedDataGrid.xaml    # Hlavný komponent
│   │   ├── DataGridCell.xaml        # Bunka komponent
│   │   ├── SearchAndSortHeader.xaml # Search/Sort header
│   │   └── SpecialColumns/          # DeleteRows, ValidAlerts
│   ├── Models/                      # PUBLIC API triedy
│   │   ├── ColumnDefinition.cs      # ✅ PUBLIC
│   │   ├── ValidationRule.cs        # ✅ PUBLIC
│   │   ├── ThrottlingConfig.cs      # ✅ PUBLIC
│   │   └── DataGridColorConfig.cs   # ✅ PUBLIC
│   ├── Services/                    # INTERNAL business logika
│   │   ├── DataManagementService.cs # AUTO-ADD logika
│   │   ├── ValidationService.cs     # Realtime validácie
│   │   ├── SearchAndSortService.cs  # Search/Sort/Zebra
│   │   └── Interfaces/              # INTERNAL interfaces
│   ├── Utilities/                   # INTERNAL helpers
│   └── Themes/                      # XAML resources
└── RpaWinUiComponents.Demo/         # 🎯 DEMO PROJECT
    └── (používa Package Reference)   # KĽÚČOVÉ: Nie Project Reference!
Kľúčové princípy:

Package Reference testovanie - Demo používa balík cez NuGet
Clean API - Iba 5 PUBLIC tried, všetko ostatné INTERNAL
SOLID principles - Dependency injection, clean architecture
Performance optimalizácie - Throttling, virtualizácia, lazy loading


📈 Changelog
v1.0.16 (Current) - NETSDK1152 FIXED
✅ OPRAVENÉ: NETSDK1152 duplicitné dll chyba
✅ NOVÉ: Search & Filter v stĺpcoch
✅ NOVÉ: Header Click Sorting (vzostupne/zostupne)
✅ NOVÉ: Zebra Rows effect pre lepšiu čitateľnosť
✅ VYLEPŠENÉ: Individual Colors API namiesto tém
✅ VYLEPŠENÉ: AUTO-ADD s inteligentným mazaním
v1.0.2
✅ AUTO-ADD riadkov funkcionalita
✅ Individual Colors system
✅ Custom delete validation
✅ Performance optimalizácie
v1.0.0
✅ Initial release
✅ Dynamic column generation
✅ Realtime validations
✅ Copy/Paste Excel functionality

🐛 Troubleshooting
Časté problémy
Q: DataGrid sa nezobrazuje
csharp// A: Skontroluj či je zavolaná InitializeAsync
await DataGridControl.InitializeAsync(columns, rules, throttling, 15);
Q: NETSDK1152 chyba
bash# A: Vymaž NuGet cache a rebuild
dotnet nuget locals global-packages --clear
dotnet restore --force
dotnet build
Q: Validácie nefungujú
csharp// A: Skontroluj názvy stĺpcov v pravidlách
ValidationRule.Required("Name", "Required") // "Name" musí existovať v columns
Q: Search/Sort nefunguje
csharp// A: Skontroluj či sú dáta načítané a nie prázdne
await DataGridControl.LoadDataAsync(data);
await DataGridControl.SetColumnSearchAsync("columnName", "searchText");

💼 Príklady Použitia
Employee Management System
csharpvar columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) { Header = "👤 Employee ID", Width = 80 },
    new("FirstName", typeof(string)) { Header = "📝 First Name", Width = 120 },
    new("LastName", typeof(string)) { Header = "📝 Last Name", Width = 120 },
    new("Email", typeof(string)) { Header = "📧 Email", Width = 200 },
    new("Department", typeof(string)) { Header = "🏢 Department", Width = 150 },
    new("Salary", typeof(decimal)) { Header = "💰 Salary", Width = 120 },
    new("DeleteRows", typeof(string)) { Width = 40 }
};

// Corporate blue theme s zebra
var corporateColors = new DataGridColorConfig
{
    HeaderBackgroundColor = Color.FromArgb(255, 0, 50, 100),
    HeaderTextColor = Colors.White,
    AlternateRowColor = Color.FromArgb(15, 0, 120, 215),
    ValidationErrorColor = Colors.DarkRed
};

await DataGridControl.InitializeAsync(columns, rules, throttling, 10, corporateColors);

