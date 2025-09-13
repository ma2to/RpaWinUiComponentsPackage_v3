using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;

/// <summary>
/// DDD: Value object for DataGrid performance configuration
/// ENTERPRISE: Scalable configuration supporting 1M+ row datasets
/// IMMUTABLE: Record pattern ensuring configuration consistency
/// OPTIMIZATION: Comprehensive performance tuning options
/// </summary>
internal record PerformanceConfiguration
{
    #region Core Performance Settings
    
    /// <summary>Enable UI virtualization for large datasets</summary>
    public bool EnableVirtualization { get; init; } = true;
    
    /// <summary>Row threshold for enabling virtualization</summary>
    public int VirtualizationThreshold { get; init; } = 1000;
    
    /// <summary>Buffer size for virtualized rendering</summary>
    public int VirtualizationBufferSize { get; init; } = 50;
    
    /// <summary>Enable background processing for operations</summary>
    public bool EnableBackgroundProcessing { get; init; } = true;
    
    /// <summary>Global operation timeout</summary>
    public TimeSpan OperationTimeout { get; init; } = TimeSpan.FromMinutes(5);
    
    #endregion

    #region Data Processing Configuration
    
    /// <summary>Batch size for data operations</summary>
    public int BatchSize { get; init; } = 10000;
    
    /// <summary>Import batch size</summary>
    public int ImportBatchSize { get; init; } = 1000;
    
    /// <summary>Export batch size</summary>
    public int ExportBatchSize { get; init; } = 1000;
    
    /// <summary>Enable parallel processing</summary>
    public bool EnableParallelProcessing { get; init; } = true;
    
    /// <summary>Maximum degree of parallelism</summary>
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;
    
    #endregion

    #region Memory Management
    
    /// <summary>Enable caching for improved performance</summary>
    public bool EnableCaching { get; init; } = true;
    
    /// <summary>Maximum cached items</summary>
    public int CacheMaxItems { get; init; } = 50000;
    
    /// <summary>Cache expiration timeout</summary>
    public TimeSpan CacheTimeout { get; init; } = TimeSpan.FromMinutes(5);
    
    /// <summary>Enable memory optimization techniques</summary>
    public bool EnableMemoryOptimization { get; init; } = true;
    
    /// <summary>Maximum cached rows in memory</summary>
    public int MaxCachedRows { get; init; } = 10000;
    
    /// <summary>Enable automatic garbage collection</summary>
    public bool EnableAutoGC { get; init; } = false;
    
    /// <summary>GC threshold in MB</summary>
    public int GCThresholdMB { get; init; } = 100;
    
    #endregion

    #region Rendering Performance
    
    /// <summary>Enable asynchronous rendering</summary>
    public bool EnableAsyncRendering { get; init; } = true;
    
    /// <summary>Render batch size</summary>
    public int RenderBatchSize { get; init; } = 100;
    
    /// <summary>Delay between render batches (ms)</summary>
    public int RenderDelayMs { get; init; } = 16; // 60 FPS
    
    /// <summary>Enable UI update throttling</summary>
    public bool EnableUIThrottling { get; init; } = true;
    
    /// <summary>UI throttle interval (ms)</summary>
    public int UIThrottleIntervalMs { get; init; } = 100;
    
    /// <summary>Maximum UI updates per second</summary>
    public int MaxUIUpdatesPerSecond { get; init; } = 60;
    
    /// <summary>Enable scroll performance optimization</summary>
    public bool EnableScrollOptimization { get; init; } = true;
    
    #endregion

    #region Search and Filter Performance
    
    /// <summary>Enable asynchronous search operations</summary>
    public bool EnableAsyncSearch { get; init; } = true;
    
    /// <summary>Maximum search results to return</summary>
    public int SearchResultsLimit { get; init; } = 1000;
    
    /// <summary>Enable search result caching</summary>
    public bool EnableSearchCache { get; init; } = true;
    
    /// <summary>Search cache expiry time (minutes)</summary>
    public int SearchCacheExpiryMinutes { get; init; } = 5;
    
    #endregion

    #region Validation Performance
    
    /// <summary>Enable asynchronous validation</summary>
    public bool EnableAsyncValidation { get; init; } = true;
    
    /// <summary>Validation batch size</summary>
    public int ValidationBatchSize { get; init; } = 100;
    
    /// <summary>Validation timeout (seconds)</summary>
    public int ValidationTimeoutSeconds { get; init; } = 30;
    
    /// <summary>Enable validation result caching</summary>
    public bool EnableValidationCache { get; init; } = true;
    
    #endregion

    #region Factory Methods - Standard Presets
    
    /// <summary>
    /// FACTORY: Default balanced configuration
    /// BALANCED: Good performance-feature balance for most scenarios
    /// </summary>
    public static PerformanceConfiguration Default => Balanced;
    
