using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ImportData;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ExportData;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.InitializeGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;

/// <summary>
/// API: Main DataGrid API facade
/// CLEAN ARCHITECTURE: Interface Adapter layer
/// ENTERPRISE: Unified API for both UI and headless operations
/// FACTORY: Service factory pattern for different operational modes
/// </summary>
internal static class DataGridAPI
{
    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid service for UI mode with professional logging
    /// ENTERPRISE: Factory method with ComponentLogger integration and comprehensive error tracking
    /// </summary>
    /// <param name="componentLogger">Professional component logger with configured options</param>
    /// <returns>DataGrid service configured for UI operations</returns>
    public static IDataGridService CreateForUI(ComponentLogger componentLogger)
    {
        ArgumentNullException.ThrowIfNull(componentLogger);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("DataGridAPI.CreateForUI started - Creating UI-enabled DataGrid service");
            
            var service = DataGridServiceFactory.CreateWithUI(componentLogger);
            
            if (service == null)
            {
                throw new InvalidOperationException("DataGrid UI service factory returned null");
            }
            
            componentLogger.LogInformation("DataGridAPI.CreateForUI completed successfully - Service type: {ServiceType}", service.GetType().Name);
            return service;
            
        }, nameof(CreateForUI));
    }

    /// <summary>
    /// BACKWARD COMPATIBILITY: Create DataGrid service for UI mode with simple logger
    /// LEGACY: Maintains compatibility with existing code that doesn't use ComponentLogger
    /// </summary>
    /// <param name="logger">Optional logger for operations</param>
    /// <returns>DataGrid service configured for UI operations</returns>
    public static IDataGridService CreateForUI(ILogger? logger = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var componentLogger = new ComponentLogger(actualLogger, LoggingOptions.Development);
        return CreateForUI(componentLogger);
    }
    
    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid service for headless mode with professional logging
    /// ENTERPRISE: Factory method optimized for automation with performance monitoring
    /// </summary>
    /// <param name="componentLogger">Professional component logger with configured options</param>
    /// <returns>DataGrid service configured for headless operations</returns>
    public static IDataGridService CreateHeadless(ComponentLogger componentLogger)
    {
        ArgumentNullException.ThrowIfNull(componentLogger);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("DataGridAPI.CreateHeadless started - Creating headless DataGrid service");
            
            var service = DataGridServiceFactory.CreateHeadless(componentLogger);
            
            if (service == null)
            {
                throw new InvalidOperationException("DataGrid headless service factory returned null");
            }
            
            componentLogger.LogInformation("DataGridAPI.CreateHeadless completed successfully - Service type: {ServiceType}", service.GetType().Name);
            return service;
            
        }, nameof(CreateHeadless));
    }

    /// <summary>
    /// BACKWARD COMPATIBILITY: Create DataGrid service for headless mode with simple logger
    /// LEGACY: Maintains compatibility with existing code that doesn't use ComponentLogger
    /// </summary>
    /// <param name="logger">Optional logger for operations</param>
    /// <returns>DataGrid service configured for headless operations</returns>
    public static IDataGridService CreateHeadless(ILogger? logger = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var componentLogger = new ComponentLogger(actualLogger, LoggingOptions.Production); // Production settings for headless
        return CreateHeadless(componentLogger);
    }
}

/// <summary>
/// API: Simplified DataGrid operations for common use cases
/// ENTERPRISE: High-level API for frequent operations
/// FACADE: Simplified interface over the complex domain
/// </summary>
internal static class SimpleDataGridAPI
{
    /// <summary>
    /// ENTERPRISE: Quick initialization with minimal configuration
    /// </summary>
    internal static async Task<IDataGridService> QuickInitializeAsync(
        IReadOnlyList<string> columnNames,
        ILogger? logger = null)
    {
        var service = DataGridAPI.CreateHeadless(logger);
        
        var columns = columnNames.Select((name, index) => new ColumnDefinition
        {
            Name = name,
            DisplayName = name,
            DataType = typeof(string),
            IsVisible = true,
            IsReadOnly = false,
            IsRequired = false
        }).ToList();
        
        var command = InitializeUICommand.Create(columns);
        var result = await service.InitializeAsync(columns);
        
        if (!result.IsSuccess)
        {
            service.Dispose();
            throw new InvalidOperationException($"DataGrid initialization failed: {result.Error}");
        }
        
        return service;
    }
    
