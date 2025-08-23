namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;

/// <summary>
/// PUBLIC Interface for validation configuration - Applications implement this
/// SEPARATION: Public interface in Domain, internal implementation in modules
/// CONFIGURATION: Applications define their own business validation rules
/// </summary>
public interface IValidationConfiguration
{
    /// <summary>
    /// Is validation enabled
    /// APPLICATION-DEFINED: Application controls validation on/off
    /// </summary>
    bool IsValidationEnabled { get; }

    /// <summary>
    /// Enable real-time validation (during typing)
    /// PERFORMANCE: Applications can disable for better performance
    /// </summary>
    bool EnableRealtimeValidation { get; }

    /// <summary>
    /// Enable batch validation (during import/paste operations)
    /// BULK OPERATIONS: Applications can configure batch validation behavior
    /// </summary>
    bool EnableBatchValidation { get; }

    /// <summary>
    /// Get validation rules for all columns
    /// APPLICATION BUSINESS LOGIC: Applications implement their own validation rules
    /// </summary>
    ValidationRuleSet GetValidationRules();

    /// <summary>
    /// Get cross-row validation rules (rules that validate across multiple rows)
    /// CROSS-VALIDATION: Rules that check relationships between rows/columns
    /// </summary>
    IReadOnlyList<CrossValidationRule> GetCrossRowValidationRules();
}

/// <summary>
/// Validation rule set containing all column validation rules
/// ORGANIZATION: Groups validation rules by column for efficient lookup
/// </summary>
public sealed class ValidationRuleSet
{
    private readonly Dictionary<string, List<ValidationRule>> _columnRules = new();

    /// <summary>
    /// Add validation rule for specific column
    /// BUILDER PATTERN: Fluent API for building validation rules
    /// </summary>
    public ValidationRuleSet AddRule(string columnName, ValidationRule rule)
    {
        if (!_columnRules.ContainsKey(columnName))
            _columnRules[columnName] = new List<ValidationRule>();

        _columnRules[columnName].Add(rule);
        return this;
    }

    /// <summary>
    /// Get validation rules for specific column
    /// LOOKUP: Efficient rule retrieval by column name
    /// </summary>
    public IReadOnlyList<ValidationRule> GetRules(string columnName)
    {
        return _columnRules.TryGetValue(columnName, out var rules) 
            ? rules.AsReadOnly() 
            : Array.Empty<ValidationRule>();
    }

    /// <summary>
    /// Get all columns that have validation rules
    /// INTROSPECTION: Applications can query which columns are validated
    /// </summary>
    public IEnumerable<string> GetValidatedColumns()
    {
        return _columnRules.Keys;
    }

    /// <summary>
    /// Check if column has any validation rules
    /// PERFORMANCE: Quick check before running validation
    /// </summary>
    public bool HasRules(string columnName)
    {
        return _columnRules.ContainsKey(columnName) && _columnRules[columnName].Count > 0;
    }

    /// <summary>
    /// Total number of validation rules across all columns
    /// STATISTICS: Useful for performance analysis and reporting
    /// </summary>
    public int TotalRulesCount => _columnRules.Values.Sum(rules => rules.Count);

    /// <summary>
    /// Empty validation rule set (no validation)
    /// FACTORY: Default empty set for when validation is disabled
    /// </summary>
    public static ValidationRuleSet Empty => new();
}

/// <summary>
/// Individual validation rule for a column
/// FUNCTIONAL: Immutable validation rule with custom logic
/// </summary>
public sealed class ValidationRule
{
    /// <summary>
    /// Rule name for identification and debugging
    /// IDENTIFICATION: Unique name for rule management
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Validation function (returns ValidationResult)
    /// FLEXIBILITY: Applications can implement any validation logic
    /// </summary>
    public Func<object?, SingleValidationResult> Validator { get; }

    /// <summary>
    /// Error message when validation fails
    /// APPLICATION-DEFINED: Custom error messages from applications
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Rule priority (higher values = higher priority)
    /// ORDERING: Rules with higher priority run first
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Is rule currently enabled
    /// DYNAMIC CONTROL: Applications can enable/disable rules dynamically
    /// </summary>
    public bool IsEnabled { get; }

