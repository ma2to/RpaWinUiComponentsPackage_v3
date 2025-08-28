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
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Coordination;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Managers;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Orchestration;

/// <summary>
/// PROFESSIONAL Event Orchestrator - ONLY event flow coordination
/// RESPONSIBILITY: Coordinate between event detection and manager responses (NO event handling logic, NO UI operations)  
/// SEPARATION: Pure event orchestration - delegates to appropriate coordinators and managers
/// ANTI-GOD: Thin orchestration layer that coordinates without doing the actual work
/// </summary>
internal sealed class EventOrchestrator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private readonly EventCoordinator _eventCoordinator;
    private readonly InteractionCoordinator _interactionCoordinator;
    private readonly ClipboardCoordinator _clipboardCoordinator;
    private readonly DataGridSelectionManager _selectionManager;
    private readonly DataGridEditingManager _editingManager;
    private readonly DataGridResizeManager _resizeManager;
    private bool _disposed = false;

    public EventOrchestrator(
        ILogger? logger,
        GlobalExceptionHandler exceptionHandler,
        EventCoordinator eventCoordinator,
        InteractionCoordinator interactionCoordinator,
        ClipboardCoordinator clipboardCoordinator,
        DataGridSelectionManager selectionManager,
        DataGridEditingManager editingManager,
        DataGridResizeManager resizeManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        _eventCoordinator = eventCoordinator ?? throw new ArgumentNullException(nameof(eventCoordinator));
        _interactionCoordinator = interactionCoordinator ?? throw new ArgumentNullException(nameof(interactionCoordinator));
        _clipboardCoordinator = clipboardCoordinator ?? throw new ArgumentNullException(nameof(clipboardCoordinator));
        _selectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager));
        _editingManager = editingManager ?? throw new ArgumentNullException(nameof(editingManager));
        _resizeManager = resizeManager ?? throw new ArgumentNullException(nameof(resizeManager));
        
        _logger?.Info("🎭 EVENT ORCHESTRATOR: Initialized - Event flow coordination only");
    }

    /// <summary>
    /// Orchestrate cell pointer pressed event
    /// ORCHESTRATION: Interaction analysis + Manager coordination
    /// </summary>
    public async Task OrchestrateCellPointerPressedAsync(DataGridCell cell, PointerRoutedEventArgs e)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATE: Cell pointer pressed - {CellId}", cell.CellId);
            
            // Step 1: Update interaction state (Interaction layer)
            await _interactionCoordinator.UpdateModifierKeyStatesAsync();
            
            // Step 2: Analyze interaction timing (Interaction layer)
            var analysisResult = await _interactionCoordinator.AnalyzeCellClickAsync(cell, DateTime.Now);
            
            if (analysisResult.IsSuccess)
            {
                // Step 3: Route to appropriate manager based on interaction type
                if (analysisResult.Value.IsDoubleClick)
                {
                    // Double-click: Start editing (Editing manager)
                    await _editingManager.StartEditingAsync(cell, cell.RowIndex, cell.ColumnIndex);
                }
                else
                {
                    // Single-click: Handle selection (Selection manager)
                    await _selectionManager.SelectCellAsync(cell.RowIndex, cell.ColumnIndex, 
                        analysisResult.Value.ModifierKeys.Ctrl);
                    await _selectionManager.SetFocusAsync(cell.RowIndex, cell.ColumnIndex);
                }
            }
            
            _logger?.Info("✅ ORCHESTRATE: Cell pointer pressed orchestration completed");
            
        }, $"OrchestrateCellPointerPressed-{cell.CellId}", _logger);
    }

    /// <summary>
    /// Orchestrate keyboard input event
    /// ORCHESTRATION: Modifier state + Manager routing + Clipboard operations
    /// </summary>
    public async Task OrchestrateKeyboardInputAsync(VirtualKey key)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATE: Keyboard input - {Key}", key);
            
            // Step 1: Update modifier states (Interaction layer)
            await _interactionCoordinator.UpdateModifierKeyStatesAsync();
            var modifiers = _interactionCoordinator.ModifierKeys;
            
            // Step 2: Handle editing keys first (Editing manager)
            if (_editingManager.IsEditMode)
            {
                var handled = await _editingManager.HandleEditingKeyAsync(key, modifiers.Ctrl, modifiers.Shift);
                if (handled)
                {
                    _logger?.Info("🎭 ORCHESTRATE: Key handled by editing manager");
                    return;
                }
            }

            // Step 3: Handle navigation keys (Selection manager)
            var navigationHandled = await _selectionManager.HandleKeyNavigationAsync(key, modifiers.Ctrl, modifiers.Shift);
            if (navigationHandled)
            {
                _logger?.Info("🎭 ORCHESTRATE: Key handled by selection manager");
                return;
            }

            // Step 4: Handle special keys
            await HandleSpecialKeysAsync(key, modifiers);
            
            _logger?.Info("✅ ORCHESTRATE: Keyboard input orchestration completed");
            
        }, $"OrchestrateKeyboardInput-{key}", _logger);
    }

    /// <summary>
    /// Orchestrate copy operation
    /// ORCHESTRATION: Selection data + Clipboard formatting + System interaction
    /// </summary>
    public async Task OrchestrateCopyAsync()
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATE: Copy operation");
            
            // Step 1: Get selected cells (Selection manager)
            var selectedCells = _selectionManager.SelectedCells;
            if (selectedCells == null || !selectedCells.Any())
            {
                _logger?.Info("🎭 ORCHESTRATE: No cells selected for copy");
                return;
            }

            // Step 2: Format data for clipboard (Clipboard coordinator)
            var formattedDataResult = await _clipboardCoordinator.FormatCellDataAsync(selectedCells.ToList());
            if (!formattedDataResult.IsSuccess)
            {
                _logger?.Error("❌ ORCHESTRATE: Failed to format copy data - {Error}", formattedDataResult.ErrorMessage);
                return;
            }

            // Step 3: Copy to system clipboard (Clipboard coordinator)
            var copyResult = await _clipboardCoordinator.CopyToClipboardAsync(formattedDataResult.Value);
            if (copyResult.IsSuccess)
            {
                _logger?.Info("✅ ORCHESTRATE: Copy operation completed - {CellCount} cells", selectedCells.Count());
            }
            else
            {
                _logger?.Error("❌ ORCHESTRATE: Copy operation failed - {Error}", copyResult.ErrorMessage);
            }
            
        }, "OrchestrateCopy", _logger);
    }

    /// <summary>
    /// Orchestrate paste operation
    /// ORCHESTRATION: Clipboard retrieval + Data parsing + Manager operations
    /// </summary>
    public async Task OrchestratePasteAsync()
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATE: Paste operation");
            
            // Step 1: Get current selection position (Selection manager)
            if (_selectionManager.SelectedCell == null)
            {
                _logger?.Info("🎭 ORCHESTRATE: No cell selected as paste destination");
                return;
            }

            var startRow = _selectionManager.SelectedRowIndex;
            var startColumn = _selectionManager.SelectedColumnIndex;

            // Step 2: Get clipboard data (Clipboard coordinator)
            var clipboardResult = await _clipboardCoordinator.GetFromClipboardAsync();
            if (!clipboardResult.IsSuccess || !clipboardResult.Value.HasText)
            {
                _logger?.Info("🎭 ORCHESTRATE: No text data in clipboard for paste");
                return;
            }

            // Step 3: Parse clipboard data (Clipboard coordinator)
            var parseResult = await _clipboardCoordinator.ParseClipboardTextAsync(clipboardResult.Value.TextData!);
            if (!parseResult.IsSuccess)
            {
                _logger?.Error("❌ ORCHESTRATE: Failed to parse clipboard data - {Error}", parseResult.ErrorMessage);
                return;
            }

            // Step 4: Apply parsed data (would coordinate with data managers)
            // TODO: Implement paste data application through appropriate managers
            _logger?.Info("✅ ORCHESTRATE: Paste operation prepared - {RowCount}x{ColumnCount} at R{Row}C{Column}",
                parseResult.Value.RowCount, parseResult.Value.ColumnCount, startRow, startColumn);
            
        }, "OrchestratePaste", _logger);
    }

    /// <summary>
    /// Orchestrate resize operation
    /// ORCHESTRATION: Resize manager + Event coordination
    /// </summary>
    public async Task OrchestrateResizeOperationAsync(int columnIndex, PointerRoutedEventArgs e, string operation)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATE: Resize operation - {Operation} for column {Column}", operation, columnIndex);
            
            // Route to resize manager based on operation
            switch (operation.ToLower())
            {
                case "start":
                    _resizeManager.HandleResizeHandlePressed(columnIndex, e);
                    break;
                case "move":
                    _resizeManager.HandleResizePointerMoved(e);
                    break;
                case "end":
                    _resizeManager.HandleResizePointerReleased(e);
                    break;
            }
            
            _logger?.Info("✅ ORCHESTRATE: Resize operation completed");
            
        }, $"OrchestrateResize-{operation}-{columnIndex}", _logger);
    }

    /// <summary>
    /// Orchestrate focus change
    /// ORCHESTRATION: Interaction state + Manager notifications
    /// </summary>
    public async Task OrchestrateFocusChangeAsync(bool hasFocus, FrameworkElement? focusedElement = null)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATE: Focus change - HasFocus: {HasFocus}", hasFocus);
            
            // Step 1: Update interaction state (Interaction coordinator)
            await _interactionCoordinator.UpdateFocusStateAsync(hasFocus, focusedElement);
            
            // Step 2: Notify managers of focus change
            // TODO: Notify appropriate managers about focus changes
            
            _logger?.Info("✅ ORCHESTRATE: Focus change orchestration completed");
            
        }, "OrchestrateFocusChange", _logger);
    }

    #region Private Orchestration Methods

    private async Task HandleSpecialKeysAsync(VirtualKey key, (bool Ctrl, bool Shift, bool Alt) modifiers)
    {
        switch (key)
        {
            case VirtualKey.F2:
                // F2: Start editing current cell
                if (_selectionManager.SelectedCell != null)
                {
                    await _editingManager.StartEditingAsync(_selectionManager.SelectedCell, 
                        _selectionManager.SelectedRowIndex, _selectionManager.SelectedColumnIndex);
                }
                break;

            case VirtualKey.Delete:
                // Delete: Clear selected cells content
                // TODO: Implement delete operation through appropriate managers
                _logger?.Info("🗑️ ORCHESTRATE: Delete key pressed");
                break;

            case VirtualKey.Enter:
                // Enter: Start editing or move to next row
                if (_selectionManager.SelectedCell != null)
                {
                    await _editingManager.StartEditingAsync(_selectionManager.SelectedCell,
                        _selectionManager.SelectedRowIndex, _selectionManager.SelectedColumnIndex);
                }
                break;

            case VirtualKey.C when modifiers.Ctrl:
                // Ctrl+C: Copy selected cells
                await OrchestrateCopyAsync();
                break;

            case VirtualKey.V when modifiers.Ctrl:
                // Ctrl+V: Paste to selected cells
                await OrchestratePasteAsync();
                break;

            case VirtualKey.X when modifiers.Ctrl:
                // Ctrl+X: Cut selected cells (Copy + Delete)
                await OrchestrateCopyAsync();
                // TODO: Implement delete operation for cut
                _logger?.Info("✂️ ORCHESTRATE: Cut operation (copy completed, delete pending)");
                break;
        }
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 EVENT ORCHESTRATOR DISPOSE: Cleaning up event orchestration");
            
            _eventCoordinator?.Dispose();
            _interactionCoordinator?.Dispose();
            _clipboardCoordinator?.Dispose();
            
            _disposed = true;
            _logger?.Info("✅ EVENT ORCHESTRATOR DISPOSE: Event orchestrator disposed successfully");
        }
    }
}