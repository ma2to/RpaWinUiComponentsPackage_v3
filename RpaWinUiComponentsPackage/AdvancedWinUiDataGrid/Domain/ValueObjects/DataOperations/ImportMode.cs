namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;

/// <summary>
/// DDD: Value object defining import modes for data grid
/// ENTERPRISE: Support for different data import strategies
/// </summary>
internal enum ImportMode
{
    /// <summary>Replace all existing data</summary>
    Replace,
    
    /// <summary>Append to existing data</summary>
    Append,
    
    /// <summary>Merge with existing data based on key columns</summary>
    Merge,
    
    /// <summary>Update existing rows only, ignore new rows</summary>
    Update
}