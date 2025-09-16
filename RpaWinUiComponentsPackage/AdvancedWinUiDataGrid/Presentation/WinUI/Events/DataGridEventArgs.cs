using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Events;

/// <summary>
/// UI: Data changed event arguments
/// CLEAN ARCHITECTURE: UI layer event handling
/// SOLID: Single responsibility for data change notifications
/// </summary>
internal sealed class DataChangedEventArgs : EventArgs
{
    public string? ColumnName { get; init; }
    public int RowIndex { get; init; }
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
    public string? Operation { get; init; }
    public int AffectedRows { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public DataChangedEventArgs(string? columnName, int rowIndex, object? oldValue, object? newValue, string? operation = null, int affectedRows = 1)
    {
        ColumnName = columnName;
        RowIndex = rowIndex;
        OldValue = oldValue;
        NewValue = newValue;
        Operation = operation;
        AffectedRows = affectedRows;
    }
}

/// <summary>
/// UI: Operation completed event arguments
/// ENTERPRISE: Professional operation completion notification
/// </summary>
internal sealed class OperationCompletedEventArgs : EventArgs
{
    public string OperationType { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
    public TimeSpan Duration { get; init; }
    public int AffectedRows { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public OperationCompletedEventArgs(
        string operationType,
        bool success,
        string? message = null,
        TimeSpan duration = default,
        int affectedRows = 0)
    {
        OperationType = operationType;
        Success = success;
        Message = message;
        Duration = duration;
        AffectedRows = affectedRows;
    }
}

/// <summary>
/// UI: Error event arguments
/// ENTERPRISE: Comprehensive error information for UI layer
/// </summary>
internal sealed class ErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
    public string? OperationType { get; init; }
    public ErrorSeverity Severity { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object?> Context { get; init; } = new();

    public ErrorEventArgs(
        string errorMessage,
        Exception? exception = null,
        string? operationType = null,
        ErrorSeverity severity = ErrorSeverity.Error)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
        OperationType = operationType;
        Severity = severity;
    }
}

/// <summary>
/// UI: Item clicked event arguments
/// CLEAN ARCHITECTURE: UI interaction event handling
/// </summary>
internal sealed class ItemClickedEventArgs : EventArgs
{
    public int RowIndex { get; init; }
    public string? ColumnName { get; init; }
    public object? Value { get; init; }
    public ClickType ClickType { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public ItemClickedEventArgs(
        int rowIndex,
        string? columnName,
        object? value,
        ClickType clickType = ClickType.Single)
    {
        RowIndex = rowIndex;
        ColumnName = columnName;
        Value = value;
        ClickType = clickType;
    }
}

/// <summary>
/// UI: Selection changed event arguments
/// ENTERPRISE: Professional selection state management
/// </summary>
internal sealed class DataGridSelectionChangedEventArgs : EventArgs
{
    public IReadOnlyList<int> SelectedRowIndices { get; init; }
    public IReadOnlyList<int> PreviouslySelectedRowIndices { get; init; }
    public SelectionChangeType ChangeType { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public DataGridSelectionChangedEventArgs(
        IReadOnlyList<int> selectedRowIndices,
        IReadOnlyList<int> previouslySelectedRowIndices,
        SelectionChangeType changeType)
    {
        SelectedRowIndices = selectedRowIndices;
        PreviouslySelectedRowIndices = previouslySelectedRowIndices;
        ChangeType = changeType;
    }
}

/// <summary>
/// ENUM: Error severity levels
/// </summary>
internal enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// ENUM: Click types for UI interaction
/// </summary>
internal enum ClickType
{
    Single,
    Double,
    RightClick,
    MiddleClick
}

/// <summary>
/// ENUM: Selection change types
/// </summary>
internal enum SelectionChangeType
{
    Added,
    Removed,
    Replaced,
    Cleared,
    SelectAll
}