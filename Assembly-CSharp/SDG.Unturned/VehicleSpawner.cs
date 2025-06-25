using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to spawn vehicles.
/// </summary>
[AddComponentMenu("Unturned/Vehicle Spawner")]
public class VehicleSpawner : MonoBehaviour
{
    [Tooltip("ID or GUID of vehicle asset (or spawn table) to spawn when SpawnDefault is invoked.")]
    public string DefaultAsset;

    [Tooltip("If true, apply PaintColorOverride.")]
    public bool UsePaintColorOverride;

    [Tooltip("If UsePaintColorOverride is true, this paint color is used instead of the vehicle's default.")]
    public Color32 PaintColorOverride;

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
        if (!CachingBcAssetRef.TryParse(assetId, EAssetType.VEHICLE, out var result))
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
            asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.VEHICLE, OnGetSpawnErrorContext);
            if (asset == null)
            {
                return;
            }
        }
        if (!(asset is VehicleAsset) && !(asset is VehicleRedirectorAsset))
        {
            UnturnedLog.warn(base.transform.GetSceneHierarchyPath() + " tried to spawn vehicle but asset (" + asset.FriendlyName + ") is " + asset.GetTypeFriendlyName());
        }
        else
        {
            Color32? paintColor = (UsePaintColorOverride ? new Color32?(PaintColorOverride) : null);
            base.transform.GetPositionAndRotation(out var position, out var rotation);
            VehicleManager.spawnVehicleV2(asset, position, rotation, paintColor);
        }
    }

    private string OnGetSpawnErrorContext()
    {
        return "vehicle spawner " + base.transform.GetSceneHierarchyPath();
    }
}
