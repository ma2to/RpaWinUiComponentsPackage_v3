# AdvancedWinUiDataGrid - Domain Layer Architectural Documentation

## Kompletné Vysvetlenie Domain Driven Design (DDD) Implementácie

### Úvod do Domain Layer

Domain layer reprezentuje **jadro našej aplikácie** - obsahuje všetky obchodné pravidlá, entity, value objects a domain services. Je to **nezávislá vrstva** ktorá nepozná infraštruktúru ani prezentačnú vrstvu.

### Prečo sme sa rozhodli pre DDD prístup?

#### **Výhody DDD prístupu:**
1. **Business Logic Separation** - Obchodná logika je oddelená od technických detailov
2. **Domain Expert Communication** - Kód hovorí jazykom domény (ubiquitous language)
3. **Rich Domain Model** - Entities obsahujú správanie, nie len dáta
4. **Testability** - Domain layer sa dá testovať bez závislostí
5. **Long-term Maintainability** - Jasná štruktúra uľahčuje rozšírenia

#### **Nevýhody DDD prístupu:**
1. **Learning Curve** - Vyžaduje pochopenie DDD patterns
2. **Initial Complexity** - Na začiatku môže vyzerať nad-engineered
3. **Potential Over-Engineering** - Pre malé aplikácie môže byť zbytočne komplexný

---

## 1. ENTITIES - Doménové Entity

### 1.1 GridState.cs - Aggregate Root 
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridState:20`

```csharp
public sealed class GridState : Entity<Guid>
```

#### **Prečo GridState ako Aggregate Root?**
- **Consistency Boundary** - GridState zaisťuje konzistenciu celého gridu
- **Transaction Boundary** - Všetky zmeny idú cez GridState
- **Encapsulation** - Skrýva komplexnosť interného stavu

#### **Analýza Kľúčových Properties:**

**Core State Properties:**
```csharp
public IReadOnlyList<ColumnDefinition> Columns { get; private set; }  // Immutable columns
public List<GridRow> Rows { get; private set; }                       // Mutable rows
public Dictionary<int, bool> CheckboxStates { get; private set; }     // Row selections
public List<int>? FilteredRowIndices { get; set; }                   // Filter results
```

**Prečo tieto štruktúry?**

1. **IReadOnlyList<ColumnDefinition>** - Columns are immutable once set
   - **Výhoda:** Prevents accidental column modification
   - **Nevýhoda:** Requires rebuilding for column changes

2. **List<GridRow>** - Mutable collection for performance
   - **Výhoda:** Fast row additions/deletions
   - **Nevýhoda:** No encapsulation of row operations

3. **Dictionary<int, bool> CheckboxStates** - O(1) checkbox lookups
   - **Výhoda:** Constant time access
   - **Nevýhoda:** Memory usage for large grids

#### **State Tracking Properties:**
```csharp
public int Version { get; private set; }          // Change tracking
public DateTime LastModified { get; private set; } // Audit trail
public DateTime CreatedAt { get; private set; }    // Creation timestamp
```

**Prečo Version-based Change Tracking?**
- **Optimistic Concurrency** - Prevents lost updates
- **State Validation** - Ensures data integrity
- **Audit Requirements** - Enterprise compliance

#### **Factory Methods Analysis:**

**GridState.Create() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridState:107**
```csharp
public static GridState Create(
    IReadOnlyList<ColumnDefinition> columns,
    ColorConfiguration? colorConfiguration = null,
    ValidationConfiguration? validationConfiguration = null,
    PerformanceConfiguration? performanceConfiguration = null)
