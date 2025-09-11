using System;
using System.Collections.Generic;
using System.Data;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ImportData;

/// <summary>
/// Command for importing data into the data grid
/// </summary>
public record ImportDataCommand
{
    public List<Dictionary<string, object?>>? DictionaryData { get; init; }
    public DataTable? DataTableData { get; init; }
    public Dictionary<int, bool>? CheckboxStates { get; init; }
    public int StartRow { get; init; } = 1;
    public ImportMode Mode { get; init; } = ImportMode.Replace;
    public TimeSpan? Timeout { get; init; }
    public IProgress<ValidationProgress>? ValidationProgress { get; init; }
    
    // Backward compatibility aliases
    public List<Dictionary<string, object?>>? Data => DictionaryData;
    public bool ValidateBeforeImport { get; init; } = true;

    public static ImportDataCommand FromDictionary(
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

    public static ImportDataCommand FromDataTable(
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
public sealed record ImportFromDataTableCommand : ImportDataCommand
{
    // Backward compatibility aliases
    public DataTable? DataTable => DataTableData;
    public IProgress<ValidationProgress>? Progress => ValidationProgress;
    public static new ImportFromDataTableCommand FromDataTable(
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