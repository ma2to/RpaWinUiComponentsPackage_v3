# RpaWinUiComponentsPackage - KOMPLETNÁ PROFESIONÁLNA DOKUMENTÁCIA

> **👨‍💻 Developer Context**  
> Professional .NET 8.0 + WinUI3 Enterprise Component Package  
> Clean API Architecture with Zero Internal Namespace Exposure  
> Production-ready, scalable, maintainable solution  

## 📋 ROZŠÍRENÝ OBSAH DOKUMENTÁCIE

### **🏗️ ARCHITEKTÚRA A DESIGN**
1. [Prehľad Balíka](#1-prehľad-balíka)
2. [Professional Architecture Overview](#2-professional-architecture-overview)
3. [Clean API Design Patterns](#3-clean-api-design-patterns)
4. [Hybrid Functional-OOP Implementation](#4-hybrid-functional-oop-implementation)

### **🗃️ KOMPONENTY DETAILNE**
5. [AdvancedWinUiDataGrid - Complete Guide](#5-advancedwinuidatagrid-complete-guide)
6. [AdvancedWinUiLogger - Complete Guide](#6-advancedwinuilogger-complete-guide)
7. [Result<T> Monadic Error Handling](#7-result-monadic-error-handling)

### **💼 PRACTICAL IMPLEMENTATION**
8. [Usage Examples & Tutorials](#8-usage-examples-tutorials)
9. [Demo Application Guide](#9-demo-application-guide)
10. [Best Practices & Rules](#10-best-practices-rules)

---

## 1️⃣ PREHĽAD BALÍKA

### **🏢 Enterprise-Level Component Package**

**RpaWinUiComponentsPackage** je profesionálny komponentový balík pre enterprise WinUI3 aplikácie.

#### **📋 Základné Informácie**
- **📦 Názov:** RpaWinUiComponentsPackage
- **🎯 Typ:** Premium NuGet balík (.nupkg) pre WinUI3 aplikácie
- **🔧 Target Framework:** net8.0-windows10.0.19041.0
- **🏗️ Architektúra:** Clean API Design s nulovou expozíciou Internal namespace
- **📊 Performance Target:** 10M+ rows, sub-second response times

#### **🎯 Komponenty Balíka**

### **1. 🗃️ AdvancedWinUiDataGrid**
> **Profesionálna tabuľka s pokročilými funkciami**

### **2. 📝 AdvancedWinUiLogger**  
> **Enterprise logger s UI a file logging**

---

## 2️⃣ PROFESSIONAL ARCHITECTURE OVERVIEW

### **🏗️ Clean API Design Principles**

#### **✅ JEDNA USING DIREKTÍVA PRE UŽÍVATEĽA**
```csharp
// ✅ SPRÁVNE - Jediná using direktíva potrebná
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger;

// ❌ ZLÉ - Toto sa NIKDY nemá stať
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal;
```

#### **🔒 STRICT NAMESPACE ENCAPSULATION**
- **Public API:** Len verejné triedy a interfejsy
- **Internal Implementation:** Kompletne skryté od používateľov
- **Zero Exposure:** Internal namespace nie je nikdy viditeľný

---

## 3️⃣ CLEAN API DESIGN PATTERNS

### **📚 Result<T> Pattern**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    
    public static Result<T> Success(T value)
    public static Result<T> Failure(string errorMessage, Exception? exception = null)
}
```

**Používanie:**
```csharp
var result = await dataGrid.LoadDataAsync(data);
if (result.IsSuccess)
{
    _logger.LogInfo("Data loaded successfully: {Count} rows", result.Value.Count);
}
else
{
    _logger.LogError("Failed to load data: {Error}", result.ErrorMessage);
}
```

### **⚙️ Configuration Pattern**
Všetky komponenty používajú konfiguračné objekty pre nastavenie:
```csharp
public class DataGridConfiguration
{
    public DataGridColorConfiguration Colors { get; set; }
    public DataGridPerformanceConfiguration Performance { get; set; }
    public DataGridValidationConfiguration Validation { get; set; }
}
```

---

## 4️⃣ HYBRID FUNCTIONAL-OOP IMPLEMENTATION

### **🎯 Functional Approach**
- **Immutable Configurations:** Všetky konfigurácie sú immutable
- **Pure Functions:** Business logika v pure functions
- **Result Pattern:** Monadic error handling
- **Option Pattern:** Null safety

### **🏗️ OOP Approach**  
- **UI Components:** MVVM pattern pre WinUI3
- **Dependency Injection:** Constructor injection
- **Interface Segregation:** Malé, špecifické interfejsy
- **Encapsulation:** Private internal implementation

---

## 5️⃣ ADVANCEDWINUIDATAGRID - COMPLETE GUIDE

### **🎯 Overview**
AdvancedWinUiDataGrid je profesionálny DataGrid komponent navrhnutý pre enterprise aplikácie s podporou veľkých dátových sad (10M+ riadkov).

### **📋 Core Features**
- ✅ **High Performance:** Virtualizácia pre 10M+ riadkov
- ✅ **Advanced Search:** Multi-column regex search
- ✅ **Filtering System:** Complex filter expressions  
- ✅ **Sorting:** Multi-column sorting
- ✅ **Validation:** Real-time data validation
- ✅ **Color Theming:** Configurable color schemes
- ✅ **Export/Import:** Excel, CSV, JSON support
- ✅ **Responsive UI:** Adaptive layout system

### **🏗️ Class Structure**

#### **AdvancedDataGrid (Main Component)**
```csharp
public sealed partial class AdvancedDataGrid : UserControl
{
    // Public API Methods
    public async Task<Result<bool>> LoadDataAsync<T>(
        IEnumerable<T> data, 
        DataGridConfiguration? configuration = null)
    
    public async Task<Result<bool>> RefreshDataAsync()
    
    public async Task<Result<SearchResult>> SearchAsync(
        string searchTerm, 
        SearchOptions? options = null)
    
    public async Task<Result<bool>> ApplyFiltersAsync(
        IEnumerable<FilterExpression> filters)
    
    public async Task<Result<bool>> ValidateDataAsync(
        IProgress<ValidationProgress>? progress = null)
    
    public async Task<Result<ExportResult>> ExportDataAsync(
        ExportFormat format, 
        string filePath, 
        ExportOptions? options = null)
    
    public async Task<Result<ImportResult>> ImportDataAsync(
        string filePath, 
        ImportOptions? options = null)
    
    public Result<bool> UpdateConfiguration(DataGridConfiguration newConfiguration)
    
    // Properties
    public bool IsInitialized { get; }
    public int TotalRows { get; }
    public int VisibleRows { get; }
    public DataGridConfiguration Configuration { get; }
}
```

### **⚙️ Configuration Classes**

#### **DataGridConfiguration**
```csharp
public class DataGridConfiguration
{
    public DataGridColorConfiguration Colors { get; set; } = new();
    public DataGridPerformanceConfiguration Performance { get; set; } = new();
    public DataGridValidationConfiguration Validation { get; set; } = new();
    
    public static DataGridConfiguration CreateDefault() => new();
    public static DataGridConfiguration CreateDarkTheme() => // Dark theme preset
    public static DataGridConfiguration CreateHighPerformance() => // Performance preset
}
```

#### **DataGridColorConfiguration**
```csharp
public class DataGridColorConfiguration
{
    // Header Colors
    public string HeaderBackgroundColor { get; set; } = "#F5F5F5";
    public string HeaderForegroundColor { get; set; } = "#333333";
    public string HeaderBorderColor { get; set; } = "#CCCCCC";
    
    // Row Colors
    public string RowBackgroundColor { get; set; } = "#FFFFFF";
    public string AlternateRowBackgroundColor { get; set; } = "#F9F9F9";
    public string SelectedRowBackgroundColor { get; set; } = "#0078D4";
    public string HoverRowBackgroundColor { get; set; } = "#E3F2FD";
    
    // Cell Colors
    public string CellForegroundColor { get; set; } = "#333333";
    public string CellBorderColor { get; set; } = "#E0E0E0";
    
    // Validation Colors
    public string ErrorBackgroundColor { get; set; } = "#FFEBEE";
    public string ErrorForegroundColor { get; set; } = "#C62828";
    public string WarningBackgroundColor { get; set; } = "#FFF3E0";
    public string WarningForegroundColor { get; set; } = "#EF6C00";
    public string SuccessBackgroundColor { get; set; } = "#E8F5E8";
    public string SuccessForegrou5dColor { get; set; } = "#2E7D32";
    
    // Theme Settings
    public bool UseDarkTheme { get; set; } = false;
    
    // Methods
    public string GetValidationColor(ValidationLevel level) => level switch
    {
        ValidationLevel.Error => ErrorBackgroundColor,
        ValidationLevel.Warning => WarningBackgroundColor,
        ValidationLevel.Success => SuccessBackgroundColor,
        _ => RowBackgroundColor
    };
}
```

#### **DataGridPerformanceConfiguration**
```csharp
public class DataGridPerformanceConfiguration
{
    // Virtualization Settings
    public bool EnableVirtualization { get; set; } = true;
    public int VirtualizationThreshold { get; set; } = 1000;
    public int BufferSize { get; set; } = 50;
    
    // Rendering Settings
    public bool EnableAsyncRendering { get; set; } = true;
    public int RenderBatchSize { get; set; } = 100;
    public TimeSpan RenderDelay { get; set; } = TimeSpan.FromMilliseconds(16);
    
    // Memory Management
    public bool EnableMemoryOptimization { get; set; } = true;
    public int MaxCachedRows { get; set; } = 10000;
    
    // Search and Filter Performance
    public bool EnableAsyncSearch { get; set; } = true;
    public int SearchResultsLimit { get; set; } = 1000;
    
    public bool IsValid() => 
        VirtualizationThreshold > 0 && 
        BufferSize > 0 && 
        RenderBatchSize > 0 && 
        MaxCachedRows > 0 &&
        SearchResultsLimit > 0 &&
        RenderDelay >= TimeSpan.Zero;
}
```

#### **DataGridValidationConfiguration**
```csharp
public class DataGridValidationConfiguration
{
    public bool EnableRealTimeValidation { get; set; } = true;
    public bool ShowValidationIndicators { get; set; } = true;
    public bool StopOnFirstError { get; set; } = false;
    public ValidationMode ValidationMode { get; set; } = ValidationMode.OnEdit;
    
    public Dictionary<string, List<IValidationRule>> ColumnValidators { get; set; } = new();
    
    public void AddValidator<T>(string columnName, IValidationRule<T> validator)
    {
        if (!ColumnValidators.ContainsKey(columnName))
            ColumnValidators[columnName] = new List<IValidationRule>();
            
        ColumnValidators[columnName].Add(validator);
    }
}

public enum ValidationMode
{
    None,
    OnEdit,
    OnSave,
    RealTime
}

public enum ValidationLevel
{
    Success,
    Warning,
    Error
}
```

### **🔍 Search System**

#### **SearchOptions**
```csharp
public class SearchOptions
{
    public bool UseRegex { get; set; } = false;
    public bool CaseSensitive { get; set; } = false;
    public bool SearchAllColumns { get; set; } = true;
    public List<string> TargetColumns { get; set; } = new();
    public int MaxResults { get; set; } = 1000;
}
```

#### **SearchResult**
```csharp
public class SearchResult
{
    public int TotalMatches { get; set; }
    public List<SearchMatch> Matches { get; set; } = new();
    public bool WasLimited { get; set; }
    public TimeSpan SearchDuration { get; set; }
}

public class SearchMatch
{
    public int RowIndex { get; set; }
    public string ColumnName { get; set; }
    public string MatchedText { get; set; }
    public int StartPosition { get; set; }
    public int Length { get; set; }
}
```

### **🎨 Color System Priority Rules**

#### **Cell Background Color Priority (Highest to Lowest):**
1. **Validation Error Color** - `#FFEBEE` (červená pre chyby)
2. **Validation Warning Color** - `#FFF3E0` (oranžová pre upozornenia)  
3. **Selected Row Color** - `#0078D4` (modrá pre výber)
4. **Hover Row Color** - `#E3F2FD` (svetlo modrá pre hover)
5. **Alternate Row Color** - `#F9F9F9` (svetlo sivá pre párne riadky)
6. **Default Row Color** - `#FFFFFF` (biela pre nepárne riadky)

#### **Text Color Rules:**
- **Normal Text:** `#333333` (tmavo sivá)
- **Error Text:** `#C62828` (červená)
- **Warning Text:** `#EF6C00` (oranžová)  
- **Success Text:** `#2E7D32` (zelená)
- **Selected Text:** `#FFFFFF` (biela na modrém pozadí)

### **📏 Layout Rules**

#### **Column Width Rules:**
```csharp
// Automatické šírky stĺpcov
public enum ColumnWidthMode
{
    Auto,           // Podľa obsahu
    Fixed,          // Pevná šírka v pixeloch
    Percentage,     // Percentuálna šírka
    Star,          // Proporcionálna šírka (*)
    MinMax         // Min/Max obmedzenia
}
```

#### **Row Height Rules:**
- **Default Row Height:** 32px
- **Header Row Height:** 40px  
- **Minimum Row Height:** 24px
- **Maximum Row Height:** 200px
- **Auto-resize:** Podľa obsahu bunky

### **🔍 Advanced Search & Filter System**

#### **SearchAsync (Advanced)**
```csharp
public async Task<Result<SearchResult>> SearchAsync(
    AdvancedSearchCriteria criteria,
    CancellationToken cancellationToken = default)
```

**AdvancedSearchCriteria:**
```csharp
public record AdvancedSearchCriteria(
    string SearchText,
    IReadOnlyList<string>? TargetColumns = null,
    bool CaseSensitive = false,
    bool WholeWord = false,
    bool UseRegex = false,
    int MaxResults = 1000)
```

#### **Advanced Filtering**
```csharp
public async Task<Result<bool>> ApplyFiltersAsync(
    IReadOnlyList<AdvancedFilter> filters,
    CancellationToken cancellationToken = default)

public record AdvancedFilter(
    string ColumnName,
    FilterOperator Operator,
    object? Value,
    FilterLogicOperator LogicOperator = FilterLogicOperator.And)

public enum FilterOperator
{
    Equals, NotEquals, Contains, NotContains,
    StartsWith, EndsWith, GreaterThan, LessThan,
    GreaterOrEqual, LessOrEqual, IsNull, IsNotNull
}
```

### **📊 Core Import/Export API**

#### **Dictionary Import/Export**
```csharp
// Dictionary Import
public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
    List<Dictionary<string, object?>> data,
    Dictionary<int, bool>? checkboxStates = null,
    int? startRow = null,
    bool insertMode = false,
    TimeSpan? timeout = null,
    IProgress<ValidationProgress>? validationProgress = null)

// Dictionary Export
public async Task<List<Dictionary<string, object?>>> ExportToDictionaryAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)

// Dictionary Filtered Export
public async Task<List<Dictionary<string, object?>>> ExportFilteredToDictionaryAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)
```

#### **DataTable Import/Export**
```csharp
// DataTable Import
public async Task<Result<ImportResult>> ImportFromDataTableAsync(
    DataTable dataTable,
    Dictionary<int, bool>? checkboxStates = null,
    int? startRow = null,
    bool insertMode = false,
    TimeSpan? timeout = null,
    IProgress<ValidationProgress>? validationProgress = null)

// DataTable Export
public async Task<DataTable> ExportToDataTableAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)

// DataTable Filtered Export
public async Task<DataTable> ExportFilteredToDataTableAsync(
    bool includeValidAlerts = false,
    bool removeAfter = false,
    TimeSpan? timeout = null,
    IProgress<ExportProgress>? exportProgress = null)
```

#### **Import/Export Parameters Explanation**

##### **Import Parameters:**
- **`checkboxStates`** - Dictionary mapping row indices to checkbox states (relevant only if CheckBox column is enabled)
  - If CheckBox column is visible: Maps row indices to their checkbox values
  - If CheckBox column is hidden: Determines which rows should have internal checkbox state set to true
  - If null: All imported rows get default checkbox state (false)
- **`startRow`** - Starting row index for import (null = append to end)
- **`insertMode`** - Import behavior:
  - `false` (default): Replace existing data
  - `true`: Insert between existing rows at startRow position
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
// ]
```

### **🚨 Advanced Validation System**

#### **Multi-Level Validation Rules**

##### **1. Single Cell Validation**
```csharp
public record ValidationRule(
    string ColumnName,
    Func<object?, bool> Validator,
    string ErrorMessage,
    ValidationSeverity Severity = ValidationSeverity.Error,
    int Priority = 0)

// Example: Age validation
var ageRule = new ValidationRule(
    "Age",
    value => value is int age && age >= 18 && age <= 120,
    "Age must be between 18 and 120",
    ValidationSeverity.Error);
```

##### **2. Multi-Cell Same Row Validation**
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
```

##### **3. Cross-Row Validation**
```csharp
public record CrossRowValidationRule(
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, 
         IReadOnlyList<ValidationResult>> Validator,
    string ErrorMessage)

// Example: Unique email validation
var uniqueEmailRule = new CrossRowValidationRule(
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
    "Duplicate email addresses found");
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

#### **Validation Management API**
```csharp
// Single column validation rules
public async Task<Result<bool>> AddValidationRulesAsync(
    string columnName,
    IReadOnlyList<ValidationRule> rules)

public async Task<Result<bool>> RemoveValidationRulesAsync(
    params string[] columnNames)

public async Task<Result<bool>> ReplaceValidationRulesAsync(
    IReadOnlyDictionary<string, IReadOnlyList<ValidationRule>> columnRules)

// Cross-cell validation (same row)
public async Task<Result<bool>> AddCrossCellValidationAsync(
    CrossCellValidationRule rule)

// Cross-row validation
public async Task<Result<bool>> AddCrossRowValidationAsync(
    CrossRowValidationRule rule)

// Cross-column validation
public async Task<Result<bool>> AddCrossColumnValidationAsync(
    CrossColumnValidationRule rule)

// Complex validation
public async Task<Result<bool>> AddComplexValidationAsync(
    ComplexValidationRule rule)

// Conditional validation
public async Task<Result<bool>> AddConditionalValidationAsync(
    ConditionalValidationRule rule)
```

#### **Row Deletion Based on Validation**
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
    ValidationSeverity? MinimumSeverity = null,
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

public record ValidationDeletionOptions(
    bool RequireConfirmation = true,
    bool CreateBackup = true,
    int MaxDeletionLimit = 1000,
    bool AllowEmptyResult = false,
    IProgress<ValidationDeletionProgress>? Progress = null)

public record ValidationBasedDeleteResult(
    int TotalRowsEvaluated,
    int RowsDeleted,
    int RemainingRows,
    IReadOnlyList<ValidationError> ValidationErrors,
    TimeSpan OperationDuration,
    string? BackupLocation = null)
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
    customCriteria,
    new ValidationDeletionOptions { CreateBackup = true, MaxDeletionLimit = 500 });
```

### **🔧 Methods - Detailed Documentation**

#### **LoadDataAsync<T>**
```csharp
public async Task<Result<bool>> LoadDataAsync<T>(
    IEnumerable<T> data, 
    DataGridConfiguration? configuration = null)
```

**Purpose:** Načíta dáta do DataGrid komponentu s možnou konfiguráciou.

**Parameters:**
- `data` - Kolekcia dát typu T
- `configuration` - Voliteľná konfigurácia (použije default ak null)

**Returns:** `Result<bool>` - Success ak sa dáta načítali úspešne

**Behavior:**
1. Validuje vstupné parametre
2. Aplikuje konfiguráciu (default ak nie je poskytnutá)
3. Inicializuje virtualizáciu pre veľké datasety
4. Generuje stĺpce automaticky z typu T  
5. Načíta dáta s progress reporting
6. Spustí validáciu ak je povolená

**Usage:**
```csharp
var people = GetPeopleData();
var config = DataGridConfiguration.CreateDefault();
var result = await dataGrid.LoadDataAsync(people, config);

if (result.IsSuccess)
{
    logger.LogInfo("Loaded {Count} rows", people.Count());
}
```

#### **SearchAsync**
```csharp
public async Task<Result<SearchResult>> SearchAsync(
    string searchTerm, 
    SearchOptions? options = null)
```

**Purpose:** Vykonáva pokročilé vyhľadávanie v dátach DataGrid.

**Parameters:**
- `searchTerm` - Hľadaný text (podporuje regex ak je povolený)
- `options` - Nastavenia vyhľadávania

**Returns:** `Result<SearchResult>` - Výsledky vyhľadávania s detailmi

**Behavior:**
1. Validuje search term (nemôže byť null/empty)
2. Aplikuje search options (default ak null)
3. Pre regex: kompiluje pattern s error handling
4. Prehľadáva target columns alebo všetky stĺpce
5. Limituje výsledky podľa MaxResults
6. Meria performance (SearchDuration)
7. Zvýrazní nájdené výsledky v UI

**Usage:**
```csharp
var options = new SearchOptions
{
    UseRegex = true,
    CaseSensitive = false,
    TargetColumns = { "Name", "Email" }
};

var result = await dataGrid.SearchAsync("john.*@gmail", options);
if (result.IsSuccess)
{
    foreach (var match in result.Value.Matches)
    {
        Console.WriteLine($"Found at row {match.RowIndex}, column {match.ColumnName}");
    }
}
```

### **🚨 Exception Handling Rules**

#### **Common Exceptions:**
```csharp
// ArgumentNullException - keď sú povinné parametre null
if (data == null) 
    return Result<bool>.Failure("Data cannot be null");

// InvalidOperationException - keď nie je inicializovaný
if (!IsInitialized)
    return Result<bool>.Failure("DataGrid not initialized. Call LoadDataAsync first.");

// ArgumentException - neplatné konfiguračné hodnoty  
if (!configuration.Performance.IsValid())
    return Result<bool>.Failure("Invalid performance configuration");
```

#### **Error Recovery:**
1. **Graceful Degradation** - Pokračuje s default hodnotami
2. **Logging** - Všetky chyby sú logované
3. **User Notification** - Clear error messages
4. **Rollback** - Obnovenie predchádzajúceho stavu pri chybách

---

## 6️⃣ ADVANCEDWINUILOGGER - COMPLETE GUIDE

### **🎯 Overview**
AdvancedWinUiLogger je enterprise-grade logger komponent s dual-mode operáciou: UI komponent pre real-time viewing a headless file logger pre automation scripts.

### **📋 Core Features**
- ✅ **Dual Mode:** UI component + Headless file logging
- ✅ **File Rotation:** Automatická rotácia logov
- ✅ **Real-time UI:** Live log viewing v UI komponente
- ✅ **Microsoft.Extensions.Logging Integration** - Seamless integrácia
- ✅ **Color Theming:** Configurable log level colors
- ✅ **Performance Optimized:** Async operations, memory efficient
- ✅ **Enterprise Features:** Size limits, retention policies

### **🏗️ Class Structure**

#### **LoggerComponent (Main UI Component)**
```csharp
public sealed partial class LoggerComponent : UserControl
{
    // Factory Methods
    public static LoggerAPIComponent CreateHeadless(ILogger? logger = null)
    
    // Initialization
    public async Task<Result<bool>> InitializeAsync(
        LoggerUIConfiguration? configuration = null,
        ILogger? logger = null)
    
    // Log Management
    public async Task<Result<bool>> AddLogEntryAsync(LogEntry logEntry)
    public async Task<Result<bool>> ClearLogEntriesAsync()
    
    // Configuration
    public async Task<Result<bool>> UpdateConfigurationAsync(LoggerUIConfiguration newConfiguration)
    public LoggerUIConfiguration GetCurrentConfiguration()
    
    // Filtering
    public void SetMinimumLogLevel(LoggerLevel minimumLevel)
    
    // Properties
    public ObservableCollection<LogEntryViewModel> LogEntries { get; }
    public bool IsAutoScrollEnabled { get; set; }
    public LoggerLevel MinimumLevel { get; set; }
    public int TotalEntries { get; }
    public bool IsInitialized { get; }
}
```

#### **LoggerAPIComponent (Headless/File Operations)**
```csharp
public sealed class LoggerAPIComponent : IDisposable
{
    // Factory Methods
    public static LoggerAPIComponent CreateFileLogger(
        string logDirectory, 
        string baseFileName = "application", 
        int maxFileSizeMB = 10)
        
    public static LoggerAPIComponent CreateForUI(ILogger? logger = null)
    public static LoggerAPIComponent CreateHeadless(ILogger? logger = null)
    
    // Initialization
    public async Task<LoggerResult<bool>> InitializeAsync(LoggerConfiguration config)
    
    // File Operations
    public async Task<LoggerResult<bool>> SetLogDirectoryAsync(string directory)
    public async Task<LoggerResult<RotationResult>> RotateLogsAsync()
    public async Task<LoggerResult<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30)
    public async Task<LoggerResult<long>> GetCurrentLogSizeAsync()
    public async Task<LoggerResult<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync()
    
    // UI Operations (only in UI mode)
    public async Task<LoggerResult<bool>> RefreshUIAsync()
    public async Task<LoggerResult<bool>> ShowLogFileAsync(string filePath)
    
    // Properties
    public LoggerComponent? UIComponent { get; }
    public bool IsUIMode { get; }
    public bool IsInitialized { get; }
}
```

### **⚙️ Configuration Classes**

#### **LoggerUIConfiguration**
```csharp
public class LoggerUIConfiguration
{
    public LoggerColorConfiguration Colors { get; set; } = new();
    public LoggerPerformanceConfiguration Performance { get; set; } = new();
    
    // Display Settings
    public bool ShowTimestamp { get; set; } = true;
    public bool ShowLevel { get; set; } = true;
    public bool ShowSource { get; set; } = false;
    public bool ShowCategory { get; set; } = false;
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";
    
    // Factory Methods
    public static LoggerUIConfiguration CreateDefault() => new();
    public static LoggerUIConfiguration CreateDarkTheme() => // Dark preset
}
```

#### **LoggerColorConfiguration**
```csharp
public class LoggerColorConfiguration
{
    // Background & Base Colors
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string ForegroundColor { get; set; } = "#000000";
    
    // Log Level Colors
    public string TraceColor { get; set; } = "#607D8B";      // Blue Grey
    public string DebugColor { get; set; } = "#9E9E9E";      // Grey  
    public string InfoColor { get; set; } = "#2196F3";       // Blue
    public string WarningColor { get; set; } = "#FF9800";    // Orange
    public string ErrorColor { get; set; } = "#FF0000";      // Red
    public string CriticalColor { get; set; } = "#D32F2F";   // Dark Red
    
    // Theme
    public bool UseDarkTheme { get; set; } = false;
    
    // Helper Method
    public string GetColorForLevel(LoggerLevel level) => level switch
    {
        LoggerLevel.Trace => TraceColor,
        LoggerLevel.Debug => DebugColor,
        LoggerLevel.Information => InfoColor,
        LoggerLevel.Warning => WarningColor,
        LoggerLevel.Error => ErrorColor,
        LoggerLevel.Critical => CriticalColor,
        _ => ForegroundColor
    };
}
```

#### **LoggerPerformanceConfiguration**
```csharp
public class LoggerPerformanceConfiguration
{
    // Capacity Settings
    public int MaxLogEntries { get; set; } = 10000;
    
    // UI Performance
    public bool EnableVirtualization { get; set; } = true;
    public int VirtualizationThreshold { get; set; } = 1000;
    public bool EnableAutoScroll { get; set; } = true;
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    
    // Processing Settings
    public bool EnableAsyncProcessing { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    
    // Validation
    public bool IsValid() => 
        MaxLogEntries > 0 && 
        VirtualizationThreshold > 0 && 
        RefreshInterval > TimeSpan.Zero &&
        BatchSize > 0;
}
```

#### **LoggerConfiguration (File Logger)**
```csharp
public record LoggerConfiguration
{
    public required string LogDirectory { get; init; }
    public string BaseFileName { get; init; } = "application";
    public int MaxFileSizeMB { get; init; } = 10;
    public int MaxLogFiles { get; init; } = 10;
    public bool EnableAutoRotation { get; init; } = true;
    public bool EnableRealTimeViewing { get; init; } = false;
    public LogLevel MinLogLevel { get; init; } = LogLevel.Information;
}
```

### **📝 LogEntry System**

#### **LogEntry**
```csharp
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LoggerLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    
    // Constructors
    public LogEntry() { }
    public LogEntry(DateTime timestamp, LoggerLevel level, string message, string? exception = null)
    
    // Conversion Methods
    public static LoggerLevel FromLogLevel(LogLevel logLevel)
    public static LogLevel ToLogLevel(LoggerLevel loggerLevel)
}

public enum LoggerLevel
{
    Trace,      // Najdetailnejšie informácie
    Debug,      // Debug informácie
    Information, // Bežné informácie  
    Warning,    // Upozornenia
    Error,      // Chyby
    Critical    // Kritické chyby
}
```

#### **LogEntryViewModel (MVVM for UI)**
```csharp
public class LogEntryViewModel : INotifyPropertyChanged
{
    // Core Properties
    public DateTime Timestamp { get; }
    public LoggerLevel Level { get; }
    public string Message { get; }
    public string? Exception { get; }
    public string? Category { get; }
    public string? Source { get; }
    
    // Display Properties
    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
    public string LevelString => Level.ToString().ToUpperInvariant();
    public bool HasException => !string.IsNullOrEmpty(Exception);
    
    // UI Properties
    public SolidColorBrush TextBrush { get; }      // Color based on log level
    public SolidColorBrush BackgroundBrush { get; } // Background color
    public string DisplayMessage { get; }          // Message + exception if present
    
    // Configuration Update
    public void UpdateColorConfiguration(LoggerColorConfiguration newConfiguration)
}
```

### **🎨 Logger Color Priority Rules**

#### **Text Color Priority (Log Levels):**
1. **Critical:** `#D32F2F` (Dark Red) - System failures
2. **Error:** `#FF0000` (Red) - Application errors
3. **Warning:** `#FF9800` (Orange) - Warning conditions
4. **Information:** `#2196F3` (Blue) - General information
5. **Debug:** `#9E9E9E` (Grey) - Debug information
6. **Trace:** `#607D8B` (Blue Grey) - Detailed tracing

#### **Background Color Rules:**
- **Default:** `#FFFFFF` (White) / `#1E1E1E` (Dark mode)
- **Alternating Rows:** Automatic alternating background
- **Exception Indicator:** Red border pre záznamy s výnimkami

### **🔧 Methods - Detailed Documentation**

#### **InitializeAsync (UI Component)**
```csharp
public async Task<Result<bool>> InitializeAsync(
    LoggerUIConfiguration? configuration = null,
    ILogger? logger = null)
```

**Purpose:** Inicializuje Logger komponent s konfiguráciou.

**Parameters:**
- `configuration` - UI konfigurácia (použije default ak null)
- `logger` - Microsoft.Extensions.Logging logger pre internal logging

**Returns:** `Result<bool>` - Success ak sa inicializácia podarila

**Behavior:**
1. Validuje konfiguráciu (Performance.IsValid())
2. Aktualizuje ViewModel konfiguráciu
3. Inicializuje internal API s temporary directory
4. Nastaví real-time viewing mode
5. Označí komponent ako inicializovaný

**Usage:**
```csharp
var config = LoggerUIConfiguration.CreateDarkTheme();
var result = await loggerComponent.InitializeAsync(config, _logger);
if (result.IsSuccess)
{
    // Logger je pripravený na použitie
    MyContainer.Content = loggerComponent;
}
```

#### **AddLogEntryAsync**
```csharp
public async Task<Result<bool>> AddLogEntryAsync(LogEntry logEntry)
```

**Purpose:** Pridá nový log entry do UI komponenta.

**Parameters:**
- `logEntry` - Log záznam na pridanie

**Returns:** `Result<bool>` - Success ak sa záznam pridal

**Behavior:**
1. Validuje že komponent je inicializovaný
2. Validuje že logEntry nie je null
3. Kontroluje minimum log level filter
4. Pridá entry do ViewModel (UI update)
5. Enforcement MaxLogEntries limit (remove oldest)
6. Trigger auto-scroll ak je povolený

#### **CreateFileLogger (Static Factory)**
```csharp
public static LoggerAPIComponent CreateFileLogger(
    string logDirectory, 
    string baseFileName = "application", 
    int maxFileSizeMB = 10)
```

**Purpose:** Vytvára headless file logger pre automation scripts.

**Usage:**
```csharp
var fileLogger = LoggerAPIComponent.CreateFileLogger(
    logDirectory: @"C:\Logs\MyApp",
    baseFileName: "automation",  
    maxFileSizeMB: 50);
    
await fileLogger.InitializeAsync(new LoggerConfiguration 
{ 
    LogDirectory = @"C:\Logs\MyApp",
    BaseFileName = "automation",
    MaxFileSizeMB = 50
});

// Logger je pripravený pre headless operácie
await fileLogger.RotateLogsAsync();
var logFiles = await fileLogger.GetLogFilesAsync();
```

### **📁 File Management**

#### **Log File Naming Convention:**
```
{BaseFileName}_{DateTime}.log
automation_20241128_143052.log
application_20241128_143052.log
```

#### **Rotation Strategy:**
1. **Size-based:** Rotácia keď súbor dosiahne MaxFileSizeMB
2. **Time-based:** Denná rotácia o polnoci  
3. **Manual:** Cez `RotateLogsAsync()` metódu

#### **Cleanup Rules:**
- **Retention:** Uchováva posledných `MaxLogFiles` súborov
- **Age-based:** `CleanupOldLogsAsync(maxAgeInDays)` odstraňuje staré súbory
- **Size monitoring:** `GetCurrentLogSizeAsync()` pre monitoring

### **🚨 Exception Handling Rules**

#### **Initialization Errors:**
```csharp
// Neplatná konfigurácia
if (!configuration.Performance.IsValid())
    return Result<bool>.Failure("Invalid performance configuration provided");

// File system errors
catch (UnauthorizedAccessException)
    return Result<bool>.Failure("Access denied to log directory");
    
catch (DirectoryNotFoundException)  
    return Result<bool>.Failure("Log directory does not exist");
```

#### **Runtime Errors:**
```csharp
// Null reference protection
if (logEntry == null)
    return Result<bool>.Failure("LogEntry cannot be null");
    
// State validation
if (!_isInitialized)
    return Result<bool>.Failure("Logger component not initialized. Call InitializeAsync first.");
```

---

## 7️⃣ RESULT<T> MONADIC ERROR HANDLING

### **🎯 Overview**
Result<T> pattern poskytuje type-safe error handling bez exceptions pre business logic.

### **📋 Result<T> Class**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    
    // Factory Methods
    public static Result<T> Success(T value)
    public static Result<T> Failure(string errorMessage, Exception? exception = null)
    
    // Implicit Conversions
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator bool(Result<T> result) => result.IsSuccess;
}
```

### **🔧 Usage Patterns**

#### **Basic Usage:**
```csharp
public async Task<Result<Customer>> GetCustomerAsync(int id)
{
    if (id <= 0)
        return Result<Customer>.Failure("Invalid customer ID");
        
    try
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer != null 
            ? Result<Customer>.Success(customer)
            : Result<Customer>.Failure("Customer not found");
    }
    catch (Exception ex)
    {
        return Result<Customer>.Failure("Database error occurred", ex);
    }
}

// Usage
var result = await GetCustomerAsync(123);
if (result.IsSuccess)
{
    ProcessCustomer(result.Value);
}
else
{
    _logger.LogError("Failed: {Error}", result.ErrorMessage);
    ShowErrorMessage(result.ErrorMessage);
}
```

#### **Chaining Results:**
```csharp
var result = await dataGrid.LoadDataAsync(customers)
    .ContinueWith(async _ => await dataGrid.ValidateDataAsync())
    .ContinueWith(async _ => await dataGrid.ApplyFiltersAsync(filters));
```

---

## 8️⃣ USAGE EXAMPLES & TUTORIALS

### **🗃️ DataGrid Examples**

#### **Basic Setup:**
```csharp
public partial class MainWindow : Window
{
    private AdvancedDataGrid _dataGrid;
    
    public MainWindow()
    {
        InitializeComponent();
        _dataGrid = new AdvancedDataGrid();
        DataGridContainer.Content = _dataGrid;
    }
    
    private async void LoadData_Click(object sender, RoutedEventArgs e)
    {
        var customers = await GetCustomersAsync();
        var config = DataGridConfiguration.CreateDefault();
        
        var result = await _dataGrid.LoadDataAsync(customers, config);
        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
        }
    }
}
```

#### **Advanced Configuration:**
```csharp
private DataGridConfiguration CreateCustomConfiguration()
{
    return new DataGridConfiguration
    {
        Colors = new DataGridColorConfiguration
        {
            HeaderBackgroundColor = "#2C3E50",
            HeaderForegroundColor = "#FFFFFF",
            SelectedRowBackgroundColor = "#3498DB",
            ErrorBackgroundColor = "#E74C3C",
            UseDarkTheme = true
        },
        Performance = new DataGridPerformanceConfiguration
        {
            EnableVirtualization = true,
            VirtualizationThreshold = 500,
            MaxCachedRows = 5000,
            EnableAsyncRendering = true
        },
        Validation = new DataGridValidationConfiguration
        {
            EnableRealTimeValidation = true,
            ValidationMode = ValidationMode.RealTime
        }
    };
}
```

### **📝 Logger Examples**

#### **UI Logger Setup:**
```csharp
public partial class MainWindow : Window
{
    private LoggerComponent _logger;
    private ILogger<MainWindow> _msLogger;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Setup Microsoft.Extensions.Logging
        var factory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        _msLogger = factory.CreateLogger<MainWindow>();
        
        // Create Logger UI Component
        _logger = new LoggerComponent();
        LoggerContainer.Content = _logger;
        
        InitializeLoggerAsync();
    }
    
    private async void InitializeLoggerAsync()
    {
        var config = LoggerUIConfiguration.CreateDarkTheme();
        var result = await _logger.InitializeAsync(config, _msLogger);
        
        if (result.IsSuccess)
        {
            // Logger je pripravený
            await AddSampleLogs();
        }
    }
    
    private async Task AddSampleLogs()
    {
        var entries = new[]
        {
            new LogEntry(DateTime.Now, LoggerLevel.Information, "Application started"),
            new LogEntry(DateTime.Now, LoggerLevel.Debug, "Loading configuration"),  
            new LogEntry(DateTime.Now, LoggerLevel.Warning, "Config file not found, using defaults"),
            new LogEntry(DateTime.Now, LoggerLevel.Error, "Database connection failed", "ConnectionString invalid")
        };
        
        foreach (var entry in entries)
        {
            await _logger.AddLogEntryAsync(entry);
            await Task.Delay(100); // Simulate time between logs
        }
    }
}
```

#### **Headless File Logger:**
```csharp
public class AutomationScript
{
    private LoggerAPIComponent _fileLogger;
    
    public async Task RunAsync()
    {
        // Create file logger
        _fileLogger = LoggerAPIComponent.CreateFileLogger(
            logDirectory: @"C:\Automation\Logs",
            baseFileName: "automation-script",
            maxFileSizeMB: 25);
            
        var config = new LoggerConfiguration
        {
            LogDirectory = @"C:\Automation\Logs",
            BaseFileName = "automation-script",
            MaxFileSizeMB = 25,
            MaxLogFiles = 20,
            EnableAutoRotation = true,
            MinLogLevel = LogLevel.Information
        };
        
        var result = await _fileLogger.InitializeAsync(config);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Logger init failed: {result.Error}");
            return;
        }
        
        // Run automation with logging
        await ProcessDataWithLogging();
        
        // Cleanup old logs
        await _fileLogger.CleanupOldLogsAsync(maxAgeInDays: 7);
        
        _fileLogger.Dispose();
    }
    
    private async Task ProcessDataWithLogging()
    {
        // File logger automatically writes to log files
        // No UI updates needed for headless mode
        
        // Log files are rotated automatically when size limit reached
        var sizeResult = await _fileLogger.GetCurrentLogSizeAsync();
        if (sizeResult.IsSuccess)
        {
            Console.WriteLine($"Current log size: {sizeResult.Value} bytes");
        }
        
        // Manual rotation if needed
        if (sizeResult.Value > 20_000_000) // 20MB
        {
            var rotationResult = await _fileLogger.RotateLogsAsync();
            if (rotationResult.IsSuccess)
            {
                Console.WriteLine($"Log rotated: {rotationResult.Value.NewFileName}");
            }
        }
    }
}
```

---

## 9️⃣ DEMO APPLICATION GUIDE

### **🎯 Overview**
Demo aplikácia demonštruje všetky funkcie balíka RpaWinUiComponentsPackage v praktických scenároch.

### **📋 Demo Features**
- ✅ **DataGrid Demo:** Načítanie sample dát, search, filtering, validation
- ✅ **Logger Demo:** Real-time log viewing, level filtering, color themes  
- ✅ **Integration Demo:** Jak komponenty spolupracujú spolu
- ✅ **Performance Demo:** Testovanie s veľkými datasetmi
- ✅ **Configuration Demo:** Rôzne konfiguračné scenáre

### **🏗️ Demo App Structure**
```
📁 RpaWinUiComponents.Demo/
├── MainWindow.xaml.cs              # Main application window
├── Views/
│   ├── DataGridDemoView.xaml       # DataGrid demonstration
│   ├── LoggerDemoView.xaml         # Logger demonstration  
│   └── IntegrationDemoView.xaml    # Integration examples
├── ViewModels/                     # MVVM ViewModels
├── Models/                         # Sample data models
└── Services/                       # Demo services
```

### **🔧 Main Window Setup**
```csharp
public sealed partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly ILoggerFactory? _loggerFactory;
    
    // Components
    private AdvancedDataGrid? _dataGrid;
    private LoggerComponent? _loggerComponent;
    
    public MainWindow()
    {
        this.InitializeComponent();
        
        // IMPORTANT: Only using public API namespaces
        // ✅ using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
        // ✅ using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
        // ❌ NEVER: using RpaWinUiComponentsPackage.*.Internal
        
        SetupLogging();
        InitializeComponents();
    }
    
    private void SetupLogging()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug);
        });
        
        _logger = _loggerFactory.CreateLogger<MainWindow>();
        _logger.LogInformation("🚀 Demo Application Started");
    }
    
    private void InitializeComponents()
    {
        // Initialize DataGrid
        _dataGrid = new AdvancedDataGrid();
        DataGridContainer.Content = _dataGrid;
        
        // Initialize Logger UI  
        _loggerComponent = new LoggerComponent();
        LoggerContainer.Content = _loggerComponent;
        
        // Initialize both components
        _ = Task.Run(InitializeComponentsAsync);
    }
    
    private async Task InitializeComponentsAsync()
    {
        // Initialize Logger first
        var loggerConfig = LoggerUIConfiguration.CreateDarkTheme();
        var loggerResult = await _loggerComponent!.InitializeAsync(loggerConfig, _logger);
        
        if (loggerResult.IsSuccess)
        {
            _logger.LogInformation("✅ Logger Component Initialized");
            
            // Add sample log entries
            await AddSampleLogEntries();
        }
        
        // Initialize DataGrid
        var dataGridConfig = DataGridConfiguration.CreateDefault();
        var sampleData = GenerateSampleData(1000);
        var dataResult = await _dataGrid!.LoadDataAsync(sampleData, dataGridConfig);
        
        if (dataResult.IsSuccess)
        {
            _logger.LogInformation("✅ DataGrid Component Initialized with {Count} rows", sampleData.Count());
        }
    }
}
```

### **🎭 Sample Data Generation**
```csharp
public class PersonModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Department { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
}

