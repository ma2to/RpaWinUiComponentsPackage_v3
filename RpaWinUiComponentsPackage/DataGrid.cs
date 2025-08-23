using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.Core.Extensions;
using RpaWinUiComponentsPackage.Core.Interfaces;
using RpaWinUiComponentsPackage.Core.Models;
using RpaWinUiComponentsPackage.Core.Bridge;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using System.Data;
using ImportMode = RpaWinUiComponentsPackage.Core.Models.ImportMode;
using CorePerformanceConfiguration = RpaWinUiComponentsPackage.Core.Models.PerformanceConfiguration;

// CLEAN PUBLIC API NAMESPACE - Jediný using statement pre aplikácie
namespace RpaWinUiComponentsPackage.DataGrid;

/// <summary>
/// Professional DataGrid Component - Clean API
/// 
/// POUŽITIE V APLIKÁCII:
/// using RpaWinUiComponentsPackage.DataGrid;
/// 
/// var dataGrid = new DataGrid();
/// await dataGrid.InitializeAsync(columns, colorConfig, validationConfig);
/// 
/// UNIFIED API: Funguje ako UI komponent aj headless pre automatizačné skripty
/// </summary>
public sealed class DataGrid : UserControl, IDataGridComponent
{
    #region Private Fields

    private readonly IDataGridComponent _implementation;
    private readonly ILogger? _logger;
    private bool _isInitialized;

    #endregion

    #region Constructors

    /// <summary>
    /// Create DataGrid with UI support
    /// </summary>
    public DataGrid(ILogger? logger = null)
    {
        _logger = logger;
        _logger?.Info("🔧 Creating DataGrid with UI support");
        
        // Use the new MODULAR AdvancedDataGrid implementation
        _implementation = new DataGridBridge(new AdvancedDataGrid(), _logger);
    }

    /// <summary>
    /// Create headless DataGrid for automation scripts
    /// </summary>
    public static IDataGridComponent CreateHeadless(ILogger? logger = null)
    {
        logger?.Info("🔧 Creating headless DataGrid for automation");
        return new DataGridBridge(new AdvancedDataGrid(), logger);
    }

    #endregion

    #region IDataGridComponent Implementation

    #region Initialization & Configuration

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
        try
        {
            _logger?.Info("🔧 DATAGRID INIT: Initializing with {ColumnCount} columns, minimumRows: {MinimumRows}", 
                columns.Count, minimumRows);

            var result = await _implementation.InitializeAsync(
                columns, colorConfig, validationConfig, performanceConfig,
                minimumRows, enableSort, enableSearch, enableFilter,
                enableRealtimeValidation, enableBatchValidation,
                maxRowsForOptimization, operationTimeout, logger ?? _logger);

            _isInitialized = result;
            
            if (result)
            {
                _logger?.Info("✅ DATAGRID INIT SUCCESS: DataGrid initialized successfully");
            }
            else
            {
                _logger?.Error("❌ DATAGRID INIT FAILED: DataGrid initialization failed");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 DATAGRID INIT ERROR: Error during DataGrid initialization");
            return false;
        }
    }

    public bool IsInitialized => _isInitialized;

    #endregion

    #region Import Operations

