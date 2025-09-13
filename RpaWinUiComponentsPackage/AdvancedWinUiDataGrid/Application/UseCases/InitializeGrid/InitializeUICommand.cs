using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.InitializeGrid;

/// <summary>
/// COMPATIBILITY: UI-specific initialization command
/// </summary>
internal sealed record InitializeUICommand : InitializeGridCommand
{
    internal bool EnableUI { get; init; } = true;
    internal string? WindowTitle { get; init; }
    internal int? InitialWidth { get; init; }
    internal int? InitialHeight { get; init; }
    
    internal static InitializeUICommand Create(
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