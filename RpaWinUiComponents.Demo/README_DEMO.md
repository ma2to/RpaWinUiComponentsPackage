# RpaWinUiComponents.AdvancedWinUiDataGrid

🚀 **Profesionálny WinUI3 DataGrid komponent s pokročilými funkciami pre dátové aplikácie**

[![NuGet](https://img.shields.io/nuget/v/RpaWinUiComponents.AdvancedWinUiDataGrid.svg)](https://www.nuget.org/packages/RpaWinUiComponents.AdvancedWinUiDataGrid/)
[![Downloads](https://img.shields.io/nuget/dt/RpaWinUiComponents.AdvancedWinUiDataGrid.svg)](https://www.nuget.org/packages/RpaWinUiComponents.AdvancedWinUiDataGrid/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🎯 Účel balíka

Tento balík poskytuje vysoko optimalizovaný DataGrid komponent pre WinUI3 aplikácie s podporou:

- ✅ **Dynamického generovania stĺpcov**
- ✅ **Realtime validácií s throttling**
- ✅ **Copy/Paste Excel funkcionalite**
- ✅ **Custom validačných pravidiel pre mazanie riadkov** ⭐ **NOVÉ**
- ✅ **Optimalizácie pre veľké datasety**
- ✅ **Profesionálneho clean API**

## 🚀 Quick Start

### 1. Inštalácia

```bash
# Package Manager Console
Install-Package RpaWinUiComponents.AdvancedWinUiDataGrid

# .NET CLI
dotnet add package RpaWinUiComponents.AdvancedWinUiDataGrid

# PackageReference
<PackageReference Include="RpaWinUiComponents.AdvancedWinUiDataGrid" Version="1.0.0" />
```

### 2. Základné použitie

```xml
<!-- MainWindow.xaml -->
<Window x:Class="YourApp.MainWindow"
        xmlns:grid="using:RpaWinUiComponents.AdvancedWinUiDataGrid">
    
    <Grid>
        <grid:AdvancedDataGrid x:Name="DataGridControl"/>
    </Grid>
</Window>
```

```csharp
// MainWindow.xaml.cs
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
```

## ⭐ NOVÁ FUNKCIONALITA: Custom Validačné Mazanie Riadkov

### 🎯 DeleteRowsByCustomValidationAsync

Najnovšia pridaná funkcionalita umožňuje mazať riadky na základe custom validačných pravidiel.

```csharp
// Definuj custom validačné pravidlá pre mazanie
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
```

### 📋 Pokročilé príklady custom delete validácií

```csharp
// Zložitejšie podmienky
var advancedDeleteRules = new List<ValidationRule>
{
    // Zmaž riadky s nevalidným email formátom
    ValidationRule.Custom("Email", value =>
    {
        var email = value?.ToString() ?? "";
        return !string.IsNullOrWhiteSpace(email) && !email.Contains("@");
    }, "Invalid email format"),

    // Zmaž riadky kde meno obsahuje "test" (case insensitive)
    ValidationRule.Custom("Name", value =>
    {
        var name = value?.ToString() ?? "";
        return name.Contains("test", StringComparison.OrdinalIgnoreCase);
    }, "Test data removed"),

    // Zmaž riadky kde ID je párne číslo
    ValidationRule.Custom("ID", value =>
    {
        if (int.TryParse(value?.ToString(), out var id))
            return id % 2 == 0; // párne čísla
        return false;
    }, "Even ID removed"),

    // Kombinované podmienky - zmaž ak je vek < 18 A zároveň plat > 1000
    ValidationRule.Custom("Age", value =>
    {
        // Poznámka: Pre kombinované podmienky je lepšie použiť komplexnejšiu logiku
        // alebo viacero samostatných pravidiel
        return false; // Placeholder
    }, "Complex condition")
};

await DataGridControl.DeleteRowsByCustomValidationAsync(advancedDeleteRules);
```

## 🔧 Kompletné API Reference

### Inicializácia

```csharp
Task InitializeAsync(
    List<ColumnDefinition> columns,
    List<ValidationRule> validationRules,
    ThrottlingConfig throttling,
    int emptyRowsCount = 15
);
```

### Dátové operácie

```csharp
// Načítanie dát
Task LoadDataAsync(List<Dictionary<string, object?>> data);
Task LoadDataAsync(DataTable dataTable);

// Export dát  
Task<DataTable> ExportToDataTableAsync();

// Mazanie dát
Task ClearAllDataAsync();
Task DeleteRowsByCustomValidationAsync(List<ValidationRule> deleteRules); // NOVÉ ⭐
```

### Validácie

```csharp
// Validácia všetkých riadkov
Task<bool> ValidateAllRowsAsync();
```

## 📊 Konfigurácia Stĺpcov

```csharp
var columns = new List<ColumnDefinition>
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
        Header = "📧 Email",
        DisplayFormat = "{0:toLower}" // Custom formátovanie
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
```

## ✅ Validačné Pravidlá

### Predpripravené validácie

```csharp
var validationRules = new List<ValidationRule>
{
    // Základné validácie
    ValidationRule.Required("Name", "Name is required"),
    ValidationRule.Email("Email", "Invalid email format"),
    ValidationRule.Range("Age", 18, 100, "Age must be 18-100"),
    ValidationRule.MinLength("Name", 3, "Name too short"),
    ValidationRule.MaxLength("Name", 50, "Name too long"),
    ValidationRule.Pattern("Phone", @"^\d{10}$", "Invalid phone format"),
    
    // Extension metódy
    "Name".Required("Name is required"),
    "Email".Email("Invalid email"),
    "Age".Range(18, 100, "Age 18-100 required"),
    "Salary".IsPositive("Salary must be positive")
};
```

### Custom validácie

```csharp
var customRules = new List<ValidationRule>
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
    }, "Password must be 8+ chars with upper, lower, and digit"),
    
    // Validácia závislá na iných poliach (pokročilé)
    ValidationRule.Custom("ConfirmPassword", value =>
    {
        // Poznámka: Pre cross-field validácie je potrebné rozšíriť API
        return true; // Placeholder
    }, "Passwords must match")
};
```

## ⚙️ Throttling Konfigurácia

```csharp
// Predpripravené konfigurácie
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
    EnableValidationThrottling = true
};
```

## 🖱️ Navigácia a Ovládanie

### Klávesové skratky

| Klávesa | Akcia |
|---------|-------|
| **Tab** | Ďalšia bunka + potvrdenie zmien |
| **Shift+Tab** | Predchádzajúca bunka + potvrdenie |
| **Enter** | Bunka o riadok nižšie + potvrdenie |
| **Esc** | Zrušenie zmien + výskok z bunky |
| **Shift+Enter** | Nový riadok v bunke (multiline) |
| **Ctrl+C** | Kopírovanie označených buniek |
| **Ctrl+V** | Vloženie z clipboardu |
| **Ctrl+X** | Vystrihávacie označených buniek |

### Excel funkcionalita

- ✅ Copy/Paste medzi Excel a DataGrid
- ✅ Zachovanie formátovania
- ✅ Multiline text support
- ✅ Automatické parsovanie typov

## 🎨 Špeciálne Stĺpce

### DeleteRows stĺpec

```csharp
// Automaticky sa vytvorí ak pridáš stĺpec s názvom "DeleteRows"
new("DeleteRows", typeof(string)) { Width = 40, Header = "🗑️" }
```

- Zobrazuje ikonku krížika
- Maže obsah riadku (nie fyzické zmazanie)
- Automaticky kompaktuje riadky

### ValidAlerts stĺpec

- Automaticky sa pridáva na koniec tabuľky
- Zobrazuje validačné chyby pre daný riadok
- Formát: `"ColumnName: Error message; OtherColumn: Other error"`

## 🔍 Validačný Systém

### Realtime validácie

- Validácia sa spúšťa pri každej zmene (throttling 300ms default)
- Validuje sa iba na riadkoch ktoré NIE SÚ úplne prázdne
- Červené orámovanie nevalidných buniek
- Žiadne tooltips - len vizuálna indikácia

### Riadok je považovaný za prázdny ak:

```csharp
// Všetky bunky (okrem DeleteRows a ValidAlerts) sú null alebo prázdne
bool isEmpty = row.Cells
    .Where(c => c.ColumnName != "DeleteRows" && c.ColumnName != "ValidAlerts")
    .All(c => c.Value == null || string.IsNullOrWhiteSpace(c.Value?.ToString()));
```

## 📊 Export Funkcionalita

```csharp
// Export všetkých dát (bez DeleteRows, s ValidAlerts)
DataTable allData = await DataGridControl.ExportToDataTableAsync();

// Export iba validných riadkov
DataTable validData = await DataGridControl.ExportValidRowsOnlyAsync();

// Export špecifických stĺpcov
DataTable specificData = await DataGridControl.ExportSpecificColumnsAsync(
    new[] { "Name", "Email", "Salary" }
);

// Export do CSV
string csvContent = await DataGridControl.ExportToCsvAsync(includeHeaders: true);

// Export štatistiky
ExportStatistics stats = await DataGridControl.GetExportStatisticsAsync();
Console.WriteLine($"Valid rows: {stats.ValidRows}, Invalid: {stats.InvalidRows}");
```

## 🛠️ Pokročilé Používanie

### Memory Management

```csharp
// Automatické cleanup
await DataGridControl.ClearAllDataAsync(); // Fyzicky vymaže dáta z pamäte

// Manual cleanup
DataGridControl.Dispose(); // IDisposable implementácia
```

### Performance optimalizácie

- **Virtualizácia UI** - iba viditeľné bunky v DOM
- **Lazy loading** - dáta sa načítavajú postupne
- **Throttling validácií** - debounce 300ms default
- **Batch operations** - 50 items per batch default
- **Memory pooling** - reuse objektov
- **Background validation** - non-critical validácie

### Dependency Injection

```csharp
// Balík používa Microsoft.Extensions.DependencyInjection
services.AddSingleton<IValidationService, ValidationService>();
services.AddSingleton<IDataManagementService, DataManagementService>();
services.AddSingleton<ICopyPasteService, CopyPasteService>();
services.AddTransient<IExportService, ExportService>();
```

## 🐛 Troubleshooting

### Časté problémy

**Q: DataGrid sa nezobrazuje**
```csharp
// A: Skontroluj či je zavolaná InitializeAsync
await DataGridControl.InitializeAsync(columns, rules, throttling, 15);
```

**Q: Validácie nefungujú**
```csharp
// A: Skontroluj či sú definované validačné pravidlá pre správne názvy stĺpcov
var rules = new List<ValidationRule>
{
    ValidationRule.Required("Name", "Name is required") // "Name" musí existovať v columns
};
```

**Q: Copy/Paste nefunguje**
```csharp
// A: Skontroluj povolenia aplikácie pre prístup k clipboardu
// V Package.appxmanifest pridaj:
<Capability Name="clipboardRead" />
```

**Q: Performance problémy s veľkými datasetmi**
```csharp
// A: Použij PerformanceCritical throttling
var throttling = ThrottlingConfig.PerformanceCritical;
await DataGridControl.InitializeAsync(columns, rules, throttling, 15);
```

## 📋 Príklady Použitia

### Employee Management

```csharp
var columns = new List<ColumnDefinition>
{
    new("ID", typeof(int)) { Header = "👤 ID", MinWidth = 60 },
    new("FirstName", typeof(string)) { Header = "📝 First Name", MinWidth = 120 },
    new("LastName", typeof(string)) { Header = "📝 Last Name", MinWidth = 120 },
    new("Email", typeof(string)) { Header = "📧 Email", MinWidth = 200 },
    new("Department", typeof(string)) { Header = "🏢 Department", MinWidth = 150 },
    new("Salary", typeof(decimal)) { Header = "💰 Salary", MinWidth = 120 },
    new("HireDate", typeof(DateTime)) { Header = "📅 Hire Date", MinWidth = 120 },
    new("DeleteRows", typeof(string)) { Width = 40 }
};

var validationRules = new List<ValidationRule>
{
    ValidationRule.Required("FirstName", "First name is required"),
    ValidationRule.Required("LastName", "Last name is required"),
    ValidationRule.Email("Email", "Invalid email format"),
    ValidationRule.Required("Department", "Department is required"),
    ValidationRule.Range("Salary", 20000m, 200000m, "Salary must be 20k-200k"),
    
    // Custom validácia pre hire date
    ValidationRule.Custom("HireDate", value =>
    {
        if (DateTime.TryParse(value?.ToString(), out var date))
            return date <= DateTime.Now && date >= DateTime.Now.AddYears(-50);
        return false;
    }, "Invalid hire date")
};

// Custom delete pre ukončených zamestnancov
var deleteRules = new List<ValidationRule>
{
    // Zmaž zamestnancov starších ako 65 rokov
    ValidationRule.Custom("HireDate", value =>
    {
        if (DateTime.TryParse(value?.ToString(), out var hireDate))
        {
            var age = DateTime.Now.Year - hireDate.Year;
            return age > 65;
        }
        return false;
    }, "Retirement age reached"),
    
    // Zmaž zamestnancov s veľmi nízkym platom (možno neaktívni)
    ValidationRule.Custom("Salary", value =>
    {
        if (decimal.TryParse(value?.ToString(), out var salary))
            return salary < 15000m;
        return false;
    }, "Below minimum wage")
};

await DataGridControl.DeleteRowsByCustomValidationAsync(deleteRules);
```

## 🔗 Odkazy

- 📦 [NuGet Package](https://www.nuget.org/packages/RpaWinUiComponents.AdvancedWinUiDataGrid/)
- 📖 [Dokumentácia](https://docs.rpawininui.com/datagrid)
- 🐛 [Issues & Bug Reports](https://github.com/company/rpa-winui-components/issues)
- 💡 [Feature Requests](https://github.com/company/rpa-winui-components/discussions)

## 🤝 Prispievanie

Chcete prispieť? Skvelé! 

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## 📄 Licencia

Tento projekt je licencovaný pod MIT License - pozri [LICENSE](LICENSE) súbor pre detaily.

## 🎉 Changelog

### v1.0.0 (2025-01-xx)
- ✅ Initial release
- ✅ Dynamic column generation
- ✅ Realtime validations
- ✅ Copy/Paste Excel functionality
- ✅ **NEW: DeleteRowsByCustomValidationAsync method** ⭐
- ✅ Performance optimizations
- ✅ Professional clean API

---

**Vyvinuté s ❤️ pre WinUI3 komunitu**

*Pre ďalšie otázky alebo podporu kontaktujte vývojársky tím.*