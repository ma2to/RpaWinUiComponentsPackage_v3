using System;
using Windows.UI;
using Microsoft.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

/// <summary>
/// DDD: Value object for DataGrid color configuration
/// ENTERPRISE: Professional color palette with theme support
/// IMMUTABLE: Record pattern for configuration consistency
/// ACCESSIBILITY: High contrast and dark theme support
/// </summary>
internal record ColorConfiguration
{
    #region Core Colors
    
    /// <summary>Primary background color for data cells</summary>
    public Color? BackgroundColor { get; init; }
    
    /// <summary>Primary foreground color for data text</summary>
    public Color? ForegroundColor { get; init; }
    
    /// <summary>Border color for cells and grid lines</summary>
    public Color? BorderColor { get; init; }
    
    /// <summary>Grid line color</summary>
    public Color? GridLineColor { get; init; }
    
    #endregion

    #region Header Colors
    
    /// <summary>Background color for column headers</summary>
    public Color? HeaderBackgroundColor { get; init; }
    
    /// <summary>Foreground color for header text</summary>
    public Color? HeaderForegroundColor { get; init; }
    
    /// <summary>Border color for headers</summary>
    public Color? HeaderBorderColor { get; init; }
    
    #endregion

    #region Row Colors
    
    /// <summary>Background color for alternating rows</summary>
    public Color? AlternatingRowBackgroundColor { get; init; }
    
    /// <summary>Background color for selected rows</summary>
    public Color? SelectedRowBackgroundColor { get; init; }
    
    /// <summary>Foreground color for selected row text</summary>
    public Color? SelectedRowForegroundColor { get; init; }
    
    /// <summary>Background color for hover state</summary>
    public Color? HoverBackgroundColor { get; init; }
    
    #endregion

    #region Validation Colors
    
    /// <summary>Background color for validation errors</summary>
    public Color? ValidationErrorBackgroundColor { get; init; }
    
    /// <summary>Foreground color for validation error text</summary>
    public Color? ValidationErrorForegroundColor { get; init; }
    
    /// <summary>Color for validation error indicators</summary>
    public Color? ValidationErrorColor { get; init; }
    
    /// <summary>Background color for validation warnings</summary>
    public Color? ValidationWarningBackgroundColor { get; init; }
    
    /// <summary>Foreground color for validation warning text</summary>
    public Color? ValidationWarningForegroundColor { get; init; }
    
    /// <summary>Color for validation warning indicators</summary>
    public Color? ValidationWarningColor { get; init; }
    
    /// <summary>Background color for validation success</summary>
    public Color? ValidationSuccessBackgroundColor { get; init; }
    
    /// <summary>Foreground color for validation success text</summary>
    public Color? ValidationSuccessForegroundColor { get; init; }
    
    /// <summary>Color for validation success indicators</summary>
    public Color? ValidationSuccessColor { get; init; }
    
    /// <summary>Text color for validation error messages in UI</summary>
    public Color? ValidationErrorTextColor { get; init; }
    
    #endregion

    #region Special Column Colors
    
    /// <summary>Color for CheckBox controls</summary>
    public Color? CheckBoxColor { get; init; }
    
    /// <summary>Color for Delete button</summary>
    public Color? DeleteButtonColor { get; init; }
    
    /// <summary>Color for ValidAlerts text and indicators</summary>
    public Color? ValidAlertsColor { get; init; }
    
    #endregion

    #region Theme Properties
    
    /// <summary>Indicates if this is a dark theme configuration</summary>
    public bool IsDarkTheme { get; init; } = false;
    
    /// <summary>Theme name for identification</summary>
    public string ThemeName { get; init; } = "Default";
    
    #endregion

    #region Factory Methods - Predefined Themes
    
    /// <summary>
    /// FACTORY: Default light theme configuration
    /// PROFESSIONAL: Clean, modern appearance suitable for business applications
    /// </summary>
    public static ColorConfiguration Default => Light;
    
