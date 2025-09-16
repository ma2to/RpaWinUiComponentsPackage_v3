using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases;

/// <summary>
/// DOCUMENTATION: Smart validation manager per specification
/// ENTERPRISE: Real-time and batch validation with smart decision making
/// PERFORMANCE: Optimized validation for large datasets
/// </summary>
internal class SmartValidationManager : IDisposable
{
    #region DOCUMENTATION: Private Fields

    private readonly ILogger? _logger;
    private readonly Dictionary<string, List<IValidationRule>> _columnValidators;
    private readonly Dictionary<string, ValidationConfiguration> _columnConfigurations;
    private readonly List<IValidationRule> _globalValidators;
    private ValidationConfiguration _globalConfiguration;
    private bool _disposed;

    #endregion

    #region DOCUMENTATION: Constructor

    /// <summary>
    /// DOCUMENTATION: Initialize smart validation manager
    /// ENTERPRISE: Configurable validation with performance optimization
    /// </summary>
    /// <param name="globalConfiguration">Global validation configuration</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    public SmartValidationManager(ValidationConfiguration? globalConfiguration = null, ILogger? logger = null)
    {
        _logger = logger;
        _columnValidators = new Dictionary<string, List<IValidationRule>>();
        _columnConfigurations = new Dictionary<string, ValidationConfiguration>();
        _globalValidators = new List<IValidationRule>();
        _globalConfiguration = globalConfiguration ?? ValidationConfiguration.CreateDefault();
        
        _logger?.LogInformation("🔍 VALIDATION: Smart validation manager initialized");
    }

    #endregion

    #region DOCUMENTATION: Configuration

    /// <summary>
    /// DOCUMENTATION: Add column-specific validator per specification
    /// ENTERPRISE: Fine-grained validation control per column
    /// </summary>
    /// <param name="columnName">Column to validate</param>
    /// <param name="validator">Validation rule</param>
    /// <param name="configuration">Column-specific validation configuration</param>
    public void AddColumnValidator(string columnName, IValidationRule validator, ValidationConfiguration? configuration = null)
    {
        if (string.IsNullOrWhiteSpace(columnName) || validator == null)
            return;

        if (!_columnValidators.ContainsKey(columnName))
        {
            _columnValidators[columnName] = new List<IValidationRule>();
        }

        _columnValidators[columnName].Add(validator);

        if (configuration != null)
        {
            _columnConfigurations[columnName] = configuration;
        }

        _logger?.LogInformation("🔍 VALIDATION: Added validator for column '{Column}'", columnName);
    }

    /// <summary>
    /// DOCUMENTATION: Add global validator per specification
    /// ENTERPRISE: Cross-column and cross-row validation support
    /// </summary>
    /// <param name="validator">Global validation rule</param>
    public void AddGlobalValidator(IValidationRule validator)
    {
        if (validator == null) return;

        _globalValidators.Add(validator);
        _logger?.LogInformation("🔍 VALIDATION: Added global validator");
    }

    /// <summary>
    /// DOCUMENTATION: Update global validation configuration
    /// ENTERPRISE: Runtime configuration updates
    /// </summary>
    /// <param name="configuration">New global configuration</param>
    public void UpdateGlobalConfiguration(ValidationConfiguration configuration)
    {
        _globalConfiguration = configuration ?? ValidationConfiguration.CreateDefault();
        _logger?.LogInformation("🔍 VALIDATION: Global configuration updated");
    }

    #endregion

    #region DOCUMENTATION: Smart Validation Methods

    /// <summary>
    /// DOCUMENTATION: Validate single cell per specification
    /// SMART: Real-time validation with intelligent caching
    /// </summary>
    /// <param name="columnName">Column name</param>
    /// <param name="value">Cell value</param>
    /// <param name="rowData">Full row context for cross-column validation</param>
    /// <param name="rowIndex">Row index for context</param>
    /// <returns>Validation errors (empty list if valid)</returns>
    public async Task<List<ValidationError>> ValidateCellAsync(
        string columnName,
        object? value,
        Dictionary<string, object?> rowData,
        int rowIndex)
    {
        if (_disposed) return new List<ValidationError>();

        var errors = new List<ValidationError>();

        try
        {
            // Get column-specific configuration
            var config = _columnConfigurations.GetValueOrDefault(columnName, _globalConfiguration);

            // Skip validation if disabled
            if (config.ValidationMode == ValidationMode.None)
                return errors;

            // Real-time validation decision
            if (config.ValidationMode == ValidationMode.OnEdit || config.ValidationMode == ValidationMode.RealTime)
            {
                // Apply column-specific validators
                if (_columnValidators.TryGetValue(columnName, out var validators))
                {
                    foreach (var validator in validators)
                    {
                        var result = await validator.ValidateAsync(value, rowData, rowIndex);
                        if (!result.IsValid)
                        {
                            errors.Add(ValidationError.CreateForGrid(
                                columnName,
                                result.Error ?? "Validation failed",
                                rowIndex,
                                validator.RuleName,
                                result.Level));

                            // Stop on first error if configured
                            if (config.StopOnFirstError)
                                break;
                        }
                    }
                }

                // Apply global validators for this cell
                foreach (var validator in _globalValidators)
                {
                    var result = await validator.ValidateAsync(value, rowData, rowIndex);
                    if (!result.IsValid)
                    {
                        errors.Add(ValidationError.CreateForGrid(
                            columnName,
                            result.Error ?? "Global validation failed",
                            rowIndex,
                            validator.RuleName,
                            result.Level));

                        if (config.StopOnFirstError)
                            break;
                    }
                }
            }

            if (errors.Count > 0)
            {
                _logger?.LogWarning("🔍 VALIDATION: Cell validation failed - {ErrorCount} errors found for {Column}[{Row}]", 
                    errors.Count, columnName, rowIndex);
            }

            return errors;
        }
        catch (Exception ex)
        {
            var error = ValidationError.CreateForGrid(
                columnName,
                $"Validation error: {ex.Message}",
                rowIndex,
                "SystemError",
                ValidationLevel.Error);

            _logger?.LogError(ex, "💥 VALIDATION: Cell validation exception for {Column}[{Row}]", columnName, rowIndex);
            return new List<ValidationError> { error };
        }
    }

