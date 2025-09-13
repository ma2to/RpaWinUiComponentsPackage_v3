using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger;

/// <summary>
/// CLEAN API: Public logger configuration options
/// PROFESSIONAL: Simple, type-safe configuration for public API
/// </summary>
public sealed record LoggerOptions
{
    /// <summary>Base directory for log files</summary>
    public required string LogDirectory { get; init; }
    
    /// <summary>Base file name for logs</summary>
    public required string BaseFileName { get; init; }
    
    /// <summary>Maximum log file size in bytes</summary>
    public long MaxFileSizeBytes { get; init; } = 100 * 1024 * 1024; // 100 MB
    
    /// <summary>Maximum number of log files to keep</summary>
    public int MaxFileCount { get; init; } = 10;
    
    /// <summary>Enable automatic file rotation</summary>
    public bool EnableAutoRotation { get; init; } = true;
    
    /// <summary>Enable background logging</summary>
    public bool EnableBackgroundLogging { get; init; } = true;
    
    /// <summary>Enable performance monitoring</summary>
    public bool EnablePerformanceMonitoring { get; init; } = false;
    
    /// <summary>Date format for log entries</summary>
    public string DateFormat { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";
    
    /// <summary>Default factory method</summary>
    public static LoggerOptions Create(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName
    };
    
    /// <summary>Debug configuration</summary>
    public static LoggerOptions Debug(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName,
        EnablePerformanceMonitoring = true,
        MaxFileSizeBytes = 50 * 1024 * 1024 // 50 MB
    };
    
    /// <summary>Production configuration</summary>
    public static LoggerOptions Production(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName,
        EnablePerformanceMonitoring = false,
        MaxFileSizeBytes = 200 * 1024 * 1024, // 200 MB
        MaxFileCount = 30
    };
}