using System.ComponentModel;
using Windows.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;

// TYPE ALIASES: Map advanced types to standard names for compatibility

/// <summary>
/// Advanced performance configuration for large datasets (1M+ rows)
/// FUNCTIONAL: Immutable performance settings
/// </summary>
internal record PerformanceConfiguration(
    int UIUpdateIntervalMs = 16,          // ~60 FPS
    int ValidationUpdateIntervalMs = 100,  // Real-time validation throttling
    int SearchUpdateIntervalMs = 300,      // Search debouncing
    int BulkOperationBatchSize = 1000,     // Batch size for bulk operations
    int VirtualizationBufferSize = 50,     // UI virtualization buffer
    int UIOperationTimeoutMs = 5000,       // 5 seconds
    int DataOperationTimeoutMs = 60000,    // 1 minute
    int ImportExportTimeoutMs = 300000,    // 5 minutes
    int MaxRowsForRealtimeValidation = 1000, // Above this use batch validation
    int MemoryCleanupIntervalMs = 30000,   // 30 seconds
    int CacheCleanupIntervalMs = 60000,    // 1 minute
    bool EnableAggressiveMemoryManagement = false,
    bool EnableMultiLevelCaching = true,
    bool EnableBackgroundProcessing = true)
{
    /// <summary>
    /// Default balanced performance configuration
    /// </summary>
    public static PerformanceConfiguration Default => new();
    
    /// <summary>
    /// High performance for small datasets (&lt;10K rows)
    /// </summary>
    public static PerformanceConfiguration HighPerformance => new()
    {
        UIUpdateIntervalMs = 8, // ~120 FPS
        ValidationUpdateIntervalMs = 50,
        SearchUpdateIntervalMs = 150,
        BulkOperationBatchSize = 500,
        VirtualizationBufferSize = 100,
        MaxRowsForRealtimeValidation = 2000,
        EnableAggressiveMemoryManagement = false
    };
    
    /// <summary>
    /// Optimized for large datasets (100K+ rows)
    /// </summary>
    public static PerformanceConfiguration LargeDataset => new()
    {
        UIUpdateIntervalMs = 20, // ~50 FPS
        ValidationUpdateIntervalMs = 200,
        SearchUpdateIntervalMs = 400,
        BulkOperationBatchSize = 5000,
        VirtualizationBufferSize = 40,
        MaxRowsForRealtimeValidation = 0, // Disable real-time validation
        DataOperationTimeoutMs = 120000, // 2 minutes
        ImportExportTimeoutMs = 600000, // 10 minutes
        EnableAggressiveMemoryManagement = true,
        EnableMultiLevelCaching = true,
        EnableBackgroundProcessing = true
    };
    
    /// <summary>
    /// Ultra performance for 1M+ rows
    /// </summary>
    public static PerformanceConfiguration UltraLarge => new()
    {
        UIUpdateIntervalMs = 33, // ~30 FPS
        ValidationUpdateIntervalMs = 500,
        SearchUpdateIntervalMs = 600,
        BulkOperationBatchSize = 10000,
        VirtualizationBufferSize = 30,
        MaxRowsForRealtimeValidation = 0, // No real-time validation
        DataOperationTimeoutMs = 300000, // 5 minutes
        ImportExportTimeoutMs = 1800000, // 30 minutes
        MemoryCleanupIntervalMs = 15000, // 15 seconds
        EnableAggressiveMemoryManagement = true,
        EnableMultiLevelCaching = true,
        EnableBackgroundProcessing = true
    };
}

