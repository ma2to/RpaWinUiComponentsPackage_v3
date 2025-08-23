using Microsoft.UI.Xaml.Media;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DomainColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.DataGrid.ColumnDefinition;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.UI.Models;

/// <summary>
/// UI Models for XAML Data Binding
/// FUNCTIONAL: Immutable data structures for UI display
/// REACTIVE: Implements INotifyPropertyChanged for UI updates
/// </summary>

/// <summary>
/// Header model for column display
/// IMMUTABLE: Represents column header in UI
/// </summary>
internal sealed record HeaderUIModel(
    string Name,
    string DisplayName,
    Type DataType,
    bool IsRequired,
    bool IsReadOnly,
    double Width = 120.0)
{
    public string TypeDisplayName => GetTypeDisplayName(DataType);
    
    private static string GetTypeDisplayName(Type type) => type.Name switch
    {
        nameof(String) => "Text",
        nameof(Int32) => "Number",
        nameof(Decimal) => "Decimal", 
        nameof(DateTime) => "Date",
        nameof(Boolean) => "Yes/No",
        _ => type.Name
    };
}

/// <summary>
/// Cell model for individual data cell display
/// IMMUTABLE: Represents single cell in UI
/// </summary>
internal sealed record CellUIModel(
    object? Value,
    string DisplayText,
    Type DataType,
    bool IsValid,
    string? ValidationMessage,
    SolidColorBrush BackgroundBrush,
    SolidColorBrush ForegroundBrush,
    bool IsEditable = true)
{
    public static CellUIModel Create(
        object? value,
        Type dataType,
        bool isValid = true,
        string? validationMessage = null,
        bool isEditable = true)
    {
        var displayText = FormatDisplayText(value, dataType);
        var (background, foreground) = GetUIColors(isValid, isEditable);
        
        return new CellUIModel(
            value,
            displayText,
            dataType,
            isValid,
            validationMessage,
            background,
            foreground,
            isEditable);
    }
    
    private static string FormatDisplayText(object? value, Type dataType)
    {
        if (value == null) return string.Empty;
        
        return dataType.Name switch
        {
            nameof(DateTime) when value is DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
            nameof(Decimal) when value is decimal dec => dec.ToString("F2"),
            nameof(Boolean) when value is bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? string.Empty
        };
    }
    
    private static (SolidColorBrush background, SolidColorBrush foreground) GetUIColors(bool isValid, bool isEditable)
    {
        // THEME: Use system theme colors for consistency
        var background = new SolidColorBrush(isValid 
            ? Microsoft.UI.Colors.Transparent 
            : Microsoft.UI.Colors.LightPink);
            
        var foreground = new SolidColorBrush(isEditable 
            ? Microsoft.UI.Colors.Black 
            : Microsoft.UI.Colors.Gray);
            
        return (background, foreground);
    }
}

/// <summary>
/// Row model for complete data row display
/// OBSERVABLE: Implements change notification for UI reactivity
/// </summary>
internal sealed class RowUIModel : INotifyPropertyChanged
{
    private readonly List<CellUIModel> _cells = new();
    private bool _isSelected;
    private bool _hasValidationErrors;
    
    public RowUIModel(int index, IReadOnlyList<CellUIModel> cells)
    {
        Index = index;
        _cells.AddRange(cells);
        _hasValidationErrors = cells.Any(c => !c.IsValid);
    }
    
    public int Index { get; }
    public IReadOnlyList<CellUIModel> Cells => _cells.AsReadOnly();
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool HasValidationErrors
    {
        get => _hasValidationErrors;
        private set
        {
            if (_hasValidationErrors != value)
            {
                _hasValidationErrors = value;
                OnPropertyChanged();
            }
        }
    }
    
    public SolidColorBrush RowBackgroundBrush => new(HasValidationErrors 
        ? Microsoft.UI.Colors.MistyRose 
        : Microsoft.UI.Colors.Transparent);
    
