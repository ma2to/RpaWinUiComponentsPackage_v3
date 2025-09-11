using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
// CLEAN PUBLIC API - Single using statement per component!
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Configuration;
// Internal types - temporarily needed until moved to main namespace
using ColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition;
using ColumnWidth = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnWidth;
using ColumnValidationRule = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnValidationRule;
using ColorConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI.ColorConfiguration;
using ValidationConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation.ValidationConfiguration;
using PerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration.PerformanceConfiguration;
using ImportResult = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations.ImportResult;
using ValidationError = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results.ValidationError;
using ValidationProgress = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations.ValidationProgress;
using LoggingOptions = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Logging.LoggingOptions;
// Result<T> is generic, so no type alias needed

namespace RpaWinUiComponents.Demo;

/// <summary>
/// 🎯 MODERN API DEMO APPLICATION
/// 
/// Táto demo aplikácia ukazuje použitie nového modern API s jedným using statementom.
/// Fokus je na professional použitie Clean Architecture bez legacy vrstiev.
/// </summary>
public sealed partial class MainWindow : Window
{
    #region Private Fields

    private readonly ILogger<MainWindow> _logger;
    private readonly ILogger _fileLogger;
    private readonly System.Text.StringBuilder _logOutput = new();
    private AdvancedWinUiDataGrid? _testDataGrid;
    private bool _isGridInitialized = false;

    #endregion

    #region Constructor and Initialization

    public MainWindow()
    {
        this.InitializeComponent();
        
        // Setup comprehensive logging for SENIOR DEVELOPER debugging
        _logger = App.LoggerFactory?.CreateLogger<MainWindow>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<MainWindow>.Instance;
        
        var loggerOptions = new LoggerOptions
        {
            LogDirectory = Path.Combine(Path.GetTempPath(), "RpaWinUiDemo"),
            BaseFileName = "demo",
            MaxFileSizeMB = 5
        };
        _fileLogger = LoggerAPI.CreateFileLogger(_logger, loggerOptions);
        
        // SENIOR DEV: Log initial logger setup to verify it works
        _logger?.LogInformation("[DEMO-SETUP] MainWindow logger initialized - Type: {LoggerType}", _logger?.GetType()?.Name ?? "null");
        _logger?.LogDebug("[DEMO-SETUP] File logger setup: Directory={Directory}, FileName={FileName}", loggerOptions.LogDirectory, loggerOptions.BaseFileName);

        AddLogMessage("🚀 Demo application started");
        AddLogMessage("✅ Modern Public API - No Legacy Layers");
        AddLogMessage($"📂 File logging: {loggerOptions.LogDirectory}");
    }

    #endregion

    #region UI Event Handlers

    private async void InitButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AddLogMessage("🔧 MODERN API DEMO: Basic initialization...");
            
            // SENIOR DEV: Detailed pre-initialization logging
            _logger?.LogInformation("[DEMO-INIT] Basic initialization started");
            _logger?.LogDebug("[DEMO-INIT] Logger instance check - HasLogger: {HasLogger}, LoggerType: {LoggerType}", 
                _logger != null, _logger?.GetType()?.Name ?? "null");

            // 📋 STEP 1: Define columns using modern ColumnDefinition factory methods
            var columns = new List<ColumnDefinition>
            {
                // Standard data columns using proper factory methods
                ColumnDefinition.Numeric<int>("ID", "ID") with { IsReadOnly = true, Width = ColumnWidth.Pixels(80) },
                ColumnDefinition.Required("Name", typeof(string), "Name") with { Width = ColumnWidth.Pixels(200) },
                ColumnDefinition.Text("Email", "Email") with { Width = ColumnWidth.Pixels(250) },
                ColumnDefinition.CheckBox("Active", "Active"),
                
                // Special columns using factory methods
                ColumnDefinition.ValidAlerts("Validation", 120),
                ColumnDefinition.DeleteRow("Delete")
            };

