using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using GridColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core.ColumnDefinition;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Components;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Managers;
using UIErrorEventArgs = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Events.ErrorEventArgs;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Events;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Components;

/// <summary>
/// UI: DataGrid business logic component wrapper (REFACTORED)
/// CLEAN ARCHITECTURE: UI layer - Uses manager pattern for focused responsibilities
/// RESPONSIBILITY: Coordinate managers and expose clean API
/// SENIOR DESIGN: Separated into focused managers maintaining clean architecture
/// EXTRACTED: God-level component broken into specialized managers
/// </summary>
internal sealed class DataGridComponentLogic : IDisposable
{
    #region Private Fields - Managers and Dependencies

    private readonly AdvancedDataGridComponent _xamlComponent;
    private readonly ILogger? _logger;

    // SENIOR PATTERN: Specialized managers for focused responsibilities
    private DataGridBusinessLogicManager? _businessLogicManager;
    private DataGridEventManager? _eventManager;
    private DataGridUIDataManager? _uiDataManager;

    // Services
    private IDataGridService? _dataGridService;
    private IDataGridAutoRowHeightService? _autoRowHeightService;

    private bool _isDisposed;

    #endregion

    #region Constructor and Initialization

    /// <summary>
    /// CONSTRUCTOR: Initialize with XAML component
    /// SENIOR PATTERN: Minimal constructor, managers initialized on-demand
    /// </summary>
    public DataGridComponentLogic(AdvancedDataGridComponent xamlComponent)
    {
        _xamlComponent = xamlComponent ?? throw new ArgumentNullException(nameof(xamlComponent));
        _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DataGridComponentLogic>();

        InitializeManagers();
        _logger?.LogInformation("[COMPONENT-LOGIC] DataGridComponentLogic initialized with manager pattern");
    }

