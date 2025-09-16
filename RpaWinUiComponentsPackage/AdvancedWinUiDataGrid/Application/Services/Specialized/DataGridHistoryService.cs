using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services.Specialized;

/// <summary>
/// APPLICATION: Grid history and undo/redo operations service
/// CLEAN ARCHITECTURE: Application layer - Manages operation history
/// SOLID: Single responsibility for history management
/// EXTRACTED FROM: DataGridUnifiedService - history operations
/// </summary>
internal sealed class DataGridHistoryService : IDataGridHistoryService
{
    #region Private Fields - History Management

    private readonly IDataGridStateManagementService _stateService;
    private readonly ILogger? _logger;
    private readonly List<GridState> _history;
    private readonly List<GridState> _redoStack;
    private readonly int _maxHistorySize;
    private int _currentHistoryIndex;
    private bool _disposed;

    #endregion

    #region Constructor - Dependency Injection

    public DataGridHistoryService(
        IDataGridStateManagementService stateService,
        ILogger<DataGridHistoryService>? logger = null,
        int maxHistorySize = 50)
    {
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _logger = logger;
        _maxHistorySize = Math.Max(1, maxHistorySize);

        _history = new List<GridState>();
        _redoStack = new List<GridState>();
        _currentHistoryIndex = -1;
    }

    #endregion

    #region HISTORY MANAGEMENT: Core history operations