```

**Prečo Factory Pattern?**
- **Validation** - Ensures valid aggregate creation
- **Encapsulation** - Hides complex construction logic
- **Flexibility** - Easy to extend with new parameters

**Analýza parametrov:**
- `IReadOnlyList<ColumnDefinition> columns` - **Required** - Core grid structure
- `ColorConfiguration? colorConfiguration` - **Optional** - UI concerns
- `ValidationConfiguration? validationConfiguration` - **Optional** - Business rules
- `PerformanceConfiguration? performanceConfiguration` - **Optional** - Technical concerns

**Výhody tohto prístupu:**
1. **Separation of Concerns** - UI/Business/Technical concerns separated
2. **Default Values** - Smart defaults pre optional parameters
3. **Extensibility** - Easy to add new configuration types

#### **State Modification Methods:**

**UpdateState() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridState:138**
```csharp
public void UpdateState()
{
    Version++;
    LastModified = DateTime.UtcNow;
}
```

**Prečo táto metóda?**
- **Atomic Updates** - Thread-safe version increment
- **Audit Trail** - Automatic timestamp update
- **Change Detection** - External systems can detect changes

#### **Query Methods:**

**GetVisibleRowCount() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridState:173**
```csharp
public int GetVisibleRowCount()
{
    return FilteredRowIndices?.Count ?? Rows.Count;
}
```

**Prečo táto logika?**
- **Null Coalescing** - FilteredRowIndices == null means "show all"
- **Performance** - O(1) operation instead of filtering
- **Clarity** - Explicit visible vs total count

---

### 1.2 GridRow.cs - Entity
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridRow:13`

```csharp
public class GridRow : Entity
```

#### **Prečo GridRow ako Entity?**
- **Identity** - Each row has unique ID
- **Mutable State** - Row data can change
- **Behavior** - Contains validation and data access logic

#### **Analýza Internal Storage:**
```csharp
private readonly Dictionary<string, object?> _data;
private readonly List<string> _validationErrors;
private readonly List<ValidationError> _validationErrorObjects;
```

**Dictionary<string, object?> _data - Prečo?**
- **Flexibility** - Can store any column type
- **Dynamic Schema** - Columns can be added/removed
- **Performance** - O(1) access by column name
- **Type Safety Issue** - object? can lead to runtime errors

**Dual Validation Storage - Prečo?**
```csharp
private readonly List<string> _validationErrors;           // Legacy support
private readonly List<ValidationError> _validationErrorObjects; // Rich validation
```

**Výhody:**
- **Backward Compatibility** - Supports existing string-based errors
- **Rich Information** - ValidationError objects contain metadata
- **Migration Path** - Gradual transition to rich validation

**Nevýhody:**
- **Duplication** - Two storage mechanisms for same concept
- **Synchronization** - Risk of inconsistency
- **Memory Overhead** - Extra storage requirements

#### **Key Methods Analysis:**

**SetValue() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridRow:48**
```csharp
public void SetValue(string columnName, object? value)
{
    _data[columnName] = value;
    // Domain event could be raised here
}
```

**Prečo táto implementácia?**
- **Simplicity** - Direct dictionary assignment
- **Flexibility** - Accepts any value type
- **Domain Events** - Comment indicates future extension point

**Potential Issues:**
- **No Validation** - Value type not checked
- **No Change Detection** - Always overwrites
- **No Audit Trail** - No history of changes

#### **Validation Methods:**

**AddValidationError(ValidationError error) - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridRow:62**
```csharp
public void AddValidationError(ValidationError error)
{
    if (!_validationErrorObjects.Any(e => e.Property == error.Property && e.Message == error.Message))
    {
        _validationErrorObjects.Add(error);
    }
}
```

**Prečo táto logika?**
- **Duplicate Prevention** - Avoids same error multiple times
- **Composite Equality** - Checks both Property and Message
- **LINQ Usage** - Any() for readability

**Performance Consideration:**
- **O(n) Check** - Any() iterates through all errors
- **Alternative:** HashSet<ValidationError> for O(1) duplicate check

---

