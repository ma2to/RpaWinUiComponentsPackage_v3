using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Common;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.DataGrid;

/// <summary>
/// Core DataGrid Service Interface - Headless functionality
/// FUNCTIONAL: Pure business logic without UI dependencies
/// DUAL-USE: Can be used by UI components or automation scripts
/// </summary>
internal interface IDataGridService : IDisposable
{
    #region Initialization
    
    /// <summary>
    /// Initialize service with column schema
    /// FUNCTIONAL: Pure initialization with immutable configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(
        IReadOnlyList<DomainColumnDefinition> columns,
        DataGridConfiguration? configuration = null);
    
    /// <summary>
    /// Check if service is initialized
    /// FUNCTIONAL: Pure state query
    /// </summary>
    bool IsInitialized { get; }
    
    #endregion
    
    #region Data Operations
    
    /// <summary>
    /// Import data from dictionaries
    /// FUNCTIONAL: Immutable data transformation pipeline
    /// </summary>
    Task<Result<ImportResult>> ImportDataAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions? options = null);
    
    /// <summary>
    /// Import data from DataTable
    /// FUNCTIONAL: DataTable to immutable transformation
    /// </summary>
    Task<Result<ImportResult>> ImportDataAsync(
        System.Data.DataTable dataTable,
        ImportOptions? options = null);
    
    /// <summary>
    /// Export data to dictionaries
    /// FUNCTIONAL: Pure data extraction with filtering
    /// </summary>
    Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportToDictionariesAsync(
        ExportOptions? options = null);
    
    /// <summary>
    /// Export data to DataTable
    /// FUNCTIONAL: Immutable to DataTable transformation
    /// </summary>
    Task<Result<System.Data.DataTable>> ExportToDataTableAsync(
        ExportOptions? options = null);
    
    #endregion
    
    #region Data Management
    
    /// <summary>
    /// Delete rows by indices
    /// FUNCTIONAL: Immutable deletion with index mapping
    /// </summary>
    Task<Result<DeleteResult>> DeleteRowsAsync(IReadOnlyList<int> rowIndices);
    
    /// <summary>
    /// Clear all data
    /// FUNCTIONAL: Reset to initial state
    /// </summary>
    Task<Result<bool>> ClearDataAsync();
    
    /// <summary>
    /// Validate all data
    /// FUNCTIONAL: Pure validation pipeline
    /// </summary>
    Task<Result<ValidationResult>> ValidateAllAsync();
    
    #endregion
    
    #region State Queries
    
    /// <summary>
    /// Get total row count
    /// FUNCTIONAL: Pure count operation
    /// </summary>
    int RowCount { get; }
    
    /// <summary>
    /// Get column count
    /// FUNCTIONAL: Pure count operation
    /// </summary>
    int ColumnCount { get; }
    
    /// <summary>
    /// Check if service has data
    /// FUNCTIONAL: Pure state check
    /// </summary>
    bool HasData { get; }
    
    /// <summary>
    /// Get current service state
    /// FUNCTIONAL: Immutable state snapshot
    /// </summary>
    DataGridState CurrentState { get; }
    
    #endregion
    
    #region Reactive Subscriptions
    
    /// <summary>
    /// Subscribe to data changes
    /// REACTIVE: Functional reactive programming
    /// </summary>
    IObservable<DataChangeEvent> DataChanges { get; }
    
    /// <summary>
    /// Subscribe to validation changes
    /// REACTIVE: Validation state streams
    /// </summary>
    IObservable<ValidationChangeEvent> ValidationChanges { get; }
    
    #endregion
}