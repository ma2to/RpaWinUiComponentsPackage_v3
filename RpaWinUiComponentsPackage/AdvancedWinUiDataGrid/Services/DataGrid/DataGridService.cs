using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.Core.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using System.Collections.Concurrent;
using System.Data;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using DomainDataRow = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.DataRow;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.DataGrid;

/// <summary>
/// Headless DataGrid Service Implementation
/// HYBRID FUNCTIONAL-OOP: Combines functional programming with OOP service patterns
/// DUAL-USE: Can be used by UI components or automation scripts independently
/// </summary>
internal sealed class DataGridService : IDataGridService, IDisposable
{
    #region Private Fields - OOP State Management
    
    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<int, DomainDataRow> _dataStore = new();
    private readonly Subject<DataChangeEvent> _dataChanges = new();
    private readonly Subject<ValidationChangeEvent> _validationChanges = new();
    
    private IReadOnlyList<DomainColumnDefinition> _columns = Array.Empty<DomainColumnDefinition>();
    private DataGridConfiguration _configuration = DataGridConfiguration.Default;
    private bool _isInitialized = false;
    private bool _isDisposed = false;
    private int _nextRowId = 0;
    private DateTime _lastModified = DateTime.MinValue;
    
    #endregion
    
    #region Constructor
    
    public DataGridService(ILogger? logger = null)
    {
        _logger = logger;
        _logger?.Info("🚀 HEADLESS SERVICE: DataGridService initialized");
    }
    
    #endregion
    
