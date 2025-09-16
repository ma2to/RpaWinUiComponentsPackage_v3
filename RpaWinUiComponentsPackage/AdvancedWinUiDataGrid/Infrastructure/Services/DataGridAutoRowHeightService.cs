using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.AutoRowHeight;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Services;

/// <summary>
/// INFRASTRUCTURE: Integration service for auto row height functionality
/// ENTERPRISE: Orchestrates row height calculations and UI updates
/// CLEAN ARCHITECTURE: Infrastructure layer service implementation
/// </summary>
internal class DataGridAutoRowHeightService : IDataGridAutoRowHeightService
{
    private readonly AutoRowHeightHandlers _handlers;
    private readonly UIConfiguration _uiConfiguration;
    private readonly ILogger _logger;
    private bool _disposed = false;
    private bool _isEnabled = true;

    public DataGridAutoRowHeightService(
        IRowHeightCalculationService rowHeightService,
        IDataGridUIService uiService,
        UIConfiguration uiConfiguration,
        ILogger<DataGridAutoRowHeightService> logger)
    {
        // Create a generic logger for AutoRowHeightHandlers
        var handlersLogger = logger as ILogger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        _handlers = new AutoRowHeightHandlers(rowHeightService, uiService, handlersLogger);
        _uiConfiguration = uiConfiguration ?? throw new ArgumentNullException(nameof(uiConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isEnabled = uiConfiguration.EnableAutoRowHeight;
        
        _logger.LogInformation("DataGridAutoRowHeightService initialized with auto height {Status}", 
            _isEnabled ? "enabled" : "disabled");
    }

    /// <summary>
    /// ORCHESTRATION: Calculate and apply row height for single row when data changes
    /// </summary>
    public async Task<Result<bool>> UpdateRowHeightAsync(
        int rowIndex,
        Dictionary<string, object?> rowData,
        IReadOnlyList<ColumnDefinition> columns,
        double availableWidth)
    {
        if (_disposed) return Result<bool>.Failure("Service disposed");
        if (!_isEnabled) return Result<bool>.Success(true); // Skip if disabled

        try
        {
            _logger.LogTrace("Updating row height for row {RowIndex}", rowIndex);

            // Calculate height
            var calculateCommand = CalculateRowHeightCommand.Create(
                rowIndex, rowData, columns, _uiConfiguration, availableWidth);
            
            var heightResult = await _handlers.HandleAsync(calculateCommand);
            if (!heightResult.IsSuccess)
            {
                return Result<bool>.Failure($"Failed to calculate height: {heightResult.Error}", heightResult.Exception);
            }

            // Apply height
            var applyCommand = ApplyRowHeightsCommand.Create(
                new Dictionary<int, double> { [rowIndex] = heightResult.Value });
            
            var applyResult = await _handlers.HandleAsync(applyCommand);
            if (!applyResult.IsSuccess)
            {
                return Result<bool>.Failure($"Failed to apply height: {applyResult.Error}", applyResult.Exception);
            }

            _logger.LogTrace("Row height updated successfully for row {RowIndex} to {Height}px", 
                rowIndex, heightResult.Value);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while updating row height for row {RowIndex}", rowIndex);
            return Result<bool>.Failure($"Exception while updating row height for row {rowIndex}", ex);
        }
    }

    /// <summary>
    /// BATCH PROCESSING: Calculate and apply row heights for multiple rows
    /// </summary>
    public async Task<Result<Dictionary<int, double>>> RefreshAllRowHeightsAsync(
        IReadOnlyList<Dictionary<string, object?>> rowsData,
        IReadOnlyList<ColumnDefinition> columns,
        double availableWidth,
        IProgress<BatchCalculationProgress>? progress = null)
    {
        if (_disposed) return Result<Dictionary<int, double>>.Failure("Service disposed");
        if (!_isEnabled) return Result<Dictionary<int, double>>.Success(new Dictionary<int, double>());

        try
        {
            _logger.LogInformation("Refreshing row heights for {RowCount} rows", rowsData.Count);

            // Calculate all heights in batch
            var calculateCommand = CalculateBatchRowHeightsCommand.Create(
                rowsData, columns, _uiConfiguration, availableWidth);
            calculateCommand = calculateCommand with { Progress = progress };
            
            var heightsResult = await _handlers.HandleAsync(calculateCommand);
            if (!heightsResult.IsSuccess)
            {
                return Result<Dictionary<int, double>>.Failure(
                    $"Failed to calculate batch heights: {heightsResult.Error}", heightsResult.Exception);
            }

            // Apply all heights
            var applyCommand = ApplyRowHeightsCommand.Create(heightsResult.Value);
            var applyResult = await _handlers.HandleAsync(applyCommand);
            
            if (!applyResult.IsSuccess)
            {
                _logger.LogWarning("Heights calculated but failed to apply: {Error}", applyResult.Error);
                // Still return the calculated heights even if apply failed
            }

            _logger.LogInformation("Refreshed row heights for {RowCount} rows successfully", rowsData.Count);
            return heightsResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while refreshing all row heights");
            return Result<Dictionary<int, double>>.Failure("Exception while refreshing all row heights", ex);
        }
    }

    /// <summary>
    /// CONFIGURATION: Enable or disable auto row height functionality
    /// </summary>
    public async Task<Result<bool>> SetEnabledAsync(bool enabled, bool refreshExistingRows = true)
    {
        if (_disposed) return Result<bool>.Failure("Service disposed");

        try
        {
            _logger.LogInformation("Setting auto row height enabled: {Enabled}", enabled);

            var command = enabled 
                ? ToggleAutoRowHeightCommand.Enable(refreshExistingRows)
                : ToggleAutoRowHeightCommand.Disable();
            
            var result = await _handlers.HandleAsync(command);
            
            if (result.IsSuccess)
            {
                _isEnabled = enabled;
                _logger.LogInformation("Auto row height {Status} successfully", enabled ? "enabled" : "disabled");
            }
            else
            {
                _logger.LogError("Failed to {Action} auto row height: {Error}", 
                    enabled ? "enable" : "disable", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while setting auto row height enabled state");
            return Result<bool>.Failure("Exception while setting auto row height enabled state", ex);
        }
    }

    /// <summary>
    /// Get current enabled state
    /// </summary>
    public bool IsEnabled => _isEnabled && !_disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _logger.LogInformation("DataGridAutoRowHeightService disposed");
        }
    }
}