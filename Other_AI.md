PROFESIONÁLNY PROMPT PRE VÝVOJ RPAWINUICOMPONENTSPACKAGE
KONTEXTOVÁ ŠPECIFIKÁCIA PRE AI SYSTÉM
Konajte ako senior full-stack C# developer a softvérový architekt pre špičkovú developerskú spoločnosť. Vašou úlohou je vytvoriť a implementovať enterprise-grade balík komponentov pre WinUI3 aplikácie v .NET 8, ktorý obsahuje dva plne nezávislé, profesionálne komponenty s najvyššou kvalitou kódu a architektúry.
1. ZÁKLADNÉ ARCHITEKTÚRNE POŽIADAVKY
1.1 Technologická špecifikácia

Target Framework: .NET 8.0-windows10.0.19041.0
UI Framework: WinUI3 (Windows App SDK najnovšia verzia)
Balík názov: RpaWinUiComponentsPackage
Architektúra: Clean Architecture + SOLID princípy
Paradigma: Hybrid Functional-OOP
DI: Microsoft.Extensions.DependencyInjection

1.2 Štruktúra riešenia
RpaWinUiComponentsPackage/
├── AdvancedWinUiLogger/
│   ├── Core/
│   ├── Infrastructure/
│   ├── Extensions/
│   └── Models/
├── AdvancedWinUiDataGrid/
│   ├── Core/
│   ├── Business/
│   ├── UI/
│   ├── Managers/
│   └── Models/
├── Demo/
│   └── RpaWinUiComponents.Demo/
└── Tests/
    ├── AdvancedWinUiLogger.Tests/
    └── AdvancedWinUiDataGrid.Tests/
1.3 Kľúčové princípy implementácie
ELIMINOVANIE "GOD-LEVEL" SÚBOROV:


Single Responsibility Principle pre každú triedu
Použitie partial classes pre rozdelenie komplexnej logiky

CLEAN ARCHITECTURE:
csharp// Core vrstva - abstrakcie a entity (žiadne závislosti)
namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core
{
    public interface IDataGridService
    {
        Task<Result<ImportResult>> ImportFromDictionaryAsync(
            List<Dictionary<string, object?>> data,
            bool updateUI = true);
    }
}

// Business vrstva - biznis logika (závisí len na Core)
namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Business
{
    public class DataGridBusinessManager : IDataGridService
    {
        // Čistá biznis logika bez UI závislostí
    }
}

// UI vrstva - prezentačná vrstva (závisí na Business cez rozhrania)
namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.UI
{
    public class DataGridUIDecorator : IDataGridService
    {
        private readonly IDataGridService _businessManager;
        // Decorator pattern pre pridanie UI logiky
    }
}
2. KOMPONENT I: ADVANCEDWINUILOGGER
2.1 Architektúra a požiadavky
ZÁKLADNÉ CHARAKTERISTIKY:

Žiadne UI závislosti - čisto .NET knižnica
Postavený na Microsoft.Extensions.Logging.Abstractions
Asynchrónny file logging s rotáciou
Thread-safe operácie
Konfigurovateľné cez LoggerOptions

2.2 Implementačná špecifikácia
csharp// Rozhranie pre konfiguráciu
public class LoggerOptions
{
    public string LogDirectory { get; set; } = "";
    public string BaseFileName { get; set; } = "app";
    public int MaxFileSizeMB { get; set; } = 10;
    public int MaxBackupFiles { get; set; } = 5;
    public bool UseTimestampInName { get; set; } = true;
}

// Extension pre jednoduché používanie
public static class LoggerExtensions
{
    public static void Info(this ILogger? logger, string message, params object[] args)
        => logger?.LogInformation(message, args);
        
    public static void Error(this ILogger? logger, Exception exception, string message, params object[] args)
        => logger?.LogError(exception, message, args);
}

// Registrácia v DI
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdvancedWinUiLogger(
        this IServiceCollection services, 
        Action<LoggerOptions>? configure = null)
    {
        // Implementácia registrácie FileLoggerProvider
        return services;
    }
}
FUNKČNÉ POŽIADAVKY:

