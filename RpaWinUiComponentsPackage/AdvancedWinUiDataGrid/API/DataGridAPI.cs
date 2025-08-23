using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Common;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.DataGrid;
using System.Data;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.API;

/// <summary>
/// Static API for headless DataGrid operations
/// NON-UI: Pure functional API for automation scripts
/// BACKWARD COMPATIBILITY: Matches existing method signatures where possible
/// </summary>
internal static class DataGridAPI
{
    /// <summary>
    /// Create a new headless DataGrid service instance
    /// NON-UI: Returns service that can operate without UI
    /// </summary>
    /// <param name="logger">Optional logger for operations</param>
    /// <returns>New DataGrid service instance</returns>
    public static IDataGridService CreateService(ILogger? logger = null)
    {
        return new DataGridService(logger);
    }

    /// <summary>
    /// Create and initialize a DataGrid service in one step
    /// NON-UI: Convenience method for quick setup
    /// </summary>
    /// <param name="columns">Column definitions</param>
    /// <param name="configuration">Optional configuration</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>Initialized service or error result</returns>
    public static async Task<Result<IDataGridService>> CreateAndInitializeAsync(
        IReadOnlyList<DomainColumnDefinition> columns,
        DataGridConfiguration? configuration = null,
        ILogger? logger = null)
    {
        try
        {
            var service = new DataGridService(logger);
            var initResult = await service.InitializeAsync(columns, configuration);
            
            return initResult.IsSuccess 
                ? Result<IDataGridService>.Success(service)
                : Result<IDataGridService>.Failure(initResult.ErrorMessage ?? "Initialization failed");
        }
        catch (Exception ex)
        {
            return Result<IDataGridService>.Failure(ex);
        }
    }

    #region Quick Operations - Static Convenience Methods

