using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;

/// <summary>
/// ENTERPRISE: Import options configuration
/// </summary>
internal record ImportOptions
{
    public ImportMode Mode { get; init; } = ImportMode.Replace;
    public int StartRow { get; init; } = 1;
    public bool ValidateOnImport { get; init; } = true;
    public bool StopOnError { get; init; } = false;
    public TimeSpan? Timeout { get; init; }
    public int BatchSize { get; init; } = 1000;
    
    public static ImportOptions Default => new();
    public static ImportOptions FastImport => new() { ValidateOnImport = false, BatchSize = 5000 };
    public static ImportOptions SafeImport => new() { ValidateOnImport = true, StopOnError = true };
}

/// <summary>
/// ENTERPRISE: Export options configuration
/// </summary>
internal record ExportOptions
{
    public bool IncludeValidAlerts { get; init; } = false;
    public bool ExportOnlyChecked { get; init; } = false;
    public bool ExportOnlyFiltered { get; init; } = false;
    public bool RemoveAfter { get; init; } = false;
    public TimeSpan? Timeout { get; init; }
    public string? DateFormat { get; init; } = "yyyy-MM-dd";
    public string? NumberFormat { get; init; }
    
    public static ExportOptions Default => new();
    public static ExportOptions CheckedOnly => new() { ExportOnlyChecked = true };
    public static ExportOptions FilteredOnly => new() { ExportOnlyFiltered = true };
}

/// <summary>
/// ENTERPRISE: Data import result with metadata
/// </summary>
internal record DataImportResult
{
    public bool Success { get; init; }
    public int TotalRows { get; init; }
    public int ImportedRows { get; init; }
    public int SkippedRows { get; init; }
    public int ErrorRows { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
    
    public static DataImportResult CreateSuccess(int total, int imported, TimeSpan time) =>
        new() 
        { 
            Success = true, 
            TotalRows = total, 
            ImportedRows = imported, 
            ProcessingTime = time 
        };
    
    public static DataImportResult Failed(IReadOnlyList<string> errors) =>
        new() { Success = false, Errors = errors };
}

/// <summary>
/// ENTERPRISE: Data export result with metadata
/// </summary>
internal record DataExportResult
{
    public bool Success { get; init; }
    public int TotalRows { get; init; }
    public int ExportedRows { get; init; }
    public long FileSizeBytes { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public string? ExportPath { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    
    public static DataExportResult CreateSuccess(int total, int exported, string path, TimeSpan time) =>
        new() 
        { 
            Success = true, 
            TotalRows = total, 
            ExportedRows = exported, 
            ExportPath = path,
            ProcessingTime = time 
        };
    
    public static DataExportResult Failed(IReadOnlyList<string> errors) =>
        new() { Success = false, Errors = errors };
}

/// <summary>
/// ENTERPRISE: DataGrid initialization result
/// </summary>
internal record DataGridInitializationResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public int ColumnCount { get; init; }
    public TimeSpan InitializationTime { get; init; }
    public IReadOnlyList<string> ValidationErrors { get; init; } = [];
    
    public static DataGridInitializationResult CreateSuccess(int columnCount, TimeSpan time) =>
        new() 
        { 
            Success = true, 
            ColumnCount = columnCount, 
            InitializationTime = time,
            Message = "DataGrid initialized successfully"
        };
    
    public static DataGridInitializationResult Failed(string message, IReadOnlyList<string>? errors = null) =>
        new() 
        { 
            Success = false, 
            Message = message, 
            ValidationErrors = errors ?? []
        };
}

/// <summary>
/// ENTERPRISE: DataGrid health status
/// </summary>
internal record DataGridHealthStatus
{
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = "Unknown";
    public Dictionary<string, object> Metrics { get; init; } = new();
    public DateTime LastChecked { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<string> Issues { get; init; } = [];
    
    public static DataGridHealthStatus Healthy => 
        new() { IsHealthy = true, Status = "Healthy" };
    
    public static DataGridHealthStatus Unhealthy(IReadOnlyList<string> issues) =>
        new() { IsHealthy = false, Status = "Unhealthy", Issues = issues };
}

/// <summary>
/// ENTERPRISE: Performance metrics for monitoring
/// </summary>
internal record PerformanceMetrics
{
    public TimeSpan LastOperationTime { get; init; }
    public TimeSpan AverageOperationTime { get; init; }
    public long MemoryUsageBytes { get; init; }
    public int TotalOperations { get; init; }
    public double OperationsPerSecond { get; init; }
    public DateTime MeasuredAt { get; init; } = DateTime.UtcNow;
    
    public static PerformanceMetrics Default => new();
    
    public static PerformanceMetrics Create(TimeSpan lastOpTime, long memUsage, int totalOps) =>
        new()
        {
            LastOperationTime = lastOpTime,
            MemoryUsageBytes = memUsage,
            TotalOperations = totalOps
        };
}

/// <summary>
/// ENTERPRISE: Grid performance statistics
/// </summary>
internal record GridPerformanceStatistics
{
    public PerformanceMetrics ImportMetrics { get; init; } = PerformanceMetrics.Default;
    public PerformanceMetrics ExportMetrics { get; init; } = PerformanceMetrics.Default;
    public PerformanceMetrics SearchMetrics { get; init; } = PerformanceMetrics.Default;
    public PerformanceMetrics ValidationMetrics { get; init; } = PerformanceMetrics.Default;
    public long TotalMemoryUsage { get; init; }
    public TimeSpan Uptime { get; init; }
    
    public static GridPerformanceStatistics Default => new();
}