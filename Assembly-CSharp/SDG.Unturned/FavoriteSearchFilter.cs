using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned;

public struct FavoriteSearchFilter<T>
{
    public delegate bool SubFilterParser(string input, out T subFilter);

    public T[] subFilters { get; private set; }

    public static FavoriteSearchFilter<T>? parse(string filter, SubFilterParser parseSubFilter)
    {
        if (!SearchFilterUtil.parseKeyValue(filter, "fv:", out var value))
        {
            return null;
        }
        string path = Path.Combine(ReadWrite.PATH, value) + ".txt";
        if (!File.Exists(path))
        {
            return null;
        }
        List<T> list = new List<T>();
        string[] array = File.ReadAllLines(path);
        foreach (string text in array)
        {
            if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith("//") && parseSubFilter(text, out var subFilter))
            {
                list.Add(subFilter);
            }
        }
        if (list.Count < 1)
        {
            return null;
        }
        return new FavoriteSearchFilter<T>(list.ToArray());
    }

    public FavoriteSearchFilter(T[] subFilters)
    {
        this.subFilters = subFilters;
    }
}
