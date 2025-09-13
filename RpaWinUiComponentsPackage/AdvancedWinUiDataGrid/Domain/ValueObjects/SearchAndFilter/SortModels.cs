using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object representing sort operation results
/// ENTERPRISE: Comprehensive sort result with metadata
/// </summary>
internal record SortResult
{
    public IReadOnlyList<int> SortedIndices { get; init; } = [];
    public int TotalRows { get; init; }
    public SortCriteria? AppliedCriteria { get; init; }
    public bool IsSuccess { get; init; }
    public TimeSpan? ProcessingTime { get; init; }
    
    // Compatibility properties for existing code
    public IReadOnlyList<GridRow> SortedRows { get; init; } = [];
    public TimeSpan SortDuration => ProcessingTime ?? TimeSpan.Zero;
    public SortAlgorithm UsedAlgorithm => AppliedCriteria?.Algorithm ?? SortAlgorithm.QuickSort;
    
    public static SortResult Success(IReadOnlyList<int> sortedIndices, int totalRows, SortCriteria? criteria, TimeSpan? processingTime = null) =>
        new()
        {
            SortedIndices = sortedIndices,
            TotalRows = totalRows,
            AppliedCriteria = criteria,
            IsSuccess = true,
            ProcessingTime = processingTime
        };

    public static SortResult CreateSuccess(
        IReadOnlyList<GridRow> sortedRows,
        int totalRows,
        TimeSpan duration,
        SortAlgorithm algorithm,
        string message) =>
        new()
        {
            SortedRows = sortedRows,
            SortedIndices = sortedRows.Select((_, index) => index).ToArray(),
            TotalRows = totalRows,
            IsSuccess = true,
            ProcessingTime = duration,
            AppliedCriteria = new SortCriteria { Algorithm = algorithm }
        };
        
    public static SortResult Failed() =>
        new() { IsSuccess = false };
}

/// <summary>
/// DDD: Value object for sort criteria
/// </summary>
internal record SortCriteria
{
    public string ColumnName { get; init; } = string.Empty;
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
    public int Priority { get; init; } = 0;
    public SortAlgorithm Algorithm { get; init; } = SortAlgorithm.QuickSort;
    
    public static SortCriteria Create(string columnName, SortDirection direction = SortDirection.Ascending, int priority = 0) =>
        new()
        {
            ColumnName = columnName,
            Direction = direction,
            Priority = priority
        };
}

/// <summary>
/// DDD: Value object for current sort state
/// </summary>
internal record CurrentSortState
{
    public IReadOnlyList<SortCriteria> ActiveSorts { get; init; } = [];
    public bool HasActiveSorting => ActiveSorts.Count > 0;
    public bool IsSorted => HasActiveSorting;
    public string? PrimaryColumn => ActiveSorts.FirstOrDefault()?.ColumnName;
    public SortDirection? PrimaryDirection => ActiveSorts.FirstOrDefault()?.Direction;
    
    public static CurrentSortState Empty => new();
    
    public static CurrentSortState Create(
        IReadOnlyList<SortCriteria> sorts,
        bool isSorted,
        string? primaryColumn,
        SortDirection primaryDirection) =>
        new() 
        {
            ActiveSorts = sorts
        };
    
    public static CurrentSortState WithSorts(IReadOnlyList<SortCriteria> sorts) =>
        new() { ActiveSorts = sorts };
}

/// <summary>
/// DDD: Value object for sort statistics and performance metrics
/// </summary>
internal record SortStatistics
{
    public int TotalOperations { get; init; }
    public int TotalSorts => TotalOperations;
    public TimeSpan AverageProcessingTime { get; init; }
    public TimeSpan AverageSortTime => AverageProcessingTime;
    public TimeSpan LastProcessingTime { get; init; }
    public SortAlgorithm MostUsedAlgorithm { get; init; }
    public int RowsProcessedTotal { get; init; }
    public Dictionary<string, int> ColumnSortCounts { get; init; } = new();
    
    public static SortStatistics Default => new();
}

/// <summary>
/// ENTERPRISE: Sort algorithm enumeration
/// PERFORMANCE: Choose optimal algorithm based on data characteristics
/// </summary>
internal enum SortAlgorithm
{
    QuickSort,    // Fast for random data
    MergeSort,    // Stable sorting, good for partially sorted data
    HeapSort,     // Consistent O(n log n), low memory usage
    TimSort,      // Hybrid stable sort, excellent for real-world data
    RadixSort     // Linear time for numeric data
}