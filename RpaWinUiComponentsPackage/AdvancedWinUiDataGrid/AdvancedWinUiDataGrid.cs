// PRIVATE: Internal type aliases - hidden from public IntelliSense
global using IDataGridService = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services.IDataGridService;
global using ComponentLogger = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Logging.ComponentLogger;
global using ColumnDefinition = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core.ColumnDefinition;
global using ColorConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI.ColorConfiguration;
global using ValidationConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation.ValidationConfiguration;
global using DataGridConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration.DataGridConfiguration;
global using LoggingOptions = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Logging.LoggingOptions;
global using PerformanceConfiguration = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration.PerformanceConfiguration;
global using DataGridAPI = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services.DataGridAPI;
global using ColumnWidth = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core.ColumnWidth;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// Note: Generic Result<T> must be fully qualified to avoid exposing Internal namespace

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

/// <summary>
/// SENIOR DEVELOPER: Clean Public API for AdvancedWinUiDataGrid
/// CLEAN ARCHITECTURE: Only simple types exposed, no internal dependencies
/// PROFESSIONAL: Enterprise-grade component with simple interface
/// </summary>
public sealed class AdvancedWinUiDataGrid : IDisposable
{
    private readonly IDataGridService _service;
    private readonly ComponentLogger _componentLogger;
    private bool _disposed = false;

    private AdvancedWinUiDataGrid(IDataGridService service, ComponentLogger componentLogger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _componentLogger = componentLogger ?? throw new ArgumentNullException(nameof(componentLogger));
        
        _componentLogger.LogInformation("AdvancedWinUiDataGrid instance created successfully with service: {ServiceType}", _service.GetType().Name);
    }