private IEnumerable<PersonModel> GenerateSampleData(int count)
{
    var random = new Random();
    var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Lisa", "Tom", "Anna" };
    var lastNames = new[] { "Smith", "Johnson", "Brown", "Davis", "Wilson", "Miller", "Taylor", "Anderson" };
    var departments = new[] { "IT", "HR", "Finance", "Marketing", "Operations", "Sales" };
    
    return Enumerable.Range(1, count).Select(i => new PersonModel
    {
        Id = i,
        FirstName = firstNames[random.Next(firstNames.Length)],
        LastName = lastNames[random.Next(lastNames.Length)],
        Email = $"user{i}@company.com",
        BirthDate = DateTime.Now.AddYears(-random.Next(25, 65)),
        Department = departments[random.Next(departments.Length)],
        Salary = random.Next(30000, 150000),
        IsActive = random.Next(2) == 0
    });
}
```

### **🔍 Demo Scenarios**

#### **1. Search Demo**
```csharp
private async void SearchButton_Click(object sender, RoutedEventArgs e)
{
    if (_dataGrid == null) return;
    
    var searchOptions = new SearchOptions
    {
        UseRegex = RegexCheckBox.IsChecked == true,
        CaseSensitive = CaseCheckBox.IsChecked == true,
        SearchAllColumns = AllColumnsCheckBox.IsChecked == true,
        MaxResults = 500
    };
    
    if (!searchOptions.SearchAllColumns)
    {
        searchOptions.TargetColumns.AddRange(SelectedColumnsListBox.SelectedItems.Cast<string>());
    }
    
    var searchTerm = SearchTextBox.Text;
    var result = await _dataGrid.SearchAsync(searchTerm, searchOptions);
    
    if (result.IsSuccess)
    {
        SearchResultsText.Text = $"Found {result.Value.TotalMatches} matches in {result.Value.SearchDuration.TotalMilliseconds:F0}ms";
        
        // Log search results
        await _loggerComponent!.AddLogEntryAsync(new LogEntry(
            DateTime.Now,
            LoggerLevel.Information,
            $"Search completed: '{searchTerm}' found {result.Value.TotalMatches} matches"
        ));
    }
    else
    {
        SearchResultsText.Text = $"Search failed: {result.ErrorMessage}";
        
        await _loggerComponent!.AddLogEntryAsync(new LogEntry(
            DateTime.Now,
            LoggerLevel.Error,
            $"Search failed: {result.ErrorMessage}"
        ));
    }
}
```

#### **2. Filter Demo**
```csharp
private async void ApplyFilters_Click(object sender, RoutedEventArgs e)
{
    if (_dataGrid == null) return;
    
    var filters = new List<FilterExpression>();
    
    // Department filter
    if (DepartmentFilterBox.SelectedItem != null)
    {
        filters.Add(new FilterExpression
        {
            ColumnName = "Department",
            Operator = FilterOperator.Equals,
            Value = DepartmentFilterBox.SelectedItem.ToString()
        });
    }
    
    // Salary range filter  
    if (MinSalaryBox.Value.HasValue && MaxSalaryBox.Value.HasValue)
    {
        filters.Add(new FilterExpression
        {
            ColumnName = "Salary",
            Operator = FilterOperator.Between,
            Value = MinSalaryBox.Value.Value,
            Value2 = MaxSalaryBox.Value.Value
        });
    }
    
    // Active status filter
    if (ActiveOnlyCheckBox.IsChecked == true)
    {
        filters.Add(new FilterExpression
        {
            ColumnName = "IsActive", 
            Operator = FilterOperator.Equals,
            Value = true
        });
    }
    
    var result = await _dataGrid.ApplyFiltersAsync(filters);
    
    if (result.IsSuccess)
    {
        FilterStatusText.Text = $"Applied {filters.Count} filters";
        _logger.LogInformation("Applied {Count} filters to DataGrid", filters.Count);
    }
}
```

#### **3. Theme Switching Demo**
```csharp
private async void ThemeToggle_Click(object sender, RoutedEventArgs e)
{
    var isDarkMode = ThemeToggleButton.IsChecked == true;
    
    // Update DataGrid theme
    if (_dataGrid != null)
    {
        var dataGridConfig = isDarkMode 
            ? DataGridConfiguration.CreateDarkTheme()
            : DataGridConfiguration.CreateDefault();
            
        var result = _dataGrid.UpdateConfiguration(dataGridConfig);
        if (result.IsSuccess)
        {
            _logger.LogInformation("DataGrid theme updated to {Theme}", isDarkMode ? "Dark" : "Light");
        }
    }
    
    // Update Logger theme
    if (_loggerComponent != null)
    {
        var loggerConfig = isDarkMode
            ? LoggerUIConfiguration.CreateDarkTheme() 
            : LoggerUIConfiguration.CreateDefault();
            
        var result = await _loggerComponent.UpdateConfigurationAsync(loggerConfig);
        if (result.IsSuccess)
        {
            await _loggerComponent.AddLogEntryAsync(new LogEntry(
                DateTime.Now,
                LoggerLevel.Information,
                $"Theme switched to {(isDarkMode ? "Dark" : "Light")} mode"
            ));
        }
    }
}
```

---

## 🔟 BEST PRACTICES & RULES

### **🚀 Architecture Rules**

#### **✅ DO:**
- **Single Responsibility:** Jeden komponent = jedna zodpovědnost
- **Clean API:** Žiadna Internal namespace exposure pre užívateľov
- **Result Pattern:** Používaj Result<T> pre všetky async operácie
- **Configuration Objects:** Immutable konfiguračné objekty
- **Dependency Injection:** Constructor injection pre dependencies
- **Async/Await:** Pre všetky I/O operácie
- **Logging:** Microsoft.Extensions.Logging pre všetky komponenty
- **Error Handling:** Graceful degradation s clear error messages

#### **❌ DON'T:**
- **God Objects:** Obrovské klasy čo robia všetko
- **Internal Exposure:** Nikdy neukazuj Internal namespace užívateľom  
- **Synchronous I/O:** Blokujúce operácie v UI thread
- **Direct Exception Throwing:** Používaj Result<T> namiesto exceptions
- **Tight Coupling:** Komponenty nesmú závisieť jeden od druhého
- **Magic Numbers:** Používaj named constants
- **Mutable Configurations:** Konfigurácie majú byť immutable

### **🎨 Color Consistency Rules**

#### **Standard Color Palette:**
```csharp
// Light Theme  
public static class LightThemeColors
{
    public const string Primary = "#0078D4";           // Microsoft Blue
    public const string Secondary = "#106EBE";         // Darker Blue  
    public const string Success = "#107C10";           // Green
    public const string Warning = "#FF8C00";           // Orange
    public const string Error = "#D13438";             // Red
    public const string Info = "#0078D4";             // Blue
    public const string Background = "#FFFFFF";        // White
    public const string Surface = "#F8F8F8";          // Light Grey
    public const string OnBackground = "#323130";      // Dark Grey
}