Rotácia súborov: app.log → app_1.log → app_2.log atď.
Logovanie na začiatku každej metódy: logger?.Info("🔧 Entering {MethodName}", nameof(MethodName))
Kompletné chybové logovanie: Všetky exceptions s full stack trace
Asynchrónny zápis: Non-blocking file operations
Konfigurovateľnosť: Dynamické nastavenie ciest, veľkostí, rotácie

3. KOMPONENT II: ADVANCEDWINUIDATAGRID
3.1 Architektúrna vízia a refaktoring
PROBLÉM: Existujúci AdvancedDataGrid.xaml.cs má 3345 riadkov (GOD-LEVEL súbor)
RIEŠENIE: Rozdelenie na specialized partial classes:
csharp// AdvancedDataGrid.xaml.cs (hlavný súbor ~200 riadkov)
public sealed partial class AdvancedDataGrid : UserControl
{
    private readonly IDataGridService _dataService;
    private readonly DataGridSelectionManager _selectionManager;
    private readonly DataGridEditingManager _editingManager;
    
    public AdvancedDataGrid(IDataGridService dataService, ILogger<AdvancedDataGrid> logger)
    {
        _dataService = dataService;
        InitializeComponent();
        InitializeManagers();
    }
}

// AdvancedDataGrid.EventHandlers.cs (~200 riadkov)
public sealed partial class AdvancedDataGrid
{
    private void OnCellClick(object sender, RoutedEventArgs e) { /* delegovanie na managery */ }
    private void OnKeyDown(object sender, KeyRoutedEventArgs e) { /* keyboard handling */ }
}

// AdvancedDataGrid.Selection.cs (~200 riadkov)  
public sealed partial class AdvancedDataGrid
{
    private void HandleCellSelection(int row, int column) { /* selection logic */ }
    private void UpdateSelectionVisuals() { /* visual feedback */ }
}
3.2 Duálne API - UI vs Headless režim
DECORATOR PATTERN implementácia:
csharp// 1. Základné rozhranie (zdieľané API)
public interface IDataGridService
{
    Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        bool updateUI = true);
        
    Task<List<Dictionary<string, object?>>> ExportToDictionaryAsync(
        bool includeValidAlerts = false,
        bool removeAfter = false);
        
    Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData, bool updateUI = true);
    Task<Result<bool>> DeleteRowAsync(int index, bool updateUI = true);
    Task RefreshUIAsync(); // Manuálne UI refresh pre headless → UI prechod
}

// 2. HEADLESS implementácia (čistá biznis logika)
public class DataGridBusinessManager : IDataGridService
{
    private readonly ILogger<DataGridBusinessManager> _logger;
    private readonly List<Dictionary<string, object?>> _data = new();
    
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(...)
    {
        _logger?.Info("📥 IMPORT START: Importing {RowCount} rows", data.Count);
        
        try
        {
            // PHASE 1: Validation
            var validationResults = await ValidateBatchAsync(data);
            if (validationResults.HasErrors)
            {
                _logger?.Error("❌ Import validation failed: {ErrorCount} errors", 
                    validationResults.ErrorCount);
                return Result<ImportResult>.Failure("Validation errors found");
            }
            
            // PHASE 2: Data import (functional approach)
            var importedCount = await ImportDataInternalAsync(data, mode, startRow);
            
            // PHASE 3: Success
            _logger?.Info("✅ IMPORT SUCCESS: {Count} rows imported", importedCount);
            return Result<ImportResult>.Success(new ImportResult 
            { 
                ImportedRows = importedCount, 
                IsSuccess = true 
            });
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 IMPORT CRITICAL ERROR: {Details}", ex.Message);
            return Result<ImportResult>.Failure($"Import failed: {ex.Message}");
        }
    }
    
    // updateUI parameter sa ignoruje v headless režime
    public async Task RefreshUIAsync() 
    { 
        // V headless režime sa nič nerobí
        await Task.CompletedTask; 
    }
}

// 3. UI DECORATOR (pridáva UI funkcionalitu)
public class DataGridUIDecorator : IDataGridService
{
    private readonly IDataGridService _businessManager;
    private readonly AdvancedDataGrid _uiGrid;
    private readonly ILogger<DataGridUIDecorator> _logger;
    
