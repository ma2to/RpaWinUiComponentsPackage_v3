using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Core.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Core.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Infrastructure.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Infrastructure.Services;

/// <summary>
/// Core implementation for Logger file operations
/// Provides headless file management functionality
/// </summary>
internal class LoggerCore : ILoggerCore
{
    private readonly ILogger? _logger;
    private bool _isInitialized;
    private bool _disposed;
    private LoggerOptions? _configuration;
    private string? _logDirectory;
    private string? _currentLogFile;

    #region Constructor

    public LoggerCore(ILogger? logger)
    {
        _logger = logger;
        _logger?.Info("LoggerCore initialized");
    }

    #endregion

    #region Properties

    public string? LogDirectory => _logDirectory;
    public string? CurrentLogFile => _currentLogFile;
    public bool IsInitialized => _isInitialized;
    
    public double TotalLogSizeMB
    {
        get
        {
            if (!_isInitialized || _logDirectory == null || !Directory.Exists(_logDirectory))
            {
                return 0;
            }

            try
            {
                var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                var totalBytes = logFiles.Sum(file => new FileInfo(file).Length);
                return totalBytes / (1024.0 * 1024.0);
            }
            catch
            {
                return 0;
            }
        }
    }

    #endregion

    #region Initialization

    public async Task<Result<bool>> InitializeAsync(LoggerOptions config)
    {
        try
        {
            _logger?.Info("Initializing LoggerCore with directory: {LogDirectory}", config.LogDirectory);

            _configuration = config;
            
            // Set and create log directory
            var dirResult = await SetLogDirectoryAsync(config.LogDirectory);
            if (!dirResult.IsSuccess)
            {
                return dirResult;
            }

            // Create initial log file
            _currentLogFile = Path.Combine(_logDirectory!, 
                $"{config.BaseFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            _isInitialized = true;
            _logger?.Info("LoggerCore initialization completed successfully");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "LoggerCore initialization failed");
            return Result<bool>.Failure($"Initialization failed: {ex.Message}");
        }
    }

    #endregion

    #region File Management

    public async Task<Result<bool>> SetLogDirectoryAsync(string directory)
    {
        try
        {
            _logger?.Info("Setting log directory to: {Directory}", directory);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger?.Info("Created log directory: {Directory}", directory);
            }

            _logDirectory = directory;
            _logger?.Info("Log directory set successfully");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to set log directory");
            return Result<bool>.Failure($"Set directory failed: {ex.Message}");
        }
    }

    public async Task<Result<RotationResult>> RotateLogsAsync()
    {
        if (!_isInitialized || _logDirectory == null || _configuration == null)
        {
            return Result<RotationResult>.Failure("LoggerCore not initialized");
        }

        try
        {
            _logger?.Info("Starting log rotation");

            string? archiveFile = null;
            int filesRotated = 0;

            // Archive current log file if it exists
            if (_currentLogFile != null && File.Exists(_currentLogFile))
            {
                archiveFile = Path.ChangeExtension(_currentLogFile, ".archive.log");
                if (File.Exists(archiveFile))
                {
                    File.Delete(archiveFile);
                }
                File.Move(_currentLogFile, archiveFile);
                filesRotated++;
            }

            // Create new log file
            string newLogFile = Path.Combine(_logDirectory, 
                $"{_configuration.BaseFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            _currentLogFile = newLogFile;

            var result = RotationResult.Success(archiveFile, newLogFile, 
                archiveFile != null && File.Exists(archiveFile) ? new FileInfo(archiveFile).Length : 0);
            _logger?.Info("Log rotation completed: {FilesRotated} files rotated", filesRotated);

            return Result<RotationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Log rotation failed");
            return Result<RotationResult>.Failure($"Rotation failed: {ex.Message}");
        }
    }

    public async Task<Result<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30)
    {
        if (!_isInitialized || _logDirectory == null)
        {
            return Result<CleanupResult>.Failure("LoggerCore not initialized");
        }

        try
        {
            _logger?.Info("Cleaning up logs older than {MaxAge} days", maxAgeInDays);

            var cutoffDate = DateTime.Now.AddDays(-maxAgeInDays);
            var logFiles = Directory.GetFiles(_logDirectory, "*.log");
            
            int filesDeleted = 0;
            long bytesFreed = 0;
            var deletedFiles = new List<string>();

            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate && file != _currentLogFile)
                {
                    bytesFreed += fileInfo.Length;
                    deletedFiles.Add(Path.GetFileName(file));
                    File.Delete(file);
                    filesDeleted++;
                }
            }

            var result = CleanupResult.Success(filesDeleted, bytesFreed);
            _logger?.Info("Cleanup completed: {FilesDeleted} files deleted, {BytesFreed} bytes freed", 
                filesDeleted, bytesFreed);

            return Result<CleanupResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Cleanup failed");
            return Result<CleanupResult>.Failure($"Cleanup failed: {ex.Message}");
        }
    }

    public async Task<Result<long>> GetCurrentLogSizeAsync()
    {
        try
        {
            if (_currentLogFile == null || !File.Exists(_currentLogFile))
            {
                return Result<long>.Success(0);
            }

            var fileInfo = new FileInfo(_currentLogFile);
            return Result<long>.Success(fileInfo.Length);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to get current log size");
            return Result<long>.Failure($"Get size failed: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync()
    {
        if (!_isInitialized || _logDirectory == null)
        {
            return Result<IReadOnlyList<LogFileInfo>>.Failure("LoggerCore not initialized");
        }

        try
        {
            var logFiles = Directory.GetFiles(_logDirectory, "*.log");
            var fileInfos = new List<LogFileInfo>();

            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);
                fileInfos.Add(new LogFileInfo
                {
                    FilePath = file,
                    SizeBytes = fileInfo.Length,
                    CreatedTime = fileInfo.CreationTime,
                    ModifiedTime = fileInfo.LastWriteTime,
                    IsActive = file == _currentLogFile,
                    LineCount = 0 // TODO: Calculate if needed
                });
            }

            var sortedFiles = fileInfos.OrderByDescending(f => f.ModifiedTime).ToList().AsReadOnly();
            return Result<IReadOnlyList<LogFileInfo>>.Success(sortedFiles);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to get log files");
            return Result<IReadOnlyList<LogFileInfo>>.Failure($"Get files failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetCurrentLogFileAsync()
    {
        try
        {
            if (_currentLogFile == null)
            {
                return Result<string>.Failure("No current log file available");
            }

            return Result<string>.Success(_currentLogFile);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to get current log file");
            return Result<string>.Failure($"Get current file failed: {ex.Message}");
        }
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;

        _logger?.Info("Disposing LoggerCore");
        _disposed = true;
        _logger?.Info("LoggerCore disposed successfully");
    }

    #endregion
}