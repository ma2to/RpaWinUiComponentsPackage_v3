namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// Clean API configuration class pre farby DataGrid elementov
/// Používa sa namiesto internal DataGridColorConfig
/// Všetky farby sú voliteľné - ak nie sú nastavené, použijú sa default hodnoty
/// </summary>
public class ColorConfiguration
{
    /// <summary>Farba pozadia buniek</summary>
    public string? CellBackground { get; set; }
    
    /// <summary>Farba textu v bunkách</summary>
    public string? CellForeground { get; set; }
    
    /// <summary>Farba okrajov buniek</summary>
    public string? CellBorder { get; set; }
    
    /// <summary>Farba pozadia hlavičky</summary>
    public string? HeaderBackground { get; set; }
    
    /// <summary>Farba textu hlavičky</summary>
    public string? HeaderForeground { get; set; }
    
    /// <summary>Farba okrajov hlavičky</summary>
    public string? HeaderBorder { get; set; }
    
    /// <summary>Farba pozadia pri označení/selection</summary>
    public string? SelectionBackground { get; set; }
    
    /// <summary>Farba textu pri označení/selection</summary>
    public string? SelectionForeground { get; set; }
    
    /// <summary>Farba okraja validačných chýb</summary>
    public string? ValidationErrorBorder { get; set; }
    
    /// <summary>Farba pozadia validačných chýb</summary>
    public string? ValidationErrorBackground { get; set; }
    
    /// <summary>Farba okraja pri focus</summary>
    public string? FocusBorder { get; set; }
    
    /// <summary>Farba pozadia pri focus</summary>
    public string? FocusBackground { get; set; }
    
    /// <summary>Farba okraja validačných varovaní</summary>
    public string? ValidationWarningBorder { get; set; }
    
    /// <summary>Farba pozadia validačných varovaní</summary>
    public string? ValidationWarningBackground { get; set; }
    
    /// <summary>Farba okraja validačných informácií</summary>
    public string? ValidationInfoBorder { get; set; }
    
    /// <summary>Farba pozadia validačných informácií</summary>
    public string? ValidationInfoBackground { get; set; }
    
    /// <summary>Povoliť zebra efekt (striedanie farieb riadkov)</summary>
    public bool EnableZebraStripes { get; set; } = false;
    
    /// <summary>Farba pozadia párnych riadkov (zebra efekt)</summary>
    public string? AlternateRowBackground { get; set; }
    
    /// <summary>Farba pozadia pri hover nad riadkom</summary>
    public string? HoverBackground { get; set; }
    
    /// <summary>Farba pozadia status baru</summary>
    public string? StatusBarBackground { get; set; }
    
    /// <summary>Farba textu status baru</summary>
    public string? StatusBarForeground { get; set; }
    
    /// <summary>Používa dark theme</summary>
    public bool UseDarkTheme { get; set; } = false;
    
    // HELPER METHODS - Získanie efektívnych farieb s profesionálnymi defaultmi
    
    /// <summary>Získanie farby pozadia bunky s defaultom</summary>
    public string GetEffectiveCellBackground() => 
        UseDarkTheme ? (CellBackground ?? "#1E1E1E") : (CellBackground ?? "#FFFFFF");
    
    /// <summary>Získanie farby textu bunky s defaultom</summary>
    public string GetEffectiveCellForeground() => 
        UseDarkTheme ? (CellForeground ?? "#FFFFFF") : (CellForeground ?? "#000000");
        
    /// <summary>Získanie farby okraja bunky s defaultom</summary>
    public string GetEffectiveCellBorder() => 
        UseDarkTheme ? (CellBorder ?? "#3E3E42") : (CellBorder ?? "#E0E0E0");
        
    /// <summary>Získanie farby pozadia hlavičky s defaultom</summary>
    public string GetEffectiveHeaderBackground() => 
        UseDarkTheme ? (HeaderBackground ?? "#2D2D30") : (HeaderBackground ?? "#F5F5F5");
        
    /// <summary>Získanie farby textu hlavičky s defaultom</summary>
    public string GetEffectiveHeaderForeground() => 
        UseDarkTheme ? (HeaderForeground ?? "#FFFFFF") : (HeaderForeground ?? "#333333");
        
    /// <summary>Získanie farby pozadia pri selection s defaultom</summary>
    public string GetEffectiveSelectionBackground() => 
        UseDarkTheme ? (SelectionBackground ?? "#0E639C") : (SelectionBackground ?? "#0078D4");
        
    /// <summary>Získanie farby pozadia alternujúcich riadkov s defaultom</summary>
    public string GetEffectiveAlternateRowBackground() => 
        UseDarkTheme ? (AlternateRowBackground ?? "#252526") : (AlternateRowBackground ?? "#FAFAFA");
        
    /// <summary>Získanie farby pozadia pri hover s defaultom</summary>
    public string GetEffectiveHoverBackground() => 
        UseDarkTheme ? (HoverBackground ?? "#3E3E42") : (HoverBackground ?? "#F0F0F0");
        
    /// <summary>Získanie farby okraja validačnej chyby s defaultom</summary>
    public string GetEffectiveValidationErrorBorder() => ValidationErrorBorder ?? "#FF0000";
        
    /// <summary>Získanie farby pozadia validačnej chyby s defaultom</summary>
    public string GetEffectiveValidationErrorBackground() => ValidationErrorBackground ?? "#FFEBEE";
        
    /// <summary>Získanie farby okraja pri focus s defaultom</summary>
    public string GetEffectiveFocusBorder() => FocusBorder ?? "#0078D4";
        
    /// <summary>Získanie farby pozadia pri focus s defaultom</summary>
    public string GetEffectiveFocusBackground() => FocusBackground ?? "#E7F3FF";
}