### 1.3 GridColumn.cs - Entity
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridColumn:16`

```csharp
public class GridColumn : Entity
```

#### **Prečo GridColumn ako Entity?**
- **Column Identity** - Each column has unique ID
- **Runtime Behavior** - Width, visibility can change
- **User Interaction** - Resizing, reordering

#### **Immutable vs Mutable Properties:**
```csharp
public string Name { get; }        // Immutable - column identity
public Type DataType { get; }      // Immutable - cannot change type
public bool IsVisible { get; set; } // Mutable - user can hide/show
public double Width { get; set; }   // Mutable - user can resize
```

**Design Rationale:**
- **Name + DataType** are immutable because they define column identity
- **Display properties** are mutable for user customization
- **Business properties** (AllowSorting) are mutable for configuration

#### **Domain Methods:**

**SetWidth() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities.GridColumn:50**
```csharp
public void SetWidth(double width)
{
    if (width <= 0)
        throw new ArgumentException("Width must be positive", nameof(width));
    
    Width = width;
    // Domain event could be raised here
}
```

**Prečo táto metóda namiesto property setter?**
- **Business Rules** - Width validation
- **Domain Events** - Future event publishing
- **Encapsulation** - Controlled width setting
- **Consistency** - All mutations go through methods

---

## 2. VALUE OBJECTS - Doménové Value Objects

### 2.1 ColumnDefinition.cs - Rich Value Object
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:15`

```csharp
public record ColumnDefinition
```

#### **Prečo Record Type?**
- **Immutability** - Records are immutable by default
- **Value Equality** - Structural equality comparison
- **Init-only Properties** - Can set during construction only
- **Concise Syntax** - Less boilerplate code

#### **Property Groups Analysis:**

**Core Properties - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:17**
```csharp
public required string Name { get; init; }
public string? DisplayName { get; init; }
public required Type DataType { get; init; }
public string PropertyName { get; init; } = string.Empty;
```

**Required vs Optional Analysis:**
- `Name` - **Required** - Column identifier, cannot be null
- `DisplayName` - **Optional** - Falls back to Name if null
- `DataType` - **Required** - Essential for data binding
- `PropertyName` - **Optional** - Defaults to empty, falls back to Name

#### **Behavior Configuration Properties:**
```csharp
public bool IsRequired { get; init; }
public bool IsReadOnly { get; init; }
public bool IsVisible { get; init; } = true;
public bool AllowSorting { get; init; } = true;
public bool AllowFiltering { get; init; } = true;
public bool AllowResizing { get; init; } = true;
public bool AllowReordering { get; init; } = true;
```

**Default Values Strategy:**
- **Permissive Defaults** - Most capabilities enabled by default
- **Explicit Disable** - Must explicitly disable functionality
- **User Expectations** - Matches common DataGrid behavior

#### **Factory Methods - Type-Safe Construction:**

**Text() Factory - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:103**
```csharp
public static ColumnDefinition Text(string name, string? displayName = null, string? propertyName = null)
{
    return new ColumnDefinition
    {
        Name = name,
        DisplayName = displayName,
        DataType = typeof(string),
        PropertyName = propertyName ?? name,
        Alignment = ColumnAlignment.Left
    };
}
```

**Prečo Factory Methods namiesto Constructor?**
- **Type Safety** - Cannot create invalid combinations
- **Smart Defaults** - Appropriate defaults per data type
- **Readability** - ColumnDefinition.Text("Name") vs new ColumnDefinition(...)
- **Domain Language** - Methods match business concepts

#### **Numeric Factory Analysis:**
```csharp
public static ColumnDefinition Numeric<T>(string name, string? displayName = null, string? displayFormat = null) 
    where T : struct, IComparable<T>
```

**Generic Constraint Analysis:**
- `struct` - Ensures value type (not nullable)
- `IComparable<T>` - Enables sorting functionality
- **Type Safety** - Prevents string being used with Numeric()
- **Performance** - No boxing for value types

#### **Special Column Factories:**

