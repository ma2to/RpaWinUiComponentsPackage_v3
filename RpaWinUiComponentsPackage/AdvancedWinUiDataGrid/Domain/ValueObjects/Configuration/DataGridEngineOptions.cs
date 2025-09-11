using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;

/// <summary>
/// ENTERPRISE: Core options for DataGrid engine per documentation
/// CLEAN ARCHITECTURE: Core layer configuration separate from UI layer
/// </summary>
public record DataGridEngineOptions
{
    /// <summary>Minimum number of rows to maintain</summary>
    public int MinimumRows { get; init; } = 1;
    
    /// <summary>Maximum number of rows (0 = unlimited)</summary>
    public int MaximumRows { get; init; } = 0;
    
    /// <summary>Enable background processing</summary>
    public bool EnableBackgroundProcessing { get; init; } = true;
    
    /// <summary>Enable data validation</summary>
    public bool EnableValidation { get; init; } = true;
    
    /// <summary>Enable audit logging</summary>
    public bool EnableAuditLog { get; init; } = true;
    
    /// <summary>Enable performance monitoring</summary>
    public bool EnablePerformanceMonitoring { get; init; } = true;
    
    /// <summary>Batch size for operations</summary>
    public int BatchSize { get; init; } = 10000;
    
    /// <summary>Operation timeout</summary>
    public TimeSpan OperationTimeout { get; init; } = TimeSpan.FromMinutes(5);
    
    /// <summary>Enable caching</summary>
    public bool EnableCaching { get; init; } = true;
    
    /// <summary>Cache size limit</summary>
    public int CacheSizeLimit { get; init; } = 50000;
    
    /// <summary>Enable virtualization for large datasets</summary>
    public bool EnableVirtualization { get; init; } = true;
    
    /// <summary>Threshold for enabling virtualization</summary>
    public int VirtualizationThreshold { get; init; } = 1000;
    
    /// <summary>Factory method for default options</summary>
    public static DataGridEngineOptions Default => new();
    
    /// <summary>Factory method for high performance options</summary>
    public static DataGridEngineOptions HighPerformance => new()
    {
        MinimumRows = 0,
        MaximumRows = 0,
        EnableBackgroundProcessing = true,
        EnableValidation = false, // Disable validation for max performance
        EnableAuditLog = false,
        EnablePerformanceMonitoring = true,
        BatchSize = 50000,
        EnableCaching = true,
        CacheSizeLimit = 100000,
        EnableVirtualization = true,
        VirtualizationThreshold = 500
    };
    
    /// <summary>Factory method for memory-optimized options</summary>
    public static DataGridEngineOptions MemoryOptimized => new()
    {
        MinimumRows = 1,
        MaximumRows = 10000,
        EnableBackgroundProcessing = false,
        EnableValidation = true,
        EnableAuditLog = false,
        EnablePerformanceMonitoring = false,
        BatchSize = 1000,
        EnableCaching = false,
        CacheSizeLimit = 1000,
        EnableVirtualization = true,
        VirtualizationThreshold = 100
    };
}