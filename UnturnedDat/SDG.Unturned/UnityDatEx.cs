using UnityEngine;

namespace SDG.Unturned;

public static class UnityDatEx
{
    public static bool TryParseVector2(this IDatValue node, out Vector2 value)
    {
        if (string.IsNullOrEmpty(node.Value))
        {
            value = default(Vector2);
            return false;
        }
        int num = node.Value.IndexOf('(');
        int num3;
        int num4;
        if (num >= 0)
        {
            int num2 = node.Value.IndexOf(')', num + 2);
            if (num2 < 0)
            {
                value = default(Vector2);
                return false;
            }
            num3 = num + 1;
            num4 = num2 - 1;
        }
        else
        {
            num3 = 0;
            num4 = node.Value.Length - 1;
        }
        int num5 = node.Value.IndexOf(',', num3);
        if (num5 < 0 || num5 + 1 > num4)
        {
            value = default(Vector2);
            return false;
        }
        if (!float.TryParse(node.Value.Substring(num3, num5 - num3), out value.x))
        {
            value = default(Vector2);
            return false;
        }
        if (!float.TryParse(node.Value.Substring(num5 + 1, num4 - num5), out value.y))
        {
            value = default(Vector2);
            return false;
        }
        return true;
    }

    public static Vector2 ParseVector2(this IDatValue node, Vector2 defaultValue = default(Vector2))
    {
        if (!node.TryParseVector2(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseVector2(this IDatDictionary dictionary, string key, out Vector2 value)
    {
        if (!dictionary.TryGetNode(key, out var node))
        {
            value = default(Vector2);
            return false;
        }
        if (node is IDatValue node2)
        {
            return node2.TryParseVector2(out value);
        }
        if (node is IDatDictionary dictionary2)
        {
            dictionary2.TryParseFloat("X", out value.x);
            dictionary2.TryParseFloat("Y", out value.y);
            return true;
        }
        value = default(Vector2);
        return false;
    }

    public static Vector2 ParseVector2(this IDatDictionary dictionary, string key, Vector2 defaultValue = default(Vector2))
    {
        if (!dictionary.TryParseVector2(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseVector3(this IDatValue node, out Vector3 value)
    {
        if (string.IsNullOrEmpty(node.Value))
        {
            value = default(Vector3);
            return false;
        }
        int num = node.Value.IndexOf('(');
        int num3;
        int num4;
        if (num >= 0)
        {
            int num2 = node.Value.IndexOf(')', num + 2);
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
            num4 = node.Value.Length - 1;
        }
        int num5 = node.Value.IndexOf(',', num3);
        if (num5 < 0 || num5 + 2 > num4)
        {
            value = default(Vector3);
            return false;
        }
        int num6 = node.Value.IndexOf(',', num5 + 2);
        if (num6 < 0 || num6 + 1 > num4)
        {
            value = default(Vector3);
            return false;
        }
        if (!float.TryParse(node.Value.Substring(num3, num5 - num3), out value.x))
        {
            value = default(Vector3);
            return false;
        }
        if (!float.TryParse(node.Value.Substring(num5 + 1, num6 - num5 - 1), out value.y))
        {
            value = default(Vector3);
            return false;
        }
        if (!float.TryParse(node.Value.Substring(num6 + 1, num4 - num6), out value.z))
        {
            value = default(Vector3);
            return false;
        }
        return true;
    }

    public static Vector3 ParseVector3(this IDatValue node, Vector3 defaultValue = default(Vector3))
    {
        if (!node.TryParseVector3(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseVector3(this IDatDictionary dictionary, string key, out Vector3 value)
    {
        if (!dictionary.TryGetNode(key, out var node))
        {
            value = default(Vector3);
            return false;
        }
        if (node is IDatValue node2)
        {
            return node2.TryParseVector3(out value);
        }
        if (node is IDatDictionary dictionary2)
        {
            dictionary2.TryParseFloat("X", out value.x);
            dictionary2.TryParseFloat("Y", out value.y);
            dictionary2.TryParseFloat("Z", out value.z);
            return true;
        }
        value = default(Vector3);
        return false;
    }

    public static Vector3 ParseVector3(this IDatDictionary dictionary, string key, Vector3 defaultValue = default(Vector3))
    {
        if (!dictionary.TryParseVector3(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static Vector3 LegacyParseVector3(this IDatDictionary dict, string key)
    {
        if (dict.TryParseVector3(key, out var value))
        {
            return value;
        }
        return new Vector3(dict.ParseFloat(key + "_X"), dict.ParseFloat(key + "_Y"), dict.ParseFloat(key + "_Z"));
    }
}
