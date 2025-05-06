using System.Globalization;
using UnityEngine;

namespace SDG.Unturned;

public static class UnityDatColorEx
{
    public static bool TryParseColor32RGB(this IDatValue node, out Color32 value)
    {
        if (string.IsNullOrEmpty(node.Value))
        {
            value = new Color32(0, 0, 0, byte.MaxValue);
            return false;
        }
        int num = ((node.Value[0] == '#') ? 1 : 0);
        if (node.Value.Length != 6 + num)
        {
            value = new Color32(0, 0, 0, byte.MaxValue);
            return false;
        }
        if (!byte.TryParse(node.Value.Substring(num, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
        {
            value = new Color32(0, 0, 0, byte.MaxValue);
            return false;
        }
        if (!byte.TryParse(node.Value.Substring(num + 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result2))
        {
            value = new Color32(0, 0, 0, byte.MaxValue);
            return false;
        }
        if (!byte.TryParse(node.Value.Substring(num + 4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result3))
        {
            value = new Color32(0, 0, 0, byte.MaxValue);
            return false;
        }
        value = new Color32(result, result2, result3, byte.MaxValue);
        return true;
    }

    public static Color32 ParseColor32RGB(this IDatValue node, Color32 defaultValue = default(Color32))
    {
        if (!node.TryParseColor32RGB(out var value))
        {
            return new Color32(defaultValue.r, defaultValue.g, defaultValue.b, byte.MaxValue);
        }
        return value;
    }

    public static bool TryParseColor32RGB(this IDatDictionary dictionary, string key, out Color32 value)
    {
        if (!dictionary.TryGetNode(key, out var node))
        {
            value = new Color32(0, 0, 0, byte.MaxValue);
            return false;
        }
        if (node is IDatValue node2)
        {
            return node2.TryParseColor32RGB(out value);
        }
        if (node is IDatDictionary dictionary2)
        {
            dictionary2.TryParseUInt8("R", out var value2);
            dictionary2.TryParseUInt8("G", out var value3);
            dictionary2.TryParseUInt8("B", out var value4);
            value = new Color32(value2, value3, value4, byte.MaxValue);
            return true;
        }
        value = new Color32(0, 0, 0, byte.MaxValue);
        return false;
    }

    public static Color32 ParseColor32RGB(this IDatDictionary dictionary, string key, Color32 defaultValue = default(Color32))
    {
        if (!dictionary.TryParseColor32RGB(key, out var value))
        {
            return new Color32(defaultValue.r, defaultValue.g, defaultValue.b, byte.MaxValue);
        }
        return value;
    }

    public static bool TryParseColor32RGBA(this IDatValue node, out Color32 value)
    {
        if (string.IsNullOrEmpty(node.Value))
        {
            value = default(Color32);
            return false;
        }
        int num = ((node.Value[0] == '#') ? 1 : 0);
        bool flag;
        if (node.Value.Length == 8 + num)
        {
            flag = true;
        }
        else
        {
            if (node.Value.Length != 6 + num)
            {
                value = default(Color32);
                return false;
            }
            flag = false;
        }
        if (!byte.TryParse(node.Value.Substring(num, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
        {
            value = default(Color32);
            return false;
        }
        if (!byte.TryParse(node.Value.Substring(num + 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result2))
        {
            value = default(Color32);
            return false;
        }
        if (!byte.TryParse(node.Value.Substring(num + 4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result3))
        {
            value = default(Color32);
            return false;
        }
        byte result4;
        if (flag)
        {
            if (!byte.TryParse(node.Value.Substring(num + 6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result4))
            {
                value = default(Color32);
                return false;
            }
        }
        else
        {
            result4 = byte.MaxValue;
        }
        value = new Color32(result, result2, result3, result4);
        return true;
    }

    public static Color32 ParseColor32RGBA(this IDatValue node, Color32 defaultValue = default(Color32))
    {
        if (!node.TryParseColor32RGBA(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseColor32RGBA(this IDatDictionary dictionary, string key, out Color32 value)
    {
        if (!dictionary.TryGetNode(key, out var node))
        {
            value = default(Color32);
            return false;
        }
        if (node is IDatValue node2)
        {
            return node2.TryParseColor32RGBA(out value);
        }
        if (node is IDatDictionary dictionary2)
        {
            dictionary2.TryParseUInt8("R", out var value2);
            dictionary2.TryParseUInt8("G", out var value3);
            dictionary2.TryParseUInt8("B", out var value4);
            dictionary2.TryParseUInt8("A", out var value5);
            value = new Color32(value2, value3, value4, value5);
            return true;
        }
        value = default(Color32);
        return false;
    }

    public static Color32 ParseColor32RGBA(this IDatDictionary dictionary, string key, Color32 defaultValue = default(Color32))
    {
        if (!dictionary.TryParseColor32RGBA(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static Color LegacyParseColor(this IDatDictionary dict, string key, Color defaultValue)
    {
        if (dict.TryParseColor32RGB(key, out var value))
        {
            Color result = value;
            result.a = 1f;
            return result;
        }
        return new Color(dict.ParseFloat(key + "_R", defaultValue.r), dict.ParseFloat(key + "_G", defaultValue.g), dict.ParseFloat(key + "_B", defaultValue.b));
    }

    public static Color32 LegacyParseColor32RGB(this IDatDictionary dict, string key, Color32 defaultValue)
    {
        if (dict.TryParseColor32RGB(key, out var value))
        {
            return value;
        }
        return new Color32(dict.ParseUInt8(key + "_R", defaultValue.r), dict.ParseUInt8(key + "_G", defaultValue.g), dict.ParseUInt8(key + "_B", defaultValue.b), byte.MaxValue);
    }
}
