using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;

/// <summary>
/// Interface for DataGrid UI management operations
/// Handles WinUI3 visual component interactions
/// </summary>
public interface IUIManager : IDisposable
{
    #region Properties
    
    /// <summary>
    /// Is UI manager initialized and ready
    /// </summary>
    bool IsInitialized { get; }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize UI manager with configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridConfiguration? config = null);
    
    #endregion
    
    #region UI Operations
    
    /// <summary>
    /// Refresh the visual data display
    /// </summary>
    Task RefreshDataAsync();
    
    /// <summary>
    /// Highlight search results in UI
    /// </summary>
    Task HighlightSearchResultsAsync(SearchResult searchResult);
    
    /// <summary>
    /// Update validation results in UI
    /// </summary>
    Task UpdateValidationResultsAsync(RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult validationResult);
    
    /// <summary>
    /// Apply color theme to the UI
    /// </summary>
    Task ApplyColorThemeAsync(ColorTheme theme);
    
    #endregion
}