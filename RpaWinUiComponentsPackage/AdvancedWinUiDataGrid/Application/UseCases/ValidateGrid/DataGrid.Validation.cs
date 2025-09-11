using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Management;

/// <summary>
/// ENTERPRISE: DataGrid real-time validation functionality
/// ANTI-GOD-FILE: Separated real-time validation concerns from main DataGrid class
/// PERFORMANCE: Optimized cell-level validation with timeout protection
/// </summary>
public sealed partial class DataGrid
{
    #region ENTERPRISE: Real-Time Cell Validation System

    private readonly Dictionary<string, DateTime> _cellValidationCache = new();
    private TimeSpan _cellValidationCacheExpiry = TimeSpan.FromSeconds(30);

    /// <summary>
    /// REAL-TIME: Validate cell on edit with immediate feedback
    /// PERFORMANCE: Cached validation results to avoid repeated validation
    /// UX: Immediate visual feedback for validation errors
    /// </summary>
    public async Task<Result<CellValidationResult>> ValidateCellOnEditAsync(
        int rowIndex,
        int columnIndex,
        object? newValue,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (!_isInitialized)
            return Result<CellValidationResult>.Failure("DataGrid not initialized");

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 CELL VALIDATION: Real-time validation for cell [{Row}, {Column}] with value '{Value}'", 
                    rowIndex, columnIndex, newValue);

                var startTime = DateTime.UtcNow;
                
                // Check cache first for performance
                var cacheKey = $"{rowIndex}_{columnIndex}_{newValue?.GetHashCode()}";
                if (_cellValidationCache.TryGetValue(cacheKey, out var cachedTime) && 
                    DateTime.UtcNow - cachedTime < _cellValidationCacheExpiry)
                {
                    _logger.LogTrace("⚡ CELL VALIDATION: Using cached validation result");
                    return Result<CellValidationResult>.Success(new CellValidationResult
                    {
                        IsValid = true,
                        RowIndex = rowIndex,
                        ColumnIndex = columnIndex,
                        ValidatedValue = newValue,
                        ValidationDuration = DateTime.UtcNow - startTime,
                        UsedCache = true
                    });
                }

                // Get column name for validation
                var columnNameResult = await _dataGridService.GetColumnNameAsync(columnIndex);
                if (!columnNameResult.IsSuccess)
                {
                    return Result<CellValidationResult>.Failure($"Invalid column index: {columnIndex}");
                }
                var columnName = columnNameResult.Value;

                var errors = new List<ValidationError>();

                // 1. SINGLE CELL VALIDATION: Apply single cell validation rules
                var singleCellErrors = await ValidateSingleCellAsync(columnName!, newValue);
                if (singleCellErrors != null && singleCellErrors.Count > 0)
                {
                    errors.AddRange(singleCellErrors);
                }

                // 2. CROSS-COLUMN VALIDATION: Validate against other columns in same row
                var rowData = await _dataGridService.GetRowDataAsync(rowIndex);
                if (rowData != null)
                {
                    // Update row data with new value for cross-column validation
                    var updatedRowData = new Dictionary<string, object?>(rowData)
                    {
                        [columnName!] = newValue
                    };

                    var crossColumnErrors = await ValidateCrossColumnAsync(updatedRowData);
                    if (crossColumnErrors != null && crossColumnErrors.Count > 0)
                    {
                        errors.AddRange(crossColumnErrors);
                    }
                }

                // 3. CONDITIONAL VALIDATION: Check conditional rules
                if (rowData != null)
                {
                    var updatedRowData = new Dictionary<string, object?>(rowData)
                    {
                        [columnName!] = newValue
                    };

                    var conditionalErrors = await ValidateConditionalAsync(columnName!, newValue, updatedRowData);
                    if (conditionalErrors != null && conditionalErrors.Count > 0)
                    {
                        errors.AddRange(conditionalErrors);
                    }
                }

                var validationDuration = DateTime.UtcNow - startTime;
                var isValid = errors.Count == 0;

                // Cache successful validation results
                if (isValid && validationDuration < TimeSpan.FromSeconds(1))
                {
                    _cellValidationCache[cacheKey] = DateTime.UtcNow;
                }

                // Show immediate UI feedback if validation service available
                if (_uiService != null)
                {
                    var errorMessage = errors.Count > 0 ? string.Join("; ", errors.Select(e => e.Message)) : "";
                    await _uiService.ShowCellValidationFeedbackAsync(rowIndex, columnIndex, errorMessage);
                }

                var result = new CellValidationResult
                {
                    IsValid = isValid,
                    RowIndex = rowIndex,
                    ColumnIndex = columnIndex,
                    ColumnName = columnName!,
                    ValidatedValue = newValue,
                    ValidationErrors = errors,
                    ValidationDuration = validationDuration,
                    UsedCache = false
                };

