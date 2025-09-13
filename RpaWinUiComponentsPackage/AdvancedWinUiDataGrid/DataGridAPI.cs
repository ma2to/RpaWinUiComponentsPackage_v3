using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// SENIOR DEVELOPER: Clean public API types for AdvancedWinUiDataGrid
/// CLEAN ARCHITECTURE: Simple DTOs for external consumers
/// NO INTERNAL DEPENDENCIES: Only basic .NET types exposed
/// </summary>

#region Column Definitions

/// <summary>
/// Public API: Column definition for DataGrid
/// SIMPLE: Only basic properties exposed to external consumers
/// </summary>
public sealed class DataGridColumn
{
    public required string Name { get; init; }
    public required string Header { get; init; }
    public required Type DataType { get; init; }
    public bool IsRequired { get; init; } = false;
    public bool IsReadOnly { get; init; } = false;
    public int Width { get; init; } = 120;
    public DataGridColumnType ColumnType { get; init; } = DataGridColumnType.Text;
    public string? ValidationPattern { get; init; }
    public int? MaxLength { get; init; }
}

/// <summary>
/// Public API: Column types supported by DataGrid
/// </summary>
public enum DataGridColumnType
{
    Text,
    Numeric,
    CheckBox,
    Required,
    ValidAlerts,
    DeleteRow
}

#endregion

#region Configuration Types

/// <summary>
/// Public API: Theme configuration for DataGrid
/// </summary>
public enum DataGridTheme
{
    Light,
    Dark,
    Auto
}

/// <summary>
/// Public API: Validation configuration for DataGrid
/// </summary>
public sealed class DataGridValidationConfig
{
    public bool EnableValidation { get; init; } = true;
    public bool EnableRealTimeValidation { get; init; } = false;
    public bool StrictValidation { get; init; } = false;
    public bool ValidateEmptyRows { get; init; } = false;
}

/// <summary>
/// Public API: Performance configuration for DataGrid
/// </summary>
public sealed class DataGridPerformanceConfig
{
    public bool EnableVirtualization { get; init; } = true;
    public int VirtualizationThreshold { get; init; } = 1000;
    public bool EnableBackgroundProcessing { get; init; } = false;
}

/// <summary>
/// Public API: Logging configuration for DataGrid
/// </summary>
public sealed class DataGridLoggingConfig
{
    public string CategoryPrefix { get; init; } = "DataGrid";
    public bool LogMethodParameters { get; init; } = false;
    public bool LogPerformanceMetrics { get; init; } = true;
    public bool LogErrors { get; init; } = true;
    public DataGridLoggingLevel MinimumLevel { get; init; } = DataGridLoggingLevel.Information;
}

/// <summary>
/// Public API: Logging levels
/// </summary>
public enum DataGridLoggingLevel
{
    Debug,
    Information,
    Warning,
    Error
}

#endregion

#region Result Types

/// <summary>
/// Public API: Generic result wrapper
/// </summary>
public sealed class DataGridResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static DataGridResult Success() => new() { IsSuccess = true };
    public static DataGridResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

/// <summary>
/// Public API: Generic result wrapper with value
/// </summary>
public sealed class DataGridResult<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Value { get; init; }
    
    public static DataGridResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static DataGridResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

/// <summary>
/// Public API: Import operation statistics
/// </summary>
public sealed class DataGridImportStats
{
    public int TotalRows { get; init; }
    public int ImportedRows { get; init; }
    public int SuccessfulRows { get; init; }
    public int FailedRows { get; init; }
    public int ErrorRows { get; init; }
    public TimeSpan Duration { get; init; }
}

#endregion