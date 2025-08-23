using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Logger;
using RpaWinUiComponentsPackage.Core.Extensions;
using System.Reactive.Subjects;
using DomainLoggerLevel = RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger.LoggerLevel;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.API;

/// <summary>
/// Unified Logger API Implementation - Single API for UI and NON-UI
/// ADAPTIVE: Automatically detects if UI is available and adapts behavior
/// BACKWARD COMPATIBLE: Maintains same method signatures as before
/// INDEPENDENT: Completely independent from DataGrid components
/// </summary>
public sealed class UnifiedLoggerAPI : ILoggerAPI, IDisposable
{
    #region Private Fields
    
    private readonly ILoggerService _coreService;
    private readonly ILogger? _logger;
    private readonly bool _hasUI;
    private readonly UserControl? _uiComponent;
    private readonly LoggerUIManager? _uiManager;
    private bool _isDisposed = false;
    
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Create headless API instance
    /// NON-UI: Pure headless operation
    /// </summary>
    /// <param name="logger">Optional logger</param>
    public UnifiedLoggerAPI(ILogger? logger = null)
    {
        _coreService = new LoggerService(null, logger);
        _logger = logger;
        _hasUI = false;
        _uiComponent = null;
        
        _logger?.LogDebug("🤖 UNIFIED LOGGER API: Created headless instance");
    }
    
    /// <summary>
    /// Create UI-enabled API instance
    /// UI: Connects to actual UI component
    /// </summary>
    /// <param name="uiComponent">UI component (LoggerComponent UserControl)</param>
    /// <param name="logger">Optional logger</param>
    public UnifiedLoggerAPI(UserControl uiComponent, ILogger? logger = null)
    {
        _coreService = new LoggerService(null, logger);
        _logger = logger;
        _hasUI = true;
        _uiComponent = uiComponent;
        _uiManager = new LoggerUIManager(uiComponent, _coreService, logger);
        
        _logger?.LogDebug("🖥️ UNIFIED LOGGER API: Created UI-enabled instance");
    }
    
    #endregion
    
    #region ILoggerAPI Implementation
    
