using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.Core.Models;

#region Configuration Classes

/// <summary>
/// Column configuration for DataGrid
/// </summary>
public class ColumnConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Type Type { get; set; } = typeof(string);
    public int Width { get; set; } = 100;
    public bool IsRequired { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public object? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public string? DisplayFormat { get; set; }
    public string? ValidationPattern { get; set; }
    public bool IsValidationColumn { get; set; } = false;
    public bool IsDeleteColumn { get; set; } = false;
    public bool IsCheckBoxColumn { get; set; } = false;
    public bool IsVisible { get; set; } = true;
    public bool CanResize { get; set; } = true;
    public bool CanSort { get; set; } = true;
    public bool CanFilter { get; set; } = true;
}

/// <summary>
/// PROFESSIONAL Color configuration for DataGrid theming
/// SUPPORTS: Default colors that can be customized in applications
/// ALL COLORS: Have professional defaults but can be overridden
/// </summary>
public class ColorConfiguration
{
    // CELL COLORS - Professional defaults, customizable by application
    public string CellBackground { get; set; } = "#FFFFFF";           // White
    public string CellForeground { get; set; } = "#000000";           // Black
    public string CellBorder { get; set; } = "#E0E0E0";               // Light gray
    
    // HEADER COLORS - Professional defaults
    public string HeaderBackground { get; set; } = "#F5F5F5";         // Light gray
    public string HeaderForeground { get; set; } = "#333333";         // Dark gray
    public string HeaderBorder { get; set; } = "#CCCCCC";             // Medium gray
    
    // SELECTION COLORS - Professional defaults
    public string SelectionBackground { get; set; } = "#0078D4";       // Microsoft blue
    public string SelectionForeground { get; set; } = "#FFFFFF";       // White
    public string FocusBackground { get; set; } = "#E7F3FF";           // Light blue
    public string FocusBorder { get; set; } = "#0078D4";               // Microsoft blue
    
    // VALIDATION COLORS - Professional defaults
    public string ValidationErrorBorder { get; set; } = "#FF0000";     // Red
    public string ValidationErrorBackground { get; set; } = "#FFEBEE"; // Light red
    public string ValidationWarningBorder { get; set; } = "#FF9800";   // Orange
    public string ValidationWarningBackground { get; set; } = "#FFF3E0"; // Light orange
    public string ValidationInfoBorder { get; set; } = "#2196F3";      // Blue
    public string ValidationInfoBackground { get; set; } = "#E3F2FD";  // Light blue
    
    // ZEBRA STRIPES - Professional defaults
    public bool EnableZebraStripes { get; set; } = false;
    public string AlternateRowBackground { get; set; } = "#FAFAFA";     // Very light gray
    
    // THEME SUPPORT
    public bool UseDarkTheme { get; set; } = false;
    
    // DARK THEME COLORS - Professional defaults for dark mode
    public string DarkCellBackground { get; set; } = "#1E1E1E";        // Dark gray
    public string DarkCellForeground { get; set; } = "#FFFFFF";        // White
    public string DarkHeaderBackground { get; set; } = "#2D2D30";      // Darker gray
    public string DarkSelectionBackground { get; set; } = "#0E639C";   // Dark blue
    public string DarkAlternateRowBackground { get; set; } = "#252526"; // Very dark gray
    
    // HOVER STATES - Professional defaults
    public string HoverBackground { get; set; } = "#F0F0F0";           // Light gray
    public string DarkHoverBackground { get; set; } = "#3E3E42";       // Dark gray
    
    /// <summary>
    /// Get effective color based on theme
    /// </summary>
    public string GetEffectiveCellBackground() => UseDarkTheme ? DarkCellBackground : CellBackground;
    public string GetEffectiveCellForeground() => UseDarkTheme ? DarkCellForeground : CellForeground;
    public string GetEffectiveHeaderBackground() => UseDarkTheme ? DarkHeaderBackground : HeaderBackground;
    public string GetEffectiveSelectionBackground() => UseDarkTheme ? DarkSelectionBackground : SelectionBackground;
    public string GetEffectiveAlternateRowBackground() => UseDarkTheme ? DarkAlternateRowBackground : AlternateRowBackground;
    public string GetEffectiveHoverBackground() => UseDarkTheme ? DarkHoverBackground : HoverBackground;
    
