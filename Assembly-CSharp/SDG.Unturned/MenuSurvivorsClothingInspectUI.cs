using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsClothingInspectUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekConstraintFrame inventory;

    private static ISleekImage image;

    private static ISleekSlider slider;

    private static int item;

    private static ulong instance;

    private static Transform inspect;

    private static Transform model;

    private static ItemLook look;

    private static Camera camera;

    public static void open()
    {
        if (!active)
        {
            active = true;
            camera.gameObject.SetActive(value: true);
            look._yaw = 0f;
            look.yaw = 0f;
            slider.state = 0f;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            camera.gameObject.SetActive(value: false);
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static bool getInspectedItemStatTrackerValue(out EStatTrackerType type, out int kills)
    {
        return Provider.provider.economyService.getInventoryStatTrackerValue(instance, out type, out kills);
    }

    public static void viewItem(int newItem, ulong newInstance)
    {
        item = newItem;
        instance = newInstance;
        if (model != null)
        {
            Object.Destroy(model.gameObject);
        }
        Provider.provider.economyService.getInventoryTargetID(item, out var item_guid, out var vehicle_guid);
        ushort inventorySkinID = Provider.provider.economyService.getInventorySkinID(item);
        ushort num = Provider.provider.economyService.getInventoryMythicID(item);
        if (num == 0)
        {
            num = Provider.provider.economyService.getInventoryParticleEffect(instance);
        }
        ItemAsset itemAsset = Assets.find<ItemAsset>(item_guid);
        VehicleAsset vehicleAsset = Assets.find<VehicleAsset>(vehicle_guid);
        if (itemAsset == null && vehicleAsset == null)
        {
            return;
        }
        if (inventorySkinID != 0)
        {
            SkinAsset skinAsset = Assets.find(EAssetType.SKIN, inventorySkinID) as SkinAsset;
            if (vehicleAsset != null)
            {
                model = VehicleTool.getVehicle(vehicleAsset.id, skinAsset.id, num, vehicleAsset, skinAsset);
            }
            else
            {
                model = ItemTool.getItem(itemAsset.id, inventorySkinID, 100, itemAsset.getState(), viewmodel: false, itemAsset, skinAsset, getInspectedItemStatTrackerValue);
                if (num != 0)
                {
                    ItemTool.applyEffect(model, num, EEffectType.THIRD);
                }
            }
        }
        else
        {
            model = ItemTool.getItem(itemAsset.id, 0, 100, itemAsset.getState(), viewmodel: false, itemAsset, getInspectedItemStatTrackerValue);
            if (num != 0)
            {
                ItemTool.applyEffect(model, num, EEffectType.HOOK);
            }
        }
        model.parent = inspect;
        model.localPosition = Vector3.zero;
        if (vehicleAsset != null)
        {
            model.localRotation = Quaternion.identity;
        }
        else if (itemAsset != null && itemAsset.type == EItemType.MELEE)
        {
            model.localRotation = Quaternion.Euler(0f, -90f, 90f);
        }
        else
        {
            model.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }
        look.target = model.gameObject;
    }

    private static void onDraggedSlider(ISleekSlider slider, float state)
    {
        look.yaw = state * 360f;
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuSurvivorsClothingItemUI.open();
        close();
    }

    public MenuSurvivorsClothingInspectUI()
    {
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        inventory = Glazier.Get().CreateConstraintFrame();
        inventory.positionScale_X = 0.5f;
        inventory.positionOffset_Y = 10;
        inventory.positionScale_Y = 0.125f;
        inventory.sizeScale_X = 0.5f;
        inventory.sizeScale_Y = 0.75f;
        inventory.sizeOffset_Y = -20;
        inventory.constraint = ESleekConstraint.FitInParent;
        container.AddChild(inventory);
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.sizeScale_X = 1f;
        sleekBox.sizeScale_Y = 1f;
        inventory.AddChild(sleekBox);
        image = Glazier.Get().CreateImage(Resources.Load<Texture>("RenderTextures/Item"));
        image.sizeScale_X = 1f;
        image.sizeScale_Y = 1f;
        sleekBox.AddChild(image);
        slider = Glazier.Get().CreateSlider();
        slider.positionOffset_Y = 10;
        slider.positionScale_Y = 1f;
        slider.sizeOffset_Y = 20;
        slider.sizeScale_X = 1f;
        slider.orientation = ESleekOrientation.HORIZONTAL;
        slider.onDragged += onDraggedSlider;
        sleekBox.AddChild(slider);
        inspect = GameObject.Find("Inspect").transform;
        look = inspect.GetComponent<ItemLook>();
        camera = look.inspectCamera;
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
