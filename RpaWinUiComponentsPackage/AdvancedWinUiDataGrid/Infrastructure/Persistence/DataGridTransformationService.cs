using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Persistence;

/// <summary>
/// INFRASTRUCTURE: Data transformation service implementation
/// CLEAN ARCHITECTURE: Infrastructure layer - handles data conversion
/// RESPONSIBILITY: Transform between external data formats and domain models
/// </summary>
public sealed class DataGridTransformationService : IDataGridTransformationService
{
    #region ENTERPRISE: Private Fields
    
    private readonly ILogger? _logger;
    
    #endregion

    #region ENTERPRISE: Constructor
    
    public DataGridTransformationService(ILogger? logger = null)
    {
        _logger = logger;
    }
    
    #endregion

    #region INTERFACE: Dictionary Transformations
    
    public List<Dictionary<string, object?>> TransformFromDictionary(
        List<Dictionary<string, object?>> inputData,
        IReadOnlyList<ColumnDefinition> columns)
    {
        try
        {
            _logger?.LogDebug("Transforming {RowCount} dictionary rows", inputData.Count);
            
            var transformedData = new List<Dictionary<string, object?>>();
            
            foreach (var rowData in inputData)
            {
                var transformed = TransformRowData(rowData, columns, isImport: true);
                transformedData.Add(transformed);
            }
            
            _logger?.LogDebug("Successfully transformed {RowCount} dictionary rows", transformedData.Count);
            return transformedData;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transform dictionary data");
            return new List<Dictionary<string, object?>>();
        }
    }
    
    public List<Dictionary<string, object?>> TransformToDictionary(
        IReadOnlyList<Dictionary<string, object?>> internalData,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeValidAlerts = false)
    {
        try
        {
            _logger?.LogDebug("Transforming {RowCount} internal data to dictionary", internalData.Count);
            
            var dictionaries = new List<Dictionary<string, object?>>();
            
            foreach (var rowData in internalData.Where(r => !IsRowEmpty(r)))
            {
                var dictionary = new Dictionary<string, object?>();
                
                foreach (var column in columns.Where(c => c.IsVisible))
                {
                    if (rowData.TryGetValue(column.Name, out var value))
                    {
                        dictionary[column.Name] = TransformValueForExport(value, column);
                    }
                    else
                    {
                        dictionary[column.Name] = GetDefaultValue(column.DataType);
                    }
                }
                
                dictionaries.Add(dictionary);
            }
            
            _logger?.LogDebug("Successfully transformed {RowCount} rows to dictionary", dictionaries.Count);
            return dictionaries;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transform to dictionary");
            return new List<Dictionary<string, object?>>();
        }
    }
    
    #endregion

    #region INTERFACE: DataTable Transformations
    
    public List<Dictionary<string, object?>> TransformFromDataTable(
        DataTable dataTable,
        IReadOnlyList<ColumnDefinition> columns)
    {
        try
        {
            _logger?.LogDebug("Transforming DataTable with {RowCount} rows", dataTable.Rows.Count);
            
            var dictionaries = new List<Dictionary<string, object?>>();
            
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var rowData = new Dictionary<string, object?>();
                
                // Transform DataTable columns to row data
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    var value = dataRow[dataColumn];
                    
                    // Handle DBNull
                    if (value == DBNull.Value)
                        value = null;
                    
                    rowData[dataColumn.ColumnName] = value;
                }
                
                // Ensure all columns are represented
                foreach (var column in columns)
                {
                    if (!rowData.ContainsKey(column.Name))
                    {
                        rowData[column.Name] = GetDefaultValue(column.DataType);
                    }
                }
                
                dictionaries.Add(rowData);
            }
            
