using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Managers;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using InternalValidationResult = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult;
using ValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationRule;
using ValidationSeverity = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationSeverity;
using PerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.PerformanceConfiguration;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.GridColumnDefinition;
using InternalImportResult = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportResult;
using ImportOptions = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportOptions;
using ExportOptions = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportOptions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Core;

/// <summary>
/// Professional DataGrid Coordinator - Hybrid Functional-OOP Architecture
/// 
/// FUNCTIONAL-OOP PATTERNS:
/// - Immutable configuration records
/// - Result types for error handling
/// - Reactive streams for data changes
/// - Pure functions for data transformations
/// - Monadic operations with Result<T>
/// 
/// OOP PATTERNS:
/// - Manager composition for UI concerns
/// - Event-driven architecture for UI interactions
/// - Dependency injection for managers
/// </summary>
internal sealed class DataGridCoordinator : IDisposable
{
    #region Private Fields - Functional Core

    // IMMUTABLE STATE (Functional)
    private readonly record struct CoordinatorConfig(
        ILogger? Logger,
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration Performance,
        ColorConfiguration Colors,
        ValidationConfiguration Validation,
        int MinimumRows
    );

    private readonly CoordinatorConfig _config;
    private readonly Func<string, Task<Result<bool>>> _logOperation;

    // REACTIVE STREAMS (Functional-OOP)
    private readonly Subject<DataChangeEvent> _dataChanges = new();
    private readonly Subject<ValidationChangeEvent> _validationChanges = new();
    private readonly Subject<UIUpdateEvent> _uiUpdates = new();

    #endregion

    #region Private Fields - OOP Managers

    // MANAGER COMPOSITION (OOP)
    private readonly UserControl _parentGrid;
    private readonly DataGridSelectionManager _selectionManager;
    private readonly DataGridEditingManager _editingManager;
    private readonly DataGridResizeManager _resizeManager;
    private readonly DataGridEventManager _eventManager;

    // DATA STATE (OOP)
    private readonly ObservableCollection<GridColumnDefinition> _headers = new();
    private readonly ObservableCollection<DataGridRow> _dataRows = new();
    private readonly Dictionary<string, ValidationRule[]> _validationRules = new();

    #endregion

    #region Constructor - Functional Style

    private DataGridCoordinator(
        UserControl parentGrid,
        CoordinatorConfig config,
        DataGridSelectionManager selectionManager,
        DataGridEditingManager editingManager,
        DataGridResizeManager resizeManager,
        DataGridEventManager eventManager)
    {
        _parentGrid = parentGrid;
        _config = config;
        _selectionManager = selectionManager;
        _editingManager = editingManager;
        _resizeManager = resizeManager;
        _eventManager = eventManager;

        // FUNCTIONAL: Pure function for logging
        _logOperation = message => LogAsync(message, LogLevel.Information);

        WireManagerEvents();
        _config.Logger?.Info("🔧 DataGridCoordinator created with hybrid architecture");
    }

    #endregion

    #region Factory Methods - Functional Pattern

    /// <summary>
    /// Create DataGrid coordinator with functional factory pattern
    /// FUNCTIONAL: Static factory with immutable configuration
    /// </summary>
    public static Result<DataGridCoordinator> Create(
        UserControl parentGrid,
        ILogger? logger = null,
        Option<PerformanceConfiguration> performance = default,
        Option<ColorConfiguration> colors = default,
        Option<ValidationConfiguration> validation = default,
        int minimumRows = 10)
    {
        try
        {
            // FUNCTIONAL: Build immutable configuration
            var advancedPerformanceConfig = performance.ValueOr(() => CreateDefaultAdvancedPerformanceConfig());
            
            var config = new CoordinatorConfig(
                Logger: logger,
                Performance: advancedPerformanceConfig,
                Colors: colors.ValueOr(() => CreateDefaultColorConfig()),
                Validation: validation.ValueOr(() => CreateDefaultValidationConfig()),
                MinimumRows: minimumRows
            );

            return CreateManagers(parentGrid, config)
                .Bind(managers => CreateCoordinator(parentGrid, config, managers))
                .Map(coordinator => coordinator);
        }
        catch (Exception ex)
        {
            return Result<DataGridCoordinator>.Failure("Failed to create DataGridCoordinator", ex);
        }
    }

