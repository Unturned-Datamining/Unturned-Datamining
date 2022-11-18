using SDG.NetPak;

namespace SDG.Unturned;

public static class EDeathCause_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EDeathCause value)
    {
        uint value2;
        bool result = reader.ReadBits(5, out value2);
        if (value2 <= 29)
        {
            value = (EDeathCause)value2;
            return result;
        }
        value = EDeathCause.BLEEDING;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EDeathCause value)
    {
        return writer.WriteBits((uint)value, 5);
    }
}
