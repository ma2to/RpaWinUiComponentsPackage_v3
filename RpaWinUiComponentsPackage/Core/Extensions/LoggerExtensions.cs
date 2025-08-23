using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.Core.Extensions;

/// <summary>
/// Extension methods for ILogger with professional null-safe patterns
/// USAGE: logger?.Info("message"), logger?.Warning("message"), logger?.Error("message")
/// CONSISTENT across DEBUG and RELEASE builds
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs an informational message with null safety
    /// USAGE: logger?.Info("🔧 Operation started with {Count} items", count)
    /// </summary>
    public static void Info(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message with null safety
    /// USAGE: logger?.Warning("⚠️ Performance threshold exceeded: {Value}ms", duration)
    /// </summary>
    public static void Warning(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogWarning(message, args);
    }

    /// <summary>
    /// Logs an error message with null safety
    /// USAGE: logger?.Error("❌ Operation failed: {Error}", errorMessage)
    /// </summary>
    public static void Error(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogError(message, args);
    }

    /// <summary>
    /// Logs an error message with exception and null safety
    /// USAGE: logger?.Error(exception, "🚨 Critical error in {Operation}", operationName)
    /// </summary>
    public static void Error(this ILogger? logger, Exception exception, string message, params object[] args)
    {
        logger?.LogError(exception, message, args);
    }
}