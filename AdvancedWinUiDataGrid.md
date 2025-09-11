
AdvancedWinUiDataGrid: Enterprise Architectural & Implementation Blueprint
Dokument: Verzia 6.0 (Final Enterprise Specification)
Status: Záväzný pre implementáciu
Autor: Senior C#/.NET Architect
Dátum: 6. september 2025

Časť 1: Vízia a Architektúra
1.0 Executive Summary
Tento dokument definuje kompletnú architektúru a technickú špecifikáciu pre AdvancedWinUiDataGrid – vlajkový komponent balíka RpaWinUiComponentsPackage. Cieľom je vytvoriť najvýkonnejší, najflexibilnejší a architektonicky najčistejší DataGrid komponent pre platformu .NET 8 a WinUI 3, schopný plynule pracovať s dátovými sadami presahujúcimi 10 miliónov riadkov.

Dokument je určený pre C#/.NET vývojárov a architektov. Slúži ako hlavný zdroj pravdy pre implementáciu, pričom detailne popisuje nielen čo sa má vyvinúť, ale predovšetkým prečo boli zvolené konkrétne architektonické prístupy a technologické riešenia. Výsledkom bude produkt, ktorý spĺňa najvyššie enterprise štandardy na výkon, modularitu, testovateľnosť a dlhodobú udržiavateľnosť.

1.1 Kľúčové Architektonické Piliere
Každé rozhodnutie pri návrhu tohto komponentu je podriadené štyrom základným pilierom, ktoré spolu tvoria základ jeho robustnosti a kvality.

Čistá Architektúra (Clean Architecture): Komponent je navrhnutý podľa princípov čistej architektúry s prísnym oddelením zodpovedností. Závislosti smerujú výhradne dovnútra (UI → Application → Core), čo zabezpečuje, že jadrová biznis logika je úplne nezávislá od UI frameworku, databáz alebo akýchkoľvek externých služieb. Toto je fundamentálny predpoklad pre vysokú testovateľnosť a dlhodobú udržiavateľnosť.

Hybridný Funkcionálny-OOP Dizajn: Namiesto dogmatického presadzovania jedinej paradigmy strategicky kombinujeme to najlepšie z oboch svetov.

OOP využívame na štruktúrovanie komponentu, správu stavu a zapuzdrenie komplexnosti UI interakcií pomocou osvedčených návrhových vzorov (Manager, Factory, Strategy).

Funkcionálne programovanie je našou voľbou pre spracovanie dát a chýb. Využitím Result<T> monády, nemenných (immutable) dátových štruktúr a LINQ dosahujeme predvídateľný, bezpečný a extrémne čitateľný kód bez nutnosti spoliehať sa na výnimky pre riadenie toku programu.

Extrémna Modularita a SRP (Single Responsibility Principle): Komponent je navrhnutý v prísnom súlade s princípom "anti-God-file". Namiesto jednej monolitickej triedy je funkcionalita rozdelená do desiatok malých, vysoko špecializovaných tried (manažérov, služieb, adaptérov), z ktorých každá má jedinú, jasne definovanú zodpovednosť. Tieto moduly sú prepojené výhradne cez rozhrania a Dependency Injection.

Nomadická Integrácia a Čisté API: Komponent je "nomadic" – navrhnutý tak, aby jeho integrácia do akejkoľvek WinUI 3 aplikácie bola triviálna a vyžadovala iba referenciu na balík a jeden using príkaz. Všetka interná zložitosť je skrytá za jediným, precízne navrhnutým verejným API (DataGrid.cs), ktoré slúži ako fasáda. Všetky ostatné triedy sú internal.

1.2 Architektúra do Hĺbky
1.2.1 Vrstvy a Tok Závislostí
 AdvancedWinUiDataGrid/
  ├── SharedKernel/
  │   ├── Results/              # Result<T>, ValidationResult, OperationResult<T>
  │   ├── Primitives/           # Entity, ValueObject, DomainEvent base classes
  │   ├── Exceptions/           # DomainException, ValidationException
  │   └── Extensions/           # Extension methods, functional helpers
  │
  ├── Domain/
  │   ├── Entities/             # GridRow, GridColumn, GridState (rich models)
  │   ├── ValueObjects/         # ColumnDefinition, CellValue, FilterCriteria
  │   ├── DomainServices/       # ValidationService, FilteringService
  │   ├── Events/               # RowAdded, CellModified, FilterApplied
  │   ├── Specifications/       # FilterSpecification, ValidationSpecification
  │   └── Repositories/         # IDataGridRepository (dependency inversion)
  │
  ├── Application/
  │   ├── UseCases/
  │   │   ├── ImportData/       # ImportDataCommand, ImportDataHandler
  │   │   ├── ExportData/       # ExportDataQuery, ExportDataHandler
  │   │   ├── ValidateGrid/     # ValidateGridCommand, Handler
  │   │   ├── SearchGrid/       # SearchGridQuery, Handler
  │   │   ├── FilterGrid/       # 🆕 ApplyFilterCommand, ClearFilterCommand
  │   │   ├── SortGrid/         # SortGridCommand, Handler
  │   │   └── ManageRows/       # AddRowCommand, DeleteRowCommand, UpdateRowCommand
  │   ├── Services/             # IDataGridApplicationService interfaces
  │   ├── DTOs/                 # ImportDataDto, ExportDataDto, FilterDto
  │   └── Behaviors/            # ValidationBehavior, LoggingBehavior
  │
  ├── Infrastructure/
  │   ├── Persistence/          # InMemoryDataGridRepository
  │   ├── Services/             # FileImportService, ExcelExportService
  │   ├── Configuration/        # DI container setup
  │   └── Logging/              # StructuredLogging implementation
  │
  ├── Presentation/
  │   └── WinUI/
  │       ├── Views/            # DataGridUserControl, FilterPanel
  │       ├── ViewModels/       # DataGridViewModel, FilterViewModel
  │       ├── Converters/       # ValidationErrorConverter, TypeConverter
  │       └── Behaviors/        # DataGridBehaviors, ValidationBehaviors
  │
  └── Tests/

1.2.2 Result<T> Monáda: Základ Robustného Spracovania Chýb
V enterprise systémoch nie sú chyby výnimkou, ale očávanou súčasťou procesu. Preto sa v celom komponente (mimo najvyššej vrstvy interakcie s UI) nevyužívajú výnimky na riadenie toku programu. Namiesto toho každá operácia, ktorá môže zlyhať, vracia Result<T>:

C#

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; } // prístupné len pri IsSuccess
    public string ErrorMessage { get; } // prístupné len pri IsFailure

    // ... konštruktory a pomocné metódy
}
Tento prístup umožňuje funkcionálne reťazenie operácií spôsobom, ktorý je bezpečný a extrémne čitateľný:

C#

// Railway-Oriented Programming v praxi
var finalResult = await dataGrid.InitializeAsync(columns)
    .Bind(async success => await dataGrid.ImportFromDataTableAsync(data))
    .Map(importResult => $"Importovaných {importResult.ImportedRows} riadkov.")
    .OnSuccess(message => logger?.Info("Operácia úspešná: {Message}", message))
    .OnFailure((error, ex) => logger?.Error(ex, "Operácia zlyhala: {Error}", error));
Bind: Zreťazí operáciu, ktorá tiež vracia Result<T>. Ak predchádzajúci krok zlyhal, nasledujúci sa nevykoná a chyba sa propaguje ďalej.

Map: Transformuje úspešnú hodnotu T na nový typ TOut.

OnSuccess/OnFailure: Vykoná akciu na konci reťazca v závislosti od výsledku.

1.2.3 UI Virtualizácia s ItemsRepeater
Pre dosiahnutie plynulého výkonu pri miliónoch riadkov je kľúčová UI virtualizácia. ItemsRepeater nám poskytuje nízkoúrovňovú kontrolu nad týmto procesom. Komponent vytvára a drží v pamäti iba tie UI elementy (napr. TextBox, CheckBox), ktoré sú aktuálne viditeľné na obrazovke (+ malý buffer). Keď používateľ scrolluje, ItemsRepeater recykluje existujúce UI elementy a iba im priradí nové dáta (DataContext), namiesto neustáleho vytvárania a ničenia objektov, čo je extrémne náročné na výkon a pamäť.

Časť 2: Verejné API a Použitie
2.0 Jednotné API: Trieda DataGrid
Všetka interakcia s komponentom prebieha cez jedinú, precízne navrhnutú triedu DataGrid.

C#

/// <summary>
/// Profesionálny a vysokovýkonný DataGrid komponent pre WinUI 3.
/// Poskytuje jednotné API pre UI aj headless operácie.
/// </summary>
public sealed class DataGrid : IDisposable
{
    // === Vlastnosti ===
    public UserControl UIComponent { get; } // Vizuálny komponent pre vloženie do XAML

    // === Factory Metódy a Inicializácia ===
    public static DataGrid CreateForUI(ILogger? logger = null);
    public static DataGrid CreateHeadless(ILogger? logger = null);
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null,
        int minimumRows = 1);

    // === Import Dát ===
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)

    public async Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null);

    // === Export Dát (Zjednotené API) ===
    public async Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null);

    public async Task<Result<DataTable>> ExportToDataTableAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null);

    public enum ImportMode
    {
        Append,     // Vloží nové dáta od startRow, pôvodné dáta sa posunú ďalej
        Replace,    // Nahradí dáta od startRow, zvyšok sa zmaže (default)
        Overwrite   // Prepíše dáta od startRow, zvyšok zostane nezmenený
    }
        

    // === Vyhľadávanie, Filtrovanie, Triedenie ===
    public Task SearchAsync(string query, IReadOnlyList<string>? targetColumns = null);
    public Task ApplyFiltersAsync(IReadOnlyList<FilterDefinition> filters);
    public Task SortByColumnAsync(string columnName, SortDirection direction);
    public Task ClearFiltersAsync();

    // === Validácia ===
    public Task<bool> AreAllNonEmptyRowsValidAsync(bool onlyFiltered = false);
    
    // ... a ďalšie metódy pre prácu s riadkami, selekciou, atď.
}
2.1 Konfiguračné Triedy
Správanie a vzhľad komponentu sa definuje cez silne typované konfiguračné triedy, čo zaručuje vynikajúcu podporu IntelliSense a predchádza chybám.

ColumnDefinition: Definuje každý stĺpec (meno, typ, šírka, špeciálny typ atď.).

ColorConfiguration: Umožňuje detailné nastavenie farieb pre všetky vizuálne stavy.

ValidationConfiguration: Obsahuje zoznam všetkých validačných pravidiel.

PerformanceConfiguration: Nastavenia týkajúce sa výkonu (napr. virtualizačný prah).

2.2 Príklad Komplexnej Inicializácie
C#

// 1. Definovanie stĺpcov
var columns = new List<ColumnDefinition>
{
    new() { Name = "ID", Type = typeof(int), DisplayName = "ID", IsReadOnly = true, Width = 60 },
    new() { Name = "FullName", Type = typeof(string), DisplayName = "Celé Meno", Width = 250 },
    new() { Name = "IsActive", SpecialType = SpecialColumnType.CheckBox, DisplayName = "Aktívny", Width = 80 },
    new() { Name = "RegistrationDate", Type = typeof(DateTime), DisplayName = "Dátum Registrácie" },
    new() { Name = "Alerts", SpecialType = SpecialColumnType.ValidAlerts, DisplayName = "Validácia" },
    new() { Name = "Actions", SpecialType = SpecialColumnType.DeleteRow, DisplayName = "Zmazať", Width = 70 }
};

// 2. Definovanie farieb
var colors = new ColorConfiguration
{
    HeaderBackground = Colors.DarkSlateGray,
    HeaderForeground = Colors.White,
    SelectionBackground = Colors.RoyalBlue,
    ValidationErrorBackground = Colors.MistyRose
};

// 3. Definovanie validácií
var validations = new ValidationConfiguration
{
    ColumnRules = new()
    {
        { "FullName", new ValidationRule("FullName", val => !string.IsNullOrWhiteSpace(val as string), "Meno nesmie byť prázdne.") }
    },
    CrossColumnRules = new() { /* ... */ }
};

