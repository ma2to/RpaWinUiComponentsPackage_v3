# AdvancedWinUiDataGrid - Infrastructure Layer Architectural Documentation

## Kompletné Vysvetlenie Infrastructure Layer Implementácie

> **🔧 AKTUALIZÁCIA - Január 2025:** Infrastructure layer významne vylepšený po oprave compilation errors. Pridané nové service metódy, vylepšené API rozhrania a rozšírená funkcionalita.

### Úvod do Infrastructure Layer

Infrastructure layer je **najnižšia vrstva** v Clean Architecture - obsahuje všetky technické implementácie, external services, persistence, a infrastructure concerns. Táto vrstva implementuje interfaces definované v Application layer a komunikuje s externými systémami.

### Prečo sme sa rozhodli pre Infrastructure Layer separation?

#### **Výhody Infrastructure Layer Separation:**
1. **Testability** - Domain/Application layers sa dajú testovať bez infrastructure
2. **Flexibility** - Infrastructure implementations sa dajú vymeniť
3. **Maintainability** - Technical concerns oddelené od business logic
4. **Scalability** - Infrastructure optimizations bez zmeny business logic
5. **Technology Independence** - Core logic nezávisí od konkrétnych technológií

#### **Nevýhody Infrastructure Layer Separation:**
1. **Complexity** - Viacero vrstiev a abstraktions
2. **Performance Overhead** - Abstraction penalties
3. **Learning Curve** - Developers musia pochopiť Clean Architecture
4. **Initial Development Time** - Viac kódu na setup

---

## 1. INFRASTRUCTURE SERVICES

### 1.1 DataGridApplicationService.cs - Application Coordinator
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services.DataGridApplicationService:18`

```csharp
public class DataGridApplicationService : IDisposable
{
    private readonly ILogger _logger;
    private readonly DataGridPerformanceService _performanceService;
    private readonly DataGridValidationService _validationService;
    private readonly DataGridConfiguration _configuration;
}
```

#### **Prečo Application Service Pattern?**
- **Orchestration** - Koordinuje multiple domain services
- **Transaction Boundary** - Manages complex operations
- **Dependency Injection** - Central point for service composition
- **Cross-cutting Concerns** - Logging, performance monitoring

#### **Constructor Analysis:**
```csharp
public DataGridApplicationService(
    ILogger logger,
    DataGridPerformanceService performanceService,
    DataGridValidationService validationService,
    DataGridConfiguration configuration)
```

**Dependency Injection Pattern:**
- **Constructor Injection** - Explicit dependencies
- **Null Checks** - ArgumentNullException pre required services
- **Immutable Dependencies** - readonly fields prevent accidental modification
- **Clean Disposal** - IDisposable implementation

#### **Key Methods Analysis:**

**InitializeAsync() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services.DataGridApplicationService:43**
```csharp
public async Task<Result<bool>> InitializeAsync()
{
    if (_disposed) return Result<bool>.Failure("Service disposed");
    
    try
    {
        _performanceService.StartOperation("ApplicationService.Initialize");
        
        _logger.LogInformation("DataGridApplicationService initialized successfully");
        
        _performanceService.StopOperation("ApplicationService.Initialize");
        return await Task.FromResult(Result<bool>.Success(true));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to initialize DataGridApplicationService");
        return Result<bool>.Failure("Failed to initialize DataGridApplicationService", ex);
    }
}
```

**Pattern Analysis:**
- **Disposed Check** - Prevents operation on disposed service
- **Performance Monitoring** - Automatic operation timing
- **Result<T> Pattern** - Functional error handling
- **Structured Logging** - LogInformation with context
- **Exception Wrapping** - Domain-friendly error messages

**Prečo async/await pattern?**
- **Future Extensibility** - Ready for async initialization
- **Consistency** - Matches async patterns across codebase  
- **Non-blocking** - Won't block UI thread
- **Task.FromResult** - Maintains async signature for sync operation

#### **ProcessDataOperationAsync() Method:**
```csharp
public async Task<Result<DataOperationResult>> ProcessDataOperationAsync(
    string operationType,
    Dictionary<string, object?> parameters)
