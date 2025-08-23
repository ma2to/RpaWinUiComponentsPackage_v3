using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Utilities;
using RpaWinUiComponentsPackage.Core.Extensions;
using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Logger;

/// <summary>
/// Core Logger Service - Headless functionality
/// FUNCTIONAL: Pure business logic without UI dependencies
/// DUAL-USE: Can be used by UI components or automation scripts
/// </summary>
internal class LoggerService : ILoggerService, IDisposable
{
    private readonly ConcurrentQueue<LogEntry> _logEntries = new();
    private readonly Subject<LogEntry> _logEntryAdded = new();
    private readonly Subject<IReadOnlyList<LogEntry>> _bulkLogEntriesAdded = new();
    private readonly LoggerConfiguration _configuration;
    private readonly ILogger? _logger;
    private bool _isInitialized = false;
    
    public LoggerService(LoggerConfiguration? configuration = null, ILogger? logger = null)
    {
        LoggerLogging.LogMethodEntry($"Config: {configuration?.GetType().Name ?? "Default"}, ExternalLogger: {logger != null}");
        _configuration = configuration ?? new LoggerConfiguration();
        _logger = logger;
        LoggerLogging.LogInfo($"LoggerService created - MaxEntries: {_configuration.Performance?.MaxLogEntries ?? 10000}, MinLevel: {_configuration.MinimumLevel}");
    }
    
    /// <summary>
    /// Observable stream of new log entries
    /// REACTIVE: Functional reactive programming
    /// </summary>
    public IObservable<LogEntry> LogEntryAdded => _logEntryAdded;
    
    /// <summary>
    /// Observable stream of bulk log entries
    /// REACTIVE: For batch operations
    /// </summary>
    public IObservable<IReadOnlyList<LogEntry>> BulkLogEntriesAdded => _bulkLogEntriesAdded;
    