// Dark Theme
public static class DarkThemeColors  
{
    public const string Primary = "#60CDFF";           // Light Blue
    public const string Secondary = "#409CFF";         // Medium Blue
    public const string Success = "#6CCB5F";           // Light Green  
    public const string Warning = "#FCE100";           // Yellow
    public const string Error = "#FF99A4";             // Light Red
    public const string Info = "#60CDFF";             // Light Blue
    public const string Background = "#1E1E1E";        // Dark Grey
    public const string Surface = "#2D2D30";          // Medium Grey
    public const string OnBackground = "#FFFFFF";      // White
}
```

### **⚡ Performance Rules**

#### **DataGrid Performance:**
1. **Virtualization:** Vždy zapnutá pre >1000 riadkov
2. **Async Operations:** Všetky data operations
3. **Batch Processing:** MaxBatchSize = 100-500 riadkov  
4. **Memory Limits:** MaxCachedRows = 10,000
5. **Search Limits:** MaxSearchResults = 1,000

#### **Logger Performance:**
1. **Entry Limits:** MaxLogEntries = 10,000
2. **File Size Limits:** MaxFileSizeMB = 10-50MB
3. **Async Writes:** EnableAsyncProcessing = true
4. **UI Updates:** RefreshInterval = 500ms

## 🔍 COMPREHENSIVE COMPONENT LOGGING SYSTEM

### **📊 Senior Developer Logging Implementation**

> **👨‍💻 20-Year Experience Approach**  
> Professional, detailed logging identical for Release and Debug builds  
> Complete execution flow tracking with error context and data visibility  

#### **🎯 Logging Architecture Overview**

The AdvancedWinUiDataGrid component implements **comprehensive enterprise-grade logging** throughout the entire component architecture:

- ✅ **Microsoft.Extensions.Logging.Abstractions** integration
- ✅ **Structured logging** with contextual prefixes for easy debugging
- ✅ **Complete operation tracking** - successful flows and error paths
- ✅ **Data context logging** - what data caused errors, what operations were attempted
- ✅ **Performance monitoring** with timing and service creation tracking
- ✅ **Fallback mechanisms** with detailed logging when errors occur

#### **🏗️ Logging Implementation Layers**

##### **1. Factory Layer Logging (`[FACTORY]` prefix)**
```csharp
// AdvancedWinUiDataGrid.CreateForUI() - Main entry point
logger?.LogInformation("[FACTORY] CreateForUI started - Creating DataGrid service for UI mode");
logger?.LogDebug("[FACTORY] Calling DataGridAPI.CreateForUI with logger: {HasLogger}", logger != null);
logger?.LogDebug("[FACTORY] DataGridAPI.CreateForUI returned service: {ServiceType}", service?.GetType()?.Name ?? "null");