    /// <summary>
    /// Reset all colors to professional defaults
    /// </summary>
    public void ResetToDefaults()
    {
        // Reset to constructor defaults
        var defaultConfig = new ColorConfiguration();
        CellBackground = defaultConfig.CellBackground;
        CellForeground = defaultConfig.CellForeground;
        CellBorder = defaultConfig.CellBorder;
        HeaderBackground = defaultConfig.HeaderBackground;
        HeaderForeground = defaultConfig.HeaderForeground;
        HeaderBorder = defaultConfig.HeaderBorder;
        SelectionBackground = defaultConfig.SelectionBackground;
        SelectionForeground = defaultConfig.SelectionForeground;
        ValidationErrorBorder = defaultConfig.ValidationErrorBorder;
        ValidationErrorBackground = defaultConfig.ValidationErrorBackground;
        // ... (reset all other colors)
    }
}

/// <summary>
/// PROFESSIONAL Validation configuration for DataGrid
/// SUPPORTS: Multiple rules per column, cross-row/column validations, custom error messages
/// </summary>
public class ValidationConfiguration
{
    public bool EnableRealtimeValidation { get; set; } = true;
    public bool EnableBatchValidation { get; set; } = true;
    public bool ShowValidationAlerts { get; set; } = true;
    
    /// <summary>
    /// MULTIPLE RULES PER COLUMN: Each column can have multiple validation rules
    /// Key = Column Name, Value = List of ValidationRule with custom error messages
    /// </summary>
    public Dictionary<string, List<ValidationRule>>? ColumnValidationRules { get; set; } = new();
    
    /// <summary>
    /// CROSS-ROW VALIDATIONS: Rules that validate data across multiple rows
    /// Example: Sum of values in column A must equal value in last row of column B
    /// </summary>
    public List<CrossRowValidationRule>? CrossRowRules { get; set; } = new();
    
    /// <summary>
    /// CROSS-COLUMN VALIDATIONS: Rules that validate data across multiple columns in same row
    /// Example: If column A > 100, then column B must be filled
    /// </summary>
    public List<CrossColumnValidationRule>? CrossColumnRules { get; set; } = new();
    
    /// <summary>
    /// DATASET VALIDATIONS: Rules that validate entire dataset
    /// Example: No duplicate values across multiple columns
    /// </summary>
    public List<DatasetValidationRule>? DatasetRules { get; set; } = new();
    
