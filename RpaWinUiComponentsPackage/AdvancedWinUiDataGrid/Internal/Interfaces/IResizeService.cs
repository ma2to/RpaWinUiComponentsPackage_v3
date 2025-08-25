namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;

/// <summary>
/// Professional resize service interface
/// Handles column/row resizing operations
/// </summary>
internal interface IResizeService : IDisposable
{
    /// <summary>
    /// Resize column to specific width
    /// </summary>
    void ResizeColumn(int columnIndex, double width);
    
    /// <summary>
    /// Resize row to specific height
    /// </summary>
    void ResizeRow(int rowIndex, double height);
    
    /// <summary>
    /// Auto-resize column to fit content
    /// </summary>
    void AutoResizeColumn(int columnIndex);
    
    /// <summary>
    /// Auto-resize all columns to fit content
    /// </summary>
    void AutoResizeAllColumns();
    
    /// <summary>
    /// Get column width
    /// </summary>
    double GetColumnWidth(int columnIndex);
    
    /// <summary>
    /// Get row height
    /// </summary>
    double GetRowHeight(int rowIndex);
    
    /// <summary>
    /// Enable/disable column resizing
    /// </summary>
    bool AllowColumnResize { get; set; }
    
    /// <summary>
    /// Enable/disable row resizing
    /// </summary>
    bool AllowRowResize { get; set; }
    
    /// <summary>
    /// Minimum column width
    /// </summary>
    double MinColumnWidth { get; set; }
    
    /// <summary>
    /// Maximum column width
    /// </summary>
    double MaxColumnWidth { get; set; }
    
    // Events
    event EventHandler<ColumnResizedEventArgs>? ColumnResized;
    event EventHandler<RowResizedEventArgs>? RowResized;
}

/// <summary>
/// Event arguments for resize operations
/// </summary>
internal record ColumnResizedEventArgs(int ColumnIndex, double OldWidth, double NewWidth);
internal record RowResizedEventArgs(int RowIndex, double OldHeight, double NewHeight);