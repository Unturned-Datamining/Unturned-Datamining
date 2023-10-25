using System;

namespace SDG.Unturned;

public struct NetId : IEquatable<NetId>
{
    public static readonly NetId INVALID = new NetId(0u);

    public uint id;

    public NetId(uint id)
    {
        this.id = id;
    }

    /// <summary>
    /// Zero is treated as unset.
    /// </summary>
    public bool IsNull()
    {
        return id == 0;
    }

    public void Clear()
    {
        id = 0u;
    }

    public override bool Equals(object obj)
    {
        if (obj is NetId)
        {
            return id == ((NetId)obj).id;
        }
        return false;
    }

    public bool Equals(NetId other)
    {
        return id == other.id;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public override string ToString()
    {
        return id.ToString("X8");
    }

    public static bool operator ==(NetId lhs, NetId rhs)
    {
        return lhs.id == rhs.id;
    }

    public static bool operator !=(NetId lhs, NetId rhs)
    {
        return lhs.id != rhs.id;
    }

    public static NetId operator +(NetId lhs, uint rhs)
    {
        return new NetId(lhs.id + rhs);
    }

    public static NetId operator ++(NetId value)
    {
        return new NetId(value.id + 1);
    }
}
