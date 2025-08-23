using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.Core.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Data;
using DomainDataRow = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.DataRow;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.DataGrid;

/// <summary>
/// Enhanced DataGrid Service with Smart Delete Logic and Performance Optimizations
/// HYBRID FUNCTIONAL-OOP: Advanced algorithms from backup with 1M+ row support
/// SMART ALGORITHMS: Intelligent row management, content vs. whole row deletion
/// </summary>
internal sealed class SmartDataGridService : IDataGridService, IDisposable
{
    #region Private Fields - Enhanced State Management
    
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
    
    // SMART DELETE: Minimum row management
    private int _minimumRowCount = 15; // Application can configure this
    
    // PERFORMANCE: 1M+ row optimizations
    private readonly object _lockObject = new();
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);
    
    #endregion
    
    #region Constructor
    
    public SmartDataGridService(ILogger? logger = null)
    {
        _logger = logger;
        _logger?.Info("🚀 SMART SERVICE: Enhanced DataGridService initialized with smart algorithms");
    }
    
    #endregion
    
    #region IDataGridService Implementation
    
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<DomainColumnDefinition> columns,
        DataGridConfiguration? configuration = null)
    {
        try
        {
            await _operationSemaphore.WaitAsync();
            
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
            _configuration = configuration ?? DataGridConfiguration.Default;
            
            // SMART: Set minimum row count from configuration
            if (_configuration.PerformanceConfig != null)
            {
                _minimumRowCount = Math.Max(15, _configuration.PerformanceConfig.VirtualizationBufferSize);
            }
            
            // PERFORMANCE: Clear existing data efficiently
            lock (_lockObject)
            {
                _dataStore.Clear();
                _nextRowId = 0;
            }
            
            // SMART: Ensure minimum empty rows for user experience
            await EnsureMinimumRowsAsync();
            
            _isInitialized = true;
            _lastModified = DateTime.UtcNow;
            
            _logger?.Info("✅ SMART INIT: Initialized with {Columns} columns, minimum {MinRows} rows", 
                columns.Count, _minimumRowCount);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART INIT ERROR: Initialization failed");
            return Result<bool>.Failure(ex);
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }
    
    public bool IsInitialized => _isInitialized;
    public int RowCount { get; private set; }
    public int ColumnCount => _columns.Count;
    public bool HasData => _dataStore.Values.Any(row => !IsRowEmpty(row));
    
    public DataGridState CurrentState => new(
        _isInitialized,
        RowCount,
        ColumnCount,
        _columns,
        false, // TODO: Implement proper validation error tracking
        _lastModified);
    
    public async Task<Result<ImportResult>> ImportDataAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions? options = null)
    {
        var effectiveOptions = options ?? ImportOptions.Default;
        var errors = new List<string>();
        int successfulRows = 0;
        var startTime = DateTime.UtcNow;
        
        try
        {
            await _operationSemaphore.WaitAsync();
            
            _logger?.Info("🔄 SMART IMPORT: Starting import of {Count} rows", data.Count);
            
            // PERFORMANCE: Batch processing for large datasets
            var batchSize = _configuration.PerformanceConfig?.BulkOperationBatchSize ?? 1000;
            var batches = data.Chunk(batchSize);
            
            foreach (var batch in batches)
            {
                var batchResult = await ProcessDataBatchAsync(batch, effectiveOptions, errors);
                successfulRows += batchResult;
                
                // PERFORMANCE: Progress reporting for large imports
                if (effectiveOptions.ProgressReporter != null)
                {
                    var progress = new ImportProgress(
                        data.Count,
                        successfulRows + errors.Count,
                        successfulRows,
                        errors.Count,
                        "Importing data batch",
                        DateTime.UtcNow - startTime);
                    effectiveOptions.ProgressReporter.Report(progress);
                }
                
                // CANCELLATION: Support for large dataset cancellation
                effectiveOptions.CancellationToken.ThrowIfCancellationRequested();
            }
            
            // SMART: Ensure minimum rows after import
            await EnsureMinimumRowsAsync();
            
            var duration = DateTime.UtcNow - startTime;
            var result = new ImportResult(data.Count, successfulRows, errors.Count, errors, duration);
            
            // REACTIVE: Notify data change
            var affectedRows = Enumerable.Range(0, successfulRows).ToList().AsReadOnly();
            _dataChanges.OnNext(new DataChangeEvent(DataChangeType.DataImported, affectedRows, DateTime.UtcNow));
            
            _logger?.Info("✅ SMART IMPORT: Completed - Success: {Success}, Errors: {Errors}, Duration: {Duration}ms",
                successfulRows, errors.Count, duration.TotalMilliseconds);
            
            return Result<ImportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART IMPORT ERROR: Import failed");
            return Result<ImportResult>.Failure(ex);
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }
    
    public async Task<Result<ImportResult>> ImportDataAsync(
        DataTable dataTable,
        ImportOptions? options = null)
    {
        if (dataTable == null)
            return Result<ImportResult>.Failure("DataTable cannot be null");
            
        // CONVERSION: Convert DataTable to dictionaries efficiently
        var data = DataTableToDictionaries(dataTable);
        return await ImportDataAsync(data, options);
    }
    
    /// <summary>
    /// SMART DELETE: Intelligent row deletion with minimum row management
    /// ALGORITHM: Content vs. whole row based on row count and user experience
    /// </summary>
    public async Task<Result<DeleteResult>> SmartDeleteRowAsync(int rowIndex)
    {
        return await SmartDeleteRowsAsync(new[] { rowIndex });
    }
    
    /// <summary>
    /// SMART DELETE: Batch intelligent deletion for multiple rows
    /// ALGORITHM: Preserves user experience while managing data efficiently
    /// </summary>
    public async Task<Result<DeleteResult>> SmartDeleteRowsAsync(IReadOnlyList<int> rowIndices)
    {
        try
        {
            await _operationSemaphore.WaitAsync();
            
            var errors = new List<string>();
            int actualDeletes = 0;
            int requestedDeletes = rowIndices.Count;
            
            _logger?.Info("🗑️ SMART DELETE: Processing {Count} rows with smart algorithm", requestedDeletes);
            
            // SMART ALGORITHM: Analyze deletion strategy
            var currentRowCount = RowCount;
            var nonEmptyRows = _dataStore.Values.Count(row => !IsRowEmpty(row));
            
            // Sort indices in descending order to avoid index shifting issues
            var sortedIndices = rowIndices.OrderByDescending(i => i).ToArray();
            
            foreach (var rowIndex in sortedIndices)
            {
                if (rowIndex < 0 || rowIndex >= currentRowCount)
                {
                    errors.Add($"Row index {rowIndex} is out of range");
                    continue;
                }
                
                // SMART DECISION: Content clear vs. complete row deletion
                if (currentRowCount > _minimumRowCount + 1)
                {
                    // STRATEGY 1: Complete row deletion (we have enough rows)
                    await DeleteCompleteRowAsync(rowIndex);
                    actualDeletes++;
                    currentRowCount--;
                    
                    _logger?.Info("🗑️ COMPLETE DELETE: Row {Index} deleted entirely", rowIndex);
                }
                else
                {
                    // STRATEGY 2: Content clearing (preserve structure for UX)
                    await ClearRowContentAsync(rowIndex);
                    actualDeletes++;
                    
                    _logger?.Info("🧹 CONTENT CLEAR: Row {Index} content cleared, structure preserved", rowIndex);
                }
            }
            
            // SMART: Ensure minimum rows after deletion
            await EnsureMinimumRowsAsync();
            
            var result = new DeleteResult(requestedDeletes, actualDeletes, errors);
            
            // REACTIVE: Notify data change
            _dataChanges.OnNext(new DataChangeEvent(DataChangeType.RowsDeleted, rowIndices, DateTime.UtcNow));
            
            _logger?.Info("✅ SMART DELETE: Completed - Requested: {Requested}, Actual: {Actual}, Errors: {Errors}",
                requestedDeletes, actualDeletes, errors.Count);
            
            return Result<DeleteResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART DELETE ERROR: Deletion failed");
            return Result<DeleteResult>.Failure(ex);
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }
    
    /// <summary>
    /// SMART DELETE: Legacy method name for backward compatibility
    /// </summary>
    public async Task<Result<DeleteResult>> DeleteRowsAsync(IReadOnlyList<int> rowIndices)
    {
        return await SmartDeleteRowsAsync(rowIndices);
    }
    
    public async Task<Result<bool>> ClearDataAsync()
    {
        try
        {
            await _operationSemaphore.WaitAsync();
            
            lock (_lockObject)
            {
                _dataStore.Clear();
                _nextRowId = 0;
            }
            
            // SMART: Recreate minimum rows for user experience
            await EnsureMinimumRowsAsync();
            
            _dataChanges.OnNext(new DataChangeEvent(DataChangeType.DataCleared, new List<int>().AsReadOnly(), DateTime.UtcNow));
            
            _logger?.Info("✅ SMART CLEAR: Data cleared, minimum rows restored");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART CLEAR ERROR: Clear failed");
            return Result<bool>.Failure(ex);
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }
    
    public async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportToDictionariesAsync(
        ExportOptions? options = null)
    {
        var effectiveOptions = options ?? ExportOptions.Default;
        
        try
        {
            var allRows = _dataStore.Values.OrderBy(r => r.RowIndex).ToArray();
            
            // FILTERING: Apply export options
            var filteredRows = effectiveOptions.OnlyValidRows 
                ? allRows.Where(row => IsRowValid(row))
                : allRows;
                
            if (effectiveOptions.OnlyVisibleRows)
            {
                // TODO: Implement visible rows filtering based on UI state
                // For now, use all rows
            }
            
            var result = filteredRows
                .Where(row => !IsRowEmpty(row)) // Export only non-empty rows
                .Select(row => row.Data)
                .ToArray();
            
            _logger?.Info("✅ SMART EXPORT: Exported {Count} rows", result.Length);
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART EXPORT ERROR: Export failed");
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(ex);
        }
    }
    
    public async Task<Result<DataTable>> ExportToDataTableAsync(ExportOptions? options = null)
    {
        try
        {
            var dictResult = await ExportToDictionariesAsync(options);
            if (!dictResult.IsSuccess)
                return Result<DataTable>.Failure(dictResult.ErrorMessage ?? "Export failed");
            
            var dataTable = DictionariesToDataTable(dictResult.Value!, _columns);
            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART EXPORT DT ERROR: DataTable export failed");
            return Result<DataTable>.Failure(ex);
        }
    }
    
    public async Task<Result<ValidationResult>> ValidateAllAsync()
    {
        try
        {
            var allRows = _dataStore.Values.ToArray();
            var errors = new List<ValidationError>();
            int validRows = 0;
            int invalidRows = 0;
            
            foreach (var row in allRows)
            {
                if (IsRowEmpty(row))
                    continue; // Skip empty rows
                    
                var rowValid = true;
                foreach (var column in _columns)
                {
                    var value = row.Data.TryGetValue(column.Name, out var val) ? val : null;
                    
                    // Basic validation
                    if (column.IsRequired && (value == null || string.IsNullOrWhiteSpace(value?.ToString())))
                    {
                        errors.Add(new ValidationError(column.Name, "Required field is empty", row.RowIndex));
                        rowValid = false;
                    }
                }
                
                if (rowValid) validRows++;
                else invalidRows++;
            }
            
            var result = new ValidationResult(errors.Count == 0, errors, validRows, invalidRows);
            return Result<ValidationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SMART VALIDATION ERROR: Validation failed");
            return Result<ValidationResult>.Failure(ex);
        }
    }
    
    #endregion
    
    #region Smart Algorithm Implementations
    
    /// <summary>
    /// SMART: Ensure minimum number of rows for optimal user experience
    /// ALGORITHM: Maintains minimum rows while respecting data integrity
    /// </summary>
    private async Task EnsureMinimumRowsAsync()
    {
        lock (_lockObject)
        {
            var currentCount = _dataStore.Count;
            var targetCount = Math.Max(_minimumRowCount, currentCount);
            
            // Add empty rows to reach minimum
            for (int i = currentCount; i < targetCount + 1; i++) // +1 for always having an empty row at the end
            {
                var emptyRow = CreateEmptyRow(i);
                _dataStore.TryAdd(i, emptyRow);
            }
            
            RowCount = _dataStore.Count;
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// SMART: Complete row deletion - removes entire row from dataset
    /// ALGORITHM: Used when we have sufficient rows for good UX
    /// </summary>
    private async Task DeleteCompleteRowAsync(int rowIndex)
    {
        lock (_lockObject)
        {
            if (_dataStore.TryRemove(rowIndex, out _))
            {
                // Reindex remaining rows
                var remainingRows = _dataStore.Values.Where(r => r.RowIndex > rowIndex).ToArray();
                foreach (var row in remainingRows)
                {
                    _dataStore.TryRemove(row.RowIndex, out _);
                    var adjustedRow = new DomainDataRow(row.Data, row.RowIndex - 1);
                    _dataStore.TryAdd(adjustedRow.RowIndex, adjustedRow);
                }
                
                RowCount = _dataStore.Count;
            }
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// SMART: Content clearing - preserves row structure, clears data only
    /// ALGORITHM: Used when deleting would go below minimum rows (UX preservation)
    /// </summary>
    private async Task ClearRowContentAsync(int rowIndex)
    {
        lock (_lockObject)
        {
            if (_dataStore.TryGetValue(rowIndex, out var existingRow))
            {
                var clearedRow = CreateEmptyRow(rowIndex);
                _dataStore.TryUpdate(rowIndex, clearedRow, existingRow);
            }
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// PERFORMANCE: Process data batch efficiently for large imports
    /// </summary>
    private async Task<int> ProcessDataBatchAsync(
        IEnumerable<IReadOnlyDictionary<string, object?>> batch,
        ImportOptions options,
        List<string> errors)
    {
        int successCount = 0;
        
        foreach (var (row, index) in batch.Select((r, i) => (r, i)))
        {
            try
            {
                var rowIndex = _nextRowId++;
                var dataRow = new DomainDataRow(row.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), rowIndex);
                
                // VALIDATION: Validate if enabled
                if (options.ValidateData)
                {
                    var validationResult = ValidateDataRow(dataRow);
                    if (!validationResult.IsSuccess && options.SkipInvalidRows)
                    {
                        errors.Add($"Row {rowIndex}: {validationResult.ErrorMessage}");
                        continue;
                    }
                }
                
                // STORAGE: Add to concurrent store
                lock (_lockObject)
                {
                    _dataStore.TryAdd(rowIndex, dataRow);
                }
                
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row processing failed: {ex.Message}");
            }
        }
        
        // Update row count
        lock (_lockObject)
        {
            RowCount = _dataStore.Count;
        }
        
        return successCount;
    }
    
    #endregion
    
    #region Helper Methods
    
    private DomainDataRow CreateEmptyRow(int rowIndex)
    {
        var emptyData = _columns.ToDictionary(col => col.Name, col => (object?)null);
        return new DomainDataRow(emptyData, rowIndex);
    }
    
    private bool IsRowEmpty(DomainDataRow row)
    {
        return row.Data.Values.All(value => value == null || string.IsNullOrWhiteSpace(value?.ToString()));
    }
    
    private Result<bool> ValidateDataRow(DomainDataRow row)
    {
        foreach (var column in _columns)
        {
            var hasValue = row.Data.TryGetValue(column.Name, out var value);
            
            // Required field validation
            if (column.IsRequired && (!hasValue || value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                return Result<bool>.Failure($"Required field '{column.Name}' is empty");
            }
        }
        
        return Result<bool>.Success(true);
    }
    
    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> DataTableToDictionaries(DataTable dataTable)
    {
        var result = new List<IReadOnlyDictionary<string, object?>>();
        
        foreach (System.Data.DataRow dataRow in dataTable.Rows)
        {
            var dictionary = new Dictionary<string, object?>();
            foreach (DataColumn column in dataTable.Columns)
            {
                dictionary[column.ColumnName] = dataRow[column] == DBNull.Value ? null : dataRow[column];
            }
            result.Add(dictionary);
        }
        
        return result;
    }
    
    private DataTable DictionariesToDataTable(IReadOnlyList<IReadOnlyDictionary<string, object?>> data, IReadOnlyList<DomainColumnDefinition> columns)
    {
        var dataTable = new DataTable();
        
        // Add columns
        foreach (var column in columns)
        {
            dataTable.Columns.Add(column.Name, column.DataType);
        }
        
        // Add rows
        foreach (var dict in data)
        {
            var row = dataTable.NewRow();
            foreach (var column in columns)
            {
                row[column.Name] = dict.TryGetValue(column.Name, out var value) && value != null ? value : DBNull.Value;
            }
            dataTable.Rows.Add(row);
        }
        
        return dataTable;
    }
    
    #endregion
    
    #region Reactive Properties
    
    public IObservable<DataChangeEvent> DataChanges => _dataChanges;
    public IObservable<ValidationChangeEvent> ValidationChanges => _validationChanges;
    
    #endregion
    
    #region IDisposable
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _dataChanges?.Dispose();
        _validationChanges?.Dispose();
        _operationSemaphore?.Dispose();
        _isDisposed = true;
        
        _logger?.Info("🧹 SMART SERVICE: Disposed with smart cleanup");
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Check if row is valid using current validation configuration
    /// </summary>
    private bool IsRowValid(DomainDataRow row)
    {
        if (_configuration?.ValidationConfig == null)
            return true;
            
        // Simple validation check - can be extended
        foreach (var column in _columns)
        {
            if (column.IsRequired)
            {
                var value = row.GetValue<object>(column.Name);
                if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                    return false;
            }
        }
        
        return true;
    }
    
    #endregion
}