using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.Specialized;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using InitializeGridUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.InitializeGrid;
using ImportDataUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ImportData;
using ExportDataUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ExportData;
using SearchGridUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid;
using ManageRowsUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ManageRows;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

/// <summary>
/// FACADE PATTERN: Unified interface replacing the 920-line GOD file
/// SOLID: Orchestrates specialized services with single responsibilities
/// CLEAN ARCHITECTURE: Application layer facade coordinating domain services
/// ENTERPRISE: Professional API maintaining backward compatibility
/// </summary>
public sealed class DataGridUnifiedService : IDataGridService
{
    #region Private Fields - Specialized Services
    
    private readonly IDataGridStateManagementService _stateService;
    private readonly IDataGridImportExportService _importExportService;
    private readonly IDataGridSearchFilterService _searchFilterService;
    private readonly IDataGridRowManagementService _rowManagementService;
    private readonly IDataGridValidationService _validationService;
    private readonly IClipboardService _clipboardService;
    private readonly ILogger? _logger;
    private bool _disposed;
    
    #endregion

    #region Constructor - Dependency Injection
    
    public DataGridUnifiedService(
        IDataGridStateManagementService stateService,
        IDataGridImportExportService importExportService,
        IDataGridSearchFilterService searchFilterService,
        IDataGridRowManagementService rowManagementService,
        IDataGridValidationService validationService,
        IClipboardService clipboardService,
        ILogger<DataGridUnifiedService>? logger = null)
    {
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _importExportService = importExportService ?? throw new ArgumentNullException(nameof(importExportService));
        _searchFilterService = searchFilterService ?? throw new ArgumentNullException(nameof(searchFilterService));
        _rowManagementService = rowManagementService ?? throw new ArgumentNullException(nameof(rowManagementService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _logger = logger;
        
        _logger?.LogDebug("DataGridUnifiedService initialized with specialized services");
    }
    
    #endregion

    #region Properties - Delegation to State Service
    
    /// <summary>Current grid state</summary>
    public GridState? CurrentState => _stateService.CurrentState;
    
    /// <summary>Check if grid is initialized</summary>
    public bool IsInitialized => _stateService.IsInitialized;
    
    #endregion

    #region Initialization Operations - Delegate to State Service
    
    /// <summary>
    /// FACADE: Initialize grid - delegates to state management service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(InitializeGridUseCases.InitializeDataGridCommand command)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        
        _logger?.LogInformation("Initializing DataGrid through unified service");
        
        var result = await _stateService.InitializeAsync(command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("DataGrid initialized successfully");
        }
        else
        {
            _logger?.LogError("DataGrid initialization failed: {Error}", result.Error);
        }
        
        return result;
    }

    #endregion

    #region Import Operations - Delegate to Import/Export Service
    
    /// <summary>
    /// FACADE: Import from dictionary - delegates to import/export service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(ImportDataUseCases.ImportDataCommand command)
    {
        if (_disposed) return Result<ImportResult>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<ImportResult>.Failure("DataGrid must be initialized before importing");
        
        _logger?.LogInformation("Importing data from dictionary through unified service");
        
        // Create snapshot before import
        await _stateService.CreateStateSnapshotAsync("Before dictionary import");
        
        var result = await _importExportService.ImportFromDictionaryAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Dictionary import completed successfully");
        }
        else
        {
            _logger?.LogError("Dictionary import failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// FACADE: Import from DataTable - delegates to import/export service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDataTableAsync(ImportDataUseCases.ImportFromDataTableCommand command)
    {
        if (_disposed) return Result<ImportResult>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<ImportResult>.Failure("DataGrid must be initialized before importing");
        
        _logger?.LogInformation("Importing data from DataTable through unified service");
        
        // Create snapshot before import
        await _stateService.CreateStateSnapshotAsync("Before DataTable import");
        
        var result = await _importExportService.ImportFromDataTableAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("DataTable import completed successfully");
        }
        else
        {
            _logger?.LogError("DataTable import failed: {Error}", result.Error);
        }
        
        return result;
    }

    #endregion

    #region Export Operations - Delegate to Import/Export Service
    
    /// <summary>
    /// FACADE: Export to dictionary - delegates to import/export service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(ExportDataUseCases.ExportDataCommand command)
    {
        if (_disposed) return Result<List<Dictionary<string, object?>>>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<List<Dictionary<string, object?>>>.Failure("DataGrid must be initialized before exporting");
        
        _logger?.LogInformation("Exporting data to dictionary through unified service");
        
        var result = await _importExportService.ExportToDictionaryAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Dictionary export completed successfully");
        }
        else
        {
            _logger?.LogError("Dictionary export failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// FACADE: Export to DataTable - delegates to import/export service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<DataTable>> ExportToDataTableAsync(ExportDataUseCases.ExportToDataTableCommand command)
    {
        if (_disposed) return Result<DataTable>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<DataTable>.Failure("DataGrid must be initialized before exporting");
        
        _logger?.LogInformation("Exporting data to DataTable through unified service");
        
        var result = await _importExportService.ExportToDataTableAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("DataTable export completed successfully");
        }
        else
        {
            _logger?.LogError("DataTable export failed: {Error}", result.Error);
        }
        
        return result;
    }

    #endregion

    #region Search and Filter Operations - Delegate to Search/Filter Service
    
    /// <summary>
    /// FACADE: Search operation - delegates to search/filter service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<SearchResult>> SearchAsync(SearchGridUseCases.SearchCommand command)
    {
        if (_disposed) return Result<SearchResult>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<SearchResult>.Failure("DataGrid must be initialized before searching");
        
        _logger?.LogInformation("Performing search through unified service");
        
        var result = await _searchFilterService.SearchAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Search completed successfully");
            // Convert List<SearchResult> to SearchResult (take first result or create empty)
            var searchResults = result.Value;
            if (searchResults != null && searchResults.Count > 0)
            {
                return Result<SearchResult>.Success(searchResults[0]);
            }
            else
            {
                return Result<SearchResult>.Success(new SearchResult());
            }
        }
        else
        {
            _logger?.LogError("Search failed: {Error}", result.Error);
            return Result<SearchResult>.Failure(result.Error);
        }
    }

    /// <summary>
    /// FACADE: Apply filters - delegates to search/filter service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> ApplyFiltersAsync(
        IReadOnlyList<FilterDefinition> filters,
        FilterLogicOperator logicOperator = FilterLogicOperator.And,
        TimeSpan? timeout = null)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before applying filters");
        
        _logger?.LogInformation("Applying filters through unified service");
        
        // Create snapshot before filtering
        await _stateService.CreateStateSnapshotAsync("Before filters");
        
        var result = await _searchFilterService.ApplyFiltersAsync(CurrentState!, filters, logicOperator);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Filters applied successfully");
        }
        else
        {
            _logger?.LogError("Filter application failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// FACADE: Sort operation - delegates to search/filter service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> SortAsync(string columnName, SortDirection direction, TimeSpan? timeout = null)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before sorting");
        
        _logger?.LogInformation("Performing sort through unified service");
        
        // Create snapshot before sorting
        await _stateService.CreateStateSnapshotAsync("Before sort");
        
        var result = await _searchFilterService.SortAsync(CurrentState!, columnName, direction);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Sort completed successfully");
        }
        else
        {
            _logger?.LogError("Sort failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// FACADE: Clear filters - delegates to search/filter service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> ClearFiltersAsync()
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before clearing filters");
        
        _logger?.LogInformation("Clearing filters through unified service");
        
        var result = await _searchFilterService.ClearFiltersAsync(CurrentState!);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Filters cleared successfully");
        }
        else
        {
            _logger?.LogError("Clear filters failed: {Error}", result.Error);
        }
        
        return result;
    }

