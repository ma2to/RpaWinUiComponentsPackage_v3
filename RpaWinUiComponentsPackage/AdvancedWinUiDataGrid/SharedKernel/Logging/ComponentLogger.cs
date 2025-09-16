using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Logging;

/// <summary>
/// SENIOR DEVELOPER: Professional component logger with comprehensive error tracking
/// ENTERPRISE: Integrates with Result<T> pattern and captures unhandled errors
/// CLEAN ARCHITECTURE: Centralized logging concerns with structured logging
/// </summary>
internal sealed class ComponentLogger : IDisposable
{
    internal readonly ILogger _baseLogger; // Made internal for component access
    internal readonly LoggingOptions _options; // Made internal for component access
    private readonly string _categoryPrefix;
    private readonly PerformanceTracker _performanceTracker;
    private bool _disposed = false;

    public ComponentLogger(ILogger baseLogger, LoggingOptions options)
    {
        _baseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        
        if (!_options.IsValid())
            throw new ArgumentException("Invalid logging options configuration", nameof(options));
            
        _categoryPrefix = _options.CategoryPrefix;
        _performanceTracker = new PerformanceTracker(_baseLogger, _options);
        
        LogConfigurationDetails();
    }

    #region Core Logging Methods

    /// <summary>
    /// SENIOR DEV: Log information with contextual prefix
    /// </summary>
    public void LogInformation(string message, params object[] args)
    {
        if (ShouldLog(LogLevel.Information))
        {
            var prefixedMessage = $"[{_categoryPrefix}] {message}";
            _baseLogger.LogInformation(prefixedMessage, args);
        }
    }


    /// <summary>
    /// SENIOR DEV: Log errors with full exception context
    /// </summary>
    public void LogError(Exception? exception, string message, params object[] args)
    {
        if (ShouldLog(LogLevel.Error))
        {
            var prefixedMessage = $"[{_categoryPrefix}] {message}";
            _baseLogger.LogError(exception, prefixedMessage, args);
            
            // Additional context for unhandled errors
            if (_options.LogUnhandledErrors && exception != null)
            {
                LogUnhandledErrorDetails(exception);
            }
        }
    }

    /// <summary>
    /// SENIOR DEV: Log warnings with context
    /// </summary>
    public void LogWarning(string message, params object[] args)
    {
        if (ShouldLog(LogLevel.Warning))
        {
            var prefixedMessage = $"[{_categoryPrefix}] {message}";
            _baseLogger.LogWarning(prefixedMessage, args);
        }
    }

    #endregion

    #region Method Execution Logging

    /// <summary>
    /// ENTERPRISE: Log method entry with parameters (if enabled)
    /// </summary>
    public void LogMethodEntry([CallerMemberName] string methodName = "", params object[] parameters)
    {
        if (!ShouldLog(LogLevel.Debug)) return;

        if (_options.LogMethodParameters && parameters.Length > 0)
        {
            LogInformation("Method {MethodName} ENTRY - Parameters: {Parameters}", 
                methodName, string.Join(", ", parameters));
        }
        else
        {
            LogInformation("Method {MethodName} ENTRY", methodName);
        }
    }

    /// <summary>
    /// ENTERPRISE: Log method exit with result
    /// </summary>
    public void LogMethodExit([CallerMemberName] string methodName = "", object? result = null)
    {
        if (!ShouldLog(LogLevel.Debug)) return;

        if (result != null)
        {
            LogInformation("Method {MethodName} EXIT - Result: {ResultType}", 
                methodName, result.GetType().Name);
        }
        else
        {
            LogInformation("Method {MethodName} EXIT", methodName);
        }
    }

