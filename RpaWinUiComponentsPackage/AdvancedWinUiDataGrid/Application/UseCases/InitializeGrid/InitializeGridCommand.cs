using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.InitializeGrid;

/// <summary>
/// Command for initializing the data grid
/// </summary>
internal record InitializeGridCommand
{
    internal IReadOnlyList<ColumnDefinition> Columns { get; init; } = [];
    internal ColorConfiguration? ColorConfiguration { get; init; }
    internal ValidationConfiguration? ValidationConfiguration { get; init; }
    internal PerformanceConfiguration? PerformanceConfiguration { get; init; }
    internal int MinimumRows { get; init; } = 1;

    internal static InitializeGridCommand Create(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null,
        int minimumRows = 1)
    {
        return new InitializeGridCommand
        {
            Columns = columns,
            ColorConfiguration = colorConfiguration,
            ValidationConfiguration = validationConfiguration,
            PerformanceConfiguration = performanceConfiguration,
            MinimumRows = minimumRows
        };
    }
    
    /// <summary>
    /// Validate the command parameters
    /// </summary>
    internal virtual Result<bool> Validate()
    {
        if (Columns == null || Columns.Count == 0)
            return Result<bool>.Failure("Columns cannot be null or empty");
            
        if (MinimumRows < 0)
            return Result<bool>.Failure("MinimumRows cannot be negative");
            
        return Result<bool>.Success(true);
    }
}

/// <summary>
/// COMPATIBILITY: Type alias for InitializeGridCommand
/// </summary>
internal sealed record InitializeDataGridCommand : InitializeGridCommand
{
    internal static new InitializeDataGridCommand Create(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null,
        int minimumRows = 1)
    {
        return new InitializeDataGridCommand
        {
            Columns = columns,
            ColorConfiguration = colorConfiguration,
            ValidationConfiguration = validationConfiguration,
            PerformanceConfiguration = performanceConfiguration,
            MinimumRows = minimumRows
        };
    }
}