using SDG.NetPak;

namespace SDG.Unturned;

public static class EFiremode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EFiremode value)
    {
        uint value2;
        bool result = reader.ReadBits(2, out value2);
        value = (EFiremode)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, EFiremode value)
    {
        return writer.WriteBits((uint)value, 2);
    }
}
