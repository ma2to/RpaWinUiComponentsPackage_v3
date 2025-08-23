using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities.Helpers;

/// <summary>
/// Nezávislé logovanie pre AdvancedWinUiDataGrid komponent
/// INFO/WARNING/ERROR: Pre debug aj release - chyby, dôležité operácie, workflow, dáta
/// DEBUG: Len pre debug builds - dodatočné detaily
/// INTERNAL: Nie je súčasťou public API
/// </summary>
internal static class DataGridLogging
{
    private static readonly object _lock = new();
    private static bool _isConsoleEnabled = true;

    /// <summary>
    /// Konfigurácia console outputu
    /// </summary>
    public static void ConfigureConsoleOutput(bool enabled)
    {
        lock (_lock)
        {
            _isConsoleEnabled = enabled;
        }
    }

    /// <summary>
    /// INFO logging - pre debug aj release
    /// Používať pre: workflow kroky, dôležité operácie, dáta processing
    /// </summary>
    public static void LogInfo(string message, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        WriteLog("INFO", message, methodName, filePath);
    }

    /// <summary>
    /// WARNING logging - pre debug aj release  
    /// Používať pre: potential issues, fallback behaviors, performance warnings
    /// </summary>
    public static void LogWarning(string message, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        WriteLog("WARN", message, methodName, filePath);
    }

    /// <summary>
    /// ERROR logging - pre debug aj release
    /// Používať pre: všetky chyby, exceptions, failed operations
    /// </summary>
    public static void LogError(string message, Exception? exception = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var fullMessage = exception != null ? $"{message} | Exception: {exception.Message}" : message;
        WriteLog("ERROR", fullMessage, methodName, filePath);
        
        if (exception != null)
        {
            WriteLog("ERROR", $"StackTrace: {exception.StackTrace}", methodName, filePath);
        }
    }

    /// <summary>
    /// DEBUG logging - len pre debug builds
    /// Používať pre: detailed traces, internal state, verbose diagnostics
    /// </summary>
    [Conditional("DEBUG")]
    public static void LogDebug(string message, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        WriteLog("DEBUG", message, methodName, filePath);
    }

    /// <summary>
    /// Method entry logging s parametrami - INFO level
    /// </summary>
    public static void LogMethodEntry(object? parameters = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var message = parameters != null 
            ? $"Method started with parameters: {FormatParameters(parameters)}"
            : "Method started";
        LogInfo(message, methodName, filePath);
    }

    /// <summary>
    /// Method exit logging s výsledkom - INFO level
    /// </summary>
    public static void LogMethodExit(object? result = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var message = result != null 
            ? $"Method completed with result: {FormatParameters(result)}"
            : "Method completed";
        LogInfo(message, methodName, filePath);
    }

    /// <summary>
    /// Data operation logging - INFO level
    /// </summary>
    public static void LogDataOperation(string operation, int recordCount, object? additionalData = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var message = additionalData != null
            ? $"Data operation: {operation} | Records: {recordCount} | Data: {FormatParameters(additionalData)}"
            : $"Data operation: {operation} | Records: {recordCount}";
        LogInfo(message, methodName, filePath);
    }

    /// <summary>
    /// Performance logging s časovaním
    /// </summary>
    public static void LogPerformance(string operation, TimeSpan duration, object? additionalData = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var message = additionalData != null
            ? $"Performance: {operation} took {duration.TotalMilliseconds:F2}ms | Data: {FormatParameters(additionalData)}"
            : $"Performance: {operation} took {duration.TotalMilliseconds:F2}ms";

        if (duration.TotalMilliseconds > 1000) // Dlhšie ako 1 sekunda
        {
            LogWarning(message, methodName, filePath);
        }
        else
        {
            LogInfo(message, methodName, filePath);
        }
    }

    /// <summary>
    /// UI event logging
    /// </summary>
    public static void LogUIEvent(string eventName, object? eventData = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var message = eventData != null
            ? $"UI Event: {eventName} | Data: {FormatParameters(eventData)}"
            : $"UI Event: {eventName}";
        LogInfo(message, methodName, filePath);
    }

    /// <summary>
    /// Validation operation logging
    /// </summary>
    public static void LogValidation(string operation, bool isValid, int recordCount, string? details = null, [CallerMemberName] string? methodName = null, [CallerFilePath] string? filePath = null)
    {
        var message = details != null
            ? $"Validation: {operation} | Valid: {isValid} | Records: {recordCount} | Details: {details}"
            : $"Validation: {operation} | Valid: {isValid} | Records: {recordCount}";
        
        if (!isValid)
        {
            LogWarning(message, methodName, filePath);
        }
        else
        {
            LogInfo(message, methodName, filePath);
        }
    }

    private static void WriteLog(string level, string message, string? methodName, string? filePath)
    {
        if (!_isConsoleEnabled) return;

        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var fileName = !string.IsNullOrEmpty(filePath) ? Path.GetFileNameWithoutExtension(filePath) : "Unknown";
            var logMessage = $"[{timestamp}] [{level}] [DataGrid.{fileName}.{methodName}] {message}";
            
            Console.WriteLine(logMessage);
            
            // Output do Debug window v IDE
            Debug.WriteLine(logMessage);
        }
    }

    private static string FormatParameters(object? obj)
    {
        if (obj == null) return "null";
        
        try
        {
            // Jednoduché formátovanie pre základné typy
            return obj switch
            {
                string s => $"\"{s}\"",
                int i => i.ToString(),
                bool b => b.ToString().ToLower(),
                double d => d.ToString("F2"),
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                _ => obj.ToString() ?? "null"
            };
        }
        catch
        {
            return obj.GetType().Name;
        }
    }
}