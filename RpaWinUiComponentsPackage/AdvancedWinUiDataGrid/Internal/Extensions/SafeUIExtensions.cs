using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;

/// <summary>
/// PROFESSIONAL Safe UI Extension Methods
/// RESPONSIBILITY: Provide safe wrappers for all UI operations with comprehensive error handling
/// SEPARATION: Separate error handling from business logic
/// </summary>
internal static class SafeUIExtensions
{
    /// <summary>
    /// Execute UI operation safely with global exception handling
    /// </summary>
    public static async Task SafeExecuteUIAsync(this GlobalExceptionHandler exceptionHandler, 
        Func<Task> uiOperation, string context, ILogger? logger = null)
    {
        try
        {
            logger?.Info("🎨 SAFE UI: Starting {Context}", context);
            var startTime = DateTime.UtcNow;
            
            await uiOperation();
            
            var duration = DateTime.UtcNow - startTime;
            logger?.Info("✅ SAFE UI: Completed {Context} in {Duration}ms", context, (int)duration.TotalMilliseconds);
            
            // Check for performance issues
            if (duration > TimeSpan.FromSeconds(2))
            {
                exceptionHandler.HandlePerformanceIssue(context, duration, TimeSpan.FromSeconds(1));
            }
        }
        catch (Exception ex)
        {
            exceptionHandler.HandleUIException(ex, context);
            logger?.Error(ex, "🚨 SAFE UI FAILED: {Context}", context);
            // Don't rethrow - this is safe execution
        }
    }

    /// <summary>
    /// Execute UI operation safely with result
    /// </summary>
    public static async Task<T?> SafeExecuteUIAsync<T>(this GlobalExceptionHandler exceptionHandler,
        Func<Task<T>> uiOperation, string context, T? defaultValue = default, ILogger? logger = null)
    {
        try
        {
            logger?.Info("🎨 SAFE UI: Starting {Context}", context);
            var startTime = DateTime.UtcNow;
            
            var result = await uiOperation();
            
            var duration = DateTime.UtcNow - startTime;
            logger?.Info("✅ SAFE UI: Completed {Context} in {Duration}ms", context, (int)duration.TotalMilliseconds);
            
            // Check for performance issues
            if (duration > TimeSpan.FromSeconds(2))
            {
                exceptionHandler.HandlePerformanceIssue(context, duration, TimeSpan.FromSeconds(1));
            }
            
            return result;
        }
        catch (Exception ex)
        {
            exceptionHandler.HandleUIException(ex, context, new { DefaultValue = defaultValue });
            logger?.Error(ex, "🚨 SAFE UI FAILED: {Context}, returning default: {DefaultValue}", context, defaultValue);
            return defaultValue;
        }
    }

    /// <summary>
    /// Execute synchronous UI operation safely
    /// </summary>
    public static void SafeExecuteUI(this GlobalExceptionHandler exceptionHandler,
        Action uiOperation, string context, ILogger? logger = null)
    {
        try
        {
            logger?.Info("🎨 SAFE UI: Starting {Context}", context);
            var startTime = DateTime.UtcNow;
            
            uiOperation();
            
            var duration = DateTime.UtcNow - startTime;
            logger?.Info("✅ SAFE UI: Completed {Context} in {Duration}ms", context, (int)duration.TotalMilliseconds);
            
            // Check for performance issues
            if (duration > TimeSpan.FromMilliseconds(500))
            {
                exceptionHandler.HandlePerformanceIssue(context, duration, TimeSpan.FromMilliseconds(200));
            }
        }
        catch (Exception ex)
        {
            exceptionHandler.HandleUIException(ex, context);
            logger?.Error(ex, "🚨 SAFE UI FAILED: {Context}", context);
            // Don't rethrow - this is safe execution
        }
    }

    /// <summary>
    /// Execute validation operation safely
    /// </summary>
    public static T? SafeExecuteValidation<T>(this GlobalExceptionHandler exceptionHandler,
        Func<T> validationOperation, string cellId, string columnName, object? value = null, 
        T? defaultValue = default, ILogger? logger = null)
    {
        try
        {
            logger?.Info("🔍 SAFE VALIDATION: Starting for Cell {CellId}, Column {Column}", cellId, columnName);
            
            var result = validationOperation();
            
            logger?.Info("✅ SAFE VALIDATION: Completed for Cell {CellId}, Column {Column}", cellId, columnName);
            return result;
        }
        catch (Exception ex)
        {
            exceptionHandler.HandleValidationException(ex, cellId, columnName, value);
            logger?.Error(ex, "🚨 SAFE VALIDATION FAILED: Cell {CellId}, Column {Column}, returning default: {DefaultValue}", 
                cellId, columnName, defaultValue);
            return defaultValue;
        }
    }

    /// <summary>
    /// Execute data operation safely
    /// </summary>
    public static async Task<T?> SafeExecuteDataAsync<T>(this GlobalExceptionHandler exceptionHandler,
        Func<Task<T>> dataOperation, string operation, int rowCount = 0, 
        T? defaultValue = default, ILogger? logger = null)
    {
        try
        {
            logger?.Info("💾 SAFE DATA: Starting {Operation} for {RowCount} rows", operation, rowCount);
            var startTime = DateTime.UtcNow;
            
            var result = await dataOperation();
            
            var duration = DateTime.UtcNow - startTime;
            logger?.Info("✅ SAFE DATA: Completed {Operation} in {Duration}ms", operation, (int)duration.TotalMilliseconds);
            
            // Check for performance issues with data operations
            var expectedDuration = TimeSpan.FromMilliseconds(Math.Max(100, rowCount * 2)); // 2ms per row + 100ms base
            if (duration > expectedDuration)
            {
                exceptionHandler.HandlePerformanceIssue(operation, duration, expectedDuration);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            exceptionHandler.HandleDataException(ex, operation, rowCount, new { DefaultValue = defaultValue });
            logger?.Error(ex, "🚨 SAFE DATA FAILED: {Operation}, RowCount: {RowCount}, returning default: {DefaultValue}", 
                operation, rowCount, defaultValue);
            return defaultValue;
        }
    }

    /// <summary>
    /// Execute dispatcher operation safely
    /// </summary>
    public static async Task SafeDispatchAsync(this DispatcherQueue dispatcher, 
        GlobalExceptionHandler exceptionHandler, Action action, string context, ILogger? logger = null)
    {
        try
        {
            if (dispatcher.HasThreadAccess)
            {
                exceptionHandler.SafeExecuteUI(action, context, logger);
            }
            else
            {
                dispatcher.TryEnqueue(() => exceptionHandler.SafeExecuteUI(action, context, logger));
            }
        }
        catch (Exception ex)
        {
            exceptionHandler.HandleUIException(ex, $"Dispatch: {context}");
            logger?.Error(ex, "🚨 SAFE DISPATCH FAILED: {Context}", context);
        }
    }
}