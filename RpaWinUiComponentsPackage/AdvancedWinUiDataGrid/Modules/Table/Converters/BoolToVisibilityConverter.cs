using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Modules.Table.Converters;

/// <summary>
/// Konvertor boolean hodnôt na Visibility
/// FUNCTIONAL: Čistý UI converter bez side effects
/// </summary>
internal sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje bool na Visibility
    /// True → Visible, False → Collapsed
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Konvertuje Visibility späť na bool
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Inverzný konvertor boolean hodnôt na Visibility
/// FUNCTIONAL: False → Visible, True → Collapsed
/// </summary>
internal sealed class BoolToVisibilityInverseConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje bool na Visibility (inverzne)
    /// False → Visible, True → Collapsed
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    /// <summary>
    /// Konvertuje Visibility späť na bool (inverzne)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return true;
    }
}

/// <summary>
/// Konvertor boolean hodnôt na Thickness pre validačné borders
/// FUNCTIONAL: False (error) → Thick border, True (valid) → Normal border
/// </summary>
internal sealed class BoolToThicknessConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje bool na Thickness
    /// False (error) → Thick border (2px), True (valid) → Normal border (1px)
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isValid)
        {
            return isValid ? new Thickness(0, 0, 1, 1) : new Thickness(2); // Error gets thick border
        }
        return new Thickness(0, 0, 1, 1); // Default normal border
    }

    /// <summary>
    /// Konvertuje Thickness späť na bool
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Thickness thickness)
        {
            return thickness.Left <= 1 && thickness.Top <= 1;
        }
        return true;
    }
}