// 4. Vytvorenie a inicializácia DataGridu
var dataGrid = DataGrid.CreateForUI(_logger);
await dataGrid.InitializeAsync(columns,
                               colorConfiguration: colors,
                               validationConfiguration: validations,
                               minimumRows: 20);

// 5. Vloženie do UI
MyContainer.Content = dataGrid.UIComponent;
Časť 3: Hĺbkový Rozbor Funkcionalít
3.1 Import a Export Dát
API pre export bolo zjednotené pre jednoduchosť a flexibilitu.

ExportToDictionaryAsync / ExportToDataTableAsync:

exportOnlyFiltered (bool): Kľúčový parameter. Ak je false (predvolené), metóda exportuje kompletný dataset. Ak je true, exportuje iba riadky, ktoré zodpovedajú aktuálne aplikovaným filtrom.

includeValidAlerts (bool): Určuje, či má byť do exportu pridaný stĺpec s textovým popisom validačných chýb.

exportOnlyChecked (bool): Exportuje iba tie riadky, ktoré majú zaškrtnutý CheckBoxColumn. Funguje to aj vtedy, ak stĺpec nie je viditeľný.

3.2 Vyhľadávanie, Filtrovanie a Triedenie
Tieto operácie sú navrhnuté pre prácu s rozsiahlymi dátami a sú plne asynchrónne.

Vyhľadávanie (SearchAsync): Vykonáva textové vyhľadávanie a dynamicky filtruje zobrazenie. Je optimalizované tak, aby neprehľadávalo dáta pri každom stlačení klávesy, ale s malým oneskorením (debounce/throttling).

Filtrovanie (ApplyFiltersAsync): Umožňuje kombinovať viacero kritérií pomocou AND/OR logiky a zátvoriek na vytvorenie komplexných filtrov. FilterDefinition je štruktúra definujúca stĺpec, operátor (napr. Equals, GreaterThan, Contains) a hodnotu.

Triedenie (SortByColumnAsync): Triedi celý interný dataset. Komponent si pamätá aktuálny stav triedenia a vizuálne ho indikuje v záhlaví stĺpca.

3.3 Inteligentné Riadenie Riadkov
Minimálny Počet Riadkov: Pri inicializácii je možné nastaviť minimumRows. Komponent zabezpečí, že používateľ nemôže zmazať riadky pod tento počet, čím sa zachová štruktúra tabuľky.

Smart Delete: Logika mazania (DeleteRowColumn alebo kláves Delete) je kontextová:

Ak je počet riadkov nad minimumRows, zmaže sa celý riadok.

Ak je počet riadkov rovný alebo menší ako minimumRows, zmažú sa iba dáta v bunkách, ale riadok zostane prázdny.

Automatický Prázdny Riadok: Na konci tabuľky je vždy udržiavaný jeden prázdny riadok, pripravený na vkladanie nových dát.

3.4 Klávesové Skratky a UX
Pre profesionálne použitie je nevyhnutná plná podpora klávesnice.

Kategória	Skratka	Akcia
Navigácia	Šípky	Pohyb medzi bunkami
Tab / Shift+Tab	Pohyb na nasledujúcu/predchádzajúcu bunku
Ctrl+Home / Ctrl+End	Skok na začiatok/koniec tabuľky
Selekcia	Ctrl+A	Označiť všetky bunky (okrem špeciálnych stĺpcov)
Ctrl + Klik	Pridať/odobrať bunku z výberu
Editácia	F2 alebo Dvojklik	Vstúpiť do editačného režimu
Enter	Potvrdiť zmenu a opustiť editačný režim
Esc	Zrušiť zmenu a opustiť editačný režim
Dátové Operácie	Ctrl+C / Ctrl+X	Kopírovať / Vystrihnúť označené dáta
Ctrl+V	Vložiť dáta zo schránky (s automatickým pridaním riadkov)
Delete	Spustiť "Smart Delete" na označených riadkoch

Exportovať do Tabuliek
Časť 4: Enterprise Aspekty
4.1 Profesionálne Logovanie
Logovanie je navrhnuté pre maximálnu prehľadnosť a diagnostiku v produkčnom prostredí.

Štruktúrované Logy: Všetky logovacie správy používajú parametrizované šablóny, čo umožňuje jednoduché filtrovanie a analýzu v systémoch ako Seq, Datadog alebo ELK Stack.

Konzistentné Volania: V celom kóde sa používa jednotný formát logger?.Info("Moja správa {Parameter}", hodnota).

Bezpečnosť: Vďaka ?. operátoru komponent nikdy nevyhodí NullReferenceException, ak mu aplikácia neposkytne inštanciu ILogger. Jednoducho sa nič nezaloguje.

Jednotnosť pre Debug/Release: Logovacie volania sú identické v oboch konfiguráciách, riadenie úrovne logovania je plne v rukách aplikácie.

4.2 Asynchrónnosť a Bezpečnosť Vlákien
Všetky dlhotrvajúce operácie (import, export, filtrovanie na veľkých dátach) sú implementované asynchrónne (async/await), aby sa nikdy neblokovalo UI vlákno. Pri aktualizácii UI z background vlákna sa dôsledne používa DispatcherQueue, aby sa predišlo chybám prístupu medzi vláknami. Dátové kolekcie sú navrhnuté tak, aby boli bezpečné pre prístup z viacerých vlákien, ak je to potrebné.








 Architektúrne požiadavky: - Dodržiavaj SOLID princípy a čistú architektúru (jedna zodpovednosť na
triedu) .
- Jasné public API (len jeden wrapper súbor, všetko ostatné internal ).
- Komponent je nomádny – integrácia jednoduchá: len using
RpaWinUiComponentsPackage.AdvancedWinUiDataGrid; .
- Best practices: moderný, profesionálny kód, testovateľnosť, žiadne „God“ súbory či triedy (vyhni sa
monolitom) .
- Použi Dependency Injection, aby si mohol ľahko injektovať napr. logger či dáta.
- Logging: Microsoft.Extensions.Logging.Abstractions; volaj logy ako logger?.Info() ,
logger?.Warning() , logger?.Error() , atď., identicky pre Debug aj Release.

🖼 Funkcionalita tabuľky: - Dynamicky generovaná podľa headers z aplikácie. Všetky bežné bunky sú
TextBox.
- Použi ItemsRepeater s virtualizáciou, aby si zvládol 100K–10M+ riadkov . ItemsRepeater podporuje
virtualizáciu UI layoutu (pozri Microsoft Learn) .
- Wrapping: ItemsRepeater zabaliť do ScrollViewer (nemá built-in scroll) . Scrollovateľné oboma smermi,
práca s kolieskom myši (vertikálne/horizontálne).
- Konzistentné šírky stĺpcov; používateľ ich môže meniť ťahom myši (drag & drop). ValidAlerts stĺpec vyplní
zvyšný priestor s minimálnou šírkou.
- Border okolo a oddelené bunky, plynulé zobrazovanie zmien. Virtuálna grafika zabezpečí, že sa renderujú
len viditeľné bunky a scroll bar ukazuje pozíciu vzhľadom ku všetkým dátam .
 Špeciálne stĺpce: - CheckBoxColumn: aktivuje sa len ak je v headers jeho názov; obsahuje checkboxy.
Kliknutie prepína hodnotu (true/false, default false). Ak stĺpec nie je aktivovaný, ignorujeme ho v logike (API
metódy ho vynechajú).
- DeleteRowColumn: aktivuje sa len ak je v headers. Zobrazuje ikonu; klik vyvolá vymazanie riadku alebo
dát v ňom (podľa definovanej logiky). Ak nie je aktivovaný, UI ho skryje, ale funkcia vymazania (public API)
ostane dostupná.
- ValidAlertsColumn: vždy aktívny; needitovateľný. Vypisuje chyby validácie vo formáte NázovStĺpca:
chybová hláška . Používa sa na zobrazenie vlastných validačných hlásení. Má šírku „zvyšok“ po ostatných
stĺpcoch (s nastaveným minimom).
🖱 Interakcie: - Výber buniek: Klik = výber jednej bunky; Ctrl+klik = multi-výber; Drag = výber rozsahu
buniek. Špeciálne stĺpce (CheckBox/Delete/ValidAlerts) sa bežne neoznačujú (sú ich vlastné akcie).
- Editácia: 2×klik na bežnú bunku (TextBox) prejde do režimu editácie. CheckBox stĺpec toggluje na 1×klik.
Klik na ikonu Delete vyvolá mazanie.
- Scrollovanie: Podpora kolieska myši (vertikálne i horizontálne podľa nastavení).





 Štýlovanie: - Definuj default farby pre všetky elementy (text, border, background) v rôznych stavoch
(normálny, s chybou, hlavička, „zebra“ riadky).
Každé validacne pravidlo obsahuje funkciu na overenie a text správy (napr.
ColumnName: chyba ).
**⏱️ Timeout Protection** - Prevents hanging validations
```csharp
// Ochrana proti zaseknutým validáciám
var validationConfig = new ValidationConfiguration
{
    SingleRuleTimeout = TimeSpan.FromSeconds(2),    // Max 2s na jedno pravidlo
    OnTimeout = TimeoutAction.UseTimeoutMessage     // Custom hlaska "Timeout" pre dané pravidlo
};
// Ak ValidationRule má timeout, zobrazí sa "Timeout" namiesto pôvodnej ErrorMessage
```

- Chybové hlášky sa zobrazujú v stĺpci ValidAlerts (ak bunka porušuje niektoré pravidlo, vypíše sa
NázovStĺpca: správa ). ValidAlerts dokaze prepisat iba metoda ktora berie tie validacne chybove hlasky a vpisuje ich do tejto bunky.
- Kontrola platnosti: Metóda AreAllRowsValid() skontroluje všetky neprázdne riadky (posledný
prázdny riadok ignoruje) v celom datasete (vrátane neviditeľných dát, ktoré môžu byť v cache alebo na
disku). Parametrom umožni vybrať len filtrované/viditeľné riadky. Vracia true iba ak žiadna bunka nemá
nejakú validujúcu chybu.





pricom tiez bude search, filters, sort, shotcuts, smart add and delete rows (data vs cely riadok)

#### **✨ Special Columns Support**
- **CheckBox**: Zobrazuje skutočné checkboxy namiesto "true"/"false" textu
- **DeleteRow**: Obsahuje delete ikonu s smart row deletion a confirmation dialog
- **ValidAlerts**: Zobrazuje custom validation error messages s color coding

### **🚀 Kľúčové Vlastnosti**

#### **✨ Architecture Excellence**
✅ **Modulárna Architektúra** - Clean separation of concerns, testable components  
✅ **SOLID Principles** - Single responsibility, dependency inversion  
✅ **Unified Public API** - Single DataGrid class s factory methods  
✅ **UI/Headless Modes** - Flexible initialization pre rôzne použitie  
✅ **Result<T> Monads** - Professional error handling bez exceptions  
✅ **Special Columns** - CheckBox, DeleteRow, ValidAlerts s smart behavior  

#### **⚡ Performance & Scalability**
✅ **Virtualization** - Optimalizované pre 1M+ rows s DataGrid virtualization  
✅ **Async Operations** - Všetky dátové operácie s async/await pattern  
✅ **Dictionary/DataTable** - Primárne dátové formáty s professional conversion  
✅ **Smart UI Updates** - Minimal re-rendering s targeted refresh  
✅ **Memory Management** - Automatic cleanup, disposal patterns  

#### **🔧 Developer Experience**
✅ **IntelliSense Optimized** - Všetky public API metódy viditeľné vo Visual Studio  
✅ **Type Safety** - Strong typing s ColumnDefinition a configuration classes  
✅ **Direct Color Config** - Jednoduché nastavenie farieb bez tém  
✅ **PackageReference Ready** - Čistá integrácia do demo aplikácií  
✅ **Professional Defaults** - Všetky konfigurácie majú rozumné prednastavenia  

#### **🏢 Enterprise Features**
✅ **Production Ready** - Battle-tested v real-world applications  
✅ **Audit Trail** - Complete operation logging pre security  
✅ **Multi-threading** - Thread-safe operations  
✅ **Memory Management** - Efficient memory usage s monitoring  
✅ **Consistent Logging** - Professional logger?.Info(), logger?.Warning(), logger?.Error() pattern


