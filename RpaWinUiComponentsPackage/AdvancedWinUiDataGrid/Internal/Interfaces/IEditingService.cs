using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;

/// <summary>
/// Professional editing service interface
/// Handles cell editing operations with validation
/// </summary>
internal interface IEditingService : IDisposable
{
    /// <summary>
    /// Start editing a cell
    /// </summary>
    Task<bool> StartEditAsync(int row, int column);
    
    /// <summary>
    /// End editing and commit changes
    /// </summary>
    Task<bool> EndEditAsync(bool saveChanges = true);
    
    /// <summary>
    /// Cancel current edit operation
    /// </summary>
    void CancelEdit();
    
    /// <summary>
    /// Check if currently in edit mode
    /// </summary>
    bool IsInEditMode { get; }
    
    /// <summary>
    /// Get current editing cell position
    /// </summary>
    (int Row, int Column)? CurrentEditingCell { get; }
    
    /// <summary>
    /// Update cell value with validation
    /// </summary>
    Task<bool> UpdateCellValueAsync(int row, int column, object? value);
    
    /// <summary>
    /// Enable/disable real-time validation
    /// </summary>
    bool EnableRealtimeValidation { get; set; }
    
    // Events
    event EventHandler<CellEditStartedEventArgs>? EditStarted;
    event EventHandler<CellEditEndedEventArgs>? EditEnded;
    event EventHandler<CellValueChangedEventArgs>? ValueChanged;
}

/// <summary>
/// Event arguments for editing operations
/// </summary>
internal record CellEditStartedEventArgs(int Row, int Column, object? OriginalValue);
internal record CellEditEndedEventArgs(int Row, int Column, object? NewValue, bool WasCancelled);
internal record CellValueChangedEventArgs(int Row, int Column, object? OldValue, object? NewValue);