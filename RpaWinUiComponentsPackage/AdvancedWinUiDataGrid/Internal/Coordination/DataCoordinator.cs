using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Coordination;

/// <summary>
/// PROFESSIONAL Data Coordinator - ONLY data operations
/// RESPONSIBILITY: Handle data storage, retrieval, transformation (NO UI, NO validation, NO events)
/// SEPARATION: Pure data layer - immutable operations, functional patterns
/// ANTI-GOD: Single responsibility - only data coordination
/// </summary>
internal sealed class DataCoordinator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private readonly List<DataGridRow> _dataRows;
    private readonly List<GridColumnDefinition> _headers;
    private bool _disposed = false;

    public DataCoordinator(ILogger? logger, GlobalExceptionHandler exceptionHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        _dataRows = new List<DataGridRow>();
        _headers = new List<GridColumnDefinition>();
        
        _logger?.Info("📊 DATA COORDINATOR: Initialized - Pure data operations only");
    }

    public IReadOnlyList<DataGridRow> DataRows => _dataRows.AsReadOnly();
    public IReadOnlyList<GridColumnDefinition> Headers => _headers.AsReadOnly();

    /// <summary>
    /// Initialize data structure with columns
    /// PURE DATA: Only sets up data structure, no UI operations
    /// </summary>
    public async Task<Result<bool>> InitializeDataStructureAsync(IReadOnlyList<ColumnConfiguration> columns)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📊 DATA INIT: Initializing data structure for {ColumnCount} columns", columns.Count);

            _headers.Clear();
            
            foreach (var column in columns)
            {
                var header = new GridColumnDefinition
                {
                    Name = column.Name,
                    DisplayName = column.DisplayName,
                    Width = column.Width ?? 120,
                    Type = column.Type,
                    IsValidationColumn = column.IsValidationColumn ?? false,
                    IsDeleteColumn = column.IsDeleteColumn ?? false
                };
                
                _headers.Add(header);
                _logger?.Info("📋 DATA COLUMN: Added column {Name} ({Type}) - Width: {Width}", 
                    column.Name, column.Type.Name, column.Width);
            }

            await Task.CompletedTask;
            
            _logger?.Info("✅ DATA INIT: Data structure initialized with {HeaderCount} columns", _headers.Count);
            return true;
            
        }, "InitializeDataStructure", columns.Count, false, _logger);
    }

    /// <summary>
    /// Import data into data structure
    /// PURE DATA: Only processes and stores data, no validation, no UI
    /// </summary>
    public async Task<Result<ImportResult>> ImportDataAsync(IReadOnlyList<IReadOnlyDictionary<string, object?>> data)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📥 DATA IMPORT: Starting data import for {RowCount} rows", data.Count);
            
            var startTime = DateTime.UtcNow;
            var importedRows = 0;
            var errorRows = 0;

            foreach (var rowData in data)
            {
                try
                {
                    var dataRow = await CreateDataRowFromDictionary(rowData);
                    if (dataRow != null)
                    {
                        _dataRows.Add(dataRow);
                        importedRows++;
                        _logger?.Info("📝 DATA ROW: Imported row {RowIndex} with {CellCount} cells", 
                            importedRows, dataRow.Cells?.Count ?? 0);
                    }
                    else
                    {
                        errorRows++;
                        _logger?.Warning("⚠️ DATA ROW: Failed to create row {RowIndex}", importedRows + errorRows);
                    }
                }
                catch (Exception ex)
                {
                    errorRows++;
                    _logger?.Error(ex, "🚨 DATA IMPORT ERROR: Failed to import row {RowIndex}", importedRows + errorRows);
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger?.Info("✅ DATA IMPORT: Completed - Imported: {Imported}, Errors: {Errors}, Duration: {Duration}ms",
                importedRows, errorRows, (int)duration.TotalMilliseconds);

            return new ImportResult(importedRows, errorRows, duration);
            
        }, "ImportData", data.Count, new ImportResult(0, data.Count, TimeSpan.Zero), _logger);
    }

    /// <summary>
    /// Export data from data structure
    /// PURE DATA: Only retrieves and formats data, no business logic
    /// </summary>
    public async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportDataAsync(bool includeValidationAlerts = false)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("📤 DATA EXPORT: Starting data export - IncludeValidation: {IncludeValidation}, Rows: {RowCount}", 
                includeValidationAlerts, _dataRows.Count);

            var exportedData = new List<IReadOnlyDictionary<string, object?>>();

            foreach (var row in _dataRows)
            {
                if (row?.Cells == null) continue;

                var rowData = new Dictionary<string, object?>();
                
                foreach (var cell in row.Cells)
                {
                    if (cell == null) continue;
                    
                    // Add cell data
                    rowData[cell.ColumnName] = cell.Value;
                    
                    // Optionally add validation data
                    if (includeValidationAlerts && cell.HasValidationErrors)
                    {
                        rowData[$"{cell.ColumnName}_ValidationError"] = cell.ValidationError;
                    }
                }

                exportedData.Add(rowData);
            }

            await Task.CompletedTask;
            
            _logger?.Info("✅ DATA EXPORT: Exported {ExportedRows} rows with validation: {WithValidation}", 
                exportedData.Count, includeValidationAlerts);

            return exportedData.AsReadOnly();
            
        }, "ExportData", _dataRows.Count, new List<IReadOnlyDictionary<string, object?>>().AsReadOnly(), _logger);
    }

    /// <summary>
    /// Delete row from data structure
    /// PURE DATA: Only removes data, no UI updates, no validation
    /// </summary>
    public async Task<Result<bool>> DeleteRowAsync(int rowIndex)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            if (rowIndex < 0 || rowIndex >= _dataRows.Count)
            {
                _logger?.Warning("⚠️ DATA DELETE: Invalid row index {RowIndex} (valid range: 0-{MaxIndex})", 
                    rowIndex, _dataRows.Count - 1);
                return false;
            }

            var deletedRow = _dataRows[rowIndex];
            _dataRows.RemoveAt(rowIndex);
            
            _logger?.Info("🗑️ DATA DELETE: Deleted row {RowIndex} with {CellCount} cells", 
                rowIndex, deletedRow?.Cells?.Count ?? 0);

            await Task.CompletedTask;
            return true;
            
        }, "DeleteRow", 1, false, _logger);
    }

    /// <summary>
    /// Add empty rows to meet minimum requirements
    /// PURE DATA: Only adds data structure elements
    /// </summary>
    public async Task<Result<int>> EnsureMinimumRowsAsync(int minimumRows)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            var currentRows = _dataRows.Count;
            var rowsToAdd = Math.Max(0, minimumRows - currentRows);
            
            if (rowsToAdd == 0)
            {
                _logger?.Info("📊 MIN ROWS: Already have {CurrentRows} rows (minimum: {MinRows})", currentRows, minimumRows);
                return 0;
            }

            _logger?.Info("📊 MIN ROWS: Adding {RowsToAdd} empty rows (current: {Current}, minimum: {Min})", 
                rowsToAdd, currentRows, minimumRows);

            for (int i = 0; i < rowsToAdd; i++)
            {
                var emptyRow = await CreateEmptyDataRow(currentRows + i);
                if (emptyRow != null)
                {
                    _dataRows.Add(emptyRow);
                }
            }

            await Task.CompletedTask;
            
            _logger?.Info("✅ MIN ROWS: Added {AddedRows} empty rows - Total rows: {TotalRows}", 
                rowsToAdd, _dataRows.Count);

            return rowsToAdd;
            
        }, "EnsureMinimumRows", minimumRows, 0, _logger);
    }

    /// <summary>
    /// Find cell by ID in data structure
    /// PURE DATA: Only data lookup operations
    /// </summary>
    public DataGridCell? FindCellById(string cellId)
    {
        try
        {
            foreach (var row in _dataRows)
            {
                if (row?.Cells == null) continue;
                
                var cell = row.Cells.FirstOrDefault(c => c?.CellId == cellId);
                if (cell != null)
                {
                    _logger?.Info("🔍 DATA FIND: Found cell {CellId} in row {RowIndex}", cellId, row.Index);
                    return cell;
                }
            }
            
            _logger?.Warning("⚠️ DATA FIND: Cell {CellId} not found", cellId);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 DATA FIND ERROR: Exception finding cell {CellId}", cellId);
            return null;
        }
    }

    /// <summary>
    /// Get data statistics
    /// PURE DATA: Only data analysis operations
    /// </summary>
    public async Task<Result<DataStatistics>> GetDataStatisticsAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            var totalCells = _dataRows.Sum(r => r?.Cells?.Count ?? 0);
            var nonEmptyCells = _dataRows.SelectMany(r => r?.Cells ?? new List<DataGridCell>())
                                        .Count(c => c != null && !string.IsNullOrEmpty(c.Value?.ToString()));
            var validationErrors = _dataRows.SelectMany(r => r?.Cells ?? new List<DataGridCell>())
                                          .Count(c => c?.HasValidationErrors == true);

            await Task.CompletedTask;

            var stats = new DataStatistics(
                TotalRows: _dataRows.Count,
                TotalColumns: _headers.Count,
                TotalCells: totalCells,
                NonEmptyCells: nonEmptyCells,
                ValidationErrors: validationErrors);
            
            _logger?.Info("📊 DATA STATS: Rows: {Rows}, Columns: {Cols}, Cells: {Cells}, NonEmpty: {NonEmpty}, Errors: {Errors}",
                stats.TotalRows, stats.TotalColumns, stats.TotalCells, stats.NonEmptyCells, stats.ValidationErrors);

            return stats;
            
        }, "GetDataStatistics", _dataRows.Count, new DataStatistics(0, 0, 0, 0, 0), _logger);
    }

    private async Task<DataGridRow?> CreateDataRowFromDictionary(IReadOnlyDictionary<string, object?> rowData)
    {
        try
        {
            var cells = new List<DataGridCell>();
            var rowIndex = _dataRows.Count;

            for (int colIndex = 0; colIndex < _headers.Count; colIndex++)
            {
                var header = _headers[colIndex];
                var cellValue = rowData.TryGetValue(header.Name, out var value) ? value : null;
                
                var cell = new DataGridCell
                {
                    RowIndex = rowIndex,
                    ColumnIndex = colIndex,
                    ColumnName = header.Name,
                    Value = cellValue,
                    ValidationState = true,
                    HasValidationErrors = false
                };
                
                cells.Add(cell);
            }

            var dataRow = new DataGridRow
            {
                Index = rowIndex,
                Cells = cells
            };

            await Task.CompletedTask;
            return dataRow;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 CREATE ROW ERROR: Failed to create data row");
            return null;
        }
    }

    private async Task<DataGridRow?> CreateEmptyDataRow(int rowIndex)
    {
        try
        {
            var cells = new List<DataGridCell>();

            for (int colIndex = 0; colIndex < _headers.Count; colIndex++)
            {
                var header = _headers[colIndex];
                
                var cell = new DataGridCell
                {
                    RowIndex = rowIndex,
                    ColumnIndex = colIndex,
                    ColumnName = header.Name,
                    Value = null,
                    ValidationState = true,
                    HasValidationErrors = false
                };
                
                cells.Add(cell);
            }

            var dataRow = new DataGridRow
            {
                Index = rowIndex,
                Cells = cells
            };

            await Task.CompletedTask;
            return dataRow;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 CREATE EMPTY ROW ERROR: Failed to create empty row {RowIndex}", rowIndex);
            return null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 DATA COORDINATOR DISPOSE: Cleaning up data structure");
            
            _dataRows.Clear();
            _headers.Clear();
            
            _disposed = true;
            _logger?.Info("✅ DATA COORDINATOR DISPOSE: Disposed successfully");
        }
    }
}

/// <summary>
/// Data statistics record for analytics
/// </summary>
internal record DataStatistics(
    int TotalRows,
    int TotalColumns, 
    int TotalCells,
    int NonEmptyCells,
    int ValidationErrors);

/// <summary>
/// Import operation result
/// </summary>
internal record ImportResult(int ImportedRows, int ErrorRows, TimeSpan Duration);