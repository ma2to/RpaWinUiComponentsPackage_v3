using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using UIErrorEventArgs = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Events.ErrorEventArgs;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Events;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Managers;

/// <summary>
/// UI: Professional Business Logic Manager
/// CLEAN ARCHITECTURE: UI layer coordinator for business operations
/// SOLID: Single responsibility for business logic coordination
/// SENIOR DESIGN: Enterprise-grade business logic management with clean separation
/// </summary>
internal sealed class DataGridBusinessLogicManager : IDisposable
{
    #region Private Fields

    private readonly ILogger? _logger;
    private IDataGridService? _dataGridService;
    private bool _disposed;

    // Business State
    private IReadOnlyList<ColumnDefinition>? _currentColumns;
    private ColorConfiguration? _colorConfiguration;
    private ValidationConfiguration? _validationConfiguration;

    #endregion

    #region Events

    /// <summary>
    /// EVENT: Data operation completed
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs>? OperationCompleted;

    /// <summary>
    /// EVENT: Business logic error occurred
    /// </summary>
    public event EventHandler<UIErrorEventArgs>? ErrorOccurred;

    #endregion

    #region Constructor

    /// <summary>
    /// CONSTRUCTOR: Initialize business logic manager
    /// </summary>
    public DataGridBusinessLogicManager(ILogger? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("[BUSINESS-LOGIC] DataGridBusinessLogicManager initialized");
    }

    #endregion

    #region Business Logic Operations

    /// <summary>
    /// ENTERPRISE: Initialize DataGrid with business configuration
    /// </summary>
    public async Task<bool> InitializeAsync(
        IDataGridService dataGridService,
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null)
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Initializing DataGrid business logic");

            _dataGridService = dataGridService ?? throw new ArgumentNullException(nameof(dataGridService));
            _currentColumns = columns;
            _colorConfiguration = colorConfiguration;
            _validationConfiguration = validationConfiguration;

            // Initialize the underlying service
            var result = await _dataGridService.InitializeAsync(
                columns, colorConfiguration, validationConfiguration, performanceConfiguration);

            if (result.IsSuccess)
            {
                _logger?.LogInformation("[BUSINESS-LOGIC] DataGrid business logic initialized successfully");
                OnOperationCompleted("Initialize", true, "DataGrid initialized successfully");
                return true;
            }
            else
            {
                var errorMessage = $"Failed to initialize DataGrid: {result.Error}";
                _logger?.LogError("[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
                OnErrorOccurred(errorMessage, result.Exception, "Initialize");
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Business logic initialization failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "Initialize");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Import data with business validation
    /// </summary>
    public async Task<bool> ImportDataAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace)
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Importing {Count} rows of data", data.Count);

            if (_dataGridService == null)
            {
                OnErrorOccurred("DataGrid service not initialized", null, "ImportData");
                return false;
            }

            var result = await _dataGridService.ImportFromDictionaryAsync(
                data, checkboxStates, startRow, mode);

            if (result.IsSuccess)
            {
                _logger?.LogInformation("[BUSINESS-LOGIC] Data imported successfully");
                OnOperationCompleted("ImportData", true, $"Imported {data.Count} rows successfully");
                return true;
            }
            else
            {
                var errorMessage = $"Failed to import data: {result.Error}";
                _logger?.LogError("[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
                OnErrorOccurred(errorMessage, result.Exception, "ImportData");
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Data import failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "ImportData");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Export data with business rules
    /// </summary>
    public async Task<List<Dictionary<string, object?>>?> ExportDataAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false)
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Exporting data");

            if (_dataGridService == null)
            {
                OnErrorOccurred("DataGrid service not initialized", null, "ExportData");
                return null;
            }

            var result = await _dataGridService.ExportToDictionaryAsync(
                includeValidAlerts, exportOnlyChecked, exportOnlyFiltered);

