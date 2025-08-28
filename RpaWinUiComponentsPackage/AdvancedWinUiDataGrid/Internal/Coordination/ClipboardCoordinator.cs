using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Coordination;

/// <summary>
/// PROFESSIONAL Clipboard Coordinator - ONLY clipboard operations and data formatting
/// RESPONSIBILITY: Handle copy/paste data transformation, clipboard interaction (NO cell operations, NO UI updates)
/// SEPARATION: Pure clipboard data layer - text formatting, data conversion patterns
/// ANTI-GOD: Single responsibility - only clipboard coordination
/// </summary>
internal sealed class ClipboardCoordinator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private bool _disposed = false;

    // CLIPBOARD CONFIGURATION (Immutable pattern)
    private readonly record struct ClipboardConfiguration(
        string RowSeparator,
        string ColumnSeparator,
        bool IncludeHeaders,
        bool IncludeValidationData,
        string ValidationSuffix
    );

    private ClipboardConfiguration _config;

    public ClipboardCoordinator(
        ILogger? logger, 
        GlobalExceptionHandler exceptionHandler,
        string rowSeparator = "\n",
        string columnSeparator = "\t",
        bool includeHeaders = false,
        bool includeValidationData = false,
        string validationSuffix = "_ValidationError")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        
        _config = new ClipboardConfiguration(
            rowSeparator,
            columnSeparator,
            includeHeaders,
            includeValidationData,
            validationSuffix
        );
        
        _logger?.Info("📋 CLIPBOARD COORDINATOR: Initialized - Pure clipboard operations only");
        LogConfiguration();
    }

    // PUBLIC READ-ONLY PROPERTIES (Immutable exposure)
    public string RowSeparator => _config.RowSeparator;
    public string ColumnSeparator => _config.ColumnSeparator;
    public bool IncludeHeaders => _config.IncludeHeaders;
    public bool IncludeValidationData => _config.IncludeValidationData;

    /// <summary>
    /// Update clipboard configuration
    /// PURE CLIPBOARD: Only updates formatting configuration
    /// </summary>
    public async Task<Result<bool>> UpdateConfigurationAsync(
        string? rowSeparator = null,
        string? columnSeparator = null,
        bool? includeHeaders = null,
        bool? includeValidationData = null,
        string? validationSuffix = null)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📋 CONFIG UPDATE: Updating clipboard configuration");
            
            // Immutable update
            _config = _config with 
            {
                RowSeparator = rowSeparator ?? _config.RowSeparator,
                ColumnSeparator = columnSeparator ?? _config.ColumnSeparator,
                IncludeHeaders = includeHeaders ?? _config.IncludeHeaders,
                IncludeValidationData = includeValidationData ?? _config.IncludeValidationData,
                ValidationSuffix = validationSuffix ?? _config.ValidationSuffix
            };
            
            _logger?.Info("✅ CONFIG UPDATE: Clipboard configuration updated");
            LogConfiguration();
            
            await Task.CompletedTask;
            return true;
            
        }, "UpdateConfiguration", 1, false, _logger);
    }

    /// <summary>
    /// Format cell data for clipboard export
    /// PURE CLIPBOARD: Only formats data, no cell operations
    /// </summary>
    public async Task<Result<string>> FormatCellDataAsync(IReadOnlyList<DataGridCell> cells, IReadOnlyList<GridColumnDefinition>? headers = null)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📋 FORMAT: Formatting {CellCount} cells for clipboard", cells.Count);
            
            if (!cells.Any())
            {
                _logger?.Info("📋 FORMAT: No cells to format, returning empty string");
                return "";
            }

            var formattedRows = new List<string>();
            
            // Add headers if configured
            if (_config.IncludeHeaders && headers != null)
            {
                var headerNames = headers.Select(h => h.DisplayName).ToList();
                formattedRows.Add(string.Join(_config.ColumnSeparator, headerNames));
                _logger?.Info("📋 FORMAT: Added {HeaderCount} headers", headerNames.Count);
            }

            // Group cells by row
            var cellsByRow = cells
                .Where(c => c != null)
                .GroupBy(c => c.RowIndex)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var rowGroup in cellsByRow)
            {
                var rowCells = rowGroup.OrderBy(c => c.ColumnIndex).ToList();
                var cellValues = new List<string>();
                
                foreach (var cell in rowCells)
                {
                    // Add main cell value
                    var cellValue = FormatCellValue(cell.Value);
                    cellValues.Add(cellValue);
                    
                    // Add validation data if configured
                    if (_config.IncludeValidationData && cell.HasValidationErrors)
                    {
                        var validationValue = FormatValidationError(cell.ValidationError);
                        cellValues.Add(validationValue);
                    }
                }
                
                formattedRows.Add(string.Join(_config.ColumnSeparator, cellValues));
            }
            
            var result = string.Join(_config.RowSeparator, formattedRows);
            
            _logger?.Info("✅ FORMAT: Formatted {RowCount} rows for clipboard - Length: {Length}", 
                formattedRows.Count, result.Length);
            
            await Task.CompletedTask;
            return result;
            
        }, "FormatCellData", cells.Count, "", _logger);
    }

    /// <summary>
    /// Copy formatted data to system clipboard
    /// PURE CLIPBOARD: Only clipboard system interaction
    /// </summary>
    public async Task<Result<bool>> CopyToClipboardAsync(string formattedData, string? htmlData = null, string? rtfData = null)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📋 COPY: Copying data to clipboard - Length: {Length}", formattedData.Length);
            
            if (string.IsNullOrEmpty(formattedData))
            {
                _logger?.Warning("⚠️ COPY: No data to copy to clipboard");
                return false;
            }

            var dataPackage = new DataPackage();
            
            // Set text data (always included)
            dataPackage.SetText(formattedData);
            
            // Set HTML data if provided
            if (!string.IsNullOrEmpty(htmlData))
            {
                dataPackage.SetHtmlFormat(htmlData);
                _logger?.Info("📋 COPY: Added HTML format - Length: {Length}", htmlData.Length);
            }
            
            // Set RTF data if provided
            if (!string.IsNullOrEmpty(rtfData))
            {
                dataPackage.SetRtf(rtfData);
                _logger?.Info("📋 COPY: Added RTF format - Length: {Length}", rtfData.Length);
            }
            
            // Copy to clipboard
            Clipboard.SetContent(dataPackage);
            
            _logger?.Info("✅ COPY: Data copied to clipboard successfully");
            
            await Task.CompletedTask;
            return true;
            
        }, "CopyToClipboard", formattedData?.Length ?? 0, false, _logger);
    }

    /// <summary>
    /// Get data from system clipboard
    /// PURE CLIPBOARD: Only clipboard system interaction
    /// </summary>
    public async Task<Result<ClipboardData>> GetFromClipboardAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📋 PASTE: Getting data from clipboard");
            
            var dataPackageView = Clipboard.GetContent();
            var clipboardData = new ClipboardData(
                HasText: false,
                HasHtml: false,
                HasRtf: false,
                TextData: null,
                HtmlData: null,
                RtfData: null
            );

            // Get text data
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                var textData = await dataPackageView.GetTextAsync();
                clipboardData = clipboardData with 
                { 
                    HasText = true, 
                    TextData = textData 
                };
                _logger?.Info("📋 PASTE: Found text data - Length: {Length}", textData?.Length ?? 0);
            }

            // Get HTML data
            if (dataPackageView.Contains(StandardDataFormats.Html))
            {
                var htmlData = await dataPackageView.GetHtmlFormatAsync();
                clipboardData = clipboardData with 
                { 
                    HasHtml = true, 
                    HtmlData = htmlData 
                };
                _logger?.Info("📋 PASTE: Found HTML data - Length: {Length}", htmlData?.Length ?? 0);
            }

            // Get RTF data
            if (dataPackageView.Contains(StandardDataFormats.Rtf))
            {
                var rtfData = await dataPackageView.GetRtfAsync();
                clipboardData = clipboardData with 
                { 
                    HasRtf = true, 
                    RtfData = rtfData 
                };
                _logger?.Info("📋 PASTE: Found RTF data - Length: {Length}", rtfData?.Length ?? 0);
            }

            if (!clipboardData.HasText && !clipboardData.HasHtml && !clipboardData.HasRtf)
            {
                _logger?.Info("📋 PASTE: No supported data found in clipboard");
            }
            else
            {
                _logger?.Info("✅ PASTE: Retrieved clipboard data - Text: {Text}, HTML: {Html}, RTF: {Rtf}",
                    clipboardData.HasText, clipboardData.HasHtml, clipboardData.HasRtf);
            }

            return clipboardData;
            
        }, "GetFromClipboard", 1, new ClipboardData(false, false, false, null, null, null), _logger);
    }

    /// <summary>
    /// Parse clipboard text data into structured format
    /// PURE CLIPBOARD: Only data parsing, no cell operations
    /// </summary>
    public async Task<Result<ParsedClipboardData>> ParseClipboardTextAsync(string clipboardText)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📋 PARSE: Parsing clipboard text - Length: {Length}", clipboardText?.Length ?? 0);
            
            if (string.IsNullOrEmpty(clipboardText))
            {
                return new ParsedClipboardData(new List<List<string>>(), 0, 0);
            }

            // Split into rows
            var rows = clipboardText.Split(new[] { _config.RowSeparator, "\r\n", "\n" }, StringSplitOptions.None);
            var parsedRows = new List<List<string>>();
            var maxColumns = 0;

            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row)) continue;
                
                // Split row into columns
                var columns = row.Split(new[] { _config.ColumnSeparator }, StringSplitOptions.None)
                                .Select(col => col.Trim())
                                .ToList();
                
                parsedRows.Add(columns);
                maxColumns = Math.Max(maxColumns, columns.Count);
            }

            var result = new ParsedClipboardData(parsedRows, parsedRows.Count, maxColumns);
            
            _logger?.Info("✅ PARSE: Parsed {RowCount} rows with max {ColumnCount} columns", 
                result.RowCount, result.ColumnCount);
            
            await Task.CompletedTask;
            return result;
            
        }, "ParseClipboardText", clipboardText?.Length ?? 0, new ParsedClipboardData(new List<List<string>>(), 0, 0), _logger);
    }

    /// <summary>
    /// Clear system clipboard
    /// PURE CLIPBOARD: Only clipboard system interaction
    /// </summary>
    public async Task<Result<bool>> ClearClipboardAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📋 CLEAR: Clearing clipboard");
            
            Clipboard.Clear();
            
            _logger?.Info("✅ CLEAR: Clipboard cleared successfully");
            
            await Task.CompletedTask;
            return true;
            
        }, "ClearClipboard", 1, false, _logger);
    }

    #region Private Helper Methods (Pure Data Formatting)

    private string FormatCellValue(object? value)
    {
        if (value == null) return "";
        
        var stringValue = value.ToString() ?? "";
        
        // Escape special characters for tab-separated values
        if (stringValue.Contains(_config.ColumnSeparator) || 
            stringValue.Contains(_config.RowSeparator) ||
            stringValue.Contains("\""))
        {
            stringValue = $"\"{stringValue.Replace("\"", "\"\"")}\"";
        }
        
        return stringValue;
    }

    private string FormatValidationError(string? validationError)
    {
        return string.IsNullOrEmpty(validationError) ? "" : $"ERROR: {validationError}";
    }

    private void LogConfiguration()
    {
        _logger?.Info("📋 CONFIG: Row: '{Row}', Column: '{Column}', Headers: {Headers}, Validation: {Validation}",
            _config.RowSeparator.Replace("\n", "\\n").Replace("\t", "\\t"),
            _config.ColumnSeparator.Replace("\n", "\\n").Replace("\t", "\\t"),
            _config.IncludeHeaders,
            _config.IncludeValidationData);
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 CLIPBOARD COORDINATOR DISPOSE: Cleaning up clipboard operations");
            _disposed = true;
            _logger?.Info("✅ CLIPBOARD COORDINATOR DISPOSE: Disposed successfully");
        }
    }
}

/// <summary>
/// Clipboard data record for different formats
/// </summary>
internal record ClipboardData(
    bool HasText,
    bool HasHtml,
    bool HasRtf,
    string? TextData,
    string? HtmlData,
    string? RtfData
);

/// <summary>
/// Parsed clipboard data record
/// </summary>
internal record ParsedClipboardData(
    IReadOnlyList<List<string>> Rows,
    int RowCount,
    int ColumnCount
);