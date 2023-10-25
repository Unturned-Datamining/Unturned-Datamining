using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Represents a vehicle the vendor is selling to players.
/// </summary>
public class VendorSellingVehicle : VendorSellingBase
{
    public override string displayName
    {
        get
        {
            if (!(Assets.find(EAssetType.VEHICLE, base.id) is VehicleAsset vehicleAsset))
            {
                return null;
            }
            return vehicleAsset.vehicleName;
        }
    }

    public override EItemRarity rarity
    {
        get
        {
            if (!(Assets.find(EAssetType.VEHICLE, base.id) is VehicleAsset vehicleAsset))
            {
                return EItemRarity.COMMON;
            }
            return vehicleAsset.rarity;
        }
    }

    public override bool hasIcon => false;

    public string spawnpoint { get; protected set; }

    public override void buy(Player player)
    {
        base.buy(player);
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
        VehicleManager.spawnLockedVehicleForPlayerV2(base.id, point, rotation, player);
    }

    public VendorSellingVehicle(VendorAsset newOuterAsset, byte newIndex, ushort newID, uint newCost, string newSpawnpoint, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
        : base(newOuterAsset, newIndex, newID, newCost, newConditions, newRewardsList)
    {
        spawnpoint = newSpawnpoint;
    }
}
