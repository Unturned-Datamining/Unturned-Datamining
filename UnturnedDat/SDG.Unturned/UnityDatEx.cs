using UnityEngine;

namespace SDG.Unturned;

public static class UnityDatEx
{
    public static bool TryParseVector3(this DatValue node, out Vector3 value)
    {
        if (string.IsNullOrEmpty(node.value))
        {
            value = default(Vector3);
            return false;
        }
        int num = node.value.IndexOf('(');
        int num3;
        int num4;
        if (num >= 0)
        {
            int num2 = node.value.IndexOf(')', num + 2);
            if (num2 < 0)
            {
                value = default(Vector3);
                return false;
            }
            num3 = num + 1;
            num4 = num2 - 1;
        }
        else
        {
            num3 = 0;
            num4 = node.value.Length - 1;
        }
        int num5 = node.value.IndexOf(',', num3);
        if (num5 < 0 || num5 + 2 > num4)
        {
            value = default(Vector3);
            return false;
        }
        int num6 = node.value.IndexOf(',', num5 + 2);
        if (num6 < 0 || num6 + 1 > num4)
        {
            value = default(Vector3);
            return false;
        }
        if (!float.TryParse(node.value.Substring(num3, num5 - num3), out value.x))
        {
            value = default(Vector3);
            return false;
        }
        if (!float.TryParse(node.value.Substring(num5 + 1, num6 - num5 - 1), out value.y))
        {
            value = default(Vector3);
            return false;
        }
        if (!float.TryParse(node.value.Substring(num6 + 1, num4 - num6), out value.z))
        {
            value = default(Vector3);
            return false;
        }
        return true;
    }

    public static Vector3 ParseVector3(this DatValue node, Vector3 defaultValue = default(Vector3))
    {
        if (!node.TryParseVector3(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseVector3(this DatDictionary dictionary, string key, out Vector3 value)
    {
        if (!dictionary.TryGetNode(key, out var node))
        {
            value = default(Vector3);
            return false;
        }
        if (node is DatValue node2)
        {
            return node2.TryParseVector3(out value);
        }
        if (node is DatDictionary datDictionary)
        {
            datDictionary.TryParseFloat("X", out value.x);
            datDictionary.TryParseFloat("Y", out value.y);
            datDictionary.TryParseFloat("Z", out value.z);
            return true;
        }
        value = default(Vector3);
        return false;
    }

    public static Vector3 ParseVector3(this DatDictionary dictionary, string key, Vector3 defaultValue = default(Vector3))
    {
        if (!dictionary.TryParseVector3(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static Vector3 LegacyParseVector3(this DatDictionary dict, string key)
    {
        if (dict.TryParseVector3(key, out var value))
        {
            return value;
        }
        return new Vector3(dict.ParseFloat(key + "_X"), dict.ParseFloat(key + "_Y"), dict.ParseFloat("_Z"));
    }
}