```

**Generic Operation Processing:**
- **String Operation Type** - Flexible operation identification
- **Dictionary Parameters** - Generic parameter passing
- **Performance Tracking** - Automatic operation timing
- **Result Wrapping** - DataOperationResult with metadata

**Potential Issues:**
- **String-based Operation** - No compile-time safety
- **Generic Parameters** - Type safety concerns
- **Limited Validation** - No parameter validation

#### **Service Accessor Methods:**
```csharp
public DataGridValidationService GetValidationService()
{
    if (_disposed) throw new ObjectDisposedException(nameof(DataGridApplicationService));
    return _validationService;
}
```

**Prečo Service Locator Pattern?**
- **Convenience** - Easy access to sub-services
- **Disposal Safety** - ObjectDisposedException on disposed service
- **Service Encapsulation** - Controls access to infrastructure services

**Anti-Pattern Concerns:**
- **Service Locator** - Generally considered anti-pattern
- **Tight Coupling** - Clients know about multiple services
- **Testing Complexity** - Harder to mock individual services

---

### 1.2 DataGridPerformanceService.cs - Performance Monitoring
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services.DataGridPerformanceService:18`

```csharp
public class DataGridPerformanceService : IDisposable
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, Stopwatch> _activeOperations;
    private readonly List<PerformanceMetric> _performanceHistory;
}
```

#### **Performance Monitoring Strategy:**
- **Stopwatch-based Timing** - High-precision timing
- **Active Operations Tracking** - Multiple concurrent operations
- **Historical Data** - Performance trend analysis
- **Memory Usage Monitoring** - GC.GetTotalMemory() integration

#### **Operation Tracking Methods:**

**StartOperation() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services.DataGridPerformanceService:37**
```csharp
public void StartOperation(string operationName)
{
    if (_disposed) return;
    
    var stopwatch = Stopwatch.StartNew();
    _activeOperations[operationName] = stopwatch;
    
    _logger.LogTrace("Started measuring operation: {OperationName}", operationName);
}
```

**Design Decisions:**
- **Silent Disposal** - Returns early if disposed (no exception)
- **Dictionary Key Override** - New operation with same name replaces old
- **Trace Logging** - Minimal logging overhead
- **Immediate Start** - Stopwatch.StartNew() for immediate timing

**StopOperation() Analysis:**
```csharp
public TimeSpan StopOperation(string operationName)
{
    if (_disposed) return TimeSpan.Zero;
    
    if (_activeOperations.TryGetValue(operationName, out var stopwatch))
    {
        stopwatch.Stop();
        var duration = stopwatch.Elapsed;
        
        var metric = new PerformanceMetric
        {
            OperationName = operationName,
            Duration = duration,
            Timestamp = DateTime.UtcNow
        };
        
        _performanceHistory.Add(metric);
        _activeOperations.Remove(operationName);
        
        return duration;
    }
    
    _logger.LogWarning("Operation not found for stopping: {OperationName}", operationName);
    return TimeSpan.Zero;
}
```

**Robust Error Handling:**
- **TryGetValue Pattern** - Safe dictionary access
- **Graceful Degradation** - Returns TimeSpan.Zero for missing operations
- **Automatic Cleanup** - Removes from active operations
- **History Tracking** - Adds to performance history
- **Warning Logging** - Logs missing operations for debugging

#### **Statistics Generation:**

