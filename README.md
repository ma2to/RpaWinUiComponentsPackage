# RpaWinUiComponentsPackage - AdvancedWinUiDataGrid

Profesionálny WinUI3 komponent pre dynamické tabuľky s pokročilými funkciami pre dátové aplikácie.

## 🎯 Účel balíka

Tento balík poskytuje vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie s podporou:
- Dynamického generovania stĺpcov
- Realtime validácií
- Copy/Paste Excel funkcionalite
- Optimalizácie pre veľké datasety
- Profesionálneho API

## 🏗️ Štruktúra balíka

```
RpaWinUiComponentsPackage/                               # Solution
│
├── AdvancedWinUiDataGrid/                               # Hlavný projekt balíka
│   ├── Controls/                                        # UI Kontroly
│   │   ├── AdvancedDataGrid.xaml                        # Hlavný UserControl
│   │   ├── AdvancedDataGrid.xaml.cs                     # Code-behind
│   │   ├── DataGridCell.xaml                            # Bunka tabuľky
│   │   ├── DataGridCell.xaml.cs                         # Cell logic
│   │   └── SpecialColumns/                              # Špeciálne stĺpce
│   │       ├── DeleteRowColumn.xaml                     # Delete stĺpec
│   │       ├── DeleteRowColumn.xaml.cs
│   │       ├── ValidationAlertsColumn.xaml              # Validačný stĺpec
│   │       └── ValidationAlertsColumn.xaml.cs
│   │
│   ├── Models/                                          # Dátové modely
│   │   ├── ColumnDefinition.cs                          # Definícia stĺpca
│   │   ├── ValidationRule.cs                            # Validačné pravidlá
│   │   ├── CellData.cs                                  # Dáta bunky
│   │   ├── GridConfiguration.cs                         # Konfigurácia gridu
│   │   └── ThrottlingConfig.cs                          # Throttling nastavenia
│   │
│   ├── Services/                                        # Business logika
│   │   ├── Interfaces/                                  # Rozhrania
│   │   │   ├── IValidationService.cs                    # Validačné služby
│   │   │   ├── IDataManagementService.cs               # Správa dát
│   │   │   ├── ICopyPasteService.cs                    # Copy/Paste
│   │   │   ├── IExportService.cs                       # Export funkcionalita
│   │   │   └── ILoggingService.cs                      # Logovanie
│   │   │
│   │   ├── ValidationService.cs                        # Implementácia validácií
│   │   ├── DataManagementService.cs                    # Správa dátových operácií
│   │   ├── CopyPasteService.cs                         # Excel copy/paste
│   │   ├── ExportService.cs                            # Export do DataTable
│   │   └── NavigationService.cs                        # Tab/Enter navigácia
│   │
│   ├── Utilities/                                       # Pomocné triedy
│   │   ├── DataTypeConverter.cs                        # Konverzia typov
│   │   ├── ExcelClipboardHelper.cs                     # Excel formát helper
│   │   ├── ThrottleHelper.cs                           # Throttling logika
│   │   └── ResourceCleanupHelper.cs                    # Cleanup resources
│   │
│   ├── Extensions/                                      # Extension metódy
│   │   ├── ColumnDefinitionExtensions.cs               # Rozšírenia pre stĺpce
│   │   └── ValidationExtensions.cs                     # Validačné rozšírenia
│   │
│   └── AdvancedWinUiDataGrid.csproj                    # Projekt súbor
│
├── RpaWinUiComponents.Demo/                             # Demo aplikácia
│   ├── MainWindow.xaml                                 # Test okno
│   ├── MainWindow.xaml.cs                              # Test kód
│   └── RpaWinUiComponents.Demo.csproj                 # Demo projekt
│
└── README.md                                           # Tento súbor
```

## 🚀 Kľúčové funkcie

### 1. Dynamické stĺpce
```csharp
var columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
    new("Name", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Name" },
    new("DeleteRows", typeof(string)) { Width = 40 } // Špeciálny delete stĺpec
};
```

### 2. Validačné pravidlá
```csharp
var validationRules = new List<ValidationRule>
{
    ValidationRule.Required("Name", "Name is required"),
    ValidationRule.Email("Email", "Invalid email format"),
    ValidationRule.Range("Age", 18, 100, "Age must be 18-100")
};
```

### 3. Profesionálne API
```csharp
// Inicializácia
await dataGrid.InitializeAsync(columns, validationRules, throttlingConfig, emptyRowsCount: 15);

// Načítanie dát
await dataGrid.LoadDataAsync(dataList);

// Validácia všetkých riadkov
bool isValid = await dataGrid.ValidateAllRowsAsync();

// Export do DataTable
DataTable exportedData = await dataGrid.ExportToDataTableAsync();

// Vymazanie všetkých dát
await dataGrid.ClearAllDataAsync();
```

