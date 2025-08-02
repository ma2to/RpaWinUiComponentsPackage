# RpaWinUiComponentsPackage

> **Pokročilé WinUI3 komponenty pre .NET Core 8 aplikácie**

## 📦 Obsah balíka

### 1. **AdvancedWinUiDataGrid** ⭐
Pokročilý data grid komponent s komplexnými funkciami pre enterprise aplikácie.

### 2. **LoggerComponent** 📊  
Univerzálny logging komponent s real-time monitoringom a diagnostikou.

---

## 🎯 AdvancedWinUiDataGrid

**Enterprise-grade data grid komponent** s pokročilými funkciami pre prácu s veľkými datasetmi.

### ✅ **Implementované funkcionality**

#### **Základné funkcie**
- 🏗️ **Column Management** - Dynamické stĺpce, resize, header customization
- 📊 **Data Binding** - Auto-add rows, real-time updates, data validation
- 🎨 **UI Theming** - Custom colors, zebra rows, responsive design
- 🔄 **Scroll Synchronization** - Header/data sync, smooth scrolling

#### **Advanced Navigation**
- ⌨️ **Keyboard Navigation** - Arrow keys, Tab/Enter, Shift combinations
- 🖱️ **Mouse Operations** - Click selection, drag selection, context operations
- 📱 **Selection System** - Single/multi-cell, range selection, visual feedback
- 🎯 **Focus Management** - Cell focus, keyboard shortcuts (Ctrl+A, Shift+Arrow)

#### **Copy/Paste System**
- 📋 **Basic Operations** - Ctrl+C/V/X support, Excel compatibility
- 🔄 **Range Operations** - Multi-cell copy/paste s intelligent paste
- 📄 **Format Support** - CSV, TSV, tab-separated clipboard formats
- ✂️ **Cut Operations** - Copy + clear functionality pre ranges
- 🎯 **Smart Detection** - Auto-detects delimiters, adjusts target ranges

#### **Advanced Validation**
- ✅ **Cross-Cell Dependencies** - Validácia závislá od iných buniek v riadku
- 🔗 **Business Logic Rules** - Komplexné business validation s custom functions
- ⚡ **Real-time Validation** - Live feedback počas editácie s debouncing
- 🎛️ **Conditional Chains** - If-then-else validation logic chains
- 📊 **ValidationRuleSet** - Management system pre pravidlá s priority
- 🔄 **Async Support** - Async validation pre external API calls

#### **Search & Filter**
- 🔍 **Basic Search** - Text search, column filtering
- 🎯 **Advanced Filtering** - Multi-column filters s 17 operators
- 📝 **Filter Types** - Contains, Regex, Range, Empty/NotEmpty, In/NotIn
- 🔗 **Logical Operators** - AND/OR kombinovanie filtrov
- 📈 **Sorting** - Multi-column sort, custom comparers
- 🔄 **Real-time Updates** - Live search results

#### **Row Selection & Management**
- ☑️ **CheckBox Column** - Row selection s checkbox-ami (prvý stĺpec ak povolený)
- 🔄 **Check All/Uncheck All** - Tri-state header checkbox pre bulk operácie
- 🗑️ **Bulk Delete** - Zmazanie len označených riadkov
- 📤 **Selective Export** - Export len označených riadkov
- 📥 **Import with Selection** - Import s nastavením checkbox states
- ✅ **Validation Aware** - Kontrola validity respektuje checkbox selection

### ✅ **Kompletne implementované** 
- 🔍 **Advanced Search** - Fuzzy search, regex patterns, search highlighting (100%)
- 🎛️ **Multi-Sort** - Complex sorting scenarios s UI indicators (100%)
- 📁 **Export/Import** - CSV, Excel, JSON support s templates (100%)
- ☑️ **CheckBox Column** - Row selection s Check All/Uncheck All functionality (100%)

### 🚀 **Aktuálne v implementácii**

#### **⚡ Virtual Scrolling Enhancement** (Priority: Vysoká - V IMPLEMENTÁCII)
- ✅ **Horizontal virtualization** - Efektívne zobrazenie stoviek stĺpcov (v progrese)
- ⏳ **Variable row heights** - Riadky s rôznou výškou pre wrapping text
- ⏳ **Smooth scrolling animations** - Plynulé scrollovanie a animácie
- ⏳ **Memory monitoring** - Sledovanie a optimalizácia pamäte pre veľké datasety
- ⏳ **Cell recycling** - Recyklovanie UI elementov pre optimálny výkon
- ⏳ **Viewport management** - Inteligentné renderovanie len viditeľných buniek

