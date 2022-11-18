using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerGesture_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerGesture value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        value = (EPlayerGesture)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerGesture value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
