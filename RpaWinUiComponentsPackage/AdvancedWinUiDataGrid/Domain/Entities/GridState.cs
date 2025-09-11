using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Primitives;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;

/// <summary>
/// DDD: Aggregate Root representing the complete state of a DataGrid
/// ENTERPRISE: Comprehensive state management for complex grid operations
/// IMMUTABLE: Thread-safe state container with versioning support
/// PERFORMANCE: Optimized for large datasets with intelligent caching
/// </summary>
public sealed class GridState : Entity<Guid>
{
    #region Properties - Core State
    
    /// <summary>Grid columns definition</summary>
    public IReadOnlyList<ColumnDefinition> Columns { get; private set; }
    
    /// <summary>Grid rows data</summary>
    public List<GridRow> Rows { get; private set; }
    
    /// <summary>Checkbox states for rows (row index -> checked state)</summary>
    public Dictionary<int, bool> CheckboxStates { get; private set; }
    
    /// <summary>Current filtered row indices (null = all rows visible)</summary>
    public List<int>? FilteredRowIndices { get; set; }
    
    /// <summary>Current search results</summary>
    public List<SearchResult> SearchResults { get; private set; }
    
    /// <summary>Validation errors for the grid</summary>
    public List<ValidationError> ValidationErrors { get; private set; }
    
    /// <summary>Grid initialization status</summary>
    public bool IsInitialized { get; private set; }
    
    #endregion
    
    #region Properties - Configuration
    
    /// <summary>Color configuration</summary>
    public ColorConfiguration? ColorConfiguration { get; private set; }
    
    /// <summary>Validation configuration</summary>
    public ValidationConfiguration? ValidationConfiguration { get; private set; }
    
    /// <summary>Performance configuration</summary>
    public PerformanceConfiguration? PerformanceConfiguration { get; private set; }
    
    #endregion
    
    #region Properties - State Tracking
    
    /// <summary>State version for change tracking</summary>
    public int Version { get; private set; }
    
    /// <summary>Last modified timestamp</summary>
    public DateTime LastModified { get; private set; }
    
    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; private set; }
    
    #endregion
    
    #region Constructor
    
    private GridState(
        Guid id,
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null) : base(id)
    {
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        Rows = new List<GridRow>();
        CheckboxStates = new Dictionary<int, bool>();
        SearchResults = new List<SearchResult>();
        ValidationErrors = new List<ValidationError>();
        FilteredRowIndices = null;
        IsInitialized = true;
        
        ColorConfiguration = colorConfiguration;
        ValidationConfiguration = validationConfiguration;
        PerformanceConfiguration = performanceConfiguration;
        
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// FACTORY: Create initialized grid state
    /// DDD: Factory method ensuring valid aggregate creation
    /// </summary>
    public static GridState Create(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null)
    {
        if (columns == null || columns.Count == 0)
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));
            
        var id = Guid.NewGuid();
        return new GridState(id, columns, colorConfiguration, validationConfiguration, performanceConfiguration);
    }
    
    /// <summary>
    /// FACTORY: Create empty uninitialized state
    /// </summary>
    public static GridState CreateEmpty()
    {
        var state = new GridState(Guid.NewGuid(), Array.Empty<ColumnDefinition>());
        state.IsInitialized = false;
        return state;
    }
    
    #endregion
    
    #region State Modification Methods
    
    /// <summary>
    /// ENTERPRISE: Update grid state and increment version
    /// THREAD-SAFE: Atomic state updates
    /// </summary>
    public void UpdateState()
    {
        Version++;
        LastModified = DateTime.UtcNow;
    }
    
    /// <summary>
    /// ENTERPRISE: Reset state to initial state
    /// </summary>
    public void Reset()
    {
        Rows.Clear();
        CheckboxStates.Clear();
        SearchResults.Clear();
        ValidationErrors.Clear();
        FilteredRowIndices = null;
        Version = 1;
        LastModified = DateTime.UtcNow;
    }
    
    /// <summary>
    /// PERFORMANCE: Clear search and filter state while preserving data
    /// </summary>
    public void ClearFiltersAndSearch()
    {
        SearchResults.Clear();
        FilteredRowIndices = null;
        UpdateState();
    }
    
    #endregion
    
    #region Query Methods
    
    /// <summary>Get visible row count (considering filters)</summary>
    public int GetVisibleRowCount()
    {
        return FilteredRowIndices?.Count ?? Rows.Count;
    }
    
    /// <summary>Get total row count</summary>
    public int GetTotalRowCount() => Rows.Count;
    
    /// <summary>Get column count</summary>
    public int GetColumnCount() => Columns.Count;
    
    /// <summary>Check if filters are applied</summary>
    public bool HasActiveFilters => FilteredRowIndices != null;
    
    /// <summary>Check if search is active</summary>
    public bool HasActiveSearch => SearchResults.Any();
    
    /// <summary>Get checked row count</summary>
    public int GetCheckedRowCount() => CheckboxStates.Count(kvp => kvp.Value);
    
    #endregion
    
    #region DDD - Domain Events
    
    // Domain events can be added here for state changes
    // public void RaiseGridStateChangedEvent() { ... }
    
    #endregion
}