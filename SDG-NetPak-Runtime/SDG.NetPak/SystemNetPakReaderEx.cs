using System;
using System.Collections.Generic;

namespace SDG.NetPak;

public static class SystemNetPakReaderEx
{
    public delegate bool ReadListItem<T>(out T item);

    public static bool ReadSignedInt(this NetPakReader reader, int bitCount, out int value)
    {
        uint value2;
        bool result = reader.ReadBits(bitCount, out value2);
        int num = 1 << bitCount - 1;
        value = (int)value2 - num;
        return result;
    }

    public static bool ReadClampedFloat(this NetPakReader reader, int intBitCount, int fracBitCount, out float value)
    {
        int value2;
        uint value3;
        bool result = reader.ReadSignedInt(intBitCount, out value2) & reader.ReadBits(fracBitCount, out value3);
        uint num = (uint)(1 << fracBitCount);
        float num2 = (float)value3 / (float)num;
        value = (float)value2 + num2;
        return result;
    }

    public static bool ReadInt8(this NetPakReader reader, out sbyte value)
    {
        uint value2;
        bool result = reader.ReadBits(8, out value2);
        value = (sbyte)value2;
        return result;
    }

    public static bool ReadInt16(this NetPakReader reader, out short value)
    {
        uint value2;
        bool result = reader.ReadBits(16, out value2);
        value = (short)value2;
        return result;
    }

    public static bool ReadInt32(this NetPakReader reader, out int value)
    {
        uint value2;
        bool result = reader.ReadBits(32, out value2);
        value = (int)value2;
        return result;
    }

    public static bool ReadInt64(this NetPakReader reader, out long value)
    {
        uint value2;
        uint value3;
        bool result = reader.ReadBits(32, out value2) & reader.ReadBits(32, out value3);
        value = (long)(((ulong)value2 << 32) | value3);
        return result;
    }

    public static bool ReadUInt8(this NetPakReader reader, out byte value)
    {
        uint value2;
        bool result = reader.ReadBits(8, out value2);
        value = (byte)value2;
        return result;
    }

    public static bool ReadUInt16(this NetPakReader reader, out ushort value)
    {
        uint value2;
        bool result = reader.ReadBits(16, out value2);
        value = (ushort)value2;
        return result;
    }

    public static bool ReadUInt32(this NetPakReader reader, out uint value)
    {
        return reader.ReadBits(32, out value);
    }

    public static bool ReadUInt64(this NetPakReader reader, out ulong value)
    {
        uint value2;
        uint value3;
        bool result = reader.ReadBits(32, out value2) & reader.ReadBits(32, out value3);
        value = ((ulong)value2 << 32) | value3;
        return result;
    }

    public static bool ReadUnsignedNormalizedFloat(this NetPakReader reader, int bitCount, out float value)
    {
        uint value2;
        bool result = reader.ReadBits(bitCount, out value2);
        uint num = (uint)((1 << bitCount) - 1);
        value = (float)value2 / (float)num;
        return result;
    }

    public static bool ReadSignedNormalizedFloat(this NetPakReader reader, int bitCount, out float value)
    {
        uint value2;
        bool result = reader.ReadBits(bitCount, out value2);
        uint num = (uint)(1 << bitCount - 1);
        uint num2 = num - 1;
        if ((value2 & num) == num)
        {
            value = 0f - (float)(value2 & num2) / (float)num2;
            return result;
        }
        value = (float)value2 / (float)num2;
        return result;
    }

    public unsafe static bool ReadFloat(this NetPakReader reader, out float value)
    {
        uint value2;
        bool result = reader.ReadUInt32(out value2);
        value = *(float*)(&value2);
        return result;
    }

    public static bool ReadRadians(this NetPakReader reader, out float value, int bitCount = 8)
    {
        uint value2;
        bool result = reader.ReadBits(bitCount, out value2);
        uint num = (uint)(1 << bitCount);
        value = (float)value2 / (float)num * ((float)Math.PI * 2f);
        return result;
    }

    public static bool ReadDegrees(this NetPakReader reader, out float value, int bitCount = 8)
    {
        uint value2;
        bool result = reader.ReadBits(bitCount, out value2);
        uint num = (uint)(1 << bitCount);
        value = (float)value2 / (float)num * 360f;
        return result;
    }

    public static bool ReadString(this NetPakReader reader, out string value, int lengthBitCount = 11)
    {
        if (!reader.ReadBit(out var value2))
        {
            value = string.Empty;
            return false;
        }
        if (value2)
        {
            value = string.Empty;
            return true;
        }
        if (!reader.ReadBits(lengthBitCount, out var value3))
        {
            value = string.Empty;
            return false;
        }
        int num = (int)(value3 + 1);
        if (!reader.ReadBytes(NetPakConst.STRING_BUFFER, num))
        {
            value = string.Empty;
            return false;
        }
        try
        {
            value = NetPakConst.stringEncoding.GetString(NetPakConst.STRING_BUFFER, 0, num);
            return true;
        }
        catch
        {
            value = string.Empty;
            return false;
        }
    }

    public unsafe static bool ReadGuid(this NetPakReader reader, out Guid value)
    {
        ulong value2;
        ulong value3;
        bool result = reader.ReadUInt64(out value2) & reader.ReadUInt64(out value3);
        fixed (Guid* ptr = &value)
        {
            ulong* ptr2 = (ulong*)ptr;
            *ptr2 = value2;
            ptr2[1] = value3;
        }
        return result;
    }

    public static bool ReadList<T>(this NetPakReader reader, List<T> list, ReadListItem<T> readFunc, NetLength maxLength)
    {
        uint value;
        bool flag = reader.ReadBits(maxLength.bitCount, out value);
        value = maxLength.Clamp(value);
        for (int i = 0; i < value; i++)
        {
            flag &= readFunc(out var item);
            list.Add(item);
        }
        return flag;
    }

    public static bool ReadStateArray(this NetPakReader reader, out byte[] value)
    {
        byte value2;
        bool num = reader.ReadUInt8(out value2);
        value = new byte[value2];
        return num & reader.ReadBytes(value, value2);
    }
}
