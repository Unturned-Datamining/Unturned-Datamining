using System;

namespace SDG.Unturned;

public class ItemIconInfo
{
    [Obsolete("Removed in favor of itemAsset")]
    public ushort id;

    [Obsolete("Removed in favor of skinAsset")]
    public ushort skin;

    public byte quality;

    public byte[] state;

    public ItemAsset itemAsset;

    public SkinAsset skinAsset;

    public string tags;

    public string dynamic_props;

    public int x;

    public int y;

    public bool scale;

    public bool readableOnCPU;

    internal bool isEligibleForCaching;

    public ItemIconReady callback;
}