**CheckBox() Factory - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:215**
```csharp
public static ColumnDefinition CheckBox(string name, string? displayName = null)
{
    return new ColumnDefinition
    {
        Name = name,
        DisplayName = displayName,
        DataType = typeof(bool),
        PropertyName = name,
        SpecialType = SpecialColumnType.CheckBox,
        Alignment = ColumnAlignment.Center,
        Width = ColumnWidth.Pixels(80),
        AllowSorting = true,
        AllowFiltering = true
    };
}
```

**Specialized Configuration:**
- **SpecialType** - Indicates special rendering
- **Fixed Width** - CheckBoxes don't need variable width
- **Center Alignment** - Visual consistency
- **Sorting/Filtering** - Boolean values are sortable/filterable

#### **DeleteRow Factory Analysis:**
```csharp
public static ColumnDefinition DeleteRow(string displayName = "Actions", bool requireConfirmation = true, string? confirmationMessage = null)
```

**Safety Features:**
- **Default Confirmation** - Prevents accidental deletions
- **Configurable Message** - Custom confirmation text
- **Read-Only** - Cannot edit action column
- **No Sorting/Filtering** - Action columns don't sort

#### **Auto-Detection Logic:**

**DetectSpecialColumnType() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:364**
```csharp
private static SpecialColumnType DetectSpecialColumnType(string name)
{
    var lowerName = name.ToLowerInvariant();
    
    return lowerName switch
    {
        _ when lowerName.Contains("checkbox") || lowerName.Contains("check") || lowerName.Contains("selected") => SpecialColumnType.CheckBox,
        _ when lowerName.Contains("delete") || lowerName.Contains("remove") || lowerName.Contains("action") => SpecialColumnType.DeleteRow,
        _ when lowerName.Contains("valid") || lowerName.Contains("error") || lowerName.Contains("alert") => SpecialColumnType.ValidAlerts,
        _ => SpecialColumnType.None
    };
}
```

**Naming Convention Strategy:**
- **Flexible Patterns** - Multiple keywords per type
- **Case Insensitive** - ToLowerInvariant() for reliability
- **Contains Logic** - Partial matches (not exact)
- **Priority Order** - First match wins

#### **Type Safety Helpers:**

**IsNumericType() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:357**
```csharp
private static bool IsNumericType(Type type)
{
    return type == typeof(int) || type == typeof(long) || type == typeof(decimal) || 
           type == typeof(double) || type == typeof(float) || type == typeof(short) ||
           type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort);
}
```

**Comprehensive Numeric Support:**
- **All CLR Numeric Types** - int, long, decimal, double, float, short, uint, ulong, ushort
- **No Nullable Types** - Handles only non-nullable numerics
- **Explicit List** - Clear and maintainable

---

### 2.2 ColumnWidth Value Object
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:384`

```csharp
public record ColumnWidth
{
    public double Value { get; init; }
    public ColumnWidthType Type { get; init; }
    public double? MinWidth { get; init; }
    public double? MaxWidth { get; init; }
    
    public static ColumnWidth Auto(double? minWidth = null, double? maxWidth = null) => 
        new() { Type = ColumnWidthType.Auto, MinWidth = minWidth, MaxWidth = maxWidth };
    
    public static ColumnWidth Star(double value = 1.0, double? minWidth = null, double? maxWidth = null) => 
        new() { Value = value, Type = ColumnWidthType.Star, MinWidth = minWidth, MaxWidth = maxWidth };
    
    public static ColumnWidth Pixels(double pixels, double? minWidth = null, double? maxWidth = null) => 
        new() { Value = pixels, Type = ColumnWidthType.Pixels, MinWidth = minWidth, MaxWidth = maxWidth };
}
```

#### **Enhanced WPF/WinUI Alignment with Constraints:**
- **Auto** - Content-based sizing (like GridLength.Auto) with optional min/max constraints
- **Star** - Proportional sizing (like GridLength.Star) with optional min/max constraints
- **Pixels** - Fixed pixel sizing (like GridLength.Absolute) with optional min/max constraints

#### **Min/Max Width Enhancement:**
```csharp
// Basic usage (unchanged)
var basicWidth = ColumnWidth.Auto();
var starWidth = ColumnWidth.Star(2.0);
var pixelWidth = ColumnWidth.Pixels(150);

