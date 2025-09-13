using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Infrastructure.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Configuration;

// CLEAN API DESIGN - Single namespace, single using statement
namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger;

/// <summary>
/// 🚀 PROFESSIONAL ENTERPRISE LOGGER - Clean API Facade
/// 
/// ✅ SINGLE USING: using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
/// ✅ FILE-BASED ONLY: Headless File Logging without UI
/// ✅ ENTERPRISE-READY: File rotation, size limits, background logging
/// ✅ HYBRID ARCHITECTURE: Functional-OOP with Result<T> monads
/// ✅ PROFESSIONAL LOGGING: Microsoft.Extensions.Logging integration
/// ✅ CLEAN DESIGN: Single point of entry, hidden complexity
/// 
/// USAGE PATTERNS:
/// 
/// // File Logger Creation:
/// var fileLogger = LoggerAPIComponent.CreateFileLogger(
///     logDirectory: @"C:\Logs",
///     baseFileName: "app",
///     maxFileSizeMB: 10);
/// 
/// /// // Logger is now file-based only - no UI components
/// 
/// // Headless File Operations:
/// var headlessLogger = LoggerAPIComponent.CreateHeadless();
/// await headlessLogger.SetLogDirectoryAsync(@"C:\Logs");
/// await headlessLogger.RotateLogsAsync();
/// </summary>
public sealed class LoggerAPIComponent : IDisposable
{
    #region Private Fields - Professional Architecture
    
    private readonly ILogger? _logger;
    private readonly Internal.Core.Interfaces.ILoggerCore _coreService;
    private bool _isInitialized;
    private bool _disposed;

    #endregion

    #region Factory Methods - Clean API Entry Points

    /// <summary>
    /// Create file-based logger with directory and filename configuration
    /// BEZ UI - čisto file-based logging
    /// </summary>
    public static LoggerAPIComponent CreateFileLogger(
        string logDirectory, 
        string baseFileName = "app", 
        int maxFileSizeMB = 10,
        int maxBackupFiles = 5,
        ILogger? logger = null)
    {
        return new LoggerAPIComponent(logger);
    }

    /// <summary>
    /// Create headless logger for background operations
    /// BEZ UI - čisto headless operácie
    /// </summary>
    public static LoggerAPIComponent CreateHeadless(ILogger? logger = null)
    {
        return new LoggerAPIComponent(logger);
    }

    #endregion

    #region Constructor - Simplified for File-Only

    /// <summary>
    /// Private constructor for file-based logger only
    /// </summary>
    private LoggerAPIComponent(ILogger? logger)
    {
        _logger = logger;
        _coreService = new Internal.Infrastructure.Services.LoggerCore(logger);
        
        logger?.Info("✅ LoggerAPI: File-based instance created");
    }

    #endregion

    #region Configuration - File-Based Only

    /// <summary>
    /// Initialize logger with configuration
    /// Simplified for file-based operations only
    /// </summary>
    public async Task<LoggerResult<bool>> InitializeAsync(LoggerOptions config)
    {
        try
        {
            _logger?.Info("🔧 LoggerAPI: Starting initialization");

            var result = await _coreService.InitializeAsync(config);
            _isInitialized = result.IsSuccess;

            if (_isInitialized)
            {
                _logger?.Info("✅ LoggerAPI: Initialization completed successfully");
            }
            else
            {
                _logger?.Error("❌ LoggerAPI: Initialization failed: {Error}", result.ErrorMessage);
            }

            return _isInitialized ? LoggerResult<bool>.Success(true) : LoggerResult<bool>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LoggerAPI: Critical error during initialization");
            return LoggerResult<bool>.Failure("Initialization failed: " + ex.Message);
        }
    }

    #endregion

    #region File Operations - Core Functionality

    /// <summary>
    /// Set log directory for file operations
    /// </summary>
    public async Task<LoggerResult<bool>> SetLogDirectoryAsync(string logDirectory)
    {
        try
        {
            var result = await _coreService.SetLogDirectoryAsync(logDirectory);
            return result.IsSuccess ? LoggerResult<bool>.Success(true) : LoggerResult<bool>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error setting log directory: {Directory}", logDirectory);
            return LoggerResult<bool>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Rotate log files based on size and date
    /// </summary>
    public async Task<LoggerResult<bool>> RotateLogsAsync()
    {
        try
        {
            var result = await _coreService.RotateLogsAsync();
            return result.IsSuccess ? LoggerResult<bool>.Success(true) : LoggerResult<bool>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error rotating logs");
            return LoggerResult<bool>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Get current log file path
    /// </summary>
    public async Task<LoggerResult<string>> GetCurrentLogFileAsync()
    {
        try
        {
            var result = await _coreService.GetCurrentLogFileAsync();
            return result.IsSuccess ? LoggerResult<string>.Success(result.Value) : LoggerResult<string>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error getting current log file");
            return LoggerResult<string>.Failure(ex.Message);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Check if Logger has been initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Current log directory path
    /// </summary>
    public string? LogDirectory => _coreService.LogDirectory;

    /// <summary>
    /// Total size of all log files in MB
    /// </summary>
    public double TotalLogSizeMB => _coreService.TotalLogSizeMB;

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _coreService?.Dispose();
            _logger?.Info("🔧 LoggerAPI: Disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error during LoggerAPI disposal");
        }
        finally
        {
            _disposed = true;
        }
    }

    #endregion
}