    /// <summary>
    /// SENIOR DEV: Execute method with comprehensive logging and performance tracking
    /// </summary>
    public T ExecuteWithLogging<T>(Func<T> operation, [CallerMemberName] string methodName = "")
    {
        LogMethodEntry(methodName);
        
        using var _ = _performanceTracker.StartOperation(methodName);
        
        try
        {
            var result = operation();
            LogMethodExit(methodName, result);
            return result;
        }
        catch (Exception ex)
        {
            LogError(ex, "Method {MethodName} FAILED - Exception: {ErrorMessage}", methodName, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// SENIOR DEV: Execute async method with comprehensive logging
    /// </summary>
    public async Task<T> ExecuteWithLoggingAsync<T>(Func<Task<T>> operation, [CallerMemberName] string methodName = "")
    {
        LogMethodEntry(methodName);
        
        using var _ = _performanceTracker.StartOperation(methodName);
        
        try
        {
            var result = await operation();
            LogMethodExit(methodName, result);
            return result;
        }
        catch (Exception ex)
        {
            LogError(ex, "Async method {MethodName} FAILED - Exception: {ErrorMessage}", methodName, ex.Message);
            throw;
        }
    }

    #endregion

    #region Result<T> Pattern Integration

    /// <summary>
    /// ENTERPRISE: Log Result<T> pattern outcomes with detailed context
    /// </summary>
    public void LogResult<T>(Result<T> result, string operation, [CallerMemberName] string methodName = "")
    {
        if (!_options.LogResultPatternStates) return;

        if (result.IsSuccess)
        {
            LogInformation("Result SUCCESS - Operation: {Operation}, Method: {Method}, Value: {ValueType}", 
                operation, methodName, typeof(T).Name);
        }
        else
        {
            LogWarning("Result FAILURE - Operation: {Operation}, Method: {Method}, Error: {Error}", 
                operation, methodName, result.Error);
        }
    }

    /// <summary>
    /// ENTERPRISE: Create Result<T> with automatic logging
    /// </summary>
    public Result<T> CreateResult<T>(T value, string operation, [CallerMemberName] string methodName = "")
    {
        var result = Result<T>.Success(value);
        LogResult(result, operation, methodName);
        return result;
    }

    /// <summary>
    /// ENTERPRISE: Create failure Result<T> with automatic logging
    /// </summary>
    public Result<T> CreateFailureResult<T>(string error, string operation, [CallerMemberName] string methodName = "")
    {
        var result = Result<T>.Failure(error);
        LogResult(result, operation, methodName);
        return result;
    }

    /// <summary>
    /// ENTERPRISE: Create failure Result<T> from exception with automatic logging
    /// </summary>
    public Result<T> CreateFailureResult<T>(Exception exception, string operation, [CallerMemberName] string methodName = "")
    {
        LogError(exception, "Creating failure result - Operation: {Operation}, Method: {Method}", operation, methodName);
        return Result<T>.Failure($"{operation} failed: {exception.Message}");
    }

    #endregion

    #region Private Methods

    private bool ShouldLog(LogLevel level)
    {
        return _baseLogger.IsEnabled(level) && level >= _options.MinimumLogLevel;
    }

    private void LogConfigurationDetails()
    {
        if (!_options.LogConfigurationDetails) return;

        LogInformation("ComponentLogger initialized - Strategy: {Strategy}, MinLevel: {MinLevel}, BatchSize: {BatchSize}",
            _options.Strategy, _options.MinimumLogLevel, _options.BatchSize);
    }

    private void LogUnhandledErrorDetails(Exception exception)
    {
        LogError(null, "UNHANDLED ERROR DETAILS - Type: {ExceptionType}, Source: {Source}, HelpLink: {HelpLink}",
            exception.GetType().FullName, exception.Source, exception.HelpLink);

        if (exception.Data.Count > 0)
        {
            LogError(null, "Exception Data: {ExceptionData}", string.Join(", ", exception.Data));
        }

        if (exception.InnerException != null)
        {
            LogError(exception.InnerException, "INNER EXCEPTION: {InnerMessage}", exception.InnerException.Message);
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (!_disposed)
        {
            _performanceTracker?.Dispose();
            LogInformation("ComponentLogger disposed");
            _disposed = true;
        }
    }

    #endregion
}

/// <summary>
/// PERFORMANCE: Performance tracking for method execution times
/// </summary>
internal sealed class PerformanceTracker : IDisposable
{
    private readonly ILogger _logger;
    private readonly LoggingOptions _options;
    private readonly Dictionary<string, List<TimeSpan>> _operationTimes = new();
    private bool _disposed = false;

    public PerformanceTracker(ILogger logger, LoggingOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public IDisposable StartOperation(string operationName)
    {
        return new OperationTimer(operationName, this);
    }

    internal void RecordOperation(string operationName, TimeSpan duration)
    {
        if (!_options.LogPerformanceMetrics) return;

        lock (_operationTimes)
        {
            if (!_operationTimes.ContainsKey(operationName))
                _operationTimes[operationName] = new List<TimeSpan>();

            _operationTimes[operationName].Add(duration);
        }

        if (duration.TotalMilliseconds > 1000) // Log slow operations
        {
            _logger.LogWarning("[PERFORMANCE] Slow operation detected - {Operation}: {Duration}ms", 
                operationName, duration.TotalMilliseconds);
        }
        else if (_options.LogPerformanceMetrics)
        {
            _logger.LogInformation("[PERFORMANCE] Operation completed - {Operation}: {Duration}ms", 
                operationName, duration.TotalMilliseconds);
        }
    }

    public void Dispose()
    {
        if (!_disposed && _options.LogPerformanceMetrics)
        {
            LogPerformanceSummary();
            _disposed = true;
        }
    }

    private void LogPerformanceSummary()
    {
        lock (_operationTimes)
        {
            foreach (var kvp in _operationTimes)
            {
                var times = kvp.Value;
                if (times.Count > 0)
                {
                    var avg = TimeSpan.FromMilliseconds(times.Select(t => t.TotalMilliseconds).Average());
                    var max = times.Max();
                    
                    _logger.LogInformation("[PERFORMANCE-SUMMARY] {Operation} - Count: {Count}, Avg: {AvgMs}ms, Max: {MaxMs}ms",
                        kvp.Key, times.Count, avg.TotalMilliseconds, max.TotalMilliseconds);
                }
            }
        }
    }
}

/// <summary>
/// PERFORMANCE: Individual operation timer
/// </summary>
internal sealed class OperationTimer : IDisposable
{
    private readonly string _operationName;
    private readonly PerformanceTracker _tracker;
    private readonly Stopwatch _stopwatch;

    public OperationTimer(string operationName, PerformanceTracker tracker)
    {
        _operationName = operationName;
        _tracker = tracker;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _tracker.RecordOperation(_operationName, _stopwatch.Elapsed);
    }
}