**GetPerformanceStatisticsAsync() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services.DataGridPerformanceService:82**
```csharp
public async Task<Result<GridPerformanceStatistics>> GetPerformanceStatisticsAsync()
{
    var lastOpTime = _performanceHistory.LastOrDefault()?.Duration ?? TimeSpan.Zero;
    var avgTime = _performanceHistory.Count > 0 
        ? TimeSpan.FromTicks((long)_performanceHistory.Average(m => m.Duration.Ticks))
        : TimeSpan.Zero;
    var memUsage = GC.GetTotalMemory(false);
    var totalOps = _performanceHistory.Count;

    var performanceMetrics = PerformanceMetrics.Create(lastOpTime, memUsage, totalOps);
    
    var statistics = new GridPerformanceStatistics
    {
        ImportMetrics = performanceMetrics,
        ExportMetrics = performanceMetrics,
        SearchMetrics = performanceMetrics,
        ValidationMetrics = performanceMetrics,
        TotalMemoryUsage = memUsage,
        Uptime = TimeSpan.FromSeconds((DateTime.UtcNow - DateTime.Today).TotalSeconds)
    };
}
```

**Statistical Calculations:**
- **LastOrDefault()** - Safe access to last operation
- **LINQ Average()** - Average time calculation with tick precision
- **GC.GetTotalMemory(false)** - Current memory usage without GC
- **Uptime Calculation** - Time since midnight (approximation)

**Architectural Issues:**
- **Shared Metrics** - Same metrics used for all operation types
- **Uptime Calculation** - DateTime.Today is not service start time
- **Memory Measurement** - Single point measurement, not trend

#### **PerformanceMetric Value Object:**
```csharp
public record PerformanceMetric
{
    public string OperationName { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public DateTime Timestamp { get; init; }
}
```

**Value Object Design:**
- **Record Type** - Immutable performance metric
- **Init-only Properties** - Construction-time setting only
- **UTC Timestamps** - Timezone-neutral timing
- **String Operation Name** - Flexible operation identification

---

### 1.3 DataGridTransformationService.cs - Data Transformation
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Persistence.DataGridTransformationService:18`

```csharp
public sealed class DataGridTransformationService : IDataGridTransformationService
```

#### **Prečo Transformation Service?**
- **Data Format Abstraction** - Domain works with dictionaries, external systems use DataTable/etc
- **Type Safety** - Controlled type conversions between external and internal formats
- **Error Isolation** - Transformation errors don't crash domain logic
- **Testability** - Transformation logic easily testable in isolation

#### **Transformation Strategies:**

**Dictionary ↔ Internal Format:**
```csharp
public List<Dictionary<string, object?>> TransformFromDictionary(
    List<Dictionary<string, object?>> inputData,
    IReadOnlyList<ColumnDefinition> columns)
```

**DataTable ↔ Internal Format:**
```csharp
public List<Dictionary<string, object?>> TransformFromDataTable(
    DataTable dataTable,
    IReadOnlyList<ColumnDefinition> columns)

public DataTable TransformToDataTable(
    IReadOnlyList<Dictionary<string, object?>> data,
    IReadOnlyList<ColumnDefinition> columns,
    bool includeValidAlerts = false)
```

#### **DataTable Creation Analysis:**
```csharp
// Create DataTable columns
foreach (var column in columns.Where(c => c.IsVisible))
{
    var dataType = GetDataTableType(column.DataType);
    var dataColumn = new DataColumn(column.Name, dataType);
    dataColumn.AllowDBNull = true;
    dataColumn.Caption = column.DisplayName ?? column.Name;
    
    dataTable.Columns.Add(dataColumn);
}
```

**Column Creation Strategy:**
- **Visibility Filtering** - Only visible columns included
- **Type Mapping** - GetDataTableType() handles nullable types
- **AllowDBNull = true** - Permits null values in all columns
- **Caption Setting** - Uses DisplayName or falls back to Name

#### **Type Conversion Logic:**

**GetDataTableType() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Persistence.DataGridTransformationService:344**
```csharp
private Type GetDataTableType(Type columnType)
{
    // Convert nullable types to their underlying types for DataTable
    var underlyingType = Nullable.GetUnderlyingType(columnType);
    if (underlyingType != null)
        return underlyingType;
        
    return columnType;
}
```

**Nullable Type Handling:**
- **Nullable.GetUnderlyingType()** - Extracts T from Nullable<T>
- **DataTable Compatibility** - DataTable handles nulls differently than Nullable<T>
- **Type Safety** - Prevents runtime type errors

#### **Value Transformation Methods:**

**TransformValueForImport() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Persistence.DataGridTransformationService:252**
```csharp
public object? TransformValueForImport(object? value, ColumnDefinition column)
{
    if (value == null) return null;
    
