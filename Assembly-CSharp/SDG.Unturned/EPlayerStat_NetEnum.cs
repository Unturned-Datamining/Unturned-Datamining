using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerStat_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerStat value)
    {
        uint value2;
        bool result = reader.ReadBits(5, out value2);
        if (value2 <= 18)
        {
            value = (EPlayerStat)value2;
            return result;
        }
        value = EPlayerStat.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerStat value)
    {
        return writer.WriteBits((uint)value, 5);
    }
}
