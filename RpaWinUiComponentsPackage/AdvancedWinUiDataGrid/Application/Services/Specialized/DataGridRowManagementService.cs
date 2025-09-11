using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.Specialized;

/// <summary>
/// SOLID: Single Responsibility - Row lifecycle management only
/// DDD: Domain Service for row operations (CRUD)
/// CLEAN ARCHITECTURE: Application layer service
/// ENTERPRISE: Comprehensive row management with validation and constraints
/// </summary>
public sealed class DataGridRowManagementService : IDataGridRowManagementService
{
    #region Private Fields
    
    private readonly IDataGridValidationService _validationService;
    private readonly ILogger? _logger;
    private readonly RowManagementConfiguration _configuration;
    
    #endregion

    #region Constructor
    
    public DataGridRowManagementService(
        IDataGridValidationService validationService,
        RowManagementConfiguration? configuration = null,
        ILogger<DataGridRowManagementService>? logger = null)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _configuration = configuration ?? RowManagementConfiguration.Default;
        _logger = logger;
    }
    
    #endregion

    #region Row Addition Operations
    
    /// <summary>
    /// ENTERPRISE: Add new row with comprehensive validation
    /// BUSINESS RULES: Respects row limits and constraints
    /// </summary>
    public async Task<Result<bool>> AddRowAsync(
        GridState currentState,
        AddRowCommand command)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized before adding rows");

        try
        {
            _logger?.LogInformation("Adding new row at position {Position}", command.InsertIndex ?? currentState.Rows.Count);
            var stopwatch = Stopwatch.StartNew();

            // 1. BUSINESS RULES: Check row limits
            var limitsCheck = ValidateRowLimits(currentState, 1);
            if (!limitsCheck.IsSuccess)
                return limitsCheck;

            // 2. VALIDATION: Pre-add validation if required
            if (command.ValidateBeforeAdd)
            {
                var validationResult = await ValidateRowDataAsync(command.RowData, currentState.Columns);
                if (!validationResult.IsSuccess)
                {
                    _logger?.LogWarning("Row validation failed: {Error}", validationResult.Error);
                    return Result<bool>.Failure($"Row validation failed: {validationResult.Error}");
                }
            }

            // 3. ROW CREATION: Create new row with proper indexing
            var newRow = CreateNewRow(command.RowData, currentState);
            
            // 4. INSERTION: Add row at specified position
            var insertIndex = command.InsertIndex ?? currentState.Rows.Count;
            if (insertIndex >= 0 && insertIndex <= currentState.Rows.Count)
            {
                currentState.Rows.Insert(insertIndex, newRow);
                
                // Update indices for subsequent rows
                UpdateRowIndicesAfterInsertion(currentState, insertIndex);
            }
            else
            {
                return Result<bool>.Failure($"Invalid insert position: {insertIndex}");
            }

            // 5. STATE UPDATE: Update checkboxes if needed
            UpdateCheckboxStateAfterAddition(currentState, newRow.Index);

            stopwatch.Stop();
            _logger?.LogInformation("Row added successfully in {ElapsedMs}ms. Total rows: {TotalRows}", 
                stopwatch.ElapsedMilliseconds, currentState.Rows.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Add row operation failed");
            return Result<bool>.Failure($"Add row failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Add multiple rows with batch validation
    /// PERFORMANCE: Optimized for bulk operations
    /// </summary>
    public async Task<Result<int>> AddRowsAsync(
        GridState currentState,
        IReadOnlyList<Dictionary<string, object?>> rowsData,
        bool validateBeforeAdd = true)
    {
        if (currentState == null)
            return Result<int>.Failure("DataGrid must be initialized before adding rows");

        try
        {
            _logger?.LogInformation("Adding {RowCount} rows in batch", rowsData.Count);
            var stopwatch = Stopwatch.StartNew();

            // 1. BUSINESS RULES: Check row limits
            var limitsCheck = ValidateRowLimits(currentState, rowsData.Count);
            if (!limitsCheck.IsSuccess)
                return Result<int>.Failure(limitsCheck.Error!);

            // 2. BATCH VALIDATION: Validate all rows if required
            if (validateBeforeAdd)
            {
                var batchValidationResult = await ValidateMultipleRowsAsync(rowsData, currentState.Columns);
                if (!batchValidationResult.IsSuccess)
                {
                    _logger?.LogWarning("Batch validation failed: {Error}", batchValidationResult.Error);
                    return Result<int>.Failure($"Batch validation failed: {batchValidationResult.Error}");
                }
            }

            // 3. BATCH CREATION: Create all rows
            var newRows = rowsData.Select(rowData => CreateNewRow(rowData, currentState)).ToList();
            
            // 4. BATCH INSERTION: Add all rows
            currentState.Rows.AddRange(newRows);
            
            // 5. STATE UPDATE: Update checkboxes for all new rows
            foreach (var row in newRows)
            {
                UpdateCheckboxStateAfterAddition(currentState, row.Index);
            }

            stopwatch.Stop();
            _logger?.LogInformation("Batch add completed in {ElapsedMs}ms. Added {AddedCount} rows. Total: {TotalRows}", 
                stopwatch.ElapsedMilliseconds, newRows.Count, currentState.Rows.Count);

            return Result<int>.Success(newRows.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Batch add rows operation failed");
            return Result<int>.Failure($"Batch add rows failed: {ex.Message}");
        }
    }

    #endregion

    #region Row Deletion Operations
    
    /// <summary>
    /// ENTERPRISE: Delete row with validation and confirmation
    /// BUSINESS RULES: Respects minimum row constraints
    /// </summary>
    public async Task<Result<bool>> DeleteRowAsync(
        GridState currentState,
        DeleteRowCommand command)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized before deleting rows");

        try
        {
            _logger?.LogInformation("Deleting row at index {RowIndex}", command.RowIndex);
            var stopwatch = Stopwatch.StartNew();

            // 1. VALIDATION: Check if row exists
            if (command.RowIndex < 0 || command.RowIndex >= currentState.Rows.Count)
                return Result<bool>.Failure($"Row index {command.RowIndex} is out of range");

            // 2. BUSINESS RULES: Check minimum row constraints
            var constraintsCheck = ValidateMinimumRowConstraints(currentState);
            if (!constraintsCheck.IsSuccess)
                return constraintsCheck;

            // 3. CONFIRMATION: Handle confirmation if required
            if (command.RequireConfirmation && _configuration.ConfirmDelete)
            {
                _logger?.LogDebug("Delete confirmation would be required for row {RowIndex}", command.RowIndex);
                // In a real implementation, this would trigger UI confirmation
                // For now, we assume confirmation is granted
            }

            // 4. SMART DELETE: Handle smart deletion logic
            if (command.SmartDelete)
            {
                var smartDeleteResult = await PerformSmartDeleteAsync(currentState, command.RowIndex);
                if (!smartDeleteResult.IsSuccess)
                    return smartDeleteResult;
            }

            // 5. DELETION: Remove the row
            var deletedRow = currentState.Rows[command.RowIndex];
            currentState.Rows.RemoveAt(command.RowIndex);

            // 6. STATE UPDATE: Update indices and cleanup
            UpdateRowIndicesAfterDeletion(currentState, command.RowIndex);
            UpdateCheckboxStateAfterDeletion(currentState, deletedRow.Index);
            UpdateFilteredIndicesAfterDeletion(currentState, command.RowIndex);

            stopwatch.Stop();
            _logger?.LogInformation("Row deleted successfully in {ElapsedMs}ms. Remaining rows: {TotalRows}", 
                stopwatch.ElapsedMilliseconds, currentState.Rows.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Delete row operation failed");
            return Result<bool>.Failure($"Delete row failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Delete multiple rows with batch processing
    /// PERFORMANCE: Optimized bulk deletion
    /// </summary>
    public async Task<Result<int>> DeleteRowsAsync(
        GridState currentState,
        IReadOnlyList<int> rowIndices,
        bool requireConfirmation = true)
    {
        if (currentState == null)
            return Result<int>.Failure("DataGrid must be initialized before deleting rows");

        try
        {
            _logger?.LogInformation("Deleting {RowCount} rows in batch", rowIndices.Count);
            var stopwatch = Stopwatch.StartNew();

            // 1. VALIDATION: Sort and validate indices
            var sortedIndices = rowIndices.OrderByDescending(i => i).ToList();
            foreach (var index in sortedIndices)
            {
                if (index < 0 || index >= currentState.Rows.Count)
                    return Result<int>.Failure($"Row index {index} is out of range");
            }

            // 2. BUSINESS RULES: Check minimum row constraints after deletion
            var finalRowCount = currentState.Rows.Count - sortedIndices.Count;
            if (finalRowCount < _configuration.EffectiveMinRows)
            {
                return Result<int>.Failure($"Cannot delete rows: would violate minimum row constraint ({_configuration.EffectiveMinRows})");
            }

            // 3. BATCH DELETION: Delete in reverse order to maintain indices
            var deletedCount = 0;
            foreach (var index in sortedIndices)
            {
                var deletedRow = currentState.Rows[index];
                currentState.Rows.RemoveAt(index);
                
                // Update state immediately for each deletion
                UpdateCheckboxStateAfterDeletion(currentState, deletedRow.Index);
                deletedCount++;
            }

            // 4. STATE UPDATE: Update all indices and filtered state
            ReindexAllRows(currentState);
            UpdateFilteredIndicesAfterBatchDeletion(currentState, sortedIndices);

            stopwatch.Stop();
            _logger?.LogInformation("Batch delete completed in {ElapsedMs}ms. Deleted {DeletedCount} rows. Remaining: {TotalRows}", 
                stopwatch.ElapsedMilliseconds, deletedCount, currentState.Rows.Count);

            return Result<int>.Success(deletedCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Batch delete rows operation failed");
            return Result<int>.Failure($"Batch delete rows failed: {ex.Message}");
        }
    }

    #endregion

    #region Row Update Operations
    
    /// <summary>
    /// ENTERPRISE: Update existing row with validation
    /// BUSINESS RULES: Maintains data integrity
    /// </summary>
    public async Task<Result<bool>> UpdateRowAsync(
        GridState currentState,
        UpdateRowCommand command)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized before updating rows");

        try
        {
            _logger?.LogInformation("Updating row at index {RowIndex}", command.RowIndex);
            var stopwatch = Stopwatch.StartNew();

            // 1. VALIDATION: Check if row exists
            if (command.RowIndex < 0 || command.RowIndex >= currentState.Rows.Count)
                return Result<bool>.Failure($"Row index {command.RowIndex} is out of range");

            var existingRow = currentState.Rows[command.RowIndex];

            // 2. VALIDATION: Validate new data if required
            if (command.ValidateAfterUpdate)
            {
                var validationResult = await ValidateRowDataAsync(command.NewData, currentState.Columns);
                if (!validationResult.IsSuccess)
                {
                    _logger?.LogWarning("Row update validation failed: {Error}", validationResult.Error);
                    return Result<bool>.Failure($"Row update validation failed: {validationResult.Error}");
                }
            }

            // 3. UPDATE: Apply changes to existing row
            var updatedRow = UpdateRowData(existingRow, command.NewData, currentState.Columns);
            currentState.Rows[command.RowIndex] = updatedRow;

            // 4. STATE UPDATE: Clear any cached validation results for this row
            ClearRowValidationCache(currentState, command.RowIndex);

            stopwatch.Stop();
            _logger?.LogInformation("Row updated successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Update row operation failed");
            return Result<bool>.Failure($"Update row failed: {ex.Message}");
        }
    }

    #endregion

    #region Row Reordering Operations
    
    /// <summary>
    /// ENTERPRISE: Move row to new position
    /// UI: Support for drag-and-drop reordering
    /// </summary>
    public async Task<Result<bool>> MoveRowAsync(
        GridState currentState,
        int sourceIndex,
        int targetIndex)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized before moving rows");

        try
        {
            // 1. CONFIGURATION: Check if reordering is allowed
            if (!_configuration.AllowReorderRows)
                return Result<bool>.Failure("Row reordering is not allowed by current configuration");

            // 2. VALIDATION: Check indices
            if (sourceIndex < 0 || sourceIndex >= currentState.Rows.Count)
                return Result<bool>.Failure($"Source index {sourceIndex} is out of range");
            
            if (targetIndex < 0 || targetIndex >= currentState.Rows.Count)
                return Result<bool>.Failure($"Target index {targetIndex} is out of range");

            if (sourceIndex == targetIndex)
                return Result<bool>.Success(true); // No-op

            _logger?.LogInformation("Moving row from index {SourceIndex} to {TargetIndex}", sourceIndex, targetIndex);

            // 3. MOVE OPERATION: Perform the move
            var rowToMove = currentState.Rows[sourceIndex];
            currentState.Rows.RemoveAt(sourceIndex);
            currentState.Rows.Insert(targetIndex, rowToMove);

            // 4. STATE UPDATE: Update filtered indices if needed
            UpdateFilteredIndicesAfterMove(currentState, sourceIndex, targetIndex);

            _logger?.LogInformation("Row moved successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Move row operation failed");
            return Result<bool>.Failure($"Move row failed: {ex.Message}");
        }
    }

    #endregion

    #region Validation and Helper Methods

    private Result<bool> ValidateRowLimits(GridState currentState, int additionalRows)
    {
        var newRowCount = currentState.Rows.Count + additionalRows;
        
        if (newRowCount > _configuration.EffectiveMaxRows && _configuration.EffectiveMaxRows != int.MaxValue)
        {
            return Result<bool>.Failure($"Cannot add rows: would exceed maximum row limit ({_configuration.EffectiveMaxRows})");
        }

        return Result<bool>.Success(true);
    }

    private Result<bool> ValidateMinimumRowConstraints(GridState currentState)
    {
        if (currentState.Rows.Count <= _configuration.EffectiveMinRows)
        {
            return Result<bool>.Failure($"Cannot delete row: minimum row count ({_configuration.EffectiveMinRows}) would be violated");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateRowDataAsync(
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns)
    {
        // Validate required fields
        foreach (var column in columns.Where(c => c.IsRequired))
        {
            if (!rowData.ContainsKey(column.Name) || rowData[column.Name] == null)
            {
                return Result<bool>.Failure($"Required field '{column.Name}' is missing or null");
            }
        }

        // Validate data types
        foreach (var kvp in rowData)
        {
            var column = columns.FirstOrDefault(c => c.Name == kvp.Key);
            if (column != null && kvp.Value != null)
            {
                if (!IsValidDataType(kvp.Value, column.DataType))
                {
                    return Result<bool>.Failure($"Invalid data type for column '{kvp.Key}'");
                }
            }
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateMultipleRowsAsync(
        IReadOnlyList<Dictionary<string, object?>> rowsData,
        IReadOnlyList<ColumnDefinition> columns)
    {
        for (int i = 0; i < rowsData.Count; i++)
        {
            var validationResult = await ValidateRowDataAsync(rowsData[i], columns);
            if (!validationResult.IsSuccess)
            {
                return Result<bool>.Failure($"Row {i + 1}: {validationResult.Error}");
            }
        }

        return Result<bool>.Success(true);
    }

    private GridRow CreateNewRow(Dictionary<string, object?> rowData, GridState currentState)
    {
        var newIndex = currentState.Rows.Count > 0 
            ? currentState.Rows.Max(r => r.Index) + 1 
            : 0;

        var gridRow = new GridRow(newIndex)
        {
            RowIndex = newIndex,
            IsSelected = false
        };
        
        gridRow.SetAllData(rowData);
        return gridRow;
    }

    private GridRow UpdateRowData(
        GridRow existingRow, 
        Dictionary<string, object?> newData,
        IReadOnlyList<ColumnDefinition> columns)
    {
        var updatedData = new Dictionary<string, object?>(existingRow.Data);
        
        foreach (var kvp in newData)
        {
            var column = columns.FirstOrDefault(c => c.Name == kvp.Key);
            if (column != null && !column.IsReadOnly)
            {
                updatedData[kvp.Key] = kvp.Value;
            }
        }

        // Update existing row data
        foreach (var kvp in updatedData)
        {
            existingRow.SetValue(kvp.Key, kvp.Value);
        }
        return existingRow;
    }

    private async Task<Result<bool>> PerformSmartDeleteAsync(GridState currentState, int rowIndex)
    {
        // Smart delete logic - could include:
        // - Checking for dependencies
        // - Cascade deletions
        // - Soft delete vs hard delete
        // For now, just standard validation
        
        var row = currentState.Rows[rowIndex];
        
        // Check if row has validation errors (maybe warn user)
        if (row.ValidationErrors.Any())
        {
            _logger?.LogDebug("Deleting row with validation errors - row index {RowIndex}", rowIndex);
        }

        return Result<bool>.Success(true);
    }

    private bool IsValidDataType(object value, Type expectedType)
    {
        if (value == null)
            return true; // Nulls are handled separately

        var valueType = value.GetType();
        return expectedType.IsAssignableFrom(valueType) || 
               (Nullable.GetUnderlyingType(expectedType) ?? expectedType).IsAssignableFrom(valueType);
    }

    #region State Update Methods

    private void UpdateRowIndicesAfterInsertion(GridState currentState, int insertIndex)
    {
        for (int i = insertIndex + 1; i < currentState.Rows.Count; i++)
        {
            currentState.Rows[i].RowIndex = i;
        }
    }

    private void UpdateRowIndicesAfterDeletion(GridState currentState, int deletedIndex)
    {
        for (int i = deletedIndex; i < currentState.Rows.Count; i++)
        {
            currentState.Rows[i].RowIndex = i;
        }
    }

    private void ReindexAllRows(GridState currentState)
    {
        for (int i = 0; i < currentState.Rows.Count; i++)
        {
            currentState.Rows[i].RowIndex = i;
        }
    }

    private void UpdateCheckboxStateAfterAddition(GridState currentState, int newRowIndex)
    {
        // Initialize checkbox state for new row
        if (!currentState.CheckboxStates.ContainsKey(newRowIndex))
        {
            currentState.CheckboxStates[newRowIndex] = false;
        }
    }

    private void UpdateCheckboxStateAfterDeletion(GridState currentState, int deletedRowIndex)
    {
        // Remove checkbox state for deleted row
        currentState.CheckboxStates.Remove(deletedRowIndex);
    }

    private void UpdateFilteredIndicesAfterDeletion(GridState currentState, int deletedIndex)
    {
        if (currentState.FilteredRowIndices != null)
        {
            // Remove deleted index and adjust remaining indices
            currentState.FilteredRowIndices.Remove(deletedIndex);
            for (int i = 0; i < currentState.FilteredRowIndices.Count; i++)
            {
                if (currentState.FilteredRowIndices[i] > deletedIndex)
                {
                    currentState.FilteredRowIndices[i]--;
                }
            }
        }
    }

    private void UpdateFilteredIndicesAfterBatchDeletion(GridState currentState, IReadOnlyList<int> deletedIndices)
    {
        if (currentState.FilteredRowIndices != null)
        {
            // Remove all deleted indices
            foreach (var deletedIndex in deletedIndices)
            {
                currentState.FilteredRowIndices.Remove(deletedIndex);
            }
            
            // Adjust remaining indices
            foreach (var deletedIndex in deletedIndices.OrderBy(i => i))
            {
                for (int i = 0; i < currentState.FilteredRowIndices.Count; i++)
                {
                    if (currentState.FilteredRowIndices[i] > deletedIndex)
                    {
                        currentState.FilteredRowIndices[i]--;
                    }
                }
            }
        }
    }

    private void UpdateFilteredIndicesAfterMove(GridState currentState, int sourceIndex, int targetIndex)
    {
        if (currentState.FilteredRowIndices != null)
        {
            // Update filtered indices to reflect the move
            for (int i = 0; i < currentState.FilteredRowIndices.Count; i++)
            {
                var currentIndex = currentState.FilteredRowIndices[i];
                if (currentIndex == sourceIndex)
                {
                    currentState.FilteredRowIndices[i] = targetIndex;
                }
                else if (sourceIndex < targetIndex && currentIndex > sourceIndex && currentIndex <= targetIndex)
                {
                    currentState.FilteredRowIndices[i]--;
                }
                else if (sourceIndex > targetIndex && currentIndex >= targetIndex && currentIndex < sourceIndex)
                {
                    currentState.FilteredRowIndices[i]++;
                }
            }
        }
    }

    private void ClearRowValidationCache(GridState currentState, int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < currentState.Rows.Count)
        {
            var row = currentState.Rows[rowIndex];
            if (row.ValidationErrors.Any() || row.ValidationErrorObjects.Any())
            {
                row.ClearValidationErrors();
            }
        }
    }

    #endregion

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _logger?.LogDebug("DataGridRowManagementService disposed");
    }

    #endregion
}

/// <summary>
/// SOLID: Interface segregation for Row Management operations
/// </summary>
public interface IDataGridRowManagementService : IDisposable
{
    // Row addition
    Task<Result<bool>> AddRowAsync(GridState currentState, AddRowCommand command);
    Task<Result<int>> AddRowsAsync(GridState currentState, IReadOnlyList<Dictionary<string, object?>> rowsData, bool validateBeforeAdd = true);
    
    // Row deletion
    Task<Result<bool>> DeleteRowAsync(GridState currentState, DeleteRowCommand command);
    Task<Result<int>> DeleteRowsAsync(GridState currentState, IReadOnlyList<int> rowIndices, bool requireConfirmation = true);
    
    // Row updates
    Task<Result<bool>> UpdateRowAsync(GridState currentState, UpdateRowCommand command);
    
    // Row reordering
    Task<Result<bool>> MoveRowAsync(GridState currentState, int sourceIndex, int targetIndex);
}