            _logger?.LogDebug("[DEMO-INIT] Created {ColumnCount} column definitions", columns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                _logger?.LogDebug("[DEMO-INIT] Column[{Index}]: Name='{Name}', Type='{DataType}'", 
                    i, columns[i]?.Name ?? "null", columns[i]?.DataType?.Name ?? "null");
            }

            // 📋 STEP 2: Create DataGrid for UI mode with professional logging options
            _logger?.LogInformation("[DEMO-INIT] About to call AdvancedWinUiDataGrid.CreateForUI with professional logging options");
            _logger?.LogDebug("[DEMO-INIT] Calling CreateForUI with logger type: {LoggerType}", _logger?.GetType()?.Name ?? "null");
            
            // SENIOR DEVELOPER: Create comprehensive logging options for development/debugging
            var loggingOptions = LoggingOptions.Development with
            {
                LogMethodParameters = true,       // Log all method parameters for debugging
                LogPerformanceMetrics = true,     // Track performance for optimization
                LogConfigurationDetails = true,   // Log configuration for troubleshooting
                LogUnhandledErrors = true,        // Capture all unhandled errors
                LogResultPatternStates = true     // Log Result<T> success/failure states
            };
            
            _logger?.LogDebug("[DEMO-INIT] Created LoggingOptions - Strategy: {Strategy}, LogParameters: {LogParams}, LogPerformance: {LogPerf}", 
                loggingOptions.Strategy, loggingOptions.LogMethodParameters, loggingOptions.LogPerformanceMetrics);
            
            _testDataGrid = AdvancedWinUiDataGrid.CreateForUI(_logger!, loggingOptions); // Use null-forgiving operator since _logger is initialized in constructor
            
            _logger?.LogInformation("[DEMO-INIT] AdvancedWinUiDataGrid.CreateForUI completed - DataGrid: {DataGridType}", 
                _testDataGrid?.GetType()?.Name ?? "null");

            // 📋 STEP 3: Use Light theme and standard validation
            var colorConfig = ColorConfiguration.Light;
            var validationConfig = new ValidationConfiguration
            {
                EnableValidation = true,
                EnableRealTimeValidation = true,
                StrictValidation = false
            };
            var performanceConfig = new PerformanceConfiguration
            {
                EnableVirtualization = true,
                VirtualizationThreshold = 1000
            };
            
            _logger?.LogDebug("[DEMO-INIT] Configuration created - ColorConfig: {ColorConfig}, ValidationEnabled: {ValidationEnabled}",
                colorConfig?.GetType()?.Name ?? "null", validationConfig?.EnableValidation ?? false);
            
            AddLogMessage($"📊 Initializing with {columns.Count} columns using modern API");
            
            _logger?.LogInformation("[DEMO-INIT] About to call InitializeAsync on DataGrid");
            var result = await _testDataGrid.InitializeAsync(columns, colorConfig, validationConfig, performanceConfig, minimumRows: 20);
            _logger?.LogInformation("[DEMO-INIT] InitializeAsync completed - Success: {Success}", result.IsSuccess);
            
