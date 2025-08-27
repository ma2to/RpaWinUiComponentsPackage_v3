using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using System.Collections.ObjectModel;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Managers;

/// <summary>
/// Professional Resize Manager - handles column resizing with mouse interaction
/// Separates resize concerns from main DataGrid component
/// Provides smooth resizing with visual feedback and constraints
/// </summary>
internal sealed class DataGridResizeManager : IDisposable
{
    #region Private Fields

    private readonly UserControl _parentGrid;
    private readonly ILogger? _logger;
    private readonly ObservableCollection<GridColumnDefinition> _headers;

    // RESIZE STATE
    private bool _isResizing = false;
    private GridColumnDefinition? _resizingColumn = null;
    private double _resizeStartX = 0;
    private double _resizeStartWidth = 0;
    private int _resizingColumnIndex = -1;

    // RESIZE CONSTRAINTS
    private const double MinColumnWidth = 50;
    private const double MaxColumnWidth = 800;
    private const double DefaultColumnWidth = 100;

    // VISUAL ELEMENTS
    private Rectangle? _resizePreviewLine = null;
    private readonly List<Rectangle> _resizeHandles = new();

    // EVENTS
    public event EventHandler<ColumnResizeStartedEventArgs>? ResizeStarted;
    public event EventHandler<ColumnResizeChangedEventArgs>? ResizeChanged;
    public event EventHandler<ColumnResizeEndedEventArgs>? ResizeEnded;

    #endregion

    #region Constructor

