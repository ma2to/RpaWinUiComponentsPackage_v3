using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object for filter definition
/// ENTERPRISE: Comprehensive filtering system supporting complex business rules
/// IMMUTABLE: Record pattern ensuring filter consistency
/// </summary>
internal record FilterDefinition
{
    /// <summary>Name of the column to filter</summary>
    public required string ColumnName { get; init; }
    
    /// <summary>Filter operator determining comparison type</summary>
    public FilterOperator Operator { get; init; } = FilterOperator.Equals;
    
    /// <summary>Value to filter by</summary>
    public object? Value { get; init; }
    
    /// <summary>Logic operator for combining with other filters</summary>
    public FilterLogicOperator LogicOperator { get; init; } = FilterLogicOperator.And;
    
    /// <summary>Case sensitivity for string comparisons</summary>
    public bool CaseSensitive { get; init; } = false;
    
    /// <summary>Filter priority for evaluation order</summary>
    public int Priority { get; init; } = 0;
    
    /// <summary>Whether this filter starts a logical group</summary>
    public bool GroupStart { get; init; } = false;
    
    /// <summary>Whether this filter ends a logical group</summary>
    public bool GroupEnd { get; init; } = false;
    
    /// <summary>Whether this filter is currently active</summary>
    public bool IsEnabled { get; init; } = true;
    
    /// <summary>Custom filter name for complex scenarios</summary>
    public string? FilterName { get; init; }
    
    /// <summary>Additional filter parameters</summary>
    public IReadOnlyDictionary<string, object?>? Parameters { get; init; }
    
    #region Factory Methods
    
    /// <summary>Create an equals filter</summary>
    public static FilterDefinition Equals(string columnName, object? value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.Equals, Value = value, CaseSensitive = caseSensitive };
    
    /// <summary>Create a not equals filter</summary>
    public static FilterDefinition NotEquals(string columnName, object? value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.NotEquals, Value = value, CaseSensitive = caseSensitive };
    
    /// <summary>Create a contains filter</summary>
    public static FilterDefinition Contains(string columnName, string value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.Contains, Value = value, CaseSensitive = caseSensitive };
    
    /// <summary>Create a not contains filter</summary>
    public static FilterDefinition NotContains(string columnName, string value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.NotContains, Value = value, CaseSensitive = caseSensitive };
    
    /// <summary>Create a starts with filter</summary>
    public static FilterDefinition StartsWith(string columnName, string value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.StartsWith, Value = value, CaseSensitive = caseSensitive };
    
    /// <summary>Create an ends with filter</summary>
    public static FilterDefinition EndsWith(string columnName, string value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.EndsWith, Value = value, CaseSensitive = caseSensitive };
    
    /// <summary>Create a greater than filter</summary>
    public static FilterDefinition GreaterThan(string columnName, IComparable value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.GreaterThan, Value = value };
    
    /// <summary>Create a greater than or equal filter</summary>
    public static FilterDefinition GreaterThanOrEqual(string columnName, IComparable value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.GreaterThanOrEqual, Value = value };
    
    /// <summary>Create a less than filter</summary>
    public static FilterDefinition LessThan(string columnName, IComparable value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.LessThan, Value = value };
    
    /// <summary>Create a less than or equal filter</summary>
    public static FilterDefinition LessThanOrEqual(string columnName, IComparable value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.LessThanOrEqual, Value = value };
    
    /// <summary>Create an is null filter</summary>
    public static FilterDefinition IsNull(string columnName) =>
        new() { ColumnName = columnName, Operator = FilterOperator.IsNull, Value = null };
    
    /// <summary>Create an is not null filter</summary>
    public static FilterDefinition IsNotNull(string columnName) =>
        new() { ColumnName = columnName, Operator = FilterOperator.IsNotNull, Value = null };
    
    /// <summary>Create an is empty filter (for strings)</summary>
    public static FilterDefinition IsEmpty(string columnName) =>
        new() { ColumnName = columnName, Operator = FilterOperator.IsEmpty, Value = null };
    
    /// <summary>Create an is not empty filter (for strings)</summary>
    public static FilterDefinition IsNotEmpty(string columnName) =>
        new() { ColumnName = columnName, Operator = FilterOperator.IsNotEmpty, Value = null };
    
    /// <summary>Create an in list filter</summary>
    public static FilterDefinition In(string columnName, IEnumerable<object?> values) =>
        new() { ColumnName = columnName, Operator = FilterOperator.In, Value = values.ToArray() };
    
