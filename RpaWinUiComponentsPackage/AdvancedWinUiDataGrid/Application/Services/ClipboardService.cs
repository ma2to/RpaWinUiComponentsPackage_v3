using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using Windows.ApplicationModel.DataTransfer;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;

/// <summary>
/// CLIPBOARD: Advanced clipboard operations for DataGrid
/// EXCEL_COMPATIBLE: Full Excel format support with TSV/CSV parsing
/// ENTERPRISE: Production-ready copy/paste functionality
/// </summary>
public class ClipboardService : IClipboardService
{
    #region Private Fields

    private readonly ILogger _logger;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public ClipboardService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("ClipboardService initialized");
    }

    #endregion

    #region Public Methods - Copy Operations

    /// <summary>
    /// Copy selected data to clipboard in Excel-compatible format
    /// EXCEL_FORMAT: TSV (Tab Separated Values) for Excel compatibility
    /// </summary>
    public async Task<Result<bool>> CopyToClipboardAsync(
        IReadOnlyList<Dictionary<string, object?>> selectedRows,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeHeaders = true)
    {
        if (_disposed) return Result<bool>.Failure("Service disposed");

        try
        {
            var tsvData = ConvertToTsv(selectedRows, columns, includeHeaders);
            var csvData = ConvertToCsv(selectedRows, columns, includeHeaders);

            var dataPackage = new DataPackage();
            
            // Add multiple formats for better compatibility
            dataPackage.SetText(tsvData); // Primary format - Excel compatible
            dataPackage.Properties.Add("Csv", csvData); // Secondary CSV format
            dataPackage.Properties.ApplicationName = "AdvancedDataGrid";
            
            Clipboard.SetContent(dataPackage);
            
            _logger.LogInformation("Copied {RowCount} rows with {ColumnCount} columns to clipboard", 
                selectedRows.Count, columns.Count);
                
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy data to clipboard");
            return Result<bool>.Failure("Failed to copy data to clipboard", ex);
        }
    }

    /// <summary>
    /// Copy single cell value to clipboard
    /// </summary>
    public async Task<Result<bool>> CopyCellAsync(object? cellValue)
    {
        if (_disposed) return Result<bool>.Failure("Service disposed");

        try
        {
            var text = cellValue?.ToString() ?? string.Empty;
            
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            
            _logger.LogTrace("Copied single cell value to clipboard: {Value}", text);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy cell value to clipboard");
            return Result<bool>.Failure("Failed to copy cell value to clipboard", ex);
        }
    }

    #endregion

    #region Public Methods - Paste Operations

    /// <summary>
    /// Paste data from clipboard into grid format
    /// EXCEL_COMPATIBLE: Handles Excel TSV/CSV format automatically
    /// SMART_PARSING: Auto-detects delimiter (Tab, Comma, Semicolon)
    /// </summary>
    public async Task<Result<ClipboardParseResult>> PasteFromClipboardAsync(
        IReadOnlyList<ColumnDefinition> targetColumns,
        int startRowIndex = 0,
        int startColumnIndex = 0)
    {
        if (_disposed) return Result<ClipboardParseResult>.Failure("Service disposed");

        try
        {
            var dataPackageView = Clipboard.GetContent();
            
            if (!dataPackageView.Contains(StandardDataFormats.Text))
            {
                return Result<ClipboardParseResult>.Failure("No text data found in clipboard");
            }

            var clipboardText = await dataPackageView.GetTextAsync();
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                return Result<ClipboardParseResult>.Failure("Clipboard text is empty");
            }

            var parseResult = ParseClipboardData(clipboardText, targetColumns, startRowIndex, startColumnIndex);
            
            _logger.LogInformation("Parsed clipboard data: {RowCount} rows, {ColumnCount} columns", 
                parseResult.ParsedRows.Count, parseResult.ColumnCount);
                
            return Result<ClipboardParseResult>.Success(parseResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to paste data from clipboard");
            return Result<ClipboardParseResult>.Failure("Failed to paste data from clipboard", ex);
        }
    }

    #endregion

    #region Private Methods - Format Conversion

    /// <summary>
    /// Convert data to TSV format (Excel compatible)
    /// </summary>
    private string ConvertToTsv(
        IReadOnlyList<Dictionary<string, object?>> rows,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeHeaders)
    {
        var sb = new StringBuilder();

        // Add headers if requested
        if (includeHeaders)
        {
            var headers = columns.Select(c => EscapeTsvValue(c.DisplayName ?? c.Name));
            sb.AppendLine(string.Join("\t", headers));
        }

        // Add data rows
        foreach (var row in rows)
        {
            var values = columns.Select(column =>
            {
                var value = row.TryGetValue(column.Name, out var val) ? val : null;
                return EscapeTsvValue(FormatValueForExport(value, column));
            });
            
            sb.AppendLine(string.Join("\t", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert data to CSV format
    /// </summary>
    private string ConvertToCsv(
        IReadOnlyList<Dictionary<string, object?>> rows,
        IReadOnlyList<ColumnDefinition> columns,
        bool includeHeaders)
    {
        var sb = new StringBuilder();

        // Add headers if requested
        if (includeHeaders)
        {
            var headers = columns.Select(c => EscapeCsvValue(c.DisplayName ?? c.Name));
            sb.AppendLine(string.Join(",", headers));
        }

        // Add data rows
        foreach (var row in rows)
        {
            var values = columns.Select(column =>
            {
                var value = row.TryGetValue(column.Name, out var val) ? val : null;
                return EscapeCsvValue(FormatValueForExport(value, column));
            });
            
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parse clipboard data with smart delimiter detection
    /// EXCEL_COMPATIBLE: Handles Excel paste format
    /// </summary>
    private ClipboardParseResult ParseClipboardData(
        string clipboardText,
        IReadOnlyList<ColumnDefinition> targetColumns,
        int startRowIndex,
        int startColumnIndex)
    {
        var lines = clipboardText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            .Where(line => !string.IsNullOrEmpty(line))
            .ToList();

        if (lines.Count == 0)
        {
            return new ClipboardParseResult
            {
                ParsedRows = new List<Dictionary<string, object?>>(),
                ColumnCount = 0,
                RowCount = 0,
                HasHeaders = false
            };
        }

        // Detect delimiter (Excel uses tabs, CSV uses commas)
        char delimiter = DetectDelimiter(lines[0]);
        _logger.LogDebug("Detected delimiter: '{Delimiter}'", delimiter);

        var parsedRows = new List<Dictionary<string, object?>>();
        bool hasHeaders = ShouldTreatFirstRowAsHeaders(lines, delimiter, targetColumns);
        int dataStartIndex = hasHeaders ? 1 : 0;

        // Get column mapping
        var columnMapping = GetColumnMapping(lines, delimiter, hasHeaders, targetColumns, startColumnIndex);

        // Parse data rows
        for (int i = dataStartIndex; i < lines.Count; i++)
        {
            var cells = ParseLine(lines[i], delimiter);
            var rowData = new Dictionary<string, object?>();

            for (int cellIndex = 0; cellIndex < cells.Length && cellIndex < columnMapping.Count; cellIndex++)
            {
                var targetColumn = columnMapping[cellIndex];
                if (targetColumn != null)
                {
                    var parsedValue = ParseCellValue(cells[cellIndex], targetColumn);
                    rowData[targetColumn.Name] = parsedValue;
                }
            }

            parsedRows.Add(rowData);
        }

        return new ClipboardParseResult
        {
            ParsedRows = parsedRows,
            ColumnCount = columnMapping.Count,
            RowCount = parsedRows.Count,
            HasHeaders = hasHeaders,
            DetectedDelimiter = delimiter
        };
    }

    /// <summary>
    /// Smart delimiter detection for clipboard data
    /// </summary>
    private char DetectDelimiter(string firstLine)
    {
        // Count different delimiters
        int tabs = firstLine.Count(c => c == '\t');
        int commas = firstLine.Count(c => c == ',');
        int semicolons = firstLine.Count(c => c == ';');

        // Excel typically uses tabs, so prioritize tabs
        if (tabs > 0) return '\t';
        if (semicolons > commas) return ';';
        return ','; // Default to comma
    }

    /// <summary>
    /// Parse single line with given delimiter
    /// </summary>
    private string[] ParseLine(string line, char delimiter)
    {
        var cells = new List<string>();
        var currentCell = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentCell.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                cells.Add(currentCell.ToString());
                currentCell.Clear();
            }
            else
            {
                currentCell.Append(c);
            }
        }

        cells.Add(currentCell.ToString());
        return cells.ToArray();
    }

    /// <summary>
    /// Get column mapping based on headers or position
    /// </summary>
    private List<ColumnDefinition?> GetColumnMapping(
        List<string> lines,
        char delimiter,
        bool hasHeaders,
        IReadOnlyList<ColumnDefinition> targetColumns,
        int startColumnIndex)
    {
        var mapping = new List<ColumnDefinition?>();
        
        if (hasHeaders && lines.Count > 0)
        {
            var headers = ParseLine(lines[0], delimiter);
            
            foreach (var header in headers)
            {
                var matchingColumn = targetColumns.FirstOrDefault(c => 
                    string.Equals(c.Name, header.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.DisplayName, header.Trim(), StringComparison.OrdinalIgnoreCase));
                    
                mapping.Add(matchingColumn);
            }
        }
        else
        {
            // Map by position
            var firstLineColumns = ParseLine(lines[0], delimiter).Length;
            
            for (int i = 0; i < firstLineColumns; i++)
            {
                int targetIndex = startColumnIndex + i;
                var targetColumn = targetIndex < targetColumns.Count ? targetColumns[targetIndex] : null;
                mapping.Add(targetColumn);
            }
        }

        return mapping;
    }

    /// <summary>
    /// Check if first row should be treated as headers
    /// </summary>
    private bool ShouldTreatFirstRowAsHeaders(List<string> lines, char delimiter, IReadOnlyList<ColumnDefinition> targetColumns)
    {
        if (lines.Count < 2) return false;

        var firstRow = ParseLine(lines[0], delimiter);
        var secondRow = ParseLine(lines[1], delimiter);

        // Check if first row contains text that matches column names
        foreach (var cell in firstRow)
        {
            if (targetColumns.Any(c => 
                string.Equals(c.Name, cell.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.DisplayName, cell.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Parse individual cell value according to column type
    /// </summary>
    private object? ParseCellValue(string cellText, ColumnDefinition column)
    {
        if (string.IsNullOrWhiteSpace(cellText))
            return null;

        try
        {
            var trimmedText = cellText.Trim();

            // Handle different data types
            if (column.DataType == typeof(string))
                return trimmedText;

            if (column.DataType == typeof(bool))
            {
                return trimmedText.ToLowerInvariant() switch
                {
                    "true" or "1" or "yes" or "y" => true,
                    "false" or "0" or "no" or "n" => false,
                    _ => bool.TryParse(trimmedText, out var boolValue) ? boolValue : null
                };
            }

            if (column.DataType == typeof(int))
                return int.TryParse(trimmedText, out var intValue) ? intValue : null;

            if (column.DataType == typeof(decimal))
                return decimal.TryParse(trimmedText, out var decimalValue) ? decimalValue : null;

            if (column.DataType == typeof(double))
                return double.TryParse(trimmedText, out var doubleValue) ? doubleValue : null;

            if (column.DataType == typeof(DateTime))
            {
                if (DateTime.TryParse(trimmedText, out var dateValue))
                    return dateValue;
                    
                // Try Excel date formats
                if (double.TryParse(trimmedText, out var excelDate))
                {
                    try
                    {
                        return DateTime.FromOADate(excelDate);
                    }
                    catch
                    {
                        // Fall through to default conversion
                    }
                }
            }

            // Default conversion attempt
            return Convert.ChangeType(trimmedText, column.DataType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse cell value '{Value}' for column '{Column}' of type '{Type}'", 
                cellText, column.Name, column.DataType.Name);
            return cellText; // Return original text if parsing fails
        }
    }

    /// <summary>
    /// Format value for export (considering column format)
    /// </summary>
    private string FormatValueForExport(object? value, ColumnDefinition column)
    {
        if (value == null) return string.Empty;

        // Apply column-specific formatting
        if (!string.IsNullOrEmpty(column.DisplayFormat))
        {
            try
            {
                if (value is IFormattable formattable)
                    return formattable.ToString(column.DisplayFormat, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply display format '{Format}' to value '{Value}'", 
                    column.DisplayFormat, value);
            }
        }

        // Default formatting
        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            decimal dec => dec.ToString("F2"),
            double dbl => dbl.ToString("F2"),
            bool b => b ? "TRUE" : "FALSE",
            _ => value.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Escape value for TSV format
    /// </summary>
    private string EscapeTsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        
        // Replace tabs with spaces and handle newlines
        return value.Replace('\t', ' ')
                   .Replace('\r', ' ')
                   .Replace('\n', ' ');
    }

    /// <summary>
    /// Escape value for CSV format
    /// </summary>
    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Quote if contains comma, quote, or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _logger.LogDebug("ClipboardService disposed");
        }
    }

    #endregion
}

/// <summary>
/// Result of clipboard parsing operation
/// </summary>
public record ClipboardParseResult
{
    /// <summary>Parsed row data</summary>
    public required List<Dictionary<string, object?>> ParsedRows { get; init; }
    
    /// <summary>Number of columns detected</summary>
    public int ColumnCount { get; init; }
    
    /// <summary>Number of rows parsed</summary>
    public int RowCount { get; init; }
    
    /// <summary>Whether first row was treated as headers</summary>
    public bool HasHeaders { get; init; }
    
    /// <summary>Detected delimiter character</summary>
    public char DetectedDelimiter { get; init; }
}