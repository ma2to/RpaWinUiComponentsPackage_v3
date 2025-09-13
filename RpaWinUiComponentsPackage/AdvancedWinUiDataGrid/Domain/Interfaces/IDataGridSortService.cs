using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;

/// <summary>
/// ENTERPRISE: High-performance sorting service interface
/// SCALABILITY: Designed for 1M+ row datasets with virtualization
/// SOLID: Interface Segregation - sorting concerns only
/// </summary>
internal interface IDataGridSortService : IDisposable
{
    /// <summary>
    /// PERFORMANCE: Apply single column sorting with intelligent algorithms
    /// ENTERPRISE: Optimized for large datasets
    /// </summary>
    Task<Result<SortResult>> SortByColumnAsync(
        string columnName,
        SortDirection direction = SortDirection.Ascending,
        bool maintainSelection = true,
        TimeSpan? timeout = null);

    /// <summary>
    /// ADVANCED: Multi-level sorting with priority ordering
    /// ENTERPRISE: Complex sorting scenarios (e.g., Name ASC, Date DESC, Priority ASC)
    /// </summary>
    Task<Result<SortResult>> ApplyMultiLevelSortAsync(
        IReadOnlyList<SortCriteria> sortCriteria,
        bool maintainSelection = true,
        TimeSpan? timeout = null);

    /// <summary>
    /// PERFORMANCE: Clear all sorting and return to natural order
    /// MEMORY: Efficient cleanup and state reset
    /// </summary>
    Task<Result<bool>> ClearSortingAsync();

    /// <summary>
    /// ENTERPRISE: Get current sorting state
    /// INTROSPECTION: Allow external components to query sort state
    /// </summary>
    Task<Result<CurrentSortState>> GetSortStateAsync();

    /// <summary>
    /// PERFORMANCE: Get sorting statistics and performance metrics
    /// MONITORING: Sort operation analysis
    /// </summary>
    Task<Result<SortStatistics>> GetSortStatisticsAsync();
}


