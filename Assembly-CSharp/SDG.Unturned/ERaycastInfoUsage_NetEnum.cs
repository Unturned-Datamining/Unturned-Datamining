using SDG.NetPak;

namespace SDG.Unturned;

public static class ERaycastInfoUsage_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ERaycastInfoUsage value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        if (value2 <= 14)
        {
            value = (ERaycastInfoUsage)value2;
            return result;
        }
        value = ERaycastInfoUsage.Punch;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ERaycastInfoUsage value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
