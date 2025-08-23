namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// Clean API configuration class pre výkonnostné nastavenia DataGrid
/// Používa sa namiesto internal GridThrottlingConfig
/// </summary>
public class PerformanceConfiguration
{
    /// <summary>Počet riadkov od ktorého sa zapne virtualizácia</summary>
    public int? VirtualizationThreshold { get; set; }
    
    /// <summary>Veľkosť dávky pri batch operáciách</summary>
    public int? BatchSize { get; set; }
    
    /// <summary>Oneskorenie pri renderovaní UI (milliseconds)</summary>
    public int? RenderDelayMs { get; set; }
    
    /// <summary>Throttling delay pre search operácie (milliseconds)</summary>
    public int? SearchThrottleMs { get; set; }
    
    /// <summary>Throttling delay pre validation operácie (milliseconds)</summary>
    public int? ValidationThrottleMs { get; set; }
    
    /// <summary>Maximálny počet search history položiek</summary>
    public int? MaxSearchHistoryItems { get; set; }
    
    /// <summary>Je zapnuté UI throttling pre lepšiu responzivitu</summary>
    public bool? EnableUIThrottling { get; set; }
    
    /// <summary>Je zapnutá lazy loading pre veľké datasety</summary>
    public bool? EnableLazyLoading { get; set; }
    
    /// <summary>Interval aktualizácie UI v milisekundách</summary>
    public int? UIUpdateIntervalMs { get; set; }
    
    /// <summary>Je zapnuté agresívne riadenie pamäte</summary>
    public bool? EnableAggressiveMemoryManagement { get; set; }
    
    /// <summary>Je zapnuté multi-level cache</summary>
    public bool? EnableMultiLevelCaching { get; set; }
    
    /// <summary>Interval čistenia cache v milisekundách</summary>
    public int? CacheCleanupIntervalMs { get; set; }
    
    /// <summary>Veľkosť dávky pri hromadných operáciách</summary>
    public int? BulkOperationBatchSize { get; set; }
    
    /// <summary>Je zapnuté spracovanie na pozadí</summary>
    public bool? EnableBackgroundProcessing { get; set; }
    
    /// <summary>Maximálny počet riadkov pre validáciu v reálnom čase</summary>
    public int? MaxRowsForRealtimeValidation { get; set; }
    
    /// <summary>Veľkosť buffer pre virtualizáciu</summary>
    public int? VirtualizationBufferSize { get; set; }
}
