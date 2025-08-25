namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// Performance configuration for AdvancedDataGrid compatibility
/// Compatibility wrapper for Internal.Models.PerformanceConfiguration
/// </summary>
public class PerformanceConfiguration
{
    public bool EnableVirtualization { get; set; } = true;
    public int VirtualizationThreshold { get; set; } = 1000;
    public bool EnableBackgroundProcessing { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public int CacheSize { get; set; } = 10000;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableThrottling { get; set; } = true;
    public TimeSpan ThrottleDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public int MaxConcurrentOperations { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Convert to internal performance configuration
    /// </summary>
    public Internal.Models.PerformanceConfiguration ToInternal()
    {
        return new Internal.Models.PerformanceConfiguration
        {
            EnableVirtualization = this.EnableVirtualization,
            VirtualizationThreshold = this.VirtualizationThreshold,
            EnableBackgroundProcessing = this.EnableBackgroundProcessing,
            EnableCaching = this.EnableCaching,
            CacheSize = this.CacheSize,
            OperationTimeout = this.OperationTimeout,
            EnableThrottling = this.EnableThrottling,
            ThrottleDelay = this.ThrottleDelay,
            MaxConcurrentOperations = this.MaxConcurrentOperations
        };
    }

    /// <summary>
    /// Create from internal performance configuration
    /// </summary>
    public static PerformanceConfiguration FromInternal(Internal.Models.PerformanceConfiguration internal_config)
    {
        return new PerformanceConfiguration
        {
            EnableVirtualization = internal_config.EnableVirtualization,
            VirtualizationThreshold = internal_config.VirtualizationThreshold,
            EnableBackgroundProcessing = internal_config.EnableBackgroundProcessing,
            EnableCaching = internal_config.EnableCaching,
            CacheSize = internal_config.CacheSize,
            OperationTimeout = internal_config.OperationTimeout,
            EnableThrottling = internal_config.EnableThrottling,
            ThrottleDelay = internal_config.ThrottleDelay,
            MaxConcurrentOperations = internal_config.MaxConcurrentOperations
        };
    }
}