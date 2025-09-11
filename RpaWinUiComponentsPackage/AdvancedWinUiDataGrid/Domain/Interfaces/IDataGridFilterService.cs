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
/// ENTERPRISE: High-performance filtering service interface
/// SCALABILITY: Designed for 1M+ row datasets with smart indexing
/// SOLID: Interface Segregation - filtering concerns only
/// </summary>
public interface IDataGridFilterService : IDisposable
{
    /// <summary>
    /// PERFORMANCE: Apply simple column-based filters
    /// ENTERPRISE: Quick filtering for common scenarios
    /// </summary>
    Task<Result<FilterResult>> ApplySimpleFilterAsync(
        string columnName,
        FilterOperator filterOperator,
        object? value,
        bool caseSensitive = false,
        TimeSpan? timeout = null);

    /// <summary>
    /// ADVANCED: Apply complex multi-column filters with grouping
    /// ENTERPRISE: Complex filter expressions (Age > 18 AND (Dept = "IT" OR Salary > 50000))
    /// </summary>
    Task<Result<FilterResult>> ApplyAdvancedFiltersAsync(
        IReadOnlyList<AdvancedFilter> filters,
        FilterCombinationMode mode = FilterCombinationMode.And,
        TimeSpan? timeout = null);

    /// <summary>
    /// PERFORMANCE: Clear all filters and show all data
    /// MEMORY: Efficient cleanup and index reset
    /// </summary>
    Task<Result<bool>> ClearAllFiltersAsync();

    /// <summary>
    /// ENTERPRISE: Remove specific filter by column name
    /// FLEXIBILITY: Granular filter management
    /// </summary>
    Task<Result<bool>> RemoveFilterAsync(string columnName);

    /// <summary>
    /// ENTERPRISE: Get current filter state
    /// INTROSPECTION: Allow external components to query filter state
    /// </summary>
    Task<Result<CurrentFilterState>> GetFilterStateAsync();

    /// <summary>
    /// PERFORMANCE: Get filtering statistics and performance metrics
    /// MONITORING: Filter operation analysis
    /// </summary>
    Task<Result<FilterStatistics>> GetFilterStatisticsAsync();

    /// <summary>
    /// ADVANCED: Create and save custom filter presets
    /// UX: Allow users to save common filter combinations
    /// </summary>
    Task<Result<bool>> SaveFilterPresetAsync(string presetName, IReadOnlyList<AdvancedFilter> filters);

    /// <summary>
    /// CONVENIENCE: Load and apply saved filter preset
    /// UX: Quick access to saved filter combinations
    /// </summary>
    Task<Result<FilterResult>> LoadFilterPresetAsync(string presetName);
}

// Note: FilterOperator, FilterCombinationMode, and FilterLogicOperator are now defined in Domain/ValueObjects/FilterModels.cs