    public ValidationRule(
        string name, 
        Func<object?, SingleValidationResult> validator, 
        string errorMessage, 
        int priority = 0, 
        bool isEnabled = true)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Validator = validator ?? throw new ArgumentNullException(nameof(validator));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        Priority = priority;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Validate value using this rule
    /// EXECUTION: Safe validation with error handling
    /// </summary>
    public SingleValidationResult Validate(object? value)
    {
        if (!IsEnabled)
            return SingleValidationResult.Success();

        try
        {
            return Validator(value);
        }
        catch (Exception ex)
        {
            return SingleValidationResult.Error($"Validation rule '{Name}' failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Create simple validation rule with boolean validator
    /// CONVENIENCE: Easy creation for simple true/false validations
    /// </summary>
    public static ValidationRule Create(
        string name, 
        Func<object?, bool> simpleValidator, 
        string errorMessage, 
        int priority = 0)
    {
        return new ValidationRule(
            name, 
            value => simpleValidator(value) ? SingleValidationResult.Success() : SingleValidationResult.Error(errorMessage),
            errorMessage, 
            priority);
    }

    /// <summary>
    /// Create required field validation rule
    /// COMMON PATTERN: Standard required field validation
    /// </summary>
    public static ValidationRule Required(string fieldName) =>
        Create(
            $"Required_{fieldName}",
            value => value != null && !string.IsNullOrWhiteSpace(value.ToString()),
            $"Field '{fieldName}' is required");

    /// <summary>
    /// Create maximum length validation rule
    /// COMMON PATTERN: Standard length validation
    /// </summary>
    public static ValidationRule MaxLength(string fieldName, int maxLength) =>
        Create(
            $"MaxLength_{fieldName}_{maxLength}",
            value => value?.ToString()?.Length <= maxLength,
            $"Field '{fieldName}' cannot exceed {maxLength} characters");

    /// <summary>
    /// Create regex pattern validation rule
    /// COMMON PATTERN: Standard pattern matching validation
    /// </summary>
    public static ValidationRule Pattern(string fieldName, string pattern, string description) =>
        Create(
            $"Pattern_{fieldName}",
            value => value == null || System.Text.RegularExpressions.Regex.IsMatch(value.ToString() ?? "", pattern),
            $"Field '{fieldName}' must match pattern: {description}");
}

/// <summary>
/// Cross-validation rule that validates across multiple rows/columns
/// CROSS-VALIDATION: Complex business rules that span multiple data points
/// </summary>
public sealed class CrossValidationRule
{
    /// <summary>
    /// Rule name for identification
    /// IDENTIFICATION: Unique name for cross-validation rule
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Validation function for entire dataset
    /// DATASET VALIDATION: Validates relationships across all data
    /// </summary>
    public Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, CrossValidationResult> Validator { get; }

    /// <summary>
    /// Is rule currently enabled
    /// DYNAMIC CONTROL: Applications can enable/disable cross-validation
    /// </summary>
    public bool IsEnabled { get; }

    public CrossValidationRule(
        string name,
        Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, CrossValidationResult> validator,
        bool isEnabled = true)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Validator = validator ?? throw new ArgumentNullException(nameof(validator));
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Validate entire dataset using this rule
    /// EXECUTION: Safe cross-validation with error handling
    /// </summary>
    public CrossValidationResult Validate(IReadOnlyList<IReadOnlyDictionary<string, object?>> allRowData)
    {
        if (!IsEnabled)
            return CrossValidationResult.Success();

        try
        {
            return Validator(allRowData);
        }
        catch (Exception ex)
        {
            return CrossValidationResult.Error($"Cross-validation rule '{Name}' failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Result of single field validation
/// FUNCTIONAL: Immutable validation result
/// </summary>
public record SingleValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    Dictionary<string, object?>? AdditionalInfo = null)
{
    public static SingleValidationResult Success() => new(true);
    public static SingleValidationResult Error(string message) => new(false, message);
    public static SingleValidationResult Warning(string message) => new(true, message);
}

/// <summary>
/// Result of cross-validation across multiple rows
/// CROSS-VALIDATION: Captures validation results that span multiple rows
/// </summary>
public record CrossValidationResult(
    bool IsValid,
    string? GlobalErrorMessage = null,
    IReadOnlyDictionary<int, string>? RowErrors = null)
{
    public static CrossValidationResult Success() => new(true);
    public static CrossValidationResult Error(string globalMessage) => new(false, globalMessage);
    public static CrossValidationResult ErrorWithRowDetails(Dictionary<int, string> rowErrors) => 
        new(false, null, rowErrors);
}

/// <summary>
/// Simple validation configuration implementation for testing/defaults
/// CONVENIENCE: Basic implementation for simple scenarios
/// </summary>
internal sealed class SimpleValidationConfiguration : IValidationConfiguration
{
    public bool IsValidationEnabled { get; }
    public bool EnableRealtimeValidation { get; }
    public bool EnableBatchValidation { get; }

    private readonly ValidationRuleSet _rules;
    private readonly IReadOnlyList<CrossValidationRule> _crossRules;

    public SimpleValidationConfiguration(
        bool isEnabled = true,
        bool enableRealtime = true,
        bool enableBatch = true,
        ValidationRuleSet? rules = null,
        IReadOnlyList<CrossValidationRule>? crossRules = null)
    {
        IsValidationEnabled = isEnabled;
        EnableRealtimeValidation = enableRealtime;
        EnableBatchValidation = enableBatch;
        _rules = rules ?? ValidationRuleSet.Empty;
        _crossRules = crossRules ?? Array.Empty<CrossValidationRule>();
    }

    public ValidationRuleSet GetValidationRules() => _rules;
    public IReadOnlyList<CrossValidationRule> GetCrossRowValidationRules() => _crossRules;

    /// <summary>
    /// Disabled validation configuration
    /// FACTORY: No validation configuration
    /// </summary>
    public static SimpleValidationConfiguration Disabled => new(false, false, false);

    /// <summary>
    /// Default validation configuration with basic rules
    /// FACTORY: Standard validation setup
    /// </summary>
    public static SimpleValidationConfiguration Default => new(true, true, true);
}