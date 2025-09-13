using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;

/// <summary>
/// Service responsible for data transformation operations
/// </summary>
internal interface IDataGridTransformationService
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