    /// <summary>Create a not in list filter</summary>
    public static FilterDefinition NotIn(string columnName, IEnumerable<object?> values) =>
        new() { ColumnName = columnName, Operator = FilterOperator.NotIn, Value = values.ToArray() };
    
    /// <summary>Create a between range filter</summary>
    public static FilterDefinition Between(string columnName, IComparable minValue, IComparable maxValue) =>
        new() 
        { 
            ColumnName = columnName, 
            Operator = FilterOperator.Between, 
            Value = new[] { minValue, maxValue } 
        };
    
    /// <summary>Create a regex filter</summary>
    public static FilterDefinition Regex(string columnName, string pattern, bool caseSensitive = false) =>
        new() { ColumnName = columnName, Operator = FilterOperator.Regex, Value = pattern, CaseSensitive = caseSensitive };
    
    #endregion
}

/// <summary>
/// DDD: Value object for advanced filter definition with grouping
/// ENTERPRISE: Complex filtering with nested logical conditions
/// </summary>
internal record AdvancedFilterDefinition : FilterDefinition
{
    /// <summary>Nested child filters for complex logic</summary>
    public IReadOnlyList<FilterDefinition>? ChildFilters { get; init; }
    
    /// <summary>Logic operator for child filters</summary>
    public FilterLogicOperator ChildLogicOperator { get; init; } = FilterLogicOperator.And;
    
    /// <summary>Filter group identifier for complex nesting</summary>
    public string? GroupId { get; init; }
    
    /// <summary>Parent group identifier</summary>
    public string? ParentGroupId { get; init; }
    
    /// <summary>Depth level in filter hierarchy</summary>
    public int GroupLevel { get; init; } = 0;
    
    /// <summary>Custom filter predicate for complex scenarios</summary>
    public Func<Dictionary<string, object?>, bool>? CustomPredicate { get; init; }
}

/// <summary>
/// COMPATIBILITY: Type alias for legacy code compatibility
/// </summary>
internal record AdvancedFilter : AdvancedFilterDefinition
{
    public static AdvancedFilter Create(
        string columnName,
        FilterOperator filterOperator,
        object? value,
        IReadOnlyList<FilterDefinition>? childFilters = null) => new()
        {
            ColumnName = columnName,
            Operator = filterOperator,
            Value = value,
            ChildFilters = childFilters
        };
}

/// <summary>
/// DDD: Value object for filter operation result
/// ENTERPRISE: Comprehensive filtering result with metadata
/// </summary>
internal record FilterResult
{
    public IReadOnlyList<int> FilteredRowIndices { get; init; } = [];
    public int TotalRows { get; init; }
    public int FilteredRows => FilteredRowIndices.Count;
    public IReadOnlyList<FilterDefinition> AppliedFilters { get; init; } = [];
    public bool HasResults => FilteredRows > 0;
    public TimeSpan? ProcessingTime { get; init; }
    public FilterStatistics? Statistics { get; init; }
    
    // Compatibility properties
    public int MatchingRows => FilteredRows;
    public double FilterEfficiency { get; init; } = 100.0;
    public TimeSpan FilterDuration => ProcessingTime ?? TimeSpan.Zero;
    
    public static FilterResult Create(
        IReadOnlyList<int> filteredIndices, 
        int totalRows,
        IReadOnlyList<FilterDefinition> appliedFilters,
        TimeSpan? processingTime = null) =>
        new()
        {
            FilteredRowIndices = filteredIndices,
            TotalRows = totalRows,
            AppliedFilters = appliedFilters,
            ProcessingTime = processingTime
        };

    public static FilterResult CreateSuccess(
        IReadOnlyList<GridRow> filteredRows,
        int originalRowCount,
        int filteredRowCount,
        TimeSpan duration,
        int filtersApplied,
        string message) =>
        new()
        {
            FilteredRowIndices = filteredRows.Select((_, index) => index).ToArray(),
            TotalRows = originalRowCount,
            ProcessingTime = duration,
            FilterEfficiency = originalRowCount > 0 ? (double)filteredRowCount / originalRowCount * 100 : 100
        };
        
    public static FilterResult Empty(int totalRows) =>
        new() { TotalRows = totalRows };
}

