using SDG.NetPak;

namespace SDG.Unturned;

public static class ENPCHoliday_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ENPCHoliday value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        if (value2 <= 5)
        {
            value = (ENPCHoliday)value2;
            return result;
        }
        value = ENPCHoliday.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ENPCHoliday value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
