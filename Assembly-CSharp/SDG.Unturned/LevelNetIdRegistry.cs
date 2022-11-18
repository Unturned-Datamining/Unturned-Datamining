namespace SDG.Unturned;

internal static class LevelNetIdRegistry
{
    private const uint TREE_FLAG = 2147483648u;

    private const uint REGULAR_OBJECT_FLAG = 1073741824u;

    private const uint DEVKIT_OBJECT_FLAG = 3221225472u;

    public static NetId GetTreeNetId(byte regionX, byte regionY, ushort index)
    {
        return new NetId(0x80000000u | (uint)(regionX << 22) | (uint)(regionY << 16) | index);
    }

    public static NetId GetRegularObjectNetId(byte regionX, byte regionY, ushort index)
    {
        return new NetId(0x40000000u | (uint)(regionX << 22) | (uint)(regionY << 16) | index);
    }

    public static NetId GetDevkitObjectNetId(uint instanceId)
    {
        return new NetId(0xC0000000u | instanceId);
    }
}
