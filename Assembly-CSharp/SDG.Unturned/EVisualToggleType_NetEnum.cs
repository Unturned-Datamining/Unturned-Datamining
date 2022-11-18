using SDG.NetPak;

namespace SDG.Unturned;

public static class EVisualToggleType_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EVisualToggleType value)
    {
        uint value2;
        bool result = reader.ReadBits(2, out value2);
        if (value2 <= 2)
        {
            value = (EVisualToggleType)value2;
            return result;
        }
        value = EVisualToggleType.COSMETIC;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EVisualToggleType value)
    {
        return writer.WriteBits((uint)value, 2);
    }
}
