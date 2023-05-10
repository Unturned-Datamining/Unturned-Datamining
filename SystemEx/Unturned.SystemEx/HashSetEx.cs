using System.Collections.Generic;

namespace Unturned.SystemEx;

public static class HashSetEx
{
    public static bool AddAny<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        bool flag = false;
        foreach (T item in collection)
        {
            flag |= hashSet.Add(item);
        }
        return flag;
    }
}
