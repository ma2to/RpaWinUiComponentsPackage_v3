using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object for sort progress tracking
/// ENTERPRISE: Progress reporting for long-running sort operations
/// </summary>
internal record SortProgress
{
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public SortStatus Status { get; init; } = SortStatus.NotStarted;
    public string? CurrentOperation { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EstimatedCompletion { get; init; }
    
    /// <summary>Percentage of completion (0-100)</summary>
    public double PercentageComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    
    /// <summary>Elapsed time since start</summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    
    /// <summary>Processing rate (rows per second)</summary>
    public double ProcessingRate
    {
        get
        {
            var elapsed = ElapsedTime.TotalSeconds;
            return elapsed > 0 ? ProcessedRows / elapsed : 0;
        }
    }
    
    public static SortProgress Create(
        int totalRows,
        int processedRows,
        SortStatus status,
        string? currentOperation = null,
        DateTime? startTime = null) =>
        new()
        {
            TotalRows = totalRows,
            ProcessedRows = processedRows,
            Status = status,
            CurrentOperation = currentOperation,
            StartTime = startTime ?? DateTime.UtcNow,
            EstimatedCompletion = CalculateEstimatedCompletion(totalRows, processedRows, startTime ?? DateTime.UtcNow)
        };
        
    private static DateTime? CalculateEstimatedCompletion(int totalRows, int processedRows, DateTime startTime)
    {
        if (processedRows <= 0 || totalRows <= processedRows) return null;
        
        var elapsed = DateTime.UtcNow - startTime;
        var rate = processedRows / elapsed.TotalSeconds;
        var remainingRows = totalRows - processedRows;
        var remainingSeconds = remainingRows / rate;
        
        return DateTime.UtcNow.AddSeconds(remainingSeconds);
    }
}

/// <summary>
/// DDD: Value object for filter progress tracking
/// ENTERPRISE: Progress reporting for long-running filter operations
/// </summary>
internal record FilterProgress
{
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int FilteredRows { get; init; }
    public FilterStatus Status { get; init; } = FilterStatus.NotStarted;
    public string? CurrentOperation { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EstimatedCompletion { get; init; }
    
    /// <summary>Percentage of completion (0-100)</summary>
    public double PercentageComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    
    /// <summary>Filter rate (rows passing filter / total processed)</summary>
    public double FilterRate => ProcessedRows > 0 ? (double)FilteredRows / ProcessedRows * 100 : 0;
    
    /// <summary>Elapsed time since start</summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    
    public static FilterProgress Create(
        int totalRows,
        int processedRows,
        int filteredRows,
        FilterStatus status,
        string? currentOperation = null,
        DateTime? startTime = null) =>
        new()
        {
            TotalRows = totalRows,
            ProcessedRows = processedRows,
            FilteredRows = filteredRows,
            Status = status,
            CurrentOperation = currentOperation,
            StartTime = startTime ?? DateTime.UtcNow,
            EstimatedCompletion = CalculateEstimatedCompletion(totalRows, processedRows, startTime ?? DateTime.UtcNow)
        };
        
    private static DateTime? CalculateEstimatedCompletion(int totalRows, int processedRows, DateTime startTime)
    {
        if (processedRows <= 0 || totalRows <= processedRows) return null;
        
        var elapsed = DateTime.UtcNow - startTime;
        var rate = processedRows / elapsed.TotalSeconds;
        var remainingRows = totalRows - processedRows;
        var remainingSeconds = remainingRows / rate;
        
        return DateTime.UtcNow.AddSeconds(remainingSeconds);
    }
}

/// <summary>
/// DDD: Status enumeration for sort operations
/// </summary>
internal enum SortStatus
{
    NotStarted,
    Starting,
    Processing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// DDD: Status enumeration for filter operations
/// </summary>
internal enum FilterStatus
{
    NotStarted,
    Starting,
    Processing,
    Completed,
    Failed,
    Cancelled
}