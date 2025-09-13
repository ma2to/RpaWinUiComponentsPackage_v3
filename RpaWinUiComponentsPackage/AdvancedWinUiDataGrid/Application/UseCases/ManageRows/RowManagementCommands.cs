using System;
using System.Collections.Generic;
using System.Data;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ManageRows;

// Note: ImportProgress and ExportProgress are now defined in Domain/ValueObjects/ProgressTracking.cs

/// <summary>
/// CLEAN ARCHITECTURE: Application layer commands (Use Cases)
/// DDD: Commands represent business intentions/actions
/// CQRS: Command side of Command Query Responsibility Segregation
/// IMMUTABLE: Commands are immutable data structures
/// </summary>

/// <summary>
/// APPLICATION: Initialize DataGrid command
/// DDD: Use case for DataGrid initialization
/// </summary>
internal sealed record InitializeDataGridCommand(
    IReadOnlyList<ColumnDefinition> Columns,
    ColorConfiguration? ColorConfiguration = null,
    ValidationConfiguration? ValidationConfiguration = null,
    PerformanceConfiguration? PerformanceConfiguration = null,
    int MinimumRows = 1)
{
    /// <summary>
    /// ENTERPRISE: Command validation
    /// DDD: Business rule validation at application boundary
    /// </summary>
    internal IEnumerable<string> Validate()
    {
        if (Columns == null || Columns.Count == 0)
            yield return "Columns collection cannot be empty";
        
        if (MinimumRows < 0)
            yield return "MinimumRows cannot be negative";
            
        // Validate column names are unique
        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in Columns)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
                yield return "Column name cannot be empty";
                
            if (!columnNames.Add(column.Name))
                yield return $"Duplicate column name: {column.Name}";
        }
        
        // Validate special columns
        var deleteColumns = 0;
        var validAlertsColumns = 0;
        
        foreach (var column in Columns)
        {
            if (column.SpecialType == SpecialColumnType.DeleteRow)
                deleteColumns++;
            else if (column.SpecialType == SpecialColumnType.ValidAlerts)
                validAlertsColumns++;
        }
        
        if (deleteColumns > 1)
            yield return "Only one DeleteRow column is allowed";
            
        if (validAlertsColumns > 1)
            yield return "Only one ValidAlerts column is allowed";
    }
}

/// <summary>
/// APPLICATION: Import from dictionary command
/// </summary>
internal sealed record ImportFromDictionaryCommand(
    List<Dictionary<string, object?>> Data,
    Dictionary<int, bool>? CheckboxStates = null,
    int StartRow = 1,
    ImportMode Mode = ImportMode.Replace,
    TimeSpan? Timeout = null,
    IProgress<ImportProgress>? Progress = null)
{
    internal IEnumerable<string> Validate()
    {
        if (Data == null)
            yield return "Data cannot be null";
            
        if (Data.Count == 0)
            yield return "Data cannot be empty";
            
        if (StartRow < 1)
            yield return "StartRow must be >= 1";
            
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Import from DataTable command
/// </summary>
internal sealed record ImportFromDataTableCommand(
    DataTable DataTable,
    Dictionary<int, bool>? CheckboxStates = null,
    int StartRow = 1,
    ImportMode Mode = ImportMode.Replace,
    TimeSpan? Timeout = null,
    IProgress<ImportProgress>? Progress = null)
{
    internal IEnumerable<string> Validate()
    {
        if (DataTable == null)
            yield return "DataTable cannot be null";
            
        if (DataTable.Rows.Count == 0)
            yield return "DataTable cannot be empty";
            
        if (StartRow < 1)
            yield return "StartRow must be >= 1";
            
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Export to dictionary command
/// </summary>
internal sealed record ExportToDictionaryCommand(
    bool ExportOnlyFiltered = false,
    bool IncludeValidationAlerts = false,
    bool ExportOnlyChecked = false,
    TimeSpan? Timeout = null,
    IProgress<ExportProgress>? Progress = null)
{
    internal IEnumerable<string> Validate()
    {
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Export to DataTable command
/// </summary>
internal sealed record ExportToDataTableCommand(
    bool ExportOnlyFiltered = false,
    bool IncludeValidationAlerts = false,
    bool ExportOnlyChecked = false,
    TimeSpan? Timeout = null,
    IProgress<ExportProgress>? Progress = null)
{
    internal IEnumerable<string> Validate()
    {
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Search command
/// </summary>
internal sealed record SearchCommand(
    string SearchText,
    IReadOnlyList<string>? TargetColumns = null,
    bool CaseSensitive = false,
    bool UseRegex = false,
    int MaxResults = 1000,
    TimeSpan? Timeout = null)
{
    internal IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            yield return "SearchText cannot be empty";
            
        if (MaxResults <= 0)
            yield return "MaxResults must be positive";
            
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Apply filters command
/// </summary>
internal sealed record ApplyFiltersCommand(
    IReadOnlyList<FilterDefinition> Filters,
    FilterLogicOperator LogicOperator = FilterLogicOperator.And,
    TimeSpan? Timeout = null)
{
    internal IEnumerable<string> Validate()
    {
        if (Filters == null)
            yield return "Filters cannot be null";
            
        if (Filters.Count == 0)
            yield return "At least one filter is required";
            
        foreach (var filter in Filters)
        {
            if (string.IsNullOrWhiteSpace(filter.ColumnName))
                yield return "Filter column name cannot be empty";
        }
        
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Sort command
/// </summary>
internal sealed record SortCommand(
    string ColumnName,
    SortDirection Direction = SortDirection.Ascending,
    TimeSpan? Timeout = null)
{
    internal IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(ColumnName))
            yield return "ColumnName cannot be empty";
            
        if (Direction == SortDirection.None)
            yield return "SortDirection cannot be None";
            
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Validate all command
/// </summary>
internal sealed record ValidateAllCommand(
    bool OnlyFiltered = false,
    bool OnlyVisible = false,
    TimeSpan? Timeout = null,
    IProgress<ValidationProgress>? Progress = null)
{
    internal IEnumerable<string> Validate()
    {
        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            yield return "Timeout must be positive";
    }
}

/// <summary>
/// APPLICATION: Add row command
/// </summary>
internal sealed record AddRowCommand(
    Dictionary<string, object?> RowData,
    int? InsertIndex = null,
    bool ValidateBeforeAdd = true)
{
    internal IEnumerable<string> Validate()
    {
        if (RowData == null)
            yield return "RowData cannot be null";
            
        if (InsertIndex.HasValue && InsertIndex.Value < 0)
            yield return "InsertIndex cannot be negative";
    }
}

/// <summary>
/// APPLICATION: Delete row command
/// </summary>
internal sealed record DeleteRowCommand(
    int RowIndex,
    bool RequireConfirmation = true,
    bool SmartDelete = true)
{
    internal IEnumerable<string> Validate()
    {
        if (RowIndex < 0)
            yield return "RowIndex cannot be negative";
    }
}

/// <summary>
/// APPLICATION: Update row command
/// </summary>
internal sealed record UpdateRowCommand(
    int RowIndex,
    Dictionary<string, object?> NewData,
    bool ValidateAfterUpdate = true)
{
    internal IEnumerable<string> Validate()
    {
        if (RowIndex < 0)
            yield return "RowIndex cannot be negative";
            
        if (NewData == null)
            yield return "NewData cannot be null";
    }
}