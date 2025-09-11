using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;

/// <summary>
/// DDD: Value object for comprehensive validation configuration
/// ENTERPRISE: Professional validation supporting all business scenarios
/// IMMUTABLE: Record pattern ensuring configuration consistency
/// COMPREHENSIVE: Combines timing, rules, performance, and UI aspects
/// </summary>
public record ValidationConfiguration
{
    #region Core Validation Settings
    
    /// <summary>Enable validation system</summary>
    public bool EnableValidation { get; init; } = true;
    
    /// <summary>Enable real-time validation during editing</summary>
    public bool EnableRealTimeValidation { get; init; } = true;
    
    /// <summary>Show validation errors in UI</summary>
    public bool ShowValidationErrors { get; init; } = true;
    
    /// <summary>Show validation warnings in UI</summary>
    public bool ShowValidationWarnings { get; init; } = true;
    
    /// <summary>Show validation indicators in UI</summary>
    public bool ShowValidationIndicators { get; init; } = true;
    
    /// <summary>Stop validation on first error encountered</summary>
    public bool StopOnFirstError { get; init; } = false;
    
    /// <summary>Maximum number of validation errors to collect (null = unlimited)</summary>
    public int? MaxValidationErrors { get; init; } = null;
    
    #endregion

    #region Validation Timing Configuration
    
    /// <summary>Validation mode - controls when validation occurs</summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.OnEdit;
    
    /// <summary>Validate on data import operations</summary>
    public bool ValidateOnImport { get; init; } = true;
    
    /// <summary>Validate on data export operations</summary>
    public bool ValidateOnExport { get; init; } = true;
    
    /// <summary>Validate on individual cell edit</summary>
    public bool ValidateOnCellEdit { get; init; } = true;
    
    /// <summary>Validate when moving between rows</summary>
    public bool ValidateOnRowChange { get; init; } = true;
    
    /// <summary>Validate empty rows (performance consideration)</summary>
    public bool ValidateEmptyRows { get; init; } = false;
    
    #endregion

    #region Performance and Timeout Settings
    
    /// <summary>Global validation operation timeout</summary>
    public TimeSpan ValidationTimeout { get; init; } = TimeSpan.FromSeconds(30);
    
    /// <summary>Enable strict validation (fail-fast approach)</summary>
    public bool StrictValidation { get; init; } = false;
    
    #endregion

    #region Advanced Validation Rules
    
    /// <summary>Column-specific validation rules</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<ValidationRule>> ColumnValidationRules { get; init; } = 
        new Dictionary<string, IReadOnlyList<ValidationRule>>();
    
    /// <summary>Cross-column validation rules (relationships between columns)</summary>
    public IReadOnlyList<CrossColumnValidationRule> CrossColumnValidationRules { get; init; } = 
        Array.Empty<CrossColumnValidationRule>();
    
    /// <summary>Global validation rules (entire dataset validation)</summary>
    public IReadOnlyList<GlobalValidationRule> GlobalValidationRules { get; init; } = 
        Array.Empty<GlobalValidationRule>();
    
    /// <summary>Columns that must have unique values across the dataset</summary>
    public IReadOnlyList<string> UniqueColumns { get; init; } = Array.Empty<string>();
    
    #endregion

    #region Custom Error Message Configuration
    
    /// <summary>Custom error message formats by validation type</summary>
    public IReadOnlyDictionary<string, string> ErrorMessageFormats { get; init; } = 
        new Dictionary<string, string>();
    
    /// <summary>Custom warning message formats by validation type</summary>
    public IReadOnlyDictionary<string, string> WarningMessageFormats { get; init; } = 
        new Dictionary<string, string>();
    
    /// <summary>Default error message for unknown validation failures</summary>
    public string DefaultErrorMessage { get; init; } = "Validation failed";
    
    /// <summary>Default warning message</summary>
    public string DefaultWarningMessage { get; init; } = "Validation warning";
    
    #endregion

    #region Legacy Compatibility (String-based validators)
    
