using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;

/// <summary>
/// DDD: Rich domain model for DataGrid column definition
/// ENTERPRISE: Comprehensive column metadata supporting all business scenarios
/// IMMUTABLE: Value object following DDD principles with factory pattern
/// CLEAN API: Professional interface for column configuration
/// </summary>
internal record ColumnDefinition
{
    #region Core Properties
    
    /// <summary>Column name/identifier (required)</summary>
    public required string Name { get; init; }
    
    /// <summary>Display name for column header</summary>
    public string? DisplayName { get; init; }
    
    /// <summary>Data type for the column</summary>
    public required Type DataType { get; init; }
    
    /// <summary>Property name for data binding</summary>
    public string PropertyName { get; init; } = string.Empty;
    
    #endregion

    #region Behavior Configuration
    
    /// <summary>Is column required (cannot be null/empty)</summary>
    public bool IsRequired { get; init; }
    
    /// <summary>Is column read-only</summary>
    public bool IsReadOnly { get; init; }
    
    /// <summary>Is column visible</summary>
    public bool IsVisible { get; init; } = true;
    
    /// <summary>Allow sorting on this column</summary>
    public bool AllowSorting { get; init; } = true;
    
    /// <summary>Allow filtering on this column</summary>
    public bool AllowFiltering { get; init; } = true;
    
    /// <summary>Allow resizing this column</summary>
    public bool AllowResizing { get; init; } = true;
    
    /// <summary>Allow reordering this column</summary>
    public bool AllowReordering { get; init; } = true;
    
    #endregion

    #region Layout Configuration
    
    /// <summary>Column width specification</summary>
    public ColumnWidth Width { get; init; } = ColumnWidth.Auto();
    
    /// <summary>Column text alignment</summary>
    public ColumnAlignment Alignment { get; init; } = ColumnAlignment.Left;
    
    /// <summary>Display format for values (e.g., date/number formatting)</summary>
    public string? DisplayFormat { get; init; }
    
    /// <summary>Enable text wrapping for this column</summary>
    public bool EnableTextWrapping { get; init; } = true;
    
    /// <summary>Text wrapping mode for this column</summary>
    public Microsoft.UI.Xaml.TextWrapping TextWrapping { get; init; } = Microsoft.UI.Xaml.TextWrapping.Wrap;
    
    /// <summary>Column header tooltip</summary>
    public string? Tooltip { get; init; }
    
    #endregion

    #region Validation Configuration
    
    /// <summary>Validation rules for this column</summary>
    public IReadOnlyList<ColumnValidationRule> ValidationRules { get; init; } = Array.Empty<ColumnValidationRule>();
    
    /// <summary>Custom validation function name</summary>
    public string? ValidationRule { get; init; }
    
    /// <summary>Does this column require validation</summary>
    public bool RequiresValidation { get; init; }
    
    #endregion

    #region Special Column Support
    
    /// <summary>Special column type (CheckBox, DeleteRow, ValidAlerts, etc.)</summary>
    public SpecialColumnType SpecialType { get; init; } = SpecialColumnType.None;
    
    /// <summary>Configuration for special column types</summary>
    public SpecialColumnConfiguration? SpecialConfiguration { get; init; }
    
    #endregion

    #region Factory Methods - Basic Types
    
