using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;

/// <summary>
/// DDD: Value object representing column information for ListView-based DataGrid
/// ENTERPRISE: High-performance column definition without external dependencies
/// IMMUTABLE: Thread-safe column metadata
/// </summary>
internal sealed record ColumnInfo
{
    /// <summary>Column header text</summary>
    public string Header { get; init; } = string.Empty;

    /// <summary>Column width specification</summary>
    public ColumnWidth Width { get; init; } = ColumnWidth.Auto();

    /// <summary>Is column read-only</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>Allow sorting on this column</summary>
    public bool CanUserSort { get; init; } = true;

    /// <summary>Allow resizing this column</summary>
    public bool CanUserResize { get; init; } = true;

    /// <summary>Allow reordering this column</summary>
    public bool CanUserReorder { get; init; } = true;

    /// <summary>Column data binding</summary>
    public Binding? Binding { get; init; }

    /// <summary>Cell template for complex columns</summary>
    public DataTemplate? CellTemplate { get; init; }

    /// <summary>Special column type</summary>
    public SpecialColumnType SpecialType { get; init; } = SpecialColumnType.None;

    /// <summary>Column identifier</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// FACTORY: Create text column
    /// </summary>
    public static ColumnInfo CreateText(string name, string header, ColumnWidth? width = null)
    {
        return new ColumnInfo
        {
            Name = name,
            Header = header,
            Width = width ?? ColumnWidth.Auto(),
            Binding = new Binding { Path = new PropertyPath(name) }
        };
    }

    /// <summary>
    /// FACTORY: Create checkbox column
    /// </summary>
    public static ColumnInfo CreateCheckBox(string name, string header, ColumnWidth? width = null)
    {
        return new ColumnInfo
        {
            Name = name,
            Header = header,
            Width = width ?? ColumnWidth.Pixels(80),
            SpecialType = SpecialColumnType.CheckBox,
            Binding = new Binding { Path = new PropertyPath(name) }
        };
    }

    /// <summary>
    /// FACTORY: Create template column
    /// </summary>
    public static ColumnInfo CreateTemplate(string name, string header, DataTemplate template, ColumnWidth? width = null)
    {
        return new ColumnInfo
        {
            Name = name,
            Header = header,
            Width = width ?? ColumnWidth.Auto(),
            CellTemplate = template
        };
    }
}