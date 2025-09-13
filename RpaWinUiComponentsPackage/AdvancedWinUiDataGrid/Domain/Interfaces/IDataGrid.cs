using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ManageRows;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.InitializeGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ExportData;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Interfaces;

/// <summary>
/// CLEAN ARCHITECTURE: Core domain interface for DataGrid operations
/// SOLID: Interface Segregation - contains only essential DataGrid operations
/// DDD: Represents the DataGrid aggregate root contract
/// LAYER: Core (no dependencies - pure abstraction)
/// </summary>
internal interface IDataGrid : IDisposable
{
    /// <summary>
    /// ENTERPRISE: Initialize DataGrid with domain configuration
    /// DDD: Uses domain models and value objects
    /// FUNCTIONAL: Returns Result monad for error handling
    /// </summary>
    Task<Result<DataGridInitializationResult>> InitializeAsync(
        IReadOnlyList<Domain.ValueObjects.Core.ColumnDefinition> columns,
        Domain.ValueObjects.Configuration.DataGridConfiguration? options = null);
    
    /// <summary>
    /// FUNCTIONAL: Import data with monadic error handling
    /// PERFORMANCE: Optimized for large datasets (1M+ rows)
    /// DDD: Uses immutable dictionary collections
    /// </summary>
    Task<Result<DataImportResult>> ImportFromDictionaryAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> data,
        ImportOptions? options = null);
    
    /// <summary>
    /// ENTERPRISE: Export data in multiple formats
    /// SOLID: Open/Closed - extensible via ExportFormat enum
    /// </summary>
    Task<Result<DataExportResult>> ExportDataAsync(
        ExportFormat format, 
        ExportOptions? options = null);
    
    /// <summary>
    /// UI INTEGRATION: Access to UI component when available
    /// SOLID: Interface Segregation - UI concerns separated
    /// </summary>
    UserControl? UIComponent { get; }
    
    /// <summary>
    /// ENTERPRISE: Health monitoring for production systems
    /// OBSERVABILITY: Provides metrics and status information
    /// </summary>
    Task<Result<DataGridHealthStatus>> GetHealthStatusAsync();
}


/// <summary>
/// CLEAN ARCHITECTURE: UI service interface (optional dependency)
/// SOLID: Interface Segregation - separate UI concerns
/// DECORATOR: Decorates core service with UI functionality
/// </summary>
internal interface IDataGridUIService : IDisposable
{
    Task<Result<bool>> InitializeAsync(InitializeUICommand command);
    Task<Result<bool>> RefreshAsync();
    UserControl? GetUIComponent();
}


/// <summary>
/// ENTERPRISE: Performance monitoring and optimization
/// OBSERVABILITY: Metrics collection and performance tracking
/// SCALABILITY: Handles 1M+ row scenarios
/// </summary>
internal interface IDataGridPerformanceService : IDisposable
{
    IPerformanceScope CreateScope(string operationName);
    Task<PerformanceMetrics> GetMetricsAsync();
}

/// <summary>
/// PERFORMANCE: Disposable scope for operation timing
/// </summary>
internal interface IPerformanceScope : IDisposable
{
    void RecordMetric(string name, double value);
}

/// <summary>
/// ENTERPRISE: UI Service alias for clean API
/// PUBLIC API: Type alias for IDataGridUIService
/// </summary>
internal interface IUIService : IDataGridUIService
{
}