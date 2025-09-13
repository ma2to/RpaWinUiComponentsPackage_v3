using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases;

/// <summary>
/// ENTERPRISE: DataGrid row management functionality
/// ANTI-GOD-FILE: Separated row management concerns from main DataGrid class
/// SMART BEHAVIOR: Intelligent row expansion and deletion based on configuration
/// </summary>
internal sealed partial class DataGrid
{
    #region ENTERPRISE: Smart Row Management System

    private RowManagementConfiguration _rowManagementConfig = new();

    /// <summary>
    /// CONFIGURATION: Set row management behavior configuration
    /// SMART: Configure intelligent row expansion and deletion behavior
    /// </summary>
    internal async Task<Result<bool>> ConfigureRowManagementAsync(
        RowManagementConfiguration configuration,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (configuration == null)
            return Result<bool>.Failure("Row management configuration cannot be null");

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔧 ROW MANAGEMENT: Configuring row management - MinRows: {MinRows}, MaxRows: {MaxRows}, AllowDelete: {AllowDelete}", 
                    configuration.MinimumRows, configuration.MaximumRows, configuration.AllowDeleteRows);

                _rowManagementConfig = configuration;
                
                // Initialize with minimum rows if not yet initialized
                if (_isInitialized)
                {
                    await EnsureMinimumRowsAsync();
                }

