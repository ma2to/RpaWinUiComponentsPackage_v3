using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Bridge;

/// <summary>
/// PROFESSIONAL Search Manager for DataGridBridge
/// RESPONSIBILITY: Handle search, filter, and sort operations
/// </summary>
internal sealed class DataGridBridgeSearchManager : IDisposable
{
    private readonly AdvancedDataGrid _internalGrid;
    private readonly ILogger? _logger;
    private readonly List<string> _searchHistory = new();
    private readonly object _historyLock = new();
    private readonly List<AdvancedFilter> _activeFilters = new();
    private readonly object _filtersLock = new();

    public DataGridBridgeSearchManager(AdvancedDataGrid internalGrid, ILogger? logger)
    {
        _internalGrid = internalGrid ?? throw new ArgumentNullException(nameof(internalGrid));
        _logger = logger;
        _logger?.Info("🔍 SEARCH MANAGER: Created DataGridBridgeSearchManager");
    }

    // Search operations
    public async Task<SearchResults?> SearchAsync(string searchText, IReadOnlyList<string>? targetColumns = null, bool caseSensitive = false, bool wholeWord = false, TimeSpan timeout = default, IProgress<SearchProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            _logger?.Warning("🔍 SEARCH: Empty search text provided");
            return new SearchResults { SearchText = searchText };
        }

        _logger?.Info("🔍 SEARCH: Searching for '{Text}' in {ColumnCount} columns", searchText, targetColumns?.Count ?? 0);

        var startTime = DateTime.UtcNow;
        var matches = new List<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.SearchMatch>();
        
