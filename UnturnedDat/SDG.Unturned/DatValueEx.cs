using System;
using System.Globalization;

namespace SDG.Unturned;

public static class DatValueEx
{
    public static bool IsValueNullOrEmpty(this IDatValue valueNode)
    {
        if (valueNode != null)
        {
            return string.IsNullOrEmpty(valueNode.Value);
        }
        return true;
    }

    public static bool TryParseInt8(this IDatValue valueNode, out sbyte value)
    {
        return sbyte.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static sbyte ParseInt8(this IDatValue valueNode, sbyte defaultValue = 0)
    {
        if (!valueNode.TryParseInt8(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt8(this IDatValue valueNode, out byte value)
    {
        return byte.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static byte ParseUInt8(this IDatValue valueNode, byte defaultValue = 0)
    {
        if (!valueNode.TryParseUInt8(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt16(this IDatValue valueNode, out short value)
    {
        return short.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static short ParseInt16(this IDatValue valueNode, short defaultValue = 0)
    {
        if (!valueNode.TryParseInt16(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt16(this IDatValue valueNode, out ushort value)
    {
        return ushort.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static ushort ParseUInt16(this IDatValue valueNode, ushort defaultValue = 0)
    {
        if (!valueNode.TryParseUInt16(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt32(this IDatValue valueNode, out int value)
    {
        return int.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static int ParseInt32(this IDatValue valueNode, int defaultValue = 0)
    {
        if (!valueNode.TryParseInt32(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt32(this IDatValue valueNode, out uint value)
    {
        return uint.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static uint ParseUInt32(this IDatValue valueNode, uint defaultValue = 0u)
    {
        if (!valueNode.TryParseUInt32(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseInt64(this IDatValue valueNode, out long value)
    {
        return long.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static long ParseInt64(this IDatValue valueNode, long defaultValue = 0L)
    {
        if (!valueNode.TryParseInt64(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseUInt64(this IDatValue valueNode, out ulong value)
    {
        return ulong.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static ulong ParseUInt64(this IDatValue valueNode, ulong defaultValue = 0uL)
    {
        if (!valueNode.TryParseUInt64(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseFloat(this IDatValue valueNode, out float value)
    {
        return float.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static float ParseFloat(this IDatValue valueNode, float defaultValue = 0f)
    {
        if (!valueNode.TryParseFloat(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseDouble(this IDatValue valueNode, out double value)
    {
        return double.TryParse(valueNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static double ParseDouble(this IDatValue valueNode, double defaultValue = 0.0)
    {
        if (!valueNode.TryParseDouble(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseEnum<T>(this IDatValue valueNode, out T value) where T : struct
    {
        return Enum.TryParse<T>(valueNode.Value, ignoreCase: true, out value);
    }

    public static T ParseEnum<T>(this IDatValue valueNode, T defaultValue) where T : struct
    {
        if (!valueNode.TryParseEnum<T>(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseEnum(this IDatValue valueNode, Type enumType, out object value)
    {
        return Enum.TryParse(enumType, valueNode.Value, ignoreCase: true, out value);
    }

    public static object ParseEnum(this IDatValue valueNode, Type enumType, object defaultValue)
    {
        if (!valueNode.TryParseEnum(enumType, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseBool(this IDatValue valueNode, out bool value)
    {
        if (!string.IsNullOrEmpty(valueNode.Value))
        {
            if (valueNode.Value.Length != 1)
            {
                return bool.TryParse(valueNode.Value, out value);
            }
            switch (valueNode.Value[0])
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

    public static bool ParseBool(this IDatValue valueNode, bool defaultValue = false)
    {
        if (!valueNode.TryParseBool(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseGuid(this IDatValue valueNode, out Guid value)
    {
        return Guid.TryParse(valueNode.Value, out value);
    }

    public static Guid ParseGuid(this IDatValue valueNode, Guid defaultValue = default(Guid))
    {
        if (!valueNode.TryParseGuid(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryParseDateTimeUtc(this IDatValue valueNode, out DateTime value)
    {
        DateTimeStyles styles = DateTimeStyles.AssumeUniversal;
        bool result = DateTime.TryParse(valueNode.Value, CultureInfo.InvariantCulture, styles, out value);
        value = value.ToUniversalTime();
        return result;
    }

    public static DateTime ParseDateTimeUtc(this IDatValue valueNode, DateTime defaultValue = default(DateTime))
    {
        if (!valueNode.TryParseDateTimeUtc(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static Type ParseType(this IDatValue valueNode, Type defaultValue = null)
    {
        if (string.IsNullOrEmpty(valueNode.Value) || valueNode.Value.IndexOfAny(DatValue.INVALID_TYPE_CHARS) >= 0)
        {
            return defaultValue;
        }
        Type type = Type.GetType(valueNode.Value, throwOnError: false, ignoreCase: true);
        if (!(type != null))
        {
            return defaultValue;
        }
        return type;
    }

    public static TValueNode SetString<TValueNode>(this TValueNode valueNode, string value) where TValueNode : IDatValue
    {
        valueNode.Value = value;
        return valueNode;
    }

    public static TValueNode SetInt8<TValueNode>(this TValueNode valueNode, sbyte value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetUInt8<TValueNode>(this TValueNode valueNode, byte value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetInt16<TValueNode>(this TValueNode valueNode, short value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetUInt16<TValueNode>(this TValueNode valueNode, ushort value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetInt32<TValueNode>(this TValueNode valueNode, int value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetUInt32<TValueNode>(this TValueNode valueNode, uint value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetInt64<TValueNode>(this TValueNode valueNode, long value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetUInt64<TValueNode>(this TValueNode valueNode, ulong value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetFloat<TValueNode>(this TValueNode valueNode, float value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetDouble<TValueNode>(this TValueNode valueNode, double value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString(CultureInfo.InvariantCulture);
        return valueNode;
    }

    public static TValueNode SetBool<TValueNode>(this TValueNode valueNode, bool value) where TValueNode : IDatValue
    {
        valueNode.Value = (value ? "true" : "false");
        return valueNode;
    }

    public static TValueNode SetGuid<TValueNode>(this TValueNode valueNode, Guid value) where TValueNode : IDatValue
    {
        valueNode.Value = value.ToString("N");
        return valueNode;
    }

    public static TValueNode SetEnumString<TValueNode, TEnum>(this TValueNode valueNode, TEnum value) where TValueNode : IDatValue where TEnum : struct
    {
        valueNode.Value = value.ToString();
        return valueNode;
    }

    public static TValueNode SetDateTimeUtc<TValueNode>(this TValueNode valueNode, DateTime value) where TValueNode : IDatValue
    {
        if (value.Hour == 0 && value.Minute == 0 && value.Second == 0)
        {
            valueNode.Value = value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
        }
        else
        {
            valueNode.Value = value.ToString("yyyy'-'MM'-'dd HH':'mm':'ss", CultureInfo.InvariantCulture);
        }
        return valueNode;
    }
}