    /// <summary>
    /// FACTORY: Create standard text column
    /// USAGE: Most common column type for string data
    /// </summary>
    public static ColumnDefinition Text(string name, string? displayName = null, string? propertyName = null)
    {
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = typeof(string),
            PropertyName = propertyName ?? name,
            Alignment = ColumnAlignment.Left
        };
    }
    
    /// <summary>
    /// FACTORY: Create numeric column with proper alignment and formatting
    /// USAGE: For integers, decimals, doubles with right alignment
    /// </summary>
    public static ColumnDefinition Numeric<T>(string name, string? displayName = null, string? displayFormat = null) 
        where T : struct, IComparable<T>
    {
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = typeof(T),
            PropertyName = name,
            Alignment = ColumnAlignment.Right,
            DisplayFormat = displayFormat
        };
    }
    
    /// <summary>
    /// FACTORY: Create date/time column with formatting
    /// USAGE: For DateTime columns with proper formatting and center alignment
    /// </summary>
    public static ColumnDefinition DateTime(string name, string? displayName = null, string displayFormat = "yyyy-MM-dd")
    {
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = typeof(DateTime),
            PropertyName = name,
            Alignment = ColumnAlignment.Center,
            DisplayFormat = displayFormat
        };
    }
    
    /// <summary>
    /// FACTORY: Create boolean column
    /// USAGE: For true/false values with center alignment
    /// </summary>
    public static ColumnDefinition Boolean(string name, string? displayName = null)
    {
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = typeof(bool),
            PropertyName = name,
            Alignment = ColumnAlignment.Center,
            Width = ColumnWidth.Pixels(100)
        };
    }
    
    #endregion

    #region Factory Methods - Validation
    
    /// <summary>
    /// FACTORY: Create required column with validation
    /// USAGE: For mandatory fields that cannot be empty
    /// </summary>
    public static ColumnDefinition Required(string name, Type dataType, string? displayName = null, string? validationMessage = null)
    {
        var validationRule = ColumnValidationRule.Required(validationMessage);
        
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = dataType,
            PropertyName = name,
            IsRequired = true,
            RequiresValidation = true,
            ValidationRules = new[] { validationRule }
        };
    }
    
    /// <summary>
    /// FACTORY: Create column with custom validation rules
    /// USAGE: For complex business rule validation
    /// </summary>
    public static ColumnDefinition WithValidation(string name, Type dataType, params ColumnValidationRule[] validationRules)
    {
        return new ColumnDefinition
        {
            Name = name,
            DataType = dataType,
            PropertyName = name,
            RequiresValidation = true,
            ValidationRules = validationRules.ToList().AsReadOnly()
        };
    }
    
    #endregion

    #region Factory Methods - Special Columns
    
    /// <summary>
    /// FACTORY: Create CheckBox column for boolean values
    /// ENTERPRISE: Displays actual checkboxes instead of True/False text
    /// </summary>
    public static ColumnDefinition CheckBox(string name, string? displayName = null)
    {
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = typeof(bool),
            PropertyName = name,
            SpecialType = SpecialColumnType.CheckBox,
            Alignment = ColumnAlignment.Center,
            Width = ColumnWidth.Pixels(80),
            AllowSorting = true,
            AllowFiltering = true
        };
    }
    
    /// <summary>
    /// FACTORY: Create DeleteRow action column
    /// ENTERPRISE: Provides row deletion functionality with confirmation
    /// </summary>
    public static ColumnDefinition DeleteRow(string displayName = "Actions", bool requireConfirmation = true, string? confirmationMessage = null)
    {
        return new ColumnDefinition
        {
            Name = "_DeleteRow",
            DisplayName = displayName,
            DataType = typeof(object),
            PropertyName = "_DeleteRow",
            SpecialType = SpecialColumnType.DeleteRow,
            IsReadOnly = true,
            AllowSorting = false,
            AllowFiltering = false,
            Alignment = ColumnAlignment.Center,
            Width = ColumnWidth.Pixels(80),
            SpecialConfiguration = SpecialColumnConfiguration.ForDeleteButton(
                requireConfirmation: requireConfirmation,
                confirmationMessage: confirmationMessage ?? "Are you sure you want to delete this row?"
            )
        };
    }
    
    /// <summary>
    /// FACTORY: Create ValidAlerts column for validation error display
    /// ENTERPRISE: Always active, shows validation errors in format "ColumnName: Error Message"
    /// </summary>
    public static ColumnDefinition ValidAlerts(string displayName = "Validation Errors", double minimumWidth = 200)
    {
        return new ColumnDefinition
        {
            Name = "_ValidAlerts",
            DisplayName = displayName,
            DataType = typeof(string),
            PropertyName = "_ValidAlerts",
            SpecialType = SpecialColumnType.ValidAlerts,
            IsReadOnly = true,
            AllowSorting = false,
            AllowFiltering = false,
            Alignment = ColumnAlignment.Left,
            Width = ColumnWidth.Auto(), // Auto-width to fill remaining space
            SpecialConfiguration = SpecialColumnConfiguration.ForValidationAlerts(
                minimumWidth: minimumWidth
            )
        };
    }
    
    #endregion

    #region Advanced Factory Methods
    
    /// <summary>
    /// FACTORY: Create column with auto-detection based on name and type
    /// ENTERPRISE: Smart factory that configures column based on naming conventions
    /// </summary>
    public static ColumnDefinition AutoDetected(string name, Type dataType, string? displayName = null)
    {
        // Auto-detect special column types based on name patterns
        var specialType = DetectSpecialColumnType(name);
        
        return specialType switch
        {
            SpecialColumnType.CheckBox when dataType == typeof(bool) => CheckBox(name, displayName),
            SpecialColumnType.DeleteRow => DeleteRow(displayName ?? "Actions"),
            SpecialColumnType.ValidAlerts => ValidAlerts(displayName ?? "Validation Errors"),
            _ => CreateStandardColumn(name, dataType, displayName)
        };
    }
    
    /// <summary>
    /// FACTORY: Create read-only column
    /// USAGE: For computed or display-only data
    /// </summary>
    public static ColumnDefinition ReadOnly(string name, Type dataType, string? displayName = null)
    {
        return CreateStandardColumn(name, dataType, displayName) with 
        { 
            IsReadOnly = true,
            AllowSorting = true,
            AllowFiltering = true
        };
    }
    
    #endregion

    #region Private Helpers
    
    private static ColumnDefinition CreateStandardColumn(string name, Type dataType, string? displayName)
    {
        var alignment = GetDefaultAlignment(dataType);
        
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName,
            DataType = dataType,
            PropertyName = name,
            Alignment = alignment,
            Width = GetDefaultWidth(dataType)
        };
    }
    
    private static ColumnAlignment GetDefaultAlignment(Type dataType)
    {
        if (IsNumericType(dataType))
            return ColumnAlignment.Right;
        
        if (dataType == typeof(bool) || dataType == typeof(DateTime))
            return ColumnAlignment.Center;
        
        return ColumnAlignment.Left;
    }
    
    private static ColumnWidth GetDefaultWidth(Type dataType)
    {
        return dataType switch
        {
            _ when dataType == typeof(bool) => ColumnWidth.Pixels(80),
            _ when dataType == typeof(DateTime) => ColumnWidth.Pixels(120),
            _ when IsNumericType(dataType) => ColumnWidth.Pixels(100),
            _ => ColumnWidth.Auto()
        };
    }
    
    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(decimal) || 
               type == typeof(double) || type == typeof(float) || type == typeof(short) ||
               type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort);
    }
    
    private static SpecialColumnType DetectSpecialColumnType(string name)
    {
        var lowerName = name.ToLowerInvariant();
        
        return lowerName switch
        {
            _ when lowerName.Contains("checkbox") || lowerName.Contains("check") || lowerName.Contains("selected") => SpecialColumnType.CheckBox,
            _ when lowerName.Contains("delete") || lowerName.Contains("remove") || lowerName.Contains("action") => SpecialColumnType.DeleteRow,
            _ when lowerName.Contains("valid") || lowerName.Contains("error") || lowerName.Contains("alert") => SpecialColumnType.ValidAlerts,
            _ => SpecialColumnType.None
        };
    }
    
    #endregion
}

