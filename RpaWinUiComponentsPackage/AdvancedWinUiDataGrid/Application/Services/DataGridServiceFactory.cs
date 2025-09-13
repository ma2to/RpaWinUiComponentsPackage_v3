using System;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services.Specialized;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Factories;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;

/// <summary>
/// FACTORY PATTERN: Creates DataGrid services with proper dependency injection
/// SOLID: Single Responsibility - Service creation and configuration
/// CLEAN ARCHITECTURE: Replaces the old monolithic service factory
/// NO_GOD_FILES: Orchestrates multiple specialized services instead of one giant service
/// </summary>
internal static class DataGridServiceFactory
{
    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid service for UI mode with professional logging
    /// ENTERPRISE: Assembles all specialized services with ComponentLogger integration
    /// </summary>
    public static IDataGridService CreateWithUI(ComponentLogger componentLogger)
    {
        ArgumentNullException.ThrowIfNull(componentLogger);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("DataGridServiceFactory.CreateWithUI started - Creating UI-enabled service");
            
            // Create configuration with comprehensive logging
            var configuration = CreateConfigurationWithLogging(componentLogger, true);
            
            // Create specialized services with professional logging
            var validationService = CreateValidationServiceWithLogging(componentLogger);
            var transformationService = CreateTransformationServiceWithLogging(componentLogger);
            var searchService = CreateSearchServiceWithLogging(componentLogger);
            var filterService = CreateFilterServiceWithLogging(componentLogger);
            var sortService = CreateSortServiceWithLogging(componentLogger);

            // Create compound services
            var stateService = new DataGridStateManagementService(configuration, CreateLogger<DataGridStateManagementService>(componentLogger));
            var importExportService = new DataGridImportExportService(transformationService, validationService, CreateLogger<DataGridImportExportService>(componentLogger));
            var searchFilterService = new DataGridSearchFilterService(searchService, filterService, sortService, CreateLogger<DataGridSearchFilterService>(componentLogger));
            var rowManagementService = new DataGridRowManagementService(validationService, RowManagementConfiguration.Default, CreateLogger<DataGridRowManagementService>(componentLogger));
            var clipboardService = CreateClipboardServiceWithLogging(componentLogger);
            var rowHeightCalculationService = CreateRowHeightCalculationServiceWithLogging(componentLogger);

            // Create unified service
            var unifiedService = new DataGridUnifiedService(
                stateService, importExportService, searchFilterService, rowManagementService, 
                validationService, clipboardService, CreateLogger<DataGridUnifiedService>(componentLogger));

            componentLogger.LogInformation("DataGridServiceFactory.CreateWithUI completed successfully - Service type: {ServiceType}", unifiedService.GetType().Name);
            return unifiedService;
            
        }, nameof(CreateWithUI));
    }

    /// <summary>
    /// BACKWARD COMPATIBILITY: Create DataGrid service for UI mode with simple logger
    /// LEGACY: Maintains compatibility with existing code
    /// </summary>
    public static IDataGridService CreateWithUI(ILogger? logger = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var componentLogger = new ComponentLogger(actualLogger, LoggingOptions.Development);
        return CreateWithUI(componentLogger);
    }

    /// <summary>
    /// SENIOR DEVELOPER: Create DataGrid service for headless mode with professional logging
    /// ENTERPRISE: Optimized for automation with performance monitoring
    /// </summary>
    public static IDataGridService CreateHeadless(ComponentLogger componentLogger)
    {
        ArgumentNullException.ThrowIfNull(componentLogger);
        
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogInformation("DataGridServiceFactory.CreateHeadless started - Creating headless service");
            
            // Create configuration for headless mode
            var configuration = CreateConfigurationWithLogging(componentLogger, false);
            
            // Create specialized services with professional logging
            var validationService = CreateValidationServiceWithLogging(componentLogger);
            var transformationService = CreateTransformationServiceWithLogging(componentLogger);
            var searchService = CreateSearchServiceWithLogging(componentLogger);
            var filterService = CreateFilterServiceWithLogging(componentLogger);
            var sortService = CreateSortServiceWithLogging(componentLogger);

            // Create compound services for headless mode
            var stateService = new DataGridStateManagementService(configuration, CreateLogger<DataGridStateManagementService>(componentLogger));
            var importExportService = new DataGridImportExportService(transformationService, validationService, CreateLogger<DataGridImportExportService>(componentLogger));
            var searchFilterService = new DataGridSearchFilterService(searchService, filterService, sortService, CreateLogger<DataGridSearchFilterService>(componentLogger));
            var rowManagementService = new DataGridRowManagementService(validationService, RowManagementConfiguration.HighVolume, CreateLogger<DataGridRowManagementService>(componentLogger));
            var clipboardService = CreateClipboardServiceWithLogging(componentLogger);
            var rowHeightCalculationService = CreateRowHeightCalculationServiceWithLogging(componentLogger);

            // Create unified service
            var unifiedService = new DataGridUnifiedService(
                stateService, importExportService, searchFilterService, rowManagementService, 
                validationService, clipboardService, CreateLogger<DataGridUnifiedService>(componentLogger));

            componentLogger.LogInformation("DataGridServiceFactory.CreateHeadless completed successfully - Service type: {ServiceType}", unifiedService.GetType().Name);
            return unifiedService;
            
        }, nameof(CreateHeadless));
    }

    /// <summary>
    /// BACKWARD COMPATIBILITY: Create DataGrid service for headless mode with simple logger
    /// LEGACY: Maintains compatibility with existing code
    /// </summary>
    public static IDataGridService CreateHeadless(ILogger? logger = null)
    {
        var actualLogger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var componentLogger = new ComponentLogger(actualLogger, LoggingOptions.Production);
        return CreateHeadless(componentLogger);
    }

    #region Legacy Implementation - Will be removed in future version
    
    [Obsolete("This method will be removed in future version. Use ComponentLogger overload instead.")]
    private static IDataGridService CreateWithUILegacy(ILogger? logger = null)
    {
        logger?.LogInformation("[FACTORY] DataGridServiceFactory.CreateWithUI started - Creating UI-enabled service");
        try
        {
            logger?.LogDebug("[FACTORY] Step 1: Creating DataGrid configuration for UI mode");
            
            // FIX: Create default UI configuration if UIConfiguration.Default doesn't exist
            DataGridConfiguration configuration;
            try 
            {
                logger?.LogDebug("[FACTORY] Attempting to create DataGridConfiguration.ForUI with UIConfiguration.Default");
                configuration = DataGridConfiguration.ForUI(UIConfiguration.Default);
                logger?.LogDebug("[FACTORY] DataGridConfiguration.ForUI created successfully");
            }
            catch (Exception configEx)
            {
                logger?.LogWarning(configEx, "[FACTORY] UIConfiguration.Default failed, creating fallback configuration");
                // Fallback: Create basic UI configuration using DataGridConfiguration.Default
                configuration = DataGridConfiguration.Default;
                logger?.LogDebug("[FACTORY] Created fallback DataGridConfiguration using Default settings");
            }
            
            logger?.LogDebug("[FACTORY] Step 2: Creating specialized services");
            
            logger?.LogDebug("[FACTORY] Creating validation service");
            var validationService = CreateValidationService(logger);
            logger?.LogDebug("[FACTORY] Validation service created: {ServiceType}", validationService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating transformation service");
            var transformationService = CreateTransformationService(logger);
            logger?.LogDebug("[FACTORY] Transformation service created: {ServiceType}", transformationService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating search service");
            var searchService = CreateSearchService(logger);
            logger?.LogDebug("[FACTORY] Search service created: {ServiceType}", searchService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating filter service");
            var filterService = CreateFilterService(logger);
            logger?.LogDebug("[FACTORY] Filter service created: {ServiceType}", filterService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating sort service");
            var sortService = CreateSortService(logger);
            logger?.LogDebug("[FACTORY] Sort service created: {ServiceType}", sortService?.GetType()?.Name ?? "null");

            logger?.LogDebug("[FACTORY] Step 3: Creating compound services");
            
            logger?.LogDebug("[FACTORY] Creating state management service");
            var stateService = new DataGridStateManagementService(configuration, CreateLogger<DataGridStateManagementService>(logger));
            logger?.LogDebug("[FACTORY] State service created: {ServiceType}", stateService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating import/export service");
            var importExportService = new DataGridImportExportService(transformationService, validationService, CreateLogger<DataGridImportExportService>(logger));
            logger?.LogDebug("[FACTORY] Import/Export service created: {ServiceType}", importExportService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating search/filter service");
            var searchFilterService = new DataGridSearchFilterService(searchService, filterService, sortService, CreateLogger<DataGridSearchFilterService>(logger));
            logger?.LogDebug("[FACTORY] Search/Filter service created: {ServiceType}", searchFilterService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating row management service");
            var rowManagementService = new DataGridRowManagementService(validationService, RowManagementConfiguration.Default, CreateLogger<DataGridRowManagementService>(logger));
            logger?.LogDebug("[FACTORY] Row management service created: {ServiceType}", rowManagementService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating clipboard service");
            var clipboardService = CreateClipboardService(logger);
            logger?.LogDebug("[FACTORY] Clipboard service created: {ServiceType}", clipboardService?.GetType()?.Name ?? "null");
            
            logger?.LogDebug("[FACTORY] Creating row height calculation service");
            var rowHeightCalculationService = CreateRowHeightCalculationService(logger);
            logger?.LogDebug("[FACTORY] Row height service created: {ServiceType}", rowHeightCalculationService?.GetType()?.Name ?? "null");

            logger?.LogDebug("[FACTORY] Step 4: Creating unified service with all dependencies");
            var unifiedService = new DataGridUnifiedService(
                stateService, importExportService, searchFilterService, rowManagementService, 
                validationService, clipboardService, CreateLogger<DataGridUnifiedService>(logger));
            logger?.LogDebug("[FACTORY] Unified service created: {ServiceType}", unifiedService?.GetType()?.Name ?? "null");

            logger?.LogInformation("[FACTORY] DataGridServiceFactory.CreateWithUI completed successfully - Service type: {ServiceType}", unifiedService.GetType().Name);
            return unifiedService;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[FACTORY] DataGridServiceFactory.CreateWithUI failed - Exception during service creation: {ErrorMessage}\nStackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            throw new InvalidOperationException("Failed to create DataGrid service for UI mode", ex);
        }
    }


    #endregion

    #region Private Infrastructure Service Factories

    private static IDataGridValidationService CreateValidationService(ILogger? logger)
    {
        logger?.LogDebug("[FACTORY-SUB] Creating DataGridValidationService");
        try
        {
            var service = new DataGridValidationService(CreateLogger<DataGridValidationService>(logger));
            logger?.LogDebug("[FACTORY-SUB] DataGridValidationService created successfully");
            return service;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[FACTORY-SUB] Failed to create DataGridValidationService: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private static IDataGridTransformationService CreateTransformationService(ILogger? logger)
    {
        logger?.LogDebug("[FACTORY-SUB] Creating DataGridTransformationService");
        try
        {
            var service = new Infrastructure.Persistence.DataGridTransformationService(CreateLogger<Infrastructure.Persistence.DataGridTransformationService>(logger));
            logger?.LogDebug("[FACTORY-SUB] DataGridTransformationService created successfully");
            return service;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[FACTORY-SUB] Failed to create DataGridTransformationService: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private static IDataGridSearchService CreateSearchService(ILogger? logger)
    {
        logger?.LogDebug("[FACTORY-SUB] Creating DataGridSearchService");
        try
        {
            var service = new Infrastructure.Persistence.DataGridSearchService(CreateLogger<Infrastructure.Persistence.DataGridSearchService>(logger));
            logger?.LogDebug("[FACTORY-SUB] DataGridSearchService created successfully");
            return service;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[FACTORY-SUB] Failed to create DataGridSearchService: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private static IDataGridFilterService CreateFilterService(ILogger? logger)
    {
        return new Infrastructure.Services.DataGridFilterService(CreateLogger<Infrastructure.Services.DataGridFilterService>(logger));
    }

    private static IDataGridSortService CreateSortService(ILogger? logger)
    {
        return new Infrastructure.Services.DataGridSortService(CreateLogger<Infrastructure.Services.DataGridSortService>(logger));
    }

    private static IClipboardService CreateClipboardService(ILogger? logger)
    {
        return new ClipboardService(logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
    }

    private static IRowHeightCalculationService CreateRowHeightCalculationService(ILogger? logger)
    {
        return new Infrastructure.Services.RowHeightCalculationService(CreateLogger<Infrastructure.Services.RowHeightCalculationService>(logger));
    }

    private static ILogger<T>? CreateLogger<T>(ILogger? baseLogger)
    {
        return baseLogger as ILogger<T>;
    }

    #endregion

    #region Professional Service Factory Methods with ComponentLogger

    /// <summary>
    /// SENIOR DEV: Create configuration with comprehensive logging
    /// </summary>
    private static DataGridConfiguration CreateConfigurationWithLogging(ComponentLogger componentLogger, bool isUIMode)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating DataGrid configuration - UI Mode: {IsUIMode}", isUIMode);
            
            try 
            {
                var configuration = isUIMode ? 
                    DataGridConfiguration.ForUI(UIConfiguration.Default) : 
                    DataGridConfiguration.ForHeadless();
                    
                componentLogger.LogDebug("Configuration created successfully - Type: {ConfigurationType}", configuration.GetType().Name);
                return configuration;
            }
            catch (Exception ex)
            {
                componentLogger.LogWarning("UIConfiguration.Default failed, creating fallback configuration - Error: {Error}", ex.Message);
                var fallbackConfig = DataGridConfiguration.Default;
                componentLogger.LogDebug("Created fallback configuration successfully");
                return fallbackConfig;
            }
        }, nameof(CreateConfigurationWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create validation service with professional logging
    /// </summary>
    private static IDataGridValidationService CreateValidationServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating DataGridValidationService");
            var service = new DataGridValidationService(CreateLogger<DataGridValidationService>(componentLogger));
            componentLogger.LogDebug("DataGridValidationService created successfully");
            return service;
        }, nameof(CreateValidationServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create transformation service with professional logging
    /// </summary>
    private static IDataGridTransformationService CreateTransformationServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating DataGridTransformationService");
            var service = new Infrastructure.Persistence.DataGridTransformationService(CreateLogger<Infrastructure.Persistence.DataGridTransformationService>(componentLogger));
            componentLogger.LogDebug("DataGridTransformationService created successfully");
            return service;
        }, nameof(CreateTransformationServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create search service with professional logging
    /// </summary>
    private static IDataGridSearchService CreateSearchServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating DataGridSearchService");
            var service = new Infrastructure.Persistence.DataGridSearchService(CreateLogger<Infrastructure.Persistence.DataGridSearchService>(componentLogger));
            componentLogger.LogDebug("DataGridSearchService created successfully");
            return service;
        }, nameof(CreateSearchServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create filter service with professional logging
    /// </summary>
    private static IDataGridFilterService CreateFilterServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating DataGridFilterService");
            var service = new Infrastructure.Services.DataGridFilterService(CreateLogger<Infrastructure.Services.DataGridFilterService>(componentLogger));
            componentLogger.LogDebug("DataGridFilterService created successfully");
            return service;
        }, nameof(CreateFilterServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create sort service with professional logging
    /// </summary>
    private static IDataGridSortService CreateSortServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating DataGridSortService");
            var service = new Infrastructure.Services.DataGridSortService(CreateLogger<Infrastructure.Services.DataGridSortService>(componentLogger));
            componentLogger.LogDebug("DataGridSortService created successfully");
            return service;
        }, nameof(CreateSortServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create clipboard service with professional logging
    /// </summary>
    private static IClipboardService CreateClipboardServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating ClipboardService");
            var service = new ClipboardService(componentLogger._baseLogger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            componentLogger.LogDebug("ClipboardService created successfully");
            return service;
        }, nameof(CreateClipboardServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create row height calculation service with professional logging
    /// </summary>
    private static IRowHeightCalculationService CreateRowHeightCalculationServiceWithLogging(ComponentLogger componentLogger)
    {
        return componentLogger.ExecuteWithLogging(() =>
        {
            componentLogger.LogDebug("Creating RowHeightCalculationService");
            var service = new Infrastructure.Services.RowHeightCalculationService(CreateLogger<Infrastructure.Services.RowHeightCalculationService>(componentLogger));
            componentLogger.LogDebug("RowHeightCalculationService created successfully");
            return service;
        }, nameof(CreateRowHeightCalculationServiceWithLogging));
    }

    /// <summary>
    /// SENIOR DEV: Create typed logger from ComponentLogger
    /// </summary>
    private static ILogger<T>? CreateLogger<T>(ComponentLogger componentLogger)
    {
        if (componentLogger._baseLogger == null)
            return null;
            
        // Try to cast to typed logger first
        if (componentLogger._baseLogger is ILogger<T> typedLogger)
            return typedLogger;
            
        // If not typed, return null (services should handle null loggers gracefully)
        return null;
    }

    #endregion
}