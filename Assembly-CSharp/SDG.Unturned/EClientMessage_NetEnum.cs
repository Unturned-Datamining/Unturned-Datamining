using SDG.NetPak;

namespace SDG.Unturned;

public static class EClientMessage_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EClientMessage value)
    {
        uint value2;
        bool result = reader.ReadBits(5, out value2);
        if (value2 <= 17)
        {
            value = (EClientMessage)value2;
            return result;
        }
        value = EClientMessage.UPDATE_RELIABLE_BUFFER;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EClientMessage value)
    {
        return writer.WriteBits((uint)value, 5);
    }
}
