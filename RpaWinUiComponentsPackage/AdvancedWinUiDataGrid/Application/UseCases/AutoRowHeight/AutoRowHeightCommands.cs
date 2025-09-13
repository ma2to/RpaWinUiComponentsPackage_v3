using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.AutoRowHeight;

/// <summary>
/// CQRS: Command for calculating row height for specific row
/// ENTERPRISE: Single row height calculation with caching
/// </summary>
internal sealed record CalculateRowHeightCommand
{
    internal required int RowIndex { get; init; }
    internal required Dictionary<string, object?> RowData { get; init; }
    internal required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    internal required UIConfiguration UIConfiguration { get; init; }
    internal required double AvailableWidth { get; init; }
    internal bool UseCache { get; init; } = true;
    
    /// <summary>
    /// Factory method for single row calculation
    /// </summary>
    internal static CalculateRowHeightCommand Create(
        int rowIndex,
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns,
        UIConfiguration uiConfiguration,
        double availableWidth) =>
        new()
        {
            RowIndex = rowIndex,
            RowData = rowData,
            Columns = columns,
            UIConfiguration = uiConfiguration,
            AvailableWidth = availableWidth
        };
}

/// <summary>
/// CQRS: Command for calculating multiple row heights in batch
/// PERFORMANCE: Optimized batch processing for better performance
/// </summary>
internal sealed record CalculateBatchRowHeightsCommand
{
    internal required IReadOnlyList<Dictionary<string, object?>> RowsData { get; init; }
    internal required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    internal required UIConfiguration UIConfiguration { get; init; }
    internal required double AvailableWidth { get; init; }
    internal bool UseCache { get; init; } = true;
    internal int BatchSize { get; init; } = 50;
    internal IProgress<BatchCalculationProgress>? Progress { get; init; }
    
    /// <summary>
    /// Factory method for batch calculation
    /// </summary>
    internal static CalculateBatchRowHeightsCommand Create(
        IReadOnlyList<Dictionary<string, object?>> rowsData,
        IReadOnlyList<ColumnDefinition> columns,
        UIConfiguration uiConfiguration,
        double availableWidth) =>
        new()
        {
            RowsData = rowsData,
            Columns = columns,
            UIConfiguration = uiConfiguration,
            AvailableWidth = availableWidth
        };
}

/// <summary>
/// CQRS: Command for applying calculated row heights to UI
/// UI: Update visual layout with calculated heights
/// </summary>
internal sealed record ApplyRowHeightsCommand
{
    internal required Dictionary<int, double> RowHeights { get; init; }
    internal bool AnimateChanges { get; init; } = false;
    internal TimeSpan? AnimationDuration { get; init; }
    internal bool PreserveSelection { get; init; } = true;
    
    /// <summary>
    /// Factory method for applying heights
    /// </summary>
    internal static ApplyRowHeightsCommand Create(Dictionary<int, double> rowHeights) =>
        new() { RowHeights = rowHeights };
}

/// <summary>
/// CQRS: Command for enabling/disabling auto row height
/// CONFIGURATION: Runtime configuration changes
/// </summary>
internal sealed record ToggleAutoRowHeightCommand
{
    internal required bool Enabled { get; init; }
    internal bool RefreshExistingRows { get; init; } = true;
    internal bool PreserveCustomHeights { get; init; } = false;
    
    /// <summary>
    /// Factory method for toggling auto row height
    /// </summary>
    internal static ToggleAutoRowHeightCommand Enable(bool refreshExisting = true) =>
        new() { Enabled = true, RefreshExistingRows = refreshExisting };
    
    /// <summary>
    /// Factory method for disabling auto row height
    /// </summary>
    internal static ToggleAutoRowHeightCommand Disable(bool preserveCustom = false) =>
        new() { Enabled = false, PreserveCustomHeights = preserveCustom };
}

/// <summary>
/// Progress reporting for batch row height calculations
/// </summary>
internal record BatchCalculationProgress
{
    internal int ProcessedRows { get; init; }
    internal int TotalRows { get; init; }
    internal double CompletionPercentage => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    internal TimeSpan ElapsedTime { get; init; }
    internal TimeSpan? EstimatedTimeRemaining { get; init; }
    internal int CacheHits { get; init; }
    internal int CacheMisses { get; init; }
    
    internal static BatchCalculationProgress Create(int processed, int total, TimeSpan elapsed) =>
        new()
        {
            ProcessedRows = processed,
            TotalRows = total,
            ElapsedTime = elapsed
        };
}