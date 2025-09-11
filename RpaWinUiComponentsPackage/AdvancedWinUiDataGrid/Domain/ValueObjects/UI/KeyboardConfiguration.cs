using System;
using System.Collections.Generic;
using Windows.System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;

/// <summary>
/// ENTERPRISE: Keyboard shortcut configuration per documentation
/// </summary>
public record KeyboardShortcutConfiguration
{
    /// <summary>Enable keyboard shortcuts</summary>
    public bool EnableShortcuts { get; init; } = true;
    
    /// <summary>Copy shortcut key</summary>
    public VirtualKey CopyKey { get; init; } = VirtualKey.C;
    
    /// <summary>Paste shortcut key</summary>
    public VirtualKey PasteKey { get; init; } = VirtualKey.V;
    
    /// <summary>Select all shortcut key</summary>
    public VirtualKey SelectAllKey { get; init; } = VirtualKey.A;
    
    /// <summary>Delete row shortcut key</summary>
    public VirtualKey DeleteRowKey { get; init; } = VirtualKey.Delete;
    
    /// <summary>Search shortcut key</summary>
    public VirtualKey SearchKey { get; init; } = VirtualKey.F;
    
    /// <summary>Save shortcut key</summary>
    public VirtualKey SaveKey { get; init; } = VirtualKey.S;
    
    /// <summary>Export shortcut key</summary>
    public VirtualKey ExportKey { get; init; } = VirtualKey.E;
    
    /// <summary>Custom shortcuts</summary>
    public Dictionary<string, VirtualKey> CustomShortcuts { get; init; } = new();
    
    /// <summary>Factory method for default shortcuts</summary>
    public static KeyboardShortcutConfiguration Default => new();
    
    /// <summary>Factory method for disabled shortcuts</summary>
    public static KeyboardShortcutConfiguration Disabled => new() { EnableShortcuts = false };
}


/// <summary>
/// ENTERPRISE: Logger options for logger component
/// </summary>
public record LoggerOptions
{
    /// <summary>Log level</summary>
    public LogLevel LogLevel { get; init; } = LogLevel.Information;
    
    /// <summary>Log directory path</summary>
    public string LogDirectory { get; init; } = "logs";
    
    /// <summary>Base filename for log files</summary>
    public string BaseFileName { get; init; } = "application";
    
    /// <summary>Maximum file size in bytes</summary>
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024; // 10MB
    
    /// <summary>Maximum file size in MB (computed property)</summary>
    public int MaxFileSizeMB => (int)(MaxFileSizeBytes / (1024 * 1024));
    
    /// <summary>Maximum number of log files to keep</summary>
    public int MaxFileCount { get; init; } = 10;
    
    /// <summary>Log file path</summary>
    public string LogFilePath { get; init; } = "logs/application.log";
    
    /// <summary>Enable file rotation</summary>
    public bool EnableRotation { get; init; } = true;
    
    /// <summary>Log format template</summary>
    public string LogFormat { get; init; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message}{NewLine}{Exception}";
    
    /// <summary>Enable async logging</summary>
    public bool EnableAsync { get; init; } = true;
    
    /// <summary>Buffer size for async logging</summary>
    public int BufferSize { get; init; } = 1000;
    
    /// <summary>Factory method for default configuration</summary>
    public static LoggerOptions Default => new();
    
    /// <summary>Factory method for development configuration</summary>
    public static LoggerOptions Development => new()
    {
        LogLevel = LogLevel.Debug,
        MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
        MaxFileCount = 5,
        EnableAsync = false
    };
    
    /// <summary>Factory method for production configuration</summary>
    public static LoggerOptions Production => new()
    {
        LogLevel = LogLevel.Warning,
        MaxFileSizeBytes = 50 * 1024 * 1024, // 50MB
        MaxFileCount = 20,
        EnableAsync = true,
        BufferSize = 5000
    };
}

/// <summary>
/// ENTERPRISE: Log level enumeration
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}