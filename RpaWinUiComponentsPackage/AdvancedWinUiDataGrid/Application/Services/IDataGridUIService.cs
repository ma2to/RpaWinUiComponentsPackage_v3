using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;

/// <summary>
/// ENTERPRISE: UI service interface for DataGrid UI operations
/// APPLICATION: Application layer UI abstraction
/// </summary>
internal interface IDataGridUIService : IDisposable
{
    /// <summary>
    /// REFRESH: Update UI display after data changes
    /// </summary>
    Task<Result<bool>> RefreshAsync();
    
    /// <summary>
    /// INVALIDATE: Force UI redraw
    /// </summary>
    Task<Result<bool>> InvalidateAsync();
    
    /// <summary>
    /// SCROLL: Scroll to specific row
    /// </summary>
    Task<Result<bool>> ScrollToRowAsync(int rowIndex);
    
    /// <summary>
    /// FOCUS: Set focus to specific cell
    /// </summary>
    Task<Result<bool>> FocusCellAsync(int rowIndex, int columnIndex);
    
    /// <summary>
    /// VALIDATION: Show validation feedback for a specific cell
    /// </summary>
    Task<Result<bool>> ShowCellValidationFeedbackAsync(int rowIndex, int columnIndex, string message);
    
    /// <summary>
    /// ROW HEIGHT: Update row height for auto-sizing based on content
    /// PERFORMANCE: Efficient row height adjustment with caching
    /// </summary>
    Task<Result<bool>> UpdateRowHeightAsync(int rowIndex, double newHeight);
    
    /// <summary>
    /// ROW HEIGHT: Refresh all row heights based on current content
    /// LAYOUT: Recalculate and apply optimal heights for all visible rows
    /// </summary>
    Task<Result<Dictionary<int, double>>> RefreshAllRowHeightsAsync();
    
    /// <summary>
    /// ROW HEIGHT: Enable or disable auto row height adjustment
    /// CONFIGURATION: Runtime toggle for auto-sizing behavior
    /// </summary>
    Task<Result<bool>> SetAutoRowHeightEnabledAsync(bool enabled);
}