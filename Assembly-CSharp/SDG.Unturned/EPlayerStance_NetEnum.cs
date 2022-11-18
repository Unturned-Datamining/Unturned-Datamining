using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerStance_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerStance value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        value = (EPlayerStance)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerStance value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