// Enhanced usage with constraints
var constrainedAuto = ColumnWidth.Auto(minWidth: 50, maxWidth: 300);
var constrainedStar = ColumnWidth.Star(1.5, minWidth: 100, maxWidth: 500);
var constrainedPixels = ColumnWidth.Pixels(200, minWidth: 100, maxWidth: 400);
```

**Prečo Min/Max Width Support?**
- **UI Flexibility** - Prevents columns from becoming too narrow or wide
- **User Experience** - Maintains readable column widths during resizing
- **Responsive Design** - Adapts to different screen sizes while respecting constraints
- **Business Requirements** - Some columns need minimum width for content visibility

**Design Benefits:**
- **WPF Compatibility** - Matches and extends GridLength behavior
- **Type Safety** - Cannot create invalid width combinations
- **Factory Methods** - Clear intent: ColumnWidth.Pixels(100, minWidth: 50)
- **Immutability** - Width constraints cannot change after creation
- **Backward Compatibility** - Existing code continues to work without changes
- **Optional Constraints** - MinWidth/MaxWidth are optional (null by default)

---

### 2.3 ColumnValidationRule Value Object
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:405`

```csharp
public record ColumnValidationRule
{
    public required string RuleName { get; init; }
    public required Func<object?, ValidationError?> Validator { get; init; }
    public string? ErrorMessage { get; init; }
}
```

#### **Functional Validation Design:**
- **Func<object?, ValidationError?>** - Pure function validation
- **Nullable Return** - null = valid, ValidationError = invalid
- **Flexible Input** - object? accepts any value type

#### **Validation Rule Factories:**

**Required() Factory - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition:412**
```csharp
public static ColumnValidationRule Required(string? customMessage = null) =>
    new()
    {
        RuleName = "Required",
        ErrorMessage = customMessage ?? "Field is required",
        Validator = value => value == null || (value is string str && string.IsNullOrWhiteSpace(str))
            ? ValidationError.Create("Value", customMessage ?? "Field is required", value)
            : null
    };
```

**Validation Logic Analysis:**
- **Null Check** - `value == null`
- **String Empty Check** - `string.IsNullOrWhiteSpace(str)`
- **Type Pattern** - `value is string str`
- **Custom Message** - Falls back to default if null

#### **MaxLength Validation:**
```csharp
public static ColumnValidationRule MaxLength(int maxLength, string? customMessage = null) =>
    new()
    {
        RuleName = "MaxLength",
        ErrorMessage = customMessage ?? $"Maximum length is {maxLength} characters",
        Validator = value => value is string str && str.Length > maxLength
            ? ValidationError.Create("Value", customMessage ?? $"Maximum length is {maxLength} characters", value)
            : null
    };
```

**String Interpolation in Default:**
- **Dynamic Message** - Includes actual maxLength value
- **User Feedback** - Clear error message
- **Type Safety** - Only validates strings

---

### 2.4 DataGridConfiguration Value Object
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration.DataGridConfiguration:17`

```csharp
public record DataGridConfiguration
{
    // CORE: Basic configuration
    public string Name { get; init; } = "DataGrid";
    public bool IsReadOnly { get; init; } = false;
    
    // PERFORMANCE: Optimization settings
    public PerformanceConfiguration Performance { get; init; } = PerformanceConfiguration.Default;
    
    // VALIDATION: Business rules configuration
    public ValidationConfiguration Validation { get; init; } = ValidationConfiguration.Comprehensive;
    
