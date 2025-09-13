using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;

/// <summary>
/// DDD: Value object for single-column validation rule
/// ENTERPRISE: Business rule validation at domain level
/// </summary>
internal record ValidationRule
{
    public required string RuleName { get; init; }
    public required string ColumnName { get; init; }
    public required Func<object?, ValidationError?> Validator { get; init; }
    public string? ErrorMessage { get; init; }
    public int Priority { get; init; } = 0;
    
    public static ValidationRule Required(string columnName, string? customMessage = null) =>
        new()
        {
            RuleName = "Required",
            ColumnName = columnName,
            ErrorMessage = customMessage ?? $"{columnName} is required",
            Validator = value => value == null || (value is string str && string.IsNullOrWhiteSpace(str))
                ? ValidationError.Create(columnName, customMessage ?? $"{columnName} is required", value)
                : null
        };
}

/// <summary>
/// DDD: Value object for cross-column validation rule
/// ENTERPRISE: Business rule validation across multiple columns
/// </summary>
internal record CrossColumnValidationRule
{
    public required string RuleName { get; init; }
    public required string[] ColumnNames { get; init; }
    public required Func<Dictionary<string, object?>, ValidationError?> Validator { get; init; }
    public string? ErrorMessage { get; init; }
    public int Priority { get; init; } = 0;
}

/// <summary>
/// DDD: Value object for global validation rule
/// ENTERPRISE: Business rule validation across entire row/grid
/// </summary>
internal record GlobalValidationRule
{
    public required string RuleName { get; init; }
    public required Func<List<Dictionary<string, object?>>, ValidationError[]> Validator { get; init; }
    public string? ErrorMessage { get; init; }
    public int Priority { get; init; } = 0;
}

