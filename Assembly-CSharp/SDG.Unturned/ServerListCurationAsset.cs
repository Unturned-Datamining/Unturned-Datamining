using UnityEngine;

namespace SDG.Unturned;

public class ServerListCurationAsset : Asset
{
    internal ServerListCurationFile curationFile;

    /// <summary>
    /// Optional image bundled alongside the asset file.
    /// </summary>
    public Texture2D Icon { get; protected set; }

    public override string FriendlyName => curationFile?.Name ?? name;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        Icon = LoadRedirectableAsset<Texture2D>(p.bundle, "Icon", p.data, "IconAssetPath");
        curationFile = new ServerListCurationFile();
        curationFile.Populate(this, p.data, p.localization);
        if (string.IsNullOrEmpty(curationFile.Name))
        {
            curationFile.Name = name;
        }
    }
}