### bude sa implementovat
#### **📱 Advanced Selection & Navigation** 
1. **Extended Selection Modes** - Range selection, Multi-range selection, Row/Column header selection, Block selection
2. Custom validation engine - ak sa mysli custom validacie buniek stlpcov a nemam to tak implementuj ak to mam alebo sa mysli nieco ine tak to neimplementuj.
3. **Background Processing** - Async data loading - toto mozes implementovat.
4. **Background Processing** - NEIMPLEMENTUJE SA ale chcem vediet vysvetlenie, o co sa jedna - Background validation

### ❌ **Funkcie ktoré sa NEBUDÚ implementovať** (Používateľ nevybral)

#### **🎨 Pokročilé UI a UX funkcie** - NEIMPLEMENTUJE SA
1. **Cell Templates & Renderers** - Custom cell renderers, ComboBox/DatePicker/Button v bunkách, Image thumbnails, Progress bars, Star ratings, Rich text

2. **Advanced Column Management** - Column pinning, Column grouping, Drag & drop reordering, Auto-sizing, Column templates

3. **Enhanced Row Operations** - Row grouping, Row templates, Context menus, Row drag & drop

4. **Advanced Navigation** - Extended keyboard shortcuts, Jump to row/column, Find & Go To, Bookmarks, Breadcrumb navigation

#### **🔍 Enhanced Search & Filter** - NEIMPLEMENTUJE SA
5. **Advanced Search Panel** - Visual search builder, Drag & drop queries, Saved search queries, Search history, Multi-column search

6. **Smart Filtering** - Auto-complete suggestions, Filter by selection, Quick filter toolbar, Filter presets, Visual filter indicators

#### **📊 Data Analysis Features** - NEIMPLEMENTUJE SA
7. **Built-in Analytics** - Summary row, Aggregations, Column statistics panel, Data quality indicators, Outlier detection

8. **Data Transformation** - In-line formulas, Calculated columns, Dependencies, Data type detection, Bulk operations

#### **🔐 Enterprise Features** - NEIMPLEMENTUJE SA
9. **Advanced Security** - Cell-level permissions, Role-based visibility, Data masking, Audit trail, Digital signatures

10. **Compliance & Governance** - Data lineage, Approval workflows, Compliance reporting, Retention policies, GDPR compliance

#### **🧪 Developer Experience** - NEIMPLEMENTUJE SA
11. **API & Extensibility** - Plugin architecture, Event system, REST API, Configuration management

12. **Testing & Debugging** - Data generation, Performance profiler, Accessibility tester, Automated UI tests

#### **📱 Modern UX Patterns** - NEIMPLEMENTUJE SA
13. **Progressive Web App Features** - Offline functionality, Data synchronization, Push notifications, Service worker caching, Mobile-first design

14. **Collaboration Features** - Real-time multi-user editing, Comment system, @mentions, Change suggestions, Live cursors

#### **♿ Accessibility & Usability** - NEIMPLEMENTUJE SA
15. **Screen Reader Support** - ARIA labels, Keyboard navigation, Audio feedback, High contrast mode

16. **Internationalization** - Multi-language UI, RTL support, Date/number localization, Cultural sorting

#### **🖨️ Reporting & Export Extensions** - NEIMPLEMENTUJE SA
17. **Advanced Print Support** - Page layout designer, Headers/footers, Print preview, PDF generation, Multi-page tables

18. **Enhanced Export Features** - Excel templates, Chart integration, Email integration, Scheduled exports, Custom formats

#### **⚡ Performance & Scalability** - ČIASTOČNE IMPLEMENTOVANÉ
19. **Background Processing** - NEIMPLEMENTUJE SA ( Background validation, Auto-save, Undo/Redo)

#### **♿ Accessibility & Usability** (Priority: Vysoká)

20. **Screen Reader Support**
    - **ARIA labels** - Kompletné označenie pre screen readery
    - **Keyboard navigation** - Plná funkcionalita bez myši
    - **Audio feedback** - Zvukové signály pre akcie
    - **High contrast mode** - Podpora pre zrakovo postihnutých

