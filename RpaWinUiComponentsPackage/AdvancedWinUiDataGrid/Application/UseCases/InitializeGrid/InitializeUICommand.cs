using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.InitializeGrid;

/// <summary>
/// COMPATIBILITY: UI-specific initialization command
/// </summary>
public sealed record InitializeUICommand : InitializeGridCommand
{
    public bool EnableUI { get; init; } = true;
    public string? WindowTitle { get; init; }
    public int? InitialWidth { get; init; }
    public int? InitialHeight { get; init; }
    
    public static InitializeUICommand Create(
        IReadOnlyList<ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null,
        int minimumRows = 1,
        bool enableUI = true,
        string? windowTitle = null) =>
        new()
        {
            Columns = columns,
            ColorConfiguration = colorConfiguration,
            ValidationConfiguration = validationConfiguration,
            PerformanceConfiguration = performanceConfiguration,
            MinimumRows = minimumRows,
            EnableUI = enableUI,
            WindowTitle = windowTitle
        };
}