    /// <summary>
    /// Update cell validation status
    /// REACTIVE: Triggers UI updates when validation changes
    /// </summary>
    public void UpdateValidation(IReadOnlyList<bool> cellValidations)
    {
        var newHasErrors = false;
        
        for (int i = 0; i < Math.Min(_cells.Count, cellValidations.Count); i++)
        {
            var isValid = cellValidations[i];
            if (!isValid) newHasErrors = true;
            
            // Create updated cell model
            var currentCell = _cells[i];
            if (currentCell.IsValid != isValid)
            {
                _cells[i] = currentCell with { IsValid = isValid };
            }
        }
        
        HasValidationErrors = newHasErrors;
        OnPropertyChanged(nameof(Cells));
        OnPropertyChanged(nameof(RowBackgroundBrush));
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Main UI data context for the DataGrid
/// OBSERVABLE: Central UI state management
/// </summary>
internal sealed class DataGridUIContext : INotifyPropertyChanged
{
    private readonly ObservableCollection<HeaderUIModel> _headers = new();
    private readonly ObservableCollection<RowUIModel> _rows = new();
    private bool _isLoading;
    private string _statusText = string.Empty;
    private bool _hasData;
    private int _totalRowCount;
    
    public ObservableCollection<HeaderUIModel> Headers => _headers;
    public ObservableCollection<RowUIModel> Rows => _rows;
    
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool HasData
    {
        get => _hasData;
        set
        {
            if (_hasData != value)
            {
                _hasData = value;
                OnPropertyChanged();
            }
        }
    }
    
    public int TotalRowCount
    {
        get => _totalRowCount;
        set
        {
            if (_totalRowCount != value)
            {
                _totalRowCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusSummary));
            }
        }
    }
    
    public string StatusSummary => $"{TotalRowCount} rows • {Headers.Count} columns";
    
    /// <summary>
    /// Initialize headers from domain column definitions
    /// FUNCTIONAL: Pure transformation from domain to UI models
    /// </summary>
    public void SetHeaders(IReadOnlyList<DomainColumnDefinition> columns)
    {
        _headers.Clear();
        
        // AUTOMATIC VALIDATION ALERTS: Add ValidationAlerts column if not present
        var columnsWithValidation = columns.ToList();
        if (!columns.Any(c => c.IsValidationColumn))
        {
            var validationColumn = new DomainColumnDefinition(
                Name: "ValidationAlerts",
                DataType: typeof(string),
                IsRequired: false,
                IsReadOnly: true,
                DefaultValue: "",
                MaxLength: null,
                DisplayFormat: null,
                ValidationPattern: null,
                DisplayName: "Errors",
                Width: 100,
                IsValidationColumn: true,
                IsDeleteColumn: false
            );
            
            // Add ValidationAlerts as second-to-last column (before delete column if present)
            if (columns.Any(c => c.IsDeleteColumn))
            {
                var deleteColumnIndex = columnsWithValidation.FindIndex(c => c.IsDeleteColumn);
                columnsWithValidation.Insert(deleteColumnIndex, validationColumn);
            }
            else
            {
                columnsWithValidation.Add(validationColumn);
            }
        }
        
        var uiHeaders = columnsWithValidation.Select(col => new HeaderUIModel(
            col.Name,
            col.DisplayName ?? col.Name, // Use DisplayName if available, fallback to Name
            col.DataType,
            col.IsRequired,
            col.IsReadOnly));
            
        foreach (var header in uiHeaders)
        {
            _headers.Add(header);
        }
        
        OnPropertyChanged(nameof(StatusSummary));
    }
    
    /// <summary>
    /// Update rows from domain data
    /// FUNCTIONAL: Transform domain data to UI models
    /// </summary>
    public void SetRows(IReadOnlyList<DataRow> domainRows)
    {
        _rows.Clear();
        
        for (int i = 0; i < domainRows.Count; i++)
        {
            var domainRow = domainRows[i];
            var cells = CreateCellsFromDomainRow(domainRow);
            var uiRow = new RowUIModel(i, cells);
            _rows.Add(uiRow);
        }
        
        TotalRowCount = domainRows.Count;
        HasData = domainRows.Count > 0;
        StatusText = HasData ? $"Loaded {domainRows.Count} rows" : "No data";
    }
    
    /// <summary>
    /// Clear all data
    /// UI: Reset to empty state
    /// </summary>
    public void Clear()
    {
        _rows.Clear();
        TotalRowCount = 0;
        HasData = false;
        StatusText = "No data";
    }
    
    private IReadOnlyList<CellUIModel> CreateCellsFromDomainRow(DataRow domainRow)
    {
        var cells = new List<CellUIModel>();
        
        foreach (var header in _headers)
        {
            var value = domainRow.Data.TryGetValue(header.Name, out var val) ? val : null;
            var cell = CellUIModel.Create(value, header.DataType, true, null, !header.IsReadOnly);
            cells.Add(cell);
        }
        
        return cells.AsReadOnly();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}