using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Components;

/// <summary>
/// UI: XAML code-behind for AdvancedDataGridComponent
/// SENIOR DESIGN: Minimal XAML code-behind following WinUI3 patterns
/// CLEAN ARCHITECTURE: UI layer - handles only XAML event routing
/// </summary>
internal sealed partial class AdvancedDataGridComponent : UserControl
{
    private DataGridComponentLogic? _logicComponent;
    private ILogger? _logger;

    /// <summary>
    /// XAML CONSTRUCTOR: Initialize XAML component
    /// SENIOR DESIGN: Alternative initialization for NuGet package context
    /// </summary>
    public AdvancedDataGridComponent()
    {
        // SENIOR LOGGING: Initialize logger first
        InitializeLogger();

        _logger?.LogInformation("[UI-INIT] AdvancedDataGridComponent constructor started");

        try
        {
            _logger?.LogDebug("[UI-INIT] Attempting XAML InitializeComponent");
            this.InitializeComponent();
            _logger?.LogInformation("[UI-INIT] XAML InitializeComponent succeeded");
        }
        catch (Exception ex)
        {
            // SENIOR FIX: If InitializeComponent fails, try programmatic creation
            _logger?.LogWarning(ex, "[UI-INIT] XAML InitializeComponent failed, attempting programmatic creation: {ErrorMessage}", ex.Message);
            System.Diagnostics.Debug.WriteLine($"InitializeComponent failed, attempting programmatic creation: {ex.Message}");
            InitializeProgrammatically();
        }

        _logger?.LogInformation("[UI-INIT] AdvancedDataGridComponent constructor completed");

        // SENIOR DESIGN: Business logic component will be initialized
        // after XAML elements are available in OnApplyTemplate
    }

    /// <summary>
    /// SENIOR DESIGN: Expose business logic component for external access
    /// CLEAN ARCHITECTURE: Facade pattern for accessing functionality
    /// </summary>
    public DataGridComponentLogic Logic => _logicComponent ?? throw new InvalidOperationException("Logic component not initialized");

