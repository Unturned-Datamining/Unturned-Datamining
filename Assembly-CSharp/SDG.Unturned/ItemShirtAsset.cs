using UnityEngine;

namespace SDG.Unturned;

public class ItemShirtAsset : ItemBagAsset
{
    protected Texture2D _shirt;

    protected Texture2D _emission;

    protected Texture2D _metallic;

    protected bool _ignoreHand;

    public Material characterMaterialOverride;

    public Texture2D shirt => _shirt;

    public Texture2D emission => _emission;

    public Texture2D metallic => _metallic;

    public bool ignoreHand => _ignoreHand;

    public Mesh[] characterMeshOverride1pLODs { get; protected set; }

    public Mesh[] characterMeshOverride3pLODs { get; protected set; }

    public ItemShirtAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        characterMeshOverride1pLODs = null;
        characterMeshOverride3pLODs = null;
        characterMaterialOverride = null;
        _ignoreHand = data.has("Ignore_Hand");
    }
}
