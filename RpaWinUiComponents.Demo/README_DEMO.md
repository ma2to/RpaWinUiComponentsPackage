# 🚀 RPA WinUI Components Demo Application

Táto demo aplikácia poskytuje kompletnú testovaciu platformu pre všetky **PUBLIC API metódy** RpaWinUiComponentsPackage. Umožňuje interaktívne testovanie všetkých funkcionalít bez nutnosti písania kódu.

## 📋 Obsah

- [Spustenie Demo](#spustenie-demo)
- [Testovacie Sekcie](#testovacie-sekcie)
- [Detailné Testovanie](#detailne-testovanie)
- [Background Validation](#background-validation)
- [Troubleshooting](#troubleshooting)

## 🎯 Spustenie Demo

1. **Build Package**: Najprv si zbuildujte hlavný package
   ```bash
   dotnet build RpaWinUiComponentsPackage/RpaWinUiComponentsPackage.csproj
   ```

2. **Build Demo**: Potom zbuildujte demo aplikáciu
   ```bash
   dotnet build RpaWinUiComponents.Demo/RpaWinUiComponents.Demo.csproj
   ```

3. **Spustenie**: Demo aplikáciu spustite z Visual Studio alebo príkazom
   ```bash
   dotnet run --project RpaWinUiComponents.Demo/RpaWinUiComponents.Demo.csproj
   ```

## 🧪 Testovacie Sekcie

Demo aplikácia je rozdelená do **10 hlavných testovacích sekcií**:

### 1. 🎨 Individual Colors Configuration
- **Light Colors**: Aplikuje svetlú farebnú schému
- **Dark Colors**: Aplikuje tmavú farebnú schému  
- **Blue Colors**: Aplikuje modrú farebnú schému
- **Custom Orange + Strong Zebra**: Vlastná oranžová schéma so silným zebra efektom
- **Reset (No Zebra)**: Resetuje na základné farby bez zebra efektu

### 2. 🔍 Search/Sort/Zebra Testing
- **🔍 Test Search**: Testuje vyhľadávacie funkcie
- **⬆️⬇️ Test Sort**: Testuje sortovacie funkcie
- **🦓 Toggle Zebra**: Prepína zebra efekt (striedavé farby riadkov)
- **🧹 Clear Search**: Vyčistí všetky vyhľadávacie filtre

### 3. 🔥 AUTO-ADD Functionality Testing
- **Load 2 Rows**: Načíta 2 ukážkové riadky
- **Load 20 Rows**: Načíta 20 ukážkových riadkov
- **Test Smart Delete**: Testuje inteligentné mazanie riadkov

### 4. 📊 Standard DataGrid Functions
- **📊 Load Sample Data**: Načíta ukážkové dáta do gridu
- **✅ Validate All**: Spustí validáciu všetkých riadkov
- **🗑️ Clear All Data**: Vyčistí všetky dáta z gridu
- **📤 Export Data**: Exportuje dáta do súboru

### 5. ⏳ Background Validation Testing
- **🔍 Test Email Duplicate**: Pridá duplikátny email pre testovanie BG validácie
- **🏢 Test Tax Number**: Pridá platné/neplatné daňové čísla
- **➕ Add BG Rule**: Pridá novú background validation rule
- **📊 BG Diagnostics**: Zobrazí diagnostické informácie BG validácie

### 6. 🔍 Search & Filter Testing
- **🔍 Search 'Anna'**: Vyhľadá všetky riadky obsahujúce "Anna"
- **📧 Search 'test'**: Vyhľadá všetky emaily obsahujúce "test"
- **🗑️ Clear Search**: Vyčistí vyhľadávacie filtre
- **🔢 Sort by Age**: Zoradí dáta podľa veku

### 7. ☑️ CheckBox Operations Testing
- **☑️ Check All**: Označí všetky riadky
- **☐ Uncheck All**: Odznačí všetky riadky
- **📊 Get Checked Count**: Zobrazí počet označených riadkov
- **🗑️ Delete Checked**: Vymaže označené riadky
- **📤 Export Checked**: Exportuje iba označené riadky

### 8. 🧭 Navigation Testing
- **🎯 Move to [0,0]**: Presunie kurzor na prvú bunku
- **➡️ Move Next**: Presunie kurzor na ďalšiu bunku
- **⬅️ Move Previous**: Presunie kurzor na predchádzajúcu bunku
- **🎯 Select All Cells**: Označí všetky bunky

### 9. 📁 Import/Export Testing
- **📄 Export CSV**: Exportuje dáta do CSV formátu
- **📄 Export JSON**: Exportuje dáta do JSON formátu
- **📥 Import CSV**: Importuje dáta z CSV súboru
- **📊 Export History**: Zobrazí históriu exportov

### 10. 📝 LoggerComponent Testing
- **ℹ️ Log Info**: Zapíše info správu do logu
- **⚠️ Log Warning**: Zapíše warning správu do logu
- **❌ Log Error**: Zapíše error správu do logu
- **📊 Logger Diagnostics**: Zobrazí diagnostické informácie loggera

### 11. 🧪 Advanced Testing
- **📊 Load Large Dataset (100 rows)**: Načíta veľký dataset pre testovanie výkonu
- **✅ Test All Validation**: Testuje všetky typy validácií naraz
- **⚡ Stress Test**: Spustí záťažový test všetkých funkcionalít

## 🔬 Detailné Testovanie

### Background Validation Workflow
1. **Načítajte dáta**: Stlačte "Load Sample Data"
2. **Testujte duplicate email**: Stlačte "Test Email Duplicate" - pridá email "test@duplicate.sk" 
3. **Pozorujte BG validáciu**: V pozadí sa spustí validácia duplikátov emailov
4. **Kontrola diagnostiky**: Stlačte "BG Diagnostics" pre zobrazenie štatistík
5. **Pridanie pravidla**: Stlačte "Add BG Rule" pre pridanie nového pravidla

### Realtime vs Background Validation
- **Realtime validácia**: Spúšťa sa okamžite pri zmene hodnoty
- **Background validácia**: Spúšťa sa v pozadí pre zložité operácie (databáza, API)
- **Oba systémy fungujú paralelne** bez konfliktu

### Testovanie Export/Import
1. **Načítajte dáta**: "Load Sample Data"
2. **Označte riadky**: Použite checkbox stlpce
3. **Export označených**: "Export Checked" - exportuje iba označené
4. **Export všetkých**: "Export CSV/JSON" - exportuje všetky dáta
5. **Import test**: "Import CSV" - importuje dáta zo súboru

### Color Theme Testing
1. **Základné témy**: Light, Dark, Blue
2. **Custom téma**: Orange + Strong Zebra pre silný vizuálny efekt
3. **Zebra toggle**: Možnosť zapnúť/vypnúť striedavé farby riadkov
4. **Reset**: Návrat na štandardné farby

## 🛠️ Background Validation

Demo obsahuje **4 typy background validácií**:

### 1. Email Duplicate Check
```csharp
// Simuluje kontrolu duplikátnych emailov v databáze
await Task.Delay(800); // Simulácia API volania
var isDuplicate = existingEmails.Contains(email);
```

### 2. Tax Number Validation  
```csharp
// Validácia slovenského daňového čísla
await Task.Delay(1200); // Simulácia externého API
bool isValid = IsSlovakTaxNumberValid(taxNumber);
```

### 3. Complex Business Rule
```csharp
// Zložité biznis pravidlo pre vek vs email doménu
await Task.Delay(600);
if (age < 18 && email.EndsWith("@business.sk"))
    return Error("Maloletí nemôžu mať business email");
```

### 4. Address Validation
```csharp
// Validácia adresy cez externý servis
await Task.Delay(1000);
// Simulácia GPS/address validation API
```

## 🔧 Troubleshooting

### Package nie je dostupný
Ak vidíte hlášku "Package nie je dostupný":
1. Skontrolujte, či je package správne zbuildovaný
2. Overte projekt referencie v demo aplikácii
3. Skúste Clean + Rebuild celého solution

### Validácie nefungujú
1. **Realtime validácia**: Skontrolujte konzolu pre error hlášky
2. **Background validácia**: Použite "BG Diagnostics" pre kontrolu stavu
3. **Logger**: Skontrolujte "Logger Diagnostics" pre log správy

### Pomalé načítanie
1. **Large Dataset**: 100 riadkov môže trvať dlhšie pri prvom načítaní
2. **Background validácia**: Môže spomaľovať UI ak beží veľa validácií
3. **Cache**: BG validácia používa cache pre zrýchlenie opakovaných validácií

### Export/Import chyby
1. **File permissions**: Skontrolujte oprávnenia pre zápis súborov
2. **CSV format**: Použite správny delimiter (čiarka)
3. **JSON format**: Skontrolujte validity JSON štruktúry

## 📊 Monitoring a Diagnostika

Demo poskytuje **4 typy diagnostiky**:

1. **BG Diagnostics**: Štatistiky background validácie
2. **Logger Diagnostics**: Informácie o logovaní
3. **Export History**: História všetkých exportov
4. **Checked Count**: Počet označených riadkov

## 🎯 Ciele Demo Aplikácie

Táto demo aplikácia slúži na:
- ✅ **Testovanie všetkých PUBLIC API metód**
- ✅ **Overenie funkčnosti pred produkčným nasadením**
- ✅ **Demonštrácia možností package pre klientov**
- ✅ **Debugging a troubleshooting problémov**
- ✅ **Performance testing s veľkými datasetmi**

---

**💡 Tip**: Pre najlepšie výsledky začnite s "Load Sample Data", potom testujte jednotlivé funkcionality postupne podľa sekcií vyššie.