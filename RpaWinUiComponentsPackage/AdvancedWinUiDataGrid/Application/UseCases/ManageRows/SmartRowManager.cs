using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Management;

/// <summary>
/// DOCUMENTATION: Smart row management per specification
/// ENTERPRISE: Intelligent row insertion, deletion, and maintenance
/// SMART: Automatically manages empty last row per specification
/// </summary>
public class SmartRowManager : IDisposable
{
    #region DOCUMENTATION: Private Fields

    private readonly ILogger? _logger;
    private readonly List<Dictionary<string, object?>> _data;
    private readonly List<ColumnDefinition> _columns;
    private readonly SmartValidationManager _validationManager;
    private DataGridCoreOptions? _options;
    private bool _disposed;

    #endregion

    #region DOCUMENTATION: Events

    /// <summary>Event fired when a row is added</summary>
    public event EventHandler<RowChangedEventArgs>? RowAdded;
    
    /// <summary>Event fired when a row is deleted</summary>
    public event EventHandler<RowChangedEventArgs>? RowDeleted;
    
    /// <summary>Event fired when a row is modified</summary>
    public event EventHandler<RowChangedEventArgs>? RowModified;
    
    /// <summary>Event fired when row validation completes</summary>
    public event EventHandler<RowValidationEventArgs>? RowValidated;

    #endregion

    #region DOCUMENTATION: Constructor

    /// <summary>
    /// DOCUMENTATION: Initialize smart row manager
    /// ENTERPRISE: Advanced row management with validation integration
    /// </summary>
    /// <param name="data">Reference to data collection</param>
    /// <param name="columns">Column definitions</param>
    /// <param name="validationManager">Validation manager instance</param>
    /// <param name="options">DataGrid options</param>
    /// <param name="logger">Optional logger</param>
    public SmartRowManager(
        List<Dictionary<string, object?>> data,
        List<ColumnDefinition> columns,
        SmartValidationManager validationManager,
        DataGridCoreOptions? options = null,
        ILogger? logger = null)
    {
        _logger = logger;
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
        _options = options;

        _logger?.LogDebug("🧠 SMART ROW: Smart row manager initialized");
    }

    #endregion

    #region DOCUMENTATION: Properties

    /// <summary>Total number of rows including empty last row</summary>
    public int TotalRows => _data.Count;

    /// <summary>Number of non-empty rows</summary>
    public int NonEmptyRows => _data.Count(row => !IsRowEmpty(row));

    /// <summary>Minimum number of rows to maintain</summary>
    public int MinimumRows => _options?.MinimumRows ?? 1;

    #endregion

    #region DOCUMENTATION: Smart Row Addition

