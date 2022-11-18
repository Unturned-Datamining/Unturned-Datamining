namespace SDG.Unturned;

public struct SkinIconInfo
{
    public ushort id;

    public ESkinIconSize size;

    public SkinIconInfo(ushort newID, ESkinIconSize newSize)
    {
        id = newID;
        size = newSize;
    }
}