/// <summary>
/// Color theme configuration for application-defined styling
/// FUNCTIONAL: Immutable color configuration
/// </summary>
internal record ColorConfiguration(
    Color? CellBackgroundColor = null,
    Color? CellForegroundColor = null,
    Color? CellBorderColor = null,
    Color? SelectionBackgroundColor = null,     // Focus color (light green recommended)
    Color? SelectionForegroundColor = null,
    Color? CopyModeBackgroundColor = null,      // Copy mode (light blue recommended)
    Color? ValidationErrorBorderColor = null,   // Red for validation errors
    Color? ValidationErrorBackgroundColor = null,
    Color? HoverBackgroundColor = null,
    Color? FocusRingColor = null,              // Light green for focus ring
    Color? EvenRowBackgroundColor = null,       // Zebra pattern - even rows
    Color? OddRowBackgroundColor = null,        // Zebra pattern - odd rows  
    Color? HeaderBackgroundColor = null,
    Color? HeaderForegroundColor = null,
    Color? HeaderBorderColor = null,           // Black borders recommended
    bool EnableZebraPattern = true)
{
    /// <summary>
    /// Default Windows 11 style with user-specified colors
    /// Focus: light green, Copy: light blue, Borders: black, Validation: red
    /// </summary>
    public static ColorConfiguration Default => new()
    {
        CellBackgroundColor = Microsoft.UI.Colors.White,
        CellForegroundColor = Microsoft.UI.Colors.Black,
        CellBorderColor = Microsoft.UI.Colors.Black,
        SelectionBackgroundColor = Color.FromArgb(100, 144, 238, 144), // Light green focus
        SelectionForegroundColor = Microsoft.UI.Colors.Black,
        CopyModeBackgroundColor = Color.FromArgb(100, 173, 216, 230), // Light blue copy
        ValidationErrorBorderColor = Microsoft.UI.Colors.Red,
        ValidationErrorBackgroundColor = Color.FromArgb(50, 255, 0, 0),
        HoverBackgroundColor = Color.FromArgb(50, 0, 0, 0),
        FocusRingColor = Color.FromArgb(255, 144, 238, 144), // Light green focus ring
        EvenRowBackgroundColor = Microsoft.UI.Colors.White,
        OddRowBackgroundColor = Color.FromArgb(255, 249, 249, 249),
        HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
        HeaderForegroundColor = Microsoft.UI.Colors.Black,
        HeaderBorderColor = Microsoft.UI.Colors.Black,
        EnableZebraPattern = true
    };
    
    /// <summary>
    /// Dark theme variant
    /// </summary>
    public static ColorConfiguration Dark => new()
    {
        CellBackgroundColor = Color.FromArgb(255, 32, 32, 32),
        CellForegroundColor = Microsoft.UI.Colors.White,
        CellBorderColor = Color.FromArgb(255, 60, 60, 60),
        SelectionBackgroundColor = Color.FromArgb(100, 0, 120, 215),
        SelectionForegroundColor = Microsoft.UI.Colors.White,
        CopyModeBackgroundColor = Color.FromArgb(100, 70, 130, 180),
        ValidationErrorBorderColor = Microsoft.UI.Colors.Red,
        ValidationErrorBackgroundColor = Color.FromArgb(50, 255, 0, 0),
        HoverBackgroundColor = Color.FromArgb(50, 255, 255, 255),
        FocusRingColor = Color.FromArgb(255, 0, 120, 215),
        EvenRowBackgroundColor = Color.FromArgb(255, 32, 32, 32),
        OddRowBackgroundColor = Color.FromArgb(255, 28, 28, 28),
        HeaderBackgroundColor = Color.FromArgb(255, 40, 40, 40),
        HeaderForegroundColor = Microsoft.UI.Colors.White,
        HeaderBorderColor = Color.FromArgb(255, 60, 60, 60),
        EnableZebraPattern = true
    };
}


