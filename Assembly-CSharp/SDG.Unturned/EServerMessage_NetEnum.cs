using SDG.NetPak;

namespace SDG.Unturned;

public static class EServerMessage_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EServerMessage value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        if (value2 <= 8)
        {
            value = (EServerMessage)value2;
            return result;
        }
        value = EServerMessage.GetWorkshopFiles;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EServerMessage value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
