using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;

/// <summary>
/// PROFESSIONAL Global Exception Handler for AdvancedDataGrid
/// RESPONSIBILITY: Handle all uncaught exceptions, UI errors, and provide comprehensive error logging
/// SEPARATION: Separate error handling from UI logic for better maintainability
/// </summary>
internal sealed class GlobalExceptionHandler : IDisposable
{
    private readonly ILogger? _logger;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed = false;

    public GlobalExceptionHandler(ILogger? logger, DispatcherQueue dispatcherQueue)
    {
        _logger = logger;
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        
        // Subscribe to unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        
        _logger?.Info("🛡️ GLOBAL EXCEPTION HANDLER: Initialized and monitoring all exceptions (Debug & Release)");
    }

    /// <summary>
    /// Handle UI-specific exceptions with detailed logging
    /// </summary>
    public void HandleUIException(Exception exception, string context, object? additionalData = null)
    {
        try
        {
            _logger?.Error(exception, "🚨 UI EXCEPTION: Context: {Context}, Additional Data: {@AdditionalData}", 
                context, additionalData);
            
            // Log UI-specific details
            if (exception.Data.Count > 0)
            {
                _logger?.Error("📊 UI EXCEPTION DATA: {ExceptionData}", 
                    string.Join(", ", exception.Data.Keys));
            }
            
            // Log stack trace for debugging
            _logger?.Error("🔍 UI EXCEPTION STACK: {StackTrace}", exception.StackTrace);
            
        }
        catch (Exception loggingException)
        {
            // Fallback logging - never let logging exceptions crash the app
            System.Diagnostics.Debug.WriteLine($"LOGGING EXCEPTION: {loggingException.Message}");
        }
    }

    /// <summary>
    /// Handle validation exceptions with context
    /// </summary>
    public void HandleValidationException(Exception exception, string cellId, string columnName, object? value = null)
    {
        try
        {
            _logger?.Error(exception, "🚨 VALIDATION EXCEPTION: CellId: {CellId}, Column: {Column}, Value: {Value}", 
                cellId, columnName, value);
        }
        catch (Exception loggingException)
        {
            System.Diagnostics.Debug.WriteLine($"VALIDATION LOGGING EXCEPTION: {loggingException.Message}");
        }
    }

    /// <summary>
    /// Handle data operation exceptions
    /// </summary>
    public void HandleDataException(Exception exception, string operation, int rowCount = 0, object? context = null)
    {
        try
        {
            _logger?.Error(exception, "🚨 DATA EXCEPTION: Operation: {Operation}, RowCount: {RowCount}, Context: {@Context}", 
                operation, rowCount, context);
        }
        catch (Exception loggingException)
        {
            System.Diagnostics.Debug.WriteLine($"DATA LOGGING EXCEPTION: {loggingException.Message}");
        }
    }

    /// <summary>
    /// Handle UI thread exceptions safely
    /// </summary>
    public void HandleUIThreadException(Exception exception, string context)
    {
        try
        {
            if (_dispatcherQueue.HasThreadAccess)
            {
                HandleUIException(exception, context);
            }
            else
            {
                _dispatcherQueue.TryEnqueue(() => HandleUIException(exception, context));
            }
        }
        catch (Exception dispatchException)
        {
            _logger?.Error(dispatchException, "🚨 DISPATCHER EXCEPTION: Failed to handle UI thread exception");
        }
    }

    /// <summary>
    /// Log performance issues as errors
    /// </summary>
    public void HandlePerformanceIssue(string operation, TimeSpan duration, TimeSpan expectedDuration)
    {
        try
        {
            if (duration > expectedDuration)
            {
                _logger?.Error("⚡ PERFORMANCE ISSUE: Operation: {Operation}, Duration: {Duration}ms, Expected: {Expected}ms, Overrun: {Overrun}ms",
                    operation, (int)duration.TotalMilliseconds, (int)expectedDuration.TotalMilliseconds, 
                    (int)(duration - expectedDuration).TotalMilliseconds);
            }
        }
        catch (Exception loggingException)
        {
            System.Diagnostics.Debug.WriteLine($"PERFORMANCE LOGGING EXCEPTION: {loggingException.Message}");
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var exception = e.ExceptionObject as Exception;
            _logger?.Error(exception, "🚨 UNHANDLED EXCEPTION: IsTerminating: {IsTerminating}", e.IsTerminating);
        }
        catch (Exception loggingException)
        {
            System.Diagnostics.Debug.WriteLine($"UNHANDLED LOGGING EXCEPTION: {loggingException.Message}");
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            _logger?.Error(e.Exception, "🚨 UNOBSERVED TASK EXCEPTION: Observed: {Observed}", e.Observed);
            e.SetObserved(); // Prevent app crash
        }
        catch (Exception loggingException)
        {
            System.Diagnostics.Debug.WriteLine($"UNOBSERVED LOGGING EXCEPTION: {loggingException.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            
            _logger?.Info("🛡️ GLOBAL EXCEPTION HANDLER: Disposed and unsubscribed from exception monitoring");
            _disposed = true;
        }
    }
}