    /// <summary>
    /// Legacy: Custom validation rules per column (string-based)
    /// NOTE: Prefer ColumnValidationRules for type-safe validation
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> LegacyColumnValidators { get; init; } = 
        new Dictionary<string, IReadOnlyList<string>>();
    
    /// <summary>
    /// Legacy: Global validation rules (string-based)
    /// NOTE: Prefer GlobalValidationRules for type-safe validation
    /// </summary>
    public IReadOnlyList<string> LegacyGlobalValidators { get; init; } = Array.Empty<string>();
    
    /// <summary>
    /// Legacy: Cross-column validation rules (string-based)
    /// NOTE: Prefer CrossColumnValidationRules for type-safe validation
    /// </summary>
    public IReadOnlyDictionary<string, string> LegacyCrossColumnValidators { get; init; } = 
        new Dictionary<string, string>();
    
    /// <summary>
    /// Legacy: Cross-row validation rules (string-based)
    /// NOTE: Use GlobalValidationRules for similar functionality
    /// </summary>
    public IReadOnlyList<string> LegacyCrossRowValidators { get; init; } = Array.Empty<string>();
    
    #endregion

    #region Factory Methods - Standard Presets
    
    /// <summary>
    /// FACTORY: Default balanced validation configuration
    /// RECOMMENDED: Good balance for most business applications
    /// </summary>
    public static ValidationConfiguration Default => Comprehensive;
    
    /// <summary>
    /// FACTORY: Comprehensive validation configuration
    /// ENTERPRISE: Complete validation with detailed reporting
    /// USAGE: Full-featured validation for critical business data
    /// </summary>
    public static ValidationConfiguration Comprehensive => new()
    {
        // Core validation
        EnableValidation = true,
        EnableRealTimeValidation = true,
        ShowValidationErrors = true,
        ShowValidationWarnings = true,
        ShowValidationIndicators = true,
        StopOnFirstError = false,
        MaxValidationErrors = null, // Unlimited
        
        // Timing
        ValidationMode = ValidationMode.OnEdit,
        ValidateOnImport = true,
        ValidateOnExport = true,
        ValidateOnCellEdit = true,
        ValidateOnRowChange = true,
        ValidateEmptyRows = false,
        
        // Performance
        ValidationTimeout = TimeSpan.FromSeconds(30),
        StrictValidation = false,
        
        // Messages
        DefaultErrorMessage = "Validation failed",
        DefaultWarningMessage = "Validation warning"
    };
    
    /// <summary>
    /// FACTORY: Strict validation configuration
    /// ENTERPRISE: Maximum validation with immediate feedback
    /// USAGE: Critical systems requiring immediate validation feedback
    /// </summary>
    public static ValidationConfiguration Strict => new()
    {
        // Core validation
        EnableValidation = true,
        EnableRealTimeValidation = true,
        ShowValidationErrors = true,
        ShowValidationWarnings = true,
        ShowValidationIndicators = true,
        StopOnFirstError = true,
        MaxValidationErrors = 10, // Limit for strict mode
        
        // Timing
        ValidationMode = ValidationMode.RealTime,
        ValidateOnImport = true,
        ValidateOnExport = true,
        ValidateOnCellEdit = true,
        ValidateOnRowChange = true,
        ValidateEmptyRows = true,
        
        // Performance
        ValidationTimeout = TimeSpan.FromSeconds(10),
        StrictValidation = true,
        
        // Messages
        DefaultErrorMessage = "Validation error - immediate attention required",
        DefaultWarningMessage = "Validation warning - please review"
    };
    
    /// <summary>
    /// FACTORY: Relaxed validation configuration
    /// PERFORMANCE: Basic validation with minimal performance impact
    /// USAGE: High-volume scenarios where performance is critical
    /// </summary>
    public static ValidationConfiguration Relaxed => new()
    {
        // Core validation
        EnableValidation = true,
        EnableRealTimeValidation = false,
        ShowValidationErrors = true,
        ShowValidationWarnings = true,
        ShowValidationIndicators = true,
        StopOnFirstError = false,
        MaxValidationErrors = 100,
        
        // Timing
        ValidationMode = ValidationMode.OnSave,
        ValidateOnImport = true,
        ValidateOnExport = false,
        ValidateOnCellEdit = false,
        ValidateOnRowChange = false,
        ValidateEmptyRows = false,
        
        // Performance
        ValidationTimeout = TimeSpan.FromMinutes(2),
        StrictValidation = false,
        
        // Messages
        DefaultErrorMessage = "Validation issue detected",
        DefaultWarningMessage = "Please review this field"
    };
    
