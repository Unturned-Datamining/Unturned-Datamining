using System;
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public sealed class DatDictionary : Dictionary<string, IDatNode>, IDatNode
{
    public DatDictionary()
        : base((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
    {
    }

    public bool TryGetNode(string key, out IDatNode node)
    {
        return TryGetValue(key, out node);
    }

    public bool TryGetValue(string key, out DatValue node)
    {
        IDatNode value;
        bool num = TryGetValue(key, out value);
        node = value as DatValue;
        if (num)
        {
            return node != null;
        }
        return false;
    }

    public bool TryGetDictionary(string key, out DatDictionary node)
    {
        IDatNode value;
        bool num = TryGetValue(key, out value);
        node = value as DatDictionary;
        if (num)
        {
            return node != null;
        }
        return false;
    }

    public DatDictionary GetDictionary(string key)
    {
        if (!TryGetDictionary(key, out var node))
        {
            return null;
        }
        return node;
    }

    public bool TryGetList(string key, out DatList node)
    {
        IDatNode value;
        bool num = TryGetValue(key, out value);
        node = value as DatList;
        if (num)
        {
            return node != null;
        }
        return false;
    }

    public DatList GetList(string key)
    {
        if (!TryGetList(key, out var node))
        {
            return null;
        }
        return node;
    }

    public bool TryGetString(string key, out string value)
    {
        if (TryGetValue(key, out var node))
        {
            value = node.value;
            return true;
        }
        value = null;
        return false;
    }

    public string GetString(string key, string defaultValue = null)
    {
        if (!TryGetString(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseInt8(string key, out sbyte value)
    {
        value = 0;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseInt8(out value);
        }
        return false;
    }

    public sbyte ParseInt8(string key, sbyte defaultValue = 0)
    {
        if (!TryParseInt8(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseUInt8(string key, out byte value)
    {
        value = 0;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseUInt8(out value);
        }
        return false;
    }

    public byte ParseUInt8(string key, byte defaultValue = 0)
    {
        if (!TryParseUInt8(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseInt16(string key, out short value)
    {
        value = 0;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseInt16(out value);
        }
        return false;
    }

    public short ParseInt16(string key, short defaultValue = 0)
    {
        if (!TryParseInt16(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseUInt16(string key, out ushort value)
    {
        value = 0;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseUInt16(out value);
        }
        return false;
    }

    public ushort ParseUInt16(string key, ushort defaultValue = 0)
    {
        if (!TryParseUInt16(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseInt32(string key, out int value)
    {
        value = 0;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseInt32(out value);
        }
        return false;
    }

    public int ParseInt32(string key, int defaultValue = 0)
    {
        if (!TryParseInt32(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseUInt32(string key, out uint value)
    {
        value = 0u;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseUInt32(out value);
        }
        return false;
    }

    public uint ParseUInt32(string key, uint defaultValue = 0u)
    {
        if (!TryParseUInt32(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseInt64(string key, out long value)
    {
        value = 0L;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseInt64(out value);
        }
        return false;
    }

    public long ParseInt64(string key, long defaultValue = 0L)
    {
        if (!TryParseInt64(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseUInt64(string key, out ulong value)
    {
        value = 0uL;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseUInt64(out value);
        }
        return false;
    }

    public ulong ParseUInt64(string key, ulong defaultValue = 0uL)
    {
        if (!TryParseUInt64(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseFloat(string key, out float value)
    {
        value = 0f;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseFloat(out value);
        }
        return false;
    }

    public float ParseFloat(string key, float defaultValue = 0f)
    {
        if (!TryParseFloat(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseDouble(string key, out double value)
    {
        value = 0.0;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseDouble(out value);
        }
        return false;
    }

    public double ParseDouble(string key, double defaultValue = 0.0)
    {
        if (!TryParseDouble(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseEnum<T>(string key, out T value) where T : struct
    {
        value = default(T);
        if (TryGetValue(key, out var node))
        {
            return node.TryParseEnum<T>(out value);
        }
        return false;
    }

    public T ParseEnum<T>(string key, T defaultValue = default(T)) where T : struct
    {
        if (!TryParseEnum<T>(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseBool(string key, out bool value)
    {
        value = false;
        if (TryGetValue(key, out var node))
        {
            return node.TryParseBool(out value);
        }
        return false;
    }

    public bool ParseBool(string key, bool defaultValue = false)
    {
        if (!TryParseBool(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public bool TryParseGuid(string key, out Guid value)
    {
        value = default(Guid);
        if (TryGetValue(key, out var node))
        {
            return node.TryParseGuid(out value);
        }
        return false;
    }

    public Guid ParseGuid(string key, Guid defaultValue = default(Guid))
    {
        if (!TryParseGuid(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public Type ParseType(string key, Type defaultValue = null)
    {
        Type result = defaultValue;
        if (TryGetValue(key, out var node))
        {
            result = node.ParseType(defaultValue);
        }
        return result;
    }

    public bool TryParseStruct<T>(string key, out T value) where T : struct, IDatParseable
    {
        value = default(T);
        if (TryGetNode(key, out var node))
        {
            return value.TryParse(node);
        }
        return false;
    }

    public T ParseStruct<T>(string key, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        if (!TryParseStruct<T>(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public List<T> ParseListOfStructs<T>(string key) where T : struct, IDatParseable
    {
        if (!TryGetList(key, out var node))
        {
            return null;
        }
        return node.ParseListOfStructs<T>();
    }

    public T[] ParseArrayOfStructs<T>(string key, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        if (!TryGetList(key, out var node))
        {
            return null;
        }
        return node.ParseArrayOfStructs(defaultValue);
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        output.AppendLine("{");
        using (Enumerator enumerator = GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, IDatNode> current = enumerator.Current;
                for (int i = 0; i < indentationLevel + 1; i++)
                {
                    output.Append('\t');
                }
                output.Append("\"" + current.Key + "\"");
                output.Append(" = ");
                if (current.Value != null)
                {
                    current.Value.DebugDumpToStringBuilder(output, indentationLevel + 1);
                }
                else
                {
                    output.AppendLine("null");
                }
            }
        }
        for (int j = 0; j < indentationLevel; j++)
        {
            output.Append('\t');
        }
        output.AppendLine("}");
    }

    public string DebugDumpToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }
}
