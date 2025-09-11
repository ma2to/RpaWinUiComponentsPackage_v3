using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Management;

/// <summary>
/// ENTERPRISE: Advanced DataGrid Sort System per documentation
/// CLEAN ARCHITECTURE: Sort domain logic following documentation standards  
/// PERFORMANCE: Intelligent algorithm selection for 10M+ row datasets
/// DOCUMENTATION: Aligned with professional documentation architecture
/// </summary>
public sealed partial class DataGrid
{
    #region ENTERPRISE: High-Performance Sorting System

    /// <summary>
    /// PERFORMANCE: Apply single column sorting with intelligent algorithms
    /// ENTERPRISE: Optimized for large datasets with virtualization support
    /// </summary>
    public async Task<Result<SortResult>> SortByColumnAsync(
        string columnName,
        SortDirection direction = SortDirection.Ascending,
        bool maintainSelection = true,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (string.IsNullOrWhiteSpace(columnName))
            return Result<SortResult>.Failure("Column name cannot be empty");

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(2);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔄 SORT: Starting single column sort - Column: '{ColumnName}', Direction: {Direction}, Maintain selection: {MaintainSelection}", 
                    columnName, direction, maintainSelection);

                var result = await _sortService.SortByColumnAsync(columnName, direction, maintainSelection, effectiveTimeout);
                
                if (result.IsSuccess && result.Value != null)
                {
                    _logger.LogInformation("✅ SORT: Sorted {SortedRows} rows by '{ColumnName}' {Direction} in {Duration}ms using {Algorithm}", 
                        result.Value.SortedRows, columnName, direction,
                        result.Value.SortDuration.TotalMilliseconds,
                        result.Value.UsedAlgorithm);

                    // Refresh UI if available
                    if (_uiService != null)
                    {
                        await _uiService.RefreshAsync();
                    }
                }
                else
                {
                    _logger.LogWarning("❌ SORT: Sort failed for column '{ColumnName}': {Error}", columnName, result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Sort operation failed for column '{columnName}'";
            _logger.LogError(ex, "💥 SORT: {ErrorMessage}", errorMessage);
            return Result<SortResult>.Failure(ex);
        }
    }

    /// <summary>
    /// ADVANCED: Multi-level sorting with priority ordering
    /// ENTERPRISE: Complex sorting scenarios (e.g., Name ASC, Date DESC, Priority ASC)
    /// </summary>
    public async Task<Result<SortResult>> ApplyMultiLevelSortAsync(
        IReadOnlyList<SortCriteria> sortCriteria,
        bool maintainSelection = true,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (sortCriteria == null || sortCriteria.Count == 0)
            return Result<SortResult>.Failure("Sort criteria cannot be null or empty");

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                var sortSummary = string.Join(", ", sortCriteria.Select(c => $"{c.ColumnName} {c.Direction}"));
                _logger.LogTrace("🔄 SORT: Starting multi-level sort - Criteria: [{SortSummary}], Maintain selection: {MaintainSelection}", 
                    sortSummary, maintainSelection);

                var result = await _sortService.ApplyMultiLevelSortAsync(sortCriteria, maintainSelection, effectiveTimeout);
                
                if (result.IsSuccess && result.Value != null)
                {
                    _logger.LogInformation("✅ SORT: Multi-level sort completed - {SortedRows} rows sorted by {CriteriaCount} criteria in {Duration}ms", 
                        result.Value.SortedRows, sortCriteria.Count, result.Value.SortDuration.TotalMilliseconds);

                    // Refresh UI if available
                    if (_uiService != null)
                    {
                        await _uiService.RefreshAsync();
                    }
                }
                else
                {
                    _logger.LogWarning("❌ SORT: Multi-level sort failed: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Multi-level sort operation failed";
            _logger.LogError(ex, "💥 SORT: {ErrorMessage}", errorMessage);
            return Result<SortResult>.Failure(ex);
        }
    }

    /// <summary>
    /// PERFORMANCE: Clear all sorting and return to natural order
    /// MEMORY: Efficient cleanup and state reset
    /// </summary>
    public async Task<Result<bool>> ClearSortingAsync(TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔄 SORT: Clearing all sorting");

                var result = await _sortService.ClearSortingAsync();
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("✅ SORT: All sorting cleared, returned to natural order");

                    // Refresh UI if available
                    if (_uiService != null)
                    {
                        await _uiService.RefreshAsync();
                    }
                }
                else
                {
                    _logger.LogWarning("❌ SORT: Failed to clear sorting: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to clear sorting";
            _logger.LogError(ex, "💥 SORT: {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// ENTERPRISE: Get current sorting state
    /// INTROSPECTION: Allow external components to query sort state
    /// </summary>
    public async Task<Result<CurrentSortState>> GetSortStateAsync(TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔄 SORT: Getting current sort state");

                var result = await _sortService.GetSortStateAsync();
                
                if (result.IsSuccess && result.Value != null)
                {
                    _logger.LogDebug("✅ SORT: Retrieved sort state - IsSorted: {IsSorted}, Active sorts: {ActiveSortCount}", 
                        result.Value.IsSorted, result.Value.ActiveSorts.Count);
                }
                else
                {
                    _logger.LogWarning("❌ SORT: Failed to get sort state: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to get sort state";
            _logger.LogError(ex, "💥 SORT: {ErrorMessage}", errorMessage);
            return Result<CurrentSortState>.Failure(ex);
        }
    }

    /// <summary>
    /// PERFORMANCE: Get sorting statistics and performance metrics
    /// MONITORING: Sort operation analysis
    /// </summary>
    public async Task<Result<SortStatistics>> GetSortStatisticsAsync(TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔄 SORT: Getting sort statistics");

                var result = await _sortService.GetSortStatisticsAsync();
                
                if (result.IsSuccess && result.Value != null)
                {
                    _logger.LogDebug("✅ SORT: Retrieved sort statistics - Total sorts: {TotalSorts}, Avg time: {AvgTime}ms", 
                        result.Value.TotalSorts, result.Value.AverageSortTime.TotalMilliseconds);
                }
                else
                {
                    _logger.LogWarning("❌ SORT: Failed to get sort statistics: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to get sort statistics";
            _logger.LogError(ex, "💥 SORT: {ErrorMessage}", errorMessage);
            return Result<SortStatistics>.Failure(ex);
        }
    }

    #endregion
}