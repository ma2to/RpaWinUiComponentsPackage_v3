namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// Clean API configuration class pre definíciu stĺpca DataGrid
/// Používa sa namiesto internal GridColumnDefinition
/// </summary>
public class ColumnConfiguration
{
    /// <summary>Názov stĺpca (povinný)</summary>
    public string? Name { get; set; }
    
    /// <summary>Dátový typ stĺpca</summary>
    public Type? Type { get; set; }
    
    /// <summary>Šírka stĺpca v pixeloch</summary>
    public int? Width { get; set; }
    
    /// <summary>Zobrazovaný názov stĺpca (ak sa líši od Name)</summary>
    public string? DisplayName { get; set; }
    
    /// <summary>Je stĺpec povinný pri validácii</summary>
    public bool? IsRequired { get; set; }
    
    /// <summary>Je to stĺpec pre zobrazenie validačných chýb</summary>
    public bool? IsValidationColumn { get; set; }
    
    /// <summary>Je to stĺpec s tlačidlom na zmazanie riadka</summary>
    public bool? IsDeleteColumn { get; set; }
    
    /// <summary>Je to stĺpec s checkboxom</summary>
    public bool? IsCheckboxColumn { get; set; }
    
    /// <summary>Minimálna šírka stĺpca</summary>
    public int? MinWidth { get; set; }
    
    /// <summary>Maximálna šírka stĺpca</summary>
    public int? MaxWidth { get; set; }
    
    /// <summary>Je stĺpec editovateľný</summary>
    public bool? IsReadOnly { get; set; }
    
    /// <summary>Je stĺpec viditeľný</summary>
    public bool? IsVisible { get; set; }
    
    /// <summary>Poradie stĺpca</summary>
    public int? Order { get; set; }
    
    /// <summary>Maximálna dĺžka hodnoty v stĺpci</summary>
    public int? MaxLength { get; set; }
    
    /// <summary>Môže sa stĺpec sortovať</summary>
    public bool? CanSort { get; set; }
    
    /// <summary>Môže sa stĺpec filtrovať</summary>
    public bool? CanFilter { get; set; }
    
    /// <summary>Môže sa stĺpec meniť veľkosť</summary>
    public bool? CanResize { get; set; }
    
    /// <summary>Default hodnota pre nové bunky</summary>
    public object? DefaultValue { get; set; }
    
    /// <summary>Format pre zobrazenie hodnôt</summary>
    public string? DisplayFormat { get; set; }
    
    /// <summary>Validačný pattern pre stĺpec</summary>
    public string? ValidationPattern { get; set; }
}