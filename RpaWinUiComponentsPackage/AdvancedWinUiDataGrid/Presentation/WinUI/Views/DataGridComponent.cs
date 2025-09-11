using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ImportData;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ExportData;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.InitializeGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.ManageRows;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.UI.Components;

/// <summary>
/// UI: Main AdvancedDataGrid component
/// CLEAN ARCHITECTURE: UI layer - WinUI 3 UserControl
/// RESPONSIBILITY: Handle UI interactions and coordinate with application services
/// </summary>
public sealed partial class AdvancedDataGridComponent : UserControl, IDisposable
{
    #region UI: Private Fields
    
    private IDataGridService? _dataGridService;
    private IDataGridAutoRowHeightService? _autoRowHeightService;
    private GridState? _currentState;
    private bool _isDisposed;
    private ILogger? _logger;
    
    #endregion

    #region UI: Constructor
    
    public AdvancedDataGridComponent()
    {
        this.InitializeComponent();
        InitializeServices();
    }
    
    #endregion

    #region UI: Public Properties
    
    /// <summary>
    /// Current grid state
    /// </summary>
    public GridState? CurrentState => _currentState;
    
    /// <summary>
    /// Is the grid initialized
    /// </summary>
    public bool IsInitialized => _currentState != null && _currentState.IsInitialized;
    
    /// <summary>
    /// Total row count
    /// </summary>
    public int TotalRowCount => _currentState?.Rows?.Count ?? 0;
    
    /// <summary>
    /// Filtered row count
    /// </summary>
    public int FilteredRowCount => _currentState?.FilteredRowIndices?.Count ?? _currentState?.Rows.Count ?? 0;
    
    #endregion

    #region UI: Events
    
    /// <summary>
    /// Raised when data changes
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? DataChanged;
    
    /// <summary>
    /// Raised when validation completes
    /// </summary>
    public event EventHandler<ValidationCompletedEventArgs>? ValidationCompleted;
    
    /// <summary>
    /// Raised when import/export operations complete
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs>? OperationCompleted;
    
    /// <summary>
    /// Raised when errors occur
    /// </summary>
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;
    
    /// <summary>
    /// Raised when item is clicked in data view
    /// </summary>
    public event EventHandler<ItemClickedEventArgs>? ItemClicked;
    
    /// <summary>
    /// Raised when selection changes in data view
    /// </summary>
    public event EventHandler<DataGridSelectionChangedEventArgs>? SelectionChanged;
    
    #endregion

    #region UI: Initialization
    
    private void InitializeServices()
    {
        try
        {
            _dataGridService = DataGridServiceFactory.CreateWithUI(_logger);
            
            // Initialize auto row height service
            var uiConfiguration = UIConfiguration.Default;
            var uiService = new DataGridUIService(_logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            var rowHeightService = new RowHeightCalculationService(
                _logger as ILogger<RowHeightCalculationService> ?? 
                Microsoft.Extensions.Logging.Abstractions.NullLogger<RowHeightCalculationService>.Instance);
            
            _autoRowHeightService = new DataGridAutoRowHeightService(
                rowHeightService, 
                uiService, 
                uiConfiguration,
                _logger as ILogger<DataGridAutoRowHeightService> ?? 
                Microsoft.Extensions.Logging.Abstractions.NullLogger<DataGridAutoRowHeightService>.Instance);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize DataGrid services");
            OnErrorOccurred(new ErrorEventArgs($"Service initialization failed: {ex.Message}", ex));
        }
    }
    
    /// <summary>
    /// Initialize the DataGrid with column definitions
    /// </summary>
    public async Task<bool> InitializeAsync(
        IReadOnlyList<Domain.ValueObjects.Core.ColumnDefinition> columns,
        ColorConfiguration? colorConfiguration = null,
        ValidationConfiguration? validationConfiguration = null,
        PerformanceConfiguration? performanceConfiguration = null,
        int minimumRows = 1)
    {
        try
        {
            if (_dataGridService == null)
            {
                _logger?.LogError("DataGrid service not available");
                return false;
            }
            
            var result = await _dataGridService.InitializeAsync(
                columns,
                colorConfiguration,
                validationConfiguration,
                performanceConfiguration);
            
            if (result.IsSuccess)
            {
                // Note: GetCurrentStateAsync would need to be implemented or use alternative approach
                //_currentState = await GetCurrentStateAsync();
                await RefreshUIAsync();
                OnDataChanged(new DataChangedEventArgs("Initialize", TotalRowCount));
            }
            else
            {
                OnErrorOccurred(new ErrorEventArgs($"Initialization failed: {result.Error}", result.Exception));
            }
            
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DataGrid initialization failed");
            OnErrorOccurred(new ErrorEventArgs($"Initialization failed: {ex.Message}", ex));
            return false;
        }
    }
    
    #endregion

    #region UI: Data Import/Export
    
    /// <summary>
    /// Import data from dictionary list
    /// </summary>
    public async Task<bool> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        Dictionary<int, bool>? checkboxStates = null,
        int startRow = 1,
        ImportMode mode = ImportMode.Replace,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? progress = null)
    {
        try
        {
            if (_dataGridService == null) return false;
            
            var result = await _dataGridService.ImportFromDictionaryAsync(
                data, checkboxStates, startRow, mode, timeout, progress);
            
            if (result.IsSuccess)
            {
                // Note: GetCurrentStateAsync would need to be implemented or use alternative approach
                //_currentState = await GetCurrentStateAsync();
                await RefreshUIAsync();
                OnDataChanged(new DataChangedEventArgs("ImportDictionary", TotalRowCount));
                OnOperationCompleted(new OperationCompletedEventArgs("ImportDictionary", result.Value));
            }
            else
            {
                OnErrorOccurred(new ErrorEventArgs($"Dictionary import failed: {result.Error}", result.Exception));
            }
            
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Dictionary import failed");
            OnErrorOccurred(new ErrorEventArgs($"Dictionary import failed: {ex.Message}", ex));
            return false;
        }
    }
    