    /// <summary>
    /// XAML EVENT: Override to ensure components are available after loading
    /// SENIOR DESIGN: Late initialization of business logic
    /// </summary>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    /// <summary>
    /// SENIOR DESIGN: Initialize business logic after XAML is fully loaded
    /// CLEAN ARCHITECTURE: Delayed initialization for UI safety
    /// </summary>
    private void EnsureLogicComponent()
    {
        if (_logicComponent == null)
        {
            try
            {
                _logicComponent = new DataGridComponentLogic(this);
            }
            catch (Exception ex)
            {
                // SENIOR DESIGN: Graceful degradation if initialization fails
                System.Diagnostics.Debug.WriteLine($"Failed to initialize logic component: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// XAML EVENT: Handle item click events in the main data view
    /// SENIOR DESIGN: Minimal XAML event handler - delegates to logic layer
    /// </summary>
    private void MainDataView_ItemClick(object sender, ItemClickEventArgs e)
    {
        // ARCHITECTURE: Delegate to the business logic component
        // This maintains separation of concerns - XAML only handles UI events
        EnsureLogicComponent();
        _logicComponent?.HandleItemClick(sender, e);
    }

    /// <summary>
    /// XAML EVENT: Handle selection changes in the main data view
    /// SENIOR DESIGN: Minimal XAML event handler - delegates to logic layer
    /// </summary>
    private void MainDataView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ARCHITECTURE: Delegate to the business logic component
        // This maintains separation of concerns - XAML only handles UI events
        EnsureLogicComponent();
        _logicComponent?.HandleSelectionChanged(sender, e);
    }

    /// <summary>
    /// SENIOR DESIGN: Provide access to UI elements for business logic layer
    /// CLEAN ARCHITECTURE: Controlled access with fallback for programmatic UI
    /// </summary>
    internal Microsoft.UI.Xaml.Controls.ListView? GetMainDataView()
    {
        _logger?.LogDebug("[UI-ACCESS] GetMainDataView called");

        // Try to get from XAML first, then from programmatic UI
        try
        {
            var xamlView = MainDataView;
            if (xamlView != null)
            {
                _logger?.LogDebug("[UI-ACCESS] MainDataView found from XAML");
                return xamlView;
            }
            else
            {
                _logger?.LogDebug("[UI-ACCESS] MainDataView from XAML is null");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "[UI-ACCESS] Exception accessing XAML MainDataView: {ErrorMessage}", ex.Message);
        }

        _logger?.LogDebug("[UI-ACCESS] Attempting to find MainDataView programmatically");
        var programmaticView = FindElementByName<Microsoft.UI.Xaml.Controls.ListView>("MainDataView");

        if (programmaticView != null)
        {
            _logger?.LogInformation("[UI-ACCESS] MainDataView found programmatically");
        }
        else
        {
            _logger?.LogWarning("[UI-ACCESS] MainDataView not found via XAML or programmatic search");
        }

        return programmaticView;
    }

    internal Microsoft.UI.Xaml.Controls.TextBlock? GetRowCountText()
    {
        try { return RowCountText; } catch { }
        return FindElementByName<Microsoft.UI.Xaml.Controls.TextBlock>("RowCountText");
    }

    internal Microsoft.UI.Xaml.Controls.TextBlock? GetFilteredRowCountText()
    {
        try { return FilteredRowCountText; } catch { }
        return FindElementByName<Microsoft.UI.Xaml.Controls.TextBlock>("FilteredRowCountText");
    }

    internal Microsoft.UI.Xaml.Controls.TextBlock? GetValidationStatusText()
    {
        try { return ValidationStatusText; } catch { }
        return FindElementByName<Microsoft.UI.Xaml.Controls.TextBlock>("ValidationStatusText");
    }

    internal Microsoft.UI.Xaml.Controls.TextBlock? GetOperationStatusText()
    {
        try { return OperationStatusText; } catch { }
        return FindElementByName<Microsoft.UI.Xaml.Controls.TextBlock>("OperationStatusText");
    }

    /// <summary>
    /// SENIOR HELPER: Find UI elements by name in programmatically created UI
    /// </summary>
    private T? FindElementByName<T>(string name) where T : Microsoft.UI.Xaml.FrameworkElement
    {
        return FindElementByNameRecursive<T>(this.Content as Microsoft.UI.Xaml.FrameworkElement, name);
    }

    private T? FindElementByNameRecursive<T>(Microsoft.UI.Xaml.FrameworkElement? parent, string name) where T : Microsoft.UI.Xaml.FrameworkElement
    {
        if (parent == null) return null;

        if (parent.Name == name && parent is T element)
            return element;

        if (parent is Microsoft.UI.Xaml.Controls.Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Microsoft.UI.Xaml.FrameworkElement frameworkElement)
                {
                    var found = FindElementByNameRecursive<T>(frameworkElement, name);
                    if (found != null) return found;
                }
            }
        }
        else if (parent is Microsoft.UI.Xaml.Controls.ContentControl contentControl && contentControl.Content is Microsoft.UI.Xaml.FrameworkElement contentElement)
        {
            return FindElementByNameRecursive<T>(contentElement, name);
        }

        return null;
    }

    /// <summary>
    /// SENIOR DESIGN: Initialize with sample data - delegate to business logic
    /// </summary>
    public async Task<bool> InitializeWithSampleDataAsync()
    {
        // SENIOR DESIGN: Ensure logic component is available before use
        EnsureLogicComponent();

        if (_logicComponent == null)
        {
            return false; // Initialization failed gracefully
        }

        return await _logicComponent.InitializeWithSampleDataAsync();
    }

    /// <summary>
    /// SENIOR LOGGING: Initialize logger for comprehensive debugging
    /// </summary>
    private void InitializeLogger()
    {
        try
        {
            // Try to get logger from the common DataGrid logging infrastructure
            _logger = NullLogger<AdvancedDataGridComponent>.Instance;
        }
        catch
        {
            // Fallback to null logger if initialization fails
            _logger = NullLogger<AdvancedDataGridComponent>.Instance;
        }
    }

    /// <summary>
    /// SENIOR FIX: Programmatic UI creation when XAML loading fails
    /// ENTERPRISE GRADE: Fallback UI creation for package deployment
    /// </summary>
    private void InitializeProgrammatically()
    {
        _logger?.LogInformation("[UI-PROGRAMMATIC] Starting programmatic UI creation");

        try
        {
            // Create the main grid structure programmatically
            _logger?.LogDebug("[UI-PROGRAMMATIC] Creating main Grid");
            var mainGrid = new Microsoft.UI.Xaml.Controls.Grid();
            mainGrid.Name = "MainGrid";

            // Define row definitions
            _logger?.LogDebug("[UI-PROGRAMMATIC] Adding row definitions");
            mainGrid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto }); // Toolbar
            mainGrid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition { Height = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) }); // Main content
            mainGrid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto }); // Status bar
            mainGrid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition { Height = Microsoft.UI.Xaml.GridLength.Auto }); // Validation panel

            // Create toolbar
            _logger?.LogDebug("[UI-PROGRAMMATIC] Creating toolbar");
            CreateToolbar(mainGrid);

            // Create main data view
            _logger?.LogDebug("[UI-PROGRAMMATIC] Creating main data view");
            CreateMainDataView(mainGrid);

            // Create status bar
            _logger?.LogDebug("[UI-PROGRAMMATIC] Creating status bar");
            CreateStatusBar(mainGrid);

            // Create validation panel
            _logger?.LogDebug("[UI-PROGRAMMATIC] Creating validation panel");
            CreateValidationPanel(mainGrid);

            // Create loading overlay
            _logger?.LogDebug("[UI-PROGRAMMATIC] Creating loading overlay");
            CreateLoadingOverlay(mainGrid);

            _logger?.LogDebug("[UI-PROGRAMMATIC] Setting Content to main grid");
            this.Content = mainGrid;

            _logger?.LogInformation("[UI-PROGRAMMATIC] Programmatic UI creation completed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-PROGRAMMATIC] Programmatic UI creation failed: {ErrorMessage}", ex.Message);
            System.Diagnostics.Debug.WriteLine($"Programmatic UI creation failed: {ex.Message}");

            // Create minimal fallback
            _logger?.LogWarning("[UI-PROGRAMMATIC] Creating minimal fallback UI");
            this.Content = new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = "DataGrid component failed to initialize",
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };
        }
    }

    private void CreateToolbar(Microsoft.UI.Xaml.Controls.Grid mainGrid)
    {
        var toolbar = new Microsoft.UI.Xaml.Controls.Border
        {
            Name = "ToolbarBorder",
            Height = 40,
            BorderThickness = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 1)
        };

        var stackPanel = new Microsoft.UI.Xaml.Controls.StackPanel
        {
            Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal,
            Margin = new Microsoft.UI.Xaml.Thickness(8, 0, 8, 0)
        };

        var addButton = new Microsoft.UI.Xaml.Controls.Button { Name = "AddRowButton", Content = "Add Row" };
        var deleteButton = new Microsoft.UI.Xaml.Controls.Button { Name = "DeleteRowButton", Content = "Delete Row" };
        var validateButton = new Microsoft.UI.Xaml.Controls.Button { Name = "ValidateButton", Content = "Validate" };
        var clearFiltersButton = new Microsoft.UI.Xaml.Controls.Button { Name = "ClearFiltersButton", Content = "Clear Filters" };
        var searchTextBox = new Microsoft.UI.Xaml.Controls.TextBox { Name = "SearchTextBox", PlaceholderText = "Search...", Width = 200 };
        var searchButton = new Microsoft.UI.Xaml.Controls.Button { Name = "SearchButton", Content = "Search" };

        stackPanel.Children.Add(addButton);
        stackPanel.Children.Add(deleteButton);
        stackPanel.Children.Add(validateButton);
        stackPanel.Children.Add(clearFiltersButton);
        stackPanel.Children.Add(searchTextBox);
        stackPanel.Children.Add(searchButton);

        toolbar.Child = stackPanel;
        Microsoft.UI.Xaml.Controls.Grid.SetRow(toolbar, 0);
        mainGrid.Children.Add(toolbar);
    }

    private void CreateMainDataView(Microsoft.UI.Xaml.Controls.Grid mainGrid)
    {
        _logger?.LogDebug("[UI-CREATE] Creating main data view components");

        var scrollViewer = new Microsoft.UI.Xaml.Controls.ScrollViewer
        {
            Name = "DataGridScrollViewer",
            ZoomMode = Microsoft.UI.Xaml.Controls.ZoomMode.Disabled,
            HorizontalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto
        };

        _logger?.LogDebug("[UI-CREATE] ScrollViewer created with name: {Name}", scrollViewer.Name);

        var dataView = new Microsoft.UI.Xaml.Controls.ListView
        {
            Name = "MainDataView",
            SelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode.Extended,
            IsItemClickEnabled = true
        };

        _logger?.LogInformation("[UI-CREATE] ListView (MainDataView) created with name: {Name}", dataView.Name);

        // SENIOR ENHANCEMENT: Add header to make table structure clear
        try
        {
            var header = CreateTableHeader();
            dataView.Header = header;
            _logger?.LogDebug("[UI-CREATE] Header added to ListView");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-CREATE] Failed to create header: {ErrorMessage}", ex.Message);
        }

        // Hook up event handlers
        try
        {
            dataView.ItemClick += MainDataView_ItemClick;
            dataView.SelectionChanged += MainDataView_SelectionChanged;
            _logger?.LogDebug("[UI-CREATE] Event handlers attached to ListView");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-CREATE] Failed to attach event handlers to ListView: {ErrorMessage}", ex.Message);
        }

        // CRITICAL: Create and set ItemTemplate for data display
        _logger?.LogDebug("[UI-CREATE] Creating DataTemplate for ListView items");
        try
        {
            var itemTemplate = CreateDataTemplate();
            dataView.ItemTemplate = itemTemplate;
            _logger?.LogInformation("[UI-CREATE] DataTemplate set on ListView");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UI-CREATE] Failed to create or set DataTemplate, will use simple display: {ErrorMessage}", ex.Message);
            // Fallback: use simple ToString display
        }

        scrollViewer.Content = dataView;
        Microsoft.UI.Xaml.Controls.Grid.SetRow(scrollViewer, 1);
        mainGrid.Children.Add(scrollViewer);

        _logger?.LogInformation("[UI-CREATE] Main data view added to grid at row 1");
    }

    private void CreateStatusBar(Microsoft.UI.Xaml.Controls.Grid mainGrid)
    {
        var statusBar = new Microsoft.UI.Xaml.Controls.Border
        {
            Name = "StatusBar",
            Height = 24,
            BorderThickness = new Microsoft.UI.Xaml.Thickness(0, 1, 0, 0)
        };

        var stackPanel = new Microsoft.UI.Xaml.Controls.StackPanel
        {
            Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal,
            Margin = new Microsoft.UI.Xaml.Thickness(8, 2, 8, 2)
        };

        var rowCountText = new Microsoft.UI.Xaml.Controls.TextBlock { Name = "RowCountText", Text = "Rows: 0" };
        var filteredRowCountText = new Microsoft.UI.Xaml.Controls.TextBlock { Name = "FilteredRowCountText", Text = "Filtered: 0" };
        var validationStatusText = new Microsoft.UI.Xaml.Controls.TextBlock { Name = "ValidationStatusText", Text = "Valid" };
        var operationStatusText = new Microsoft.UI.Xaml.Controls.TextBlock { Name = "OperationStatusText", Text = "Ready" };

        stackPanel.Children.Add(rowCountText);
        stackPanel.Children.Add(filteredRowCountText);
        stackPanel.Children.Add(validationStatusText);
        stackPanel.Children.Add(operationStatusText);

        statusBar.Child = stackPanel;
        Microsoft.UI.Xaml.Controls.Grid.SetRow(statusBar, 2);
        mainGrid.Children.Add(statusBar);
    }

    private void CreateValidationPanel(Microsoft.UI.Xaml.Controls.Grid mainGrid)
    {
        var validationPanel = new Microsoft.UI.Xaml.Controls.Border
        {
            Name = "ValidationPanel",
            Visibility = Microsoft.UI.Xaml.Visibility.Collapsed,
            MaxHeight = 150,
            BorderThickness = new Microsoft.UI.Xaml.Thickness(0, 1, 0, 0)
        };

        var scrollViewer = new Microsoft.UI.Xaml.Controls.ScrollViewer
        {
            VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto
        };

        var stackPanel = new Microsoft.UI.Xaml.Controls.StackPanel
        {
            Name = "ValidationErrorsPanel",
            Margin = new Microsoft.UI.Xaml.Thickness(8, 8, 8, 8)
        };

        var titleText = new Microsoft.UI.Xaml.Controls.TextBlock
        {
            Text = "Validation Errors:",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 4)
        };

        var errorsList = new Microsoft.UI.Xaml.Controls.ItemsControl { Name = "ValidationErrorsList" };

        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(errorsList);

        scrollViewer.Content = stackPanel;
        validationPanel.Child = scrollViewer;
        Microsoft.UI.Xaml.Controls.Grid.SetRow(validationPanel, 3);
        mainGrid.Children.Add(validationPanel);
    }

    private void CreateLoadingOverlay(Microsoft.UI.Xaml.Controls.Grid mainGrid)
    {
        var loadingOverlay = new Microsoft.UI.Xaml.Controls.Border
        {
            Name = "LoadingOverlay",
            Opacity = 0.8,
            Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
        };

        Microsoft.UI.Xaml.Controls.Grid.SetRowSpan(loadingOverlay, 4);

        var stackPanel = new Microsoft.UI.Xaml.Controls.StackPanel
        {
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };

        var progressRing = new Microsoft.UI.Xaml.Controls.ProgressRing
        {
            IsActive = true,
            Width = 48,
            Height = 48,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 16)
        };

        var loadingText = new Microsoft.UI.Xaml.Controls.TextBlock
        {
            Name = "LoadingText",
            Text = "Loading...",
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center
        };

        stackPanel.Children.Add(progressRing);
        stackPanel.Children.Add(loadingText);
        loadingOverlay.Child = stackPanel;

        mainGrid.Children.Add(loadingOverlay);
    }

    /// <summary>
    /// SENIOR ENHANCEMENT: Create table header programmatically
    /// </summary>
    private Microsoft.UI.Xaml.FrameworkElement CreateTableHeader()
    {
        _logger?.LogDebug("[HEADER-CREATE] Creating table header");

        var headerBorder = new Microsoft.UI.Xaml.Controls.Border
        {
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray),
            BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
            BorderThickness = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 1)
        };

        var headerGrid = new Microsoft.UI.Xaml.Controls.Grid();
        headerGrid.Margin = new Microsoft.UI.Xaml.Thickness(4, 8, 4, 8);

        // Define column definitions to match data template
        headerGrid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(60) }); // ID
        headerGrid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(150) }); // Name
        headerGrid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(200) }); // Email
        headerGrid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(100) }); // Status

        // Create header TextBlocks
        var idHeader = new Microsoft.UI.Xaml.Controls.TextBlock
        {
            Text = "ID",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Microsoft.UI.Xaml.Thickness(8, 4, 8, 4),
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };
        Microsoft.UI.Xaml.Controls.Grid.SetColumn(idHeader, 0);

        var nameHeader = new Microsoft.UI.Xaml.Controls.TextBlock
        {
            Text = "Name",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Microsoft.UI.Xaml.Thickness(8, 4, 8, 4),
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };
        Microsoft.UI.Xaml.Controls.Grid.SetColumn(nameHeader, 1);

        var emailHeader = new Microsoft.UI.Xaml.Controls.TextBlock
        {
            Text = "Email",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Microsoft.UI.Xaml.Thickness(8, 4, 8, 4),
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };
        Microsoft.UI.Xaml.Controls.Grid.SetColumn(emailHeader, 2);

        var statusHeader = new Microsoft.UI.Xaml.Controls.TextBlock
        {
            Text = "Status",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Microsoft.UI.Xaml.Thickness(8, 4, 8, 4),
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };
        Microsoft.UI.Xaml.Controls.Grid.SetColumn(statusHeader, 3);

        // Add headers to grid
        headerGrid.Children.Add(idHeader);
        headerGrid.Children.Add(nameHeader);
        headerGrid.Children.Add(emailHeader);
        headerGrid.Children.Add(statusHeader);

        headerBorder.Child = headerGrid;

        _logger?.LogInformation("[HEADER-CREATE] Table header created successfully");
        return headerBorder;
    }

    /// <summary>
    /// CRITICAL FIX: Create DataTemplate programmatically for ListView items
    /// SENIOR SOLUTION: WinUI3-compatible template creation for data display
    /// </summary>
    private Microsoft.UI.Xaml.DataTemplate CreateDataTemplate()
    {
        _logger?.LogDebug("[TEMPLATE-CREATE] Creating DataTemplate for ListView items using XAML parsing");

        try
        {
            // SENIOR APPROACH: Use XAML string parsing for WinUI3 DataTemplate
            string xamlTemplate = @"
                <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                    <Grid Margin=""4,2"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""60""/>
                            <ColumnDefinition Width=""150""/>
                            <ColumnDefinition Width=""200""/>
                            <ColumnDefinition Width=""100""/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column=""0"" Text=""{Binding ID}"" Margin=""8,4"" VerticalAlignment=""Center""/>
                        <TextBlock Grid.Column=""1"" Text=""{Binding Name}"" Margin=""8,4"" VerticalAlignment=""Center""/>
                        <TextBlock Grid.Column=""2"" Text=""{Binding Email}"" Margin=""8,4"" VerticalAlignment=""Center""/>
                        <TextBlock Grid.Column=""3"" Text=""{Binding Status}"" Margin=""8,4"" VerticalAlignment=""Center""/>
                    </Grid>
                </DataTemplate>";

            var dataTemplate = Microsoft.UI.Xaml.Markup.XamlReader.Load(xamlTemplate) as Microsoft.UI.Xaml.DataTemplate;

            if (dataTemplate != null)
            {
                _logger?.LogInformation("[TEMPLATE-CREATE] DataTemplate created successfully via XAML parsing");
                return dataTemplate;
            }
            else
            {
                _logger?.LogError("[TEMPLATE-CREATE] XAML parsing returned null DataTemplate");
                throw new InvalidOperationException("Failed to parse XAML DataTemplate");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[TEMPLATE-CREATE] Failed to create DataTemplate via XAML parsing: {ErrorMessage}", ex.Message);

            // FALLBACK: Create simple template programmatically
            _logger?.LogInformation("[TEMPLATE-CREATE] Creating fallback simple DataTemplate");
            return CreateSimpleDataTemplate();
        }
    }

    /// <summary>
    /// FALLBACK: Create simple DataTemplate when XAML parsing fails
    /// </summary>
    private Microsoft.UI.Xaml.DataTemplate CreateSimpleDataTemplate()
    {
        _logger?.LogInformation("[TEMPLATE-CREATE] Creating simple fallback DataTemplate");

        // SENIOR APPROACH: Return null and let ListView use default display
        // This will show the ToString() representation of objects
        var dataTemplate = new Microsoft.UI.Xaml.DataTemplate();

        _logger?.LogInformation("[TEMPLATE-CREATE] Simple DataTemplate created");
        return dataTemplate;
    }

    /// <summary>
    /// SENIOR DESIGN: Implement IDisposable pattern for proper cleanup
    /// </summary>
    public void Dispose()
    {
        _logicComponent?.Dispose();
        _logicComponent = null;
    }
}