using SDG.NetPak;

namespace SDG.Unturned;

public static class EClientPlatform_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EClientPlatform value)
    {
        uint value2;
        bool result = reader.ReadBits(2, out value2);
        if (value2 <= 2)
        {
            value = (EClientPlatform)value2;
            return result;
        }
        value = EClientPlatform.Windows;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EClientPlatform value)
    {
        return writer.WriteBits((uint)value, 2);
    }
}