// Comprehensive error handling with full exception context
catch (Exception ex)
{
    logger?.LogError(ex, "[FACTORY] CreateForUI failed - Exception in factory method: {ErrorMessage}", ex.Message);
    throw new InvalidOperationException("Failed to create DataGrid service for UI mode", ex);
}
```

##### **2. API Layer Logging (`[API]` prefix)**
```csharp
// DataGridAPI layer - service creation orchestration
logger?.LogInformation("[API] DataGridAPI.CreateForUI started - Creating UI-enabled DataGrid service");
logger?.LogDebug("[API] Calling DataGridServiceFactory.CreateWithUI with logger: {HasLogger}", logger != null);

// Null safety validation with detailed logging
if (service == null)
{
    logger?.LogError("[API] DataGridServiceFactory.CreateWithUI returned null service");
    throw new InvalidOperationException("DataGrid UI service factory returned null");
}
```

##### **3. Service Factory Logging (`[FACTORY]` and `[FACTORY-SUB]` prefixes)**
```csharp
// DataGridServiceFactory - Core service composition
logger?.LogInformation("[FACTORY] DataGridServiceFactory.CreateWithUI started - Creating UI-enabled service");
logger?.LogDebug("[FACTORY] Step 1: Creating DataGrid configuration for UI mode");

