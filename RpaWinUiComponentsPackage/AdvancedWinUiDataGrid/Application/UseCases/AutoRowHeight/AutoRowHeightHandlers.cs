using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.AutoRowHeight;

/// <summary>
/// CQRS: Command handlers for auto row height operations
/// ENTERPRISE: Production-ready use case handlers with proper error handling
/// CLEAN ARCHITECTURE: Application layer use case handlers
/// </summary>
internal class AutoRowHeightHandlers
{
    private readonly IRowHeightCalculationService _rowHeightService;
    private readonly IDataGridUIService _uiService;
    private readonly ILogger _logger;

    public AutoRowHeightHandlers(
        IRowHeightCalculationService rowHeightService,
        IDataGridUIService uiService,
        ILogger logger)
    {
        _rowHeightService = rowHeightService ?? throw new ArgumentNullException(nameof(rowHeightService));
        _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogInformation("AutoRowHeightHandlers initialized");
    }

    /// <summary>
    /// Handle single row height calculation
    /// </summary>
    internal async Task<Result<double>> HandleAsync(CalculateRowHeightCommand command)
    {
        if (command == null) return Result<double>.Failure("Command cannot be null");

        try
        {
            _logger.LogTrace("Calculating height for row {RowIndex}", command.RowIndex);

            var result = await _rowHeightService.CalculateRowHeightAsync(
                command.RowData,
                command.Columns,
                command.UIConfiguration,
                command.AvailableWidth);

            if (result.IsSuccess)
            {
                _logger.LogTrace("Height calculated successfully for row {RowIndex}: {Height}px", 
                    command.RowIndex, result.Value);
            }
            else
            {
                _logger.LogWarning("Failed to calculate height for row {RowIndex}: {Error}", 
                    command.RowIndex, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calculating height for row {RowIndex}", command.RowIndex);
            return Result<double>.Failure($"Exception while calculating height for row {command.RowIndex}", ex);
        }
    }

    /// <summary>
    /// Handle batch row height calculations
    /// </summary>
    internal async Task<Result<Dictionary<int, double>>> HandleAsync(CalculateBatchRowHeightsCommand command)
    {
        if (command == null) return Result<Dictionary<int, double>>.Failure("Command cannot be null");

        try
        {
            _logger.LogInformation("Calculating heights for {RowCount} rows in batch", command.RowsData.Count);

            var result = await _rowHeightService.CalculateRowHeightsBatchAsync(
                command.RowsData,
                command.Columns,
                command.UIConfiguration,
                command.AvailableWidth);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Batch height calculation completed successfully for {RowCount} rows", 
                    command.RowsData.Count);
            }
            else
            {
                _logger.LogWarning("Batch height calculation failed: {Error}", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during batch height calculation");
            return Result<Dictionary<int, double>>.Failure("Exception during batch height calculation", ex);
        }
    }

    /// <summary>
    /// Handle applying calculated row heights to UI
    /// </summary>
    internal async Task<Result<bool>> HandleAsync(ApplyRowHeightsCommand command)
    {
        if (command == null) return Result<bool>.Failure("Command cannot be null");

        try
        {
            _logger.LogInformation("Applying heights for {RowCount} rows", command.RowHeights.Count);

            var results = new List<Result<bool>>();

            // Apply heights individually for better error handling
            foreach (var kvp in command.RowHeights)
            {
                var result = await _uiService.UpdateRowHeightAsync(kvp.Key, kvp.Value);
                results.Add(result);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to apply height for row {RowIndex}: {Error}", 
                        kvp.Key, result.Error);
                }
            }

            var successCount = results.Count(r => r.IsSuccess);
            var failureCount = results.Count - successCount;

            if (failureCount > 0)
            {
                _logger.LogWarning("Applied heights with {SuccessCount} successes and {FailureCount} failures", 
                    successCount, failureCount);
                return Result<bool>.Failure($"Applied heights partially: {successCount}/{results.Count} successful");
            }

            _logger.LogInformation("Successfully applied all {RowCount} row heights", command.RowHeights.Count);
            
            // Refresh UI if needed
            if (successCount > 0)
            {
                await _uiService.RefreshAsync();
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while applying row heights");
            return Result<bool>.Failure("Exception while applying row heights", ex);
        }
    }

    /// <summary>
    /// Handle toggling auto row height functionality
    /// </summary>
    internal async Task<Result<bool>> HandleAsync(ToggleAutoRowHeightCommand command)
    {
        if (command == null) return Result<bool>.Failure("Command cannot be null");

        try
        {
            _logger.LogInformation("Toggling auto row height: {Enabled}", command.Enabled);

            var result = await _uiService.SetAutoRowHeightEnabledAsync(command.Enabled);

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to toggle auto row height: {Error}", result.Error);
                return result;
            }

            // Refresh existing rows if requested
            if (command.RefreshExistingRows && command.Enabled)
            {
                _logger.LogInformation("Refreshing existing rows after enabling auto row height");
                var refreshResult = await _uiService.RefreshAllRowHeightsAsync();
                
                if (!refreshResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to refresh existing rows: {Error}", refreshResult.Error);
                    return Result<bool>.Failure("Auto row height enabled but failed to refresh existing rows", refreshResult.Exception);
                }

                _logger.LogInformation("Auto row height toggled and {RowCount} existing rows refreshed", 
                    refreshResult.Value.Count);
            }

            _logger.LogInformation("Auto row height successfully {Status}", command.Enabled ? "enabled" : "disabled");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while toggling auto row height");
            return Result<bool>.Failure("Exception while toggling auto row height", ex);
        }
    }
}