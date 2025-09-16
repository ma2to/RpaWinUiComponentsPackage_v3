using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core.ColumnDefinition;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Events;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Managers;

/// <summary>
/// UI: Professional UI Data Manager
/// CLEAN ARCHITECTURE: UI layer - Manages DataGrid data binding and UI state
/// SOLID: Single responsibility for UI data coordination
/// SENIOR DESIGN: Enterprise-grade UI data management with MVVM patterns
/// </summary>
internal sealed class DataGridUIDataManager : IDisposable
{
    #region Private Fields

    private readonly ILogger? _logger;
    private object? _dataGrid; // Using object to avoid WinUI 3 DataGrid dependency
    private ObservableCollection<object>? _itemsSource;
    private IReadOnlyList<GridColumnDefinition>? _columns;
    private bool _disposed;

    // UI State
    private readonly Dictionary<string, object?> _uiState = new();
    private readonly object _dataLock = new object();

    #endregion

    #region Events

    /// <summary>
    /// EVENT: UI data changed
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? UIDataChanged;

    /// <summary>
    /// EVENT: UI state changed
    /// </summary>
    public event EventHandler<UIStateChangedEventArgs>? UIStateChanged;

    #endregion

    #region Constructor

    /// <summary>
    /// CONSTRUCTOR: Initialize UI data manager
    /// </summary>
    public DataGridUIDataManager(ILogger? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("[UI-DATA] DataGridUIDataManager initialized");
    }

    #endregion

    #region UI Data Management

    /// <summary>
    /// ENTERPRISE: Initialize UI data binding
    /// </summary>
    public async Task<bool> InitializeAsync(object dataGrid, IReadOnlyList<GridColumnDefinition> columns)
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Initializing UI data binding");

