using SDG.NetPak;

namespace SDG.Unturned;

public static class ERaycastInfoType_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ERaycastInfoType value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        if (value2 <= 9)
        {
            value = (ERaycastInfoType)value2;
            return result;
        }
        value = ERaycastInfoType.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ERaycastInfoType value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
