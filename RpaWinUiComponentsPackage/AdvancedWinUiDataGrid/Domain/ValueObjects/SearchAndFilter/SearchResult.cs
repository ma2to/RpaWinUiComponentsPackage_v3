using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object representing search operation results
/// ENTERPRISE: Comprehensive search result with metadata
/// </summary>
internal record SearchResult
{
    public IReadOnlyList<int> MatchingRowIndices { get; init; } = [];
    public int TotalMatches { get; init; }
    public int TotalSearched { get; init; }
    public string SearchTerm { get; init; } = string.Empty;
    public SearchCriteria? Criteria { get; init; }
    public bool HasResults => TotalMatches > 0;
    public int ReturnedMatches => MatchingRowIndices.Count;
    public TimeSpan SearchDuration { get; init; } = TimeSpan.Zero;
    
    public static SearchResult Empty(string searchTerm = "") =>
        new() { SearchTerm = searchTerm };
    
    public static SearchResult Create(IReadOnlyList<int> matchingRows, int totalSearched, string searchTerm, SearchCriteria? criteria = null) =>
        new()
        {
            MatchingRowIndices = matchingRows,
            TotalMatches = matchingRows.Count,
            TotalSearched = totalSearched,
            SearchTerm = searchTerm,
            Criteria = criteria
        };
}

/// <summary>
/// DDD: Value object for search criteria
/// </summary>
internal record SearchCriteria
{
    public string SearchTerm { get; init; } = string.Empty;
    public IReadOnlyList<string>? ColumnNames { get; init; }
    public bool CaseSensitive { get; init; } = false;
    public bool WholeWordOnly { get; init; } = false;
    public SearchType Type { get; init; } = SearchType.Contains;
}

/// <summary>
/// DDD: Value object for advanced search criteria
/// </summary>
internal record AdvancedSearchCriteria : SearchCriteria
{
    public bool UseRegex { get; init; } = false;
    public IReadOnlyDictionary<string, object?>? FilterValues { get; init; }
    public TimeSpan? Timeout { get; init; }
    public string SearchText => SearchTerm;
    public SearchScope Scope { get; init; } = SearchScope.AllColumns;
    public SearchAlgorithm Algorithm { get; init; } = SearchAlgorithm.Linear;
}


/// <summary>
/// DDD: Enum for search scope
/// </summary>
internal enum SearchScope
{
    AllColumns,
    SelectedColumns,
    VisibleColumns
}

/// <summary>
/// DDD: Enum for search algorithms
/// </summary>
internal enum SearchAlgorithm
{
    Linear,
    Indexed,
    FullText,
    RegexOptimized
}

