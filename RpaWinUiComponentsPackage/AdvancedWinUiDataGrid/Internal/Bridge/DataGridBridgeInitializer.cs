using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using CorePerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.PerformanceConfiguration;
using AdvancedPerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Bridge;

/// <summary>
/// PROFESSIONAL Initialization Manager for DataGridBridge
/// RESPONSIBILITY: Handle initialization logic with configuration mapping
/// ARCHITECTURE: Single Responsibility Principle
/// </summary>
internal sealed class DataGridBridgeInitializer : IDisposable
{
    #region Private Fields

    private readonly AdvancedDataGrid _internalGrid;
    private readonly ILogger? _logger;
    private bool _isDisposed;

    #endregion

    #region Constructor

    public DataGridBridgeInitializer(AdvancedDataGrid internalGrid, ILogger? logger)
    {
        _internalGrid = internalGrid ?? throw new ArgumentNullException(nameof(internalGrid));
        _logger = logger;
        
        _logger?.Info("🔧 INITIALIZER: Created DataGridBridgeInitializer");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// PROFESSIONAL initialization with configuration mapping and error handling
    /// </summary>
    public async Task<bool> InitializeAsync(
        IReadOnlyList<ColumnConfiguration> columns,
        ColorConfiguration? colorConfig,
        ValidationConfiguration? validationConfig,
        CorePerformanceConfiguration? performanceConfig,
        int minimumRows,
        bool enableSort,
        bool enableSearch,
        bool enableFilter,
        bool enableRealtimeValidation,
        bool enableBatchValidation,
        int maxRowsForOptimization,
        TimeSpan operationTimeout,
        ILogger? logger)
    {
        try
        {
            _logger?.Info("🔧 INITIALIZER: Starting initialization with {ColumnCount} columns", columns?.Count ?? 0);
            
            if (columns == null || !columns.Any())
            {
                _logger?.Error("❌ INITIALIZER ERROR: No columns provided for initialization");
                return false;
            }

            // Map CorePerformanceConfiguration to AdvancedPerformanceConfiguration
            var mappedPerformanceConfig = MapPerformanceConfiguration(performanceConfig);
            
            _logger?.Info("🔧 INITIALIZER: Mapped performance configuration - Virtualization: {Virtualization}, Background: {Background}", 
                mappedPerformanceConfig?.VirtualizationThreshold ?? 0,
                mappedPerformanceConfig?.EnableBackgroundProcessing ?? false);

            // Call internal grid initialization
            var result = await _internalGrid.InitializeAsync(
                columns: columns,
                colors: colorConfig,
                validation: validationConfig,
                performance: mappedPerformanceConfig,
                emptyRowsCount: minimumRows,
                logger: logger ?? _logger);

            if (result.IsSuccess)
            {
                _logger?.Info("✅ INITIALIZER SUCCESS: DataGrid initialized successfully with {Rows} minimum rows", minimumRows);
                return true;
            }
            else
            {
                _logger?.Error("❌ INITIALIZER FAILED: {ErrorMessage}", result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 INITIALIZER CRITICAL ERROR: Exception during initialization");
            return false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// PROFESSIONAL configuration mapping with null-safe defaults
    /// </summary>
    private AdvancedPerformanceConfiguration? MapPerformanceConfiguration(CorePerformanceConfiguration? coreConfig)
    {
        if (coreConfig == null)
        {
            _logger?.Info("🔧 INITIALIZER: Using default performance configuration");
            return new AdvancedPerformanceConfiguration
            {
                VirtualizationThreshold = 1000,
                EnableThrottling = true,
                ThrottleDelay = TimeSpan.FromMilliseconds(100)
            };
        }

        _logger?.Info("🔧 INITIALIZER: Mapping custom performance configuration");
        return new AdvancedPerformanceConfiguration
        {
            VirtualizationThreshold = coreConfig.VirtualizationThreshold,
            EnableThrottling = coreConfig.EnableThrottling,
            ThrottleDelay = coreConfig.EnableThrottling ? coreConfig.ThrottleDelay : TimeSpan.Zero
        };
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _logger?.Info("🔧 INITIALIZER DISPOSE: Cleaning up initialization resources");
            // No specific resources to dispose for initializer
            _isDisposed = true;
        }
    }

    #endregion
}