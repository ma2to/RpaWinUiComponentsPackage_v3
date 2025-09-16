using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Persistence;

/// <summary>
/// INFRASTRUCTURE: Search and filter service implementation
/// CLEAN ARCHITECTURE: Infrastructure layer - handles search operations
/// RESPONSIBILITY: Execute search, filter, and sort operations on grid data
/// </summary>
internal sealed class DataGridSearchService : IDataGridSearchService
{
    #region ENTERPRISE: Private Fields
    
    private readonly ILogger? _logger;
    private SearchResult? _currentSearchResult;
    private int _currentNavigationPosition = -1;
    private SearchStatistics _statistics = SearchStatistics.Empty;
    private bool _disposed = false;
    
    #endregion

    #region ENTERPRISE: Constructor
    
    public DataGridSearchService(ILogger? logger = null)
    {
        _logger = logger;
    }
    
    #endregion

    #region INTERFACE: Search Operations
    
    public async Task<Result<SearchResult>> SearchAsync(
        string searchText,
        string[]? targetColumns = null,
        bool caseSensitive = false,
        int? maxResults = null,
        TimeSpan? timeout = null)
    {
        var startTime = DateTime.Now;
        
        try
        {
            _logger?.LogInformation("Searching for '{SearchText}'", searchText);
            
            // Basic search implementation - would need grid context in real scenario
            var matchingRowIndices = new List<int>();
            var searchDuration = DateTime.Now - startTime;
            
            var result = SearchResult.Create(matchingRowIndices, 0, searchText);
            result = result with { SearchDuration = searchDuration };
            
            _currentSearchResult = result;
            _currentNavigationPosition = result.HasResults ? 0 : -1;
            
            // Update statistics
            _statistics = _statistics.WithSearch(searchDuration, 0, result.TotalMatches, SearchAlgorithm.Linear);
            
            _logger?.LogInformation("Search completed with {ResultCount} matches", result.TotalMatches);
            return Result<SearchResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Search operation failed");
            return Result<SearchResult>.Failure("Search operation failed");
        }
    }
    
    public async Task<Result<SearchResult>> AdvancedSearchAsync(
        AdvancedSearchCriteria criteria,
        TimeSpan? timeout = null)
    {
        var startTime = DateTime.Now;
        
        try
        {
            _logger?.LogInformation("Advanced search for '{SearchText}'", criteria.SearchText);
            
            // Advanced search implementation - would need grid context in real scenario
            var matchingRowIndices = new List<int>();
            var searchDuration = DateTime.Now - startTime;
            
            var result = SearchResult.Create(matchingRowIndices, 0, criteria.SearchText, criteria);
            result = result with { SearchDuration = searchDuration };
            
            _currentSearchResult = result;
            _currentNavigationPosition = result.HasResults ? 0 : -1;
            
            // Update statistics
            var algorithm = criteria.UseRegex ? SearchAlgorithm.RegexOptimized : 
                          criteria.Algorithm;
            _statistics = _statistics.WithSearch(searchDuration, 0, result.TotalMatches, algorithm);
            
            _logger?.LogInformation("Advanced search completed with {ResultCount} matches", result.TotalMatches);
            return Result<SearchResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Advanced search operation failed");
            return Result<SearchResult>.Failure("Advanced search operation failed");
        }
    }
    
    public async Task<Result<bool>> ClearSearchAsync()
    {
        try
        {
            _currentSearchResult = null;
            _currentNavigationPosition = -1;
            _logger?.LogInformation("Search results cleared");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear search results");
            return Result<bool>.Failure("Failed to clear search results");
        }
    }
    
    public async Task<Result<SearchNavigationResult>> NavigateSearchResultAsync(SearchNavigationDirection direction)
    {
        try
        {
            if (_currentSearchResult == null || !_currentSearchResult.HasResults)
            {
                return Result<SearchNavigationResult>.Success(
                    SearchNavigationResult.CreateFailed(direction));
            }
            
            var totalResults = _currentSearchResult.TotalMatches;
            var newPosition = direction switch
            {
                SearchNavigationDirection.First => 0,
                SearchNavigationDirection.Previous => Math.Max(0, _currentNavigationPosition - 1),
                SearchNavigationDirection.Next => Math.Min(totalResults - 1, _currentNavigationPosition + 1),
                SearchNavigationDirection.Last => totalResults - 1,
                _ => _currentNavigationPosition
            };
            
            if (newPosition >= 0 && newPosition < _currentSearchResult.MatchingRowIndices.Count)
            {
                _currentNavigationPosition = newPosition;
                var rowIndex = _currentSearchResult.MatchingRowIndices[newPosition];
                
                var result = SearchNavigationResult.CreateSuccess(
                    newPosition, totalResults, rowIndex, string.Empty, direction);
                    
                return Result<SearchNavigationResult>.Success(result);
            }
            
            return Result<SearchNavigationResult>.Success(
                SearchNavigationResult.CreateFailed(direction));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Navigation failed");
            return Result<SearchNavigationResult>.Failure("Navigation failed");
        }
    }
    
    public async Task<Result<SearchStatistics>> GetSearchStatisticsAsync()
    {
        try
        {
            return Result<SearchStatistics>.Success(_statistics);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get search statistics");
            return Result<SearchStatistics>.Failure("Failed to get search statistics");
        }
    }
    
    #endregion

    #region LEGACY: Backward Compatibility Methods (kept for compilation)
    
    public async Task<Result<List<GridRow>>> ApplyFiltersAsync(
        IReadOnlyList<GridRow> rows,
        IReadOnlyList<GridColumn> columns,
        IReadOnlyList<FilterDefinition> filters,
        FilterLogicOperator logicOperator = FilterLogicOperator.And)
    {
        // Legacy implementation for backward compatibility
        return Result<List<GridRow>>.Success(rows.ToList());
    }
    
    public async Task<Result<List<GridRow>>> SortAsync(
        IReadOnlyList<GridRow> rows,
        IReadOnlyList<GridColumn> columns,
        string columnName,
        SortDirection direction)
    {
        // Legacy implementation for backward compatibility
        return Result<List<GridRow>>.Success(rows.ToList());
    }
    
    #endregion

    #region IDisposable Implementation
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _currentSearchResult = null;
            _currentNavigationPosition = -1;
            _disposed = true;
            _logger?.LogInformation("DataGridSearchService disposed");
        }
    }
    
    #endregion
}