    public async Task<Result<bool>> InitializeAsync(
        LoggerConfiguration? configuration = null,
        ILogger? logger = null,
        LoggerColorConfiguration? colorConfig = null,
        LoggerPerformanceConfiguration? performanceConfig = null,
        LogFilterOptions? filterConfig = null)
    {
        try
        {
            // ENHANCED CONFIGURATION: Merge all configuration sources
            var enhancedConfig = BuildEnhancedConfiguration(
                configuration, colorConfig, performanceConfig, filterConfig);
            
            // CORE: Initialize core service with enhanced configuration
            var coreResult = await _coreService.InitializeAsync(enhancedConfig);
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Initialize UI if present with color configuration
            if (_hasUI && _uiManager != null)
            {
                var uiResult = await _uiManager.InitializeAsync(enhancedConfig);
                if (!uiResult)
                {
                    _logger?.LogWarning("⚠️ LOGGER UI initialization failed, continuing with core only");
                    // Continue with core-only operation
                }
                
                // COLORS: Apply color configuration to UI
                if (colorConfig != null || enhancedConfig.ColorConfig != null)
                {
                    await ApplyColorConfigurationAsync(colorConfig ?? enhancedConfig.ColorConfig);
                }
            }
            
            _logger?.LogDebug("✅ UNIFIED LOGGER INIT: Initialized successfully (UI: {HasUI}, Performance: {PerfConfig}, Colors: {ColorConfig})", 
                _hasUI, 
                performanceConfig?.GetType().Name ?? "Default",
                colorConfig?.GetType().Name ?? "Default");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED LOGGER INIT ERROR: Initialization failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public bool IsInitialized => _coreService.IsInitialized;
    
    public async Task<Result<bool>> AddLogAsync(
        LoggerLevel level,
        string message,
        Exception? exception = null)
    {
        return await AddLogAsync(level, message, null, exception);
    }
    
    public async Task<Result<bool>> AddLogAsync(
        LoggerLevel level,
        string message,
        object? state = null,
        Exception? exception = null)
    {
        try
        {
            // CORE: Always add to core service
            var logEntry = new LogEntry(DateTime.UtcNow, level, message, "Default", exception, state?.ToString());
            var coreResult = await _coreService.AddLogAsync(logEntry);
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterLogChangeAsync("Log added");
            }
            
            _logger?.LogDebug("✅ UNIFIED LOGGER ADD: Added log entry (UI: {HasUI})", _hasUI);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED LOGGER ADD ERROR: Add log failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task<Result<bool>> ClearLogsAsync()
    {
        try
        {
            // CORE: Always clear core service
            var coreResult = await _coreService.ClearLogsAsync();
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterLogChangeAsync("Logs cleared");
            }
            
            _logger?.LogDebug("✅ UNIFIED LOGGER CLEAR: Cleared all logs (UI: {HasUI})", _hasUI);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED LOGGER CLEAR ERROR: Clear failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportLogsAsync(
        LogExportFormat format = LogExportFormat.Dictionary,
        LogFilterOptions? filterOptions = null)
    {
        try
        {
            // CORE: Always export from core service
            var result = await _coreService.ExportLogsAsync(format, filterOptions);
            
            _logger?.LogDebug("✅ UNIFIED LOGGER EXPORT: Exported {Count} logs", 
                result.IsSuccess ? result.Value?.ExportedLogs.Count ?? 0 : 0);
            
            return result.IsSuccess 
                ? Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(result.Value!.ExportedLogs)
                : Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(result.ErrorMessage ?? "Export failed");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED LOGGER EXPORT ERROR: Export failed");
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(ex);
        }
    }
    
    public async Task<Result<IReadOnlyList<LogEntry>>> FilterLogsAsync(LogFilterOptions filterOptions)
    {
        try
        {
            // CORE: Always filter from core service
            var result = await _coreService.FilterLogsAsync(filterOptions);
            
            _logger?.LogDebug("✅ UNIFIED LOGGER FILTER: Filtered to {Count} logs", 
                result.IsSuccess ? result.Value?.Count ?? 0 : 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED LOGGER FILTER ERROR: Filter failed");
            return Result<IReadOnlyList<LogEntry>>.Failure(ex);
        }
    }
    
    public async Task<Result<IReadOnlyList<LogEntry>>> SearchLogsAsync(string searchText, bool caseSensitive = false)
    {
        try
        {
            // CORE: Always search from core service
            var result = await _coreService.SearchLogsAsync(searchText, caseSensitive);
            
            _logger?.LogDebug("✅ UNIFIED LOGGER SEARCH: Found {Count} matching logs", 
                result.IsSuccess ? result.Value?.MatchingLogs.Count ?? 0 : 0);
            
            return result.IsSuccess 
                ? Result<IReadOnlyList<LogEntry>>.Success(result.Value!.MatchingLogs)
                : Result<IReadOnlyList<LogEntry>>.Failure(result.ErrorMessage ?? "Search failed");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED LOGGER SEARCH ERROR: Search failed");
            return Result<IReadOnlyList<LogEntry>>.Failure(ex);
        }
    }
    
    #endregion
    
    #region UI Operations - Optional
    
    public async Task<Result<bool>> RefreshUIAsync()
    {
        try
        {
            if (!_hasUI)
            {
                _logger?.LogDebug("ℹ️ LOGGER REFRESH UI: No UI present, skipping");
                return Result<bool>.Success(true); // No-op for headless
            }
            
            // UI: Refresh actual UI component
            if (_uiManager != null)
            {
                await _uiManager.RefreshAllAsync();
            }
            
            _logger?.LogDebug("✅ LOGGER REFRESH UI: UI refreshed successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER REFRESH UI ERROR: UI refresh failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task SetLoggerLevelFilterAsync(LoggerLevel minimumLevel)
    {
        try
        {
            // CORE: Update core service filter
            await _coreService.SetLoggerLevelFilterAsync(minimumLevel);
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateFilterUIAsync();
            }
            
            _logger?.LogDebug("✅ LOGGER FILTER: Set minimum level to {Level}", minimumLevel);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER FILTER ERROR: Set filter failed");
        }
    }
    
    public void SetAutoScroll(bool enabled)
    {
        try
        {
            // UI: Auto-scroll is UI-only feature
            if (_hasUI && _uiManager != null)
            {
                _uiManager.SetAutoScroll(enabled);
            }
            
            _logger?.LogDebug("✅ LOGGER AUTO-SCROLL: Set to {Enabled}", enabled);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER AUTO-SCROLL ERROR: Set auto-scroll failed");
        }
    }
    
    #endregion
    
    #region State Queries
    
    public int GetTotalLogCount() => _coreService.TotalLogCount;
    
    public async Task<int> GetVisibleLogCountAsync()
    {
        // ADAPTIVE: Return UI visible count if UI present, otherwise total count
        if (_hasUI && _uiManager != null)
        {
            return _uiManager.GetVisibleLogCount();
        }
        
        return _coreService.TotalLogCount;
    }
    
    public bool HasLogs => _coreService.HasLogs;
    
    #endregion
    
    #region Reactive Properties
    
    public IObservable<LogChangeEvent> LogChanges => _coreService.LogChanges;
    
    #endregion
    
    #region Private Configuration Helpers
    
    /// <summary>
    /// Build enhanced configuration from all sources
    /// CONFIGURATION: Merge application-defined settings
    /// </summary>
    private LoggerConfiguration BuildEnhancedConfiguration(
        LoggerConfiguration? baseConfig,
        LoggerColorConfiguration? colorConfig,
        LoggerPerformanceConfiguration? performanceConfig,
        LogFilterOptions? filterConfig)
    {
        var config = baseConfig ?? LoggerConfiguration.Default;
        
        // PERFORMANCE: Apply performance configuration
        if (performanceConfig != null)
        {
            config = config with
            {
                MaxLogEntries = performanceConfig.MaxLogEntries,
                PerformanceConfig = performanceConfig
            };
        }
        
        // COLORS: Apply color configuration
        if (colorConfig != null)
        {
            config = config with
            {
                ColorConfig = colorConfig
            };
        }
        
        return config;
    }
    
    /// <summary>
    /// Apply color configuration to UI components
    /// UI: Update visual styling based on application preferences
    /// IMPLEMENTATION: Real XAML resource updates for application-defined logger colors
    /// </summary>
    private async Task ApplyColorConfigurationAsync(LoggerColorConfiguration? colorConfig)
    {
        if (!_hasUI || _uiManager == null || colorConfig == null || _uiComponent == null)
            return;
            
        try
        {
            await Task.Run(() =>
            {
                // DISPATCHER: Ensure UI updates happen on UI thread
                _uiComponent.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        // XAML RESOURCES: Update application-defined logger colors
                        var resources = _uiComponent.Resources;
                        
                        // BACKGROUND COLORS: Main logger background
                        if (colorConfig.BackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.BackgroundColor.Value);
                            resources["LoggerBackgroundBrush"] = brush;
                            _logger?.LogTrace("🎨 LOGGER BG: Applied background color - {Color}", colorConfig.BackgroundColor.Value);
                        }
                        
                        // DEFAULT TEXT COLORS: Base text color
                        if (colorConfig.DefaultTextColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.DefaultTextColor.Value);
                            resources["LoggerDefaultTextBrush"] = brush;
                        }
                        
                        // LOG LEVEL COLORS: Different colors for each log level
                        if (colorConfig.EnableColorCoding)
                        {
                            if (colorConfig.TraceColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.TraceColor.Value);
                                resources["LoggerTraceColorBrush"] = brush;
                            }
                            
                            if (colorConfig.DebugColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.DebugColor.Value);
                                resources["LoggerDebugColorBrush"] = brush;
                                _logger?.LogTrace("🎨 DEBUG COLOR: Applied debug log color - {Color}", colorConfig.DebugColor.Value);
                            }
                            
                            if (colorConfig.InfoColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.InfoColor.Value);
                                resources["LoggerInfoColorBrush"] = brush;
                                _logger?.LogTrace("🎨 INFO COLOR: Applied info log color - {Color}", colorConfig.InfoColor.Value);
                            }
                            
                            if (colorConfig.WarningColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.WarningColor.Value);
                                resources["LoggerWarningColorBrush"] = brush;
                                _logger?.LogTrace("🎨 WARNING COLOR: Applied warning log color - {Color}", colorConfig.WarningColor.Value);
                            }
                            
                            if (colorConfig.ErrorColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.ErrorColor.Value);
                                resources["LoggerErrorColorBrush"] = brush;
                                _logger?.LogTrace("🎨 ERROR COLOR: Applied error log color - {Color}", colorConfig.ErrorColor.Value);
                            }
                            
                            if (colorConfig.CriticalColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.CriticalColor.Value);
                                resources["LoggerCriticalColorBrush"] = brush;
                                _logger?.LogTrace("🎨 CRITICAL COLOR: Applied critical log color - {Color}", colorConfig.CriticalColor.Value);
                            }
                        }
                        
                        // TIMESTAMP COLORS: Timestamp text styling
                        if (colorConfig.TimestampColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.TimestampColor.Value);
                            resources["LoggerTimestampColorBrush"] = brush;
                        }
                        
                        // CATEGORY COLORS: Category text styling
                        if (colorConfig.CategoryColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.CategoryColor.Value);
                            resources["LoggerCategoryColorBrush"] = brush;
                        }
                        
                        // SELECTION COLORS: Selected log entry highlighting
                        if (colorConfig.SelectionBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.SelectionBackgroundColor.Value);
                            resources["LoggerSelectionBackgroundBrush"] = brush;
                            _logger?.LogTrace("🎨 SELECTION COLOR: Applied selection background - {Color}", colorConfig.SelectionBackgroundColor.Value);
                        }
                        
