using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;

/// <summary>
/// DDD: Value object for row management configuration
/// ENTERPRISE: Comprehensive row lifecycle management configuration
/// IMMUTABLE: Record pattern ensuring configuration consistency
/// COMPREHENSIVE: All aspects of row creation, deletion, editing, and interaction
/// </summary>
internal record RowManagementConfiguration
{
    #region Row Lifecycle Management
    
    /// <summary>Allow adding new rows to the grid</summary>
    public bool AllowAddRows { get; init; } = true;
    
    /// <summary>Allow deleting existing rows from the grid</summary>
    public bool AllowDeleteRows { get; init; } = true;
    
    /// <summary>Allow editing existing row data</summary>
    public bool AllowEditRows { get; init; } = true;
    
    /// <summary>Allow reordering rows (drag and drop)</summary>
    public bool AllowReorderRows { get; init; } = true;
    
    #endregion

    #region Row Limits and Constraints
    
    /// <summary>Minimum number of rows that must exist</summary>
    public int MinRows { get; init; } = 0;
    
    /// <summary>Minimum number of rows (alternative property for compatibility)</summary>
    public int MinimumRows { get; init; } = 1;
    
    /// <summary>Maximum number of rows allowed (0 = unlimited)</summary>
    public int MaxRows { get; init; } = int.MaxValue;
    
    /// <summary>Maximum number of rows (alternative property for compatibility)</summary>
    public int MaximumRows { get; init; } = 0;
    
    /// <summary>Effective minimum rows (uses the maximum of both min properties)</summary>
    public int EffectiveMinRows => Math.Max(MinRows, MinimumRows);
    
    /// <summary>Effective maximum rows (uses the minimum of both max properties, 0 treated as unlimited)</summary>
    public int EffectiveMaxRows
    {
        get
        {
            var max1 = MaxRows == int.MaxValue ? 0 : MaxRows;
            var max2 = MaximumRows;
            
            if (max1 == 0 && max2 == 0) return int.MaxValue; // Both unlimited
            if (max1 == 0) return max2; // First unlimited, use second
            if (max2 == 0) return max1; // Second unlimited, use first
            return Math.Min(max1, max2); // Both have limits, use smaller
        }
    }
    
    #endregion

    #region UI and User Experience
    
    /// <summary>Automatically add an empty row at the end for easy data entry</summary>
    public bool AutoAddEmptyRow { get; init; } = true;
    
    /// <summary>Auto-generate row numbers for display</summary>
    public bool AutoGenerateRowNumbers { get; init; } = false;
    
    /// <summary>Show row numbers in the grid</summary>
    public bool ShowRowNumbers { get; init; } = false;
    
    /// <summary>Require confirmation before deleting rows</summary>
    public bool ConfirmDelete { get; init; } = true;
    
    #endregion

    #region Selection Configuration
    
    /// <summary>Enable row selection functionality</summary>
    public bool EnableSelection { get; init; } = true;
    
    /// <summary>Allow multiple row selection</summary>
    public bool AllowMultiSelect { get; init; } = true;
    
    #endregion

    #region Factory Methods - Standard Presets
    
    /// <summary>
    /// FACTORY: Default row management configuration
    /// BALANCED: Good balance for most business applications
    /// </summary>
    public static RowManagementConfiguration Default => new()
    {
        // Lifecycle
        AllowAddRows = true,
        AllowDeleteRows = true,
        AllowEditRows = true,
        AllowReorderRows = true,
        
        // Limits
        MinRows = 0,
        MinimumRows = 1,
        MaxRows = int.MaxValue,
        MaximumRows = 0,
        
        // UI/UX
        AutoAddEmptyRow = true,
        AutoGenerateRowNumbers = false,
        ShowRowNumbers = false,
        ConfirmDelete = true,
        
        // Selection
        EnableSelection = true,
        AllowMultiSelect = true
    };
    
