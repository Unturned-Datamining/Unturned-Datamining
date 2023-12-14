using SDG.NetPak;

namespace SDG.Unturned;

public static class EGameMode_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EGameMode value)
    {
        uint value2;
        bool result = reader.ReadBits(3, out value2);
        if (value2 <= 4)
        {
            value = (EGameMode)value2;
            return result;
        }
        value = EGameMode.EASY;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EGameMode value)
    {
        return writer.WriteBits((uint)value, 3);
    }
}
