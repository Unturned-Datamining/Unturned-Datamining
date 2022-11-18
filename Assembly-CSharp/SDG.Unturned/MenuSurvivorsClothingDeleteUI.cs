using System.Collections.Generic;
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsClothingDeleteUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static int item;

    private static ulong instance;

    private static ushort quantity;

    private static EDeleteMode mode;

    private static ulong instigator;

    private static ISleekConstraintFrame inventory;

    private static ISleekBox deleteBox;

    private static ISleekLabel intentLabel;

    private static ISleekLabel warningLabel;

    private static ISleekLabel confirmLabel;

    private static ISleekField confirmField;

    private static ISleekButton yesButton;

    private static ISleekButton noButton;

    private static ISleekLabel quantityLabel;

    private static ISleekUInt16Field quantityField;

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

    public static void salvageItem(int itemID, ulong instanceID)
    {
        int scrapExchangeItem = Provider.provider.economyService.getScrapExchangeItem(itemID);
        if (scrapExchangeItem < 1)
        {
            UnturnedLog.warn("Unable to find exchange target for salvaging itemdef {0} ({1})", itemID, instanceID);
        }
        else
        {
            Provider.provider.economyService.exchangeInventory(scrapExchangeItem, new List<EconExchangePair>
            {
                new EconExchangePair(instanceID, 1)
            });
        }
    }

    public static void applyTagTool(int itemID, ulong targetID, ulong toolID)
    {
        List<EconExchangePair> destroy = new List<EconExchangePair>
        {
            new EconExchangePair(targetID, 1),
            new EconExchangePair(toolID, 1)
        };
        Provider.provider.economyService.exchangeInventory(itemID, destroy);
    }

    public static void viewItem(int newItem, ulong newInstance, ushort newQuantity, EDeleteMode newMode, ulong newInstigator)
    {
        item = newItem;
        instance = newInstance;
        quantity = newQuantity;
        mode = newMode;
        instigator = newInstigator;
        deleteBox.sizeOffset_Y = 130;
        yesButton.tooltipText = localization.format((mode == EDeleteMode.SALVAGE) ? "Yes_Salvage_Tooltip" : ((mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE) ? "Yes_Tag_Tool_Tooltip" : "Yes_Delete_Tooltip"));
        if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
        {
            int inventoryItem = Provider.provider.economyService.getInventoryItem(instigator);
            intentLabel.text = localization.format("Intent_Tag_Tool", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(inventoryItem)) + ">" + Provider.provider.economyService.getInventoryName(inventoryItem) + "</color>", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryName(item) + "</color>");
        }
        else
        {
            intentLabel.text = localization.format((mode == EDeleteMode.SALVAGE) ? "Intent_Salvage" : "Intent_Delete", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryName(item) + "</color>");
        }
        confirmLabel.text = localization.format("Confirm", localization.format((mode == EDeleteMode.SALVAGE) ? "Salvage" : "Delete"));
        confirmLabel.isVisible = mode != EDeleteMode.TAG_TOOL_ADD && mode != EDeleteMode.TAG_TOOL_REMOVE;
        confirmField.hint = localization.format((mode == EDeleteMode.SALVAGE) ? "Salvage" : "Delete");
        confirmField.text = string.Empty;
        confirmField.isVisible = mode != EDeleteMode.TAG_TOOL_ADD && mode != EDeleteMode.TAG_TOOL_REMOVE;
        if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
        {
            yesButton.positionOffset_X = -65;
            yesButton.positionScale_X = 0.5f;
            noButton.positionOffset_X = 5;
            noButton.positionScale_X = 0.5f;
        }
        else
        {
            yesButton.positionOffset_X = -135;
            yesButton.positionScale_X = 1f;
            noButton.positionOffset_X = -65;
            noButton.positionScale_X = 1f;
        }
        if (mode == EDeleteMode.TAG_TOOL_ADD)
        {
            warningLabel.text = localization.format("Warning_UndoableWithTool");
        }
        else
        {
            warningLabel.text = localization.format("Warning");
        }
        quantityField.state = 1;
        quantityField.maxValue = quantity;
        if (mode == EDeleteMode.DELETE && quantity > 1)
        {
            quantityLabel.text = localization.format("Quantity", quantity);
            deleteBox.sizeOffset_Y += 50;
            quantityLabel.isVisible = true;
            quantityField.isVisible = true;
        }
        else
        {
            quantityLabel.isVisible = false;
            quantityField.isVisible = false;
        }
    }

    private static void onClickedYesButton(ISleekElement button)
    {
        if (mode == EDeleteMode.SALVAGE)
        {
            if (confirmField.text != localization.format("Salvage"))
            {
                return;
            }
            salvageItem(item, instance);
        }
        else if (mode == EDeleteMode.DELETE)
        {
            if (confirmField.text != localization.format("Delete"))
            {
                return;
            }
            Provider.provider.economyService.consumeItem(instance, MathfEx.Min(quantityField.state, quantity));
        }
        MenuSurvivorsClothingUI.open();
        close();
        if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
        {
            MenuSurvivorsClothingUI.prepareForCraftResult();
            applyTagTool(item, instance, instigator);
        }
    }

    private static void onClickedNoButton(ISleekElement button)
    {
        MenuSurvivorsClothingItemUI.open();
        close();
    }

    public MenuSurvivorsClothingDeleteUI()
    {
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothingDelete.dat");
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
        deleteBox = Glazier.Get().CreateBox();
        deleteBox.positionOffset_Y = -65;
        deleteBox.positionScale_Y = 0.5f;
        deleteBox.sizeOffset_Y = 130;
        deleteBox.sizeScale_X = 1f;
        inventory.AddChild(deleteBox);
        intentLabel = Glazier.Get().CreateLabel();
        intentLabel.enableRichText = true;
        intentLabel.positionOffset_X = 5;
        intentLabel.positionOffset_Y = 0;
        intentLabel.sizeOffset_X = -10;
        intentLabel.sizeOffset_Y = 30;
        intentLabel.sizeScale_X = 1f;
        intentLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        intentLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        deleteBox.AddChild(intentLabel);
        warningLabel = Glazier.Get().CreateLabel();
        warningLabel.positionOffset_X = 5;
        warningLabel.positionOffset_Y = 20;
        warningLabel.sizeOffset_X = -10;
        warningLabel.sizeOffset_Y = 30;
        warningLabel.sizeScale_X = 1f;
        deleteBox.AddChild(warningLabel);
        confirmLabel = Glazier.Get().CreateLabel();
        confirmLabel.positionOffset_X = 5;
        confirmLabel.positionOffset_Y = 40;
        confirmLabel.sizeOffset_X = -10;
        confirmLabel.sizeOffset_Y = 30;
        confirmLabel.sizeScale_X = 1f;
        deleteBox.AddChild(confirmLabel);
        confirmField = Glazier.Get().CreateStringField();
        confirmField.positionOffset_X = 5;
        confirmField.positionOffset_Y = 75;
        confirmField.sizeOffset_X = -150;
        confirmField.sizeOffset_Y = 50;
        confirmField.sizeScale_X = 1f;
        confirmField.fontSize = ESleekFontSize.Medium;
        deleteBox.AddChild(confirmField);
        yesButton = Glazier.Get().CreateButton();
        yesButton.positionOffset_X = -135;
        yesButton.positionOffset_Y = 75;
        yesButton.positionScale_X = 1f;
        yesButton.sizeOffset_X = 60;
        yesButton.sizeOffset_Y = 50;
        yesButton.fontSize = ESleekFontSize.Medium;
        yesButton.text = localization.format("Yes");
        yesButton.onClickedButton += onClickedYesButton;
        deleteBox.AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.positionOffset_X = -65;
        noButton.positionOffset_Y = 75;
        noButton.positionScale_X = 1f;
        noButton.sizeOffset_X = 60;
        noButton.sizeOffset_Y = 50;
        noButton.fontSize = ESleekFontSize.Medium;
        noButton.text = localization.format("No");
        noButton.tooltipText = localization.format("No_Tooltip");
        noButton.onClickedButton += onClickedNoButton;
        deleteBox.AddChild(noButton);
        quantityLabel = Glazier.Get().CreateLabel();
        quantityLabel.positionOffset_X = 5;
        quantityLabel.positionOffset_Y = -35;
        quantityLabel.positionScale_Y = 1f;
        quantityLabel.sizeOffset_X = -10;
        quantityLabel.sizeOffset_Y = 30;
        quantityLabel.sizeScale_X = 0.75f;
        quantityLabel.fontAlignment = TextAnchor.MiddleRight;
        quantityLabel.isVisible = false;
        deleteBox.AddChild(quantityLabel);
        quantityField = Glazier.Get().CreateUInt16Field();
        quantityField.positionOffset_X = 5;
        quantityField.positionOffset_Y = -35;
        quantityField.positionScale_X = 0.75f;
        quantityField.positionScale_Y = 1f;
        quantityField.sizeOffset_X = -10;
        quantityField.sizeOffset_Y = 30;
        quantityField.sizeScale_X = 0.25f;
        quantityField.state = 1;
        quantityField.minValue = 1;
        quantityField.isVisible = false;
        deleteBox.AddChild(quantityField);
    }
}