    try
    {
        // Handle boolean values
        if (column.DataType == typeof(bool) || column.DataType == typeof(bool?))
        {
            return value switch
            {
                bool b => b,
                string s => bool.TryParse(s, out var b) ? b : false,
                int i => i != 0,
                _ => false
            };
        }
        
        // ... more type conversions
        
        return Convert.ChangeType(value, column.DataType);
    }
    catch (Exception ex)
    {
        _logger?.LogWarning(ex, "Failed to convert value {Value} to type {Type} for column {Column}", 
            value, column.DataType.Name, column.Name);
        return GetDefaultValue(column.DataType);
    }
}
```

**Comprehensive Type Conversion:**
- **Null Handling** - Early return for null values
- **Boolean Conversion** - Multiple input formats (bool, string, int)
- **Pattern Matching** - Modern C# switch expressions
- **Fallback Strategy** - Convert.ChangeType() for unhandled types
- **Error Recovery** - Returns default value on conversion failure
- **Detailed Logging** - Logs conversion failures with context

#### **Type Conversion Strategies:**

**String Conversion:**
```csharp
if (column.DataType == typeof(string))
    return value.ToString();
```

**Numeric Conversion:**
```csharp
if (column.DataType == typeof(int) || column.DataType == typeof(int?))
{
    if (int.TryParse(value.ToString(), out var intValue))
        return intValue;
    return column.DataType == typeof(int?) ? (int?)null : 0;
}
```

**DateTime Conversion:**
```csharp
if (column.DataType == typeof(DateTime) || column.DataType == typeof(DateTime?))
{
    if (DateTime.TryParse(value.ToString(), out var dateValue))
        return dateValue;
    return column.DataType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
}
```

**Conversion Pattern Analysis:**
- **TryParse Pattern** - Safe parsing without exceptions
- **Nullable Type Handling** - Different defaults for nullable vs non-nullable
- **Consistent Fallbacks** - Zero values for failed conversions
- **Type-specific Logic** - Appropriate conversion for each type

#### **Data Validation Helpers:**

**IsRowEmpty() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Persistence.DataGridTransformationService:329**
```csharp
public bool IsRowEmpty(Dictionary<string, object?> rowData)
{
    return rowData.Values.All(v => v == null || 
        (v is string s && string.IsNullOrWhiteSpace(s)));
}
```

**Empty Row Detection:**
- **LINQ All()** - Checks all values in row
- **Null Check** - Handles null values
- **String Empty Check** - Whitespace-only strings considered empty
- **Type Pattern** - `v is string s` pattern matching

---

## 2. INFRASTRUCTURE FACTORIES

### 2.1 DataGridFactory.cs - Component Factory
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Factories.DataGridFactory:15`

```csharp
public static class DataGridFactory
```

#### **Prečo Static Factory Class?**
- **Stateless Operations** - No instance state needed
- **Simple API** - Easy to use factory methods
- **Performance** - No object allocation for factory
- **Namespace Organization** - Groups related factory methods

#### **Factory Methods Analysis:**

**CreateForUI() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Factories.DataGridFactory:21**
```csharp
public static Result<AdvancedWinUiDataGrid> CreateForUI(ILogger? logger = null)
{
    try
    {
        var dataGrid = AdvancedWinUiDataGrid.CreateForUI(logger);
        logger?.LogInformation("DataGrid created successfully for UI mode via factory");
        return Result<AdvancedWinUiDataGrid>.Success(dataGrid);
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Failed to create DataGrid for UI mode via factory");
        return Result<AdvancedWinUiDataGrid>.Failure("Failed to create DataGrid for UI mode", ex);
    }
}
```