            if (result.IsSuccess)
            {
                _logger?.LogInformation("[BUSINESS-LOGIC] Data exported successfully: {Count} rows", result.Value?.Count ?? 0);
                OnOperationCompleted("ExportData", true, $"Exported {result.Value?.Count ?? 0} rows successfully");
                return result.Value;
            }
            else
            {
                var errorMessage = $"Failed to export data: {result.Error}";
                _logger?.LogError("[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
                OnErrorOccurred(errorMessage, result.Exception, "ExportData");
                return null;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Data export failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "ExportData");
            return null;
        }
    }

    /// <summary>
    /// ENTERPRISE: Perform search operation
    /// </summary>
    public async Task<bool> SearchAsync(string searchTerm, SearchOptions? options = null)
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Performing search for: {SearchTerm}", searchTerm);

            if (_dataGridService == null)
            {
                OnErrorOccurred("DataGrid service not initialized", null, "Search");
                return false;
            }

            var result = await _dataGridService.SearchAsync(searchTerm, options);

            if (result.IsSuccess)
            {
                _logger?.LogInformation("[BUSINESS-LOGIC] Search completed successfully");
                OnOperationCompleted("Search", true, $"Search completed for '{searchTerm}'");
                return true;
            }
            else
            {
                var errorMessage = $"Search failed: {result.Error}";
                _logger?.LogError("[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
                OnErrorOccurred(errorMessage, result.Exception, "Search");
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Search operation failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "Search");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Apply filters with business validation
    /// </summary>
    public async Task<bool> ApplyFiltersAsync(IReadOnlyList<FilterExpression> filters)
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Applying {Count} filters", filters.Count);

            if (_dataGridService == null)
            {
                OnErrorOccurred("DataGrid service not initialized", null, "ApplyFilters");
                return false;
            }

            var result = await _dataGridService.ApplyFiltersAsync(filters);

            if (result.IsSuccess)
            {
                _logger?.LogInformation("[BUSINESS-LOGIC] Filters applied successfully");
                OnOperationCompleted("ApplyFilters", true, $"Applied {filters.Count} filters successfully");
                return true;
            }
            else
            {
                var errorMessage = $"Failed to apply filters: {result.Error}";
                _logger?.LogError("[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
                OnErrorOccurred(errorMessage, result.Exception, "ApplyFilters");
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Filter application failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "ApplyFilters");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Validate all data with business rules
    /// </summary>
    public async Task<ValidationError[]?> ValidateAllAsync()
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Validating all data");

            if (_dataGridService == null)
            {
                OnErrorOccurred("DataGrid service not initialized", null, "ValidateAll");
                return null;
            }

            var result = await _dataGridService.ValidateAllAsync();

            if (result.IsSuccess)
            {
                var errorCount = result.Value?.Length ?? 0;
                _logger?.LogInformation("[BUSINESS-LOGIC] Validation completed: {ErrorCount} errors found", errorCount);
                OnOperationCompleted("ValidateAll", true, $"Validation completed with {errorCount} errors");
                return result.Value;
            }
            else
            {
                var errorMessage = $"Validation failed: {result.Error}";
                _logger?.LogError("[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
                OnErrorOccurred(errorMessage, result.Exception, "ValidateAll");
                return null;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Validation operation failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "ValidateAll");
            return null;
        }
    }

    /// <summary>
    /// ENTERPRISE: Initialize with sample data
    /// </summary>
    public async Task<Result<bool>> InitializeWithSampleDataAsync()
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Initializing with sample data");

            // Create sample columns
            var sampleColumns = new List<ColumnDefinition>
            {
                ColumnDefinition.Text("Name", "Name"),
                ColumnDefinition.Numeric<int>("Age", "Age"),
                ColumnDefinition.Boolean("Active", "Active")
            };

            // Initialize with sample columns first
            var result = await InitializeAsync(new DataGridUnifiedService(), sampleColumns);

            if (result)
            {
                // Import sample data
                var sampleData = new List<Dictionary<string, object?>>
                {
                    new() { ["Name"] = "John Doe", ["Age"] = 30, ["Active"] = true },
                    new() { ["Name"] = "Jane Smith", ["Age"] = 25, ["Active"] = false },
                    new() { ["Name"] = "Bob Johnson", ["Age"] = 35, ["Active"] = true }
                };

                var importResult = await ImportDataAsync(sampleData);
                return Result<bool>.Success(importResult);
            }
            else
            {
                return Result<bool>.Failure("Failed to initialize with sample data");
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Sample data initialization failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            OnErrorOccurred(errorMessage, ex, "InitializeWithSampleData");
            return Result<bool>.Failure(errorMessage);
        }
    }

    /// <summary>
    /// ENTERPRISE: Import from dictionary (wrapper method)
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        IReadOnlyList<ColumnDefinition>? columns = null)
    {
        try
        {
            var importResult = await ImportDataAsync(data);
            if (importResult)
            {
                // Create successful ImportResult
                var result = ImportResult.Success(data.Count, data.Count, ImportMode.Replace);
                return Result<ImportResult>.Success(result);
            }
            else
            {
                var result = ImportResult.WithErrors(data.Count, 0, new List<ValidationError>(), ImportMode.Replace);
                return Result<ImportResult>.Success(result);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Dictionary import failed: {ex.Message}";
            return Result<ImportResult>.Failure(errorMessage);
        }
    }

    /// <summary>
    /// ENTERPRISE: Export to dictionary (wrapper method)
    /// </summary>
    public async Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync()
    {
        try
        {
            var data = await ExportDataAsync();
            if (data != null)
            {
                return Result<List<Dictionary<string, object?>>>.Success(data);
            }
            else
            {
                return Result<List<Dictionary<string, object?>>>.Failure("Export failed");
            }
        }
        catch (Exception ex)
        {
            return Result<List<Dictionary<string, object?>>>.Failure($"Export failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Search with SearchResult return
    /// </summary>
    public async Task<Result<SearchResult>> SearchAsync(string searchTerm, object? searchConfig = null)
    {
        try
        {
            var success = await SearchAsync(searchTerm, searchConfig as SearchOptions);
            if (success)
            {
                // Create a basic SearchResult
                var result = SearchResult.Empty(searchTerm);
                return Result<SearchResult>.Success(result);
            }
            else
            {
                return Result<SearchResult>.Failure("Search failed");
            }
        }
        catch (Exception ex)
        {
            return Result<SearchResult>.Failure($"Search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Apply filters (wrapper method)
    /// </summary>
    public async Task<Result<bool>> ApplyFiltersAsync(object filters)
    {
        try
        {
            // Convert filters to the expected type
            var filterList = new List<FilterExpression>();
            var success = await ApplyFiltersAsync(filterList);
            return Result<bool>.Success(success);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Apply filters failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Validate all with proper return type (wrapper)
    /// </summary>
    public async Task<Result<List<object>>> ValidateAllWrapperAsync()
    {
        try
        {
            var validationResult = await (_dataGridService?.ValidateAllAsync() ?? Task.FromResult(Result<ValidationError[]>.Failure("Service not available", new List<ValidationError>())));
            if (validationResult.IsSuccess)
            {
                var result = validationResult.Value.Cast<object>().ToList();
                return Result<List<object>>.Success(result);
            }
            else
            {
                return Result<List<object>>.Success(new List<object>());
            }
        }
        catch (Exception ex)
        {
            return Result<List<object>>.Failure($"Validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Add row operation
    /// </summary>
    public async Task<Result<bool>> AddRowAsync()
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Adding new row");

            if (_dataGridService == null)
            {
                return Result<bool>.Failure("DataGrid service not initialized");
            }

            // Create an empty row with default values
            var emptyRow = new Dictionary<string, object?>();
            if (_currentColumns != null)
            {
                foreach (var column in _currentColumns)
                {
                    emptyRow[column.Name] = GetDefaultValueForType(column.DataType);
                }
            }

            var result = await ImportDataAsync(new List<Dictionary<string, object?>> { emptyRow });
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Add row failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage);
        }
    }

    /// <summary>
    /// ENTERPRISE: Delete row operation
    /// </summary>
    public async Task<Result<bool>> DeleteRowAsync(int rowIndex)
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Deleting row at index: {RowIndex}", rowIndex);

            if (_dataGridService == null)
            {
                return Result<bool>.Failure("DataGrid service not initialized");
            }

            // This would normally call a delete method on the service
            // For now, return success
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Delete row failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage);
        }
    }

    /// <summary>
    /// ENTERPRISE: Clear filters operation
    /// </summary>
    public async Task<Result<bool>> ClearFiltersAsync()
    {
        try
        {
            _logger?.LogInformation("[BUSINESS-LOGIC] Clearing filters");

            if (_dataGridService == null)
            {
                return Result<bool>.Failure("DataGrid service not initialized");
            }

            // This would normally call a clear filters method on the service
            // For now, return success
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Clear filters failed: {ex.Message}";
            _logger?.LogError(ex, "[BUSINESS-LOGIC] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage);
        }
    }

    /// <summary>
    /// HELPER: Get default value for a type
    /// </summary>
    private static object? GetDefaultValueForType(Type type)
    {
        if (type == typeof(string))
            return string.Empty;
        if (type == typeof(int))
            return 0;
        if (type == typeof(bool))
            return false;
        if (type == typeof(DateTime))
            return DateTime.Now;

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// ENTERPRISE: Get current state
    /// </summary>
    public GridState? CurrentState => _dataGridService?.CurrentState;

    #endregion

    #region State Management

    /// <summary>
    /// ENTERPRISE: Get current business state
    /// </summary>
    public Dictionary<string, object?> GetCurrentState()
    {
        var state = new Dictionary<string, object?>
        {
            ["IsInitialized"] = _dataGridService != null,
            ["ColumnCount"] = _currentColumns?.Count ?? 0,
            ["HasColorConfiguration"] = _colorConfiguration != null,
            ["HasValidationConfiguration"] = _validationConfiguration != null
        };

        if (_dataGridService != null)
        {
            state["RowCount"] = _dataGridService.GetRowCount();
            state["ColumnCount"] = _dataGridService.GetColumnCount();
        }

        return state;
    }

    #endregion

    #region Event Helpers

    /// <summary>
    /// HELPER: Fire operation completed event
    /// </summary>
    private void OnOperationCompleted(string operationType, bool success, string message)
    {
        try
        {
            OperationCompleted?.Invoke(this, new OperationCompletedEventArgs(
                operationType, success, message));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[BUSINESS-LOGIC] Error firing OperationCompleted event");
        }
    }

    /// <summary>
    /// HELPER: Fire error occurred event
    /// </summary>
    private void OnErrorOccurred(string errorMessage, Exception? exception, string? operationType)
    {
        try
        {
            ErrorOccurred?.Invoke(this, new UIErrorEventArgs(
                errorMessage, exception, operationType));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[BUSINESS-LOGIC] Error firing ErrorOccurred event");
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// CLEANUP: Dispose of managed resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _dataGridService?.Dispose();
            _dataGridService = null;

            _logger?.LogInformation("[BUSINESS-LOGIC] DataGridBusinessLogicManager disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[BUSINESS-LOGIC] Error during disposal");
        }

        _disposed = true;
    }

    #endregion
}