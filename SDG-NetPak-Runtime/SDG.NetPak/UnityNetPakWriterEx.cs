using UnityEngine;

namespace SDG.NetPak;

public static class UnityNetPakWriterEx
{
    public static bool WriteQuaternion(this NetPakWriter writer, Quaternion value, int bitsPerComponent = 9)
    {
        int num = 0;
        float num2;
        float num3;
        if (value.x < 0f)
        {
            num2 = 0f - value.x;
            num3 = -1f;
        }
        else
        {
            num2 = value.x;
            num3 = 1f;
        }
        for (int i = 1; i < 4; i++)
        {
            float num4 = value[i];
            if (num4 < 0f)
            {
                num4 = 0f - num4;
                if (num4 > num2)
                {
                    num = i;
                    num2 = num4;
                    num3 = -1f;
                }
            }
            else if (num4 > num2)
            {
                num = i;
                num2 = num4;
                num3 = 1f;
            }
        }
        float num5;
        float num6;
        float num7;
        switch (num)
        {
        case 0:
            num5 = value.y;
            num6 = value.z;
            num7 = value.w;
            break;
        case 1:
            num5 = value.x;
            num6 = value.z;
            num7 = value.w;
            break;
        case 2:
            num5 = value.x;
            num6 = value.y;
            num7 = value.w;
            break;
        case 3:
            num5 = value.x;
            num6 = value.y;
            num7 = value.z;
            break;
        default:
            return false;
        }
        return writer.WriteBits((uint)num, 2) & writer.WriteSignedNormalizedFloat(num5 * num3 * 1.4142135f, bitsPerComponent) & writer.WriteSignedNormalizedFloat(num6 * num3 * 1.4142135f, bitsPerComponent) & writer.WriteSignedNormalizedFloat(num7 * num3 * 1.4142135f, bitsPerComponent);
    }

    public static bool WriteNormalVector3(this NetPakWriter writer, Vector3 value, int bitsPerComponent = 9)
    {
        int num = 0;
        float num2;
        bool value2;
        if (value.x < 0f)
        {
            num2 = 0f - value.x;
            value2 = true;
        }
        else
        {
            num2 = value.x;
            value2 = false;
        }
        for (int i = 1; i < 3; i++)
        {
            float num3 = value[i];
            if (num3 < 0f)
            {
                num3 = 0f - num3;
                if (num3 > num2)
                {
                    num = i;
                    num2 = num3;
                    value2 = true;
                }
            }
            else if (num3 > num2)
            {
                num = i;
                num2 = num3;
                value2 = false;
            }
        }
        float num4;
        float num5;
        switch (num)
        {
        case 0:
            num4 = value.y;
            num5 = value.z;
            break;
        case 1:
            num4 = value.x;
            num5 = value.z;
            break;
        case 2:
            num4 = value.x;
            num5 = value.y;
            break;
        default:
            return false;
        }
        return writer.WriteBits((uint)num, 2) & writer.WriteBit(value2) & writer.WriteSignedNormalizedFloat(num4 * 1.4142135f, bitsPerComponent) & writer.WriteSignedNormalizedFloat(num5 * 1.4142135f, bitsPerComponent);
    }

    public static bool WriteClampedVector3(this NetPakWriter writer, Vector3 value, int intBitCount = 13, int fracBitCount = 7)
    {
        return writer.WriteClampedFloat(value.x, intBitCount, fracBitCount) & writer.WriteClampedFloat(value.y, intBitCount, fracBitCount) & writer.WriteClampedFloat(value.z, intBitCount, fracBitCount);
    }

    public static bool WriteColor32RGB(this NetPakWriter writer, Color32 value)
    {
        return writer.WriteUInt8(value.r) & writer.WriteUInt8(value.g) & writer.WriteUInt8(value.b);
    }

    public static bool WriteColor32RGBA(this NetPakWriter writer, Color32 value)
    {
        return writer.WriteUInt8(value.r) & writer.WriteUInt8(value.g) & writer.WriteUInt8(value.b) & writer.WriteUInt8(value.a);
    }
}
