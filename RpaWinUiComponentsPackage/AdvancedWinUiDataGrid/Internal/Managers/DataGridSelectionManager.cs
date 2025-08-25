using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;
using Windows.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using System.Collections.ObjectModel;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Managers;

/// <summary>
/// Professional Selection Manager - handles all cell selection, focus, and navigation
/// Separates selection concerns from main DataGrid component
/// Optimized for large datasets with millions of rows
/// </summary>
internal sealed class DataGridSelectionManager : IDisposable
{
    #region Private Fields

    private readonly UserControl _parentGrid;
    private readonly ILogger? _logger;
    private readonly ObservableCollection<DataGridRow> _dataRows;
    private readonly ObservableCollection<GridColumnDefinition> _headers;

    // SELECTION STATE
    private int _selectedRowIndex = 0;
    private int _selectedColumnIndex = 0;
    private DataGridCell? _currentSelectedCell = null;
    private readonly HashSet<DataGridCell> _selectedCells = new();

    // FOCUS STATE
    private DataGridCell? _focusedCell = null;
    private int _focusedRowIndex = 0;
    private int _focusedColumnIndex = 0;

    // DRAG SELECTION STATE
    private DataGridCell? _dragStartCell = null;
    private DataGridCell? _dragEndCell = null;
    private bool _isDragging = false;
    private bool _isMultiSelectMode = false;

    // NAVIGATION STATE
    private DateTime _lastCellClickTime = DateTime.MinValue;
    private const int DoubleClickThresholdMs = 500;

    // EVENTS
    public event EventHandler<CellSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<CellFocusChangedEventArgs>? FocusChanged;
    public event EventHandler<CellDoubleClickedEventArgs>? CellDoubleClicked;

    #endregion

    #region Constructor

    public DataGridSelectionManager(
        UserControl parentGrid,
        ObservableCollection<DataGridRow> dataRows,
        ObservableCollection<GridColumnDefinition> headers,
        ILogger? logger = null)
    {
        _parentGrid = parentGrid ?? throw new ArgumentNullException(nameof(parentGrid));
        _dataRows = dataRows ?? throw new ArgumentNullException(nameof(dataRows));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        _logger = logger;

        _logger?.Info("🔧 DataGridSelectionManager initialized");
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Currently selected cell
    /// </summary>
    public DataGridCell? SelectedCell => _currentSelectedCell;

    /// <summary>
    /// Currently focused cell (may differ from selected)
    /// </summary>
    public DataGridCell? FocusedCell => _focusedCell;

    /// <summary>
    /// Selected row index
    /// </summary>
    public int SelectedRowIndex => _selectedRowIndex;

    /// <summary>
    /// Selected column index
    /// </summary>
    public int SelectedColumnIndex => _selectedColumnIndex;

    /// <summary>
    /// All currently selected cells
    /// </summary>
    public IReadOnlySet<DataGridCell> SelectedCells => _selectedCells;

    /// <summary>
    /// Is in multi-selection mode (Ctrl held)
    /// </summary>
    public bool IsMultiSelectMode => _isMultiSelectMode;

    /// <summary>
    /// Is currently dragging selection
    /// </summary>
    public bool IsDragging => _isDragging;

    #endregion

    #region Public Methods - Selection Management

    /// <summary>
    /// Select cell at specified position
    /// </summary>
    public async Task<bool> SelectCellAsync(int rowIndex, int columnIndex, bool addToSelection = false)
    {
        try
        {
            if (!IsValidPosition(rowIndex, columnIndex))
            {
                _logger?.Warning("⚠️ Invalid cell position: ({Row}, {Column})", rowIndex, columnIndex);
                return false;
            }

            var targetCell = GetCellAt(rowIndex, columnIndex);
            if (targetCell == null)
            {
                _logger?.Warning("⚠️ Cell not found at position: ({Row}, {Column})", rowIndex, columnIndex);
                return false;
            }

            if (!addToSelection)
            {
                ClearSelection();
            }

            await SelectCellInternalAsync(targetCell, rowIndex, columnIndex);
            _logger?.Info("✅ Cell selected at ({Row}, {Column})", rowIndex, columnIndex);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error selecting cell at ({Row}, {Column})", rowIndex, columnIndex);
            return false;
        }
    }

    /// <summary>
    /// Select range of cells
    /// </summary>
    public async Task<bool> SelectRangeAsync(int startRow, int startColumn, int endRow, int endColumn)
    {
        try
        {
            _logger?.Info("🔄 Selecting range: ({StartRow},{StartCol}) to ({EndRow},{EndCol})", 
                startRow, startColumn, endRow, endColumn);

            ClearSelection();

            var minRow = Math.Min(startRow, endRow);
            var maxRow = Math.Max(startRow, endRow);
            var minCol = Math.Min(startColumn, endColumn);
            var maxCol = Math.Max(startColumn, endColumn);

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (IsValidPosition(row, col))
                    {
                        var cell = GetCellAt(row, col);
                        if (cell != null)
                        {
                            await AddCellToSelectionAsync(cell, row, col);
                        }
                    }
                }
            }

            _logger?.Info("✅ Range selected: {CellCount} cells", _selectedCells.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error selecting range");
            return false;
        }
    }