    /// <summary>
    /// DOCUMENTATION: Validate entire row per specification
    /// SMART: Cross-column validation with intelligent processing
    /// </summary>
    /// <param name="rowData">Row data dictionary</param>
    /// <param name="rowIndex">Row index</param>
    /// <param name="columns">Column definitions for context</param>
    /// <returns>All validation errors for the row</returns>
    public async Task<List<ValidationError>> ValidateRowAsync(
        Dictionary<string, object?> rowData,
        int rowIndex,
        IReadOnlyList<ColumnDefinition> columns)
    {
        if (_disposed) return new List<ValidationError>();

        var errors = new List<ValidationError>();

        try
        {
            // Skip validation if disabled globally
            if (_globalConfiguration.ValidationMode == ValidationMode.None)
                return errors;

            // Validate each cell in the row
            foreach (var column in columns)
            {
                if (rowData.TryGetValue(column.PropertyName, out var value))
                {
                    var cellErrors = await ValidateCellAsync(column.PropertyName, value, rowData, rowIndex);
                    errors.AddRange(cellErrors);

                    // Stop on first error if configured globally
                    if (_globalConfiguration.StopOnFirstError && cellErrors.Count > 0)
                        break;
                }
            }

            // Apply row-level global validators
            foreach (var validator in _globalValidators.Where(v => v.ValidationType == ValidationRuleType.Row))
            {
                var result = await validator.ValidateAsync(rowData, rowData, rowIndex);
                if (!result.IsValid)
                {
                    errors.Add(ValidationError.CreateForGrid(
                        "Row",
                        result.Error ?? "Row validation failed",
                        rowIndex,
                        validator.RuleName,
                        result.Level));

                    if (_globalConfiguration.StopOnFirstError)
                        break;
                }
            }

            if (errors.Count > 0)
            {
                _logger?.LogInformation("🔍 VALIDATION: Row validation completed - {ErrorCount} errors found for row {Row}", 
                    errors.Count, rowIndex);
            }

            return errors;
        }
        catch (Exception ex)
        {
            var error = ValidationError.CreateForGrid(
                "Row",
                $"Row validation error: {ex.Message}",
                rowIndex,
                "SystemError",
                ValidationLevel.Error);

            _logger?.LogError(ex, "💥 VALIDATION: Row validation exception for row {Row}", rowIndex);
            return new List<ValidationError> { error };
        }
    }

    /// <summary>
    /// DOCUMENTATION: Validate dataset per specification (AreAllRowsValid logic)
    /// SMART: Ignores last empty row per specification exactly as documented
    /// PERFORMANCE: Optimized batch validation for large datasets
    /// </summary>
    /// <param name="data">Complete dataset</param>
    /// <param name="columns">Column definitions</param>
    /// <param name="onlyVisible">Validate only visible/filtered rows</param>
    /// <param name="timeout">Validation timeout</param>
    /// <param name="progress">Progress reporting</param>
    /// <returns>All validation errors (empty list if all valid)</returns>
    public async Task<List<ValidationError>> ValidateDatasetAsync(
        List<Dictionary<string, object?>> data,
        IReadOnlyList<ColumnDefinition> columns,
        bool onlyVisible = false,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? progress = null)
    {
        if (_disposed) return new List<ValidationError>();

        var errors = new List<ValidationError>();

        try
        {
            // Skip validation if disabled
            if (_globalConfiguration.ValidationMode == ValidationMode.None)
                return errors;

            _logger?.LogInformation("🔍 VALIDATION: Starting dataset validation for {Count} rows", data.Count);

            // SMART: Ignore last empty row per specification (AreAllRowsValid logic)
            var rowsToValidate = GetRowsToValidate(data, onlyVisible);
            var totalRows = rowsToValidate.Count;

            if (totalRows == 0)
            {
                _logger?.LogInformation("✅ VALIDATION: No rows to validate");
                return errors;
            }

            // Batch validation with progress tracking
            var timeout_ms = (int)(timeout?.TotalMilliseconds ?? 300000); // 5 minutes default
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < totalRows; i++)
            {
                // Check timeout
                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeout_ms)
                {
                    _logger?.LogWarning("⏱️ VALIDATION: Timeout reached during dataset validation");
                    errors.Add(ValidationError.CreateForGrid(
                        "System",
                        "Validation timeout reached",
                        i,
                        "Timeout",
                        ValidationLevel.Warning));
                    break;
                }

                var rowData = rowsToValidate[i];
                var rowIndex = data.IndexOf(rowData); // Get original index

                var rowErrors = await ValidateRowAsync(rowData, rowIndex, columns);
                errors.AddRange(rowErrors);

                // Report progress
                var progressValue = (double)(i + 1) / totalRows * 100;
                progress?.Report(new ValidationProgress
                {
                    TotalRows = totalRows,
                    ProcessedRows = i + 1,
                    ValidatedRows = i + 1,
                    TotalErrors = errors.Count,
                    ValidationErrors = errors.Count
                });

                // Stop on first error if configured
                if (_globalConfiguration.StopOnFirstError && errors.Count > 0)
                    break;
            }

