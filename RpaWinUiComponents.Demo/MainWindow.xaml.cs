using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
// 🎯 CLEAN PUBLIC API - Only two using statements needed!
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
// ✅ NO MORE INTERNAL TYPE ALIASES - Clean API achieved!

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

    private readonly ILogger<MainWindow> _baseLogger;
    private readonly ILogger _advancedWinUiLogger;
    private readonly System.Text.StringBuilder _logOutput = new();
    private AdvancedWinUiDataGrid? _testDataGrid;
    private bool _isGridInitialized = false;

    #endregion

    #region Constructor and Initialization

    public MainWindow()
    {
        this.InitializeComponent();
        
        // 📋 STEP 1: Setup base Microsoft.Extensions.Logging logger
        _baseLogger = App.LoggerFactory?.CreateLogger<MainWindow>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<MainWindow>.Instance;
        
        // 📋 STEP 2: Create AdvancedWinUiLogger with file rotation (10MB limit as requested)
        string logDirectory = Path.Combine(Path.GetTempPath(), "RpaWinUiDemo");
        string baseFileName = "AdvancedDataGridDemo";
        int maxFileSizeMB = 10; // 10MB rotation as requested
        
        _advancedWinUiLogger = LoggerAPI.CreateFileLogger(
            externalLogger: _baseLogger,    // Pass base logger to AdvancedWinUiLogger
            logDirectory: logDirectory,
            baseFileName: baseFileName,
            maxFileSizeMB: maxFileSizeMB);
        
        // SENIOR DEV: Log initial setup to verify configuration
        _baseLogger.LogInformation("🔧 [DEMO-SETUP] Base logger initialized - Type: {LoggerType}", _baseLogger.GetType().Name);
        _advancedWinUiLogger.LogInformation("📁 [DEMO-SETUP] AdvancedWinUiLogger created with 10MB rotation");
        _advancedWinUiLogger.LogInformation("📂 [DEMO-SETUP] Log directory: {LogDirectory}", logDirectory);
        _advancedWinUiLogger.LogInformation("📄 [DEMO-SETUP] Base filename: {BaseFileName} (will create {BaseFileName}.log)", baseFileName);
        _advancedWinUiLogger.LogInformation("🔄 [DEMO-SETUP] File rotation: Every {MaxSizeMB}MB", maxFileSizeMB);

        AddLogMessage("🚀 Demo application started");
        AddLogMessage("✅ CLEAN PUBLIC API - No more internal type aliases needed!");
        AddLogMessage($"📂 File logging with 10MB rotation: {logDirectory}");
        AddLogMessage($"📄 Log files: {baseFileName}.log (rotating)");
    }

    #endregion

    #region UI Event Handlers

    private async void InitButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AddLogMessage("🔧 MODERN API DEMO: Basic initialization...");
            
            // SENIOR DEV: Detailed pre-initialization logging
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] Basic initialization started");
            _advancedWinUiLogger.LogDebug("[DEMO-INIT] Using AdvancedWinUiLogger - Type: {LoggerType}", _advancedWinUiLogger.GetType().Name);

            // 📋 STEP 1: Define columns using CLEAN PUBLIC API - simple DataGridColumn objects
            var columns = new List<DataGridColumn>
            {
                // Standard data columns using simple public API
                new DataGridColumn { Name = "ID", Header = "ID", DataType = typeof(int), ColumnType = DataGridColumnType.Numeric, IsReadOnly = true, Width = 80 },
                new DataGridColumn { Name = "Name", Header = "Name", DataType = typeof(string), ColumnType = DataGridColumnType.Required, Width = 200 },
                new DataGridColumn { Name = "Email", Header = "Email", DataType = typeof(string), ColumnType = DataGridColumnType.Text, Width = 250 },
                new DataGridColumn { Name = "Active", Header = "Active", DataType = typeof(bool), ColumnType = DataGridColumnType.CheckBox, Width = 80 },
                
                // Special columns using simple public API
                new DataGridColumn { Name = "Validation", Header = "Validation", DataType = typeof(string), ColumnType = DataGridColumnType.ValidAlerts, Width = 120 },
                new DataGridColumn { Name = "Delete", Header = "Delete", DataType = typeof(bool), ColumnType = DataGridColumnType.DeleteRow, Width = 50 }
            };

            _advancedWinUiLogger.LogDebug("[DEMO-INIT] Created {ColumnCount} column definitions", columns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                _advancedWinUiLogger.LogDebug("[DEMO-INIT] Column[{Index}]: Name='{Name}', Type='{DataType}'", 
                    i, columns[i]?.Name ?? "null", columns[i]?.DataType?.Name ?? "null");
            }

            // 📋 STEP 2: Create simple logging config for AdvancedWinUiDataGrid using CLEAN PUBLIC API
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] Creating DataGridLoggingConfig for AdvancedWinUiDataGrid component");
            
            // SENIOR DEVELOPER: Use simple public API logging config
            var loggingConfig = new DataGridLoggingConfig
            {
                CategoryPrefix = "DataGridDemo",                    // Custom prefix for easy log filtering
                LogMethodParameters = true,                         // Enable detailed method logging for demo
                LogPerformanceMetrics = true,                       // Track performance metrics
                LogErrors = true,                                   // Essential error capture
                MinimumLevel = DataGridLoggingLevel.Debug           // Debug level for comprehensive logging
            };
            
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] DataGridLoggingConfig configured - Prefix: {Prefix}, MethodParams: {LogParams}", 
                loggingConfig.CategoryPrefix, loggingConfig.LogMethodParameters);
            
            // 📋 STEP 3: Create DataGrid using AdvancedWinUiLogger and simple DataGridLoggingConfig
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] Creating AdvancedWinUiDataGrid with clean public API");
            
            _testDataGrid = AdvancedWinUiDataGrid.CreateForUI(_advancedWinUiLogger, loggingConfig);
            
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] AdvancedWinUiDataGrid.CreateForUI completed successfully - DataGrid type: {DataGridType}", 
                _testDataGrid?.GetType()?.Name ?? "null");

            // 📋 STEP 4: Configure DataGrid settings using CLEAN PUBLIC API
            var theme = DataGridTheme.Light;
            var validationConfig = new DataGridValidationConfig
            {
                EnableValidation = true,
                EnableRealTimeValidation = true,
                StrictValidation = false,
                ValidateEmptyRows = false
            };
            var performanceConfig = new DataGridPerformanceConfig
            {
                EnableVirtualization = true,
                VirtualizationThreshold = 1000,
                EnableBackgroundProcessing = false
            };
            
            _advancedWinUiLogger.LogDebug("[DEMO-INIT] Configuration created - Theme: {Theme}, ValidationEnabled: {ValidationEnabled}",
                theme, validationConfig.EnableValidation);
            
            AddLogMessage($"📊 Initializing with {columns.Count} columns using CLEAN PUBLIC API");
            
            // 📋 STEP 5: Initialize DataGrid with clean public API
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] About to call InitializeAsync on DataGrid using clean public API");
            var result = await _testDataGrid.InitializeAsync(columns, theme, validationConfig, performanceConfig, minimumRows: 20);
            _advancedWinUiLogger.LogInformation("[DEMO-INIT] InitializeAsync completed - Success: {Success}, Error: {Error}", 
                result.IsSuccess, result.ErrorMessage ?? "None");
            
            if (result.IsSuccess)
            {
                _isGridInitialized = true;
                AddLogMessage("✅ CLEAN API DEMO: Basic initialization completed with professional logging!");
                _advancedWinUiLogger.LogInformation("[DEMO-INIT] Initialization SUCCESS - Grid is ready using clean public API");

                // SENIOR DEV: Display UI element after successful initialization
                await DisplayDataGridUI();
            }
            else
            {
                AddLogMessage($"❌ CLEAN API DEMO: Initialization failed: {result.ErrorMessage}");
                _advancedWinUiLogger.LogError("[DEMO-INIT] Initialization FAILED - Error: {Error}", result.ErrorMessage ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
            _advancedWinUiLogger.LogError(ex, "[DEMO-INIT] EXCEPTION in initialization - Message: {ErrorMessage}", ex.Message);
            
            // SENIOR DEV: Log inner exception details if present
            if (ex.InnerException != null)
            {
                _advancedWinUiLogger.LogError("[DEMO-INIT] Inner Exception: {InnerMessage}", ex.InnerException.Message);
            }
        }
    }

    private async void InitWithValidationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AddLogMessage("🔧 CLEAN API DEMO: Advanced initialization with dark theme...");

            // 📋 Advanced columns with comprehensive validation using CLEAN PUBLIC API
            var columns = new List<DataGridColumn>
            {
                // Standard data columns using simple public API
                new DataGridColumn { Name = "ProductID", Header = "Product ID", DataType = typeof(int), ColumnType = DataGridColumnType.Numeric, Width = 100 },
                new DataGridColumn { Name = "ProductName", Header = "Product Name", DataType = typeof(string), ColumnType = DataGridColumnType.Required, Width = 200, MaxLength = 50 },
                new DataGridColumn { Name = "Price", Header = "Price", DataType = typeof(decimal), ColumnType = DataGridColumnType.Text, Width = 120 },
                new DataGridColumn { Name = "InStock", Header = "In Stock", DataType = typeof(bool), ColumnType = DataGridColumnType.CheckBox, Width = 80 },
                
                // Special columns using clean public API
                new DataGridColumn { Name = "Alerts", Header = "Alerts", DataType = typeof(string), ColumnType = DataGridColumnType.ValidAlerts, Width = 100 },
                new DataGridColumn { Name = "Delete", Header = "🗑️", DataType = typeof(bool), ColumnType = DataGridColumnType.DeleteRow, Width = 50 }
            };

            // 📋 Create DataGrid for validation demo using CLEAN PUBLIC API
            // SENIOR DEVELOPER: Use optimized logging config for validation scenarios
            var validationLoggingConfig = new DataGridLoggingConfig
            {
                CategoryPrefix = "ValidationDemo",      // Custom prefix for validation demo
                LogMethodParameters = false,            // Reduce noise for validation scenarios
                LogPerformanceMetrics = true,           // Track validation performance - important
                LogErrors = true,                       // Capture all errors
                MinimumLevel = DataGridLoggingLevel.Information // Less verbose for validation scenario
            };
            
            _testDataGrid = AdvancedWinUiDataGrid.CreateForUI(_advancedWinUiLogger, validationLoggingConfig);

            // 📋 CUSTOM CONFIGURATION: Dark theme + strict validation using CLEAN PUBLIC API
            var theme = DataGridTheme.Dark;
            var validationConfig = new DataGridValidationConfig
            {
                EnableValidation = true,
                EnableRealTimeValidation = true,
                StrictValidation = true,  // Strict validation mode
                ValidateEmptyRows = false
            };
            var performanceConfig = new DataGridPerformanceConfig
            {
                EnableVirtualization = true,
                VirtualizationThreshold = 1000,
                EnableBackgroundProcessing = true
            };
            
            AddLogMessage($"📊 Using CLEAN PUBLIC API: Dark theme + Strict validation");
            
            var result = await _testDataGrid.InitializeAsync(columns, theme, validationConfig, performanceConfig, minimumRows: 20);
            
            if (result.IsSuccess)
            {
                _isGridInitialized = true;
                AddLogMessage("✅ CLEAN API DEMO: Advanced initialization completed!");

                // SENIOR DEV: Display UI element after successful initialization
                await DisplayDataGridUI();
            }
            else
            {
                AddLogMessage($"❌ CLEAN API DEMO: Advanced initialization failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"❌ Error: {ex.Message}");
            _advancedWinUiLogger.LogError(ex, "[DEMO-VALIDATION] Advanced initialization failed - Message: {ErrorMessage}", ex.Message);
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
                AddLogMessage($"❌ Import failed: {result.ErrorMessage}");
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
                AddLogMessage($"❌ Export failed: {result.ErrorMessage}");
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
                AddLogMessage($"❌ Clear failed: {result.ErrorMessage}");
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

    #region UI Display Methods

    /// <summary>
    /// SENIOR DEVELOPER: Display actual DataGrid UI component in the container
    /// UI INTEGRATION: Creates actual DataGrid UserControl with table functionality
    /// </summary>
    private async Task DisplayDataGridUI()
    {
        try
        {
            if (_testDataGrid == null)
            {
                _advancedWinUiLogger.LogError("[UI-DISPLAY] Cannot display UI - DataGrid instance is null");
                AddLogMessage("❌ Cannot display UI - DataGrid not initialized");
                return;
            }

            _advancedWinUiLogger.LogInformation("[UI-DISPLAY] Creating DataGrid UI UserControl using public API");

            // SENIOR DEV: Use the new public API method to get DataGrid UI component
            var dataGridUserControl = GetDataGridUserControl();

            _advancedWinUiLogger.LogInformation("[UI-DISPLAY] DataGrid UserControl obtained successfully - Type: {ComponentType}",
                dataGridUserControl.GetType().Name);

            // Replace container content with DataGrid UserControl
            GridContainer.Child = dataGridUserControl;
            AddLogMessage("✅ DataGrid UI displayed - table will load with sample data automatically");

            _advancedWinUiLogger.LogInformation("[UI-DISPLAY] DataGrid UserControl successfully added to container");
        }
        catch (Exception ex)
        {
            _advancedWinUiLogger.LogError(ex, "[UI-DISPLAY] Failed to display DataGrid UI - Error: {ErrorMessage}", ex.Message);
            AddLogMessage($"❌ UI display failed: {ex.Message}");

            // Show error in UI but with more helpful message
            try
            {
                GridContainer.Child = new TextBlock
                {
                    Text = $"❌ DataGrid UI Creation Error:\n\n{ex.Message}\n\n" +
                           "This may be due to missing UI bindings in the component architecture.\n" +
                           "The DataGrid service is still functional for data operations.\n" +
                           "Check logs for technical details.",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                    TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.OrangeRed),
                    FontSize = 12,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                    MaxWidth = 500
                };
            }
            catch
            {
                _advancedWinUiLogger.LogError("[UI-DISPLAY] Critical failure - unable to show error message in UI");
            }
        }
    }

    /// <summary>
    /// SENIOR DEV: Get the DataGrid UI UserControl component using public API
    /// </summary>
    private Microsoft.UI.Xaml.Controls.UserControl GetDataGridUserControl()
    {
        try
        {
            _advancedWinUiLogger.LogInformation("[UI-DISPLAY] Creating DataGrid UI UserControl using public API");

            // Use the new public API method to create the UI component
            var userControl = _testDataGrid.CreateUserControlWithSampleData();

            AddLogMessage("✅ DataGrid UI UserControl created - actual table will load with sample data");
            _advancedWinUiLogger.LogInformation("[UI-DISPLAY] DataGrid UserControl created successfully via public API");

            return userControl;
        }
        catch (Exception ex)
        {
            _advancedWinUiLogger.LogError(ex, "[UI-DISPLAY] Failed to create DataGrid UserControl via public API");
            AddLogMessage($"❌ DataGrid UserControl creation failed: {ex.Message}");

            // Return fallback UserControl
            return new Microsoft.UI.Xaml.Controls.UserControl
            {
                Content = new Microsoft.UI.Xaml.Controls.TextBlock
                {
                    Text = $"❌ DataGrid UserControl Failed: {ex.Message}",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                }
            };
        }
    }

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
        
        // Log to AdvancedWinUiLogger file system as well
        _advancedWinUiLogger?.LogInformation("[UI-MESSAGE] {Message}", message);
    }

    #endregion
}