// Configuration creation with fallback logic
try 
{
    logger?.LogDebug("[FACTORY] Attempting to create DataGridConfiguration.ForUI with UIConfiguration.Default");
    configuration = DataGridConfiguration.ForUI(UIConfiguration.Default);
}
catch (Exception configEx)
{
    logger?.LogWarning(configEx, "[FACTORY] UIConfiguration.Default failed, creating fallback configuration");
    configuration = DataGridConfiguration.Default;
}

// Specialized service creation with detailed tracking
logger?.LogDebug("[FACTORY-SUB] Creating DataGridValidationService");
logger?.LogDebug("[FACTORY-SUB] DataGridValidationService created successfully");
```

##### **4. Component Initialization Logging (`[CONSTRUCTOR]`, `[INITIALIZE]` prefixes)**
```csharp
// Constructor logging
logger?.LogDebug("[CONSTRUCTOR] AdvancedWinUiDataGrid constructor started with service: {ServiceType}", service?.GetType()?.Name ?? "null");
logger?.LogInformation("[CONSTRUCTOR] AdvancedWinUiDataGrid instance created successfully with service: {ServiceType}", _service.GetType().Name);

// Initialization with parameter validation and detailed tracking
logger?.LogInformation("[INITIALIZE] InitializeAsync started with {ColumnCount} columns, minimumRows: {MinRows}", columns?.Count ?? 0, minimumRows);

