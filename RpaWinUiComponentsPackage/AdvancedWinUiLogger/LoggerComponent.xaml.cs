using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.API;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Common;
using RpaWinUiComponentsPackage.Core.Extensions;
using System.Collections.ObjectModel;
using Windows.UI;
using DomainLoggerLevel = RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Logger.LoggerLevel;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger;

/// <summary>
/// LoggerComponent UserControl - WinUI3 Component with Hybrid Functional-OOP Architecture
/// DUAL-USE: Works as UI component or through unified API for headless operations
/// INDEPENDENT: Completely independent from DataGrid components
/// BACKWARD COMPATIBLE: Maintains same public interface as before
/// </summary>
public sealed partial class LoggerComponent : UserControl
{
    #region Private Fields
    
    private readonly UnifiedLoggerAPI _api;
    private readonly ILogger? _logger;
    private bool _isInitialized = false;
    
    // UI STATE: Observable collections for data binding
    private readonly ObservableCollection<LogEntryViewModel> _logEntries = new();
    
    #endregion
    
    #region Constructor
    
    public LoggerComponent()
    {
        this.InitializeComponent();
        
        // API: Create unified API with UI component reference
        _api = new UnifiedLoggerAPI(this);
        
        // BINDING: Connect observable collections to UI
        // TODO: Connect UI when full XAML is implemented
        
        // EVENTS: Wire up UI events  
        // TODO: Wire up events when full XAML is implemented
        
        _logger?.LogDebug("🖥️ LOGGER COMPONENT: UI component created");
    }
    
    #endregion
    
    #region Public Static Factory Methods
    
    /// <summary>
    /// Create headless Logger API instance (no UI)
    /// NON-UI: Pure functional operation for automation scripts
    /// </summary>
    /// <param name="logger">Optional logger</param>
    /// <returns>Unified API for headless operations</returns>
    public static UnifiedLoggerAPI CreateHeadless(ILogger? logger = null)
    {
        return new UnifiedLoggerAPI(logger);
    }
    
    #endregion
    
    #region Public API Methods - UI Component Interface
    