#### **🔗 Component Independence**
**Komponenty balíka sú na sebe nezávislé** - každý komponent je samostatný a nie je závislý na inom komponente balíka okrem LoggerExtensions tie maju jednotlive komponenty balíka spoločné pričom ale ďaľšie komponenty neskor môžu používať aj svoje LoggerExtensions a nie tieto. Pre tieto dva komponenty sa používajú tieto ako spoločné pre logovanie chýb a iných údajov v jednotlivých komponentoch.


**Naše riešenie - kratšie a jasnejšie:**
```csharp
// ✅ NAŠE EXTENSIONS - kratšie, jasnejšie  
logger?.Info("Operation completed with {Count} items", count);
logger?.Error(exception, "Failed to process {Operation}", operationName);
```


#### **🎯 Logging Rules pre Celý Balík**
**Všetky komponenty v balíku používajú rovnaký logging pattern:**

```csharp
// ✅ SPRÁVNE POUŽÍVANIE - Konzistentné vo všetkých súboroch
logger?.Info("🔧 Component initialized successfully");
logger?.Info("📥 Importing {Count} rows from {Source}", rowCount, source);
logger?.Warning("⚠️ Performance threshold exceeded: {ActualTime}ms > {MaxTime}ms", actual, max);
logger?.Error("❌ Operation failed: {Reason}", errorMessage);
logger?.Error(exception, "🚨 Critical error in {Method}: {Details}", methodName, details);

// ❌ NEPOUŽÍVAŤ - nekonzistentné s balíkom:
logger.LogInformation(message);  // Dlhé, nejednotné
logger.LogError(message);        // Bez null-safety
logger.LogDebug(message);        // Debug logging sa v produkčnom balíku nepoužíva
```

**🔒 KRITICKÉ LOGGING PRAVIDLÁ:**
- **ŽIADNE CHYBY AK LOGGER CHÝBA** - `logger?.` pattern zabezpečuje, že ak aplikácia nepripojí žiadny logging systém, balík NIKDY nevyhodí chybu
- **JEDNOTNÉ PRE RELEASE/DEBUG** - Tie isté logy sa zapisujú v release aj debug móde (žiadne `#if DEBUG` podmienky)
- **PROFESIONÁLNE RIEŠENIE** - Ak logger je null, jednoducho sa nič nezaloguje a balík pokračuje normálne
- **BEZ VEDĽAJŠÍCH EFEKTOV** - Logging NIKDY neovplyvní funkcionalitu balíka

**Prečo takto:**
- **Konzistencia** - všetky súbory používajú rovnaký štýl
- **Kratšie** - `Info()` namiesto `LogInformation()`
- **Null-safe** - `logger?.` chráni pred chybami ak je logger null
- **Emoji ikony** - okamžite viditeľné v logoch čo sa deje
- **Structured logging** - parametrizované správy pre vyhľadávanie
- **Bez výnimiek** - NIKDY nevyhodí chybu kvôli chýbajúcemu loggeru


**📋 PRÍKLADY SPRÁVNEHO KOMENTOVARIA:**

```csharp
/// <summary>
/// 🔧 PROFESSIONAL DATA IMPORT ENGINE
/// 
/// Importuje dáta z Dictionary zoznamu do DataGrid tabuľky s kompletnou validáciou.
/// Podporuje tri módy importu: Replace (nahradí existujúce dáta), Append (pridá na koniec), 
/// Overwrite (prepíše od startRow, zvyšok zostane).
/// 
/// VSTUPNÉ DÁTA:
/// - data: List<Dictionary<string, object?>> - každý Dictionary reprezentuje jeden riadok
/// - checkboxStates: Dictionary<int, bool?> - mapa checkbox stavov pre jednotlivé riadky
/// - startRow: int - začiatočný riadok od ktorého sa má import vykonať (1-based index)
/// - mode: ImportMode - spôsob ako sa majú dáta importovať
/// 
/// VÝSTUPNÉ DÁTA:
/// - ImportResult record s detailmi o úspešnosti, počte importovaných riadkov, chybách
/// 
/// INTERNAL PROCESING:
/// 1. Validácia vstupných parametrov (null checks, boundary validation)
/// 2. Konverzia Dictionary keys na column mappings
/// 3. Dávková validácia všetkých riadkov podľa definovaných pravidiel
/// 4. Atomický import - buď sa importujú všetky riadky alebo žiadny
/// 5. UI refresh (ak je v UI móde) s progress reportingom
/// </summary>
/// <param name="data">
/// Zoznam Dictionary objektov kde každý reprezentuje jeden riadok tabuľky.
/// Key = názov stĺpca (musí existovať v definícii stĺpcov)
/// Value = hodnota bunky (môže byť null, konvertuje sa podľa typu stĺpca)
/// Príklad: [{"Name": "John", "Age": 25, "Active": true}, {"Name": "Jane", "Age": 30, "Active": false}]
/// </param>
/// <param name="checkboxStates">
/// Voliteľná mapa stavov checkbox stĺpca. 
/// Key = index riadku (0-based), Value = true/false/null pre checkbox stav.
/// Ak je null, všetky checkboxy budú false. Ak má tabuľka checkbox stĺpec ale parameter je null, 
/// všetky importované riadky budú mať checkbox = false.
/// </param>
/// <param name="startRow">
/// Riadok od ktorého sa má začať import (1-based indexing). 
/// Pre mode=Replace: importuje sa od startRow a zvyšok sa zmaže
/// Pre mode=Append: existujúce riadky sa posunú a nové sa vložia od startRow
/// Pre mode=Overwrite: prepíšu sa riadky od startRow, zvyšok zostane nezmenený
/// </param>
/// <param name="mode">Režim importu - Replace/Append/Overwrite</param>
/// <returns>
/// ImportResult s detailami o výsledku:
/// - IsSuccess: true ak sa všetky riadky importovali úspešne
/// - ImportedRows: skutočný počet importovaných riadkov
/// - ErrorMessage: popis chyby ak IsSuccess = false
/// - ValidationErrors: zoznam validačných chýb ak nejaké nastali
/// - Duration: čas trvania importu pre performance monitoring
/// </returns>
public async Task<ImportResult> ImportFromDictionaryAsync(...)
{
    // 📊 PHASE 1: INPUT VALIDATION AND LOGGING
    // Validujeme všetky vstupné parametre pred spustením import procesu.
    // Logujeme začiatok operácie s kľúčovými parametrami pre audit trail.
    _logger?.Info("📥 IMPORT START: Importing {RowCount} rows from Dictionary, StartRow={StartRow}, Mode={Mode}", 
        data.Count, startRow, mode);
        
    // Kontrola null referencií - zabránime NullReferenceException v neskoršej fáze
    if (data == null)
    {
        const string errorMsg = "Input data cannot be null";
        _logger?.Error("❌ IMPORT VALIDATION FAILED: {Error}", errorMsg);
        return ImportResult.Failure(errorMsg);
    }
    
    // Boundary validation - startRow musí byť v platnom rozsahu
    if (startRow < 1)
    {
        const string errorMsg = "StartRow must be >= 1 (1-based indexing)";
        _logger?.Error("❌ IMPORT BOUNDARY ERROR: {Error}, provided StartRow={StartRow}", errorMsg, startRow);
        return ImportResult.Failure(errorMsg);
    }
    
    try 
    {
        // 📊 PHASE 2: DATA STRUCTURE ANALYSIS
        // Analyzujeme štruktúru príchodzích dát a mapujeme na existujúce stĺpce
        var columnMappings = AnalyzeDataStructure(data);
        _logger?.Info("📋 COLUMN MAPPING: Found {MappedColumns} valid columns from {TotalKeys} dictionary keys", 
            columnMappings.ValidColumns.Count, columnMappings.TotalKeys);
            
        if (columnMappings.InvalidKeys.Any())
        {
            _logger?.Warning("⚠️ UNMAPPED KEYS: {InvalidKeys} dictionary keys don't match table columns", 
                string.Join(", ", columnMappings.InvalidKeys));
        }
        
        // 📊 PHASE 3: BATCH VALIDATION
        // Validujeme všetky riadky naraz pred importom aby sme zachytili všetky chyby
        _logger?.Info("✅ VALIDATION START: Validating {RowCount} rows with {RuleCount} rules", 
            data.Count, _validationRules.Count);
            
        var validationResults = await ValidateBatchAsync(data, columnMappings);
        
        if (validationResults.HasErrors && !_allowPartialImport)
        {
            _logger?.Error("❌ VALIDATION FAILED: {ErrorCount} validation errors found, import aborted", 
                validationResults.ErrorCount);
            return ImportResult.ValidationFailure(validationResults.Errors);
        }
        
        // ... pokračovanie implementácie s podrobnými komentármi pre každú fázu
    }
    catch (Exception ex)
    {
        // 🚨 CRITICAL ERROR HANDLING
        // Zachytávame všetky neočakávané chyby a logujeme ich s kontextom pre debugging
        _logger?.Error(ex, "🚨 IMPORT CRITICAL ERROR: Unexpected exception during import operation, " +
            "Data.Count={DataCount}, StartRow={StartRow}, Mode={Mode}", 
            data?.Count ?? 0, startRow, mode);
        return ImportResult.CriticalFailure($"Import failed due to unexpected error: {ex.Message}");
    }
}
```


**🔍 POŽADOVANÁ ÚROVEŇ KOMENTÁROV:**
- **XML dokumentácia** pre všetky public metódy/properties
- **Fázové komentáre** pre komplexné operácie (PHASE 1, PHASE 2, atď.)
- **Inline komentáre** vysvetľujúce prečo sa niečo robí (nie len čo)
- **Dátové formáty** - presný popis aké dáta sa očakávajú/produkujú
- **Error handling** - popis všetkých možných chýb a dôvodov
- **Performance notes** - upozornenia na náročné operácie


**Výhody tohto prístupu:**
- **Jednotnosť** - všetky komponenty používajú rovnaké extensions
- **Čitateľnosť** - kratšie metódy (`Info` vs `LogInformation`)
- **Bezpečnosť** - null-safe operátor `?.` všade
- **Vlastné** - nezávislé od externých balíkov, máme kontrolu

#### **📊 Logging Categories**
- **🔧 INITIALIZATION**: `logger?.Info("🔧 Component initialized")`
- **📥 DATA OPERATIONS**: `logger?.Info("📥 Importing {Count} rows", count)`
- **📤 EXPORT**: `logger?.Info("📤 Exporting to {Format}", format)`  
- **✅ VALIDATION**: `logger?.Info("✅ Validation completed")`
- **🗑️ DELETE OPERATIONS**: `logger?.Info("🗑️ Deleting {Count} rows", count)`
- **⚠️ WARNINGS**: `logger?.Warning("⚠️ Performance issue detected")`
- **❌ ERRORS**: `logger?.Error("❌ Operation failed")`
- **🚨 CRITICAL**: `logger?.Error(ex, "🚨 Critical error")`
---
### **🎯 Design Princípy a Patterns**

#### **🔥 Functional Programming Patterns**
```csharp
// Result<T> Monadic Composition
var result = await dataGrid.InitializeAsync(columns)
    .Bind(async success => await dataGrid.ImportFromDatatableAsync(data))
    .Map(importResult => importResult.ImportedRows)
    .OnFailure((error, ex) => logger.Error(ex, "Import failed: {Error}", error));
```

- **🏗️ Result<T> Monads** - Composable error handling bez try-catch
- **📦 Immutable Records** - Thread-safe configuration objekty
- **⚡ Pure Functions** - Predictable data transformations
- **🔄 Reactive Streams** - IObservable pre data changes
- **🎯 Option<T> Types** - Null-safe optional parametre
- **📋 Pattern Matching** - Property patterns, tuple patterns, relational patterns a when guards pre type-safe operation handling

#### **🏢 Object-Oriented Patterns**
```csharp
// Manager Pattern s Dependency Injection
var selectionManager = new DataGridSelectionManager(grid, logger);
var editingManager = new DataGridEditingManager(grid, validationRules, logger);
var eventManager = new DataGridEventManager(grid, selectionManager, editingManager, logger);
```

- **🎯 Manager Pattern** - Specialized UI interaction handlers
- **🔄 Observer Pattern** - Event-driven UI updates
- **🏗️ Builder Pattern** - Fluent configuration APIs
- **🎭 Strategy Pattern** - Pluggable validation rules
- **🔧 Factory Pattern** - Component creation s configuration

