using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;

/// <summary>
/// ENTERPRISE: Result of clipboard paste operation
/// CLIPBOARD: Contains detailed information about paste results
/// </summary>
public record PasteResult
{
    /// <summary>Whether paste operation was successful</summary>
    public bool Success { get; init; }
    
    /// <summary>Number of new rows inserted</summary>
    public int InsertedRows { get; init; }
    
    /// <summary>Number of existing rows updated</summary>
    public int UpdatedRows { get; init; }
    
    /// <summary>Total number of rows processed from clipboard</summary>
    public int TotalProcessedRows { get; init; }
    
    /// <summary>Detected clipboard format</summary>
    public string DetectedFormat { get; init; } = string.Empty;
    
    /// <summary>Whether clipboard data contained headers</summary>
    public bool HasHeaders { get; init; }
    
    /// <summary>List of errors that occurred during paste</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];
    
    /// <summary>Create successful paste result</summary>
    public static PasteResult CreateSuccess(int inserted, int updated, string format, bool hasHeaders) =>
        new()
        {
            Success = true,
            InsertedRows = inserted,
            UpdatedRows = updated,
            TotalProcessedRows = inserted + updated,
            DetectedFormat = format,
            HasHeaders = hasHeaders
        };
    
    /// <summary>Create failed paste result</summary>
    public static PasteResult CreateFailed(IReadOnlyList<string> errors) =>
        new()
        {
            Success = false,
            Errors = errors
        };
}