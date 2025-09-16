using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Managers;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Components;

/// <summary>
/// UI: XAML code-behind for AdvancedDataGridComponent (REFACTORED)
/// SENIOR DESIGN: Minimal XAML code-behind using manager pattern
/// CLEAN ARCHITECTURE: UI layer - delegates to specialized managers
/// SOLID: Single responsibility - only XAML coordination
/// </summary>
internal sealed partial class AdvancedDataGridComponent : UserControl
{
    #region UI: Private Fields - Managers

    private DataGridComponentLogic? _logicComponent;
    private ILogger? _logger;
    private ColorConfiguration? _colorConfiguration;

    // SENIOR PATTERN: Specialized managers for focused responsibilities
    private DataGridUIElementManager? _uiElementManager;
    private DataGridUIFactory? _uiFactory;
    private DataGridCellEditManager? _cellEditManager;
    private DataGridTemplateManager? _templateManager;

    // Data properties
    private IReadOnlyList<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core.ColumnDefinition>? _columns;

    #endregion

    #region Properties

    /// <summary>
    /// Column definitions for the DataGrid
    /// </summary>
    internal IReadOnlyList<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core.ColumnDefinition>? Columns
    {
        get => _columns;
        set => _columns = value;
    }

    #endregion

    #region UI: Constructor and Initialization

    /// <summary>
    /// XAML CONSTRUCTOR: Initialize XAML component with manager pattern
    /// SENIOR DESIGN: Minimal constructor with delegation to managers
    /// </summary>
    public AdvancedDataGridComponent()
    {
        InitializeLogger();
        _logger?.LogInformation("[UI-INIT] AdvancedDataGridComponent constructor started");

        try
        {
            InitializeComponent();
            _logger?.LogInformation("[UI-INIT] XAML InitializeComponent succeeded");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-INIT] XAML InitializeComponent failed, attempting programmatic creation");
            InitializeProgrammatically();
        }

