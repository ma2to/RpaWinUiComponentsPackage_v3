using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;

/// <summary>
/// DDD: Value object representing import operation results
/// ENTERPRISE: Comprehensive import result with validation details
/// </summary>
public record ImportResult
{
    public int TotalRows { get; init; }
    public int ImportedRows { get; set; }
    public int RejectedRows { get; init; }
    public int SkippedRows { get; set; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = [];
    public ImportMode Mode { get; init; }
    public bool IsSuccess => RejectedRows == 0;
    public double SuccessRate => TotalRows > 0 ? (double)ImportedRows / TotalRows * 100 : 0;
    
    public static ImportResult Success(int totalRows, int importedRows, ImportMode mode) =>
        new()
        {
            TotalRows = totalRows,
            ImportedRows = importedRows,
            RejectedRows = totalRows - importedRows,
            SkippedRows = 0,
            Mode = mode
        };
    
    public static ImportResult WithErrors(int totalRows, int importedRows, IReadOnlyList<ValidationError> errors, ImportMode mode) =>
        new()
        {
            TotalRows = totalRows,
            ImportedRows = importedRows,
            RejectedRows = totalRows - importedRows,
            SkippedRows = 0,
            ValidationErrors = errors,
            Mode = mode
        };
}