using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.AutoRowHeight;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;

/// <summary>
/// APPLICATION: Auto row height service interface
/// ENTERPRISE: High-level orchestration of auto row height functionality
/// CLEAN ARCHITECTURE: Application layer service abstraction
/// </summary>
internal interface IDataGridAutoRowHeightService : IDisposable
{
    /// <summary>
    /// ORCHESTRATION: Calculate and apply row height for single row when data changes
    /// PERFORMANCE: Optimized for single row updates during editing
    /// </summary>
    Task<Result<bool>> UpdateRowHeightAsync(
        int rowIndex,
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns,
        double availableWidth);

    /// <summary>
    /// BATCH PROCESSING: Calculate and apply row heights for multiple rows
    /// PERFORMANCE: Optimized batch processing with progress reporting
    /// </summary>
    Task<Result<Dictionary<int, double>>> RefreshAllRowHeightsAsync(
        IReadOnlyList<Dictionary<string, object?>> rowsData,
        IReadOnlyList<ColumnDefinition> columns,
        double availableWidth,
        IProgress<BatchCalculationProgress>? progress = null);

    /// <summary>
    /// CONFIGURATION: Enable or disable auto row height functionality
    /// RUNTIME: Dynamic configuration changes
    /// </summary>
    Task<Result<bool>> SetEnabledAsync(bool enabled, bool refreshExistingRows = true);

    /// <summary>
    /// STATUS: Get current enabled state
    /// </summary>
    bool IsEnabled { get; }
}