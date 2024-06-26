using System;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Represents a vehicle the vendor is selling to players.
/// </summary>
public class VendorSellingVehicle : VendorSellingBase
{
    public override string displayName => FindVehicleAssetAndHandleRedirects()?.vehicleName;

    public override EItemRarity rarity => FindVehicleAssetAndHandleRedirects()?.rarity ?? EItemRarity.COMMON;

    public override bool hasIcon => false;

    public string spawnpoint { get; protected set; }

    /// <summary>
    /// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
    /// vehicle spawner requires to properly set paint color.
    /// </summary>
    public Asset FindAsset()
    {
        return Assets.FindBaseVehicleAssetByGuidOrLegacyId(base.TargetAssetGuid, base.id);
    }

    public VehicleAsset FindVehicleAssetAndHandleRedirects()
    {
        Asset asset = FindAsset();
        if (asset is VehicleRedirectorAsset { TargetVehicle: var targetVehicle })
        {
            asset = targetVehicle.Find();
        }
        return asset as VehicleAsset;
    }

    public override void buy(Player player)
    {
        base.buy(player);
        Asset asset = FindAsset();
        if (asset != null)
        {
            Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(this.spawnpoint);
            Vector3 point;
            Quaternion rotation;
            if (spawnpoint != null)
            {
                point = spawnpoint.transform.position;
                rotation = spawnpoint.transform.rotation;
            }
            else
            {
                UnturnedLog.error("Failed to find vendor selling spawnpoint: " + this.spawnpoint);
                point = VehicleTool.GetPositionForVehicle(player);
                rotation = player.transform.rotation;
            }
            VehicleManager.spawnLockedVehicleForPlayerV2(asset, point, rotation, player);
        }
    }

    public VendorSellingVehicle(VendorAsset newOuterAsset, byte newIndex, Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, string newSpawnpoint, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
        : base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditions, newRewardsList)
    {
        spawnpoint = newSpawnpoint;
    }
}