    /// <summary>
    /// FUNCTIONAL: Pure function to create default Advanced PerformanceConfiguration
    /// </summary>
    private static RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration CreateDefaultAdvancedPerformanceConfig() => new()
    {
        VirtualizationThreshold = 1000,
        EnableThrottling = true,
        ThrottleDelay = TimeSpan.FromMilliseconds(100)
    };

    /// <summary>
    /// FUNCTIONAL: Pure function to create default Core configurations  
    /// </summary>
    private static RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.PerformanceConfiguration CreateDefaultPerformanceConfig() => new()
    {
        EnableVirtualization = true,
        VirtualizationThreshold = 1000,
        EnableBackgroundProcessing = true,
        EnableCaching = true,
        CacheSize = 10000,
        OperationTimeout = TimeSpan.FromMinutes(5),
        EnableThrottling = true,
        ThrottleDelay = TimeSpan.FromMilliseconds(100),
        MaxConcurrentOperations = Environment.ProcessorCount
    };

    private static ColorConfiguration CreateDefaultColorConfig() => new()
    {
        CellBorder = "#000000",
        ValidationErrorBorder = "#FF0000",
        UseDarkTheme = false
    };

    private static ValidationConfiguration CreateDefaultValidationConfig() => new()
    {
        EnableRealtimeValidation = true,
        EnableBatchValidation = true,
        ShowValidationAlerts = true
    };

    #endregion

    #region Manager Factory - Functional Composition

    /// <summary>
    /// FUNCTIONAL: Create managers with functional composition
    /// </summary>
    private static Result<(DataGridSelectionManager, DataGridEditingManager, DataGridResizeManager, DataGridEventManager)> CreateManagers(
        UserControl parentGrid, 
        CoordinatorConfig config)
    {
        try
        {
            var headers = new ObservableCollection<GridColumnDefinition>();
            var dataRows = new ObservableCollection<DataGridRow>();

            var selectionManager = new DataGridSelectionManager(parentGrid, dataRows, headers, config.Logger);
            var editingManager = new DataGridEditingManager(parentGrid, dataRows, headers, config.Logger);
            var resizeManager = new DataGridResizeManager(parentGrid, headers, config.Logger);
            var eventManager = new DataGridEventManager(parentGrid, selectionManager, editingManager, resizeManager, config.Logger);

            return Result<(DataGridSelectionManager, DataGridEditingManager, DataGridResizeManager, DataGridEventManager)>
                .Success((selectionManager, editingManager, resizeManager, eventManager));
        }
        catch (Exception ex)
        {
            return Result<(DataGridSelectionManager, DataGridEditingManager, DataGridResizeManager, DataGridEventManager)>
                .Failure("Failed to create managers", ex);
        }
    }

    /// <summary>
    /// FUNCTIONAL: Create coordinator with functional composition
    /// </summary>
    private static Result<DataGridCoordinator> CreateCoordinator(
        UserControl parentGrid,
        CoordinatorConfig config,
        (DataGridSelectionManager selection, DataGridEditingManager editing, DataGridResizeManager resize, DataGridEventManager events) managers)
    {
        try
        {
            var coordinator = new DataGridCoordinator(
                parentGrid, config, 
                managers.selection, managers.editing, managers.resize, managers.events);

            return Result<DataGridCoordinator>.Success(coordinator);
        }
        catch (Exception ex)
        {
            return Result<DataGridCoordinator>.Failure("Failed to create coordinator", ex);
        }
    }

    #endregion

    #region Public API - Functional Operations

    /// <summary>
    /// FUNCTIONAL: Initialize with immutable column configuration
    /// MONADIC: Returns Result<T> for composable error handling
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(IReadOnlyList<ColumnConfiguration> columns)
    {
        var logResult = await _logOperation("Starting initialization");
        if (!logResult.IsSuccess) return Result<bool>.Failure(logResult.ErrorMessage);

        var validationResult = ValidateColumns(columns);
        if (!validationResult.IsSuccess) return Result<bool>.Failure(validationResult.ErrorMessage);

        var columnDefinitionsResult = CreateColumnDefinitions(validationResult.Value);
        if (!columnDefinitionsResult.IsSuccess) return Result<bool>.Failure(columnDefinitionsResult.ErrorMessage);

        var applyResult = ApplyColumnDefinitions(columnDefinitionsResult.Value);
        if (!applyResult.IsSuccess) return Result<bool>.Failure(applyResult.ErrorMessage);

        return await InitializeManagers();
    }

