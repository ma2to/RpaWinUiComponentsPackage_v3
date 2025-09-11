using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Primitives;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Entities;

/// <summary>
/// DDD: Rich domain entity representing a data grid column
/// ENTERPRISE: Encapsulates column behavior and business rules
/// </summary>
public class GridColumn : Entity
{
    public GridColumn(int id, string name, Type dataType) : base(id)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        IsVisible = true;
        Width = 100;
        SortDirection = SortDirection.None;
    }

    public string Name { get; }
    public string? DisplayName { get; set; }
    public Type DataType { get; }
    public bool IsVisible { get; set; }
    public double Width { get; set; }
    public int DisplayOrder { get; set; }
    public SortDirection SortDirection { get; set; }
    public bool IsReadOnly { get; set; }
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;

    public void Hide()
    {
        IsVisible = false;
        // Domain event could be raised here
    }

    public void Show()
    {
        IsVisible = true;
        // Domain event could be raised here
    }

    public void SetWidth(double width)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        
        Width = width;
        // Domain event could be raised here
    }
}