    /// <summary>
    /// Clear all selection
    /// </summary>
    public void ClearSelection()
    {
        try
        {
            var wasSelected = _selectedCells.Count > 0;
            
            foreach (var cell in _selectedCells)
            {
                UpdateCellVisualState(cell, isSelected: false);
            }

            _selectedCells.Clear();
            _currentSelectedCell = null;
            _selectedRowIndex = 0;
            _selectedColumnIndex = 0;

            if (wasSelected)
            {
                OnSelectionChanged();
                _logger?.Info("🧹 Selection cleared");
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error clearing selection");
        }
    }

    #endregion

    #region Public Methods - Focus Management

    /// <summary>
    /// Set focus to specific cell
    /// </summary>
    public async Task<bool> SetFocusAsync(int rowIndex, int columnIndex)
    {
        try
        {
            if (!IsValidPosition(rowIndex, columnIndex))
            {
                return false;
            }

            var targetCell = GetCellAt(rowIndex, columnIndex);
            if (targetCell == null)
            {
                return false;
            }

            await SetFocusInternalAsync(targetCell, rowIndex, columnIndex);
            _logger?.Info("🎯 Focus set to cell ({Row}, {Column})", rowIndex, columnIndex);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error setting focus to ({Row}, {Column})", rowIndex, columnIndex);
            return false;
        }
    }

    /// <summary>
    /// Move focus in specified direction
    /// </summary>
    public async Task<bool> MoveFocusAsync(NavigationDirection direction)
    {
        try
        {
            var (newRow, newCol) = CalculateNewPosition(_focusedRowIndex, _focusedColumnIndex, direction);
            
            if (!IsValidPosition(newRow, newCol))
            {
                _logger?.Info("🚫 Cannot move focus {Direction} - would go out of bounds", direction);
                return false;
            }

            return await SetFocusAsync(newRow, newCol);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error moving focus {Direction}", direction);
            return false;
        }
    }

    #endregion

    #region Public Methods - Drag Selection

    /// <summary>
    /// Start drag selection from specified cell
    /// </summary>
    public void StartDragSelection(DataGridCell startCell, int rowIndex, int columnIndex)
    {
        try
        {
            _dragStartCell = startCell;
            _dragEndCell = startCell;
            _isDragging = true;

            _logger?.Info("🎯 Drag selection started at ({Row}, {Column})", rowIndex, columnIndex);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error starting drag selection");
        }
    }

    /// <summary>
    /// Update drag selection to specified cell
    /// </summary>
    public async Task UpdateDragSelectionAsync(DataGridCell currentCell, int rowIndex, int columnIndex)
    {
        try
        {
            if (!_isDragging || _dragStartCell == null)
            {
                return;
            }

            _dragEndCell = currentCell;

            // Get start and end positions
            var startPos = GetCellPosition(_dragStartCell);
            var endPos = (rowIndex, columnIndex);

            if (startPos.HasValue)
            {
                await SelectRangeAsync(startPos.Value.Row, startPos.Value.Column, endPos.rowIndex, endPos.columnIndex);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error updating drag selection");
        }
    }

    /// <summary>
    /// End drag selection
    /// </summary>
    public void EndDragSelection()
    {
        try
        {
            _isDragging = false;
            _dragStartCell = null;
            _dragEndCell = null;

            _logger?.Info("✅ Drag selection completed");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error ending drag selection");
        }
    }

    #endregion

    #region Public Methods - Keyboard Navigation

    /// <summary>
    /// Handle keyboard navigation
    /// </summary>
    public async Task<bool> HandleKeyNavigationAsync(VirtualKey key, bool isCtrlPressed, bool isShiftPressed)
    {
        try
        {
            var direction = GetNavigationDirection(key);
            if (direction == null)
            {
                return false;
            }

            _isMultiSelectMode = isCtrlPressed;

            if (isShiftPressed)
            {
                // Extend selection
                return await ExtendSelectionAsync(direction.Value);
            }
            else
            {
                // Move focus/selection
                return await MoveFocusAsync(direction.Value);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling keyboard navigation");
            return false;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handle cell pointer pressed (click/touch)
    /// </summary>
    public async Task HandleCellPointerPressedAsync(DataGridCell cell, PointerRoutedEventArgs e)
    {
        try
        {
            var position = GetCellPosition(cell);
            if (!position.HasValue)
            {
                return;
            }

            var (row, col) = (position.Value.Row, position.Value.Column);
            var isCtrlPressed = IsCtrlPressed();

            // Check for double-click
            var now = DateTime.Now;
            var isDoubleClick = (now - _lastCellClickTime).TotalMilliseconds < DoubleClickThresholdMs &&
                               _currentSelectedCell == cell;
            _lastCellClickTime = now;

            if (isDoubleClick)
            {
                OnCellDoubleClicked(cell, row, col);
                return;
            }

            // Handle selection
            var addToSelection = isCtrlPressed && _isMultiSelectMode;
            await SelectCellAsync(row, col, addToSelection);

            // Start potential drag
            StartDragSelection(cell, row, col);

            // Capture pointer for drag operations
            _parentGrid.CapturePointer(e.Pointer);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling cell pointer pressed");
        }
    }

    /// <summary>
    /// Handle pointer moved (for drag selection)
    /// </summary>
    public async Task HandlePointerMovedAsync(PointerRoutedEventArgs e)
    {
        try
        {
            if (!_isDragging)
            {
                return;
            }

            var currentCell = FindCellUnderPointer(e.GetCurrentPoint(_parentGrid).Position);
            if (currentCell != null)
            {
                var position = GetCellPosition(currentCell);
                if (position.HasValue)
                {
                    await UpdateDragSelectionAsync(currentCell, position.Value.Row, position.Value.Column);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling pointer moved");
        }
    }

    /// <summary>
    /// Handle pointer released (end drag)
    /// </summary>
    public void HandlePointerReleased(PointerRoutedEventArgs e)
    {
        try
        {
            EndDragSelection();
            _parentGrid.ReleasePointerCapture(e.Pointer);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling pointer released");
        }
    }

    #endregion

    #region Private Methods - Core Selection Logic

    private async Task SelectCellInternalAsync(DataGridCell cell, int rowIndex, int columnIndex)
    {
        // Remove from selection if already selected in multi-select mode
        if (_isMultiSelectMode && _selectedCells.Contains(cell))
        {
            await RemoveCellFromSelectionAsync(cell, rowIndex, columnIndex);
            return;
        }

        await AddCellToSelectionAsync(cell, rowIndex, columnIndex);
    }

    private async Task AddCellToSelectionAsync(DataGridCell cell, int rowIndex, int columnIndex)
    {
        _selectedCells.Add(cell);
        _currentSelectedCell = cell;
        _selectedRowIndex = rowIndex;
        _selectedColumnIndex = columnIndex;

        UpdateCellVisualState(cell, isSelected: true);
        await SetFocusInternalAsync(cell, rowIndex, columnIndex);
        
        OnSelectionChanged();
    }

    private async Task RemoveCellFromSelectionAsync(DataGridCell cell, int rowIndex, int columnIndex)
    {
        _selectedCells.Remove(cell);
        UpdateCellVisualState(cell, isSelected: false);

        if (_currentSelectedCell == cell)
        {
            _currentSelectedCell = _selectedCells.FirstOrDefault();
        }

        OnSelectionChanged();
        await Task.CompletedTask;
    }

    private async Task SetFocusInternalAsync(DataGridCell cell, int rowIndex, int columnIndex)
    {
        if (_focusedCell != null)
        {
            UpdateCellVisualState(_focusedCell, isFocused: false);
        }

        _focusedCell = cell;
        _focusedRowIndex = rowIndex;
        _focusedColumnIndex = columnIndex;

        UpdateCellVisualState(cell, isFocused: true);
        
        OnFocusChanged();
        await Task.CompletedTask;
    }

    #endregion

    #region Private Methods - Visual Updates

    private void UpdateCellVisualState(DataGridCell cell, bool? isSelected = null, bool? isFocused = null)
    {
        try
        {
            // Priority-based styling: Copy → Focus/Selection → Validation → Normal
            
            if (cell.IsCopied)
            {
                // Copy mode - light blue (highest priority)
                cell.BackgroundBrush = new SolidColorBrush(Color.FromArgb(100, 173, 216, 230));
                cell.BorderThickness = "2";
            }
            else if ((isSelected ?? cell.IsSelected) || (isFocused ?? cell.IsFocused))
            {
                // Selection/Focus - blue selection
                cell.BackgroundBrush = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
                cell.BorderThickness = (isFocused ?? cell.IsFocused) ? "2" : "1";
                cell.BorderBrush = new SolidColorBrush(Colors.Blue);
            }
            else if (!cell.ValidationState)
            {
                // Validation error - red border
                cell.BorderBrush = new SolidColorBrush(Colors.Red);
                cell.BorderThickness = "2";
                cell.BackgroundBrush = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                // Normal state
                cell.BorderBrush = new SolidColorBrush(Colors.Black);
                cell.BorderThickness = "1";
                cell.BackgroundBrush = new SolidColorBrush(Colors.Transparent);
            }

            // Update properties for binding
            if (isSelected.HasValue) cell.IsSelected = isSelected.Value;
            if (isFocused.HasValue) cell.IsFocused = isFocused.Value;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error updating cell visual state");
        }
    }

    #endregion

    #region Private Methods - Helper Methods

    private bool IsValidPosition(int rowIndex, int columnIndex)
    {
        return rowIndex >= 0 && rowIndex < _dataRows.Count &&
               columnIndex >= 0 && columnIndex < _headers.Count;
    }

    private DataGridCell? GetCellAt(int rowIndex, int columnIndex)
    {
        try
        {
            if (!IsValidPosition(rowIndex, columnIndex))
            {
                return null;
            }

            var row = _dataRows[rowIndex];
            return row.Cells.ElementAtOrDefault(columnIndex);
        }
        catch
        {
            return null;
        }
    }

    private (int Row, int Column)? GetCellPosition(DataGridCell cell)
    {
        try
        {
            for (int rowIndex = 0; rowIndex < _dataRows.Count; rowIndex++)
            {
                var row = _dataRows[rowIndex];
                for (int colIndex = 0; colIndex < row.Cells.Count; colIndex++)
                {
                    if (row.Cells[colIndex] == cell)
                    {
                        return (rowIndex, colIndex);
                    }
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private (int newRow, int newCol) CalculateNewPosition(int currentRow, int currentCol, NavigationDirection direction)
    {
        return direction switch
        {
            NavigationDirection.Up => (Math.Max(0, currentRow - 1), currentCol),
            NavigationDirection.Down => (Math.Min(_dataRows.Count - 1, currentRow + 1), currentCol),
            NavigationDirection.Left => (currentRow, Math.Max(0, currentCol - 1)),
            NavigationDirection.Right => (currentRow, Math.Min(_headers.Count - 1, currentCol + 1)),
            NavigationDirection.Home => (currentRow, 0),
            NavigationDirection.End => (currentRow, _headers.Count - 1),
            NavigationDirection.PageUp => (Math.Max(0, currentRow - 10), currentCol),
            NavigationDirection.PageDown => (Math.Min(_dataRows.Count - 1, currentRow + 10), currentCol),
            _ => (currentRow, currentCol)
        };
    }

    private NavigationDirection? GetNavigationDirection(VirtualKey key)
    {
        return key switch
        {
            VirtualKey.Up => NavigationDirection.Up,
            VirtualKey.Down => NavigationDirection.Down,
            VirtualKey.Left => NavigationDirection.Left,
            VirtualKey.Right => NavigationDirection.Right,
            VirtualKey.Home => NavigationDirection.Home,
            VirtualKey.End => NavigationDirection.End,
            VirtualKey.PageUp => NavigationDirection.PageUp,
            VirtualKey.PageDown => NavigationDirection.PageDown,
            _ => null
        };
    }

    private bool IsCtrlPressed()
    {
        try
        {
            return InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                .HasFlag(CoreVirtualKeyStates.Down);
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
            // TODO: Implement hit testing to find cell under pointer
            // This would involve traversing the visual tree to find the cell
            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> ExtendSelectionAsync(NavigationDirection direction)
    {
        // TODO: Implement selection extension logic
        await Task.CompletedTask;
        return false;
    }

    #endregion

    #region Event Raising

    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, new CellSelectionChangedEventArgs(_selectedCells.ToList()));
    }

    private void OnFocusChanged()
    {
        FocusChanged?.Invoke(this, new CellFocusChangedEventArgs(_focusedCell, _focusedRowIndex, _focusedColumnIndex));
    }

    private void OnCellDoubleClicked(DataGridCell cell, int row, int col)
    {
        CellDoubleClicked?.Invoke(this, new CellDoubleClickedEventArgs(cell, row, col));
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        try
        {
            ClearSelection();
            _logger?.Info("🔧 DataGridSelectionManager disposed");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error disposing DataGridSelectionManager");
        }
    }

    #endregion
}

#region Event Args Classes

/// <summary>
/// Event args for selection changes
/// </summary>
public class CellSelectionChangedEventArgs : EventArgs
{
    public IReadOnlyList<DataGridCell> SelectedCells { get; }

    public CellSelectionChangedEventArgs(IReadOnlyList<DataGridCell> selectedCells)
    {
        SelectedCells = selectedCells;
    }
}

/// <summary>
/// Event args for focus changes
/// </summary>
public class CellFocusChangedEventArgs : EventArgs
{
    public DataGridCell? FocusedCell { get; }
    public int RowIndex { get; }
    public int ColumnIndex { get; }

    public CellFocusChangedEventArgs(DataGridCell? focusedCell, int rowIndex, int columnIndex)
    {
        FocusedCell = focusedCell;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
    }
}

/// <summary>
/// Event args for double-click
/// </summary>
public class CellDoubleClickedEventArgs : EventArgs
{
    public DataGridCell Cell { get; }
    public int RowIndex { get; }
    public int ColumnIndex { get; }

    public CellDoubleClickedEventArgs(DataGridCell cell, int rowIndex, int columnIndex)
    {
        Cell = cell;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
    }
}

#endregion

#region Navigation Direction Enum

/// <summary>
/// Navigation directions for keyboard movement
/// </summary>
public enum NavigationDirection
{
    Up,
    Down,
    Left,
    Right,
    Home,
    End,
    PageUp,
    PageDown
}

#endregion