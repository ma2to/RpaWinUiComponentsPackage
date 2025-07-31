RpaWinUiComponentsPackage
Profesionálny multi-component balík pre WinUI3 aplikácie s pokročilými komponentmi pre enterprise aplikácie

🎯 O balíku
Tento balík poskytuje súbor nezávislých WinUI3 komponentov pre enterprise aplikácie s dôrazom na produktivitu, výkon a diagnostiku. Komponenty sú navrhnuté ako samostatné moduly bez vzájomných závislostí.
📦 Komponenty v balíku
KomponentÚčelKľúčové funkcieStatusAdvancedWinUiDataGridPokročilý DataGrid pre dátové aplikácieAuto-Add, Resize, Scroll, Stretch, Search/Sort/Zebra, Realtime validácie✅ StableLoggerComponentThread-safe súborové logovanieRotácia súborov, async operácie, konfigurovateľný✅ Stable

🏗️ Architektúra balíka
✅ Nezávislé komponenty

AdvancedWinUiDataGrid a LoggerComponent sú úplne nezávislé
Žiadny komponent nepoužíva metódy z iného komponentu
Každý komponent má svoje vlastné internal implementácie
Zdieľané utility sú iba v Common/SharedUtilities

🔌 Logging architektúra

Komponenty používajú iba Microsoft.Extensions.Logging.Abstractions
Nie Microsoft.Extensions.Logging (žiadne konkrétne implementácie)
Ak nezašlete logger → komponenty sa tvária že logujú (NullLogger)
Ak zašlete logger → komponenty používajú váš logging systém

📁 Projektová štruktúra
RpaWinUiComponentsPackage/
├── AdvancedWinUiDataGrid/              # 📊 DataGrid komponent
│   ├── Controls/                       # UI komponenty (XAML/CS)
│   ├── Models/                         # ✅ PUBLIC API triedy
│   ├── Services/                       # INTERNAL business logika
│   ├── Themes/                         # XAML resources
│   └── Utilities/                      # INTERNAL helpers
|       └── Extensions/                 # Extension metódy
├── LoggerComponent/                    # 📝 Logger komponent
│   ├── Core/                           # ✅ PUBLIC API
│   ├── Configuration/                  # INTERNAL config
│   └── FileManagement/                 # INTERNAL file ops
└── Common/                             # 🔧 Zdieľané utility
    └── SharedUtilities/                # INTERNAL helpers
        

📊 1. AdvancedWinUiDataGrid
🎯 Účel
Vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie s podporou pokročilých funkcionalít pre dátové aplikácie a enterprise použitie.
✨ Kompletné funkcie
🔄 Auto-Add System

✅ Inteligentné pridávanie riadkov - automaticky pridáva prázdne riadky
✅ Minimálny count režim - vždy udržuje min. počet riadkov
✅ Smart deletion - nad minimum zmaže, na minimum vyčistí

🎨 UI/UX Features

✅ Column Resize - myšou ťahanie hrán stĺpcov
✅ Scroll Support - horizontálny + vertikálny scroll
✅ ValidAlerts Stretching - ValidAlerts stĺpec sa rozťahuje na koniec
✅ Individual Colors - nastavenie vlastných farieb namiesto tém
✅ Zebra Rows - striedavé farby riadkov pre lepšiu čitateľnosť
✅ Responzívny dizajn - prispôsobuje sa veľkosti okna

🔍 Search & Sort

✅ Column Search - vyhľadávanie v jednotlivých stĺpcoch
✅ Header Click Sorting - klik na header: None → Asc → Desc → None
✅ Multi-column filtering - kombinácia search filtrov

⚡ Validácie

✅ Realtime validácie s throttling pre výkon
✅ Custom validačné pravidlá (Required, Email, Range, Pattern, Custom)
✅ Batch validation - validácia všetkých riadkov naraz
✅ Custom delete validation - validačné pravidlá pre mazanie riadkov

📋 Import/Export

✅ Copy/Paste Excel funkcionalita
✅ Export to DataTable - bez DeleteRows, s ValidAlerts
✅ CSV export s konfigurovateľnými headers

📝 Logging Integration

✅ Kompletné logovanie všetkých operácií cez ILogger abstractions
✅ Nezávislý logging - funguje s ľubovoľným logging systémom
✅ Performance tracking - metriky operácií
✅ Error diagnostics - detailné chybové logy

