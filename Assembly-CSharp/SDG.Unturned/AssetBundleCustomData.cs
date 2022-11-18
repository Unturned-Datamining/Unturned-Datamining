using UnityEngine;

namespace SDG.Unturned;

[CreateAssetMenu(fileName = "AssetBundleCustomData", menuName = "Unturned/Asset Bundle Custom Data")]
public class AssetBundleCustomData : ScriptableObject
{
    public ulong ownerWorkshopFileId;
}
