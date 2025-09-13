using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Factories;

/// <summary>
/// CLEAN ARCHITECTURE: Infrastructure layer factory
/// DELEGATION: Delegates to the main API for component creation
/// ENTERPRISE: Simplified factory that uses the established patterns
/// </summary>
internal static class DataGridFactory
{
    /// <summary>
    /// FACTORY METHOD: Create DataGrid for UI mode using established API
    /// DELEGATION: Delegates to AdvancedWinUiDataGrid.CreateForUI
    /// </summary>
    public static Result<AdvancedWinUiDataGrid> CreateForUI(ILogger? logger = null)
    {
        try
        {
            var dataGrid = AdvancedWinUiDataGrid.CreateForUI(logger);
            logger?.LogInformation("DataGrid created successfully for UI mode via factory");
            return Result<AdvancedWinUiDataGrid>.Success(dataGrid);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to create DataGrid for UI mode via factory");
            return Result<AdvancedWinUiDataGrid>.Failure("Failed to create DataGrid for UI mode", ex);
        }
    }
    
    /// <summary>
    /// FACTORY METHOD: Create DataGrid for headless mode using established API
    /// DELEGATION: Delegates to AdvancedWinUiDataGrid.CreateHeadless
    /// </summary>
    public static Result<AdvancedWinUiDataGrid> CreateHeadless(ILogger? logger = null)
    {
        try
        {
            var dataGrid = AdvancedWinUiDataGrid.CreateHeadless(logger);
            logger?.LogInformation("DataGrid created successfully for headless mode via factory");
            return Result<AdvancedWinUiDataGrid>.Success(dataGrid);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to create DataGrid for headless mode via factory");
            return Result<AdvancedWinUiDataGrid>.Failure("Failed to create DataGrid for headless mode", ex);
        }
    }
    
    /// <summary>
    /// VALIDATION: Basic validation for factory parameters
    /// ENTERPRISE: Parameter validation for factory operations
    /// </summary>
    private static Result<bool> ValidateFactoryParameters()
    {
        // Basic validation - could be extended in future
        return Result<bool>.Success(true);
    }
}