    /// <summary>
    /// FACTORY: Professional light theme
    /// ENTERPRISE: Optimized for productivity and readability
    /// </summary>
    public static ColorConfiguration Light => new()
    {
        ThemeName = "Light",
        IsDarkTheme = false,
        
        // Core colors
        BackgroundColor = Colors.White,
        ForegroundColor = Colors.Black,
        BorderColor = Color.FromArgb(255, 224, 224, 224),
        GridLineColor = Color.FromArgb(255, 240, 240, 240),
        
        // Header colors
        HeaderBackgroundColor = Color.FromArgb(255, 245, 245, 245),
        HeaderForegroundColor = Colors.Black,
        HeaderBorderColor = Color.FromArgb(255, 200, 200, 200),
        
        // Row colors
        AlternatingRowBackgroundColor = Color.FromArgb(255, 248, 248, 248),
        SelectedRowBackgroundColor = Color.FromArgb(255, 51, 153, 255),
        SelectedRowForegroundColor = Colors.White,
        HoverBackgroundColor = Color.FromArgb(255, 230, 240, 255),
        
        // Validation colors
        ValidationErrorBackgroundColor = Color.FromArgb(255, 255, 235, 235),
        ValidationErrorForegroundColor = Colors.DarkRed,
        ValidationErrorColor = Colors.Red,
        
        ValidationWarningBackgroundColor = Color.FromArgb(255, 255, 250, 235),
        ValidationWarningForegroundColor = Color.FromArgb(255, 204, 102, 0),
        ValidationWarningColor = Colors.Orange,
        
        ValidationSuccessBackgroundColor = Color.FromArgb(255, 235, 255, 235),
        ValidationSuccessForegroundColor = Colors.DarkGreen,
        ValidationSuccessColor = Colors.Green,
        ValidationErrorTextColor = Colors.Red,
        
        // Special column colors
        CheckBoxColor = Color.FromArgb(255, 51, 153, 255),
        DeleteButtonColor = Colors.Red,
        ValidAlertsColor = Color.FromArgb(255, 204, 102, 0)
    };
    
    /// <summary>
    /// FACTORY: Professional dark theme
    /// MODERN: Dark mode optimized for reduced eye strain
    /// </summary>
    public static ColorConfiguration Dark => new()
    {
        ThemeName = "Dark",
        IsDarkTheme = true,
        
        // Core colors
        BackgroundColor = Color.FromArgb(255, 30, 30, 30),
        ForegroundColor = Colors.White,
        BorderColor = Color.FromArgb(255, 62, 62, 66),
        GridLineColor = Color.FromArgb(255, 45, 45, 48),
        
        // Header colors
        HeaderBackgroundColor = Color.FromArgb(255, 45, 45, 48),
        HeaderForegroundColor = Colors.White,
        HeaderBorderColor = Color.FromArgb(255, 80, 80, 84),
        
        // Row colors
        AlternatingRowBackgroundColor = Color.FromArgb(255, 40, 40, 40),
        SelectedRowBackgroundColor = Color.FromArgb(255, 0, 120, 215),
        SelectedRowForegroundColor = Colors.White,
        HoverBackgroundColor = Color.FromArgb(255, 64, 64, 64),
        
        // Validation colors
        ValidationErrorBackgroundColor = Color.FromArgb(255, 64, 32, 32),
        ValidationErrorForegroundColor = Color.FromArgb(255, 255, 128, 128),
        ValidationErrorColor = Color.FromArgb(255, 255, 99, 99),
        
        ValidationWarningBackgroundColor = Color.FromArgb(255, 64, 48, 32),
        ValidationWarningForegroundColor = Color.FromArgb(255, 255, 192, 128),
        ValidationWarningColor = Color.FromArgb(255, 255, 206, 84),
        
        ValidationSuccessBackgroundColor = Color.FromArgb(255, 32, 64, 32),
        ValidationSuccessForegroundColor = Color.FromArgb(255, 128, 255, 128),
        ValidationSuccessColor = Color.FromArgb(255, 144, 238, 144),
        ValidationErrorTextColor = Color.FromArgb(255, 255, 99, 99),
        
        // Special column colors
        CheckBoxColor = Color.FromArgb(255, 0, 120, 215),
        DeleteButtonColor = Color.FromArgb(255, 255, 128, 128),
        ValidAlertsColor = Color.FromArgb(255, 255, 206, 84)
    };
    
    /// <summary>
    /// FACTORY: High contrast theme for accessibility
    /// ACCESSIBILITY: Maximum contrast ratios for visual impairments
    /// </summary>
    public static ColorConfiguration HighContrast => new()
    {
        ThemeName = "HighContrast",
        IsDarkTheme = false,
        
        // Core colors
        BackgroundColor = Colors.White,
        ForegroundColor = Colors.Black,
        BorderColor = Colors.Black,
        GridLineColor = Colors.Black,
        
        // Header colors
        HeaderBackgroundColor = Colors.Black,
        HeaderForegroundColor = Colors.White,
        HeaderBorderColor = Colors.Black,
        
        // Row colors
        AlternatingRowBackgroundColor = Color.FromArgb(255, 240, 240, 240),
        SelectedRowBackgroundColor = Colors.Blue,
        SelectedRowForegroundColor = Colors.White,
        HoverBackgroundColor = Color.FromArgb(255, 200, 200, 255),
        
        // Validation colors
        ValidationErrorBackgroundColor = Colors.Red,
        ValidationErrorForegroundColor = Colors.White,
        ValidationErrorColor = Colors.Red,
        
        ValidationWarningBackgroundColor = Color.FromArgb(255, 255, 165, 0),
        ValidationWarningForegroundColor = Colors.Black,
        ValidationWarningColor = Color.FromArgb(255, 255, 165, 0),
        
        ValidationSuccessBackgroundColor = Colors.Green,
        ValidationSuccessForegroundColor = Colors.White,
        ValidationSuccessColor = Colors.Green,
        ValidationErrorTextColor = Colors.Red,
        
        // Special column colors
        CheckBoxColor = Colors.Blue,
        DeleteButtonColor = Colors.Red,
        ValidAlertsColor = Color.FromArgb(255, 255, 165, 0)
    };
    
