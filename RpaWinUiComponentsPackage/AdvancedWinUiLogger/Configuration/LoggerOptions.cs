using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Configuration;

/// <summary>
/// ENTERPRISE: Logger configuration options
/// PROFESSIONAL: Comprehensive logging configuration with enterprise features
/// </summary>
public record LoggerOptions
{
    /// <summary>Base directory for log files</summary>
    public string LogDirectory { get; init; } = "logs";
    
    /// <summary>Base file name for logs</summary>
    public string BaseFileName { get; init; } = "app";
    
    /// <summary>Maximum log file size in bytes</summary>
    public long MaxFileSizeBytes { get; init; } = 100 * 1024 * 1024;
    
    /// <summary>Maximum log file size in MB (legacy compatibility)</summary>
    public long? MaxFileSizeMB { get; init; } = 100;
    
    /// <summary>Maximum number of log files to keep</summary>
    public int MaxFileCount { get; init; } = 10;
    
    /// <summary>Maximum log files (legacy compatibility)</summary>
    public int MaxLogFiles { get; init; } = 10;
    
    /// <summary>Enable automatic file rotation</summary>
    public bool EnableAutoRotation { get; init; } = true;
    
    /// <summary>Enable real time viewing</summary>
    public bool EnableRealTimeViewing { get; init; } = false;
    
    /// <summary>Enable background logging</summary>
    public bool EnableBackgroundLogging { get; init; } = true;
    
    /// <summary>Log level filter</summary>
    public LogLevel MinLogLevel { get; init; } = LogLevel.Information;
    
    /// <summary>Enable structured logging</summary>
    public bool EnableStructuredLogging { get; init; } = true;
    
    /// <summary>Buffer size for background logging</summary>
    public int BufferSize { get; init; } = 1000;
    
    /// <summary>Flush interval for background logging</summary>
    public TimeSpan FlushInterval { get; init; } = TimeSpan.FromSeconds(5);
    
    /// <summary>Enable performance monitoring</summary>
    public bool EnablePerformanceMonitoring { get; init; } = true;
    
    /// <summary>Date format for log entries</summary>
    public string DateFormat { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";
    
    /// <summary>Default factory method</summary>
    public static LoggerOptions Default => new();
    
    /// <summary>Debug configuration</summary>
    public static LoggerOptions Debug => new()
    {
        MinLogLevel = LogLevel.Debug,
        EnableStructuredLogging = true,
        EnablePerformanceMonitoring = true,
        MaxFileSizeBytes = 50 * 1024 * 1024
    };
    
    /// <summary>Production configuration</summary>
    public static LoggerOptions Production => new()
    {
        MinLogLevel = LogLevel.Information,
        EnableStructuredLogging = false,
        EnablePerformanceMonitoring = false,
        MaxFileSizeBytes = 200 * 1024 * 1024,
        MaxFileCount = 30
    };
    
    /// <summary>High performance configuration</summary>
    public static LoggerOptions HighPerformance => new()
    {
        MinLogLevel = LogLevel.Warning,
        EnableStructuredLogging = false,
        EnableBackgroundLogging = true,
        BufferSize = 5000,
        FlushInterval = TimeSpan.FromSeconds(10),
        MaxFileSizeBytes = 500 * 1024 * 1024
    };
}

/// <summary>
/// ENTERPRISE: Log levels for filtering
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}