---

## 6️⃣ ADVANCEDWINUIDATAGRID - COMPLETE GUIDE

### **🎯 Professional Enterprise DataGrid Component**

**AdvancedWinUiDataGrid** je naša flagship komponent - enterprise-grade tabulkový systém navrhnutý pre handling komplexných business dat s maximálnym výkonom a flexibility.


**Architektúra charakteristiky:**
- ✅ Kompaktný main file (200 lines)
- ✅ Specialized components
- ✅ Clean separation of concerns
- ✅ Testable, maintainable, scalable
- ✅ Memory-efficient, high performance
- ✅ **Anti-God Pattern** - Žiadne god-level súbory s tisíckami lines kódu


### **🏗️ Architektúra Layer-by-Layer**

#### **🎯 CLEAN ARCHITECTURE PRINCIPLES - FULLY IMPLEMENTED**

**✅ DEPENDENCY RULE COMPLIANCE:**
- **Core** → Žiadne dependencies (Pure domain logic)
- **Application** → Závislí len na Core (Business use cases)
- **Infrastructure** → Závislí na Core + Application (External services)
- **UI** → Závislí na všetkých layeroch cez DI (Presentation only)

**✅ SOLID PRINCIPLES - COMPLETE IMPLEMENTATION:**
- **SRP**: Každá trieda má jednu zodpovednosť
- **OCP**: Extensible cez interfaces, closed pre modification
- **LSP**: Všetky implementations respect base interfaces
- **ISP**: Interface segregation s fokusovanými interfaces
- **DIP**: Dependencies injected cez interfaces, not concrete classes

#### **🏛️ Layer 1: Clean API Surface**
```csharp
// Single using statement - Clean API
using RpaWinUiComponentsPackage.DataGrid;

// Unified initialization for both modes
var dataGrid = new DataGrid(logger);

// Same methods with optional UI update control (default: true)
await dataGrid.AddRowAsync(rowData);                    // UI Mode - automatic updates (default)
await dataGrid.AddRowAsync(rowData, updateUI: false);   // Headless Mode - no UI update
await dataGrid.DeleteRowAsync(index);                   // UI updates by default
await dataGrid.DeleteRowAsync(index, updateUI: false);  // Headless - no UI update
await dataGrid.UpdateCellAsync(row, col, value);        // UI updates by default
await dataGrid.UpdateCellAsync(row, col, value, updateUI: false); // Headless

// Manual UI refresh when needed (for headless scenarios)
await dataGrid.RefreshUIAsync(); // Update UI with all pending changes
```

```
📁 Root API Layer:
├── DataGrid.cs                     # Clean public API (400+ lines)
└── LoggerComponent.cs              # Logger integration
```

#### **🧠 Layer 2: Core Functional Layer**
```csharp
// Result<T> Monadic Operations
var result = await dataGrid.InitializeAsync(columns)
    .Bind(async _ => await dataGrid.ImportFromDictionaryAsync(data))
    .Map(importResult => importResult.ImportedRows)
    .OnFailure((error, ex) => logger.Error(ex, error));
```

```
📁 Internal/
├── 🎯 Core/
│   ├── DataGridCoordinator.cs      # Main coordinator
│   │   ├── Monadic data operations
│   │   ├── Reactive stream management  
│   │   ├── Manager composition
│   │   └── Error handling coordination
│   └── DataGridCore.cs             # Core functionality
├── 🔧 Functional/
│   └── Result.cs                   # Result<T> monad  
│       ├── Bind, Map, Tap operations
│       ├── Async monadic chains
│       ├── Error composition
│       └── Collection operations
├── 📋 Interfaces/
│   ├── IDataGridComponent.cs       # Complete API contract
│   ├── IDataGridCore.cs
│   ├── IEditingService.cs
│   ├── IResizeService.cs
│   ├── ISelectionService.cs
│   └── IUIManager.cs
├── 📦 Models/
│   ├── DataGridModels.cs           # Core data models
│   ├── AdvancedDataGridModels.cs   # Advanced configurations  
│   └── ValidationModels.cs         # Validation rules
├── 🏢 Business/
│   └── DataGridBusinessManager.cs  # Pure business logic
├── 🎨 UI/
│   └── DataGridUIManager.cs        # Pure UI operations
├── 🔗 Bridge/                      # 9 specialized bridge components
├── 🤝 Coordination/                # 6 coordination components
├── 🎭 Orchestration/               # 2 orchestration components
├── 🎯 Managers/                    # 5 UI interaction managers
├── 🛠️ Services/                   # Specialized services
└── 📐 Extensions/                  # Helper extensions
```

**📋 Root Level Configurations:**
```
├── ColumnConfiguration.cs          # Column setup
├── ColorConfiguration.cs           # Theme configurations
├── ValidationConfiguration.cs      # Validation rules
└── PerformanceConfiguration.cs     # Performance settings
```

#### **🎮 Layer 3: Professional UI Managers**
```csharp
// Manager Pattern s Dependency Injection
public DataGridEventManager(
    UserControl parentGrid,
    DataGridSelectionManager selectionManager,
    DataGridEditingManager editingManager, 
    DataGridResizeManager resizeManager,
    ILogger? logger)
```

```
📁 Internal/Managers/
├── 🎯 DataGridSelectionManager.cs  # Selection & Focus
│   ├── Single/Multi selection
│   ├── Keyboard navigation  
│   ├── Cell range selection
│   ├── Focus management
│   └── Selection persistence
├── ✏️ DataGridEditingManager.cs    # Cell Editing
│   ├── Inline cell editors (TextBox, ComboBox, DatePicker)
│   ├── Real-time validation during editing
│   ├── Edit mode management
│   ├── Value conversion & formatting
│   └── Validation error visualization
├── 📏 DataGridResizeManager.cs     # Column Resizing
│   ├── Mouse drag resizing
│   ├── Auto-fit to content
│   ├── Min/Max width constraints
│   ├── Proportional resizing
│   └── Resize handle visual feedback
└── 🎪 DataGridEventManager.cs      # Event Coordination
    ├── Centralized event handling
    ├── Keyboard shortcut management
    ├── Mouse interaction coordination
    └── Event simulation for automation
```

### **✅ Advanced Validation System Architecture**

#### **🔐 Comprehensive Multi-Level Validation Framework**

**Podporované typy validácií:**

**1️⃣ Single Cell Validation** - Validácia jednej bunky
**2️⃣ Multiple Rules per Column** - Viac pravidiel na jeden stĺpec  
**3️⃣ Cross-Column Validation** - Validácia cez viac stĺpcov v riadku
**4️⃣ Cross-Row Validation** - Validácia cez viac riadkov
**5️⃣ Cross-Row & Cross-Column** - Kombinovaná validácia cez riadky a stĺpce
**6️⃣ Conditional Validation** - Podmienková validácia
**7️⃣ Business Rule Validation** - Komplexné obchodné pravidlá
**8️⃣ Real-time vs Batch Validation** - Okamžitá alebo dávková validácia

**🔧 Validation Rule Types (každý s optional Priority a RuleName):**
```csharp
// 1️⃣ Single Cell Validation  
public record ValidationRule(
    string ColumnName,
    Func<object?, bool> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int? Priority = null,          // Optional priority
    string? RuleName = null)       // Optional unique rule name

// 2️⃣ Cross-Column Validation (same row)
public record CrossColumnValidationRule(
    string[] DependentColumns,
    Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int? Priority = null,          // Optional priority
    string? RuleName = null)       // Optional unique rule name

// 3️⃣ Cross-Row Validation
public record CrossRowValidationRule(
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, IReadOnlyList<ValidationResult>> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int? Priority = null,          // Optional priority
    string? RuleName = null)       // Optional unique rule name

// 4️⃣ Conditional Validation
public record ConditionalValidationRule(
    string ColumnName,
    Func<IReadOnlyDictionary<string, object?>, bool> Condition,
    ValidationRule ValidationRule,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int? Priority = null,          // Optional priority
    string? RuleName = null)       // Optional unique rule name

// 5️⃣ Complex Validation (Cross-Row & Cross-Column)
public record ComplexValidationRule(
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, ValidationResult> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int? Priority = null,          // Optional priority
    string? RuleName = null)       // Optional unique rule name
```

**🔄 Automatické revalidovanie:**
```csharp
// Cross-column: Ak sa zmení Age alebo Email → automaticky revaliduj toto pravidlo
var crossColumnRule = new CrossColumnValidationRule(
    DependentColumns: ["Age", "Email"],  // Zmena ktoréhokoľvek spustí revalidáciu
    Validator: row => ValidateAgeEmailRule(row),
    ErrorMessage: "If Age > 18, Email required",
    RuleName: "AgeEmailRule"
);

// Cross-row: Ak sa zmení akýkoľvek Email → revaliduj unique pravidlo pre všetky riadky  
var crossRowRule = new CrossRowValidationRule(
    Validator: rows => ValidateUniqueEmails(rows), // Detekuje Email stĺpec automaticky
    ErrorMessage: "Emails must be unique",
    RuleName: "UniqueEmailRule"
);

// Single cell závislé od inej bunky: Ak sa zmení Country → revaliduj PostalCode
var postalCodeRule = new ValidationRule(
    ColumnName: "PostalCode",
    Validator: (value, row) => ValidatePostalForCountry(value, row["Country"]),
    DependentOn: ["Country"], // Ak sa Country zmení → revaliduj PostalCode
    ErrorMessage: "Invalid postal code for this country",
    RuleName: "PostalCodeRule"
);
```

**⚡ Priority-based validation príklady:**
```csharp
// Logické poradie pravidiel pre jeden stĺpec
var emailRequiredRule = new ValidationRule("Email", v => v != null, "Email required", Priority: 1, RuleName: "EmailRequired");
var emailLengthRule = new ValidationRule("Email", v => v.Length > 5, "Email too short", Priority: 2, RuleName: "EmailLength");
var emailFormatRule = new ValidationRule("Email", v => IsValidFormat(v), "Invalid format", Priority: 3, RuleName: "EmailFormat");

// Cross-column s prioritou
var prioritizedCrossRule = new CrossColumnValidationRule(
    ["StartDate", "EndDate"],
    row => ValidateDateRange(row),
    "End date must be after start date",
    Priority: 5,
    RuleName: "DateRangeRule"
);
```

##### **1. Single Cell Validation**
```csharp
public record ValidationRule(
    string ColumnName,
    Func<object?, bool> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int Priority = 0)

// Example: Age validation with multiple rules per column
var validationConfig = new ValidationConfiguration
{
    ColumnValidationRules = new()
    {
        ["Age"] = new List<ValidationRule>
        {
            new() { RuleName = "Required", Validator = v => v != null, ErrorMessage = "Age is required" },
            new() { RuleName = "Range", Validator = v => (int)v >= 18 && (int)v <= 120, ErrorMessage = "Age must be 18-120" },
            new() { RuleName = "Business", Validator = v => IsValidAge((int)v), ErrorMessage = "Invalid business age rule" }
        },
        ["Email"] = new List<ValidationRule>
        {
            new() 
            { 
                RuleName = "Format",
                Validator = email => IsValidEmail(email?.ToString()),
                ErrorMessage = "Invalid email format",
                Severity = ValidationSeverity.Error
            },
            new() 
            { 
                RuleName = "Domain",
                Validator = email => IsValidDomain(email?.ToString()),
                ErrorMessage = "Email domain not allowed",
                Severity = ValidationSeverity.Warning
            }
        }
    }
};
```

##### **2. Multi-Cell Same Row Validation (Cross-Cell)**
```csharp
public record CrossCellValidationRule(
    IReadOnlyList<string> ColumnNames,
    Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error)

// Example: Start/End date validation
var dateRangeRule = new CrossCellValidationRule(
    new[] { "StartDate", "EndDate" },
    row => {
        var start = (DateTime?)row["StartDate"];
        var end = (DateTime?)row["EndDate"];
        return start <= end 
            ? ValidationResult.Success() 
            : ValidationResult.Error("End date must be after start date");
    },
    "Invalid date range");

// CROSS-COLUMN VALIDATIONS
CrossColumnRules = new()
{
    new()
    {
        RuleName = "AgeEmail", 
        Validator = row => ValidateAgeEmailRule(row),
        ErrorMessage = "If Age > 18, Email must be provided",
        DependentColumns = new[] { "Age", "Email" },
        PrimaryColumn = "Age"
    }
}
```

