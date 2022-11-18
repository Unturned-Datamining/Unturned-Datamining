using UnityEngine;

namespace SDG.Unturned;

public interface ISkinableAsset
{
    Texture albedoBase { get; }

    Texture metallicBase { get; }

    Texture emissionBase { get; }
}
