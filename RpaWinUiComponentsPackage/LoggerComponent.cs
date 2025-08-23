using Microsoft.Extensions.Logging;
using System.IO;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.API;
using RpaWinUiComponentsPackage.Core.Extensions;

// Clean Public API Namespace - hlavný vstupný bod pre LoggerComponent
namespace RpaWinUiComponentsPackage.LoggerComponent
{
    /// <summary>
    /// Hlavný LoggerAPI pre clean public API
    /// Aplikácie môžu používať: using RpaWinUiComponentsPackage.LoggerComponent;
    /// </summary>
    public static class LoggerAPI
    {
        private static readonly Dictionary<ILogger, UnifiedLoggerAPI> _loggerInstances = new();

        /// <summary>
        /// Vytvorí file logger s external logger forwarding
        /// </summary>
        public static ILogger CreateFileLogger(
            ILogger externalLogger,
            string logDirectory,
            string baseFileName,
            int maxFileSizeMB = 10)
        {
            try
            {
                // CRITICAL FIX: Actually create file logging configuration
                // Create directory if it doesn't exist
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                // Create actual file path with date
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                var logFileName = $"{baseFileName}_{today}_1.log";
                var fullLogPath = Path.Combine(logDirectory, logFileName);
                
                // Create unified logger API instance with actual file logging
                var loggerAPI = new UnifiedLoggerAPI(externalLogger);
                
                // BASIC FILE LOGGING: Create simple file logger wrapper
                var fileLogger = new FileLoggerWrapper(externalLogger, fullLogPath, maxFileSizeMB);
                
                // Store for later retrieval
                _loggerInstances[externalLogger] = loggerAPI;
                
                // For now, log to external logger that we created file logger
                externalLogger?.Info("📁 FILE LOGGER CREATED: File logger created at {LogPath}", fullLogPath);
                
                // Return the file logger wrapper for actual file logging
                return fileLogger;
            }
            catch (Exception ex)
            {
                externalLogger?.Error(ex, "🚨 FILE LOGGER ERROR: Failed to create file logger");
                return externalLogger;
            }
        }

        /// <summary>
        /// Získa aktuálny log file path
        /// </summary>
        public static string GetCurrentLogFile(ILogger logger)
        {
            try
            {
                // For now, return a default path since the internal implementation
                // doesn't expose the current log file path directly
                return Path.Combine(Path.GetTempPath(), "RpaWinUiDemo", "current.log");
            }
            catch
            {
                return "unknown.log";
            }
        }

        /// <summary>
        /// Vytvorí logger configuration pre clean API
        /// </summary>
        public static LoggerConfiguration CreateConfiguration(
            LogLevel minimumLevel = LogLevel.Information,
            bool enableFileLogging = true,
            bool enableConsoleLogging = true,
            string? logDirectory = null)
        {
            return new LoggerConfiguration
            {
                MinimumLevel = minimumLevel,
                EnableFileLogging = enableFileLogging,
                EnableConsoleLogging = enableConsoleLogging,
                LogDirectory = logDirectory ?? Path.Combine(Path.GetTempPath(), "RpaWinUiLogs")
            };
        }
    }

    /// <summary>
    /// Logger configuration pre clean public API
    /// </summary>
    public class LoggerConfiguration
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
        public bool EnableFileLogging { get; set; } = true;
        public bool EnableConsoleLogging { get; set; } = true;
        public string LogDirectory { get; set; } = string.Empty;
        public int MaxFileSizeMB { get; set; } = 10;
        public string BaseFileName { get; set; } = "app";
    }

    /// <summary>
    /// Simple file logger wrapper that forwards to external logger AND writes to file
    /// </summary>
    internal class FileLoggerWrapper : ILogger
    {
        private readonly ILogger _externalLogger;
        private readonly string _logFilePath;
        private readonly int _maxFileSizeMB;
        private readonly object _fileLock = new object();

        public FileLoggerWrapper(ILogger externalLogger, string logFilePath, int maxFileSizeMB)
        {
            _externalLogger = externalLogger;
            _logFilePath = logFilePath;
            _maxFileSizeMB = maxFileSizeMB;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => _externalLogger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel)
            => _externalLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Forward to external logger (console/debug)
            _externalLogger.Log(logLevel, eventId, state, exception, formatter);

            // Also write to file
            if (IsEnabled(logLevel))
            {
                var message = formatter(state, exception);
                WriteToFile(logLevel, message, exception);
            }
        }

        private void WriteToFile(LogLevel logLevel, string message, Exception? exception)
        {
            try
            {
                lock (_fileLock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var levelText = logLevel.ToString().ToUpper();
                    var logLine = $"[{timestamp}] [{levelText}] {message}";
                    
                    if (exception != null)
                    {
                        logLine += Environment.NewLine + exception.ToString();
                    }
                    
                    logLine += Environment.NewLine;

                    // Simple file writing - append to file
                    File.AppendAllText(_logFilePath, logLine);
                }
            }
            catch
            {
                // Ignore file write errors - don't break application
            }
        }
    }
}