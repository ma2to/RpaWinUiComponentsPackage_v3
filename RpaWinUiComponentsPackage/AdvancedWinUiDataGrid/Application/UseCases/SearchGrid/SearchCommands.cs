using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid;

/// <summary>
/// CQRS: Command for performing search operations
/// </summary>
public sealed record SearchCommand
{
    public required string SearchTerm { get; init; }
    public SearchOptions? Options { get; init; }
    public IReadOnlyList<string>? ColumnNames { get; init; }
    public bool CaseSensitive { get; init; } = false;
    public SearchType SearchType { get; init; } = SearchType.Contains;

    public static SearchCommand Create(
        string searchTerm,
        SearchOptions? options = null) =>
        new()
        {
            SearchTerm = searchTerm,
            Options = options
        };

    public static SearchCommand CreateWithColumns(
        string searchTerm,
        IReadOnlyList<string> columnNames,
        bool caseSensitive = false,
        SearchType searchType = SearchType.Contains) =>
        new()
        {
            SearchTerm = searchTerm,
            ColumnNames = columnNames,
            CaseSensitive = caseSensitive,
            SearchType = searchType
        };
}

/// <summary>
/// CQRS: Command for validation operations
/// </summary>
public sealed record ValidateAllCommand
{
    public IProgress<ValidationProgress>? Progress { get; init; }
    public bool StrictMode { get; init; } = false;
    public IReadOnlyList<string>? ColumnsToValidate { get; init; }

    public static ValidateAllCommand Create(
        IProgress<ValidationProgress>? progress = null,
        bool strictMode = false) =>
        new()
        {
            Progress = progress,
            StrictMode = strictMode
        };

    public static ValidateAllCommand ForColumns(
        IReadOnlyList<string> columnsToValidate,
        IProgress<ValidationProgress>? progress = null,
        bool strictMode = false) =>
        new()
        {
            ColumnsToValidate = columnsToValidate,
            Progress = progress,
            StrictMode = strictMode
        };
}

/// <summary>
/// CQRS: Command for row management operations
/// </summary>
public sealed record AddRowCommand
{
    public required Dictionary<string, object?> RowData { get; init; }
    public int? InsertAtIndex { get; init; }
    public bool ValidateBeforeAdd { get; init; } = true;
    
    // Backward compatibility alias
    public int? InsertIndex => InsertAtIndex;

    public static AddRowCommand Create(Dictionary<string, object?> rowData) =>
        new() { RowData = rowData };

    public static AddRowCommand CreateAtIndex(Dictionary<string, object?> rowData, int index) =>
        new() { RowData = rowData, InsertAtIndex = index };
}

/// <summary>
/// CQRS: Command for updating row operations
/// </summary>
public sealed record UpdateRowCommand
{
    public required int RowIndex { get; init; }
    public required Dictionary<string, object?> RowData { get; init; }
    public bool ValidateBeforeUpdate { get; init; } = true;
    public bool MergeWithExisting { get; init; } = false;
    
    // Backward compatibility aliases
    public bool ValidateAfterUpdate => ValidateBeforeUpdate;
    public Dictionary<string, object?> NewData => RowData;

    public static UpdateRowCommand Create(int rowIndex, Dictionary<string, object?> rowData) =>
        new() { RowIndex = rowIndex, RowData = rowData };

    public static UpdateRowCommand CreateMerging(int rowIndex, Dictionary<string, object?> rowData) =>
        new() { RowIndex = rowIndex, RowData = rowData, MergeWithExisting = true };
}

/// <summary>
/// CQRS: Command for deleting row operations
/// </summary>
public sealed record DeleteRowCommand
{
    public required int RowIndex { get; init; }
    public bool RequireConfirmation { get; init; } = false;
    public string? ConfirmationMessage { get; init; }
    
    // Backward compatibility alias
    public bool SmartDelete => RequireConfirmation;

    public static DeleteRowCommand Create(int rowIndex) =>
        new() { RowIndex = rowIndex };

    public static DeleteRowCommand CreateWithConfirmation(int rowIndex, string confirmationMessage) =>
        new() 
        { 
            RowIndex = rowIndex, 
            RequireConfirmation = true, 
            ConfirmationMessage = confirmationMessage 
        };
}