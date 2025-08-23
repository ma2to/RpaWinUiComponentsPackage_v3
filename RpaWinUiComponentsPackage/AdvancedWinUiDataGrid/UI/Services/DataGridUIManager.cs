using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.UI.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.DataGrid;
using RpaWinUiComponentsPackage.Core.Extensions;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.UI.Services;

/// <summary>
/// DataGrid UI Manager - Bridges domain services to UI
/// REACTIVE: Manages UI state based on domain events
/// SEPARATION: Clean boundary between business logic and UI
/// </summary>
internal sealed class DataGridUIManager : IDisposable
{
    #region Private Fields
    
    private readonly UserControl _userControl;
    private readonly IDataGridService _coreService;
    private readonly DataGridUIContext _uiContext;
    private readonly ILogger? _logger;
    private readonly List<IDisposable> _subscriptions = new();
    private bool _isDisposed = false;
    
    // UI Control References
    private ItemsRepeater? _headersRepeater;
    private ItemsRepeater? _dataRowsRepeater;
    private ProgressRing? _loadingIndicator;
    private TextBlock? _statusText;
    private Border? _statusBar;
    
    #endregion
    
    #region Constructor
    
    public DataGridUIManager(
        UserControl userControl,
        IDataGridService coreService,
        ILogger? logger = null)
    {
        _userControl = userControl ?? throw new ArgumentNullException(nameof(userControl));
        _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
        _logger = logger;
        _uiContext = new DataGridUIContext();
        
        InitializeUIReferences();
        SetupDataBinding();
        SubscribeToServiceEvents();
        
        _logger?.LogDebug("🖥️ UI MANAGER: Initialized for {UserControl}", userControl.GetType().Name);
    }
    
    #endregion
    
    #region Public Properties
    
    public DataGridUIContext UIContext => _uiContext;
    public bool IsInitialized { get; private set; }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initialize UI with column schema
    /// UI: Set up headers and prepare for data
    /// </summary>
    public async Task<bool> InitializeAsync(IReadOnlyList<DomainColumnDefinition> columns)
    {
        try
        {
            _logger?.LogDebug("🖥️ UI INIT: Initializing with {Count} columns", columns.Count);
            
            // UI: Set up headers
            _uiContext.SetHeaders(columns);
            
            // UI: Clear any existing data
            _uiContext.Clear();
            
            // UI: Update status
            _uiContext.StatusText = "Ready";
            _uiContext.IsLoading = false;
            
            IsInitialized = true;
            _logger?.LogDebug("✅ UI INIT: Initialized successfully");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UI INIT ERROR: Initialization failed");
            return false;
        }
    }
    
    /// <summary>
    /// Refresh entire UI from current service state
    /// UI: Complete UI refresh
    /// </summary>
    public async Task RefreshAllAsync()
    {
        try
        {
            _logger?.LogDebug("🖥️ UI REFRESH: Starting full refresh");
            
            ShowLoading("Refreshing...");
            
            // Get current data from service
            var exportResult = await _coreService.ExportToDictionariesAsync();
            if (exportResult.IsSuccess && exportResult.Value != null)
            {
                await UpdateDataDisplayAsync(exportResult.Value);
            }
            
            HideLoading();
            _logger?.LogDebug("✅ UI REFRESH: Completed successfully");
        }
        catch (Exception ex)
        {
            HideLoading();
            _logger?.Error(ex, "🚨 UI REFRESH ERROR: Refresh failed");
            _uiContext.StatusText = $"Refresh failed: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Update UI after data changes
    /// UI: Incremental UI update
    /// </summary>
    public async Task UpdateAfterDataChangeAsync(string reason)
    {
        try
        {
            _logger?.LogDebug("🖥️ UI UPDATE: Updating after {Reason}", reason);
            
            await RefreshAllAsync(); // For now, do full refresh
            _uiContext.StatusText = reason;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UI UPDATE ERROR: Update failed");
        }
    }
    
    /// <summary>
    /// Update validation visuals with provided validation result (LOOP-SAFE: Never calls validation recursively)
    /// UI: Show validation status in cells
    /// </summary>
    public async Task UpdateValidationUIAsync(ValidationResult? validationResult = null)
    {
        try
        {
            _logger?.LogDebug("🖥️ UI VALIDATION: Updating validation visuals (Loop-safe)");
            
            // LOOP PREVENTION: Only update visuals if result is provided, never call validation
            if (validationResult != null)
            {
                UpdateValidationVisuals(validationResult);
                _logger?.LogDebug("✅ UI VALIDATION: Validation visuals updated - Valid: {IsValid}", validationResult.IsValid);
            }
            else
            {
                // SAFE FALLBACK: Just update status without triggering validation
                _uiContext.StatusText = "Validation pending...";
                _logger?.LogDebug("⚠️ UI VALIDATION: No validation result provided, status updated without validation call");
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UI VALIDATION ERROR: Validation update failed");
        }
    }
    
    /// <summary>
    /// Invalidate UI layout
    /// UI: Force layout recalculation
    /// </summary>
    public void InvalidateLayout()
    {
        try
        {
            _userControl.InvalidateMeasure();
            _userControl.InvalidateArrange();
            _userControl.UpdateLayout();
            
            _logger?.LogDebug("✅ UI LAYOUT: Layout invalidated");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 UI LAYOUT ERROR: Layout invalidation failed");
        }
    }
    
    /// <summary>
    /// Get UI-specific metrics
    /// UI: Return actual UI state
    /// </summary>
    public int GetVisibleRowCount() => _uiContext.Rows.Count;
    public int GetLastDataRowIndex() => Math.Max(0, _uiContext.Rows.Count - 1);
    public int GetMinimumRowCount() => 0; // Could be configurable
    
    #endregion
    
    #region Private UI Setup
    
    private void InitializeUIReferences()
    {
        // Find UI controls by name
        _headersRepeater = FindChildByName<ItemsRepeater>("HeadersRepeater");
        _dataRowsRepeater = FindChildByName<ItemsRepeater>("DataRowsRepeater");
        _loadingIndicator = FindChildByName<ProgressRing>("LoadingIndicator");
        _statusText = FindChildByName<TextBlock>("StatusText");
        _statusBar = FindChildByName<Border>("StatusBar");
        
        _logger?.LogDebug("🖥️ UI REFS: Found controls - Headers:{Headers}, Rows:{Rows}, Loading:{Loading}", 
            _headersRepeater != null, _dataRowsRepeater != null, _loadingIndicator != null);
    }
    
    private void SetupDataBinding()
    {
        // TEMPORARY DISABLE: AdvancedDataGrid manages its own data binding
        // Avoid conflict between DataGridUIManager and AdvancedDataGrid collections
        
        // TODO: Implement proper synchronization between DataGridUIManager and AdvancedDataGrid
        // For now, let AdvancedDataGrid handle its own _headers and _dataRows collections
        
        _logger?.LogDebug("🖥️ DATA BINDING: Skipping automatic binding - AdvancedDataGrid manages collections");
        
        if (_statusText != null)
        {
            // Bind status text
            _uiContext.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DataGridUIContext.StatusText))
                {
                    _userControl.DispatcherQueue.TryEnqueue(() =>
                    {
                        _statusText.Text = _uiContext.StatusText;
                    });
                }
            };
        }
        
        _logger?.LogDebug("✅ UI BINDING: Data binding configured");
    }
    
