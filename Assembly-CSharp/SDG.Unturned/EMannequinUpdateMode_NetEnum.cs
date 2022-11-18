using SDG.NetPak;

namespace SDG.Unturned;

public static class EMannequinUpdateMode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EMannequinUpdateMode value)
    {
        uint value2;
        bool result = reader.ReadBits(2, out value2);
        value = (EMannequinUpdateMode)value2;
        return result;
    }

    public static bool WriteEnum(this NetPakWriter writer, EMannequinUpdateMode value)
    {
        return writer.WriteBits((uint)value, 2);
    }
}
