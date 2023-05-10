using System.Collections.Generic;

namespace Unturned.SystemEx;

public static class EnumerableEx
{
    public static T EnumerateFirst<T>(this IEnumerable<T> collection)
    {
        IEnumerator<T> enumerator = collection.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.Current;
    }
}
