using SDG.NetPak;

namespace SDG.Unturned;

public static class EPlayerSkillset_NetEnum
{
    public static bool ReadEnum(this NetPakReader reader, out EPlayerSkillset value)
    {
        uint value2;
        bool result = reader.ReadBits(4, out value2);
        if (value2 <= 10)
        {
            value = (EPlayerSkillset)value2;
            return result;
        }
        value = EPlayerSkillset.NONE;
        return false;
    }

    public static bool WriteEnum(this NetPakWriter writer, EPlayerSkillset value)
    {
        return writer.WriteBits((uint)value, 4);
    }
}
