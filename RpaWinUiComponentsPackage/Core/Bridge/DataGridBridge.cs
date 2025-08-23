using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.Core.Extensions;
using RpaWinUiComponentsPackage.Core.Interfaces;
using RpaWinUiComponentsPackage.Core.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using ImportMode = RpaWinUiComponentsPackage.Core.Models.ImportMode;
using CorePerformanceConfiguration = RpaWinUiComponentsPackage.Core.Models.PerformanceConfiguration;
using AdvancedPerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration;

namespace RpaWinUiComponentsPackage.Core.Bridge;

/// <summary>
/// PROFESSIONAL MODULAR Bridge connecting Clean API to AdvancedDataGrid implementation
/// ARCHITECTURE: Composition-based with specialized operation managers
/// REPLACES: Single god-level DataGridBridge class
/// </summary>
internal sealed class DataGridBridge : IDataGridComponent, IDisposable
{
    #region Private Fields - Composition Components
    
    private readonly AdvancedDataGrid _internalGrid;
    private readonly ILogger? _logger;
    private readonly DataGridBridgeInitializer _initializer;
    private readonly DataGridBridgeImportManager _importManager;
    private readonly DataGridBridgeExportManager _exportManager;
    private readonly DataGridBridgeRowManager _rowManager;
    private readonly DataGridBridgeValidationManager _validationManager;
    private readonly DataGridBridgeSearchManager _searchManager;
    private readonly DataGridBridgeNavigationManager _navigationManager;
    private readonly DataGridBridgePerformanceManager _performanceManager;
    
    private bool _isInitialized;

    #endregion

    #region Constructor - Dependency Injection Pattern

    /// <summary>
    /// Professional constructor with modular dependency injection
    /// </summary>
    public DataGridBridge(AdvancedDataGrid internalGrid, ILogger? logger)
    {
        _internalGrid = internalGrid ?? throw new ArgumentNullException(nameof(internalGrid));
        _logger = logger;

        // Composition: Create specialized managers for each area of responsibility
        _initializer = new DataGridBridgeInitializer(internalGrid, logger);
        _importManager = new DataGridBridgeImportManager(internalGrid, logger);
        _exportManager = new DataGridBridgeExportManager(internalGrid, logger);
        _rowManager = new DataGridBridgeRowManager(internalGrid, logger);
        _validationManager = new DataGridBridgeValidationManager(internalGrid, logger);
        _searchManager = new DataGridBridgeSearchManager(internalGrid, logger);
        _navigationManager = new DataGridBridgeNavigationManager(internalGrid, logger);
        _performanceManager = new DataGridBridgePerformanceManager(internalGrid, logger);
        
        _logger?.Info("🔧 BRIDGE: Created modular DataGridBridge with specialized managers");
    }

    #endregion

    #region Public Properties

    public bool IsInitialized => _isInitialized;
    public bool HasData => _internalGrid?.HasData ?? false;

    #endregion

    #region Initialization - Delegated to Specialized Manager

    public async Task<bool> InitializeAsync(
        IReadOnlyList<ColumnConfiguration> columns,
        ColorConfiguration? colorConfig = null,
        ValidationConfiguration? validationConfig = null,
        CorePerformanceConfiguration? performanceConfig = null,
        int minimumRows = 10,
        bool enableSort = true,
        bool enableSearch = true,
        bool enableFilter = true,
        bool enableRealtimeValidation = true,
        bool enableBatchValidation = true,
        int maxRowsForOptimization = 100000,
        TimeSpan operationTimeout = default,
        ILogger? logger = null)
    {
        _logger?.Info("🔧 BRIDGE INIT: Starting modular initialization");
        
        var result = await _initializer.InitializeAsync(
            columns, colorConfig, validationConfig, performanceConfig,
            minimumRows, enableSort, enableSearch, enableFilter,
            enableRealtimeValidation, enableBatchValidation,
            maxRowsForOptimization, operationTimeout, logger ?? _logger);
            
        _isInitialized = result;
        return result;
    }

    #endregion

    #region Import Operations - Delegated to Import Manager

    public Task<ImportResult> ImportFromDictionaryAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _importManager.ImportFromDictionaryAsync(data, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);

