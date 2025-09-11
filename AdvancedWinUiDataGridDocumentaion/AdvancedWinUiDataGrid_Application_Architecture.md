# AdvancedWinUiDataGrid - Application Layer Architectural Documentation

## Kompletné Vysvetlenie Application Layer Implementácie

> **🚀 AKTUALIZÁCIA - Január 2025:** Application layer rozšírený o nové service metódy a vylepšené Command objekty. Pridaná podpora pre GetColumnNameAsync, ShowCellValidationFeedbackAsync a vylepšené CQRS command handling.

### Úvod do Application Layer

Application layer je **orchestračná vrstva** v Clean Architecture - obsahuje use cases, application services, command/query handlers, a koordinuje domain logic. Táto vrstva implementuje business workflows a aplikačnú logiku bez závislosti na UI alebo Infrastructure.

### Prečo sme sa rozhodli pre Application Layer separation?

#### **Výhody Application Layer Separation:**
1. **Use Case Clarity** - Každý use case má vlastnú implementáciu
2. **Business Logic Organization** - Workflows organizované podľa business scenárov
3. **Testability** - Application logic testovateľný nezávisle od UI/Infrastructure
4. **Reusability** - Application services použiteľné v rôznych UI kontextoch
5. **CQRS Support** - Oddelenie Commands od Queries pre lepšie performance

#### **Nevýhody Application Layer Separation:**
1. **Additional Abstraction** - Viacero vrstiev medzi UI a Domain
2. **Learning Curve** - Developers musia pochopiť Clean Architecture koncepty
3. **Initial Complexity** - Viac boilerplate kódu na začiatku
4. **Potential Over-Engineering** - Pre jednoduché aplikácie môže byť zbytočné

---

## 1. APPLICATION SERVICES - UNIFIED ARCHITECTURE

### 1.1 DataGridUnifiedService.cs - Facade Pattern Implementation
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.DataGridUnifiedService:30`

```csharp
/// <summary>
/// FACADE PATTERN: Unified interface replacing the 920-line GOD file
/// SOLID: Orchestrates specialized services with single responsibilities
/// CLEAN ARCHITECTURE: Application layer facade coordinating domain services
/// ENTERPRISE: Professional API maintaining backward compatibility
/// </summary>
public sealed class DataGridUnifiedService : IDataGridService
{
    private readonly IDataGridStateManagementService _stateService;
    private readonly IDataGridImportExportService _importExportService;
    private readonly IDataGridSearchFilterService _searchFilterService;
    private readonly IDataGridRowManagementService _rowManagementService;
    private readonly IDataGridValidationService _validationService;
    private readonly IClipboardService _clipboardService;
    private readonly ILogger? _logger;
}
```

#### **Prečo Unified Service Pattern?**
- **Facade Pattern** - Jednoduchý interface pre komplexné operácie
- **Service Orchestration** - Koordinuje specialized services
- **Backward Compatibility** - Maintains API compatibility while improving architecture
- **Dependency Injection** - Clean service composition through constructor injection

#### **Service Composition Strategy:**
```csharp
public DataGridUnifiedService(
    IDataGridStateManagementService stateService,
    IDataGridImportExportService importExportService,
    IDataGridSearchFilterService searchFilterService,
    IDataGridRowManagementService rowManagementService,
    IDataGridValidationService validationService,
    IClipboardService clipboardService,
    ILogger? logger = null)