    /// <summary>
    /// FUNCTIONAL: Import data with immutable operations
    /// MONADIC: Composable async operations with Result<T>
    /// </summary>
    public async Task<Result<InternalImportResult>> ImportDataAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        Option<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportOptions> options = default)
    {
        var importOptions = options.ValueOr(() => new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportOptions());
        
        return await _logOperation($"Importing {data.Count} rows")
            .Bind(_ => ValidateImportData(data))
            .Bind(validData => TransformDataForImport(validData, importOptions))
            .Bind(transformedData => ApplyDataToGrid(transformedData))
            .Bind(result => ValidateAfterImport(result))
            .Bind(result => UpdateUIAfterImport(result));
    }

    /// <summary>
    /// FUNCTIONAL: Export data as immutable collection
    /// PURE: No side effects, returns immutable data
    /// </summary>
    public async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ExportDataAsync(
        Option<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportOptions> options = default)
    {
        var exportOptions = options.ValueOr(() => new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportOptions());

        return await Task.FromResult(
            ExtractDataFromGrid()
                .Bind(rawData => FilterDataForExport(rawData, exportOptions))
                .Bind(filteredData => TransformDataForExport(filteredData, exportOptions))
                .Map(transformedData => (IReadOnlyList<IReadOnlyDictionary<string, object?>>)transformedData.ToList().AsReadOnly())
        );
    }

    /// <summary>
    /// FUNCTIONAL: Smart delete with functional validation
    /// MONADIC: Composable validation and execution
    /// </summary>
    public async Task<Result<bool>> SmartDeleteRowAsync(int rowIndex)
    {
        return await ValidateRowForDeletion(rowIndex)
            .Bind(validIndex => CheckMinimumRowConstraint())
            .Bind(_ => DetermineDeleteStrategy(rowIndex))
            .Bind(strategy => ExecuteDeleteStrategy(strategy, rowIndex))
            .Bind(_ => UpdateUIAfterDelete());
    }

    /// <summary>
    /// FUNCTIONAL: Batch validation with reactive streams
    /// REACTIVE: Progress reporting through observables
    /// </summary>
    public async Task<Result<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationResult>> ValidateAllAsync(
        Option<IProgress<ValidationProgress>> progress = default)
    {
        var progressReporter = progress.ValueOr(() => new Progress<ValidationProgress>());

        return await ExtractDataFromGrid()
            .Bind(data => ValidateDataBatch(data, progressReporter))
            .Bind(validationResults => AggregateValidationResults(validationResults))
            .Bind(result => UpdateValidationUI(result));
    }

    #endregion

    #region Reactive Streams - Functional-OOP Bridge

    /// <summary>
    /// REACTIVE: Observable stream for data changes
    /// FUNCTIONAL: Immutable event objects
    /// </summary>
    public IObservable<DataChangeEvent> DataChanges => _dataChanges.AsObservable();

    /// <summary>
    /// REACTIVE: Observable stream for validation changes  
    /// FUNCTIONAL: Immutable validation events
    /// </summary>
    public IObservable<ValidationChangeEvent> ValidationChanges => _validationChanges.AsObservable();

    /// <summary>
    /// REACTIVE: Observable stream for UI updates
    /// OOP: UI-specific events for manager coordination
    /// </summary>
    public IObservable<UIUpdateEvent> UIUpdates => _uiUpdates.AsObservable();

    #endregion

    #region Private Methods - Functional Data Operations

    /// <summary>
    /// FUNCTIONAL: Pure validation function
    /// </summary>
    private Result<IReadOnlyList<ColumnConfiguration>> ValidateColumns(IReadOnlyList<ColumnConfiguration> columns)
    {
        if (!columns.Any())
        {
            return Result<IReadOnlyList<ColumnConfiguration>>.Failure("No columns provided");
        }

        var duplicateNames = columns.GroupBy(c => c.Name).Where(g => g.Count() > 1).Select(g => g.Key);
        if (duplicateNames.Any())
        {
            return Result<IReadOnlyList<ColumnConfiguration>>.Failure($"Duplicate column names: {string.Join(", ", duplicateNames)}");
        }

        return Result<IReadOnlyList<ColumnConfiguration>>.Success(columns);
    }

    /// <summary>
    /// FUNCTIONAL: Pure transformation function
    /// </summary>
    private Result<IReadOnlyList<GridColumnDefinition>> CreateColumnDefinitions(IReadOnlyList<ColumnConfiguration> columns)
    {
        try
        {
            var definitions = columns.Select(col => new GridColumnDefinition
            {
                Name = col.Name,
                DisplayName = col.DisplayName,
                Type = col.Type,
                Width = col.Width ?? 100,
                IsReadOnly = col.IsReadOnly ?? false,
                IsVisible = col.IsVisible ?? true,
                CanSort = col.CanSort ?? true,
                CanFilter = col.CanFilter ?? true,
                IsValidationColumn = col.IsValidationColumn ?? false,
                IsDeleteColumn = col.IsDeleteColumn ?? false,
                IsCheckBoxColumn = col.IsCheckboxColumn ?? false
            }).ToList().AsReadOnly();

            return Result<IReadOnlyList<GridColumnDefinition>>.Success(definitions);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<GridColumnDefinition>>.Failure("Failed to create column definitions", ex);
        }
    }

    /// <summary>
    /// FUNCTIONAL: Pure data extraction
    /// </summary>
    private Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExtractDataFromGrid()
    {
        try
        {
            var data = _dataRows
                .Where(row => !IsRowEmpty(row))
                .Select(row => CreateRowDictionary(row))
                .ToList()
                .AsReadOnly();

            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(data);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure("Failed to extract data", ex);
        }
    }

    /// <summary>
    /// FUNCTIONAL: Pure transformation with options
    /// </summary>
    private Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>> FilterDataForExport(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportOptions options)
    {
        try
        {
            var filtered = data.AsEnumerable();

            if (!options.IncludeEmptyRows)
            {
                filtered = filtered.Where(row => !IsRowEmpty(row));
            }

            if (options.ColumnNames?.Any() == true)
            {
                filtered = filtered.Select(row => 
                    options.ColumnNames.ToDictionary(
                        colName => colName,
                        colName => row.ContainsKey(colName) ? row[colName] : null
                    ) as IReadOnlyDictionary<string, object?>
                );
            }

            var result = filtered.ToList().AsReadOnly();
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Failure("Failed to filter export data", ex);
        }
    }

    #endregion

    #region Private Methods - OOP Manager Operations

    /// <summary>
    /// OOP: Manager coordination and UI updates
    /// </summary>
    private void WireManagerEvents()
    {
        // Selection Manager Events
        _selectionManager.SelectionChanged += (sender, e) => 
        {
            _uiUpdates.OnNext(new UIUpdateEvent("SelectionChanged", e.SelectedCells));
        };

        _selectionManager.FocusChanged += (sender, e) =>
        {
            _uiUpdates.OnNext(new UIUpdateEvent("FocusChanged", e.FocusedCell));
        };

        // Editing Manager Events
        _editingManager.ValueChanged += (sender, e) =>
        {
            var changeEvent = new DataChangeEvent(e.Cell, e.OldValue, e.NewValue);
            _dataChanges.OnNext(changeEvent);
        };

        _editingManager.ValidationChanged += (sender, e) =>
        {
            // Convert to public ValidationResult for the event
            var publicResult = new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationResult(
                TotalCells: 1,
                ValidCells: e.ValidationResult.IsValid ? 1 : 0,
                InvalidCells: e.ValidationResult.IsValid ? 0 : 1,
                Errors: e.ValidationResult.IsValid ? Array.Empty<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationError>() : 
                    new[] { new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationError(0, 0, e.ValidationResult.ErrorMessage ?? "Validation error") }
            );
            var validationEvent = new ValidationChangeEvent(e.Cell, publicResult);
            _validationChanges.OnNext(validationEvent);
        };

        // Resize Manager Events  
        _resizeManager.ResizeEnded += (sender, e) =>
        {
            _uiUpdates.OnNext(new UIUpdateEvent("ColumnResized", e.Column));
        };
    }

    /// <summary>
    /// OOP: Apply column definitions to UI managers
    /// </summary>
    private Result<bool> ApplyColumnDefinitions(IReadOnlyList<GridColumnDefinition> definitions)
    {
        try
        {
            _headers.Clear();
            foreach (var definition in definitions)
            {
                _headers.Add(definition);
            }

            // Note: Validation rules are handled directly in validation methods instead of through editing manager
            // This keeps the architecture clean and avoids complex type conversions

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Failed to apply column definitions", ex);
        }
    }

    /// <summary>
    /// OOP: Initialize all managers
    /// </summary>
    private async Task<Result<bool>> InitializeManagers()
    {
        try
        {
            // Initialize minimum empty rows
            await EnsureMinimumRows();

            _config.Logger?.Info("✅ All managers initialized successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Failed to initialize managers", ex);
        }
    }

    #endregion

    #region Private Methods - Hybrid Helper Functions

    /// <summary>
    /// FUNCTIONAL: Pure function to check if row is empty
    /// </summary>
    private bool IsRowEmpty(DataGridRow row) =>
        row.Cells.All(cell => cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()));

    /// <summary>
    /// FUNCTIONAL: Pure function to check if dictionary row is empty
    /// </summary>
    private bool IsRowEmpty(IReadOnlyDictionary<string, object?> row) =>
        row.Values.All(value => value == null || string.IsNullOrWhiteSpace(value?.ToString()));

    /// <summary>
    /// FUNCTIONAL: Pure transformation function
    /// </summary>
    private IReadOnlyDictionary<string, object?> CreateRowDictionary(DataGridRow row)
    {
        var dictionary = new Dictionary<string, object?>();
        
        for (int i = 0; i < Math.Min(row.Cells.Count, _headers.Count); i++)
        {
            var header = _headers[i];
            var cell = row.Cells[i];
            dictionary[header.Name] = cell.Value;
        }

        return dictionary;
    }

    /// <summary>
    /// FUNCTIONAL: Convert Core PerformanceConfiguration to Advanced PerformanceConfiguration
    /// </summary>
    private static RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration ConvertToAdvancedPerformanceConfig(RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.PerformanceConfiguration coreConfig)
    {
        if (coreConfig == null)
        {
            return new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration
            {
                VirtualizationThreshold = 1000,
                EnableThrottling = true,
                ThrottleDelay = TimeSpan.FromMilliseconds(100)
            };
        }
        
        return new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.PerformanceConfiguration
        {
            VirtualizationThreshold = coreConfig.VirtualizationThreshold,
            EnableThrottling = coreConfig.EnableThrottling,
            ThrottleDelay = coreConfig.EnableThrottling ? coreConfig.ThrottleDelay : TimeSpan.Zero
        };
    }

    /// <summary>
    /// FUNCTIONAL: Pure function to create validation rule
    /// </summary>
    private ValidationRule CreateValidationRule(GridColumnDefinition definition) => new()
    {
        Validator = value => ValidateValue(value, definition),
        ErrorMessage = $"Invalid value for {definition.DisplayName}",
        Severity = ValidationSeverity.Error
    };

    /// <summary>
    /// FUNCTIONAL: Pure validation function
    /// </summary>
    private bool ValidateValue(object? value, GridColumnDefinition definition)
    {
        if (!string.IsNullOrEmpty(definition.ValidationPattern) && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
        {
            return false;
        }

        // Add more validation logic here based on GridColumnDefinition properties
        return true;
    }

    /// <summary>
    /// FUNCTIONAL: Monadic logging operation
    /// </summary>
    private async Task<Result<bool>> LogAsync(string message, LogLevel level)
    {
        try
        {
            _config.Logger?.Log(level, message);
            await Task.CompletedTask;
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Logging failed: {message}", ex);
        }
    }

    #endregion

    #region Placeholder Methods - To Be Implemented

    private async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ValidateImportData(IReadOnlyList<IReadOnlyDictionary<string, object?>> data) =>
        await Task.FromResult(Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(data));

    private async Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> TransformDataForImport(IReadOnlyList<IReadOnlyDictionary<string, object?>> data, RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ImportOptions options) =>
        await Task.FromResult(Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(data));

    private async Task<Result<InternalImportResult>> ApplyDataToGrid(IReadOnlyList<IReadOnlyDictionary<string, object?>> data)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var importedRows = 0;
            var errorRows = 0;
            var errors = new List<string>();

            // Clear existing data
            _dataRows.Clear();

            // Import each row
            foreach (var rowData in data)
            {
                try
                {
                    var newRow = new DataGridRow
                    {
                        Cells = new List<DataGridCell>()
                    };

                    // Create cells for each column based on headers
                    foreach (var header in _headers)
                    {
                        var cellValue = rowData.ContainsKey(header.Name) ? rowData[header.Name] : null;
                        var cell = new DataGridCell
                        {
                            Value = cellValue,
                            ValidationState = true,
                            IsSelected = false,
                            IsFocused = false,
                            ColumnWidth = header.Width,
                            IsValidationCell = header.IsValidationColumn,
                            IsDeleteCell = header.IsDeleteColumn,
                            CellBackgroundBrush = "White",
                            BorderBrush = "#808080"
                        };
                        
                        // Set validation info for validation cells
                        if (header.IsValidationColumn)
                        {
                            // TODO: Calculate actual validation errors for this row
                            cell.HasValidationErrors = CheckRowHasValidationErrors(rowData);
                            cell.ValidationErrorCount = CountRowValidationErrors(rowData);
                            cell.Value = cell.HasValidationErrors ? $"⚠️ {cell.ValidationErrorCount}" : "✓";
                        }
                        else
                        {
                            // Check if this specific cell has validation errors
                            var hasError = CheckCellValidationError(header.Name, cellValue);
                            if (hasError)
                            {
                                cell.BorderBrush = "Red"; // Validation error border
                                cell.ValidationState = false;
                            }
                        }
                        
                        newRow.Cells.Add(cell);
                    }

                    _dataRows.Add(newRow);
                    importedRows++;
                }
                catch (Exception ex)
                {
                    errorRows++;
                    errors.Add($"Row {importedRows + errorRows}: {ex.Message}");
                }
            }

            // Ensure minimum rows
            await EnsureMinimumRows();

            stopwatch.Stop();
            
            return await Task.FromResult(Result<InternalImportResult>.Success(new InternalImportResult 
            { 
                Success = errorRows == 0,
                ImportedRows = importedRows,
                SkippedRows = 0,
                ErrorRows = errorRows,
                Errors = errors.ToArray(),
                Duration = stopwatch.Elapsed,
                WasCancelled = false
            }));
        }
        catch (Exception ex)
        {
            return Result<InternalImportResult>.Failure($"Failed to apply data to grid: {ex.Message}", ex);
        }
    }

    private async Task<Result<InternalImportResult>> ValidateAfterImport(InternalImportResult result) =>
        await Task.FromResult(Result<InternalImportResult>.Success(result));

    private async Task<Result<InternalImportResult>> UpdateUIAfterImport(InternalImportResult result) =>
        await Task.FromResult(Result<InternalImportResult>.Success(result));

    private async Task EnsureMinimumRows()
    {
        while (_dataRows.Count < _config.MinimumRows)
        {
            var emptyRow = new DataGridRow { Cells = CreateEmptyCells() };
            _dataRows.Add(emptyRow);
        }
    }

    private List<DataGridCell> CreateEmptyCells() =>
        _headers.Select(header => new DataGridCell 
        { 
            Value = null, 
            ValidationState = true,
            IsSelected = false,
            IsFocused = false
        }).ToList();

    #endregion

    #region Public Properties

    /// <summary>
    /// Has any data been imported
    /// </summary>
    public bool HasData => _dataRows.Any(row => !IsRowEmpty(row));

    /// <summary>
    /// Current row count
    /// </summary>
    public int RowCount => _dataRows.Count;

    /// <summary>
    /// Current column count
    /// </summary>
    public int ColumnCount => _headers.Count;

    /// <summary>
    /// Observable collection of headers for UI binding
    /// </summary>
    public ObservableCollection<GridColumnDefinition> Headers => _headers;

    /// <summary>
    /// Observable collection of data rows for UI binding
    /// </summary>
    public ObservableCollection<DataGridRow> DataRows => _dataRows;

    #endregion

    #region Missing Implementation Methods

    private Result<int> ValidateRowForDeletion(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _dataRows.Count)
        {
            return Result<int>.Failure($"Invalid row index: {rowIndex}");
        }
        return Result<int>.Success(rowIndex);
    }

    private Result<bool> CheckMinimumRowConstraint()
    {
        var nonEmptyRows = _dataRows.Count(row => !IsRowEmpty(row));
        if (nonEmptyRows <= 1 && _dataRows.Count <= _config.MinimumRows)
        {
            return Result<bool>.Failure("Cannot delete: Minimum row constraint would be violated");
        }
        return Result<bool>.Success(true);
    }

    private Result<string> DetermineDeleteStrategy(int rowIndex)
    {
        var row = _dataRows[rowIndex];
        if (IsRowEmpty(row))
        {
            return Result<string>.Success("RemoveEmptyRow");
        }
        return Result<string>.Success("ClearRowData");
    }

    private async Task<Result<bool>> ExecuteDeleteStrategy(string strategy, int rowIndex)
    {
        try
        {
            switch (strategy)
            {
                case "RemoveEmptyRow":
                    _dataRows.RemoveAt(rowIndex);
                    break;
                case "ClearRowData":
                    var row = _dataRows[rowIndex];
                    foreach (var cell in row.Cells)
                    {
                        cell.Value = null;
                        cell.ValidationState = true;
                    }
                    break;
            }
            await EnsureMinimumRows();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to execute delete strategy: {ex.Message}", ex);
        }
    }

    private async Task<Result<bool>> UpdateUIAfterDelete()
    {
        _uiUpdates.OnNext(new UIUpdateEvent("RowDeleted", null));
        return await Task.FromResult(Result<bool>.Success(true));
    }

    private async Task<Result<IReadOnlyList<InternalValidationResult>>> ValidateDataBatch(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data, 
        IProgress<ValidationProgress> progress)
    {
        var results = new List<InternalValidationResult>();
        var totalRows = data.Count;

        for (int i = 0; i < totalRows; i++)
        {
            var row = data[i];
            var validationResult = ValidateRow(row);
            results.Add(validationResult);

            // Report progress
            progress?.Report(new ValidationProgress 
            { 
                CurrentRow = i + 1, 
                ProcessedRows = i + 1,
                TotalRows = totalRows
            });
        }

        return await Task.FromResult(Result<IReadOnlyList<InternalValidationResult>>.Success(results.AsReadOnly()));
    }

    private InternalValidationResult ValidateRow(IReadOnlyDictionary<string, object?> row)
    {
        var errors = new List<string>();
        var isValid = true;

        foreach (var header in _headers)
        {
            if (row.TryGetValue(header.Name, out var value))
            {
                if (!ValidateValue(value, header))
                {
                    isValid = false;
                    errors.Add($"Invalid value in column '{header.DisplayName}': {value}");
                }
            }
            else if (!string.IsNullOrEmpty(header.ValidationPattern))
            {
                isValid = false;
                errors.Add($"Missing required value for column '{header.DisplayName}'");
            }
        }

        return new InternalValidationResult
        {
            IsValid = isValid,
            ErrorMessages = errors,
            TotalCells = 1,
            ValidCells = isValid ? 1 : 0,
            InvalidCells = isValid ? 0 : 1,
            ValidationErrors = errors.Select(e => new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationError { Message = e, Severity = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ValidationSeverity.Error }).ToArray()
        };
    }

    private async Task<Result<InternalValidationResult>> AggregateValidationResults(IReadOnlyList<InternalValidationResult> results)
    {
        var allErrors = results.SelectMany(r => r.ErrorMessages).ToList();
        var overallResult = new InternalValidationResult
        {
            IsValid = results.All(r => r.IsValid),
            ErrorMessages = allErrors,
            TotalCells = results.Sum(r => r.TotalCells),
            ValidCells = results.Sum(r => r.ValidCells),
            InvalidCells = results.Sum(r => r.InvalidCells),
            ValidationErrors = results.SelectMany(r => r.ValidationErrors).ToArray()
        };

        return await Task.FromResult(Result<InternalValidationResult>.Success(overallResult));
    }

    private async Task<Result<InternalValidationResult>> UpdateValidationUI(InternalValidationResult result)
    {
        // Convert internal ValidationResult to public ValidationResult for the event
        var publicErrors = result.ValidationErrors?.Select(e => 
            new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationError(
                Row: e.RowIndex ?? 0,
                Column: 0, // TODO: Map column name to index
                Message: e.Message ?? "Unknown error"
            )).ToArray() ?? new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationError[0];
            
        var publicResult = new RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationResult(
            TotalCells: result.TotalCells,
            ValidCells: result.ValidCells,
            InvalidCells: result.InvalidCells,
            Errors: publicErrors
        );
        
        _validationChanges.OnNext(new ValidationChangeEvent(null, publicResult));
        return await Task.FromResult(Result<InternalValidationResult>.Success(result));
    }

    private Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>> TransformDataForExport(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data, 
        RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models.ExportOptions options)
    {
        // For now, just return the data as-is
        return Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>.Success(data);
    }

    /// <summary>
    /// Check if row has validation errors (simple implementation)
    /// </summary>
    private bool CheckRowHasValidationErrors(IReadOnlyDictionary<string, object?> rowData)
    {
        // Simple validation rules - empty Name, invalid Age, invalid Email
        var name = rowData.GetValueOrDefault("Name")?.ToString();
        var age = rowData.GetValueOrDefault("Age");
        var email = rowData.GetValueOrDefault("Email")?.ToString();

        if (string.IsNullOrEmpty(name)) return true;
        if (age != null && int.TryParse(age.ToString(), out int ageInt) && (ageInt < 0 || ageInt > 120)) return true;
        if (!string.IsNullOrEmpty(email) && (!email.Contains("@") || !email.Contains("."))) return true;

        return false;
    }

    /// <summary>
    /// Count validation errors in row (simple implementation)
    /// </summary>
    private int CountRowValidationErrors(IReadOnlyDictionary<string, object?> rowData)
    {
        int errorCount = 0;
        
        var name = rowData.GetValueOrDefault("Name")?.ToString();
        var age = rowData.GetValueOrDefault("Age");
        var email = rowData.GetValueOrDefault("Email")?.ToString();

        if (string.IsNullOrEmpty(name)) errorCount++;
        if (age != null && int.TryParse(age.ToString(), out int ageInt) && (ageInt < 0 || ageInt > 120)) errorCount++;
        if (!string.IsNullOrEmpty(email) && (!email.Contains("@") || !email.Contains("."))) errorCount++;

        return errorCount;
    }

    /// <summary>
    /// Check if specific cell has validation error
    /// </summary>
    private bool CheckCellValidationError(string fieldName, object? value)
    {
        try
        {
            return fieldName.ToLower() switch
            {
                "name" => string.IsNullOrEmpty(value?.ToString()),
                "age" => value != null && int.TryParse(value.ToString(), out int age) && (age < 0 || age > 120),
                "email" => !string.IsNullOrEmpty(value?.ToString()) && 
                          (!value.ToString()!.Contains("@") || !value.ToString()!.Contains(".")),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        try
        {
            _dataChanges?.Dispose();
            _validationChanges?.Dispose();
            _uiUpdates?.Dispose();
            
            _selectionManager?.Dispose();
            _editingManager?.Dispose();
            _resizeManager?.Dispose();
            _eventManager?.Dispose();

            _config.Logger?.Info("🔧 DataGridCoordinator disposed");
        }
        catch (Exception ex)
        {
            _config.Logger?.Error(ex, "🚨 Error disposing DataGridCoordinator");
        }
    }

    #endregion
}

#region Functional Types & Events

/// <summary>
/// FUNCTIONAL: Immutable data change event
/// </summary>
public readonly record struct DataChangeEvent(
    DataGridCell Cell,
    string? OldValue,
    string? NewValue
);

/// <summary>
/// FUNCTIONAL: Immutable validation change event
/// </summary>
public readonly record struct ValidationChangeEvent(
    DataGridCell Cell,
    RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.ValidationResult ValidationResult
);

/// <summary>
/// OOP: UI update event for manager coordination
/// </summary>
public class UIUpdateEvent
{
    public string UpdateType { get; }
    public object? UpdateData { get; }

    public UIUpdateEvent(string updateType, object? updateData)
    {
        UpdateType = updateType;
        UpdateData = updateData;
    }
}

// ImportOptions definition removed - using Models.ImportOptions instead

// ExportOptions definition removed - using Models.ExportOptions instead

#endregion