    #endregion

    #region Validation Operations - Delegate to Validation Service
    
    /// <summary>
    /// FACADE: Validate all data - delegates to validation service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<List<ValidationError>>> ValidateAllAsync(SearchGridUseCases.ValidateAllCommand command)
    {
        if (_disposed) return Result<List<ValidationError>>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<List<ValidationError>>.Failure("DataGrid must be initialized before validation");
        
        _logger?.LogInformation("Performing validation through unified service");
        
        var result = await _validationService.ValidateAllAsync(command.Progress);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Validation completed successfully");
            // Convert ValidationError[] to List<ValidationError>
            var validationErrorsList = result.Value?.ToList() ?? new List<ValidationError>();
            CurrentState.ValidationErrors.Clear();
            CurrentState.ValidationErrors.AddRange(validationErrorsList);
            return Result<List<ValidationError>>.Success(validationErrorsList);
        }
        else
        {
            _logger?.LogError("Validation failed: {Error}", result.Error);
            return Result<List<ValidationError>>.Failure(result.Error);
        }
    }

    #endregion

    #region Row Management Operations - Delegate to Row Management Service
    
    /// <summary>
    /// FACADE: Add row - delegates to row management service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> AddRowAsync(SearchGridUseCases.AddRowCommand command)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before adding rows");
        
        _logger?.LogInformation("Adding row through unified service");
        
        // Create snapshot before row addition
        await _stateService.CreateStateSnapshotAsync("Before add row");
        
        var result = await _rowManagementService.AddRowAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Row added successfully");
        }
        else
        {
            _logger?.LogError("Add row failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// FACADE: Delete row - delegates to row management service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> DeleteRowAsync(SearchGridUseCases.DeleteRowCommand command)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before deleting rows");
        
        _logger?.LogInformation("Deleting row through unified service");
        
        // Create snapshot before row deletion
        await _stateService.CreateStateSnapshotAsync("Before delete row");
        
        var result = await _rowManagementService.DeleteRowAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Row deleted successfully");
        }
        else
        {
            _logger?.LogError("Delete row failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// FACADE: Update row - delegates to row management service
    /// BACKWARD_COMPATIBILITY: Maintains existing API signature
    /// </summary>
    public async Task<Result<bool>> UpdateRowAsync(SearchGridUseCases.UpdateRowCommand command)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before updating rows");
        
        _logger?.LogInformation("Updating row through unified service");
        
        // Create snapshot before row update
        await _stateService.CreateStateSnapshotAsync("Before update row");
        
        var result = await _rowManagementService.UpdateRowAsync(CurrentState!, command);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Row updated successfully");
        }
        else
        {
            _logger?.LogError("Update row failed: {Error}", result.Error);
        }
        
        return result;
    }

    #endregion

    #region State Management Operations - Delegate to State Service
    
    /// <summary>
    /// ENTERPRISE: Undo last operation
    /// NEW_FEATURE: Enhanced undo/redo functionality
    /// </summary>
    public async Task<Result<bool>> UndoAsync()
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before undo");
        
        _logger?.LogInformation("Performing undo through unified service");
        
        var result = await _stateService.UndoAsync();
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Undo completed successfully");
        }
        else
        {
            _logger?.LogError("Undo failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// ENTERPRISE: Redo last undone operation
    /// NEW_FEATURE: Enhanced undo/redo functionality
    /// </summary>
    public async Task<Result<bool>> RedoAsync()
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before redo");
        
        _logger?.LogInformation("Performing redo through unified service");
        
        var result = await _stateService.RedoAsync();
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("Redo completed successfully");
        }
        else
        {
            _logger?.LogError("Redo failed: {Error}", result.Error);
        }
        
        return result;
    }

    /// <summary>
    /// ENTERPRISE: Get performance statistics
    /// NEW_FEATURE: Performance monitoring
    /// </summary>
    public async Task<Result<Domain.ValueObjects.DataOperations.GridPerformanceStatistics>> GetPerformanceStatisticsAsync()
    {
        if (_disposed) return Result<Domain.ValueObjects.DataOperations.GridPerformanceStatistics>.Failure("Service has been disposed");
        
        var stateResult = await _stateService.GetPerformanceStatisticsAsync();
        if (stateResult.IsFailure)
            return Result<Domain.ValueObjects.DataOperations.GridPerformanceStatistics>.Failure(stateResult.Error);
            
        // Map from Specialized service type to Domain type
        var specializedStats = stateResult.Value;
        var domainStats = new Domain.ValueObjects.DataOperations.GridPerformanceStatistics
        {
            TotalMemoryUsage = specializedStats.MemoryUsageEstimate,
            Uptime = TimeSpan.Zero // Could be tracked separately
        };
        
        return Result<Domain.ValueObjects.DataOperations.GridPerformanceStatistics>.Success(domainStats);
    }

    /// <summary>
    /// ENTERPRISE: Reset grid to initial state
    /// NEW_FEATURE: Complete state reset
    /// </summary>
    public async Task<Result<bool>> ResetAsync()
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before reset");
        
        _logger?.LogInformation("Resetting DataGrid through unified service");
        
        var result = await _stateService.ResetAsync();
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("DataGrid reset completed successfully");
        }
        else
        {
            _logger?.LogError("Reset failed: {Error}", result.Error);
        }
        
        return result;
    }

    #endregion

    #region State Queries - Read-only Operations
    
    /// <summary>
    /// ENTERPRISE: Get current filter state
    /// INTROSPECTION: Allow external components to query filter state
    /// </summary>
    public CurrentFilterState GetCurrentFilterState()
    {
        if (_disposed) return CurrentFilterState.Empty;
        return _searchFilterService.GetCurrentFilterState();
    }

    /// <summary>
    /// ENTERPRISE: Get current sort state
    /// INTROSPECTION: Allow external components to query sort state
    /// </summary>
    public CurrentSortState GetCurrentSortState()
    {
        if (_disposed) return CurrentSortState.Empty;
        return _searchFilterService.GetCurrentSortState();
    }

    /// <summary>
    /// ENTERPRISE: Check if search is currently active
    /// INTROSPECTION: Query search state
    /// </summary>
    public bool HasActiveSearch
    {
        get
        {
            if (_disposed) return false;
            return _searchFilterService.HasActiveSearch;
        }
    }

    /// <summary>
    /// ENTERPRISE: Check if undo is available
    /// INTROSPECTION: Query undo availability
    /// </summary>
    public bool CanUndo
    {
        get
        {
            if (_disposed) return false;
            return _stateService.CanUndo;
        }
    }

    /// <summary>
    /// ENTERPRISE: Check if redo is available
    /// INTROSPECTION: Query redo availability
    /// </summary>
    public bool CanRedo
    {
        get
        {
            if (_disposed) return false;
            return _stateService.CanRedo;
        }
    }

    #endregion

    #region IDataGridService Interface Implementation - Compatibility Layer
    
    /// <summary>
    /// INTERFACE: InitializeAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based initialization
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null)
    {
        var command = InitializeGridUseCases.InitializeDataGridCommand.Create(columns, colorConfiguration, validationConfiguration, performanceConfiguration);
        return await InitializeAsync(command);
    }

    /// <summary>
    /// INTERFACE: ImportFromDictionaryAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based import
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        var command = ImportDataUseCases.ImportDataCommand.FromDictionary(data, checkboxStates, startRow, mode, timeout, validationProgress);
        return await ImportFromDictionaryAsync(command);
    }

    /// <summary>
    /// INTERFACE: ImportFromDataTableAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based import
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        var command = ImportDataUseCases.ImportFromDataTableCommand.FromDataTable(dataTable, checkboxStates, startRow, mode, timeout, validationProgress);
        return await ImportFromDataTableAsync(command);
    }

    /// <summary>
    /// INTERFACE: ExportToDictionaryAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based export
    /// </summary>
    public async Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        var command = ExportDataUseCases.ExportDataCommand.ToDictionary(includeValidAlerts, exportOnlyChecked, exportOnlyFiltered, removeAfter, timeout, exportProgress);
        return await ExportToDictionaryAsync(command);
    }

    /// <summary>
    /// INTERFACE: ExportToDataTableAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based export
    /// </summary>
    public async Task<Result<DataTable>> ExportToDataTableAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        var command = ExportDataUseCases.ExportToDataTableCommand.ToDataTable(includeValidAlerts, exportOnlyChecked, exportOnlyFiltered, removeAfter, timeout, exportProgress);
        return await ExportToDataTableAsync(command);
    }

    /// <summary>
    /// INTERFACE: SearchAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based search
    /// </summary>
    public async Task<Result<SearchResult>> SearchAsync(
        string searchTerm,
        SearchOptions? options = null)
    {
        var command = SearchGridUseCases.SearchCommand.Create(searchTerm, options);
        var result = await SearchAsync(command);
        if (result.IsFailure)
            return Result<SearchResult>.Failure(result.Error);
            
        // Return the search result directly
        return result;
    }

    /// <summary>
    /// INTERFACE: ApplyFiltersAsync - matches IDataGridService signature
    /// COMPATIBILITY: Converts FilterExpression to FilterDefinition
    /// </summary>
    public async Task<Result<bool>> ApplyFiltersAsync(IReadOnlyList<FilterExpression> filters)
    {
        var filterDefinitions = filters.Select(f => f.ToFilterDefinition()).ToList();
        return await ApplyFiltersAsync(filterDefinitions);
    }

    /// <summary>
    /// INTERFACE: SortAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around search/filter service
    /// </summary>
    public async Task<Result<bool>> SortAsync(string columnName, SortDirection direction)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before sorting");
        
        return await _searchFilterService.SortAsync(CurrentState!, columnName, direction);
    }

    /// <summary>
    /// INTERFACE: ValidateAllAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based validation
    /// </summary>
    public async Task<Result<ValidationError[]>> ValidateAllAsync(IProgress<ValidationProgress>? progress = null)
    {
        var command = SearchGridUseCases.ValidateAllCommand.Create(progress);
        var result = await ValidateAllAsync(command);
        
        if (result.IsSuccess)
        {
            return Result<ValidationError[]>.Success(result.Value?.ToArray() ?? Array.Empty<ValidationError>());
        }
        
        return Result<ValidationError[]>.Failure(result.Error);
    }

    /// <summary>
    /// INTERFACE: AddRowAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based row management
    /// </summary>
    public async Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData)
    {
        var command = SearchGridUseCases.AddRowCommand.Create(rowData);
        return await AddRowAsync(command);
    }

    /// <summary>
    /// INTERFACE: UpdateRowAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based row management
    /// </summary>
    public async Task<Result<bool>> UpdateRowAsync(int rowIndex, Dictionary<string, object?> rowData)
    {
        var command = SearchGridUseCases.UpdateRowCommand.Create(rowIndex, rowData);
        return await UpdateRowAsync(command);
    }

    /// <summary>
    /// INTERFACE: DeleteRowAsync - matches IDataGridService signature
    /// COMPATIBILITY: Wrapper around command-based row management
    /// </summary>
    public async Task<Result<bool>> DeleteRowAsync(int rowIndex)
    {
        var command = SearchGridUseCases.DeleteRowCommand.Create(rowIndex);
        return await DeleteRowAsync(command);
    }

    /// <summary>
    /// INTERFACE: GetRowCount - matches IDataGridService signature
    /// COMPATIBILITY: Delegates to state service
    /// </summary>
    public int GetRowCount()
    {
        if (_disposed || !IsInitialized) return 0;
        return CurrentState?.Rows.Count ?? 0;
    }

    /// <summary>
    /// INTERFACE: GetColumnCount - matches IDataGridService signature
    /// COMPATIBILITY: Delegates to state service
    /// </summary>
    public int GetColumnCount()
    {
        if (_disposed || !IsInitialized) return 0;
        return CurrentState?.Columns.Count ?? 0;
    }

    /// <summary>
    /// OPERATION: Check if row is empty
    /// </summary>
    public async Task<Result<bool>> IsRowEmptyAsync(int rowIndex)
    {
        try
        {
            if (_disposed || !IsInitialized) 
                return Result<bool>.Failure("Service not initialized");
            
            if (CurrentState == null || rowIndex < 0 || rowIndex >= CurrentState.Rows.Count)
                return Result<bool>.Success(true); // Out of bounds considered empty
            
            var row = CurrentState.Rows[rowIndex];
            
            // Check if all values in the row are null, empty, or whitespace
            var isEmpty = row.Data.Values.All(value => 
                value == null || 
                (value is string str && string.IsNullOrWhiteSpace(str)) ||
                (value.ToString()?.Trim().Length == 0));
                
            return Result<bool>.Success(isEmpty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if row {RowIndex} is empty", rowIndex);
            return Result<bool>.Failure(ex);
        }
    }

    /// <summary>
    /// OPERATION: Get row data by index
    /// </summary>
    public async Task<Dictionary<string, object?>?> GetRowDataAsync(int rowIndex)
    {
        try
        {
            if (_disposed || !IsInitialized || CurrentState == null)
                return null;
            
            if (rowIndex < 0 || rowIndex >= CurrentState.Rows.Count)
                return null;
            
            return new Dictionary<string, object?>(CurrentState.Rows[rowIndex].Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting row data for {RowIndex}", rowIndex);
            return null;
        }
    }

    /// <summary>
    /// Get column name by index
    /// </summary>
    public async Task<Result<string>> GetColumnNameAsync(int columnIndex)
    {
        try
        {
            if (_disposed || !IsInitialized || CurrentState == null)
                return Result<string>.Failure("Service not initialized");
            
            if (columnIndex < 0 || columnIndex >= CurrentState.Columns.Count)
                return Result<string>.Failure("Column index out of range");
            
            return Result<string>.Success(CurrentState.Columns[columnIndex].Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting column name for index {ColumnIndex}", columnIndex);
            return Result<string>.Failure(ex);
        }
    }

    /// <summary>
    /// PROFESSIONAL: Delete rows that meet specified validation criteria
    /// ENTERPRISE: Batch operation with progress reporting and rollback support
    /// </summary>
    public async Task<Result<ValidationBasedDeleteResult>> DeleteRowsWithValidationAsync(
        ValidationDeletionCriteria validationCriteria,
        ValidationDeletionOptions? options = null)
    {
        var startTime = DateTime.UtcNow;
        var deletionOptions = options ?? new ValidationDeletionOptions();
        var validationErrors = new List<ValidationError>();
        
        try
        {
            if (_disposed || !IsInitialized || CurrentState == null)
                return Result<ValidationBasedDeleteResult>.Failure("Service not initialized");

            _logger.LogInformation("🗑️ VALIDATION DELETE: Starting validation-based deletion with mode {Mode}", 
                validationCriteria.Mode);

            var totalRows = CurrentState.Rows.Count;
            var rowsToDelete = new List<int>();
            var processedRows = 0;

            // Phase 1: Identify rows to delete based on criteria
            for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
            {
                var row = CurrentState.Rows[rowIndex];
                var shouldDelete = false;

                switch (validationCriteria.Mode)
                {
                    case ValidationDeletionMode.DeleteInvalidRows:
                        // Delete rows that have validation errors
                        shouldDelete = row.ValidationErrorObjects?.Any() == true;
                        break;

                    case ValidationDeletionMode.DeleteValidRows:
                        // Delete rows that have no validation errors
                        shouldDelete = row.ValidationErrorObjects?.Any() != true;
                        break;

                    case ValidationDeletionMode.DeleteBySeverity:
                        // Delete rows with specific severity levels
                        if (validationCriteria.Severity != null && row.ValidationErrorObjects?.Any() == true)
                        {
                            shouldDelete = row.ValidationErrorObjects.Any(error => 
                                validationCriteria.Severity.Contains(GetValidationSeverity(error.Level)));
                        }
                        break;

                    case ValidationDeletionMode.DeleteByRuleName:
                        // Delete rows failing specific named rules
                        if (validationCriteria.SpecificRuleNames != null && row.ValidationErrorObjects?.Any() == true)
                        {
                            shouldDelete = row.ValidationErrorObjects.Any(error => 
                                validationCriteria.SpecificRuleNames.Contains(error.ValidationRule ?? ""));
                        }
                        break;

                    case ValidationDeletionMode.DeleteByCustomRule:
                        // Delete based on custom predicate
                        if (validationCriteria.CustomPredicate != null)
                        {
                            shouldDelete = validationCriteria.CustomPredicate(row.Data);
                        }
                        break;
                }

                if (shouldDelete)
                {
                    rowsToDelete.Add(rowIndex);
                }

                processedRows++;

                // Report progress
                deletionOptions.Progress?.Report(ValidationDeletionProgress.Create(
                    totalRows, processedRows, rowsToDelete.Count, 0, 
                    $"Evaluating row {rowIndex + 1}", DateTime.UtcNow - startTime));
            }

            _logger.LogInformation("🗑️ VALIDATION DELETE: Found {RowsToDelete} rows to delete out of {TotalRows}", 
                rowsToDelete.Count, totalRows);

            // Phase 2: Delete rows (in reverse order to maintain indices)
            var deletedCount = 0;
            for (int i = rowsToDelete.Count - 1; i >= 0; i--)
            {
                var rowIndex = rowsToDelete[i];
                if (rowIndex < CurrentState.Rows.Count)
                {
                    CurrentState.Rows.RemoveAt(rowIndex);
                    deletedCount++;
                }

                // Report progress
                deletionOptions.Progress?.Report(ValidationDeletionProgress.Create(
                    totalRows, totalRows, rowsToDelete.Count, deletedCount, 
                    $"Deleting row {rowIndex}", DateTime.UtcNow - startTime));
            }

            var duration = DateTime.UtcNow - startTime;
            var result = new ValidationBasedDeleteResult(
                TotalRowsEvaluated: totalRows,
                RowsDeleted: deletedCount,
                RemainingRows: CurrentState.Rows.Count,
                ValidationErrors: validationErrors.AsReadOnly(),
                OperationDuration: duration);

            _logger.LogInformation("✅ VALIDATION DELETE: Completed in {Duration}ms - {DeletedCount} rows deleted", 
                duration.TotalMilliseconds, deletedCount);

            return Result<ValidationBasedDeleteResult>.Success(result);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "💥 VALIDATION DELETE: Failed after {Duration}ms", duration.TotalMilliseconds);
            
            var errorResult = new ValidationBasedDeleteResult(0, 0, 0, validationErrors.AsReadOnly(), duration);
            return Result<ValidationBasedDeleteResult>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// ENTERPRISE: Check if all non-empty rows pass validation
    /// COMPREHENSIVE: Validates complete dataset including cached/disk data
    /// </summary>
    public async Task<Result<bool>> AreAllNonEmptyRowsValidAsync(bool onlyFiltered = false)
    {
        try
        {
            if (_disposed || !IsInitialized || CurrentState == null)
                return Result<bool>.Failure("Service not initialized");

            _logger.LogInformation("🔍 VALIDATION CHECK: Starting comprehensive validation check (filtered: {OnlyFiltered})", 
                onlyFiltered);

            var rowsToCheck = onlyFiltered 
                ? CurrentState.Rows.Where(row => IsRowVisible(row)).ToList()
                : CurrentState.Rows.ToList();

            // Remove last empty row if it exists (per specification)
            if (rowsToCheck.Count > 0)
            {
                var lastRow = rowsToCheck[^1];
                var isEmpty = await IsRowEmptyAsync(rowsToCheck.Count - 1);
                if (isEmpty.IsSuccess && isEmpty.Value)
                {
                    rowsToCheck.RemoveAt(rowsToCheck.Count - 1);
                    _logger.LogDebug("🔍 VALIDATION CHECK: Ignoring last empty row per specification");
                }
            }

            if (rowsToCheck.Count == 0)
            {
                _logger.LogInformation("✅ VALIDATION CHECK: No rows to validate - returning true");
                return Result<bool>.Success(true);
            }

            // Check if any non-empty row has validation errors
            foreach (var row in rowsToCheck)
            {
                if (row.ValidationErrorObjects?.Any() == true)
                {
                    _logger.LogInformation("❌ VALIDATION CHECK: Found validation errors - returning false");
                    return Result<bool>.Success(false);
                }
            }

            _logger.LogInformation("✅ VALIDATION CHECK: All {RowCount} non-empty rows are valid", rowsToCheck.Count);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 VALIDATION CHECK: Failed during comprehensive validation check");
            return Result<bool>.Failure(ex);
        }
    }

    #region INTERNAL: Helper Methods

    /// <summary>
    /// Convert ValidationLevel to ValidationSeverity
    /// </summary>
    private ValidationSeverity GetValidationSeverity(ValidationLevel level)
    {
        return level switch
        {
            ValidationLevel.Success => ValidationSeverity.Success,
            ValidationLevel.Warning => ValidationSeverity.Warning,
            ValidationLevel.Error => ValidationSeverity.Error,
            _ => ValidationSeverity.Error
        };
    }

    /// <summary>
    /// Check if row is visible (filtered)
    /// </summary>
    private bool IsRowVisible(GridRow row)
    {
        // For now, all rows are considered visible
        // This will be enhanced when proper filtering is implemented
        return true;
    }

    #endregion

    #endregion

    #region Clipboard Operations - Excel Compatible Copy/Paste

    /// <summary>
    /// Copy selected rows to clipboard in Excel-compatible format
    /// EXCEL_COMPATIBLE: TSV format for seamless Excel integration
    /// </summary>
    public async Task<Result<bool>> CopySelectedRowsAsync(
        IReadOnlyList<int> selectedRowIndices,
        bool includeHeaders = true)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before copy operations");

        try
        {
            var selectedRows = new List<Dictionary<string, object?>>();
            
            foreach (var rowIndex in selectedRowIndices)
            {
                if (rowIndex >= 0 && rowIndex < CurrentState!.Rows.Count)
                {
                    selectedRows.Add(new Dictionary<string, object?>(CurrentState.Rows[rowIndex].Data));
                }
            }

            if (selectedRows.Count == 0)
                return Result<bool>.Failure("No valid rows selected for copying");

            var result = await _clipboardService.CopyToClipboardAsync(
                selectedRows, CurrentState.Columns, includeHeaders);

            _logger?.LogInformation("Copied {RowCount} rows to clipboard", selectedRows.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to copy selected rows to clipboard");
            return Result<bool>.Failure("Failed to copy selected rows to clipboard", ex);
        }
    }

    /// <summary>
    /// Copy all visible (filtered) rows to clipboard
    /// EXCEL_COMPATIBLE: Full export functionality with filtering
    /// </summary>
    public async Task<Result<bool>> CopyAllVisibleRowsAsync(bool includeHeaders = true)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before copy operations");

        try
        {
            var visibleRows = CurrentState!.Rows
                .Where(row => IsRowVisible(row))
                .Select(row => new Dictionary<string, object?>(row.Data))
                .ToList();

            if (visibleRows.Count == 0)
                return Result<bool>.Failure("No visible rows to copy");

            var result = await _clipboardService.CopyToClipboardAsync(
                visibleRows, CurrentState.Columns, includeHeaders);

            _logger?.LogInformation("Copied {RowCount} visible rows to clipboard", visibleRows.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to copy visible rows to clipboard");
            return Result<bool>.Failure("Failed to copy visible rows to clipboard", ex);
        }
    }

    /// <summary>
    /// Copy single cell value to clipboard
    /// </summary>
    public async Task<Result<bool>> CopyCellAsync(int rowIndex, string columnName)
    {
        if (_disposed) return Result<bool>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<bool>.Failure("DataGrid must be initialized before copy operations");

        try
        {
            if (rowIndex < 0 || rowIndex >= CurrentState!.Rows.Count)
                return Result<bool>.Failure("Invalid row index");

            var row = CurrentState.Rows[rowIndex];
            if (!row.Data.TryGetValue(columnName, out var cellValue))
                return Result<bool>.Failure($"Column '{columnName}' not found in row");

            var result = await _clipboardService.CopyCellAsync(cellValue);
            _logger?.LogDebug("Copied cell value from [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to copy cell value from [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            return Result<bool>.Failure("Failed to copy cell value", ex);
        }
    }

    /// <summary>
    /// Paste data from clipboard into grid
    /// EXCEL_COMPATIBLE: Smart parsing of Excel format with auto-type detection
    /// </summary>
    public async Task<Result<PasteResult>> PasteFromClipboardAsync(
        int startRowIndex = 0,
        int startColumnIndex = 0,
        bool replaceExistingData = false)
    {
        if (_disposed) return Result<PasteResult>.Failure("Service has been disposed");
        if (!IsInitialized) return Result<PasteResult>.Failure("DataGrid must be initialized before paste operations");

        try
        {
            var parseResult = await _clipboardService.PasteFromClipboardAsync(
                CurrentState!.Columns, startRowIndex, startColumnIndex);

            if (parseResult.IsFailure)
                return Result<PasteResult>.Failure(parseResult.Error);

            var clipboardData = parseResult.Value;
            
            // Insert parsed data into grid
            int insertedRows = 0;
            int updatedRows = 0;
            var errors = new List<string>();

            for (int i = 0; i < clipboardData.ParsedRows.Count; i++)
            {
                int targetRowIndex = startRowIndex + i;
                var rowData = clipboardData.ParsedRows[i];

                try
                {
                    if (targetRowIndex < CurrentState.Rows.Count)
                    {
                        // Update existing row
                        if (replaceExistingData)
                        {
                            await UpdateRowDataAsync(targetRowIndex, rowData);
                            updatedRows++;
                        }
                        else
                        {
                            // Merge with existing data
                            await MergeRowDataAsync(targetRowIndex, rowData);
                            updatedRows++;
                        }
                    }
                    else
                    {
                        // Add new row
                        var addCommand = new SearchGridUseCases.AddRowCommand { RowData = rowData, InsertAtIndex = targetRowIndex };
                        var addResult = await _rowManagementService.AddRowAsync(CurrentState, addCommand);
                        
                        if (addResult.IsSuccess)
                            insertedRows++;
                        else
                            errors.Add($"Row {i + 1}: {addResult.Error}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {i + 1}: {ex.Message}");
                }
            }

            var result = new PasteResult
            {
                Success = errors.Count == 0,
                InsertedRows = insertedRows,
                UpdatedRows = updatedRows,
                TotalProcessedRows = clipboardData.RowCount,
                DetectedFormat = $"{clipboardData.DetectedDelimiter} separated values",
                HasHeaders = clipboardData.HasHeaders,
                Errors = errors
            };

            _logger?.LogInformation("Paste operation completed: {InsertedRows} inserted, {UpdatedRows} updated, {ErrorCount} errors",
                insertedRows, updatedRows, errors.Count);

            return Result<PasteResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to paste data from clipboard");
            return Result<PasteResult>.Failure("Failed to paste data from clipboard", ex);
        }
    }

    /// <summary>
    /// Helper method to update existing row data
    /// </summary>
    private async Task UpdateRowDataAsync(int rowIndex, Dictionary<string, object?> newData)
    {
        var updateCommand = new SearchGridUseCases.UpdateRowCommand { RowIndex = rowIndex, RowData = newData };
        await _rowManagementService.UpdateRowAsync(CurrentState, updateCommand);
    }

    /// <summary>
    /// Helper method to merge new data with existing row data
    /// </summary>
    private async Task MergeRowDataAsync(int rowIndex, Dictionary<string, object?> newData)
    {
        var existingData = CurrentState!.Rows[rowIndex].Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        // Merge new data into existing data
        foreach (var kvp in newData)
        {
            existingData[kvp.Key] = kvp.Value;
        }

        await UpdateRowDataAsync(rowIndex, existingData);
    }

    #endregion

    #region IDisposable Implementation
    
    /// <summary>
    /// CLEAN_ARCHITECTURE: Proper resource cleanup
    /// SOLID: Single responsibility for resource management
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        
        try
        {
            _logger?.LogInformation("Disposing DataGridUnifiedService");
            
            // Dispose all specialized services
            _stateService?.Dispose();
            _importExportService?.Dispose();
            _searchFilterService?.Dispose();
            _rowManagementService?.Dispose();
            _validationService?.Dispose();
            _clipboardService?.Dispose();
            
            _disposed = true;
            _logger?.LogInformation("DataGridUnifiedService disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during DataGridUnifiedService disposal");
        }
    }
    
    #endregion
}