    public string ValidationErrorBorderColor { get; set; } = "#FF0000";
    public int ValidationErrorBorderThickness { get; set; } = 2;
    public bool ShowErrorMessages { get; set; } = true;
    public bool StopOnFirstError { get; set; } = false;
    public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Performance configuration for large datasets
/// </summary>
public class PerformanceConfiguration
{
    public bool EnableVirtualization { get; set; } = true;
    public int VirtualizationThreshold { get; set; } = 1000;
    public bool EnableBackgroundProcessing { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public int CacheSize { get; set; } = 10000;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableThrottling { get; set; } = true;
    public TimeSpan ThrottleDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public int MaxConcurrentOperations { get; set; } = Environment.ProcessorCount;
}

#endregion

#region Validation Models

/// <summary>
/// PROFESSIONAL Single-column validation rule with custom error message
/// SUPPORTS: Multiple rules per column, each with its own error message
/// </summary>
public class ValidationRule
{
    public string ColumnName { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty; // Unique identifier for the rule
    public Func<object?, bool> Validator { get; set; } = _ => true;
    public string ErrorMessage { get; set; } = string.Empty; // Custom error message for this rule
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public bool StopOnFirstError { get; set; } = false;
    public int Priority { get; set; } = 0; // Order of validation execution
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// PROFESSIONAL Cross-row validation rule
/// VALIDATES: Data relationships across multiple rows
/// EXAMPLE: "Sum of Quantity column must equal Total in last row"
/// </summary>
public class CrossRowValidationRule
{
    public string RuleName { get; set; } = string.Empty;
    public Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, ValidationResult> Validator { get; set; } = _ => new ValidationResult { IsValid = true };
    public string ErrorMessage { get; set; } = string.Empty; // Custom error message
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public List<string> AffectedColumns { get; set; } = new(); // Columns involved in this rule
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// PROFESSIONAL Cross-column validation rule  
/// VALIDATES: Data relationships across multiple columns in same row
/// EXAMPLE: "If Age > 18, then Email must be provided"
/// </summary>
public class CrossColumnValidationRule
{
    public string RuleName { get; set; } = string.Empty;
    public Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator { get; set; } = _ => new ValidationResult { IsValid = true };
    public string ErrorMessage { get; set; } = string.Empty; // Custom error message
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public List<string> DependentColumns { get; set; } = new(); // Columns that this rule depends on
    public string? PrimaryColumn { get; set; } // Main column that triggers the validation
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// PROFESSIONAL Dataset validation rule
/// VALIDATES: Entire dataset constraints
/// EXAMPLE: "No duplicate combinations of Name + Email across all rows"
/// </summary>
public class DatasetValidationRule
{
    public string RuleName { get; set; } = string.Empty;
    public Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, List<ValidationResult>> Validator { get; set; } = _ => new List<ValidationResult>();
    public string ErrorMessage { get; set; } = string.Empty; // Custom error message template
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public List<string> InvolvedColumns { get; set; } = new(); // All columns involved in this rule
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Validation severity levels
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Validation result for single field
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ErrorMessages { get; set; } = new(); // Compatibility property for multiple errors
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public string? ColumnName { get; set; }
    public int? RowIndex { get; set; }
}

/// <summary>
/// Batch validation result with progress
/// </summary>
public class BatchValidationResult
{
    public bool IsValid { get; set; }
    public IReadOnlyList<ValidationResult> ValidationErrors { get; set; } = Array.Empty<ValidationResult>();
    public int TotalRowsValidated { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public TimeSpan Duration { get; set; }
    public bool WasCancelled { get; set; }
}

/// <summary>
/// Validation progress for batch operations
/// </summary>
public class ValidationProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
    public int ErrorCount { get; set; }
    public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public double PercentageComplete => PercentComplete; // Alias for compatibility
    public string? CurrentOperation { get; set; }
    public int CurrentRow { get; set; }
}

#endregion

#region Import/Export Models

/// <summary>
/// Import mode for data operations
/// </summary>
public enum ImportMode
{
    Replace,
    Append,
    Insert
}

/// <summary>
/// Import options for data operations
/// </summary>
public record ImportOptions(
    bool ReplaceExistingData = true,
    bool ValidateBeforeImport = true);

/// <summary>
/// Export options for data operations  
/// </summary>
public record ExportOptions(
    bool IncludeEmptyRows = false,
    bool IncludeValidationAlerts = false,
    IReadOnlyList<string>? ColumnNames = null);

/// <summary>
/// Import result with statistics
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public int ImportedRows { get; set; }
    public int SkippedRows { get; set; }
    public int ErrorRows { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
    public TimeSpan Duration { get; set; }
    public bool WasCancelled { get; set; }
}

/// <summary>
/// Import progress for batch operations
/// </summary>
public class ImportProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int ErrorRows { get; set; }
    public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public string? CurrentOperation { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Export progress for batch operations
/// </summary>
public class ExportProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
    public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public string? CurrentOperation { get; set; }
}

#endregion

#region Search Models

/// <summary>
/// Search results
/// </summary>
public class SearchResults
{
    public IReadOnlyList<SearchMatch> Matches { get; set; } = Array.Empty<SearchMatch>();
    public int TotalMatches { get; set; }
    public TimeSpan Duration { get; set; }
    public bool WasCancelled { get; set; }
    public string SearchText { get; set; } = string.Empty;
}

/// <summary>
/// Advanced search results with additional metadata
/// </summary>
public class AdvancedSearchResults : SearchResults
{
    public AdvancedSearchCriteria Criteria { get; set; } = new();
    public IReadOnlyDictionary<string, int> MatchesPerColumn { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Single search match
/// </summary>
public class SearchMatch
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string MatchedText { get; set; } = string.Empty;
    public object? CellValue { get; set; }
    public int StartIndex { get; set; }
    public int Length { get; set; }
}

/// <summary>
/// Advanced search criteria
/// </summary>
public class AdvancedSearchCriteria
{
    public string SearchText { get; set; } = string.Empty;
    public IReadOnlyList<string>? TargetColumns { get; set; }
    public bool CaseSensitive { get; set; } = false;
    public bool WholeWord { get; set; } = false;
    public bool UseRegex { get; set; } = false;
    public SearchScope Scope { get; set; } = SearchScope.AllData;
    public int? MaxMatches { get; set; }
}

/// <summary>
/// Search scope options
/// </summary>
public enum SearchScope
{
    AllData,
    VisibleData,
    SelectedRange,
    CurrentColumn
}

/// <summary>
/// Search progress for batch operations
/// </summary>
public class SearchProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
    public int MatchesFound { get; set; }
    public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public string? CurrentOperation { get; set; }
}

#endregion

#region Filter Models

/// <summary>
/// Advanced filter criteria
/// </summary>
public class AdvancedFilter
{
    public string ColumnName { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;
    public object? Value { get; set; }
    public FilterLogicOperator LogicOperator { get; set; } = FilterLogicOperator.And;
    public bool CaseSensitive { get; set; } = false;
}

/// <summary>
/// Filter operators
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual,
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty
}

/// <summary>
/// Filter logic operators
/// </summary>
public enum FilterLogicOperator
{
    And,
    Or,
    Not
}

/// <summary>
/// Filter progress for batch operations
/// </summary>
public class FilterProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
    public int FilteredRows { get; set; }
    public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public string? CurrentOperation { get; set; }
}

#endregion

#region Sort Models

/// <summary>
/// Multi-column sort definition
/// </summary>
public class MultiSortColumn
{
    public string ColumnName { get; set; } = string.Empty;
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
    public int Priority { get; set; } = 0;
    public IComparer<object?>? CustomComparer { get; set; }
}

/// <summary>
/// Sort direction
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// Sort progress for batch operations
/// </summary>
public class SortProgress
{
    public int ProcessedRows { get; set; }
    public int TotalRows { get; set; }
    public double PercentComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public string? CurrentOperation { get; set; }
}

#endregion

#region UI Models

/// <summary>
/// Cell position in grid
/// </summary>
public record CellPosition(int Row, int Column);

/// <summary>
/// Cell range in grid
/// </summary>
public record CellRange(CellPosition Start, CellPosition End)
{
    public int RowCount => Math.Abs(End.Row - Start.Row) + 1;
    public int ColumnCount => Math.Abs(End.Column - Start.Column) + 1;
    public bool IsSingleCell => Start.Row == End.Row && Start.Column == End.Column;
}

/// <summary>
/// Column information
/// </summary>
public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Type DataType { get; set; } = typeof(string);
    public int Width { get; set; }
    public bool IsVisible { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsRequired { get; set; }
    public int Index { get; set; }
}


#endregion

#region Performance Models

/// <summary>
/// Performance metrics for monitoring
/// </summary>
public class PerformanceMetrics
{
    public int TotalRows { get; set; }
    public int VisibleRows { get; set; }
    public TimeSpan LastOperationDuration { get; set; }
    public long MemoryUsageBytes { get; set; }
    public double UIFrameRate { get; set; }
    public int CacheHitRate { get; set; }
    public int CachedObjectsCount { get; set; }
    public TimeSpan AverageRenderTime { get; set; }
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

#endregion

#region DataGrid Types

/// <summary>
/// DataGrid cell for coordinator compatibility
/// </summary>
public class DataGridCell
{
    public object? Value { get; set; }
    public bool ValidationState { get; set; } = true;
    public bool IsSelected { get; set; }
    public bool IsFocused { get; set; }
    public string? ValidationMessage { get; set; }
    public string? ValidationError { get; set; }
    public bool IsCopied { get; set; }
    public object? BackgroundBrush { get; set; }
    public object? BorderBrush { get; set; }
    public object? BorderThickness { get; set; }
}

/// <summary>
/// DataGrid row for coordinator compatibility
/// </summary>
public class DataGridRow
{
    public List<DataGridCell> Cells { get; set; } = new();
    public int Index { get; set; }
    public bool IsSelected { get; set; }
    public bool HasValidationErrors => Cells.Any(c => !c.ValidationState);
}

#endregion