🚀 Quick Start
csharp// MainWindow.xaml
<Window x:Class="YourApp.MainWindow"
        xmlns:grid="using:RpaWinUiComponentsPackage.AdvancedWinUiDataGrid">
    <Grid>
        <grid:AdvancedDataGrid x:Name="DataGridControl"/>
    </Grid>
</Window>
csharp// MainWindow.xaml.cs - NEZÁVISLÉ použitie (bez loggingu)
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;

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
            new("ValidAlerts", typeof(string)) { MinWidth = 200, Header = "⚠️ Validation" }
        };

        // 2. Validačné pravidlá
        var rules = new List<ValidationRule>
        {
            ValidationRule.Required("Name", "Name is required"),
            ValidationRule.Email("Email", "Invalid email format"),
            ValidationRule.Range("Age", 18, 100, "Age must be 18-100")
        };

        // 3. Individual Colors s Zebra effect
        var colors = new DataGridColorConfig
        {
            CellBackgroundColor = Microsoft.UI.Colors.White,
            AlternateRowColor = Color.FromArgb(20, 0, 120, 215), // Zebra effect
            ValidationErrorColor = Microsoft.UI.Colors.Red
        };

        // 4. ⭐ INICIALIZÁCIA bez loggingu (používa NullLogger)
        await DataGridControl.InitializeAsync(
            columns, 
            rules, 
            ThrottlingConfig.Default, 
            emptyRowsCount: 5, 
            colors
        );

        // 5. Načítanie dát
        var data = new List<Dictionary<string, object?>>
        {
            new() { ["ID"] = 1, ["Name"] = "John", ["Email"] = "john@test.com", ["Age"] = 30 }
        };
        await DataGridControl.LoadDataAsync(data);
    }
}
🔗 Použitie s LoggerComponent
csharp// S LoggerComponent integráciou
public sealed partial class MainWindow : Window
{
    private LoggerComponent? _logger;
    
    private async void InitializeWithLogging()
    {
        // 1. Vytvor LoggerComponent
        var logDir = @"C:\Logs";
        var logFileName = "DataGrid.log";
        var maxSizeMB = 10;
        
        _logger = new LoggerComponent(yourExternalLogger, logDir, logFileName, maxSizeMB);
        
        // 2. Vytvor AdvancedDataGrid s logging integráciou
        var dataGrid = new AdvancedDataGrid(_logger.ExternalLogger);
        
        // 3. Inicializuj s loggerom
        await dataGrid.InitializeAsync(columns, rules, throttling, 15, colors);
        
        // Teraz sa všetky DataGrid operácie logujú cez LoggerComponent
    }
}
🎨 Individual Colors System
csharp// Predpripravené konfigurácie
var lightColors = DataGridColorConfig.Light;           // Svetlá téma
var darkColors = DataGridColorConfig.Dark;             // Tmavá téma  
var blueColors = DataGridColorConfig.Blue;             // Modrá téma
var zebraColors = DataGridColorConfig.WithStrongZebra; // Výrazný zebra effect

// Custom individual colors
var customColors = new DataGridColorConfig
{
    CellBackgroundColor = Microsoft.UI.Colors.LightYellow,
    CellBorderColor = Microsoft.UI.Colors.Orange,
    HeaderBackgroundColor = Microsoft.UI.Colors.DarkBlue,
    HeaderTextColor = Microsoft.UI.Colors.White,
    ValidationErrorColor = Microsoft.UI.Colors.Red,
    AlternateRowColor = Color.FromArgb(30, 255, 165, 0), // Zebra rows
    SelectionColor = Color.FromArgb(100, 0, 120, 215),
    EditingCellColor = Color.FromArgb(50, 255, 215, 0)
};
📐 Resize, Scroll & Stretch Features
csharp// ✅ RESIZE - automaticky povolené
// - Behni myšou na okraj header stĺpca
// - Klikni a ťahaj pre zmenu šírky
// - ValidAlerts stĺpec nemá resize grip (stretch mode)

// ✅ SCROLL - automaticky povolené  
// - Horizontálny scroll ak stĺpce presahujú šírku
// - Vertikálny scroll ak riadky presahujú výšku
// - Synchronizovaný scroll medzi header a data

