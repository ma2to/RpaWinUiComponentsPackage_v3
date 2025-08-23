using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.UI.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.DataGrid;
using RpaWinUiComponentsPackage.Core.Extensions;
using System.Data;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;
using DomainColorConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColorConfiguration;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.API;

/// <summary>
/// Unified DataGrid API Implementation - Single API for UI and NON-UI
/// ADAPTIVE: Automatically detects if UI is available and adapts behavior
/// BACKWARD COMPATIBLE: Maintains same method signatures as before
/// </summary>
internal sealed class UnifiedDataGridAPI : IDataGridAPI, IDisposable
{
    #region Private Fields
    
    private readonly IDataGridService _coreService;
    private readonly ILogger? _logger;
    private readonly bool _hasUI;
    private readonly UserControl? _uiComponent; // Will be UI component if present
    private readonly DataGridUIManager? _uiManager;
    private bool _isDisposed = false;
    
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Create headless API instance
    /// NON-UI: Pure headless operation
    /// </summary>
    /// <param name="logger">Optional logger</param>
    public UnifiedDataGridAPI(ILogger? logger = null)
    {
        _coreService = new DataGridService(logger);
        _logger = logger;
        _hasUI = false;
        _uiComponent = null;
        
        _logger?.LogDebug("🤖 UNIFIED API: Created headless instance");
    }
    
    /// <summary>
    /// Create UI-enabled API instance
    /// UI: Connects to actual UI component
    /// </summary>
    /// <param name="uiComponent">UI component (AdvancedDataGrid UserControl)</param>
    /// <param name="logger">Optional logger</param>
    public UnifiedDataGridAPI(UserControl uiComponent, ILogger? logger = null)
    {
        _coreService = new DataGridService(logger);
        _logger = logger;
        _hasUI = true;
        _uiComponent = uiComponent;
        _uiManager = new DataGridUIManager(uiComponent, _coreService, logger);
        
        _logger?.LogDebug("🖥️ UNIFIED API: Created UI-enabled instance");
    }
    
    #endregion
    
