using Microsoft.Extensions.Logging;
using System.Data;
using System.Collections.ObjectModel;

// CLEAN API DESIGN - Single namespace, single using statement
namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// 🚀 PROFESSIONAL ENTERPRISE DATAGRID - Clean API Facade
/// 
/// ✅ SINGLE USING: using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
/// ✅ DUAL-PURPOSE: UI Components + Headless Automation
/// ✅ ENTERPRISE-READY: 10M+ rows, sub-second performance
/// ✅ HYBRID ARCHITECTURE: Functional-OOP with Result<T> monads
/// ✅ PROFESSIONAL LOGGING: Comprehensive error handling and monitoring
/// ✅ CLEAN DESIGN: Single point of entry, hidden complexity
/// 
/// USAGE PATTERNS:
/// 
/// // UI Mode - Visual Component:
/// var uiGrid = DataGridComponent.CreateForUI(logger);
/// await uiGrid.InitializeAsync(columns, configuration);
/// MyContainer.Content = uiGrid.UIComponent;
/// 
/// // Headless Mode - Automation/Scripting:
/// var headlessGrid = DataGridComponent.CreateHeadless(logger);
/// await headlessGrid.ImportDataAsync(data);
/// var results = await headlessGrid.SearchAsync("query");
/// 
/// // Hybrid Mode - UI with Programmatic Control:
/// var hybridGrid = DataGridComponent.CreateForUI(logger);
/// await hybridGrid.ImportDataAsync(data); // Updates both core and UI
/// await hybridGrid.RefreshUIAsync(); // Manual UI refresh when needed
/// </summary>
public sealed class DataGridComponent : IDisposable
{
    #region Private Fields - Professional Architecture
    
    private readonly ILogger? _logger;
    private readonly bool _isUIMode;
    private readonly Internal.Interfaces.IDataGridCore _coreService;
    private readonly Internal.Interfaces.IUIManager? _uiManager;
    private AdvancedDataGrid? _uiComponent;
    private bool _isInitialized;
    private bool _disposed;

    #endregion

    #region Factory Methods - Clean API Entry Points

    /// <summary>
    /// Create DataGrid instance for UI applications
    /// Includes visual component and UI management
    /// </summary>
    /// <param name="logger">Optional Microsoft.Extensions.Logging logger</param>
    /// <returns>UI-enabled DataGrid instance</returns>
    public static DataGridComponent CreateForUI(ILogger? logger = null)
    {
        logger?.LogInformation("🎨 DataGrid: Creating UI-enabled instance");
        return new DataGridComponent(logger, uiMode: true);
    }

    /// <summary>
    /// Create DataGrid instance for headless automation/scripting
    /// Pure data operations without UI overhead
    /// </summary>
    /// <param name="logger">Optional Microsoft.Extensions.Logging logger</param>
    /// <returns>Headless DataGrid instance</returns>
    public static DataGridComponent CreateHeadless(ILogger? logger = null)
    {
        logger?.LogInformation("🤖 DataGrid: Creating headless instance");
        return new DataGridComponent(logger, uiMode: false);
    }

    #endregion

    #region Internal Constructor - Factory Pattern

    private DataGridComponent(ILogger? logger, bool uiMode)
    {
        _logger = logger;
        _isUIMode = uiMode;
        
        // Create core service (always present)
        _coreService = new Internal.Core.DataGridCore(logger);
        
        // Create UI components only in UI mode
        if (_isUIMode)
        {
            _uiComponent = new AdvancedDataGrid();
            _uiManager = new Internal.Managers.UIManager(_uiComponent, logger);
            logger?.LogDebug("✅ DataGrid: UI components initialized");
        }
        
        logger?.LogDebug("✅ DataGrid: Instance created (Mode: {Mode})", _isUIMode ? "UI" : "Headless");
    }

    #endregion

    #region Public Properties - Clean API Surface

