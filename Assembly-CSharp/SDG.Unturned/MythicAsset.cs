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
        if (id < 500 && !bundle.isCoreAsset && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 500");
        }
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        particleTagName = localization.format("Particle_Tag_Name");
        if (string.IsNullOrEmpty(particleTagName))
        {
            particleTagName = name;
        }
        _systemArea = bundle.load<GameObject>("System_Area");
        _systemHook = bundle.load<GameObject>("System_Hook");
        _systemFirst = bundle.load<GameObject>("System_First");
        _systemThird = bundle.load<GameObject>("System_Third");
        if ((bool)Assets.shouldValidateAssets)
        {
            if (systemArea != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemArea, 10);
            }
            if (systemHook != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemHook, 10);
            }
            if (systemFirst != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemFirst, 11);
            }
            if (systemThird != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemThird, 13);
            }
        }
    }
}