    public DataGridUIDecorator(IDataGridService businessManager, 
        AdvancedDataGrid uiGrid, 
        ILogger<DataGridUIDecorator> logger)
    {
        _businessManager = businessManager;
        _uiGrid = uiGrid;
        _logger = logger;
    }
    
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        bool updateUI = true)
    {
        // 1. Zavolaj biznis logiku
        var result = await _businessManager.ImportFromDictionaryAsync(
            data, checkboxStates, startRow, mode, updateUI: false);
        
        // 2. Ak je úspešné A updateUI = true, aktualizuj UI
        if (result.IsSuccess && updateUI)
        {
            _logger?.Info("🎨 Updating UI after successful import");
            await RefreshUIAsync();
        }
        
        return result;
    }
    
    public async Task RefreshUIAsync()
    {
        _logger?.Info("🔄 Refreshing DataGrid UI");
        await _uiGrid.Dispatcher.BeginInvoke(() =>
        {
            _uiGrid.InvalidateVisual();
            _uiGrid.UpdateLayout();
        });
    }
}
3.3 Pokročilé validačné systémy
MULTI-LEVEL VALIDATION ARCHITECTURE:
csharp// 1. Single Cell Validation
public record ValidationRule(
    string ColumnName,
    Func<object?, bool> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error);

// 2. Cross-Column Validation (same row)
public record CrossColumnValidationRule(
    IReadOnlyList<string> ColumnNames,
    Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator,
    string ErrorMessage);

// 3. Cross-Row Validation  
public record CrossRowValidationRule(
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, 
         IReadOnlyList<ValidationResult>> Validator,
    string ErrorMessage);

// Používanie:
var validationConfig = new ValidationConfiguration
{
    ColumnValidationRules = new()
    {
        ["Age"] = new List<ValidationRule>
        {
            new("Age", v => v != null, "Age is required"),
            new("Age", v => (int)v >= 18 && (int)v <= 120, "Age must be 18-120")
        },
        ["Email"] = new List<ValidationRule>
        {
            new("Email", email => IsValidEmail(email?.ToString()), "Invalid email format")
        }
    },
    CrossColumnRules = new()
    {
        new(new[] {"StartDate", "EndDate"}, 
            row => ValidateDateRange(row), 
            "End date must be after start date")
    }
};
VALIDATION MODES:

Real-time: Počas písania do bunky (INotifyDataErrorInfo)
Batch: Pri importe/paste operáciách (všetky riadky naraz)
Smart: Kombinuje oba prístupy podľa kontextu

3.4 Performance a škálovateľnosť
VIRTUALIZÁCIA PRE MILIÓNY RIADKOV:
csharppublic class VirtualizedDataProvider
{
    private readonly List<Dictionary<string, object?>> _allData = new();
    private readonly int _pageSize = 1000;
    
    public async Task<IEnumerable<Dictionary<string, object?>>> GetVisibleDataAsync(
        int startIndex, int count)
    {
        // Načítaj len viditeľné riadky
        return _allData.Skip(startIndex).Take(count);
    }
    
    // Background operations pre veľké datasety
    public async Task ProcessLargeDatasetAsync(
        IEnumerable<Dictionary<string, object?>> data,
        IProgress<ProcessingProgress> progress,
        CancellationToken cancellationToken)
    {
        var batches = data.Chunk(_pageSize);
        var processedCount = 0;
        
        foreach (var batch in batches)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await ProcessBatchAsync(batch);
            processedCount += batch.Length;
            
            progress?.Report(new ProcessingProgress 
            { 
                ProcessedItems = processedCount,
                TotalItems = data.Count()
            });
        }
    }
}
3.5 Smart Row Management
INTELIGENTNÁ SPRÁVA RIADKOV:
csharppublic class SmartRowManager
{
    private readonly int _minimumRows;
    private readonly ILogger _logger;
    
    public async Task<Result<bool>> HandleRowOperationAsync(
        RowOperation operation, int rowIndex)
    {
        switch (operation)
        {
            case RowOperation.Delete:
                if (GetCurrentRowCount() > _minimumRows)
                {
                    // Fyzicky vymaž celý riadok
                    await DeletePhysicalRowAsync(rowIndex);
                    _logger?.Info("🗑️ Physical row deleted at index {Index}", rowIndex);
                }
                else
                {
                    // Vymaž len obsah, zachovaj štruktúru
                    await ClearRowDataAsync(rowIndex);
                    _logger?.Info("🧹 Row data cleared at index {Index}", rowIndex);
                }
                break;
                
            case RowOperation.Add:
                // Vždy udržuj jeden prázdny riadok na konci
                await EnsureEmptyRowAtEndAsync();
                _logger?.Info("➕ Empty row ensured at end");
                break;
        }
        
        return Result<bool>.Success(true);
    }
}
3.6 Pokročilé UI funkcie a štýlovanie
DYNAMICKÉ TÉMY A FARBY:
csharppublic class DataGridColorConfiguration
{
    // Základné farby buniek
    public string CellBackground { get; set; } = "#FFFFFF";
    public string CellForeground { get; set; } = "#000000";
    public string CellBorder { get; set; } = "#E0E0E0";
    
    // Validačné farby (Error má najvyššiu prioritu)
    public string ValidationErrorBorder { get; set; } = "#FF0000";
    public string ValidationWarningBorder { get; set; } = "#FF9800";
    
    // Selection a Focus
    public string SelectionBackground { get; set; } = "#0078D4";
    public string FocusBackground { get; set; } = "#F0F8FF";
    
    // Predefined themes
    public static DataGridColorConfiguration DarkTheme => new()
    {
        CellBackground = "#2D2D2D",
        CellForeground = "#FFFFFF",
        SelectionBackground = "#0078D4"
    };
}