##### **3. Cross-Row Validation**
```csharp
public record CrossRowValidationRule(
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, 
         IReadOnlyList<ValidationResult>> Validator,
    string ErrorMessage)

// CROSS-ROW VALIDATIONS  
CrossRowRules = new()
{
    // Example: Unique email validation
    new CrossRowValidationRule(
        rows => {
            var results = new List<ValidationResult>();
            var emails = new HashSet<string>();
            
            for (int i = 0; i < rows.Count; i++)
            {
                var email = rows[i]["Email"]?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    if (emails.Contains(email))
                    {
                        results.Add(ValidationResult.ErrorForRow(i, "Email must be unique"));
                    }
                    else
                    {
                        emails.Add(email);
                        results.Add(ValidationResult.Success());
                    }
                }
                else
                {
                    results.Add(ValidationResult.Success());
                }
            }
            return results;
        },
        "Duplicate email addresses found"),
    
    new() 
    { 
        RuleName = "TotalSum",
        Validator = rows => ValidateTotalSum(rows),
        ErrorMessage = "Sum of Amount column must equal Total in last row",
        AffectedColumns = new[] { "Amount", "Total" }
    }
}
```

##### **4. Cross-Column Validation**
```csharp
public record CrossColumnValidationRule(
    IReadOnlyList<string> ColumnNames,
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, 
         IReadOnlyList<string>, ValidationResult> Validator,
    string ErrorMessage)

// Example: Sum validation across columns
var budgetSumRule = new CrossColumnValidationRule(
    new[] { "Q1Budget", "Q2Budget", "Q3Budget", "Q4Budget", "TotalBudget" },
    (rows, columns) => {
        foreach (var row in rows)
        {
            var q1 = Convert.ToDecimal(row["Q1Budget"] ?? 0);
            var q2 = Convert.ToDecimal(row["Q2Budget"] ?? 0);
            var q3 = Convert.ToDecimal(row["Q3Budget"] ?? 0);
            var q4 = Convert.ToDecimal(row["Q4Budget"] ?? 0);
            var total = Convert.ToDecimal(row["TotalBudget"] ?? 0);
            
            if (Math.Abs((q1 + q2 + q3 + q4) - total) > 0.01m)
            {
                return ValidationResult.Error($"Total budget mismatch");
            }
        }
        return ValidationResult.Success();
    },
    "Quarterly budget totals don't match");
```

##### **5. Cross-Row Cross-Column Validation**
```csharp
public record ComplexValidationRule(
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, ValidationResult> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error)

// Example: Department budget limits
var departmentBudgetRule = new ComplexValidationRule(
    rows => {
        var departmentTotals = rows
            .GroupBy(r => r["Department"]?.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(r => Convert.ToDecimal(r["Budget"] ?? 0)));
        
        foreach (var dept in departmentTotals)
        {
            if (dept.Value > 1_000_000) // 1M limit
            {
                return ValidationResult.Error($"Department {dept.Key} exceeds budget limit");
            }
        }
        return ValidationResult.Success();
    },
    "Department budget limits exceeded");

// DATASET VALIDATIONS
DatasetRules = new()
{
    departmentBudgetRule,
    new()
    {
        RuleName = "UniqueEmail",
        Validator = dataset => ValidateUniqueEmails(dataset),
        ErrorMessage = "Email addresses must be unique",
        InvolvedColumns = new[] { "Email" }
    }
}
```

##### **6. Conditional Validation Rules**
```csharp
public record ConditionalValidationRule(
    string ColumnName,
    Func<IReadOnlyDictionary<string, object?>, bool> Condition,
    ValidationRule ValidationRule,
    string ErrorMessage)

// Example: Validate phone only if contact method is phone
var conditionalPhoneRule = new ConditionalValidationRule(
    "Phone",
    row => row["ContactMethod"]?.ToString() == "Phone",
    new ValidationRule(
        "Phone",
        value => !string.IsNullOrEmpty(value?.ToString()) && IsValidPhone(value.ToString()),
        "Phone number is required when contact method is Phone"),
    "Conditional phone validation failed");

// Example: Validate manager approval for high amounts
var managerApprovalRule = new ConditionalValidationRule(
    "ManagerApproval",
    row => Convert.ToDecimal(row["Amount"] ?? 0) > 10000,
    new ValidationRule(
        "ManagerApproval",
        value => value is bool approved && approved,
        "Manager approval required for amounts over $10,000"),
    "High amount requires manager approval");
```

**🔧 Jednotné Validation Management API:**
```csharp
// JEDNO API pre všetky typy validácie:
public async Task<Result<bool>> AddValidationRuleAsync<T>(T rule) where T : IValidationRule

// Príklady použitia - jednotlivé pravidlá:
await dataGrid.AddValidationRuleAsync(emailRequiredRule);    // Single cell s Priority
await dataGrid.AddValidationRuleAsync(crossColumnRule);     // Cross-column s RuleName
await dataGrid.AddValidationRuleAsync(crossRowRule);        // Cross-row s RuleName

// Pridanie všetkých email pravidiel jednotlivo:
await dataGrid.AddValidationRuleAsync(emailRequiredRule);
await dataGrid.AddValidationRuleAsync(emailLengthRule);
await dataGrid.AddValidationRuleAsync(emailFormatRule);

// Príklad - definovanie a pridanie v jednom kroku:
var emailRules = new[]
{
    new ValidationRule("Email", v => v != null, "Email required", Priority: 1, RuleName: "EmailRequired"),
    new ValidationRule("Email", v => v.Length > 5, "Email too short", Priority: 2, RuleName: "EmailLength"),
    new ValidationRule("Email", v => IsValidFormat(v), "Invalid format", Priority: 3, RuleName: "EmailFormat")
};

foreach(var rule in emailRules)
{
    await dataGrid.AddValidationRuleAsync(rule);
}

// Remove validation rules:
await dataGrid.RemoveValidationRulesAsync("Age", "Email");           // Podľa stĺpcov
await dataGrid.RemoveValidationRuleAsync("EmailRequired");           // Podľa RuleName
await dataGrid.RemoveValidationRuleAsync("UniqueEmailRule");         // Podľa RuleName
await dataGrid.ClearAllValidationRulesAsync();                       // Všetky pravidlá
```

#### **🗑️ Row Deletion Based on Validation**
```csharp
/// <summary>
/// PROFESSIONAL: Delete rows that meet specified validation criteria
/// ENTERPRISE: Batch operation with progress reporting and rollback support
/// </summary>
/// <param name="validationCriteria">Criteria for determining which rows to delete</param>
/// <param name="options">Deletion options including safety checks</param>
/// <returns>Result with deletion statistics</returns>
public async Task<Result<ValidationBasedDeleteResult>> DeleteRowsWithValidationAsync(
    ValidationDeletionCriteria validationCriteria,
    ValidationDeletionOptions? options = null)

public record ValidationDeletionCriteria(
    ValidationDeletionMode Mode,
    IReadOnlyList<ValidationSeverity>? Severity = null,     // Zoznam závažností na zmazanie
    IReadOnlyList<string>? SpecificRuleNames = null,
    Func<IReadOnlyDictionary<string, object?>, bool>? CustomPredicate = null)

public enum ValidationDeletionMode
{
    DeleteInvalidRows,      // Delete rows that fail validation
    DeleteValidRows,        // Delete rows that pass validation  
    DeleteByCustomRule,     // Delete based on custom predicate
    DeleteBySeverity,       // Delete rows with specific severity levels
    DeleteByRuleName        // Delete rows failing specific named rules
}

// Príklady použitia:
// Zmaž riadky s Error a Warning
await dataGrid.DeleteRowsWithValidationAsync(new ValidationDeletionCriteria(
    Mode: ValidationDeletionMode.DeleteBySeverity,
    Severity: [ValidationSeverity.Error, ValidationSeverity.Warning]
));

// Zmaž riadky ktoré zlyhali na konkrétnych pravidlách
await dataGrid.DeleteRowsWithValidationAsync(new ValidationDeletionCriteria(
    Mode: ValidationDeletionMode.DeleteByRuleName,
    SpecificRuleNames: ["EmailRequired", "UniqueEmail"]
));

// Zmaž podľa vlastnej funkcie
await dataGrid.DeleteRowsWithValidationAsync(new ValidationDeletionCriteria(
    Mode: ValidationDeletionMode.DeleteByCustomRule,
    CustomPredicate: row => (int)(row["Age"] ?? 0) > 65
));

public record ValidationDeletionOptions(
    bool RequireConfirmation = true,        // Vyžaduj potvrdenie pred zmazaním iba pre UI mode pri headles to potvrdenie bude vzdy false. 
    IProgress<ValidationDeletionProgress>? Progress = null  // Progress reporting
)

public record ValidationBasedDeleteResult(
    int TotalRowsEvaluated,
    int RowsDeleted,
    int RemainingRows,
    IReadOnlyList<ValidationError> ValidationErrors,
    TimeSpan OperationDuration)
```

**Usage Examples:**
```csharp
// Delete all rows with validation errors
var errorCriteria = new ValidationDeletionCriteria(
    ValidationDeletionMode.DeleteInvalidRows,
    MinimumSeverity: ValidationSeverity.Error);

var result = await dataGrid.DeleteRowsWithValidationAsync(errorCriteria);

// Delete rows failing specific rules
var specificRuleCriteria = new ValidationDeletionCriteria(
    ValidationDeletionMode.DeleteByRuleName,
    SpecificRuleNames: new[] { "AgeValidation", "EmailValidation" });

// Delete with custom logic
var customCriteria = new ValidationDeletionCriteria(
    ValidationDeletionMode.DeleteByCustomRule,
    CustomPredicate: row => Convert.ToDecimal(row["Amount"] ?? 0) < 0);

var customResult = await dataGrid.DeleteRowsWithValidationAsync(
    customCriteria);
```

#### **⚡ Real-Time Validation Features**

**🔥 Instant Feedback** - Real-time validation
```csharp
// Real-time validácia - okamžite počas písania
dataGrid.EnableRealTimeValidation = true;
dataGrid.ValidationTrigger = ValidationTrigger.OnTextChanged; // Počas písania
```


#### **🚀 ENTERPRISE FEATURES IMPLEMENTED**

**✅ FACTORY METHOD PATTERN:**
```csharp
// UI Mode - Full WinUI3 integration
var uiDataGrid = await DataGrid.CreateForUIAsync(logger, configuration);

// Headless Mode - Server/background processing
var headlessDataGrid = await DataGrid.CreateHeadlessAsync(logger, configuration);
```

**✅ RESULT<T> MONADIC ERROR HANDLING:**
```csharp
// Railway-oriented programming throughout
var initResult = await dataGrid.InitializeAsync(columns, options)
    .Map(result => result.IsSuccess)
    .Bind(success => ImportFromDatatableAsync(data))
    .OnSuccess(result => _logger.LogInformation("Success: {Result}", result))
    .OnFailure((error, ex) => _logger.LogError("Failed: {Error}", error));
```

**✅ CQRS COMMAND PATTERN:**
```csharp
// Commands are immutable with built-in validation
var command = new InitializeDataGridCommand(columns, options, configuration);
var validationErrors = command.Validate();
var result = await applicationService.InitializeAsync(command);
```

**✅ PERFORMANCE MONITORING:**
```csharp
// Automatic performance tracking
using var scope = performanceService.CreateScope("ImportFromDatatableAsync");
scope.RecordMetric("RowCount", data.Count);
// Automatic timing and metrics collection
```

**✅ VALIDATION ENGINE:**
```csharp
// Multi-level validation with business rules
var validationResult = await validationService.ValidateImportAsync(command);
if (validationResult.IsFailure)
{
    return Result.Failure("Validation failed")
        .WithValidationErrors(validationResult.ValidationErrors);
}
```

### **🎯 HYBRID FUNCTIONAL-OOP DESIGN - SKUTOČNÝ HYBRID**

Implementácia je **skutočný hybrid** - nie kompromis, ale strategické použitie oboch paradigiem tam, kde sú najlepšie.

