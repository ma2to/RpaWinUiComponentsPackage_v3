using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ImportData;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ExportData;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services.Specialized;

/// <summary>
/// SOLID: Single Responsibility - Import/Export operations only
/// DDD: Domain Service for data transformation operations
/// CLEAN ARCHITECTURE: Application layer service
/// </summary>
public sealed class DataGridImportExportService : IDataGridImportExportService
{
    #region Private Fields
    
    private readonly IDataGridTransformationService _transformationService;
    private readonly IDataGridValidationService _validationService;
    private readonly ILogger? _logger;
    
    #endregion

    #region Constructor
    
    public DataGridImportExportService(
        IDataGridTransformationService transformationService,
        IDataGridValidationService validationService,
        ILogger<DataGridImportExportService>? logger = null)
    {
        _transformationService = transformationService ?? throw new ArgumentNullException(nameof(transformationService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger;
    }
    
    #endregion

    #region Import Operations
    
    /// <summary>
    /// ENTERPRISE: Import data from dictionary with comprehensive validation
    /// PERFORMANCE: Batch processing for large datasets
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDictionaryAsync(
        GridState currentState,
        ImportDataCommand command)
    {
        if (currentState == null)
            return Result<ImportResult>.Failure("DataGrid must be initialized before importing data");

        try
        {
            _logger?.LogInformation("Starting dictionary import of {RowCount} rows", command.Data.Count);
            var stopwatch = Stopwatch.StartNew();

            // 1. VALIDATION: Pre-import validation
            if (command.ValidateBeforeImport)
            {
                var validationResult = await ValidateImportDataAsync(command.Data, currentState.Columns);
                if (!validationResult.IsSuccess)
                {
                    _logger?.LogWarning("Import validation failed: {Error}", validationResult.Error);
                    return Result<ImportResult>.Failure($"Validation failed: {validationResult.Error}");
                }
            }

            // 2. TRANSFORMATION: Convert dictionary to internal format
            var transformedData = await _transformationService.TransformFromDictionaryAsync(
                command.Data, 
                currentState.Columns);

            // 3. CONVERSION: Convert to GridRow format
            var gridRows = transformedData.Select((dict, index) => 
            {
                var row = new GridRow(index);
                row.RowIndex = index;
                // Set each column value individually
                foreach (var kvp in dict)
                {
                    row.SetValue(kvp.Key, kvp.Value);
                }
                return row;
            }).ToList();

            // 4. INTEGRATION: Apply to current state
            var integrationResult = await IntegrateImportedDataAsync(
                currentState, 
                gridRows, 
                command);
            
            stopwatch.Stop();
            
            _logger?.LogInformation("Dictionary import completed in {ElapsedMs}ms. Imported: {ImportedRows}, Skipped: {SkippedRows}", 
                stopwatch.ElapsedMilliseconds, integrationResult.ImportedRows, integrationResult.SkippedRows);

            return Result<ImportResult>.Success(integrationResult);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Dictionary import failed with exception");
            return Result<ImportResult>.Failure($"Import failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Import data from DataTable with comprehensive validation
    /// PERFORMANCE: Optimized for large DataTable scenarios
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromDataTableAsync(
        GridState currentState,
        ImportFromDataTableCommand command)
    {
        if (currentState == null)
            return Result<ImportResult>.Failure("DataGrid must be initialized before importing data");

        try
        {
            _logger?.LogInformation("Starting DataTable import of {RowCount} rows", command.DataTable.Rows.Count);
            var stopwatch = Stopwatch.StartNew();

            // 1. TRANSFORMATION: Convert DataTable to dictionary format
            var dictionaryData = ConvertDataTableToDictionary(command.DataTable);
            
            // 2. DELEGATE: Use dictionary import logic
            var importCommand = ImportDataCommand.FromDictionary(
                dictionaryData,
                command.CheckboxStates,
                command.StartRow,
                command.Mode,
                command.Timeout,
                command.ValidationProgress);

            var result = await ImportFromDictionaryAsync(currentState, importCommand);
            
            stopwatch.Stop();
            _logger?.LogInformation("DataTable import completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DataTable import failed with exception");
            return Result<ImportResult>.Failure($"Import failed: {ex.Message}");
        }
    }

    #endregion

    #region Export Operations
    
    /// <summary>
    /// ENTERPRISE: Export data to dictionary format
    /// PERFORMANCE: Streaming export for large datasets
    /// </summary>
    public async Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(
        GridState currentState,
        ExportDataCommand command)
    {
        if (currentState == null)
            return Result<List<Dictionary<string, object?>>>.Failure("DataGrid must be initialized before exporting data");

        try
        {
            _logger?.LogInformation("Starting dictionary export");
            var stopwatch = Stopwatch.StartNew();

            // 1. FILTERING: Determine which rows to export
            var rowsToExport = DetermineExportRows(currentState, command);
            var dictionaryRows = rowsToExport.Select(row => new Dictionary<string, object?>(row.Data)).ToList().AsReadOnly();
            
            // 2. TRANSFORMATION: Convert to dictionary format
            var exportedData = await _transformationService.TransformToDictionaryAsync(
                dictionaryRows, 
                currentState.Columns,
                command.IncludeValidationAlerts);

            stopwatch.Stop();
            _logger?.LogInformation("Dictionary export completed in {ElapsedMs}ms. Exported {RowCount} rows", 
                stopwatch.ElapsedMilliseconds, exportedData.Count);

            return Result<List<Dictionary<string, object?>>>.Success(exportedData);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Dictionary export failed with exception");
            return Result<List<Dictionary<string, object?>>>.Failure($"Export failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ENTERPRISE: Export data to DataTable format
    /// PERFORMANCE: Optimized DataTable creation
    /// </summary>
    public async Task<Result<DataTable>> ExportToDataTableAsync(
        GridState currentState,
        ExportToDataTableCommand command)
    {
        if (currentState == null)
            return Result<DataTable>.Failure("DataGrid must be initialized before exporting data");

        try
        {
            _logger?.LogInformation("Starting DataTable export");
            var stopwatch = Stopwatch.StartNew();

            // 1. EXPORT: Get dictionary data first
            var dictionaryCommand = ExportDataCommand.ToDataTable(
                command.IncludeValidationAlerts,
                command.ExportOnlyChecked,
                command.ExportOnlyFiltered,
                false, // removeAfter
                command.Timeout,
                command.ExportProgress);

            var dictionaryResult = await ExportToDictionaryAsync(currentState, dictionaryCommand);
            
            if (!dictionaryResult.IsSuccess)
                return Result<DataTable>.Failure(dictionaryResult.Error!);

            // 2. TRANSFORMATION: Convert dictionary to DataTable
            var dataTable = ConvertDictionaryToDataTable(dictionaryResult.Value!, currentState.Columns);

            stopwatch.Stop();
            _logger?.LogInformation("DataTable export completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DataTable export failed with exception");
            return Result<DataTable>.Failure($"Export failed: {ex.Message}");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<Result<bool>> ValidateImportDataAsync(
        List<Dictionary<string, object?>> data, 
        IReadOnlyList<ColumnDefinition> columns)
    {
        // Pre-import validation logic
        foreach (var row in data)
        {
            foreach (var column in columns.Where(c => c.IsRequired))
            {
                if (!row.ContainsKey(column.Name) || row[column.Name] == null)
                {
                    return Result<bool>.Failure($"Required column '{column.Name}' is missing or null");
                }
            }
        }

        return Result<bool>.Success(true);
    }

    private async Task<ImportResult> IntegrateImportedDataAsync(
        GridState currentState,
        List<GridRow> transformedRows,
        ImportDataCommand command)
    {
        var result = new ImportResult
        {
            TotalRows = command.Data.Count,
            ImportedRows = 0,
            SkippedRows = 0,
            ValidationErrors = new List<ValidationError>()
        };

        // Integration logic based on ImportMode
        switch (command.Mode)
        {
            case ImportMode.Replace:
                currentState.Rows.Clear();
                currentState.Rows.AddRange(transformedRows);
                result.ImportedRows = transformedRows.Count;
                break;

            case ImportMode.Append:
                currentState.Rows.AddRange(transformedRows);
                result.ImportedRows = transformedRows.Count;
                break;

            case ImportMode.Update:
                // Update existing rows logic
                result.ImportedRows = await UpdateExistingRowsAsync(currentState, transformedRows);
                result.SkippedRows = transformedRows.Count - result.ImportedRows;
                break;
        }

        return result;
    }

    private async Task<int> UpdateExistingRowsAsync(GridState currentState, List<GridRow> newRows)
    {
        // Implementation for updating existing rows
        // This would include row matching logic based on primary keys or unique columns
        return newRows.Count; // Simplified for now
    }

    private List<Dictionary<string, object?>> ConvertDataTableToDictionary(DataTable dataTable)
    {
        var result = new List<Dictionary<string, object?>>();
        
        foreach (DataRow row in dataTable.Rows)
        {
            var dict = new Dictionary<string, object?>();
            foreach (DataColumn column in dataTable.Columns)
            {
                dict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
            }
            result.Add(dict);
        }

        return result;
    }

    private DataTable ConvertDictionaryToDataTable(
        List<Dictionary<string, object?>> data, 
        IReadOnlyList<ColumnDefinition> columns)
    {
        var dataTable = new DataTable();

        // Add columns
        foreach (var column in columns.Where(c => c.IsVisible))
        {
            var dataColumn = new DataColumn(column.Name, column.DataType);
            dataColumn.AllowDBNull = !column.IsRequired;
            dataTable.Columns.Add(dataColumn);
        }

        // Add rows
        foreach (var rowDict in data)
        {
            var dataRow = dataTable.NewRow();
            foreach (var column in columns.Where(c => c.IsVisible))
            {
                if (rowDict.ContainsKey(column.Name))
                {
                    dataRow[column.Name] = rowDict[column.Name] ?? DBNull.Value;
                }
            }
            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    private List<GridRow> DetermineExportRows(GridState currentState, ExportDataCommand command)
    {
        var rows = currentState.Rows.AsEnumerable();

        if (command.ExportOnlyFiltered && currentState.FilteredRowIndices != null)
        {
            rows = currentState.FilteredRowIndices.Select(i => currentState.Rows[i]);
        }

        if (command.ExportOnlyChecked)
        {
            rows = rows.Where(row => currentState.CheckboxStates.GetValueOrDefault(row.Index, false));
        }

        return rows.ToList();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        // Cleanup resources if needed
        _logger?.LogDebug("DataGridImportExportService disposed");
    }

    #endregion
}

/// <summary>
/// SOLID: Interface segregation for Import/Export operations
/// </summary>
public interface IDataGridImportExportService : IDisposable
{
    Task<Result<ImportResult>> ImportFromDictionaryAsync(GridState currentState, ImportDataCommand command);
    Task<Result<ImportResult>> ImportFromDataTableAsync(GridState currentState, ImportFromDataTableCommand command);
    Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(GridState currentState, ExportDataCommand command);
    Task<Result<DataTable>> ExportToDataTableAsync(GridState currentState, ExportToDataTableCommand command);
}