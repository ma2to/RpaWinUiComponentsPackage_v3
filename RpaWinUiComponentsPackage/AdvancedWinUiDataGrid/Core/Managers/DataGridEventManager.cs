using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Modules.Table.Models;
using RpaWinUiComponentsPackage.Core.Models;
using RpaWinUiComponentsPackage.Core.Extensions;
using System.Collections.ObjectModel;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Managers;

/// <summary>
/// Professional Event Manager - centralized event handling for DataGrid
/// Coordinates between different managers and handles complex event interactions
/// Provides clean separation of concerns for UI events
/// </summary>
internal sealed class DataGridEventManager : IDisposable
{
    #region Private Fields

    private readonly UserControl _parentGrid;
    private readonly ILogger? _logger;
    
    // MANAGER REFERENCES
    private readonly DataGridSelectionManager _selectionManager;
    private readonly DataGridEditingManager _editingManager;
    private readonly DataGridResizeManager _resizeManager;

    // EVENT STATE
    private bool _eventsAttached = false;
    private readonly Dictionary<FrameworkElement, List<(string eventName, Delegate handler)>> _attachedEvents = new();

    // TIMING AND INTERACTION
    private DateTime _lastClickTime = DateTime.MinValue;
    private DataGridCell? _lastClickedCell = null;
    private const int DoubleClickThresholdMs = 500;

    // KEYBOARD STATE
    private bool _isCtrlPressed = false;
    private bool _isShiftPressed = false;
    private bool _isAltPressed = false;

    // FOCUS MANAGEMENT
    private bool _gridHasFocus = false;
    private FrameworkElement? _lastFocusedElement = null;

    // EVENTS
    public event EventHandler<DataGridEventArgs>? GridEvent;
    public event EventHandler<CellInteractionEventArgs>? CellInteraction;
    public event EventHandler<KeyboardEventArgs>? KeyboardInput;

    #endregion

    #region Constructor

