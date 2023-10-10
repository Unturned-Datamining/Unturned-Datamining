using UnityEngine;

namespace SDG.Unturned;

public class SleekEconIcon : SleekWrapper
{
    private ISleekImage internalImage;

    private bool isExpectingIconReadyCallback;

    public SleekColor color
    {
        get
        {
            return internalImage.TintColor;
        }
        set
        {
            internalImage.TintColor = value;
        }
    }

    public void SetItemDefId(int itemdefid)
    {
        if (itemdefid < 1)
        {
            internalImage.IsVisible = false;
            isExpectingIconReadyCallback = false;
            return;
        }
        ushort inventorySkinID = Provider.provider.economyService.getInventorySkinID(itemdefid);
        if (inventorySkinID > 0 && Assets.find(EAssetType.SKIN, inventorySkinID) is SkinAsset skinAsset)
        {
            Provider.provider.economyService.getInventoryTargetID(itemdefid, out var item_guid, out var vehicle_guid);
            ItemAsset itemAsset = Assets.find<ItemAsset>(item_guid);
            VehicleAsset vehicleAsset = Assets.find<VehicleAsset>(vehicle_guid);
            if (vehicleAsset != null)
            {
                VehicleTool.getIcon(vehicleAsset.id, skinAsset.id, vehicleAsset, skinAsset, 400, 400, readableOnCPU: false, OnIconReady);
                isExpectingIconReadyCallback = true;
                return;
            }
            if (itemAsset != null)
            {
                ItemTool.getIcon(itemAsset.id, skinAsset.id, 100, itemAsset.getState(), itemAsset, skinAsset, string.Empty, string.Empty, 400, 400, scale: true, readableOnCPU: false, OnIconReady);
                isExpectingIconReadyCallback = true;
                return;
            }
        }
        Texture2D texture2D = Provider.provider.economyService.LoadItemIcon(itemdefid);
        internalImage.Texture = texture2D;
        internalImage.IsVisible = texture2D != null;
        isExpectingIconReadyCallback = false;
    }

    public void SetIsBoxMythicalIcon()
    {
        internalImage.Texture = Resources.Load<Texture2D>("Economy/Mystery/Icon_Large");
        internalImage.IsVisible = true;
        isExpectingIconReadyCallback = false;
    }

    public override void OnDestroy()
    {
        internalImage = null;
    }

    public SleekEconIcon()
    {
        internalImage = Glazier.Get().CreateImage();
        internalImage.SizeScale_X = 1f;
        internalImage.SizeScale_Y = 1f;
        AddChild(internalImage);
    }

    private void OnIconReady(Texture2D texture)
    {
        if (internalImage != null && isExpectingIconReadyCallback)
        {
            internalImage.Texture = texture;
        }
    }
}
