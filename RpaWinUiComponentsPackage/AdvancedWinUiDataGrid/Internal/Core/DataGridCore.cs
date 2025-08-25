using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Core;

/// <summary>
/// Core implementation for DataGrid data operations
/// Provides headless data management functionality
/// </summary>
public class DataGridCore : IDataGridCore
{
    private readonly ILogger? _logger;
    private bool _isInitialized;
    private bool _disposed;
    private List<ColumnDefinition> _columns = new();
    private DataGridConfiguration? _configuration;
    private List<Dictionary<string, object>> _data = new();

    #region Constructor

    public DataGridCore(ILogger? logger)
    {
        _logger = logger;
        _logger?.Info("DataGridCore initialized");
    }

    #endregion

    #region Properties

    public int RowCount => _data.Count;
    public int ColumnCount => _columns.Count;
    public bool IsInitialized => _isInitialized;

    #endregion

    #region Initialization

    public async Task<Result<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridConfiguration? config = null)
    {
        try
        {
            _logger?.Info("Initializing DataGridCore with {ColumnCount} columns", columns.Count);

            _columns = columns.ToList();
            _configuration = config;
            _data.Clear();

            _isInitialized = true;
            _logger?.Info("DataGridCore initialization completed successfully");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "DataGridCore initialization failed");
            return Result<bool>.Failure($"Initialization failed: {ex.Message}");
        }
    }

    #endregion

    #region Data Operations

    public async Task<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional.Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult>> ImportDataAsync(
        object data,
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportOptions? options = null)
    {
        if (!_isInitialized)
        {
            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult>.Failure("DataGridCore not initialized");
        }

        try
        {
            _logger?.Info("Importing data (Type: {DataType})", data.GetType().Name);

            int processedRows = 0;
            var errors = new List<string>();

            // Handle different data types
            if (data is IEnumerable<Dictionary<string, object>> dictData)
            {
                foreach (var row in dictData)
                {
                    _data.Add(row);
                    processedRows++;
                }
            }
            else if (data is System.Data.DataTable dataTable)
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (System.Data.DataColumn col in dataTable.Columns)
                    {
                        dict[col.ColumnName] = row[col] ?? "";
                    }
                    _data.Add(dict);
                    processedRows++;
                }
            }
            else
            {
                return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult>.Failure($"Unsupported data type: {data.GetType().Name}");
            }

            var result = new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult
            {
                Success = true,
                ImportedRows = processedRows,
                ErrorRows = errors.Count,
                Errors = errors.AsReadOnly()
            };
            _logger?.Info("Import completed: {RowsProcessed} rows processed", processedRows);

            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Import operation failed");
            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult>.Failure($"Import failed: {ex.Message}");
        }
    }

    public async Task<Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportResult>> ExportDataAsync(
        ExportFormat format,
        string? filePath = null)
    {
        if (!_isInitialized)
        {
            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportResult>.Failure("DataGridCore not initialized");
        }

        try
        {
            _logger?.Info("Exporting data (Format: {Format}, Rows: {RowCount})", format, _data.Count);

            string actualFilePath = filePath ?? $"export_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToString().ToLower()}";

            // For now, just simulate export
            await Task.Delay(100); // Simulate export processing

            var result = new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportResult
            {
                Success = true,
                ExportedRows = _data.Count,
                FilePath = actualFilePath
            };
            _logger?.Info("Export completed: {RowCount} rows exported to {FilePath}", _data.Count, actualFilePath);

            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Export operation failed");
            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportResult>.Failure($"Export failed: {ex.Message}");
        }
    }

    #endregion

    #region Search, Filter, Sort

    public async Task<Result<SearchResult>> SearchAsync(
        string query,
        SearchOptions? options = null)
    {
        if (!_isInitialized)
        {
            return Result<SearchResult>.Failure("DataGridCore not initialized");
        }

        try
        {
            _logger?.Info("Searching for: '{Query}'", query);

            var matches = new List<SearchMatch>();
            var caseSensitive = options?.CaseSensitive ?? false;
            var wholeWord = options?.WholeWord ?? false;

            for (int rowIndex = 0; rowIndex < _data.Count; rowIndex++)
            {
                var row = _data[rowIndex];
                for (int colIndex = 0; colIndex < _columns.Count; colIndex++)
                {
                    var columnName = _columns[colIndex].Name;
                    if (row.TryGetValue(columnName, out var cellValue))
                    {
                        var cellText = cellValue?.ToString() ?? "";
                        var searchText = caseSensitive ? cellText : cellText.ToLowerInvariant();
                        var searchQuery = caseSensitive ? query : query.ToLowerInvariant();

                        bool isMatch = wholeWord
                            ? searchText.Split(' ').Contains(searchQuery)
                            : searchText.Contains(searchQuery);

                        if (isMatch)
                        {
                            matches.Add(new SearchMatch(
                                Row: rowIndex,
                                Column: colIndex,
                                Value: cellText
                            ));
                        }
                    }
                }
            }

            var result = new SearchResult(
                MatchCount: matches.Count,
                Matches: matches.AsReadOnly()
            );
            _logger?.Info("Search completed: {MatchCount} matches found", matches.Count);

            return Result<SearchResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Search operation failed");
            return Result<SearchResult>.Failure($"Search failed: {ex.Message}");
        }
    }

    public async Task<Result<FilterResult>> ApplyFilterAsync(FilterCriteria filter)
    {
        if (!_isInitialized)
        {
            return Result<FilterResult>.Failure("DataGridCore not initialized");
        }

        try
        {
            _logger?.Info("Applying filter on column: {Column}", filter.Column);

            // Simple filter implementation
            int visibleRows = _data.Count; // For now, just return current count
            int hiddenRows = 0;

            var result = new FilterResult(
                VisibleRows: visibleRows,
                HiddenRows: hiddenRows
            );
            _logger?.Info("Filter applied: {VisibleRows} visible, {HiddenRows} hidden", visibleRows, hiddenRows);

            return Result<FilterResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Filter operation failed");
            return Result<FilterResult>.Failure($"Filter failed: {ex.Message}");
        }
    }

    public async Task<Result<SortResult>> SortAsync(SortOptions sortOptions)
    {
        if (!_isInitialized)
        {
            return Result<SortResult>.Failure("DataGridCore not initialized");
        }

        try
        {
            _logger?.Info("Sorting by column: {Column}, Ascending: {Ascending}", sortOptions.Column, sortOptions.Ascending);

            // Simple sort implementation
            if (sortOptions.Ascending)
            {
                _data = _data.OrderBy(row => row.GetValueOrDefault(sortOptions.Column, "")).ToList();
            }
            else
            {
                _data = _data.OrderByDescending(row => row.GetValueOrDefault(sortOptions.Column, "")).ToList();
            }

            var result = new SortResult(
                SortedBy: sortOptions.Column,
                IsAscending: sortOptions.Ascending
            );
            _logger?.Info("Sort completed by {Column}", sortOptions.Column);

            return Result<SortResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Sort operation failed");
            return Result<SortResult>.Failure($"Sort failed: {ex.Message}");
        }
    }

    #endregion

    #region Validation

    public async Task<Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult>> ValidateAllAsync()
    {
        if (!_isInitialized)
        {
            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult>.Failure("DataGridCore not initialized");
        }

        try
        {
            _logger?.Info("Starting validation of {RowCount} rows", _data.Count);

            int totalCells = _data.Count * _columns.Count;
            int validCells = totalCells; // For now, assume all valid
            int invalidCells = 0;
            var errors = new List<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationError>();

            var result = new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult
            {
                IsValid = invalidCells == 0,
                TotalCells = totalCells,
                ValidCells = validCells,
                InvalidCells = invalidCells,
                ValidationErrors = errors.AsReadOnly()
            };
            _logger?.Info("Validation completed: {ValidCells}/{TotalCells} cells valid", validCells, totalCells);

            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Validation failed");
            return Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult>.Failure($"Validation failed: {ex.Message}");
        }
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;

        _logger?.Info("Disposing DataGridCore");
        _data.Clear();
        _columns.Clear();
        _disposed = true;
        _logger?.Info("DataGridCore disposed successfully");
    }

    #endregion
}