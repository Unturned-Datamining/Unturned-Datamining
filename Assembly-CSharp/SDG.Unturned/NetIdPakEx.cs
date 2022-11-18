using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

public static class NetIdPakEx
{
    internal const int TRANSFORM_PATH_BYTE_COUNT_BITS = 7;

    public static bool ReadNetId(this NetPakReader reader, out NetId value)
    {
        return reader.ReadBits(32, out value.id);
    }

    public static bool WriteNetId(this NetPakWriter writer, NetId value)
    {
        return writer.WriteBits(value.id, 32);
    }

    public static bool ReadTransform(this NetPakReader reader, out Transform value)
    {
        bool flag = reader.ReadBit(out var value2);
        if (flag && value2)
        {
            flag &= reader.ReadNetId(out var value3);
            flag &= reader.ReadString(out var value4, 7);
            value = NetIdRegistry.GetTransform(value3, value4);
        }
        else
        {
            value = null;
        }
        return flag;
    }

    public static bool WriteTransform(this NetPakWriter writer, Transform value)
    {
        if (NetIdRegistry.GetTransformNetId(value, out var netId, out var path))
        {
            if (writer.WriteBit(value: true) && writer.WriteNetId(netId))
            {
                return writer.WriteString(path, 7);
            }
            return false;
        }
        return writer.WriteBit(value: false);
    }
}