    public DataGridResizeManager(
        UserControl parentGrid,
        ObservableCollection<GridColumnDefinition> headers,
        ILogger? logger = null)
    {
        _parentGrid = parentGrid ?? throw new ArgumentNullException(nameof(parentGrid));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        _logger = logger;

        InitializeResizeHandles();
        _logger?.Info("🔧 DataGridResizeManager initialized");
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Is currently resizing a column
    /// </summary>
    public bool IsResizing => _isResizing;

    /// <summary>
    /// Currently resizing column
    /// </summary>
    public GridColumnDefinition? ResizingColumn => _resizingColumn;

    /// <summary>
    /// Minimum allowed column width
    /// </summary>
    public double MinimumColumnWidth => MinColumnWidth;

    /// <summary>
    /// Maximum allowed column width
    /// </summary>
    public double MaximumColumnWidth => MaxColumnWidth;

    #endregion

    #region Public Methods - Resize Operations

    /// <summary>
    /// Start column resize operation
    /// </summary>
    public bool StartResize(int columnIndex, double startX)
    {
        try
        {
            if (columnIndex < 0 || columnIndex >= _headers.Count)
            {
                _logger?.Warning("⚠️ Invalid column index for resize: {Index}", columnIndex);
                return false;
            }

            var column = _headers[columnIndex];
            if (!CanResizeColumn(column, columnIndex))
            {
                _logger?.Warning("⚠️ Column {Index} cannot be resized", columnIndex);
                return false;
            }

            _isResizing = true;
            _resizingColumn = column;
            _resizingColumnIndex = columnIndex;
            _resizeStartX = startX;
            _resizeStartWidth = column.Width;
            
            _logger?.Info("🔄 RESIZE START DETAILS: Column {Index} - StartX: {StartX}, StartWidth: {StartWidth}, ColumnName: '{Name}'", 
                columnIndex, startX, _resizeStartWidth, column.DisplayName);

            // Show resize preview
            ShowResizePreview(startX);

            // Change cursor - Temporarily disabled for WinUI3 compatibility
            // _parentGrid.Cursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);

            OnResizeStarted(column, columnIndex, _resizeStartWidth);
            _logger?.Info("📏 Started resizing column {Index} from width {Width}", columnIndex, _resizeStartWidth);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error starting column resize");
            return false;
        }
    }

    /// <summary>
    /// Update column resize during mouse movement
    /// </summary>
    public bool UpdateResize(double currentX)
    {
        try
        {
            if (!_isResizing || _resizingColumn == null)
            {
                return false;
            }

            var deltaX = currentX - _resizeStartX;
            var newWidth = Math.Max(MinColumnWidth, Math.Min(MaxColumnWidth, _resizeStartWidth + deltaX));

            // Update preview
            UpdateResizePreview(currentX);
            
            // Log resize update details
            _logger?.Info("📏 RESIZE UPDATE: Column {Index} - CurrentX: {CurrentX}, DeltaX: {DeltaX}, NewWidth: {NewWidth}", 
                _resizingColumnIndex, currentX, deltaX, newWidth);

            // Raise change event (but don't apply yet)
            OnResizeChanged(_resizingColumn, _resizingColumnIndex, _resizeStartWidth, newWidth);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error updating column resize");
            return false;
        }
    }

    /// <summary>
    /// End column resize operation
    /// </summary>
    public bool EndResize(double endX, bool applyChanges = true)
    {
        try
        {
            if (!_isResizing || _resizingColumn == null)
            {
                return false;
            }

            var deltaX = endX - _resizeStartX;
            var newWidth = Math.Max(MinColumnWidth, Math.Min(MaxColumnWidth, _resizeStartWidth + deltaX));

            if (applyChanges)
            {
                var currentWidth = _resizingColumn.Width;
                // Apply the new width
                _resizingColumn.Width = (int)newWidth;
                _logger?.Info("✅ RESIZE APPLY: Column {Index} - Changed from {OldWidth} to {NewWidth} (actual: {ActualWidth})", 
                    _resizingColumnIndex, currentWidth, newWidth, _resizingColumn.Width);
                
                // Verify the change was applied
                if (_resizingColumn.Width != (int)newWidth)
                {
                    _logger?.Error("🚨 RESIZE ERROR: Width change failed! Expected {Expected}, got {Actual}", 
                        (int)newWidth, _resizingColumn.Width);
                }
            }

            // Hide resize preview
            HideResizePreview();

            // Reset cursor
            // _parentGrid.Cursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

            // Store resize info for event
            var column = _resizingColumn;
            var columnIndex = _resizingColumnIndex;
            var oldWidth = _resizeStartWidth;

            // Clear resize state
            _isResizing = false;
            _resizingColumn = null;
            _resizingColumnIndex = -1;
            _resizeStartX = 0;
            _resizeStartWidth = 0;

            OnResizeEnded(column, columnIndex, oldWidth, applyChanges ? newWidth : oldWidth, applyChanges);
            _logger?.Info("🏁 Ended column resize, applied: {Applied}", applyChanges);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error ending column resize");
            return false;
        }
    }

    /// <summary>
    /// Cancel current resize operation
    /// </summary>
    public bool CancelResize()
    {
        return EndResize(0, applyChanges: false);
    }

    #endregion

    #region Public Methods - Column Width Management

    /// <summary>
    /// Set column width programmatically
    /// </summary>
    public bool SetColumnWidth(int columnIndex, double width)
    {
        try
        {
            if (columnIndex < 0 || columnIndex >= _headers.Count)
            {
                _logger?.Warning("⚠️ Invalid column index: {Index}", columnIndex);
                return false;
            }

            var column = _headers[columnIndex];
            var constrainedWidth = Math.Max(MinColumnWidth, Math.Min(MaxColumnWidth, width));

            var oldWidth = column.Width;
            column.Width = (int)constrainedWidth;

            // Update resize handles
            UpdateResizeHandles();

            OnResizeEnded(column, columnIndex, oldWidth, constrainedWidth, true);
            _logger?.Info("📏 Set column {Index} width to {Width}", columnIndex, constrainedWidth);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error setting column width");
            return false;
        }
    }

    /// <summary>
    /// Auto-fit column width to content
    /// </summary>
    public async Task<bool> AutoFitColumnAsync(int columnIndex)
    {
        try
        {
            if (columnIndex < 0 || columnIndex >= _headers.Count)
            {
                return false;
            }

            var optimalWidth = await CalculateOptimalColumnWidthAsync(columnIndex);
            return SetColumnWidth(columnIndex, optimalWidth);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error auto-fitting column {Index}", columnIndex);
            return false;
        }
    }

    /// <summary>
    /// Auto-fit all columns
    /// </summary>
    public async Task<bool> AutoFitAllColumnsAsync()
    {
        try
        {
            var tasks = new List<Task<bool>>();
            
            for (int i = 0; i < _headers.Count; i++)
            {
                tasks.Add(AutoFitColumnAsync(i));
            }

            var results = await Task.WhenAll(tasks);
            var success = results.All(r => r);

            _logger?.Info("📏 Auto-fit all columns, success: {Success}", success);
            return success;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error auto-fitting all columns");
            return false;
        }
    }

    /// <summary>
    /// Reset column width to default
    /// </summary>
    public bool ResetColumnWidth(int columnIndex)
    {
        return SetColumnWidth(columnIndex, (int)DefaultColumnWidth);
    }

    /// <summary>
    /// Reset all column widths to default
    /// </summary>
    public bool ResetAllColumnWidths()
    {
        try
        {
            for (int i = 0; i < _headers.Count; i++)
            {
                var column = _headers[i];
                column.Width = (int)DefaultColumnWidth;
            }

            UpdateResizeHandles();
            _logger?.Info("📏 Reset all column widths to default");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error resetting all column widths");
            return false;
        }
    }

    #endregion

    #region Public Methods - Event Handlers

    /// <summary>
    /// Handle pointer pressed on resize handle
    /// </summary>
    public bool HandleResizeHandlePressed(int columnIndex, PointerRoutedEventArgs e)
    {
        try
        {
            var position = e.GetCurrentPoint(_parentGrid);
            return StartResize(columnIndex, position.Position.X);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling resize handle pressed");
            return false;
        }
    }

    /// <summary>
    /// Handle pointer moved during resize
    /// </summary>
    public bool HandleResizePointerMoved(PointerRoutedEventArgs e)
    {
        try
        {
            if (!_isResizing)
            {
                return false;
            }

            var position = e.GetCurrentPoint(_parentGrid);
            return UpdateResize(position.Position.X);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling resize pointer moved");
            return false;
        }
    }

    /// <summary>
    /// Handle pointer released during resize
    /// </summary>
    public bool HandleResizePointerReleased(PointerRoutedEventArgs e)
    {
        try
        {
            if (!_isResizing)
            {
                return false;
            }

            var position = e.GetCurrentPoint(_parentGrid);
            return EndResize(position.Position.X, applyChanges: true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling resize pointer released");
            return false;
        }
    }

    /// <summary>
    /// Handle double-click on resize handle (auto-fit)
    /// </summary>
    public async Task<bool> HandleResizeHandleDoubleClickAsync(int columnIndex)
    {
        try
        {
            _logger?.Info("🖱️ Double-click on resize handle for column {Index}", columnIndex);
            return await AutoFitColumnAsync(columnIndex);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error handling resize handle double-click");
            return false;
        }
    }

    #endregion

    #region Private Methods - Visual Elements

    private void InitializeResizeHandles()
    {
        try
        {
            // Create resize handles for each column
            UpdateResizeHandles();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error initializing resize handles");
        }
    }

    private void UpdateResizeHandles()
    {
        try
        {
            // TODO: Update resize handle positions based on column positions
            // This would involve creating/updating Rectangle elements that serve as resize handles
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error updating resize handles");
        }
    }

    private void ShowResizePreview(double x)
    {
        try
        {
            if (_resizePreviewLine == null)
            {
                _resizePreviewLine = new Rectangle
                {
                    Width = 2,
                    Fill = new SolidColorBrush(Colors.Gray),
                    Opacity = 0.7
                };

                // TODO: Add preview line to visual tree
            }

            // Position the preview line
            Canvas.SetLeft(_resizePreviewLine, x);
            _resizePreviewLine.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error showing resize preview");
        }
    }

    private void UpdateResizePreview(double x)
    {
        try
        {
            if (_resizePreviewLine != null)
            {
                Canvas.SetLeft(_resizePreviewLine, x);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error updating resize preview");
        }
    }

    private void HideResizePreview()
    {
        try
        {
            if (_resizePreviewLine != null)
            {
                _resizePreviewLine.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error hiding resize preview");
        }
    }

    #endregion

    #region Private Methods - Width Calculations

    private async Task<double> CalculateOptimalColumnWidthAsync(int columnIndex)
    {
        try
        {
            // TODO: Calculate optimal width based on:
            // 1. Header text width
            // 2. Maximum content width in visible cells
            // 3. Minimum/maximum constraints

            await Task.Delay(1); // Placeholder for async calculation

            // For now, return a calculated width based on column name length
            if (columnIndex >= 0 && columnIndex < _headers.Count)
            {
                var column = _headers[columnIndex];
                var headerLength = 10; // Default header length since ColumnDefinition doesn't have Name
                var calculatedWidth = Math.Max(MinColumnWidth, Math.Min(MaxColumnWidth, headerLength * 8 + 40));
                
                _logger?.Info("📏 Calculated optimal width for column {Index}: {Width}", columnIndex, calculatedWidth);
                return calculatedWidth;
            }

            return DefaultColumnWidth;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error calculating optimal column width");
            return DefaultColumnWidth;
        }
    }

    private double MeasureTextWidth(string text, double fontSize = 12)
    {
        try
        {
            // TODO: Implement proper text measurement using TextBlock
            // For now, use approximation
            return text.Length * fontSize * 0.6;
        }
        catch
        {
            return DefaultColumnWidth;
        }
    }

    #endregion

    #region Private Methods - Helper Methods

    private bool CanResizeColumn(GridColumnDefinition column, int columnIndex)
    {
        try
        {
            // Check if column allows resizing - basic ColumnDefinition always allows resize
            // TODO: Implement proper column configuration checking
            
            // Special columns might have resize restrictions - disabled for now
            // TODO: Add proper column type checking
            if (false) // Temporarily disabled
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Event Raising

    private void OnResizeStarted(GridColumnDefinition column, int columnIndex, double originalWidth)
    {
        ResizeStarted?.Invoke(this, new ColumnResizeStartedEventArgs(column, columnIndex, originalWidth));
    }

    private void OnResizeChanged(GridColumnDefinition column, int columnIndex, double originalWidth, double newWidth)
    {
        ResizeChanged?.Invoke(this, new ColumnResizeChangedEventArgs(column, columnIndex, originalWidth, newWidth));
    }

    private void OnResizeEnded(GridColumnDefinition column, int columnIndex, double originalWidth, double finalWidth, bool wasApplied)
    {
        ResizeEnded?.Invoke(this, new ColumnResizeEndedEventArgs(column, columnIndex, originalWidth, finalWidth, wasApplied));
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        try
        {
            if (_isResizing)
            {
                CancelResize();
            }

            // Clean up visual elements
            _resizeHandles.Clear();
            _resizePreviewLine = null;

            _logger?.Info("🔧 DataGridResizeManager disposed");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 Error disposing DataGridResizeManager");
        }
    }

    #endregion
}

#region Event Args Classes

internal class ColumnResizeStartedEventArgs : EventArgs
{
    public GridColumnDefinition Column { get; }
    public int ColumnIndex { get; }
    public double OriginalWidth { get; }

    public ColumnResizeStartedEventArgs(GridColumnDefinition column, int columnIndex, double originalWidth)
    {
        Column = column;
        ColumnIndex = columnIndex;
        OriginalWidth = originalWidth;
    }
}

internal class ColumnResizeChangedEventArgs : EventArgs
{
    public GridColumnDefinition Column { get; }
    public int ColumnIndex { get; }
    public double OriginalWidth { get; }
    public double NewWidth { get; }

    public ColumnResizeChangedEventArgs(GridColumnDefinition column, int columnIndex, double originalWidth, double newWidth)
    {
        Column = column;
        ColumnIndex = columnIndex;
        OriginalWidth = originalWidth;
        NewWidth = newWidth;
    }
}

internal class ColumnResizeEndedEventArgs : EventArgs
{
    public GridColumnDefinition Column { get; }
    public int ColumnIndex { get; }
    public double OriginalWidth { get; }
    public double FinalWidth { get; }
    public bool WasApplied { get; }

    public ColumnResizeEndedEventArgs(GridColumnDefinition column, int columnIndex, double originalWidth, double finalWidth, bool wasApplied)
    {
        Column = column;
        ColumnIndex = columnIndex;
        OriginalWidth = originalWidth;
        FinalWidth = finalWidth;
        WasApplied = wasApplied;
    }
}

#endregion