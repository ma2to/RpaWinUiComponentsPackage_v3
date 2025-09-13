using System;
using System.Collections.Generic;
using Windows.UI;
using Microsoft.UI.Xaml;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;



/// <summary>
/// ENTERPRISE: Comprehensive DataGrid configuration
/// DDD: Aggregate root configuration
/// IMMUTABLE: Configuration object following functional principles
/// </summary>
internal record DataGridConfiguration
{
    // CORE: Basic configuration
    public string Name { get; init; } = "DataGrid";
    public bool IsReadOnly { get; init; } = false;
    
    // PERFORMANCE: Optimization settings
    public PerformanceConfiguration Performance { get; init; } = PerformanceConfiguration.Default;
    
    // VALIDATION: Business rules configuration
    public ValidationConfiguration Validation { get; init; } = ValidationConfiguration.Comprehensive;
    
    // UI: Presentation configuration (optional - only for UI mode)
    public UIConfiguration? UI { get; init; }
    
    // ENTERPRISE: Audit and monitoring
    public bool EnableAuditLog { get; init; } = true;
    public bool EnablePerformanceMonitoring { get; init; } = true;
    
    public static DataGridConfiguration Default => new();
    
    public static DataGridConfiguration ForUI(UIConfiguration uiConfig) =>
        Default with { UI = uiConfig };
    
    public static DataGridConfiguration ForHeadless() =>
        Default with { UI = null };
}



/// <summary>
/// UI: User interface configuration (WinUI3 specific)
/// SOLID: Single Responsibility - only UI concerns
/// DECORATOR: Optional UI decoration for core functionality
/// </summary>
internal record UIConfiguration
{
    // THEMING: Direct color configuration (no themes as per requirements)
    public ColorConfiguration Colors { get; init; } = ColorConfiguration.Default;
    
    // LAYOUT: Visual layout settings
    public double RowHeight { get; init; } = 32;
    public double HeaderHeight { get; init; } = 40;
    public bool ShowGridLines { get; init; } = true;
    public bool ShowRowNumbers { get; init; } = false;
    
    // TEXT RENDERING: Advanced text display options
    public bool EnableAutoRowHeight { get; init; } = true;
    public double MinRowHeight { get; init; } = 24;
    public double MaxRowHeight { get; init; } = 200;
    public TextWrapping TextWrappingMode { get; init; } = TextWrapping.Wrap;
    
    // INTERACTION: User interaction settings
    public bool AllowColumnReordering { get; init; } = true;
    public bool AllowColumnResizing { get; init; } = true;
    public bool AllowRowSelection { get; init; } = true;
    public SelectionMode SelectionMode { get; init; } = SelectionMode.Multiple;
    
    public static UIConfiguration Default => new();
}


/// <summary>
/// ENTERPRISE: Selection mode enumeration
/// UI: User selection behavior configuration
/// </summary>
internal enum SelectionMode
{
    None,
    Single,
    Multiple,
    Extended
}