    // UI: Presentation configuration (optional - only for UI mode)
    public UIConfiguration? UI { get; init; }
    
    // ENTERPRISE: Audit and monitoring
    public bool EnableAuditLog { get; init; } = true;
    public bool EnablePerformanceMonitoring { get; init; } = true;
}
```

#### **Configuration Composition Strategy:**
- **Nested Configuration Objects** - Each concern has own configuration
- **Optional UI** - Can run headless without UI configuration
- **Smart Defaults** - Performance.Default, Validation.Comprehensive
- **Enterprise Features** - Audit logging, performance monitoring

#### **Factory Methods:**

**ForUI() vs ForHeadless() - RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration.DataGridConfiguration:38**
```csharp
public static DataGridConfiguration ForUI(UIConfiguration uiConfig) =>
    Default with { UI = uiConfig };

public static DataGridConfiguration ForHeadless() =>
    Default with { UI = null };
```

**Record with Expression:**
- **Immutable Updates** - Creates new instance with changed property
- **Explicit Intent** - ForUI() vs ForHeadless() is clear
- **Performance** - Only changes specified properties

---

### 2.5 SearchResult Value Object
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter.SearchResult:10`

```csharp
public record SearchResult
{
    public IReadOnlyList<int> MatchingRowIndices { get; init; } = [];
    public int TotalMatches { get; init; }
    public int TotalSearched { get; init; }
    public string SearchTerm { get; init; } = string.Empty;
    public SearchCriteria? Criteria { get; init; }
    public bool HasResults => TotalMatches > 0;
    public int ReturnedMatches => MatchingRowIndices.Count;
    public TimeSpan SearchDuration { get; init; } = TimeSpan.Zero;
}
```

#### **Comprehensive Search Metadata:**
- **MatchingRowIndices** - Actual results (row indices)
- **TotalMatches** vs **ReturnedMatches** - Support for paging
- **SearchDuration** - Performance monitoring
- **Immutable Results** - Thread-safe result object

#### **Computed Properties:**
```csharp
public bool HasResults => TotalMatches > 0;
public int ReturnedMatches => MatchingRowIndices.Count;
```

**Prečo computed properties?**
- **Consistency** - Always accurate derived values
- **Readability** - HasResults is clearer than TotalMatches > 0
- **Performance** - Simple calculations, no storage overhead

#### **SearchCriteria Hierarchy:**

**Base SearchCriteria:**
```csharp
public record SearchCriteria
{
    public string SearchTerm { get; init; } = string.Empty;
    public IReadOnlyList<string>? ColumnNames { get; init; }
    public bool CaseSensitive { get; init; } = false;
    public bool WholeWordOnly { get; init; } = false;
    public SearchType Type { get; init; } = SearchType.Contains;
}
```

**Advanced SearchCriteria:**
```csharp
public record AdvancedSearchCriteria : SearchCriteria
{
    public bool UseRegex { get; init; } = false;
    public IReadOnlyDictionary<string, object?>? FilterValues { get; init; }
    public TimeSpan? Timeout { get; init; }
    public string SearchText => SearchTerm; // Backward compatibility
    public SearchScope Scope { get; init; } = SearchScope.AllColumns;
    public SearchAlgorithm Algorithm { get; init; } = SearchAlgorithm.Linear;
}
```

**Inheritance vs Composition Analysis:**
- **Record Inheritance** - Natural extension of base criteria
- **Backward Compatibility** - SearchText property aliases SearchTerm
- **Algorithm Selection** - Allows optimization strategy selection
- **Timeout Support** - Prevents long-running searches

---