            _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));

            // Initialize data source
            _itemsSource = new ObservableCollection<object>();
            if (_dataGrid is ListView listView)
            {
                listView.ItemsSource = _itemsSource;
            }

            // Setup columns
            await SetupDataGridColumnsAsync();

            // Initialize UI state
            InitializeUIState();

            _logger?.LogInformation("[UI-DATA] UI data binding initialized successfully");
            OnUIStateChanged("Initialized", true);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to initialize UI data binding");
            OnUIStateChanged("Initialized", false);
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Update data source with new data
    /// </summary>
    public async Task<bool> UpdateDataSourceAsync(IEnumerable<Dictionary<string, object?>> data)
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Updating data source");

            if (_itemsSource == null)
            {
                _logger?.LogError("[UI-DATA] Data source not initialized");
                return false;
            }

            lock (_dataLock)
            {
                _itemsSource.Clear();

                foreach (var row in data)
                {
                    var dataItem = CreateDataItem(row);
                    _itemsSource.Add(dataItem);
                }
            }

            _logger?.LogInformation("[UI-DATA] Data source updated with {Count} items", _itemsSource.Count);
            OnUIStateChanged("DataSourceUpdated", true);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to update data source");
            OnUIStateChanged("DataSourceUpdated", false);
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Add new row to UI data
    /// </summary>
    public async Task<bool> AddRowAsync(Dictionary<string, object?> rowData, int? insertIndex = null)
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Adding new row at index {Index}", insertIndex);

            if (_itemsSource == null)
            {
                _logger?.LogError("[UI-DATA] Data source not initialized");
                return false;
            }

            var dataItem = CreateDataItem(rowData);

            lock (_dataLock)
            {
                if (insertIndex.HasValue && insertIndex.Value >= 0 && insertIndex.Value <= _itemsSource.Count)
                {
                    _itemsSource.Insert(insertIndex.Value, dataItem);
                }
                else
                {
                    _itemsSource.Add(dataItem);
                }
            }

            _logger?.LogInformation("[UI-DATA] Row added successfully");
            OnUIDataChanged("RowAdded", insertIndex ?? _itemsSource.Count - 1, null, dataItem);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to add row");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Update existing row in UI data
    /// </summary>
    public async Task<bool> UpdateRowAsync(int rowIndex, Dictionary<string, object?> newData)
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Updating row at index {RowIndex}", rowIndex);

            if (_itemsSource == null || rowIndex < 0 || rowIndex >= _itemsSource.Count)
            {
                _logger?.LogError("[UI-DATA] Invalid row index or data source not initialized");
                return false;
            }

            var oldItem = _itemsSource[rowIndex];
            var newItem = CreateDataItem(newData);

            lock (_dataLock)
            {
                _itemsSource[rowIndex] = newItem;
            }

            _logger?.LogInformation("[UI-DATA] Row updated successfully");
            OnUIDataChanged("RowUpdated", rowIndex, oldItem, newItem);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to update row");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Remove row from UI data
    /// </summary>
    public async Task<bool> RemoveRowAsync(int rowIndex)
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Removing row at index {RowIndex}", rowIndex);

            if (_itemsSource == null || rowIndex < 0 || rowIndex >= _itemsSource.Count)
            {
                _logger?.LogError("[UI-DATA] Invalid row index or data source not initialized");
                return false;
            }

            var removedItem = _itemsSource[rowIndex];

            lock (_dataLock)
            {
                _itemsSource.RemoveAt(rowIndex);
            }

            _logger?.LogInformation("[UI-DATA] Row removed successfully");
            OnUIDataChanged("RowRemoved", rowIndex, removedItem, null);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to remove row");
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Clear all UI data
    /// </summary>
    public async Task<bool> ClearDataAsync()
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Clearing all data");

            if (_itemsSource == null)
            {
                _logger?.LogError("[UI-DATA] Data source not initialized");
                return false;
            }

            var itemCount = _itemsSource.Count;

            lock (_dataLock)
            {
                _itemsSource.Clear();
            }

            _logger?.LogInformation("[UI-DATA] Cleared {Count} items", itemCount);
            OnUIStateChanged("DataCleared", true);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to clear data");
            OnUIStateChanged("DataCleared", false);
            return false;
        }
    }

    /// <summary>
    /// ENTERPRISE: Refresh display data
    /// </summary>
    public async Task RefreshDisplayDataAsync()
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Refreshing display data");

            if (_dataGrid == null || _itemsSource == null)
            {
                _logger?.LogError("[UI-DATA] Cannot refresh - data source or grid not initialized");
                return;
            }

            // Trigger UI refresh by notifying collection changed
            lock (_dataLock)
            {
                // Force refresh the ItemsSource
                if (_dataGrid is ListView listView)
                {
                    var currentSource = _itemsSource;
                    listView.ItemsSource = null;
                    listView.ItemsSource = currentSource;
                }
            }

            _logger?.LogInformation("[UI-DATA] Display data refreshed successfully");
            OnUIStateChanged("DataRefreshed", true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to refresh display data");
            OnUIStateChanged("DataRefreshed", false);
        }
    }

    /// <summary>
    /// ENTERPRISE: Update data source with new data
    /// </summary>
    public async Task UpdateDataSourceAsync(List<Dictionary<string, object?>> data)
    {
        try
        {
            _logger?.LogInformation("[UI-DATA] Updating data source with {Count} items", data.Count);

            if (_itemsSource == null)
            {
                _logger?.LogError("[UI-DATA] Data source not initialized");
                return;
            }

            lock (_dataLock)
            {
                _itemsSource.Clear();
                foreach (var item in data)
                {
                    _itemsSource.Add(item);
                }
            }

            _logger?.LogInformation("[UI-DATA] Data source updated successfully");
            OnUIDataChanged("DataSourceUpdated", -1, null, data);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to update data source");
        }
    }

    #endregion

    #region UI State Management

    /// <summary>
    /// ENTERPRISE: Get current UI state
    /// </summary>
    public Dictionary<string, object?> GetCurrentUIState()
    {
        lock (_dataLock)
        {
            var state = new Dictionary<string, object?>(_uiState)
            {
                ["ItemCount"] = _itemsSource?.Count ?? 0,
                ["ColumnCount"] = _columns?.Count ?? 0,
                ["IsInitialized"] = _dataGrid != null && _itemsSource != null,
                ["LastUpdated"] = DateTime.UtcNow
            };

            return state;
        }
    }

    /// <summary>
    /// ENTERPRISE: Update UI state
    /// </summary>
    public void UpdateUIState(string key, object? value)
    {
        try
        {
            lock (_dataLock)
            {
                var oldValue = _uiState.GetValueOrDefault(key);
                _uiState[key] = value;

                _logger?.LogDebug("[UI-DATA] UI state updated: {Key} = {Value}", key, value);
                OnUIStateChanged(key, true, oldValue, value);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to update UI state for key: {Key}", key);
        }
    }

    /// <summary>
    /// ENTERPRISE: Get selected items from DataGrid
    /// </summary>
    public IReadOnlyList<object> GetSelectedItems()
    {
        try
        {
            if (_dataGrid is ListView listView && listView.SelectedItems != null)
            {
                var selectedItems = new List<object>();
                foreach (var item in listView.SelectedItems)
                {
                    selectedItems.Add(item);
                }
                return selectedItems.AsReadOnly();
            }

            return Array.Empty<object>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to get selected items");
            return Array.Empty<object>();
        }
    }

    /// <summary>
    /// ENTERPRISE: Get selected row indices
    /// </summary>
    public IReadOnlyList<int> GetSelectedRowIndices()
    {
        try
        {
            var selectedIndices = new List<int>();
            var selectedItems = GetSelectedItems();

            if (_itemsSource != null)
            {
                foreach (var item in selectedItems)
                {
                    var index = _itemsSource.IndexOf(item);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }

            return selectedIndices.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to get selected row indices");
            return Array.Empty<int>();
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// HELPER: Setup DataGrid columns
    /// </summary>
    private async Task SetupDataGridColumnsAsync()
    {
        try
        {
            if (_dataGrid == null || _columns == null)
                return;

            // Note: For ListView-based implementation, columns are managed through templates
            // This method is kept for interface compatibility
            await Task.CompletedTask;

            await Task.CompletedTask;
            _logger?.LogDebug("[UI-DATA] DataGrid columns setup completed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Failed to setup DataGrid columns");
            throw;
        }
    }

    /// <summary>
    /// HELPER: This method is no longer needed for ListView implementation
    /// </summary>
    private void CreateDataGridColumn_Deprecated()
    {
        // This method is deprecated - columns are now handled via templates
    }

    /// <summary>
    /// HELPER: Create data item from dictionary
    /// </summary>
    private object CreateDataItem(Dictionary<string, object?> rowData)
    {
        // In real implementation, this would create a proper data object
        // For now, return the dictionary itself
        return rowData;
    }

    /// <summary>
    /// HELPER: Initialize UI state
    /// </summary>
    private void InitializeUIState()
    {
        lock (_dataLock)
        {
            _uiState.Clear();
            _uiState["InitializedAt"] = DateTime.UtcNow;
            _uiState["Version"] = "1.0";
        }
    }

    #endregion

    #region Event Helpers

    /// <summary>
    /// HELPER: Fire UI data changed event
    /// </summary>
    private void OnUIDataChanged(string operation, int rowIndex, object? oldValue, object? newValue)
    {
        try
        {
            var args = new DataChangedEventArgs(null, rowIndex, oldValue, newValue);
            UIDataChanged?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Error firing UIDataChanged event");
        }
    }

    /// <summary>
    /// HELPER: Fire UI state changed event
    /// </summary>
    private void OnUIStateChanged(string operation, bool success, object? oldValue = null, object? newValue = null)
    {
        try
        {
            var args = new UIStateChangedEventArgs(operation, success, oldValue, newValue);
            UIStateChanged?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Error firing UIStateChanged event");
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// CLEANUP: Dispose of managed resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            lock (_dataLock)
            {
                _itemsSource?.Clear();
                _itemsSource = null;
                _uiState.Clear();
            }

            if (_dataGrid is ListView listView)
            {
                listView.ItemsSource = null;
                _dataGrid = null;
            }

            _logger?.LogInformation("[UI-DATA] DataGridUIDataManager disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DATA] Error during disposal");
        }

        _disposed = true;
    }

    #endregion
}

/// <summary>
/// EVENT: UI state changed event arguments
/// </summary>
internal sealed class UIStateChangedEventArgs : EventArgs
{
    public string Operation { get; }
    public bool Success { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public UIStateChangedEventArgs(string operation, bool success, object? oldValue = null, object? newValue = null)
    {
        Operation = operation;
        Success = success;
        OldValue = oldValue;
        NewValue = newValue;
    }
}