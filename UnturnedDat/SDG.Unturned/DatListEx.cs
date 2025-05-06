using System.Collections.Generic;

namespace SDG.Unturned;

public static class DatListEx
{
    public static bool TryGetValue(this IDatList list, int index, out IDatValue value)
    {
        list.TryGetNode(index, out var node);
        value = node as IDatValue;
        return value != null;
    }

    public static bool TryGetDictionary(this IDatList list, int index, out IDatDictionary dictionary)
    {
        list.TryGetNode(index, out var node);
        dictionary = node as IDatDictionary;
        return dictionary != null;
    }

    public static IDatDictionary GetDictionary(this IDatList list, int index)
    {
        if (!list.TryGetDictionary(index, out var dictionary))
        {
            return null;
        }
        return dictionary;
    }

    public static bool TryGetList(this IDatList thisList, int index, out IDatList list)
    {
        thisList.TryGetNode(index, out var node);
        list = node as IDatList;
        return list != null;
    }

    public static IDatList GetList(this IDatList thisList, int index)
    {
        if (!thisList.TryGetList(index, out var list))
        {
            return null;
        }
        return list;
    }

    public static bool TryGetString(this IDatList list, int index, out string value)
    {
        if (list.TryGetValue(index, out var value2))
        {
            value = value2.Value;
            return true;
        }
        value = null;
        return false;
    }

    public static string GetString(this IDatList list, int index, string defaultValue = null)
    {
        if (!list.TryGetString(index, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static List<T> ParseListOfStructs<T>(this IDatList list) where T : struct, IDatParseable
    {
        List<T> list2 = new List<T>(list.Count);
        foreach (IDatNode item in list)
        {
            if (item != null && item.TryParseStruct<T>(out var value))
            {
                list2.Add(value);
            }
        }
        return list2;
    }

    public static T[] ParseArrayOfStructs<T>(this IDatList list, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        T[] array = new T[list.Count];
        for (int i = 0; i < array.Length; i++)
        {
            if (list.TryGetNode(i, out var node) && node.TryParseStruct<T>(out var value))
            {
                array[i] = value;
            }
            else
            {
                array[i] = defaultValue;
            }
        }
        return array;
    }
}