### 4. Klávesové skratky
- **Tab**: Ďalšia bunka + potvrdenie zmien
- **Enter**: Bunka o riadok nižšie + potvrdenie
- **Esc**: Zahodenie zmien + výskok z bunky
- **Shift+Enter**: Nový riadok v bunke

### 5. Resource Management & Cleanup
```csharp
// Automatické uvoľňovanie zdrojov
await dataGrid.ClearAllDataAsync(); // Fyzicky vymaže dáta z pamäte
dataGrid.Dispose(); // IDisposable implementácia

// Memory management
- Weak references pre event handlery
- Automatic cleanup pri ClearAllDataAsync()
- Garbage collection optimization
- Memory pooling pre častý reuse objektov
```

### 6. Validačný systém - detailne
```csharp
// Realtime validácia - OPRAVENÉ
- Validácia sa spúšťa IHNEĎ pri písaní (throttling 300ms default)
- Validuje sa na RIADKOCH ktoré NIE SÚ úplne prázdne
- Ak je v riadku čo i len JEDNA bunka vyplnená → validujú sa VŠETKY bunky v tom riadku (pre ktoré existujú pravidlá)
- Ak je celý riadok prázdny → žiadna validácia sa nespúšťa
- Špeciálne stĺpce (DeleteRows, ValidAlerts) sa do "prázdnosti" riadku NEZAPOČÍTAVAJÚ
- Červené orámovanie nevalidných buniek (bez tooltipov)

// Príklad:
// Riadok: [ID="", Meno="", Email=""] → PRÁZDNY riadok, žiadna validácia
// Riadok: [ID="", Meno="Ján", Email=""] → NEPRÁZDNY riadok, validujú sa ID, Meno, Email
// Riadok: [ID="1", Meno="", Email=""] → NEPRÁZDNY riadok, validujú sa ID, Meno, Email

// Custom validačné pravidlá
ValidationRule.Custom("StĺpecNázov", (value) => 
{
    // Vlastná validačná logika
    return value != null && value.ToString().Length > 5;
}, "Text ktorý sa zobrazí pri chybe");

// Formát chybových správ v ValidAlerts stĺpci (oddelené ";"):
"StĺpecNázov: Text ktorý sa zobrazí pri chybe; InýStĺpec: Ďalšia chyba"
```

### 7. Bunka ako základná jednotka
```csharp
// Nie riadok, nie stĺpec - BUNKA je hlavný element
- Každá bunka má vlastné dáta, validáciu, formatting
- Bunka má dynamickú výšku podľa obsahu
- Multiline text support s Shift+Enter
- Individual cell selection & editing
- Cell-level validation s realtime feedback
```

### 8. API Design - Clean & Simple (všetky metódy začínajú bez prefixa)
```csharp
// Verejné API - iba to čo používateľ potrebuje
namespace AdvancedWinUiDataGrid
{
    public class AdvancedDataGrid : UserControl 
    {
        // Initialization
        Task InitializeAsync(List<ColumnDefinition> columns, 
                           List<ValidationRule> validationRules, 
                           ThrottlingConfig throttling, 
                           int emptyRowsCount);
        
        // Data Operations  
        Task LoadDataAsync(List<Dictionary<string, object?>> data);
        Task LoadDataAsync(DataTable dataTable);
        Task ClearAllDataAsync();
        
        // Validation
        Task<bool> ValidateAllRowsAsync();
        
        // Export/Import
        Task<DataTable> ExportToDataTableAsync();
        
        // Copy/Paste (automaticky funguje s Ctrl+C/Ctrl+V/Ctrl+X)
        // NavigationService (automaticky funguje s Tab/Enter/Esc/Shift+Enter)
    }
    
    // Models - iba tieto sú verejné
    public class ColumnDefinition { ... }
    public class ValidationRule { ... }  
    public class ThrottlingConfig { ... }
}

// SKRYTÉ od používateľa:
- Všetky Services (IValidationService, IDataManagementService, ...)
- Všetky Utilities  
- Všetky internal Models
- Cell implementation details
```

### 9. Štruktúra projektu - Best Practices
```csharp
// Dependency Injection Container
services.AddSingleton<IValidationService, ValidationService>();
services.AddSingleton<IDataManagementService, DataManagementService>();
services.AddSingleton<ICopyPasteService, CopyPasteService>();
services.AddTransient<IExportService, ExportService>();
services.AddSingleton<ILogger>(provider => ...); // Microsoft.Extensions.Logging.Abstraction

// Interfaces pre testovateľnosť
- Každá služba má interface s "I" prefixom
- Dependency injection ready
- Mock-friendly pre unit testy
- Clean separation of concerns
```

