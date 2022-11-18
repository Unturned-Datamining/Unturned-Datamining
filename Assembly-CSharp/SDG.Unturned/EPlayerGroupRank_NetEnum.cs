using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerGroupRank_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerGroupRank value)
    {
        uint value2;
        bool result = reader.ReadBits(2, out value2);
        if (value2 <= 2)
        {
            value = (EPlayerGroupRank)value2;
            return result;
        }
        value = EPlayerGroupRank.MEMBER;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerGroupRank value)
    {
        return writer.WriteBits((uint)value, 2);
    }
}
