using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases;

/// <summary>
/// ENTERPRISE: Main DataGrid class with shared infrastructure for partial classes
/// CLEAN ARCHITECTURE: Core infrastructure for specialized partial classes
/// DDD: Aggregate root for DataGrid operations
/// </summary>
internal sealed partial class DataGrid : IDisposable
{
    #region ENTERPRISE: Core Infrastructure
    
    private readonly ILogger _logger;
    private bool _disposed = false;
    private bool _isInitialized = false;
    
    // Core services - will be injected through factory or constructor
    private readonly IDataGridService _dataGridService;
    private readonly Internal.Application.Services.IDataGridUIService? _uiService;
    private readonly IDataGridFilterService _filterService;
    private readonly IDataGridSearchService _searchService;
    private readonly IDataGridSortService _sortService;
    private readonly Internal.Application.Services.IDataGridValidationService _validationService;
    
    #endregion
    
    #region ENTERPRISE: Constructor and Factory
    
    /// <summary>
    /// Internal constructor - use factory methods instead
    /// </summary>
    private DataGrid(
        IDataGridService dataGridService,
        Internal.Application.Services.IDataGridUIService? uiService,
        IDataGridFilterService filterService,
        IDataGridSearchService searchService,
        IDataGridSortService sortService,
        Internal.Application.Services.IDataGridValidationService validationService,
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
    internal static DataGrid Create(
        IDataGridService dataGridService,
        Internal.Application.Services.IDataGridUIService? uiService,
        IDataGridFilterService filterService,
        IDataGridSearchService searchService,
        IDataGridSortService sortService,
        Internal.Application.Services.IDataGridValidationService validationService,
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
    /// LIFECYCLE: Initialize DataGrid using nomadic/monadic pattern
    /// FUNCTIONAL: Uses Result<T> pattern for railway-oriented programming
    /// </summary>
    internal async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridConfiguration? configuration = null)
    {
        if (_disposed) return Result<bool>.Failure("DataGrid has been disposed");
        
        // NOMADIC PATTERN: Use Result<T>.TryAsync for automatic exception wrapping
        var operationResult = await Result<bool>.TryAsync(async () =>
        {
            var result = await _dataGridService.InitializeAsync(columns, null, null, null);
            
            // RAILWAY-ORIENTED: Process the result and set internal state
            if (result.IsSuccess && result.Value)
            {
                _isInitialized = true;
                _logger.LogInformation("✅ CORE: DataGrid initialized successfully with {ColumnCount} columns", columns.Count);
                return true;
            }
            else
            {
                var error = result.IsFailure ? result.Error : "DataGrid initialization returned false";
                _logger.LogError("❌ CORE: DataGrid initialization failed: {Error}", error);
                throw new InvalidOperationException(error);
            }
        });
        
        // NOMADIC: Side effects for logging
        return operationResult
            .OnSuccess(success => _logger.LogDebug("DataGrid initialization pipeline completed successfully"))
            .OnFailure((error, ex) => _logger.LogError(ex, "💥 CORE: DataGrid initialization pipeline failed: {Error}", error));
    }
    
    #endregion
    
    #region ENTERPRISE: Dispose Pattern
    
    /// <summary>
    /// ENTERPRISE: Clean disposal of resources using safe disposal pattern
    /// SENIOR DEVELOPER: Each resource disposed independently to prevent cascading failures
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // SENIOR PATTERN: Dispose each resource safely and independently
            SafeDispose(_dataGridService, "DataGridService");
            SafeDispose(_uiService, "UIService");
            
            _logger?.LogDebug("🗑️ CORE: DataGrid disposed successfully");
            _disposed = true;
        }
    }
    
    /// <summary>
    /// SENIOR DEVELOPER: Safe disposal helper to prevent exception cascading
    /// </summary>
    private void SafeDispose(IDisposable? disposable, string resourceName)
    {
        if (disposable == null) return;
        
        try
        {
            disposable.Dispose();
        }
        catch (Exception ex)
        {
            // Log but don't throw - disposal should never fail the entire disposal process
            _logger?.LogWarning(ex, "⚠️ CORE: Error disposing {ResourceName}", resourceName);
        }
    }
    
    #endregion
}