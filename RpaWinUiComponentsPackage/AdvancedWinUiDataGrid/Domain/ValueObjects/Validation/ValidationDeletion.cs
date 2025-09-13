using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;

/// <summary>
/// ENTERPRISE: Validation deletion criteria for batch row operations
/// DDD: Value object representing deletion conditions
/// </summary>
internal record ValidationDeletionCriteria(
    ValidationDeletionMode Mode,
    IReadOnlyList<ValidationSeverity>? Severity = null,     // Zoznam závažností na zmazanie
    IReadOnlyList<string>? SpecificRuleNames = null,
    Func<IReadOnlyDictionary<string, object?>, bool>? CustomPredicate = null);

/// <summary>
/// ENTERPRISE: Validation deletion modes for different business scenarios
/// </summary>
internal enum ValidationDeletionMode
{
    DeleteInvalidRows,      // Delete rows that fail validation
    DeleteValidRows,        // Delete rows that pass validation  
    DeleteByCustomRule,     // Delete based on custom predicate
    DeleteBySeverity,       // Delete rows with specific severity levels
    DeleteByRuleName        // Delete rows failing specific named rules
}

/// <summary>
/// ENTERPRISE: Validation severity levels for filtering
/// </summary>
internal enum ValidationSeverity
{
    Success = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// ENTERPRISE: Options for validation-based deletion operations
/// </summary>
internal record ValidationDeletionOptions(
    bool RequireConfirmation = true,        // Vyžaduj potvrdenie pred zmazaním iba pre UI mode pri headless to potvrdenie bude vzdy false
    IProgress<ValidationDeletionProgress>? Progress = null  // Progress reporting
);

/// <summary>
/// ENTERPRISE: Result of validation-based deletion operation
/// IMMUTABLE: Record pattern for consistent operation results
/// </summary>
internal record ValidationBasedDeleteResult(
    int TotalRowsEvaluated,
    int RowsDeleted,
    int RemainingRows,
    IReadOnlyList<ValidationError> ValidationErrors,
    TimeSpan OperationDuration);

/// <summary>
/// ENTERPRISE: Progress reporting for validation deletion operations
/// </summary>
internal record ValidationDeletionProgress
{
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int RowsMarkedForDeletion { get; init; }
    public int RowsDeleted { get; init; }
    public string? CurrentOperation { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    
    /// <summary>Percentage of completion (0-100)</summary>
    public double PercentageComplete => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    
    public static ValidationDeletionProgress Create(
        int totalRows,
        int processedRows,
        int markedForDeletion = 0,
        int deleted = 0,
        string? operation = null,
        TimeSpan? elapsed = null) =>
        new()
        {
            TotalRows = totalRows,
            ProcessedRows = processedRows,
            RowsMarkedForDeletion = markedForDeletion,
            RowsDeleted = deleted,
            CurrentOperation = operation,
            ElapsedTime = elapsed ?? TimeSpan.Zero
        };
}