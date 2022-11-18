using System;
using UnityEngine;

namespace SDG.Unturned;

public class MythicAsset : Asset
{
    protected GameObject _systemArea;

    protected GameObject _systemHook;

    protected GameObject _systemFirst;

    protected GameObject _systemThird;

    public string particleTagName { get; protected set; }

    public GameObject systemArea => _systemArea;

    public GameObject systemHook => _systemHook;

    public GameObject systemFirst => _systemFirst;

    public GameObject systemThird => _systemThird;

    public override EAssetType assetCategory => EAssetType.MYTHIC;

    public MythicAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 500 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 500");
        }
    }
}
