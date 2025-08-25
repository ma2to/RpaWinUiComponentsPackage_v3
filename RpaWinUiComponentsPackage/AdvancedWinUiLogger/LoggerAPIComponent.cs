using Microsoft.Extensions.Logging;

// CLEAN API DESIGN - Single namespace, single using statement
namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger;

/// <summary>
/// 🚀 PROFESSIONAL ENTERPRISE LOGGER - Clean API Facade
/// 
/// ✅ SINGLE USING: using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
/// ✅ DUAL-PURPOSE: UI Components + Headless File Logging
/// ✅ ENTERPRISE-READY: File rotation, size limits, real-time viewing
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
/// // UI Logger with Visual Component:
/// var uiLogger = LoggerAPIComponent.CreateForUI(logger);
/// await uiLogger.InitializeAsync(config);
/// MyContainer.Content = uiLogger.UIComponent;
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
    private readonly bool _isUIMode;
    private readonly Internal.Interfaces.ILoggerCore _coreService;
    private readonly Internal.Interfaces.IUIManager? _uiManager;
    private LoggerComponent? _uiComponent;
    private bool _isInitialized;
    private bool _disposed;

    #endregion

    #region Factory Methods - Clean API Entry Points

    /// <summary>
    /// Create file logger with automatic rotation and size management
    /// Most common usage - pure file logging without UI
    /// </summary>
    /// <param name="logDirectory">Directory for log files</param>
    /// <param name="baseFileName">Base name for log files (without extension)</param>
    /// <param name="maxFileSizeMB">Maximum file size before rotation (MB)</param>
    /// <param name="externalLogger">Optional external logger for internal logging</param>
    /// <returns>File logger instance that implements ILogger</returns>
    public static ILogger CreateFileLogger(
        string logDirectory,
        string baseFileName = "application",
        int maxFileSizeMB = 10,
        ILogger? externalLogger = null)
    {
        externalLogger?.LogInformation("📁 LoggerAPI: Creating file logger (Directory: {Directory}, Base: {Base}, MaxSize: {Size}MB)", 
            logDirectory, baseFileName, maxFileSizeMB);
        
        var config = new LoggerConfiguration
        {
            LogDirectory = logDirectory,
            BaseFileName = baseFileName,
            MaxFileSizeMB = maxFileSizeMB,
            EnableAutoRotation = true,
            EnableRealTimeViewing = false
        };
        
        return new Internal.Services.FileLoggerService(config, externalLogger);
    }

    /// <summary>
    /// Create Logger instance for UI applications
    /// Includes visual component and real-time log viewing
    /// </summary>
    /// <param name="logger">Optional Microsoft.Extensions.Logging logger</param>
    /// <returns>UI-enabled Logger instance</returns>
    public static LoggerAPIComponent CreateForUI(ILogger? logger = null)
    {
        logger?.LogInformation("🎨 LoggerAPI: Creating UI-enabled instance");
        return new LoggerAPIComponent(logger, uiMode: true);
    }

    /// <summary>
    /// Create Logger instance for headless file operations
    /// Pure file management without UI overhead
    /// </summary>
    /// <param name="logger">Optional Microsoft.Extensions.Logging logger</param>
    /// <returns>Headless Logger instance</returns>
    public static LoggerAPIComponent CreateHeadless(ILogger? logger = null)
    {
        logger?.LogInformation("🤖 LoggerAPI: Creating headless instance");
        return new LoggerAPIComponent(logger, uiMode: false);
    }

    #endregion

    #region Internal Constructor - Factory Pattern

    private LoggerAPIComponent(ILogger? logger, bool uiMode)
    {
        _logger = logger;
        _isUIMode = uiMode;
        
        // Create core service (always present)
        _coreService = new Internal.Services.LoggerCore(logger);
        
        // Create UI components only in UI mode
        if (_isUIMode)
        {
            _uiComponent = new LoggerComponent();
            _uiManager = new Internal.Services.UIManager(_uiComponent, logger);
            logger?.LogDebug("✅ LoggerAPI: UI components initialized");
        }
        
        logger?.LogDebug("✅ LoggerAPI: Instance created (Mode: {Mode})", _isUIMode ? "UI" : "Headless");
    }

    #endregion

    #region Public Properties - Clean API Surface

    /// <summary>
    /// Get the WinUI3 visual component for embedding in UI
    /// Only available in UI mode, null in headless mode
    /// </summary>
    public LoggerComponent? UIComponent => _uiComponent;

    /// <summary>
    /// Check if Logger has been initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Check if Logger is in UI mode
    /// </summary>
    public bool IsUIMode => _isUIMode;

    /// <summary>
    /// Current log directory path
    /// </summary>
    public string? LogDirectory => _coreService.LogDirectory;

    /// <summary>
    /// Current log file path
    /// </summary>
    public string? CurrentLogFile => _coreService.CurrentLogFile;

    #endregion

    #region Initialization - Enterprise Configuration

    /// <summary>
    /// Initialize Logger with configuration
    /// Supports both UI and headless modes
    /// </summary>
    /// <param name="config">Logger configuration</param>
    /// <returns>Success/failure result</returns>
    public async Task<LoggerResult<bool>> InitializeAsync(LoggerConfiguration config)
    {
        try
        {
            _logger?.LogInformation("🚀 LoggerAPI: Initializing (Mode: {Mode}, Directory: {Directory})", 
                _isUIMode ? "UI" : "Headless", config.LogDirectory);
            
            // Initialize core service
            var coreResult = await _coreService.InitializeAsync(config);
            if (!coreResult.IsSuccess)
            {
                _logger?.LogError("❌ LoggerAPI: Core initialization failed: {Error}", coreResult.ErrorMessage);
                return LoggerResult<bool>.Failure(coreResult.ErrorMessage);
            }

            // Initialize UI components if in UI mode
            if (_isUIMode && _uiManager != null && _uiComponent != null)
            {
                var uiResult = await _uiManager.InitializeAsync(config);
                if (!uiResult.IsSuccess)
                {
                    _logger?.LogError("❌ LoggerAPI: UI initialization failed: {Error}", uiResult.ErrorMessage);
                    return LoggerResult<bool>.Failure(uiResult.ErrorMessage);
                }
            }

            _isInitialized = true;
            _logger?.LogInformation("✅ LoggerAPI: Initialization completed successfully");
            return LoggerResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Initialization failed with exception");
            return LoggerResult<bool>.Failure($"Initialization failed: {ex.Message}");
        }
    }

    #endregion

    #region File Management - Core Functionality

    /// <summary>
    /// Set log directory and create if needed
    /// </summary>
    /// <param name="directory">Log directory path</param>
    /// <returns>Success/failure result</returns>
    public async Task<LoggerResult<bool>> SetLogDirectoryAsync(string directory)
    {
        try
        {
            _logger?.LogInformation("📂 LoggerAPI: Setting log directory to '{Directory}'", directory);
            
            var result = await _coreService.SetLogDirectoryAsync(directory);
            if (!result.IsSuccess)
            {
                _logger?.LogError("❌ LoggerAPI: Failed to set log directory: {Error}", result.ErrorMessage);
                return LoggerResult<bool>.Failure(result.ErrorMessage);
            }

            // Update UI if in UI mode
            if (_isUIMode && _uiManager != null)
            {
                await _uiManager.RefreshLogInfoAsync();
                _logger?.LogDebug("🎨 LoggerAPI: UI refreshed after directory change");
            }

            _logger?.LogInformation("✅ LoggerAPI: Log directory set successfully");
            return LoggerResult<bool>.Success(result.Value);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Set directory failed with exception");
            return LoggerResult<bool>.Failure($"Set directory failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Rotate log files (archive current, start new)
    /// </summary>
    /// <returns>Rotation result with file information</returns>
    public async Task<LoggerResult<RotationResult>> RotateLogsAsync()
    {
        try
        {
            _logger?.LogInformation("🔄 LoggerAPI: Starting log rotation");
            
            var result = await _coreService.RotateLogsAsync();
            
            // Update UI if in UI mode
            if (_isUIMode && _uiManager != null)
            {
                await _uiManager.RefreshLogInfoAsync();
            }

            _logger?.LogInformation("✅ LoggerAPI: Log rotation completed ({FilesRotated} files)", 
                result.IsSuccess ? result.Value.FilesRotated : 0);
            
            return result.IsSuccess 
                ? LoggerResult<RotationResult>.Success(result.Value)
                : LoggerResult<RotationResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Log rotation failed");
            return LoggerResult<RotationResult>.Failure($"Rotation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Clean up old log files based on retention policy
    /// </summary>
    /// <param name="maxAgeInDays">Maximum age of log files to keep</param>
    /// <returns>Cleanup result</returns>
    public async Task<LoggerResult<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30)
    {
        try
        {
            _logger?.LogInformation("🧹 LoggerAPI: Cleaning up logs older than {Days} days", maxAgeInDays);
            
            var result = await _coreService.CleanupOldLogsAsync(maxAgeInDays);
            
            // Update UI if in UI mode
            if (_isUIMode && _uiManager != null)
            {
                await _uiManager.RefreshLogInfoAsync();
            }

            _logger?.LogInformation("✅ LoggerAPI: Cleanup completed ({FilesDeleted} files removed)", 
                result.IsSuccess ? result.Value.FilesDeleted : 0);
            
            return result.IsSuccess 
                ? LoggerResult<CleanupResult>.Success(result.Value)
                : LoggerResult<CleanupResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Cleanup failed");
            return LoggerResult<CleanupResult>.Failure($"Cleanup failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get current log file size
    /// </summary>
    /// <returns>File size in bytes</returns>
    public async Task<LoggerResult<long>> GetCurrentLogSizeAsync()
    {
        try
        {
            var result = await _coreService.GetCurrentLogSizeAsync();
            return result.IsSuccess 
                ? LoggerResult<long>.Success(result.Value)
                : LoggerResult<long>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Get log size failed");
            return LoggerResult<long>.Failure($"Get size failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get list of all log files in directory
    /// </summary>
    /// <returns>List of log file information</returns>
    public async Task<LoggerResult<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync()
    {
        try
        {
            var result = await _coreService.GetLogFilesAsync();
            return result.IsSuccess 
                ? LoggerResult<IReadOnlyList<LogFileInfo>>.Success(result.Value)
                : LoggerResult<IReadOnlyList<LogFileInfo>>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Get log files failed");
            return LoggerResult<IReadOnlyList<LogFileInfo>>.Failure($"Get files failed: {ex.Message}");
        }
    }

    #endregion

    #region UI-Specific Methods - Only Available in UI Mode

    /// <summary>
    /// Manually refresh the UI (only in UI mode)
    /// </summary>
    /// <returns>Success/failure result</returns>
    public async Task<LoggerResult<bool>> RefreshUIAsync()
    {
        if (!_isUIMode)
        {
            return LoggerResult<bool>.Success(true); // No-op in headless mode
        }

        if (!_isInitialized)
        {
            return LoggerResult<bool>.Failure("Logger not initialized");
        }

        try
        {
            _logger?.LogDebug("🎨 LoggerAPI: Manual UI refresh requested");
            
            if (_uiManager != null)
            {
                await _uiManager.RefreshLogInfoAsync();
                _logger?.LogDebug("✅ LoggerAPI: UI refresh completed");
                return LoggerResult<bool>.Success(true);
            }
            
            return LoggerResult<bool>.Failure("UI manager not available");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: UI refresh failed");
            return LoggerResult<bool>.Failure($"UI refresh failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Show log file content in UI (only in UI mode)
    /// </summary>
    /// <param name="filePath">Log file to display</param>
    /// <returns>Success/failure result</returns>
    public async Task<LoggerResult<bool>> ShowLogFileAsync(string filePath)
    {
        if (!_isUIMode)
        {
            return LoggerResult<bool>.Success(true); // No-op in headless mode
        }

        try
        {
            _logger?.LogDebug("📄 LoggerAPI: Showing log file in UI: {FilePath}", filePath);
            
            if (_uiManager != null)
            {
                await _uiManager.ShowLogFileAsync(filePath);
                return LoggerResult<bool>.Success(true);
            }
            
            return LoggerResult<bool>.Failure("UI manager not available");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Show log file failed");
            return LoggerResult<bool>.Failure($"Show file failed: {ex.Message}");
        }
    }

    #endregion

    #region Static Utility Methods - Common Operations

    /// <summary>
    /// Get current log file path for existing logger
    /// </summary>
    /// <param name="logger">Logger instance created with CreateFileLogger</param>
    /// <returns>Current log file path or null</returns>
    public static string? GetCurrentLogFile(ILogger logger)
    {
        if (logger is Internal.Services.FileLoggerService fileLogger)
        {
            return fileLogger.CurrentLogFile;
        }
        return null;
    }

    /// <summary>
    /// Get log directory for existing logger
    /// </summary>
    /// <param name="logger">Logger instance created with CreateFileLogger</param>
    /// <returns>Log directory or null</returns>
    public static string? GetLogDirectory(ILogger logger)
    {
        if (logger is Internal.Services.FileLoggerService fileLogger)
        {
            return fileLogger.LogDirectory;
        }
        return null;
    }

    #endregion

    #region Resource Management - Professional Disposal

    /// <summary>
    /// Dispose of all resources properly
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger?.LogDebug("🧹 LoggerAPI: Disposing resources");

        try
        {
            _uiManager?.Dispose();
            _coreService?.Dispose();
            _uiComponent = null;
            
            _disposed = true;
            _logger?.LogDebug("✅ LoggerAPI: Resources disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 LoggerAPI: Error during disposal");
        }
    }

    #endregion
}

#region Supporting Types - Clean API Models

/// <summary>
/// Result pattern for Logger operations
/// Provides consistent error handling across all operations
/// </summary>
/// <typeparam name="T">Result value type</typeparam>
public record LoggerResult<T>
{
    public bool IsSuccess { get; init; }
    public T Value { get; init; } = default!;
    public string Error { get; init; } = string.Empty;

    public static LoggerResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static LoggerResult<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

/// <summary>
/// Logger configuration for initialization
/// </summary>
public record LoggerConfiguration
{
    public required string LogDirectory { get; init; }
    public string BaseFileName { get; init; } = "application";
    public int MaxFileSizeMB { get; init; } = 10;
    public int MaxLogFiles { get; init; } = 10;
    public bool EnableAutoRotation { get; init; } = true;
    public bool EnableRealTimeViewing { get; init; } = false;
    public LogLevel MinLogLevel { get; init; } = LogLevel.Information;
}

/// <summary>
/// Log file rotation result
/// </summary>
public record RotationResult(int FilesRotated, string? NewLogFile, string? ArchiveFile);

/// <summary>
/// Log cleanup result
/// </summary>
public record CleanupResult(int FilesDeleted, long BytesFreed, IReadOnlyList<string> DeletedFiles);

/// <summary>
/// Log file information
/// </summary>
public record LogFileInfo(string FileName, string FullPath, long SizeBytes, DateTime CreatedAt, DateTime LastModified);

#endregion