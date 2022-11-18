using SDG.NetPak;

namespace SDG.Unturned;

public static class ELimb_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ELimb value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        if (value2 <= 13)
        {
            value = (ELimb)value2;
            return result;
        }
        value = ELimb.LEFT_FOOT;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ELimb value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
