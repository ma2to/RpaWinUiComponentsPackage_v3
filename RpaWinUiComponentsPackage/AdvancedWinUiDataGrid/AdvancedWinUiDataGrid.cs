using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// CLEAN ARCHITECTURE: Main DataGrid component preserving public API
/// DELEGATION: Delegates to specialized services via unified service layer
/// PRESERVES API: Maintains exact API from documentation for backward compatibility
/// ENTERPRISE: Professional component supporting both UI and headless modes
/// </summary>
public sealed class AdvancedWinUiDataGrid : IDisposable
{
    private readonly IDataGridService _service;
    private readonly ComponentLogger _componentLogger;
    private bool _disposed = false;

    private AdvancedWinUiDataGrid(IDataGridService service, ComponentLogger componentLogger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _componentLogger = componentLogger ?? throw new ArgumentNullException(nameof(componentLogger));
        
        _componentLogger.LogInformation("AdvancedWinUiDataGrid instance created successfully with service: {ServiceType}", _service.GetType().Name);
    }

    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid for UI mode with professional logging
    /// ENTERPRISE: Supports configurable logging strategies and comprehensive error tracking
    /// </summary>
    /// <param name="logger">Base logger instance (required)</param>
    /// <param name="loggingOptions">Logging configuration options (optional, uses Development defaults)</param>
    /// <returns>DataGrid instance configured for UI operations</returns>
    public static AdvancedWinUiDataGrid CreateForUI(ILogger logger, LoggingOptions? loggingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        var options = loggingOptions ?? LoggingOptions.Development;
        var componentLogger = new ComponentLogger(logger, options);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("CreateForUI started - Creating DataGrid service for UI mode");
            
            var service = DataGridAPI.CreateForUI(componentLogger);
            
            if (service == null)
            {
                throw new InvalidOperationException("DataGrid service creation failed - service is null");
            }
            
            var result = new AdvancedWinUiDataGrid(service, componentLogger);
            componentLogger.LogInformation("CreateForUI completed successfully - DataGrid instance created");
            
            return result;
        }, nameof(CreateForUI));
    }

    /// <summary>
    /// BACKWARD COMPATIBILITY: Create DataGrid for UI mode with simple logger
    /// LEGACY: Maintains compatibility with existing code that doesn't use LoggingOptions
    /// </summary>
    /// <param name="logger">Optional logger (uses NullLogger if not provided)</param>
    /// <returns>DataGrid instance with development logging settings</returns>
    public static AdvancedWinUiDataGrid CreateForUI(ILogger? logger = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        return CreateForUI(actualLogger, LoggingOptions.Development);
    }

    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid for headless mode with professional logging
    /// ENTERPRISE: Optimized for automation and high-volume processing with performance logging
    /// </summary>
    /// <param name="logger">Base logger instance (required)</param>
    /// <param name="loggingOptions">Logging configuration options (optional, uses Production defaults for headless)</param>
    /// <returns>DataGrid instance configured for headless operations</returns>
    public static AdvancedWinUiDataGrid CreateHeadless(ILogger logger, LoggingOptions? loggingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        var options = loggingOptions ?? LoggingOptions.Production; // Production defaults for headless
        var componentLogger = new ComponentLogger(logger, options);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("CreateHeadless started - Creating DataGrid service for headless mode");
            
            var service = DataGridAPI.CreateHeadless(componentLogger);
            
            if (service == null)
            {
                throw new InvalidOperationException("DataGrid headless service creation failed - service is null");
            }
            
            var result = new AdvancedWinUiDataGrid(service, componentLogger);
            componentLogger.LogInformation("CreateHeadless completed successfully - DataGrid instance created");
            
            return result;
        }, nameof(CreateHeadless));
    }

    /// <summary>
    /// BACKWARD COMPATIBILITY: Create DataGrid for headless mode with simple logger
    /// LEGACY: Maintains compatibility with existing code that doesn't use LoggingOptions
    /// </summary>
    /// <param name="logger">Optional logger (uses NullLogger if not provided)</param>
    /// <returns>DataGrid instance with production logging settings</returns>
    public static AdvancedWinUiDataGrid CreateHeadless(ILogger? logger = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        return CreateHeadless(actualLogger, LoggingOptions.Production);
    }

    /// <summary>Get the underlying service for advanced operations</summary>
    public IDataGridService GetService() => _service;

    /// <summary>
    /// SENIOR DEVELOPER: Initialize DataGrid with comprehensive logging and error tracking
    /// ENTERPRISE: Professional initialization with Result<T> pattern integration
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null,
        int minimumRows = 1)
    {
        return await _componentLogger.ExecuteWithLoggingAsync(async () =>
        {
            _componentLogger.LogMethodEntry(nameof(InitializeAsync), columns?.Count ?? 0, minimumRows);
            
            // Validate input parameters with detailed logging
            if (columns == null || columns.Count == 0)
            {
                return _componentLogger.CreateFailureResult<bool>("Columns collection cannot be null or empty", "Parameter validation");
            }
            
            // Log configuration details if enabled
            _componentLogger.LogDebug("Configuration details - Columns: {ColumnCount}, ColorConfig: {HasColorConfig}, ValidationConfig: {HasValidationConfig}, PerformanceConfig: {HasPerformanceConfig}", 
                columns.Count, 
                colorConfiguration != null,
                validationConfiguration != null, 
                performanceConfiguration != null);
            
            // Log each column definition if detailed logging is enabled
            if (_componentLogger._options.LogMethodParameters)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    _componentLogger.LogDebug("Column[{Index}]: Name='{Name}', Type='{DataType}', Required={IsRequired}, ReadOnly={IsReadOnly}", 
                        i, column?.Name ?? "null", column?.DataType?.Name ?? "null", column?.IsRequired ?? false, column?.IsReadOnly ?? false);
                }
            }
            
            // Execute service initialization with error tracking
            var result = await _service.InitializeAsync(columns, colorConfiguration, validationConfiguration, performanceConfiguration);
            
            // Log result using Result<T> pattern integration
            _componentLogger.LogResult(result, "DataGrid initialization");
            
            return result;
            
        }, nameof(InitializeAsync));
    }

    /// <summary>Import data from dictionary collection - preserves documented API</summary>
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        return await _service.ImportFromDictionaryAsync(data, checkboxStates, startRow, mode, timeout, validationProgress);
    }

    /// <summary>Import data from DataTable - preserves documented API</summary>
    public async Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        return await _service.ImportFromDataTableAsync(dataTable, checkboxStates, startRow, mode, timeout, validationProgress);
    }

    /// <summary>Export data to dictionary collection - preserves documented API</summary>
    public async Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        return await _service.ExportToDictionaryAsync(includeValidAlerts, exportOnlyChecked, exportOnlyFiltered, removeAfter, timeout, exportProgress);
    }

    /// <summary>Export data to DataTable - preserves documented API</summary>
    public async Task<Result<DataTable>> ExportToDataTableAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        return await _service.ExportToDataTableAsync(includeValidAlerts, exportOnlyChecked, exportOnlyFiltered, removeAfter, timeout, exportProgress);
    }

    /// <summary>Search data in all or specific columns</summary>
    public async Task<Result<SearchResult>> SearchAsync(
        string searchTerm,
        SearchOptions? options = null)
    {
        return await _service.SearchAsync(searchTerm, options);
    }

    /// <summary>Apply filters to data</summary>
    public async Task<Result<bool>> ApplyFiltersAsync(IReadOnlyList<FilterExpression> filters)
    {
        return await _service.ApplyFiltersAsync(filters);
    }

    /// <summary>Sort data by column</summary>
    public async Task<Result<bool>> SortAsync(string columnName, SortDirection direction)
    {
        return await _service.SortAsync(columnName, direction);
    }

    /// <summary>Clear all applied filters</summary>
    public async Task<Result<bool>> ClearFiltersAsync()
    {
        return await _service.ClearFiltersAsync();
    }

    /// <summary>Validate all data</summary>
    public async Task<Result<ValidationError[]>> ValidateAllAsync(IProgress<ValidationProgress>? progress = null)
    {
        return await _service.ValidateAllAsync(progress);
    }

    /// <summary>Add new row</summary>
    public async Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData)
    {
        return await _service.AddRowAsync(rowData);
    }

    /// <summary>Update existing row</summary>
    public async Task<Result<bool>> UpdateRowAsync(int rowIndex, Dictionary<string, object?> rowData)
    {
        return await _service.UpdateRowAsync(rowIndex, rowData);
    }

    /// <summary>Delete row</summary>
    public async Task<Result<bool>> DeleteRowAsync(int rowIndex)
    {
        return await _service.DeleteRowAsync(rowIndex);
    }

    /// <summary>Get current row count</summary>
    public int GetRowCount() => _service.GetRowCount();

    /// <summary>Get column count</summary>
    public int GetColumnCount() => _service.GetColumnCount();

    /// <summary>
    /// PROFESSIONAL: Delete rows that meet specified validation criteria
    /// ENTERPRISE: Batch operation with progress reporting and rollback support
    /// </summary>
    /// <param name="validationCriteria">Criteria for determining which rows to delete</param>
    /// <param name="options">Deletion options including safety checks</param>
    /// <returns>Result with deletion statistics</returns>
    public async Task<Result<ValidationBasedDeleteResult>> DeleteRowsWithValidationAsync(
        ValidationDeletionCriteria validationCriteria,
        ValidationDeletionOptions? options = null)
    {
        if (_disposed) 
            return Result<ValidationBasedDeleteResult>.Failure("DataGrid has been disposed");
        
        // Delegate to service implementation
        return await _service.DeleteRowsWithValidationAsync(validationCriteria, options);
    }

    /// <summary>
    /// ENTERPRISE: Check if all non-empty rows pass validation
    /// COMPREHENSIVE: Validates complete dataset including cached/disk data
    /// </summary>
    /// <param name="onlyFiltered">If true, validate only filtered rows; if false, validate entire dataset</param>
    /// <returns>True only if no validation errors exist in any non-empty row</returns>
    public async Task<Result<bool>> AreAllNonEmptyRowsValidAsync(bool onlyFiltered = false)
    {
        if (_disposed) 
            return Result<bool>.Failure("DataGrid has been disposed");
        
        // Delegate to service implementation
        return await _service.AreAllNonEmptyRowsValidAsync(onlyFiltered);
    }

    /// <summary>Dispose resources</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _service?.Dispose();
            _disposed = true;
        }
    }
}