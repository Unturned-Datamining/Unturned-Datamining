using UnityEngine;

namespace SDG.NetPak;

public static class UnityNetPakWriterEx
{
    /// <summary>
    /// Uses "smallest three" optimization described by Glenn Fiedler: https://gafferongames.com/post/snapshot_compression/
    /// Quoting here in case the link moves: "Since we know the quaternion represents a rotation its length must
    /// be 1, so x^2+y^2+z^2+w^2 = 1. We can use this identity to drop one component and reconstruct it on the
    /// other side. For example, if you send x,y,z you can reconstruct w = sqrt(1 - x^2 - y^2 - z^2). You might
    /// think you need to send a sign bit for w in case it is negative, but you don’t, because you can make w always
    /// positive by negating the entire quaternion if w is negative (in quaternion space (x,y,z,w) and (-x,-y,-z,-w)
    /// represent the same rotation.) Don’t always drop the same component due to numerical precision issues.
    /// Instead, find the component with the largest absolute value and encode its index using two bits [0, 3]
    /// (0=x, 1=y, 2=z, 3=w), then send the index of the largest component and the smallest three components over
    /// the network (hence the name). On the other side use the index of the largest bit to know which component
    /// you have to reconstruct from the other three."
    /// </summary>
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

    /// <summary>
    /// Similar to the quaternion optimization, but needs a sign bit for the largest value.
    /// </summary>
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

    /// <summary>
    /// Default intBitCount of 13 allows a range of [-4096, +4096).
    /// </summary>
    public static bool WriteClampedVector3(this NetPakWriter writer, Vector3 value, int intBitCount = 13, int fracBitCount = 7)
    {
        return writer.WriteClampedFloat(value.x, intBitCount, fracBitCount) & writer.WriteClampedFloat(value.y, intBitCount, fracBitCount) & writer.WriteClampedFloat(value.z, intBitCount, fracBitCount);
    }

    /// <summary>
    /// Write 8-bit per channel color excluding alpha.
    /// </summary>
    public static bool WriteColor32RGB(this NetPakWriter writer, Color32 value)
    {
        return writer.WriteUInt8(value.r) & writer.WriteUInt8(value.g) & writer.WriteUInt8(value.b);
    }

    /// <summary>
    /// Write 8-bit per channel color including alpha.
    /// </summary>
    public static bool WriteColor32RGBA(this NetPakWriter writer, Color32 value)
    {
        return writer.WriteUInt8(value.r) & writer.WriteUInt8(value.g) & writer.WriteUInt8(value.b) & writer.WriteUInt8(value.a);
    }

    /// <summary>
    /// Note: "Special" here refers to the -90 rotation on the X axis. :)
    /// If quaternion is only a rotation around the Y axis (yaw) which is common for barricades and structures,
    /// write only yaw. Otherwise, write full quaternion.
    /// </summary>
    public static bool WriteSpecialYawOrQuaternion(this NetPakWriter writer, Quaternion value, int yawBitCount = 9, int quaternionBitsPerComponent = 9)
    {
        bool flag;
        if ((value * Vector3.forward).y > 0.9999f)
        {
            flag = writer.WriteBit(value: true);
            Vector3 vector = value * Vector3.up;
            Vector2 normalized = new Vector2(0f - vector.z, 0f - vector.x).normalized;
            float value2 = Mathf.Atan2(normalized.y, normalized.x);
            return flag & writer.WriteRadians(value2, yawBitCount);
        }
        flag = writer.WriteBit(value: false);
        return flag & writer.WriteQuaternion(value, quaternionBitsPerComponent);
    }
}