#### **🔄 Functional Programming Časti:**

**1. Result<T> Monadic Error Handling**
```csharp
// FUNCTIONAL: Railway-oriented programming
public readonly struct Result<T>
{
    // Monadic operations pre composable error handling
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    public T ValueOr(T defaultValue)
    
    // Combinator patterns
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> r1, Result<T2> r2)
    public static Result<IReadOnlyList<T>> Traverse(IEnumerable<Result<T>> results)
}
```

**2. Pure Business Logic Functions**
```csharp
// FUNCTIONAL: Pure functions bez side effects
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(object data)
public async Task<Result<ExportResult>> ExportToDictionary(ExportFormat format)
public async Task<Result<ValidationResult>> ValidateAllAsync()
```

**3. Immutable Data Structures**
```csharp
// FUNCTIONAL: Immutable configuration objects
public IReadOnlyList<ColumnDefinition> Columns { get; }
public IReadOnlyList<ValidationError> ValidationErrors { get; }
public readonly struct Option<T> // Optional values
```

**4. Functional Composition**
```csharp
// FUNCTIONAL: Chainable operations
var result = await dataGrid
    .ImportFromDictionaryAsync(data)
    .Bind(async r => await dataGrid.ValidateAllAsync())
    .Map(r => r.IsValid)
    .OnFailure((error, ex) => logger?.Error("Import failed: {Error}", error));
```

#### **🏗️ Object-Oriented Programming Časti:**

**1. UI Management Classes**
```csharp
// OOP: Encapsulation pre WinUI3 integration
public class DataGridUIManager : IUIManager
{
    private Microsoft.UI.Xaml.Controls.DataGrid? _dataGrid;
    
    public async Task<Result<bool>> InitializeAsync(DataGrid dataGrid)
    public async Task<Result<bool>> RefreshUIAsync()
    public async Task<Result<bool>> UpdateColumnsAsync(IReadOnlyList<ColumnDefinition> columns)
}
```

**2. Factory Pattern**
```csharp
// OOP: Factory methods pre creation
public static DataGrid CreateForUI(ILogger? logger = null)
public static DataGrid CreateHeadless(ILogger? logger = null)
```

**3. Interface Contracts**
```csharp
// OOP: Contracts pre dependency injection
internal interface IDataGridCore
internal interface IUIManager
```

**4. Resource Management**
```csharp
// OOP: IDisposable pattern pre resource cleanup
public sealed class DataGrid : IDisposable
{
    public void Dispose() // Proper resource cleanup
}
```

#### **🎯 Why Hybrid? - Strategic Design Decisions:**

**🔄 Functional sa používa pre:**
- **Data Transformations** - Import/export operácie sú pure functions
- **Error Handling** - Result<T> monads eliminujú dangerous exceptions
- **Validation Pipeline** - Composable validation s functional composition
- **Business Logic** - Pure functions sú easy to test a reason about

**🏗️ OOP sa používa pre:**
- **UI Integration** - WinUI3 je inherentne OOP framework
- **Resource Management** - IDisposable, lifecycle management
- **Service Contracts** - Interfaces pre dependency injection
- **Complex State** - UI state management vyžaduje mutable objects

#### **✅ Výsledok: Best of Both Worlds**

```csharp
// HYBRID V AKCII: Functional composition + OOP encapsulation
var dataGrid = DataGrid.CreateForUI(logger);  // OOP: Factory

var result = await dataGrid                    // OOP: Method call
    .ImportFromDictionaryAsync(data)           // FUNCTIONAL: Pure function
    .Bind(r => dataGrid.ValidateAllAsync())    // FUNCTIONAL: Monadic composition
    .Map(r => r.IsValid)                       // FUNCTIONAL: Transformation
    .Tap(isValid => {                          // FUNCTIONAL: Side effect
        if (isValid) 
            MyContainer.Content = dataGrid.UIComponent;  // OOP: UI assignment
    });
    
if (result.IsFailure)  // FUNCTIONAL: Pattern matching
{
    logger?.Error(result.ErrorMessage);  // OOP: Logging service
}
```

#### **📊 Hybrid Benefits:**
✅ **Type Safety** - Functional types + OOP inheritance  
✅ **Composability** - Monadic operations + fluent interfaces  
✅ **Testability** - Pure functions + injectable dependencies  
✅ **Maintainability** - Immutable data + encapsulated behavior  
✅ **Performance** - Functional optimizations + OOP resource management  
✅ **Enterprise Integration** - .NET ecosystem compatibility

### **📋 PROFESSIONAL LOGGING STANDARDS**

#### **🎯 Logging Dependencies - Microsoft.Extensions.Logging**

**Balík používa:**
```xml
<!-- V .csproj balíka - iba abstractions -->
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
```

**V C# kóde balíka:**
```csharp
// Môžeme používať - je súčasťou Abstractions
using Microsoft.Extensions.Logging;  

// Používame len interfaces
ILogger logger;
LogLevel.Information;
```

**Aplikácia môže používať hocijaký logovací systém:**
```csharp
// V aplikácii - môže používať hocijaký logger
using Microsoft.Extensions.Logging;
using Serilog;
using NLog;

// Vytvorenie logger factory s vlastným systémom
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();      // Console logging
    builder.AddSerilog();      // Serilog
    builder.AddNLog();         // NLog  
    builder.AddCustomLogger(); // Vlastný systém
});

var logger = loggerFactory.CreateLogger<MyApp>();
```

**Výhody:**
- **Flexibilita** - použije sa hocijaký logovací systém
- **Žiadne závislosti** - komponent nie je viazaný na konkrétnu implementáciu
- **Enterprise ready** - integrácia s existujúcimi logging systémami

#### **🎯 Vlastné Logger Extensions**

**Problém s štandardným Microsoft.Extensions.Logging:**
```csharp
// ❌ ŠTANDARDNÉ - dlhé, komplikované
logger.LogInformation("Operation completed with {Count} items", count);
logger.LogError(exception, "Failed to process {Operation}", operationName);
```

**Naše riešenie - kratšie a jasnejšie:**
```csharp
// ✅ NAŠE EXTENSIONS - kratšie, jasnejšie  
logger?.Info("Operation completed with {Count} items", count);
logger?.Error(exception, "Failed to process {Operation}", operationName);
```

---

## 3️⃣ ADVANCEDWINUIDATAGRID KOMPONENT

### **🗃️ Profesionálna tabuľka s Clean API Design**

**AdvancedWinUiDataGrid** je pokročilý komponent pre zobrazovanie a editáciu veľkých datasetov s podporou validácie, sortovania, filtrovácia a exportu.


### **📊 Core Import/Export API**

#### **Dictionary Import/Export**
```csharp
// Dictionary Import
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
    List<Dictionary<string, object?>> data,
    Dictionary<int, bool>? checkboxStates = null,
    int startRow = 1,
    ImportMode mode = ImportMode.Replace,
    TimeSpan? timeout = null,
    IProgress<ValidationProgress>? validationProgress = null)

// Dictionary Export
public async Task<List<Dictionary<string, object?>>> ExportToDictionaryAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    bool exportOnlyChecked = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)

// Dictionary Filtered Export - export only from filtered data
public async Task<List<Dictionary<string, object?>>> ExportFilteredToDictionaryAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    bool exportOnlyChecked = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)
```

#### **DataTable Import/Export**
```csharp
// DataTable Import
public async Task<Result<ImportResult>> ImportFromDataTableAsync(
    DataTable dataTable,
    Dictionary<int, bool>? checkboxStates = null,
    int startRow = 1,
    ImportMode mode = ImportMode.Replace,
    TimeSpan? timeout = null,
    IProgress<ValidationProgress>? validationProgress = null)

// DataTable Export
public async Task<DataTable> ExportToDataTableAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    bool exportOnlyChecked = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)

// DataTable Filtered Export - export only from filtered data
public async Task<DataTable> ExportFilteredToDataTableAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    bool exportOnlyChecked = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)

// Get total rows count
public int GetRowsCount()

// Get filtered rows count (same logic as ExportFiltered methods)
public int GetFilteredRowsCount()
```

#### **Import Modes**
```csharp
public enum ImportMode
{
    Append,     // Vloží nové dáta od startRow, pôvodné dáta sa posunú ďalej
    Replace,    // Nahradí dáta od startRow, zvyšok sa zmaže (default)
    Overwrite   // Prepíše dáta od startRow, zvyšok zostane nezmenený
}
```

### **📈 Import/Export Features**
- **🔄 Progress Reporting** - Real-time progress reporting s možnosťou cancellácie operácie
  - `IProgress<ValidationProgress>` pre import validácie
  - `IProgress<ExportProgress>` pre export progress
  - Cancel token support pre prerušenie dlhotrvajúcich operácií
- **✅ Data Validation** - Automatická validácia dát počas importu
  - Real-time validácia každého riadku počas importu
  - Batch validácia pre veľké datasety
  - Validačné chyby sa zobrazia v ValidationAlerts stĺpci
- **🔀 Data Compatibility Check** - Kontrola kompatibility importovaných dát
  - Dictionary keys/DataTable columns ktoré sú v importe ale nie sú v štruktúre sa neimportujú
- **📊 Statistics** - Detailné štatistiky import/export operácií
  - Počet importovaných/exportovaných riadkov
  - Počet validačných chýb
  - Čas trvania operácie
  - Veľkosť spracovaných dát
- **🚨 Error Handling** - Komplexné spracovanie chýb s možnosťou obnovy
  - Partial import pri chybách (importuje správne riadky, označí chybné)
  - Retry mechanizmus pre sieťové chyby
  - Detailné error reporty s presným umiestnením chyby
- **💾 Large Files** - Streaming podpora pre veľké datasety (GB+)
  - Memory-efficient spracovanie veľkých datasetov
  - Chunk-based processing pre GB+ datasety
  - Background processing s progress reporting
- **🎯 Selective Export** - Export špecifických častí dát
  - Export vybraných stĺpcov
  - Export vybraných riadkov (checked rows)
  - Export filtered dát (iba zobrazené riadky po filtrovaní)
  - Export ranges (od-do riadku)

### **🔄 Intelligent Row Management:**
```csharp
// Definovaný minimálny počet riadkov z aplikácie
await dataGrid.InitializeAsync(columns, emptyRowsCount: 15);  // Minimum 15 riadkov

// Automatic row expansion
// Ak paste/import prinesie viac riadkov → tabuľka sa rozšíri
// Vždy zostane +1 prázdny riadok na konci pre nové dáta

// Smart delete behavior
// Riadky > definovaný počet: DELETE = zmaže kompletný riadok 
// (pričom ale vždy zostane na konci prázdny riadok, čiže ak zmaže všetky 
// riadky až po minimálny počet a posledný riadok minimálneho počtu je vyplnený, 
// tak sa na konci vytvorí nový prázdny riadok)
// Riadky <= definovaný počet: DELETE = vyčistí iba obsah (zachová štruktúru)
```

#### **Import/Export Parameters Explanation**

##### **Import Parameters:**
- **`checkboxStates`** - Dictionary mapping row indices to checkbox states (relevant only if CheckBox column is enabled)
  - If CheckBox column is visible: Maps row indices to their checkbox values
  - If CheckBox column is hidden: Determines which rows should have internal checkbox state set to true
  - If null: All imported rows get default checkbox state (false)
- **`startRow`** - Starting row index for import (default = 1)
- **`mode`** - Import behavior:
  - `Replace` (default): Nahradí dáta od startRow, zvyšok sa zmaže
  - `Append`: Vloží nové dáta od startRow, pôvodné dáta sa posunú ďalej
  - `Overwrite`: Prepíše dáta od startRow, zvyšok zostane nezmenený
- **`timeout`** - Operation timeout (default: 1 minute for large datasets)
- **`validationProgress`** - Progress reporting for real-time validation during import

##### **Export Parameters:**
- **`includeValidAlerts`** - Whether to include ValidationAlerts column in export:
  - `false` (default): ValidationAlerts column excluded from export data
  - `true`: Export includes ValidationAlerts column with error descriptions
  - Note: Only applies if ValidationAlerts column exists and contains data
- **`removeAfter`** - Post-export behavior:
  - `false` (default): Keep data in grid after successful export
  - `true`: Clear data from grid after successful export (useful for batch processing)
