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
            useButton.IsVisible = false;
            inspectButton.IsVisible = false;
            marketButton.IsVisible = false;
            scrapButton.IsVisible = false;
            deleteButton.IsVisible = true;
            descriptionBox.SizeOffset_Y = -60f;
            deleteButton.PositionOffset_Y = 0f - descriptionBox.SizeOffset_Y - 50f;
            deleteButton.SizeScale_X = 0.5f;
            infoLabel.Text = localization.format("Unknown");
            return;
        }
        if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.KEY)
        {
            if ((packageBox.itemAsset as ItemKeyAsset).exchangeWithTargetItem)
            {
                useButton.IsVisible = true;
                useButton.Text = localization.format("Target_Item_Text");
                useButton.TooltipText = localization.format("Target_Item_Tooltip");
            }
            else
            {
                useButton.IsVisible = false;
            }
            inspectButton.IsVisible = false;
        }
        else if (packageBox.itemAsset != null && packageBox.itemAsset.type == EItemType.BOX)
        {
            useButton.IsVisible = true;
            inspectButton.IsVisible = false;
            useButton.Text = localization.format("Contents_Text");
            useButton.TooltipText = localization.format("Contents_Tooltip");
        }
        else
        {
            useButton.IsVisible = true;
            inspectButton.IsVisible = true;
            bool flag = ((packageBox.itemAsset != null && packageBox.itemAsset.proPath != null && packageBox.itemAsset.proPath.Length != 0) ? Characters.isCosmeticEquipped(instance) : Characters.isSkinEquipped(instance));
            useButton.Text = localization.format(flag ? "Dequip_Text" : "Equip_Text");
            useButton.TooltipText = localization.format(flag ? "Dequip_Tooltip" : "Equip_Tooltip");
        }
        marketButton.IsVisible = Provider.provider.economyService.getInventoryMarketable(item);
        int inventoryScraps = Provider.provider.economyService.getInventoryScraps(item);
        scrapButton.Text = localization.format("Scrap_Text", inventoryScraps);
        scrapButton.TooltipText = localization.format("Scrap_Tooltip", inventoryScraps);
        scrapButton.IsVisible = inventoryScraps > 0;
        descriptionBox.SizeOffset_Y = 0f;
        if (useButton.IsVisible || inspectButton.IsVisible)
        {
            descriptionBox.SizeOffset_Y -= 60f;
            useButton.PositionOffset_Y = 0f - descriptionBox.SizeOffset_Y - 50f;
            inspectButton.PositionOffset_Y = 0f - descriptionBox.SizeOffset_Y - 50f;
        }
        if (scrapButton.IsVisible)
        {
            deleteButton.SizeScale_X = 0.25f;
        }
        else
        {
            deleteButton.SizeScale_X = 0.5f;
        }
        if (marketButton.IsVisible || deleteButton.IsVisible || scrapButton.IsVisible)
        {
            descriptionBox.SizeOffset_Y -= 60f;
            marketButton.PositionOffset_Y = 0f - descriptionBox.SizeOffset_Y - 50f;
            deleteButton.PositionOffset_Y = 0f - descriptionBox.SizeOffset_Y - 50f;
            scrapButton.PositionOffset_Y = 0f - descriptionBox.SizeOffset_Y - 50f;
        }
        infoLabel.Text = "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryType(item) + "</color>\n\n" + Provider.provider.economyService.getInventoryDescription(item);
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
            Characters.ToggleEquipItemByInstanceId(instance);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        inventory = Glazier.Get().CreateConstraintFrame();
        inventory.PositionScale_X = 0.5f;
        inventory.PositionOffset_Y = 10f;
        inventory.SizeScale_X = 0.5f;
        inventory.SizeScale_Y = 1f;
        inventory.SizeOffset_Y = -20f;
        inventory.Constraint = ESleekConstraint.FitInParent;
        container.AddChild(inventory);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.SizeScale_X = 1f;
        sleekConstraintFrame.SizeScale_Y = 0.5f;
        sleekConstraintFrame.SizeOffset_Y = -5f;
        sleekConstraintFrame.Constraint = ESleekConstraint.FitInParent;
        inventory.AddChild(sleekConstraintFrame);
        packageBox = new SleekInventory();
        packageBox.SizeScale_X = 1f;
        packageBox.SizeScale_Y = 1f;
        sleekConstraintFrame.AddChild(packageBox);
        descriptionBox = Glazier.Get().CreateBox();
        descriptionBox.PositionOffset_Y = 10f;
        descriptionBox.PositionScale_Y = 1f;
        descriptionBox.SizeScale_X = 1f;
        descriptionBox.SizeScale_Y = 1f;
        packageBox.AddChild(descriptionBox);
        infoLabel = Glazier.Get().CreateLabel();
        infoLabel.AllowRichText = true;
        infoLabel.PositionOffset_X = 5f;
        infoLabel.PositionOffset_Y = 5f;
        infoLabel.SizeScale_X = 1f;
        infoLabel.SizeScale_Y = 1f;
        infoLabel.SizeOffset_X = -10f;
        infoLabel.SizeOffset_Y = -10f;
        infoLabel.TextAlignment = TextAnchor.UpperLeft;
        infoLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        infoLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        descriptionBox.AddChild(infoLabel);
        useButton = Glazier.Get().CreateButton();
        useButton.PositionScale_Y = 1f;
        useButton.SizeOffset_X = -5f;
        useButton.SizeOffset_Y = 50f;
        useButton.SizeScale_X = 0.5f;
        useButton.OnClicked += onClickedUseButton;
        descriptionBox.AddChild(useButton);
        useButton.FontSize = ESleekFontSize.Medium;
        useButton.IsVisible = false;
        inspectButton = Glazier.Get().CreateButton();
        inspectButton.PositionOffset_X = 5f;
        inspectButton.PositionScale_X = 0.5f;
        inspectButton.PositionScale_Y = 1f;
        inspectButton.SizeOffset_X = -5f;
        inspectButton.SizeOffset_Y = 50f;
        inspectButton.SizeScale_X = 0.5f;
        inspectButton.Text = localization.format("Inspect_Text");
        inspectButton.TooltipText = localization.format("Inspect_Tooltip");
        inspectButton.OnClicked += onClickedInspectButton;
        descriptionBox.AddChild(inspectButton);
        inspectButton.FontSize = ESleekFontSize.Medium;
        inspectButton.IsVisible = false;
        marketButton = Glazier.Get().CreateButton();
        marketButton.PositionScale_Y = 1f;
        marketButton.SizeOffset_X = -5f;
        marketButton.SizeOffset_Y = 50f;
        marketButton.SizeScale_X = 0.5f;
        marketButton.Text = localization.format("Market_Text");
        marketButton.TooltipText = localization.format("Market_Tooltip");
        marketButton.OnClicked += onClickedMarketButton;
        descriptionBox.AddChild(marketButton);
        marketButton.FontSize = ESleekFontSize.Medium;
        marketButton.IsVisible = false;
        deleteButton = Glazier.Get().CreateButton();
        deleteButton.PositionOffset_X = 5f;
        deleteButton.PositionScale_X = 0.5f;
        deleteButton.PositionScale_Y = 1f;
        deleteButton.SizeOffset_Y = 50f;
        deleteButton.SizeScale_X = 0.5f;
        deleteButton.Text = localization.format("Delete_Text");
        deleteButton.TooltipText = localization.format("Delete_Tooltip");
        deleteButton.OnClicked += onClickedDeleteButton;
        descriptionBox.AddChild(deleteButton);
        deleteButton.FontSize = ESleekFontSize.Medium;
        scrapButton = Glazier.Get().CreateButton();
        scrapButton.PositionOffset_X = 5f;
        scrapButton.PositionScale_X = 0.75f;
        scrapButton.PositionScale_Y = 1f;
        scrapButton.SizeOffset_Y = 50f;
        scrapButton.SizeScale_X = 0.25f;
        scrapButton.OnClicked += onClickedScrapButton;
        descriptionBox.AddChild(scrapButton);
        scrapButton.FontSize = ESleekFontSize.Medium;
        scrapButton.IsVisible = false;
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
