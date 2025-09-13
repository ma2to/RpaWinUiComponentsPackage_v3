using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ExportData;

/// <summary>
/// Command for exporting data from the data grid
/// </summary>
internal record ExportDataCommand
{
    internal bool IncludeValidAlerts { get; init; } = false;
    internal bool ExportOnlyChecked { get; init; } = false;
    internal bool ExportOnlyFiltered { get; init; } = false;
    internal bool RemoveAfter { get; init; } = false;
    internal TimeSpan? Timeout { get; init; }
    internal IProgress<ExportProgress>? ExportProgress { get; init; }
    internal ExportFormat Format { get; init; } = ExportFormat.Dictionary;
    
    // Backward compatibility alias
    internal bool IncludeValidationAlerts => IncludeValidAlerts;

    internal static ExportDataCommand ToDictionary(
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

    internal static ExportDataCommand ToDataTable(
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

internal enum ExportFormat
{
    Dictionary,
    DataTable
}

/// <summary>
/// COMPATIBILITY: Command for exporting data to DataTable
/// </summary>
internal sealed record ExportToDataTableCommand : ExportDataCommand
{
    // Backward compatibility aliases
    internal new bool IncludeValidationAlerts => IncludeValidAlerts;
    internal IProgress<ExportProgress>? Progress => ExportProgress;
    internal static new ExportToDataTableCommand ToDataTable(
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