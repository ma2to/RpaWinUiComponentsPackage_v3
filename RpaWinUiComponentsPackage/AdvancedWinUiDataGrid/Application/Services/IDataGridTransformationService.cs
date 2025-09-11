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
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

/// <summary>
/// Service responsible for data transformation operations
/// </summary>
public interface IDataGridTransformationService
{
    List<Dictionary<string, object?>> TransformFromDictionary(
        List<Dictionary<string, object?>> inputData,
        IReadOnlyList<ColumnDefinition> columns);

    List<Dictionary<string, object?>> TransformToDictionary(
        IReadOnlyList<Dictionary<string, object?>> internalData,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeValidAlerts = false);

    List<Dictionary<string, object?>> TransformFromDataTable(
        DataTable dataTable,
        IReadOnlyList<ColumnDefinition> columns);

    DataTable TransformToDataTable(
        IReadOnlyList<Dictionary<string, object?>> data,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeValidAlerts = false);

    Dictionary<string, object?> TransformRowData(
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns,
        bool isImport = true);

    object? TransformValueForImport(
        object? value,
        ColumnDefinition column);

    object? TransformValueForExport(
        object? value,
        ColumnDefinition column);

    bool IsRowEmpty(Dictionary<string, object?> rowData);
    
    // Async versions for the missing methods
    Task<List<Dictionary<string, object?>>> TransformFromDictionaryAsync(
        List<Dictionary<string, object?>> inputData,
        IReadOnlyList<ColumnDefinition> columns);
        
    Task<List<Dictionary<string, object?>>> TransformToDictionaryAsync(
        IReadOnlyList<Dictionary<string, object?>> internalData,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeValidAlerts = false);
}