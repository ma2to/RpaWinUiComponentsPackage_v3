using Microsoft.Extensions.Logging;
using System.Data;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using CorePerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.PerformanceConfiguration;
using NavigationDirection = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.NavigationDirection;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;

/// <summary>
/// Core interface for DataGrid component - supports both UI and headless operations
/// Unified API pattern for professional enterprise applications
/// Zachováva všetky argumenty z pôvodnej dokumentácie
/// IMPORTANT: Validácia a export sa vždy aplikujú na CELÝ DATASET bez ohľadu na cache/disk/memory
/// </summary>
public interface IDataGridComponent
{
    #region Initialization & Configuration
    
    /// <summary>
    /// Initialize DataGrid with full argument support from documentation
    /// Supports both UI and headless scenarios
    /// </summary>
    Task<bool> InitializeAsync(
        IReadOnlyList<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ColumnConfiguration> columns,
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ColorConfiguration? colorConfig = null,
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationConfiguration? validationConfig = null,
        CorePerformanceConfiguration? performanceConfig = null,
        int minimumRows = 10,
        bool enableSort = true,
        bool enableSearch = true,
        bool enableFilter = true,
        bool enableRealtimeValidation = true,
        bool enableBatchValidation = true,
        int maxRowsForOptimization = 100000,
        TimeSpan operationTimeout = default,
        ILogger? logger = null);

    /// <summary>
    /// Is component initialized and ready for operations
    /// </summary>
    bool IsInitialized { get; }

    #endregion

    #region Import Operations - Complete Arguments