            _logger?.LogDebug("Successfully transformed DataTable to {RowCount} dictionaries", dictionaries.Count);
            return dictionaries;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transform DataTable");
            return new List<Dictionary<string, object?>>();
        }
    }
    
    public DataTable TransformToDataTable(
        IReadOnlyList<Dictionary<string, object?>> data,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeValidAlerts = false)
    {
        try
        {
            _logger?.LogDebug("Transforming {RowCount} dictionaries to DataTable", data.Count);
            
            var dataTable = new DataTable();
            
            // Create DataTable columns
            foreach (var column in columns.Where(c => c.IsVisible))
            {
                var dataType = GetDataTableType(column.DataType);
                var dataColumn = new DataColumn(column.Name, dataType);
                dataColumn.AllowDBNull = true;
                dataColumn.Caption = column.DisplayName ?? column.Name;
                
                dataTable.Columns.Add(dataColumn);
            }
            
            // Add validation alerts column if requested
            if (includeValidAlerts)
            {
                var validationColumn = new DataColumn("ValidationAlerts", typeof(string));
                validationColumn.AllowDBNull = true;
                dataTable.Columns.Add(validationColumn);
            }
            
            // Add data rows
            foreach (var rowData in data.Where(r => !IsRowEmpty(r)))
            {
                var dataRow = dataTable.NewRow();
                
                foreach (var column in columns.Where(c => c.IsVisible))
                {
                    if (rowData.TryGetValue(column.Name, out var value))
                    {
                        dataRow[column.Name] = value ?? DBNull.Value;
                    }
                    else
                    {
                        dataRow[column.Name] = DBNull.Value;
                    }
                }
                
                dataTable.Rows.Add(dataRow);
            }
            
            _logger?.LogDebug("Successfully transformed {RowCount} dictionaries to DataTable", dataTable.Rows.Count);
            return dataTable;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transform to DataTable");
            return new DataTable();
        }
    }
    
    #endregion

    #region ENTERPRISE: Private Helper Methods
    
    public Dictionary<string, object?> TransformRowData(
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns,
        bool isImport = true)
    {
        var transformedData = new Dictionary<string, object?>();
        
        foreach (var column in columns)
        {
            if (rowData.TryGetValue(column.Name, out var value))
            {
                transformedData[column.Name] = isImport 
                    ? TransformValueForImport(value, column)
                    : TransformValueForExport(value, column);
            }
            else
            {
                transformedData[column.Name] = GetDefaultValue(column.DataType);
            }
        }
        
        // Include any additional data from source that isn't in columns
        foreach (var kvp in rowData)
        {
            if (!transformedData.ContainsKey(kvp.Key))
            {
                transformedData[kvp.Key] = kvp.Value;
            }
        }
        
        return transformedData;
    }
    
    public object? TransformValueForImport(object? value, ColumnDefinition column)
    {
        if (value == null) return null;
        
        try
        {
            // Handle boolean values
            if (column.DataType == typeof(bool) || column.DataType == typeof(bool?))
            {
                return value switch
                {
                    bool b => b,
                    string s => bool.TryParse(s, out var b) ? b : false,
                    int i => i != 0,
                    _ => false
                };
            }
            
            // Handle type conversion
            if (column.DataType == typeof(string))
                return value.ToString();
            
            if (column.DataType == typeof(int) || column.DataType == typeof(int?))
            {
                if (int.TryParse(value.ToString(), out var intValue))
                    return intValue;
                return column.DataType == typeof(int?) ? (int?)null : 0;
            }
            
            if (column.DataType == typeof(double) || column.DataType == typeof(double?))
            {
                if (double.TryParse(value.ToString(), out var doubleValue))
                    return doubleValue;
                return column.DataType == typeof(double?) ? (double?)null : 0.0;
            }
            
            if (column.DataType == typeof(DateTime) || column.DataType == typeof(DateTime?))
            {
                if (DateTime.TryParse(value.ToString(), out var dateValue))
                    return dateValue;
                return column.DataType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
            }
            
            if (column.DataType == typeof(bool) || column.DataType == typeof(bool?))
            {
                if (bool.TryParse(value.ToString(), out var boolValue))
                    return boolValue;
                return column.DataType == typeof(bool?) ? (bool?)null : false;
            }
            
            // Try direct conversion
            if (value.GetType() == column.DataType)
                return value;
                
            return Convert.ChangeType(value, column.DataType);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to convert value {Value} to type {Type} for column {Column}", 
                value, column.DataType.Name, column.Name);
            return GetDefaultValue(column.DataType);
        }
    }
    
    public object? TransformValueForExport(object? value, ColumnDefinition column)
    {
        if (value == null) return null;
        
        // Special handling for boolean values
        if ((column.DataType == typeof(bool) || column.DataType == typeof(bool?)) && value is bool boolValue)
        {
            return boolValue; // Keep as boolean for dictionary/DataTable export
        }
        
        return value;
    }
    
    public bool IsRowEmpty(Dictionary<string, object?> rowData)
    {
        return rowData.Values.All(v => v == null || 
            (v is string s && string.IsNullOrWhiteSpace(s)));
    }
    
    private object? GetDefaultValue(Type type)
    {
        if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
    
    private Type GetDataTableType(Type columnType)
    {
        // Convert nullable types to their underlying types for DataTable
        var underlyingType = Nullable.GetUnderlyingType(columnType);
        if (underlyingType != null)
            return underlyingType;
            
        return columnType;
    }
    
    // Async versions of transformation methods
    public async Task<List<Dictionary<string, object?>>> TransformFromDictionaryAsync(
        List<Dictionary<string, object?>> inputData,
        IReadOnlyList<ColumnDefinition> columns)
    {
        return await Task.FromResult(TransformFromDictionary(inputData, columns));
    }
    
    public async Task<List<Dictionary<string, object?>>> TransformToDictionaryAsync(
        IReadOnlyList<Dictionary<string, object?>> internalData,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeValidAlerts = false)
    {
        return await Task.FromResult(TransformToDictionary(internalData, columns, includeValidAlerts));
    }
    
    #endregion
}