    /// <summary>
    /// INITIALIZATION: Initialize all managers
    /// SOLID: Each manager has single responsibility
    /// </summary>
    private void InitializeManagers()
    {
        try
        {
            // Initialize services (these would typically come from DI container)
            InitializeServices();

            // Initialize managers
            _businessLogicManager = new DataGridBusinessLogicManager(_logger);
            _eventManager = new DataGridEventManager(_logger);
            _uiDataManager = new DataGridUIDataManager(_logger);

            // Wire up event handling
            WireUpEventHandling();

            _logger?.LogInformation("[COMPONENT-LOGIC] All managers initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT-LOGIC] Failed to initialize managers: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// SERVICES: Initialize required services
    /// NOTE: In production, these would come from DI container
    /// </summary>
    private void InitializeServices()
    {
        // This is a simplified initialization
        // In production, these would be injected via DI container
        _dataGridService = new DataGridUnifiedService();

        // Initialize dependencies for DataGridAutoRowHeightService
        // TODO: These should be injected via DI container in production
        var rowHeightService = new RowHeightCalculationService(_logger as ILogger<RowHeightCalculationService> ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RowHeightCalculationService>.Instance);
        var uiService = new DataGridUIService(_logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        var uiConfiguration = new UIConfiguration();

        _autoRowHeightService = new DataGridAutoRowHeightService(
            rowHeightService,
            uiService,
            uiConfiguration,
            _logger as ILogger<DataGridAutoRowHeightService> ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<DataGridAutoRowHeightService>.Instance);
    }

    /// <summary>
    /// EVENTS: Wire up event handling between managers
    /// </summary>
    private void WireUpEventHandling()
    {
        if (_eventManager == null) return;

        // Subscribe to business events to update UI
        _eventManager.DataChanged += PublicOnDataChanged;
        _eventManager.OperationCompleted += PublicOnOperationCompleted;
        _eventManager.ErrorOccurred += PublicOnErrorOccurred;
    }

    /// <summary>
    /// Handle data changed event from event manager
    /// </summary>
    private void PublicOnDataChanged(object? sender, DataChangedEventArgs e)
    {
        try
        {
            _logger?.LogInformation("[COMPONENT] Data changed event received for row {RowIndex}", e.RowIndex);
            // Forward to public API or handle business logic
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT] Error handling data changed event");
        }
    }

    /// <summary>
    /// Handle operation completed event from event manager
    /// </summary>
    private void PublicOnOperationCompleted(object? sender, OperationCompletedEventArgs e)
    {
        try
        {
            _logger?.LogInformation("[COMPONENT] Operation completed: {Operation}", e.OperationType);
            // Forward to public API or handle business logic
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT] Error handling operation completed event");
        }
    }

    /// <summary>
    /// Handle error occurred event from event manager
    /// </summary>
    private void PublicOnErrorOccurred(object? sender, UIErrorEventArgs e)
    {
        try
        {
            _logger?.LogError("[COMPONENT] Error occurred: {Message}", e.ErrorMessage);
            // Forward to public API or handle business logic
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT] Error handling error occurred event");
        }
    }

    #endregion

    #region PUBLIC API: Main business operations (delegated to managers)

    /// <summary>
    /// API: Initialize grid (delegated to business logic manager)
    /// </summary>
    public async Task<bool> InitializeAsync(
        IReadOnlyList<GridColumnDefinition> columns,
        List<Dictionary<string, object?>>? initialData = null,
        DataGridConfiguration? configuration = null)
    {
        try
        {
            _logger?.LogInformation("[COMPONENT-API] Delegating initialization to business logic manager");

            var colorConfig = configuration?.UI?.Colors ?? RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI.ColorConfiguration.Default;
            var result = await _businessLogicManager!.InitializeAsync(_dataGridService!, columns, colorConfig);

            if (result)
            {
                await _uiDataManager!.RefreshDisplayDataAsync();
                _eventManager?.FireOperationCompleted("Initialize", true, "Initialization completed successfully");
                _eventManager?.FireDataChanged(null, -1, null, initialData?.Count ?? 0);
            }
            else
            {
                _eventManager?.PublicOnErrorOccurred("Initialization failed");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT-API] Initialize failed: {ErrorMessage}", ex.Message);
            _eventManager?.PublicOnErrorOccurred($"Initialize failed: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// API: Initialize with sample data (delegated to business logic manager)
    /// </summary>
    public async Task<bool> InitializeWithSampleDataAsync()
    {
        try
        {
            var result = await _businessLogicManager!.InitializeWithSampleDataAsync();

            if (result.IsSuccess)
            {
                await _uiDataManager!.RefreshDisplayDataAsync();
                _eventManager?.PublicOnOperationCompleted("InitializeWithSampleData", result.Value);
                _eventManager?.PublicOnDataChanged("InitializeWithSampleData", 3); // Sample data count
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _eventManager?.PublicOnErrorOccurred($"Sample data initialization failed: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// API: Import from dictionary (delegated to business logic manager)
    /// </summary>
    public async Task<bool> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        IReadOnlyList<GridColumnDefinition>? columns = null)
    {
        try
        {
            var result = await _businessLogicManager!.ImportFromDictionaryAsync(data, columns);

            if (result.IsSuccess)
            {
                await _uiDataManager!.UpdateDataSourceAsync(data);
                _eventManager?.PublicOnOperationCompleted("ImportFromDictionary", result.Value);
                _eventManager?.PublicOnDataChanged("Import", data.Count);
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _eventManager?.PublicOnErrorOccurred($"Dictionary import failed: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// API: Export to dictionary (delegated to business logic manager)
    /// </summary>
    public async Task<List<Dictionary<string, object?>>?> ExportToDictionaryAsync()
    {
        try
        {
            var result = await _businessLogicManager!.ExportToDictionaryAsync();

            if (result.IsSuccess)
            {
                _eventManager?.PublicOnOperationCompleted("ExportToDictionary", result.Value);
                return result.Value;
            }

            return null;
        }
        catch (Exception ex)
        {
            _eventManager?.PublicOnErrorOccurred($"Dictionary export failed: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// API: Search grid (delegated to business logic manager)
    /// </summary>
    public async Task<List<SearchResult>?> SearchAsync(
        string searchTerm,
        object? searchConfig = null)
    {
        try
        {
            var result = await _businessLogicManager!.SearchAsync(searchTerm, searchConfig);

            if (result.IsSuccess)
            {
                _eventManager?.PublicOnOperationCompleted("Search", result.Value);
                return new List<SearchResult> { result.Value! };
            }

            return null;
        }
        catch (Exception ex)
        {
            _eventManager?.PublicOnErrorOccurred($"Search failed: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// API: Apply filters (delegated to business logic manager)
    /// </summary>
    public async Task<bool> ApplyFiltersAsync(object filters)
    {
        try
        {
            var result = await _businessLogicManager!.ApplyFiltersAsync(filters);

            if (result.IsSuccess)
            {
                await _uiDataManager!.RefreshDisplayDataAsync();
                _eventManager?.PublicOnOperationCompleted("ApplyFilters", result.Value);
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _eventManager?.PublicOnErrorOccurred($"Apply filters failed: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// API: Validate all data (delegated to business logic manager)
    /// </summary>
    public async Task<List<object>?> ValidateAllAsync()
    {
        try
        {
            var result = await _businessLogicManager!.ValidateAllWrapperAsync();

            if (result.IsSuccess)
            {
                await _uiDataManager!.RefreshDisplayDataAsync(); // Refresh to show validation errors
                _eventManager?.PublicOnOperationCompleted("ValidateAll", result.Value);
                return result.Value;
            }

            return null;
        }
        catch (Exception ex)
        {
            _eventManager?.PublicOnErrorOccurred($"Validation failed: {ex.Message}", ex);
            return null;
        }
    }

    #endregion

    #region UI EVENT HANDLERS: XAML event delegation to event manager

    /// <summary>
    /// UI EVENT: Handle item click (delegated to event manager)
    /// </summary>
    public void HandleItemClick(object sender, ItemClickEventArgs e)
    {
        _eventManager?.HandleItemClick(sender, e);
    }

    /// <summary>
    /// UI EVENT: Handle selection changed (delegated to event manager)
    /// </summary>
    public void HandleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _eventManager?.HandleSelectionChanged(sender, e);
    }

    /// <summary>
    /// UI EVENT: Handle add row (delegated to event manager)
    /// </summary>
    public void HandleAddRow()
    {
        _eventManager?.HandleAddRow(this, EventArgs.Empty);
        // Trigger actual business operation
        Task.Run(async () =>
        {
            var result = await _businessLogicManager!.AddRowAsync();
            if (result.IsSuccess)
            {
                await _uiDataManager!.RefreshDisplayDataAsync();
                _eventManager?.PublicOnDataChanged("AddRow", 1);
            }
        });
    }

    /// <summary>
    /// UI EVENT: Handle delete row (delegated to event manager)
    /// </summary>
    public void HandleDeleteRow()
    {
        _eventManager?.HandleDeleteRow(this, EventArgs.Empty);
        // Trigger actual business operation (would need selected row index)
        Task.Run(async () =>
        {
            // This is simplified - in reality we'd get the selected row index
            var result = await _businessLogicManager!.DeleteRowAsync(0);
            if (result.IsSuccess)
            {
                await _uiDataManager!.RefreshDisplayDataAsync();
                _eventManager?.PublicOnDataChanged("DeleteRow", 1);
            }
        });
    }

    /// <summary>
    /// UI EVENT: Handle validate (delegated to event manager)
    /// </summary>
    public void HandleValidate()
    {
        _eventManager?.HandleValidate(this, EventArgs.Empty);
        Task.Run(async () => await ValidateAllAsync());
    }

    /// <summary>
    /// UI EVENT: Handle clear filters (delegated to event manager)
    /// </summary>
    public void HandleClearFilters()
    {
        _eventManager?.HandleClearFilters(this, EventArgs.Empty);
        Task.Run(async () =>
        {
            var result = await _businessLogicManager!.ClearFiltersAsync();
            if (result.IsSuccess)
            {
                await _uiDataManager!.RefreshDisplayDataAsync();
            }
        });
    }

    /// <summary>
    /// UI EVENT: Handle search (delegated to event manager)
    /// </summary>
    public void HandleSearch()
    {
        _eventManager?.HandleSearch(this, EventArgs.Empty);
        // Would get search term from UI and trigger search
    }

    #endregion

    #region EVENT HANDLING: Manager event handlers

    /// <summary>
    /// EVENT: Handle data changed events
    /// </summary>
    private void OnDataChanged(object? sender, DataChangedEventArgs e)
    {
        _logger?.LogInformation("[COMPONENT-LOGIC] Data changed - Operation: {Operation}, Rows: {Rows}",
            e.Operation, e.AffectedRows);
    }

    /// <summary>
    /// EVENT: Handle operation completed events
    /// </summary>
    private void OnOperationCompleted(object? sender, OperationCompletedEventArgs e)
    {
        _logger?.LogInformation("[COMPONENT-LOGIC] Operation completed - Operation: {Operation}",
            e.OperationType);
    }

    /// <summary>
    /// EVENT: Handle error events
    /// </summary>
    private void OnErrorOccurred(object? sender, UIErrorEventArgs e)
    {
        _logger?.LogError("[COMPONENT-LOGIC] Error occurred - Message: {Message}",
            e.ErrorMessage);
    }

    #endregion

    #region PUBLIC EVENTS: Expose events for external consumption

    /// <summary>
    /// PUBLIC EVENT: Data changed
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? DataChanged
    {
        add => _eventManager!.DataChanged += value;
        remove => _eventManager!.DataChanged -= value;
    }

    /// <summary>
    /// PUBLIC EVENT: Operation completed
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs>? OperationCompleted
    {
        add => _eventManager!.OperationCompleted += value;
        remove => _eventManager!.OperationCompleted -= value;
    }

    /// <summary>
    /// PUBLIC EVENT: Error occurred
    /// </summary>
    public event EventHandler<UIErrorEventArgs>? ErrorOccurred
    {
        add => _eventManager!.ErrorOccurred += value;
        remove => _eventManager!.ErrorOccurred -= value;
    }

    /// <summary>
    /// PUBLIC EVENT: Item clicked
    /// </summary>
    public event EventHandler<ItemClickedEventArgs>? ItemClicked
    {
        add => _eventManager!.ItemClicked += value;
        remove => _eventManager!.ItemClicked -= value;
    }

    /// <summary>
    /// PUBLIC EVENT: Selection changed
    /// </summary>
    public event EventHandler<DataGridSelectionChangedEventArgs>? SelectionChanged
    {
        add => _eventManager!.SelectionChanged += value;
        remove => _eventManager!.SelectionChanged -= value;
    }

    #endregion

    #region AUTO ROW HEIGHT: Height management (delegated to service)

    /// <summary>
    /// API: Set auto row height enabled
    /// </summary>
    public async Task<bool> SetAutoRowHeightEnabledAsync(bool enabled)
    {
        try
        {
            if (_autoRowHeightService == null)
            {
                _logger?.LogWarning("[COMPONENT-API] Auto row height service not available");
                return false;
            }

            var result = await _autoRowHeightService.SetEnabledAsync(enabled);
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT-API] Set auto row height failed: {ErrorMessage}", ex.Message);
            _eventManager?.PublicOnErrorOccurred($"Set auto row height failed: {ex.Message}", ex);
            return false;
        }
    }

    #endregion

    #region STATE ACCESS: Current state access

    /// <summary>
    /// API: Get current state
    /// </summary>
    public GridState? CurrentState => _businessLogicManager?.CurrentState;

    #endregion

    #region CLEANUP: Disposal pattern

    /// <summary>
    /// CLEANUP: Dispose of all managers and resources
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        try
        {
            // Unsubscribe from events
            if (_eventManager != null)
            {
                _eventManager.DataChanged -= PublicOnDataChanged;
                _eventManager.OperationCompleted -= PublicOnOperationCompleted;
                _eventManager.ErrorOccurred -= PublicOnErrorOccurred;
                _eventManager.ClearAllSubscriptions();
            }

            // Dispose managers
            _businessLogicManager?.Dispose();
            _uiDataManager?.Dispose();

            // Dispose services
            _dataGridService?.Dispose();
            _autoRowHeightService?.Dispose();

            _logger?.LogInformation("[COMPONENT-LOGIC] DataGridComponentLogic disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[COMPONENT-LOGIC] Error during disposal: {ErrorMessage}", ex.Message);
        }

        _isDisposed = true;
    }

    #endregion
}