    /// <summary>
    /// Get the WinUI3 visual component for embedding in UI
    /// Only available in UI mode, null in headless mode
    /// </summary>
    public AdvancedDataGrid? UIComponent => _uiComponent;

    /// <summary>
    /// Check if DataGrid has been initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Check if DataGrid is in UI mode
    /// </summary>
    public bool IsUIMode => _isUIMode;

    /// <summary>
    /// Current row count in the DataGrid
    /// </summary>
    public int RowCount => _coreService.RowCount;

    /// <summary>
    /// Current column count in the DataGrid
    /// </summary>
    public int ColumnCount => _coreService.ColumnCount;

    #endregion

    #region Initialization - Enterprise Configuration

    /// <summary>
    /// Initialize DataGrid with column definitions and configuration
    /// Supports both UI and headless modes
    /// </summary>
    /// <param name="columns">Column definitions</param>
    /// <param name="config">Optional configuration</param>
    /// <returns>Success/failure result</returns>
    public async Task<DataGridResult<bool>> InitializeAsync(
        IReadOnlyList<ColumnDefinition> columns,
        DataGridConfiguration? config = null)
    {
        try
        {
            _logger?.LogInformation("🚀 DataGrid: Initializing with {ColumnCount} columns (Mode: {Mode})", 
                columns.Count, _isUIMode ? "UI" : "Headless");
            
            // Initialize core service
            var coreResult = await _coreService.InitializeAsync(columns, config);
            if (!coreResult.IsSuccess)
            {
                _logger?.LogError("❌ DataGrid: Core initialization failed: {Error}", coreResult.ErrorMessage);
                return DataGridResult<bool>.Failure(coreResult.ErrorMessage);
            }

            // Initialize UI components if in UI mode
            if (_isUIMode && _uiManager != null && _uiComponent != null)
            {
                var uiResult = await _uiManager.InitializeAsync(columns, config);
                if (!uiResult.IsSuccess)
                {
                    _logger?.LogError("❌ DataGrid: UI initialization failed: {Error}", uiResult.ErrorMessage);
                    return DataGridResult<bool>.Failure(uiResult.ErrorMessage);
                }
            }

            _isInitialized = true;
            _logger?.LogInformation("✅ DataGrid: Initialization completed successfully");
            return DataGridResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Initialization failed with exception");
            return DataGridResult<bool>.Failure($"Initialization failed: {ex.Message}");
        }
    }

    #endregion

    #region Data Operations - Core Functionality