21. **Internationalization**
    - **Multi-language UI** - Rozhranie v rôznych jazykoch
    - **RTL support** - Podpora Right-to-Left jazykov (arabčina, hebrejčina)
    - **Date/number localization** - Formátovanie podľa lokality
    - **Cultural sorting** - Triedenie podľa miestnych pravidiel

#### **🖨️ Reporting & Export Extensions** (Priority: Stredná)

22. **Advanced Print Support**
    - **Page layout designer** - WYSIWYG editor pre tlač
    - **Headers/footers** - Vlastné hlavičky a pätičky
    - **Print preview** - Náhľad pred tlačou
    - **PDF generation** - Priamy export do PDF
    - **Multi-page tables** - Správne delenie na stránky

23. **Enhanced Export Features**
    - **Excel templates** - Export s formátovaním a formulami
    - **Chart integration** - Embedded grafy v exportoch
    - **Email integration** - Priame odoslanie exportov emailom
    - **Scheduled exports** - Automatické exporty podľa plánu
    - **Custom formats** - Vlastné export formáty



### 🎯 **Aktuálny plán implementácie**

#### **Phase 1 - Virtual Scrolling Enhancement** (Aktuálne v implementácii)
1. ✅ **VirtualScrollingConfiguration model** - Konfigurácia pre rôzne scenáre použitia
2. ⏳ **VirtualScrollingService** - Koordinácia virtualization logic  
3. ⏳ **Horizontal virtualization** - Efektívne zobrazenie stoviek stĺpcov
4. ⏳ **Variable row heights** - Support pre text wrapping
5. ⏳ **Smooth scrolling** - Plynulé animácie a transitions
6. ⏳ **Memory monitoring** - Sledovanie a optimalizácia pamäte
7. ⏳ **Integration** - Integrácia do AdvancedDataGrid komponentu
8. bunky podporuju multiline text - ak este nie je implementovane tak implementuj, ak uz je tak netreba implementovat. Bunky vzdy zobrazia vsetok text co  v sebe maju cize ak by som zadal aj len jeden riadok do bunky a nemestil by sa zobrazit tak sa navysi na tom riadku heigh a zobrazi sa tym padom ten text cely v tej bunke.

**Očakávaný výsledok:** DataGrid schopný efektívne pracovať s datasetmi obsahujúcimi tisíce riadkov a stovky stĺpcov bez výkonnostných problémov.
    - **Change suggestions** - Návrhy zmien na schválenie
    - **Live cursors** - Zobrazenie kurzora iných užívateľov



### 💡 **Odporúčané priority pre ďalšiu implementáciu**

#### **Phase 1 - Kritické pre produkčné použitie** (Vysoká priorita)
1. **Virtual Scrolling Enhancement** (10) - Potrebné pre datasety s tisíckami riadkov/stĺpcov  
2. **Advanced Search Panel** (6) - Výrazne zlepší user experience

#### **Phase 2 - Rozšírené funkcionality** (Stredná priorita)
1. **Built-in Analytics** (8) - Summary rows, aggregations, štatistiky
2. **Background Processing** (11) - Async operations

### 💻 **Použitie**

```csharp
// Základná inicializácia
var dataGrid = new AdvancedDataGrid();
await dataGrid.InitializeAsync(
    columns: columnDefinitions,
    emptyRowsCount: 20,
    colorConfig: customColors
);

// S advanced validation
var validationRules = ValidationRuleSet.CreateEmployeeRuleSet()
    .AddRule(AdvancedValidationRule.Required("FirstName"))
    .AddRule(AdvancedValidationRule.EmailFormat("Email"))
    .AddRule(AdvancedValidationRule.Unique("Email"))
    .AddRule(AdvancedValidationRule.ConditionalRequired("ManagerId", "EmployeeType", "Employee"));

await dataGrid.InitializeAsync(
    columns: columnDefinitions,
    advancedValidationRules: validationRules
);

// Advanced filtering
var filterSet = FilterSet.Create("EmployeeFilters")
    .AddFilter(AdvancedFilter.Contains("FirstName", "John"))
    .AddFilter(AdvancedFilter.NumberRange("Salary", 30000, 80000))
    .AddFilter(AdvancedFilter.In("Department", "IT", "Sales", "Marketing"))
    .WithOperator(LogicalOperator.And);

// Range copy/paste
var range = new CellRange(0, 0, 5, 3); // 6 rows x 4 columns
await copyPasteService.CopyRangeAsync(range, gridData, columnNames);
await copyPasteService.PasteRangeAsync(targetRange, gridData, columnNames);

// Import/Export operácie
var importResult = await dataGrid.ImportFromFileAsync("data.csv", ImportExportConfiguration.DefaultCsv);
await dataGrid.ExportToFileAsync("output.json", ImportExportConfiguration.DefaultJson);

// Load data
await dataGrid.LoadDataAsync(employeeData);
```

