using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Coordination;

/// <summary>
/// PROFESSIONAL Interaction Coordinator - ONLY interaction timing and state management
/// RESPONSIBILITY: Handle click timing, modifier keys, interaction patterns (NO UI operations, NO business logic)
/// SEPARATION: Pure interaction state - timing patterns, click detection
/// ANTI-GOD: Single responsibility - only interaction coordination
/// </summary>
internal sealed class InteractionCoordinator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private bool _disposed = false;

    // INTERACTION TIMING STATE (Immutable pattern)
    private readonly record struct InteractionState(
        DateTime LastClickTime,
        DataGridCell? LastClickedCell,
        bool IsCtrlPressed,
        bool IsShiftPressed,
        bool IsAltPressed,
        bool GridHasFocus,
        FrameworkElement? LastFocusedElement
    );

    private InteractionState _state;
    private const int DoubleClickThresholdMs = 500;

    // INTERACTION EVENTS (Functional pattern)
    public event EventHandler<InteractionAnalysisResult>? InteractionAnalyzed;

    public InteractionCoordinator(
        ILogger? logger, 
        GlobalExceptionHandler exceptionHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        
        _state = new InteractionState(
            DateTime.MinValue,
            null,
            false,
            false, 
            false,
            false,
            null
        );
        
        _logger?.Info("🖱️ INTERACTION COORDINATOR: Initialized - Pure interaction state only");
    }

    // PUBLIC READ-ONLY PROPERTIES (Immutable exposure)
    public bool IsCtrlPressed => _state.IsCtrlPressed;
    public bool IsShiftPressed => _state.IsShiftPressed;
    public bool IsAltPressed => _state.IsAltPressed;
    public bool GridHasFocus => _state.GridHasFocus;
    public FrameworkElement? LastFocusedElement => _state.LastFocusedElement;
    public (bool Ctrl, bool Shift, bool Alt) ModifierKeys => (_state.IsCtrlPressed, _state.IsShiftPressed, _state.IsAltPressed);

    /// <summary>
    /// Update modifier key states from system
    /// PURE INTERACTION: Only updates modifier state, no side effects
    /// </summary>
    public async Task<Result<bool>> UpdateModifierKeyStatesAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            try
            {
                var keyboardState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
                var isCtrlPressed = keyboardState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                keyboardState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
                var isShiftPressed = keyboardState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                keyboardState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);
                var isAltPressed = keyboardState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                // Immutable update
                _state = _state with 
                { 
                    IsCtrlPressed = isCtrlPressed,
                    IsShiftPressed = isShiftPressed,
                    IsAltPressed = isAltPressed
                };

                _logger?.Info("⌨️ MODIFIER UPDATE: Ctrl: {Ctrl}, Shift: {Shift}, Alt: {Alt}", 
                    isCtrlPressed, isShiftPressed, isAltPressed);

                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "🚨 MODIFIER UPDATE ERROR: Failed to update modifier key states");
                return false;
            }
            
        }, "UpdateModifierKeyStates", 1, false, _logger);
    }

    /// <summary>
    /// Analyze cell click interaction for timing patterns
    /// PURE INTERACTION: Only analyzes timing and patterns, no business operations
    /// </summary>
    public async Task<Result<InteractionAnalysisResult>> AnalyzeCellClickAsync(DataGridCell cell, DateTime clickTime)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🖱️ CLICK ANALYSIS: Analyzing click on cell {CellId}", cell.CellId);
            
            var isDoubleClick = (_state.LastClickedCell == cell) && 
                               (clickTime - _state.LastClickTime).TotalMilliseconds < DoubleClickThresholdMs;

            var timeSinceLastClick = clickTime - _state.LastClickTime;
            
            // Immutable update
            _state = _state with 
            { 
                LastClickTime = clickTime,
                LastClickedCell = cell
            };

            var result = new InteractionAnalysisResult(
                Cell: cell,
                ClickTime: clickTime,
                IsDoubleClick: isDoubleClick,
                TimeSinceLastClick: timeSinceLastClick,
                ModifierKeys: (_state.IsCtrlPressed, _state.IsShiftPressed, _state.IsAltPressed),
                InteractionType: isDoubleClick ? "DoubleClick" : "SingleClick"
            );
            
            _logger?.Info("🖱️ CLICK ANALYSIS: {InteractionType} detected - TimeSince: {TimeSince}ms", 
                result.InteractionType, (int)timeSinceLastClick.TotalMilliseconds);
            
            // Notify subscribers (functional pattern)
            OnInteractionAnalyzed(result);
            
            await Task.CompletedTask;
            return result;
            
        }, "AnalyzeCellClick", 1, new InteractionAnalysisResult(cell, clickTime, false, TimeSpan.Zero, (false, false, false), "SingleClick"), _logger);
    }

    /// <summary>
    /// Update focus state
    /// PURE INTERACTION: Only updates focus tracking, no UI operations
    /// </summary>
    public async Task<Result<bool>> UpdateFocusStateAsync(bool hasFocus, FrameworkElement? focusedElement = null)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            var previousFocus = _state.GridHasFocus;
            
            // Immutable update
            _state = _state with 
            { 
                GridHasFocus = hasFocus,
                LastFocusedElement = focusedElement ?? _state.LastFocusedElement
            };
            
            _logger?.Info("🎯 FOCUS UPDATE: Focus changed from {Previous} to {Current}", previousFocus, hasFocus);
            
            await Task.CompletedTask;
            return true;
            
        }, "UpdateFocusState", 1, false, _logger);
    }

    /// <summary>
    /// Simulate interaction for testing
    /// PURE INTERACTION: Only simulates timing patterns for analysis
    /// </summary>
    public async Task<Result<InteractionAnalysisResult>> SimulateInteractionAsync(DataGridCell cell, bool isDoubleClick = false)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🎮 SIMULATE: Simulating {InteractionType} on cell {CellId}", 
                isDoubleClick ? "double-click" : "single-click", cell.CellId);
            
            var simulatedTime = DateTime.Now;
            
            if (isDoubleClick)
            {
                // Simulate first click
                await AnalyzeCellClickAsync(cell, simulatedTime.AddMilliseconds(-100));
                
                // Simulate second click within threshold
                return await AnalyzeCellClickAsync(cell, simulatedTime);
            }
            else
            {
                return await AnalyzeCellClickAsync(cell, simulatedTime);
            }
            
        }, "SimulateInteraction", 1, new InteractionAnalysisResult(cell, DateTime.Now, false, TimeSpan.Zero, (false, false, false), "SingleClick"), _logger);
    }

    /// <summary>
    /// Get interaction statistics
    /// PURE INTERACTION: Only reports interaction state and statistics
    /// </summary>
    public async Task<Result<InteractionStatistics>> GetInteractionStatisticsAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            var timeSinceLastClick = DateTime.Now - _state.LastClickTime;
            var hasRecentActivity = timeSinceLastClick.TotalSeconds < 30;
            
            var stats = new InteractionStatistics(
                LastClickTime: _state.LastClickTime,
                TimeSinceLastClick: timeSinceLastClick,
                HasRecentActivity: hasRecentActivity,
                LastClickedCellId: _state.LastClickedCell?.CellId,
                CurrentModifierKeys: (_state.IsCtrlPressed, _state.IsShiftPressed, _state.IsAltPressed),
                GridHasFocus: _state.GridHasFocus
            );
            
            _logger?.Info("📊 INTERACTION STATS: Recent: {Recent}, TimeSince: {TimeSince}s, Focus: {Focus}",
                stats.HasRecentActivity, (int)stats.TimeSinceLastClick.TotalSeconds, stats.GridHasFocus);
            
            await Task.CompletedTask;
            return stats;
            
        }, "GetInteractionStatistics", 1, new InteractionStatistics(DateTime.MinValue, TimeSpan.Zero, false, null, (false, false, false), false), _logger);
    }

    /// <summary>
    /// Reset interaction state
    /// PURE INTERACTION: Only clears interaction timing state
    /// </summary>
    public async Task<Result<bool>> ResetInteractionStateAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔄 INTERACTION RESET: Resetting interaction state");
            
            _state = new InteractionState(
                DateTime.MinValue,
                null,
                _state.IsCtrlPressed, // Keep modifier keys
                _state.IsShiftPressed,
                _state.IsAltPressed,
                _state.GridHasFocus, // Keep focus state
                _state.LastFocusedElement
            );
            
            _logger?.Info("✅ INTERACTION RESET: Interaction state reset successfully");
            
            await Task.CompletedTask;
            return true;
            
        }, "ResetInteractionState", 1, false, _logger);
    }

    /// <summary>
    /// Check if interaction should be treated as double-click
    /// PURE INTERACTION: Only timing analysis, no side effects
    /// </summary>
    public bool IsWithinDoubleClickThreshold(DateTime currentTime, DateTime previousTime)
    {
        var timeDiff = (currentTime - previousTime).TotalMilliseconds;
        return timeDiff > 0 && timeDiff < DoubleClickThresholdMs;
    }

    /// <summary>
    /// Get current double-click threshold
    /// PURE INTERACTION: Only returns configuration values
    /// </summary>
    public int GetDoubleClickThresholdMs() => DoubleClickThresholdMs;

    #region Event Raising (Functional pattern)

    private void OnInteractionAnalyzed(InteractionAnalysisResult result)
    {
        try
        {
            InteractionAnalyzed?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 EVENT ERROR: Error raising InteractionAnalyzed event");
        }
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 INTERACTION COORDINATOR DISPOSE: Cleaning up interaction state");
            
            // Clear event subscribers
            InteractionAnalyzed = null;
            
            _disposed = true;
            _logger?.Info("✅ INTERACTION COORDINATOR DISPOSE: Disposed successfully");
        }
    }
}

/// <summary>
/// Interaction analysis result record
/// </summary>
internal record InteractionAnalysisResult(
    DataGridCell Cell,
    DateTime ClickTime,
    bool IsDoubleClick,
    TimeSpan TimeSinceLastClick,
    (bool Ctrl, bool Shift, bool Alt) ModifierKeys,
    string InteractionType
);

/// <summary>
/// Interaction statistics record
/// </summary>
internal record InteractionStatistics(
    DateTime LastClickTime,
    TimeSpan TimeSinceLastClick,
    bool HasRecentActivity,
    string? LastClickedCellId,
    (bool Ctrl, bool Shift, bool Alt) CurrentModifierKeys,
    bool GridHasFocus
);