    /// <summary>
    /// ENTERPRISE: Quick import from dictionary with default settings
    /// </summary>
    internal static async Task<bool> QuickImportAsync(
        IDataGridService service,
        List<Dictionary<string, object?>> data)
    {
        var result = await service.ImportFromDictionaryAsync(data);
        return result.IsSuccess;
    }
    
    /// <summary>
    /// ENTERPRISE: Quick export to dictionary with default settings
    /// </summary>
    internal static async Task<List<Dictionary<string, object?>>?> QuickExportAsync(IDataGridService service)
    {
        var result = await service.ExportToDictionaryAsync();
        return result.IsSuccess ? result.Value : null;
    }
    
    /// <summary>
    /// ENTERPRISE: Quick validation of all data
    /// </summary>
    internal static async Task<ValidationError[]?> QuickValidateAsync(IDataGridService service)
    {
        // Validate all - using direct service call instead of command
        var result = await service.ValidateAllAsync();
        return result.IsSuccess ? result.Value : null;
    }
    
    /// <summary>
    /// ENTERPRISE: Quick search in all columns
    /// </summary>
    internal static async Task<SearchResult?> QuickSearchAsync(
        IDataGridService service, 
        string searchText)
    {
        // Search - using direct service call instead of command
        var result = await service.SearchAsync(searchText);
        return result.IsSuccess ? result.Value : null;
    }
}

/// <summary>
/// API: Configuration builders for complex scenarios
/// ENTERPRISE: Builder pattern for complex configurations
/// FLUENT: Fluent interface for intuitive configuration
/// </summary>
internal static class DataGridConfigurationBuilder
{
    /// <summary>
    /// ENTERPRISE: Create simple column configuration
    /// </summary>
    public static ColumnDefinition CreateColumn(string name, Type dataType, string? displayName = null)
    {
        return new ColumnDefinition
        {
            Name = name,
            DisplayName = displayName ?? name,
            DataType = dataType
        };
    }
    
    /// <summary>
    /// ENTERPRISE: Start building validation configuration
    /// </summary>
    public static ValidationConfigurationBuilder CreateValidationConfiguration()
    {
        return new ValidationConfigurationBuilder();
    }
    
    /// <summary>
    /// ENTERPRISE: Start building color configuration
    /// </summary>
    public static ColorConfigurationBuilder CreateColorConfiguration()
    {
        return new ColorConfigurationBuilder();
    }
    
    /// <summary>
    /// ENTERPRISE: Start building performance configuration
    /// </summary>
    public static PerformanceConfigurationBuilder CreatePerformanceConfiguration()
    {
        return new PerformanceConfigurationBuilder();
    }
}


/// <summary>
/// API: Fluent validation configuration builder
/// </summary>
internal class ValidationConfigurationBuilder
{
    private bool _strictValidation = false;
    private bool _validateEmptyRows = false;
    private readonly Dictionary<string, List<ValidationRule>> _columnRules = new();
    private readonly List<CrossColumnValidationRule> _crossColumnRules = new();
    private readonly List<GlobalValidationRule> _globalRules = new();
    private readonly List<string> _uniqueColumns = new();
    
    public ValidationConfigurationBuilder WithStrictValidation(bool strict = true)
    {
        _strictValidation = strict;
        return this;
    }
    
    public ValidationConfigurationBuilder ValidateEmptyRows(bool validate = true)
    {
        _validateEmptyRows = validate;
        return this;
    }
    
    public ValidationConfigurationBuilder AddColumnRule(string columnName, ValidationRule rule)
    {
        if (!_columnRules.ContainsKey(columnName))
            _columnRules[columnName] = new List<ValidationRule>();
        
        _columnRules[columnName].Add(rule);
        return this;
    }
    
    public ValidationConfigurationBuilder AddUniqueColumn(string columnName)
    {
        if (!_uniqueColumns.Contains(columnName))
            _uniqueColumns.Add(columnName);
        return this;
    }
    
    public ValidationConfiguration Build()
    {
        return new ValidationConfiguration
        {
            EnableValidation = true,
            EnableRealTimeValidation = true,
            StrictValidation = _strictValidation,
            ValidateEmptyRows = _validateEmptyRows,
            ColumnValidationRules = _columnRules.ToDictionary(
                kvp => kvp.Key, 
                kvp => (IReadOnlyList<ValidationRule>)kvp.Value),
            CrossColumnValidationRules = _crossColumnRules,
            GlobalValidationRules = _globalRules,
            UniqueColumns = _uniqueColumns
        };
    }
}