    /// <summary>
    /// Import data from dictionary collection - úplné argumenty
    /// </summary>
    Task<ImportResult> ImportFromDictionaryAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Import data from DataTable - úplné argumenty
    /// </summary>
    Task<ImportResult> ImportFromDataTableAsync(
        DataTable dataTable,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Import from Excel file - plné argumenty
    /// </summary>
    Task<ImportResult> ImportFromExcelAsync(
        byte[] excelData,
        string? worksheetName = null,
        bool hasHeaders = true,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Import from CSV - plné argumenty
    /// </summary>
    Task<ImportResult> ImportFromCsvAsync(
        string csvContent,
        string delimiter = ",",
        bool hasHeaders = true,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Import from JSON - plné argumenty
    /// </summary>
    Task<ImportResult> ImportFromJsonAsync(
        string jsonContent,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Import from XML - plné argumenty
    /// </summary>
    Task<ImportResult> ImportFromXmlAsync(
        string xmlContent,
        string? rootElementName = null,
        IReadOnlyList<bool>? checkboxStates = null,
        int startRow = 0,
        ImportMode insertMode = ImportMode.Replace,
        TimeSpan timeout = default,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Export Operations - Complete Arguments
    // IMPORTANT: Export defaultne exportuje CELÝ DATASET (nie len viditeľnú časť)

    /// <summary>
    /// Export to dictionary collection - úplné argumenty
    /// CELÝ DATASET: Exportuje celý dataset bez ohľadu na UI/cache stav
    /// </summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExportToDictionaryAsync(
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null, // null = celý dataset
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export to DataTable - úplné argumenty
    /// CELÝ DATASET: Exportuje celý dataset bez ohľadu na UI/cache stav
    /// </summary>
    Task<DataTable> ExportToDataTableAsync(
        string? tableName = null,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null, // null = celý dataset
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export to Excel - plné argumenty
    /// CELÝ DATASET: Exportuje celý dataset bez ohľadu na UI/cache stav
    /// </summary>
    Task<byte[]> ExportToExcelAsync(
        string worksheetName = "Data",
        bool includeHeaders = true,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null, // null = celý dataset
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export to CSV - plné argumenty
    /// CELÝ DATASET: Exportuje celý dataset bez ohľadu na UI/cache stav
    /// </summary>
    Task<string> ExportToCsvAsync(
        string delimiter = ",",
        bool includeHeaders = true,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null, // null = celý dataset
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export to JSON - plné argumenty
    /// CELÝ DATASET: Exportuje celý dataset bez ohľadu na UI/cache stav
    /// </summary>
    Task<string> ExportToJsonAsync(
        bool prettyPrint = false,
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null, // null = celý dataset
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export to XML - plné argumenty
    /// CELÝ DATASET: Exportuje celý dataset bez ohľadu na UI/cache stav
    /// </summary>
    Task<string> ExportToXmlAsync(
        string rootElementName = "Data",
        string rowElementName = "Row",
        bool includeValidationAlerts = false,
        bool includeEmptyRows = false,
        IReadOnlyList<string>? columnNames = null,
        int startRow = 0,
        int? maxRows = null, // null = celý dataset
        TimeSpan timeout = default,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Row Management

    /// <summary>
    /// Smart delete row - plné argumenty s force delete
    /// </summary>
    Task<bool> DeleteRowAsync(int rowIndex, bool forceDelete = false);

    /// <summary>
    /// Smart delete multiple rows - plné argumenty
    /// </summary>
    Task<bool> DeleteMultipleRowsAsync(
        IReadOnlyList<int> rowIndices, 
        bool forceDelete = false);

    /// <summary>
    /// Can delete specific row (checks minimum constraints)
    /// </summary>
    bool CanDeleteRow(int rowIndex);

    /// <summary>
    /// Get count of rows that can be deleted
    /// </summary>
    int GetDeletableRowsCount();

    /// <summary>
    /// Delete selected rows (UI only)
    /// </summary>
    void DeleteSelectedRows();

    /// <summary>
    /// Delete rows matching predicate
    /// </summary>
    void DeleteRowsWhere(Func<Dictionary<string, object?>, bool> predicate);

    /// <summary>
    /// Clear all data while maintaining structure
    /// </summary>
    Task<bool> ClearDataAsync();

    /// <summary>
    /// Compact rows after deletion
    /// </summary>
    Task<bool> CompactAfterDeletionAsync();

    /// <summary>
    /// Compact rows - remove empty rows, move empty rows to end
    /// </summary>
    Task<bool> CompactRowsAsync();

    /// <summary>
    /// Paste data at specific position - plné argumenty
    /// </summary>
    Task<bool> PasteDataAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        int startRow,
        int startColumn);

    /// <summary>
    /// Check if row is empty
    /// </summary>
    bool IsRowEmpty(int rowIndex);

    /// <summary>
    /// Get last row with data
    /// </summary>
    Task<int> GetLastDataRowAsync();

    #endregion

    #region Validation - Complete Arguments
    // IMPORTANT: Validácia sa VŽDY aplikuje na CELÝ DATASET (nie len viditeľnú časť)

    /// <summary>
    /// Validate all rows with batch processing - plné argumenty
    /// CELÝ DATASET: Validuje celý dataset bez ohľadu na UI/cache/disk stav
    /// </summary>
    Task<BatchValidationResult?> ValidateAllRowsBatchAsync(
        TimeSpan timeout = default,
        IProgress<ValidationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if all non-empty rows are valid
    /// CELÝ DATASET: Kontroluje celý dataset, nie len viditeľnú časť
    /// </summary>
    Task<bool> AreAllNonEmptyRowsValidAsync(bool wholeDataset = true); // default true = celý dataset

    /// <summary>
    /// Update validation UI (no-op in headless mode)
    /// </summary>
    Task UpdateValidationUIAsync();

    /// <summary>
    /// Add validation rules dynamically
    /// </summary>
    Task AddValidationRulesAsync(
        string columnName,
        IReadOnlyList<ValidationRule> rules);

    /// <summary>
    /// Remove validation rules
    /// </summary>
    Task RemoveValidationRulesAsync(params string[] columnNames);

    /// <summary>
    /// Replace validation rules completely
    /// </summary>
    Task ReplaceValidationRulesAsync(
        IReadOnlyDictionary<string, IReadOnlyList<ValidationRule>> columnRules);

    #endregion

    #region Search, Filter, Sort - Complete Arguments

    /// <summary>
    /// Search data with advanced criteria - plné argumenty
    /// </summary>
    Task<SearchResults?> SearchAsync(
        string searchText,
        IReadOnlyList<string>? targetColumns = null,
        bool caseSensitive = false,
        bool wholeWord = false,
        TimeSpan timeout = default,
        IProgress<SearchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Advanced search with complex criteria
    /// </summary>
    Task<AdvancedSearchResults?> AdvancedSearchAsync(
        AdvancedSearchCriteria criteria,
        TimeSpan timeout = default,
        IProgress<SearchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add search to history
    /// </summary>
    Task AddSearchToHistoryAsync(string searchText);

    /// <summary>
    /// Get search history
    /// </summary>
    Task<IReadOnlyList<string>> GetSearchHistoryAsync();

    /// <summary>
    /// Clear search history
    /// </summary>
    Task ClearSearchHistoryAsync();

    /// <summary>
    /// Apply filters to data - plné argumenty
    /// </summary>
    Task ApplyFiltersAsync(
        IReadOnlyList<AdvancedFilter> filters,
        TimeSpan timeout = default,
        IProgress<FilterProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all filters
    /// </summary>
    Task ClearFiltersAsync();

    /// <summary>
    /// Get active filters
    /// </summary>
    Task<IReadOnlyList<AdvancedFilter>> GetActiveFiltersAsync();

    /// <summary>
    /// Apply sorting - plné argumenty
    /// Empty rows always moved to end
    /// </summary>
    Task ApplySortAsync(
        IReadOnlyList<MultiSortColumn> sortColumns,
        TimeSpan timeout = default,
        IProgress<SortProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear sorting
    /// </summary>
    Task ClearSortAsync();

    /// <summary>
    /// Get active sorts
    /// </summary>
    Task<IReadOnlyList<MultiSortColumn>> GetActiveSortsAsync();

    #endregion

    #region Navigation & Selection (UI only)

    /// <summary>
    /// Get currently selected cell (null in headless mode)
    /// </summary>
    Task<CellPosition?> GetSelectedCellAsync();

    /// <summary>
    /// Set selected cell (no-op in headless mode)
    /// </summary>
    Task SetSelectedCellAsync(int row, int column);

    /// <summary>
    /// Get selected range (null in headless mode)
    /// </summary>
    Task<CellRange?> GetSelectedRangeAsync();

    /// <summary>
    /// Set selected range (no-op in headless mode)
    /// </summary>
    Task SetSelectedRangeAsync(CellRange range);

    /// <summary>
    /// Move cell selection (no-op in headless mode)
    /// </summary>
    Task MoveCellSelectionAsync(NavigationDirection direction);

    /// <summary>
    /// Check if cell is being edited
    /// </summary>
    Task<bool> IsCellEditingAsync();

    /// <summary>
    /// Start cell editing (no-op in headless mode)
    /// </summary>
    Task StartCellEditingAsync(int row, int column);

    /// <summary>
    /// Stop cell editing (no-op in headless mode)
    /// </summary>
    Task StopCellEditingAsync(bool saveChanges = true);

    /// <summary>
    /// Get visible range (full range in headless mode)
    /// </summary>
    Task<CellRange> GetVisibleRangeAsync();

    #endregion

    #region State Queries

    /// <summary>
    /// Total number of rows including empty rows
    /// </summary>
    int GetTotalRowCount();

    /// <summary>
    /// Number of columns
    /// </summary>
    int GetColumnCount();

    /// <summary>
    /// Get visible rows count
    /// </summary>
    Task<int> GetVisibleRowsCountAsync();

    /// <summary>
    /// Minimum number of rows maintained
    /// </summary>
    int GetMinimumRowCount();

    /// <summary>
    /// Actual row count with data
    /// </summary>
    int GetActualRowCount();

    /// <summary>
    /// Has any data been imported
    /// </summary>
    bool HasData { get; }

    /// <summary>
    /// Get column information
    /// </summary>
    Task<IReadOnlyList<ColumnInfo>> GetColumnsInfoAsync();

    #endregion

    #region UI Operations (No-op in headless mode)

    /// <summary>
    /// Refresh UI display (no-op in headless mode)
    /// </summary>
    Task RefreshUIAsync();

    /// <summary>
    /// Invalidate UI layout (no-op in headless mode)
    /// </summary>
    Task InvalidateUIAsync();

    /// <summary>
    /// Invalidate layout (no-op in headless mode)
    /// </summary>
    void InvalidateLayout();

    #endregion

    #region Performance & Diagnostics

    /// <summary>
    /// Get performance metrics
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();

    /// <summary>
    /// Optimize performance for current dataset
    /// </summary>
    Task OptimizePerformanceAsync();

    #endregion
}