    #region IDataGridService Implementation
    
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<DomainColumnDefinition> columns,
        DataGridConfiguration? configuration = null)
    {
        _logger?.Info("🚀 METHOD START: InitializeAsync - Columns: {ColumnCount}, Config: {ConfigType}", 
            columns?.Count ?? 0, configuration?.GetType().Name ?? "Default");
        
        try
        {
            if (_isDisposed) 
            {
                _logger?.Warning("⚠️ INITIALIZATION: Service is already disposed");
                return Result<bool>.Failure("Service is disposed");
            }
                
            if (columns == null || columns.Count == 0)
            {
                _logger?.Error("❌ INITIALIZATION: Invalid columns - null or empty");
                return Result<bool>.Failure("Columns cannot be null or empty");
            }
            
            _logger?.Info("🔧 INITIALIZATION: Initializing with {ColumnCount} columns", columns.Count);
            
            // FUNCTIONAL VALIDATION: Validate column definitions
            var validationResult = ValidateColumns(columns);
            if (!validationResult.IsSuccess)
                return Result<bool>.Failure(validationResult.ErrorMessage);
            
            _columns = columns;
            _configuration = configuration ?? DataGridConfiguration.Default;
            _dataStore.Clear();
            _nextRowId = 0;
            _isInitialized = true;
            _lastModified = DateTime.UtcNow;
            
            // REACTIVE: Notify initialization
            _dataChanges.OnNext(new DataChangeEvent(
                DataChangeType.Initialized, 
                Array.Empty<int>(), 
                DateTime.UtcNow));
            
            _logger?.Info("✅ INITIALIZATION: Service initialized successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 INITIALIZATION ERROR: Failed to initialize service");
            return Result<bool>.Failure(ex);
        }
    }
    
    public bool IsInitialized => _isInitialized;
    
    public async Task<Result<ImportResult>> ImportDataAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions? options = null)
    {
        _logger?.Info("🚀 METHOD START: ImportDataAsync - DataRows: {RowCount}, ValidateData: {ValidateData}, ReplaceExisting: {ReplaceExisting}", 
            data?.Count ?? 0, options?.ValidateData ?? true, options?.ReplaceExistingData ?? false);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (!_isInitialized)
            {
                _logger?.Error("❌ IMPORT: Service not initialized");
                return Result<ImportResult>.Failure("Service not initialized");
            }
                
            if (data == null)
            {
                _logger?.Error("❌ IMPORT: Data parameter is null");
                return Result<ImportResult>.Failure("Data cannot be null");
            }
            
            var effectiveOptions = options ?? ImportOptions.Default;
            _logger?.Info("📥 IMPORT: Starting import of {RowCount} rows with options: {Options}", 
                data.Count, effectiveOptions.GetType().Name);
            
            // FUNCTIONAL: Clear existing data if requested
            if (effectiveOptions.ReplaceExistingData)
            {
                _dataStore.Clear();
                _nextRowId = 0;
            }
            
            var errors = new List<string>();
            var successfulRows = 0;
            var startIndex = effectiveOptions.StartRowIndex ?? _nextRowId;
            
            // FUNCTIONAL PIPELINE: Process each row immutably
            var processedRows = await ProcessDataRowsAsync(data, effectiveOptions, errors);
            
            foreach (var (row, index) in processedRows.Select((r, i) => (r, i)))
            {
                try
                {
                    var rowIndex = startIndex + index;
                    var dataRow = new DomainDataRow(row.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), rowIndex);
                    
                    // FUNCTIONAL VALIDATION: Validate if enabled
                    if (effectiveOptions.ValidateData)
                    {
                        var validationResult = ValidateDataRow(dataRow);
                        if (!validationResult.IsSuccess)
                        {
                            errors.Add($"Row {rowIndex}: {validationResult.ErrorMessage}");
                            continue;
                        }
                    }
                    
                    _dataStore.TryAdd(_nextRowId++, dataRow);
                    successfulRows++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {startIndex + index}: {ex.Message}");
                }
            }
            
            stopwatch.Stop();
            _lastModified = DateTime.UtcNow;
            
            var result = new ImportResult(
                data.Count,
                successfulRows,
                data.Count - successfulRows,
                errors.AsReadOnly(),
                stopwatch.Elapsed);
            
            // REACTIVE: Notify data change
            _dataChanges.OnNext(new DataChangeEvent(
                DataChangeType.DataImported,
                Enumerable.Range(startIndex, successfulRows).ToArray(),
                DateTime.UtcNow));
            
            _logger?.Info("✅ IMPORT: Imported {Successful}/{Total} rows in {Duration}ms", 
                successfulRows, data.Count, stopwatch.ElapsedMilliseconds);
            
            return Result<ImportResult>.Success(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.Error(ex, "🚨 IMPORT ERROR: Failed to import data");
            return Result<ImportResult>.Failure(ex);
        }
    }
    
    public async Task<Result<ImportResult>> ImportDataAsync(
        DataTable dataTable,
        ImportOptions? options = null)
    {
        try
        {
            if (dataTable == null)
                return Result<ImportResult>.Failure("DataTable cannot be null");
            
            // FUNCTIONAL TRANSFORMATION: DataTable -> Immutable dictionaries
            var dictionaries = DataTableToDictionaries(dataTable);
            return await ImportDataAsync(dictionaries, options);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 IMPORT ERROR: Failed to import from DataTable");
            return Result<ImportResult>.Failure(ex);
        }
    }
    
    public async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportToDictionariesAsync(
        ExportOptions? options = null)
    {
        try
        {
            if (!_isInitialized)
                return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure("Service not initialized");
            
            var effectiveOptions = options ?? ExportOptions.Default;
            _logger?.Info("📤 EXPORT: Starting export to dictionaries");
            
            // FUNCTIONAL PIPELINE: Filter and transform data
            var allRows = _dataStore.Values.OrderBy(r => r.RowIndex).ToList();
            
            // FILTERING: Apply row filtering if specific rows are requested
            var filteredRows = effectiveOptions.RowsToExport != null 
                ? allRows.Where(row => effectiveOptions.RowsToExport.Contains(row.RowIndex)).ToList()
                : allRows;
            
            if (effectiveOptions.OnlyValidRows)
            {
                var validRows = new List<DomainDataRow>();
                foreach (var row in filteredRows)
                {
                    var validation = ValidateDataRow(row);
                    if (validation.IsSuccess)
                        validRows.Add(row);
                }
                filteredRows = validRows;
            }
            
            // FUNCTIONAL TRANSFORMATION: Extract data dictionaries
            var result = filteredRows.Select(row => row.Data).ToArray();
            
            _logger?.Info("✅ EXPORT: Exported {Count} rows to dictionaries", result.Length);
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 EXPORT ERROR: Failed to export to dictionaries");
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(ex);
        }
    }
    
    public async Task<Result<DataTable>> ExportToDataTableAsync(ExportOptions? options = null)
    {
        try
        {
            var dictionariesResult = await ExportToDictionariesAsync(options);
            if (!dictionariesResult.IsSuccess)
                return Result<DataTable>.Failure(dictionariesResult.ErrorMessage ?? "Export failed");
            
            // FUNCTIONAL TRANSFORMATION: Dictionaries -> DataTable
            var dataTable = DictionariesToDataTable(dictionariesResult.Value!, options);
            
            _logger?.Info("✅ EXPORT: Exported {Count} rows to DataTable", dataTable.Rows.Count);
            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 EXPORT ERROR: Failed to export to DataTable");
            return Result<DataTable>.Failure(ex);
        }
    }
    
    public async Task<Result<DeleteResult>> DeleteRowsAsync(IReadOnlyList<int> rowIndices)
    {
        _logger?.Info("🚀 METHOD START: DeleteRowsAsync - RowIndices: [{Indices}], Count: {Count}", 
            string.Join(", ", rowIndices?.Take(10) ?? Array.Empty<int>()), rowIndices?.Count ?? 0);
        
        try
        {
            if (!_isInitialized)
            {
                _logger?.Error("❌ DELETE: Service not initialized");
                return Result<DeleteResult>.Failure("Service not initialized");
            }
                
            if (rowIndices == null)
            {
                _logger?.Error("❌ DELETE: Row indices parameter is null");
                return Result<DeleteResult>.Failure("Row indices cannot be null");
            }
            
            _logger?.Info("🗑️ DELETE: Deleting {Count} rows, Current store size: {StoreSize}", 
                rowIndices.Count, _dataStore.Count);
            
            var errors = new List<string>();
            var actualDeletes = 0;
            
            // FUNCTIONAL: Find and remove rows by data row index, not storage key
            var rowsToDelete = _dataStore.Values
                .Where(row => rowIndices.Contains(row.RowIndex))
                .ToList();
            
            foreach (var row in rowsToDelete)
            {
                var keyToRemove = _dataStore.FirstOrDefault(kvp => kvp.Value.RowIndex == row.RowIndex).Key;
                if (_dataStore.TryRemove(keyToRemove, out _))
                {
                    actualDeletes++;
                }
                else
                {
                    errors.Add($"Failed to delete row {row.RowIndex}");
                }
            }
            
            _lastModified = DateTime.UtcNow;
            
            var result = new DeleteResult(rowIndices.Count, actualDeletes, errors.AsReadOnly());
            
            // REACTIVE: Notify deletion
            _dataChanges.OnNext(new DataChangeEvent(
                DataChangeType.RowsDeleted,
                rowIndices.ToArray(),
                DateTime.UtcNow));
            
            _logger?.Info("✅ DELETE: Deleted {Actual}/{Requested} rows", actualDeletes, rowIndices.Count);
            return Result<DeleteResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 DELETE ERROR: Failed to delete rows");
            return Result<DeleteResult>.Failure(ex);
        }
    }
    
    public async Task<Result<bool>> ClearDataAsync()
    {
        try
        {
            if (!_isInitialized)
                return Result<bool>.Failure("Service not initialized");
            
            _logger?.Info("🧹 CLEAR: Clearing all data");
            
            _dataStore.Clear();
            _nextRowId = 0;
            _lastModified = DateTime.UtcNow;
            
            // REACTIVE: Notify clear
            _dataChanges.OnNext(new DataChangeEvent(
                DataChangeType.DataCleared,
                Array.Empty<int>(),
                DateTime.UtcNow));
            
            _logger?.Info("✅ CLEAR: All data cleared successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 CLEAR ERROR: Failed to clear data");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task<Result<ValidationResult>> ValidateAllAsync()
    {
        try
        {
            if (!_isInitialized)
                return Result<ValidationResult>.Failure("Service not initialized");
            
            _logger?.Info("✅ VALIDATION: Starting validation of all data");
            
            var allRows = _dataStore.Values.ToList();
            var errors = new List<ValidationError>();
            var validRows = 0;
            var invalidRows = 0;
            
            // FUNCTIONAL VALIDATION PIPELINE: Process all rows
            foreach (var row in allRows)
            {
                var validation = ValidateDataRow(row);
                if (validation.IsSuccess)
                {
                    validRows++;
                }
                else
                {
                    invalidRows++;
                    errors.Add(new ValidationError("Row", validation.ErrorMessage ?? "Validation failed", row.RowIndex));
                }
            }
            
            var result = errors.Count == 0
                ? ValidationResult.Success(validRows)
                : ValidationResult.Failure(errors.AsReadOnly(), validRows, invalidRows);
            
            // REACTIVE: Notify validation
            _validationChanges.OnNext(new ValidationChangeEvent(
                result,
                allRows.Select(r => r.RowIndex).ToArray(),
                DateTime.UtcNow));
            
            _logger?.Info("✅ VALIDATION: Validated {Valid}/{Total} rows", validRows, allRows.Count);
            return Result<ValidationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 VALIDATION ERROR: Failed to validate data");
            return Result<ValidationResult>.Failure(ex);
        }
    }
    
    #endregion
    
    #region Properties
    
    public int RowCount => _dataStore.Count;
    public int ColumnCount => _columns.Count;
    public bool HasData => _dataStore.Count > 0;
    
    public DataGridState CurrentState => new(
        _isInitialized,
        RowCount,
        ColumnCount,
        _columns,
        false, // TODO: Track validation state
        _lastModified);
    
    public IObservable<DataChangeEvent> DataChanges => _dataChanges.AsObservable();
    public IObservable<ValidationChangeEvent> ValidationChanges => _validationChanges.AsObservable();
    
    #endregion
    
    #region Private Helper Methods - Pure Functions
    
    private Result<bool> ValidateColumns(IReadOnlyList<DomainColumnDefinition> columns)
    {
        var columnNames = new HashSet<string>();
        
        foreach (var column in columns)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
                return Result<bool>.Failure("Column name cannot be null or empty");
                
            if (!columnNames.Add(column.Name))
                return Result<bool>.Failure($"Duplicate column name: {column.Name}");
        }
        
        return Result<bool>.Success(true);
    }
    
    private Result<bool> ValidateDataRow(DomainDataRow row)
    {
        foreach (var column in _columns)
        {
            var hasValue = row.HasColumn(column.Name);
            var value = hasValue ? row.Data[column.Name] : null;
            
            // Required field validation
            if (column.IsRequired && (value == null || string.IsNullOrWhiteSpace(value?.ToString())))
                return Result<bool>.Failure($"Column '{column.Name}' is required");
            
            // Type validation
            if (value != null && !IsValueCompatibleWithType(value, column.DataType))
                return Result<bool>.Failure($"Value '{value}' is not compatible with type '{column.DataType.Name}'");
            
            // Length validation
            if (column.MaxLength.HasValue && value?.ToString()?.Length > column.MaxLength.Value)
                return Result<bool>.Failure($"Value exceeds maximum length of {column.MaxLength.Value}");
        }
        
        return Result<bool>.Success(true);
    }
    
    private static bool IsValueCompatibleWithType(object value, Type targetType)
    {
        if (value == null) return true;
        
        var valueType = value.GetType();
        if (valueType == targetType) return true;
        
        // Handle nullable types
        var nullableType = Nullable.GetUnderlyingType(targetType);
        if (nullableType != null && valueType == nullableType) return true;
        
        // Try basic conversions
        try
        {
            Convert.ChangeType(value, targetType);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ProcessDataRowsAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions options,
        List<string> errors)
    {
        // FUNCTIONAL: Apply any data transformations here
        return await Task.FromResult(data);
    }
    
    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> DataTableToDictionaries(DataTable dataTable)
    {
        var result = new List<IReadOnlyDictionary<string, object?>>();
        
        foreach (System.Data.DataRow dataRow in dataTable.Rows)
        {
            var dictionary = new Dictionary<string, object?>();
            foreach (DataColumn column in dataTable.Columns)
            {
                var value = dataRow[column];
                dictionary[column.ColumnName] = value == DBNull.Value ? null : value;
            }
            result.Add(dictionary);
        }
        
        return result.AsReadOnly();
    }
    
    private DataTable DictionariesToDataTable(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ExportOptions? options)
    {
        var dataTable = new DataTable();
        
        if (data.Count == 0) return dataTable;
        
        // FUNCTIONAL: Create columns from schema and first row
        foreach (var column in _columns)
        {
            dataTable.Columns.Add(column.Name, Nullable.GetUnderlyingType(column.DataType) ?? column.DataType);
        }
        
        // FUNCTIONAL: Add data rows
        foreach (var rowData in data)
        {
            var dataRow = dataTable.NewRow();
            foreach (var column in _columns)
            {
                var value = rowData.ContainsKey(column.Name) ? rowData[column.Name] : null;
                dataRow[column.Name] = value ?? DBNull.Value;
            }
            dataTable.Rows.Add(dataRow);
        }
        
        return dataTable;
    }
    
    #endregion
    
    #region IDisposable
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _dataChanges?.OnCompleted();
        _dataChanges?.Dispose();
        _validationChanges?.OnCompleted();
        _validationChanges?.Dispose();
        
        _dataStore.Clear();
        _isDisposed = true;
        
        _logger?.Info("🧹 DISPOSE: DataGridService disposed");
    }
    
    #endregion
}