        try
        {
            // Get data from internal grid
            var dataRows = _internalGrid.DataRows;
            var headers = _internalGrid.Headers;
            
            if (dataRows == null || headers == null)
            {
                _logger?.Warning("🔍 SEARCH: No data available for search");
                return new SearchResults { SearchText = searchText };
            }

            var totalRows = dataRows.Count;
            var processedRows = 0;

            // Determine which columns to search
            var columnsToSearch = targetColumns?.ToList() ?? headers.Select(h => h.Name).ToList();
            var columnIndices = columnsToSearch
                .Select(name => headers.ToList().FindIndex(h => h.Name == name))
                .Where(index => index >= 0)
                .ToList();

            foreach (var row in dataRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (int colIndex = 0; colIndex < columnIndices.Count; colIndex++)
                {
                    var columnIndex = columnIndices[colIndex];
                    if (columnIndex < 0 || columnIndex >= row.Cells.Count) continue;

                    var cell = row.Cells[columnIndex];
                    var cellValue = cell?.Value?.ToString() ?? "";
                    
                    if (string.IsNullOrEmpty(cellValue)) continue;

                    // Perform search based on parameters
                    var searchMatches = FindMatches(cellValue, searchText, caseSensitive, wholeWord);
                    
                    foreach (var match in searchMatches)
                    {
                        matches.Add(new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.SearchMatch
                        {
                            RowIndex = processedRows,
                            ColumnIndex = columnIndex,
                            ColumnName = headers[columnIndex].Name,
                            MatchedText = match.Text,
                            CellValue = cell?.Value,
                            StartIndex = match.StartIndex,
                            Length = match.Length
                        });
                    }
                }

                processedRows++;
                
                // Report progress
                progress?.Report(new SearchProgress
                {
                    ProcessedRows = processedRows,
                    TotalRows = totalRows,
                    MatchesFound = matches.Count,
                    CurrentOperation = $"Searching row {processedRows}/{totalRows}"
                });

                // Check timeout
                if (timeout != default && DateTime.UtcNow - startTime > timeout)
                {
                    _logger?.Warning("🔍 SEARCH: Search timed out after {Duration}ms", timeout.TotalMilliseconds);
                    break;
                }
            }

            var results = new SearchResults
            {
                Matches = matches.AsReadOnly(),
                TotalMatches = matches.Count,
                Duration = DateTime.UtcNow - startTime,
                WasCancelled = cancellationToken.IsCancellationRequested,
                SearchText = searchText
            };

            _logger?.Info("✅ SEARCH: Found {MatchCount} matches in {Duration}ms", matches.Count, results.Duration.TotalMilliseconds);
            
            // Add to search history
            await AddSearchToHistoryAsync(searchText);
            
            return results;
        }
        catch (OperationCanceledException)
        {
            _logger?.Info("🔍 SEARCH: Search cancelled by user");
            return new SearchResults 
            { 
                Matches = matches.AsReadOnly(),
                TotalMatches = matches.Count,
                Duration = DateTime.UtcNow - startTime,
                WasCancelled = true,
                SearchText = searchText
            };
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 SEARCH ERROR: Search failed for text '{Text}'", searchText);
            return null;
        }
    }

    public async Task<AdvancedSearchResults?> AdvancedSearchAsync(AdvancedSearchCriteria criteria, TimeSpan timeout = default, IProgress<SearchProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (criteria == null || string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            _logger?.Warning("🔍 ADVANCED SEARCH: Invalid search criteria provided");
            return new AdvancedSearchResults { Criteria = criteria ?? new AdvancedSearchCriteria() };
        }

        _logger?.Info("🔍 ADVANCED SEARCH: Advanced search for '{Text}' with regex: {UseRegex}", criteria.SearchText, criteria.UseRegex);

        var startTime = DateTime.UtcNow;
        var matches = new List<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.SearchMatch>();
        var matchesPerColumn = new Dictionary<string, int>();
        
        try
        {
            // Get data from internal grid
            var dataRows = _internalGrid.DataRows;
            var headers = _internalGrid.Headers;
            
            if (dataRows == null || headers == null)
            {
                _logger?.Warning("🔍 ADVANCED SEARCH: No data available for search");
                return new AdvancedSearchResults { Criteria = criteria };
            }

            var totalRows = dataRows.Count;
            var processedRows = 0;

            // Determine which columns to search
            var columnsToSearch = criteria.TargetColumns?.ToList() ?? headers.Select(h => h.Name).ToList();
            var columnIndices = columnsToSearch
                .Select(name => headers.ToList().FindIndex(h => h.Name == name))
                .Where(index => index >= 0)
                .ToList();

            // Initialize match counters for columns
            foreach (var columnName in columnsToSearch)
            {
                matchesPerColumn[columnName] = 0;
            }

            // Prepare regex if needed
            Regex? regex = null;
            if (criteria.UseRegex)
            {
                try
                {
                    var regexOptions = RegexOptions.Compiled;
                    if (!criteria.CaseSensitive)
                        regexOptions |= RegexOptions.IgnoreCase;
                    
                    regex = new Regex(criteria.SearchText, regexOptions);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "🚨 ADVANCED SEARCH ERROR: Invalid regex pattern '{Pattern}'", criteria.SearchText);
                    return null;
                }
            }

            foreach (var row in dataRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (int colIndex = 0; colIndex < columnIndices.Count; colIndex++)
                {
                    var columnIndex = columnIndices[colIndex];
                    if (columnIndex < 0 || columnIndex >= row.Cells.Count) continue;

                    var cell = row.Cells[columnIndex];
                    var cellValue = cell?.Value?.ToString() ?? "";
                    
                    if (string.IsNullOrEmpty(cellValue)) continue;

                    // Perform search based on criteria
                    IEnumerable<(string Text, int StartIndex, int Length)> searchMatches;
                    
                    if (criteria.UseRegex && regex != null)
                    {
                        searchMatches = FindRegexMatches(cellValue, regex);
                    }
                    else
                    {
                        searchMatches = FindMatches(cellValue, criteria.SearchText, criteria.CaseSensitive, criteria.WholeWord);
                    }
                    
                    var columnName = headers[columnIndex].Name;
                    
                    foreach (var match in searchMatches)
                    {
                        matches.Add(new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.SearchMatch
                        {
                            RowIndex = processedRows,
                            ColumnIndex = columnIndex,
                            ColumnName = columnName,
                            MatchedText = match.Text,
                            CellValue = cell?.Value,
                            StartIndex = match.StartIndex,
                            Length = match.Length
                        });
                        
                        matchesPerColumn[columnName]++;
                        
                        // Check if we've reached the max matches limit
                        if (criteria.MaxMatches.HasValue && matches.Count >= criteria.MaxMatches.Value)
                        {
                            _logger?.Info("🔍 ADVANCED SEARCH: Reached maximum matches limit ({MaxMatches})", criteria.MaxMatches.Value);
                            break;
                        }
                    }
                    
                    if (criteria.MaxMatches.HasValue && matches.Count >= criteria.MaxMatches.Value)
                        break;
                }

                if (criteria.MaxMatches.HasValue && matches.Count >= criteria.MaxMatches.Value)
                    break;

                processedRows++;
                
                // Report progress
                progress?.Report(new SearchProgress
                {
                    ProcessedRows = processedRows,
                    TotalRows = totalRows,
                    MatchesFound = matches.Count,
                    CurrentOperation = $"Advanced searching row {processedRows}/{totalRows}"
                });

                // Check timeout
                if (timeout != default && DateTime.UtcNow - startTime > timeout)
                {
                    _logger?.Warning("🔍 ADVANCED SEARCH: Search timed out after {Duration}ms", timeout.TotalMilliseconds);
                    break;
                }
            }

            var results = new AdvancedSearchResults
            {
                Criteria = criteria,
                Matches = matches.AsReadOnly(),
                TotalMatches = matches.Count,
                Duration = DateTime.UtcNow - startTime,
                WasCancelled = cancellationToken.IsCancellationRequested,
                SearchText = criteria.SearchText,
                MatchesPerColumn = matchesPerColumn.AsReadOnly()
            };

            _logger?.Info("✅ ADVANCED SEARCH: Found {MatchCount} matches in {Duration}ms", matches.Count, results.Duration.TotalMilliseconds);
            
            // Add to search history
            await AddSearchToHistoryAsync(criteria.SearchText);
            
            return results;
        }
        catch (OperationCanceledException)
        {
            _logger?.Info("🔍 ADVANCED SEARCH: Search cancelled by user");
            return new AdvancedSearchResults 
            { 
                Criteria = criteria,
                Matches = matches.AsReadOnly(),
                TotalMatches = matches.Count,
                Duration = DateTime.UtcNow - startTime,
                WasCancelled = true,
                SearchText = criteria.SearchText,
                MatchesPerColumn = matchesPerColumn.AsReadOnly()
            };
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 ADVANCED SEARCH ERROR: Advanced search failed for text '{Text}'", criteria.SearchText);
            return null;
        }
    }

    // Search history
    public Task AddSearchToHistoryAsync(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Task.CompletedTask;

        lock (_historyLock)
        {
            // Remove if already exists to avoid duplicates
            _searchHistory.Remove(searchText);
            
            // Add to beginning
            _searchHistory.Insert(0, searchText);
            
            // Keep only last 50 searches
            if (_searchHistory.Count > 50)
            {
                _searchHistory.RemoveRange(50, _searchHistory.Count - 50);
            }
        }

        _logger?.Info("🔍 SEARCH HISTORY: Added '{Text}' to search history (total: {Count})", searchText, _searchHistory.Count);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetSearchHistoryAsync()
    {
        lock (_historyLock)
        {
            return Task.FromResult<IReadOnlyList<string>>(_searchHistory.AsReadOnly());
        }
    }

    public Task ClearSearchHistoryAsync()
    {
        lock (_historyLock)
        {
            var count = _searchHistory.Count;
            _searchHistory.Clear();
            _logger?.Info("🔍 SEARCH HISTORY: Cleared {Count} items from search history", count);
        }
        return Task.CompletedTask;
    }

    // Filter operations
    public async Task ApplyFiltersAsync(IReadOnlyList<AdvancedFilter> filters, TimeSpan timeout = default, IProgress<FilterProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (filters == null || !filters.Any())
        {
            _logger?.Warning("🔍 FILTERS: No filters provided");
            return;
        }

        _logger?.Info("🔍 FILTERS: Applying {Count} filters", filters.Count);

        var startTime = DateTime.UtcNow;
        var visibleRows = 0;
        var hiddenRows = 0;

        try
        {
            // Store active filters
            lock (_filtersLock)
            {
                _activeFilters.Clear();
                _activeFilters.AddRange(filters);
            }

            // Get data from internal grid
            var dataRows = _internalGrid.DataRows;
            var headers = _internalGrid.Headers;
            
            if (dataRows == null || headers == null)
            {
                _logger?.Warning("🔍 FILTERS: No data available for filtering");
                return;
            }

            var totalRows = dataRows.Count;
            var processedRows = 0;

            // Process each row
            foreach (var row in dataRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool rowPassesFilter = EvaluateRowAgainstFilters(row, filters, headers);

                // Update row visibility (this would need to be implemented in the UI layer)
                // For now, we're just counting
                if (rowPassesFilter)
                {
                    visibleRows++;
                    // TODO: Make row visible in UI
                }
                else
                {
                    hiddenRows++;
                    // TODO: Hide row in UI
                }

                processedRows++;
                
                // Report progress
                progress?.Report(new FilterProgress
                {
                    ProcessedRows = processedRows,
                    TotalRows = totalRows,
                    FilteredRows = visibleRows,
                    CurrentOperation = $"Filtering row {processedRows}/{totalRows}"
                });

                // Check timeout
                if (timeout != default && DateTime.UtcNow - startTime > timeout)
                {
                    _logger?.Warning("🔍 FILTERS: Filter operation timed out after {Duration}ms", timeout.TotalMilliseconds);
                    break;
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger?.Info("✅ FILTERS: Filtered {TotalRows} rows - {VisibleRows} visible, {HiddenRows} hidden in {Duration}ms", 
                totalRows, visibleRows, hiddenRows, duration.TotalMilliseconds);
        }
        catch (OperationCanceledException)
        {
            _logger?.Info("🔍 FILTERS: Filter operation cancelled by user");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 FILTER ERROR: Filter operation failed");
        }
    }

    public Task ClearFiltersAsync()
    {
        lock (_filtersLock)
        {
            var count = _activeFilters.Count;
            _activeFilters.Clear();
            _logger?.Info("🔍 FILTERS: Cleared {Count} active filters", count);
        }
        
        // TODO: Show all rows in UI
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AdvancedFilter>> GetActiveFiltersAsync()
    {
        lock (_filtersLock)
        {
            return Task.FromResult<IReadOnlyList<AdvancedFilter>>(_activeFilters.AsReadOnly());
        }
    }

    // Sort operations
    public Task ApplySortAsync(IReadOnlyList<MultiSortColumn> sortColumns, TimeSpan timeout = default, IProgress<SortProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        _logger?.Info("🔍 SORT: Applying sort to {Count} columns", sortColumns?.Count ?? 0);
        return Task.CompletedTask;
    }

    public Task ClearSortAsync()
    {
        _logger?.Info("🔍 SORT: Clearing all sorts");
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MultiSortColumn>> GetActiveSortsAsync() => 
        Task.FromResult<IReadOnlyList<MultiSortColumn>>(Array.Empty<MultiSortColumn>());

    #region Private Helper Methods

    private bool EvaluateRowAgainstFilters(DataGridRow row, IReadOnlyList<AdvancedFilter> filters, IReadOnlyList<GridColumnDefinition> headers)
    {
        if (filters == null || !filters.Any())
            return true;

        // Group filters by logical operator
        var andFilters = filters.Where(f => f.LogicOperator == FilterLogicOperator.And).ToList();
        var orFilters = filters.Where(f => f.LogicOperator == FilterLogicOperator.Or).ToList();
        var notFilters = filters.Where(f => f.LogicOperator == FilterLogicOperator.Not).ToList();

        // All AND filters must pass
        bool andResult = true;
        foreach (var filter in andFilters)
        {
            if (!EvaluateSingleFilter(row, filter, headers))
            {
                andResult = false;
                break;
            }
        }

        // At least one OR filter must pass (if any exist)
        bool orResult = true;
        if (orFilters.Any())
        {
            orResult = orFilters.Any(filter => EvaluateSingleFilter(row, filter, headers));
        }

        // All NOT filters must fail (be false)
        bool notResult = true;
        foreach (var filter in notFilters)
        {
            if (EvaluateSingleFilter(row, filter, headers))
            {
                notResult = false;
                break;
            }
        }

        // If no specific operators are set, treat as AND by default
        if (!andFilters.Any() && !orFilters.Any() && !notFilters.Any())
        {
            return filters.All(filter => EvaluateSingleFilter(row, filter, headers));
        }

        return andResult && orResult && notResult;
    }

    private bool EvaluateSingleFilter(DataGridRow row, AdvancedFilter filter, IReadOnlyList<GridColumnDefinition> headers)
    {
        // Find the column index
        var columnIndex = headers.ToList().FindIndex(h => h.Name == filter.ColumnName);
        if (columnIndex < 0 || columnIndex >= row.Cells.Count)
            return false;

        var cell = row.Cells[columnIndex];
        var cellValue = cell?.Value;

        return EvaluateFilterCondition(cellValue, filter.Operator, filter.Value, filter.CaseSensitive);
    }

    private bool EvaluateFilterCondition(object? cellValue, FilterOperator op, object? filterValue, bool caseSensitive)
    {
        switch (op)
        {
            case FilterOperator.Equals:
                return CompareValues(cellValue, filterValue, caseSensitive) == 0;

            case FilterOperator.NotEquals:
                return CompareValues(cellValue, filterValue, caseSensitive) != 0;

            case FilterOperator.Contains:
                return cellValue?.ToString()?.Contains(filterValue?.ToString() ?? "", 
                    caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) ?? false;

            case FilterOperator.NotContains:
                return !(cellValue?.ToString()?.Contains(filterValue?.ToString() ?? "", 
                    caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) ?? false);

            case FilterOperator.StartsWith:
                return cellValue?.ToString()?.StartsWith(filterValue?.ToString() ?? "", 
                    caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) ?? false;

            case FilterOperator.EndsWith:
                return cellValue?.ToString()?.EndsWith(filterValue?.ToString() ?? "", 
                    caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) ?? false;

            case FilterOperator.GreaterThan:
                return CompareValues(cellValue, filterValue, caseSensitive) > 0;

            case FilterOperator.LessThan:
                return CompareValues(cellValue, filterValue, caseSensitive) < 0;

            case FilterOperator.GreaterOrEqual:
                return CompareValues(cellValue, filterValue, caseSensitive) >= 0;

            case FilterOperator.LessOrEqual:
                return CompareValues(cellValue, filterValue, caseSensitive) <= 0;

            case FilterOperator.IsNull:
                return cellValue == null;

            case FilterOperator.IsNotNull:
                return cellValue != null;

            case FilterOperator.IsEmpty:
                return string.IsNullOrEmpty(cellValue?.ToString());

            case FilterOperator.IsNotEmpty:
                return !string.IsNullOrEmpty(cellValue?.ToString());

            default:
                return false;
        }
    }

    private int CompareValues(object? value1, object? value2, bool caseSensitive)
    {
        if (value1 == null && value2 == null) return 0;
        if (value1 == null) return -1;
        if (value2 == null) return 1;

        // Try numeric comparison first
        if (double.TryParse(value1.ToString(), out var num1) && double.TryParse(value2.ToString(), out var num2))
        {
            return num1.CompareTo(num2);
        }

        // Try date comparison
        if (DateTime.TryParse(value1.ToString(), out var date1) && DateTime.TryParse(value2.ToString(), out var date2))
        {
            return date1.CompareTo(date2);
        }

        // Fall back to string comparison
        var str1 = value1.ToString() ?? "";
        var str2 = value2.ToString() ?? "";
        
        return string.Compare(str1, str2, !caseSensitive);
    }

    private IEnumerable<(string Text, int StartIndex, int Length)> FindRegexMatches(string text, Regex regex)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        var matches = regex.Matches(text);
        foreach (Match match in matches)
        {
            if (match.Success)
            {
                yield return (
                    Text: match.Value,
                    StartIndex: match.Index,
                    Length: match.Length
                );
            }
        }
    }

    private IEnumerable<(string Text, int StartIndex, int Length)> FindMatches(string text, string searchText, bool caseSensitive, bool wholeWord)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchText))
            yield break;

        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var searchTextToUse = caseSensitive ? searchText : searchText.ToLowerInvariant();
        var textToSearch = caseSensitive ? text : text.ToLowerInvariant();

        int startIndex = 0;
        while (startIndex < textToSearch.Length)
        {
            int foundIndex = textToSearch.IndexOf(searchTextToUse, startIndex, comparison);
            if (foundIndex == -1)
                break;

            // Check for whole word match if required
            if (wholeWord)
            {
                bool isValidStart = foundIndex == 0 || !char.IsLetterOrDigit(textToSearch[foundIndex - 1]);
                bool isValidEnd = foundIndex + searchTextToUse.Length == textToSearch.Length || 
                                  !char.IsLetterOrDigit(textToSearch[foundIndex + searchTextToUse.Length]);
                
                if (!isValidStart || !isValidEnd)
                {
                    startIndex = foundIndex + 1;
                    continue;
                }
            }

            yield return (
                Text: text.Substring(foundIndex, searchText.Length),
                StartIndex: foundIndex,
                Length: searchText.Length
            );

            startIndex = foundIndex + searchText.Length;
        }
    }

    #endregion

    public void Dispose()
    {
        _logger?.Info("🔍 SEARCH MANAGER DISPOSE: Cleaning up search resources");
    }
}