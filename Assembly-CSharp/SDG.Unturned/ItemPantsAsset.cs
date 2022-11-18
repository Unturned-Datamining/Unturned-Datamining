using UnityEngine;

namespace SDG.Unturned;

public class ItemPantsAsset : ItemBagAsset
{
    protected Texture2D _pants;

    protected Texture2D _emission;

    protected Texture2D _metallic;

    public Texture2D pants => _pants;

    public Texture2D emission => _emission;

    public Texture2D metallic => _metallic;

    public ItemPantsAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
    }
}
