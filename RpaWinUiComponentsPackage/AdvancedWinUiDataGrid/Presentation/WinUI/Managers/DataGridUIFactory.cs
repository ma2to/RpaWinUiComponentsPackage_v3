using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.Logging;
using Windows.UI;
using Microsoft.UI;
using XamlApplication = Microsoft.UI.Xaml.Application;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Managers;

/// <summary>
/// UI: Factory for creating UI elements programmatically
/// CLEAN ARCHITECTURE: UI layer - Handles programmatic UI creation when XAML fails
/// SOLID: Single responsibility for UI element creation
/// </summary>
internal sealed class DataGridUIFactory
{
    private readonly ILogger? _logger;
    private readonly ColorConfiguration? _colorConfiguration;

    public DataGridUIFactory(ILogger? logger = null, ColorConfiguration? colorConfiguration = null)
    {
        _logger = logger;
        _colorConfiguration = colorConfiguration;
    }

    /// <summary>
    /// SENIOR APPROACH: Create main grid layout programmatically
    /// </summary>
    public Grid CreateMainGrid()
    {
        _logger?.LogInformation("[UI-CREATE] Creating main grid layout");

        var mainGrid = new Grid { Name = "MainGrid" };

        // Define row definitions
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Toolbar
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Main content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status bar
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Validation panel

        _logger?.LogInformation("[UI-CREATE] Main grid created with 4 rows");
        return mainGrid;
    }

    /// <summary>
    /// SENIOR APPROACH: Create toolbar programmatically
    /// </summary>
    public void CreateToolbar(Grid mainGrid)
    {
        _logger?.LogInformation("[UI-CREATE] Creating toolbar");

        var toolbarBorder = new Border
        {
            Name = "ToolbarBorder",
            Height = 40,
            BorderThickness = new Thickness(0, 0, 0, 1)
        };

        // Apply theme resources
        try
        {
            if (XamlApplication.Current?.Resources != null)
            {
                toolbarBorder.Background = (Brush)XamlApplication.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
                toolbarBorder.BorderBrush = (Brush)XamlApplication.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-CREATE] Failed to apply theme resources to toolbar");
        }

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(8, 0, 8, 0)
        };

        // Create toolbar buttons
        stackPanel.Children.Add(CreateToolbarButton("AddRowButton", "Add Row"));
        stackPanel.Children.Add(CreateToolbarButton("DeleteRowButton", "Delete Row"));
        stackPanel.Children.Add(CreateToolbarSeparator());
        stackPanel.Children.Add(CreateToolbarButton("ValidateButton", "Validate"));
        stackPanel.Children.Add(CreateToolbarButton("ClearFiltersButton", "Clear Filters"));
        stackPanel.Children.Add(CreateToolbarSeparator());

        var searchTextBox = new TextBox
        {
            Name = "SearchTextBox",
            PlaceholderText = "Search...",
            Width = 200,
            Margin = new Thickness(4, 0, 4, 0)
        };
        stackPanel.Children.Add(searchTextBox);
        stackPanel.Children.Add(CreateToolbarButton("SearchButton", "Search"));

        toolbarBorder.Child = stackPanel;
        Grid.SetRow(toolbarBorder, 0);
        mainGrid.Children.Add(toolbarBorder);