/// <summary>
/// DDD: Value object for column width specification
/// ENTERPRISE: Flexible width configuration supporting different strategies with min/max constraints
/// </summary>
internal record ColumnWidth
{
    public double Value { get; init; }
    public ColumnWidthType Type { get; init; }
    public double? MinWidth { get; init; }
    public double? MaxWidth { get; init; }
    
    public static ColumnWidth Auto(double? minWidth = null, double? maxWidth = null) => 
        new() { Type = ColumnWidthType.Auto, MinWidth = minWidth, MaxWidth = maxWidth };
    
    public static ColumnWidth Star(double value = 1.0, double? minWidth = null, double? maxWidth = null) => 
        new() { Value = value, Type = ColumnWidthType.Star, MinWidth = minWidth, MaxWidth = maxWidth };
    
    public static ColumnWidth Pixels(double pixels, double? minWidth = null, double? maxWidth = null) => 
        new() { Value = pixels, Type = ColumnWidthType.Pixels, MinWidth = minWidth, MaxWidth = maxWidth };
    
    public enum ColumnWidthType
    {
        Auto,
        Star,
        Pixels
    }
}

/// <summary>
/// DDD: Column validation rule value object
/// ENTERPRISE: Business rule validation at domain level
/// </summary>
internal record ColumnValidationRule
{
    public required string RuleName { get; init; }
    public required Func<object?, ValidationError?> Validator { get; init; }
    public string? ErrorMessage { get; init; }
    
    /// <summary>Factory: Create required field validation rule</summary>
    public static ColumnValidationRule Required(string? customMessage = null) =>
        new()
        {
            RuleName = "Required",
            ErrorMessage = customMessage ?? "Field is required",
            Validator = value => value == null || (value is string str && string.IsNullOrWhiteSpace(str))
                ? ValidationError.Create("Value", customMessage ?? "Field is required", value)
                : null
        };
    
    /// <summary>Factory: Create maximum length validation rule</summary>
    public static ColumnValidationRule MaxLength(int maxLength, string? customMessage = null) =>
        new()
        {
            RuleName = "MaxLength",
            ErrorMessage = customMessage ?? $"Maximum length is {maxLength} characters",
            Validator = value => value is string str && str.Length > maxLength
                ? ValidationError.Create("Value", customMessage ?? $"Maximum length is {maxLength} characters", value)
                : null
        };
    
    /// <summary>Factory: Create custom validation rule</summary>
    public static ColumnValidationRule Custom(string ruleName, Func<object?, bool> validator, string errorMessage) =>
        new()
        {
            RuleName = ruleName,
            ErrorMessage = errorMessage,
            Validator = value => validator(value) ? null : ValidationError.Create("Value", errorMessage, value)
        };
}

/// <summary>
/// ENTERPRISE: Column alignment options
/// UI: Visual alignment configuration for data display
/// </summary>
internal enum ColumnAlignment
{
    /// <summary>Left-aligned (default for text)</summary>
    Left,
    
    /// <summary>Center-aligned (default for booleans, dates)</summary>
    Center,
    
    /// <summary>Right-aligned (default for numbers)</summary>
    Right
}