    #endregion

    #region Factory Methods - Custom Themes
    
    /// <summary>
    /// FACTORY: Create custom theme from base theme
    /// EXTENSIBILITY: Allow customization while maintaining consistency
    /// </summary>
    public static ColorConfiguration Custom(ColorConfiguration baseTheme, Action<ColorConfigurationBuilder>? customizer = null)
    {
        if (customizer == null) return baseTheme;
        
        var builder = new ColorConfigurationBuilder(baseTheme);
        customizer(builder);
        return builder.Build();
    }
    
    /// <summary>
    /// FACTORY: Create theme from primary colors
    /// SIMPLE: Quick theme creation from key colors
    /// </summary>
    public static ColorConfiguration FromPrimaryColors(
        Color background, 
        Color foreground, 
        Color accent, 
        bool isDark = false,
        string themeName = "Custom")
    {
        var accentLight = Color.FromArgb(255, 
            (byte)Math.Min(255, accent.R + 40),
            (byte)Math.Min(255, accent.G + 40),
            (byte)Math.Min(255, accent.B + 40));
            
        var accentDark = Color.FromArgb(255,
            (byte)Math.Max(0, accent.R - 40),
            (byte)Math.Max(0, accent.G - 40),
            (byte)Math.Max(0, accent.B - 40));
        
        return new ColorConfiguration
        {
            ThemeName = themeName,
            IsDarkTheme = isDark,
            BackgroundColor = background,
            ForegroundColor = foreground,
            BorderColor = isDark ? accentLight : accentDark,
            HeaderBackgroundColor = isDark ? accentDark : accentLight,
            HeaderForegroundColor = foreground,
            SelectedRowBackgroundColor = accent,
            SelectedRowForegroundColor = isDark ? Colors.White : Colors.Black,
            CheckBoxColor = accent,
            ValidationErrorColor = Colors.Red,
            ValidationWarningColor = Colors.Orange,
            ValidationSuccessColor = Colors.Green,
            ValidationErrorTextColor = Colors.Red
        };
    }
    
    #endregion
}

/// <summary>
/// BUILDER: Fluent builder for ColorConfiguration
/// PROFESSIONAL: Type-safe color configuration with validation
/// </summary>
internal class ColorConfigurationBuilder
{
    private ColorConfiguration _config;
    
    public ColorConfigurationBuilder() : this(ColorConfiguration.Light) { }
    
    public ColorConfigurationBuilder(ColorConfiguration baseConfig)
    {
        _config = baseConfig;
    }
    
    public ColorConfigurationBuilder WithThemeName(string name)
    {
        _config = _config with { ThemeName = name };
        return this;
    }
    
    public ColorConfigurationBuilder WithDarkTheme(bool isDark = true)
    {
        _config = _config with { IsDarkTheme = isDark };
        return this;
    }
    
    public ColorConfigurationBuilder WithCoreColors(Color? background = null, Color? foreground = null, Color? border = null)
    {
        _config = _config with 
        { 
            BackgroundColor = background ?? _config.BackgroundColor,
            ForegroundColor = foreground ?? _config.ForegroundColor,
            BorderColor = border ?? _config.BorderColor
        };
        return this;
    }
    
    public ColorConfigurationBuilder WithHeaderColors(Color? background = null, Color? foreground = null)
    {
        _config = _config with
        {
            HeaderBackgroundColor = background ?? _config.HeaderBackgroundColor,
            HeaderForegroundColor = foreground ?? _config.HeaderForegroundColor
        };
        return this;
    }
    
    public ColorConfigurationBuilder WithSelectionColors(Color? background = null, Color? foreground = null)
    {
        _config = _config with
        {
            SelectedRowBackgroundColor = background ?? _config.SelectedRowBackgroundColor,
            SelectedRowForegroundColor = foreground ?? _config.SelectedRowForegroundColor
        };
        return this;
    }
    
    public ColorConfigurationBuilder WithValidationColors(Color? error = null, Color? warning = null, Color? success = null)
    {
        _config = _config with
        {
            ValidationErrorColor = error ?? _config.ValidationErrorColor,
            ValidationWarningColor = warning ?? _config.ValidationWarningColor,
            ValidationSuccessColor = success ?? _config.ValidationSuccessColor
        };
        return this;
    }
    
    public ColorConfiguration Build() => _config;
}