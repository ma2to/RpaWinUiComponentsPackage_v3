using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using InitializeGridUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.InitializeGrid;
using SearchGridUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.SearchGrid;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services.Specialized;

/// <summary>
/// ENTERPRISE: Core grid operations service interface
/// SOLID: Single responsibility for initialization, validation, state management
/// CLEAN ARCHITECTURE: Application layer service contract
/// </summary>
internal interface IDataGridOperationsService : IDisposable
{
    /// <summary>
    /// Initialize grid with configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(InitializeGridUseCases.InitializeDataGridCommand command);

    /// <summary>
    /// Validate all grid data
    /// </summary>
    Task<Result<List<ValidationError>>> ValidateAllAsync();

    /// <summary>
    /// Check if all non-empty rows are valid
    /// </summary>
    Task<Result<bool>> AreAllNonEmptyRowsValidAsync(bool onlyFiltered = false);

    /// <summary>
    /// Get current grid state
    /// </summary>
    Task<Result<GridState>> GetCurrentStateAsync();

    /// <summary>
    /// Get row count
    /// </summary>
    int GetRowCount();

    /// <summary>
    /// Get column count
    /// </summary>
    int GetColumnCount();

    /// <summary>
    /// Get column name by index
    /// </summary>
    Task<Result<string>> GetColumnNameAsync(int columnIndex);
}