// Aplikovanie témy
await dataGrid.SetColorConfigurationAsync(DataGridColorConfiguration.DarkTheme);
4. ERROR HANDLING A RESULT<T> PATTERN
MONADIC ERROR HANDLING:
csharppublic record Result<T>
{
    public T? Value { get; init; }
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
    
    public static Result<T> Success(T value) => 
        new() { Value = value, IsSuccess = true };
        
    public static Result<T> Failure(string error, Exception? ex = null) => 
        new() { IsSuccess = false, ErrorMessage = error, Exception = ex };
        
    // Monadic operations
    public async Task<Result<TOut>> Bind<TOut>(Func<T, Task<Result<TOut>>> func)
    {
        if (!IsSuccess) return Result<TOut>.Failure(ErrorMessage, Exception);
        return await func(Value!);
    }
    
    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    {
        if (!IsSuccess) return Result<TOut>.Failure(ErrorMessage, Exception);
        return Result<TOut>.Success(func(Value!));
    }
}

// Používanie
var result = await dataGrid.ImportFromDictionaryAsync(data)
    .Bind(async importResult => await dataGrid.ValidateAllAsync())
    .Map(validationResult => CreateProcessingSummary(validationResult));

if (result.IsSuccess)
{
    logger?.Info("✅ Operation completed successfully");
}
else
{
    logger?.Error("❌ Operation failed: {Error}", result.ErrorMessage);
}
5. DEPENDENCY INJECTION REGISTRÁCIA
UNIFIED REGISTRATION EXTENSIONS:
csharppublic static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdvancedWinUiDataGrid(
        this IServiceCollection services,
        Action<DataGridOptions>? configure = null)
    {
        // Core services
        services.AddScoped<IDataGridService, DataGridBusinessManager>();
        services.AddScoped<DataGridUIDecorator>();
        
        // Specialized managers
        services.AddScoped<DataGridSelectionManager>();
        services.AddScoped<DataGridEditingManager>();
        services.AddScoped<DataGridResizeManager>();
        services.AddScoped<DataGridEventManager>();
        
        // Configuration
        if (configure != null)
        {
            services.Configure<DataGridOptions>(configure);
        }
        
        return services;
    }
    
    public static IServiceCollection AddAdvancedWinUiLogger(
        this IServiceCollection services,
        Action<LoggerOptions>? configure = null)
    {
        services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        
        if (configure != null)
        {
            services.Configure<LoggerOptions>(configure);
        }
        
        return services;
    }
}

// Používanie v aplikácii
services.AddAdvancedWinUiLogger(opts => 
{
    opts.LogDirectory = @"C:\MyApp\Logs";
    opts.MaxFileSizeMB = 50;
    opts.MaxBackupFiles = 10;
});

