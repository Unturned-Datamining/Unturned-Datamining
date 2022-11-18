using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public static class ListExtension
{
    public static int GetRandomIndex<T>(this List<T> list)
    {
        return Random.Range(0, list.Count);
    }

    public static T RandomOrDefault<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            return list[Random.Range(0, list.Count)];
        }
        return default(T);
    }

    public static T AddDefaulted<T>(this List<T> list) where T : class, new()
    {
        T val = new T();
        list.Add(val);
        return val;
    }

    public static bool IsEmpty<T>(this List<T> list)
    {
        return list.Count < 1;
    }

    public static T HeadOrDefault<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            return list[0];
        }
        return default(T);
    }

    public static T TailOrDefault<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            return list[list.Count - 1];
        }
        return default(T);
    }

    public static T GetTail<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    public static T GetAndRemoveTail<T>(this List<T> list)
    {
        int index = list.Count - 1;
        T result = list[index];
        list.RemoveAt(index);
        return result;
    }

    public static void RemoveTail<T>(this List<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }

    internal static int FindInsertionIndex<T>(this List<T> list, T item)
    {
        int num = list.BinarySearch(item);
        if (num < 0)
        {
            num = ~num;
        }
        return num;
    }

    internal static int FindInsertionIndex<T>(this List<T> list, T item, IComparer<T> comparer)
    {
        int num = list.BinarySearch(item, comparer);
        if (num < 0)
        {
            num = ~num;
        }
        return num;
    }
}
