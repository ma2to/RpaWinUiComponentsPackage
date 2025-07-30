# 🚀 RpaWinUiComponentsPackage

**Profesionálny multi-component balík pre WinUI3 aplikácie s pokročilými komponentmi pre enterprise aplikácie**

[![NuGet](https://img.shields.io/nuget/v/RpaWinUiComponentsPackage.svg)](https://www.nuget.org/packages/RpaWinUiComponentsPackage/)
[![Downloads](https://img.shields.io/nuget/dt/RpaWinUiComponentsPackage.svg)](https://www.nuget.org/packages/RpaWinUiComponentsPackage/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🎯 O balíku

Tento balík poskytuje súbor pokročilých WinUI3 komponentov pre enterprise aplikácie s dôrazom na produktivitu, logovanie a dátové operácie.

### 📦 Komponenty v balíku

| Komponent | Účel | Kľúčové funkcie |
|-----------|------|-----------------|
| **AdvancedWinUiDataGrid** | Pokročilý DataGrid pre dátové aplikácie | Auto-Add riadkov, Search/Sort/Zebra, Individual Colors, Excel Copy/Paste |
| **LoggerComponent** | Súborové logovanie s rotáciou | Thread-safe, automatická rotácia, konfigurovateľné |

---

## 🏗️ 1. AdvancedWinUiDataGrid

### 🎯 Účel
Vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie s podporou pokročilých funkcionalít pre dátové aplikácie.

### ✨ Kľúčové funkcie
- ✅ **Auto-Add riadkov** - automaticky pridáva prázdne riadky pri práci s dátami
- ✅ **Individual Colors** - nastav jednotlivé farby namiesto celých tém
- ✅ **Search & Filter** - vyhľadávanie v jednotlivých stĺpcoch
- ✅ **Header Click Sorting** - kliknutím na header sort vzostupne/zostupne
- ✅ **Zebra Rows** - striedavé farby riadkov pre lepšiu čitateľnosť
- ✅ **Realtime validácie** s throttling pre výkon
- ✅ **Copy/Paste Excel** funkcionalitu
- ✅ **Custom validačné pravidlá** pre realtime validaciu
- ✅ **Custom validačné pravidlá pre mazanie riadkov**
- ✅ **Integrovaný LoggerComponent** pre kompletné logovanie

### 🚀 Quick Start - AdvancedWinUiDataGrid

```xml
<!-- MainWindow.xaml -->
<Window x:Class="YourApp.MainWindow"
        xmlns:grid="using:RpaWinUiComponentsPackage.AdvancedWinUiDataGrid"
        xmlns:logger="using:RpaWinUiComponentsPackage.Logger">
    
    <Grid>
        <grid:AdvancedDataGrid x:Name="DataGridControl"/>
    </Grid>
</Window>
```

```csharp
// MainWindow.xaml.cs
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.Logger;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using GridValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ValidationRule;
using GridThrottlingConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using GridDataGridColorConfig = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.DataGridColorConfig;

public sealed partial class MainWindow : Window
{
    private LoggerComponent? _logger;
    
    public MainWindow()
    {
        this.InitializeComponent();
        InitializeDataGrid();
    }

    private async void InitializeDataGrid()
    {
        // 1. Vytvor LoggerComponent
        var tempDir = System.IO.Path.GetTempPath();
        var logFileName = $"DataGrid_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
        _logger = new LoggerComponent(tempDir, logFileName, 10); // 10MB max size

        // 2. Definícia stĺpcov
        var columns = new List<GridColumnDefinition>
        {
            new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
            new("Name", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Name" },
            new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
            new("Age", typeof(int)) { MinWidth = 80, Width = 100, Header = "🎂 Age" },
            new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
        };

        // 3. Validačné pravidlá
        var rules = new List<GridValidationRule>
        {
            GridValidationRule.Required("Name", "Name is required"),
            GridValidationRule.Email("Email", "Invalid email format"),
            GridValidationRule.Range("Age", 18, 100, "Age must be 18-100")
        };

        // 4. Individual Colors
        var colors = new GridDataGridColorConfig
        {
            CellBackgroundColor = Microsoft.UI.Colors.White,
            AlternateRowColor = Color.FromArgb(20, 0, 120, 215), // Zebra effect
            ValidationErrorColor = Microsoft.UI.Colors.Red
        };

        // 5. ⭐ KĽÚČOVÉ: Inicializácia s LoggerComponent integráciou
        await DataGridControl.InitializeAsync(
            columns, 
            rules, 
            GridThrottlingConfig.Default, 
            emptyRowsCount: 5, 
            colors,
            _logger  // 🔗 LoggerComponent integrácia
        );

        // 6. Načítanie dát
        var data = new List<Dictionary<string, object?>>
        {
            new() { ["ID"] = 1, ["Name"] = "John", ["Email"] = "john@test.com", ["Age"] = 30 }
        };
        await DataGridControl.LoadDataAsync(data);
    }
}
```

### 🎨 Individual Colors System

```csharp
// Predpripravené konfigurácie
var lightColors = GridDataGridColorConfig.Light;
var darkColors = GridDataGridColorConfig.Dark;
var blueColors = GridDataGridColorConfig.Blue;

// Custom individual colors
var customColors = new GridDataGridColorConfig
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
```

### 🔥 AUTO-ADD Funkcionalita

```csharp
// Inicializácia s 5 minimálnymi riadkami
await DataGridControl.InitializeAsync(columns, rules, throttling, emptyRowsCount: 5, colors, logger);

// ✅ Ak načítaš 3 riadky dát → bude 5 riadkov (3 s dátami + 2 prázdne)
// ✅ Ak načítaš 10 riadkov dát → bude 11 riadkov (10 s dátami + 1 prázdny)
// ✅ Vyplníš posledný prázdny → automaticky sa pridá nový prázdny
// ✅ Mažeš riadky: nad minimum = fyzicky zmaže, na minimum = len vyčistí obsah
```

### 🔍 Search, Sort & Zebra Rows

```csharp
// Search v stĺpcoch (automaticky integrované v UI)
await DataGridControl.SetColumnSearchAsync("Name", "John");

// Header Click Sorting (automatické v UI)
// Klik na header: None → Ascending → Descending → None

// Zebra Rows (automatické cez Individual Colors)
var zebraConfig = new GridDataGridColorConfig 
{ 
    AlternateRowColor = Color.FromArgb(20, 0, 120, 215) 
};
```

### 📊 Validačné Systém

```csharp
// Predpripravené validácie
var rules = new List<GridValidationRule>
{
    GridValidationRule.Required("Name", "Name is required"),
    GridValidationRule.Email("Email", "Invalid email format"),
    GridValidationRule.Range("Age", 18, 100, "Age must be 18-100"),
    GridValidationRule.MinLength("Name", 3, "Name too short"),
    GridValidationRule.MaxLength("Name", 50, "Name too long"),
    GridValidationRule.Pattern("Phone", @"^\d{10}$", "Invalid phone format")
};

// Custom validácie
var customRules = new List<GridValidationRule>
{
    GridValidationRule.Custom("Username", value =>
    {
        var username = value?.ToString() ?? "";
        return username.Length >= 3 && !username.Contains(" ");
    }, "Username must be 3+ chars without spaces")
};
```

### 🗑️ Custom Delete Validation

```csharp
// Definuj custom validačné pravidlá pre mazanie
var deleteRules = new List<GridValidationRule>
{
    GridValidationRule.Custom("Salary", value =>
    {
        if (decimal.TryParse(value?.ToString(), out var salary))
            return salary > 10000; // TRUE = zmaž riadok
        return false;
    }, "High salary - row deleted"),
    
    GridValidationRule.Custom("Age", value =>
    {
        if (int.TryParse(value?.ToString(), out var age))
            return age > 65; // TRUE = zmaž riadok
        return false;
    }, "Retirement age - row deleted")
};

// Aplikuj delete pravidlá
await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);
```

---

## 📝 2. LoggerComponent

### 🎯 Účel
Thread-safe súborový logger s automatickou rotáciou súborov a konfigurovateľnou veľkosťou, určený pre enterprise aplikácie.

### ✨ Kľúčové funkcie
- ✅ **Thread-safe logovanie** - bezpečné pre concurrent použitie
- ✅ **Automatická rotácia súborov** - pri dosiahnutí max veľkosti
- ✅ **Konfigurovateľná veľkosť** - nastaviteľný limit súborov
- ✅ **Async operácie** - neblokujúce I/O operácie
- ✅ **Flexibilné log levely** - INFO, ERROR, DEBUG, WARN...
- ✅ **Integrácia s DataGrid** - automatické logovanie všetkých operácií
- ✅ **Jednoduchý API** - iba jedna hlavná metóda LogAsync()

### 🚀 Quick Start - LoggerComponent

```csharp
using RpaWinUiComponentsPackage.Logger;

// Základné použitie
public class MyService
{
    private readonly LoggerComponent _logger;
    
    public MyService()
    {
        var logDir = @"C:\Logs";
        var fileName = "MyApp.log";
        var maxSizeMB = 10; // 10MB per file
        
        _logger = new LoggerComponent(logDir, fileName, maxSizeMB);
    }
    
    public async Task DoSomethingAsync()
    {
        await _logger.LogAsync("Operation started", "INFO");
        
        try
        {
            // Vaša logika
            await SomeOperation();
            await _logger.LogAsync("Operation completed successfully", "INFO");
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Operation failed: {ex.Message}", "ERROR");
            throw;
        }
    }
    
    public void Dispose()
    {
        _logger?.Dispose();
    }
}
```

### 🔗 Integrácia LoggerComponent s AdvancedWinUiDataGrid

```csharp
// Vytvor LoggerComponent
var logger = new LoggerComponent(@"C:\Logs", "DataGrid.log", 10);

// Pošli ho do DataGrid pri inicializácii
await dataGrid.InitializeAsync(columns, rules, throttling, 15, colors, logger);

// Teraz sa všetky operácie DataGrid automaticky logujú:
// - UI operácie (click, selection, navigation)
// - Dátové operácie (load, save, delete, validation)
// - Chyby a výnimky
// - Performance metriky
// - Search/Sort/Zebra operácie
```

### 📊 Rotácia súborov

```csharp
// Bez rotácie
var logger = new LoggerComponent(@"C:\Logs", "app.log", 0);
// Vytvorí: app.log (neobmedzená veľkosť)

// S rotáciou (odporúčané)
var logger = new LoggerComponent(@"C:\Logs", "app.log", 5);
// Vytvorí: app_1.log, app_2.log, app_3.log... (každý max 5MB)
```

### 🔍 Diagnostické vlastnosti

```csharp
// Získaj informácie o súčasnom stave
Console.WriteLine($"Current log file: {logger.CurrentLogFile}");
Console.WriteLine($"File size: {logger.CurrentFileSizeMB:F2} MB");
Console.WriteLine($"Rotation files: {logger.RotationFileCount}");
Console.WriteLine($"Rotation enabled: {logger.IsRotationEnabled}");
```

---

## 📦 Inštalácia

```bash
# Package Manager Console
Install-Package RpaWinUiComponentsPackage

# .NET CLI
dotnet add package RpaWinUiComponentsPackage
```

```xml
<!-- PackageReference -->
<PackageReference Include="RpaWinUiComponentsPackage" Version="1.0.0" />
```

---

## 🏗️ Projektová štruktúra

```
RpaWinUiComponentsPackage/
├── AdvancedWinUiDataGrid/              # 📊 DataGrid komponent
│   ├── Controls/                       # UI komponenty
│   │   ├── AdvancedDataGrid.xaml       # Hlavný komponent
│   │   ├── DataGridCell.xaml           # Bunka komponent
│   │   ├── SearchAndSortHeader.xaml    # Search/Sort header
│   │   └── SpecialColumns/             # DeleteRows, ValidAlerts
│   ├── Models/                         # ✅ PUBLIC API triedy
│   │   ├── ColumnDefinition.cs         # Definícia stĺpca
│   │   ├── ValidationRule.cs           # Validačné pravidlá
│   │   ├── ThrottlingConfig.cs         # Performance config
│   │   ├── DataGridColorConfig.cs      # Individual colors
│   │   └── SortDirection.cs            # Sort enum
│   ├── Services/                       # INTERNAL business logika
│   │   ├── DataManagementService.cs    # AUTO-ADD logika
│   │   ├── ValidationService.cs        # Realtime validácie
│   │   ├── SearchAndSortService.cs     # Search/Sort/Zebra
│   │   └── Interfaces/                 # INTERNAL interfaces
│   └── Utilities/                      # INTERNAL helpers
├── LoggerComponent/                    # 📝 Logger komponent
│   ├── Core/                           # ✅ PUBLIC API
│   │   └── LoggerComponent.cs          # Hlavná trieda
│   ├── Configuration/                  # INTERNAL config
│   │   └── LoggerConfiguration.cs      # Konfiguračné triedy
│   └── FileManagement/                 # INTERNAL file ops
│       └── LogFileManager.cs           # Rotácia súborov
└── Common/                             # 🔧 Zdieľané utility
    └── SharedUtilities/                # INTERNAL helpers
        └── Extensions/                 # Extension metódy
```

---

## 🔒 PUBLIC vs INTERNAL API

### ✅ PUBLIC API (použiteľné v aplikácii)

**AdvancedWinUiDataGrid:**
- `AdvancedDataGrid` - hlavný komponent
- `ColumnDefinition` - definícia stĺpca  
- `ValidationRule` - validačné pravidlá
- `ThrottlingConfig` - throttling nastavenia
- `DataGridColorConfig` - individual color configuration
- `SortDirection` - enum pre sorting

**LoggerComponent:**
- `LoggerComponent` - hlavná trieda

### ❌ INTERNAL (nie je dostupné)
- Všetky Services, Models, Utilities
- Všetky Interfaces  
- Všetky Helper triedy
- Všetky implementation detaily

---

## 🔗 Kombinácia komponentov

```csharp
public sealed partial class MainWindow : Window
{
    private LoggerComponent? _logger;
    private AdvancedDataGrid? _dataGrid;
    
    private async void InitializeComponents()
    {
        // 1. Vytvor LoggerComponent
        _logger = new LoggerComponent(@"C:\AppLogs", "MyApp.log", 10);
        await _logger.LogAsync("Application starting...", "INFO");
        
        // 2. Vytvor AdvancedDataGrid s LoggerComponent integráciou
        _dataGrid = this.FindName("DataGridControl") as AdvancedDataGrid;
        await _dataGrid.InitializeAsync(columns, rules, throttling, 15, colors, _logger);
        
        // 3. Teraz sú komponenty prepojené - všetky DataGrid operácie sa logujú
        await _logger.LogAsync("Components initialized and integrated", "INFO");
    }
}
```

---

## 🎯 Use Cases

### 🏢 Enterprise dátové aplikácie
- Employee management systémy
- Financial reporting nástroje  
- Inventory management
- CRM systémy

### 📊 Reporting nástroje
- Komplexné validácie dát
- Excel export/import funkcionalita
- Realtime search a filtering
- Audit logging všetkých operácií

### 🔍 Debugging a monitoring
- Kompletné logovanie všetkých operácií
- Performance monitoring
- Error tracking s context informáciami
- User action audit trail

---

## 🐛 Troubleshooting

### ❓ DataGrid sa nezobrazuje
```csharp
// ✅ Skontroluj či je zavolaná InitializeAsync
await DataGridControl.InitializeAsync(columns, rules, throttling, 15, colors, logger);
```

### ❓ LoggerComponent nefunguje  
```csharp
// ✅ Skontroluj permissions na priečinok
var logDir = @"C:\Logs";
if (!Directory.Exists(logDir))
    Directory.CreateDirectory(logDir);

var logger = new LoggerComponent(logDir, "app.log", 10);
```

### ❓ Validácie nefungujú
```csharp
// ✅ Skontroluj názvy stĺpcov v pravidlách
GridValidationRule.Required("Name", "Required") // "Name" musí existovať v columns
```

---

## 📈 Changelog

### v1.0.0 (2025-01-xx)
- ✅ **NOVÉ**: Multi-component package
- ✅ **NOVÉ**: AdvancedWinUiDataGrid s Auto-Add, Search/Sort/Zebra, Individual Colors
- ✅ **NOVÉ**: LoggerComponent s rotáciou súborov
- ✅ **NOVÉ**: Integrácia LoggerComponent → AdvancedWinUiDataGrid
- ✅ **NOVÉ**: Kompletné logovanie všetkých DataGrid operácií
- ✅ Clean PUBLIC API - iba 7 PUBLIC tried
- ✅ Professional enterprise-ready balík

---

## 📄 Licencia

Tento projekt je licencovaný pod MIT License - pozri [LICENSE](LICENSE) súbor pre detaily.

## 🤝 Prispievanie

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

**Vyvinuté s ❤️ pre WinUI3 komunitu**

*Pre ďalšie otázky alebo podporu kontaktujte vývojársky tím.*