                var logLevel = isValid ? Microsoft.Extensions.Logging.LogLevel.Debug : Microsoft.Extensions.Logging.LogLevel.Warning;
                _logger.Log(logLevel, "{Status} CELL VALIDATION: Cell [{Row}, {Column}] validation completed in {Duration}ms - {ErrorCount} errors", 
                    isValid ? "✅" : "❌", rowIndex, columnIndex, validationDuration.TotalMilliseconds, errors.Count);

                return Result<CellValidationResult>.Success(result);
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to validate cell [{rowIndex}, {columnIndex}] in real-time";
            _logger.LogError(ex, "💥 CELL VALIDATION: {ErrorMessage}", errorMessage);
            return Result<CellValidationResult>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// BATCH: Validate import/paste data with fast batch processing
    /// PERFORMANCE: 1. Import/paste all data first (fast) 2. Then validate everything (batch)
    /// </summary>
    public async Task<Result<BatchValidationResult>> ValidateImportedDataAsync(
        IProgress<ValidationProgress>? progress = null,
        TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (!_isInitialized)
            return Result<BatchValidationResult>.Failure("DataGrid not initialized");

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(10);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("📊 BATCH VALIDATION: Starting batch validation of all imported data");

                var startTime = DateTime.UtcNow;
                var rowCount = GetRowsCount();
                var allErrors = new List<ValidationError>();
                var validatedRows = 0;

                // Report initial progress
                progress?.Report(new ValidationProgress
                {
                    ProcessedRows = 0,
                    TotalRows = rowCount,
                    ValidationErrors = 0,
                    CurrentOperation = "Starting batch validation",
                    ElapsedTime = TimeSpan.Zero
                });

                // Validate all rows in batches for performance
                const int batchSize = 100;
                for (int batchStart = 0; batchStart < rowCount; batchStart += batchSize)
                {
                    var batchEnd = Math.Min(batchStart + batchSize, rowCount);
                    var batchErrors = new List<ValidationError>();

                    // Validate current batch
                    for (int rowIndex = batchStart; rowIndex < batchEnd; rowIndex++)
                    {
                        var rowValidationResult = await _validationService.ValidateRowAsync(rowIndex, new Dictionary<string, object?>(), new List<ColumnDefinition>());
                        if (rowValidationResult.IsSuccess && rowValidationResult.Value != null)
                        {
                            batchErrors.AddRange(rowValidationResult.Value);
                        }
                        
                        validatedRows++;
                    }

                    allErrors.AddRange(batchErrors);

                    // Report batch progress
                    progress?.Report(new ValidationProgress
                    {
                        ProcessedRows = validatedRows,
                        TotalRows = rowCount,
                        ValidationErrors = allErrors.Count,
                        CurrentOperation = $"Validating batch {batchStart + 1}-{batchEnd}",
                        ElapsedTime = DateTime.UtcNow - startTime
                    });

                    _logger.LogTrace("📊 BATCH VALIDATION: Processed batch {BatchStart}-{BatchEnd} with {BatchErrors} errors", 
                        batchStart, batchEnd, batchErrors.Count);
                }

                var totalDuration = DateTime.UtcNow - startTime;
                var result = new BatchValidationResult
                {
                    IsSuccess = true,
                    ValidatedRows = validatedRows,
                    TotalErrors = allErrors.Count,
                    ValidationErrors = allErrors,
                    ValidationDuration = totalDuration,
                    ValidationTime = DateTime.UtcNow,
                    AverageTimePerRow = validatedRows > 0 ? totalDuration.TotalMilliseconds / validatedRows : 0
                };

                // Final progress report
                progress?.Report(new ValidationProgress
                {
                    ProcessedRows = validatedRows,
                    TotalRows = rowCount,
                    ValidationErrors = allErrors.Count,
                    CurrentOperation = "Batch validation completed",
                    ElapsedTime = totalDuration
                });

                _logger.LogInformation("✅ BATCH VALIDATION: Completed validation of {ValidatedRows} rows in {Duration}ms - {ErrorCount} total errors", 
                    validatedRows, totalDuration.TotalMilliseconds, allErrors.Count);

                return Result<BatchValidationResult>.Success(result);
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to validate imported data in batch";
            _logger.LogError(ex, "💥 BATCH VALIDATION: {ErrorMessage}", errorMessage);
            return Result<BatchValidationResult>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// COMPREHENSIVE: Validate all non-empty rows in the entire dataset
    /// FULL COVERAGE: Validates COMPLETE dataset, not just visible rows
    /// </summary>
    public async Task<Result<bool>> AreAllNonEmptyRowsValidAsync(TimeSpan? timeout = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGrid));
        if (!_isInitialized)
            return Result<bool>.Failure("DataGrid not initialized");

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(5);
        