// ✅ STRETCH - automaticky pre ValidAlerts
// - ValidAlerts stĺpec sa vždy rozťahuje na koniec tabulky
// - Ostatné stĺpce majú fixnú šírku nastavenú v ColumnDefinition
// - Responzívne správanie pri zmene veľkosti okna
🔥 Auto-Add Funkcionalita
csharp// Inicializácia s 5 minimálnymi riadkami
await DataGridControl.InitializeAsync(columns, rules, throttling, emptyRowsCount: 5, colors);

// ✅ Logika:
// - Ak načítaš 3 riadky dát → bude 6 riadkov (3 s dátami + 3 prázdne)
// - Ak načítaš 10 riadkov dát → bude 11 riadkov (10 s dátami + 1 prázdny)
// - Vyplníš posledný prázdny → automaticky sa pridá nový prázdny
// - Mažeš riadky: nad minimum = fyzicky zmaže, na minimum = len vyčistí obsah
📊 Validačné Systém
csharp// Predpripravené validácie
var rules = new List<ValidationRule>
{
    ValidationRule.Required("Name", "Name is required"),
    ValidationRule.Email("Email", "Invalid email format"),  
    ValidationRule.Range("Age", 18, 100, "Age must be 18-100"),
    ValidationRule.MinLength("Name", 3, "Name too short"),
    ValidationRule.MaxLength("Name", 50, "Name too long"),
    ValidationRule.Pattern("Phone", @"^\d{10}$", "Invalid phone format")
};

// Custom validácie
var customRules = new List<ValidationRule>
{
    ValidationRule.Custom("Username", value =>
    {
        var username = value?.ToString() ?? "";
        return username.Length >= 3 && !username.Contains(" ");
    }, "Username must be 3+ chars without spaces")
};

// Custom delete validation
var deleteRules = new List<ValidationRule>
{
    ValidationRule.Custom("Salary", value =>
    {
        if (decimal.TryParse(value?.ToString(), out var salary))
            return salary > 10000; // TRUE = zmaž riadok
        return false;
    }, "High salary - row deleted")
};

await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);
✅ PUBLIC API - AdvancedWinUiDataGrid
Hlavné triedy:

AdvancedDataGrid - hlavný komponent
ColumnDefinition - definícia stĺpca
ValidationRule - validačné pravidlá
ThrottlingConfig - throttling nastavenia
DataGridColorConfig - individual color configuration
SortDirection - enum pre sorting

Hlavné metódy:
csharp// Inicializácia
Task InitializeAsync(columns, rules, throttling, emptyRowsCount, colors)

// Dátové operácie  
Task LoadDataAsync(List<Dictionary<string, object?>> data)
Task<DataTable> ExportToDataTableAsync()
Task ClearAllDataAsync()

// Validácie
Task<bool> ValidateAllRowsAsync()
Task DeleteRowsByCustomValidationAsync(List<ValidationRule> deleteRules)

// Search & Sort
Task SetColumnSearchAsync(string columnName, string searchText)
Task SortByColumnAsync(string columnName, SortDirection direction)

📝 2. LoggerComponent
🎯 Účel
Thread-safe súborový logger s automatickou rotáciou súborov, určený pre enterprise aplikácie. Nezávislý na AdvancedWinUiDataGrid.
✨ Kľúčové funkcie

✅ Thread-safe logovanie - bezpečné pre concurrent použitie
✅ Automatická rotácia súborov - pri dosiahnutí max veľkosti
✅ Konfigurovateľná veľkosť - nastaviteľný limit súborov
✅ Async operácie - neblokujúce I/O operácie
✅ Flexibilné log levely - Info, Debug, Warning, Error
✅ Externý logger wrapper - rozširuje ľubovoľný ILogger
✅ Jednoduchý API - priame metódy + backward compatibility

🚀 Quick Start
csharpusing RpaWinUiComponentsPackage.Logger;

// Základné použitie s externým loggerom
public class MyService
{
    private readonly LoggerComponent _logger;
    
    public MyService(ILogger<MyService> externalLogger)
    {
        var logDir = @"C:\Logs";
        var fileName = "MyApp.log"; 
        var maxSizeMB = 10; // 10MB per file
        
        // LoggerComponent vyžaduje externý ILogger
        _logger = new LoggerComponent(externalLogger, logDir, fileName, maxSizeMB);
    }
    