                _logger.LogDebug("✅ ROW MANAGEMENT: Row management configured successfully");
                return Result<bool>.Success(true);
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to configure row management";
            _logger.LogError(ex, "💥 ROW MANAGEMENT: {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// SMART EXPANSION: Add rows automatically when data is imported/pasted
    /// BEHAVIOR: Automatic row expansion when paste/import brings more rows
    /// </summary>
    internal async Task<Result<int>> ExpandRowsAsync(
        int additionalRows,
        bool alwaysKeepEmptyRow = true,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (additionalRows <= 0)
            return Result<int>.Failure("Additional rows must be greater than zero");

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("📈 ROW EXPANSION: Expanding by {AdditionalRows} rows, KeepEmpty: {KeepEmpty}", 
                    additionalRows, alwaysKeepEmptyRow);

                var currentRowCount = GetRowsCount();
                var newRowCount = currentRowCount + additionalRows;
                
                // Check maximum row limit
                if (newRowCount > _rowManagementConfig.MaximumRows)
                {
                    var limitedRows = _rowManagementConfig.MaximumRows - currentRowCount;
                    if (limitedRows <= 0)
                    {
                        return Result<int>.Failure($"Cannot expand rows: Maximum row limit ({_rowManagementConfig.MaximumRows}) reached");
                    }
                    
                    _logger.LogWarning("⚠️ ROW EXPANSION: Limited expansion to {LimitedRows} rows due to maximum limit", limitedRows);
                    additionalRows = limitedRows;
                    newRowCount = _rowManagementConfig.MaximumRows;
                }

                // Perform the expansion by adding empty rows
                for (int i = 0; i < additionalRows; i++)
                {
                    await _dataGridService.AddRowAsync(new Dictionary<string, object?>());
                }
                
                // Always keep one empty row at the end if requested
                if (alwaysKeepEmptyRow)
                {
                    await EnsureEmptyRowAtEndAsync();
                }

                // Refresh UI if available
                if (_uiService != null)
                {
                    await _uiService.RefreshAsync();
                }

                _logger.LogInformation("✅ ROW EXPANSION: Successfully expanded from {OldCount} to {NewCount} rows", 
                    currentRowCount, newRowCount);

                return Result<int>.Success(newRowCount);
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to expand rows by {additionalRows}";
            _logger.LogError(ex, "💥 ROW EXPANSION: {ErrorMessage}", errorMessage);
            return Result<int>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// SMART DELETE: Intelligent row deletion based on row count and configuration
    /// BEHAVIOR: 
    /// - Rows > MinimumRows: DELETE = Delete complete row (keep empty row at end)
    /// - Rows <= MinimumRows: DELETE = Clear content only (preserve structure)
    /// </summary>
    internal async Task<Result<SmartDeleteResult>> SmartDeleteRowsAsync(
        IReadOnlyList<int> rowIndices,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (rowIndices == null || rowIndices.Count == 0)
            return Result<SmartDeleteResult>.Failure("Row indices cannot be null or empty");

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(1);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🗑️ SMART DELETE: Processing {RowCount} rows for smart deletion", rowIndices.Count);

                var currentRowCount = GetRowsCount();
                var sortedIndices = rowIndices.OrderByDescending(x => x).ToList(); // Delete from end to avoid index shifting
                
                int deletedRows = 0;
                int clearedRows = 0;

                foreach (var rowIndex in sortedIndices)
                {
                    if (rowIndex < 0 || rowIndex >= currentRowCount)
                    {
                        _logger.LogWarning("⚠️ SMART DELETE: Invalid row index {RowIndex}, skipping", rowIndex);
                        continue;
                    }

                    // Determine action based on current row count and configuration
                    var remainingRowsAfterDelete = currentRowCount - deletedRows - 1;
                    
                    if (remainingRowsAfterDelete > _rowManagementConfig.MinimumRows)
                    {
                        // DELETE: Remove complete row
                        await _dataGridService.DeleteRowAsync(rowIndex);
                        deletedRows++;
                        _logger.LogTrace("🗑️ SMART DELETE: Deleted row {RowIndex} (complete deletion)", rowIndex);
                    }
                    else
                    {
                        // CLEAR: Clear content only, preserve structure
                        // Clear row content by updating with empty dictionary
                        await _dataGridService.UpdateRowAsync(rowIndex, new Dictionary<string, object?>());
                        clearedRows++;
                        _logger.LogTrace("🧹 SMART DELETE: Cleared row {RowIndex} content (preserved structure)", rowIndex);
                    }
                }

                // Ensure minimum rows and empty row at end
                await EnsureMinimumRowsAsync();
                if (_rowManagementConfig.MinimumRows > 0)
                {
                    await EnsureEmptyRowAtEndAsync();
                }

                // Refresh UI if available
                if (_uiService != null)
                {
                    await _uiService.RefreshAsync();
                }

                var result = new SmartDeleteResult
                {
                    IsSuccess = true,
                    DeletedRows = deletedRows,
                    ClearedRows = clearedRows,
                    TotalProcessedRows = deletedRows + clearedRows,
                    FinalRowCount = GetRowsCount(),
                    DeletionTime = DateTime.UtcNow
                };

                _logger.LogInformation("✅ SMART DELETE: Completed - {DeletedRows} deleted, {ClearedRows} cleared, final count: {FinalCount}", 
                    deletedRows, clearedRows, result.FinalRowCount);

                return Result<SmartDeleteResult>.Success(result);
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to perform smart delete on {rowIndices.Count} rows";
            _logger.LogError(ex, "💥 SMART DELETE: {ErrorMessage}", errorMessage);
            return Result<SmartDeleteResult>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// SMART DELETE: Delete single row with intelligent behavior
    /// CONVENIENCE: Single row version of smart delete
    /// </summary>
    internal async Task<Result<SmartDeleteResult>> SmartDeleteRowAsync(
        int rowIndex,
        TimeSpan? timeout = null)
    {
        return await SmartDeleteRowsAsync(new[] { rowIndex }, timeout);
    }

    /// <summary>
    /// MAINTENANCE: Ensure minimum number of rows exists
    /// INTERNAL: Called automatically during operations
    /// </summary>
    private async Task EnsureMinimumRowsAsync()
    {
        try
        {
            var currentRowCount = GetRowsCount();
            if (currentRowCount < _rowManagementConfig.MinimumRows)
            {
                var rowsToAdd = _rowManagementConfig.MinimumRows - currentRowCount;
                _logger.LogTrace("📈 MAINTENANCE: Adding {RowsToAdd} rows to meet minimum requirement", rowsToAdd);
                
                for (int i = 0; i < rowsToAdd; i++)
                {
                    await _dataGridService.AddRowAsync(new Dictionary<string, object?>());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ MAINTENANCE: Failed to ensure minimum rows");
        }
    }

    /// <summary>
    /// MAINTENANCE: Ensure there's always one empty row at the end
    /// INTERNAL: Called automatically when AlwaysKeepEmptyRow is enabled
    /// </summary>
    private async Task EnsureEmptyRowAtEndAsync()
    {
        try
        {
            // Check if last row is empty by getting current state
            // This is a simplified check - would need proper implementation
            var isEmpty = false; // Placeholder
            if (!isEmpty)
            {
                _logger.LogTrace("📈 MAINTENANCE: Adding empty row at end");
                await _dataGridService.AddRowAsync(new Dictionary<string, object?>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ MAINTENANCE: Failed to ensure empty row at end");
        }
    }

    /// <summary>
    /// QUERY: Get current row management configuration
    /// INTROSPECTION: Allow external access to current configuration
    /// </summary>
    internal RowManagementConfiguration GetRowManagementConfiguration()
    {
        return _rowManagementConfig;
    }

    /// <summary>
    /// QUERY: Check if row can be deleted (vs cleared)
    /// UTILITY: Determine deletion behavior for specific row
    /// </summary>
    internal bool CanDeleteRow(int rowIndex)
    {
        if (_disposed || !_isInitialized) return false;
        
        var currentRowCount = GetRowsCount();
        if (rowIndex < 0 || rowIndex >= currentRowCount) return false;
        
        // Can delete if we have more than minimum rows
        return currentRowCount > _rowManagementConfig.MinimumRows;
    }

    #endregion
}

/// <summary>
/// ENTERPRISE: Smart delete operation result
/// FUNCTIONAL: Immutable result record
/// </summary>
internal record SmartDeleteResult
{
    internal bool IsSuccess { get; init; }
    internal int DeletedRows { get; init; }
    internal int ClearedRows { get; init; }
    internal int TotalProcessedRows { get; init; }
    internal int FinalRowCount { get; init; }
    internal DateTime DeletionTime { get; init; }
    internal string? ErrorMessage { get; init; }
}