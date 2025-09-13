using System;
using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Logging;

/// <summary>
/// SENIOR DEVELOPER: Professional logging configuration for DataGrid component
/// ENTERPRISE: Comprehensive logging options supporting multiple strategies and performance tuning
/// CLEAN ARCHITECTURE: Logging concerns separated from business logic
/// </summary>
internal sealed record LoggingOptions
{
    /// <summary>
    /// Logging strategy - how logs are written to storage
    /// </summary>
    internal LoggingStrategy Strategy { get; init; } = LoggingStrategy.Immediate;
    
    /// <summary>
    /// Batch size for batch logging strategy (number of log entries before flush)
    /// </summary>
    public int BatchSize { get; init; } = 100;
    
    /// <summary>
    /// Flush interval for batch and async strategies
    /// </summary>
    public TimeSpan FlushInterval { get; init; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Maximum buffer size in memory before forced flush (prevents memory leaks)
    /// </summary>
    public int MaxBufferSize { get; init; } = 1000;
    
    /// <summary>
    /// Minimum log level for component internal logging
    /// </summary>
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Debug;
    
    /// <summary>
    /// Enable logging of unhandled errors and exceptions
    /// </summary>
    public bool LogUnhandledErrors { get; init; } = true;
    
    /// <summary>
    /// Enable logging of Result<T> pattern success/failure states
    /// </summary>
    public bool LogResultPatternStates { get; init; } = true;
    
    /// <summary>
    /// Enable performance metrics logging (method execution times, etc.)
    /// </summary>
    public bool LogPerformanceMetrics { get; init; } = true;
    
    /// <summary>
    /// Enable detailed parameter logging for debugging
    /// </summary>
    public bool LogMethodParameters { get; init; } = true;
    
    /// <summary>
    /// Enable logging of configuration and initialization details
    /// </summary>
    public bool LogConfigurationDetails { get; init; } = true;
    
    /// <summary>
    /// Custom category prefix for component logs (helps with filtering)
    /// </summary>
    public string CategoryPrefix { get; init; } = "AdvancedDataGrid";
    
    /// <summary>
    /// Timeout for async logging operations
    /// </summary>
    public TimeSpan AsyncTimeout { get; init; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Enable structured logging with contextual data
    /// </summary>
    public bool EnableStructuredLogging { get; init; } = true;
    
    /// <summary>
    /// Default logging options for development/debugging
    /// </summary>
    public static LoggingOptions Development => new()
    {
        Strategy = LoggingStrategy.Immediate,
        MinimumLogLevel = LogLevel.Debug,
        LogUnhandledErrors = true,
        LogResultPatternStates = true,
        LogPerformanceMetrics = true,
        LogMethodParameters = true,
        LogConfigurationDetails = true,
        EnableStructuredLogging = true
    };
    
    /// <summary>
    /// Production logging options with performance optimizations
    /// </summary>
    public static LoggingOptions Production => new()
    {
        Strategy = LoggingStrategy.Batch,
        BatchSize = 500,
        FlushInterval = TimeSpan.FromSeconds(10),
        MinimumLogLevel = LogLevel.Information,
        LogUnhandledErrors = true,
        LogResultPatternStates = false,
        LogPerformanceMetrics = false,
        LogMethodParameters = false,
        LogConfigurationDetails = false,
        EnableStructuredLogging = true
    };
    
    /// <summary>
    /// High-performance logging for enterprise scenarios
    /// </summary>
    public static LoggingOptions HighPerformance => new()
    {
        Strategy = LoggingStrategy.Async,
        BatchSize = 1000,
        FlushInterval = TimeSpan.FromSeconds(30),
        MaxBufferSize = 5000,
        MinimumLogLevel = LogLevel.Warning,
        LogUnhandledErrors = true,
        LogResultPatternStates = false,
        LogPerformanceMetrics = false,
        LogMethodParameters = false,
        LogConfigurationDetails = false,
        EnableStructuredLogging = false
    };
    
    /// <summary>
    /// Validate logging options configuration
    /// </summary>
    public bool IsValid()
    {
        return BatchSize > 0 
            && FlushInterval > TimeSpan.Zero 
            && MaxBufferSize > BatchSize
            && AsyncTimeout > TimeSpan.Zero
            && !string.IsNullOrWhiteSpace(CategoryPrefix);
    }
}

/// <summary>
/// ENTERPRISE: Logging strategy enumeration
/// PERFORMANCE: Different strategies for different performance requirements
/// </summary>
internal enum LoggingStrategy
{
    /// <summary>
    /// Write each log entry immediately to storage (best for debugging)
    /// </summary>
    Immediate,
    
    /// <summary>
    /// Batch log entries and write in groups (balanced performance)
    /// </summary>
    Batch,
    
    /// <summary>
    /// Asynchronous logging with background processing (best for high-volume)
    /// </summary>
    Async,
    
    /// <summary>
    /// In-memory only logging (for testing scenarios)
    /// </summary>
    InMemory
}