    public async Task DoSomethingAsync()
    {
        // ✅ NOVÉ: Priame metódy pre log levely
        await _logger.Info("Operation started");
        await _logger.Debug("Processing data...");
        await _logger.Warning("Performance warning");
        
        try
        {
            await SomeOperation();
            await _logger.Info("Operation completed successfully");
        }
        catch (Exception ex)
        {
            // ✅ NOVÉ: Error s exception
            await _logger.Error(ex, "Operation failed");
            throw;
        }
        
        // ✅ ZACHOVANÁ: Pôvodná metóda pre backward compatibility
        await _logger.LogAsync("Legacy log message", "INFO");
    }
    
    public void Dispose()
    {
        _logger?.Dispose();
    }
}
🔧 Factory Methods
csharp// Z ILoggerFactory
var logger = LoggerComponent.FromLoggerFactory(
    loggerFactory, 
    @"C:\Logs", 
    "app.log", 
    maxSizeMB: 5,
    categoryName: "MyCategory"
);

// Bez rotácie
var simpleLogger = LoggerComponent.WithoutRotation(
    externalLogger, 
    @"C:\Logs", 
    "simple.log"
);

// S rotáciou
var rotatingLogger = LoggerComponent.WithRotation(
    externalLogger, 
    @"C:\Logs", 
    "rotating.log", 
    maxSizeMB: 10
);
📊 Rotácia súborov
csharp// Bez rotácie (maxSizeMB = null)
var logger = new LoggerComponent(externalLogger, @"C:\Logs", "app.log", null);
// Vytvorí: app.log (neobmedzená veľkosť)

// S rotáciou (maxSizeMB = 5)
var logger = new LoggerComponent(externalLogger, @"C:\Logs", "app.log", 5);
// Vytvorí: app_1.log, app_2.log, app_3.log... (každý max 5MB)
🔍 Diagnostické vlastnosti
csharp// Získaj informácie o súčasnom stave
Console.WriteLine($"Current log file: {logger.CurrentLogFile}");
Console.WriteLine($"File size: {logger.CurrentFileSizeMB:F2} MB");
Console.WriteLine($"Rotation files: {logger.RotationFileCount}");
Console.WriteLine($"Rotation enabled: {logger.IsRotationEnabled}");
Console.WriteLine($"External logger: {logger.ExternalLoggerType}");

// Diagnostika
var info = logger.GetDiagnosticInfo();
var rotationInfo = logger.GetRotationInfo();
var testResult = await logger.TestLoggingAsync();
✅ PUBLIC API - LoggerComponent
Hlavná trieda:

LoggerComponent - hlavná trieda

Priame metódy:
csharp// Nové priame metódy
Task Info(string message)
Task Debug(string message)  
Task Warning(string message)
Task Error(string message)
Task Error(Exception exception, string? message = null)

// Backward compatibility
Task LogAsync(string message, string logLevel = "INFO")
Factory metódy:
csharpstatic LoggerComponent FromLoggerFactory(...)
static LoggerComponent WithoutRotation(...)
static LoggerComponent WithRotation(...)
Vlastnosti:
csharpstring CurrentLogFile
double CurrentFileSizeMB
int RotationFileCount
bool IsRotationEnabled
ILogger ExternalLogger

🔗 3. Kombinácia komponentov
Príklad: AdvancedDataGrid + LoggerComponent
csharppublic sealed partial class MainWindow : Window
{
    private LoggerComponent? _logger;
    private AdvancedDataGrid? _dataGrid;
    
    private async void InitializeComponents()
    {
        // 1. Vytvor svoj ILogger (napr. z DI container)
        var yourLogger = serviceProvider.GetService<ILogger<MainWindow>>();
        
        // 2. Vytvor LoggerComponent (pre súborové logovanie + váš logger)
        _logger = new LoggerComponent(yourLogger, @"C:\AppLogs", "MyApp.log", 10);
        await _logger.Info("Application starting...");
        
        // 3. Vytvor AdvancedDataGrid s LoggerComponent integráciou
        _dataGrid = new AdvancedDataGrid(_logger.ExternalLogger);
        await _dataGrid.InitializeAsync(columns, rules, throttling, 15, colors);
        
        // 4. Teraz sú komponenty prepojené ale nezávislé
        // - DataGrid loguje cez váš logger systém
        // - LoggerComponent súčasne zapisuje do súborov
        // - Oba komponenty sú nezávislé na sebe
        
        await _logger.Info("Components initialized and integrated");
    }
}

