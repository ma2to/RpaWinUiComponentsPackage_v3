using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

/// <summary>
/// CLIPBOARD: Interface for clipboard operations
/// EXCEL_COMPATIBLE: Excel format support
/// ENTERPRISE: Production-ready copy/paste interface
/// </summary>
public interface IClipboardService : IDisposable
{
    /// <summary>
    /// Copy selected data to clipboard in Excel-compatible format
    /// </summary>
    Task<Result<bool>> CopyToClipboardAsync(
        IReadOnlyList<Dictionary<string, object?>> selectedRows,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeHeaders = true);

    /// <summary>
    /// Copy single cell value to clipboard
    /// </summary>
    Task<Result<bool>> CopyCellAsync(object? cellValue);

    /// <summary>
    /// Paste data from clipboard into grid format
    /// </summary>
    Task<Result<ClipboardParseResult>> PasteFromClipboardAsync(
        IReadOnlyList<ColumnDefinition> targetColumns,
        int startRowIndex = 0,
        int startColumnIndex = 0);
}