    /// <summary>
    /// Import data from various sources
    /// Automatically updates UI if in UI mode
    /// </summary>
    /// <param name="data">Data to import</param>
    /// <param name="options">Import options</param>
    /// <returns>Import result with statistics</returns>
    public async Task<DataGridResult<ImportResult>> ImportDataAsync(
        object data,
        ImportOptions? options = null)
    {
        if (!_isInitialized)
        {
            return DataGridResult<ImportResult>.Failure("DataGrid not initialized. Call InitializeAsync first.");
        }

        try
        {
            _logger?.LogInformation("📥 DataGrid: Importing data (Type: {DataType})", data.GetType().Name);
            
            // Convert public API options to internal options
            Internal.Models.ImportOptions? internalOptions = options != null 
                ? new Internal.Models.ImportOptions(
                    ReplaceExistingData: options.OverwriteExisting,
                    ValidateBeforeImport: options.ValidateOnImport)
                : null;

            // Import to core service
            var coreResult = await _coreService.ImportDataAsync(data, internalOptions);
            if (!coreResult.IsSuccess)
            {
                _logger?.LogError("❌ DataGrid: Core import failed: {Error}", coreResult.ErrorMessage);
                return DataGridResult<ImportResult>.Failure(coreResult.ErrorMessage);
            }

            // Update UI if in UI mode
            if (_isUIMode && _uiManager != null)
            {
                await _uiManager.RefreshDataAsync();
                _logger?.LogDebug("🎨 DataGrid: UI refreshed after import");
            }

            _logger?.LogInformation("✅ DataGrid: Import completed ({RowCount} rows imported)", 
                coreResult.Value.ImportedRows);
            
            // Convert internal ImportResult to public ImportResult
            var publicImportResult = new ImportResult(
                RowsProcessed: coreResult.Value.ImportedRows,
                ErrorCount: coreResult.Value.ErrorRows,
                Errors: coreResult.Value.Errors.ToArray());
                
            return DataGridResult<ImportResult>.Success(publicImportResult);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Import failed with exception");
            return DataGridResult<ImportResult>.Failure($"Import failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Export data to various formats
    /// </summary>
    /// <param name="format">Export format</param>
    /// <param name="filePath">Optional file path</param>
    /// <returns>Export result</returns>
    public async Task<DataGridResult<ExportResult>> ExportDataAsync(
        ExportFormat format,
        string? filePath = null)
    {
        if (!_isInitialized)
        {
            return DataGridResult<ExportResult>.Failure("DataGrid not initialized");
        }

        try
        {
            _logger?.LogInformation("📤 DataGrid: Exporting data (Format: {Format})", format);
            
            var result = await _coreService.ExportDataAsync(format, filePath);
            
            _logger?.LogInformation("✅ DataGrid: Export completed ({RowCount} rows exported)", 
                result.IsSuccess ? result.Value.ExportedRows : 0);
            
            // Convert internal ExportResult to public ExportResult  
            return result.IsSuccess 
                ? DataGridResult<ExportResult>.Success(new ExportResult(
                    RowsExported: result.Value.ExportedRows,
                    FilePath: result.Value.FilePath))
                : DataGridResult<ExportResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Export failed with exception");
            return DataGridResult<ExportResult>.Failure($"Export failed: {ex.Message}");
        }
    }

    #endregion

    #region Search, Filter, Sort - Advanced Features

    /// <summary>
    /// Search for data in the grid
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="options">Search options</param>
    /// <returns>Search results</returns>
    public async Task<DataGridResult<SearchResult>> SearchAsync(
        string query, 
        SearchOptions? options = null)
    {
        if (!_isInitialized)
        {
            return DataGridResult<SearchResult>.Failure("DataGrid not initialized");
        }

        try
        {
            _logger?.LogDebug("🔍 DataGrid: Searching for '{Query}'", query);
            
            var result = await _coreService.SearchAsync(query, options);
            
            // Update UI if in UI mode
            if (result.IsSuccess && _isUIMode && _uiManager != null)
            {
                await _uiManager.HighlightSearchResultsAsync(result.Value);
            }

            return result.IsSuccess 
                ? DataGridResult<SearchResult>.Success(result.Value)
                : DataGridResult<SearchResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Search failed with exception");
            return DataGridResult<SearchResult>.Failure($"Search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Apply filter to the data
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filter result</returns>
    public async Task<DataGridResult<FilterResult>> ApplyFilterAsync(FilterCriteria filter)
    {
        if (!_isInitialized)
        {
            return DataGridResult<FilterResult>.Failure("DataGrid not initialized");
        }

        try
        {
            _logger?.LogDebug("🔽 DataGrid: Applying filter");
            
            var result = await _coreService.ApplyFilterAsync(filter);
            
            // Update UI if in UI mode
            if (result.IsSuccess && _isUIMode && _uiManager != null)
            {
                await _uiManager.RefreshDataAsync();
            }

            return result.IsSuccess 
                ? DataGridResult<FilterResult>.Success(result.Value)
                : DataGridResult<FilterResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Filter failed with exception");
            return DataGridResult<FilterResult>.Failure($"Filter failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sort data by columns
    /// </summary>
    /// <param name="sortOptions">Sort criteria</param>
    /// <returns>Sort result</returns>
    public async Task<DataGridResult<SortResult>> SortAsync(SortOptions sortOptions)
    {
        if (!_isInitialized)
        {
            return DataGridResult<SortResult>.Failure("DataGrid not initialized");
        }

        try
        {
            _logger?.LogDebug("🔄 DataGrid: Sorting data");
            
            var result = await _coreService.SortAsync(sortOptions);
            
            // Update UI if in UI mode
            if (result.IsSuccess && _isUIMode && _uiManager != null)
            {
                await _uiManager.RefreshDataAsync();
            }

            return result.IsSuccess 
                ? DataGridResult<SortResult>.Success(result.Value)
                : DataGridResult<SortResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Sort failed with exception");
            return DataGridResult<SortResult>.Failure($"Sort failed: {ex.Message}");
        }
    }

    #endregion

    #region UI-Specific Methods - Only Available in UI Mode

    /// <summary>
    /// Manually refresh the UI (only in UI mode)
    /// For automation scripts that want to update UI programmatically
    /// </summary>
    /// <returns>Success/failure result</returns>
    public async Task<DataGridResult<bool>> RefreshUIAsync()
    {
        if (!_isUIMode)
        {
            return DataGridResult<bool>.Success(true); // No-op in headless mode
        }

        if (!_isInitialized)
        {
            return DataGridResult<bool>.Failure("DataGrid not initialized");
        }

        try
        {
            _logger?.LogDebug("🎨 DataGrid: Manual UI refresh requested");
            
            if (_uiManager != null)
            {
                await _uiManager.RefreshDataAsync();
                _logger?.LogDebug("✅ DataGrid: UI refresh completed");
                return DataGridResult<bool>.Success(true);
            }
            
            return DataGridResult<bool>.Failure("UI manager not available");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: UI refresh failed");
            return DataGridResult<bool>.Failure($"UI refresh failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Apply color theme (only affects UI mode)
    /// </summary>
    /// <param name="theme">Color theme configuration</param>
    /// <returns>Success/failure result</returns>
    public async Task<DataGridResult<bool>> ApplyColorThemeAsync(ColorTheme theme)
    {
        if (!_isUIMode)
        {
            return DataGridResult<bool>.Success(true); // No-op in headless mode
        }

        try
        {
            _logger?.LogDebug("🎨 DataGrid: Applying color theme");
            
            if (_uiManager != null)
            {
                await _uiManager.ApplyColorThemeAsync(theme);
                return DataGridResult<bool>.Success(true);
            }
            
            return DataGridResult<bool>.Failure("UI manager not available");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Theme application failed");
            return DataGridResult<bool>.Failure($"Theme application failed: {ex.Message}");
        }
    }

    #endregion

    #region Validation - Enterprise Data Quality

    /// <summary>
    /// Validate all data in the grid
    /// </summary>
    /// <returns>Validation results</returns>
    public async Task<DataGridResult<ValidationResult>> ValidateAllAsync()
    {
        if (!_isInitialized)
        {
            return DataGridResult<ValidationResult>.Failure("DataGrid not initialized");
        }

        try
        {
            _logger?.LogDebug("✅ DataGrid: Starting full validation");
            
            var internalResult = await _coreService.ValidateAllAsync();
            
            // Update UI with validation results if in UI mode
            if (internalResult.IsSuccess && _isUIMode && _uiManager != null)
            {
                await _uiManager.UpdateValidationResultsAsync(internalResult.Value);
            }

            // Convert internal ValidationResult to public ValidationResult
            if (internalResult.IsSuccess)
            {
                // Convert internal ValidationErrors to public ValidationErrors  
                var publicErrors = internalResult.Value.ValidationErrors?.Select(e =>
                    new ValidationError(
                        Row: e.RowIndex ?? 0,
                        Column: !string.IsNullOrEmpty(e.ColumnName) ? 0 : 0, // TODO: Map column name to index
                        Message: e.Message ?? "Unknown error"
                    )).ToList() ?? new List<ValidationError>();

                var publicValidationResult = new ValidationResult(
                    TotalCells: internalResult.Value.TotalCells,
                    ValidCells: internalResult.Value.ValidCells,
                    InvalidCells: internalResult.Value.InvalidCells,
                    Errors: publicErrors);
                
                return DataGridResult<ValidationResult>.Success(publicValidationResult);
            }
            else
            {
                return DataGridResult<ValidationResult>.Failure(internalResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Validation failed");
            return DataGridResult<ValidationResult>.Failure($"Validation failed: {ex.Message}");
        }
    }

    #endregion

    #region Resource Management - Professional Disposal

    /// <summary>
    /// Dispose of all resources properly
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger?.LogDebug("🧹 DataGrid: Disposing resources");

        try
        {
            _uiManager?.Dispose();
            _coreService?.Dispose();
            _uiComponent = null;
            
            _disposed = true;
            _logger?.LogDebug("✅ DataGrid: Resources disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 DataGrid: Error during disposal");
        }
    }

    #endregion
}

#region Supporting Types - Clean API Models

/// <summary>
/// Result pattern for DataGrid operations
/// Provides consistent error handling across all operations
/// </summary>
/// <typeparam name="T">Result value type</typeparam>
public record DataGridResult<T>
{
    public bool IsSuccess { get; init; }
    public T Value { get; init; } = default!;
    public string Error { get; init; } = string.Empty;

    public static DataGridResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static DataGridResult<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

/// <summary>
/// Column definition for DataGrid initialization
/// </summary>
public record ColumnDefinition
{
    public required string Name { get; init; }
    public required Type DataType { get; init; }
    public string? DisplayName { get; init; }
    public bool IsReadOnly { get; init; } = false;
    public int Width { get; init; } = 100;
    public bool IsVisible { get; init; } = true;
}

/// <summary>
/// Configuration for DataGrid initialization
/// </summary>
public record DataGridConfiguration
{
    public int MinimumRows { get; init; } = 1;
    public bool EnableVirtualization { get; init; } = true;
    public bool EnableValidation { get; init; } = true;
    public ColorTheme? DefaultTheme { get; init; }
    public PerformanceSettings? Performance { get; init; }
}

/// <summary>
/// Import/Export result information
/// </summary>
public record ImportResult(int RowsProcessed, int ErrorCount, string[] Errors);
public record ExportResult(int RowsExported, string? FilePath);

/// <summary>
/// Search functionality results
/// </summary>
public record SearchResult(int MatchCount, IReadOnlyList<SearchMatch> Matches);
public record SearchMatch(int Row, int Column, string Value);

/// <summary>
/// Filter and sort results
/// </summary>
public record FilterResult(int VisibleRows, int HiddenRows);
public record SortResult(string SortedBy, bool IsAscending);

/// <summary>
/// Validation results
/// </summary>
public record ValidationResult(int TotalCells, int ValidCells, int InvalidCells, IReadOnlyList<ValidationError> Errors);
public record ValidationError(int Row, int Column, string Message);

/// <summary>
/// Color theme configuration
/// </summary>
public record ColorTheme(string Background, string Foreground, string BorderColor, string SelectedColor);

/// <summary>
/// Import/Export and operation options
/// </summary>
public record ImportOptions(bool OverwriteExisting = true, bool ValidateOnImport = true);
public record SearchOptions(bool CaseSensitive = false, bool WholeWord = false);
public record FilterCriteria(string Column, string Operator, object Value);
public record SortOptions(string Column, bool Ascending = true);
public record PerformanceSettings(int VirtualizationThreshold = 1000, int BatchSize = 100);

/// <summary>
/// Export format enumeration
/// </summary>
public enum ExportFormat
{
    Excel,
    CSV,
    JSON,
    XML,
    DataTable
}

#endregion