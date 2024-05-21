using SDG.NetPak;

namespace SDG.Unturned;

public static class ERagdollEffect_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out ERagdollEffect value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        if (value2 <= 9)
        {
            value = (ERagdollEffect)value2;
            return result;
        }
        value = ERagdollEffect.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, ERagdollEffect value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
