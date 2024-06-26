using SDG.NetPak;

namespace SDG.Unturned;

public static class ESteamRejection_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ESteamRejection value)
    {
        uint value2;
        bool result = reader.ReadBits(6, out value2);
        if (value2 <= 48)
        {
            value = (ESteamRejection)value2;
            return result;
        }
        value = ESteamRejection.SERVER_FULL;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ESteamRejection value)
    {
        return writer.WriteBits((uint)value, 6);
    }
}
