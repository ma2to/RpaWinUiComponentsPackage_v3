using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Microsoft.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI.Converters;

/// <summary>
/// Converter that returns red border brush if there are validation errors, normal border otherwise
/// </summary>
internal class ValidationBorderConverter : IValueConverter
{
    public SolidColorBrush NormalBorderBrush { get; set; } = new SolidColorBrush(Colors.LightGray);
    public SolidColorBrush ErrorBorderBrush { get; set; } = new SolidColorBrush(Colors.Red);
    public SolidColorBrush NormalTextBrush { get; set; } = new SolidColorBrush(Colors.Black);
    public SolidColorBrush ErrorTextBrush { get; set; } = new SolidColorBrush(Colors.Red);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hasErrors = value is string validationText && !string.IsNullOrEmpty(validationText);
        var paramType = parameter?.ToString();

        return paramType?.ToLower() switch
        {
            "text" => hasErrors ? ErrorTextBrush : NormalTextBrush,
            "border" => hasErrors ? ErrorBorderBrush : NormalBorderBrush,
            _ => hasErrors ? ErrorBorderBrush : NormalBorderBrush
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}