                        if (colorConfig.SelectionForegroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.SelectionForegroundColor.Value);
                            resources["LoggerSelectionForegroundBrush"] = brush;
                        }
                        
                        // BORDER COLORS: Logger component borders
                        if (colorConfig.BorderColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.BorderColor.Value);
                            resources["LoggerBorderBrush"] = brush;
                        }
                        
                        _logger?.LogDebug("🎨 LOGGER COLORS: Successfully applied {Count} color configurations to Logger UI", 
                            GetAppliedLoggerColorCount(colorConfig));
                    }
                    catch (Exception innerEx)
                    {
                        _logger?.Error(innerEx, "🚨 LOGGER COLOR APPLICATION ERROR: Failed to apply colors to UI thread");
                    }
                });
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "⚠️ LOGGER COLORS: Failed to apply color configuration");
        }
    }
    
    /// <summary>
    /// Count how many logger colors were actually applied for logging
    /// </summary>
    private int GetAppliedLoggerColorCount(LoggerColorConfiguration colorConfig)
    {
        int count = 0;
        if (colorConfig.BackgroundColor.HasValue) count++;
        if (colorConfig.DefaultTextColor.HasValue) count++;
        if (colorConfig.TraceColor.HasValue) count++;
        if (colorConfig.DebugColor.HasValue) count++;
        if (colorConfig.InfoColor.HasValue) count++;
        if (colorConfig.WarningColor.HasValue) count++;
        if (colorConfig.ErrorColor.HasValue) count++;
        if (colorConfig.CriticalColor.HasValue) count++;
        if (colorConfig.TimestampColor.HasValue) count++;
        if (colorConfig.CategoryColor.HasValue) count++;
        if (colorConfig.SelectionBackgroundColor.HasValue) count++;
        if (colorConfig.SelectionForegroundColor.HasValue) count++;
        if (colorConfig.BorderColor.HasValue) count++;
        return count;
    }
    
    #endregion
    
    #region IDisposable
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _coreService?.Dispose();
        _uiManager?.Dispose();
        _isDisposed = true;
        
        _logger?.LogDebug("🧹 UNIFIED LOGGER API: Disposed");
    }
    
    #endregion
}

