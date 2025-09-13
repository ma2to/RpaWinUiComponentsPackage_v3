using Windows.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

/// <summary>
/// DDD: Value object for special column configuration
/// ENTERPRISE: Configuration for specialized column behaviors
/// </summary>
internal record SpecialColumnConfiguration
{
    /// <summary>Text to display for delete button</summary>
    public string? DeleteButtonText { get; init; } = "Delete";
    
    /// <summary>Color for delete button</summary>
    public Color? DeleteButtonColor { get; init; }
    
    /// <summary>Icon for validation alerts</summary>
    public string? ValidationAlertIcon { get; init; } = "⚠";
    
    /// <summary>Color for validation alerts</summary>
    public Color? ValidationAlertColor { get; init; }
    
    /// <summary>Custom checkbox appearance configuration</summary>
    public CheckBoxAppearance? CheckBoxAppearance { get; init; }
    
    /// <summary>Require confirmation before deletion</summary>
    public bool RequireDeleteConfirmation { get; init; } = true;
    
    /// <summary>Confirmation message for deletion</summary>
    public string? DeleteConfirmationMessage { get; init; }
    
    /// <summary>Minimum width for validation alerts column</summary>
    public double MinimumWidth { get; init; } = 200;
    
    public static SpecialColumnConfiguration Default => new();
    
    public static SpecialColumnConfiguration ForDeleteButton(string buttonText = "Delete", Color? buttonColor = null, bool requireConfirmation = true, string? confirmationMessage = null) =>
        new() 
        { 
            DeleteButtonText = buttonText, 
            DeleteButtonColor = buttonColor,
            RequireDeleteConfirmation = requireConfirmation,
            DeleteConfirmationMessage = confirmationMessage
        };
    
    public static SpecialColumnConfiguration ForValidationAlerts(string icon = "⚠", Color? color = null, double minimumWidth = 200) =>
        new() 
        { 
            ValidationAlertIcon = icon, 
            ValidationAlertColor = color,
            MinimumWidth = minimumWidth
        };
}

/// <summary>
/// DDD: Value object for checkbox appearance configuration
/// </summary>
internal record CheckBoxAppearance
{
    public Color? CheckedColor { get; init; }
    public Color? UncheckedColor { get; init; }
    public Color? BorderColor { get; init; }
    public double? Size { get; init; }
    
    public static CheckBoxAppearance Default => new();
}