    /// <summary>
    /// DOCUMENTATION: Smart row addition per specification
    /// SMART: Automatically manages empty last row per specification
    /// VALIDATION: Validates new row before addition
    /// </summary>
    /// <param name="rowData">Data for new row</param>
    /// <param name="insertIndex">Optional insertion index (null = append)</param>
    /// <param name="validateRow">Whether to validate the new row</param>
    /// <returns>Row addition result with validation details</returns>
    public async Task<Result<RowOperationResult>> AddRowAsync(
        Dictionary<string, object?> rowData,
        int? insertIndex = null,
        bool validateRow = true)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SmartRowManager));

        try
        {
            _logger?.LogDebug("🧠 SMART ROW: Adding row at index {Index}", insertIndex ?? _data.Count);

            // Validate input
            if (rowData == null)
                return Result<RowOperationResult>.Failure("Row data cannot be null");

            // Process row data to ensure all columns are present
            var processedRow = ProcessRowForInsertion(rowData);

            // Determine insertion index
            var targetIndex = insertIndex ?? _data.Count;
            if (targetIndex < 0 || targetIndex > _data.Count)
                targetIndex = _data.Count;

            // Validate row if requested
            var validationErrors = new List<ValidationError>();
            if (validateRow)
            {
                validationErrors = await _validationManager.ValidateRowAsync(processedRow, targetIndex, _columns);
            }

            // Insert the row
            if (targetIndex >= _data.Count)
            {
                _data.Add(processedRow);
            }
            else
            {
                _data.Insert(targetIndex, processedRow);
            }

            // SMART: Ensure minimum rows are maintained
            await EnsureMinimumRowsAsync();

            // SMART: Manage empty last row
            await EnsureEmptyLastRowAsync();

            var result = new RowOperationResult
            {
                Success = true,
                RowIndex = targetIndex,
                OperationType = RowOperationType.Add,
                ValidationErrors = validationErrors,
                AffectedRows = 1
            };

            // Fire events
            RowAdded?.Invoke(this, new RowChangedEventArgs { RowIndex = targetIndex, RowData = processedRow });
            
            if (validationErrors.Count > 0)
            {
                RowValidated?.Invoke(this, new RowValidationEventArgs 
                { 
                    RowIndex = targetIndex, 
                    ValidationErrors = validationErrors 
                });
            }

            _logger?.LogInformation("✅ SMART ROW: Row added successfully at index {Index} with {ErrorCount} validation errors", 
                targetIndex, validationErrors.Count);

            return Result<RowOperationResult>.Success(result);
        }
        catch (Exception ex)
        {
            var errorMessage = "Smart row addition failed";
            _logger?.LogError(ex, "💥 SMART ROW: {ErrorMessage}", errorMessage);
            return Result<RowOperationResult>.Failure(errorMessage, ex);
        }
    }

    #endregion

    #region DOCUMENTATION: Smart Row Deletion

    /// <summary>
    /// DOCUMENTATION: Smart row deletion per specification
    /// SMART: Intelligent deletion with confirmation and minimum row management
    /// VALIDATION: Maintains data integrity during deletion
    /// </summary>
    /// <param name="rowIndex">Index of row to delete</param>
    /// <param name="requireConfirmation">Whether to require confirmation</param>
    /// <param name="confirmationCallback">Confirmation callback function</param>
    /// <returns>Row deletion result</returns>
    public async Task<Result<RowOperationResult>> DeleteRowAsync(
        int rowIndex,
        bool requireConfirmation = true,
        Func<Dictionary<string, object?>, Task<bool>>? confirmationCallback = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SmartRowManager));

        try
        {
            _logger?.LogDebug("🧠 SMART ROW: Attempting to delete row at index {Index}", rowIndex);

            // Validate index
            if (rowIndex < 0 || rowIndex >= _data.Count)
                return Result<RowOperationResult>.Failure("Row index is out of range");

            var rowToDelete = _data[rowIndex];

            // Check minimum rows constraint
            var nonEmptyCount = NonEmptyRows;
            var isRowEmpty = IsRowEmpty(rowToDelete);
            
            if (!isRowEmpty && nonEmptyCount <= MinimumRows)
            {
                _logger?.LogWarning("⚠️ SMART ROW: Cannot delete row - would violate minimum row count");
                return Result<RowOperationResult>.Failure($"Cannot delete row - minimum {MinimumRows} rows required");
            }

            // Handle confirmation if required
            if (requireConfirmation && !isRowEmpty)
            {
                bool confirmed = false;
                
                if (confirmationCallback != null)
                {
                    confirmed = await confirmationCallback(rowToDelete);
                }
                else
                {
                    // Default confirmation logic (would need UI implementation)
                    confirmed = true; // For headless mode, assume confirmed
                }

                if (!confirmed)
                {
                    _logger?.LogDebug("❌ SMART ROW: Row deletion cancelled by user");
                    return Result<RowOperationResult>.Failure("Row deletion cancelled by user");
                }
            }

            // Perform deletion
            _data.RemoveAt(rowIndex);

            // SMART: Ensure minimum rows are maintained after deletion
            await EnsureMinimumRowsAsync();

            // SMART: Ensure empty last row is maintained
            await EnsureEmptyLastRowAsync();

            var result = new RowOperationResult
            {
                Success = true,
                RowIndex = rowIndex,
                OperationType = RowOperationType.Delete,
                ValidationErrors = new List<ValidationError>(),
                AffectedRows = 1
            };

            // Fire event
            RowDeleted?.Invoke(this, new RowChangedEventArgs { RowIndex = rowIndex, RowData = rowToDelete });

            _logger?.LogInformation("✅ SMART ROW: Row deleted successfully at index {Index}", rowIndex);
            
            return Result<RowOperationResult>.Success(result);
        }
        catch (Exception ex)
        {
            var errorMessage = "Smart row deletion failed";
            _logger?.LogError(ex, "💥 SMART ROW: {ErrorMessage}", errorMessage);
            return Result<RowOperationResult>.Failure(errorMessage, ex);
        }
    }

    #endregion

    #region DOCUMENTATION: Row Modification

    /// <summary>
    /// DOCUMENTATION: Smart row modification per specification
    /// SMART: Intelligent updating with validation triggers
    /// </summary>
    /// <param name="rowIndex">Index of row to modify</param>
    /// <param name="newData">New row data</param>
    /// <param name="validateRow">Whether to validate after modification</param>
    /// <returns>Row modification result</returns>
    public async Task<Result<RowOperationResult>> ModifyRowAsync(
        int rowIndex,
        Dictionary<string, object?> newData,
        bool validateRow = true)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SmartRowManager));

        try
        {
            // Validate index
            if (rowIndex < 0 || rowIndex >= _data.Count)
                return Result<RowOperationResult>.Failure("Row index is out of range");

            // Process new data
            var processedData = ProcessRowForInsertion(newData);

            // Store old data for event
            var oldData = new Dictionary<string, object?>(_data[rowIndex]);

            // Update row
            _data[rowIndex] = processedData;

            // Validate if requested
            var validationErrors = new List<ValidationError>();
            if (validateRow)
            {
                validationErrors = await _validationManager.ValidateRowAsync(processedData, rowIndex, _columns);
            }

            var result = new RowOperationResult
            {
                Success = true,
                RowIndex = rowIndex,
                OperationType = RowOperationType.Modify,
                ValidationErrors = validationErrors,
                AffectedRows = 1
            };

            // Fire events
            RowModified?.Invoke(this, new RowChangedEventArgs { RowIndex = rowIndex, RowData = processedData, OldData = oldData });
            
            if (validationErrors.Count > 0)
            {
                RowValidated?.Invoke(this, new RowValidationEventArgs 
                { 
                    RowIndex = rowIndex, 
                    ValidationErrors = validationErrors 
                });
            }

            _logger?.LogDebug("✅ SMART ROW: Row modified successfully at index {Index} with {ErrorCount} validation errors", 
                rowIndex, validationErrors.Count);

            return Result<RowOperationResult>.Success(result);
        }
        catch (Exception ex)
        {
            var errorMessage = "Smart row modification failed";
            _logger?.LogError(ex, "💥 SMART ROW: {ErrorMessage}", errorMessage);
            return Result<RowOperationResult>.Failure(errorMessage, ex);
        }
    }

    #endregion

    #region INTERNAL: Smart Management

    /// <summary>
    /// INTERNAL: Ensure minimum number of rows is maintained
    /// SMART: Automatically adds empty rows if needed
    /// </summary>
    private async Task EnsureMinimumRowsAsync()
    {
        while (_data.Count < MinimumRows)
        {
            var emptyRow = CreateEmptyRow();
            _data.Add(emptyRow);
            _logger?.LogDebug("🧠 SMART ROW: Added empty row to maintain minimum count");
        }
    }

    /// <summary>
    /// INTERNAL: Ensure there's always an empty row at the end for new data entry
    /// SMART: Core behavior per specification
    /// </summary>
    private async Task EnsureEmptyLastRowAsync()
    {
        if (_data.Count == 0)
        {
            _data.Add(CreateEmptyRow());
            return;
        }

        var lastRow = _data[^1];
        if (!IsRowEmpty(lastRow))
        {
            var emptyRow = CreateEmptyRow();
            _data.Add(emptyRow);
            _logger?.LogDebug("🧠 SMART ROW: Added empty last row per specification");
        }
    }

    /// <summary>
    /// INTERNAL: Create empty row with default values for all columns
    /// </summary>
    private Dictionary<string, object?> CreateEmptyRow()
    {
        var emptyRow = new Dictionary<string, object?>();
        
        foreach (var column in _columns)
        {
            emptyRow[column.PropertyName] = GetDefaultValueForColumn(column);
        }

        return emptyRow;
    }

    /// <summary>
    /// INTERNAL: Get default value for column based on its type
    /// </summary>
    private object? GetDefaultValueForColumn(ColumnDefinition column)
    {
        return column.DataType?.Name.ToLowerInvariant() switch
        {
            "string" => string.Empty,
            "int" or "int32" => 0,
            "double" => 0.0,
            "decimal" => 0m,
            "bool" or "boolean" => false,
            "datetime" => null, // Let user enter date
            _ => null
        };
    }

    /// <summary>
    /// INTERNAL: Process row data to ensure all columns are present
    /// </summary>
    private Dictionary<string, object?> ProcessRowForInsertion(Dictionary<string, object?> rowData)
    {
        var processedRow = new Dictionary<string, object?>();

        // Ensure all columns have values
        foreach (var column in _columns)
        {
            if (rowData.TryGetValue(column.PropertyName, out var value))
            {
                processedRow[column.PropertyName] = value;
            }
            else
            {
                processedRow[column.PropertyName] = GetDefaultValueForColumn(column);
            }
        }

        return processedRow;
    }

    /// <summary>
    /// INTERNAL: Check if row is completely empty
    /// </summary>
    private bool IsRowEmpty(Dictionary<string, object?> row)
    {
        return row.Values.All(value =>
            value == null ||
            (value is string str && string.IsNullOrWhiteSpace(str)) ||
            (value.ToString()?.Trim().Length == 0));
    }

    #endregion

    #region DOCUMENTATION: Resource Management

    /// <summary>
    /// DOCUMENTATION: Dispose smart row manager resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _disposed = true;
            _logger?.LogDebug("🗑️ SMART ROW: Smart row manager disposed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "⚠️ SMART ROW: Error during disposal");
        }
    }

    #endregion
}

#region DOCUMENTATION: Supporting Classes

/// <summary>
/// DOCUMENTATION: Row operation result per specification
/// ENTERPRISE: Comprehensive operation result with validation details
/// </summary>
public record RowOperationResult
{
    public bool Success { get; init; }
    public int RowIndex { get; init; }
    public RowOperationType OperationType { get; init; }
    public List<ValidationError> ValidationErrors { get; init; } = new();
    public int AffectedRows { get; init; }
    public Dictionary<string, object?>? Context { get; init; }
}

/// <summary>
/// DOCUMENTATION: Row operation types
/// </summary>
public enum RowOperationType
{
    Add,
    Delete,
    Modify,
    Move,
    Clear
}

/// <summary>
/// DOCUMENTATION: Row changed event arguments
/// </summary>
public class RowChangedEventArgs : EventArgs
{
    public int RowIndex { get; set; }
    public Dictionary<string, object?>? RowData { get; set; }
    public Dictionary<string, object?>? OldData { get; set; }
}

/// <summary>
/// DOCUMENTATION: Row validation event arguments
/// </summary>
public class RowValidationEventArgs : EventArgs
{
    public int RowIndex { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
}

#endregion