    /// <summary>
    /// Export data to dictionary list
    /// </summary>
    public async Task<List<Dictionary<string, object?>>?> ExportToDictionaryAsync(
        bool exportOnlyFiltered = false,
        bool includeValidationAlerts = false,
        bool exportOnlyChecked = false,
        TimeSpan? timeout = null,
        IProgress<ExportProgress>? progress = null)
    {
        try
        {
            if (_dataGridService == null) return null;
            
            var result = await _dataGridService.ExportToDictionaryAsync(
                includeValidationAlerts, exportOnlyChecked, exportOnlyFiltered, false, timeout, progress);
            
            if (result.IsSuccess)
            {
                OnOperationCompleted(new OperationCompletedEventArgs("ExportDictionary", result.Value));
                return result.Value;
            }
            else
            {
                OnErrorOccurred(new ErrorEventArgs($"Dictionary export failed: {result.Error}", result.Exception));
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Dictionary export failed");
            OnErrorOccurred(new ErrorEventArgs($"Dictionary export failed: {ex.Message}", ex));
            return null;
        }
    }
    
    #endregion

    #region UI: Search and Filter
    
    /// <summary>
    /// Search for text in grid data
    /// </summary>
    public async Task<List<SearchResult>?> SearchAsync(
        string searchText,
        IReadOnlyList<string>? targetColumns = null,
        bool caseSensitive = false,
        bool useRegex = false,
        int maxResults = 1000,
        TimeSpan? timeout = null)
    {
        try
        {
            if (_dataGridService == null) return null;
            
            var searchOptions = new SearchOptions
            {
                ColumnNames = targetColumns?.ToList(),
                CaseSensitive = caseSensitive,
                UseRegex = useRegex
            };
            
            var result = await _dataGridService.SearchAsync(searchText, searchOptions);
            
            if (result.IsSuccess)
            {
                // Convert single SearchResult to List<SearchResult>
                return result.Value != null ? new List<SearchResult> { result.Value } : new List<SearchResult>();
            }
            else
            {
                OnErrorOccurred(new ErrorEventArgs($"Search failed: {result.Error}", result.Exception));
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Search failed");
            OnErrorOccurred(new ErrorEventArgs($"Search failed: {ex.Message}", ex));
            return null;
        }
    }
    
    /// <summary>
    /// Apply filters to grid data
    /// </summary>
    public async Task<bool> ApplyFiltersAsync(
        IReadOnlyList<FilterDefinition> filters,
        FilterLogicOperator logicOperator = FilterLogicOperator.And,
        TimeSpan? timeout = null)
    {
        try
        {
            if (_dataGridService == null) return false;
            
            var filterExpressions = filters.Select(FilterExpression.FromFilterDefinition).ToList();
            var result = await _dataGridService.ApplyFiltersAsync(filterExpressions);
            
            if (result.IsSuccess)
            {
                // Note: GetCurrentStateAsync would need to be implemented or use alternative approach
                //_currentState = await GetCurrentStateAsync();
                await RefreshUIAsync();
                OnDataChanged(new DataChangedEventArgs("ApplyFilters", FilteredRowCount));
            }
            else
            {
                OnErrorOccurred(new ErrorEventArgs($"Filter application failed: {result.Error}", result.Exception));
            }
            
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Filter application failed");
            OnErrorOccurred(new ErrorEventArgs($"Filter application failed: {ex.Message}", ex));
            return false;
        }
    }
    
    #endregion

    #region UI: Validation
    
    /// <summary>
    /// Validate all grid data
    /// </summary>
    public async Task<List<ValidationError>?> ValidateAllAsync(
        bool onlyFiltered = false,
        bool onlyVisible = false,
        TimeSpan? timeout = null,
        IProgress<ValidationProgress>? progress = null)
    {
        try
        {
            if (_dataGridService == null) return null;
            
            var command = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.UseCases.SearchGrid.ValidateAllCommand.Create(progress);
            
            var result = await _dataGridService.ValidateAllAsync(command.Progress);
            
            if (result.IsSuccess)
            {
                // Note: GetCurrentStateAsync would need to be implemented or use alternative approach
                //_currentState = await GetCurrentStateAsync();
                await RefreshUIAsync();
                OnValidationCompleted(new ValidationCompletedEventArgs(result.Value.ToList()));
                return result.Value.ToList();
            }
            else
            {
                OnErrorOccurred(new ErrorEventArgs($"Validation failed: {result.Error}", result.Exception));
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Validation failed");
            OnErrorOccurred(new ErrorEventArgs($"Validation failed: {ex.Message}", ex));
            return null;
        }
    }
    
    #endregion

    #region UI: Private Methods
    
    private async Task<GridState?> GetCurrentStateAsync()
    {
        try
        {
            // Note: GetCurrentStateAsync method is not available in IDataGridService
            // This is a placeholder method for future implementation
            await Task.Delay(1); // Ensure async behavior
            return _currentState;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get current state");
            return null;
        }
    }
    
    private async Task RefreshUIAsync()
    {
        try
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                // Update UI elements based on current state
                UpdateDataGridDisplay();
                UpdateStatusDisplay();
                UpdateValidationDisplay();
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UI refresh failed");
        }
    }
    
    private void UpdateDataGridDisplay()
    {
        if (_currentState?.Rows == null) return;
        
        // Update the ListView with current data
        // Convert grid rows to display format
        var displayItems = _currentState.Rows
            .Where(r => r.Data.Values.Any(v => v != null && !string.IsNullOrWhiteSpace(v.ToString())))
            .Select(r => FormatRowForDisplay(r))
            .ToList();
        
        // Update ItemsSource - this would be done via dispatcher
        // MainDataView.ItemsSource = displayItems;
    }
    
    private string FormatRowForDisplay(GridRow row)
    {
        // Simple text representation for now
        var values = row.Data.Values.Where(v => v != null).Select(v => v.ToString());
        return string.Join(" | ", values);
    }
    
    private void UpdateStatusDisplay()
    {
        // Update status bar or status display with current counts, etc.
    }
    
    private void UpdateValidationDisplay()
    {
        // Update validation error displays, highlights, etc.
    }
    
    /// <summary>
    /// UI: Update status bar with selection information
    /// SENIOR DESIGN: Centralized status management
    /// </summary>
    private void UpdateSelectionStatus(int selectedCount)
    {
        try
        {
            // UI: Update status bar display with selection count
            _logger?.LogDebug("Selection status updated: {SelectedCount} items selected", selectedCount);
            
            // TODO: Update actual status bar UI elements when available
            // Example: StatusBarSelectionText.Text = $"Selected: {selectedCount}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating selection status");
        }
    }
    
    #endregion

    #region UI: Event Handlers
    
    private void OnDataChanged(DataChangedEventArgs args) => DataChanged?.Invoke(this, args);
    
    private void OnValidationCompleted(ValidationCompletedEventArgs args) => ValidationCompleted?.Invoke(this, args);
    
    private void OnOperationCompleted(OperationCompletedEventArgs args) => OperationCompleted?.Invoke(this, args);
    
    private void OnErrorOccurred(ErrorEventArgs args) => ErrorOccurred?.Invoke(this, args);
    
    private void OnItemClicked(ItemClickedEventArgs args) => ItemClicked?.Invoke(this, args);
    
    private void OnSelectionChanged(DataGridSelectionChangedEventArgs args) => SelectionChanged?.Invoke(this, args);
    
    /// <summary>
    /// XAML EVENT: Handle item click events in the main data view
    /// SENIOR DESIGN: Centralized item interaction handling with error safety
    /// </summary>
    private void MainDataView_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem != null)
            {
                _logger?.LogDebug("Item clicked: {Item}", e.ClickedItem);
                
                // ENTERPRISE: Trigger item click event for external handling
                OnItemClicked(new ItemClickedEventArgs(e.ClickedItem));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling item click");
            OnErrorOccurred(new ErrorEventArgs($"Item click failed: {ex.Message}", ex));
        }
    }
    