/// <summary>
/// DDD: Value object for current filter state
/// ENTERPRISE: Maintains active filter state for grid
/// </summary>
internal record CurrentFilterState
{
    public IReadOnlyList<FilterDefinition> ActiveFilters { get; init; } = [];
    public FilterLogicOperator GlobalLogicOperator { get; init; } = FilterLogicOperator.And;
    public bool HasActiveFilters => ActiveFilters.Count > 0;
    public int EnabledFiltersCount => ActiveFilters.Count(f => f.IsEnabled);
    public DateTime LastApplied { get; init; } = DateTime.UtcNow;
    public FilterResult? LastResult { get; init; }
    
    // Compatibility properties
    public bool IsFiltered => HasActiveFilters;
    public int VisibleRows { get; init; } = 0;
    
    public static CurrentFilterState Empty => new();
    
    public static CurrentFilterState WithFilters(IReadOnlyList<FilterDefinition> filters, FilterLogicOperator logicOperator = FilterLogicOperator.And) =>
        new() 
        { 
            ActiveFilters = filters, 
            GlobalLogicOperator = logicOperator,
            LastApplied = DateTime.UtcNow 
        };
}

/// <summary>
/// DDD: Value object for filter statistics and performance metrics
/// </summary>
internal record FilterStatistics
{
    public int TotalFilterOperations { get; init; }
    public TimeSpan AverageProcessingTime { get; init; }
    public TimeSpan LastProcessingTime { get; init; }
    public Dictionary<FilterOperator, int> OperatorUsageCounts { get; init; } = new();
    public Dictionary<string, int> ColumnFilterCounts { get; init; } = new();
    public int RowsFilteredTotal { get; init; }
    
    // Compatibility properties
    public int TotalFilters => TotalFilterOperations;
    public TimeSpan AverageFilterTime => AverageProcessingTime;
    public double AverageFilterEfficiency { get; init; } = 100.0; // Default to 100% efficiency
    
    public static FilterStatistics Default => new();
}

/// <summary>
/// ENTERPRISE: Filter operators for different comparison types
/// COMPREHENSIVE: All common filtering operations
/// </summary>
internal enum FilterOperator
{
    // Basic comparisons
    Equals,
    NotEquals,
    
    // String operations
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    
    // Numeric/Date comparisons
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    
    // Null checks
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty,
    
    // Collection operations
    In,
    NotIn,
    Between,
    
    // Advanced operations
    Regex,
    CustomPredicate
}

/// <summary>
/// ENTERPRISE: Logic operators for combining filters
/// BOOLEAN_LOGIC: Standard boolean operations with negation support
/// </summary>
internal enum FilterLogicOperator
{
    /// <summary>All conditions must be true (default)</summary>
    And,
    
    /// <summary>At least one condition must be true</summary>
    Or,
    
    /// <summary>All conditions must be true AND the specified condition false</summary>
    AndNot,
    
    /// <summary>At least one condition true OR the specified condition false</summary>
    OrNot,
    
    /// <summary>Exclusive OR - exactly one condition must be true</summary>
    Xor,
    
    /// <summary>None of the conditions should be true</summary>
    Nor,
    
    /// <summary>All conditions must be false</summary>
    Nand
}

/// <summary>
/// ENTERPRISE: Filter combination mode (simplified version of FilterLogicOperator)
/// COMPATIBILITY: Simplified interface for basic filtering scenarios
/// </summary>
internal enum FilterCombinationMode
{
    /// <summary>All filters must match (AND logic)</summary>
    And,
    
    /// <summary>Any filter can match (OR logic)</summary>
    Or
}

/// <summary>
/// ENTERPRISE: Filter builder for fluent filter creation
/// BUILDER_PATTERN: Type-safe filter construction
/// </summary>
internal class FilterBuilder
{
    private readonly List<FilterDefinition> _filters = new();
    private FilterLogicOperator _defaultLogicOperator = FilterLogicOperator.And;
    private int _currentPriority = 0;
    
    public FilterBuilder WithDefaultLogicOperator(FilterLogicOperator logicOperator)
    {
        _defaultLogicOperator = logicOperator;
        return this;
    }
    
    public FilterBuilder Add(FilterDefinition filter)
    {
        var filterWithDefaults = filter with 
        { 
            LogicOperator = filter.LogicOperator == FilterLogicOperator.And && _defaultLogicOperator != FilterLogicOperator.And 
                ? _defaultLogicOperator 
                : filter.LogicOperator,
            Priority = filter.Priority == 0 ? _currentPriority++ : filter.Priority
        };
        _filters.Add(filterWithDefaults);
        return this;
    }
    