    public async Task<ImportResult> ImportFromDictionaryAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📥 IMPORT DICT: Importing {RowCount} rows from dictionary, mode: {Mode}", data.Count, insertMode);
        return await _implementation.ImportFromDictionaryAsync(data, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);
    }

    public async Task<ImportResult> ImportFromDataTableAsync(
        DataTable dataTable,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📥 Importing {RowCount} rows from DataTable, mode: {Mode}", dataTable.Rows.Count, insertMode);
        return await _implementation.ImportFromDataTableAsync(dataTable, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);
    }

    public async Task<ImportResult> ImportFromExcelAsync(
        byte[] excelData,
        string? worksheetName = null,
        bool hasHeaders = true,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📥 Importing from Excel, worksheet: {Worksheet}, hasHeaders: {HasHeaders}", worksheetName, hasHeaders);
        return await _implementation.ImportFromExcelAsync(excelData, worksheetName, hasHeaders, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);
    }

    public async Task<ImportResult> ImportFromCsvAsync(
        string csvContent,
        string delimiter = ",",
        bool hasHeaders = true,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📥 Importing from CSV, delimiter: {Delimiter}, hasHeaders: {HasHeaders}", delimiter, hasHeaders);
        return await _implementation.ImportFromCsvAsync(csvContent, delimiter, hasHeaders, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);
    }

    public async Task<ImportResult> ImportFromJsonAsync(
        string jsonContent,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📥 Importing from JSON");
        return await _implementation.ImportFromJsonAsync(jsonContent, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);
    }

    public async Task<ImportResult> ImportFromXmlAsync(
        string xmlContent,
        string? rootElementName = null,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📥 Importing from XML, root: {Root}", rootElementName);
        return await _implementation.ImportFromXmlAsync(xmlContent, rootElementName, checkboxStates, startRow, insertMode, timeout, progress, cancellationToken);
    }

    #endregion

    #region Export Operations

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExportToDictionaryAsync(
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📤 Exporting to dictionary, includeEmpty: {IncludeEmpty}, maxRows: {MaxRows}", includeEmptyRows, maxRows);
        return await _implementation.ExportToDictionaryAsync(includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);
    }

    public async Task<DataTable> ExportToDataTableAsync(
        string? tableName = null,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📤 Exporting to DataTable, table: {TableName}, maxRows: {MaxRows}", tableName, maxRows);
        return await _implementation.ExportToDataTableAsync(tableName, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);
    }

    public async Task<byte[]> ExportToExcelAsync(
        string worksheetName = "Data",
        bool includeHeaders = true,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📤 Exporting to Excel, worksheet: {Worksheet}", worksheetName);
        return await _implementation.ExportToExcelAsync(worksheetName, includeHeaders, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);
    }

    public async Task<string> ExportToCsvAsync(
        string delimiter = ",",
        bool includeHeaders = true,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📤 Exporting to CSV, delimiter: {Delimiter}", delimiter);
        return await _implementation.ExportToCsvAsync(delimiter, includeHeaders, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);
    }

    public async Task<string> ExportToJsonAsync(
        bool prettyPrint = false,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📤 Exporting to JSON, prettyPrint: {PrettyPrint}", prettyPrint);
        return await _implementation.ExportToJsonAsync(prettyPrint, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);
    }

    public async Task<string> ExportToXmlAsync(
        string rootElementName = "Data",
        string rowElementName = "Row",
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null,
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("📤 Exporting to XML, root: {Root}", rootElementName);
        return await _implementation.ExportToXmlAsync(rootElementName, rowElementName, includeValidationAlerts, includeEmptyRows, columnNames, startRow, maxRows, timeout, progress, cancellationToken);
    }

    #endregion

    #region Row Management

    public async Task<bool> DeleteRowAsync(int rowIndex, bool forceDelete = false)
    {
        _logger?.Info("🗑️ Deleting row {RowIndex}, force: {Force}", rowIndex, forceDelete);
        return await _implementation.DeleteRowAsync(rowIndex, forceDelete);
    }

    public async Task<bool> DeleteMultipleRowsAsync(IReadOnlyList<int> rowIndices, bool forceDelete = false)
    {
        _logger?.Info("🗑️ Deleting {Count} rows, force: {Force}", rowIndices.Count, forceDelete);
        return await _implementation.DeleteMultipleRowsAsync(rowIndices, forceDelete);
    }

    public bool CanDeleteRow(int rowIndex) => _implementation.CanDeleteRow(rowIndex);
    public int GetDeletableRowsCount() => _implementation.GetDeletableRowsCount();
    public void DeleteSelectedRows() => _implementation.DeleteSelectedRows();
    public void DeleteRowsWhere(Func<Dictionary<string, object?>, bool> predicate) => _implementation.DeleteRowsWhere(predicate);
    public async Task<bool> ClearDataAsync() => await _implementation.ClearDataAsync();
    public async Task<bool> CompactAfterDeletionAsync() => await _implementation.CompactAfterDeletionAsync();
    public async Task<bool> CompactRowsAsync() => await _implementation.CompactRowsAsync();

    public async Task<bool> PasteDataAsync(IReadOnlyList<IReadOnlyDictionary<string, object?>> data, int startRow, int startColumn)
    {
        _logger?.Info("📋 Pasting {Count} rows at ({Row}, {Column})", data.Count, startRow, startColumn);
        return await _implementation.PasteDataAsync(data, startRow, startColumn);
    }

    public bool IsRowEmpty(int rowIndex) => _implementation.IsRowEmpty(rowIndex);
    public async Task<int> GetLastDataRowAsync() => await _implementation.GetLastDataRowAsync();

    #endregion

    #region Validation

    public async Task<BatchValidationResult?> ValidateAllRowsBatchAsync(
        TimeSpan timeout = default,
        IProgress<ValidationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info("✅ Starting batch validation of entire dataset");
        return await _implementation.ValidateAllRowsBatchAsync(timeout, progress, cancellationToken);
    }

    public async Task<bool> AreAllNonEmptyRowsValidAsync(bool wholeDataset = true)
    {
        _logger?.Info("✅ Checking if all non-empty rows are valid, wholeDataset: {WholeDataset}", wholeDataset);
        return await _implementation.AreAllNonEmptyRowsValidAsync(wholeDataset);
    }

    public async Task UpdateValidationUIAsync() => await _implementation.UpdateValidationUIAsync();

    public async Task AddValidationRulesAsync(string columnName, IReadOnlyList<ValidationRule> rules)
    {
        _logger?.Info("✅ Adding {Count} validation rules for column {Column}", rules.Count, columnName);
        await _implementation.AddValidationRulesAsync(columnName, rules);
    }

    public async Task RemoveValidationRulesAsync(params string[] columnNames)
    {
        _logger?.Info("✅ Removing validation rules for columns: {Columns}", string.Join(", ", columnNames));
        await _implementation.RemoveValidationRulesAsync(columnNames);
    }

    public async Task ReplaceValidationRulesAsync(IReadOnlyDictionary<string, IReadOnlyList<ValidationRule>> columnRules)
    {
        _logger?.Info("✅ Replacing validation rules for {Count} columns", columnRules.Count);
        await _implementation.ReplaceValidationRulesAsync(columnRules);
    }

    #endregion

    #region Search, Filter, Sort

    public async Task<SearchResults?> SearchAsync(string searchText, IReadOnlyList<string>? targetColumns = null, bool caseSensitive = false, bool wholeWord = false, TimeSpan timeout = default, IProgress<SearchProgress>? progress = null, CancellationToken cancellationToken = default) => 
        await _implementation.SearchAsync(searchText, targetColumns, caseSensitive, wholeWord, timeout, progress, cancellationToken);

    public async Task<AdvancedSearchResults?> AdvancedSearchAsync(AdvancedSearchCriteria criteria, TimeSpan timeout = default, IProgress<SearchProgress>? progress = null, CancellationToken cancellationToken = default) => 
        await _implementation.AdvancedSearchAsync(criteria, timeout, progress, cancellationToken);

    public async Task AddSearchToHistoryAsync(string searchText) => await _implementation.AddSearchToHistoryAsync(searchText);
    public async Task<IReadOnlyList<string>> GetSearchHistoryAsync() => await _implementation.GetSearchHistoryAsync();
    public async Task ClearSearchHistoryAsync() => await _implementation.ClearSearchHistoryAsync();

    public async Task ApplyFiltersAsync(IReadOnlyList<AdvancedFilter> filters, TimeSpan timeout = default, IProgress<FilterProgress>? progress = null, CancellationToken cancellationToken = default) => 
        await _implementation.ApplyFiltersAsync(filters, timeout, progress, cancellationToken);

    public async Task ClearFiltersAsync() => await _implementation.ClearFiltersAsync();
    public async Task<IReadOnlyList<AdvancedFilter>> GetActiveFiltersAsync() => await _implementation.GetActiveFiltersAsync();

    public async Task ApplySortAsync(IReadOnlyList<MultiSortColumn> sortColumns, TimeSpan timeout = default, IProgress<SortProgress>? progress = null, CancellationToken cancellationToken = default) => 
        await _implementation.ApplySortAsync(sortColumns, timeout, progress, cancellationToken);

    public async Task ClearSortAsync() => await _implementation.ClearSortAsync();
    public async Task<IReadOnlyList<MultiSortColumn>> GetActiveSortsAsync() => await _implementation.GetActiveSortsAsync();

    #endregion

    #region Navigation & Selection

    public async Task<CellPosition?> GetSelectedCellAsync() => await _implementation.GetSelectedCellAsync();
    public async Task SetSelectedCellAsync(int row, int column) => await _implementation.SetSelectedCellAsync(row, column);
    public async Task<CellRange?> GetSelectedRangeAsync() => await _implementation.GetSelectedRangeAsync();
    public async Task SetSelectedRangeAsync(CellRange range) => await _implementation.SetSelectedRangeAsync(range);
    public async Task MoveCellSelectionAsync(NavigationDirection direction) => await _implementation.MoveCellSelectionAsync(direction);
    public async Task<bool> IsCellEditingAsync() => await _implementation.IsCellEditingAsync();
    public async Task StartCellEditingAsync(int row, int column) => await _implementation.StartCellEditingAsync(row, column);
    public async Task StopCellEditingAsync(bool saveChanges = true) => await _implementation.StopCellEditingAsync(saveChanges);
    public async Task<CellRange> GetVisibleRangeAsync() => await _implementation.GetVisibleRangeAsync();

    #endregion

    #region State Queries

    public int GetTotalRowCount() => _implementation.GetTotalRowCount();
    public int GetColumnCount() => _implementation.GetColumnCount();
    public async Task<int> GetVisibleRowsCountAsync() => await _implementation.GetVisibleRowsCountAsync();
    public int GetMinimumRowCount() => _implementation.GetMinimumRowCount();
    public int GetActualRowCount() => _implementation.GetActualRowCount();
    public bool HasData => _implementation.HasData;
    public async Task<IReadOnlyList<ColumnInfo>> GetColumnsInfoAsync() => await _implementation.GetColumnsInfoAsync();

    #endregion

    #region UI Operations

    public async Task RefreshUIAsync() => await _implementation.RefreshUIAsync();
    public async Task InvalidateUIAsync() => await _implementation.InvalidateUIAsync();
    public void InvalidateLayout() => _implementation.InvalidateLayout();

    #endregion

    #region Performance & Diagnostics

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync() => await _implementation.GetPerformanceMetricsAsync();
    public async Task OptimizePerformanceAsync() => await _implementation.OptimizePerformanceAsync();

    #endregion

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_implementation is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    #endregion
}

