using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Services;

/// <summary>
/// File logger service that implements ILogger interface
/// Provides direct file logging with rotation and size management
/// </summary>
public class FileLoggerService : ILogger
{
    private readonly LoggerConfiguration _config;
    private readonly ILogger? _externalLogger;
    private string? _currentLogFile;
    private readonly object _lockObject = new();
    private bool _disposed;

    #region Constructor

    public FileLoggerService(LoggerConfiguration config, ILogger? externalLogger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _externalLogger = externalLogger;

        InitializeLogDirectory();
        CreateLogFile();

        _externalLogger?.Info("FileLoggerService initialized with directory: {LogDirectory}", _config.LogDirectory);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Current log directory
    /// </summary>
    public string LogDirectory => _config.LogDirectory;

    /// <summary>
    /// Current log file path
    /// </summary>
    public string? CurrentLogFile => _currentLogFile;

    #endregion

    #region ILogger Implementation

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null; // Scopes not supported in this implementation
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _config.MinLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || _disposed)
        {
            return;
        }

        try
        {
            lock (_lockObject)
            {
                // Check if rotation is needed
                if (_config.EnableAutoRotation && ShouldRotate())
                {
                    RotateLogFile();
                }

                // Format log entry
                var message = formatter(state, exception);
                var logEntry = FormatLogEntry(logLevel, eventId, message, exception);

                // Write to file
                WriteToFile(logEntry);
            }
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "FileLoggerService failed to write log entry");
        }
    }

    #endregion

    #region Private Methods

    private void InitializeLogDirectory()
    {
        try
        {
            if (!Directory.Exists(_config.LogDirectory))
            {
                Directory.CreateDirectory(_config.LogDirectory);
                _externalLogger?.Info("Created log directory: {LogDirectory}", _config.LogDirectory);
            }
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "Failed to create log directory: {LogDirectory}", _config.LogDirectory);
            throw;
        }
    }

    private void CreateLogFile()
    {
        try
        {
            _currentLogFile = Path.Combine(_config.LogDirectory, 
                $"{_config.BaseFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            
            // Create the file if it doesn't exist
            if (!File.Exists(_currentLogFile))
            {
                File.WriteAllText(_currentLogFile, $"# Log file created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}");
            }

            _externalLogger?.Info("Log file created: {LogFile}", _currentLogFile);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "Failed to create log file");
            throw;
        }
    }

    private bool ShouldRotate()
    {
        if (_currentLogFile == null || !File.Exists(_currentLogFile))
        {
            return false;
        }

        var fileInfo = new FileInfo(_currentLogFile);
        var maxSizeBytes = _config.MaxFileSizeMB * 1024 * 1024;
        
        return fileInfo.Length >= maxSizeBytes;
    }

    private void RotateLogFile()
    {
        try
        {
            if (_currentLogFile == null) return;

            // Archive current file
            var archiveFile = Path.ChangeExtension(_currentLogFile, ".archive.log");
            if (File.Exists(archiveFile))
            {
                File.Delete(archiveFile);
            }
            File.Move(_currentLogFile, archiveFile);

            // Create new log file
            CreateLogFile();

            _externalLogger?.Info("Log file rotated: {OldFile} -> {NewFile}", archiveFile, _currentLogFile);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "Log rotation failed");
        }
    }

    private string FormatLogEntry(LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level = logLevel.ToString().ToUpperInvariant();
        
        var logEntry = $"[{timestamp}] [{level}] {message}";
        
        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }
        
        return logEntry + Environment.NewLine;
    }

    private void WriteToFile(string logEntry)
    {
        if (_currentLogFile == null) return;

        try
        {
            File.AppendAllText(_currentLogFile, logEntry);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "Failed to write to log file: {LogFile}", _currentLogFile);
        }
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lockObject)
        {
            _disposed = true;
            _externalLogger?.Info("FileLoggerService disposed");
        }
    }

    #endregion
}