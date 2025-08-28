using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Core;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI;

/// <summary>
/// PROFESSIONAL UI Manager for AdvancedDataGrid
/// RESPONSIBILITY: Handle ONLY UI-related operations, separated from business logic
/// SEPARATION: Pure UI layer - no business logic, no data validation, no data operations
/// ANTI-GOD: Focused, single-responsibility UI management
/// </summary>
internal sealed class DataGridUIManager : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private readonly StackPanel _headersPanel;
    private readonly StackPanel _dataRowsPanel;
    private readonly ScrollViewer _mainScrollViewer;
    private readonly Border _fallbackOverlay;
    private bool _disposed = false;

    public DataGridUIManager(
        ILogger? logger, 
        GlobalExceptionHandler exceptionHandler,
        StackPanel headersPanel,
        StackPanel dataRowsPanel,
        ScrollViewer mainScrollViewer,
        Border fallbackOverlay)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        _headersPanel = headersPanel ?? throw new ArgumentNullException(nameof(headersPanel));
        _dataRowsPanel = dataRowsPanel ?? throw new ArgumentNullException(nameof(dataRowsPanel));
        _mainScrollViewer = mainScrollViewer ?? throw new ArgumentNullException(nameof(mainScrollViewer));
        _fallbackOverlay = fallbackOverlay ?? throw new ArgumentNullException(nameof(fallbackOverlay));
        
        _logger?.Info("🎨 UI MANAGER: Initialized DataGridUIManager");
    }

    /// <summary>
    /// Generate UI elements with comprehensive error handling and logging
    /// PURE UI: Only creates visual elements, no business logic
    /// </summary>
    public async Task GenerateUIElementsAsync(DataGridCoordinator coordinator)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎨 UI GENERATION: Starting direct UI element generation");
            
            // DETAILED LOGGING: Log current state before generation
            var rowCount = coordinator?.DataRows.Count ?? 0;
            var headerCount = coordinator?.Headers.Count ?? 0;
            var currentHeadersCount = _headersPanel.Children.Count;
            var currentRowsCount = _dataRowsPanel.Children.Count;
            
            _logger?.Info("📊 UI GENERATION STATE: Before - Headers: {CurrentHeaders}/{ExpectedHeaders}, Rows: {CurrentRows}/{ExpectedRows}",
                currentHeadersCount, headerCount, currentRowsCount, rowCount);
            
            // Clear existing elements
            _logger?.Info("🧹 UI GENERATION: Clearing existing UI elements");
            ClearUIElements();
            
            // Generate headers
            _logger?.Info("📋 UI GENERATION: Starting header generation");
            await GenerateHeadersAsync(coordinator);
            _logger?.Info("✅ UI GENERATION: Header generation completed - Generated: {HeadersGenerated}", _headersPanel.Children.Count);
            
            // Generate data rows
            _logger?.Info("📝 UI GENERATION: Starting data row generation");
            await GenerateDataRowsAsync(coordinator);
            _logger?.Info("✅ UI GENERATION: Data row generation completed - Generated: {RowsGenerated}", _dataRowsPanel.Children.Count);
            
            // Update scroll viewer
            _logger?.Info("🔄 UI GENERATION: Updating scroll viewer content");
            await UpdateScrollViewerContentAsync();
            _logger?.Info("✅ UI GENERATION: Scroll viewer content updated");
            
            // DETAILED LOGGING: Log final state after generation
            _logger?.Info("📊 UI GENERATION RESULT: After - Headers: {FinalHeaders}, Rows: {FinalRows}, Success: {Success}",
                _headersPanel.Children.Count, _dataRowsPanel.Children.Count, true);
                
        }, "GenerateUIElements", _logger);
    }

    /// <summary>
    /// Clear all UI elements safely
    /// PURE UI: Only clears visual elements
    /// </summary>
    public void ClearUIElements()
    {
        _exceptionHandler.SafeExecuteUI(() =>
        {
            _headersPanel.Children.Clear();
            _dataRowsPanel.Children.Clear();
            _logger?.Info("🧹 UI CLEAR: All UI elements cleared");
        }, "ClearUIElements", _logger);
    }

    /// <summary>
    /// Show or hide fallback overlay
    /// PURE UI: Only controls visibility
    /// </summary>
    public void SetFallbackVisibility(bool visible)
    {
        _exceptionHandler.SafeExecuteUI(() =>
        {
            _fallbackOverlay.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            _logger?.Info("🎭 FALLBACK: Set visibility to {Visibility}", visible ? "Visible" : "Collapsed");
        }, "SetFallbackVisibility", _logger);
    }

    /// <summary>
    /// Apply validation styling to cell border
    /// PURE UI: Only applies visual styling
    /// </summary>
    public void ApplyValidationStyling(Border cellBorder, bool isValid, string? errorMessage, ColorConfiguration? colorConfig)
    {
        _exceptionHandler.SafeExecuteUI(() =>
        {
            try
            {
                if (isValid)
                {
                    // Apply valid styling
                    var normalBorder = colorConfig?.GetEffectiveCellBorder() ?? "#E0E0E0";
                    cellBorder.BorderBrush = new SolidColorBrush(ParseColor(normalBorder));
                    cellBorder.BorderThickness = new Thickness(1);
                    
                    // Remove tooltip
                    ToolTipService.SetToolTip(cellBorder, null);
                    
                    _logger?.Info("🎨 VALIDATION STYLING: Applied VALID styling");
                }
                else
                {
                    // Apply error styling
                    var errorBorder = colorConfig?.GetEffectiveValidationErrorBorder() ?? "#FF0000";
                    cellBorder.BorderBrush = new SolidColorBrush(ParseColor(errorBorder));
                    cellBorder.BorderThickness = new Thickness(2);
                    
                    // Add error tooltip
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        var tooltip = new ToolTip { Content = errorMessage };
                        ToolTipService.SetToolTip(cellBorder, tooltip);
                    }
                    
                    _logger?.Info("🎨 VALIDATION STYLING: Applied ERROR styling - {ErrorMessage}", errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "🚨 VALIDATION STYLING ERROR: Failed to apply styling");
            }
        }, "ApplyValidationStyling", _logger);
    }

    /// <summary>
    /// Apply selection styling to cell
    /// PURE UI: Only visual selection feedback
    /// </summary>
    public void ApplySelectionStyling(Border cellBorder, bool isSelected, ColorConfiguration? colorConfig)
    {
        _exceptionHandler.SafeExecuteUI(() =>
        {
            try
            {
                if (isSelected)
                {
                    var selectionBackground = colorConfig?.GetEffectiveSelectionBackground() ?? "#0078D4";
                    var selectionForeground = colorConfig?.SelectionForeground ?? "#FFFFFF";
                    
                    cellBorder.Background = new SolidColorBrush(ParseColor(selectionBackground));
                    
                    // Update text color
                    if (cellBorder.Child is Grid cellGrid)
                    {
                        var textBlock = cellGrid.Children.OfType<TextBlock>().FirstOrDefault();
                        if (textBlock != null)
                        {
                            textBlock.Foreground = new SolidColorBrush(ParseColor(selectionForeground));
                        }
                    }
                    
                    _logger?.Info("🎨 SELECTION STYLING: Applied SELECTED styling");
                }
                else
                {
                    // Reset to normal appearance
                    var normalBackground = colorConfig?.GetEffectiveCellBackground() ?? "#FFFFFF";
                    var normalForeground = colorConfig?.GetEffectiveCellForeground() ?? "#000000";
                    
                    cellBorder.Background = new SolidColorBrush(ParseColor(normalBackground));
                    
                    // Reset text color
                    if (cellBorder.Child is Grid cellGrid)
                    {
                        var textBlock = cellGrid.Children.OfType<TextBlock>().FirstOrDefault();
                        if (textBlock != null)
                        {
                            textBlock.Foreground = new SolidColorBrush(ParseColor(normalForeground));
                        }
                    }
                    
                    _logger?.Info("🎨 SELECTION STYLING: Reset to normal styling");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "🚨 SELECTION STYLING ERROR: Failed to apply styling");
            }
        }, "ApplySelectionStyling", _logger);
    }

    private async Task GenerateHeadersAsync(DataGridCoordinator coordinator)
    {
        // TODO: Move header generation logic here from AdvancedDataGrid.xaml.cs
        await Task.CompletedTask;
    }

    private async Task GenerateDataRowsAsync(DataGridCoordinator coordinator)
    {
        // TODO: Move data row generation logic here from AdvancedDataGrid.xaml.cs
        await Task.CompletedTask;
    }

    private async Task UpdateScrollViewerContentAsync()
    {
        // TODO: Move scroll viewer update logic here from AdvancedDataGrid.xaml.cs
        await Task.CompletedTask;
    }

    private static Color ParseColor(string colorString)
    {
        try
        {
            colorString = colorString.TrimStart('#');
            if (colorString.Length == 6)
            {
                byte r = Convert.ToByte(colorString.Substring(0, 2), 16);
                byte g = Convert.ToByte(colorString.Substring(2, 2), 16);
                byte b = Convert.ToByte(colorString.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }
            return Microsoft.UI.Colors.Gray;
        }
        catch
        {
            return Microsoft.UI.Colors.Gray;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 UI MANAGER DISPOSE: Cleaning up DataGridUIManager");
            _disposed = true;
        }
    }
}