    /// <summary>
    /// HISTORY: Save current state to history
    /// </summary>
    public async Task<Result<bool>> SaveCurrentStateAsync()
    {
        try
        {
            var currentState = await _stateService.GetCurrentStateAsync();
            if (!currentState.IsSuccess)
            {
                return Result<bool>.Failure($"Cannot save state to history: {currentState.Error}");
            }

            // Create a deep copy of the state
            var stateCopy = CreateStateCopy(currentState.Value!);

            // Clear redo stack when new state is saved
            _redoStack.Clear();

            // Add to history
            _currentHistoryIndex++;
            if (_currentHistoryIndex < _history.Count)
            {
                // Remove any history after current index (overwriting future)
                _history.RemoveRange(_currentHistoryIndex, _history.Count - _currentHistoryIndex);
            }

            _history.Add(stateCopy);

            // Limit history size
            if (_history.Count > _maxHistorySize)
            {
                _history.RemoveAt(0);
                _currentHistoryIndex--;
            }

            _logger?.LogInformation("[HISTORY] State saved to history - Index: {Index}, Total: {Total}",
                _currentHistoryIndex, _history.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to save state to history: {ex.Message}";
            _logger?.LogError(ex, "[HISTORY] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// HISTORY: Undo last operation
    /// </summary>
    public async Task<Result<bool>> UndoAsync()
    {
        try
        {
            if (!CanUndo)
            {
                _logger?.LogWarning("[HISTORY] Cannot undo - no previous state available");
                return Result<bool>.Failure("No operations to undo");
            }

            // Save current state to redo stack before undoing
            var currentState = await _stateService.GetCurrentStateAsync();
            if (currentState.IsSuccess)
            {
                _redoStack.Add(CreateStateCopy(currentState.Value!));
            }

            // Move to previous state
            _currentHistoryIndex--;
            var previousState = _history[_currentHistoryIndex];

            // Restore previous state
            var restoreResult = await _stateService.UpdateStateAsync(previousState);
            if (!restoreResult.IsSuccess)
            {
                // Revert index change on failure
                _currentHistoryIndex++;
                if (_redoStack.Count > 0)
                {
                    _redoStack.RemoveAt(_redoStack.Count - 1);
                }
                return Result<bool>.Failure($"Failed to restore previous state: {restoreResult.Error}");
            }

            _logger?.LogInformation("[HISTORY] Undo completed - Index: {Index}", _currentHistoryIndex);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Undo operation failed: {ex.Message}";
            _logger?.LogError(ex, "[HISTORY] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// HISTORY: Redo last undone operation
    /// </summary>
    public async Task<Result<bool>> RedoAsync()
    {
        try
        {
            if (!CanRedo)
            {
                _logger?.LogWarning("[HISTORY] Cannot redo - no undone operations available");
                return Result<bool>.Failure("No operations to redo");
            }

            // Get next state from redo stack
            var nextState = _redoStack[_redoStack.Count - 1];
            _redoStack.RemoveAt(_redoStack.Count - 1);

            // Save current state to history
            var currentState = await _stateService.GetCurrentStateAsync();
            if (currentState.IsSuccess)
            {
                _currentHistoryIndex++;
                if (_currentHistoryIndex < _history.Count)
                {
                    _history[_currentHistoryIndex] = CreateStateCopy(currentState.Value!);
                }
                else
                {
                    _history.Add(CreateStateCopy(currentState.Value!));
                }
            }

            // Restore next state
            var restoreResult = await _stateService.UpdateStateAsync(nextState);
            if (!restoreResult.IsSuccess)
            {
                // Revert changes on failure
                _redoStack.Add(nextState);
                if (_currentHistoryIndex >= 0 && _currentHistoryIndex < _history.Count)
                {
                    _currentHistoryIndex--;
                }
                return Result<bool>.Failure($"Failed to restore next state: {restoreResult.Error}");
            }

            _logger?.LogInformation("[HISTORY] Redo completed - Index: {Index}", _currentHistoryIndex);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Redo operation failed: {ex.Message}";
            _logger?.LogError(ex, "[HISTORY] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    #endregion

    #region HISTORY QUERY: History state queries

    /// <summary>
    /// QUERY: Check if undo is available
    /// </summary>
    public bool CanUndo => _currentHistoryIndex > 0;

    /// <summary>
    /// QUERY: Check if redo is available
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// QUERY: Get history count
    /// </summary>
    public int HistoryCount => _history.Count;

    /// <summary>
    /// QUERY: Get current history index
    /// </summary>
    public int CurrentHistoryIndex => _currentHistoryIndex;

    /// <summary>
    /// QUERY: Get available redo operations count
    /// </summary>
    public int RedoCount => _redoStack.Count;

    #endregion

    #region HISTORY MANAGEMENT: Advanced operations

    /// <summary>
    /// HISTORY: Clear all history
    /// </summary>
    public async Task<Result<bool>> ClearHistoryAsync()
    {
        try
        {
            _history.Clear();
            _redoStack.Clear();
            _currentHistoryIndex = -1;

            _logger?.LogInformation("[HISTORY] History cleared successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to clear history: {ex.Message}";
            _logger?.LogError(ex, "[HISTORY] {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// HISTORY: Get history summary
    /// </summary>
    public async Task<Result<HistorySummary>> GetHistorySummaryAsync()
    {
        try
        {
            var summary = new HistorySummary
            {
                TotalStates = _history.Count,
                CurrentIndex = _currentHistoryIndex,
                CanUndo = CanUndo,
                CanRedo = CanRedo,
                RedoCount = RedoCount,
                MaxHistorySize = _maxHistorySize
            };

            return Result<HistorySummary>.Success(summary);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to get history summary: {ex.Message}";
            _logger?.LogError(ex, "[HISTORY] {ErrorMessage}", errorMessage);
            return Result<HistorySummary>.Failure(errorMessage, ex);
        }
    }

    #endregion

    #region HELPER METHODS: State management

    /// <summary>
    /// HELPER: Create a deep copy of grid state
    /// </summary>
    private GridState CreateStateCopy(GridState original)
    {
        // Since GridState is immutable and doesn't have copy methods,
        // we need to create a new one from scratch.
        // For now, we'll return the original as this is primarily used for undo/redo
        // which should work at a higher level through state management service

        return original; // TODO: Implement proper deep copy if needed for history
    }

    #endregion

    #region CLEANUP: Disposal pattern

    /// <summary>
    /// CLEANUP: Dispose of managed resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _history.Clear();
            _redoStack.Clear();
            _logger?.LogInformation("[HISTORY] DataGridHistoryService disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[HISTORY] Error during disposal: {ErrorMessage}", ex.Message);
        }

        _disposed = true;
    }

    #endregion
}

/// <summary>
/// INTERFACE: Contract for history service
/// </summary>
internal interface IDataGridHistoryService : IDisposable
{
    // History Operations
    Task<Result<bool>> SaveCurrentStateAsync();
    Task<Result<bool>> UndoAsync();
    Task<Result<bool>> RedoAsync();
    Task<Result<bool>> ClearHistoryAsync();

    // History Queries
    bool CanUndo { get; }
    bool CanRedo { get; }
    int HistoryCount { get; }
    int CurrentHistoryIndex { get; }
    int RedoCount { get; }

    // Advanced Operations
    Task<Result<HistorySummary>> GetHistorySummaryAsync();
}

/// <summary>
/// DTO: History summary information
/// </summary>
internal class HistorySummary
{
    public int TotalStates { get; set; }
    public int CurrentIndex { get; set; }
    public bool CanUndo { get; set; }
    public bool CanRedo { get; set; }
    public int RedoCount { get; set; }
    public int MaxHistorySize { get; set; }
}