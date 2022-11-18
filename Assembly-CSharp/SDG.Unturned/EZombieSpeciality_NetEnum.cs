using SDG.NetPak;

namespace SDG.Unturned;

public static class EZombieSpeciality_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EZombieSpeciality value)
    {
        uint value2;
        bool result = reader.ReadBits(5, out value2);
        if (value2 <= 20)
        {
            value = (EZombieSpeciality)value2;
            return result;
        }
        value = EZombieSpeciality.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EZombieSpeciality value)
    {
        return writer.WriteBits((uint)value, 5);
    }
}