    private void SubscribeToServiceEvents()
    {
        // REACTIVE: Subscribe to core service events
        var dataChanges = _coreService.DataChanges
            .Subscribe(async change =>
            {
                // Marshal to UI thread
                _userControl.DispatcherQueue.TryEnqueue(async () =>
                {
                    await UpdateAfterDataChangeAsync($"Data {change.ChangeType}");
                });
            });
        _subscriptions.Add(dataChanges);
        
        var validationChanges = _coreService.ValidationChanges
            .Subscribe(async change =>
            {
                // Marshal to UI thread - PASS validation result to prevent recursive calls
                _userControl.DispatcherQueue.TryEnqueue(async () =>
                {
                    // Don't call validation again, just update UI with existing state
                    _logger?.LogDebug("🖥️ VALIDATION EVENT: Validation change detected, updating status only");
                    _uiContext.StatusText = "Validation updated";
                });
            });
        _subscriptions.Add(validationChanges);
        
        _logger?.LogDebug("✅ UI EVENTS: Subscribed to service events");
    }
    
    #endregion
    
    #region Private UI Operations
    
    private async Task UpdateDataDisplayAsync(IReadOnlyList<IReadOnlyDictionary<string, object?>> data)
    {
        // Convert dictionary data back to domain rows for UI display
        var domainRows = data.Select((dict, index) => new DataRow(
            dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            index)).ToArray();
        
        _uiContext.SetRows(domainRows);
    }
    
    private void UpdateValidationVisuals(ValidationResult validationResult)
    {
        // Update overall validation status
        _uiContext.StatusText = validationResult.IsValid 
            ? $"All {validationResult.ValidRows} rows valid" 
            : $"{validationResult.InvalidRows} rows have validation errors";
        
        // For now, mark all rows as valid/invalid based on overall result
        // More granular row-level validation could be added later
        foreach (var uiRow in _uiContext.Rows)
        {
            var allValid = Enumerable.Repeat(validationResult.IsValid, _uiContext.Headers.Count).ToArray();
            uiRow.UpdateValidation(allValid);
        }
    }
    
    private void ShowLoading(string message)
    {
        _uiContext.IsLoading = true;
        _uiContext.StatusText = message;
        
        if (_loadingIndicator != null)
        {
            _userControl.DispatcherQueue.TryEnqueue(() =>
            {
                _loadingIndicator.IsActive = true;
                _loadingIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            });
        }
    }
    
    private void HideLoading()
    {
        _uiContext.IsLoading = false;
        
        if (_loadingIndicator != null)
        {
            _userControl.DispatcherQueue.TryEnqueue(() =>
            {
                _loadingIndicator.IsActive = false;
                _loadingIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
        }
    }
    
    private T? FindChildByName<T>(string name) where T : class
    {
        try
        {
            return _userControl.FindName(name) as T;
        }
        catch
        {
            return null;
        }
    }
    
    #endregion
    
    #region IDisposable
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();
        
        _isDisposed = true;
        _logger?.LogDebug("🧹 UI MANAGER: Disposed");
    }
    
    #endregion
}