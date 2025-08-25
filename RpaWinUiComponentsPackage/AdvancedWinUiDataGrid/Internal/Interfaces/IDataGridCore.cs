using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;

/// <summary>
/// Core interface for DataGrid data operations
/// Provides headless data management functionality
/// </summary>
public interface IDataGridCore : IDisposable
{
    #region Properties
    
    /// <summary>
    /// Current number of rows in the DataGrid
    /// </summary>
    int RowCount { get; }
    
    /// <summary>
    /// Current number of columns in the DataGrid
    /// </summary>
    int ColumnCount { get; }
    
    /// <summary>
    /// Is DataGrid initialized and ready for operations
    /// </summary>
    bool IsInitialized { get; }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize DataGrid with column definitions and configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridConfiguration? config = null);
    
    #endregion
    
    #region Data Operations
    
    /// <summary>
    /// Import data from various sources
    /// </summary>
    Task<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional.Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult>> ImportDataAsync(
        object data, 
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportOptions? options = null);
    
    /// <summary>
    /// Export data to various formats
    /// </summary>
    Task<Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportResult>> ExportDataAsync(
        ExportFormat format, 
        string? filePath = null);
    
    #endregion
    
    #region Search, Filter, Sort
    
    /// <summary>
    /// Search for data in the grid
    /// </summary>
    Task<Result<SearchResult>> SearchAsync(
        string query, 
        SearchOptions? options = null);
    
    /// <summary>
    /// Apply filter to the data
    /// </summary>
    Task<Result<FilterResult>> ApplyFilterAsync(FilterCriteria filter);
    
    /// <summary>
    /// Sort data by columns
    /// </summary>
    Task<Result<SortResult>> SortAsync(SortOptions sortOptions);
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validate all data in the grid
    /// </summary>
    Task<Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult>> ValidateAllAsync();
    
    #endregion
}