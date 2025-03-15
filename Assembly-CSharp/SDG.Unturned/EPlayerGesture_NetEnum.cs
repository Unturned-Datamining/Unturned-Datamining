using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerGesture_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerGesture value)
    {
        uint value2;
        bool result = reader.ReadBits(5, out value2);
        if (value2 <= 17)
        {
            value = (EPlayerGesture)value2;
            return result;
        }
        value = EPlayerGesture.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerGesture value)
    {
        return writer.WriteBits((uint)value, 5);
    }
}
