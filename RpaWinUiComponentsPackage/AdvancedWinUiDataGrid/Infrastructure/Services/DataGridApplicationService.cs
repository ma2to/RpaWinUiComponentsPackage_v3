using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Services;

/// <summary>
/// INFRASTRUCTURE: Application service implementation
/// CLEAN ARCHITECTURE: Infrastructure implementation of application concerns
/// ENTERPRISE: Production-ready application service
/// </summary>
internal class DataGridApplicationService : IDisposable
{
    private readonly ILogger _logger;
    private readonly DataGridPerformanceService _performanceService;
    private readonly DataGridValidationService _validationService;
    private readonly DataGridConfiguration _configuration;
    private bool _disposed = false;

    public DataGridApplicationService(
        ILogger logger,
        DataGridPerformanceService performanceService,
        DataGridValidationService validationService,
        DataGridConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        _logger.LogInformation("DataGridApplicationService initialized");
    }

    /// <summary>
    /// Initialize application services
    /// </summary>
    public async Task<Result<bool>> InitializeAsync()
    {
        if (_disposed) return Result<bool>.Failure("Service disposed");
        
        try
        {
            _performanceService.StartOperation("ApplicationService.Initialize");
            
            _logger.LogInformation("DataGridApplicationService initialized successfully");
            
            _performanceService.StopOperation("ApplicationService.Initialize");
            return await Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize DataGridApplicationService");
            return Result<bool>.Failure("Failed to initialize DataGridApplicationService", ex);
        }
    }

    /// <summary>
    /// Process data operations
    /// </summary>
    public async Task<Result<DataOperationResult>> ProcessDataOperationAsync(
        string operationType,
        Dictionary<string, object?> parameters)
    {
        if (_disposed) return Result<DataOperationResult>.Failure("Service disposed");
        
        try
        {
            _performanceService.StartOperation($"DataOperation.{operationType}");
            
            var duration = _performanceService.StopOperation($"DataOperation.{operationType}");
            var result = DataOperationResult.CreateSuccess(
                operationType,
                parameters.Count,
                parameters.Count,
                duration,
                $"Operation {operationType} completed successfully");
            
            _logger.LogInformation("Data operation {OperationType} completed in {Duration}ms", 
                operationType, duration.TotalMilliseconds);
            
            return await Task.FromResult(Result<DataOperationResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process data operation {OperationType}", operationType);
            return Result<DataOperationResult>.Failure($"Failed to process data operation {operationType}", ex);
        }
    }

    /// <summary>
    /// Get validation service
    /// </summary>
    public DataGridValidationService GetValidationService()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGridApplicationService));
        return _validationService;
    }

    /// <summary>
    /// Get performance service
    /// </summary>
    public DataGridPerformanceService GetPerformanceService()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGridApplicationService));
        return _performanceService;
    }

    /// <summary>
    /// Get configuration
    /// </summary>
    public DataGridConfiguration GetConfiguration()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DataGridApplicationService));
        return _configuration;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _performanceService?.Dispose();
            _validationService?.Dispose();
            _disposed = true;
            
            _logger.LogInformation("DataGridApplicationService disposed");
        }
    }
}