    /// <summary>
    /// FACTORY: Read-only row management configuration
    /// SECURITY: Prevents any modifications to existing data
    /// USAGE: Display-only grids, reports, data review scenarios
    /// </summary>
    public static RowManagementConfiguration ReadOnly => new()
    {
        // Lifecycle - all disabled
        AllowAddRows = false,
        AllowDeleteRows = false,
        AllowEditRows = false,
        AllowReorderRows = false,
        
        // Limits
        MinRows = 0,
        MinimumRows = 0,
        MaxRows = int.MaxValue,
        MaximumRows = 0,
        
        // UI/UX
        AutoAddEmptyRow = false,
        AutoGenerateRowNumbers = false,
        ShowRowNumbers = true, // Helpful for read-only reference
        ConfirmDelete = false, // Not applicable
        
        // Selection
        EnableSelection = true, // Allow selection for copying
        AllowMultiSelect = true
    };
    
    /// <summary>
    /// FACTORY: Restrictive row management configuration
    /// ENTERPRISE: Controlled environment with strict limits
    /// USAGE: Regulated industries, critical business forms
    /// </summary>
    public static RowManagementConfiguration Restrictive => new()
    {
        // Lifecycle - controlled
        AllowAddRows = true,
        AllowDeleteRows = true,
        AllowEditRows = true,
        AllowReorderRows = false, // Prevent accidental reordering
        
        // Limits - strict boundaries
        MinRows = 1,
        MinimumRows = 1,
        MaxRows = 1000,
        MaximumRows = 1000,
        
        // UI/UX - extra safety
        AutoAddEmptyRow = false, // Manual control only
        AutoGenerateRowNumbers = true,
        ShowRowNumbers = true,
        ConfirmDelete = true,
        
        // Selection - limited
        EnableSelection = true,
        AllowMultiSelect = false // Single selection only
    };
    
    /// <summary>
    /// FACTORY: High-volume row management configuration
    /// PERFORMANCE: Optimized for large datasets
    /// USAGE: Data import/export, batch processing scenarios
    /// </summary>
    public static RowManagementConfiguration HighVolume => new()
    {
        // Lifecycle - full capabilities
        AllowAddRows = true,
        AllowDeleteRows = true,
        AllowEditRows = true,
        AllowReorderRows = false, // Performance consideration
        
        // Limits - high capacity
        MinRows = 0,
        MinimumRows = 0,
        MaxRows = int.MaxValue,
        MaximumRows = 0,
        
        // UI/UX - performance optimized
        AutoAddEmptyRow = false, // Manual control for performance
        AutoGenerateRowNumbers = false, // Performance consideration
        ShowRowNumbers = false,
        ConfirmDelete = false, // Bulk operations need speed
        
        // Selection - optimized
        EnableSelection = true,
        AllowMultiSelect = true
    };
    
    /// <summary>
    /// FACTORY: Simple form configuration
    /// SIMPLICITY: Basic form-like behavior
    /// USAGE: Simple data entry forms, surveys, basic input
    /// </summary>
    public static RowManagementConfiguration SimpleForm => new()
    {
        // Lifecycle - basic
        AllowAddRows = true,
        AllowDeleteRows = true,
        AllowEditRows = true,
        AllowReorderRows = false,
        
        // Limits - form-like
        MinRows = 1,
        MinimumRows = 1,
        MaxRows = 100,
        MaximumRows = 100,
        
        // UI/UX - user friendly
        AutoAddEmptyRow = true,
        AutoGenerateRowNumbers = false,
        ShowRowNumbers = false,
        ConfirmDelete = true,
        
        // Selection - simple
        EnableSelection = true,
        AllowMultiSelect = false
    };
    
    #endregion

    #region Custom Factory Methods
    
    /// <summary>
    /// FACTORY: Custom row management configuration with fluent builder
    /// EXTENSIBILITY: Allow fine-grained customization
    /// </summary>
    public static RowManagementConfiguration Custom(Action<RowManagementConfigurationBuilder>? configurator = null)
    {
        var builder = new RowManagementConfigurationBuilder();
        configurator?.Invoke(builder);
        return builder.Build();
    }
    
