using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.AutoRowHeight;

/// <summary>
/// CQRS: Command for calculating row height for specific row
/// ENTERPRISE: Single row height calculation with caching
/// </summary>
public sealed record CalculateRowHeightCommand
{
    public required int RowIndex { get; init; }
    public required Dictionary<string, object?> RowData { get; init; }
    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    public required UIConfiguration UIConfiguration { get; init; }
    public required double AvailableWidth { get; init; }
    public bool UseCache { get; init; } = true;
    
    /// <summary>
    /// Factory method for single row calculation
    /// </summary>
    public static CalculateRowHeightCommand Create(
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
public sealed record CalculateBatchRowHeightsCommand
{
    public required IReadOnlyList<Dictionary<string, object?>> RowsData { get; init; }
    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }
    public required UIConfiguration UIConfiguration { get; init; }
    public required double AvailableWidth { get; init; }
    public bool UseCache { get; init; } = true;
    public int BatchSize { get; init; } = 50;
    public IProgress<BatchCalculationProgress>? Progress { get; init; }
    
    /// <summary>
    /// Factory method for batch calculation
    /// </summary>
    public static CalculateBatchRowHeightsCommand Create(
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
public sealed record ApplyRowHeightsCommand
{
    public required Dictionary<int, double> RowHeights { get; init; }
    public bool AnimateChanges { get; init; } = false;
    public TimeSpan? AnimationDuration { get; init; }
    public bool PreserveSelection { get; init; } = true;
    
    /// <summary>
    /// Factory method for applying heights
    /// </summary>
    public static ApplyRowHeightsCommand Create(Dictionary<int, double> rowHeights) =>
        new() { RowHeights = rowHeights };
}

/// <summary>
/// CQRS: Command for enabling/disabling auto row height
/// CONFIGURATION: Runtime configuration changes
/// </summary>
public sealed record ToggleAutoRowHeightCommand
{
    public required bool Enabled { get; init; }
    public bool RefreshExistingRows { get; init; } = true;
    public bool PreserveCustomHeights { get; init; } = false;
    
    /// <summary>
    /// Factory method for toggling auto row height
    /// </summary>
    public static ToggleAutoRowHeightCommand Enable(bool refreshExisting = true) =>
        new() { Enabled = true, RefreshExistingRows = refreshExisting };
    
    /// <summary>
    /// Factory method for disabling auto row height
    /// </summary>
    public static ToggleAutoRowHeightCommand Disable(bool preserveCustom = false) =>
        new() { Enabled = false, PreserveCustomHeights = preserveCustom };
}

/// <summary>
/// Progress reporting for batch row height calculations
/// </summary>
public record BatchCalculationProgress
{
    public int ProcessedRows { get; init; }
    public int TotalRows { get; init; }
    public double CompletionPercentage => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public TimeSpan ElapsedTime { get; init; }
    public TimeSpan? EstimatedTimeRemaining { get; init; }
    public int CacheHits { get; init; }
    public int CacheMisses { get; init; }
    
    public static BatchCalculationProgress Create(int processed, int total, TimeSpan elapsed) =>
        new()
        {
            ProcessedRows = processed,
            TotalRows = total,
            ElapsedTime = elapsed
        };
}