            if (result.IsSuccess)
            {
                _isGridInitialized = true;
                AddLogMessage("✅ MODERN API DEMO: Basic initialization completed!");
                _logger?.LogInformation("[DEMO-INIT] Initialization SUCCESS - Grid is ready");
            }
            else
            {
                AddLogMessage($"❌ MODERN API DEMO: Initialization failed: {result.Error}");
                _logger?.LogError("[DEMO-INIT] Initialization FAILED - Error: {Error}", result.Error ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
            _logger?.LogError(ex, "[DEMO-INIT] EXCEPTION in initialization - Message: {ErrorMessage}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            
            // SENIOR DEV: Log inner exception details if present
            if (ex.InnerException != null)
            {
                _logger?.LogError("[DEMO-INIT] Inner Exception: {InnerMessage}, InnerStackTrace: {InnerStackTrace}",
                    ex.InnerException.Message, ex.InnerException.StackTrace);
            }
        }
    }

    private async void InitWithValidationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AddLogMessage("🔧 MODERN API DEMO: Advanced initialization with dark theme...");

            // 📋 Advanced columns with comprehensive validation
            var columns = new List<ColumnDefinition>
            {
                // Standard data columns using factory methods with validation
                ColumnDefinition.Numeric<int>("ProductID", "Product ID") with { Width = ColumnWidth.Pixels(100) },
                ColumnDefinition.WithValidation("ProductName", typeof(string),
                    ColumnValidationRule.Required(),
                    ColumnValidationRule.MaxLength(50)) with { Width = ColumnWidth.Pixels(200) },
                ColumnDefinition.Text("Price", "Price") with { Width = ColumnWidth.Pixels(120) },
                ColumnDefinition.CheckBox("InStock", "In Stock"),
                
                // Special columns with proper configuration
                ColumnDefinition.ValidAlerts("Alerts", 100),
                ColumnDefinition.DeleteRow("🗑️") with { Width = ColumnWidth.Pixels(50) }
            };

            // 📋 Create DataGrid for UI mode with strict validation logging
            // SENIOR DEVELOPER: Use high-performance logging for validation-heavy scenarios
            var loggingOptions = LoggingOptions.Development with
            {
                LogMethodParameters = false,      // Reduce noise for validation scenarios
                LogPerformanceMetrics = true,     // Track validation performance
                LogConfigurationDetails = true,   // Log configuration for troubleshooting
                LogUnhandledErrors = true,        // Capture all unhandled errors
                LogResultPatternStates = true,    // Essential for validation results
                CategoryPrefix = "ValidationDemo" // Custom prefix for easy filtering
            };
            
            _testDataGrid = AdvancedWinUiDataGrid.CreateForUI(_logger!, loggingOptions); // Use null-forgiving operator since _logger is initialized in constructor

            // 📋 CUSTOM CONFIGURATION: Dark theme + strict validation
            var colorConfig = ColorConfiguration.Dark;
            var validationConfig = new ValidationConfiguration
            {
                EnableValidation = true,
                EnableRealTimeValidation = true,
                StrictValidation = true,  // Strict validation mode
                ValidateEmptyRows = false
            };
            var performanceConfig = new PerformanceConfiguration
            {
                EnableVirtualization = true,
                VirtualizationThreshold = 1000,
                EnableBackgroundProcessing = true
            };
            
            AddLogMessage($"📊 Using custom configuration: Dark theme + Strict validation");
            
            var result = await _testDataGrid.InitializeAsync(columns, colorConfig, validationConfig, performanceConfig, minimumRows: 20);
            
            if (result.IsSuccess)
            {
                _isGridInitialized = true;
                AddLogMessage("✅ MODERN API DEMO: Advanced initialization completed!");
            }
            else
            {
                AddLogMessage($"❌ MODERN API DEMO: Advanced initialization failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
            _logger?.LogError(ex, "Advanced initialization failed");
        }
    }

    private async void ImportDictionaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isGridInitialized || _testDataGrid == null)
        {
            AddLogMessage("⚠️ Grid must be initialized first!");
            return;
        }

        try
        {
            AddLogMessage("📥 MODERN API DEMO: Dictionary import...");

            // Sample data for demonstration
            var testData = new List<Dictionary<string, object?>>
            {
                new() { ["ID"] = 1, ["Name"] = "John Doe", ["Email"] = "john@example.com", ["Active"] = true },
                new() { ["ID"] = 2, ["Name"] = "Jane Smith", ["Email"] = "jane@example.com", ["Active"] = false },
                new() { ["ID"] = 3, ["Name"] = "Bob Wilson", ["Email"] = "bob@example.com", ["Active"] = true }
            };

            var result = await _testDataGrid.ImportFromDictionaryAsync(testData);
            
            if (result.IsSuccess)
            {
                AddLogMessage($"✅ Import successful: {testData.Count} rows imported");
            }
            else
            {
                AddLogMessage($"❌ Import failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
        }
    }

    private async void ExportDictionaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isGridInitialized || _testDataGrid == null)
        {
            AddLogMessage("⚠️ Grid must be initialized first!");
            return;
        }

