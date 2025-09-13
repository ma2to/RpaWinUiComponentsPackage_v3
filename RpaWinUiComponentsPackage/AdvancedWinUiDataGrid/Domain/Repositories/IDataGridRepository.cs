using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Repositories;

/// <summary>
/// DOMAIN: Repository abstraction for data grid persistence
/// CLEAN ARCHITECTURE: Dependency inversion - domain defines interface, infrastructure implements
/// </summary>
internal interface IDataGridRepository
{
    Task<Result<GridState>> GetGridStateAsync();
    Task<Result<bool>> SaveGridStateAsync(GridState state);
    Task<Result<IReadOnlyList<GridRow>>> GetRowsAsync();
    Task<Result<bool>> SaveRowsAsync(IReadOnlyList<GridRow> rows);
    Task<Result<GridRow>> GetRowAsync(int index);
    Task<Result<bool>> SaveRowAsync(GridRow row);
    Task<Result<bool>> DeleteRowAsync(int index);
    Task<Result<IReadOnlyList<GridColumn>>> GetColumnsAsync();
    Task<Result<bool>> SaveColumnsAsync(IReadOnlyList<GridColumn> columns);
}