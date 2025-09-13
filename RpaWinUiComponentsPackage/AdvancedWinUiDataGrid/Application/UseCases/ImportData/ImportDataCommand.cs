using System;
using System.Collections.Generic;
using System.Data;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ImportData;

/// <summary>
/// Command for importing data into the data grid
/// </summary>
internal record ImportDataCommand
{
    internal List<Dictionary<string, object?>>? DictionaryData { get; init; }
    internal DataTable? DataTableData { get; init; }
    internal Dictionary<int, bool>? CheckboxStates { get; init; }
    internal int StartRow { get; init; } = 1;
    internal ImportMode Mode { get; init; } = ImportMode.Replace;
    internal TimeSpan? Timeout { get; init; }
    internal IProgress<ValidationProgress>? ValidationProgress { get; init; }
    
    // Backward compatibility aliases
    internal List<Dictionary<string, object?>>? Data => DictionaryData;
    internal bool ValidateBeforeImport { get; init; } = true;

    internal static ImportDataCommand FromDictionary(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        return new ImportDataCommand
        {
            DictionaryData = data,
            CheckboxStates = checkboxStates,
            StartRow = startRow,
            Mode = mode,
            Timeout = timeout,
            ValidationProgress = validationProgress
        };
    }

    internal static ImportDataCommand FromDataTable(
        DataTable dataTable,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        return new ImportDataCommand
        {
            DataTableData = dataTable,
            CheckboxStates = checkboxStates,
            StartRow = startRow,
            Mode = mode,
            Timeout = timeout,
            ValidationProgress = validationProgress
        };
    }
}

/// <summary>
/// COMPATIBILITY: Command for importing data from DataTable
/// </summary>
internal sealed record ImportFromDataTableCommand : ImportDataCommand
{
    // Backward compatibility aliases
    internal DataTable? DataTable => DataTableData;
    internal IProgress<ValidationProgress>? Progress => ValidationProgress;
    internal static new ImportFromDataTableCommand FromDataTable(
        DataTable dataTable,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? validationProgress = null)
    {
        return new ImportFromDataTableCommand
        {
            DataTableData = dataTable,
            CheckboxStates = checkboxStates,
            StartRow = startRow,
            Mode = mode,
            Timeout = timeout,
            ValidationProgress = validationProgress
        };
    }
}