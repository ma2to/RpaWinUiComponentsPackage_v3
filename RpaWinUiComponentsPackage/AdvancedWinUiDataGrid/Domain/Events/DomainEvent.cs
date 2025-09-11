using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.Events;

/// <summary>
/// DOMAIN: Base domain event for grid operations
/// DDD: Domain event abstraction
/// </summary>
public abstract record DomainEvent(DateTime OccurredAt = default)
{
    public DateTime OccurredAt { get; } = OccurredAt == default ? DateTime.UtcNow : OccurredAt;
}

/// <summary>
/// DOMAIN EVENT: Row added to grid
/// </summary>
public record RowAddedEvent(int RowIndex, object RowData, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Cell value modified
/// </summary>
public record CellModifiedEvent(int RowIndex, string ColumnName, object? OldValue, object? NewValue, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Filter applied to grid
/// </summary>
public record FilterAppliedEvent(string FilterCriteria, int ResultCount, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Row deleted from grid
/// </summary>
public record RowDeletedEvent(int RowIndex, object RowData, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);

/// <summary>
/// DOMAIN EVENT: Grid validation completed
/// </summary>
public record ValidationCompletedEvent(int TotalRows, int ErrorCount, DateTime OccurredAt = default) 
    : DomainEvent(OccurredAt);