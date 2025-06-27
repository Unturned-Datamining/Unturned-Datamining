using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to spawn barricades.
/// </summary>
[AddComponentMenu("Unturned/Barricade Spawner")]
public class BarricadeSpawner : MonoBehaviour
{
    [Tooltip("ID or GUID of barricade asset (or spawn table) to spawn when SpawnDefault is invoked.")]
    public string DefaultAsset;

    [Tooltip("Finds ownership of BarricadeSpawner (e.g., parent barricade) and assigns to spawned barricade.")]
    public bool InheritOwnership;

    public void SpawnDefault()
    {
        Spawn(DefaultAsset);
    }

    public void Spawn(string assetId)
    {
        if (!Provider.isServer)
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
        if (!(asset is ItemBarricadeAsset newAsset))
        {
            UnturnedLog.warn(base.transform.GetSceneHierarchyPath() + " tried to spawn barricade but asset (" + asset.FriendlyName + ") is " + asset.GetTypeFriendlyName());
            return;
        }
        ulong ownerUser = 0uL;
        ulong ownerGroup = 0uL;
        if (InheritOwnership)
        {
            DamageTool.TryFindOwnership(base.transform, out ownerUser, out ownerGroup);
        }
        base.transform.GetPositionAndRotation(out var position, out var rotation);
        BarricadeManager.dropNonPlantedBarricade(new Barricade(newAsset), position, rotation, ownerUser, ownerGroup);
    }

    private string OnGetSpawnErrorContext()
    {
        return "barricade spawner " + base.transform.GetSceneHierarchyPath();
    }
}
