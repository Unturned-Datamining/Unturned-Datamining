using UnityEngine;

namespace SDG.NetPak;

public static class UnityNetPakReaderEx
{
    public static bool ReadQuaternion(this NetPakReader reader, out Quaternion value, int bitsPerComponent = 9)
    {
        if (!reader.ReadBits(2, out var value2))
        {
            value = Quaternion.identity;
            return false;
        }
        if (!reader.ReadSignedNormalizedFloat(bitsPerComponent, out var value3) || !reader.ReadSignedNormalizedFloat(bitsPerComponent, out var value4) || !reader.ReadSignedNormalizedFloat(bitsPerComponent, out var value5))
        {
            value = Quaternion.identity;
            return false;
        }
        value3 *= 0.70710677f;
        value4 *= 0.70710677f;
        value5 *= 0.70710677f;
        float num = Mathf.Sqrt(1f - (value3 * value3 + value4 * value4 + value5 * value5));
        switch (value2)
        {
        case 0u:
            value = new Quaternion(num, value3, value4, value5);
            return true;
        case 1u:
            value = new Quaternion(value3, num, value4, value5);
            return true;
        case 2u:
            value = new Quaternion(value3, value4, num, value5);
            return true;
        case 3u:
            value = new Quaternion(value3, value4, value5, num);
            return true;
        default:
            value = Quaternion.identity;
            return false;
        }
    }

    public static bool ReadNormalVector3(this NetPakReader reader, out Vector3 value, int bitsPerComponent = 9)
    {
        if (!reader.ReadBits(2, out var value2) || !reader.ReadBit(out var value3))
        {
            value = Vector3.forward;
            return false;
        }
        if (!reader.ReadSignedNormalizedFloat(bitsPerComponent, out var value4) || !reader.ReadSignedNormalizedFloat(bitsPerComponent, out var value5))
        {
            value = Vector3.forward;
            return false;
        }
        value4 *= 0.70710677f;
        value5 *= 0.70710677f;
        float num = Mathf.Sqrt(1f - (value4 * value4 + value5 * value5));
        if (value3)
        {
            num = 0f - num;
        }
        switch (value2)
        {
        case 0u:
            value = new Vector3(num, value4, value5);
            return true;
        case 1u:
            value = new Vector3(value4, num, value5);
            return true;
        case 2u:
            value = new Vector3(value4, value5, num);
            return true;
        default:
            value = Vector3.forward;
            return false;
        }
    }

    public static bool ReadClampedVector3(this NetPakReader reader, out Vector3 value, int intBitCount = 13, int fracBitCount = 7)
    {
        return reader.ReadClampedFloat(intBitCount, fracBitCount, out value.x) & reader.ReadClampedFloat(intBitCount, fracBitCount, out value.y) & reader.ReadClampedFloat(intBitCount, fracBitCount, out value.z);
    }

    public static bool ReadColor32RGB(this NetPakReader reader, out Color32 value)
    {
        byte value2;
        byte value3;
        byte value4;
        bool result = reader.ReadUInt8(out value2) & reader.ReadUInt8(out value3) & reader.ReadUInt8(out value4);
        value = new Color32(value2, value3, value4, byte.MaxValue);
        return result;
    }

    public static bool ReadColor32RGB(this NetPakReader reader, out Color value)
    {
        Color32 value2;
        bool result = reader.ReadColor32RGB(out value2);
        value = value2;
        return result;
    }

    public static bool ReadColor32RGBA(this NetPakReader reader, out Color32 value)
    {
        byte value2;
        byte value3;
        byte value4;
        byte value5;
        bool result = reader.ReadUInt8(out value2) & reader.ReadUInt8(out value3) & reader.ReadUInt8(out value4) & reader.ReadUInt8(out value5);
        value = new Color32(value2, value3, value4, value5);
        return result;
    }

    public static bool ReadColor32RGBA(this NetPakReader reader, out Color value)
    {
        Color32 value2;
        bool result = reader.ReadColor32RGBA(out value2);
        value = value2;
        return result;
    }
}
