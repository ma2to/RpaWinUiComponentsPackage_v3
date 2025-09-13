namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;

/// <summary>
/// DDD: Value object defining sort directions for data grid columns
/// ENTERPRISE: Standard sorting behavior enumeration
/// </summary>
internal enum SortDirection
{
    /// <summary>No sorting applied</summary>
    None,
    
    /// <summary>Ascending sort (A-Z, 0-9)</summary>
    Ascending,
    
    /// <summary>Descending sort (Z-A, 9-0)</summary>
    Descending
}