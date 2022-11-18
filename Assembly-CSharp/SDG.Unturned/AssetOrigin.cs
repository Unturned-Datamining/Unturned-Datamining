using System.Collections.Generic;

namespace SDG.Unturned;

public class AssetOrigin
{
    public string name;

    public ulong workshopFileId;

    internal List<Asset> assets = new List<Asset>();

    public IReadOnlyList<Asset> GetAssets()
    {
        return assets;
    }
}