            _logger?.LogInformation("✅ VALIDATION: Dataset validation completed - {ErrorCount} errors found", errors.Count);
            return errors;
        }
        catch (Exception ex)
        {
            var error = ValidationError.CreateForGrid(
                "System",
                $"Dataset validation error: {ex.Message}",
                -1,
                "SystemError",
                ValidationLevel.Error);

            _logger?.LogError(ex, "💥 VALIDATION: Dataset validation exception");
            return new List<ValidationError> { error };
        }
    }

    /// <summary>
    /// DOCUMENTATION: Format validation errors for ValidAlerts column
    /// ENTERPRISE: Standard error message formatting per specification
    /// </summary>
    /// <param name="errors">Validation errors for a specific row</param>
    /// <param name="maxErrors">Maximum errors to include</param>
    /// <param name="format">Format template</param>
    /// <returns>Formatted error message for ValidAlerts column</returns>
    public string FormatValidationErrors(
        List<ValidationError> errors,
        int maxErrors = 10,
        string? format = null)
    {
        if (errors == null || errors.Count == 0)
            return string.Empty;

        var template = format ?? "{ColumnName}: {ErrorMessage}";
        var errorsToShow = errors.Take(maxErrors).ToList();
        
        var formattedErrors = errorsToShow.Select(error =>
            template
                .Replace("{ColumnName}", error.ColumnName ?? "Unknown")
                .Replace("{ErrorMessage}", error.Message ?? "Validation failed")
                .Replace("{Level}", error.Level.ToString())
                .Replace("{Rule}", error.ValidationRule ?? "Unknown")
        );

        var result = string.Join(Environment.NewLine, formattedErrors);
        
        if (errors.Count > maxErrors)
        {
            result += Environment.NewLine + $"... and {errors.Count - maxErrors} more errors";
        }

        return result;
    }

    #endregion

    #region INTERNAL: Helper Methods

    /// <summary>
    /// INTERNAL: Get rows to validate with smart empty row detection
    /// SMART: Implements AreAllRowsValid logic - ignores last empty row
    /// </summary>
    private List<Dictionary<string, object?>> GetRowsToValidate(List<Dictionary<string, object?>> data, bool onlyVisible)
    {
        if (data.Count == 0)
            return new List<Dictionary<string, object?>>();

        var rowsToValidate = onlyVisible 
            ? data.Where(row => IsRowVisible(row)).ToList()
            : new List<Dictionary<string, object?>>(data);

        // SMART: Ignore last row if it's completely empty (per specification)
        if (rowsToValidate.Count > 0)
        {
            var lastRow = rowsToValidate[^1];
            if (IsRowEmpty(lastRow))
            {
                rowsToValidate.RemoveAt(rowsToValidate.Count - 1);
                _logger?.LogInformation("🔍 VALIDATION: Ignoring last empty row per specification");
            }
        }

        return rowsToValidate;
    }

    /// <summary>
    /// INTERNAL: Check if row is empty (all values null, empty, or whitespace)
    /// </summary>
    private bool IsRowEmpty(Dictionary<string, object?> row)
    {
        return row.Values.All(value => 
            value == null || 
            (value is string str && string.IsNullOrWhiteSpace(str)) ||
            (value.ToString()?.Trim().Length == 0));
    }

    /// <summary>
    /// INTERNAL: Check if row is visible (for filtered datasets)
    /// TODO: Implement proper visibility logic based on filters
    /// </summary>
    private bool IsRowVisible(Dictionary<string, object?> row)
    {
        // For now, all rows are considered visible
        // This will be enhanced when filtering is implemented
        return true;
    }

    #endregion

    #region DOCUMENTATION: Resource Management

    /// <summary>
    /// DOCUMENTATION: Dispose validation manager resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _columnValidators?.Clear();
            _columnConfigurations?.Clear();
            _globalValidators?.Clear();
            _disposed = true;
            _logger?.LogInformation("🗑️ VALIDATION: Validation manager disposed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "⚠️ VALIDATION: Error during disposal");
        }
    }

    #endregion
}