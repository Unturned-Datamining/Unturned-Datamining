using SDG.NetPak;

namespace SDG.Unturned;

public static class EVotingMessage_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EVotingMessage value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        if (value2 <= 4)
        {
            value = (EVotingMessage)value2;
            return result;
        }
        value = EVotingMessage.OFF;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EVotingMessage value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