/// <summary>
/// Enhanced DataGrid configuration with all advanced features
/// FUNCTIONAL: Immutable complete configuration
/// COMPREHENSIVE: Complete configuration for modern DataGrid usage
/// </summary>
internal record DataGridConfiguration(
    bool EnableValidation = true,
    bool EnableVirtualization = true,
    int BatchSize = 1000,
    TimeSpan ThrottleDelay = default,
    bool CacheEnabled = true,
    PerformanceConfiguration? PerformanceConfig = null,
    ColorConfiguration? ColorConfig = null,
    IValidationConfiguration? ValidationConfig = null,
    
    // UI Constraints (from backup API)
    double? MinWidth = null,
    double? MinHeight = null,
    double? MaxWidth = null,
    double? MaxHeight = null,
    
    // Feature Flags (from backup API)
    bool EnableSort = false,
    bool EnableSearch = false,
    bool EnableFilter = false,
    
    // Empty Rows (from backup API)
    int EmptyRowsCount = 15,
    
    // Search Configuration (from backup API)
    int MaxSearchHistoryItems = 0,
    
    // Advanced Validation (from backup API)
    bool EnableBatchValidation = false,
    bool EnableRealtimeValidation = true,
    bool ShowValidationAlerts = true,
    bool StopOnValidationErrors = false)
{
    public static DataGridConfiguration Default => new();
    
    /// <summary>
    /// Full-featured configuration with all capabilities enabled
    /// SMART VALIDATION: Both real-time and batch validation enabled automatically
    /// </summary>
    public static DataGridConfiguration FullFeatured => new(
        EnableValidation: true,
        EnableVirtualization: true,
        BatchSize: 1000,
        ThrottleDelay: TimeSpan.FromMilliseconds(16),
        CacheEnabled: true,
        EnableSort: true,
        EnableSearch: true,
        EnableFilter: true,
        EmptyRowsCount: 15,
        EnableBatchValidation: true,    // For bulk operations (import, paste)
        EnableRealtimeValidation: true, // For cell editing
        ShowValidationAlerts: true,
        MaxSearchHistoryItems: 10);
    
    /// <summary>
    /// UI-focused configuration with size constraints
    /// </summary>
    public static DataGridConfiguration UIFocused => new(
        EnableValidation: true,
        EnableVirtualization: true,
        BatchSize: 500,
        ThrottleDelay: TimeSpan.FromMilliseconds(16),
        CacheEnabled: true,
        EnableSort: true,
        EnableSearch: true,
        EnableFilter: false,
        EmptyRowsCount: 10,
        MinWidth: 300,
        MinHeight: 200,
        EnableRealtimeValidation: true,
        ShowValidationAlerts: true);
    
    public static DataGridConfiguration HighPerformance => new(
        EnableValidation: true,
        EnableVirtualization: true,
        BatchSize: 5000,
        ThrottleDelay: TimeSpan.FromMilliseconds(50),
        CacheEnabled: true,
        PerformanceConfig: PerformanceConfiguration.HighPerformance);
    
    public static DataGridConfiguration LargeDataset => new(
        EnableValidation: true,
        EnableVirtualization: true,
        BatchSize: 10000,
        ThrottleDelay: TimeSpan.FromMilliseconds(100),
        CacheEnabled: true,
        PerformanceConfig: PerformanceConfiguration.LargeDataset);
        
    public static DataGridConfiguration UltraLarge => new(
        EnableValidation: false, // Disable for ultra-large datasets
        EnableVirtualization: true,
        BatchSize: 20000,
        ThrottleDelay: TimeSpan.FromMilliseconds(200),
        CacheEnabled: true,
        PerformanceConfig: PerformanceConfiguration.UltraLarge);
}

/// <summary>
/// Enhanced import options with validation and performance settings
/// FUNCTIONAL: Immutable import configuration
/// </summary>
internal record ImportOptions(
    bool ValidateData = true,
    bool ReplaceExistingData = false,
    bool SkipInvalidRows = false,
    bool OnlyValidRows = false,
    int? StartRowIndex = null,
    int? MaxRows = null,
    IProgress<ImportProgress>? ProgressReporter = null,
    CancellationToken CancellationToken = default,
    bool EnableBatchProcessing = true,
    bool EnableParallelProcessing = false, // For large datasets
    int TimeoutMs = 300000) // 5 minutes default
{
    public static ImportOptions Default => new();
    public static ImportOptions QuickImport => new(ValidateData: false, EnableBatchProcessing: true);
    public static ImportOptions SafeImport => new(ValidateData: true, SkipInvalidRows: true);
    public static ImportOptions ReplaceAll => new(ValidateData: true, ReplaceExistingData: true);
}

/// <summary>
/// Import progress information
/// FUNCTIONAL: Immutable progress reporting
/// </summary>
internal record ImportProgress(
    int TotalRows,
    int ProcessedRows,
    int SuccessfulRows,
    int FailedRows,
    string CurrentOperation,
    TimeSpan ElapsedTime,
    TimeSpan? EstimatedTimeRemaining = null)
{
    public double ProgressPercentage => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100.0 : 0.0;
    public double SuccessRate => ProcessedRows > 0 ? (double)SuccessfulRows / ProcessedRows * 100.0 : 0.0;
}

/// <summary>
/// Enhanced export options
/// FUNCTIONAL: Immutable export configuration
/// </summary>
internal record ExportOptions(
    bool IncludeValidationAlerts = false,
    bool OnlyValidRows = false,
    bool OnlyVisibleRows = false,       // NEW: Export only visible rows
    IReadOnlyList<string>? ColumnsToExport = null, // NEW: Specific columns only
    IReadOnlyList<int>? RowsToExport = null,       // NEW: Specific rows only
    string? DateTimeFormat = null,
    bool IncludeHeaders = true,
    int TimeoutMs = 300000)
{
    public static ExportOptions Default => new();
    public static ExportOptions AllData => new(IncludeValidationAlerts: true);
    public static ExportOptions ValidOnly => new(OnlyValidRows: true);
    public static ExportOptions VisibleOnly => new(OnlyVisibleRows: true);
}