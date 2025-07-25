using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public static class DatDictionaryEx
{
    public static bool TryGetValue(this IDatDictionary dictionary, string key, out IDatValue node)
    {
        IDatNode node2;
        bool num = dictionary.TryGetNode(key, out node2);
        node = node2 as IDatValue;
        if (num)
        {
            return node != null;
        }
        return false;
    }

    public static bool TryGetDictionary(this IDatDictionary dictionary, string key, out IDatDictionary node)
    {
        IDatNode node2;
        bool num = dictionary.TryGetNode(key, out node2);
        node = node2 as IDatDictionary;
        if (num)
        {
            return node != null;
        }
        return false;
    }

    public static IDatDictionary GetDictionary(this IDatDictionary dictionary, string key)
    {
        if (!dictionary.TryGetDictionary(key, out var node))
        {
            return null;
        }
        return node;
    }

    public static bool TryGetList(this IDatDictionary dictionary, string key, out IDatList node)
    {
        IDatNode node2;
        bool num = dictionary.TryGetNode(key, out node2);
        node = node2 as IDatList;
        if (num)
        {
            return node != null;
        }
        return false;
    }

    public static IDatList GetList(this IDatDictionary dictionary, string key)
    {
        if (!dictionary.TryGetList(key, out var node))
        {
            return null;
        }
        return node;
    }

    public static bool TryGetString(this IDatDictionary dictionary, string key, out string value)
    {
        if (dictionary.TryGetValue(key, out var node))
        {
            value = node.Value;
            return true;
        }
        value = null;
        return false;
    }

    public static string GetString(this IDatDictionary dictionary, string key, string defaultValue = null)
    {
        if (!dictionary.TryGetString(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt8(this IDatDictionary dictionary, string key, out sbyte value)
    {
        value = 0;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseInt8(out value);
        }
        return false;
    }

    public static sbyte ParseInt8(this IDatDictionary dictionary, string key, sbyte defaultValue = 0)
    {
        if (!dictionary.TryParseInt8(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt8(this IDatDictionary dictionary, string key, out byte value)
    {
        value = 0;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseUInt8(out value);
        }
        return false;
    }

    public static byte ParseUInt8(this IDatDictionary dictionary, string key, byte defaultValue = 0)
    {
        if (!dictionary.TryParseUInt8(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt16(this IDatDictionary dictionary, string key, out short value)
    {
        value = 0;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseInt16(out value);
        }
        return false;
    }

    public static short ParseInt16(this IDatDictionary dictionary, string key, short defaultValue = 0)
    {
        if (!dictionary.TryParseInt16(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt16(this IDatDictionary dictionary, string key, out ushort value)
    {
        value = 0;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseUInt16(out value);
        }
        return false;
    }

    public static ushort ParseUInt16(this IDatDictionary dictionary, string key, ushort defaultValue = 0)
    {
        if (!dictionary.TryParseUInt16(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt32(this IDatDictionary dictionary, string key, out int value)
    {
        value = 0;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseInt32(out value);
        }
        return false;
    }

    public static int ParseInt32(this IDatDictionary dictionary, string key, int defaultValue = 0)
    {
        if (!dictionary.TryParseInt32(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt32(this IDatDictionary dictionary, string key, out uint value)
    {
        value = 0u;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseUInt32(out value);
        }
        return false;
    }

    public static uint ParseUInt32(this IDatDictionary dictionary, string key, uint defaultValue = 0u)
    {
        if (!dictionary.TryParseUInt32(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt64(this IDatDictionary dictionary, string key, out long value)
    {
        value = 0L;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseInt64(out value);
        }
        return false;
    }

    public static long ParseInt64(this IDatDictionary dictionary, string key, long defaultValue = 0L)
    {
        if (!dictionary.TryParseInt64(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt64(this IDatDictionary dictionary, string key, out ulong value)
    {
        value = 0uL;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseUInt64(out value);
        }
        return false;
    }

    public static ulong ParseUInt64(this IDatDictionary dictionary, string key, ulong defaultValue = 0uL)
    {
        if (!dictionary.TryParseUInt64(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseFloat(this IDatDictionary dictionary, string key, out float value)
    {
        value = 0f;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseFloat(out value);
        }
        return false;
    }

    public static float ParseFloat(this IDatDictionary dictionary, string key, float defaultValue = 0f)
    {
        if (!dictionary.TryParseFloat(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseDouble(this IDatDictionary dictionary, string key, out double value)
    {
        value = 0.0;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseDouble(out value);
        }
        return false;
    }

    public static double ParseDouble(this IDatDictionary dictionary, string key, double defaultValue = 0.0)
    {
        if (!dictionary.TryParseDouble(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseEnum<T>(this IDatDictionary dictionary, string key, out T value) where T : struct
    {
        value = default(T);
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseEnum<T>(out value);
        }
        return false;
    }

    public static T ParseEnum<T>(this IDatDictionary dictionary, string key, T defaultValue = default(T)) where T : struct
    {
        if (!dictionary.TryParseEnum<T>(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseBool(this IDatDictionary dictionary, string key, out bool value)
    {
        value = false;
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseBool(out value);
        }
        return false;
    }

    public static bool ParseBool(this IDatDictionary dictionary, string key, bool defaultValue = false)
    {
        if (!dictionary.TryParseBool(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseGuid(this IDatDictionary dictionary, string key, out Guid value)
    {
        value = default(Guid);
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseGuid(out value);
        }
        return false;
    }

    public static Guid ParseGuid(this IDatDictionary dictionary, string key, Guid defaultValue = default(Guid))
    {
        if (!dictionary.TryParseGuid(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseDateTimeUtc(this IDatDictionary dictionary, string key, out DateTime value)
    {
        value = default(DateTime);
        if (dictionary.TryGetValue(key, out var node))
        {
            return node.TryParseDateTimeUtc(out value);
        }
        return false;
    }

    public static DateTime ParseDateTimeUtc(this IDatDictionary dictionary, string key, DateTime defaultValue = default(DateTime))
    {
        if (!dictionary.TryParseDateTimeUtc(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static Type ParseType(this IDatDictionary dictionary, string key, Type defaultValue = null)
    {
        Type result = defaultValue;
        if (dictionary.TryGetValue(key, out var node))
        {
            result = node.ParseType(defaultValue);
        }
        return result;
    }

    public static bool TryParseStruct<T>(this IDatDictionary dictionary, string key, out T value) where T : struct, IDatParseable
    {
        value = default(T);
        if (dictionary.TryGetNode(key, out var node))
        {
            return value.TryParse(node);
        }
        return false;
    }

    public static T ParseStruct<T>(this IDatDictionary dictionary, string key, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        if (!dictionary.TryParseStruct<T>(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static List<T> ParseListOfStructs<T>(this IDatDictionary dictionary, string key) where T : struct, IDatParseable
    {
        if (!dictionary.TryGetList(key, out var node))
        {
            return null;
        }
        return node.ParseListOfStructs<T>();
    }

    public static T[] ParseArrayOfStructs<T>(this IDatDictionary dictionary, string key, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        if (!dictionary.TryGetList(key, out var node))
        {
            return null;
        }
        return node.ParseArrayOfStructs(defaultValue);
    }

    public static IEditableDatDictionary GetOrAddDictionary(this IEditableDatDictionary dictionary, string key, out bool isNew)
    {
        if (dictionary.TryGetNode(key, out var node))
        {
            isNew = false;
            if (node is IDatDictionary datDictionary)
            {
                return datDictionary.Edit();
            }
            return dictionary.ReplaceWithDictionary(key);
        }
        isNew = true;
        return dictionary.AddDictionary(key);
    }

    public static IEditableDatDictionary GetOrAddDictionary(this IEditableDatDictionary dictionary, string key)
    {
        bool isNew;
        return dictionary.GetOrAddDictionary(key, out isNew);
    }

    public static IEditableDatList GetOrAddList(this IEditableDatDictionary dictionary, string key, out bool isNew)
    {
        if (dictionary.TryGetNode(key, out var node))
        {
            isNew = false;
            if (node is IDatList datList)
            {
                return datList.Edit();
            }
            return dictionary.ReplaceWithList(key);
        }
        isNew = true;
        return dictionary.AddList(key);
    }

    public static IEditableDatList GetOrAddList(this IEditableDatDictionary dictionary, string key)
    {
        bool isNew;
        return dictionary.GetOrAddList(key, out isNew);
    }

    public static IEditableDatValue GetOrAddValue(this IEditableDatDictionary dictionary, string key, out bool isNew)
    {
        if (dictionary.TryGetNode(key, out var node))
        {
            isNew = false;
            if (node is IDatValue datValue)
            {
                return datValue.Edit();
            }
            return dictionary.ReplaceWithValue(key);
        }
        isNew = true;
        return dictionary.AddValue(key);
    }

    public static IEditableDatValue GetOrAddValue(this IEditableDatDictionary dictionary, string key)
    {
        bool isNew;
        return dictionary.GetOrAddValue(key, out isNew);
    }
}
