namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;

/// <summary>
/// Professional selection service interface
/// Handles cell/row/column selection operations
/// </summary>
internal interface ISelectionService : IDisposable
{
    /// <summary>
    /// Select single cell
    /// </summary>
    void SelectCell(int row, int column);
    
    /// <summary>
    /// Select range of cells
    /// </summary>
    void SelectRange(int startRow, int startColumn, int endRow, int endColumn);
    
    /// <summary>
    /// Select entire row
    /// </summary>
    void SelectRow(int row);
    
    /// <summary>
    /// Select entire column
    /// </summary>
    void SelectColumn(int column);
    
    /// <summary>
    /// Clear all selections
    /// </summary>
    void ClearSelection();
    
    /// <summary>
    /// Get selected cells
    /// </summary>
    IReadOnlyList<(int Row, int Column)> SelectedCells { get; }
    
    /// <summary>
    /// Get selected rows
    /// </summary>
    IReadOnlyList<int> SelectedRows { get; }
    
    /// <summary>
    /// Get selected columns
    /// </summary>
    IReadOnlyList<int> SelectedColumns { get; }
    
    /// <summary>
    /// Check if cell is selected
    /// </summary>
    bool IsCellSelected(int row, int column);
    
    /// <summary>
    /// Enable multi-selection
    /// </summary>
    bool AllowMultiSelect { get; set; }
    
    /// <summary>
    /// Enable drag selection
    /// </summary>
    bool AllowDragSelection { get; set; }
    
    // Events
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
}

/// <summary>
/// Event arguments for selection operations
/// </summary>
internal record SelectionChangedEventArgs(
    IReadOnlyList<(int Row, int Column)> NewSelection,
    IReadOnlyList<(int Row, int Column)> PreviousSelection);