    /// <summary>
    /// Initialize Logger with configuration
    /// UNIFIED: Same method signature for UI and headless usage
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(
        LoggerConfiguration? configuration = null,
        ILogger? logger = null,
        object? colorConfig = null,
        object? performanceConfig = null)
    {
        try
        {
            // COLORS: Convert dynamic color config to typed config
            LoggerColorConfiguration? typedColorConfig = null;
            if (colorConfig != null)
            {
                typedColorConfig = ConvertToLoggerColorConfiguration(colorConfig);
            }
            
            // PERFORMANCE: Convert dynamic performance config to typed config
            LoggerPerformanceConfiguration? typedPerformanceConfig = null;
            if (performanceConfig != null)
            {
                typedPerformanceConfig = ConvertToLoggerPerformanceConfiguration(performanceConfig);
            }
            
            // API: Initialize through unified API
            var result = await _api.InitializeAsync(
                configuration, 
                logger, 
                typedColorConfig,
                typedPerformanceConfig);
            
            if (result.IsSuccess)
            {
                _isInitialized = true;
                
                // UI: Update status
                // TODO: Update status when full XAML is implemented
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER INIT ERROR: Initialization failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Add log entry
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public async Task<Result<bool>> AddLogAsync(
        LoggerLevel level,
        string message,
        Exception? exception = null)
    {
        return await AddLogAsync(level, message, null, exception);
    }
    
    /// <summary>
    /// Add log entry with state
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public async Task<Result<bool>> AddLogAsync(
        LoggerLevel level,
        string message,
        object? state = null,
        Exception? exception = null)
    {
        try
        {
            // API: Add log through unified API
            var result = await _api.AddLogAsync(level, message, state, exception);
            
            if (result.IsSuccess)
            {
                // UI: Add to display collection
                await AddLogEntryToUIAsync(level, message, exception);
                
                // UI: Update count
                // TODO: Update count when full XAML is implemented
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER ADD ERROR: Add log failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Clear all logs
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public async Task<Result<bool>> ClearLogsAsync()
    {
        try
        {
            // API: Clear through unified API
            var result = await _api.ClearLogsAsync();
            
            if (result.IsSuccess)
            {
                // UI: Clear display collection
                _logEntries.Clear();
                // TODO: Update count when full XAML is implemented
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER CLEAR ERROR: Clear failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Export logs
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public async Task<IList<IReadOnlyDictionary<string, object?>>> ExportLogsAsync()
    {
        try
        {
            // API: Export through unified API
            var result = await _api.ExportLogsAsync();
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.ToList();
            }
            
            return new List<IReadOnlyDictionary<string, object?>>();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER EXPORT ERROR: Export failed");
            return new List<IReadOnlyDictionary<string, object?>>();
        }
    }
    
    /// <summary>
    /// Search logs
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public async Task<Result<IReadOnlyList<LogEntry>>> SearchLogsAsync(string searchText, bool caseSensitive = false)
    {
        try
        {
            return await _api.SearchLogsAsync(searchText, caseSensitive);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER SEARCH ERROR: Search failed");
            return Result<IReadOnlyList<LogEntry>>.Failure(ex);
        }
    }
    
    /// <summary>
    /// Set log level filter
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public async Task SetLoggerLevelFilterAsync(LoggerLevel minimumLevel)
    {
        try
        {
            await _api.SetLoggerLevelFilterAsync(minimumLevel);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER FILTER ERROR: Set filter failed");
        }
    }
    
    /// <summary>
    /// Set auto scroll
    /// UNIFIED: Same method for UI and headless operations
    /// </summary>
    public void SetAutoScroll(bool enabled)
    {
        try
        {
            _api.SetAutoScroll(enabled);
            // TODO: Update toggle when full XAML is implemented
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 LOGGER AUTO-SCROLL ERROR: Set auto-scroll failed");
        }
    }
    
    #endregion
    
    #region State Queries
    
    /// <summary>
    /// Get total log count
    /// </summary>
    public int GetTotalLogCount() => _api.GetTotalLogCount();
    
    /// <summary>
    /// Check if Logger has logs
    /// </summary>
    public bool HasLogs => _api.HasLogs;
    
    /// <summary>
    /// Check if Logger is initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    #endregion
    
    #region Private Event Handlers
    
    // TODO: Implement event handlers when full XAML is implemented
    private async void OnClearLogsClick(object sender, RoutedEventArgs e)
    {
        await ClearLogsAsync();
    }
    
    private async void OnExportLogsClick(object sender, RoutedEventArgs e)
    {
        await ExportLogsAsync();
    }
    
    private void OnAutoScrollChanged(object sender, RoutedEventArgs e)
    {
        SetAutoScroll(true); // Default to enabled since no UI toggle yet
    }
    
    #endregion
    
    #region Private UI Update Methods
    
    /// <summary>
    /// Add log entry to UI display
    /// UI: Sync log entries with data model
    /// </summary>
    private async Task AddLogEntryToUIAsync(LoggerLevel level, string message, Exception? exception = null)
    {
        await Task.Run(() =>
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                var logEntry = new LogEntryViewModel
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message,
                    Category = "Logger",
                    Exception = exception
                };
                
                _logEntries.Add(logEntry);
                
                // AUTO-SCROLL: Scroll to bottom if enabled
                // TODO: Implement auto-scroll when full XAML is implemented
            });
        });
    }
    
    #endregion
    
    #region Private Configuration Helpers
    
    /// <summary>
    /// Convert dynamic color configuration to typed configuration
    /// </summary>
    private LoggerColorConfiguration? ConvertToLoggerColorConfiguration(object colorConfig)
    {
        // TODO: Implement proper conversion from anonymous object to LoggerColorConfiguration
        // For now, return null to prevent errors
        return null;
    }
    
    /// <summary>
    /// Convert dynamic performance configuration to typed configuration
    /// </summary>
    private LoggerPerformanceConfiguration? ConvertToLoggerPerformanceConfiguration(object performanceConfig)
    {
        // TODO: Implement proper conversion from anonymous object to LoggerPerformanceConfiguration
        // For now, return null to prevent errors
        return null;
    }
    
    #endregion
}

/// <summary>
/// UI Model for log entries
/// </summary>
internal class LogEntryViewModel
{
    public DateTime Timestamp { get; set; }
    public LoggerLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

/// <summary>
/// Converter for LoggerLevel to Brush for UI styling
/// </summary>
internal class LoggerLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is LoggerLevel level)
        {
            return level switch
            {
                LoggerLevel.Trace => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                LoggerLevel.Debug => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue),
                LoggerLevel.Information => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                LoggerLevel.Warning => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                LoggerLevel.Error => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                LoggerLevel.Critical => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed),
                _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black)
            };
        }
        
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}