#region Professional Implementation Bridge - Now Modular

/// <summary>
/// PROFESSIONAL MODULAR Bridge implementation moved to separate specialized managers
/// ARCHITECTURE: Core/Bridge/DataGridBridge.cs with 8 specialized managers
/// BENEFITS: 
/// - Single Responsibility Principle
/// - Testable components  
/// - Clean separation of concerns
/// - Professional error handling
/// - Dependency injection ready
/// </summary>
// NOTE: DataGridBridge implementation moved to:
// - Core/Bridge/DataGridBridge.cs (main coordinator)
// - Core/Bridge/DataGridBridgeInitializer.cs (initialization)
// - Core/Bridge/DataGridBridgeImportManager.cs (import operations)
// - Core/Bridge/DataGridBridgeExportManager.cs (export operations)  
// - Core/Bridge/DataGridBridgeRowManager.cs (row operations)
// - Core/Bridge/DataGridBridgeValidationManager.cs (validation)
// - Core/Bridge/DataGridBridgeSearchManager.cs (search/filter/sort)
// - Core/Bridge/DataGridBridgeNavigationManager.cs (navigation/selection)
// 
// PROFESSIONAL MODULAR ARCHITECTURE:
// The old 160+ lines DataGridBridge god-level class has been completely
// refactored into 8 specialized, testable managers following Single Responsibility Principle.
//
// This demonstrates transformation from monolithic to professional modular design.

#endregion