using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerMessage_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerMessage value)
    {
        uint value2;
        bool result = reader.ReadBits(7, out value2);
        if (value2 <= 112)
        {
            value = (EPlayerMessage)value2;
            return result;
        }
        value = EPlayerMessage.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerMessage value)
    {
        return writer.WriteBits((uint)value, 7);
    }
}