### 🎯 **Pokročilé príklady**

#### **Cross-Cell Validation**
```csharp
// Employee salary nemôže byť vyšší ako manager salary
var rule = AdvancedValidationRule.CrossCellCustom(
    "ManagerSalaryCheck",
    new[] { "Salary" },
    new[] { "ManagerId", "EmployeeType" },
    (ctx, allData) => {
        if (ctx.GetStringValue("EmployeeType") != "Employee") 
            return ValidationResult.Success();
            
        var employeeSalary = Convert.ToDecimal(ctx.CurrentValue ?? 0);
        var managerId = ctx.GetStringValue("ManagerId");
        
        var manager = allData.FirstOrDefault(d => 
            d.GetStringValue("EmployeeId") == managerId);
            
        if (manager != null)
        {
            var managerSalary = Convert.ToDecimal(manager.GetValue("Salary") ?? 0);
            return employeeSalary <= managerSalary 
                ? ValidationResult.Success()
                : ValidationResult.Warning("Employee salary exceeds manager's salary");
        }
        
        return ValidationResult.Success();
    });
```

#### **Advanced Filtering s Regex**
```csharp
var advancedFilters = FilterSet.Create("ComplexSearch")
    // Email domain filtering
    .AddFilter(AdvancedFilter.Regex("Email", @"@(company1|company2)\.com$"))
    // Phone number format
    .AddFilter(AdvancedFilter.Regex("Phone", @"^\+421\d{9}$"))
    // Salary range s multiple conditions
    .AddFilter(AdvancedFilter.NumberRange("Salary", 25000, 75000))
    // Department inclusion
    .AddFilter(AdvancedFilter.In("Department", "IT", "Development", "QA"))
    .WithOperator(LogicalOperator.And);
```

#### **Import/Export s konfiguráciou**
```csharp
// CSV import s validáciou
var csvConfig = new ImportExportConfiguration
{
    Format = ExportFormat.CSV,
    IncludeHeaders = true,
    ValidateOnImport = true,
    ContinueOnErrors = false,
    SkipEmptyRows = true,
    Encoding = "UTF-8"
};

var importResult = await dataGrid.ImportFromFileAsync("employees.csv", csvConfig);
if (importResult.IsSuccessful)
{
    Console.WriteLine($"Import successful: {importResult.SuccessfullyImportedRows} rows imported");
}
else
{
    foreach (var error in importResult.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}

// JSON export s formátovaním
var jsonConfig = new ImportExportConfiguration
{
    Format = ExportFormat.JSON,
    JsonFormatting = JsonFormatting.Indented,
    BackupExistingFile = true,
    AutoOpenFile = true
};

await dataGrid.ExportToFileAsync("employees_backup.json", jsonConfig);
```

#### **Multi-Sort s UI indikátormi**
```csharp
// Nastavenie multi-sort konfigurácie
var multiSortConfig = MultiSortConfiguration.Advanced; // Až 5 stĺpcov
dataGrid.SetMultiSortConfiguration(multiSortConfig);

// Programaticky pridanie sort stĺpcov
await dataGrid.AddMultiSortColumnAsync("Department", SortDirection.Ascending, priority: 1);
await dataGrid.AddMultiSortColumnAsync("Salary", SortDirection.Descending, priority: 2);
await dataGrid.AddMultiSortColumnAsync("HireDate", SortDirection.Ascending, priority: 3);
```

#### **Advanced Search s fuzzy matching**
```csharp
// Konfigurácia advanced search
var searchConfig = new AdvancedSearchConfiguration
{
    EnableFuzzySearch = true,
    FuzzySearchThreshold = 0.8,
    EnableRegexSearch = true,
    EnableSearchHighlighting = true,  
    HighlightBackgroundColor = "yellow",
    HighlightTextColor = "black"
};

// Fuzzy search
var results = await dataGrid.PerformAdvancedSearchAsync("Jhon", searchConfig); // Nájde "John"

// Regex search
var regexResults = await dataGrid.PerformAdvancedSearchAsync(@"\b\w+@gmail\.com\b", searchConfig);
```

