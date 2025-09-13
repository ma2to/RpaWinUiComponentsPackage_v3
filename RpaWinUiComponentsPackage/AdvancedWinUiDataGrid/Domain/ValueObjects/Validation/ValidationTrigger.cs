using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;


/// <summary>
/// ENTERPRISE: Validation trigger enumeration per documentation
/// </summary>
internal enum ValidationTrigger
{
    /// <summary>No automatic validation</summary>
    None,
    
    /// <summary>Validate on cell change</summary>
    CellChange,
    
    /// <summary>Validate on any change</summary>
    OnChange,
    
    /// <summary>Validate on row change</summary>
    RowChange,
    
    /// <summary>Validate on save</summary>
    Save,
    
    /// <summary>Validate in real-time</summary>
    RealTime,
    
    /// <summary>Validate on import</summary>
    Import,
    
    /// <summary>Validate on export</summary>
    Export
}