    /// <summary>
    /// FACTORY: Fast validation configuration
    /// PERFORMANCE: Optimized for maximum performance with minimal validation
    /// USAGE: Large datasets where validation overhead must be minimized
    /// </summary>
    public static ValidationConfiguration Fast => new()
    {
        // Core validation
        EnableValidation = true,
        EnableRealTimeValidation = false,
        ShowValidationErrors = true,
        ShowValidationWarnings = false,
        ShowValidationIndicators = false,
        StopOnFirstError = true,
        MaxValidationErrors = 10, // Limit for fast mode
        
        // Timing
        ValidationMode = ValidationMode.OnSave,
        ValidateOnImport = false,
        ValidateOnExport = false,
        ValidateOnCellEdit = false,
        ValidateOnRowChange = false,
        ValidateEmptyRows = false,
        
        // Performance
        ValidationTimeout = TimeSpan.FromSeconds(5),
        StrictValidation = false,
        
        // Messages
        DefaultErrorMessage = "Error",
        DefaultWarningMessage = "Warning"
    };
    
    /// <summary>
    /// FACTORY: Disabled validation configuration
    /// PERFORMANCE: No validation for maximum performance
    /// USAGE: Trusted data scenarios or performance-critical applications
    /// </summary>
    public static ValidationConfiguration Disabled => new()
    {
        // Core validation
        EnableValidation = false,
        EnableRealTimeValidation = false,
        ShowValidationErrors = false,
        ShowValidationWarnings = false,
        ShowValidationIndicators = false,
        StopOnFirstError = false,
        MaxValidationErrors = 0,
        
        // Timing
        ValidationMode = ValidationMode.None,
        ValidateOnImport = false,
        ValidateOnExport = false,
        ValidateOnCellEdit = false,
        ValidateOnRowChange = false,
        ValidateEmptyRows = false,
        
        // Performance
        ValidationTimeout = TimeSpan.FromSeconds(1),
        StrictValidation = false,
        
        // Messages
        DefaultErrorMessage = string.Empty,
        DefaultWarningMessage = string.Empty
    };
    
    #endregion

    #region Custom Factory Methods
    
    /// <summary>
    /// FACTORY: Custom validation configuration with fluent builder
    /// EXTENSIBILITY: Allow fine-grained customization
    /// </summary>
    public static ValidationConfiguration Custom(Action<ValidationConfigurationBuilder>? configurator = null)
    {
        var builder = new ValidationConfigurationBuilder();
        configurator?.Invoke(builder);
        return builder.Build();
    }
    
    /// <summary>
    /// FACTORY: Create configuration optimized for dataset size
    /// ADAPTIVE: Automatically adjust settings based on expected data volume
    /// </summary>
    public static ValidationConfiguration ForDatasetSize(int expectedRows)
    {
        return expectedRows switch
        {
            < 1000 => Comprehensive,
            < 10000 => CreateDefault(),
            < 100000 => Relaxed,
            _ => Fast
        };
    }

    /// <summary>
    /// COMPATIBILITY: Default configuration factory method
    /// </summary>
    public static ValidationConfiguration CreateDefault() => new();
    
    #endregion
}

/// <summary>
/// BUILDER: Fluent builder for ValidationConfiguration
/// PROFESSIONAL: Type-safe configuration with validation
/// </summary>
public class ValidationConfigurationBuilder
{
    private ValidationConfiguration _config;
    
    public ValidationConfigurationBuilder() : this(ValidationConfiguration.Comprehensive) { }
    
    public ValidationConfigurationBuilder(ValidationConfiguration baseConfig)
    {
        _config = baseConfig;
    }
    