### 2.6 ImportResult Value Object
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations.ImportResult:10`

```csharp
public record ImportResult
{
    public int TotalRows { get; init; }
    public int ImportedRows { get; init; }
    public int RejectedRows { get; init; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = [];
    public ImportMode Mode { get; init; }
    public bool IsSuccess => RejectedRows == 0;
    public double SuccessRate => TotalRows > 0 ? (double)ImportedRows / TotalRows * 100 : 0;
}
```

#### **Import Statistics:**
- **TotalRows** - Original row count
- **ImportedRows** - Successfully imported
- **RejectedRows** - Failed to import
- **Detailed Errors** - ValidationError objects for each failure

#### **Computed Success Metrics:**
```csharp
public bool IsSuccess => RejectedRows == 0;
public double SuccessRate => TotalRows > 0 ? (double)ImportedRows / TotalRows * 100 : 0;
```

**Business Logic in Value Object:**
- **IsSuccess** - Simple boolean for success/failure
- **SuccessRate** - Percentage for reporting/monitoring
- **Division by Zero Protection** - TotalRows > 0 check

---

## 3. DOMAIN INTERFACES

### 3.1 IDataGrid - Core Domain Contract
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces.IDataGrid`

Tento interface definuje základné kontrakty pre prácu s DataGrid na úrovni domény.

### 3.2 IDataGridRepository - Persistence Contract
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Repositories.IDataGridRepository`

Repository pattern interface pre persitentné operácie s GridState.

---

## 4. DOMAIN EVENTS

### 4.1 DomainEvent Base Class
**Lokácia:** `RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Events.DomainEvent`

Implementácia Domain Events pre reakciu na zmeny v doméne.

---

## 5. ARCHITECTURAL DECISIONS

### 5.1 Prečo Records pre Value Objects?

**Výhody:**
- **Immutability** - Records sú immutable by default
- **Structural Equality** - Automatic value-based equality
- **Init-only Properties** - Can set during construction only
- **Concise Syntax** - Menej boilerplate kódu

**Nevýhody:**
- **C# 9+ Requirement** - Older frameworks not supported
- **Learning Curve** - Records behavior differs from classes
- **Reference Type** - Still reference types, not true value types

### 5.2 Entity vs Value Object Rozhodnutia

**GridState - Entity (Aggregate Root)**
- **Identity** - Has unique Guid ID
- **Mutable** - State changes over time
- **Lifecycle** - Created, modified, persisted
- **Consistency Boundary** - Controls access to child entities

**ColumnDefinition - Value Object**
- **No Identity** - Identified by properties
- **Immutable** - Cannot change after creation
- **Replaceable** - Old definition replaced by new one
- **Equality by Value** - Two definitions with same properties are equal

### 5.3 Factory Pattern Usage

**Prečo Factory Methods?**
- **Complex Construction** - Multiple configuration options
- **Domain Language** - ColumnDefinition.Text() is clearer
- **Validation** - Ensures valid object creation
- **Extensibility** - Easy to add new factory methods

### 5.4 Validation Strategy

**Domain-Level Validation:**
- **Business Rules** - Validation belongs in domain
- **Functional Approach** - Func<object?, ValidationError?> validators
- **Composable** - Multiple validation rules per column
- **Type Safe** - ValidationError objects instead of strings

### 5.5 Search Architecture

**Value Object Design:**
- **Immutable Results** - Thread-safe search results
- **Rich Metadata** - Duration, criteria, statistics
- **Hierarchical Criteria** - Basic and Advanced search criteria
- **Algorithm Selection** - Supports multiple search strategies

---

## 6. PERFORMANCE CONSIDERATIONS

### 6.1 Collection Choices

**IReadOnlyList vs List vs Array:**
- **IReadOnlyList<ColumnDefinition>** - Prevents modification, clear intent
- **List<GridRow>** - Dynamic sizing, fast append operations
- **Array** for factory returns - Fixed size, minimal overhead

### 6.2 Dictionary Usage

**Dictionary<string, object?> for row data:**
- **O(1) Access** - Fast column value lookup
- **Dynamic Schema** - Supports runtime column changes
- **Memory Overhead** - Hash table overhead per row
- **Type Safety Issues** - object? requires casting

### 6.3 Validation Performance

**Dual validation storage trade-offs:**
- **Memory Usage** - Two lists per row
- **Compatibility** - Supports legacy string errors
- **Migration** - Gradual transition to rich validation
- **Performance** - Any() calls are O(n) for duplicate checking

---

## 7. FUTURE ARCHITECTURAL IMPROVEMENTS

### 7.1 Domain Events Implementation
```csharp
// Currently commented out:
// public void RaiseGridStateChangedEvent() { ... }

// Recommended implementation:
public class GridStateChangedEvent : DomainEvent
{
    public Guid GridStateId { get; }
    public int Version { get; }
    public string ChangeType { get; }
    