```

**Dependency Analysis:**
- **State Management** - Core grid state operations
- **Import/Export** - Data transformation and bulk operations  
- **Search/Filter** - Query operations and result filtering
- **Row Management** - CRUD operations for individual rows
- **Validation** - Business rule validation
- **Clipboard** - Copy/paste operations with Excel compatibility

#### **Public API Methods (Unified Interface):**

**Core Data Operations:**
```csharp
public async Task<Result<bool>> InitializeAsync(IReadOnlyList<ColumnDefinition> columns, DataGridConfiguration? configuration = null)
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(List<Dictionary<string, object?>> data, ImportMode mode = ImportMode.Replace)
public async Task<Result<List<Dictionary<string, object?>>>> ExportToDataTableAsync(bool includeValidationAlerts = false)
```

**Row Management:**
```csharp
public async Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData, int? insertAtIndex = null)
public async Task<Result<bool>> UpdateRowAsync(int rowIndex, Dictionary<string, object?> rowData)
public async Task<Result<bool>> DeleteRowAsync(int rowIndex)
```

**Search and Filter:**
```csharp
public async Task<Result<SearchResult>> SearchAsync(string searchTerm, SearchOptions? options = null)
public async Task<Result<bool>> ApplyFilterAsync(string columnName, object filterValue)
public async Task<Result<bool>> ClearFiltersAsync()
```

**Clipboard Integration:**
```csharp
public async Task<Result<bool>> CopySelectedRowsAsync(IReadOnlyList<int> selectedRowIndices)
public async Task<Result<bool>> CopyAllVisibleRowsAsync()
public async Task<Result<PasteResult>> PasteFromClipboardAsync(int startRowIndex = 0, int startColumnIndex = 0, bool replaceExistingData = false)
```

#### **Service Delegation Pattern:**

**Method Implementation Example:**
```csharp
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(List<Dictionary<string, object?>> data, ImportMode mode = ImportMode.Replace)
{
    if (_disposed) return Result<ImportResult>.Failure("Service disposed");
    
    try
    {
        _logger?.LogDebug("Starting import operation with {RowCount} rows", data.Count);
        
        // Delegate to specialized service
        var result = await _importExportService.ImportFromDictionaryAsync(data, mode);
        
        if (result.IsSuccess)
        {
            // Update state after successful import
            _stateService.UpdateState();
            _logger?.LogInformation("Import completed successfully: {ImportedRows}/{TotalRows} rows", 
                result.Value.ImportedRows, result.Value.TotalRows);
        }
        
        return result;
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "Import operation failed");
        return Result<ImportResult>.Failure("Import operation failed", ex);
    }
}
```

**Delegation Benefits:**
- **Single Responsibility** - Each specialized service handles specific concerns
- **Error Handling** - Consistent exception handling and Result<T> wrapping
- **Logging** - Centralized logging with operation context
- **State Coordination** - Updates grid state after successful operations

---

### 1.2 DataGridServiceFactory.cs - Service Composition Factory
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.DataGridServiceFactory:21`

```csharp
/// <summary>
/// FACTORY PATTERN: Creates DataGrid services with proper dependency injection
/// SOLID: Single Responsibility - Service creation and configuration
/// CLEAN ARCHITECTURE: Replaces the old monolithic service factory
/// NO_GOD_FILES: Orchestrates multiple specialized services instead of one giant service
/// </summary>
public static class DataGridServiceFactory
```

#### **Prečo Static Factory Class?**
- **Stateless Creation** - No factory instance state needed
- **Centralized Configuration** - Single place for service composition
- **Dependency Resolution** - Manual dependency injection setup
- **Configuration Management** - Different configurations for different modes

#### **Factory Methods:**

**CreateWithUI() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.DataGridServiceFactory:27**
```csharp
public static IDataGridService CreateWithUI(ILogger? logger = null)
{
    try
    {
        logger?.LogInformation("Creating DataGrid service for UI mode");

        var configuration = DataGridConfiguration.ForUI(UIConfiguration.Default);
        
        var validationService = CreateValidationService(logger);
        var transformationService = CreateTransformationService(logger);
        var searchService = CreateSearchService(logger);
        var filterService = CreateFilterService(logger);
        var sortService = CreateSortService(logger);

        var stateService = new DataGridStateManagementService(configuration, CreateLogger<DataGridStateManagementService>(logger));
        var importExportService = new DataGridImportExportService(transformationService, validationService, CreateLogger<DataGridImportExportService>(logger));
        var searchFilterService = new DataGridSearchFilterService(searchService, filterService, sortService, CreateLogger<DataGridSearchFilterService>(logger));
        var rowManagementService = new DataGridRowManagementService(validationService, RowManagementConfiguration.Default, CreateLogger<DataGridRowManagementService>(logger));

        var unifiedService = new DataGridUnifiedService(
            stateService, importExportService, searchFilterService, rowManagementService, 
            validationService, CreateLogger<DataGridUnifiedService>(logger));

        return unifiedService;
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Failed to create DataGrid service for UI mode");
        throw new InvalidOperationException("Failed to create DataGrid service for UI mode", ex);
    }
}
```

