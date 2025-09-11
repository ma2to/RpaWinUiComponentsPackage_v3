using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;

/// <summary>
/// ENTERPRISE: Log rotation result for logger
/// </summary>
public record RotationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? OldFilePath { get; init; }
    public string? NewFilePath { get; init; }
    public DateTime RotationTime { get; init; } = DateTime.UtcNow;
    public long FileSizeBytes { get; init; }
    
    public static RotationResult Success(string? oldPath, string? newPath, long fileSize) =>
        new()
        {
            IsSuccess = true,
            OldFilePath = oldPath,
            NewFilePath = newPath,
            FileSizeBytes = fileSize
        };
        
    public static RotationResult Failed(string error) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = error
        };
}

/// <summary>
/// ENTERPRISE: Cleanup result for logger
/// </summary>
public record CleanupResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int FilesDeleted { get; init; }
    public long SpaceFreedBytes { get; init; }
    public DateTime CleanupTime { get; init; } = DateTime.UtcNow;
    
    public static CleanupResult Success(int filesDeleted, long spaceFreed) =>
        new()
        {
            IsSuccess = true,
            FilesDeleted = filesDeleted,
            SpaceFreedBytes = spaceFreed
        };
        
    public static CleanupResult Failed(string error) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = error
        };
}

/// <summary>
/// ENTERPRISE: Log file information
/// </summary>
public record LogFileInfo
{
    public string FilePath { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public DateTime CreatedTime { get; init; }
    public DateTime ModifiedTime { get; init; }
    public bool IsActive { get; init; }
    public int LineCount { get; init; }
}