- **`timeout`** - Operation timeout for large dataset exports
- **`exportProgress`** - Progress tracking for UI feedback during export

##### **CheckBox Column Logic:**
```csharp
// When importing with checkboxStates and CheckBox column is hidden:
var checkboxStates = new Dictionary<int, bool>
{
    { 0, true },   // Row 0 will have internal checkbox = true
    { 2, true },   // Row 2 will have internal checkbox = true
    { 4, false }   // Row 4 will have internal checkbox = false
};

// If checkbox column becomes visible later, these states will be displayed
await dataGrid.ImportFromDictionaryAsync(data, checkboxStates);

// When exporting, can filter by checkbox states even if column is hidden:
var checkedRowsData = await dataGrid.ExportFilteredToDictionaryAsync(
    includeValidAlerts: false,
    exportOnlyChecked: true);  // Only export rows with checkbox = true
```

##### **ValidationAlerts Export Logic:**
```csharp
// Export with validation alerts included
var dataWithAlerts = await dataGrid.ExportToDictionaryAsync(
    includeValidAlerts: true);

// Result includes ValidationAlerts column:
// [
//   { "Name": "John", "Age": 25, "ValidationAlerts": "" },
//   { "Name": "Jane", "Age": 17, "ValidationAlerts": "Age must be >= 18" },
//   { "Name": "Bob", "Age": 30, "ValidationAlerts": "" }
// ]

// Export without validation alerts (default)
var cleanData = await dataGrid.ExportToDictionaryAsync();
// Result excludes ValidationAlerts column:
// [
//   { "Name": "John", "Age": 25 },
//   { "Name": "Jane", "Age": 17 },
//   { "Name": "Bob", "Age": 30 }

#### **🌈 Comprehensive Color Configuration**

**Dostupné farby pre konfiguráciu (všetky majú default hodnoty):**
```csharp
// CELL COLORS - Professional defaults
CellBackground = "#FFFFFF",         // Pure white (default)
CellForeground = "#000000",         // Pure black text (default)
CellBorder = "#E0E0E0",            // Light gray (default)

// HEADER COLORS
HeaderBackground = "#F5F5F5",       // Light gray (default)
HeaderForeground = "#333333",       // Dark gray text (default)

// SELECTION COLORS - Microsoft design system
SelectionBackground = "#0078D4",    // Microsoft blue (default)
SelectionForeground = "#FFFFFF",    // White text (default)

// VALIDATION COLORS - Semantic colors (Error má najvyššiu prioritu)
ValidationErrorBorder = "#FF0000",      // Red border (default)
ValidationErrorBackground = "#FFEBEE",  // Light red background (default)
ValidationErrorForeground = "#D32F2F",  // Dark red text (default)

ValidationWarningBorder = "#FF9800",    // Orange border (default)
ValidationWarningBackground = "#FFF3E0", // Light orange background (default)
ValidationWarningForeground = "#F57C00", // Dark orange text (default)

// ZEBRA STRIPES
EnableZebraStripes = true,              // Enable/disable (default)
AlternateRowBackground = "#FAFAFA",     // Very light gray (default)
```

**Použitie z aplikácie:**
```csharp
// KONFIGURÁCIA Z APLIKÁCIE (package sa pripája cez PackageReference)
// Helper class zobrazí všetky dostupné color properties v IntelliSense
dataGrid.SetColorConfiguration(
    // Iba tie farby ktoré chcem zmeniť oproti default
    CellBackground: "#F8F8F8",           // Custom light gray namiesto white
    ValidationErrorForeground: "#CC0000", // Custom darker red
    SelectionBackground: "#007ACC",       // Custom VS Code blue
    // Ostatné farby zostanú default
);
```

**📦 Package Integration:**
```xml
<!-- Aplikácia pripája balík cez PackageReference v .csproj -->
<PackageReference Include="RpaWinUiComponentsPackage" Version="1.0.0" />
```

**💡 Konfigurácia z aplikácie:**
- Všetky farby majú svoje **default hodnoty** v komponente
- Aplikácia môže predefinovať **iba tie farby ktoré chce zmeniť** cez `SetColorConfiguration()`
- Helper class zobrazí všetky dostupné color properties v IntelliSense/Visual Studio
- Balík sa vždy pripája cez **PackageReference**

**VALIDATION PRIORITY RULES (text + background + border):**
- Error > Warning > Info
- Info nezmeňuje farby (používa CellForeground + CellBackground + CellBorder)
- Ak bunka má Error + Warning → ValidationErrorForeground + CellBackground + ValidationErrorBorder
- Ak bunka má Warning + Info → ValidationWarningForeground + CellBackground + ValidationWarningBorder  
- Ak bunka má iba Info → CellForeground + CellBackground + CellBorder
- Selected bunka → SelectionForeground + SelectionBackground + CellBorder (bez ohľadu na validation)
```

**⚡ Smart Validation Modes:**
```csharp
// Real-time validation (jednotlivé zmeny)
dataGrid.OnCellChanged += (cell) => ValidateCell(cell);  // Immediate

// Bulk validation (paste, import)
await dataGrid.ImportFromExcelAsync(data, bulkValidation: true);
// 1. Import all data first (fast)
// 2. Then validate everything (batch)

// Full dataset validation (VŠETKY riadky, nie len zobrazené)
bool allValid = await dataGrid.AreAllNonEmptyRowsValidAsync(onlyfiltered); // Default false -> ak true tak iba tie vyfiltrovane data.
// Validuje KOMPLETNE všetky neprazdne riadky v dataset, nie len viewport


### **🔍 Advanced Search & Filter System**

#### **🔎 Multi-Level Search**
```csharp
// BASIC SEARCH
var searchResults = await dataGrid.SearchAsync(
    "John Smith", 
    targetColumns: new[] { "FirstName", "LastName" },
    caseSensitive: false);

// ADVANCED SEARCH
var advancedResults = await dataGrid.AdvancedSearchAsync(new AdvancedSearchCriteria
{
    SearchText = ".*@company\\.com$",  // Regex support
    TargetColumns = new[] { "Email" },
    UseRegex = true,
    CaseSensitive = false,
    Scope = SearchScope.AllData,  // AllData, VisibleData, SelectedData
    MaxMatches = null  // Default null = find all matches
});

// FILTER OPERATIONS
await dataGrid.ApplyFiltersAsync(new[]
{
    new AdvancedFilter 
    { 
        ColumnName = "Age", 
        Operator = FilterOperator.GreaterThan, 
        Value = 18 
    },
    new AdvancedFilter 
    { 
        ColumnName = "Department", 
        Operator = FilterOperator.Contains, 
        Value = "Engineering",
        LogicOperator = FilterLogicOperator.And
    }
});

// COMPLEX FILTER WITH GROUPING: (Age > 18 AND Department = "IT") OR (Salary > 50000)
await dataGrid.ApplyFiltersAsync(new[]
{
    new AdvancedFilter 
    { 
        ColumnName = "Age", 
        Operator = FilterOperator.GreaterThan, 
        Value = 18,
        GroupStart = true  // Start group
    },
    new AdvancedFilter 
    { 
        ColumnName = "Department", 
        Operator = FilterOperator.Equals, 
        Value = "IT",
        LogicOperator = FilterLogicOperator.And,
        GroupEnd = true  // End group
    },
    new AdvancedFilter 
    { 
        ColumnName = "Salary", 
        Operator = FilterOperator.GreaterThan, 
        Value = 50000,
        LogicOperator = FilterLogicOperator.Or
    }
});
```
**🔄 Intelligent Row Management:**
```csharp
// Definovaný minimálny počet riadkov z aplikácie
await dataGrid.InitializeAsync(columns, emptyRowsCount: 15);  // Minimum 15 riadkov

// Automatic row expansion
// Ak paste/import prinesie viac riadkov → tabuľka sa rozšíri
// Vždy zostane +1 prázdny riadok na konci pre nové dáta

// Smart delete behavior
// Riadky > definovaný počet: DELETE = zmaže kompletný riadok
// Riadky <= definovaný počet: DELETE = vyčistí iba obsah (zachová štruktúru)
```

**⌨️ Complete Keyboard Shortcuts:**
```csharp
// === EDIT MODE SHORTCUTS ===
// ESC           - Zruší zmeny v bunke a ukončí edit mód
// Enter         - Potvrdí zmeny a zostane na bunke  
// Shift+Enter   - Vloží nový riadok do bunky (multiline editing)
// Tab (in edit) - Vloží tab znak do bunky

// === NAVIGATION SHORTCUTS ===
// Arrow Keys    - Navigácia medzi bunkami s auto-commit zmien
// Tab           - Ďalšia bunka (doprava → koniec riadku → prvá v novom riadku)

// === SELECTION SHORTCUTS ===
// Ctrl+A        - Označí všetky bunky (okrem DeleteRows column ak je zapnutý a chceckBox column (special) ak je zapnuty )
// Shift+Tab     - Predchádzajúca bunka (doľava → začiatok riadku → posledná v predošlom)
// Ctrl+Home     - Prvá bunka v tabuľke
// Ctrl+End      - Posledná bunka s dátami
// Ctrl+ click, click, click - Toggle selection (pridať/odobrať z výberu)
// drag and drop - znaci bunky z vyberu od do.

// === COPY/PASTE/CUT SHORTCUTS ===
// Ctrl+C        - Copy vybraných buniek do clipboardu
// Ctrl+V        - Paste z clipboardu (s intelligent row expansion)
// Ctrl+X        - Cut vybraných buniek

// === ROW OPERATIONS ===
// Delete        - Smart delete (content vs. whole row based on row count)
// Ctrl+Delete   - Delete kompletný riadok (ak je nad minimum, ak nie je tak data z celeho riadku)
// Insert        - Vloží nový riadok nad aktuálny
// ctrl+f        - search mode (on/off)
// shift+l       - filter mode (on/off) (podobne ako v exceli)
```


## 6️⃣ RESULT<T> MONADIC ERROR HANDLING

### **🔥 Professional Error Handling**

**Result<T>** je naše implementation of monadic error handling pattern, ktorý eliminuje potrebu try-catch blocks a poskytuje composable error handling.

#### **🎯 Core Concepts**
```csharp
// Traditional approach - verbose, error-prone
try 
{
    var data = await LoadDataAsync();
    var processed = ProcessData(data);
    var saved = await SaveDataAsync(processed);
    return saved;
}
catch (Exception ex)
{
    logger.Error(ex, "Operation failed");
    return null; // Loss of error information
}

// Result<T> approach - clean, composable
var result = await Result<DataModel>.Try(LoadDataAsync)
    .Bind(data => Result<ProcessedData>.Try(() => ProcessData(data)))
    .Bind(async processed => await Result<bool>.Try(() => SaveDataAsync(processed)))
    .OnFailure((error, ex) => logger.Error(ex, "Operation failed: {Error}", error));

return result.ValueOr(false);
```

#### **🔧 Monadic Operations**
```csharp
// BIND - Chain operations that may fail
var result = await dataGrid.InitializeAsync(columns)
    .Bind(async success => await dataGrid.ImportDataAsync(data))
    .Bind(async importResult => await dataGrid.ValidateAllAsync());

// MAP - Transform successful values
var rowCount = await dataGrid.ImportDataAsync(data)
    .Map(importResult => importResult.ImportedRows);

// TAP - Execute side effects
var result = await dataGrid.ImportDataAsync(data)
    .Tap(importResult => logger.Info("Imported {Rows} rows", importResult.ImportedRows))
    .Tap(async importResult => await NotifyUsersAsync(importResult));

// COMBINE - Combine multiple results
var combinedResult = Result<bool>.Combine(
    await dataGrid.InitializeAsync(columns),
    await logger.InitializeAsync(config),
    await database.ConnectAsync()
);
```

#### **📊 Error Handling Benefits**
- **🚫 No Exceptions** - Errors are values, not exceptions
- **🔗 Composable** - Chain operations s automatic error propagation  
- **📊 Rich Information** - Preserve error context throughout chain
- **⚡ Performance** - No expensive exception unwinding
- **🧪 Testable** - Easy to test error paths
- **📋 Type Safe** - Compile-time error handling verification

---

## 7️⃣ CONFIGURATION CLASSES

### **🎯 CLEAN PUBLIC API KONFIGURAČNÉ CLASSES**

**DÔLEŽITÉ:** Konfiguračné classes sú súčasťou každého komponentu v jeho namespace pre clean API separation:

```csharp
// Pre DataGrid konfiguračné classes:
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

