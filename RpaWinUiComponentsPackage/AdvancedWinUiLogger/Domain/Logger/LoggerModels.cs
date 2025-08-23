using Windows.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger;

/// <summary>
/// Log level enumeration for logger component
/// FUNCTIONAL: Immutable enumeration for log severity
/// </summary>
public enum LoggerLevel
{
    Trace,
    Debug,
    Information,
    Info, 
    Warning,
    Error,
    Critical
}

/// <summary>
/// Log entry domain model
/// FUNCTIONAL: Immutable record for log entries
/// </summary>
public record LogEntry(
    DateTime Timestamp,
    LoggerLevel Level,
    string Message,
    string? Category = null,
    Exception? Exception = null,
    string? Details = null
);

/// <summary>
/// Logger configuration for UI appearance
/// FUNCTIONAL: Immutable configuration for colors and behavior
/// </summary>
public record LoggerColorConfiguration(
    Color? DebugColor = null,
    Color? InfoColor = null,
    Color? WarningColor = null,
    Color? ErrorColor = null,
    Color? CriticalColor = null,
    Color? BackgroundColor = null,
    Color? ForegroundColor = null,
    Color? BorderColor = null,
    Color? TraceColor = null,
    Color? DefaultTextColor = null,
    Color? TimestampColor = null,
    Color? CategoryColor = null,
    Color? SelectionBackgroundColor = null,
    Color? SelectionForegroundColor = null,
    bool EnableColorCoding = true
);

/// <summary>
/// Logger performance configuration
/// FUNCTIONAL: Immutable settings for performance optimization
/// </summary>
public record LoggerPerformanceConfiguration(
    int MaxLogEntries = 10000,
    int UIUpdateIntervalMs = 100,
    bool EnableAutoScroll = true,
    bool EnableVirtualization = true,
    bool EnableTimestampFormatting = true
);

/// <summary>
/// Logger configuration combining all settings
/// FUNCTIONAL: Main configuration record for logger component
/// </summary>
public record LoggerConfiguration(
    LoggerColorConfiguration? Colors = null,
    LoggerPerformanceConfiguration? Performance = null,
    string? DateTimeFormat = null,
    bool EnableFiltering = true,
    LoggerLevel MinimumLevel = LoggerLevel.Debug,
    LoggerColorConfiguration? ColorConfig = null,
    int MaxLogEntries = 10000,
    LoggerPerformanceConfiguration? PerformanceConfig = null
)
{
    /// <summary>
    /// Default logger configuration
    /// </summary>
    public static LoggerConfiguration Default => new();
};

/// <summary>
/// Export options for logger data
/// FUNCTIONAL: Configuration for exporting log entries
/// </summary>
internal record LoggerExportOptions(
    string? FilePath = null,
    LoggerLevel? MinimumLevel = null,
    DateTime? FromDateTime = null,
    DateTime? ToDateTime = null,
    string[]? Categories = null
);

/// <summary>
/// Import result for bulk operations
/// FUNCTIONAL: Result of import operations
/// </summary>
internal record LoggerImportResult(
    int ImportedCount,
    int SkippedCount,
    string[] Errors
);

/// <summary>
/// Log filter options for querying log entries
/// FUNCTIONAL: Immutable filter configuration
/// </summary>
public record LogFilterOptions(
    LoggerLevel? MinimumLevel = null,
    DateTime? FromDateTime = null,
    DateTime? ToDateTime = null,
    string[]? Categories = null,
    string? SearchText = null
);

/// <summary>
/// Log export format enumeration
/// FUNCTIONAL: Export format options
/// </summary>
public enum LogExportFormat
{
    Dictionary,
    Json,
    Csv,
    Text,
    Xml
}

/// <summary>
/// Log change event for reactive programming
/// FUNCTIONAL: Immutable event data
/// </summary>
public record LogChangeEvent(
    LogChangeEventType EventType,
    LogEntry? LogEntry = null,
    IReadOnlyList<LogEntry>? LogEntries = null,
    int? TotalCount = null
);

/// <summary>
/// Log change event type enumeration
/// FUNCTIONAL: Event type classification
/// </summary>
public enum LogChangeEventType
{
    EntryAdded,
    BulkEntriesAdded,
    EntriesCleared,
    FilterChanged
}

/// <summary>
/// Logger export result
/// FUNCTIONAL: Result of export operations
/// </summary>
internal record LoggerExportResult(
    IReadOnlyList<IReadOnlyDictionary<string, object?>> ExportedLogs,
    int TotalExported,
    string? ErrorMessage = null
);

/// <summary>
/// Logger search result
/// FUNCTIONAL: Result of search operations
/// </summary>
internal record LoggerSearchResult(
    IReadOnlyList<LogEntry> MatchingLogs,
    int TotalMatches,
    string? ErrorMessage = null
);