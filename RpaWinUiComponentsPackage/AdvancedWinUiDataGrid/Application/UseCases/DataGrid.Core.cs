using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Management;

/// <summary>
/// ENTERPRISE: Main DataGrid class with shared infrastructure for partial classes
/// CLEAN ARCHITECTURE: Core infrastructure for specialized partial classes
/// DDD: Aggregate root for DataGrid operations
/// </summary>
public sealed partial class DataGrid : IDisposable
{
    #region ENTERPRISE: Core Infrastructure
    
    private readonly ILogger _logger;
    private bool _disposed = false;
    private bool _isInitialized = false;
    
    // Core services - will be injected through factory or constructor
    private readonly IDataGridService _dataGridService;
    private readonly Application.Services.IDataGridUIService? _uiService;
    private readonly IDataGridFilterService _filterService;
    private readonly IDataGridSearchService _searchService;
    private readonly IDataGridSortService _sortService;
    private readonly Application.Services.IDataGridValidationService _validationService;
    
    #endregion
    
    #region ENTERPRISE: Constructor and Factory
    
    /// <summary>
    /// Internal constructor - use factory methods instead
    /// </summary>
    private DataGrid(
        IDataGridService dataGridService,
        Application.Services.IDataGridUIService? uiService,
        IDataGridFilterService filterService,
        IDataGridSearchService searchService,
        IDataGridSortService sortService,
        Application.Services.IDataGridValidationService validationService,
        ILogger logger)
    {
        _dataGridService = dataGridService ?? throw new ArgumentNullException(nameof(dataGridService));
        _uiService = uiService;
        _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService));
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _sortService = sortService ?? throw new ArgumentNullException(nameof(sortService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// FACTORY: Create DataGrid instance with full services
    /// </summary>
    public static DataGrid Create(
        IDataGridService dataGridService,
        Application.Services.IDataGridUIService? uiService,
        IDataGridFilterService filterService,
        IDataGridSearchService searchService,
        IDataGridSortService sortService,
        Application.Services.IDataGridValidationService validationService,
        ILogger logger)
    {
        return new DataGrid(dataGridService, uiService, filterService, searchService, sortService, validationService, logger);
    }
    
    #endregion
    
    #region ENTERPRISE: Core Infrastructure Methods
    
    /// <summary>
    /// FUNCTIONAL: Execute operation with timeout protection
    /// ENTERPRISE: Consistent timeout handling across all operations
    /// </summary>
    private async Task<Result<T>> ExecuteWithTimeoutAsync<T>(Func<Task<Result<T>>> operation, TimeSpan timeout)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            var task = operation();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
            
            if (completedTask == task)
            {
                cts.Cancel(); // Cancel the delay task
                return await task;
            }
            else
            {
                return Result<T>.Failure($"Operation timed out after {timeout.TotalSeconds} seconds");
            }
        }
        catch (OperationCanceledException)
        {
            return Result<T>.Failure($"Operation was cancelled due to timeout ({timeout.TotalSeconds}s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 CORE: Unexpected error in ExecuteWithTimeoutAsync");
            return Result<T>.Failure("Unexpected error during operation", ex);
        }
    }
    
    /// <summary>
    /// QUERY: Get current row count from data service
    /// </summary>
    private int GetRowsCount()
    {
        if (_disposed || !_isInitialized) return 0;
        return _dataGridService?.GetRowCount() ?? 0;
    }
    
    /// <summary>
    /// QUERY: Get current column count from data service
    /// </summary>
    private int GetColumnsCount()
    {
        if (_disposed || !_isInitialized) return 0;
        return _dataGridService?.GetColumnCount() ?? 0;
    }
    
    /// <summary>
    /// LIFECYCLE: Initialize DataGrid
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridConfiguration? configuration = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        
        try
        {
            var result = await _dataGridService.InitializeAsync(columns, null, null, null);
            if (result.IsSuccess)
            {
                _isInitialized = true;
                _logger.LogInformation("✅ CORE: DataGrid initialized successfully with {ColumnCount} columns", columns.Count);
            }
            else
            {
                _logger.LogError("❌ CORE: DataGrid initialization failed: {Error}", result.Error);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 CORE: Exception during DataGrid initialization");
            return Result<bool>.Failure("DataGrid initialization failed", ex);
        }
    }
    
    #endregion
    
    #region ENTERPRISE: Dispose Pattern
    
    /// <summary>
    /// ENTERPRISE: Clean disposal of resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _dataGridService?.Dispose();
                _uiService?.Dispose();
                // Other services implement IDisposable if needed
                
                _logger.LogDebug("🗑️ CORE: DataGrid disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ CORE: Error during DataGrid disposal");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
    
    #endregion
}