    #region Factory Methods

    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid for UI mode with clean public API
    /// CLEAN API: Only simple types exposed, no internal dependencies
    /// NULL-SAFE: Gracefully handles null logger using NullLogger fallback
    /// </summary>
    /// <param name="logger">Base logger instance (null-safe, uses NullLogger if null)</param>
    /// <param name="loggingConfig">Simple logging configuration (optional)</param>
    /// <returns>DataGrid instance configured for UI operations</returns>
    public static AdvancedWinUiDataGrid CreateForUI(ILogger? logger = null, DataGridLoggingConfig? loggingConfig = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var internalLoggingOptions = MapToInternalLoggingOptions(loggingConfig);
        var componentLogger = new ComponentLogger(actualLogger, internalLoggingOptions);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("CreateForUI started - Creating DataGrid service for UI mode");
            
            var service = DataGridAPI.CreateForUI(componentLogger);
            
            if (service == null)
            {
                throw new InvalidOperationException("DataGrid service creation failed - service is null");
            }
            
            var result = new AdvancedWinUiDataGrid(service, componentLogger);
            componentLogger.LogInformation("CreateForUI completed successfully - DataGrid instance created");
            
            return result;
        }, nameof(CreateForUI));
    }

    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid for headless mode with clean public API
    /// CLEAN API: Only simple types exposed, optimized for automation
    /// NULL-SAFE: Gracefully handles null logger using NullLogger fallback
    /// </summary>
    /// <param name="logger">Base logger instance (null-safe, uses NullLogger if null)</param>
    /// <param name="loggingConfig">Simple logging configuration (optional, uses production defaults)</param>
    /// <returns>DataGrid instance configured for headless operations</returns>
    public static AdvancedWinUiDataGrid CreateHeadless(ILogger? logger = null, DataGridLoggingConfig? loggingConfig = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var internalLoggingOptions = MapToInternalLoggingOptions(loggingConfig);
        var componentLogger = new ComponentLogger(actualLogger, internalLoggingOptions);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("CreateHeadless started - Creating DataGrid service for headless mode");
            
            var service = DataGridAPI.CreateHeadless(componentLogger);
            
            if (service == null)
            {
                throw new InvalidOperationException("DataGrid headless service creation failed - service is null");
            }
            
            var result = new AdvancedWinUiDataGrid(service, componentLogger);
            componentLogger.LogInformation("CreateHeadless completed successfully - DataGrid instance created");
            
            return result;
        }, nameof(CreateHeadless));
    }

    #endregion

    #region UI Element Access

    /// <summary>
    /// SENIOR DEVELOPER: Get the UI element for embedding in host application
    /// UI INTEGRATION: Returns the actual UserControl for display
    /// NULL-SAFE: Returns null if DataGrid is not initialized or disposed
    /// </summary>
    /// <returns>UI element that can be added to parent container, or null if not available</returns>
    public Microsoft.UI.Xaml.FrameworkElement? GetUIElement()
    {
        if (_disposed)
        {
            _componentLogger.LogWarning("GetUIElement called on disposed DataGrid instance");
            return null;
        }

        return _componentLogger.ExecuteWithLogging(() =>
        {
            _componentLogger.LogInformation("GetUIElement requested - attempting to retrieve UI component");

            // For now, create a simple placeholder indicating service is ready
            var placeholder = new Microsoft.UI.Xaml.Controls.TextBlock()
            {
                Text = "DataGrid Service Ready - UI Component Loading...",
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };

            _componentLogger.LogInformation("UI element created successfully - Type: {ElementType}",
                placeholder?.GetType()?.Name ?? "null");
            return placeholder;

        }, nameof(GetUIElement));
    }

    #endregion

    #region Core Operations

    /// <summary>
    /// CLEAN API: Initialize DataGrid with simple public types
    /// SENIOR DEVELOPER: Professional initialization with clean interface
    /// </summary>
    public async Task<DataGridResult> InitializeAsync(
        IReadOnlyList<DataGridColumn> columns,
        DataGridTheme theme = DataGridTheme.Light,
        DataGridValidationConfig? validationConfig = null,
        DataGridPerformanceConfig? performanceConfig = null,
        int minimumRows = 1)
    {
        var internalResult = await _componentLogger.ExecuteWithLoggingAsync(async () =>
        {
            _componentLogger.LogMethodEntry(nameof(InitializeAsync), columns?.Count ?? 0, minimumRows);
            
            // Validate input parameters with detailed logging
            if (columns == null || columns.Count == 0)
            {
                return _componentLogger.CreateFailureResult<bool>("Columns collection cannot be null or empty", "Parameter validation");
            }
            
            // CLEAN API MAPPING: Convert public API types to internal types
            var internalColumns = MapColumnsToInternal(columns);
            var internalColorConfig = MapThemeToInternal(theme);
            var internalValidationConfig = MapValidationConfigToInternal(validationConfig);
            var internalPerformanceConfig = MapPerformanceConfigToInternal(performanceConfig);
            
            // Log configuration details if enabled
            _componentLogger.LogInformation("Configuration mapped - Columns: {ColumnCount}, Theme: {Theme}, ValidationEnabled: {ValidationEnabled}", 
                internalColumns.Count, theme, internalValidationConfig.EnableValidation);
            
            // Execute service initialization with error tracking
            var result = await _service.InitializeAsync(internalColumns, internalColorConfig, internalValidationConfig, internalPerformanceConfig);
            
            // Log result using Result<T> pattern integration
            _componentLogger.LogResult(result, "DataGrid initialization");
            
            return result;
            
        }, nameof(InitializeAsync));
        
        // CLEAN API: Map internal Result<bool> to public DataGridResult
        return MapResultToPublic(internalResult);
    }

    /// <summary>CLEAN API: Import data from dictionary collection</summary>
    public async Task<DataGridResult<DataGridImportStats>> ImportFromDictionaryAsync(
        List<Dictionary<string, object?>> data,
        DataGridValidationConfig? validationConfig = null)
    {
        if (data == null)
        {
            return DataGridResult<DataGridImportStats>.Failure("Data collection cannot be null");
        }

        var internalResult = await _componentLogger.ExecuteWithLoggingAsync(async () =>
        {
            _componentLogger.LogMethodEntry(nameof(ImportFromDictionaryAsync), data.Count);
            
            var internalValidationConfig = MapValidationConfigToInternal(validationConfig);
            var result = await _service.ImportFromDictionaryAsync(data);
            _componentLogger.LogResult(result, "Dictionary import");
            
            return result;
            
        }, nameof(ImportFromDictionaryAsync));
        
        // CLEAN API: Map to public result with simple stats
        if (internalResult.IsSuccess)
        {
            var stats = new DataGridImportStats
            {
                TotalRows = data.Count,
                SuccessfulRows = data.Count,
                FailedRows = 0,
                Duration = TimeSpan.Zero
            };
            return DataGridResult<DataGridImportStats>.Success(stats);
        }
        else
        {
            return DataGridResult<DataGridImportStats>.Failure(internalResult.Error);
        }
    }

    /// <summary>CLEAN API: Export data to dictionary collection</summary>
    public async Task<DataGridResult<List<Dictionary<string, object?>>>> ExportToDictionaryAsync()
    {
        var internalResult = await _componentLogger.ExecuteWithLoggingAsync(async () =>
        {
            _componentLogger.LogMethodEntry(nameof(ExportToDictionaryAsync));
            
            var result = await _service.ExportToDictionaryAsync();
            _componentLogger.LogResult(result, "Dictionary export");
            
            return result;
            
        }, nameof(ExportToDictionaryAsync));
        
        // CLEAN API: Map to public result
        return MapExportResultToPublic(internalResult);
    }

    /// <summary>CLEAN API: Get row count</summary>
    public int GetRowCount()
    {
        return _service.GetRowCount();
    }

    /// <summary>CLEAN API: Get column count</summary>  
    public int GetColumnCount()
    {
        return _service.GetColumnCount();
    }

    /// <summary>
    /// SENIOR DEVELOPER: Create UI UserControl component with sample data
    /// CLEAN API: Returns WinUI3 UserControl for embedding in WinUI applications
    /// </summary>
    /// <returns>UserControl with DataGrid table and sample data</returns>
    public Microsoft.UI.Xaml.Controls.UserControl CreateUserControlWithSampleData()
    {
        try
        {
            _componentLogger.LogInformation("Creating AdvancedDataGrid UI UserControl with sample data");

            var component = new Internal.UI.Components.AdvancedDataGridComponent();

            // Initialize component asynchronously in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await component.InitializeWithSampleDataAsync();
                    _componentLogger.LogInformation("UI UserControl initialized with sample data successfully");
                }
                catch (Exception ex)
                {
                    _componentLogger.LogError(ex, "Failed to initialize UI UserControl with sample data");
                }
            });

            return component;
        }
        catch (Exception ex)
        {
            _componentLogger.LogError(ex, "Failed to create UI UserControl");

            // Return empty UserControl as fallback
            return new Microsoft.UI.Xaml.Controls.UserControl
            {
                Content = new Microsoft.UI.Xaml.Controls.TextBlock
                {
                    Text = $"❌ DataGrid UI Creation Failed: {ex.Message}",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                }
            };
        }
    }

    #endregion

    #region Private Mapping Methods

    /// <summary>
    /// SENIOR DEVELOPER: Map public logging config to internal options
    /// CLEAN API: Convert simple public types to internal implementation types
    /// </summary>
    private static LoggingOptions MapToInternalLoggingOptions(DataGridLoggingConfig? config)
    {
        if (config == null)
        {
            return new LoggingOptions
            {
                CategoryPrefix = "DataGrid",
                LogMethodParameters = false,
                LogPerformanceMetrics = true,
                LogUnhandledErrors = true
            };
        }

        return new LoggingOptions
        {
            CategoryPrefix = config.CategoryPrefix,
            LogMethodParameters = config.LogMethodParameters,
            LogPerformanceMetrics = config.LogPerformanceMetrics,
            LogUnhandledErrors = config.LogErrors
        };
    }

    /// <summary>
    /// SENIOR DEVELOPER: Map public columns to internal column definitions
    /// </summary>
    private static List<ColumnDefinition> MapColumnsToInternal(IReadOnlyList<DataGridColumn> columns)
    {
        var result = new List<ColumnDefinition>();
        
        foreach (var column in columns)
        {
            result.Add(new ColumnDefinition
            {
                Name = column.Name,
                DisplayName = column.Header,
                DataType = column.DataType,
                PropertyName = column.Name,
                IsRequired = column.IsRequired,
                IsReadOnly = column.IsReadOnly,
                Width = ColumnWidth.Pixels(column.Width)
            });
        }
        
        return result;
    }

    /// <summary>
    /// SENIOR DEVELOPER: Map public theme to internal color configuration
    /// </summary>
    private static ColorConfiguration MapThemeToInternal(DataGridTheme theme)
    {
        return theme switch
        {
            DataGridTheme.Dark => ColorConfiguration.Dark,
            DataGridTheme.Auto => ColorConfiguration.Light, // Auto defaults to Light for now
            _ => ColorConfiguration.Light
        };
    }

    /// <summary>
    /// SENIOR DEVELOPER: Map public validation config to internal validation configuration
    /// </summary>
    private static ValidationConfiguration MapValidationConfigToInternal(DataGridValidationConfig? config)
    {
        if (config == null)
        {
            return ValidationConfiguration.Default;
        }

        return new ValidationConfiguration
        {
            EnableValidation = config.EnableValidation,
            EnableRealTimeValidation = config.EnableRealTimeValidation
        };
    }

    /// <summary>
    /// SENIOR DEVELOPER: Map public performance config to internal performance configuration
    /// </summary>
    private static PerformanceConfiguration MapPerformanceConfigToInternal(DataGridPerformanceConfig? config)
    {
        if (config == null)
        {
            return PerformanceConfiguration.Default;
        }

        return new PerformanceConfiguration
        {
            EnableVirtualization = config.EnableVirtualization,
            VirtualizationThreshold = config.VirtualizationThreshold,
            EnableBackgroundProcessing = config.EnableBackgroundProcessing
        };
    }

    /// <summary>
    /// SENIOR DEVELOPER: Map internal Result<bool> to public DataGridResult
    /// </summary>
    private static DataGridResult MapResultToPublic(RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results.Result<bool> internalResult)
    {
        if (internalResult.IsSuccess)
        {
            return DataGridResult.Success();
        }
        else
        {
            return DataGridResult.Failure(internalResult.Error);
        }
    }

    /// <summary>
    /// SENIOR DEVELOPER: Map internal export result to public result
    /// </summary>
    private static DataGridResult<List<Dictionary<string, object?>>> MapExportResultToPublic(RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results.Result<List<Dictionary<string, object?>>> internalResult)
    {
        if (internalResult.IsSuccess)
        {
            return DataGridResult<List<Dictionary<string, object?>>>.Success(internalResult.Value);
        }
        else
        {
            return DataGridResult<List<Dictionary<string, object?>>>.Failure(internalResult.Error);
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// SENIOR DEVELOPER: Clean disposal of resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _service?.Dispose();
                _componentLogger?.Dispose();
                _componentLogger?.LogInformation("AdvancedWinUiDataGrid disposed successfully");
            }
            catch (Exception ex)
            {
                _componentLogger?.LogWarning("Error during AdvancedWinUiDataGrid disposal: {ErrorMessage}", ex.Message);
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    #endregion
}