📦 Inštalácia
Package Manager Console
bashInstall-Package RpaWinUiComponentsPackage
.NET CLI
bashdotnet add package RpaWinUiComponentsPackage
PackageReference
xml<PackageReference Include="RpaWinUiComponentsPackage" Version="1.0.4" />

🔒 PUBLIC vs INTERNAL API
✅ PUBLIC API (dostupné v demo aplikácii)
AdvancedWinUiDataGrid:

AdvancedDataGrid - hlavný komponent
ColumnDefinition - definícia stĺpca
ValidationRule - validačné pravidlá
ThrottlingConfig - throttling nastavenia
DataGridColorConfig - individual color configuration
SortDirection - enum pre sorting

LoggerComponent:

LoggerComponent - hlavná trieda

❌ INTERNAL (nie je dostupné)

Všetky Services, Models, Utilities, Extensions v komponentoch
Všetky Interfaces
Všetky Helper triedy
Všetky implementation detaily
XAML súbory komponentov

🔧 SharedUtilities (zdieľané medzi komponentmi)

Common helpers
Logging abstractions


📝 Logging Systém
🎯 Architektúra

Nezávislé komponenty - každý má vlastné logovanie cez abstractions
Žiadne závislosti - komponenty nepoužívajú Microsoft.Extensions.Logging
Iba abstractions - Microsoft.Extensions.Logging.Abstractions
Flexibilná integrácia - kompatibilné s ľubovoľným logging systémom

🔌 Integračné možnosti
Bez loggingu (NullLogger)
csharp// AdvancedDataGrid bez loggingu
var dataGrid = new AdvancedDataGrid(); // používa NullLogger
await dataGrid.InitializeAsync(columns, rules, throttling, 15, colors);
S vaším logger systémom
csharp// S vaším ILogger
var yourLogger = serviceProvider.GetService<ILogger<MainWindow>>();
var dataGrid = new AdvancedDataGrid(yourLogger);
S LoggerComponent
csharp// LoggerComponent + váš logger + súborové logovanie
var loggerComponent = new LoggerComponent(yourLogger, @"C:\Logs", "app.log", 10);
var dataGrid = new AdvancedDataGrid(loggerComponent.ExternalLogger);
📊 Čo sa loguje v AdvancedWinUiDataGrid
csharp// ✅ Všetky operácie sú kompletne zalogované:

// Dátové operácie
"📊 LoadDataAsync START - InputRows: 150"
"✅ LoadDataAsync COMPLETED - Duration: 45.2ms, FinalRowCount: 155"

// UI operácie  
"🖱️ Resize started - Column: Name, StartWidth: 150"
"✅ Resize completed - Column: Name, FinalWidth: 200"

// Validácie
"🔍 ValidateCellAsync START - Column: Email, Value: 'john@test.com'"
"❌ Validation FAILED for Age: 1 errors - Age must be 18-100"

// Performance
"⏱️ Operation END: UpdateDisplayRows - 23.1ms"

// Chyby
"❌ CRITICAL ERROR during LoadDataAsync"
"❌ ERROR in SetCellValueAsync [5, Email]"

🎯 Use Cases
🏢 Enterprise dátové aplikácie

Employee management systémy s resize, scroll a validáciami
Financial reporting nástroje s Excel export/import
Inventory management s realtime vyhľadávaním
CRM systémy s pokročilými validáciami

📊 Reporting nástroje

Komplexné validácie dát s custom pravidlami
Excel export/import funkcionalita s copy/paste
Realtime search a filtering s zebra rows
Audit logging všetkých operácií

🔍 Debugging a monitoring

Kompletné logovanie všetkých operácií (UI + dáta)
Performance monitoring s metrikami
Error tracking s context informáciami
User action audit trail s timestamp

🏗️ Responzívne WinUI3 aplikácie