**Delegation Pattern:**
- **Delegates to Main API** - Calls AdvancedWinUiDataGrid.CreateForUI()
- **Exception Wrapping** - Converts exceptions to Result<T>
- **Additional Logging** - "via factory" context
- **Result<T> Consistency** - Maintains monadic error handling

**Architectural Questions:**
- **Redundant Factory?** - Does this add value over direct API calls?
- **Exception Translation** - Should factory handle exceptions differently?
- **Logging Duplication** - Main API likely also logs creation

#### **Headless Factory:**
```csharp
public static Result<AdvancedWinUiDataGrid> CreateHeadless(ILogger? logger = null)
{
    try
    {
        var dataGrid = AdvancedWinUiDataGrid.CreateHeadless(logger);
        logger?.LogInformation("DataGrid created successfully for headless mode via factory");
        return Result<AdvancedWinUiDataGrid>.Success(dataGrid);
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Failed to create DataGrid for headless mode via factory");
        return Result<AdvancedWinUiDataGrid>.Failure("Failed to create DataGrid for headless mode", ex);
    }
}
```

**Symmetric Design:**
- **Consistent Pattern** - Same structure as CreateForUI()
- **Mode-specific Logging** - Distinguishes headless from UI mode
- **Error Handling** - Same exception handling strategy

---

## 3. INFRASTRUCTURE PERSISTENCE

### 3.1 DataGridSearchService.cs - Search Infrastructure
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Persistence.DataGridSearchService`

Search service implementation in Infrastructure layer poskytuje concrete search algorithms a optimizations.

---

## 4. ARCHITECTURAL DECISIONS

### 4.1 Service Layer Organization

**Three-Tier Service Structure:**
1. **Application Services** - High-level orchestration (DataGridApplicationService)
2. **Domain Services** - Business logic services (in Application layer)
3. **Infrastructure Services** - Technical implementation services (DataGridPerformanceService)

**Prečo táto štruktúra?**
- **Separation of Concerns** - Each layer has distinct responsibility
- **Dependency Flow** - Infrastructure → Application → Domain
- **Testability** - Each layer can be tested independently
- **Flexibility** - Infrastructure implementations can be swapped

### 4.2 Error Handling Strategy

**Result<T> Pattern Throughout:**
```csharp
public async Task<Result<bool>> InitializeAsync()
public async Task<Result<DataOperationResult>> ProcessDataOperationAsync()
public async Task<Result<GridPerformanceStatistics>> GetPerformanceStatisticsAsync()
```

**Consistent Error Handling:**
- **No Exceptions for Business Logic** - Result<T> wraps success/failure
- **Exception Logging** - All exceptions logged with context
- **Graceful Degradation** - Default values returned on failures
- **Error Context** - Meaningful error messages for troubleshooting

### 4.3 Logging Strategy

**Structured Logging Throughout:**
```csharp
_logger.LogDebug("Transforming {RowCount} dictionary rows", inputData.Count);
_logger.LogError(ex, "Failed to process data operation {OperationType}", operationType);
_logger.LogWarning("Operation not found for stopping: {OperationName}", operationName);
```

**Logging Levels:**
- **Trace** - Operation start/stop details
- **Debug** - Transformation progress, service lifecycle
- **Information** - Service initialization, major operations
- **Warning** - Missing operations, conversion failures
- **Error** - Exception details with context

### 4.4 Performance Monitoring Architecture

**Three-Level Performance Monitoring:**
1. **Operation-Level** - Individual operation timing (StartOperation/StopOperation)
2. **Service-Level** - Service initialization and lifecycle timing
3. **System-Level** - Memory usage and overall statistics

**Performance Data Flow:**
```
Operation → PerformanceMetric → PerformanceHistory → GridPerformanceStatistics
```

### 4.5 Dependency Injection Patterns

**Constructor Injection:**
```csharp
public DataGridApplicationService(
    ILogger logger,
    DataGridPerformanceService performanceService,
    DataGridValidationService validationService,
    DataGridConfiguration configuration)
