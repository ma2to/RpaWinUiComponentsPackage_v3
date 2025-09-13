using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;

/// <summary>
/// DDD: Value object for data operation results
/// ENTERPRISE: Result tracking for data operations
/// IMMUTABLE: Record pattern ensuring consistent result state
/// </summary>
internal record DataOperationResult
{
    public string OperationType { get; init; } = string.Empty;
    public bool Success { get; init; }
    public int ProcessedItems { get; init; }
    public int AffectedRows { get; init; }
    public TimeSpan Duration { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object?>? Metadata { get; init; }
    
    /// <summary>Processing rate (items per second)</summary>
    public double ProcessingRate
    {
        get
        {
            var seconds = Duration.TotalSeconds;
            return seconds > 0 ? ProcessedItems / seconds : 0;
        }
    }
    
    /// <summary>Success rate percentage</summary>
    public double SuccessRate
    {
        get
        {
            return ProcessedItems > 0 ? (double)AffectedRows / ProcessedItems * 100 : 100;
        }
    }
    
    public static DataOperationResult CreateSuccess(
        string operationType,
        int processedItems,
        int affectedRows,
        TimeSpan duration,
        string message = "Operation completed successfully",
        Dictionary<string, object?>? metadata = null) =>
        new()
        {
            OperationType = operationType,
            Success = true,
            ProcessedItems = processedItems,
            AffectedRows = affectedRows,
            Duration = duration,
            Message = message,
            Metadata = metadata
        };
    
    public static DataOperationResult CreateFailure(
        string operationType,
        string errorMessage,
        int processedItems = 0,
        TimeSpan duration = default,
        Dictionary<string, object?>? metadata = null) =>
        new()
        {
            OperationType = operationType,
            Success = false,
            ProcessedItems = processedItems,
            AffectedRows = 0,
            Duration = duration,
            Message = "Operation failed",
            ErrorMessage = errorMessage,
            Metadata = metadata
        };
}