        _logger?.LogInformation("[UI-CREATE] Toolbar created successfully");
    }

    /// <summary>
    /// SENIOR HELPER: Create toolbar button with consistent styling
    /// </summary>
    private Button CreateToolbarButton(string name, string content)
    {
        return new Button
        {
            Name = name,
            Content = content,
            Margin = new Thickness(4, 0, 4, 0),
            Padding = new Thickness(12, 6, 12, 6),
            MinWidth = 80
        };
    }

    /// <summary>
    /// SENIOR HELPER: Create toolbar separator
    /// </summary>
    private Border CreateToolbarSeparator()
    {
        var separator = new Border
        {
            Width = 1,
            Margin = new Thickness(4, 0, 4, 0)
        };

        try
        {
            if (XamlApplication.Current?.Resources != null)
                separator.Background = (Brush)XamlApplication.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-CREATE] Failed to apply theme resource to separator");
            separator.Background = new SolidColorBrush(Colors.Gray);
        }

        return separator;
    }

    /// <summary>
    /// SENIOR APPROACH: Create main data view programmatically
    /// </summary>
    public void CreateMainDataView(Grid mainGrid)
    {
        _logger?.LogInformation("[UI-CREATE] Creating main data view");

        var scrollViewer = new ScrollViewer
        {
            Name = "DataGridScrollViewer",
            ZoomMode = ZoomMode.Disabled,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var dataView = new ItemsControl
        {
            Name = "MainDataView"
        };

        // Apply theme resources
        try
        {
            if (XamlApplication.Current?.Resources != null)
            {
                dataView.Background = (Brush)XamlApplication.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
                dataView.BorderBrush = (Brush)XamlApplication.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
            }
            dataView.BorderThickness = new Thickness(1);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-CREATE] Failed to apply theme resources to data view");
        }

        scrollViewer.Content = dataView;
        Grid.SetRow(scrollViewer, 1);
        mainGrid.Children.Add(scrollViewer);

        _logger?.LogInformation("[UI-CREATE] Main data view created successfully");
    }

    /// <summary>
    /// SENIOR APPROACH: Create status bar programmatically
    /// </summary>
    public void CreateStatusBar(Grid mainGrid)
    {
        _logger?.LogInformation("[UI-CREATE] Creating status bar");

        var statusBar = new Border
        {
            Name = "StatusBar",
            Height = 24,
            BorderThickness = new Thickness(0, 1, 0, 0)
        };

        // Apply theme resources
        try
        {
            if (XamlApplication.Current?.Resources != null)
            {
                statusBar.Background = (Brush)XamlApplication.Current.Resources["SystemControlBackgroundChromeMediumBrush"];
                statusBar.BorderBrush = (Brush)XamlApplication.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-CREATE] Failed to apply theme resources to status bar");
        }

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(8, 2, 8, 2)
        };

        stackPanel.Children.Add(new TextBlock
        {
            Name = "RowCountText",
            Text = "Rows: 0",
            Margin = new Thickness(0, 0, 16, 0)
        });

        stackPanel.Children.Add(new TextBlock
        {
            Name = "FilteredRowCountText",
            Text = "Filtered: 0",
            Margin = new Thickness(0, 0, 16, 0)
        });

        stackPanel.Children.Add(new TextBlock
        {
            Name = "ValidationStatusText",
            Text = "Valid",
            Margin = new Thickness(0, 0, 16, 0)
        });

        stackPanel.Children.Add(new TextBlock
        {
            Name = "OperationStatusText",
            Text = "Ready",
            Margin = new Thickness(0, 0, 16, 0)
        });

        statusBar.Child = stackPanel;
        Grid.SetRow(statusBar, 2);
        mainGrid.Children.Add(statusBar);

        _logger?.LogInformation("[UI-CREATE] Status bar created successfully");
    }

    /// <summary>
    /// SENIOR APPROACH: Create validation panel programmatically
    /// </summary>
    public void CreateValidationPanel(Grid mainGrid)
    {
        _logger?.LogInformation("[UI-CREATE] Creating validation panel");

        var validationPanel = new Border
        {
            Name = "ValidationPanel",
            BorderThickness = new Thickness(0, 1, 0, 0),
            Visibility = Visibility.Collapsed,
            MaxHeight = 150
        };

        // Apply theme resources
        try
        {
            if (XamlApplication.Current?.Resources != null)
            {
                validationPanel.Background = (Brush)XamlApplication.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
                validationPanel.BorderBrush = (Brush)XamlApplication.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-CREATE] Failed to apply theme resources to validation panel");
        }

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var validationErrorsPanel = new StackPanel
        {
            Name = "ValidationErrorsPanel",
            Margin = new Thickness(8)
        };

        validationErrorsPanel.Children.Add(new TextBlock
        {
            Text = "Validation Errors:",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 4)
        });

        var validationErrorsList = new ItemsControl
        {
            Name = "ValidationErrorsList"
        };

        validationErrorsPanel.Children.Add(validationErrorsList);
        scrollViewer.Content = validationErrorsPanel;
        validationPanel.Child = scrollViewer;

        Grid.SetRow(validationPanel, 3);
        mainGrid.Children.Add(validationPanel);

        _logger?.LogInformation("[UI-CREATE] Validation panel created successfully");
    }

    /// <summary>
    /// SENIOR APPROACH: Create loading overlay programmatically
    /// </summary>
    public void CreateLoadingOverlay(Grid mainGrid)
    {
        _logger?.LogInformation("[UI-CREATE] Creating loading overlay");

        var loadingOverlay = new Border
        {
            Name = "LoadingOverlay",
            Opacity = 0.8,
            Visibility = Visibility.Collapsed
        };

        Grid.SetRowSpan(loadingOverlay, 4);

        // Apply theme resources
        try
        {
            if (XamlApplication.Current?.Resources != null)
                loadingOverlay.Background = (Brush)XamlApplication.Current.Resources["SystemControlBackgroundChromeMediumBrush"];
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[UI-CREATE] Failed to apply theme resources to loading overlay");
        }

        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var progressRing = new ProgressRing
        {
            IsActive = true,
            Width = 48,
            Height = 48,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var loadingText = new TextBlock
        {
            Name = "LoadingText",
            Text = "Loading...",
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stackPanel.Children.Add(progressRing);
        stackPanel.Children.Add(loadingText);
        loadingOverlay.Child = stackPanel;

        mainGrid.Children.Add(loadingOverlay);

        _logger?.LogInformation("[UI-CREATE] Loading overlay created successfully");
    }

    /// <summary>
    /// SENIOR HELPER: Get color with fallback and comprehensive logging
    /// </summary>
    private Color GetColorWithFallback(Color? configColor, Color fallbackColor, string colorName)
    {
        if (configColor.HasValue)
        {
            _logger?.LogInformation("[COLOR-FALLBACK-FACTORY] Using configured color for {ColorName}: {Color}", colorName, configColor.Value);
            return configColor.Value;
        }
        else
        {
            _logger?.LogWarning("[COLOR-FALLBACK-FACTORY] No configured color for {ColorName}, using fallback: {FallbackColor}", colorName, fallbackColor);
            return fallbackColor;
        }
    }
}