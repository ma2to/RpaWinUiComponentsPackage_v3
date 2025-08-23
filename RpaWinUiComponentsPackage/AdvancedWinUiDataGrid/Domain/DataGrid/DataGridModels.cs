namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;

/// <summary>
/// Validation state for data rows
/// FUNCTIONAL: Immutable enumeration
/// </summary>
internal enum ValidationState
{
    Valid,
    Invalid,
    Pending,
    Warning
}

/// <summary>
/// Immutable column definition
/// FUNCTIONAL: Value object with validation
/// </summary>
internal record ColumnDefinition(
    string Name,
    Type DataType,
    bool IsRequired = false,
    bool IsReadOnly = false,
    object? DefaultValue = null,
    int? MaxLength = null,
    string? DisplayFormat = null,
    string? ValidationPattern = null,
    // EXTENDED PROPERTIES for Clean API support
    string? DisplayName = null,
    double Width = 100,
    bool IsValidationColumn = false,
    bool IsDeleteColumn = false)
{
    public static ColumnDefinition String(string name, bool required = false, int? maxLength = null) =>
        new(name, typeof(string), required, false, null, maxLength);
        
    public static ColumnDefinition Integer(string name, bool required = false) =>
        new(name, typeof(int), required);
        
    public static ColumnDefinition Decimal(string name, bool required = false, string? format = null) =>
        new(name, typeof(decimal), required, false, null, null, format);
        
    public static ColumnDefinition DateTime(string name, bool required = false, string? format = null) =>
        new(name, typeof(DateTime), required, false, null, null, format ?? "yyyy-MM-dd");
        
    public static ColumnDefinition Boolean(string name) =>
        new(name, typeof(bool));
}

/// <summary>
/// Immutable data row
/// FUNCTIONAL: Immutable dictionary wrapper with type safety
/// </summary>
internal record DataRow(IReadOnlyDictionary<string, object?> Data, int RowIndex = -1)
{
    public T? GetValue<T>(string columnName) =>
        Data.TryGetValue(columnName, out var value) && value is T typedValue ? typedValue : default;
        
    public bool HasColumn(string columnName) => Data.ContainsKey(columnName);
    
    public DataRow WithValue(string columnName, object? value) =>
        this with { Data = Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .Where(kvp => kvp.Key != columnName)
            .Append(new KeyValuePair<string, object?>(columnName, value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) };
            
    public DataRow WithRowIndex(int rowIndex) => this with { RowIndex = rowIndex };
}

/// <summary>
/// Basic DataGrid configuration
/// FUNCTIONAL: Immutable configuration object (base configuration, extended in AdvancedDataGridModels)
/// </summary>
internal record BasicDataGridConfiguration(
    bool EnableValidation = true,
    bool EnableVirtualization = true,
    int BatchSize = 1000,
    TimeSpan ThrottleDelay = default,
    bool CacheEnabled = true)
{
    public static BasicDataGridConfiguration Default => new();
    public static BasicDataGridConfiguration HighPerformance => new(true, true, 5000, TimeSpan.FromMilliseconds(50), true);
    public static BasicDataGridConfiguration MinimalValidation => new(false, true, 10000, TimeSpan.Zero, false);
}

/// <summary>
/// Basic import options
/// FUNCTIONAL: Immutable import configuration (base configuration, extended in AdvancedDataGridModels)
/// </summary>
internal record BasicImportOptions(
    bool ValidateData = true,
    bool SkipInvalidRows = false,
    IProgress<ImportProgress>? ProgressReporter = null,
    CancellationToken CancellationToken = default)
{
    public static BasicImportOptions Default => new();
    public static BasicImportOptions QuickImport => new(ValidateData: false);
    public static BasicImportOptions SafeImport => new(ValidateData: true, SkipInvalidRows: true);
}

/// <summary>
/// Basic export options
/// FUNCTIONAL: Immutable export configuration (base configuration, extended in AdvancedDataGridModels)
/// </summary>
internal record BasicExportOptions(
    bool IncludeHeaders = true,
    bool IncludeValidationAlerts = false,
    bool OnlyValidRows = false,
    bool OnlyVisibleRows = false)
{
    public static BasicExportOptions Default => new();
    public static BasicExportOptions ValidOnly => new(OnlyValidRows: true);
    public static BasicExportOptions AllData => new(IncludeValidationAlerts: true);
    public static BasicExportOptions VisibleOnly => new(OnlyVisibleRows: true);
}


/// <summary>
/// Import result
/// FUNCTIONAL: Immutable operation result
/// </summary>
internal record ImportResult(
    int ProcessedRows,
    int SuccessfulRows,
    int FailedRows,
    IReadOnlyList<string> Errors,
    TimeSpan Duration)
{
    public bool IsFullySuccessful => FailedRows == 0;
    public double SuccessRate => ProcessedRows > 0 ? (double)SuccessfulRows / ProcessedRows : 0.0;
}

/// <summary>
/// Delete result
/// FUNCTIONAL: Immutable operation result
/// </summary>
internal record DeleteResult(
    int RequestedDeletes,
    int ActualDeletes,
    IReadOnlyList<string> Errors)
{
    public bool IsFullySuccessful => ActualDeletes == RequestedDeletes && Errors.Count == 0;
}

/// <summary>
/// Validation result
/// FUNCTIONAL: Immutable validation outcome
/// </summary>
internal record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationError> Errors,
    int ValidRows,
    int InvalidRows)
{
    public static ValidationResult Success(int validRows) => 
        new(true, Array.Empty<ValidationError>(), validRows, 0);
        
    public static ValidationResult Failure(IReadOnlyList<ValidationError> errors, int validRows, int invalidRows) =>
        new(false, errors, validRows, invalidRows);
        
    public double ValidityRate => (ValidRows + InvalidRows) > 0 ? (double)ValidRows / (ValidRows + InvalidRows) : 0.0;
}

/// <summary>
/// Validation error
/// FUNCTIONAL: Immutable error description
/// </summary>
internal record ValidationError(
    string PropertyName,
    string ErrorMessage,
    int? RowIndex = null,
    object? AttemptedValue = null);

/// <summary>
/// DataGrid state snapshot
/// FUNCTIONAL: Immutable state representation
/// </summary>
internal record DataGridState(
    bool IsInitialized,
    int RowCount,
    int ColumnCount,
    IReadOnlyList<ColumnDefinition> Columns,
    bool HasValidationErrors,
    DateTime LastModified);

/// <summary>
/// Data change event
/// REACTIVE: Immutable change notification
/// </summary>
internal record DataChangeEvent(
    DataChangeType ChangeType,
    IReadOnlyList<int> AffectedRows,
    DateTime Timestamp);

/// <summary>
/// Validation change event  
/// REACTIVE: Immutable validation notification
/// </summary>
internal record ValidationChangeEvent(
    ValidationResult ValidationResult,
    IReadOnlyList<int> AffectedRows,
    DateTime Timestamp);

/// <summary>
/// Data change types
/// </summary>
public enum DataChangeType
{
    Initialized,
    DataImported,
    RowsDeleted,
    DataCleared,
    RowUpdated,
    ValidationUpdated
}