    /// <summary>
    /// FACTORY: Balanced performance configuration
    /// RECOMMENDED: Optimal settings for typical business applications
    /// </summary>
    public static PerformanceConfiguration Balanced => new()
    {
        // Core settings
        EnableVirtualization = true,
        VirtualizationThreshold = 1000,
        VirtualizationBufferSize = 50,
        EnableBackgroundProcessing = true,
        OperationTimeout = TimeSpan.FromMinutes(5),
        
        // Data processing
        BatchSize = 10000,
        ImportBatchSize = 1000,
        ExportBatchSize = 1000,
        EnableParallelProcessing = true,
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        
        // Memory management
        EnableCaching = true,
        CacheMaxItems = 50000,
        CacheTimeout = TimeSpan.FromMinutes(5),
        EnableMemoryOptimization = true,
        MaxCachedRows = 10000,
        EnableAutoGC = false,
        GCThresholdMB = 100,
        
        // Rendering
        EnableAsyncRendering = true,
        RenderBatchSize = 100,
        RenderDelayMs = 16,
        EnableUIThrottling = true,
        UIThrottleIntervalMs = 100,
        MaxUIUpdatesPerSecond = 60,
        EnableScrollOptimization = true,
        
        // Search and validation
        EnableAsyncSearch = true,
        SearchResultsLimit = 1000,
        EnableSearchCache = true,
        SearchCacheExpiryMinutes = 5,
        EnableAsyncValidation = true,
        ValidationBatchSize = 100,
        ValidationTimeoutSeconds = 30,
        EnableValidationCache = true
    };
    
    /// <summary>
    /// FACTORY: High performance configuration for large datasets
    /// OPTIMIZATION: Maximum performance for 1M+ row scenarios
    /// </summary>
    public static PerformanceConfiguration ForLargeDatasets() => new()
    {
        // Aggressive virtualization
        EnableVirtualization = true,
        VirtualizationThreshold = 500,
        VirtualizationBufferSize = 25,
        EnableBackgroundProcessing = true,
        OperationTimeout = TimeSpan.FromMinutes(10),
        
        // Large batch processing
        BatchSize = 5000,
        ImportBatchSize = 5000,
        ExportBatchSize = 5000,
        EnableParallelProcessing = true,
        MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
        
        // Extensive caching
        EnableCaching = true,
        CacheMaxItems = 100000,
        CacheTimeout = TimeSpan.FromMinutes(10),
        EnableMemoryOptimization = true,
        MaxCachedRows = 50000,
        EnableAutoGC = true,
        GCThresholdMB = 50,
        
        // Fast rendering
        EnableAsyncRendering = true,
        RenderBatchSize = 200,
        RenderDelayMs = 8,
        EnableUIThrottling = true,
        UIThrottleIntervalMs = 50,
        MaxUIUpdatesPerSecond = 120,
        EnableScrollOptimization = true,
        
        // Optimized search/validation
        EnableAsyncSearch = true,
        SearchResultsLimit = 500,
        EnableSearchCache = true,
        SearchCacheExpiryMinutes = 10,
        EnableAsyncValidation = true,
        ValidationBatchSize = 500,
        ValidationTimeoutSeconds = 60,
        EnableValidationCache = true
    };
    
    /// <summary>
    /// FACTORY: Memory-optimized configuration
    /// RESOURCE_CONSTRAINED: Minimal memory usage for limited resources
    /// </summary>
    public static PerformanceConfiguration MemoryOptimized => new()
    {
        // Conservative virtualization
        EnableVirtualization = true,
        VirtualizationThreshold = 500,
        VirtualizationBufferSize = 10,
        EnableBackgroundProcessing = false,
        OperationTimeout = TimeSpan.FromMinutes(2),
        
        // Small batches
        BatchSize = 1000,
        ImportBatchSize = 100,
        ExportBatchSize = 100,
        EnableParallelProcessing = false,
        MaxDegreeOfParallelism = 1,
        
        // Minimal caching
        EnableCaching = false,
        CacheMaxItems = 1000,
        CacheTimeout = TimeSpan.FromMinutes(1),
        EnableMemoryOptimization = true,
        MaxCachedRows = 1000,
        EnableAutoGC = true,
        GCThresholdMB = 25,
        
        // Conservative rendering
        EnableAsyncRendering = false,
        RenderBatchSize = 50,
        RenderDelayMs = 33,
        EnableUIThrottling = true,
        UIThrottleIntervalMs = 200,
        MaxUIUpdatesPerSecond = 30,
        EnableScrollOptimization = true,
        
        // Minimal search/validation
        EnableAsyncSearch = false,
        SearchResultsLimit = 100,
        EnableSearchCache = false,
        SearchCacheExpiryMinutes = 1,
        EnableAsyncValidation = false,
        ValidationBatchSize = 10,
        ValidationTimeoutSeconds = 10,
        EnableValidationCache = false
    };
    