    public DataGridEventManager(
        UserControl parentGrid,
        DataGridSelectionManager selectionManager,
        DataGridEditingManager editingManager,
        DataGridResizeManager resizeManager,
        ILogger? logger = null)
    {
        _parentGrid = parentGrid ?? throw new ArgumentNullException(nameof(parentGrid));
        _selectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager));
        _editingManager = editingManager ?? throw new ArgumentNullException(nameof(editingManager));
        _resizeManager = resizeManager ?? throw new ArgumentNullException(nameof(resizeManager));
        _logger = logger;

        AttachEvents();
        _logger?.Info("🔧 EVENT MANAGER INIT: DataGridEventManager initialized");
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Grid has keyboard focus
    /// </summary>
    public bool GridHasFocus => _gridHasFocus;

    /// <summary>
    /// Current keyboard modifier states
    /// </summary>
    public (bool Ctrl, bool Shift, bool Alt) ModifierKeys => (_isCtrlPressed, _isShiftPressed, _isAltPressed);

    /// <summary>
    /// Events are attached to UI elements
    /// </summary>
    public bool EventsAttached => _eventsAttached;

    #endregion

    #region Public Methods - Event Management

    /// <summary>
    /// Attach event handlers to UI elements
    /// </summary>
    public void AttachEvents()
    {
        try
        {
            if (_eventsAttached)
            {
                return;
            }

            // Main grid events
            AttachEvent(_parentGrid, "PointerPressed", new PointerEventHandler(OnGridPointerPressed));
            AttachEvent(_parentGrid, "PointerMoved", new PointerEventHandler(OnGridPointerMoved));
            AttachEvent(_parentGrid, "PointerReleased", new PointerEventHandler(OnGridPointerReleased));
            AttachEvent(_parentGrid, "KeyDown", new KeyEventHandler(OnGridKeyDown));
            AttachEvent(_parentGrid, "KeyUp", new KeyEventHandler(OnGridKeyUp));
            AttachEvent(_parentGrid, "GotFocus", new RoutedEventHandler(OnGridGotFocus));
            AttachEvent(_parentGrid, "LostFocus", new RoutedEventHandler(OnGridLostFocus));
            AttachEvent(_parentGrid, "RightTapped", new RightTappedEventHandler(OnGridRightTapped));
            AttachEvent(_parentGrid, "DoubleTapped", new DoubleTappedEventHandler(OnGridDoubleTapped));

            _eventsAttached = true;
            _logger?.Info("✅ Events attached to DataGrid");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error attaching events");
        }
    }

    /// <summary>
    /// Detach event handlers from UI elements
    /// </summary>
    public void DetachEvents()
    {
        try
        {
            foreach (var kvp in _attachedEvents)
            {
                var element = kvp.Key;
                var eventHandlers = kvp.Value;

                foreach (var (eventName, handler) in eventHandlers)
                {
                    DetachEventByName(element, eventName, handler);
                }
            }

            _attachedEvents.Clear();
            _eventsAttached = false;
            _logger?.Info("🧹 Events detached from DataGrid");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error detaching events");
        }
    }

    /// <summary>
    /// Attach events to cell element
    /// </summary>
    public void AttachCellEvents(FrameworkElement cellElement, DataGridCell cellModel)
    {
        try
        {
            AttachEvent(cellElement, "PointerPressed", new PointerEventHandler((sender, e) => OnCellPointerPressed(cellModel, e)));
            AttachEvent(cellElement, "PointerEntered", new PointerEventHandler((sender, e) => OnCellPointerEntered(cellModel, e)));
            AttachEvent(cellElement, "PointerExited", new PointerEventHandler((sender, e) => OnCellPointerExited(cellModel, e)));
            AttachEvent(cellElement, "RightTapped", new RightTappedEventHandler((sender, e) => OnCellRightTapped(cellModel, e)));
            AttachEvent(cellElement, "DoubleTapped", new DoubleTappedEventHandler((sender, e) => OnCellDoubleTapped(cellModel, e)));

            _logger?.Info("📎 Events attached to cell");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error attaching cell events");
        }
    }

    /// <summary>
    /// Detach events from cell element
    /// </summary>
    public void DetachCellEvents(FrameworkElement cellElement)
    {
        try
        {
            if (_attachedEvents.TryGetValue(cellElement, out var eventHandlers))
            {
                foreach (var (eventName, handler) in eventHandlers)
                {
                    DetachEventByName(cellElement, eventName, handler);
                }
                _attachedEvents.Remove(cellElement);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error detaching cell events");
        }
    }

    #endregion

    #region Public Methods - Interaction Simulation

    /// <summary>
    /// Simulate cell click programmatically
    /// </summary>
    public async Task<bool> SimulateCellClickAsync(int rowIndex, int columnIndex, bool isDoubleClick = false)
    {
        try
        {
            _logger?.Info("🖱️ Simulating cell click at ({Row}, {Column}), double: {Double}", rowIndex, columnIndex, isDoubleClick);

            // Find cell
            var cell = FindCellAt(rowIndex, columnIndex);
            if (cell == null)
            {
                _logger?.Warning("⚠️ Cell not found for simulation at ({Row}, {Column})", rowIndex, columnIndex);
                return false;
            }

            if (isDoubleClick)
            {
                await HandleCellDoubleClickAsync(cell, rowIndex, columnIndex);
            }
            else
            {
                await HandleCellSingleClickAsync(cell, rowIndex, columnIndex);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error simulating cell click");
            return false;
        }
    }

    /// <summary>
    /// Simulate keyboard input programmatically
    /// </summary>
    public async Task<bool> SimulateKeyInputAsync(VirtualKey key, bool ctrl = false, bool shift = false, bool alt = false)
    {
        try
        {
            _logger?.Info("⌨️ Simulating key input: {Key}, Ctrl: {Ctrl}, Shift: {Shift}, Alt: {Alt}", 
                key, ctrl, shift, alt);

            // Temporarily set modifier states
            var oldCtrl = _isCtrlPressed;
            var oldShift = _isShiftPressed;
            var oldAlt = _isAltPressed;

            _isCtrlPressed = ctrl;
            _isShiftPressed = shift;
            _isAltPressed = alt;

            try
            {
                await HandleKeyboardInputAsync(key);
                return true;
            }
            finally
            {
                // Restore original modifier states
                _isCtrlPressed = oldCtrl;
                _isShiftPressed = oldShift;
                _isAltPressed = oldAlt;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error simulating keyboard input");
            return false;
        }
    }

    #endregion

    #region Event Handlers - Grid Level

    private async void OnGridPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            _logger?.Info("🖱️ Grid pointer pressed");
            
            UpdateModifierKeyStates();
            
            // Check if clicking on resize handle
            if (IsPointerOnResizeHandle(e, out var columnIndex))
            {
                _resizeManager.HandleResizeHandlePressed(columnIndex, e);
                return;
            }

            // Find cell under pointer
            var cell = FindCellUnderPointer(e.GetCurrentPoint(_parentGrid).Position);
            if (cell != null)
            {
                await _selectionManager.HandleCellPointerPressedAsync(cell, e);
            }

            OnGridEvent(new DataGridEventArgs("PointerPressed", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid pointer pressed");
        }
    }

    private async void OnGridPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            // Handle resize operation
            if (_resizeManager.IsResizing)
            {
                _resizeManager.HandleResizePointerMoved(e);
                return;
            }

            // Handle selection drag
            await _selectionManager.HandlePointerMovedAsync(e);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid pointer moved");
        }
    }

    private void OnGridPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            _logger?.Info("🖱️ Grid pointer released");

            // Handle resize end
            if (_resizeManager.IsResizing)
            {
                _resizeManager.HandleResizePointerReleased(e);
                return;
            }

            // Handle selection end
            _selectionManager.HandlePointerReleased(e);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid pointer released");
        }
    }

    private async void OnGridKeyDown(object sender, KeyRoutedEventArgs e)
    {
        try
        {
            _logger?.Info("⌨️ Grid key down: {Key}", e.Key);
            
            UpdateModifierKeyStates();
            await HandleKeyboardInputAsync(e.Key);
            
            OnKeyboardInput(new KeyboardEventArgs(e.Key, _isCtrlPressed, _isShiftPressed, _isAltPressed, isKeyDown: true));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid key down");
        }
    }

    private void OnGridKeyUp(object sender, KeyRoutedEventArgs e)
    {
        try
        {
            UpdateModifierKeyStates();
            OnKeyboardInput(new KeyboardEventArgs(e.Key, _isCtrlPressed, _isShiftPressed, _isAltPressed, isKeyDown: false));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid key up");
        }
    }

    private void OnGridGotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            _gridHasFocus = true;
            _lastFocusedElement = sender as FrameworkElement;
            _logger?.Info("🎯 Grid got focus");
            OnGridEvent(new DataGridEventArgs("GotFocus", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid got focus");
        }
    }

    private void OnGridLostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            _gridHasFocus = false;
            _logger?.Info("🌫️ Grid lost focus");
            OnGridEvent(new DataGridEventArgs("LostFocus", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid lost focus");
        }
    }

    private void OnGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        try
        {
            _logger?.Info("🖱️ Grid right tapped");
            // TODO: Show context menu
            OnGridEvent(new DataGridEventArgs("RightTapped", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid right tapped");
        }
    }

    private void OnGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        try
        {
            _logger?.Info("🖱️ Grid double tapped");
            OnGridEvent(new DataGridEventArgs("DoubleTapped", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling grid double tapped");
        }
    }

    #endregion

    #region Event Handlers - Cell Level

    private async void OnCellPointerPressed(DataGridCell cell, PointerRoutedEventArgs e)
    {
        try
        {
            var now = DateTime.Now;
            var isDoubleClick = (_lastClickedCell == cell) && 
                               (now - _lastClickTime).TotalMilliseconds < DoubleClickThresholdMs;

            _lastClickedCell = cell;
            _lastClickTime = now;

            if (isDoubleClick)
            {
                var position = FindCellPosition(cell);
                if (position.HasValue)
                {
                    await HandleCellDoubleClickAsync(cell, position.Value.row, position.Value.column);
                }
            }
            else
            {
                var position = FindCellPosition(cell);
                if (position.HasValue)
                {
                    await HandleCellSingleClickAsync(cell, position.Value.row, position.Value.column);
                }
            }

            OnCellInteraction(new CellInteractionEventArgs(cell, "PointerPressed", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell pointer pressed");
        }
    }

    private void OnCellPointerEntered(DataGridCell cell, PointerRoutedEventArgs e)
    {
        try
        {
            // Visual feedback for hover
            // TODO: Apply hover styling
            OnCellInteraction(new CellInteractionEventArgs(cell, "PointerEntered", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell pointer entered");
        }
    }

    private void OnCellPointerExited(DataGridCell cell, PointerRoutedEventArgs e)
    {
        try
        {
            // Remove hover styling
            // TODO: Remove hover styling
            OnCellInteraction(new CellInteractionEventArgs(cell, "PointerExited", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell pointer exited");
        }
    }

    private void OnCellRightTapped(DataGridCell cell, RightTappedRoutedEventArgs e)
    {
        try
        {
            // TODO: Show cell context menu
            _logger?.Info("🖱️ Cell right tapped");
            OnCellInteraction(new CellInteractionEventArgs(cell, "RightTapped", e));
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell right tapped");
        }
    }

    private async void OnCellDoubleTapped(DataGridCell cell, DoubleTappedRoutedEventArgs e)
    {
        try
        {
            var position = FindCellPosition(cell);
            if (position.HasValue)
            {
                await HandleCellDoubleClickAsync(cell, position.Value.row, position.Value.column);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell double tapped");
        }
    }

    #endregion

    #region Private Methods - Interaction Handling

    private async Task HandleCellSingleClickAsync(DataGridCell cell, int rowIndex, int columnIndex)
    {
        try
        {
            // Handle selection
            await _selectionManager.SelectCellAsync(rowIndex, columnIndex, _isCtrlPressed);
            
            // Set focus
            await _selectionManager.SetFocusAsync(rowIndex, columnIndex);

            _logger?.Info("🖱️ Handled single click on cell ({Row}, {Column})", rowIndex, columnIndex);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell single click");
        }
    }

    private async Task HandleCellDoubleClickAsync(DataGridCell cell, int rowIndex, int columnIndex)
    {
        try
        {
            // Start editing on double-click
            await _editingManager.StartEditingAsync(cell, rowIndex, columnIndex);
            
            _logger?.Info("🖱️ Handled double click on cell ({Row}, {Column}) - started editing", rowIndex, columnIndex);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell double click");
        }
    }

    private async Task HandleKeyboardInputAsync(VirtualKey key)
    {
        try
        {
            // Handle editing keys first
            if (_editingManager.IsEditMode)
            {
                var handled = await _editingManager.HandleEditingKeyAsync(key, _isCtrlPressed, _isShiftPressed);
                if (handled)
                {
                    return;
                }
            }

            // Handle navigation keys
            var navigationHandled = await _selectionManager.HandleKeyNavigationAsync(key, _isCtrlPressed, _isShiftPressed);
            if (navigationHandled)
            {
                return;
            }

            // Handle other special keys
            switch (key)
            {
                case VirtualKey.F2:
                    // Start editing current cell
                    if (_selectionManager.SelectedCell != null)
                    {
                        await _editingManager.StartEditingAsync(_selectionManager.SelectedCell, 
                            _selectionManager.SelectedRowIndex, _selectionManager.SelectedColumnIndex);
                    }
                    break;

                case VirtualKey.Delete:
                    // Delete selected cells content
                    await HandleDeleteKeyAsync();
                    break;

                case VirtualKey.Enter:
                    // Start editing or move to next row
                    if (_selectionManager.SelectedCell != null)
                    {
                        await _editingManager.StartEditingAsync(_selectionManager.SelectedCell,
                            _selectionManager.SelectedRowIndex, _selectionManager.SelectedColumnIndex);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling keyboard input");
        }
    }

    private async Task HandleDeleteKeyAsync()
    {
        try
        {
            // TODO: Clear content of selected cells
            _logger?.Info("🗑️ Handling delete key");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling delete key");
        }
    }

    #endregion

    #region Private Methods - Helper Methods

    private void UpdateModifierKeyStates()
    {
        try
        {
            var keyboardState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            _isCtrlPressed = keyboardState.HasFlag(CoreVirtualKeyStates.Down);

            keyboardState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            _isShiftPressed = keyboardState.HasFlag(CoreVirtualKeyStates.Down);

            keyboardState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);
            _isAltPressed = keyboardState.HasFlag(CoreVirtualKeyStates.Down);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error updating modifier key states");
        }
    }

    private void AttachEvent(FrameworkElement element, string eventName, Delegate handler)
    {
        try
        {
            // Store event attachment for cleanup
            if (!_attachedEvents.ContainsKey(element))
            {
                _attachedEvents[element] = new List<(string, Delegate)>();
            }
            _attachedEvents[element].Add((eventName, handler));

            // Attach event using reflection or direct attachment
            AttachEventByName(element, eventName, handler);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error attaching event {EventName}", eventName);
        }
    }

    private void AttachEventByName(FrameworkElement element, string eventName, Delegate handler)
    {
        switch (eventName)
        {
            case "PointerPressed":
                element.PointerPressed += (PointerEventHandler)handler;
                break;
            case "PointerMoved":
                element.PointerMoved += (PointerEventHandler)handler;
                break;
            case "PointerReleased":
                element.PointerReleased += (PointerEventHandler)handler;
                break;
            case "KeyDown":
                element.KeyDown += (KeyEventHandler)handler;
                break;
            case "KeyUp":
                element.KeyUp += (KeyEventHandler)handler;
                break;
            case "GotFocus":
                element.GotFocus += (RoutedEventHandler)handler;
                break;
            case "LostFocus":
                element.LostFocus += (RoutedEventHandler)handler;
                break;
            case "RightTapped":
                element.RightTapped += (RightTappedEventHandler)handler;
                break;
            case "DoubleTapped":
                element.DoubleTapped += (DoubleTappedEventHandler)handler;
                break;
            case "PointerEntered":
                element.PointerEntered += (PointerEventHandler)handler;
                break;
            case "PointerExited":
                element.PointerExited += (PointerEventHandler)handler;
                break;
        }
    }

    private void DetachEventByName(FrameworkElement element, string eventName, Delegate handler)
    {
        try
        {
            switch (eventName)
            {
                case "PointerPressed":
                    element.PointerPressed -= (PointerEventHandler)handler;
                    break;
                case "PointerMoved":
                    element.PointerMoved -= (PointerEventHandler)handler;
                    break;
                case "PointerReleased":
                    element.PointerReleased -= (PointerEventHandler)handler;
                    break;
                case "KeyDown":
                    element.KeyDown -= (KeyEventHandler)handler;
                    break;
                case "KeyUp":
                    element.KeyUp -= (KeyEventHandler)handler;
                    break;
                case "GotFocus":
                    element.GotFocus -= (RoutedEventHandler)handler;
                    break;
                case "LostFocus":
                    element.LostFocus -= (RoutedEventHandler)handler;
                    break;
                case "RightTapped":
                    element.RightTapped -= (RightTappedEventHandler)handler;
                    break;
                case "DoubleTapped":
                    element.DoubleTapped -= (DoubleTappedEventHandler)handler;
                    break;
                case "PointerEntered":
                    element.PointerEntered -= (PointerEventHandler)handler;
                    break;
                case "PointerExited":
                    element.PointerExited -= (PointerEventHandler)handler;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error detaching event {EventName}", eventName);
        }
    }

    private bool IsPointerOnResizeHandle(PointerRoutedEventArgs e, out int columnIndex)
    {
        columnIndex = -1;
        try
        {
            // TODO: Implement hit testing for resize handles
            return false;
        }
        catch
        {
            return false;
        }
    }

    private DataGridCell? FindCellUnderPointer(Windows.Foundation.Point position)
    {
        try
        {
            // TODO: Implement hit testing to find cell at position
            return null;
        }
        catch
        {
            return null;
        }
    }

    private DataGridCell? FindCellAt(int rowIndex, int columnIndex)
    {
        try
        {
            // TODO: Find cell by position
            return null;
        }
        catch
        {
            return null;
        }
    }

    private (int row, int column)? FindCellPosition(DataGridCell cell)
    {
        try
        {
            // TODO: Find position of cell
            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Event Raising

    private void OnGridEvent(DataGridEventArgs e)
    {
        GridEvent?.Invoke(this, e);
    }

    private void OnCellInteraction(CellInteractionEventArgs e)
    {
        CellInteraction?.Invoke(this, e);
    }

    private void OnKeyboardInput(KeyboardEventArgs e)
    {
        KeyboardInput?.Invoke(this, e);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        try
        {
            DetachEvents();
            _logger?.Info("🔧 DataGridEventManager disposed");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error disposing DataGridEventManager");
        }
    }

    #endregion
}

#region Event Args Classes

public class DataGridEventArgs : EventArgs
{
    public string EventType { get; }
    public object EventData { get; }

    public DataGridEventArgs(string eventType, object eventData)
    {
        EventType = eventType;
        EventData = eventData;
    }
}

public class CellInteractionEventArgs : EventArgs
{
    public DataGridCell Cell { get; }
    public string InteractionType { get; }
    public object EventData { get; }

    public CellInteractionEventArgs(DataGridCell cell, string interactionType, object eventData)
    {
        Cell = cell;
        InteractionType = interactionType;
        EventData = eventData;
    }
}

public class KeyboardEventArgs : EventArgs
{
    public VirtualKey Key { get; }
    public bool IsCtrlPressed { get; }
    public bool IsShiftPressed { get; }
    public bool IsAltPressed { get; }
    public bool IsKeyDown { get; }

    public KeyboardEventArgs(VirtualKey key, bool isCtrlPressed, bool isShiftPressed, bool isAltPressed, bool isKeyDown)
    {
        Key = key;
        IsCtrlPressed = isCtrlPressed;
        IsShiftPressed = isShiftPressed;
        IsAltPressed = isAltPressed;
        IsKeyDown = isKeyDown;
    }
}

#endregion