using System;

namespace Unturned.SystemEx;

public static class TypeEx
{
    public static bool TryIsAssignableFrom(this Type type, Type otherType)
    {
        try
        {
            return type.IsAssignableFrom(otherType);
        }
        catch
        {
            return false;
        }
    }
}