// Input validation with detailed parameter logging
for (int i = 0; i < columns.Count; i++)
{
    var column = columns[i];
    logger?.LogDebug("[INITIALIZE] Column[{Index}]: Name='{Name}', Type='{DataType}', Required={IsRequired}, ReadOnly={IsReadOnly}", 
        i, column?.Name ?? "null", column?.DataType?.Name ?? "null", column?.IsRequired ?? false, column?.IsReadOnly ?? false);
}
```

#### **🔧 Logging Configuration Best Practices**

##### **Production-Ready Setup**
```csharp
// Identical logging for Release and Debug builds
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "HH:mm:ss.fff ";
        })
        .AddFile("logs/datagrid-{Date}.log") // For production file logging
        .SetMinimumLevel(LogLevel.Debug); // Capture all our detailed logs
});

var logger = loggerFactory.CreateLogger<MyApplication>();

// Create DataGrid with comprehensive logging
var dataGrid = AdvancedWinUiDataGrid.CreateForUI(logger);
```

#### **📊 Logging Output Example**

When initializing a DataGrid, you'll see comprehensive log output like:
```
14:30:25.123 [INFO] [FACTORY] CreateForUI started - Creating DataGrid service for UI mode
14:30:25.125 [DEBUG] [FACTORY] Calling DataGridAPI.CreateForUI with logger: True
14:30:25.126 [INFO] [API] DataGridAPI.CreateForUI started - Creating UI-enabled DataGrid service  
14:30:25.127 [DEBUG] [API] Calling DataGridServiceFactory.CreateWithUI with logger: True
14:30:25.128 [INFO] [FACTORY] DataGridServiceFactory.CreateWithUI started - Creating UI-enabled service
14:30:25.129 [DEBUG] [FACTORY] Step 1: Creating DataGrid configuration for UI mode
14:30:25.130 [DEBUG] [FACTORY] DataGridConfiguration.ForUI created successfully
14:30:25.131 [DEBUG] [FACTORY] Step 2: Creating specialized services
14:30:25.132 [DEBUG] [FACTORY-SUB] Creating DataGridValidationService
14:30:25.133 [DEBUG] [FACTORY-SUB] DataGridValidationService created successfully
14:30:25.134 [DEBUG] [CONSTRUCTOR] AdvancedWinUiDataGrid constructor started with service: DataGridUnifiedService
14:30:25.135 [INFO] [CONSTRUCTOR] AdvancedWinUiDataGrid instance created successfully with service: DataGridUnifiedService
```

#### **🚨 Error Scenario Logging**

When errors occur, comprehensive context is provided:
```
14:30:25.140 [WARN] [FACTORY] UIConfiguration.Default failed, creating fallback configuration
14:30:25.141 [DEBUG] [FACTORY] Created fallback DataGridConfiguration using Default settings
14:30:25.142 [ERROR] [FACTORY-SUB] Failed to create DataGridValidationService: Service dependency missing
14:30:25.143 [ERROR] [FACTORY] DataGridServiceFactory.CreateWithUI failed - Exception during service creation: Service dependency missing
Stack Trace: [Complete stack trace with file:line information]
```

#### **📈 Benefits for Debugging and Production**

1. **Development Benefits:**
   - **Complete Flow Visibility:** See exactly which methods are called and in what order
   - **Data Context:** Know what data caused failures (column definitions, configuration values)
   - **Performance Tracking:** Identify slow service creation or initialization steps
   - **Fallback Monitoring:** Track when fallback logic is triggered

2. **Production Benefits:**
   - **Issue Diagnosis:** Quickly identify where initialization failures occur
   - **Performance Monitoring:** Track service creation times and identify bottlenecks
   - **Reliability Monitoring:** Monitor fallback usage and error rates
   - **User Support:** Provide detailed error context for support tickets

3. **Senior Developer Quality:**
   - **Consistent Prefixes:** Easy filtering and searching in log aggregators
   - **Structured Parameters:** Searchable, aggregatable log data
   - **Exception Context:** Complete error information with stack traces
   - **Graceful Degradation:** Clear logging when fallback mechanisms activate
5. **Memory Management:** Auto-cleanup starých entries

### **🔒 Validation Rules**

#### **Input Validation:**
```csharp
// Always validate public API inputs
public async Task<Result<bool>> LoadDataAsync<T>(IEnumerable<T> data, DataGridConfiguration? configuration = null)
{
    // Null validation
    if (data == null)
        return Result<bool>.Failure("Data cannot be null");
    
    // Configuration validation  
    configuration ??= DataGridConfiguration.CreateDefault();
    if (!configuration.Performance.IsValid())
        return Result<bool>.Failure("Invalid performance configuration");
    
    // State validation
    if (!IsInitialized)
        return Result<bool>.Failure("Component not initialized. Call InitializeAsync first.");
        
    // Proceed with operation...
}
```

#### **Configuration Validation:**
```csharp
public bool IsValid()
{
    return MaxLogEntries > 0 && 
           VirtualizationThreshold > 0 && 
           RefreshInterval > TimeSpan.Zero &&
           BatchSize > 0 &&
           MaxCachedRows > 0;
}
```

### **📝 Logging Rules**

#### **Log Levels Usage:**
- **Trace:** Velmi detailné informácie (entry/exit methods)
- **Debug:** Debug informácie pre developers
- **Information:** Bežné application events (startup, shutdown, major operations)
- **Warning:** Neočakávané situácie ktoré nie sú chyby
- **Error:** Chyby ktoré neprerušujú aplikáciu  
- **Critical:** Serious errors ktoré môžu prerušiť aplikáciu

#### **Logging Format:**
```csharp
// ✅ Good logging
_logger.LogInformation("🚀 DataGrid initialized with {RowCount} rows in {Duration}ms", rowCount, duration);
_logger.LogError("❌ Failed to load data from {Source}: {Error}", source, error);
_logger.LogWarning("⚠️ Performance threshold exceeded: {ActualTime}ms > {ThresholdTime}ms", actual, threshold);

