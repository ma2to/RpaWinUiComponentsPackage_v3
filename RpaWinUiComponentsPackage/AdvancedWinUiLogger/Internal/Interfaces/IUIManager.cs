using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Interfaces;

/// <summary>
/// Interface for Logger UI management operations
/// Handles WinUI3 visual component interactions for log viewing
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
    Task<Result<bool>> InitializeAsync(LoggerConfiguration config);
    
    #endregion
    
    #region UI Operations
    
    /// <summary>
    /// Refresh log information in UI
    /// </summary>
    Task RefreshLogInfoAsync();
    
    /// <summary>
    /// Show log file content in UI
    /// </summary>
    Task ShowLogFileAsync(string filePath);
    
    #endregion
}