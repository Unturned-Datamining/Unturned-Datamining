using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsClothingItemUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static int item;

    private static ushort quantity;

    private static ulong instance;

    private static ISleekConstraintFrame inventory;

    private static SleekInventory packageBox;

    private static ISleekBox descriptionBox;

    private static ISleekLabel infoLabel;

    private static ISleekButton useButton;

    private static ISleekButton inspectButton;

    private static ISleekButton marketButton;

    private static ISleekButton deleteButton;

    private static ISleekButton scrapButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void viewItem()
    {
        viewItem(item, quantity, instance);
    }

    public static void viewItem(int newItem, ushort newQuantity, ulong newInstance)
    {
        UnturnedLog.info("View: " + newItem + " x" + newQuantity + " [" + newInstance + "]");
        item = newItem;
        quantity = newQuantity;
        instance = newInstance;
        packageBox.updateInventory(instance, item, newQuantity, isClickable: false, isLarge: true);
        if (packageBox.itemAsset == null && packageBox.vehicleAsset == null)
        {
            useButton.isVisible = false;
            inspectButton.isVisible = false;
            marketButton.isVisible = false;
            scrapButton.isVisible = false;
            deleteButton.isVisible = true;
            descriptionBox.sizeOffset_Y = -60;
            deleteButton.positionOffset_Y = -descriptionBox.sizeOffset_Y - 50;
            deleteButton.sizeScale_X = 0.5f;
            infoLabel.text = localization.format("Unknown");
            return;
        }
        if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.KEY)
        {
            if ((packageBox.itemAsset as ItemKeyAsset).exchangeWithTargetItem)
            {
                useButton.isVisible = true;
                useButton.text = localization.format("Target_Item_Text");
                useButton.tooltipText = localization.format("Target_Item_Tooltip");
            }
            else
            {
                useButton.isVisible = false;
            }
            inspectButton.isVisible = false;
        }
        else if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.BOX)
        {
            useButton.isVisible = true;
            inspectButton.isVisible = false;
            useButton.text = localization.format("Contents_Text");
            useButton.tooltipText = localization.format("Contents_Tooltip");
        }
        else
        {
            useButton.isVisible = true;
            inspectButton.isVisible = true;
            bool flag = ((packageBox.itemAsset != null && packageBox.itemAsset.proPath != null && packageBox.itemAsset.proPath.Length != 0) ? Characters.isCosmeticEquipped(instance) : Characters.isSkinEquipped(instance));
            useButton.text = localization.format(flag ? "Dequip_Text" : "Equip_Text");
            useButton.tooltipText = localization.format(flag ? "Dequip_Tooltip" : "Equip_Tooltip");
        }
        marketButton.isVisible = Provider.provider.economyService.getInventoryMarketable(item);
        int inventoryScraps = Provider.provider.economyService.getInventoryScraps(item);
        scrapButton.text = localization.format("Scrap_Text", inventoryScraps);
        scrapButton.tooltipText = localization.format("Scrap_Tooltip", inventoryScraps);
        scrapButton.isVisible = inventoryScraps > 0;
        descriptionBox.sizeOffset_Y = 0;
        if (useButton.isVisible || inspectButton.isVisible)
        {
            descriptionBox.sizeOffset_Y -= 60;
            useButton.positionOffset_Y = -descriptionBox.sizeOffset_Y - 50;
            inspectButton.positionOffset_Y = -descriptionBox.sizeOffset_Y - 50;
        }
        if (scrapButton.isVisible)
        {
            deleteButton.sizeScale_X = 0.25f;
        }
        else
        {
            deleteButton.sizeScale_X = 0.5f;
        }
        if (marketButton.isVisible || deleteButton.isVisible || scrapButton.isVisible)
        {
            descriptionBox.sizeOffset_Y -= 60;
            marketButton.positionOffset_Y = -descriptionBox.sizeOffset_Y - 50;
            deleteButton.positionOffset_Y = -descriptionBox.sizeOffset_Y - 50;
            scrapButton.positionOffset_Y = -descriptionBox.sizeOffset_Y - 50;
        }
        infoLabel.text = "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryType(item) + "</color>\n\n" + Provider.provider.economyService.getInventoryDescription(item);
    }

    private static void onClickedUseButton(ISleekElement button)
    {
        if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.KEY)
        {
            EEconFilterMode newFilterMode;
            switch (packageBox.itemAsset.id)
            {
            case 845:
            case 846:
                newFilterMode = EEconFilterMode.STAT_TRACKER;
                break;
            case 992:
                newFilterMode = EEconFilterMode.STAT_TRACKER_REMOVAL;
                break;
            case 993:
                newFilterMode = EEconFilterMode.RAGDOLL_EFFECT_REMOVAL;
                break;
            case 1524:
                newFilterMode = EEconFilterMode.RAGDOLL_EFFECT;
                break;
            default:
                UnturnedLog.warn("Unknown tool " + packageBox.itemAsset.name);
                newFilterMode = EEconFilterMode.STAT_TRACKER;
                break;
            }
            MenuSurvivorsClothingUI.setFilter(newFilterMode, instance);
            MenuSurvivorsClothingUI.open();
            close();
        }
        else if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.BOX)
        {
            MenuSurvivorsClothingBoxUI.viewItem(item, quantity, instance);
            MenuSurvivorsClothingBoxUI.open();
            close();
        }
        else
        {
            Characters.package(instance);
            viewItem();
        }
    }

    private static void onClickedInspectButton(ISleekElement button)
    {
        MenuSurvivorsClothingInspectUI.viewItem(item, instance);
        MenuSurvivorsClothingInspectUI.open();
        close();
    }

    private static void onClickedMarketButton(ISleekElement button)
    {
        if (!Provider.provider.economyService.canOpenInventory)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.economyService.open(instance);
        }
    }

    private static void onClickedDeleteButton(ISleekElement button)
    {
        MenuSurvivorsClothingDeleteUI.viewItem(item, instance, quantity, EDeleteMode.DELETE, 0uL);
        MenuSurvivorsClothingDeleteUI.open();
        close();
    }

    private static void onClickedScrapButton(ISleekElement button)
    {
        if (Provider.provider.economyService.getInventoryMythicID(item) != 0 || !InputEx.GetKey(ControlsSettings.other))
        {
            MenuSurvivorsClothingDeleteUI.viewItem(item, instance, quantity, EDeleteMode.SALVAGE, 0uL);
            MenuSurvivorsClothingDeleteUI.open();
            close();
        }
        else
        {
            MenuSurvivorsClothingDeleteUI.salvageItem(item, instance);
            onClickedBackButton(null);
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuSurvivorsClothingUI.open();
        close();
    }

    public MenuSurvivorsClothingItemUI()
    {
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothingItem.dat");
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
        inventory.sizeScale_X = 0.5f;
        inventory.sizeScale_Y = 1f;
        inventory.sizeOffset_Y = -20;
        inventory.constraint = ESleekConstraint.FitInParent;
        container.AddChild(inventory);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.sizeScale_X = 1f;
        sleekConstraintFrame.sizeScale_Y = 0.5f;
        sleekConstraintFrame.sizeOffset_Y = -5;
        sleekConstraintFrame.constraint = ESleekConstraint.FitInParent;
        inventory.AddChild(sleekConstraintFrame);
        packageBox = new SleekInventory();
        packageBox.sizeScale_X = 1f;
        packageBox.sizeScale_Y = 1f;
        sleekConstraintFrame.AddChild(packageBox);
        descriptionBox = Glazier.Get().CreateBox();
        descriptionBox.positionOffset_Y = 10;
        descriptionBox.positionScale_Y = 1f;
        descriptionBox.sizeScale_X = 1f;
        descriptionBox.sizeScale_Y = 1f;
        packageBox.AddChild(descriptionBox);
        infoLabel = Glazier.Get().CreateLabel();
        infoLabel.enableRichText = true;
        infoLabel.positionOffset_X = 5;
        infoLabel.positionOffset_Y = 5;
        infoLabel.sizeScale_X = 1f;
        infoLabel.sizeScale_Y = 1f;
        infoLabel.sizeOffset_X = -10;
        infoLabel.sizeOffset_Y = -10;
        infoLabel.fontAlignment = TextAnchor.UpperLeft;
        infoLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        infoLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        descriptionBox.AddChild(infoLabel);
        useButton = Glazier.Get().CreateButton();
        useButton.positionScale_Y = 1f;
        useButton.sizeOffset_X = -5;
        useButton.sizeOffset_Y = 50;
        useButton.sizeScale_X = 0.5f;
        useButton.onClickedButton += onClickedUseButton;
        descriptionBox.AddChild(useButton);
        useButton.fontSize = ESleekFontSize.Medium;
        useButton.isVisible = false;
        inspectButton = Glazier.Get().CreateButton();
        inspectButton.positionOffset_X = 5;
        inspectButton.positionScale_X = 0.5f;
        inspectButton.positionScale_Y = 1f;
        inspectButton.sizeOffset_X = -5;
        inspectButton.sizeOffset_Y = 50;
        inspectButton.sizeScale_X = 0.5f;
        inspectButton.text = localization.format("Inspect_Text");
        inspectButton.tooltipText = localization.format("Inspect_Tooltip");
        inspectButton.onClickedButton += onClickedInspectButton;
        descriptionBox.AddChild(inspectButton);
        inspectButton.fontSize = ESleekFontSize.Medium;
        inspectButton.isVisible = false;
        marketButton = Glazier.Get().CreateButton();
        marketButton.positionScale_Y = 1f;
        marketButton.sizeOffset_X = -5;
        marketButton.sizeOffset_Y = 50;
        marketButton.sizeScale_X = 0.5f;
        marketButton.text = localization.format("Market_Text");
        marketButton.tooltipText = localization.format("Market_Tooltip");
        marketButton.onClickedButton += onClickedMarketButton;
        descriptionBox.AddChild(marketButton);
        marketButton.fontSize = ESleekFontSize.Medium;
        marketButton.isVisible = false;
        deleteButton = Glazier.Get().CreateButton();
        deleteButton.positionOffset_X = 5;
        deleteButton.positionScale_X = 0.5f;
        deleteButton.positionScale_Y = 1f;
        deleteButton.sizeOffset_Y = 50;
        deleteButton.sizeScale_X = 0.5f;
        deleteButton.text = localization.format("Delete_Text");
        deleteButton.tooltipText = localization.format("Delete_Tooltip");
        deleteButton.onClickedButton += onClickedDeleteButton;
        descriptionBox.AddChild(deleteButton);
        deleteButton.fontSize = ESleekFontSize.Medium;
        scrapButton = Glazier.Get().CreateButton();
        scrapButton.positionOffset_X = 5;
        scrapButton.positionScale_X = 0.75f;
        scrapButton.positionScale_Y = 1f;
        scrapButton.sizeOffset_Y = 50;
        scrapButton.sizeScale_X = 0.25f;
        scrapButton.onClickedButton += onClickedScrapButton;
        descriptionBox.AddChild(scrapButton);
        scrapButton.fontSize = ESleekFontSize.Medium;
        scrapButton.isVisible = false;
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
