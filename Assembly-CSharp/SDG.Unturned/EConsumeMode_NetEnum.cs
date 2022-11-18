using SDG.NetPak;

namespace SDG.Unturned;

public static class EConsumeMode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EConsumeMode value)
    {
        uint value2;
        bool result = reader.ReadBits(1, out value2);
        value = (EConsumeMode)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, EConsumeMode value)
    {
        return writer.WriteBits((uint)value, 1);
    }
}