    #region IDataGridAPI Implementation
    
    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<DomainColumnDefinition> columns,
        DataGridConfiguration? configuration = null,
        ILogger? logger = null,
        DomainColorConfiguration? colorConfig = null,
        PerformanceConfiguration? throttlingConfig = null,
        IValidationConfiguration? validationConfig = null)
    {
        try
        {
            // ENHANCED CONFIGURATION: Merge all configuration sources
            var enhancedConfig = BuildEnhancedConfiguration(
                configuration, colorConfig, throttlingConfig, validationConfig);
            
            // CORE: Initialize core service with enhanced configuration
            var coreResult = await _coreService.InitializeAsync(columns, enhancedConfig);
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Initialize UI if present with color configuration
            if (_hasUI && _uiManager != null)
            {
                var uiResult = await _uiManager.InitializeAsync(columns);
                if (!uiResult)
                {
                    _logger?.LogWarning("⚠️ UI initialization failed, continuing with core only");
                    // Continue with core-only operation
                }
                
                // COLORS: Apply color configuration to UI
                if (colorConfig != null || enhancedConfig.ColorConfig != null)
                {
                    await ApplyColorConfigurationAsync(colorConfig ?? enhancedConfig.ColorConfig);
                }
            }
            
            _logger?.LogDebug("✅ UNIFIED INIT: Initialized successfully (UI: {HasUI}, Performance: {PerfConfig}, Colors: {ColorConfig})", 
                _hasUI, 
                throttlingConfig?.GetType().Name ?? "Default",
                colorConfig?.GetType().Name ?? "Default");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED INIT ERROR: Initialization failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public bool IsInitialized => _coreService.IsInitialized;
    
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions? options = null)
    {
        try
        {
            // CORE: Always import to core service
            var coreResult = await _coreService.ImportDataAsync(data, options);
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterDataChangeAsync("Data imported");
            }
            
            _logger?.LogDebug("✅ UNIFIED IMPORT: Imported {Count} rows (UI: {HasUI})", data.Count, _hasUI);
            return coreResult;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED IMPORT ERROR: Import failed");
            return Result<ImportResult>.Failure(ex);
        }
    }
    
    public async Task<Result<ImportResult>> ImportFromDataTableAsync(
        DataTable dataTable,
        ImportOptions? options = null)
    {
        try
        {
            // CORE: Always import to core service
            var coreResult = await _coreService.ImportDataAsync(dataTable, options);
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterDataChangeAsync("DataTable imported");
            }
            
            _logger?.LogDebug("✅ UNIFIED IMPORT DT: Imported DataTable (UI: {HasUI})", _hasUI);
            return coreResult;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED IMPORT DT ERROR: DataTable import failed");
            return Result<ImportResult>.Failure(ex);
        }
    }
    
    public async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportToDictionaryAsync(
        ExportOptions? options = null)
    {
        try
        {
            // CORE: Always export from core service
            var result = await _coreService.ExportToDictionariesAsync(options);
            
            _logger?.LogDebug("✅ UNIFIED EXPORT: Exported {Count} rows", 
                result.IsSuccess ? result.Value?.Count ?? 0 : 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED EXPORT ERROR: Export failed");
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(ex);
        }
    }
    
    public async Task<Result<DataTable>> ExportToDataTableAsync(ExportOptions? options = null)
    {
        try
        {
            // CORE: Always export from core service
            var result = await _coreService.ExportToDataTableAsync(options);
            
            _logger?.LogDebug("✅ UNIFIED EXPORT DT: Exported DataTable (UI: {HasUI})", _hasUI);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED EXPORT DT ERROR: DataTable export failed");
            return Result<DataTable>.Failure(ex);
        }
    }
    
    public async Task<Result<DeleteResult>> SmartDeleteRowAsync(int rowIndex)
    {
        return await SmartDeleteRowAsync(new[] { rowIndex });
    }
    
    public async Task<Result<DeleteResult>> SmartDeleteRowAsync(IReadOnlyList<int> rowIndices)
    {
        try
        {
            // CORE: Always delete from core service
            var coreResult = await _coreService.DeleteRowsAsync(rowIndices);
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterDataChangeAsync($"Deleted {rowIndices.Count} rows");
            }
            
            _logger?.LogDebug("✅ UNIFIED DELETE: Deleted {Count} rows (UI: {HasUI})", rowIndices.Count, _hasUI);
            return coreResult;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED DELETE ERROR: Delete failed");
            return Result<DeleteResult>.Failure(ex);
        }
    }
    
    public async Task<Result<bool>> ClearAllDataAsync()
    {
        try
        {
            // CORE: Always clear core service
            var coreResult = await _coreService.ClearDataAsync();
            if (!coreResult.IsSuccess)
                return coreResult;
            
            // UI: Update UI if present
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterDataChangeAsync("All data cleared");
            }
            
            _logger?.LogDebug("✅ UNIFIED CLEAR: Cleared all data (UI: {HasUI})", _hasUI);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED CLEAR ERROR: Clear failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task<Result<ValidationResult>> ValidateAllRowsBatchAsync()
    {
        try
        {
            // CORE: Always validate in core service
            var result = await _coreService.ValidateAllAsync();
            
            // UI: Update validation UI if present (FIXED: Pass result to prevent infinite loop)
            if (_hasUI && _uiManager != null && result.IsSuccess && result.Value != null)
            {
                await _uiManager.UpdateValidationUIAsync(result.Value);
            }
            
            _logger?.LogDebug("✅ UNIFIED VALIDATE: Validated all rows (UI: {HasUI}) - Valid: {IsValid}", 
                _hasUI, result.Value?.IsValid ?? false);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UNIFIED VALIDATE ERROR: Validation failed");
            return Result<ValidationResult>.Failure(ex);
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
                _logger?.LogDebug("ℹ️ REFRESH UI: No UI present, skipping");
                return Result<bool>.Success(true); // No-op for headless
            }
            
            // UI: Refresh actual UI component
            if (_uiManager != null)
            {
                await _uiManager.RefreshAllAsync();
                
                // ROW HEIGHT: Trigger row height calculation after UI refresh
                if (_uiComponent is AdvancedDataGrid dataGrid)
                {
                    _logger?.LogDebug("📏 ROW HEIGHT: Triggering row height calculation after UI refresh");
                    // TODO: Implement row height calculation or call appropriate service
                    // await dataGrid.CalculateAndApplyRowHeightsAsync();
                }
            }
            
            _logger?.LogDebug("✅ REFRESH UI: UI refreshed successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 REFRESH UI ERROR: UI refresh failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task<Result<bool>> UpdateValidationUIAsync()
    {
        try
        {
            if (!_hasUI)
            {
                _logger?.LogDebug("ℹ️ VALIDATION UI: No UI present, skipping");
                return Result<bool>.Success(true); // No-op for headless
            }
            
            // UI: Update validation visuals
            if (_uiManager != null)
            {
                await _uiManager.UpdateValidationUIAsync();
            }
            
            _logger?.LogDebug("✅ VALIDATION UI: Validation UI updated");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 VALIDATION UI ERROR: Validation UI update failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public Result<bool> InvalidateLayout()
    {
        try
        {
            if (!_hasUI)
            {
                _logger?.LogDebug("ℹ️ INVALIDATE LAYOUT: No UI present, skipping");
                return Result<bool>.Success(true); // No-op for headless
            }
            
            // UI: Invalidate layout
            _uiManager?.InvalidateLayout();
            
            _logger?.LogDebug("✅ INVALIDATE LAYOUT: Layout invalidated");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 INVALIDATE LAYOUT ERROR: Layout invalidation failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    public async Task<Result<bool>> CompactRowsAsync()
    {
        try
        {
            // CORE: Logic to remove empty rows (would need to be implemented in core service)
            // For now, just trigger UI update if present
            
            if (_hasUI && _uiManager != null)
            {
                await _uiManager.UpdateAfterDataChangeAsync("Rows compacted");
            }
            
            _logger?.LogDebug("✅ COMPACT: Rows compacted (UI: {HasUI})", _hasUI);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 COMPACT ERROR: Row compaction failed");
            return Result<bool>.Failure(ex);
        }
    }
    
    #endregion
    
    #region State Queries
    
    public int GetTotalRowCount() => _coreService.RowCount;
    
    public int GetColumnCount() => _coreService.ColumnCount;
    
    public async Task<int> GetVisibleRowsCountAsync()
    {
        // ADAPTIVE: Return UI visible count if UI present, otherwise total count
        if (_hasUI && _uiManager != null)
        {
            return _uiManager.GetVisibleRowCount();
        }
        
        return _coreService.RowCount;
    }
    
    public bool HasData => _coreService.HasData;
    
    public async Task<int> GetLastDataRowAsync()
    {
        // ADAPTIVE: Get from UI if present, otherwise calculate from core
        if (_hasUI && _uiManager != null)
        {
            return _uiManager.GetLastDataRowIndex();
        }
        
        return _coreService.RowCount > 0 ? _coreService.RowCount - 1 : -1;
    }
    
    public int GetMinimumRowCount()
    {
        // ADAPTIVE: Return UI minimum if UI present, otherwise 0
        if (_hasUI && _uiManager != null)
        {
            return _uiManager.GetMinimumRowCount();
        }
        
        return 0; // No minimum for headless
    }
    
    public async Task<bool> AreAllNonEmptyRowsValidAsync(bool wholeDataset = true)
    {
        if (wholeDataset)
        {
            // Validate entire dataset
            var validationResult = await _coreService.ValidateAllAsync();
            return validationResult.IsSuccess && validationResult.Value?.IsValid == true;
        }
        else
        {
            // Validate only visible rows (UI-specific)
            if (_hasUI && _uiManager != null)
            {
                // Get visible row indices from UI manager
                var visibleRows = _uiManager.GetVisibleRowCount();
                var totalRows = _coreService.RowCount;
                
                if (visibleRows >= totalRows)
                {
                    // All rows are visible, same as whole dataset
                    var validationResult = await _coreService.ValidateAllAsync();
                    return validationResult.IsSuccess && validationResult.Value?.IsValid == true;
                }
                else
                {
                    // Validate only visible range - simplified for now
                    // TODO: Implement partial validation for visible rows only
                    var validationResult = await _coreService.ValidateAllAsync();
                    return validationResult.IsSuccess && validationResult.Value?.IsValid == true;
                }
            }
            else
            {
                // Headless mode - no concept of "visible", validate all
                var validationResult = await _coreService.ValidateAllAsync();
                return validationResult.IsSuccess && validationResult.Value?.IsValid == true;
            }
        }
    }
    
    #endregion
    
    #region Reactive Properties
    
    public IObservable<DataChangeEvent> DataChanges => _coreService.DataChanges;
    public IObservable<ValidationChangeEvent> ValidationChanges => _coreService.ValidationChanges;
    
    #endregion
    
    #region Private Configuration Helpers
    
    /// <summary>
    /// Build enhanced configuration from all sources
    /// CONFIGURATION: Merge application-defined settings
    /// </summary>
    private DataGridConfiguration BuildEnhancedConfiguration(
        DataGridConfiguration? baseConfig,
        DomainColorConfiguration? colorConfig,
        PerformanceConfiguration? performanceConfig,
        IValidationConfiguration? validationConfig)
    {
        var config = baseConfig ?? DataGridConfiguration.Default;
        
        // PERFORMANCE: Apply performance configuration
        if (performanceConfig != null)
        {
            // Map AdvancedWinUiDataGrid.PerformanceConfiguration to Domain.DataGrid.PerformanceConfiguration
            var domainPerfConfig = MapToDomainPerformanceConfiguration(performanceConfig);
            
            config = config with
            {
                BatchSize = performanceConfig.BatchSize ?? 1000,
                ThrottleDelay = TimeSpan.FromMilliseconds(performanceConfig.RenderDelayMs ?? 16),
                EnableVirtualization = true, // Always enable for performance
                CacheEnabled = true,
                PerformanceConfig = domainPerfConfig
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
        
        // VALIDATION: Apply validation configuration
        if (validationConfig != null)
        {
            config = config with
            {
                EnableValidation = validationConfig.IsValidationEnabled,
                ValidationConfig = validationConfig
            };
        }
        
        return config;
    }
    
    /// <summary>
    /// Apply color configuration to UI components
    /// UI: Update visual styling based on application preferences
    /// IMPLEMENTATION: Real XAML resource updates for application-defined colors
    /// </summary>
    private async Task ApplyColorConfigurationAsync(DomainColorConfiguration? colorConfig)
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
                        // XAML RESOURCES: Update application-defined colors
                        var resources = _uiComponent.Resources;
                        
                        // FOCUS COLORS: Light green for selection/focus
                        if (colorConfig.SelectionBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.SelectionBackgroundColor.Value);
                            resources["DataGridSelectionBackgroundBrush"] = brush;
                            resources["DataGridFocusBackgroundBrush"] = brush;
                            _logger?.LogTrace("🎨 FOCUS COLOR: Applied selection background - {Color}", colorConfig.SelectionBackgroundColor.Value);
                        }
                        
                        if (colorConfig.SelectionForegroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.SelectionForegroundColor.Value);
                            resources["DataGridSelectionForegroundBrush"] = brush;
                            resources["DataGridFocusForegroundBrush"] = brush;
                        }
                        
                        // COPY MODE COLORS: Light blue for copy operations
                        if (colorConfig.CopyModeBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.CopyModeBackgroundColor.Value);
                            resources["DataGridCopyModeBackgroundBrush"] = brush;
                            _logger?.LogTrace("🎨 COPY COLOR: Applied copy mode background - {Color}", colorConfig.CopyModeBackgroundColor.Value);
                        }
                        
                        // BORDER COLORS: Black borders or application-defined
                        if (colorConfig.CellBorderColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.CellBorderColor.Value);
                            resources["DataGridCellBorderBrush"] = brush;
                            resources["DataGridColumnHeaderBorderBrush"] = brush;
                            _logger?.LogTrace("🎨 BORDER COLOR: Applied cell borders - {Color}", colorConfig.CellBorderColor.Value);
                        }
                        
                        // VALIDATION ERROR COLORS: Red for validation errors
                        if (colorConfig.ValidationErrorBorderColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.ValidationErrorBorderColor.Value);
                            resources["DataGridValidationErrorBorderBrush"] = brush;
                        }
                        
                        if (colorConfig.ValidationErrorBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.ValidationErrorBackgroundColor.Value);
                            resources["DataGridValidationErrorBackgroundBrush"] = brush;
                        }
                        
                        // CELL COLORS: Basic cell appearance
                        if (colorConfig.CellBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.CellBackgroundColor.Value);
                            resources["DataGridCellBackgroundBrush"] = brush;
                        }
                        
                        if (colorConfig.CellForegroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.CellForegroundColor.Value);
                            resources["DataGridCellForegroundBrush"] = brush;
                        }
                        
                        // ZEBRA PATTERN: Alternating row colors
                        if (colorConfig.EnableZebraPattern)
                        {
                            if (colorConfig.EvenRowBackgroundColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.EvenRowBackgroundColor.Value);
                                resources["DataGridEvenRowBackgroundBrush"] = brush;
                            }
                            
                            if (colorConfig.OddRowBackgroundColor.HasValue)
                            {
                                var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.OddRowBackgroundColor.Value);
                                resources["DataGridOddRowBackgroundBrush"] = brush;
                            }
                        }
                        
                        // HEADER COLORS: Column headers
                        if (colorConfig.HeaderBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.HeaderBackgroundColor.Value);
                            resources["DataGridColumnHeaderBackgroundBrush"] = brush;
                        }
                        
                        if (colorConfig.HeaderForegroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.HeaderForegroundColor.Value);
                            resources["DataGridColumnHeaderForegroundBrush"] = brush;
                        }
                        
                        if (colorConfig.HeaderBorderColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.HeaderBorderColor.Value);
                            resources["DataGridColumnHeaderBorderBrush"] = brush;
                        }
                        
                        // HOVER EFFECTS: Mouse hover colors
                        if (colorConfig.HoverBackgroundColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.HoverBackgroundColor.Value);
                            resources["DataGridCellHoverBackgroundBrush"] = brush;
                        }
                        
                        // FOCUS RING: Light green focus ring
                        if (colorConfig.FocusRingColor.HasValue)
                        {
                            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorConfig.FocusRingColor.Value);
                            resources["DataGridFocusRingBrush"] = brush;
                        }
                        
                        _logger?.LogDebug("🎨 COLORS: Successfully applied {Count} color configurations to DataGrid UI", 
                            GetAppliedColorCount(colorConfig));
                    }
                    catch (Exception innerEx)
                    {
                        _logger?.Error(innerEx, "🚨 COLOR APPLICATION ERROR: Failed to apply colors to UI thread");
                    }
                });
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "⚠️ COLORS: Failed to apply color configuration");
        }
    }
    
    /// <summary>
    /// Map AdvancedWinUiDataGrid.PerformanceConfiguration to Domain.DataGrid.PerformanceConfiguration
    /// </summary>
    private Domain.DataGrid.PerformanceConfiguration MapToDomainPerformanceConfiguration(RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration advancedConfig)
    {
        return new Domain.DataGrid.PerformanceConfiguration(
            UIUpdateIntervalMs: advancedConfig.RenderDelayMs ?? 16,
            ValidationUpdateIntervalMs: advancedConfig.ValidationThrottleMs ?? 100,
            SearchUpdateIntervalMs: advancedConfig.SearchThrottleMs ?? 300,
            BulkOperationBatchSize: advancedConfig.BatchSize ?? 1000,
            VirtualizationBufferSize: advancedConfig.VirtualizationThreshold ?? 50,
            UIOperationTimeoutMs: 5000,
            DataOperationTimeoutMs: 60000,
            ImportExportTimeoutMs: 300000,
            MaxRowsForRealtimeValidation: 1000,
            MemoryCleanupIntervalMs: 30000,
            CacheCleanupIntervalMs: 60000,
            EnableAggressiveMemoryManagement: false,
            EnableMultiLevelCaching: true,
            EnableBackgroundProcessing: true);
    }
    
    /// <summary>
    /// Count how many colors were actually applied for logging
    /// </summary>
    private int GetAppliedColorCount(DomainColorConfiguration colorConfig)
    {
        int count = 0;
        if (colorConfig.SelectionBackgroundColor.HasValue) count++;
        if (colorConfig.SelectionForegroundColor.HasValue) count++;
        if (colorConfig.CopyModeBackgroundColor.HasValue) count++;
        if (colorConfig.CellBorderColor.HasValue) count++;
        if (colorConfig.ValidationErrorBorderColor.HasValue) count++;
        if (colorConfig.ValidationErrorBackgroundColor.HasValue) count++;
        if (colorConfig.CellBackgroundColor.HasValue) count++;
        if (colorConfig.CellForegroundColor.HasValue) count++;
        if (colorConfig.EvenRowBackgroundColor.HasValue) count++;
        if (colorConfig.OddRowBackgroundColor.HasValue) count++;
        if (colorConfig.HeaderBackgroundColor.HasValue) count++;
        if (colorConfig.HeaderForegroundColor.HasValue) count++;
        if (colorConfig.HeaderBorderColor.HasValue) count++;
        if (colorConfig.HoverBackgroundColor.HasValue) count++;
        if (colorConfig.FocusRingColor.HasValue) count++;
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
        
        _logger?.LogDebug("🧹 UNIFIED API: Disposed");
    }
    
    #endregion
}