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
        deleteBox.SizeOffset_Y = 130f;
        yesButton.TooltipText = localization.format((mode == EDeleteMode.SALVAGE) ? "Yes_Salvage_Tooltip" : ((mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE) ? "Yes_Tag_Tool_Tooltip" : "Yes_Delete_Tooltip"));
        if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
        {
            int inventoryItem = Provider.provider.economyService.getInventoryItem(instigator);
            intentLabel.Text = localization.format("Intent_Tag_Tool", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(inventoryItem)) + ">" + Provider.provider.economyService.getInventoryName(inventoryItem) + "</color>", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryName(item) + "</color>");
        }
        else
        {
            intentLabel.Text = localization.format((mode == EDeleteMode.SALVAGE) ? "Intent_Salvage" : "Intent_Delete", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryName(item) + "</color>");
        }
        confirmLabel.Text = localization.format("Confirm", localization.format((mode == EDeleteMode.SALVAGE) ? "Salvage" : "Delete"));
        confirmLabel.IsVisible = mode != EDeleteMode.TAG_TOOL_ADD && mode != EDeleteMode.TAG_TOOL_REMOVE;
        confirmField.PlaceholderText = localization.format((mode == EDeleteMode.SALVAGE) ? "Salvage" : "Delete");
        confirmField.Text = string.Empty;
        confirmField.IsVisible = mode != EDeleteMode.TAG_TOOL_ADD && mode != EDeleteMode.TAG_TOOL_REMOVE;
        if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
        {
            yesButton.PositionOffset_X = -65f;
            yesButton.PositionScale_X = 0.5f;
            noButton.PositionOffset_X = 5f;
            noButton.PositionScale_X = 0.5f;
        }
        else
        {
            yesButton.PositionOffset_X = -135f;
            yesButton.PositionScale_X = 1f;
            noButton.PositionOffset_X = -65f;
            noButton.PositionScale_X = 1f;
        }
        if (mode == EDeleteMode.TAG_TOOL_ADD)
        {
            warningLabel.Text = localization.format("Warning_UndoableWithTool");
        }
        else
        {
            warningLabel.Text = localization.format("Warning");
        }
        quantityField.Value = 1;
        quantityField.MaxValue = quantity;
        if (mode == EDeleteMode.DELETE && quantity > 1)
        {
            quantityLabel.Text = localization.format("Quantity", quantity);
            deleteBox.SizeOffset_Y += 50f;
            quantityLabel.IsVisible = true;
            quantityField.IsVisible = true;
        }
        else
        {
            quantityLabel.IsVisible = false;
            quantityField.IsVisible = false;
        }
    }

    private static void onClickedYesButton(ISleekElement button)
    {
        if (mode == EDeleteMode.SALVAGE)
        {
            if (confirmField.Text != localization.format("Salvage"))
            {
                return;
            }
            salvageItem(item, instance);
        }
        else if (mode == EDeleteMode.DELETE)
        {
            if (confirmField.Text != localization.format("Delete"))
            {
                return;
            }
            Provider.provider.economyService.consumeItem(instance, MathfEx.Min(quantityField.Value, quantity));
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
        deleteBox = Glazier.Get().CreateBox();
        deleteBox.PositionOffset_Y = -65f;
        deleteBox.PositionScale_Y = 0.5f;
        deleteBox.SizeOffset_Y = 130f;
        deleteBox.SizeScale_X = 1f;
        inventory.AddChild(deleteBox);
        intentLabel = Glazier.Get().CreateLabel();
        intentLabel.AllowRichText = true;
        intentLabel.PositionOffset_X = 5f;
        intentLabel.PositionOffset_Y = 0f;
        intentLabel.SizeOffset_X = -10f;
        intentLabel.SizeOffset_Y = 30f;
        intentLabel.SizeScale_X = 1f;
        intentLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        intentLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        deleteBox.AddChild(intentLabel);
        warningLabel = Glazier.Get().CreateLabel();
        warningLabel.PositionOffset_X = 5f;
        warningLabel.PositionOffset_Y = 20f;
        warningLabel.SizeOffset_X = -10f;
        warningLabel.SizeOffset_Y = 30f;
        warningLabel.SizeScale_X = 1f;
        deleteBox.AddChild(warningLabel);
        confirmLabel = Glazier.Get().CreateLabel();
        confirmLabel.PositionOffset_X = 5f;
        confirmLabel.PositionOffset_Y = 40f;
        confirmLabel.SizeOffset_X = -10f;
        confirmLabel.SizeOffset_Y = 30f;
        confirmLabel.SizeScale_X = 1f;
        deleteBox.AddChild(confirmLabel);
        confirmField = Glazier.Get().CreateStringField();
        confirmField.PositionOffset_X = 5f;
        confirmField.PositionOffset_Y = 75f;
        confirmField.SizeOffset_X = -150f;
        confirmField.SizeOffset_Y = 50f;
        confirmField.SizeScale_X = 1f;
        confirmField.FontSize = ESleekFontSize.Medium;
        deleteBox.AddChild(confirmField);
        yesButton = Glazier.Get().CreateButton();
        yesButton.PositionOffset_X = -135f;
        yesButton.PositionOffset_Y = 75f;
        yesButton.PositionScale_X = 1f;
        yesButton.SizeOffset_X = 60f;
        yesButton.SizeOffset_Y = 50f;
        yesButton.FontSize = ESleekFontSize.Medium;
        yesButton.Text = localization.format("Yes");
        yesButton.OnClicked += onClickedYesButton;
        deleteBox.AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.PositionOffset_X = -65f;
        noButton.PositionOffset_Y = 75f;
        noButton.PositionScale_X = 1f;
        noButton.SizeOffset_X = 60f;
        noButton.SizeOffset_Y = 50f;
        noButton.FontSize = ESleekFontSize.Medium;
        noButton.Text = localization.format("No");
        noButton.TooltipText = localization.format("No_Tooltip");
        noButton.OnClicked += onClickedNoButton;
        deleteBox.AddChild(noButton);
        quantityLabel = Glazier.Get().CreateLabel();
        quantityLabel.PositionOffset_X = 5f;
        quantityLabel.PositionOffset_Y = -35f;
        quantityLabel.PositionScale_Y = 1f;
        quantityLabel.SizeOffset_X = -10f;
        quantityLabel.SizeOffset_Y = 30f;
        quantityLabel.SizeScale_X = 0.75f;
        quantityLabel.TextAlignment = TextAnchor.MiddleRight;
        quantityLabel.IsVisible = false;
        deleteBox.AddChild(quantityLabel);
        quantityField = Glazier.Get().CreateUInt16Field();
        quantityField.PositionOffset_X = 5f;
        quantityField.PositionOffset_Y = -35f;
        quantityField.PositionScale_X = 0.75f;
        quantityField.PositionScale_Y = 1f;
        quantityField.SizeOffset_X = -10f;
        quantityField.SizeOffset_Y = 30f;
        quantityField.SizeScale_X = 0.25f;
        quantityField.Value = 1;
        quantityField.MinValue = 1;
        quantityField.IsVisible = false;
        deleteBox.AddChild(quantityField);
    }
}
