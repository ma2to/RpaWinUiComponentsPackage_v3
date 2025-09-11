using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.Specialized;

/// <summary>
/// SOLID: Single Responsibility - Search and Filter operations only
/// DDD: Domain Service for data querying and filtering
/// CLEAN ARCHITECTURE: Application layer service
/// PERFORMANCE: Optimized for large datasets with smart indexing
/// </summary>
public sealed class DataGridSearchFilterService : IDataGridSearchFilterService
{
    #region Private Fields
    
    private readonly IDataGridSearchService _searchService;
    private readonly IDataGridFilterService _filterService;
    private readonly IDataGridSortService _sortService;
    private readonly ILogger? _logger;
    
    // Performance caching
    private CurrentFilterState _currentFilterState = CurrentFilterState.Empty;
    private CurrentSortState _currentSortState = CurrentSortState.Empty;
    private string? _lastSearchTerm;
    private List<SearchResult>? _lastSearchResults;
    
    #endregion

    #region Constructor
    
    public DataGridSearchFilterService(
        IDataGridSearchService searchService,
        IDataGridFilterService filterService,
        IDataGridSortService sortService,
        ILogger<DataGridSearchFilterService>? logger = null)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService));
        _sortService = sortService ?? throw new ArgumentNullException(nameof(sortService));
        _logger = logger;
    }
    
    #endregion

    #region Search Operations
    
    /// <summary>
    /// ENTERPRISE: Advanced search with caching and performance optimization
    /// PERFORMANCE: Smart caching prevents redundant searches
    /// </summary>
    public async Task<Result<List<SearchResult>>> SearchAsync(
        GridState currentState,
        SearchCommand command)
    {
        if (currentState == null)
            return Result<List<SearchResult>>.Failure("DataGrid must be initialized before searching");

        try
        {
            _logger?.LogInformation("Starting search for term: '{SearchTerm}'", command.SearchTerm);
            var stopwatch = Stopwatch.StartNew();

            // PERFORMANCE: Check cache first
            if (IsSameSearch(command.SearchTerm) && _lastSearchResults != null)
            {
                _logger?.LogDebug("Returning cached search results");
                return Result<List<SearchResult>>.Success(_lastSearchResults);
            }

            // 1. SEARCH EXECUTION: Perform the search
            var searchCriteria = new SearchCriteria
            {
                SearchTerm = command.SearchTerm,
                ColumnNames = command.ColumnNames,
                CaseSensitive = command.CaseSensitive,
                Type = command.SearchType == SearchType.Regex ? SearchType.Regex : SearchType.Contains
            };

            var searchResult = await _searchService.SearchAsync(
                command.SearchTerm,
                command.ColumnNames?.ToArray(),
                command.CaseSensitive);

            if (!searchResult.IsSuccess)
                return Result<List<SearchResult>>.Failure($"Search failed: {searchResult.Error}");

            // 2. RESULT PROCESSING: Convert to expected format
            var results = ProcessSearchResults(searchResult.Value!, command, currentState);

            // 3. PERFORMANCE: Update cache
            _lastSearchTerm = command.SearchTerm;
            _lastSearchResults = results;

            // 4. STATE UPDATE: Update current state with search results
            UpdateStateWithSearchResults(currentState, results);

            stopwatch.Stop();
            _logger?.LogInformation("Search completed in {ElapsedMs}ms. Found {ResultCount} matches", 
                stopwatch.ElapsedMilliseconds, results.Count);

            return Result<List<SearchResult>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Search failed with exception");
            return Result<List<SearchResult>>.Failure($"Search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Clear search results and show all data
    /// PERFORMANCE: Efficient state reset
    /// </summary>
    public async Task<Result<bool>> ClearSearchAsync(GridState currentState)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized");

        try
        {
            _logger?.LogInformation("Clearing search results");

            // Clear search state
            _lastSearchTerm = null;
            _lastSearchResults = null;
            currentState.SearchResults.Clear();

            // Re-apply current filters if any exist
            if (_currentFilterState.HasActiveFilters)
            {
                var filterResult = await ApplyCurrentFiltersAsync(currentState);
                if (!filterResult.IsSuccess)
                {
                    _logger?.LogWarning("Failed to re-apply filters after clearing search: {Error}", filterResult.Error);
                }
            }
            else
            {
                // Show all data
                currentState.FilteredRowIndices = null;
            }

            _logger?.LogInformation("Search cleared successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear search");
            return Result<bool>.Failure($"Failed to clear search: {ex.Message}");
        }
    }

    #endregion

    #region Filter Operations
    
    /// <summary>
    /// ENTERPRISE: Apply complex filters with AND/OR logic
    /// PERFORMANCE: Incremental filtering with caching
    /// </summary>
    public async Task<Result<bool>> ApplyFiltersAsync(
        GridState currentState,
        IReadOnlyList<FilterDefinition> filters,
        FilterLogicOperator logicOperator = FilterLogicOperator.And,
        TimeSpan? timeout = null)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized before applying filters");

        try
        {
            _logger?.LogInformation("Applying {FilterCount} filters with {LogicOperator} operator", 
                filters.Count, logicOperator);
            var stopwatch = Stopwatch.StartNew();

            // 1. FILTER EXECUTION: Apply filters through domain service
            var advancedFilters = filters.Select(f => new AdvancedFilter 
            { 
                ColumnName = f.ColumnName,
                Operator = f.Operator,
                Value = f.Value,
                LogicOperator = f.LogicOperator,
                CaseSensitive = f.CaseSensitive,
                IsEnabled = f.IsEnabled
            }).ToList();
            
            var filterResult = await _filterService.ApplyAdvancedFiltersAsync(
                advancedFilters,
                MapToFilterCombinationMode(logicOperator),
                timeout);

            if (!filterResult.IsSuccess)
                return Result<bool>.Failure($"Filter application failed: {filterResult.Error}");

            // 2. STATE UPDATE: Update current state
            currentState.FilteredRowIndices = filterResult.Value!.FilteredRowIndices.ToList();
            
            // 3. CACHE UPDATE: Update filter state cache
            _currentFilterState = CurrentFilterState.WithFilters(filters, logicOperator);

            stopwatch.Stop();
            _logger?.LogInformation("Filters applied in {ElapsedMs}ms. {FilteredCount}/{TotalCount} rows match", 
                stopwatch.ElapsedMilliseconds, currentState.FilteredRowIndices.Count, currentState.Rows.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Filter application failed with exception");
            return Result<bool>.Failure($"Filter application failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Clear all active filters
    /// PERFORMANCE: Efficient filter state reset
    /// </summary>
    public async Task<Result<bool>> ClearFiltersAsync(GridState currentState)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized");

        try
        {
            _logger?.LogInformation("Clearing all filters");

            // Clear filter state
            _currentFilterState = CurrentFilterState.Empty;
            
            // Clear domain service state
            var clearResult = await _filterService.ClearAllFiltersAsync();
            if (!clearResult.IsSuccess)
            {
                _logger?.LogWarning("Domain filter service clear failed: {Error}", clearResult.Error);
            }

            // Show all data (or apply search if active)
            if (!string.IsNullOrEmpty(_lastSearchTerm) && _lastSearchResults != null)
            {
                // Keep search results, just remove filters
                var searchRowIndices = _lastSearchResults.SelectMany(sr => sr.MatchingRowIndices).Distinct().ToList();
                currentState.FilteredRowIndices = searchRowIndices;
            }
            else
            {
                // Show all data
                currentState.FilteredRowIndices = null;
            }

            _logger?.LogInformation("Filters cleared successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear filters");
            return Result<bool>.Failure($"Failed to clear filters: {ex.Message}");
        }
    }

    #endregion

    #region Sort Operations
    
    /// <summary>
    /// ENTERPRISE: Sort data by column with multiple sort criteria support
    /// PERFORMANCE: Optimized sorting algorithms
    /// </summary>
    public async Task<Result<bool>> SortAsync(
        GridState currentState,
        string columnName,
        SortDirection direction)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized before sorting");

        try
        {
            _logger?.LogInformation("Sorting by column '{ColumnName}' in {Direction} direction", 
                columnName, direction);
            var stopwatch = Stopwatch.StartNew();

            // 1. VALIDATION: Ensure column exists
            var column = currentState.Columns.FirstOrDefault(c => c.Name == columnName);
            if (column == null)
                return Result<bool>.Failure($"Column '{columnName}' not found");

            // 2. SORT EXECUTION: Apply sort through domain service
            var sortCriteria = SortCriteria.Create(columnName, direction);
            var sortResult = await _sortService.ApplyMultiLevelSortAsync(
                new[] { sortCriteria });

            if (!sortResult.IsSuccess)
                return Result<bool>.Failure($"Sort failed: {sortResult.Error}");

            // 3. STATE UPDATE: Update current state with sorted indices
            ApplySortToCurrentState(currentState, sortResult.Value!);
            
            // 4. CACHE UPDATE: Update sort state
            _currentSortState = CurrentSortState.WithSorts(new[] { sortCriteria });

            stopwatch.Stop();
            _logger?.LogInformation("Sort completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Sort failed with exception");
            return Result<bool>.Failure($"Sort failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Clear all sorting and return to original order
    /// </summary>
    public async Task<Result<bool>> ClearSortAsync(GridState currentState)
    {
        if (currentState == null)
            return Result<bool>.Failure("DataGrid must be initialized");

        try
        {
            _logger?.LogInformation("Clearing sort");

            // Reset to original order
            _currentSortState = CurrentSortState.Empty;
            
            // Re-apply current filters/search if any
            await RefreshCurrentView(currentState);

            _logger?.LogInformation("Sort cleared successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear sort");
            return Result<bool>.Failure($"Failed to clear sort: {ex.Message}");
        }
    }

    #endregion

    #region State Management
    
    /// <summary>
    /// Get current filter state for external queries
    /// </summary>
    public CurrentFilterState GetCurrentFilterState() => _currentFilterState;

    /// <summary>
    /// Get current sort state for external queries
    /// </summary>
    public CurrentSortState GetCurrentSortState() => _currentSortState;

    /// <summary>
    /// Check if search is currently active
    /// </summary>
    public bool HasActiveSearch => !string.IsNullOrEmpty(_lastSearchTerm);

    #endregion

    #region Private Helper Methods

    private bool IsSameSearch(string searchTerm)
    {
        return string.Equals(_lastSearchTerm, searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private List<SearchResult> ProcessSearchResults(
        SearchResult domainResult, 
        SearchCommand command, 
        GridState currentState)
    {
        // Convert domain SearchResult to application SearchResult format
        var results = new List<SearchResult>();
        
        // Group matches by row for better UX
        var matchesByRow = domainResult.MatchingRowIndices
            .GroupBy(rowIndex => rowIndex)
            .Select(group => new SearchResult
            {
                MatchingRowIndices = new[] { group.Key },
                TotalMatches = group.Count(),
                TotalSearched = currentState.Rows.Count,
                SearchTerm = command.SearchTerm,
                Criteria = new SearchCriteria
                {
                    SearchTerm = command.SearchTerm,
                    ColumnNames = command.ColumnNames,
                    CaseSensitive = command.CaseSensitive,
                    Type = command.SearchType == SearchType.Regex ? SearchType.Regex : SearchType.Contains
                }
            })
            .Take(command.Options?.MaxResults ?? 1000)
            .ToList();

        return matchesByRow;
    }

    private void UpdateStateWithSearchResults(GridState currentState, List<SearchResult> results)
    {
        // Update state search results
        currentState.SearchResults.Clear();
        currentState.SearchResults.AddRange(results);

        // Update filtered indices with search results
        if (results.Any())
        {
            var searchIndices = results.SelectMany(r => r.MatchingRowIndices).Distinct().ToList();
            
            // If filters are active, intersect with search results
            if (_currentFilterState.HasActiveFilters && currentState.FilteredRowIndices != null)
            {
                currentState.FilteredRowIndices = currentState.FilteredRowIndices
                    .Intersect(searchIndices)
                    .ToList();
            }
            else
            {
                currentState.FilteredRowIndices = searchIndices;
            }
        }
    }

    private async Task<Result<bool>> ApplyCurrentFiltersAsync(GridState currentState)
    {
        if (!_currentFilterState.HasActiveFilters)
            return Result<bool>.Success(true);

        return await ApplyFiltersAsync(
            currentState, 
            _currentFilterState.ActiveFilters, 
            _currentFilterState.GlobalLogicOperator);
    }

    private FilterCombinationMode MapToFilterCombinationMode(FilterLogicOperator logicOperator)
    {
        return logicOperator switch
        {
            FilterLogicOperator.And => FilterCombinationMode.And,
            FilterLogicOperator.Or => FilterCombinationMode.Or,
            _ => FilterCombinationMode.And
        };
    }

    private void ApplySortToCurrentState(GridState currentState, SortResult sortResult)
    {
        // Apply sort order to rows
        var sortedRows = sortResult.SortedIndices
            .Select(index => currentState.Rows[index])
            .ToList();

        currentState.Rows.Clear();
        currentState.Rows.AddRange(sortedRows);

        // Update filtered indices if they exist
        if (currentState.FilteredRowIndices != null)
        {
            var newIndices = new List<int>();
            for (int i = 0; i < currentState.Rows.Count; i++)
            {
                if (currentState.FilteredRowIndices.Contains(currentState.Rows[i].Index))
                {
                    newIndices.Add(i);
                }
            }
            currentState.FilteredRowIndices = newIndices;
        }
    }

    private async Task RefreshCurrentView(GridState currentState)
    {
        // Re-apply search if active
        if (HasActiveSearch)
        {
            var searchCommand = SearchCommand.Create(_lastSearchTerm!);
            await SearchAsync(currentState, searchCommand);
        }
        // Re-apply filters if active
        else if (_currentFilterState.HasActiveFilters)
        {
            await ApplyCurrentFiltersAsync(currentState);
        }
        else
        {
            // Show all data
            currentState.FilteredRowIndices = null;
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _searchService?.Dispose();
        _filterService?.Dispose();
        _sortService?.Dispose();
        
        _logger?.LogDebug("DataGridSearchFilterService disposed");
    }

    #endregion
}

/// <summary>
/// SOLID: Interface segregation for Search and Filter operations
/// </summary>
public interface IDataGridSearchFilterService : IDisposable
{
    // Search operations
    Task<Result<List<SearchResult>>> SearchAsync(GridState currentState, SearchCommand command);
    Task<Result<bool>> ClearSearchAsync(GridState currentState);
    
    // Filter operations
    Task<Result<bool>> ApplyFiltersAsync(GridState currentState, IReadOnlyList<FilterDefinition> filters, FilterLogicOperator logicOperator = FilterLogicOperator.And, TimeSpan? timeout = null);
    Task<Result<bool>> ClearFiltersAsync(GridState currentState);
    
    // Sort operations
    Task<Result<bool>> SortAsync(GridState currentState, string columnName, SortDirection direction);
    Task<Result<bool>> ClearSortAsync(GridState currentState);
    
    // State queries
    CurrentFilterState GetCurrentFilterState();
    CurrentSortState GetCurrentSortState();
    bool HasActiveSearch { get; }
}