    /// <summary>
    /// Quick import from dictionaries without maintaining service state
    /// NON-UI: One-shot operation for simple automation
    /// </summary>
    /// <param name="data">Data to import</param>
    /// <param name="columns">Column schema</param>
    /// <param name="options">Import options</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>Import result</returns>
    public static async Task<Result<ImportResult>> QuickImportAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        IReadOnlyList<DomainColumnDefinition> columns,
        ImportOptions? options = null,
        ILogger? logger = null)
    {
        using var service = new DataGridService(logger);
        
        var initResult = await service.InitializeAsync(columns);
        if (!initResult.IsSuccess)
            return Result<ImportResult>.Failure(initResult.ErrorMessage ?? "Initialization failed");
        
        return await service.ImportDataAsync(data, options);
    }

    /// <summary>
    /// Quick import from DataTable without maintaining service state
    /// NON-UI: One-shot operation for DataTable conversion
    /// </summary>
    /// <param name="dataTable">DataTable to import</param>
    /// <param name="options">Import options</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>Import result</returns>
    public static async Task<Result<ImportResult>> QuickImportAsync(
        DataTable dataTable,
        ImportOptions? options = null,
        ILogger? logger = null)
    {
        if (dataTable == null)
            return Result<ImportResult>.Failure("DataTable cannot be null");

        // FUNCTIONAL: Auto-generate columns from DataTable schema
        var columns = GenerateColumnsFromDataTable(dataTable);
        
        using var service = new DataGridService(logger);
        
        var initResult = await service.InitializeAsync(columns);
        if (!initResult.IsSuccess)
            return Result<ImportResult>.Failure(initResult.ErrorMessage ?? "Initialization failed");
        
        return await service.ImportDataAsync(dataTable, options);
    }

    /// <summary>
    /// Quick validation without maintaining service state
    /// NON-UI: Validate data against schema
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <param name="columns">Column schema</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>Validation result</returns>
    public static async Task<Result<ValidationResult>> QuickValidateAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        IReadOnlyList<DomainColumnDefinition> columns,
        ILogger? logger = null)
    {
        using var service = new DataGridService(logger);
        
        var initResult = await service.InitializeAsync(columns);
        if (!initResult.IsSuccess)
            return Result<ValidationResult>.Failure(initResult.ErrorMessage ?? "Initialization failed");
        
        var importResult = await service.ImportDataAsync(data, ImportOptions.Default);
        if (!importResult.IsSuccess)
            return Result<ValidationResult>.Failure(importResult.ErrorMessage ?? "Import failed");
        
        return await service.ValidateAllAsync();
    }

    /// <summary>
    /// Transform data from one format to another
    /// NON-UI: Pure data transformation pipeline
    /// </summary>
    /// <param name="inputData">Input data</param>
    /// <param name="columns">Column schema for transformation</param>
    /// <param name="exportOptions">Export formatting options</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>Transformed data</returns>
    public static async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> TransformDataAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> inputData,
        IReadOnlyList<DomainColumnDefinition> columns,
        ExportOptions? exportOptions = null,
        ILogger? logger = null)
    {
        using var service = new DataGridService(logger);
        
        var initResult = await service.InitializeAsync(columns);
        if (!initResult.IsSuccess)
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(
                initResult.ErrorMessage ?? "Initialization failed");
        
        var importResult = await service.ImportDataAsync(inputData);
        if (!importResult.IsSuccess)
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure(
                importResult.ErrorMessage ?? "Import failed");
        
        return await service.ExportToDictionariesAsync(exportOptions);
    }

    #endregion

    #region Schema Helpers - Functional Utilities

    /// <summary>
    /// Generate column definitions from DataTable schema
    /// FUNCTIONAL: Pure schema inference
    /// </summary>
    /// <param name="dataTable">Source DataTable</param>
    /// <returns>Inferred column definitions</returns>
    public static IReadOnlyList<DomainColumnDefinition> GenerateColumnsFromDataTable(DataTable dataTable)
    {
        if (dataTable == null) 
            return Array.Empty<DomainColumnDefinition>();

        return dataTable.Columns.Cast<DataColumn>()
            .Select(column => new DomainColumnDefinition(
                column.ColumnName,
                column.DataType,
                !column.AllowDBNull,
                column.ReadOnly,
                column.DefaultValue == DBNull.Value ? null : column.DefaultValue,
                column.DataType == typeof(string) && column.MaxLength > 0 ? column.MaxLength : null))
            .ToArray();
    }

    /// <summary>
    /// Generate column definitions from dictionary data
    /// FUNCTIONAL: Pure schema inference from data
    /// </summary>
    /// <param name="data">Sample data for schema inference</param>
    /// <param name="inferRequired">Whether to infer required fields</param>
    /// <returns>Inferred column definitions</returns>
    public static IReadOnlyList<DomainColumnDefinition> GenerateColumnsFromData(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        bool inferRequired = false)
    {
        if (data == null || data.Count == 0)
            return Array.Empty<DomainColumnDefinition>();

        // FUNCTIONAL: Collect all unique column names
        var allColumnNames = data
            .SelectMany(row => row.Keys)
            .Distinct()
            .ToArray();

        return allColumnNames.Select(columnName =>
        {
            // FUNCTIONAL: Infer type from sample values
            var sampleValues = data
                .Where(row => row.ContainsKey(columnName) && row[columnName] != null)
                .Select(row => row[columnName])
                .Take(10) // Sample first 10 non-null values
                .ToArray();

            var inferredType = InferTypeFromValues(sampleValues);
            
            // FUNCTIONAL: Infer if required (all rows have non-null values)
            var isRequired = inferRequired && data.All(row => 
                row.ContainsKey(columnName) && 
                row[columnName] != null && 
                !string.IsNullOrWhiteSpace(row[columnName]?.ToString()));

            return new DomainColumnDefinition(
                columnName,
                inferredType,
                isRequired);
        }).ToArray();
    }

    /// <summary>
    /// Create common column sets for typical scenarios
    /// FUNCTIONAL: Predefined schema templates
    /// </summary>
    public static class CommonColumns
    {
        public static IReadOnlyList<DomainColumnDefinition> BasicText(params string[] columnNames) =>
            columnNames.Select(name => DomainColumnDefinition.String(name)).ToArray();

        public static IReadOnlyList<DomainColumnDefinition> RequiredText(params string[] columnNames) =>
            columnNames.Select(name => DomainColumnDefinition.String(name, required: true)).ToArray();

        public static IReadOnlyList<DomainColumnDefinition> Mixed(params (string name, Type type, bool required)[] columns) =>
            columns.Select(col => new DomainColumnDefinition(col.name, col.type, col.required)).ToArray();

        public static IReadOnlyList<DomainColumnDefinition> Spreadsheet() => new[]
        {
            DomainColumnDefinition.String("A"), DomainColumnDefinition.String("B"), DomainColumnDefinition.String("C"),
            DomainColumnDefinition.String("D"), DomainColumnDefinition.String("E"), DomainColumnDefinition.String("F")
        };
    }

    #endregion

    #region Private Helpers

    private static Type InferTypeFromValues(object?[] values)
    {
        if (values.Length == 0) return typeof(string);

        // Try to infer the most specific type that fits all values
        var nonNullValues = values.Where(v => v != null).ToArray();
        if (nonNullValues.Length == 0) return typeof(string);

        // Check if all are same type
        var firstType = nonNullValues[0]!.GetType();
        if (nonNullValues.All(v => v!.GetType() == firstType))
            return firstType;

        // Try common conversions
        if (nonNullValues.All(v => int.TryParse(v?.ToString(), out _)))
            return typeof(int);

        if (nonNullValues.All(v => decimal.TryParse(v?.ToString(), out _)))
            return typeof(decimal);

        if (nonNullValues.All(v => DateTime.TryParse(v?.ToString(), out _)))
            return typeof(DateTime);

        if (nonNullValues.All(v => bool.TryParse(v?.ToString(), out _)))
            return typeof(bool);

        // Default to string
        return typeof(string);
    }

    #endregion
}