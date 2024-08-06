using SDG.NetPak;

namespace SDG.Unturned;

public static class PhysicsMaterialNetIdPakEx
{
    public static bool ReadPhysicsMaterialNetId(this NetPakReader reader, out PhysicsMaterialNetId value)
    {
        return reader.ReadBits(PhysicsMaterialNetTable.idBitCount, out value.id);
    }

    public static bool WritePhysicsMaterialNetId(this NetPakWriter writer, PhysicsMaterialNetId value)
    {
        return writer.WriteBits(value.id, PhysicsMaterialNetTable.idBitCount);
    }

    public static bool ReadPhysicsMaterialName(this NetPakReader reader, out string materialName)
    {
        PhysicsMaterialNetId value;
        bool result = reader.ReadPhysicsMaterialNetId(out value);
        materialName = PhysicsMaterialNetTable.GetMaterialName(value);
        return result;
    }

    public static bool WritePhysicsMaterialName(this NetPakWriter writer, string materialName)
    {
        PhysicsMaterialNetId netId = PhysicsMaterialNetTable.GetNetId(materialName);
        return writer.WritePhysicsMaterialNetId(netId);
    }
}
