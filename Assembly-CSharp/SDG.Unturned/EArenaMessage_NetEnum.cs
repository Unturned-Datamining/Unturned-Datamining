using SDG.NetPak;

namespace SDG.Unturned;

public static class EArenaMessage_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EArenaMessage value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        value = (EArenaMessage)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, EArenaMessage value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
