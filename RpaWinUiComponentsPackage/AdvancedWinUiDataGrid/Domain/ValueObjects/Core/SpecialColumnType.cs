namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;

/// <summary>
/// DDD: Value object defining special column types for data grid
/// ENTERPRISE: Support for specialized column behaviors
/// </summary>
internal enum SpecialColumnType
{
    /// <summary>Regular data column</summary>
    None,
    
    /// <summary>Checkbox column for boolean values</summary>
    CheckBox,
    
    /// <summary>Delete row action column</summary>
    DeleteRow,
    
    /// <summary>Validation alerts display column</summary>
    ValidAlerts,
    
    /// <summary>Row number display column</summary>
    RowNumber
}