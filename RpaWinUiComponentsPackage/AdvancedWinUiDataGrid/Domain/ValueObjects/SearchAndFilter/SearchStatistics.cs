using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object representing search performance statistics
/// ENTERPRISE: Search monitoring and analytics
/// </summary>
internal record SearchStatistics
{
    /// <summary>Total number of searches performed in session</summary>
    public int TotalSearches { get; init; }
    
    /// <summary>Total number of rows searched across all operations</summary>
    public long TotalRowsSearched { get; init; }
    
    /// <summary>Total number of matches found</summary>
    public int TotalMatches { get; init; }
    
    /// <summary>Total time spent on search operations</summary>
    public TimeSpan TotalSearchTime { get; init; }
    
    /// <summary>Average search time per operation</summary>
    public TimeSpan AverageSearchTime => TotalSearches > 0 ? 
        TimeSpan.FromTicks(TotalSearchTime.Ticks / TotalSearches) : TimeSpan.Zero;
    
    /// <summary>Average rows processed per second</summary>
    public double RowsPerSecond => TotalSearchTime.TotalSeconds > 0 ? 
        TotalRowsSearched / TotalSearchTime.TotalSeconds : 0;
    
    /// <summary>Hit rate - percentage of searches that found results</summary>
    public double HitRate => TotalSearches > 0 ? 
        (double)SearchesWithResults / TotalSearches * 100 : 0;
    
    /// <summary>Number of searches that found results</summary>
    public int SearchesWithResults { get; init; }
    
    /// <summary>Fastest search time</summary>
    public TimeSpan FastestSearch { get; init; } = TimeSpan.MaxValue;
    
    /// <summary>Slowest search time</summary>
    public TimeSpan SlowestSearch { get; init; } = TimeSpan.Zero;
    
    /// <summary>Most common search algorithm used</summary>
    public SearchAlgorithm MostUsedAlgorithm { get; init; }
    
    /// <summary>Current search session start time</summary>
    public DateTime SessionStartTime { get; init; }
    
    /// <summary>Last search timestamp</summary>
    public DateTime LastSearchTime { get; init; }
    
    /// <summary>Session duration</summary>
    public TimeSpan SessionDuration => DateTime.Now - SessionStartTime;
    
    /// <summary>Create empty statistics</summary>
    public static SearchStatistics Empty => new()
    {
        SessionStartTime = DateTime.Now
    };
    
    /// <summary>Create statistics with updated search count</summary>
    public SearchStatistics WithSearch(TimeSpan searchTime, int rowsSearched, int matches, SearchAlgorithm algorithm) =>
        this with
        {
            TotalSearches = TotalSearches + 1,
            TotalRowsSearched = TotalRowsSearched + rowsSearched,
            TotalMatches = TotalMatches + matches,
            TotalSearchTime = TotalSearchTime + searchTime,
            SearchesWithResults = matches > 0 ? SearchesWithResults + 1 : SearchesWithResults,
            FastestSearch = searchTime < FastestSearch ? searchTime : FastestSearch,
            SlowestSearch = searchTime > SlowestSearch ? searchTime : SlowestSearch,
            MostUsedAlgorithm = algorithm,
            LastSearchTime = DateTime.Now
        };
}