    /// <summary>
    /// FACTORY: Configuration optimized for expected row count
    /// ADAPTIVE: Automatically adjust settings based on expected data volume
    /// </summary>
    public static RowManagementConfiguration ForExpectedRows(int expectedRowCount)
    {
        return expectedRowCount switch
        {
            < 10 => SimpleForm,
            < 100 => Default,
            < 1000 => Restrictive,
            _ => HighVolume
        };
    }
    
    #endregion

    #region Validation Methods
    
    /// <summary>
    /// Validate the configuration for internal consistency
    /// </summary>
    public (bool IsValid, string[] Errors) Validate()
    {
        var errors = new List<string>();
        
        // Check minimum row constraints
        if (EffectiveMinRows < 0)
            errors.Add("Minimum rows cannot be negative");
            
        // Check maximum row constraints
        if (EffectiveMaxRows != int.MaxValue && EffectiveMaxRows <= 0)
            errors.Add("Maximum rows must be positive or unlimited");
            
        // Check min vs max relationship
        if (EffectiveMaxRows != int.MaxValue && EffectiveMinRows > EffectiveMaxRows)
            errors.Add("Minimum rows cannot be greater than maximum rows");
            
        // Check logical constraints
        if (!AllowAddRows && AutoAddEmptyRow)
            errors.Add("Cannot auto-add empty row when adding rows is disabled");
            
        if (!AllowDeleteRows && ConfirmDelete)
            errors.Add("Delete confirmation is irrelevant when deleting is disabled");
            
        if (!EnableSelection && AllowMultiSelect)
            errors.Add("Multi-selection requires selection to be enabled");
        
        return (errors.Count == 0, errors.ToArray());
    }
    
    #endregion
}

/// <summary>
/// BUILDER: Fluent builder for RowManagementConfiguration
/// PROFESSIONAL: Type-safe configuration with validation
/// </summary>
internal class RowManagementConfigurationBuilder
{
    private RowManagementConfiguration _config;
    
    public RowManagementConfigurationBuilder() : this(RowManagementConfiguration.Default) { }
    
    public RowManagementConfigurationBuilder(RowManagementConfiguration baseConfig)
    {
        _config = baseConfig;
    }
    
    public RowManagementConfigurationBuilder WithRowOperations(
        bool allowAdd = true, 
        bool allowDelete = true, 
        bool allowEdit = true, 
        bool allowReorder = true)
    {
        _config = _config with
        {
            AllowAddRows = allowAdd,
            AllowDeleteRows = allowDelete,
            AllowEditRows = allowEdit,
            AllowReorderRows = allowReorder
        };
        return this;
    }
    
    public RowManagementConfigurationBuilder WithRowLimits(int? minRows = null, int? maxRows = null)
    {
        _config = _config with
        {
            MinRows = minRows ?? _config.MinRows,
            MinimumRows = minRows ?? _config.MinimumRows,
            MaxRows = maxRows ?? _config.MaxRows,
            MaximumRows = maxRows == null ? _config.MaximumRows : (maxRows.Value == int.MaxValue ? 0 : maxRows.Value)
        };
        return this;
    }
    
    public RowManagementConfigurationBuilder WithUserExperience(
        bool autoAddEmpty = true, 
        bool showRowNumbers = false, 
        bool confirmDelete = true)
    {
        _config = _config with
        {
            AutoAddEmptyRow = autoAddEmpty,
            ShowRowNumbers = showRowNumbers,
            AutoGenerateRowNumbers = showRowNumbers,
            ConfirmDelete = confirmDelete
        };
        return this;
    }
    
    public RowManagementConfigurationBuilder WithSelection(bool enableSelection = true, bool allowMultiSelect = true)
    {
        _config = _config with
        {
            EnableSelection = enableSelection,
            AllowMultiSelect = allowMultiSelect
        };
        return this;
    }
    
    public RowManagementConfiguration Build()
    {
        var (isValid, errors) = _config.Validate();
        if (!isValid)
            throw new InvalidOperationException($"Invalid configuration: {string.Join("; ", errors)}");
            
        return _config;
    }
}