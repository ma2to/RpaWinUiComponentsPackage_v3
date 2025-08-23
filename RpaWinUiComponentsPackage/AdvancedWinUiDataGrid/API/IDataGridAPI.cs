using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using System.Data;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;
using DomainColorConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColorConfiguration;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.API;

/// <summary>
/// Unified DataGrid API Interface - Works with or without UI
/// DUAL-USE: Same interface for UI and NON-UI scenarios
/// UI methods are no-ops when used headless
/// </summary>
internal interface IDataGridAPI
{
    #region Initialization
    
    /// <summary>
    /// Initialize with column schema and full configuration
    /// UNIFIED: Works for both UI and headless
    /// BACKWARD COMPATIBLE: Supports all original arguments
    /// </summary>
    Task<Result<bool>> InitializeAsync(
        IReadOnlyList<DomainColumnDefinition> columns,
        DataGridConfiguration? configuration = null,
        ILogger? logger = null,
        DomainColorConfiguration? colorConfig = null,
        PerformanceConfiguration? throttlingConfig = null,
        IValidationConfiguration? validationConfig = null);
    
    bool IsInitialized { get; }
    
    #endregion
    
    #region Data Operations - Core Functionality
    
    /// <summary>
    /// Import data from dictionaries
    /// UNIFIED: Core functionality always available
    /// </summary>
    Task<Result<ImportResult>> ImportFromDictionaryAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions? options = null);
    
    /// <summary>
    /// Import data from DataTable
    /// UNIFIED: Core functionality always available
    /// </summary>
    Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        ImportOptions? options = null);
    
    /// <summary>
    /// Export data to dictionaries
    /// UNIFIED: Core functionality always available
    /// </summary>
    Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportToDictionaryAsync(
        ExportOptions? options = null);
    
    /// <summary>
    /// Export data to DataTable
    /// UNIFIED: Core functionality always available
    /// </summary>
    Task<Result<DataTable>> ExportToDataTableAsync(
        ExportOptions? options = null);
    
    #endregion
    
    #region Data Management
    
    /// <summary>
    /// Delete rows by indices
    /// UNIFIED: Core functionality with optional UI update
    /// </summary>
    Task<Result<DeleteResult>> SmartDeleteRowAsync(int rowIndex);
    
    /// <summary>
    /// Delete multiple rows
    /// UNIFIED: Core functionality with optional UI update
    /// </summary>
    Task<Result<DeleteResult>> SmartDeleteRowAsync(IReadOnlyList<int> rowIndices);
    
    /// <summary>
    /// Clear all data
    /// UNIFIED: Core functionality with optional UI update
    /// </summary>
    Task<Result<bool>> ClearAllDataAsync();
    
    /// <summary>
    /// Validate all data
    /// UNIFIED: Core functionality always available
    /// </summary>
    Task<Result<ValidationResult>> ValidateAllRowsBatchAsync();
    
    #endregion
    
    #region UI Operations - Optional (No-op when headless)
    
    /// <summary>
    /// Refresh UI display
    /// UI-OPTIONAL: Updates UI if present, no-op if headless
    /// </summary>
    Task<Result<bool>> RefreshUIAsync();
    
    /// <summary>
    /// Update validation visuals
    /// UI-OPTIONAL: Updates UI validation if present, no-op if headless
    /// </summary>
    Task<Result<bool>> UpdateValidationUIAsync();
    
    /// <summary>
    /// Invalidate layout
    /// UI-OPTIONAL: Triggers layout update if UI present, no-op if headless
    /// </summary>
    Result<bool> InvalidateLayout();
    
    /// <summary>
    /// Compact rows (remove empty rows)
    /// UNIFIED: Core functionality with optional UI update
    /// </summary>
    Task<Result<bool>> CompactRowsAsync();
    
    #endregion
    
    #region State Queries
    
    /// <summary>
    /// Get total row count
    /// UNIFIED: Always available
    /// </summary>
    int GetTotalRowCount();
    
    /// <summary>
    /// Get column count
    /// UNIFIED: Always available
    /// </summary>
    int GetColumnCount();
    
    /// <summary>
    /// Get visible rows count (may differ from total in UI mode)
    /// UNIFIED: Returns total count in headless mode
    /// </summary>
    Task<int> GetVisibleRowsCountAsync();
    
    /// <summary>
    /// Check if has data
    /// UNIFIED: Always available
    /// </summary>
    bool HasData { get; }
    
    /// <summary>
    /// Get last data row index
    /// UNIFIED: Always available
    /// </summary>
    Task<int> GetLastDataRowAsync();
    
    /// <summary>
    /// Get minimum row count (UI configuration)
    /// UNIFIED: Returns 0 in headless mode
    /// </summary>
    int GetMinimumRowCount();
    
    #endregion
    
    #region Validation Convenience
    
    /// <summary>
    /// Check if all non-empty rows are valid
    /// UNIFIED: Always available
    /// </summary>
    /// <param name="wholeDataset">true = entire dataset (default), false = only visible rows</param>
    Task<bool> AreAllNonEmptyRowsValidAsync(bool wholeDataset = true);
    
    #endregion
    
    #region Reactive Subscriptions
    
    /// <summary>
    /// Subscribe to data changes
    /// UNIFIED: Always available
    /// </summary>
    IObservable<DataChangeEvent> DataChanges { get; }
    
    /// <summary>
    /// Subscribe to validation changes
    /// UNIFIED: Always available
    /// </summary>
    IObservable<ValidationChangeEvent> ValidationChanges { get; }
    
    #endregion
}