DataGrid s resize columns, scroll support
ValidAlerts stretching pre rôzne veľkosti okien
Individual colors namiesto pevných tém
Throttling pre smooth performance


🐛 Troubleshooting
❓ DataGrid sa nezobrazuje
csharp// ✅ Skontroluj či je zavolaná InitializeAsync
await DataGridControl.InitializeAsync(columns, rules, throttling, 15, colors);

// ✅ Skontroluj či máš správne XAML
<grid:AdvancedDataGrid x:Name="DataGridControl"/>
❓ LoggerComponent nefunguje
csharp// ✅ Skontroluj permissions na priečinok
var logDir = @"C:\Logs";
if (!Directory.Exists(logDir))
    Directory.CreateDirectory(logDir);

// ✅ LoggerComponent vyžaduje externý ILogger
var logger = new LoggerComponent(yourILogger, logDir, "app.log", 10);
❓ Validácie nefungujú
csharp// ✅ Skontroluj názvy stĺpcov v pravidlách
ValidationRule.Required("Name", "Required") // "Name" musí existovať v columns
❓ Resize/Scroll nefunguje
csharp// ✅ Skontroluj či je DataGrid v ScrollViewer alebo podobnom kontajneri
// ✅ ValidAlerts stretching funguje iba ak má DataGrid dostatok miesta
❓ Logovanie nefunguje
csharp// ✅ Skontroluj či si poslal logger do komponentu
var dataGrid = new AdvancedDataGrid(yourLogger); // NIE new AdvancedDataGrid()

// ✅ Skontroluj log level nastavenia
yourLogger.LogInformation("Test message"); // Testuj či logger funguje

🔧 Konfigurácia
DataGrid Performance
csharp// Rýchla konfigurácia
var fastConfig = ThrottlingConfig.Fast;

// Pomalá konfigurácia  
var slowConfig = ThrottlingConfig.Slow;

// Bez throttling
var noThrottling = ThrottlingConfig.NoThrottling;

// Custom konfigurácia
var customConfig = new ThrottlingConfig
{
    ValidationDebounceMs = 200,
    UIUpdateDebounceMs = 50,
    SearchDebounceMs = 300,
    EnableRealtimeValidation = true
};
Individual Colors
csharp// Light theme s zebra
var lightTheme = DataGridColorConfig.Light;

// Dark theme s zebra
var darkTheme = DataGridColorConfig.Dark;

// Custom colors
var customColors = new DataGridColorConfig
{
    CellBackgroundColor = Colors.White,
    AlternateRowColor = Color.FromArgb(20, 0, 120, 215), // Zebra
    ValidationErrorColor = Colors.Red,
    HeaderBackgroundColor = Colors.LightGray
};

📈 Changelog
v1.0.4 (Current)

✅ NOVÉ: Column resize podporuje myš ťahanie
✅ NOVÉ: Horizontálny + vertikálny scroll support
✅ NOVÉ: ValidAlerts stĺpec stretching na koniec
✅ NOVÉ: Responzívny dizajn pre rôzne veľkosti okien
✅ NOVÉ: Priame metódy pre LoggerComponent (Info, Debug, Warning, Error)
✅ OPRAVENÉ: Všetky XAML/reference chyby
✅ OPRAVENÉ: Kompletné logovanie pre debugging

v1.0.0 (Initial)

✅ NOVÉ: Multi-component package
✅ NOVÉ: AdvancedWinUiDataGrid s Auto-Add, Search/Sort/Zebra, Individual Colors
✅ NOVÉ: LoggerComponent s rotáciou súborov
✅ NOVÉ: Nezávislé komponenty s abstractions logging
✅ Clean PUBLIC API - iba 7 PUBLIC tried


🤝 Contributing
Development Setup

Visual Studio 2022 s WinUI 3 workload
.NET 8.0 SDK
Windows 10 SDK (19041 alebo vyšší)

Code Style

PUBLIC API - iba najnutnejšie triedy a metódy
INTERNAL - všetka implementačná logika
Nezávislé komponenty - žiadne cross-dependencies
Logging abstractions only - žiadne konkrétne implementácie

Testing
csharp// Demo aplikácia slúži ako test harness
await dataGrid.InitializeAsync(...);
await dataGrid.LoadDataAsync(...);
var isValid = await dataGrid.ValidateAllRowsAsync();
