using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.SearchGrid;

/// <summary>
/// CQRS: Command for performing search operations
/// </summary>
internal sealed record SearchCommand
{
    internal required string SearchTerm { get; init; }
    internal SearchOptions? Options { get; init; }
    internal IReadOnlyList<string>? ColumnNames { get; init; }
    internal bool CaseSensitive { get; init; } = false;
    internal SearchType SearchType { get; init; } = SearchType.Contains;

    internal static SearchCommand Create(
        string searchTerm,
        SearchOptions? options = null) =>
        new()
        {
            SearchTerm = searchTerm,
            Options = options
        };

    internal static SearchCommand CreateWithColumns(
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
internal sealed record ValidateAllCommand
{
    internal IProgress<ValidationProgress>? Progress { get; init; }
    internal bool StrictMode { get; init; } = false;
    internal IReadOnlyList<string>? ColumnsToValidate { get; init; }

    internal static ValidateAllCommand Create(
        IProgress<ValidationProgress>? progress = null,
        bool strictMode = false) =>
        new()
        {
            Progress = progress,
            StrictMode = strictMode
        };

    internal static ValidateAllCommand ForColumns(
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
internal sealed record AddRowCommand
{
    internal required Dictionary<string, object?> RowData { get; init; }
    internal int? InsertAtIndex { get; init; }
    internal bool ValidateBeforeAdd { get; init; } = true;
    
    // Backward compatibility alias
    internal int? InsertIndex => InsertAtIndex;

    internal static AddRowCommand Create(Dictionary<string, object?> rowData) =>
        new() { RowData = rowData };

    internal static AddRowCommand CreateAtIndex(Dictionary<string, object?> rowData, int index) =>
        new() { RowData = rowData, InsertAtIndex = index };
}

/// <summary>
/// CQRS: Command for updating row operations
/// </summary>
internal sealed record UpdateRowCommand
{
    internal required int RowIndex { get; init; }
    internal required Dictionary<string, object?> RowData { get; init; }
    internal bool ValidateBeforeUpdate { get; init; } = true;
    internal bool MergeWithExisting { get; init; } = false;
    
    // Backward compatibility aliases
    internal bool ValidateAfterUpdate => ValidateBeforeUpdate;
    internal Dictionary<string, object?> NewData => RowData;

    internal static UpdateRowCommand Create(int rowIndex, Dictionary<string, object?> rowData) =>
        new() { RowIndex = rowIndex, RowData = rowData };

    internal static UpdateRowCommand CreateMerging(int rowIndex, Dictionary<string, object?> rowData) =>
        new() { RowIndex = rowIndex, RowData = rowData, MergeWithExisting = true };
}

/// <summary>
/// CQRS: Command for deleting row operations
/// </summary>
internal sealed record DeleteRowCommand
{
    internal required int RowIndex { get; init; }
    internal bool RequireConfirmation { get; init; } = false;
    internal string? ConfirmationMessage { get; init; }
    
    // Backward compatibility alias
    internal bool SmartDelete => RequireConfirmation;

    internal static DeleteRowCommand Create(int rowIndex) =>
        new() { RowIndex = rowIndex };

    internal static DeleteRowCommand CreateWithConfirmation(int rowIndex, string confirmationMessage) =>
        new() 
        { 
            RowIndex = rowIndex, 
            RequireConfirmation = true, 
            ConfirmationMessage = confirmationMessage 
        };
}