```

**Benefits:**
- **Explicit Dependencies** - Clear what service needs
- **Immutable Dependencies** - readonly fields prevent modification
- **Testability** - Easy to inject mocks
- **Compile-time Safety** - Missing dependencies caught at construction

### 4.6 Type Transformation Strategy

**Flexible Type System:**
- **Dictionary<string, object?>** - Internal format
- **DataTable** - External .NET format  
- **List<Dictionary>** - JSON-like format
- **ColumnDefinition-driven** - Schema-aware transformations

**Type Safety Measures:**
- **TryParse Patterns** - Safe type conversions
- **Default Value Fallbacks** - Prevents null reference exceptions
- **Nullable Type Handling** - Proper nullable/non-nullable conversion
- **Exception Recovery** - Logging + default values on conversion failure

---

## 5. PERFORMANCE CONSIDERATIONS

### 5.1 Memory Management

**Object Pooling Opportunities:**
```csharp
// Current: New objects created frequently
var transformedData = new List<Dictionary<string, object?>>();
var dictionary = new Dictionary<string, object?>();

// Potential: Object pooling for large datasets
var dictionary = _dictionaryPool.Get();
// ... use dictionary
_dictionaryPool.Return(dictionary);
```

### 5.2 Collection Performance

**Dictionary Access Patterns:**
```csharp
// O(1) lookup performance
if (_activeOperations.TryGetValue(operationName, out var stopwatch))

// O(1) insertion/update
_activeOperations[operationName] = stopwatch;
```

**List Performance:**
```csharp
// O(1) append
_performanceHistory.Add(metric);