        InitializeManagers();
        _logger?.LogInformation("[UI-INIT] AdvancedDataGridComponent constructor completed");
    }

    /// <summary>
    /// SENIOR PATTERN: Initialize specialized managers
    /// SOLID: Each manager has single responsibility
    /// </summary>
    private void InitializeManagers()
    {
        try
        {
            _uiElementManager = new DataGridUIElementManager(this, _logger);
            _uiFactory = new DataGridUIFactory(_logger, _colorConfiguration);
            _cellEditManager = new DataGridCellEditManager(_logger);
            _templateManager = new DataGridTemplateManager(_logger);

            _logger?.LogInformation("[UI-INIT] All managers initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-INIT] Failed to initialize managers: {ErrorMessage}", ex.Message);
        }
    }

    /// <summary>
    /// SENIOR LOGGING: Initialize logger with fallback
    /// </summary>
    private void InitializeLogger()
    {
        try
        {
            _logger = new NullLogger<AdvancedDataGridComponent>();
        }
        catch
        {
            _logger = null;
        }
    }

    /// <summary>
    /// SENIOR FALLBACK: Create UI programmatically when XAML fails
    /// DELEGATION: Uses UIFactory manager for actual creation
    /// </summary>
    private void InitializeProgrammatically()
    {
        try
        {
            _logger?.LogInformation("[UI-PROGRAMMATIC] Starting programmatic UI creation");

            var mainGrid = _uiFactory?.CreateMainGrid();
            if (mainGrid == null)
            {
                _logger?.LogError("[UI-PROGRAMMATIC] Failed to create main grid");
                return;
            }

            _uiFactory.CreateToolbar(mainGrid);
            _uiFactory.CreateMainDataView(mainGrid);
            _uiFactory.CreateStatusBar(mainGrid);
            _uiFactory.CreateValidationPanel(mainGrid);
            _uiFactory.CreateLoadingOverlay(mainGrid);

            this.Content = mainGrid;
            _logger?.LogInformation("[UI-PROGRAMMATIC] Programmatic UI creation completed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-PROGRAMMATIC] Failed to create programmatic UI: {ErrorMessage}", ex.Message);
        }
    }

    #endregion

    #region UI: Public API - Clean Interface

    /// <summary>
    /// SENIOR DESIGN: Expose business logic component for external access
    /// CLEAN ARCHITECTURE: Facade pattern for accessing functionality
    /// </summary>
    public DataGridComponentLogic Logic => _logicComponent ?? throw new InvalidOperationException("Logic component not initialized");

    /// <summary>
    /// PUBLIC API: Apply color configuration
    /// DELEGATION: Passes configuration to all managers
    /// </summary>
    public void ApplyColorConfiguration(ColorConfiguration colorConfiguration)
    {
        _colorConfiguration = colorConfiguration;

        // Update all managers with new configuration
        _cellEditManager = new DataGridCellEditManager(_logger);
        _templateManager = new DataGridTemplateManager(_logger);

        _logger?.LogInformation("[UI-CONFIG] Color configuration applied to all managers");
    }

    /// <summary>
    /// ENTERPRISE: Initialize component with sample data
    /// </summary>
    public async Task<bool> InitializeWithSampleDataAsync()
    {
        try
        {
            _logger?.LogInformation("[UI-COMPONENT] Initializing with sample data");

            if (_logicComponent == null)
            {
                _logger?.LogError("[UI-COMPONENT] Logic component not initialized");
                return false;
            }

            var result = await _logicComponent.InitializeWithSampleDataAsync();
            _logger?.LogInformation("[UI-COMPONENT] Sample data initialization result: {Result}", result);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-COMPONENT] Sample data initialization failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    #endregion

    #region UI: Element Access - Delegation to UIElementManager

    /// <summary>
    /// UI ACCESS: Get main data view (delegated to manager)
    /// </summary>
    internal ItemsControl? GetMainDataView() => _uiElementManager?.GetMainDataView();

    /// <summary>
    /// UI ACCESS: Get status text elements (delegated to manager)
    /// </summary>
    internal TextBlock? GetRowCountText() => _uiElementManager?.GetRowCountText();
    internal TextBlock? GetFilteredRowCountText() => _uiElementManager?.GetFilteredRowCountText();
    internal TextBlock? GetValidationStatusText() => _uiElementManager?.GetValidationStatusText();
    internal TextBlock? GetOperationStatusText() => _uiElementManager?.GetOperationStatusText();

    /// <summary>
    /// UI ACCESS: Get toolbar elements (delegated to manager)
    /// </summary>
    internal Button? GetAddRowButton() => _uiElementManager?.GetAddRowButton();
    internal Button? GetDeleteRowButton() => _uiElementManager?.GetDeleteRowButton();
    internal Button? GetValidateButton() => _uiElementManager?.GetValidateButton();
    internal Button? GetClearFiltersButton() => _uiElementManager?.GetClearFiltersButton();
    internal TextBox? GetSearchTextBox() => _uiElementManager?.GetSearchTextBox();
    internal Button? GetSearchButton() => _uiElementManager?.GetSearchButton();

    /// <summary>
    /// UI ACCESS: Get layout elements (delegated to manager)
    /// </summary>
    internal Panel? GetValidationPanel() => _uiElementManager?.GetValidationPanel();
    internal FrameworkElement? GetLoadingOverlay() => _uiElementManager?.GetLoadingOverlay();
    internal Panel? GetValidationErrorsPanel() => _uiElementManager?.GetValidationErrorsPanel();
    internal ItemsControl? GetValidationErrorsList() => _uiElementManager?.GetValidationErrorsList();
    internal TextBlock? GetLoadingText() => _uiElementManager?.GetLoadingText();

    #endregion

    #region UI: Cell Editing - Delegation to CellEditManager

    /// <summary>
    /// CELL EDITING: Handle double-tap (delegated to manager)
    /// </summary>
    private async void Cell_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        await _cellEditManager?.HandleCellDoubleTappedAsync(sender, e);
    }

    /// <summary>
    /// CELL EDITING: Handle key down (delegated to manager)
    /// </summary>
    private async void Cell_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        await _cellEditManager?.HandleCellKeyDownAsync(sender, e);
    }

    /// <summary>
    /// CELL EDITING: Handle focus loss (delegated to manager)
    /// </summary>
    private async void Cell_LostFocus(object sender, RoutedEventArgs e)
    {
        await _cellEditManager?.HandleCellLostFocusAsync(sender, e);
    }

    #endregion

    #region UI: Template Management - Delegation to TemplateManager

    /// <summary>
    /// TEMPLATE: Create data template (delegated to manager)
    /// </summary>
    internal async Task<DataTemplate?> CreateDataTemplate() => await _templateManager?.CreateDataTemplateAsync(Columns);

    /// <summary>
    /// TEMPLATE: Create table header (delegated to manager)
    /// </summary>
    internal async Task<DataTemplate?> CreateTableHeader() => await _templateManager?.CreateTableHeaderAsync(Columns);

    #endregion

    #region UI: XAML Events - Minimal Delegation

    /// <summary>
    /// XAML EVENT: Override to ensure components are available after loading
    /// </summary>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        EnsureLogicComponent();
    }

    /// <summary>
    /// SENIOR DESIGN: Initialize business logic after XAML is fully loaded
    /// </summary>
    private void EnsureLogicComponent()
    {
        if (_logicComponent == null)
        {
            try
            {
                _logicComponent = new DataGridComponentLogic(this);
                _logger?.LogInformation("[UI-LOGIC] Logic component initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UI-LOGIC] Failed to initialize logic component: {ErrorMessage}", ex.Message);
            }
        }
    }

    /// <summary>
    /// XAML EVENT: Handle item click events (minimal delegation)
    /// </summary>
    private void MainDataView_ItemClick(object sender, ItemClickEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleItemClick(sender, e);
    }

    /// <summary>
    /// XAML EVENT: Handle selection changed events (minimal delegation)
    /// </summary>
    private void MainDataView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleSelectionChanged(sender, e);
    }

    #endregion

    #region UI: Button Events - Minimal Delegation

    /// <summary>
    /// BUTTON EVENT: Add row (minimal delegation)
    /// </summary>
    private void OnAddRowButtonClick(object sender, RoutedEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleAddRow();
    }

    /// <summary>
    /// BUTTON EVENT: Delete row (minimal delegation)
    /// </summary>
    private void OnDeleteRowButtonClick(object sender, RoutedEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleDeleteRow();
    }

    /// <summary>
    /// BUTTON EVENT: Validate (minimal delegation)
    /// </summary>
    private void OnValidateButtonClick(object sender, RoutedEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleValidate();
    }

    /// <summary>
    /// BUTTON EVENT: Clear filters (minimal delegation)
    /// </summary>
    private void OnClearFiltersButtonClick(object sender, RoutedEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleClearFilters();
    }

    /// <summary>
    /// BUTTON EVENT: Search (minimal delegation)
    /// </summary>
    private void OnSearchButtonClick(object sender, RoutedEventArgs e)
    {
        EnsureLogicComponent();
        _logicComponent?.HandleSearch();
    }

    #endregion

    #region UI: Cleanup and Disposal

    /// <summary>
    /// CLEANUP: Dispose of managed resources
    /// </summary>
    private void Dispose()
    {
        try
        {
            _logicComponent?.Dispose();
            _logicComponent = null;

            // Managers don't need disposal as they don't hold unmanaged resources
            _uiElementManager = null;
            _uiFactory = null;
            _cellEditManager = null;
            _templateManager = null;

            _logger?.LogInformation("[UI-DISPOSE] Component disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-DISPOSE] Error during disposal: {ErrorMessage}", ex.Message);
        }
    }

    #endregion
}