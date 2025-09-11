using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Models;

/// <summary>
/// Logger level enumeration for UI display
/// </summary>
internal enum LoggerLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Log entry for UI display
/// </summary>
internal class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LoggerLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    
    public LogEntry() { }
    
    public LogEntry(DateTime timestamp, LoggerLevel level, string message, string? exception = null)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message;
        Exception = exception;
    }
    
    /// <summary>
    /// Convert from Microsoft LogLevel to LoggerLevel
    /// </summary>
    public static LoggerLevel FromLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LoggerLevel.Trace,
            LogLevel.Debug => LoggerLevel.Debug,
            LogLevel.Information => LoggerLevel.Information,
            LogLevel.Warning => LoggerLevel.Warning,
            LogLevel.Error => LoggerLevel.Error,
            LogLevel.Critical => LoggerLevel.Critical,
            _ => LoggerLevel.Information
        };
    }
    
    /// <summary>
    /// Convert LoggerLevel to Microsoft LogLevel
    /// </summary>
    public static LogLevel ToLogLevel(LoggerLevel loggerLevel)
    {
        return loggerLevel switch
        {
            LoggerLevel.Trace => LogLevel.Trace,
            LoggerLevel.Debug => LogLevel.Debug,
            LoggerLevel.Information => LogLevel.Information,
            LoggerLevel.Warning => LogLevel.Warning,
            LoggerLevel.Error => LogLevel.Error,
            LoggerLevel.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }
}

/// <summary>
/// Logger color configuration for UI theming
/// </summary>
internal class LoggerColorConfiguration
{
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string ForegroundColor { get; set; } = "#000000";
    public string ErrorColor { get; set; } = "#FF0000";
    public string WarningColor { get; set; } = "#FF9800";
    public string InfoColor { get; set; } = "#2196F3";
    public string DebugColor { get; set; } = "#9E9E9E";
    public string TraceColor { get; set; } = "#607D8B";
    public bool UseDarkTheme { get; set; } = false;
}

/// <summary>
/// Logger performance configuration
/// </summary>
internal class LoggerPerformanceConfiguration
{
    public int MaxLogEntries { get; set; } = 10000;
    public bool EnableVirtualization { get; set; } = true;
    public int VirtualizationThreshold { get; set; } = 1000;
    public bool EnableAutoScroll { get; set; } = true;
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public bool EnableFiltering { get; set; } = true;
    public int MaxFileSize { get; set; } = 10; // MB
}

/// <summary>
/// Logger configuration for LoggerAPI.CreateFileLogger()
/// </summary>
internal record LoggerConfiguration
{
    public required string LogDirectory { get; init; }
    public string BaseFileName { get; init; } = "application";
    public int? MaxFileSizeMB { get; init; } = 10;
    public int MaxLogFiles { get; init; } = 10;
    public bool EnableAutoRotation { get; init; } = true;
    public bool EnableRealTimeViewing { get; init; } = false;
    public LogLevel MinLogLevel { get; init; } = LogLevel.Information;
}