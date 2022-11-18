using SDG.NetPak;

namespace SDG.Unturned;

public static class EChatMode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EChatMode value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        if (value2 <= 4)
        {
            value = (EChatMode)value2;
            return result;
        }
        value = EChatMode.GLOBAL;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EChatMode value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
