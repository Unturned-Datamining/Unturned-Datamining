using SDG.NetPak;

namespace SDG.Unturned;

public static class ESwingMode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ESwingMode value)
    {
        uint value2;
        bool result = reader.ReadBits(1, out value2);
        value = (ESwingMode)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, ESwingMode value)
    {
        return writer.WriteBits((uint)value, 1);
    }
}
