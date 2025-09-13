using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;

/// <summary>
/// DOCUMENTATION: Validation rule interface per specification
/// ENTERPRISE: Contract for custom validation rules
/// EXTENSIBLE: Supports single-cell, cross-column, and cross-row validation
/// </summary>
internal interface IValidationRule
{
    /// <summary>Rule name for identification and debugging</summary>
    string RuleName { get; }
    
    /// <summary>Validation rule type</summary>
    ValidationRuleType ValidationType { get; }
    
    /// <summary>Rule priority (higher numbers run first)</summary>
    int Priority { get; }
    
    /// <summary>Whether this rule is enabled</summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// DOCUMENTATION: Validate value according to rule specification
    /// ENTERPRISE: Async validation with full context support
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="rowData">Full row context for cross-column validation</param>
    /// <param name="rowIndex">Row index for context</param>
    /// <returns>Validation result with success/failure and error message</returns>
    Task<ValidationRuleResult> ValidateAsync(object? value, Dictionary<string, object?> rowData, int rowIndex);
}

/// <summary>
/// DOCUMENTATION: Validation rule types per specification
/// ENTERPRISE: Different validation scopes for flexibility
/// </summary>
internal enum ValidationRuleType
{
    /// <summary>Single cell validation</summary>
    Cell = 0,
    
    /// <summary>Cross-column validation within same row</summary>
    CrossColumn = 1,
    
    /// <summary>Entire row validation</summary>
    Row = 2,
    
    /// <summary>Cross-row validation (dataset level)</summary>
    CrossRow = 3,
    
    /// <summary>Global dataset validation</summary>
    Global = 4
}

/// <summary>
/// DOCUMENTATION: Validation rule result per specification
/// ENTERPRISE: Comprehensive validation result with error details
/// </summary>
internal record ValidationRuleResult
{
    /// <summary>Whether validation passed</summary>
    public bool IsValid { get; init; }
    
    /// <summary>Error message if validation failed</summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>Validation level (Error, Warning, Success)</summary>
    public RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationLevel Level { get; init; } = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationLevel.Error;
    
    /// <summary>Additional context or details</summary>
    public Dictionary<string, object?>? Context { get; init; }

    /// <summary>COMPATIBILITY: Alias for ErrorMessage</summary>
    public string? Error => ErrorMessage;
    
    /// <summary>Create successful validation result</summary>
    public static ValidationRuleResult Success() => new() { IsValid = true, Level = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationLevel.Success };
    
    /// <summary>Create failed validation result</summary>
    public static ValidationRuleResult Failure(string errorMessage, RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationLevel level = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationLevel.Error) => 
        new() { IsValid = false, ErrorMessage = errorMessage, Level = level };
    
    /// <summary>Create warning validation result</summary>
    public static ValidationRuleResult Warning(string warningMessage) => 
        new() { IsValid = true, ErrorMessage = warningMessage, Level = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationLevel.Warning };
}