### 10. ItemsRepeater Implementation
```csharp
// Tabuľka postavená na ItemsRepeater
- Každý riadok = ItemsRepeater item
- Každá bunka = DataTemplate v rámci riadku  
- Virtualizácia pre performance
- Custom layout pre responsive design
- Dynamic column generation
```

### 11. Optimalizácie pre veľké datasety
```csharp
// Performance optimizations
- Virtualizácia UI (iba viditeľné bunky v DOM)
- Lazy loading (dáta sa načítavajú postupne)
- Throttling validácií (300ms debounce)
- Batch operations (50 items per batch default)
- Memory pooling pre objekty
- Weak event handlers (prevent memory leaks)
- Background validation pre non-critical validácie
```

## 🎨 Špeciálne stĺpce

### DeleteRows stĺpec
- Automaticky sa vytvorí ak je v headers názov "DeleteRows"
- Obsahuje ikonku krížika
- Maže obsah riadku a posúva ostatné riadky nahor

### ValidAlerts stĺpec  
- Vždy prítomný na konci tabuľky
- Zobrazuje validačné chyby pre daný riadok
- Červené orámovanie nevalidných buniek

## 🔧 Technické požiadavky

- **.NET 8**
- **WinUI3**
- **Windows 10/11** (64-bit aj 32-bit support)
- **ItemsRepeater** ako základ tabuľky

## ⚡ Optimalizácie

- **Throttling** pre validácie a UI updates
- **Lazy loading** pre veľké datasety  
- **Resource cleanup** - automatické uvoľňovanie pamäte
- **Realtime validácie** iba na zmenených dátach
- **Virtualizácia** pre výkon s veľkými tabuľkami

## 🧩 Dependency Injection

Balík používa DI kontajner pre:
- `IValidationService` - Validačné služby
- `IDataManagementService` - Správa dát
- `ICopyPasteService` - Copy/Paste funkcionalita
- `IExportService` - Export operácie
- `ILoggingService` - Logovanie (abstrakcia)

## 📋 Podporované dátové typy

- `string`
- `int`, `long`, `decimal`, `double`
- `DateTime`
- `bool`
- Custom typy cez converter

## 🔍 Validačné typy

- **Required** - Povinné pole
- **Email** - Email formát
- **Range** - Číselný rozsah
- **MinLength/MaxLength** - Dĺžka textu
- **Custom** - Vlastné validačné pravidlá

---

## 🔧 Detailné špecifikácie

### Throttling konfigurácia
```csharp
public class ThrottlingConfig
{
    public int ValidationDebounceMs { get; set; } = 300;      // Default 300ms pre validácie
    public int UIUpdateDebounceMs { get; set; } = 100;       // Default 100ms pre UI updates
    public int BatchSize { get; set; } = 50;                // Batch size pre bulk operácie
    public bool EnableValidationThrottling { get; set; } = true;
    public bool EnableUIThrottling { get; set; } = true;
}
```

### Validačné pravidlá - Custom prístup
```csharp
// Hlavný dôraz na custom validácie
var validationRules = new List<ValidationRule>
{
    ValidationRule.Custom("Meno", value => !string.IsNullOrEmpty(value?.ToString()), "Meno je povinné"),
    ValidationRule.Custom("Email", value => IsValidEmail(value?.ToString()), "Neplatný email formát"), 
    ValidationRule.Custom("Vek", value => int.Parse(value.ToString()) >= 18 && int.Parse(value.ToString()) <= 100, "Vek musí byť 18-100"),
    ValidationRule.Custom("Plat", value => decimal.Parse(value.ToString()) >= 500 && decimal.Parse(value.ToString()) <= 50000, "Plat musí byť 500-50000")
};

// Formát chybovej správy: "{NázovStĺpca}: {ZadanýText}"
// Príklad: "Vek: Vek musí byť 18-100"
```

### Delete Row mechanizmus
- **NIE** fyzické zmazanie riadku z kolekcie
- **ÁNO** vyčistenie obsahu buniek riadku
- Automatické posunutie ostatných riadkov nahor (bez prázdnych medzier)
- Riadok zostáva v kolekii ale je prázdny

### Podporované dátové typy
**Základné typy:**
- `string`, `int`, `long`, `decimal`, `double`, `float`
- `DateTime`, `DateOnly`, `TimeOnly` 
- `bool`

**Zložitejšie typy:**
- `Nullable<T>` verzie všetkých základných typov
- `Enum` typy
- Custom objekty cez `ToString()` a custom converter
- `object` s automatickou detekciou typu