    public Task<ImportResult> ImportFromDataTableAsync(
        System.Data.DataTable dataTable,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _importManager.ImportFromDataTableAsync(dataTable, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);

    public Task<ImportResult> ImportFromExcelAsync(
        byte[] excelData,
        string? worksheetName = null,
        bool hasHeaders = true,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _importManager.ImportFromExcelAsync(excelData, worksheetName, hasHeaders, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);

    public Task<ImportResult> ImportFromCsvAsync(
        string csvContent,
        string delimiter = ",",
        bool hasHeaders = true,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _importManager.ImportFromCsvAsync(csvContent, delimiter, hasHeaders, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);

    public Task<ImportResult> ImportFromJsonAsync(
        string jsonContent,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _importManager.ImportFromJsonAsync(jsonContent, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);

    public Task<ImportResult> ImportFromXmlAsync(
        string xmlContent,
        string? rootElementName = null,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _importManager.ImportFromXmlAsync(xmlContent, rootElementName, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);

    #endregion

    #region Export Operations - Delegated to Export Manager

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExportToDictionaryAsync(
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _exportManager.ExportToDictionaryAsync(includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);

    public Task<System.Data.DataTable> ExportToDataTableAsync(
        string? tableName = null,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _exportManager.ExportToDataTableAsync(tableName, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);

    public Task<byte[]> ExportToExcelAsync(
        string worksheetName = "Data",
        bool includeHeaders = true,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _exportManager.ExportToExcelAsync(worksheetName, includeHeaders, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);

    public Task<string> ExportToCsvAsync(
        string delimiter = ",",
        bool includeHeaders = true,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _exportManager.ExportToCsvAsync(delimiter, includeHeaders, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);

    public Task<string> ExportToJsonAsync(
        bool prettyPrint = false,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _exportManager.ExportToJsonAsync(prettyPrint, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);

    public Task<string> ExportToXmlAsync(
        string rootElementName = "Data",
        string rowElementName = "Row",
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _exportManager.ExportToXmlAsync(rootElementName, rowElementName, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);

    #endregion

    #region Row Management - Delegated to Row Manager

    public Task<bool> DeleteRowAsync(int rowIndex, bool forceDelete = false) =>
        _rowManager.DeleteRowAsync(rowIndex, forceDelete);

    public Task<bool> DeleteMultipleRowsAsync(IReadOnlyList<int> rowIndices, bool forceDelete = false) =>
        _rowManager.DeleteMultipleRowsAsync(rowIndices, forceDelete);

    public bool CanDeleteRow(int rowIndex) => _rowManager.CanDeleteRow(rowIndex);
    public int GetDeletableRowsCount() => _rowManager.GetDeletableRowsCount();
    public void DeleteSelectedRows() => _rowManager.DeleteSelectedRows();
    
    public void DeleteRowsWhere(Func<Dictionary<string, object?>, bool> predicate) =>
        _rowManager.DeleteRowsWhere(predicate);

    public Task<bool> ClearDataAsync() => _rowManager.ClearDataAsync();
    public Task<bool> CompactAfterDeletionAsync() => _rowManager.CompactAfterDeletionAsync();
    public Task<bool> CompactRowsAsync() => _rowManager.CompactRowsAsync();
    
    public Task<bool> PasteDataAsync(IReadOnlyList<IReadOnlyDictionary<string, object?>> data, int startRow, int startColumn) =>
        _rowManager.PasteDataAsync(data, startRow, startColumn);

    public bool IsRowEmpty(int rowIndex) => _rowManager.IsRowEmpty(rowIndex);
    public Task<int> GetLastDataRowAsync() => _rowManager.GetLastDataRowAsync();

    #endregion

    #region Validation - Delegated to Validation Manager

    public Task<BatchValidationResult?> ValidateAllRowsBatchAsync(
        TimeSpan timeout = default,
        IProgress<ValidationProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _validationManager.ValidateAllRowsBatchAsync(timeout, progress, cancellationToken);

    public Task<bool> AreAllNonEmptyRowsValidAsync(bool wholeDataset = true) =>
        _validationManager.AreAllNonEmptyRowsValidAsync(wholeDataset);

    public Task UpdateValidationUIAsync() => _validationManager.UpdateValidationUIAsync();

    public Task AddValidationRulesAsync(string columnName, IReadOnlyList<ValidationRule> rules) =>
        _validationManager.AddValidationRulesAsync(columnName, rules);

    public Task RemoveValidationRulesAsync(params string[] columnNames) =>
        _validationManager.RemoveValidationRulesAsync(columnNames);

    public Task ReplaceValidationRulesAsync(IReadOnlyDictionary<string, IReadOnlyList<ValidationRule>> columnRules) =>
        _validationManager.ReplaceValidationRulesAsync(columnRules);

    #endregion

    #region Search & Filter - Delegated to Search Manager

    public Task<SearchResults?> SearchAsync(
        string searchText,
        IReadOnlyList<string>? targetColumns = null,
        bool caseSensitive = false,
        bool wholeWord = false,
        TimeSpan timeout = default,
        IProgress<SearchProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _searchManager.SearchAsync(searchText, targetColumns, caseSensitive, wholeWord, timeout, progress, cancellationToken);

    public Task<AdvancedSearchResults?> AdvancedSearchAsync(
        AdvancedSearchCriteria criteria,
        TimeSpan timeout = default,
        IProgress<SearchProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _searchManager.AdvancedSearchAsync(criteria, timeout, progress, cancellationToken);

    public Task AddSearchToHistoryAsync(string searchText) => _searchManager.AddSearchToHistoryAsync(searchText);
    public Task<IReadOnlyList<string>> GetSearchHistoryAsync() => _searchManager.GetSearchHistoryAsync();
    public Task ClearSearchHistoryAsync() => _searchManager.ClearSearchHistoryAsync();

    public Task ApplyFiltersAsync(
        IReadOnlyList<AdvancedFilter> filters,
        TimeSpan timeout = default,
        IProgress<FilterProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _searchManager.ApplyFiltersAsync(filters, timeout, progress, cancellationToken);

    public Task ClearFiltersAsync() => _searchManager.ClearFiltersAsync();
    public Task<IReadOnlyList<AdvancedFilter>> GetActiveFiltersAsync() => _searchManager.GetActiveFiltersAsync();

    public Task ApplySortAsync(
        IReadOnlyList<MultiSortColumn> sortColumns,
        TimeSpan timeout = default,
        IProgress<SortProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        _searchManager.ApplySortAsync(sortColumns, timeout, progress, cancellationToken);

    public Task ClearSortAsync() => _searchManager.ClearSortAsync();
    public Task<IReadOnlyList<MultiSortColumn>> GetActiveSortsAsync() => _searchManager.GetActiveSortsAsync();

    #endregion

    #region Navigation & Selection - Delegated to Navigation Manager

    public Task<CellPosition?> GetSelectedCellAsync() => _navigationManager.GetSelectedCellAsync();
    public Task SetSelectedCellAsync(int row, int column) => _navigationManager.SetSelectedCellAsync(row, column);
    public Task<CellRange?> GetSelectedRangeAsync() => _navigationManager.GetSelectedRangeAsync();
    public Task SetSelectedRangeAsync(CellRange range) => _navigationManager.SetSelectedRangeAsync(range);
    public Task MoveCellSelectionAsync(NavigationDirection direction) => _navigationManager.MoveCellSelectionAsync(direction);
    public Task<bool> IsCellEditingAsync() => _navigationManager.IsCellEditingAsync();
    public Task StartCellEditingAsync(int row, int column) => _navigationManager.StartCellEditingAsync(row, column);
    public Task StopCellEditingAsync(bool saveChanges = true) => _navigationManager.StopCellEditingAsync(saveChanges);
    public Task<CellRange> GetVisibleRangeAsync() => _navigationManager.GetVisibleRangeAsync();

    #endregion

    #region State Queries - Direct Implementation

    public int GetTotalRowCount() => _internalGrid?.RowCount ?? 0;
    public int GetColumnCount() => _internalGrid?.ColumnCount ?? 0;
    public Task<int> GetVisibleRowsCountAsync() => Task.FromResult(_internalGrid?.RowCount ?? 0);
    public int GetMinimumRowCount() => 10; // Default minimum rows
    public int GetActualRowCount() => _internalGrid?.RowCount ?? 0;
    
    public Task<IReadOnlyList<ColumnInfo>> GetColumnsInfoAsync() =>
        Task.FromResult<IReadOnlyList<ColumnInfo>>(Array.Empty<ColumnInfo>());

    #endregion

    #region UI Operations - Direct Implementation

    public Task RefreshUIAsync()
    {
        _logger?.Info("🔄 BRIDGE: Refreshing UI");
        // TODO: Implement UI refresh logic
        return Task.CompletedTask;
    }

    public Task InvalidateUIAsync()
    {
        _logger?.Info("🔄 BRIDGE: Invalidating UI");
        // TODO: Implement UI invalidation logic
        return Task.CompletedTask;
    }

    public void InvalidateLayout()
    {
        _logger?.Info("🔄 BRIDGE: Invalidating layout");
        // TODO: Implement layout invalidation logic
    }

    #endregion

    #region Performance - Delegated to Performance Manager

    public Task<PerformanceMetrics> GetPerformanceMetricsAsync() =>
        _performanceManager.GetPerformanceMetricsAsync();

    public Task OptimizePerformanceAsync() =>
        _performanceManager.OptimizePerformanceAsync();

    #endregion

    #region Disposal - Professional Resource Management

    public void Dispose()
    {
        try
        {
            _logger?.Info("🔧 BRIDGE DISPOSE: Starting disposal of modular managers");

            // Dispose all managers in reverse order of creation
            _performanceManager?.Dispose();
            _navigationManager?.Dispose();
            _searchManager?.Dispose();
            _validationManager?.Dispose();
            _rowManager?.Dispose();
            _exportManager?.Dispose();
            _importManager?.Dispose();
            _initializer?.Dispose();

            // Dispose main grid
            _internalGrid?.Dispose();
            
            _logger?.Info("✅ BRIDGE DISPOSE: All components disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 BRIDGE DISPOSE ERROR: Error during disposal");
        }
    }

    #endregion
}