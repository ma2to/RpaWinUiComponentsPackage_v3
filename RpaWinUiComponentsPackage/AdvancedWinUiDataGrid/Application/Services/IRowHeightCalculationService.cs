using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

/// <summary>
/// ENTERPRISE: Row height calculation service interface
/// RESPONSIBILITY: Calculate optimal row heights based on text content
/// DDD: Domain service for UI layout calculations
/// PERFORMANCE: Efficient height calculation with caching
/// </summary>
public interface IRowHeightCalculationService : IDisposable
{
    /// <summary>
    /// Calculate required height for a specific row based on content and column configurations
    /// PERFORMANCE: Cached calculations for identical content
    /// </summary>
    Task<Result<double>> CalculateRowHeightAsync(
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns,
        UIConfiguration uiConfig,
        double availableWidth);

    /// <summary>
    /// Calculate heights for multiple rows in batch
    /// PERFORMANCE: Optimized batch processing
    /// </summary>
    Task<Result<Dictionary<int, double>>> CalculateRowHeightsBatchAsync(
        IReadOnlyList<Dictionary<string, object?>> rowsData,
        IReadOnlyList<ColumnDefinition> columns,
        UIConfiguration uiConfig,
        double availableWidth);

    /// <summary>
    /// Calculate height for specific cell content
    /// CORE: Single cell height calculation
    /// </summary>
    Task<Result<double>> CalculateCellHeightAsync(
        object? content,
        ColumnDefinition column,
        UIConfiguration uiConfig,
        double columnWidth);

    /// <summary>
    /// Get estimated height based on text length
    /// PERFORMANCE: Fast estimation without full text measurement
    /// </summary>
    Result<double> EstimateHeightFromTextLength(
        string text,
        ColumnDefinition column,
        UIConfiguration uiConfig,
        double columnWidth);

    /// <summary>
    /// Clear height calculation cache
    /// MEMORY: Cache management for optimal performance
    /// </summary>
    Task<Result<bool>> ClearCacheAsync();

    /// <summary>
    /// Get calculation statistics for performance monitoring
    /// MONITORING: Performance metrics and cache hit rates
    /// </summary>
    Task<Result<HeightCalculationStatistics>> GetStatisticsAsync();
}

/// <summary>
/// Statistics for height calculation performance monitoring
/// </summary>
public record HeightCalculationStatistics
{
    public int TotalCalculations { get; init; }
    public int CacheHits { get; init; }
    public int CacheMisses { get; init; }
    public double CacheHitRate => TotalCalculations > 0 ? (double)CacheHits / TotalCalculations * 100 : 0;
    public TimeSpan AverageCalculationTime { get; init; }
    public int CachedItemsCount { get; init; }
    public long MemoryUsageBytes { get; init; }
}