services.AddAdvancedWinUiDataGrid(opts =>
{
    opts.MinimumRows = 20;
    opts.EnableValidation = true;
    opts.Colors = DataGridColorConfiguration.DarkTheme;
});
6. DEMO APLIKÁCIA A TESTOVANIE
KOMPLEXNÁ DEMO IMPLEMENTÁCIA:
csharppublic sealed partial class MainWindow : Window
{
    private readonly IDataGridService _dataGridService;
    private readonly ILogger<MainWindow> _logger;
    
    public MainWindow(IDataGridService dataGridService, ILogger<MainWindow> logger)
    {
        _dataGridService = dataGridService;
        _logger = logger;
        InitializeComponent();
        LoadDemo();
    }
    
    private async void LoadDemo()
    {
        _logger?.Info("🚀 Starting demo application");
        
        // Demo dáta pre import
        var demoData = new List<Dictionary<string, object?>>
        {
            new() { ["ID"] = 1, ["Name"] = "John Doe", ["Age"] = 30, ["Email"] = "john@example.com" },
            new() { ["ID"] = 2, ["Name"] = "Jane Smith", ["Age"] = 25, ["Email"] = "jane@example.com" }
        };
        
        var result = await _dataGridService.ImportFromDictionaryAsync(demoData);
        if (result.IsSuccess)
        {
            _logger?.Info("✅ Demo data imported: {Count} rows", result.Value.ImportedRows);
        }
    }
}
7. KVALITNÉ ŠTANDARDY A DOKUMENTÁCIA
XML DOKUMENTÁCIA POŽIADAVKY:
csharp/// <summary>
/// 📊 PROFESSIONAL DATA IMPORT ENGINE
/// 
/// Importuje dáta z Dictionary zoznamu do DataGrid tabuľky s kompletnou validáciou.
/// Podporuje tri módy importu: Replace, Append, Overwrite.
/// 
/// VSTUPNÉ DÁTA:
/// - data: List obsahujúci Dictionary objekty reprezentujúce riadky
/// - checkboxStates: Voliteľná mapa checkbox stavov pre jednotlivé riadky
/// - startRow: Začiatočný riadok od ktorého sa má import vykonať (1-based)
/// - mode: Spôsob ako sa majú dáta importovať
/// 
/// VÝSTUPNÉ DÁTA:
/// - ImportResult s detailami o úspešnosti a počte importovaných riadkov
/// </summary>
/// <param name="data">
/// Zoznam Dictionary objektov kde každý reprezentuje jeden riadok tabuľky.
/// Key = názov stĺpca, Value = hodnota bunky (môže byť null)
/// </param>
/// <param name="updateUI">Určuje či sa má aktualizovať UI po importe</param>
/// <returns>Result s ImportResult obsahujúci štatistiky operácie</returns>
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
    List<Dictionary<string, object?>> data,
    Dictionary<int, bool>? checkboxStates = null,
    int startRow = 1,
    ImportMode mode = ImportMode.Replace,
    bool updateUI = true)
LOGGING ŠTANDARDY:

Každá verejná metóda: vstupné logo s kľúčovými parametrami
Každá chyba: kompletné logovanie s stack trace
Fázové logy: označenie postupu v komplexných operáciách
Používanie emoji pre vizuálnu identifikáciu typu operácie

8. FINÁLNE OČAKÁVANÉ VÝSTUPY
KOMPLEXNÝ BALÍK OBSAHUJE:

Zdrojový kód: Plne funkčný, refaktorovaný a optimalizovaný
NuGet balík: Pripravený .nupkg súbor pre distribúciu
Demo aplikácia: Funkčná ukážka oboch komponentov
Jednotkové testy: Komplexné testy pre všetky funkcie
Dokumentácia: Aktualizovaná a kompletná dokumentácia
Performance benchmarky: Testy pre milióny riadkov

ARCHITEKTÚRNA EXCELENCIA:

Žiadne god-level súbory
Clean Architecture s prísnym dodržaním vrstiev
SOLID princípy v každej triede
Hybrid Functional-OOP optimalizované pre rôzne use cases
Monadic error handling namiesto exceptions
Enterprise-grade performance a škálovateľnosť


ZÁVEREČNÁ INŠTRUKCIA: Implementujte toto riešenie s najvyššou profesionalitou, pričom každý súbor má mať jasnú zodpovednosť, všetky operácie sú optimalizované pre výkon a celá architektúra je pripravená na budúce rozšírenia. Kód musí byť production-ready s kompletným error handlingom a logovaním.