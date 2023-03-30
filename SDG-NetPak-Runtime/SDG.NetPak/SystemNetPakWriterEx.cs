using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.NetPak;

public static class SystemNetPakWriterEx
{
    public delegate bool WriteListItem<T>(T item);

    public delegate bool WriteListItemWithWriter<T>(NetPakWriter writer, T item);

    public static bool WriteSignedInt(this NetPakWriter writer, int value, int bitCount)
    {
        int num = 1 << bitCount - 1;
        return writer.WriteBits((uint)(value + num), bitCount);
    }

    public static bool WriteClampedFloat(this NetPakWriter writer, float value, int intBitCount, int fracBitCount)
    {
        int num = 1 << intBitCount - 1;
        if (value < (float)(-num))
        {
            return writer.WriteBits(0u, intBitCount) & writer.WriteBits(0u, fracBitCount);
        }
        if (value >= (float)num)
        {
            return writer.WriteBits(uint.MaxValue, intBitCount) & writer.WriteBits(uint.MaxValue, fracBitCount);
        }
        if (Mathf.Abs(value) < 0.0001f)
        {
            return writer.WriteBits((uint)num, intBitCount) & writer.WriteBits(0u, fracBitCount);
        }
        int num2 = Mathf.FloorToInt(value);
        bool num3 = writer.WriteBits((uint)(value + (float)num), intBitCount);
        float num4 = value - (float)num2;
        uint num5 = (uint)(1 << fracBitCount);
        uint value2 = (uint)(num4 * (float)num5);
        return num3 & writer.WriteBits(value2, fracBitCount);
    }

    public static bool WriteInt8(this NetPakWriter writer, sbyte value)
    {
        return writer.WriteBits((uint)value, 8);
    }

    public static bool WriteInt16(this NetPakWriter writer, short value)
    {
        return writer.WriteBits((uint)value, 16);
    }

    public static bool WriteInt32(this NetPakWriter writer, int value)
    {
        return writer.WriteBits((uint)value, 32);
    }

    public static bool WriteInt64(this NetPakWriter writer, long value)
    {
        return writer.WriteBits((uint)(value >> 32), 32) & writer.WriteBits((uint)value, 32);
    }

    public static bool WriteUInt8(this NetPakWriter writer, byte value)
    {
        return writer.WriteBits(value, 8);
    }

    public static bool WriteUInt16(this NetPakWriter writer, ushort value)
    {
        return writer.WriteBits(value, 16);
    }

    public static bool WriteUInt32(this NetPakWriter writer, uint value)
    {
        return writer.WriteBits(value, 32);
    }

    public static bool WriteUInt64(this NetPakWriter writer, ulong value)
    {
        return writer.WriteBits((uint)(value >> 32), 32) & writer.WriteBits((uint)value, 32);
    }

    public static bool WriteUnsignedNormalizedFloat(this NetPakWriter writer, float value, int bitCount)
    {
        uint num = (uint)((1 << bitCount) - 1);
        uint value2 = (uint)(value * (float)num + 0.5f);
        return writer.WriteBits(value2, bitCount);
    }

    public static bool WriteSignedNormalizedFloat(this NetPakWriter writer, float value, int bitCount)
    {
        uint num = (uint)(1 << bitCount - 1);
        uint num2 = num - 1;
        uint value2;
        if (value >= 0f)
        {
            value2 = (uint)(value * (float)num2 + 0.5f);
        }
        else
        {
            value2 = (uint)((0f - value) * (float)num2 + 0.5f);
            value2 |= num;
        }
        return writer.WriteBits(value2, bitCount);
    }

    public unsafe static bool WriteFloat(this NetPakWriter writer, float value)
    {
        uint value2 = *(uint*)(&value);
        return writer.WriteUInt32(value2);
    }

    public static bool WriteRadians(this NetPakWriter writer, float value, int bitCount = 8)
    {
        float num = (value % ((float)Math.PI * 2f) + (float)Math.PI * 2f) % ((float)Math.PI * 2f) / ((float)Math.PI * 2f);
        uint num2 = (uint)(1 << bitCount);
        uint value2 = (uint)(num * (float)num2);
        return writer.WriteBits(value2, bitCount);
    }

    public static bool WriteDegrees(this NetPakWriter writer, float value, int bitCount = 8)
    {
        float num = (value % 360f + 360f) % 360f / 360f;
        uint num2 = (uint)(1 << bitCount);
        uint value2 = (uint)(num * (float)num2);
        return writer.WriteBits(value2, bitCount);
    }

    public static bool WriteString(this NetPakWriter writer, string value, int lengthBitCount = 11)
    {
        if (string.IsNullOrEmpty(value))
        {
            return writer.WriteBit(value: true);
        }
        try
        {
            int num = NetPakConst.stringEncoding.GetBytes(value, 0, value.Length, NetPakConst.STRING_BUFFER, 0);
            int num2 = 1 << lengthBitCount;
            if (num > num2)
            {
                num = num2;
            }
            return writer.WriteBit(value: false) & writer.WriteBits((uint)(num - 1), lengthBitCount) & writer.WriteBytes(NetPakConst.STRING_BUFFER, num);
        }
        catch
        {
            return writer.WriteBit(value: true);
        }
    }

    public unsafe static bool WriteGuid(this NetPakWriter writer, Guid value)
    {
        ulong* ptr = (ulong*)(&value);
        ulong value2 = *ptr;
        ulong value3 = ptr[1];
        return writer.WriteUInt64(value2) & writer.WriteUInt64(value3);
    }

    public static bool WriteDateTime(this NetPakWriter writer, DateTime value)
    {
        return writer.WriteInt64(value.ToBinary());
    }

    public static bool WriteList<T>(this NetPakWriter writer, List<T> list, WriteListItem<T> writeFunc, NetLength maxLength)
    {
        uint num = maxLength.Clamp(list.Count);
        bool flag = writer.WriteBits(num, maxLength.bitCount);
        for (int i = 0; i < num; i++)
        {
            flag &= writeFunc(list[i]);
        }
        return flag;
    }

    public static bool WriteList<T>(this NetPakWriter writer, List<T> list, WriteListItemWithWriter<T> writeFunc, NetLength maxLength)
    {
        uint num = maxLength.Clamp(list.Count);
        bool flag = writer.WriteBits(num, maxLength.bitCount);
        for (int i = 0; i < num; i++)
        {
            flag &= writeFunc(writer, list[i]);
        }
        return flag;
    }

    public static bool WriteStateArray(this NetPakWriter writer, byte[] value)
    {
        byte b = (byte)value.Length;
        return writer.WriteUInt8(b) & writer.WriteBytes(value, b);
    }
}
