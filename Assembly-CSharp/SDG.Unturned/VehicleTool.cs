using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class VehicleTool : MonoBehaviour
{
    private static Queue<VehicleIconInfo> icons;

    public static Transform getVehicle(ushort id, ushort skin, ushort mythic, VehicleAsset vehicleAsset, SkinAsset skinAsset)
    {
        GameObject gameObject = vehicleAsset?.GetOrLoadModel();
        if (gameObject != null)
        {
            if (id != vehicleAsset.id)
            {
                UnturnedLog.error("ID and asset ID are not in sync!");
            }
            Transform transform = Object.Instantiate(gameObject).transform;
            transform.name = id.ToString();
            if (skinAsset != null)
            {
                InteractableVehicle interactableVehicle = transform.gameObject.AddComponent<InteractableVehicle>();
                interactableVehicle.id = id;
                interactableVehicle.skinID = skin;
                interactableVehicle.mythicID = mythic;
                interactableVehicle.fuel = 10000;
                interactableVehicle.isExploded = false;
                interactableVehicle.health = 10000;
                interactableVehicle.batteryCharge = 10000;
                interactableVehicle.safeInit(vehicleAsset);
                interactableVehicle.updateFires();
                interactableVehicle.updateSkin();
            }
            return transform;
        }
        Transform obj = new GameObject().transform;
        obj.name = id.ToString();
        obj.tag = "Vehicle";
        obj.gameObject.layer = 26;
        return obj;
    }

    public static void getIcon(ushort id, ushort skin, VehicleAsset vehicleAsset, SkinAsset skinAsset, int x, int y, bool readableOnCPU, VehicleIconReady callback)
    {
        if (vehicleAsset != null && id != vehicleAsset.id)
        {
            UnturnedLog.error("ID and vehicle asset ID are not in sync!");
        }
        if (skinAsset != null && skin != skinAsset.id)
        {
            UnturnedLog.error("ID and skin asset ID are not in sync!");
        }
        VehicleIconInfo vehicleIconInfo = new VehicleIconInfo();
        vehicleIconInfo.id = id;
        vehicleIconInfo.skin = skin;
        vehicleIconInfo.vehicleAsset = vehicleAsset;
        vehicleIconInfo.skinAsset = skinAsset;
        vehicleIconInfo.x = x;
        vehicleIconInfo.y = y;
        vehicleIconInfo.readableOnCPU = readableOnCPU;
        vehicleIconInfo.callback = callback;
        icons.Enqueue(vehicleIconInfo);
    }

    internal static Vector3 GetPositionForVehicle(Player player)
    {
        Vector3 vector = player.transform.position + player.transform.forward * 6f;
        Physics.Raycast(vector + Vector3.up * 16f, Vector3.down, out var hitInfo, 32f, RayMasks.BLOCK_VEHICLE);
        if (hitInfo.collider != null)
        {
            vector.y = hitInfo.point.y + 16f;
        }
        return vector;
    }

    public static bool giveVehicle(Player player, ushort id)
    {
        if (Assets.find(EAssetType.VEHICLE, id) is VehicleAsset)
        {
            Vector3 positionForVehicle = GetPositionForVehicle(player);
            VehicleManager.spawnVehicleV2(id, positionForVehicle, player.transform.rotation);
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (icons == null || icons.Count == 0)
        {
            return;
        }
        VehicleIconInfo vehicleIconInfo = icons.Dequeue();
        if (vehicleIconInfo == null || vehicleIconInfo.vehicleAsset == null)
        {
            return;
        }
        Transform vehicle = getVehicle(vehicleIconInfo.id, vehicleIconInfo.skin, 0, vehicleIconInfo.vehicleAsset, vehicleIconInfo.skinAsset);
        vehicle.position = new Vector3(-256f, -256f, 0f);
        Transform transform = vehicle.Find("Icon2");
        if (transform == null)
        {
            Object.Destroy(vehicle.gameObject);
            Assets.reportError(vehicleIconInfo.vehicleAsset, "missing 'Icon2' Transform");
            return;
        }
        float size2_z = vehicleIconInfo.vehicleAsset.size2_z;
        Texture2D texture = ItemTool.captureIcon(vehicleIconInfo.id, vehicleIconInfo.skin, vehicle, transform, vehicleIconInfo.x, vehicleIconInfo.y, size2_z, vehicleIconInfo.readableOnCPU);
        if (vehicleIconInfo.callback != null)
        {
            vehicleIconInfo.callback(texture);
        }
    }

    private void Start()
    {
        icons = new Queue<VehicleIconInfo>();
    }
}
