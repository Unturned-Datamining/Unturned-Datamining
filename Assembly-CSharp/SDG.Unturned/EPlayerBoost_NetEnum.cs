using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerBoost_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerBoost value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        if (value2 <= 4)
        {
            value = (EPlayerBoost)value2;
            return result;
        }
        value = EPlayerBoost.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerBoost value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
