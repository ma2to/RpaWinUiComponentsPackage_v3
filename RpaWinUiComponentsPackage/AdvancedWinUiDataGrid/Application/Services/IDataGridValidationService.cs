using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

/// <summary>
/// Service responsible for data validation operations
/// </summary>
public interface IDataGridValidationService : IDisposable
{
    Task<Result<ValidationError[]>> ValidateRowAsync(
        int rowIndex,
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns);

    Task<Result<ValidationError[]>> ValidateAllRowsAsync(
        IProgress<ValidationProgress>? progress = null);

    Task<Result<ValidationError[]>> ValidateCellAsync(
        int rowIndex,
        string columnName,
        object? value,
        ColumnDefinition columnDefinition);

    ValidationError[] ValidateCellValue(
        object? value,
        ColumnDefinition columnDefinition,
        int rowIndex,
        string columnName);

    Task<Result<ValidationError[]>> ValidateCrossColumnRules(
        Dictionary<string, object?> rowData,
        int rowIndex,
        IReadOnlyList<ColumnDefinition> columns);

    Task<Result<ValidationError[]>> ValidateGlobalRules(
        IReadOnlyList<Dictionary<string, object?>> allData,
        IReadOnlyList<ColumnDefinition> columns);
    
    /// <summary>
    /// Compatibility method for ValidateAllAsync
    /// </summary>
    Task<Result<ValidationError[]>> ValidateAllAsync(IProgress<ValidationProgress>? progress = null);
    
    /// <summary>
    /// Validate single cell with context
    /// </summary>
    Task<Result<ValidationError[]>> ValidateSingleCellAsync(
        string columnName, 
        object? value, 
        Dictionary<string, object?> rowData, 
        int rowIndex);
    
    /// <summary>
    /// Validate cross-column dependencies
    /// </summary>
    Task<Result<ValidationError[]>> ValidateCrossColumnAsync(
        Dictionary<string, object?> rowData, 
        int rowIndex);
    
    /// <summary>
    /// Validate conditional rules
    /// </summary>
    Task<Result<ValidationError[]>> ValidateConditionalAsync(
        Dictionary<string, object?> rowData, 
        int rowIndex);
}