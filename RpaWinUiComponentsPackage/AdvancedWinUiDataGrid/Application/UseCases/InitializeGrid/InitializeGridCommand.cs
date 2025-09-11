using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.InitializeGrid;

/// <summary>
/// Command for initializing the data grid
/// </summary>
public record InitializeGridCommand
{
    public IReadOnlyList<ColumnDefinition> Columns { get; init; } = [];
    public ColorConfiguration? ColorConfiguration { get; init; }
    public ValidationConfiguration? ValidationConfiguration { get; init; }
    public PerformanceConfiguration? PerformanceConfiguration { get; init; }
    public int MinimumRows { get; init; } = 1;

    public static InitializeGridCommand Create(
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
    public virtual Result<bool> Validate()
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
public sealed record InitializeDataGridCommand : InitializeGridCommand
{
    public static new InitializeDataGridCommand Create(
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