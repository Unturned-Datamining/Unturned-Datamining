using System;

namespace Unturned.SystemEx;

public static class ArrayEx
{
    public static bool IsNullOrEmpty(this Array array)
    {
        if (array != null)
        {
            return array.Length < 1;
        }
        return true;
    }
}