        try
        {
            AddLogMessage("📤 MODERN API DEMO: Dictionary export...");
            
            var result = await _testDataGrid.ExportToDictionaryAsync();
            
            if (result.IsSuccess)
            {
                AddLogMessage($"✅ Export completed: {result.Value?.Count ?? 0} rows exported");
            }
            else
            {
                AddLogMessage($"❌ Export failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
        }
    }

    private async void ClearDataButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isGridInitialized || _testDataGrid == null)
        {
            AddLogMessage("⚠️ Grid must be initialized first!");
            return;
        }

        try
        {
            AddLogMessage("🗑️ MODERN API DEMO: Clearing data...");
            
            // Clear by exporting empty and then importing empty data
            var emptyData = new List<Dictionary<string, object?>>();
            var result = await _testDataGrid.ImportFromDictionaryAsync(emptyData);
            
            if (result.IsSuccess)
            {
                AddLogMessage("✅ Data cleared successfully");
            }
            else
            {
                AddLogMessage($"❌ Clear failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
        }
    }

    private async void RefreshUIButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isGridInitialized || _testDataGrid == null)
        {
            AddLogMessage("⚠️ Grid must be initialized first!");
            return;
        }

        try
        {
            AddLogMessage("🔄 MODERN API DEMO: UI refresh...");
            
            // Modern API doesn't expose RefreshUIAsync - UI updates automatically
            AddLogMessage("✅ UI refreshes automatically in modern API");
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
        }
    }

    private void GetStatsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isGridInitialized || _testDataGrid == null)
        {
            AddLogMessage("⚠️ Grid must be initialized first!");
            return;
        }

        try
        {
            AddLogMessage("📊 MODERN API DEMO: Getting statistics...");

            var totalRows = _testDataGrid.GetRowCount();
            var totalColumns = _testDataGrid.GetColumnCount();

            AddLogMessage($"📊 STATISTICS:");
            AddLogMessage($"   • Total rows: {totalRows}");
            AddLogMessage($"   • Total columns: {totalColumns}");
            AddLogMessage($"   • Initialized: {_isGridInitialized}");
            AddLogMessage("✅ Statistics displayed");
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
        }
    }

    // Dummy handlers for other buttons to prevent XAML errors
    private void ImportDataTableButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 DataTable import - Available in full API");

    private void ExportDataTableButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 DataTable export - Available in full API");

    private void ValidateAllButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Validation features - Available in full API");

    private void BatchValidationButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Batch validation - Available in full API");

    private void UpdateValidationUIButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Validation UI updates - Available in full API");

    private void InvalidateLayoutButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Layout invalidation - Available in full API");

    private void SmartDeleteButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Smart delete operations - Available in full API");

    private void CompactRowsButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Row compacting - Available in full API");

    private void PasteDataButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Data pasting - Available in full API");

    private void ApplyDarkThemeButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Dynamic theme switching - Available in full API");

    private void ResetColorsButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Color reset - Available in full API");

    private void TestSelectiveColorsButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Selective coloring - Available in full API");

    private void TestBorderOnlyButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Border-only styling - Available in full API");

    private void TestValidationOnlyButton_Click(object sender, RoutedEventArgs e) => 
        AddLogMessage("💡 Validation-only styling - Available in full API");

    #endregion

    #region Helper Methods

    private void AddLogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logLine = $"[{timestamp}] {message}";
        
        _logOutput.AppendLine(logLine);
        
        // Update UI on main thread
        this.DispatcherQueue.TryEnqueue(() =>
        {
            if (LogOutput != null)
            {
                LogOutput.Text = _logOutput.ToString();
                
                // Auto-scroll to bottom
                LogScrollViewer?.ChangeView(null, LogScrollViewer.ScrollableHeight, null);
            }
        });
        
        // Log to file as well
        _fileLogger?.LogInformation(message);
    }

    #endregion
}