    public GridStateChangedEvent(Guid gridStateId, int version, string changeType)
    {
        GridStateId = gridStateId;
        Version = version;
        ChangeType = changeType;
    }
}
```

### 7.2 Better Type Safety for Row Data
```csharp
// Instead of Dictionary<string, object?>
// Consider strongly-typed approach:
public interface ITypedRowData
{
    T GetValue<T>(string columnName);
    void SetValue<T>(string columnName, T value);
    bool TryGetValue<T>(string columnName, out T value);
}
```

### 7.3 Specification Pattern for Complex Queries
```csharp
public abstract class GridSpecification
{
    public abstract Expression<Func<GridRow, bool>> ToExpression();
    
    public GridSpecification And(GridSpecification specification) => 
        new AndSpecification(this, specification);
    
    public GridSpecification Or(GridSpecification specification) => 
        new OrSpecification(this, specification);
}
```

---

## 8. TESTING STRATEGY

### 8.1 Domain Entity Testing
```csharp
[Fact]
public void GridState_Create_ShouldInitializeCorrectly()
{
    // Arrange
    var columns = new[] { ColumnDefinition.Text("Name") };
    
    // Act
    var gridState = GridState.Create(columns);
    
    // Assert
    gridState.Columns.Should().HaveCount(1);
    gridState.IsInitialized.Should().BeTrue();
    gridState.Version.Should().Be(1);
}
```

### 8.2 Value Object Testing
```csharp
[Fact]
public void ColumnDefinition_Text_ShouldHaveCorrectDefaults()
{
    // Act
    var column = ColumnDefinition.Text("TestColumn");
    
    // Assert
    column.Name.Should().Be("TestColumn");
    column.DataType.Should().Be(typeof(string));
    column.Alignment.Should().Be(ColumnAlignment.Left);
    column.AllowSorting.Should().BeTrue();
}
```

### 8.3 Factory Method Testing
```csharp
[Theory]
[InlineData(100, true)]
[InlineData(0, false)]
[InlineData(-10, false)]
public void ColumnWidth_Pixels_ShouldValidateInput(double pixels, bool shouldBeValid)
{
    // Act & Assert
    if (shouldBeValid)
    {
        var width = ColumnWidth.Pixels(pixels);
        width.Value.Should().Be(pixels);
        width.Type.Should().Be(ColumnWidthType.Pixels);
    }
    else
    {
        Action act = () => ColumnWidth.Pixels(pixels);
        act.Should().Throw<ArgumentException>();
    }
}
```

---

## ZÁVER - Domain Layer Summary

Domain layer tvorí **jadro našej aplikácie** s jasným oddelením obchodnej logiky od technických detailov. Kľúčové architekturálne rozhodnutia:

1. **DDD Approach** - Rich domain model s entities, value objects, aggregates
2. **Immutable Value Objects** - Thread-safe, composable configuration
3. **Factory Pattern** - Type-safe object construction s domain language
4. **Functional Validation** - Pure function validators s rich error objects  
5. **Aggregate Root** - GridState kontroluje konzistenciu celého gridu

Táto architektúra poskytuje **silný základ** pre complex enterprise DataGrid funkcionalitu s dôrazom na maintainability, testability a business logic clarity.
