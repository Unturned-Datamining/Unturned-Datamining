using System;

namespace SDG.Unturned;

public struct PhysicsMaterialNetId : IEquatable<PhysicsMaterialNetId>
{
    public static readonly PhysicsMaterialNetId NULL = new PhysicsMaterialNetId(0u);

    public uint id;

    public PhysicsMaterialNetId(uint id)
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
        if (obj is PhysicsMaterialNetId)
        {
            return id == ((PhysicsMaterialNetId)obj).id;
        }
        return false;
    }

    public bool Equals(PhysicsMaterialNetId other)
    {
        return id == other.id;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public override string ToString()
    {
        return id.ToString("X2");
    }

    public static bool operator ==(PhysicsMaterialNetId lhs, PhysicsMaterialNetId rhs)
    {
        return lhs.id == rhs.id;
    }

    public static bool operator !=(PhysicsMaterialNetId lhs, PhysicsMaterialNetId rhs)
    {
        return lhs.id != rhs.id;
    }
}
