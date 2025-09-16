using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases;

/// <summary>
/// ENTERPRISE: Advanced DataGrid Search System per documentation  
/// CLEAN ARCHITECTURE: Search domain logic following documentation standards
/// PERFORMANCE: Optimized search algorithms for 10M+ row datasets
/// DOCUMENTATION: Aligned with professional documentation architecture
/// </summary>
internal sealed partial class DataGrid
{
    #region ENTERPRISE: High-Performance Search System

    /// <summary>
    /// PERFORMANCE: Basic search with intelligent indexing
    /// ENTERPRISE: Optimized for large datasets with result limiting
    /// </summary>
    internal async Task<Result<SearchResult>> SearchAsync(
        string searchText,
        string[]? targetColumns = null,
        bool caseSensitive = false,
        int? maxResults = null,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (string.IsNullOrWhiteSpace(searchText))
            return Result<SearchResult>.Failure("Search text cannot be empty");

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 SEARCH: Starting basic search for '{SearchText}' in columns [{Columns}]", 
                    searchText, targetColumns != null ? string.Join(", ", targetColumns) : "ALL");

                var result = await _searchService.SearchAsync(
                    searchText, 
                    targetColumns, 
                    caseSensitive, 
                    maxResults, 
                    effectiveTimeout);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ SEARCH: Found {TotalMatches} matches (returned {ReturnedMatches}) in {Duration}ms", 
                        result.Value?.TotalMatches ?? 0, 
                        result.Value?.ReturnedMatches ?? 0,
                        result.Value?.SearchDuration.TotalMilliseconds ?? 0);
                }
                else
                {
                    _logger.LogWarning("❌ SEARCH: Search failed: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Search operation failed for text '{searchText}'";
            _logger.LogError(ex, "💥 SEARCH: {ErrorMessage}", errorMessage);
            return Result<SearchResult>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// ADVANCED: Complex search with regex and scope control
    /// ENTERPRISE: Full-featured search for power users
    /// </summary>
    internal async Task<Result<SearchResult>> AdvancedSearchAsync(
        AdvancedSearchCriteria criteria,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (criteria == null)
            return Result<SearchResult>.Failure("Search criteria cannot be null");

        var effectiveTimeout = timeout ?? criteria.Timeout ?? TimeSpan.FromSeconds(60);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 SEARCH: Starting advanced search with criteria - Text: '{SearchText}', Regex: {UseRegex}, Scope: {Scope}", 
                    criteria.SearchText, criteria.UseRegex, criteria.Scope);

                var result = await _searchService.AdvancedSearchAsync(criteria, effectiveTimeout);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ SEARCH: Advanced search completed - {TotalMatches} matches in {Duration}ms using {Algorithm}", 
                        result.Value?.TotalMatches ?? 0,
                        result.Value?.SearchDuration.TotalMilliseconds ?? 0,
                        criteria.Algorithm);
                }
                else
                {
                    _logger.LogWarning("❌ SEARCH: Advanced search failed: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Advanced search operation failed for criteria '{criteria.SearchText}'";
            _logger.LogError(ex, "💥 SEARCH: {ErrorMessage}", errorMessage);
            return Result<SearchResult>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// PERFORMANCE: Clear search results and reset state
    /// MEMORY: Efficient cleanup for long-running sessions
    /// </summary>
    internal async Task<Result<bool>> ClearSearchAsync(TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 SEARCH: Clearing search results");

                var result = await _searchService.ClearSearchAsync();
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ SEARCH: Search results cleared successfully");
                }
                else
                {
                    _logger.LogWarning("❌ SEARCH: Failed to clear search results: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to clear search results";
            _logger.LogError(ex, "💥 SEARCH: {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// NAVIGATION: Navigate to next/previous search result
    /// UX: Seamless result navigation
    /// </summary>
    internal async Task<Result<SearchNavigationResult>> NavigateSearchResultAsync(
        SearchNavigationDirection direction,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 SEARCH: Navigating search results - Direction: {Direction}", direction);

                var result = await _searchService.NavigateSearchResultAsync(direction);
                
                if (result.IsSuccess && result.Value != null)
                {
                    _logger.LogInformation("✅ SEARCH: Navigated to result {CurrentIndex} of {TotalMatches}", 
                        result.Value.CurrentPosition + 1, result.Value.TotalResults);
                }
                else
                {
                    _logger.LogWarning("❌ SEARCH: Navigation failed: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to navigate search results in direction '{direction}'";
            _logger.LogError(ex, "💥 SEARCH: {ErrorMessage}", errorMessage);
            return Result<SearchNavigationResult>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// ENTERPRISE: Get search statistics and performance metrics
    /// MONITORING: Search performance analysis
    /// </summary>
    internal async Task<Result<SearchStatistics>> GetSearchStatisticsAsync(TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 SEARCH: Getting search statistics");

                var result = await _searchService.GetSearchStatisticsAsync();
                
                if (result.IsSuccess && result.Value != null)
                {
                    _logger.LogInformation("✅ SEARCH: Retrieved search statistics - Total searches: {TotalSearches}, Avg time: {AvgTime}ms", 
                        result.Value.TotalSearches, result.Value.AverageSearchTime.TotalMilliseconds);
                }
                else
                {
                    _logger.LogWarning("❌ SEARCH: Failed to get search statistics: {Error}", result.Error);
                }
                
                return result;
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to get search statistics";
            _logger.LogError(ex, "💥 SEARCH: {ErrorMessage}", errorMessage);
            return Result<SearchStatistics>.Failure(errorMessage, ex);
        }
    }

    #endregion
}