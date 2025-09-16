using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;

/// <summary>
/// Core data grid service interface providing all essential operations
/// </summary>
internal interface IDataGridService : IDisposable
{
    Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null);

    Task<Result<ImportResult>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null);

    Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null);

    Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null);

    Task<Result<DataTable>> ExportToDataTableAsync(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null);

    Task<Result<SearchResult>> SearchAsync(
        string searchTerm,
        SearchOptions? options = null);

    Task<Result<bool>> ApplyFiltersAsync(
        IReadOnlyList<FilterExpression> filters);

    Task<Result<bool>> SortAsync(
        string columnName,
        SortDirection direction);

    Task<Result<bool>> ClearFiltersAsync();

    Task<Result<ValidationError[]>> ValidateAllAsync(
        IProgress<ValidationProgress>? progress = null);

    Task<Result<bool>> AddRowAsync(Dictionary<string, object?> rowData);
    Task<Result<bool>> UpdateRowAsync(int rowIndex, Dictionary<string, object?> rowData);
    Task<Result<bool>> DeleteRowAsync(int rowIndex);

    /// <summary>
    /// Check if a row is empty (all values null or empty)
    /// </summary>
    Task<Result<bool>> IsRowEmptyAsync(int rowIndex);

    /// <summary>
    /// Get row data by index
    /// </summary>
    Task<Dictionary<string, object?>?> GetRowDataAsync(int rowIndex);

    /// <summary>
    /// PROFESSIONAL: Delete rows that meet specified validation criteria
    /// ENTERPRISE: Batch operation with progress reporting and rollback support
    /// </summary>
    Task<Result<ValidationBasedDeleteResult>> DeleteRowsWithValidationAsync(
        ValidationDeletionCriteria validationCriteria,
        ValidationDeletionOptions? options = null);

    /// <summary>
    /// ENTERPRISE: Check if all non-empty rows pass validation
    /// COMPREHENSIVE: Validates complete dataset including cached/disk data
    /// </summary>
    Task<Result<bool>> AreAllNonEmptyRowsValidAsync(bool onlyFiltered = false);

    int GetRowCount();
    int GetColumnCount();

    /// <summary>
    /// Get current grid state
    /// </summary>
    GridState? CurrentState { get; }

    /// <summary>
    /// Get column name by index
    /// </summary>
    Task<Result<string>> GetColumnNameAsync(int columnIndex);
}