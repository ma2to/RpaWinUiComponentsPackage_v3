using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ExportData;

/// <summary>
/// Command for exporting data from the data grid
/// </summary>
public record ExportDataCommand
{
    public bool IncludeValidAlerts { get; init; } = false;
    public bool ExportOnlyChecked { get; init; } = false;
    public bool ExportOnlyFiltered { get; init; } = false;
    public bool RemoveAfter { get; init; } = false;
    public TimeSpan? Timeout { get; init; }
    public IProgress<ExportProgress>? ExportProgress { get; init; }
    public ExportFormat Format { get; init; } = ExportFormat.Dictionary;
    
    // Backward compatibility alias
    public bool IncludeValidationAlerts => IncludeValidAlerts;

    public static ExportDataCommand ToDictionary(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        return new ExportDataCommand
        {
            IncludeValidAlerts = includeValidAlerts,
            ExportOnlyChecked = exportOnlyChecked,
            ExportOnlyFiltered = exportOnlyFiltered,
            RemoveAfter = removeAfter,
            Timeout = timeout,
            ExportProgress = exportProgress,
            Format = ExportFormat.Dictionary
        };
    }

    public static ExportDataCommand ToDataTable(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        return new ExportDataCommand
        {
            IncludeValidAlerts = includeValidAlerts,
            ExportOnlyChecked = exportOnlyChecked,
            ExportOnlyFiltered = exportOnlyFiltered,
            RemoveAfter = removeAfter,
            Timeout = timeout,
            ExportProgress = exportProgress,
            Format = ExportFormat.DataTable
        };
    }
}

public enum ExportFormat
{
    Dictionary,
    DataTable
}

/// <summary>
/// COMPATIBILITY: Command for exporting data to DataTable
/// </summary>
public sealed record ExportToDataTableCommand : ExportDataCommand
{
    // Backward compatibility aliases
    public new bool IncludeValidationAlerts => IncludeValidAlerts;
    public IProgress<ExportProgress>? Progress => ExportProgress;
    public static new ExportToDataTableCommand ToDataTable(
        bool includeValidAlerts = false,
        bool exportOnlyChecked = false,
        bool exportOnlyFiltered = false,
        bool removeAfter = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? exportProgress = null)
    {
        return new ExportToDataTableCommand
        {
            IncludeValidAlerts = includeValidAlerts,
            ExportOnlyChecked = exportOnlyChecked,
            ExportOnlyFiltered = exportOnlyFiltered,
            RemoveAfter = removeAfter,
            Timeout = timeout,
            ExportProgress = exportProgress,
            Format = ExportFormat.DataTable
        };
    }
}