    /// <summary>
    /// XAML EVENT: Handle selection changes in the main data view
    /// SENIOR DESIGN: Robust selection management with validation
    /// </summary>
    private void MainDataView_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        try
        {
            var listView = sender as ListView;
            if (listView != null)
            {
                _logger?.LogDebug("Selection changed: {SelectedCount} items", listView.SelectedItems.Count);
                
                // ENTERPRISE: Update internal state
                var selectedItems = listView.SelectedItems.ToList();
                
                // ENTERPRISE: Trigger selection changed event for external handling
                OnSelectionChanged(new DataGridSelectionChangedEventArgs(selectedItems));
                
                // UI: Update status bar with selection count
                UpdateSelectionStatus(selectedItems.Count);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling selection change");
            OnErrorOccurred(new ErrorEventArgs($"Selection change failed: {ex.Message}", ex));
        }
    }
    
    #endregion

    #region UI: Auto Row Height
    
    /// <summary>
    /// CONFIGURATION: Toggle auto row height functionality
    /// </summary>
    public async Task<bool> SetAutoRowHeightEnabledAsync(bool enabled)
    {
        try
        {
            if (_autoRowHeightService != null)
            {
                var result = await _autoRowHeightService.SetEnabledAsync(enabled, refreshExistingRows: true);
                if (result.IsSuccess)
                {
                    _logger?.LogInformation("Auto row height {Status}", enabled ? "enabled" : "disabled");
                    await RefreshUIAsync();
                    return true;
                }
                else
                {
                    OnErrorOccurred(new ErrorEventArgs($"Failed to {(enabled ? "enable" : "disable")} auto row height: {result.Error}"));
                    return false;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling auto row height");
            OnErrorOccurred(new ErrorEventArgs($"Auto row height toggle failed: {ex.Message}", ex));
            return false;
        }
    }
    
    /// <summary>
    /// Get current auto row height enabled state
    /// </summary>
    public bool IsAutoRowHeightEnabled => _autoRowHeightService?.IsEnabled ?? false;
    
    #endregion

    #region UI: Dispose Pattern
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        try
        {
            _dataGridService?.Dispose();
            _autoRowHeightService?.Dispose();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disposing DataGrid services");
        }
        
        _isDisposed = true;
    }
    
    #endregion
}

#region UI: Event Args Classes

public class DataChangedEventArgs : EventArgs
{
    public string Operation { get; }
    public int AffectedRows { get; }
    public DateTime Timestamp { get; }
    
