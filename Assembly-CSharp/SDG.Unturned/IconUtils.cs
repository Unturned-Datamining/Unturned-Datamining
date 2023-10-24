using System;
using System.Collections.Generic;
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned;

public class IconUtils
{
    public static List<ItemDefIconInfo> icons = new List<ItemDefIconInfo>();

    public static List<ExtraItemIconInfo> extraIcons = new List<ExtraItemIconInfo>();

    private static GameObject cosmeticPreviewGameObject;

    private static CosmeticPreviewCapture cosmeticPreviewCapture;

    public static void CreateExtrasDirectory()
    {
        ReadWrite.createFolder("/Extras/Econ");
        ReadWrite.createFolder("/Extras/Icons");
        ReadWrite.createFolder("/Extras/CosmeticPreviews_2048x2048");
        ReadWrite.createFolder("/Extras/CosmeticPreviews_400x400");
        ReadWrite.createFolder("/Extras/CosmeticPreviews_200x200");
        ReadWrite.createFolder("/Extras/OutfitPreviews_2048x2048");
        ReadWrite.createFolder("/Extras/OutfitPreviews_400x400");
        ReadWrite.createFolder("/Extras/OutfitPreviews_200x200");
    }

    public static ItemDefIconInfo getItemDefIcon(ushort itemID, ushort vehicleID, ushort skinID)
    {
        ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemID) as ItemAsset;
        VehicleAsset vehicleAsset = Assets.find(EAssetType.VEHICLE, vehicleID) as VehicleAsset;
        if (itemAsset == null && vehicleAsset == null)
        {
            UnturnedLog.warn("Could not find a matching item ({0}) or vehicle ({1}) asset!", itemID, vehicleID);
            return null;
        }
        return getItemDefIcon(itemAsset, vehicleAsset, skinID);
    }

    public static ItemDefIconInfo getItemDefIcon(ItemAsset itemAsset, VehicleAsset vehicleAsset, ushort skinID)
    {
        ItemDefIconInfo itemDefIconInfo = new ItemDefIconInfo();
        if (skinID != 0)
        {
            if (!(Assets.find(EAssetType.SKIN, skinID) is SkinAsset skinAsset))
            {
                UnturnedLog.warn("Couldn't find a skin asset for: " + skinID);
                return null;
            }
            ushort num = vehicleAsset?.id ?? itemAsset.id;
            string text = ((vehicleAsset == null) ? itemAsset.name : vehicleAsset.sharedSkinName);
            itemDefIconInfo.extraPath = ReadWrite.PATH + "/Extras/Econ/" + text + "_" + num + "_" + skinAsset.name + "_" + skinAsset.id;
            if (vehicleAsset != null)
            {
                VehicleTool.getIcon(vehicleAsset.id, skinAsset.id, vehicleAsset, skinAsset, 400, 400, readableOnCPU: true, itemDefIconInfo.onLargeItemIconReady);
            }
            else
            {
                ItemTool.getIcon(itemAsset.id, skinAsset.id, 100, itemAsset.getState(), itemAsset, skinAsset, string.Empty, string.Empty, 400, 400, scale: true, readableOnCPU: true, itemDefIconInfo.onLargeItemIconReady);
            }
        }
        else
        {
            if (itemAsset != null && string.IsNullOrEmpty(itemAsset.proPath))
            {
                UnturnedLog.error("Failed to find pro path for: " + itemAsset.id + " " + vehicleAsset?.id + " " + skinID);
                return null;
            }
            itemDefIconInfo.extraPath = ReadWrite.PATH + "/Extras/Econ/" + itemAsset.name + "_" + itemAsset.id;
            ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty, string.Empty, 400, 400, scale: true, readableOnCPU: true, itemDefIconInfo.onLargeItemIconReady);
        }
        icons.Add(itemDefIconInfo);
        return itemDefIconInfo;
    }

    public static void captureItemIcon(ItemAsset itemAsset)
    {
        if (itemAsset != null)
        {
            ExtraItemIconInfo extraItemIconInfo = new ExtraItemIconInfo();
            extraItemIconInfo.extraPath = ReadWrite.PATH + "/Extras/Icons/" + itemAsset.name + "_" + itemAsset.id;
            ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty, string.Empty, itemAsset.size_x * 512, itemAsset.size_y * 512, scale: false, readableOnCPU: true, extraItemIconInfo.onItemIconReady);
            extraIcons.Add(extraItemIconInfo);
        }
    }

    public static void captureAllItemIcons()
    {
        List<ItemAsset> list = new List<ItemAsset>();
        Assets.find(list);
        foreach (ItemAsset item in list)
        {
            captureItemIcon(item);
        }
    }

    public static void CaptureAllSkinIcons()
    {
        foreach (UnturnedEconInfo item in TempSteamworksEconomy.econInfo)
        {
            if (item.item_skin != 0)
            {
                ItemAsset itemAsset = Assets.find(item.item_guid) as ItemAsset;
                VehicleAsset vehicleAsset = Assets.find(item.vehicle_guid) as VehicleAsset;
                getItemDefIcon(itemAsset, vehicleAsset, (ushort)item.item_skin);
            }
        }
    }

    public static void CaptureCosmeticPreviews()
    {
        InitCapturePreview();
        cosmeticPreviewCapture.CaptureCosmetics();
    }

    public static void CaptureAllOutfitPreviews()
    {
        InitCapturePreview();
        cosmeticPreviewCapture.CaptureAllOutfits();
    }

    public static void CaptureOutfitPreview(Guid guid)
    {
        InitCapturePreview();
        cosmeticPreviewCapture.CaptureOutfit(guid);
    }

    private static void InitCapturePreview()
    {
        if (cosmeticPreviewGameObject == null)
        {
            cosmeticPreviewGameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Characters/CosmeticPreviewCapture"), new Vector3(-1000f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f));
            cosmeticPreviewCapture = cosmeticPreviewGameObject.GetComponent<CosmeticPreviewCapture>();
        }
    }
}
