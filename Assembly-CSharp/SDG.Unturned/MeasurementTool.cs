using System;

namespace SDG.Unturned;

public class MeasurementTool
{
    public static float speedToKPH(float speed)
    {
        return speed * 3.6f;
    }

    public static float KPHToMPH(float kph)
    {
        return kph / 1.609344f;
    }

    public static float KtoM(float k)
    {
        return k * 0.621371f;
    }

    public static float MtoYd(float m)
    {
        return m * 1.09361f;
    }

    public static int MtoYd(int m)
    {
        return (int)((float)m * 1.09361f);
    }

    public static long MtoYd(long m)
    {
        return (long)((float)m * 1.09361f);
    }

    public static byte angleToByte(float angle)
    {
        if (angle < 0f)
        {
            return (byte)((360f + angle % 360f) / 2f);
        }
        return (byte)(angle % 360f / 2f);
    }

    public static float byteToAngle(byte angle)
    {
        return (float)(int)angle * 2f;
    }

    [Obsolete("Newer code should not be using this, instead NetPak should handle it.")]
    public static byte angleToByte2(float angle)
    {
        if (angle < 0f)
        {
            return (byte)((360f + angle % 360f) / 1.5f);
        }
        return (byte)(angle % 360f / 1.5f);
    }

    [Obsolete("Newer code should not be using this, instead NetPak should handle it.")]
    public static float byteToAngle2(byte angle)
    {
        return (float)(int)angle * 1.5f;
    }
}