    public FilterBuilder Equals(string columnName, object? value, bool caseSensitive = false)
    {
        return Add(FilterDefinition.Equals(columnName, value, caseSensitive));
    }
    
    public FilterBuilder Contains(string columnName, string value, bool caseSensitive = false)
    {
        return Add(FilterDefinition.Contains(columnName, value, caseSensitive));
    }
    
    public FilterBuilder GreaterThan(string columnName, IComparable value)
    {
        return Add(FilterDefinition.GreaterThan(columnName, value));
    }
    
    public FilterBuilder LessThan(string columnName, IComparable value)
    {
        return Add(FilterDefinition.LessThan(columnName, value));
    }
    
    public FilterBuilder Between(string columnName, IComparable minValue, IComparable maxValue)
    {
        return Add(FilterDefinition.Between(columnName, minValue, maxValue));
    }
    
    public FilterBuilder IsNull(string columnName)
    {
        return Add(FilterDefinition.IsNull(columnName));
    }
    
    public FilterBuilder IsNotNull(string columnName)
    {
        return Add(FilterDefinition.IsNotNull(columnName));
    }
    
    public FilterBuilder In(string columnName, params object?[] values)
    {
        return Add(FilterDefinition.In(columnName, values));
    }
    
    public FilterBuilder Regex(string columnName, string pattern, bool caseSensitive = false)
    {
        return Add(FilterDefinition.Regex(columnName, pattern, caseSensitive));
    }
    
    public FilterBuilder BeginGroup()
    {
        if (_filters.Count > 0)
        {
            var lastFilter = _filters[^1];
            _filters[^1] = lastFilter with { GroupStart = true };
        }
        return this;
    }
    
    public FilterBuilder EndGroup()
    {
        if (_filters.Count > 0)
        {
            var lastFilter = _filters[^1];
            _filters[^1] = lastFilter with { GroupEnd = true };
        }
        return this;
    }
    
    public FilterBuilder WithLogicOperator(FilterLogicOperator logicOperator)
    {
        if (_filters.Count > 0)
        {
            var lastFilter = _filters[^1];
            _filters[^1] = lastFilter with { LogicOperator = logicOperator };
        }
        return this;
    }
    
    public IReadOnlyList<FilterDefinition> Build()
    {
        return _filters.AsReadOnly();
    }
    
    public static FilterBuilder Create() => new();
}

/// <summary>
/// ENTERPRISE: Search options configuration
/// COMPATIBILITY: Type alias for compatibility with main API
/// </summary>
internal record SearchOptions
{
    /// <summary>Columns to search in (null = all columns)</summary>
    public IReadOnlyList<string>? ColumnNames { get; init; }
    
    /// <summary>Case sensitive search</summary>
    public bool CaseSensitive { get; init; } = false;
    
    /// <summary>Search whole words only</summary>
    public bool WholeWordOnly { get; init; } = false;
    
    /// <summary>Use regular expressions</summary>
    public bool UseRegex { get; init; } = false;
    
    /// <summary>Search type</summary>
    public SearchType SearchType { get; init; } = SearchType.Contains;
    
    /// <summary>Maximum number of results to return (0 = unlimited)</summary>
    public int MaxResults { get; init; } = 0;
    
    /// <summary>Start searching from this row index</summary>
    public int StartIndex { get; init; } = 0;
    
    /// <summary>Search direction</summary>
    public SearchDirection Direction { get; init; } = SearchDirection.Forward;
    
    /// <summary>Default search options</summary>
    public static SearchOptions Default => new();
    
    /// <summary>Case sensitive search options</summary>
    public static SearchOptions CaseSensitiveSearch => new() { CaseSensitive = true };
    
    /// <summary>Regex search options</summary>
    public static SearchOptions Regex => new() { UseRegex = true };
    
    /// <summary>Whole word search options</summary>
    public static SearchOptions WholeWords => new() { WholeWordOnly = true };
}

/// <summary>
/// ENTERPRISE: Search types available
/// COMPATIBILITY: Supporting both simple and advanced search patterns
/// </summary>
internal enum SearchType
{
    /// <summary>Contains the search term anywhere</summary>
    Contains,
    
    /// <summary>Starts with the search term</summary>
    StartsWith,
    
    /// <summary>Ends with the search term</summary>
    EndsWith,
    
    /// <summary>Exact match only</summary>
    Exact,
    
    /// <summary>Regular expression pattern</summary>
    Regex
}

