using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to spawn items.
/// </summary>
[AddComponentMenu("Unturned/Item Spawner")]
public class ItemSpawner : MonoBehaviour
{
    public enum ESpawnState
    {
        World,
        Admin
    }

    [Tooltip("ID or GUID of item asset (or spawn table) to spawn when SpawnDefault is invoked.")]
    public string DefaultAsset;

    [Tooltip("Controls fullness and quality of spawned items.")]
    public ESpawnState ItemState;

    public void SpawnDefault()
    {
        Spawn(DefaultAsset);
    }

    public void Spawn(string assetId)
    {
        if (!Provider.isServer || EffectManager.isInstantiatingEffectForPreload)
        {
            return;
        }
        if (!CachingBcAssetRef.TryParse(assetId, EAssetType.ITEM, out var result))
        {
            UnturnedLog.warn("{0} unable to parse asset ID \"{1}\"", base.transform.GetSceneHierarchyPath(), assetId);
            return;
        }
        Asset asset = result.Get();
        if (asset == null)
        {
            UnturnedLog.warn("{0} unable to find asset \"{1}\"", base.transform.GetSceneHierarchyPath(), assetId);
            return;
        }
        if (asset is SpawnAsset spawnAsset)
        {
            asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.ITEM, OnGetSpawnErrorContext);
            if (asset == null)
            {
                return;
            }
        }
        if (!(asset is ItemAsset asset2))
        {
            UnturnedLog.warn(base.transform.GetSceneHierarchyPath() + " tried to spawn item but asset (" + asset.FriendlyName + ") is " + asset.GetTypeFriendlyName());
        }
        else
        {
            EItemOrigin origin = ((ItemState != 0) ? EItemOrigin.ADMIN : EItemOrigin.WORLD);
            ItemManager.dropItem(new Item(asset2, origin), base.transform.position, playEffect: false, isDropped: true, wideSpread: false);
        }
    }

    private string OnGetSpawnErrorContext()
    {
        return "item spawner " + base.transform.GetSceneHierarchyPath();
    }
}