### Performance špecifikácie
- **Štandardný dataset**: Do 1000 riadkov × 10 stĺpcov
- **Veľký dataset**: Do 5000 riadkov × 20 stĺpcov
- **Maximálny dataset**: Nad 5000 riadkov (môže sa stať)
- **Virtualizácia**: Povinná pre datasety nad 1000 riadkov
- **Lazy loading**: Pre datasety nad 2000 riadkov

### Logging
- **Microsoft.Extensions.Logging.Abstraction**
- Logovanie všetkých kritických operácií
- Debug, Info, Warning, Error levels
- Štruktúrované logovanie s kontextom

### Copy/Paste Excel funkcionalita - detailne
```csharp
// Excel kompatibilný formát (Tab-separated values)
- **Copy (Ctrl+C)**: Viacnásobný výber buniek → Excel TSV formát do clipboardu
- **Paste (Ctrl+V)**: Z Excel clipboard → automatické parsovanie a vyplnenie buniek
- **Cut (Ctrl+X)**: Copy + vymazanie obsahu zdrojových buniek

// Overflow handling pri paste
- Dáta ktoré presahujú posledný stĺpec (nie špeciálny) sa IGNORUJÚ
- Ak je nedostatok riadkov, automaticky sa vytvoria nové

// Paste positioning - príklad:
// Mám 2x2 dáta: ["A1","A2"] ["B1","B2"] 
// Kliknem do bunky riadok 2, stĺpec 2 a dám Ctrl+V
// Vyplní sa:
//   - Riadok 2, Stĺpec 2: "A1"
//   - Riadok 2, Stĺpec 3: "A2"  
//   - Riadok 3, Stĺpec 2: "B1"
//   - Riadok 3, Stĺpec 3: "B2"

// Dáta ktoré by išli za posledný skutočný stĺpec sa zahadia
```

### Klávesová navigácia - detailne
- **Tab**: 
  - Potvrdí zmeny v aktuálnej bunke
  - Presun na ďalšiu bunku v riadku
  - Na konci riadku presun na začiatok ďalšieho riadku
  
- **Enter**:
  - Potvrdí zmeny v aktuálnej bunke  
  - Presun na bunku o riadok nižšie v tom istom stĺpci
  
- **Esc**:
  - Zahodí nepotvrdené zmeny
  - Vráti pôvodnú hodnotu
  - Ukončí edit mode
  
- **Shift+Enter**:
  - Nový riadok v rámci tej istej bunky (multiline text)
  - Automatické zvýšenie výšky bunky/riadku

---

## 🎯 Finálne API Usage Example

```csharp
// 1. Definícia stĺpcov
var columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) { MinWidth = 60, Width = 80, Header = "🔢 ID" },
    new("Meno", typeof(string)) { MinWidth = 120, Width = 150, Header = "👤 Meno" },
    new("Email", typeof(string)) { MinWidth = 200, Width = 200, Header = "📧 Email" },
    new("DeleteRows", typeof(string)) { Width = 40 }, // Špeciálny delete stĺpec
    // ValidAlerts stĺpec sa pridá automaticky na koniec
};

// 2. Custom validácie  
var validationRules = new List<ValidationRule>
{
    ValidationRule.Custom("Meno", value => !string.IsNullOrEmpty(value?.ToString()), "Meno je povinné"),
    ValidationRule.Custom("Email", value => IsValidEmail(value?.ToString()), "Neplatný email formát"),
};

// 3. Throttling konfigurácia (optional)
var throttling = new ThrottlingConfig 
{
    ValidationDebounceMs = 300,
    UIUpdateDebounceMs = 100,
    BatchSize = 50
};

// 4. Inicializácia s 15 prázdnymi riadkami
await dataGrid.InitializeAsync(columns, validationRules, throttling, 15);

// 5. Načítanie dát
var data = new List<Dictionary<string, object?>>
{
    new() { ["ID"] = 1, ["Meno"] = "Ján Novák", ["Email"] = "jan@example.com" },
    new() { ["ID"] = 2, ["Meno"] = "Mária Svoboda", ["Email"] = "maria@company.sk" }
};
await dataGrid.LoadDataAsync(data);

// 6. Validácia všetkých riadkov
bool allValid = await dataGrid.ValidateAllRowsAsync();

// 7. Export do DataTable (bez DeleteRows stĺpca, s ValidAlerts stĺpcom)  
DataTable exportedData = await dataGrid.ExportToDataTableAsync();

// 8. Vyčistenie všetkých dát (uvoľnenie pamäte)
await dataGrid.ClearAllDataAsync();
```

---

**Toto README obsahuje všetky informácie potrebné na implementáciu balíka podľa požiadaviek. Môže slúžiť ako kompletná špecifikácia pre implementáciu v akomkoľvek chate.**