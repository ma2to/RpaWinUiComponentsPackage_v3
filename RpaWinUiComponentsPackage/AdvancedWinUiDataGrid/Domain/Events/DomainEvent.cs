using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Events;

/// <summary>
/// DOMAIN: Base domain event for grid operations
/// DDD: Domain event abstraction
/// </summary>
internal abstract record DomainEvent(DateTime OccurredAt = default)
{
    public DateTime OccurredAt { get; } = OccurredAt == default ? DateTime.UtcNow : OccurredAt;
}

/// <summary>
/// DOMAIN EVENT: Row added to grid
/// </summary>
internal record RowAddedEvent(int RowIndex, object RowData, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Cell value modified
/// </summary>
internal record CellModifiedEvent(int RowIndex, string ColumnName, object? OldValue, object? NewValue, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Filter applied to grid
/// </summary>
internal record FilterAppliedEvent(string FilterCriteria, int ResultCount, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Row deleted from grid
/// </summary>
internal record RowDeletedEvent(int RowIndex, object RowData, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Grid validation completed
/// </summary>
internal record ValidationCompletedEvent(int TotalRows, int ErrorCount, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);