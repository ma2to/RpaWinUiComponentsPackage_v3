using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;
using ImportDataUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ImportData;
using ExportDataUseCases = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.UseCases.ExportData;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services.Specialized;

/// <summary>
/// ENTERPRISE: Data import/export operations service interface
/// SOLID: Single responsibility for data operations
/// CLEAN ARCHITECTURE: Application layer service contract
/// </summary>
internal interface IDataGridDataOperationsService : IDisposable
{
    /// <summary>
    /// Import data from dictionary list
    /// </summary>
    Task<Result<ImportResult>> ImportFromDictionaryAsync(ImportDataUseCases.ImportDataCommand command);

    /// <summary>
    /// Import data from DataTable
    /// </summary>
    Task<Result<ImportResult>> ImportFromDataTableAsync(ImportDataUseCases.ImportFromDataTableCommand command);

    /// <summary>
    /// Export data to dictionary list
    /// </summary>
    Task<Result<List<Dictionary<string, object?>>>> ExportToDictionaryAsync(ExportDataUseCases.ExportDataCommand command);

    /// <summary>
    /// Export data to DataTable
    /// </summary>
    Task<Result<DataTable>> ExportToDataTableAsync(ExportDataUseCases.ExportToDataTableCommand command);
}