// ❌ Bad logging
_logger.LogInformation("DataGrid loaded");
_logger.LogError("Error occurred");
```

### **🛡️ Security Rules**

#### **Data Protection:**
- **No Sensitive Data:** Nikdy neloguj sensitive data (passwords, PII)
- **Configuration Security:** Secure storage pre connection strings
- **File Permissions:** Correct permissions na log files
- **Memory Cleanup:** Proper disposal of sensitive objects

#### **Input Sanitization:**
```csharp
// Sanitize search terms for regex
private static string SanitizeRegexInput(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        return string.Empty;
        
    // Escape special regex characters if not using regex mode
    return Regex.Escape(input.Trim());
}
```

### **🔧 Testing Rules**

#### **Unit Testing:**
- **Public API Only:** Test len public API methods
- **Result Pattern:** Test both success and failure scenarios
- **Configuration:** Test všetky konfiguračné kombinácie
- **Async Operations:** Proper async testing patterns

#### **Integration Testing:**
- **Component Integration:** Test ako komponenty spolupracujú
- **Performance Testing:** Load testing s veľkými datasetmi  
- **UI Testing:** User interaction scenarios
- **Error Recovery:** Error handling a recovery scenarios

### **📚 Documentation Rules**

#### **Code Documentation:**
```csharp
/// <summary>
/// PROFESSIONAL: Loads data into DataGrid with optional configuration
/// ENTERPRISE: Supports virtualization for large datasets (10M+ rows)
/// </summary>
/// <typeparam name="T">Data type - must be serializable</typeparam>
/// <param name="data">Data collection to load - cannot be null</param>
/// <param name="configuration">Optional configuration - uses default if null</param>
/// <returns>Result indicating success/failure with error details</returns>
/// <exception cref="ArgumentNullException">Never thrown - uses Result pattern</exception>
/// <example>
/// var people = GetPeople();
/// var config = DataGridConfiguration.CreateDefault();
/// var result = await dataGrid.LoadDataAsync(people, config);
/// if (result.IsSuccess) { /* Success */ }
/// </example>
public async Task<Result<bool>> LoadDataAsync<T>(IEnumerable<T> data, DataGridConfiguration? configuration = null)
```

#### **README Documentation:**
- **Quick Start:** Jednoduchý example na začiatku
- **API Reference:** Kompletná API dokumentácia  
- **Examples:** Praktické použitie scenarios
- **Troubleshooting:** Časté problémy a riešenia
- **Migration Guide:** Upgrading medzi verziami

---

## **🎯 SÚHRN PROFESIONÁLNYCH PRAVIDIEL**

### **✅ CLEAN API SUCCESS CRITERIA:**
1. **Single Using:** `using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;`
2. **No Internal Exposure:** Internal namespace никdy nie je viditeľný
3. **Result Pattern:** Všetky operations vracia Result<T>
4. **Configuration Objects:** Immutable, validatable configurations
5. **Performance:** Sub-second response times pre 10M+ rows
6. **Error Handling:** Graceful degradation s clear messages
7. **Logging Integration:** Seamless Microsoft.Extensions.Logging
8. **Theme Support:** Light/Dark themes s consistent colors
9. **Async Operations:** Non-blocking UI operations
10. **Memory Efficient:** Proper disposal a memory management

### **🚀 ENTERPRISE READINESS:**
- **Production Tested:** Stable, reliable, maintainable
- **Scalable Architecture:** Modular, extensible design
- **Security Compliant:** GDPR compliant logging, secure by design
- **Performance Optimized:** Virtualization, async processing, memory management
- **Developer Friendly:** Clean API, comprehensive documentation, rich examples
- **Support Ready:** Clear error messages, comprehensive logging, troubleshooting guides

---

**📝 Táto dokumentácia pokrýva kompletné API, všetky metódy, konfiguračné možnosti, pravidlá priority farieb, validačné pravidlá, výnimky a pravidlá pre oba komponenty (package + demo aplikáciu) s ľudsky čitateľnými vysvetleniami ako komponenty fungujú keď užívatelia volajú metódy, píšu kód alebo klikajú na veci.**

**🎯 Verzia dokumentácie: 3.0.0 - Professional Clean API Architecture**  
**📅 Posledná aktualizácia: 2024-11-28**  
**👨‍💻 Autor: Professional .NET 8.0 + WinUI3 Development Team**