#### **CheckBox Column Operations**
```csharp
// Inicializácia s CheckBox column (bude prvý stĺpec)
var columns = new List<ColumnDefinition>
{
    new ColumnDefinition { Name = "CheckBoxState", DataType = typeof(bool) }, // Špeciálny stĺpec
    new ColumnDefinition { Name = "FirstName", DataType = typeof(string) },
    new ColumnDefinition { Name = "Email", DataType = typeof(string) },
    new ColumnDefinition { Name = "DeleteRows", DataType = typeof(bool) }  // Špeciálny stĺpec
};

await dataGrid.InitializeAsync(columns);

// Check/Uncheck operations
dataGrid.CheckAllRows();           // Označí všetky neprázdne riadky
dataGrid.UncheckAllRows();         // Odznačí všetky riadky
var checkedCount = dataGrid.GetCheckedRowsCount();
var checkedIndices = dataGrid.GetCheckedRowIndices();

// Delete checked rows
await dataGrid.DeleteAllCheckedRowsAsync();

// Export len checked rows
var checkedData = await dataGrid.ExportCheckedRowsOnlyAsync(includeValidAlerts: false);

// Import s checkbox states
bool[] checkStates = { true, false, true, false }; // Pre prvé 4 riadky
var result = await dataGrid.ImportFromCsvAsync("data.csv", checkBoxStates: checkStates);

// Validation check (iba pre checked rows ak je checkbox column povolený)
bool allValid = await dataGrid.AreAllNonEmptyRowsValidAsync();
```

---

## 📊 LoggerComponent

**Univerzálny logging komponent** pre WinUI3 aplikácie s real-time monitoringom.

### ✅ **Implementované funkcionality**

#### **Core Logging**
- 📝 **Multiple Providers** - Console, File, Debug, Custom
- 🎯 **Log Levels** - Trace, Debug, Info, Warning, Error, Critical
- 🏷️ **Structured Logging** - JSON formatting, metadata support
- ⚡ **Performance** - Async logging, buffering, throttling

#### **Real-time Monitoring**
- 📺 **Live Updates** - Real-time log viewer UI
- 🎛️ **Filtering** - By level, category, timestamp
- 🔍 **Search** - Text search across log messages
- 📊 **Statistics** - Error counts, performance metrics

#### **Enterprise Features**
- 🔄 **Log Rotation** - Size/time-based rotation
- 💾 **Persistence** - SQLite, JSON, custom storage
- 🎨 **UI Integration** - WinUI3 controls, theming support
- ⚙️ **Configuration** - Runtime config changes

### 💻 **Použitie**

```csharp
// Základne nastavenie
var logger = LoggerComponent.CreateLogger<MyClass>();
logger.LogInformation("Application started");

// Advanced konfigurácia
var loggerComponent = new LoggerComponent();
await loggerComponent.InitializeAsync(new LoggerConfig
{
    MinLevel = LogLevel.Debug,
    Providers = { "Console", "File", "UI" },
    EnableRealTimeMonitoring = true
});

// UI Integration
<logger:LoggerViewer Grid.Row="1" 
                    LoggerInstance="{Binding Logger}"
                    ShowFilters="True"
                    MaxDisplayedLogs="1000" />
```

---

## 🔧 **Technické požiadavky**

- **.NET Core 8.0+** - Moderný .NET runtime
- **WinUI3** - Windows App SDK komponenty  
- **Microsoft.Extensions.Logging** - Nezávislý logging systém

## 📦 **Inštalácia**

```xml
<PackageReference Include="RpaWinUiComponentsPackage" Version="1.0.0" />
```

## 🎯 **Vývojový stav**

| Komponent | Stav | Pokrytie |
|-----------|------|----------|
| AdvancedDataGrid | 🟢 **Stabilný** | 95% |
| LoggerComponent | 🟢 **Stabilný** | 90% |
| Advanced Validation | 🟢 **Stabilný** | 100% |
| Advanced Filtering | 🟢 **Stabilný** | 100% |
| Range Copy/Paste | 🟢 **Stabilný** | 95% |
| Multi-Sort | 🟢 **Stabilný** | 100% |
| Advanced Search | 🟢 **Stabilný** | 100% |  
| Export/Import | 🟢 **Stabilný** | 100% |
| CheckBox Column | 🟢 **Stabilný** | 100% |

---