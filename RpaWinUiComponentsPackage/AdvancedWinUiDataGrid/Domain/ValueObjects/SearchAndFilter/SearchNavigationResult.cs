using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object representing search navigation operation result
/// ENTERPRISE: Navigation state for search results
/// </summary>
internal record SearchNavigationResult
{
    /// <summary>Current result position (0-based)</summary>
    public int CurrentPosition { get; init; }
    
    /// <summary>Total number of search results</summary>
    public int TotalResults { get; init; }
    
    /// <summary>Row index of current result</summary>
    public int RowIndex { get; init; }
    
    /// <summary>Column name of current result</summary>
    public string ColumnName { get; init; } = string.Empty;
    
    /// <summary>Whether navigation was successful</summary>
    public bool Success { get; init; }
    
    /// <summary>Navigation direction used</summary>
    public SearchNavigationDirection Direction { get; init; }
    
    /// <summary>Whether this is the first result</summary>
    public bool IsFirst => CurrentPosition == 0;
    
    /// <summary>Whether this is the last result</summary>
    public bool IsLast => CurrentPosition == TotalResults - 1;
    
    /// <summary>Whether there are more results in forward direction</summary>
    public bool HasNext => CurrentPosition < TotalResults - 1;
    
    /// <summary>Whether there are more results in backward direction</summary>
    public bool HasPrevious => CurrentPosition > 0;
    
    /// <summary>Create successful navigation result</summary>
    public static SearchNavigationResult CreateSuccess(int currentPosition, int totalResults, int rowIndex, string columnName, SearchNavigationDirection direction) =>
        new()
        {
            Success = true,
            CurrentPosition = currentPosition,
            TotalResults = totalResults,
            RowIndex = rowIndex,
            ColumnName = columnName,
            Direction = direction
        };
    
    /// <summary>Create failed navigation result</summary>
    public static SearchNavigationResult CreateFailed(SearchNavigationDirection direction) =>
        new()
        {
            Success = false,
            Direction = direction
        };
}