// Pre Logger konfiguračné classes:
using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
```

### **📋 DataGrid Configuration Classes**

#### **ColumnConfiguration** - Konfigurácia stĺpcov
```csharp
namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// Clean API konfigurácia pre DataGrid stĺpce
/// Všetky vlastnosti sú voliteľné s rozumnými defaultmi
/// </summary>
public class ColumnConfiguration
{
    public string Name { get; set; } = "";                     // Column identifier (POVINNÝ)
    public string? DisplayName { get; set; }                   // UI display name (fallback na Name)
    public Type Type { get; set; } = typeof(string);           // Data type
    public int Width { get; set; } = 100;                      // Column width
    public int? MinWidth { get; set; }                         // Minimum column width
    public int? MaxWidth { get; set; }                         // Maximum column width
    public bool IsRequired { get; set; } = false;              // Required in import
    public bool IsReadOnly { get; set; } = false;              // Read-only column
    public object? DefaultValue { get; set; }                  // Default value
    public int? MaxLength { get; set; }                        // Max text length
    public string? ValidationPattern { get; set; }             // Regex validation
    public bool IsValidationColumn { get; set; } = false;      // Special validation column
    public bool IsDeleteColumn { get; set; } = false;          // Special delete button
    public bool IsCheckBoxColumn { get; set; } = false;        // Checkbox column
    public bool IsVisible { get; set; } = true;                // Column visibility
    public bool CanResize { get; set; } = true;                // Resizable
    public bool CanSort { get; set; } = true;                  // Sortable
    public bool CanFilter { get; set; } = true;                // Filterable
}
```

#### **DataGridColors** - Farby a témy
```csharp
namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// Clean API konfigurácia farieb pre DataGrid
/// Všetky farby sú voliteľné - použijú sa default hodnoty ak nie sú nastavené
/// </summary>
public class DataGridColors
{
    // ZÁKLADNÉ FARBY BUNIEK
    public string? CellBackground { get; set; }                // Pozadie buniek
    public string? CellForeground { get; set; }                // Text buniek
    public string? CellBorder { get; set; }                    // Okraje buniek
    
    // FARBY HLAVIČKY
    public string? HeaderBackground { get; set; }              // Pozadie hlavičky
    public string? HeaderForeground { get; set; }              // Text hlavičky
    public string? HeaderBorder { get; set; }                  // Okraje hlavičky
    
    // FARBY OZNAČENIA/SELECTION
    public string? SelectionBackground { get; set; }           // Pozadie pri označení
    public string? SelectionForeground { get; set; }           // Text pri označení
    
    // VALIDAČNÉ FARBY
    public string? ValidationErrorBorder { get; set; }         // Okraj chýb
    public string? ValidationErrorBackground { get; set; }     // Pozadie chýb
    public string? ValidationWarningBorder { get; set; }       // Okraj varovaní
    public string? ValidationWarningBackground { get; set; }   // Pozadie varovaní
    
    // FOCUS FARBY
    public string? FocusBorder { get; set; }                   // Okraj pri focus
    public string? FocusBackground { get; set; }               // Pozadie pri focus

)

#### **Memory Management**
- ✅ **Weak Reference Caching** - Automatic memory cleanup
- ✅ **Aggressive Disposal** - Proper resource cleanup
- ✅ **Object Pooling** - Reuse of UI elements
- ✅ **Background GC** - Scheduled garbage collection


### **Performance Monitoring**
```csharp
// Built-in performance metrics
var metrics = await dataGrid.GetPerformanceMetricsAsync();

logger?.Info($"""
    Performance Metrics:
    - Total Rows: {metrics.TotalRows:N0}  
    - Visible Rows: {metrics.VisibleRows:N0}
    - Memory Usage: {metrics.MemoryUsageBytes / 1024 / 1024:N1} MB
    - UI Frame Rate: {metrics.UIFrameRate:F1} FPS
    - Cache Hit Rate: {metrics.CacheHitRate}%
    - Last Operation: {metrics.LastOperationDuration.TotalMilliseconds:N0}ms
    """);
```

## 🎯 DESIGN ROZHODNUTIA

### **Prečo Hybrid Functional-OOP?**

#### **Functional Pre Core Logic**
```csharp
// Prečo functional pre data operations:
var result = await dataGrid.ImportDataAsync(data)    // ✅ Immutable input
    .Bind(importResult => ValidateData(importResult)) // ✅ Composable
    .Map(validData => TransformData(validData))       // ✅ Pure transformation
    .OnFailure((error, ex) => LogError(error, ex));   // ✅ Explicit error handling

// Vs problematic OOP approach:
try 
{
    dataGrid.ImportData(data);              // ❌ Throws exceptions
    if (!dataGrid.Validate()) {             // ❌ Side effects
        throw new Exception("Invalid");      // ❌ Exception-based control flow
    }
    var transformed = dataGrid.Transform(); // ❌ Mutable state
}
catch (Exception ex) { ... }               // ❌ Catch-all exception handling
```

#### **OOP Pre UI Interactions**
```csharp
// Prečo OOP pre UI:
public class DataGridSelectionManager : ISelectionManager  // ✅ Clear responsibility
{
    public event EventHandler<SelectionChangedEventArgs> SelectionChanged; // ✅ Event-driven
    
    public async Task<bool> SelectCellAsync(int row, int col) // ✅ Stateful operations
    {
        // UI state management requires mutation
        _selectedCell = GetCellAt(row, col);  // ✅ Managed mutable state
        UpdateVisualFeedback();              // ✅ Side effects are expected
        SelectionChanged?.Invoke(this, ...); // ✅ Event notification
        return true;
    }
}

// Functional approach would be problematic pre UI:
// SelectionState UpdateSelection(SelectionState current, SelectionAction action)
// ❌ UI requires immediate visual feedback
// ❌ Event coordination is complex with immutable state
// ❌ Performance overhead of recreating UI state
```

### **Prečo Modular Manager Architecture?**

#### **Single Responsibility Principle**
```csharp
// PRED: God-level file (3,980 lines)
public class AdvancedDataGrid : UserControl
{
    // Selection logic (600+ lines)
    private void HandleCellClick(...) { ... }
    private void HandleDragSelection(...) { ... }
    
    // Editing logic (500+ lines)  
    private void StartEditing(...) { ... }
    private void ValidateCell(...) { ... }
    
    // Resizing logic (400+ lines)
    private void HandleResize(...) { ... }
    private void UpdateColumnWidth(...) { ... }
    
    // Event handling (500+ lines)
    private void OnKeyDown(...) { ... }
    private void OnPointerPressed(...) { ... }
    
    // ❌ Mixing všetkých concerns v jednom súbore
    // ❌ Ťažko testovateľné
    // ❌ Vysoká komplexita
}

// PO: Modular architecture  
public class DataGridCoordinator  // ✅ Orchestration only
{
    private readonly ISelectionManager _selection; // ✅ Focused responsibility
    private readonly IEditingManager _editing;     // ✅ Independent testing
    private readonly IResizeManager _resize;       // ✅ Clean interfaces
    private readonly IEventManager _events;       // ✅ Dependency injection
}
```

### **Prečo Result<T> Namiesto Exceptions?**

#### **Functional Error Handling**
```csharp
// PRED: Exception-based approach
public async Task ImportData(List<Dictionary<string, object?>> data)
{
    try 
    {
        ValidateData(data);        // ❌ Môže hodiť ValidationException
        ProcessData(data);         // ❌ Môže hodiť ProcessingException  
        UpdateUI();                // ❌ Môže hodiť UIException
    }
    catch (ValidationException ex) { ... }   // ❌ Specific catch blocks
    catch (ProcessingException ex) { ... }   // ❌ Exception hierarchy needed
    catch (UIException ex) { ... }           // ❌ Different handling logic
    catch (Exception ex) { ... }             // ❌ Catch-all for unknown errors
}

// PO: Monadic error handling
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(IReadOnlyList<IReadOnlyDictionary<string, object?>> data)
{
    return await ValidateDataAsync(data)         // ✅ Returns Result<ValidationResult>
        .Bind(validData => ProcessDataAsync(validData))  // ✅ Composable
        .Bind(processedData => UpdateUIAsync(processedData))  // ✅ Chain operations
        .Map(uiResult => CreateImportSummary(uiResult));      // ✅ Transform result

    // ✅ Všetky errors sú handled explicitly
    // ✅ No exception hierarchy needed  
    // ✅ Composable error handling
    // ✅ Type-safe error information
}
```

### **Prečo Clean API s Hidden Internals?**

#### **API Surface Control**
```csharp
// PRED: Everything public
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.API;           // ❌ Internal API exposed
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain;        // ❌ Domain models public  
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;      // ❌ Services exposed
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Modules;       // ❌ Implementation details

// Applications could access internals:
var internalService = new DataGridService();  // ❌ Breaking encapsulation
var domainModel = new ColumnDefinition();     // ❌ Tight coupling
// ❌ Version changes break applications

// PO: Clean API boundary
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;     // ✅ Single namespace only

var dataGrid = new DataGrid();                // ✅ Clean public interface
// ❌ RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.* not accessible
// ✅ Internal changes don't break applications
// ✅ Clear API contract
// ✅ Version-safe upgrades
```

### **Prečo Reactive Streams?**

#### **Event Coordination**
```csharp
// PRED: Traditional event handling
_selectionManager.SelectionChanged += OnSelectionChanged;
_editingManager.ValueChanged += OnValueChanged;
_resizeManager.ResizeEnded += OnResizeEnded;

private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    // ❌ Imperative event handling
    // ❌ Difficult to compose
    // ❌ No built-in error handling
    // ❌ Hard to test event chains
}

// PO: Reactive streams
coordinator.DataChanges
    .Where(change => IsImportantChange(change))           // ✅ Functional filtering
    .Throttle(TimeSpan.FromMilliseconds(100))            // ✅ Built-in operators
    .DistinctUntilChanged()                              // ✅ Duplicate elimination
    .Subscribe(
        change => HandleChange(change),                   // ✅ Success handling
        error => logger.LogError(error, "Stream error"), // ✅ Error handling
        () => logger.LogDebug("Stream completed")        // ✅ Completion handling
    );

// ✅ Composable event processing
// ✅ Built-in error handling
// ✅ Easy testing with marble diagrams
// ✅ Memory-efficient event processing
```

## 📚 ZÁVER

RpaWinUiComponentsPackage je **enterprise-grade professional solution** navrhnutý s použitím najlepších praktík moderného software developmentu:

### **🏆 Key Achievements**
- ✅ **95% Reduction** 
- ✅ **Professional Architecture** s clear separation of concerns
- ✅ **Hybrid Functional-OOP** optimalizované pre rôzne use cases
- ✅ **Clean API Design** s single using statements  
- ✅ **Enterprise Scalability** pre milióny riadkov
- ✅ **Comprehensive Error Handling** s monadic composition
- ✅ **Independent Components** s clean integráciou

### **🎯 Design Philosophy**
1. **Functional First** - Pre data operations, transformations, error handling
2. **OOP Where Better** - Pre UI interactions, state management, events
3. **Clean Boundaries** - Hidden implementation, exposed clean API
4. **Composable Operations** - Monadic error handling, reactive streams
5. **Performance Focused** - Optimalized pre real-world large datasets

### **🚀 Production Ready**
Balík je **production-ready** pre enterprise aplikácie s kompletným:
- **Testing** cez demo aplikáciu
- **Documentation** s usage examples
- **Error Handling** na všetkých úrovniach  
- **Performance Optimization** pre scalability
- **Clean Integration** do existujúcich aplikácií
Balík je navrhnutý pre **continuous evolution** s:
- **Modular Extensions** - Ľahko pridať nové features
- **API Stability** - Internal changes nevyžadujú zmeny v aplikáciách
- **Performance Scaling** - Architecture ready pre ďalšie optimalizácie
- **Feature Growth** - Clean foundation pre advanced features

Toto je ukážka **professional-grade software architecture** ako ju vytvára **top developer v najlepšej firme**! 🌟