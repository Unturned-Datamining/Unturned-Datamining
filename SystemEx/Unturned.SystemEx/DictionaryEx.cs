using System.Collections.Generic;

namespace Unturned.SystemEx;

public static class DictionaryEx
{
    public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = default(TValue);
            dictionary.Add(key, value);
        }
        return value;
    }

    public static TValue GetOrAddNew<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = new TValue();
            dictionary.Add(key, value);
        }
        return value;
    }
}