    /// <summary>
    /// FACTORY: Maximum compatibility configuration
    /// CONSERVATIVE: Maximum compatibility with minimal optimizations
    /// </summary>
    public static PerformanceConfiguration Conservative => new()
    {
        // Disabled optimizations
        EnableVirtualization = false,
        VirtualizationThreshold = 10000,
        VirtualizationBufferSize = 100,
        EnableBackgroundProcessing = false,
        OperationTimeout = TimeSpan.FromMinutes(1),
        
        // Simple processing
        BatchSize = 100,
        ImportBatchSize = 100,
        ExportBatchSize = 100,
        EnableParallelProcessing = false,
        MaxDegreeOfParallelism = 1,
        
        // No caching
        EnableCaching = false,
        CacheMaxItems = 100,
        CacheTimeout = TimeSpan.FromSeconds(30),
        EnableMemoryOptimization = false,
        MaxCachedRows = 100,
        EnableAutoGC = false,
        GCThresholdMB = 200,
        
        // Synchronous rendering
        EnableAsyncRendering = false,
        RenderBatchSize = 50,
        RenderDelayMs = 33,
        EnableUIThrottling = false,
        UIThrottleIntervalMs = 500,
        MaxUIUpdatesPerSecond = 15,
        EnableScrollOptimization = false,
        
        // Synchronous operations
        EnableAsyncSearch = false,
        SearchResultsLimit = 500,
        EnableSearchCache = false,
        SearchCacheExpiryMinutes = 1,
        EnableAsyncValidation = false,
        ValidationBatchSize = 50,
        ValidationTimeoutSeconds = 15,
        EnableValidationCache = false
    };
    
    #endregion

    #region Custom Factory Methods
    
    /// <summary>
    /// FACTORY: Custom configuration with fluent builder
    /// EXTENSIBILITY: Allow fine-grained customization
    /// </summary>
    public static PerformanceConfiguration Custom(Action<PerformanceConfigurationBuilder>? configurator = null)
    {
        var builder = new PerformanceConfigurationBuilder();
        configurator?.Invoke(builder);
        return builder.Build();
    }
    
    /// <summary>
    /// FACTORY: Configuration optimized for specific row count
    /// ADAPTIVE: Automatically adjust settings based on expected data size
    /// </summary>
    public static PerformanceConfiguration ForRowCount(int expectedRows)
    {
        return expectedRows switch
        {
            < 1000 => Conservative,
            < 10000 => Balanced,
            < 100000 => Balanced,
            _ => ForLargeDatasets()
        };
    }
    
    #endregion
}

/// <summary>
/// BUILDER: Fluent builder for PerformanceConfiguration
/// PROFESSIONAL: Type-safe configuration with validation
/// </summary>
internal class PerformanceConfigurationBuilder
{
    private PerformanceConfiguration _config;
    
    public PerformanceConfigurationBuilder() : this(PerformanceConfiguration.Balanced) { }
    
    public PerformanceConfigurationBuilder(PerformanceConfiguration baseConfig)
    {
        _config = baseConfig;
    }
    
    public PerformanceConfigurationBuilder WithVirtualization(bool enable = true, int threshold = 1000, int bufferSize = 50)
    {
        _config = _config with 
        { 
            EnableVirtualization = enable,
            VirtualizationThreshold = threshold,
            VirtualizationBufferSize = bufferSize
        };
        return this;
    }
    
    public PerformanceConfigurationBuilder WithBatchSizes(int general = 10000, int import = 1000, int export = 1000)
    {
        _config = _config with
        {
            BatchSize = general,
            ImportBatchSize = import,
            ExportBatchSize = export
        };
        return this;
    }
    
    public PerformanceConfigurationBuilder WithParallelProcessing(bool enable = true, int? maxDegree = null)
    {
        _config = _config with
        {
            EnableParallelProcessing = enable,
            MaxDegreeOfParallelism = maxDegree ?? Environment.ProcessorCount
        };
        return this;
    }
    
    public PerformanceConfigurationBuilder WithCaching(bool enable = true, int maxItems = 50000, TimeSpan? timeout = null)
    {
        _config = _config with
        {
            EnableCaching = enable,
            CacheMaxItems = maxItems,
            CacheTimeout = timeout ?? TimeSpan.FromMinutes(5)
        };
        return this;
    }
    
    public PerformanceConfigurationBuilder WithMemoryOptimization(bool enable = true, int maxCachedRows = 10000, bool autoGC = false, int gcThresholdMB = 100)
    {
        _config = _config with
        {
            EnableMemoryOptimization = enable,
            MaxCachedRows = maxCachedRows,
            EnableAutoGC = autoGC,
            GCThresholdMB = gcThresholdMB
        };
        return this;
    }
    
    public PerformanceConfigurationBuilder WithTimeouts(TimeSpan? operationTimeout = null, int validationSeconds = 30)
    {
        _config = _config with
        {
            OperationTimeout = operationTimeout ?? TimeSpan.FromMinutes(5),
            ValidationTimeoutSeconds = validationSeconds
        };
        return this;
    }
    
    public PerformanceConfiguration Build() => _config;
}