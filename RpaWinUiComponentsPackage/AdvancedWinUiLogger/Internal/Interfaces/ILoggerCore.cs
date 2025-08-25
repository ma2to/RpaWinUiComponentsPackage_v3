using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Interfaces;

/// <summary>
/// Core interface for Logger file operations
/// Provides headless file management functionality
/// </summary>
public interface ILoggerCore : IDisposable
{
    #region Properties
    
    /// <summary>
    /// Current log directory path
    /// </summary>
    string? LogDirectory { get; }
    
    /// <summary>
    /// Current log file path
    /// </summary>
    string? CurrentLogFile { get; }
    
    /// <summary>
    /// Is Logger initialized and ready for operations
    /// </summary>
    bool IsInitialized { get; }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize Logger with configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(LoggerConfiguration config);
    
    #endregion
    
    #region File Management
    
    /// <summary>
    /// Set log directory and create if needed
    /// </summary>
    Task<Result<bool>> SetLogDirectoryAsync(string directory);
    
    /// <summary>
    /// Rotate log files (archive current, start new)
    /// </summary>
    Task<Result<RotationResult>> RotateLogsAsync();
    
    /// <summary>
    /// Clean up old log files based on retention policy
    /// </summary>
    Task<Result<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30);
    
    /// <summary>
    /// Get current log file size
    /// </summary>
    Task<Result<long>> GetCurrentLogSizeAsync();
    
    /// <summary>
    /// Get list of all log files in directory
    /// </summary>
    Task<Result<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync();
    
    #endregion
}