using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;

/// <summary>
/// DOCUMENTATION: General DataGrid options per specification
/// CLEAN API: Unified configuration object for DataGrid initialization
/// INTELLISENSE: All options with professional defaults
/// </summary>
internal class GeneralOptions
{
    #region DOCUMENTATION: Basic Settings

    /// <summary>Minimum number of rows to maintain</summary>
    public int MinimumRows { get; set; } = 10;

    /// <summary>Enable UI components (false for headless mode)</summary>
    public bool EnableUI { get; set; } = true;

    /// <summary>Auto-save changes</summary>
    public bool AutoSave { get; set; } = false;

    /// <summary>Enable audit logging</summary>
    public bool EnableAuditLog { get; set; } = true;

    /// <summary>Enable performance monitoring</summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    #endregion

    #region DOCUMENTATION: Configuration Objects

    /// <summary>Color configuration (optional)</summary>
    public ColorConfiguration? Colors { get; set; }

    /// <summary>Validation configuration (optional)</summary>
    public ValidationConfiguration? Validation { get; set; }

    /// <summary>Performance configuration (optional)</summary>
    public PerformanceConfiguration? Performance { get; set; }

    #endregion

    #region DOCUMENTATION: Factory Methods

    /// <summary>
    /// FACTORY: Default configuration per documentation
    /// USAGE: Standard settings for most use cases
    /// </summary>
    public static GeneralOptions Default => new()
    {
        MinimumRows = 10,
        EnableUI = true,
        AutoSave = false,
        Colors = ColorConfiguration.Light,
        Validation = ValidationConfiguration.Relaxed,
        Performance = PerformanceConfiguration.Balanced,
        EnableAuditLog = true,
        EnablePerformanceMonitoring = true
    };

    /// <summary>
    /// FACTORY: Headless mode configuration per documentation
    /// USAGE: For data processing without UI
    /// </summary>
    public static GeneralOptions Headless => new()
    {
        MinimumRows = 0,
        EnableUI = false,
        AutoSave = false,
        Validation = ValidationConfiguration.Strict,
        Performance = PerformanceConfiguration.ForLargeDatasets(),
        EnableAuditLog = true,
        EnablePerformanceMonitoring = false
    };

    /// <summary>
    /// FACTORY: High performance configuration per documentation
    /// USAGE: Optimized for large datasets (1M+ rows)
    /// </summary>
    public static GeneralOptions HighPerformance => new()
    {
        MinimumRows = 1,
        EnableUI = true,
        AutoSave = false,
        Colors = ColorConfiguration.Light,
        Validation = ValidationConfiguration.Fast,
        Performance = PerformanceConfiguration.ForLargeDatasets(),
        EnableAuditLog = false,
        EnablePerformanceMonitoring = true
    };

    /// <summary>
    /// FACTORY: Development/testing configuration per documentation
    /// USAGE: Enhanced logging and validation for development
    /// </summary>
    public static GeneralOptions Development => new()
    {
        MinimumRows = 5,
        EnableUI = true,
        AutoSave = true,
        Colors = ColorConfiguration.Light,
        Validation = ValidationConfiguration.Comprehensive,
        Performance = PerformanceConfiguration.Balanced,
        EnableAuditLog = true,
        EnablePerformanceMonitoring = true
    };

    #endregion
}