/// <summary>
/// ENTERPRISE: Search direction
/// </summary>
internal enum SearchDirection
{
    /// <summary>Search from top to bottom</summary>
    Forward,
    
    /// <summary>Search from bottom to top</summary>
    Backward
}

/// <summary>
/// ENTERPRISE: Filter expression compatibility type
/// COMPATIBILITY: Type alias for FilterDefinition to maintain API compatibility
/// </summary>
internal record FilterExpression
{
    /// <summary>Name of the column to filter</summary>
    public required string ColumnName { get; init; }
    
    /// <summary>Filter operator determining comparison type</summary>
    public FilterOperator Operator { get; init; } = FilterOperator.Equals;
    
    /// <summary>Value to filter by</summary>
    public object? Value { get; init; }
    
    /// <summary>Logic operator for combining with other filters</summary>
    public FilterLogicOperator LogicOperator { get; init; } = FilterLogicOperator.And;
    
    /// <summary>Case sensitivity for string comparisons</summary>
    public bool CaseSensitive { get; init; } = false;
    
    /// <summary>Filter priority for evaluation order</summary>
    public int Priority { get; init; } = 0;
    
    /// <summary>Whether this filter starts a logical group</summary>
    public bool GroupStart { get; init; } = false;
    
    /// <summary>Whether this filter ends a logical group</summary>
    public bool GroupEnd { get; init; } = false;
    
    /// <summary>Whether this filter is currently active</summary>
    public bool IsEnabled { get; init; } = true;
    
    /// <summary>Custom filter name for complex scenarios</summary>
    public string? FilterName { get; init; }
    
    /// <summary>Additional filter parameters</summary>
    public IReadOnlyDictionary<string, object?>? Parameters { get; init; }
    
    /// <summary>Convert to FilterDefinition</summary>
    public FilterDefinition ToFilterDefinition() => new()
    {
        ColumnName = ColumnName,
        Operator = Operator,
        Value = Value,
        LogicOperator = LogicOperator,
        CaseSensitive = CaseSensitive,
        Priority = Priority,
        GroupStart = GroupStart,
        GroupEnd = GroupEnd,
        IsEnabled = IsEnabled,
        FilterName = FilterName,
        Parameters = Parameters
    };
    
    /// <summary>Create from FilterDefinition</summary>
    public static FilterExpression FromFilterDefinition(FilterDefinition definition) => new()
    {
        ColumnName = definition.ColumnName,
        Operator = definition.Operator,
        Value = definition.Value,
        LogicOperator = definition.LogicOperator,
        CaseSensitive = definition.CaseSensitive,
        Priority = definition.Priority,
        GroupStart = definition.GroupStart,
        GroupEnd = definition.GroupEnd,
        IsEnabled = definition.IsEnabled,
        FilterName = definition.FilterName,
        Parameters = definition.Parameters
    };
}

/// <summary>
/// COMPATIBILITY: Filter criteria type for existing code compatibility  
/// </summary>
internal record FilterCriteria
{
    public string ColumnName { get; init; } = string.Empty;
    public FilterType FilterType { get; init; } = FilterType.Equals;
    public object? Value { get; init; }
    public bool CaseSensitive { get; init; } = false;
    
    public static FilterCriteria Create(string columnName, FilterType filterType, object? value, bool caseSensitive = false) =>
        new() { ColumnName = columnName, FilterType = filterType, Value = value, CaseSensitive = caseSensitive };
}

/// <summary>
/// COMPATIBILITY: Advanced filter criteria type for existing code compatibility  
/// </summary>
internal record AdvancedFilterCriteria
{
    public IReadOnlyList<FilterCriteria>? BasicFilters { get; init; }
    public FilterLogicOperator LogicOperator { get; init; } = FilterLogicOperator.And;
    public bool UseAdvancedLogic { get; init; } = false;
    public string? CustomExpression { get; init; }
    
    public static AdvancedFilterCriteria Create(IReadOnlyList<FilterCriteria> basicFilters, FilterLogicOperator logicOperator = FilterLogicOperator.And) =>
        new() { BasicFilters = basicFilters, LogicOperator = logicOperator };
}

/// <summary>
/// COMPATIBILITY: Filter type enumeration for existing code
/// </summary>
internal enum FilterType
{
    Equals,
    NotEquals,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    IsEmpty,
    IsNotEmpty,
    IsNull,
    IsNotNull,
    Between,
    In,
    NotIn,
    Regex
}