/// <summary>
/// Logger API interface for dual-use scenarios
/// </summary>
public interface ILoggerAPI
{
    Task<Result<bool>> InitializeAsync(
        LoggerConfiguration? configuration = null,
        ILogger? logger = null,
        LoggerColorConfiguration? colorConfig = null,
        LoggerPerformanceConfiguration? performanceConfig = null,
        LogFilterOptions? filterConfig = null);
        
    bool IsInitialized { get; }
    
    Task<Result<bool>> AddLogAsync(LoggerLevel level, string message, Exception? exception = null);
    Task<Result<bool>> AddLogAsync(LoggerLevel level, string message, object? state = null, Exception? exception = null);
    Task<Result<bool>> ClearLogsAsync();
    
    Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportLogsAsync(
        LogExportFormat format = LogExportFormat.Dictionary,
        LogFilterOptions? filterOptions = null);
        
    Task<Result<IReadOnlyList<LogEntry>>> FilterLogsAsync(LogFilterOptions filterOptions);
    Task<Result<IReadOnlyList<LogEntry>>> SearchLogsAsync(string searchText, bool caseSensitive = false);
    
    Task<Result<bool>> RefreshUIAsync();
    Task SetLoggerLevelFilterAsync(LoggerLevel minimumLevel);
    void SetAutoScroll(bool enabled);
    
    int GetTotalLogCount();
    Task<int> GetVisibleLogCountAsync();
    bool HasLogs { get; }
    
    IObservable<LogChangeEvent> LogChanges { get; }
}

/// <summary>
/// Placeholder for LoggerUIManager - UI management for logger
/// </summary>
internal class LoggerUIManager : IDisposable
{
    private readonly UserControl _uiComponent;
    private readonly ILoggerService _coreService;
    private readonly ILogger? _logger;
    
    public LoggerUIManager(UserControl uiComponent, ILoggerService coreService, ILogger? logger)
    {
        _uiComponent = uiComponent;
        _coreService = coreService;
        _logger = logger;
    }
    
    public async Task<bool> InitializeAsync(LoggerConfiguration configuration)
    {
        // TODO: Implement UI initialization
        await Task.CompletedTask;
        return true;
    }
    
    public async Task UpdateAfterLogChangeAsync(string operation)
    {
        // TODO: Implement UI update after log changes
        await Task.CompletedTask;
    }
    
    public async Task RefreshAllAsync()
    {
        // TODO: Implement UI refresh
        await Task.CompletedTask;
    }
    
    public async Task UpdateFilterUIAsync()
    {
        // TODO: Implement filter UI update
        await Task.CompletedTask;
    }
    
    public void SetAutoScroll(bool enabled)
    {
        // TODO: Implement auto-scroll functionality
    }
    
    public int GetVisibleLogCount()
    {
        // TODO: Implement visible log count from UI
        return _coreService.TotalLogCount;
    }
    
    public void Dispose()
    {
        // TODO: Implement UI cleanup
    }
}