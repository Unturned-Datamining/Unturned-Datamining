using SDG.NetPak;

namespace SDG.Unturned;

public static class ECameraMode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ECameraMode value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        if (value2 <= 4)
        {
            value = (ECameraMode)value2;
            return result;
        }
        value = ECameraMode.FIRST;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ECameraMode value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
