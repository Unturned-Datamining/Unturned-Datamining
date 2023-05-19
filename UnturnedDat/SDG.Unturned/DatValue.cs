using System;
using System.Globalization;
using System.Text;

namespace SDG.Unturned;

public sealed class DatValue : IDatNode
{
    public string value;

    public EDatNodeType NodeType => EDatNodeType.Value;

    public DatValue()
    {
        value = null;
    }

    public DatValue(string value)
    {
        this.value = value;
    }

    public DatValue(sbyte value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(byte value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(short value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(ushort value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(int value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(uint value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(long value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(ulong value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(float value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(double value)
    {
        this.value = value.ToString(CultureInfo.InvariantCulture);
    }

    public DatValue(bool value)
    {
        this.value = (value ? "true" : "false");
    }

    public DatValue(Guid value)
    {
        this.value = value.ToString("N");
    }

    public bool TryParseInt8(out sbyte value)
    {
        return sbyte.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public sbyte ParseInt8(sbyte defaultValue = 0)
    {
        if (!TryParseInt8(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseUInt8(out byte value)
    {
        return byte.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public byte ParseUInt8(byte defaultValue = 0)
    {
        if (!TryParseUInt8(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseInt16(out short value)
    {
        return short.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public short ParseInt16(short defaultValue = 0)
    {
        if (!TryParseInt16(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseUInt16(out ushort value)
    {
        return ushort.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public ushort ParseUInt16(ushort defaultValue = 0)
    {
        if (!TryParseUInt16(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseInt32(out int value)
    {
        return int.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public int ParseInt32(int defaultValue = 0)
    {
        if (!TryParseInt32(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseUInt32(out uint value)
    {
        return uint.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public uint ParseUInt32(uint defaultValue = 0u)
    {
        if (!TryParseUInt32(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseInt64(out long value)
    {
        return long.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public long ParseInt64(long defaultValue = 0L)
    {
        if (!TryParseInt64(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseUInt64(out ulong value)
    {
        return ulong.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public ulong ParseUInt64(ulong defaultValue = 0uL)
    {
        if (!TryParseUInt64(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseFloat(out float value)
    {
        return float.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public float ParseFloat(float defaultValue = 0f)
    {
        if (!TryParseFloat(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseDouble(out double value)
    {
        return double.TryParse(this.value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public double ParseDouble(double defaultValue = 0.0)
    {
        if (!TryParseDouble(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseEnum<T>(out T value) where T : struct
    {
        return Enum.TryParse<T>(this.value, ignoreCase: true, out value);
    }

    public T ParseEnum<T>(T defaultValue) where T : struct
    {
        if (!TryParseEnum<T>(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseBool(out bool value)
    {
        if (!string.IsNullOrEmpty(this.value))
        {
            if (this.value.Length != 1)
            {
                return bool.TryParse(this.value, out value);
            }
            switch (this.value[0])
            {
            case '1':
            case 't':
            case 'y':
                value = true;
                return true;
            case '0':
            case 'f':
            case 'n':
                value = false;
                return true;
            }
        }
        value = false;
        return false;
    }

    public bool ParseBool(bool defaultValue = false)
    {
        if (!TryParseBool(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseGuid(out Guid value)
    {
        return Guid.TryParse(this.value, out value);
    }

    public Guid ParseGuid(Guid defaultValue = default(Guid))
    {
        if (!TryParseGuid(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public bool TryParseDateTimeUtc(out DateTime value)
    {
        return DateTime.TryParse(this.value, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    public DateTime ParseDateTimeUtc(DateTime defaultValue = default(DateTime))
    {
        if (!TryParseDateTimeUtc(out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public Type ParseType(Type defaultValue = null)
    {
        Type type = Type.GetType(value, throwOnError: false, ignoreCase: true);
        if (!(type != null))
        {
            return defaultValue;
        }
        return type;
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        if (value != null)
        {
            output.AppendLine("\"" + value + "\"");
        }
        else
        {
            output.AppendLine("value(null)");
        }
    }

    public string DebugDumpToString()
    {
        if (value == null)
        {
            return "value(null)";
        }
        return "\"" + value + "\"";
    }
}
