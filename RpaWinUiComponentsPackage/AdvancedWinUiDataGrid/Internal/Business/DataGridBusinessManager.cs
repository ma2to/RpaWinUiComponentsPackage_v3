using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Business;

/// <summary>
/// PROFESSIONAL Business Logic Manager for AdvancedDataGrid
/// RESPONSIBILITY: Handle ONLY business logic, data operations, validation logic
/// SEPARATION: Pure business layer - no UI operations, no visual styling, no user interactions
/// ANTI-GOD: Focused, single-responsibility business logic management
/// </summary>
internal sealed class DataGridBusinessManager : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private bool _disposed = false;

    public DataGridBusinessManager(ILogger? logger, GlobalExceptionHandler exceptionHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        
        _logger?.Info("💼 BUSINESS MANAGER: Initialized DataGridBusinessManager");
    }

    /// <summary>
    /// Perform comprehensive validation logic
    /// PURE BUSINESS: Only validation logic, no UI updates
    /// </summary>
    public async Task<Result<ValidationResult>> ValidateAllAsync(DataGridCoordinator coordinator, IProgress<ValidationProgress>? progress = null)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔍 BUSINESS VALIDATION: Starting comprehensive validation");
            
            var startTime = DateTime.UtcNow;
            var totalCells = 0;
            var validCells = 0;
            var invalidCells = 0;
            var errors = new List<ValidationError>();

            if (coordinator?.DataRows == null)
            {
                _logger?.Warning("⚠️ BUSINESS VALIDATION: No data rows to validate");
                return new ValidationResult(0, 0, 0, errors);
            }

            var validationConfig = coordinator.ValidationConfiguration;
            if (validationConfig?.EnableRealtimeValidation != true)
            {
                _logger?.Info("⏭️ BUSINESS VALIDATION: Real-time validation disabled, skipping");
                return new ValidationResult(0, 0, 0, errors);
            }

            // Process each row
            for (int rowIndex = 0; rowIndex < coordinator.DataRows.Count; rowIndex++)
            {
                var row = coordinator.DataRows[rowIndex];
                if (row?.Cells == null) continue;

                // Process each cell in the row
                foreach (var cell in row.Cells)
                {
                    if (cell == null) continue;
                    
                    totalCells++;
                    var cellValue = cell.Value?.ToString() ?? "";
                    
                    // Validate cell using business rules
                    var cellValidationResult = await ValidateCellBusinessLogic(cell, cellValue, validationConfig);
                    
                    if (cellValidationResult.IsValid)
                    {
                        validCells++;
                    }
                    else
                    {
                        invalidCells++;
                        errors.AddRange(cellValidationResult.Errors);
                    }

                    // Report progress
                    progress?.Report(new ValidationProgress 
                    { 
                        ProcessedRows = totalCells,
                        TotalRows = coordinator.DataRows.Sum(r => r.Cells?.Count ?? 0),
                        ErrorCount = invalidCells
                    });
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger?.Info("✅ BUSINESS VALIDATION: Completed - Total: {Total}, Valid: {Valid}, Invalid: {Invalid}, Duration: {Duration}ms",
                totalCells, validCells, invalidCells, (int)duration.TotalMilliseconds);

            return new ValidationResult(totalCells, validCells, invalidCells, errors);
            
        }, "ValidateAll", coordinator?.DataRows?.Count ?? 0, new ValidationResult(0, 0, 0, new List<ValidationError>()), _logger);
    }

    /// <summary>
    /// Validate single cell using business rules only
    /// PURE BUSINESS: No UI operations, only business validation logic
    /// </summary>
    public async Task<CellValidationResult> ValidateCellBusinessLogic(DataGridCell cell, string value, ValidationConfiguration validationConfig)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            var errors = new List<ValidationError>();
            var isValid = true;

            _logger?.Info("🔍 CELL VALIDATION: Validating cell {CellId} with value '{Value}'", cell.CellId, value);

            // Check basic validation rules (without messages)
            if (validationConfig.Rules?.TryGetValue(cell.ColumnName, out var basicRule) == true)
            {
                try
                {
                    var ruleResult = basicRule?.Invoke(value) ?? true;
                    if (!ruleResult)
                    {
                        isValid = false;
                        errors.Add(new ValidationError(cell.RowIndex, cell.ColumnIndex, $"Invalid value: {value}"));
                        _logger?.Error("❌ CELL VALIDATION: Basic rule failed for {CellId}", cell.CellId);
                    }
                    else
                    {
                        _logger?.Info("✅ CELL VALIDATION: Basic rule passed for {CellId}", cell.CellId);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "🚨 CELL VALIDATION ERROR: Basic rule exception for {CellId}", cell.CellId);
                    isValid = false;
                    errors.Add(new ValidationError(cell.RowIndex, cell.ColumnIndex, "Validation rule error"));
                }
            }

            // Check validation rules with custom messages
            if (validationConfig.RulesWithMessages?.TryGetValue(cell.ColumnName, out var ruleWithMessage) == true)
            {
                try
                {
                    var ruleResult = ruleWithMessage.Validator?.Invoke(value) ?? true;
                    if (!ruleResult)
                    {
                        isValid = false;
                        errors.Add(new ValidationError(cell.RowIndex, cell.ColumnIndex, ruleWithMessage.ErrorMessage));
                        _logger?.Error("❌ CELL VALIDATION: Rule with message failed for {CellId}: {Message}", cell.CellId, ruleWithMessage.ErrorMessage);
                    }
                    else
                    {
                        _logger?.Info("✅ CELL VALIDATION: Rule with message passed for {CellId}", cell.CellId);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "🚨 CELL VALIDATION ERROR: Rule with message exception for {CellId}", cell.CellId);
                    isValid = false;
                    errors.Add(new ValidationError(cell.RowIndex, cell.ColumnIndex, "Validation rule error"));
                }
            }

            return new CellValidationResult(isValid, errors);
            
        }, "ValidateCell", 1, new CellValidationResult(false, new List<ValidationError>()), _logger);
    }

    /// <summary>
    /// Process data import business logic
    /// PURE BUSINESS: Only data processing, no UI operations
    /// </summary>
    public async Task<Result<Models.ImportResult>> ProcessDataImportAsync(IReadOnlyList<IReadOnlyDictionary<string, object?>> data, DataGridCoordinator coordinator)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📥 BUSINESS IMPORT: Starting data import processing for {RowCount} rows", data.Count);
            
            var startTime = DateTime.UtcNow;
            var processedRows = 0;
            var errorRows = 0;

            // Process each row through business logic
            foreach (var rowData in data)
            {
                try
                {
                    // Apply business rules for data transformation
                    var processedRowData = await ProcessRowBusinessLogic(rowData);
                    
                    // TODO: Add to coordinator's data
                    processedRows++;
                    
                    _logger?.Info("💼 BUSINESS IMPORT: Processed row {RowIndex}/{Total}", processedRows, data.Count);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "🚨 BUSINESS IMPORT ERROR: Failed to process row {RowIndex}", processedRows + 1);
                    errorRows++;
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger?.Info("✅ BUSINESS IMPORT: Completed - Processed: {Processed}, Errors: {Errors}, Duration: {Duration}ms",
                processedRows, errorRows, (int)duration.TotalMilliseconds);

            return new Models.ImportResult 
            { 
                Success = true, 
                ImportedRows = processedRows, 
                ErrorRows = errorRows, 
                Duration = duration 
            };
            
        }, "ProcessDataImport", data.Count, new Models.ImportResult { Success = false, ErrorRows = data.Count }, _logger);
    }

    /// <summary>
    /// Process row data using business logic
    /// PURE BUSINESS: Data transformation logic only
    /// </summary>
    private async Task<IReadOnlyDictionary<string, object?>> ProcessRowBusinessLogic(IReadOnlyDictionary<string, object?> rowData)
    {
        // Apply business transformations
        var processedData = new Dictionary<string, object?>(rowData);
        
        // TODO: Add business-specific transformations
        // - Data type conversions
        // - Business rule applications
        // - Data normalization
        
        await Task.CompletedTask; // Placeholder for async operations
        
        return processedData;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 BUSINESS MANAGER DISPOSE: Cleaning up DataGridBusinessManager");
            _disposed = true;
        }
    }
}

/// <summary>
/// Result of cell validation business logic
/// </summary>
internal record CellValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors);

/// <summary>
/// Result of data import business processing
/// </summary>
internal record ImportResult(int ProcessedRows, int ErrorRows, TimeSpan Duration);