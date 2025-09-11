using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;

/// <summary>
/// ENTERPRISE: High-performance search service interface
/// SCALABILITY: Designed for 1M+ row datasets
/// SOLID: Interface Segregation - search concerns only
/// </summary>
public interface IDataGridSearchService : IDisposable
{
    /// <summary>
    /// PERFORMANCE: Basic search with intelligent indexing
    /// ENTERPRISE: Optimized for large datasets with result limiting
    /// </summary>
    Task<Result<SearchResult>> SearchAsync(
        string searchText,
        string[]? targetColumns = null,
        bool caseSensitive = false,
        int? maxResults = null,
        TimeSpan? timeout = null);

    /// <summary>
    /// ADVANCED: Complex search with regex and scope control
    /// ENTERPRISE: Full-featured search for power users
    /// </summary>
    Task<Result<SearchResult>> AdvancedSearchAsync(
        AdvancedSearchCriteria criteria,
        TimeSpan? timeout = null);

    /// <summary>
    /// PERFORMANCE: Clear search results and reset state
    /// MEMORY: Efficient cleanup for long-running sessions
    /// </summary>
    Task<Result<bool>> ClearSearchAsync();

    /// <summary>
    /// NAVIGATION: Navigate to next/previous search result
    /// UX: Seamless result navigation
    /// </summary>
    Task<Result<SearchNavigationResult>> NavigateSearchResultAsync(SearchNavigationDirection direction);

    /// <summary>
    /// ENTERPRISE: Get search statistics and performance metrics
    /// MONITORING: Search performance analysis
    /// </summary>
    Task<Result<SearchStatistics>> GetSearchStatisticsAsync();
}

/// <summary>
/// ENTERPRISE: Search scope enumeration
/// PERFORMANCE: Optimize search by limiting scope
/// </summary>
public enum SearchScope
{
    AllData,        // Search entire dataset (performance impact)
    VisibleData,    // Search only currently visible rows (fast)
    SelectedData,   // Search only selected rows (fastest)
    FilteredData    // Search current filter results
}

/// <summary>
/// NAVIGATION: Search result navigation direction
/// UX: Intuitive navigation controls
/// </summary>
public enum SearchNavigationDirection
{
    First,
    Previous,
    Next,
    Last
}