    public DataChangedEventArgs(string operation, int affectedRows)
    {
        Operation = operation;
        AffectedRows = affectedRows;
        Timestamp = DateTime.UtcNow;
    }
}

public class ValidationCompletedEventArgs : EventArgs
{
    public IReadOnlyList<ValidationError> ValidationErrors { get; }
    public DateTime Timestamp { get; }
    
    public ValidationCompletedEventArgs(IReadOnlyList<ValidationError> validationErrors)
    {
        ValidationErrors = validationErrors;
        Timestamp = DateTime.UtcNow;
    }
}

public class OperationCompletedEventArgs : EventArgs
{
    public string Operation { get; }
    public object? Result { get; }
    public DateTime Timestamp { get; }
    
    public OperationCompletedEventArgs(string operation, object? result)
    {
        Operation = operation;
        Result = result;
        Timestamp = DateTime.UtcNow;
    }
}

public class ErrorEventArgs : EventArgs
{
    public string Message { get; }
    public Exception? Exception { get; }
    public DateTime Timestamp { get; }
    
    public ErrorEventArgs(string message, Exception? exception = null)
    {
        Message = message;
        Exception = exception;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// XAML EVENT ARGS: Item click event arguments
/// SENIOR DESIGN: Enterprise-grade event handling with type safety
/// </summary>
public class ItemClickedEventArgs : EventArgs
{
    public object ClickedItem { get; }
    public DateTime Timestamp { get; }
    
    public ItemClickedEventArgs(object clickedItem)
    {
        ClickedItem = clickedItem;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// XAML EVENT ARGS: Selection change event arguments  
/// SENIOR DESIGN: Robust selection management with collection safety
/// </summary>
public class DataGridSelectionChangedEventArgs : EventArgs
{
    public IReadOnlyList<object> SelectedItems { get; }
    public int SelectedCount => SelectedItems.Count;
    public DateTime Timestamp { get; }
    
    public DataGridSelectionChangedEventArgs(IList<object> selectedItems)
    {
        SelectedItems = selectedItems.ToList().AsReadOnly();
        Timestamp = DateTime.UtcNow;
    }
}

#endregion