    public ValidationConfigurationBuilder WithCore(
        bool enableValidation = true, 
        bool enableRealTime = true,
        bool showErrors = true, 
        bool showWarnings = true,
        bool stopOnFirstError = false,
        int? maxErrors = null)
    {
        _config = _config with
        {
            EnableValidation = enableValidation,
            EnableRealTimeValidation = enableRealTime,
            ShowValidationErrors = showErrors,
            ShowValidationWarnings = showWarnings,
            StopOnFirstError = stopOnFirstError,
            MaxValidationErrors = maxErrors
        };
        return this;
    }
    
    public ValidationConfigurationBuilder WithTiming(
        ValidationMode mode = ValidationMode.OnEdit,
        bool onImport = true,
        bool onExport = true,
        bool onCellEdit = true,
        bool onRowChange = true,
        bool validateEmptyRows = false)
    {
        _config = _config with
        {
            ValidationMode = mode,
            ValidateOnImport = onImport,
            ValidateOnExport = onExport,
            ValidateOnCellEdit = onCellEdit,
            ValidateOnRowChange = onRowChange,
            ValidateEmptyRows = validateEmptyRows
        };
        return this;
    }
    
    public ValidationConfigurationBuilder WithPerformance(
        TimeSpan? timeout = null, 
        bool strictValidation = false)
    {
        _config = _config with
        {
            ValidationTimeout = timeout ?? TimeSpan.FromSeconds(30),
            StrictValidation = strictValidation
        };
        return this;
    }
    
    public ValidationConfigurationBuilder WithStrictValidation(bool strict = true)
    {
        _config = _config with { StrictValidation = strict };
        return this;
    }
    
    public ValidationConfigurationBuilder ValidateEmptyRows(bool validate = true)
    {
        _config = _config with { ValidateEmptyRows = validate };
        return this;
    }
    
    public ValidationConfigurationBuilder AddColumnRule(string columnName, ValidationRule rule)
    {
        var currentRules = _config.ColumnValidationRules.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value.ToList());
            
        if (!currentRules.ContainsKey(columnName))
            currentRules[columnName] = new List<ValidationRule>();
        
        currentRules[columnName].Add(rule);
        
        _config = _config with
        {
            ColumnValidationRules = currentRules.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<ValidationRule>)kvp.Value)
        };
        return this;
    }
    
    public ValidationConfigurationBuilder AddUniqueColumn(string columnName)
    {
        var currentUniqueColumns = _config.UniqueColumns.ToList();
        if (!currentUniqueColumns.Contains(columnName))
            currentUniqueColumns.Add(columnName);
            
        _config = _config with { UniqueColumns = currentUniqueColumns };
        return this;
    }
    
    public ValidationConfigurationBuilder WithErrorMessages(
        string? defaultError = null, 
        string? defaultWarning = null)
    {
        _config = _config with
        {
            DefaultErrorMessage = defaultError ?? _config.DefaultErrorMessage,
            DefaultWarningMessage = defaultWarning ?? _config.DefaultWarningMessage
        };
        return this;
    }
    
    public ValidationConfiguration Build() => _config;
}

/// <summary>
/// ENTERPRISE: Validation modes enumeration
/// DDD: Domain concept for validation timing strategies
/// </summary>
public enum ValidationMode
{
    /// <summary>No validation performed</summary>
    None = 0,

    /// <summary>Validate when user edits cells</summary>
    OnEdit = 1,

    /// <summary>Validate when user saves/commits data</summary>
    OnSave = 2,

    /// <summary>Validate in real-time as user types</summary>
    RealTime = 3
}

/// <summary>
/// ENTERPRISE: Validation severity levels
/// DDD: Domain concept for validation result classification
/// </summary>
public enum ValidationLevel
{
    /// <summary>Validation passed successfully</summary>
    Success = 0,

    /// <summary>Warning condition - not critical but should be reviewed</summary>
    Warning = 1,

    /// <summary>Error condition - requires immediate attention</summary>
    Error = 2
}