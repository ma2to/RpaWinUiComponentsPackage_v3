using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;

/// <summary>
/// DOCUMENTATION: Core DataGrid interface per specification
/// CLEAN ARCHITECTURE: Separates UI from core data logic exactly as documented
/// ENTERPRISE: Contract for both UI and headless implementations
/// SOLID: Interface Segregation - focused on core data operations only
/// </summary>
public interface IDataGridCore : IDisposable
{
    #region DOCUMENTATION: Core Properties
    
    /// <summary>
    /// DOCUMENTATION: Check if DataGrid is properly initialized
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// DOCUMENTATION: Total number of rows in dataset
    /// PERFORMANCE: Quick access for large datasets
    /// </summary>
    int TotalRows { get; }
    
    /// <summary>
    /// DOCUMENTATION: Total number of columns in dataset
    /// </summary>
    int TotalColumns { get; }
    
    #endregion
    
    #region DOCUMENTATION: Core Initialization
    
    /// <summary>
    /// DOCUMENTATION: Initialize with column definitions and options per specification
    /// ENTERPRISE: Core initialization without UI dependencies
    /// SPECIALIZED COLUMNS: Supports CheckBox, DeleteRow, ValidAlerts per specification
    /// </summary>
    /// <param name="columns">Column definitions including specialized columns</param>
    /// <param name="options">Configuration options for initialization</param>
    /// <returns>Initialization result</returns>
    Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridCoreOptions? options = null);
    
    #endregion
    
    #region DOCUMENTATION: Data Loading
    
    /// <summary>
    /// DOCUMENTATION: Load data from any enumerable source per specification
    /// DYNAMIC: Automatic type detection and conversion per specification
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="data">Data to load</param>
    /// <param name="options">Import options</param>
    /// <returns>Load result</returns>
    Task<Result<bool>> LoadDataAsync<T>(IEnumerable<T> data, ImportOptions? options = null);
    
    #endregion
    
    #region DOCUMENTATION: Import/Export Operations
    
    /// <summary>
    /// DOCUMENTATION: Import from Dictionary collection per specification
    /// ENTERPRISE: High-performance dictionary import with comprehensive options
    /// </summary>
    Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        ImportOptions? options = null);
    
    /// <summary>
    /// DOCUMENTATION: Export to Dictionary collection per specification
    /// ENTERPRISE: High-performance dictionary export with validation alerts
    /// </summary>
    Task<List<Dictionary<string, object?>>> ExportToDictionaryAsync(ExportOptions? options = null);
    
    /// <summary>
    /// DOCUMENTATION: Import from DataTable per specification
    /// ENTERPRISE: High-performance DataTable import with type preservation
    /// </summary>
    Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        ImportOptions? options = null);
    
    /// <summary>
    /// DOCUMENTATION: Export to DataTable per specification
    /// ENTERPRISE: High-performance DataTable export with type preservation
    /// </summary>
    Task<DataTable> ExportToDataTableAsync(ExportOptions? options = null);
    
    #endregion
    
    #region DOCUMENTATION: Smart Row Management
    
    /// <summary>
    /// DOCUMENTATION: Smart row addition per specification
    /// SMART: Automatically manages empty last row per specification
    /// </summary>
    /// <param name="rowData">Data for new row</param>
    /// <param name="insertIndex">Optional insertion index</param>
    /// <returns>Row addition result</returns>
    Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData, int? insertIndex = null);
    
    /// <summary>
    /// DOCUMENTATION: Smart row deletion per specification
    /// SMART: Intelligent deletion with confirmation and minimum row management
    /// </summary>
    /// <param name="rowIndex">Index of row to delete</param>
    /// <param name="requireConfirmation">Whether to require confirmation</param>
    /// <returns>Row deletion result</returns>
    Task<Result<bool>> DeleteRowAsync(int rowIndex, bool requireConfirmation = true);
    
    #endregion
    
    #region DOCUMENTATION: Validation Operations
    
    /// <summary>
    /// DOCUMENTATION: Validate data per specification
    /// SMART: Ignores last empty row per AreAllRowsValid logic
    /// </summary>
    /// <param name="onlyVisible">Validate only visible/filtered rows</param>
    /// <param name="timeout">Validation timeout</param>
    /// <param name="progress">Progress reporting</param>
    /// <returns>Validation errors (empty list if all valid)</returns>
    Task<Result<List<ValidationError>>> ValidateDataAsync(
        bool onlyVisible = false,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? progress = null);
    
    #endregion
    
    #region DOCUMENTATION: Search, Sort, Filter
    
    /// <summary>
    /// DOCUMENTATION: Advanced search per specification
    /// PERFORMANCE: Optimized for 1M+ row datasets
    /// </summary>
    Task<Result<SearchResult>> SearchAsync(string searchText, SearchOptions? options = null);
    
    /// <summary>
    /// DOCUMENTATION: Apply filters per specification
    /// </summary>
    Task<Result<int>> ApplyFiltersAsync(List<FilterExpression> filters);
    
    /// <summary>
    /// DOCUMENTATION: Clear all filters per specification
    /// </summary>
    Task<Result<bool>> ClearFiltersAsync();
    
    #endregion
}

/// <summary>
/// DOCUMENTATION: DataGrid initialization options per specification
/// ENTERPRISE: Simplified options for IDataGridCore interface
/// </summary>
public record DataGridCoreOptions
{
    /// <summary>Color configuration for UI mode</summary>
    public ColorConfiguration? ColorConfiguration { get; init; }
    
    /// <summary>Validation configuration and rules</summary>
    public ValidationConfiguration? ValidationConfiguration { get; init; }
    
    /// <summary>Performance optimization settings</summary>
    public PerformanceConfiguration? PerformanceConfiguration { get; init; }
    
    /// <summary>Enable UI components (false for headless mode)</summary>
    public bool EnableUI { get; init; } = true;
    
    /// <summary>Minimum number of rows to maintain</summary>
    public int MinimumRows { get; init; } = 1;
    
    /// <summary>Enable audit logging</summary>
    public bool EnableAuditLog { get; init; } = true;
    
    /// <summary>Enable performance monitoring</summary>
    public bool EnablePerformanceMonitoring { get; init; } = true;
}