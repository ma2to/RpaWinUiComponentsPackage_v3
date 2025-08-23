using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Logger;

/// <summary>
/// Logger Service Interface - Contract for headless functionality
/// FUNCTIONAL: Pure business logic interface
/// DUAL-USE: Can be used by UI components or automation scripts
/// </summary>
internal interface ILoggerService : IDisposable
{
    /// <summary>
    /// Observable stream of new log entries
    /// </summary>
    IObservable<LogEntry> LogEntryAdded { get; }
    
    /// <summary>
    /// Observable stream of bulk log entries
    /// </summary>
    IObservable<IReadOnlyList<LogEntry>> BulkLogEntriesAdded { get; }
    
    /// <summary>
    /// Add single log entry
    /// </summary>
    Task<Result<bool>> AddLogEntryAsync(LogEntry logEntry);
    
    /// <summary>
    /// Add multiple log entries in batch
    /// </summary>
    Task<Result<LoggerImportResult>> AddLogEntriesBulkAsync(IReadOnlyList<LogEntry> logEntries);
    
    /// <summary>
    /// Get all log entries
    /// </summary>
    Task<Result<IReadOnlyList<LogEntry>>> GetAllLogEntriesAsync();
    
    /// <summary>
    /// Get filtered log entries
    /// </summary>
    Task<Result<IReadOnlyList<LogEntry>>> GetFilteredLogEntriesAsync(
        LoggerLevel? minimumLevel = null,
        DateTime? fromDateTime = null,
        DateTime? toDateTime = null,
        string[]? categories = null);
    
    /// <summary>
    /// Clear all log entries
    /// </summary>
    Task<Result<bool>> ClearAllEntriesAsync();
    
    /// <summary>
    /// Get total entry count
    /// </summary>
    int GetTotalEntryCount();
    
    /// <summary>
    /// Check if logger has entries
    /// </summary>
    bool HasEntries { get; }
    
    /// <summary>
    /// Initialize the logger service
    /// </summary>
    Task<Result<bool>> InitializeAsync(LoggerConfiguration? configuration = null);
    
    /// <summary>
    /// Check if logger service is initialized
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// Get total log count
    /// </summary>
    int TotalLogCount { get; }
    
    /// <summary>
    /// Add log entry using LogEntry record
    /// </summary>
    Task<Result<bool>> AddLogAsync(LogEntry logEntry);
    
    /// <summary>
    /// Clear all logs
    /// </summary>
    Task<Result<bool>> ClearLogsAsync();
    
    /// <summary>
    /// Export logs with specified format and filter
    /// </summary>
    Task<Result<LoggerExportResult>> ExportLogsAsync(LogExportFormat format, LogFilterOptions? filterOptions = null);
    
    /// <summary>
    /// Filter logs with specified options
    /// </summary>
    Task<Result<IReadOnlyList<LogEntry>>> FilterLogsAsync(LogFilterOptions filterOptions);
    
    /// <summary>
    /// Search logs with text
    /// </summary>
    Task<Result<LoggerSearchResult>> SearchLogsAsync(string searchText, bool caseSensitive = false);
    
    /// <summary>
    /// Set logger level filter
    /// </summary>
    Task SetLoggerLevelFilterAsync(LoggerLevel minimumLevel);
    
    /// <summary>
    /// Check if logger has logs
    /// </summary>
    bool HasLogs { get; }
    
    /// <summary>
    /// Observable stream of log changes
    /// </summary>
    IObservable<LogChangeEvent> LogChanges { get; }
}