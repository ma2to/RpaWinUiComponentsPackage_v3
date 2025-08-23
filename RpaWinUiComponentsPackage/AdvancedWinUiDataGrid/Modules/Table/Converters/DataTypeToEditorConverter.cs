using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Modules.Table.Converters;

/// <summary>
/// Konvertor pre určenie správneho editora na základe typu dát
/// FUNCTIONAL: Pure converter pre UI editor selection s Visibility podporou
/// </summary>
internal sealed class DataTypeToEditorConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje editor type na Visibility pre konkrétny editor
    /// Parameter určuje, ktorý editor sa má kontrolovať
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var editorType = value?.ToString() ?? "TextBox";
        var targetEditor = parameter?.ToString() ?? "TextBox";
        
        // Return Visible if editor type matches target, otherwise Collapsed
        return editorType == targetEditor ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// ConvertBack nie je potrebný pre tento converter
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException("DataTypeToEditorConverter does not support ConvertBack");
    }
}

/// <summary>
/// Konvertor pre určenie typu editora na základe dátového typu
/// FUNCTIONAL: Pure data type to editor type mapper
/// </summary>
internal sealed class DataTypeToEditorTypeConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje typ dát na typ editora
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string dataTypeString)
        {
            // Boolean → CheckBox editor
            if (dataTypeString.Contains("Boolean") || dataTypeString.Contains("Bool"))
                return "CheckBox";
            
            // Numeric types → NumberBox editor
            if (dataTypeString.Contains("Int") || dataTypeString.Contains("Decimal") ||
                dataTypeString.Contains("Double") || dataTypeString.Contains("Float") ||
                dataTypeString.Contains("Number"))
                return "NumberBox";
            
            // DateTime → DatePicker editor
            if (dataTypeString.Contains("DateTime") || dataTypeString.Contains("Date"))
                return "DatePicker";
            
            // Default → TextBox editor
            return "TextBox";
        }
        
        if (value is Type dataType)
        {
            // Boolean → CheckBox editor
            if (dataType == typeof(bool) || dataType == typeof(bool?))
                return "CheckBox";
            
            // Numeric types → NumberBox editor
            if (dataType == typeof(int) || dataType == typeof(int?) ||
                dataType == typeof(decimal) || dataType == typeof(decimal?) ||
                dataType == typeof(double) || dataType == typeof(double?) ||
                dataType == typeof(float) || dataType == typeof(float?))
                return "NumberBox";
            
            // DateTime → DatePicker/TimePicker editor
            if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
                return "DatePicker";
            
            // Default → TextBox editor
            return "TextBox";
        }
        
        return "TextBox"; // Default fallback
    }

    /// <summary>
    /// ConvertBack nie je potrebný pre tento converter
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException("DataTypeToEditorTypeConverter does not support ConvertBack");
    }
}

/// <summary>
/// Konvertor pre formátovanie hodnôt na display text
/// FUNCTIONAL: Pure formatter bez side effects
/// </summary>
internal sealed class ValueToDisplayTextConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje hodnotu na zobrazovací text s formátovaním
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return string.Empty;

        // Format parameter môže obsahovať format string
        var format = parameter as string;

        try
        {
            // DateTime formatting
            if (value is DateTime dateTime)
            {
                return dateTime.ToString(format ?? "yyyy-MM-dd HH:mm");
            }

            // Decimal/numeric formatting
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString(format ?? "N2");
            }

            if (value is double doubleValue)
            {
                return doubleValue.ToString(format ?? "N2");
            }

            if (value is float floatValue)
            {
                return floatValue.ToString(format ?? "N2");
            }

            // Boolean formatting
            if (value is bool boolValue)
            {
                return boolValue ? "✓" : "✗";
            }

            // Default string representation
            return value.ToString() ?? string.Empty;
        }
        catch
        {
            // Ak formátovanie zlyhá, použije základný ToString
            return value.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// ConvertBack pre parsing hodnôt z text editoru
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var text = value?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            // Return null for nullable types, default for value types
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return null;
            
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        try
        {
            // Parse na základe target type
            if (targetType == typeof(int) || targetType == typeof(int?))
                return int.Parse(text);

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return decimal.Parse(text);

            if (targetType == typeof(double) || targetType == typeof(double?))
                return double.Parse(text);

            if (targetType == typeof(float) || targetType == typeof(float?))
                return float.Parse(text);

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return bool.Parse(text);

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return DateTime.Parse(text);

            // Default string
            return text;
        }
        catch
        {
            // Ak parsing zlyhá, vráti null alebo default hodnotu
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return null;
            
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : text;
        }
    }
}

/// <summary>
/// Konvertor pre validáciu border colors
/// FUNCTIONAL: Pure validator pre cell styling
/// </summary>
internal sealed class ValidationStateToBorderBrushConverter : IValueConverter
{
    /// <summary>
    /// Konvertuje validation state na border brush
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isValid)
        {
            if (!isValid)
            {
                // Red border pre invalid cells
                return Application.Current.Resources["SystemControlErrorTextForegroundBrush"] ?? 
                       new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        // Default border brush pre valid cells
        return Application.Current.Resources["SystemControlForegroundBaseLowBrush"] ?? 
               new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    /// <summary>
    /// ConvertBack nie je potrebný
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException("ValidationStateToBorderBrushConverter does not support ConvertBack");
    }
}