/// <summary>
/// API: Fluent color configuration builder
/// </summary>
internal class ColorConfigurationBuilder
{
    private string? _headerBackgroundColor;
    private string? _headerForegroundColor;
    private string? _evenRowBackgroundColor;
    private string? _oddRowBackgroundColor;
    private string? _selectedRowBackgroundColor;
    private string? _errorCellBackgroundColor;
    private string? _warningCellBackgroundColor;
    private string? _gridLineColor;
    
    public ColorConfigurationBuilder WithHeaderColors(string? background, string? foreground)
    {
        _headerBackgroundColor = background;
        _headerForegroundColor = foreground;
        return this;
    }
    
    public ColorConfigurationBuilder WithRowColors(string? evenRow, string? oddRow, string? selectedRow)
    {
        _evenRowBackgroundColor = evenRow;
        _oddRowBackgroundColor = oddRow;
        _selectedRowBackgroundColor = selectedRow;
        return this;
    }
    
    public ColorConfigurationBuilder WithValidationColors(string? errorBackground, string? warningBackground)
    {
        _errorCellBackgroundColor = errorBackground;
        _warningCellBackgroundColor = warningBackground;
        return this;
    }
    
    public ColorConfigurationBuilder WithGridLineColor(string? gridLineColor)
    {
        _gridLineColor = gridLineColor;
        return this;
    }
    
    public ColorConfiguration Build()
    {
        // SENIOR FEATURE: Use proper ColorConfiguration defaults instead of hardcoded colors
        // CLEAN ARCHITECTURE: Delegate to the established ColorConfiguration system
        var defaultConfig = ColorConfiguration.Light;

        return new ColorConfiguration
        {
            GridLineColor = defaultConfig.GridLineColor,
            HeaderBackgroundColor = defaultConfig.HeaderBackgroundColor,
            ValidationErrorForegroundColor = defaultConfig.ValidationErrorForegroundColor,
            // Include other important colors for completeness
            BackgroundColor = defaultConfig.BackgroundColor,
            ForegroundColor = defaultConfig.ForegroundColor,
            BorderColor = defaultConfig.BorderColor,
            ValidationErrorTextColor = defaultConfig.ValidationErrorTextColor,
            ValidationErrorBorderColor = defaultConfig.ValidationErrorBorderColor,
            ValidationErrorBackgroundColor = defaultConfig.ValidationErrorBackgroundColor
        };
    }
}

/// <summary>
/// API: Fluent performance configuration builder
/// </summary>
internal class PerformanceConfigurationBuilder
{
    private bool _enableVirtualization = true;
    private int _virtualizedRowCount = 1000;
    private bool _enableLazyLoading = false;
    private int _lazyLoadingThreshold = 10000;
    private bool _enableCaching = true;
    private TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
    private bool _optimizeForLargeDatasets = false;
    private int _batchSize = 100;
    
    public PerformanceConfigurationBuilder WithVirtualization(bool enable = true, int rowCount = 1000)
    {
        _enableVirtualization = enable;
        _virtualizedRowCount = rowCount;
        return this;
    }
    
    public PerformanceConfigurationBuilder WithLazyLoading(bool enable = true, int threshold = 10000)
    {
        _enableLazyLoading = enable;
        _lazyLoadingThreshold = threshold;
        return this;
    }
    
    public PerformanceConfigurationBuilder WithCaching(bool enable = true, TimeSpan? timeout = null)
    {
        _enableCaching = enable;
        _cacheTimeout = timeout ?? TimeSpan.FromMinutes(5);
        return this;
    }
    
    public PerformanceConfigurationBuilder OptimizeForLargeDatasets(bool optimize = true, int batchSize = 100)
    {
        _optimizeForLargeDatasets = optimize;
        _batchSize = batchSize;
        return this;
    }
    
    public PerformanceConfiguration Build()
    {
        return new PerformanceConfiguration
        {
            EnableVirtualization = _enableVirtualization,
            VirtualizationThreshold = _virtualizedRowCount,
            EnableBackgroundProcessing = _optimizeForLargeDatasets,
            OperationTimeout = _cacheTimeout
        };
    }
}