    /// <summary>
    /// Add single log entry
    /// FUNCTIONAL: Pure operation with side effects isolated
    /// </summary>
    public async Task<Result<bool>> AddLogEntryAsync(LogEntry logEntry)
    {
        LoggerLogging.LogMethodEntry($"Level: {logEntry.Level}, Category: {logEntry.Category}, MessageLength: {logEntry.Message?.Length ?? 0}");
        
        try
        {
            // Filter by minimum level
            if (logEntry.Level < _configuration.MinimumLevel)
            {
                LoggerLogging.LogDebug($"Entry filtered out - Level {logEntry.Level} < MinLevel {_configuration.MinimumLevel}");
                return Result<bool>.Success(false);
            }
            
            _logEntries.Enqueue(logEntry);
            LoggerLogging.LogInfo($"Entry queued - Total entries: {_logEntries.Count}");
            
            // Manage memory - remove old entries if over limit
            await ManageMemoryAsync();
            
            // Notify observers
            _logEntryAdded.OnNext(logEntry);
            
            _logger?.LogTrace("📝 LOGGER: Entry added - Level: {Level}, Message: {Message}", 
                             logEntry.Level, logEntry.Message);
            
            LoggerLogging.LogMethodExit("Success");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to add log entry");
            return Result<bool>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Add multiple log entries in batch
    /// FUNCTIONAL: Efficient bulk operations
    /// </summary>
    public async Task<Result<LoggerImportResult>> AddLogEntriesBulkAsync(IReadOnlyList<LogEntry> logEntries)
    {
        try
        {
            var imported = 0;
            var skipped = 0;
            var errors = new List<string>();
            
            foreach (var entry in logEntries)
            {
                if (entry.Level < _configuration.MinimumLevel)
                {
                    skipped++;
                    continue;
                }
                
                _logEntries.Enqueue(entry);
                imported++;
            }
            
            // Manage memory
            await ManageMemoryAsync();
            
            // Notify observers
            _bulkLogEntriesAdded.OnNext(logEntries);
            
            var result = new LoggerImportResult(imported, skipped, errors.ToArray());
            _logger?.LogDebug("📦 LOGGER: Bulk import - Imported: {Imported}, Skipped: {Skipped}", 
                             imported, skipped);
            
            return Result<LoggerImportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to add bulk log entries");
            return Result<LoggerImportResult>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Get all log entries
    /// FUNCTIONAL: Immutable data access
    /// </summary>
    public async Task<Result<IReadOnlyList<LogEntry>>> GetAllLogEntriesAsync()
    {
        try
        {
            var entries = _logEntries.ToArray();
            return Result<IReadOnlyList<LogEntry>>.Success(entries);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to get log entries");
            return Result<IReadOnlyList<LogEntry>>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Get filtered log entries
    /// FUNCTIONAL: Pure filtering operations
    /// </summary>
    public async Task<Result<IReadOnlyList<LogEntry>>> GetFilteredLogEntriesAsync(
        LoggerLevel? minimumLevel = null,
        DateTime? fromDateTime = null,
        DateTime? toDateTime = null,
        string[]? categories = null)
    {
        try
        {
            var entries = _logEntries.AsEnumerable();
            
            if (minimumLevel.HasValue)
                entries = entries.Where(e => e.Level >= minimumLevel.Value);
                
            if (fromDateTime.HasValue)
                entries = entries.Where(e => e.Timestamp >= fromDateTime.Value);
                
            if (toDateTime.HasValue)
                entries = entries.Where(e => e.Timestamp <= toDateTime.Value);
                
            if (categories != null && categories.Length > 0)
                entries = entries.Where(e => categories.Contains(e.Category));
            
            var result = entries.ToArray();
            return Result<IReadOnlyList<LogEntry>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to filter log entries");
            return Result<IReadOnlyList<LogEntry>>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Clear all log entries
    /// FUNCTIONAL: Clean state reset
    /// </summary>
    public async Task<Result<bool>> ClearAllEntriesAsync()
    {
        try
        {
            // Clear the queue
            while (_logEntries.TryDequeue(out _)) { }
            
            _logger?.LogDebug("🗑️ LOGGER: All entries cleared");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to clear entries");
            return Result<bool>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Get total entry count
    /// FUNCTIONAL: Simple state query
    /// </summary>
    public int GetTotalEntryCount() => _logEntries.Count;
    
    /// <summary>
    /// Check if logger has entries
    /// FUNCTIONAL: Boolean state query
    /// </summary>
    public bool HasEntries => !_logEntries.IsEmpty;
    
    /// <summary>
    /// Initialize the logger service
    /// FUNCTIONAL: Configuration setup
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(LoggerConfiguration? configuration = null)
    {
        try
        {
            _isInitialized = true;
            _logger?.LogDebug("✅ LOGGER SERVICE: Initialized successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER SERVICE ERROR: Initialization failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Check if logger service is initialized
    /// FUNCTIONAL: Boolean state query
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// Get total log count property
    /// FUNCTIONAL: Simple state query
    /// </summary>
    public int TotalLogCount => _logEntries.Count;
    
    /// <summary>
    /// Add log entry using LogEntry record
    /// FUNCTIONAL: Direct log entry addition
    /// </summary>
    public async Task<Result<bool>> AddLogAsync(LogEntry logEntry)
    {
        return await AddLogEntryAsync(logEntry);
    }
    
    /// <summary>
    /// Clear all logs
    /// FUNCTIONAL: Clear operation
    /// </summary>
    public async Task<Result<bool>> ClearLogsAsync()
    {
        return await ClearAllEntriesAsync();
    }
    
    /// <summary>
    /// Export logs with specified format and filter
    /// FUNCTIONAL: Export operation with format support
    /// </summary>
    public async Task<Result<LoggerExportResult>> ExportLogsAsync(LogExportFormat format, LogFilterOptions? filterOptions = null)
    {
        try
        {
            var entries = _logEntries.AsEnumerable();
            
            // Apply filters if provided
            if (filterOptions != null)
            {
                if (filterOptions.MinimumLevel.HasValue)
                    entries = entries.Where(e => e.Level >= filterOptions.MinimumLevel.Value);
                    
                if (filterOptions.FromDateTime.HasValue)
                    entries = entries.Where(e => e.Timestamp >= filterOptions.FromDateTime.Value);
                    
                if (filterOptions.ToDateTime.HasValue)
                    entries = entries.Where(e => e.Timestamp <= filterOptions.ToDateTime.Value);
                    
                if (filterOptions.Categories != null && filterOptions.Categories.Length > 0)
                    entries = entries.Where(e => filterOptions.Categories.Contains(e.Category));
                    
                if (!string.IsNullOrEmpty(filterOptions.SearchText))
                    entries = entries.Where(e => e.Message.Contains(filterOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }
            
            var filteredEntries = entries.ToArray();
            
            // Convert to dictionary format
            var exportedLogs = filteredEntries.Select(entry => new Dictionary<string, object?>
            {
                ["Timestamp"] = entry.Timestamp,
                ["Level"] = entry.Level.ToString(),
                ["Message"] = entry.Message,
                ["Category"] = entry.Category,
                ["Exception"] = entry.Exception?.ToString(),
                ["Details"] = entry.Details
            } as IReadOnlyDictionary<string, object?>).ToArray();
            
            var result = new LoggerExportResult(exportedLogs, exportedLogs.Length);
            return Result<LoggerExportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to export logs");
            return Result<LoggerExportResult>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Filter logs with specified options
    /// FUNCTIONAL: Filtering operation
    /// </summary>
    public async Task<Result<IReadOnlyList<LogEntry>>> FilterLogsAsync(LogFilterOptions filterOptions)
    {
        return await GetFilteredLogEntriesAsync(
            filterOptions.MinimumLevel,
            filterOptions.FromDateTime,
            filterOptions.ToDateTime,
            filterOptions.Categories);
    }
    
    /// <summary>
    /// Search logs with text
    /// FUNCTIONAL: Text search operation
    /// </summary>
    public async Task<Result<LoggerSearchResult>> SearchLogsAsync(string searchText, bool caseSensitive = false)
    {
        try
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var matchingEntries = _logEntries
                .Where(entry => entry.Message.Contains(searchText, comparison))
                .ToArray();
            
            var result = new LoggerSearchResult(matchingEntries, matchingEntries.Length);
            return Result<LoggerSearchResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ERROR: Failed to search logs");
            return Result<LoggerSearchResult>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Set logger level filter
    /// FUNCTIONAL: Filter configuration
    /// </summary>
    public async Task SetLoggerLevelFilterAsync(LoggerLevel minimumLevel)
    {
        // Implementation would update configuration minimum level
        // For now, this is a no-op since configuration is readonly
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Check if logger has logs
    /// FUNCTIONAL: Boolean state query
    /// </summary>
    public bool HasLogs => !_logEntries.IsEmpty;
    
    /// <summary>
    /// Observable stream of log changes
    /// FUNCTIONAL: Reactive stream for log events
    /// </summary>
    public IObservable<LogChangeEvent> LogChanges
    {
        get
        {
            // For now, return a simple observable that emits changes
            // In a full implementation, this would combine multiple change streams
            var subject = new Subject<LogChangeEvent>();
            
            // Subscribe to log entry additions and convert to change events
            _logEntryAdded.Subscribe(entry =>
            {
                var changeEvent = new LogChangeEvent(LogChangeEventType.EntryAdded, entry, null, TotalLogCount);
                subject.OnNext(changeEvent);
            });
            
            return subject;
        }
    }
    
    /// <summary>
    /// Memory management - remove old entries if over limit
    /// PERFORMANCE: Prevent unbounded memory growth
    /// </summary>
    private async Task ManageMemoryAsync()
    {
        var maxEntries = _configuration.Performance?.MaxLogEntries ?? 10000;
        var currentCount = _logEntries.Count;
        
        if (currentCount > maxEntries)
        {
            var removeCount = currentCount - maxEntries;
            for (int i = 0; i < removeCount; i++)
            {
                _logEntries.TryDequeue(out _);
            }
            
            _logger?.LogTrace("🧹 LOGGER: Memory cleanup - Removed {Count} old entries", removeCount);
        }
    }
    
    /// <summary>
    /// Dispose resources
    /// CLEANUP: Proper resource management
    /// </summary>
    public void Dispose()
    {
        _logEntryAdded?.OnCompleted();
        _bulkLogEntriesAdded?.OnCompleted();
        _logEntryAdded?.Dispose();
        _bulkLogEntriesAdded?.Dispose();
    }
}