// O(n) LINQ operations
var avgTime = _performanceHistory.Average(m => m.Duration.Ticks);
```

### 5.3 Type Conversion Performance

**Optimization Opportunities:**
- **Type Cache** - Cache reflection-based type information
- **Conversion Cache** - Cache frequent conversion patterns
- **Bulk Operations** - Batch process multiple values
- **Parallel Processing** - Parallel.ForEach for large datasets

---

## 6. TESTING STRATEGY

### 6.1 Infrastructure Service Testing
```csharp
[Fact]
public void DataGridPerformanceService_StartStopOperation_ShouldTrackTiming()
{
    // Arrange
    var logger = Mock.Of<ILogger>();
    var service = new DataGridPerformanceService(logger);
    
    // Act
    service.StartOperation("TestOperation");
    Thread.Sleep(100); // Simulate operation
    var duration = service.StopOperation("TestOperation");
    
    // Assert
    duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(90));
    duration.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
}
```

### 6.2 Transformation Testing
```csharp
[Theory]
[InlineData("true", true)]
[InlineData("false", false)]
[InlineData("1", true)]
[InlineData("0", false)]
[InlineData(123, true)]
[InlineData(0, false)]
public void TransformValueForImport_Boolean_ShouldConvertCorrectly(object input, bool expected)
{
    // Arrange
    var service = new DataGridTransformationService();
    var column = ColumnDefinition.Boolean("TestColumn");
    
    // Act
    var result = service.TransformValueForImport(input, column);
    
    // Assert
    result.Should().Be(expected);
}
```

### 6.3 Factory Testing
```csharp
[Fact]
public void DataGridFactory_CreateForUI_ShouldReturnSuccessResult()
{
    // Act
    var result = DataGridFactory.CreateForUI();
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeOfType<AdvancedWinUiDataGrid>();
}
```

---

## 7. FUTURE ARCHITECTURAL IMPROVEMENTS

### 7.1 Advanced Performance Monitoring
```csharp
public interface IAdvancedPerformanceService
{
    Task<PerformanceReport> GenerateReportAsync(TimeSpan period);
    void SetPerformanceThresholds(Dictionary<string, TimeSpan> thresholds);
    event EventHandler<PerformanceAlertEventArgs> PerformanceAlert;
}
```

### 7.2 Caching Infrastructure
```csharp
public interface IDataGridCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expiry);
    Task InvalidateAsync(string pattern);
}
```

### 7.3 Background Processing
```csharp
public interface IBackgroundTaskService
{
    Task QueueBackgroundTask(Func<CancellationToken, Task> task);
    Task<TResult> QueueBackgroundTask<TResult>(Func<CancellationToken, Task<TResult>> task);
}
```

### 7.4 Enhanced Transformation Pipeline
```csharp
public interface ITransformationPipeline
{
    ITransformationPipeline AddStep<T>(ITransformationStep<T> step);
    Task<Result<TOutput>> TransformAsync<TInput, TOutput>(TInput input);
}
```

---

## 8. ENTERPRISE PATTERNS

### 8.1 Circuit Breaker Pattern
```csharp
public class DataGridCircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _threshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);
    
    public async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> operation)
    {
        if (_failureCount >= _threshold && 
            DateTime.UtcNow - _lastFailureTime < _timeout)
        {
            return Result<T>.Failure("Circuit breaker is open");
        }
        
        try
        {
            var result = await operation();
            if (result.IsSuccess)
            {
                _failureCount = 0; // Reset on success
            }
            return result;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            throw;
        }
    }
}
```

### 8.2 Retry Pattern with Exponential Backoff
```csharp
public static class RetryPolicy
{
    public static async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Result<T>>> operation,
        int maxRetries = 3,
        TimeSpan baseDelay = default)
    {
        if (baseDelay == default) baseDelay = TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var result = await operation();
                if (result.IsSuccess || attempt == maxRetries)
                {
                    return result;
                }
                
                // Exponential backoff
                var delay = TimeSpan.FromMilliseconds(
                    baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                // Log and retry
                var delay = TimeSpan.FromMilliseconds(
                    baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
        }
        
        return Result<T>.Failure("Max retry attempts exceeded");
    }
}
```

### 8.3 Bulkhead Pattern for Resource Isolation
```csharp
public class DataGridResourceManager
{
    private readonly SemaphoreSlim _importSemaphore = new(maxConcurrency: 2);
    private readonly SemaphoreSlim _exportSemaphore = new(maxConcurrency: 2);
    private readonly SemaphoreSlim _searchSemaphore = new(maxConcurrency: 5);
    
    public async Task<Result<T>> ExecuteImportAsync<T>(Func<Task<Result<T>>> operation)
    {
        await _importSemaphore.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            _importSemaphore.Release();
        }
    }
    
    // Similar methods for export and search...
}
```

---

## ZÁVER - Infrastructure Layer Summary

Infrastructure layer poskytuje **robust technical foundation** pre AdvancedWinUiDataGrid s dôrazom na:

1. **Clean Architecture Compliance** - Proper layer separation a dependency inversion
2. **Performance Monitoring** - Comprehensive timing a resource usage tracking
3. **Type Safety** - Robust type conversion s error recovery
4. **Error Handling** - Result<T> pattern s structured logging
5. **Enterprise Patterns** - Scalability a reliability patterns
6. **Testability** - Clear interfaces a dependency injection

Kľúčové silné stránky:
- **Flexible Data Transformation** - Supports multiple external formats
- **Performance Visibility** - Detailed monitoring a statistics
- **Error Recovery** - Graceful handling of transformation failures
- **Service Orchestration** - Clean coordination of multiple services

Oblasti pre improvement:
- **Caching Strategy** - Add intelligent caching layer
- **Background Processing** - Async heavy operations
- **Circuit Breaker** - Fault tolerance patterns
- **Resource Management** - Better memory a thread management

Táto infrastructure architektúra poskytuje **solid foundation** pre enterprise-grade DataGrid functionality.