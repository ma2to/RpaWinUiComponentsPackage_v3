using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Primitives;

/// <summary>
/// SHARED KERNEL: Base entity with identity
/// DDD: Entity abstraction for domain entities
/// </summary>
internal abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : IEquatable<TId>
{
    public TId Id { get; protected set; }
    
    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}

/// <summary>
/// DDD: Base entity class with int identity
/// ENTERPRISE: Convenience base class for integer IDs
/// </summary>
internal abstract class Entity : Entity<int>
{
    protected Entity(int id) : base(id)
    {
    }
}