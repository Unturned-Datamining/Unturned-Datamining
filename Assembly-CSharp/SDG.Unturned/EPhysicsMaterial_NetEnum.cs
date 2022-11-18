using SDG.NetPak;

namespace SDG.Unturned;

public static class EPhysicsMaterial_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPhysicsMaterial value)
    {
        uint value2;
        bool result = reader.ReadBits(5, out value2);
        if (value2 <= 21)
        {
            value = (EPhysicsMaterial)value2;
            return result;
        }
        value = EPhysicsMaterial.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPhysicsMaterial value)
    {
        return writer.WriteBits((uint)value, 5);
    }
}
