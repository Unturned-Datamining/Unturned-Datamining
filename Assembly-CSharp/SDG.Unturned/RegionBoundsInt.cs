using System;
using UnityEngine;

namespace SDG.Unturned;

public struct RegionBoundsInt : IEquatable<RegionBoundsInt>
{
    public Vector2Int min;

    public Vector2Int max;

    public static bool operator ==(RegionBoundsInt lhs, RegionBoundsInt rhs)
    {
        if (lhs.min == rhs.min)
        {
            return lhs.max == rhs.max;
        }
        return false;
    }

    public static bool operator !=(RegionBoundsInt lhs, RegionBoundsInt rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        if (obj is RegionBoundsInt regionBoundsInt)
        {
            return this == regionBoundsInt;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(min, max);
    }

    public override string ToString()
    {
        return $"({min}, {max})";
    }

    public bool Equals(RegionBoundsInt other)
    {
        if (min == other.min)
        {
            return max == other.max;
        }
        return false;
    }

    public RegionBoundsIntEnumerator GetEnumerator()
    {
        return new RegionBoundsIntEnumerator(this);
    }

    public RegionBoundsInt(Vector2Int min, Vector2Int max)
    {
        this.min = min;
        this.max = max;
    }
}