**Service Creation Analysis:**
1. **Configuration Setup** - DataGridConfiguration.ForUI(UIConfiguration.Default)
2. **Infrastructure Services** - transformation, search, filter, sort services
3. **Application Services** - state, import/export, search/filter, row management
4. **Service Composition** - DataGridUnifiedService with all dependencies
5. **Logger Propagation** - Typed loggers for each service

**CreateHeadless() Method:**
```csharp
var configuration = DataGridConfiguration.ForHeadless();
var rowManagementService = new DataGridRowManagementService(validationService, RowManagementConfiguration.HighVolume, CreateLogger<DataGridRowManagementService>(logger));
```

**UI vs Headless Differences:**
- **Configuration** - ForUI() vs ForHeadless()
- **Row Management** - Default vs HighVolume configuration
- **Same Core Services** - Infrastructure services identical
- **Performance Optimizations** - HighVolume config for headless scenarios

#### **Service Factory Helpers:**
```csharp
private static IDataGridValidationService CreateValidationService(ILogger? logger)
{
    return new DataGridValidationService(CreateLogger<DataGridValidationService>(logger));
}

private static ILogger<T>? CreateLogger<T>(ILogger? baseLogger)
{
    return baseLogger as ILogger<T>;
}
```

**Factory Helper Analysis:**
- **Type-Safe Logger Creation** - ILogger<T> for each service
- **Infrastructure Service Instantiation** - Creates concrete implementations
- **Null Safety** - Handles null logger gracefully
- **Dependency Chain** - Each service gets appropriate dependencies

---

### 1.3 ClipboardService.cs - Excel-Compatible Clipboard Operations
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.ClipboardService:18`

```csharp
/// <summary>
/// CLIPBOARD: Advanced clipboard operations for DataGrid
/// EXCEL_COMPATIBLE: Full Excel format support with TSV/CSV parsing
/// ENTERPRISE: Production-ready copy/paste functionality
/// </summary>
public class ClipboardService : IDisposable
```

#### **Prečo Dedicated Clipboard Service?**
- **Excel Compatibility** - TSV (Tab Separated Values) format support
- **Complex Logic** - Parsing, formatting, type conversion logic
- **Platform Integration** - WinUI DataTransfer API integration
- **Reusability** - Clipboard operations used across multiple scenarios

#### **Copy Operations:**
```csharp
/// <summary>
/// Copy selected data to clipboard in Excel-compatible format
/// EXCEL_FORMAT: TSV (Tab Separated Values) for Excel compatibility
/// </summary>
public async Task<Result<bool>> CopyToClipboardAsync(
    IReadOnlyList<Dictionary<string, object?>> selectedRows,
    IReadOnlyList<ColumnDefinition> columns,
    bool includeHeaders = true)
```

**Excel Format Strategy:**
- **TSV Format** - Tab-separated values for Excel compatibility
- **Header Support** - Optional column headers in first row
- **Type-Safe Conversion** - Proper formatting for different data types
- **Large Dataset Support** - Efficient string building for bulk data

#### **Paste Operations:**
```csharp
public async Task<Result<ClipboardParseResult>> PasteFromClipboardAsync(
    IReadOnlyList<ColumnDefinition> targetColumns,
    int startRowIndex = 0,
    int startColumnIndex = 0)
```

**Parsing Strategy:**
- **Smart Delimiter Detection** - Handles TSV, CSV, and other formats
- **Type Conversion** - Converts string values to appropriate types
- **Error Recovery** - Continues parsing even with individual cell errors
- **Validation Integration** - Uses column definitions for type safety

#### **Value Objects for Clipboard:**
```csharp
public record ClipboardParseResult
{
    public List<Dictionary<string, object?>> ParsedRows { get; init; } = [];
    public int RowsParsed { get; init; }
    public int RowsSkipped { get; init; }
    public IReadOnlyList<ParseError> Errors { get; init; } = [];
    public bool IsSuccess => Errors.Count == 0;
}

