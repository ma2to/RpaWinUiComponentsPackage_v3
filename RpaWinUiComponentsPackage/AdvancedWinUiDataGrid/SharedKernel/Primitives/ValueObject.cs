using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Primitives;

/// <summary>
/// SHARED KERNEL: Value object base class
/// DDD: Value object abstraction for immutable values
/// </summary>
internal abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return ((ValueObject)obj).GetEqualityComponents()
            .SequenceEqual(GetEqualityComponents());
    }

    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Where(x => x != null)
            .Aggregate(1, (current, obj) =>
            {
                return HashCode.Combine(current, obj);
            });
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}