using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;

/// <summary>
/// DDD: Value object for tracking import progress
/// ENTERPRISE: Progress reporting for long-running import operations
/// IMMUTABLE: Record pattern ensuring consistent progress state
/// </summary>
public record ImportProgress
{
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int ImportedRows { get; init; }
    public int SkippedRows { get; init; }
    public int FailedRows { get; init; }
    public string? CurrentOperation { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EstimatedCompletion { get; init; }
    public string? CurrentFile { get; init; }
    public string? CurrentRowData { get; init; }
    
    /// <summary>Percentage of completion (0-100)</summary>
    public double PercentageComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    
    /// <summary>Success rate of imported rows</summary>
    public double SuccessRate => ProcessedRows > 0 ? (double)ImportedRows / ProcessedRows * 100 : 100;
    
    /// <summary>Elapsed time since start</summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    
    /// <summary>Estimated time remaining</summary>
    public TimeSpan? EstimatedTimeRemaining => EstimatedCompletion?.Subtract(DateTime.UtcNow);
    
    /// <summary>Processing rate (rows per second)</summary>
    public double ProcessingRate
    {
        get
        {
            var elapsed = ElapsedTime.TotalSeconds;
            return elapsed > 0 ? ProcessedRows / elapsed : 0;
        }
    }
    
    public static ImportProgress Create(
        int totalRows, 
        int processedRows, 
        int importedRows, 
        int skippedRows = 0,
        int failedRows = 0,
        string? currentOperation = null,
        DateTime? startTime = null,
        string? currentFile = null,
        string? currentRowData = null) =>
        new()
        {
            TotalRows = totalRows,
            ProcessedRows = processedRows,
            ImportedRows = importedRows,
            SkippedRows = skippedRows,
            FailedRows = failedRows,
            CurrentOperation = currentOperation,
            StartTime = startTime ?? DateTime.UtcNow,
            EstimatedCompletion = CalculateEstimatedCompletion(totalRows, processedRows, startTime ?? DateTime.UtcNow),
            CurrentFile = currentFile,
            CurrentRowData = currentRowData
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
/// DDD: Value object for tracking export progress
/// ENTERPRISE: Progress reporting for long-running export operations
/// IMMUTABLE: Record pattern ensuring consistent progress state
/// </summary>
public record ExportProgress
{
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int ExportedRows { get; init; }
    public int SkippedRows { get; init; }
    public int FailedRows { get; init; }
    public string? CurrentOperation { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EstimatedCompletion { get; init; }
    public string? CurrentFile { get; init; }
    public string? ExportFormat { get; init; }
    public long? BytesWritten { get; init; }
    
    /// <summary>Percentage of completion (0-100)</summary>
    public double PercentageComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    
    /// <summary>Success rate of exported rows</summary>
    public double SuccessRate => ProcessedRows > 0 ? (double)ExportedRows / ProcessedRows * 100 : 100;
    
    /// <summary>Elapsed time since start</summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    
    /// <summary>Estimated time remaining</summary>
    public TimeSpan? EstimatedTimeRemaining => EstimatedCompletion?.Subtract(DateTime.UtcNow);
    
    /// <summary>Processing rate (rows per second)</summary>
    public double ProcessingRate
    {
        get
        {
            var elapsed = ElapsedTime.TotalSeconds;
            return elapsed > 0 ? ProcessedRows / elapsed : 0;
        }
    }
    
    /// <summary>Data throughput (bytes per second)</summary>
    public double ThroughputBps
    {
        get
        {
            if (!BytesWritten.HasValue) return 0;
            var elapsed = ElapsedTime.TotalSeconds;
            return elapsed > 0 ? BytesWritten.Value / elapsed : 0;
        }
    }
    
    public static ExportProgress Create(
        int totalRows, 
        int processedRows, 
        int exportedRows, 
        int skippedRows = 0,
        int failedRows = 0,
        string? currentOperation = null,
        DateTime? startTime = null,
        string? currentFile = null,
        string? exportFormat = null,
        long? bytesWritten = null) =>
        new()
        {
            TotalRows = totalRows,
            ProcessedRows = processedRows,
            ExportedRows = exportedRows,
            SkippedRows = skippedRows,
            FailedRows = failedRows,
            CurrentOperation = currentOperation,
            StartTime = startTime ?? DateTime.UtcNow,
            EstimatedCompletion = CalculateEstimatedCompletion(totalRows, processedRows, startTime ?? DateTime.UtcNow),
            CurrentFile = currentFile,
            ExportFormat = exportFormat,
            BytesWritten = bytesWritten
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
/// DDD: Value object for tracking validation progress
/// ENTERPRISE: Progress reporting for long-running validation operations
/// IMMUTABLE: Record pattern ensuring consistent progress state
/// </summary>
public record ValidationProgress
{
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int ValidatedRows { get; init; }
    public int RowsWithErrors { get; init; }
    public int RowsWithWarnings { get; init; }
    public int TotalErrors { get; init; }
    public int TotalWarnings { get; init; }
    public string? CurrentOperation { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EstimatedCompletion { get; init; }
    public string? CurrentValidationRule { get; init; }
    
    /// <summary>COMPATIBILITY: Total validation errors found</summary>
    public int ValidationErrors { get; init; }
    
    /// <summary>COMPATIBILITY: Elapsed time (settable for backwards compatibility)</summary>
    public TimeSpan ElapsedTime { get; init; }
    
    /// <summary>Percentage of completion (0-100)</summary>
    public double PercentageComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    
    /// <summary>Success rate (rows without errors)</summary>
    public double SuccessRate => ProcessedRows > 0 ? (double)(ProcessedRows - RowsWithErrors) / ProcessedRows * 100 : 100;
    
    /// <summary>Error rate (rows with errors)</summary>
    public double ErrorRate => ProcessedRows > 0 ? (double)RowsWithErrors / ProcessedRows * 100 : 0;
    
    /// <summary>Warning rate (rows with warnings)</summary>
    public double WarningRate => ProcessedRows > 0 ? (double)RowsWithWarnings / ProcessedRows * 100 : 0;
    
    /// <summary>REMOVED: Elapsed time now settable property above</summary>
    // public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime; // Replaced with settable property
    
    /// <summary>Estimated time remaining</summary>
    public TimeSpan? EstimatedTimeRemaining => EstimatedCompletion?.Subtract(DateTime.UtcNow);
    
    /// <summary>Processing rate (rows per second)</summary>
    public double ProcessingRate
    {
        get
        {
            var elapsed = ElapsedTime.TotalSeconds;
            return elapsed > 0 ? ProcessedRows / elapsed : 0;
        }
    }
    
    public static ValidationProgress Create(
        int totalRows,
        int processedRows,
        int validatedRows,
        int rowsWithErrors = 0,
        int rowsWithWarnings = 0,
        int totalErrors = 0,
        int totalWarnings = 0,
        string? currentOperation = null,
        DateTime? startTime = null,
        string? currentValidationRule = null) =>
        new()
        {
            TotalRows = totalRows,
            ProcessedRows = processedRows,
            ValidatedRows = validatedRows,
            RowsWithErrors = rowsWithErrors,
            RowsWithWarnings = rowsWithWarnings,
            TotalErrors = totalErrors,
            TotalWarnings = totalWarnings,
            CurrentOperation = currentOperation,
            StartTime = startTime ?? DateTime.UtcNow,
            EstimatedCompletion = CalculateEstimatedCompletion(totalRows, processedRows, startTime ?? DateTime.UtcNow),
            CurrentValidationRule = currentValidationRule
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