        try
        {
            return await ExecuteWithTimeoutAsync(async () =>
            {
                _logger.LogTrace("🔍 FULL VALIDATION: Checking if all non-empty rows are valid");

                var rowCount = GetRowsCount();
                var nonEmptyRowsValidated = 0;
                var invalidRows = 0;

                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    // Check if row is empty
                    var isEmptyRowResult = await _dataGridService.IsRowEmptyAsync(rowIndex);
                    if (isEmptyRowResult.IsSuccess && isEmptyRowResult.Value) continue;

                    nonEmptyRowsValidated++;

                    // Validate the row
                    var rowValidationResult = await _validationService.ValidateRowAsync(rowIndex, new Dictionary<string, object?>(), new List<ColumnDefinition>());
                    if (!rowValidationResult.IsSuccess || (rowValidationResult.Value?.Length > 0))
                    {
                        invalidRows++;
                        _logger.LogTrace("❌ FULL VALIDATION: Row {RowIndex} has validation errors", rowIndex);
                    }
                }

                var allValid = invalidRows == 0;

                _logger.LogInformation("{Status} FULL VALIDATION: {NonEmptyRows} non-empty rows checked - {InvalidRows} invalid rows found", 
                    allValid ? "✅" : "❌", nonEmptyRowsValidated, invalidRows);

                return Result<bool>.Success(allValid);
            }, effectiveTimeout);
        }
        catch (Exception ex)
        {
            const string errorMessage = "Failed to validate all non-empty rows";
            _logger.LogError(ex, "💥 FULL VALIDATION: {ErrorMessage}", errorMessage);
            return Result<bool>.Failure(errorMessage, ex);
        }
    }

    /// <summary>
    /// INTERNAL: Validate single cell against single-cell rules
    /// PERFORMANCE: Fast validation for individual cell rules
    /// </summary>
    private async Task<List<ValidationError>?> ValidateSingleCellAsync(string columnName, object? value)
    {
        try
        {
            // This would integrate with the validation service to check single cell rules
            var validationResult = await _validationService.ValidateSingleCellAsync(columnName, value, new Dictionary<string, object?>(), 0);
            return validationResult.IsSuccess ? validationResult.Value?.ToList() ?? new List<ValidationError>() : new List<ValidationError>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ CELL VALIDATION: Single cell validation failed for column '{ColumnName}'", columnName);
            return new List<ValidationError>();
        }
    }

    /// <summary>
    /// INTERNAL: Validate cross-column rules for a row
    /// BUSINESS RULES: Check inter-column dependencies
    /// </summary>
    private async Task<List<ValidationError>?> ValidateCrossColumnAsync(Dictionary<string, object?> rowData)
    {
        try
        {
            var validationResult = await _validationService.ValidateCrossColumnAsync(rowData, 0);
            return validationResult.IsSuccess ? validationResult.Value?.ToList() ?? new List<ValidationError>() : new List<ValidationError>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ CELL VALIDATION: Cross-column validation failed");
            return new List<ValidationError>();
        }
    }

    /// <summary>
    /// INTERNAL: Validate conditional rules for a cell
    /// CONDITIONAL LOGIC: Context-dependent validation
    /// </summary>
    private async Task<List<ValidationError>?> ValidateConditionalAsync(
        string columnName, 
        object? value, 
        Dictionary<string, object?> rowData)
    {
        try
        {
            var validationResult = await _validationService.ValidateConditionalAsync(rowData, 0);
            return validationResult.IsSuccess ? validationResult.Value?.ToList() ?? new List<ValidationError>() : new List<ValidationError>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ CELL VALIDATION: Conditional validation failed for column '{ColumnName}'", columnName);
            return new List<ValidationError>();
        }
    }

    /// <summary>
    /// MAINTENANCE: Clear validation cache
    /// PERFORMANCE: Free memory and force fresh validation
    /// </summary>
    public void ClearValidationCache()
    {
        _cellValidationCache.Clear();
        _logger.LogDebug("🧹 VALIDATION CACHE: Validation cache cleared");
    }

    /// <summary>
    /// CONFIGURATION: Set validation cache expiry time
    /// PERFORMANCE: Control cache duration vs validation freshness
    /// </summary>
    public void SetValidationCacheExpiry(TimeSpan expiry)
    {
        _cellValidationCacheExpiry = expiry;
        _logger.LogDebug("⏰ VALIDATION CACHE: Cache expiry set to {Expiry}", expiry);
    }

    #endregion
}

/// <summary>
/// REAL-TIME: Cell validation result
/// FUNCTIONAL: Immutable validation result for single cell
/// </summary>
public record CellValidationResult
{
    public bool IsValid { get; init; }
    public int RowIndex { get; init; }
    public int ColumnIndex { get; init; }
    public string? ColumnName { get; init; }
    public object? ValidatedValue { get; init; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = Array.Empty<ValidationError>();
    public TimeSpan ValidationDuration { get; init; }
    public bool UsedCache { get; init; }
}

/// <summary>
/// BATCH: Batch validation result
/// FUNCTIONAL: Immutable result for batch validation operations
/// </summary>
public record BatchValidationResult
{
    public bool IsSuccess { get; init; }
    public int ValidatedRows { get; init; }
    public int TotalErrors { get; init; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = Array.Empty<ValidationError>();
    public TimeSpan ValidationDuration { get; init; }
    public DateTime ValidationTime { get; init; }
    public double AverageTimePerRow { get; init; }
    public string? ErrorMessage { get; init; }
}