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

    [Tooltip("If true and a vehicle exists in parent hierarchy, the spawned barricade will be attached to the vehicle.")]
    public bool ShouldAttachToVehicle;

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
        Barricade barricade = new Barricade(newAsset);
        if (ShouldAttachToVehicle)
        {
            VehicleBarricadeRegion vehicleBarricadeRegion = BarricadeManager.FindVehicleRegionByTransform(base.transform.root);
            if (vehicleBarricadeRegion != null)
            {
                position = vehicleBarricadeRegion.parent.InverseTransformPoint(position);
                rotation = vehicleBarricadeRegion.parent.InverseTransformRotation(rotation);
                BarricadeManager.dropPlantedBarricade(vehicleBarricadeRegion.parent, barricade, position, rotation, ownerUser, ownerGroup);
                return;
            }
        }
        BarricadeManager.dropNonPlantedBarricade(barricade, position, rotation, ownerUser, ownerGroup);
    }

    private string OnGetSpawnErrorContext()
    {
        return "barricade spawner " + base.transform.GetSceneHierarchyPath();
    }
}