public record PasteResult
{
    public int RowsAdded { get; init; }
    public int RowsUpdated { get; init; }
    public int RowsSkipped { get; init; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = [];
    public bool IsSuccess => ValidationErrors.Count == 0;
}
```

**Rich Result Objects:**
- **Detailed Metrics** - Rows parsed, added, updated, skipped
- **Error Tracking** - Comprehensive error information
- **Success Indicators** - Clear success/failure determination
- **Audit Trail** - Complete operation results for logging

---

## 2. USE CASES - CQRS IMPLEMENTATION

### 2.1 Command Pattern Implementation
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid.SearchCommands:15`

#### **SearchCommand - Search Use Case:**
```csharp
/// <summary>
/// CQRS: Command for performing search operations
/// </summary>
public sealed record SearchCommand
{
    public required string SearchTerm { get; init; }
    public SearchOptions? Options { get; init; }
    public IReadOnlyList<string>? ColumnNames { get; init; }
    public bool CaseSensitive { get; init; } = false;
    public SearchType SearchType { get; init; } = SearchType.Contains;
}
```

**Command Design Principles:**
- **Record Type** - Immutable command objects
- **Required Properties** - SearchTerm is required
- **Optional Configuration** - Options, ColumnNames, search parameters
- **Smart Defaults** - CaseSensitive = false, SearchType = Contains
- **Factory Methods** - Static Create() methods for common scenarios

#### **Factory Methods for Commands:**
```csharp
public static SearchCommand Create(
    string searchTerm,
    SearchOptions? options = null) =>
    new()
    {
        SearchTerm = searchTerm,
        Options = options
    };

public static SearchCommand CreateWithColumns(
    string searchTerm,
    IReadOnlyList<string> columnNames,
    bool caseSensitive = false,
    SearchType searchType = SearchType.Contains) =>
    new()
    {
        SearchTerm = searchTerm,
        ColumnNames = columnNames,
        CaseSensitive = caseSensitive,
        SearchType = searchType
    };
```

**Factory Benefits:**
- **Fluent API** - Easy command creation
- **Parameter Validation** - Ensures required parameters provided
- **Overload Reduction** - Multiple factory methods instead of constructor overloads
- **Domain Language** - CreateWithColumns() is clearer than constructor

### 2.2 Row Management Commands

#### **AddRowCommand:**
```csharp
/// <summary>
/// CQRS: Command for row management operations
/// </summary>
public sealed record AddRowCommand
{
    public required Dictionary<string, object?> RowData { get; init; }
    public int? InsertAtIndex { get; init; }
    public bool ValidateBeforeAdd { get; init; } = true;
    
    public static AddRowCommand Create(Dictionary<string, object?> rowData) =>
        new() { RowData = rowData };

    public static AddRowCommand CreateAtIndex(Dictionary<string, object?> rowData, int index) =>
        new() { RowData = rowData, InsertAtIndex = index };
}
```

**Command Features:**
- **Flexible Insertion** - Can insert at specific index or append
- **Validation Control** - ValidateBeforeAdd flag for performance scenarios
- **Type Safety** - Dictionary<string, object?> for flexible data
- **Factory Variations** - Different creation patterns for different scenarios

#### **UpdateRowCommand:**
```csharp
public sealed record UpdateRowCommand
{
    public required int RowIndex { get; init; }
    public required Dictionary<string, object?> RowData { get; init; }
    public bool ValidateBeforeUpdate { get; init; } = true;
    public bool MergeWithExisting { get; init; } = false;
    
    public static UpdateRowCommand CreateMerging(int rowIndex, Dictionary<string, object?> rowData) =>
        new() { RowIndex = rowIndex, RowData = rowData, MergeWithExisting = true };
}
```

**Update Strategies:**
- **Replace vs Merge** - MergeWithExisting flag controls update behavior
- **Validation Options** - Can skip validation for performance
- **Index-Based** - Updates specific row by index
- **Partial Updates** - Merge mode allows partial row updates

#### **DeleteRowCommand:**
```csharp
public sealed record DeleteRowCommand
{
    public required int RowIndex { get; init; }
    public bool RequireConfirmation { get; init; } = false;
    public string? ConfirmationMessage { get; init; }
    
    public static DeleteRowCommand CreateWithConfirmation(int rowIndex, string confirmationMessage) =>
        new() 
        { 
            RowIndex = rowIndex, 
            RequireConfirmation = true, 
            ConfirmationMessage = confirmationMessage 
        };
}
```

**Safety Features:**
- **Confirmation Support** - Optional user confirmation
- **Custom Messages** - Configurable confirmation messages
- **Safety Defaults** - RequireConfirmation = false by default
- **UI Integration** - Supports different UI confirmation patterns

### 2.3 Validation Commands

#### **ValidateAllCommand:**
```csharp
/// <summary>
/// CQRS: Command for validation operations
/// </summary>
public sealed record ValidateAllCommand
{
    public IProgress<ValidationProgress>? Progress { get; init; }
    public bool StrictMode { get; init; } = false;
    public IReadOnlyList<string>? ColumnsToValidate { get; init; }
    
    public static ValidateAllCommand ForColumns(
        IReadOnlyList<string> columnsToValidate,
        IProgress<ValidationProgress>? progress = null,
        bool strictMode = false) =>
        new()
        {
            ColumnsToValidate = columnsToValidate,
            Progress = progress,
            StrictMode = strictMode
        };
}
```

**Validation Features:**
- **Progress Reporting** - IProgress<T> for long-running validations
- **Selective Validation** - Can validate specific columns only
- **Strict Mode** - Enhanced validation rules
- **Async Support** - Progress reporting enables async validation

---

## 3. SPECIALIZED SERVICES

### 3.1 Service Hierarchy

**Application Service Organization:**
```
DataGridUnifiedService (Facade)
├── IDataGridStateManagementService
├── IDataGridImportExportService  
├── IDataGridSearchFilterService
├── IDataGridRowManagementService
├── IDataGridValidationService
└── IClipboardService
```

**Each Service Responsibility:**
- **State Management** - GridState lifecycle, versioning, snapshots
- **Import/Export** - Bulk data operations, transformations
- **Search/Filter** - Query operations, result filtering, sorting  
- **Row Management** - CRUD operations, row lifecycle
- **Validation** - Business rule validation, error management
- **Clipboard** - Copy/paste operations, Excel integration

### 3.2 Interface Contracts

#### **IDataGridService - Main Contract:**
```csharp
public interface IDataGridService : IDisposable
{
    // Core Operations
    Task<Result<bool>> InitializeAsync(IReadOnlyList<ColumnDefinition> columns, DataGridConfiguration? configuration = null);
    
    // Data Operations  
    Task<Result<ImportResult>> ImportFromDictionaryAsync(List<Dictionary<string, object?>> data, ImportMode mode = ImportMode.Replace);
    Task<Result<List<Dictionary<string, object?>>>> ExportToDataTableAsync(bool includeValidationAlerts = false);
    
    // Row Management
    Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData, int? insertAtIndex = null);
    Task<Result<bool>> UpdateRowAsync(int rowIndex, Dictionary<string, object?> rowData);
    Task<Result<bool>> DeleteRowAsync(int rowIndex);
    
    // Search & Filter
    Task<Result<SearchResult>> SearchAsync(string searchTerm, SearchOptions? options = null);
    Task<Result<bool>> ApplyFilterAsync(string columnName, object filterValue);
    
    // Clipboard
    Task<Result<bool>> CopySelectedRowsAsync(IReadOnlyList<int> selectedRowIndices);
    Task<Result<PasteResult>> PasteFromClipboardAsync(int startRowIndex = 0, int startColumnIndex = 0, bool replaceExistingData = false);
}
```

**Interface Design Principles:**
- **Result<T> Pattern** - Consistent error handling
- **Async/Await** - Non-blocking operations
- **Optional Parameters** - Reasonable defaults for common scenarios
- **Type Safety** - Strong typing with domain value objects
- **Comprehensive Coverage** - All major DataGrid operations

---

## 4. ARCHITECTURAL DECISIONS

### 4.1 CQRS (Command Query Responsibility Segregation)

**Command Side:**
```csharp
// Commands - Operations that change state
public sealed record AddRowCommand { ... }
public sealed record UpdateRowCommand { ... }
public sealed record DeleteRowCommand { ... }
```

**Query Side:**
```csharp  
// Queries - Operations that read state
public async Task<Result<SearchResult>> SearchAsync(...)
public async Task<Result<List<Dictionary<string, object?>>>> ExportToDataTableAsync(...)
```

**CQRS Benefits:**
- **Performance Optimization** - Different strategies for reads vs writes
- **Scalability** - Can scale read and write operations independently  
- **Complexity Management** - Clear separation of concerns
- **Future Evolution** - Easy to add event sourcing or different persistence strategies

### 4.2 Facade Pattern for Service Unification

**Problem:** Multiple specialized services create complex API
**Solution:** DataGridUnifiedService as facade

```csharp
// Instead of exposing 6 different services
_stateService.UpdateState();
_validationService.ValidateRow();
_importExportService.ImportData();

// Single unified interface
await _dataGridService.AddRowAsync(rowData);
```

**Facade Benefits:**
- **Simplified API** - Single interface instead of multiple services
- **Coordination** - Handles inter-service coordination automatically
- **Backward Compatibility** - Maintains existing API contracts
- **Testability** - Easier to mock single service than multiple services

### 4.3 Factory Pattern for Service Composition

**Manual Dependency Injection:**
```csharp
public static IDataGridService CreateWithUI(ILogger? logger = null)
{
    // Create infrastructure services
    var validationService = CreateValidationService(logger);
    var transformationService = CreateTransformationService(logger);
    
    // Create application services with dependencies
    var stateService = new DataGridStateManagementService(configuration, logger);
    
    // Compose unified service
    return new DataGridUnifiedService(stateService, /* other services */, logger);
}
```

**Factory Benefits:**
- **Dependency Management** - Centralized service composition
- **Configuration Management** - Different configurations for different modes
- **Testing Support** - Easy to create test doubles
- **Lifecycle Management** - Proper service initialization order

### 4.4 Result<T> Pattern for Error Handling

**Consistent Error Handling:**
```csharp
// All operations return Result<T>
Task<Result<bool>> AddRowAsync(...)
Task<Result<ImportResult>> ImportFromDictionaryAsync(...)
Task<Result<SearchResult>> SearchAsync(...)
```

**Result<T> Benefits:**
- **No Exception-Driven Flow** - Exceptions only for unexpected errors
- **Composable Operations** - Can chain operations with flatMap patterns
- **Clear Success/Failure** - Explicit success/failure handling
- **Rich Error Information** - Error messages with context

---

## 5. PERFORMANCE CONSIDERATIONS

### 5.1 Service Lifecycle Management

**Unified Service Disposal:**
```csharp
public void Dispose()
{
    if (_disposed) return;
    
    _stateService?.Dispose();
    _importExportService?.Dispose();
    _searchFilterService?.Dispose();
    _rowManagementService?.Dispose();
    _validationService?.Dispose();
    _clipboardService?.Dispose();
    
    _disposed = true;
}
```

**Resource Management:**
- **Cascading Disposal** - Unified service disposes all sub-services
- **Disposal Safety** - Multiple disposal calls are safe
- **Resource Cleanup** - All services properly clean up resources

### 5.2 Asynchronous Operations

**Non-blocking Design:**
```csharp
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(List<Dictionary<string, object?>> data, ImportMode mode = ImportMode.Replace)
{
    // All operations are async to prevent UI blocking
    var result = await _importExportService.ImportFromDictionaryAsync(data, mode);
    
    if (result.IsSuccess)
    {
        // State updates also async when needed
        _stateService.UpdateState();
    }
    
    return result;
}
```

**Async Benefits:**
- **UI Responsiveness** - Long operations don't block UI thread
- **Scalability** - Better resource utilization
- **Cancellation Support** - Can add CancellationToken support later
- **Progress Reporting** - IProgress<T> integration for long operations

### 5.3 Memory Management

**Large Dataset Handling:**
```csharp
// Clipboard operations with large datasets
public async Task<Result<bool>> CopyToClipboardAsync(
    IReadOnlyList<Dictionary<string, object?>> selectedRows,
    IReadOnlyList<ColumnDefinition> columns,
    bool includeHeaders = true)
{
    // Use StringBuilder for efficient string concatenation
    var stringBuilder = new StringBuilder();
    
    // Process in chunks for large datasets
    const int chunkSize = 1000;
    for (int i = 0; i < selectedRows.Count; i += chunkSize)
    {
        // Process chunk
        await ProcessChunk(selectedRows.Skip(i).Take(chunkSize), stringBuilder);
    }
}
```

**Memory Optimization:**
- **Chunked Processing** - Handle large datasets in manageable chunks
- **Streaming Operations** - Process data without loading everything into memory
- **Object Pooling** - Reuse expensive objects like StringBuilder
- **Lazy Loading** - Load data only when needed

---

## 6. TESTING STRATEGY

### 6.1 Command Testing
```csharp
[Fact]
public void SearchCommand_Create_ShouldHaveCorrectProperties()
{
    // Arrange
    var searchTerm = "test";
    var options = new SearchOptions();
    
    // Act
    var command = SearchCommand.Create(searchTerm, options);
    
    // Assert
    command.SearchTerm.Should().Be(searchTerm);
    command.Options.Should().Be(options);
    command.CaseSensitive.Should().BeFalse(); // Default value
    command.SearchType.Should().Be(SearchType.Contains); // Default value
}
```

### 6.2 Service Factory Testing
```csharp
[Fact]
public void DataGridServiceFactory_CreateWithUI_ShouldReturnValidService()
{
    // Act
    var service = DataGridServiceFactory.CreateWithUI();
    
    // Assert  
    service.Should().NotBeNull();
    service.Should().BeOfType<DataGridUnifiedService>();
    service.Should().BeAssignableTo<IDataGridService>();
}
```

### 6.3 Integration Testing
```csharp
[Fact]
public async Task DataGridUnifiedService_AddRow_ShouldUpdateState()
{
    // Arrange
    var service = DataGridServiceFactory.CreateWithUI();
    var columns = new[] { ColumnDefinition.Text("Name") };
    await service.InitializeAsync(columns);
    
    var rowData = new Dictionary<string, object?> { ["Name"] = "Test" };
    
    // Act
    var result = await service.AddRowAsync(rowData);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    var exportResult = await service.ExportToDataTableAsync();
    exportResult.IsSuccess.Should().BeTrue();
    exportResult.Value.Should().HaveCount(1);
}
```

---

## 7. FUTURE IMPROVEMENTS

### 7.1 Event-Driven Architecture
```csharp
public interface IDataGridEventPublisher
{
    Task PublishAsync<T>(T domainEvent) where T : DomainEvent;
}

// Events
public record RowAddedEvent(int RowIndex, Dictionary<string, object?> RowData) : DomainEvent;
public record DataImportedEvent(int RowCount, ImportMode Mode) : DomainEvent;
public record SearchPerformedEvent(string SearchTerm, int ResultCount) : DomainEvent;
```

### 7.2 Enhanced CQRS with Mediator
```csharp
public interface ICommandHandler<TCommand, TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<TQuery, TResult>  
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

// Usage
public class AddRowCommandHandler : ICommandHandler<AddRowCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(AddRowCommand command, CancellationToken cancellationToken = default)
    {
        // Handle command
    }
}
```

### 7.3 Caching Layer
```csharp
public interface IDataGridCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task InvalidateAsync(string pattern, CancellationToken cancellationToken = default);
}

// Cache integration in services
public async Task<Result<SearchResult>> SearchAsync(string searchTerm, SearchOptions? options = null)
{
    var cacheKey = $"search:{searchTerm}:{options?.GetHashCode()}";
    var cached = await _cache.GetAsync<SearchResult>(cacheKey);
    
    if (cached != null)
        return Result<SearchResult>.Success(cached);
        
    var result = await PerformSearchAsync(searchTerm, options);
    
    if (result.IsSuccess)
        await _cache.SetAsync(cacheKey, result.Value, TimeSpan.FromMinutes(5));
        
    return result;
}
```

---

## ZÁVER - Application Layer Summary

Application layer poskytuje **well-organized orchestration** pre AdvancedWinUiDataGrid business workflows:

**Kľúčové Architektúrne Výhody:**

1. **Clean Service Composition** - Facade pattern s specialized services
2. **CQRS Implementation** - Clear separation commands/queries  
3. **Rich Command Objects** - Type-safe, immutable command patterns
4. **Unified API** - Single interface pre complex functionality
5. **Excel Integration** - Production-ready clipboard operations
6. **Factory Pattern** - Clean service composition a configuration
7. **Result<T> Pattern** - Consistent, functional error handling

**Service Organization:**
- **Unified Service** - Single point of entry pre všetky operácie
- **Specialized Services** - Each service má single responsibility
- **Factory Composition** - Clean dependency resolution
- **Interface Contracts** - Clear boundaries medzi layers

**Performance Features:**
- **Async Operations** - Non-blocking operations pre UI responsiveness
- **Chunked Processing** - Efficient handling large datasets  
- **Resource Management** - Proper disposal patterns
- **Memory Optimization** - StringBuilder a streaming operations

Táto application architecture poskytuje **robust foundation** pre enterprise DataGrid functionality s excellent maintainability, testability, a extensibility.