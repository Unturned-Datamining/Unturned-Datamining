using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerNPCVendorUI
{
    private static SleekFullscreenBox container;

    public static Local localization;

    public static bool active;

    private static VendorAsset vendor;

    private static DialogueResponse response;

    private static DialogueAsset dialogue;

    private static DialogueAsset prevDialogue;

    private static List<VendorBuying> buying;

    private static List<VendorSellingBase> selling;

    private static List<SleekVendor> buyingButtons;

    private static List<SleekVendor> sellingButtons;

    private static VendorBuyingNameAscendingComparator buyingComparator = new VendorBuyingNameAscendingComparator();

    private static VendorSellingNameAscendingComparator sellingComparator = new VendorSellingNameAscendingComparator();

    private static ISleekBox vendorBox;

    private static ISleekLabel nameLabel;

    private static ISleekLabel descriptionLabel;

    private static ISleekLabel sellingLabel;

    private static ISleekScrollView sellingBox;

    private static ISleekLabel buyingLabel;

    private static ISleekScrollView buyingBox;

    private static ISleekBox experienceBox;

    private static ISleekBox currencyBox;

    private static ISleekElement currencyPanel;

    private static ISleekLabel currencyLabel;

    private static ISleekButton returnButton;

    private static bool needsRefresh;

    public static void open(VendorAsset newVendor, DialogueResponse newResponse, DialogueAsset newDialogue, DialogueAsset newPrevDialogue)
    {
        if (!active)
        {
            active = true;
            updateVendor(newVendor, newResponse, newDialogue, newPrevDialogue);
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

    public static void closeNicely()
    {
        close();
        PlayerNPCDialogueUI.open(dialogue, prevDialogue);
    }

    public static void MaybeRefresh()
    {
        if (!active || !needsRefresh || vendor == null)
        {
            return;
        }
        Player player = Player.player;
        if (player == null || player.inventory == null)
        {
            return;
        }
        needsRefresh = false;
        RefreshExperienceOrCurrencyBoxAmount();
        RefreshButtonVisibility();
        foreach (SleekVendor buyingButton in buyingButtons)
        {
            buyingButton.updateAmount();
        }
        foreach (SleekVendor sellingButton in sellingButtons)
        {
            sellingButton.updateAmount();
        }
    }

    private static void RefreshButtonVisibility()
    {
        Player player = Player.player;
        int num = 0;
        for (int i = 0; i < buying.Count; i++)
        {
            bool flag = buying[i].areConditionsMet(player);
            buyingButtons[i].isVisible = flag;
            if (flag)
            {
                buyingButtons[i].positionOffset_Y = num;
                num += buyingButtons[i].sizeOffset_Y;
            }
        }
        buyingBox.isVisible = num > 0;
        buyingBox.contentSizeOffset = new Vector2(0f, num);
        int num2 = 0;
        for (int j = 0; j < selling.Count; j++)
        {
            bool flag2 = selling[j].areConditionsMet(player);
            sellingButtons[j].isVisible = flag2;
            if (flag2)
            {
                sellingButtons[j].positionOffset_Y = num2;
                num2 += sellingButtons[j].sizeOffset_Y;
            }
        }
        sellingBox.isVisible = num2 > 0;
        sellingBox.contentSizeOffset = new Vector2(0f, num2);
    }

    private static void RefreshExperienceOrCurrencyBoxAmount()
    {
        if (experienceBox.isVisible)
        {
            experienceBox.text = localization.format("Experience", Player.player.skills.experience.ToString());
        }
        else
        {
            if (!currencyBox.isVisible)
            {
                return;
            }
            ItemCurrencyAsset itemCurrencyAsset = vendor.currency.Find();
            if (itemCurrencyAsset != null)
            {
                uint inventoryValue = itemCurrencyAsset.getInventoryValue(Player.player);
                if (string.IsNullOrEmpty(itemCurrencyAsset.valueFormat))
                {
                    currencyLabel.text = inventoryValue.ToString("N");
                }
                else
                {
                    currencyLabel.text = string.Format(itemCurrencyAsset.valueFormat, inventoryValue);
                }
            }
        }
    }

    private static void updateCurrencyOrExperienceBox()
    {
        currencyBox.isVisible = vendor.currency.isValid;
        experienceBox.isVisible = !currencyBox.isVisible;
        if (!currencyBox.isVisible)
        {
            return;
        }
        currencyPanel.RemoveAllChildren();
        ItemCurrencyAsset itemCurrencyAsset = vendor.currency.Find();
        if (itemCurrencyAsset == null)
        {
            Assets.reportError(vendor, "unable to find currency");
            currencyLabel.text = "Invalid";
            return;
        }
        int num = 5;
        ItemCurrencyAsset.Entry[] entries = itemCurrencyAsset.entries;
        for (int i = 0; i < entries.Length; i++)
        {
            ItemCurrencyAsset.Entry entry = entries[i];
            AssetReference<ItemAsset> item = entry.item;
            ItemAsset itemAsset = item.Find();
            if (itemAsset == null)
            {
                Assets.reportError(vendor, "unable to find entry item {0}", entry.item);
            }
            else if (entry.isVisibleInVendorMenu)
            {
                SleekItemIcon sleekItemIcon = new SleekItemIcon();
                sleekItemIcon.positionOffset_X = num;
                sleekItemIcon.positionOffset_Y = 5;
                float num2 = (float)(int)itemAsset.size_x / (float)(int)itemAsset.size_y;
                sleekItemIcon.sizeOffset_X = Mathf.RoundToInt(num2 * 40f);
                sleekItemIcon.sizeOffset_Y = 40;
                currencyPanel.AddChild(sleekItemIcon);
                sleekItemIcon.Refresh(itemAsset.id, 100, itemAsset.getState(isFull: false), itemAsset, sleekItemIcon.sizeOffset_X, sleekItemIcon.sizeOffset_Y);
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.positionOffset_X = sleekItemIcon.positionOffset_X;
                sleekLabel.positionOffset_Y = 0;
                sleekLabel.sizeOffset_X = sleekItemIcon.sizeOffset_X;
                sleekLabel.sizeScale_Y = 1f;
                uint value = entry.value;
                sleekLabel.text = value.ToString();
                sleekLabel.fontAlignment = TextAnchor.LowerCenter;
                sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                currencyPanel.AddChild(sleekLabel);
                num += sleekItemIcon.sizeOffset_X + 2;
            }
        }
    }

    private static void updateVendor(VendorAsset newVendor, DialogueResponse newResponse, DialogueAsset newDialogue, DialogueAsset newPrevDialogue)
    {
        vendor = newVendor;
        response = newResponse;
        dialogue = newDialogue;
        prevDialogue = newPrevDialogue;
        if (vendor == null)
        {
            return;
        }
        if (PlayerLifeUI.npc != null)
        {
            PlayerLifeUI.npc.SetFaceOverride(vendor.faceOverride);
        }
        nameLabel.text = vendor.vendorName;
        descriptionLabel.text = vendor.vendorDescription;
        buyingButtons.Clear();
        sellingButtons.Clear();
        buying.Clear();
        buying.AddRange(vendor.buying);
        if (vendor.enableSorting)
        {
            buying.Sort(buyingComparator);
        }
        buyingBox.RemoveAllChildren();
        foreach (VendorBuying item in buying)
        {
            SleekVendor sleekVendor = new SleekVendor(item);
            sleekVendor.sizeScale_X = 1f;
            sleekVendor.onClickedButton += onClickedBuyingButton;
            buyingBox.AddChild(sleekVendor);
            buyingButtons.Add(sleekVendor);
        }
        selling.Clear();
        selling.AddRange(vendor.selling);
        if (vendor.enableSorting)
        {
            selling.Sort(sellingComparator);
        }
        sellingBox.RemoveAllChildren();
        foreach (VendorSellingBase item2 in selling)
        {
            SleekVendor sleekVendor2 = new SleekVendor(item2);
            sleekVendor2.sizeScale_X = 1f;
            sleekVendor2.onClickedButton += onClickedSellingButton;
            sellingBox.AddChild(sleekVendor2);
            sellingButtons.Add(sleekVendor2);
        }
        needsRefresh = false;
        updateCurrencyOrExperienceBox();
        RefreshExperienceOrCurrencyBoxAmount();
        RefreshButtonVisibility();
    }

    private static void onInventoryStateUpdated()
    {
        needsRefresh = true;
    }

    private static void onExperienceUpdated(uint newExperience)
    {
        needsRefresh = true;
    }

    private static void onReputationUpdated(int newReputation)
    {
        needsRefresh = true;
    }

    private static void onFlagsUpdated()
    {
        needsRefresh = true;
    }

    private static void onFlagUpdated(ushort id)
    {
        needsRefresh = true;
    }

    private static void onClickedBuyingButton(ISleekElement button)
    {
        byte index = (byte)buyingBox.FindIndexOfChild(button);
        VendorBuying vendorBuying = buying[index];
        if (vendorBuying.canSell(Player.player))
        {
            Player.player.quests.sendSellToVendor(vendor.GUID, vendorBuying.index, InputEx.GetKey(ControlsSettings.other));
        }
    }

    private static void onClickedSellingButton(ISleekElement button)
    {
        byte index = (byte)sellingBox.FindIndexOfChild(button);
        VendorSellingBase vendorSellingBase = selling[index];
        if (vendorSellingBase.canBuy(Player.player))
        {
            Player.player.quests.sendBuyFromVendor(vendor.GUID, vendorSellingBase.index, InputEx.GetKey(ControlsSettings.other));
        }
    }

    private static void onClickedReturnButton(ISleekElement button)
    {
        closeNicely();
    }

    public PlayerNPCVendorUI()
    {
        localization = Localization.read("/Player/PlayerNPCVendor.dat");
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        buying = new List<VendorBuying>();
        selling = new List<VendorSellingBase>();
        buyingButtons = new List<SleekVendor>();
        sellingButtons = new List<SleekVendor>();
        vendorBox = Glazier.Get().CreateBox();
        vendorBox.sizeOffset_Y = -60;
        vendorBox.sizeScale_X = 1f;
        vendorBox.sizeScale_Y = 1f;
        container.AddChild(vendorBox);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_X = 5;
        nameLabel.positionOffset_Y = 5;
        nameLabel.sizeOffset_X = -10;
        nameLabel.sizeOffset_Y = 40;
        nameLabel.sizeScale_X = 1f;
        nameLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.enableRichText = true;
        nameLabel.fontSize = ESleekFontSize.Large;
        vendorBox.AddChild(nameLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.positionOffset_X = 5;
        descriptionLabel.positionOffset_Y = 40;
        descriptionLabel.sizeOffset_X = -10;
        descriptionLabel.sizeOffset_Y = 40;
        descriptionLabel.sizeScale_X = 1f;
        descriptionLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        descriptionLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        descriptionLabel.enableRichText = true;
        vendorBox.AddChild(descriptionLabel);
        buyingLabel = Glazier.Get().CreateLabel();
        buyingLabel.positionOffset_X = 5;
        buyingLabel.positionOffset_Y = 80;
        buyingLabel.sizeOffset_X = -40;
        buyingLabel.sizeOffset_Y = 30;
        buyingLabel.sizeScale_X = 0.5f;
        buyingLabel.fontSize = ESleekFontSize.Medium;
        buyingLabel.text = localization.format("Buying");
        vendorBox.AddChild(buyingLabel);
        buyingBox = Glazier.Get().CreateScrollView();
        buyingBox.positionOffset_X = 5;
        buyingBox.positionOffset_Y = 115;
        buyingBox.sizeOffset_X = -10;
        buyingBox.sizeOffset_Y = -120;
        buyingBox.sizeScale_X = 0.5f;
        buyingBox.sizeScale_Y = 1f;
        buyingBox.scaleContentToWidth = true;
        buyingBox.contentSizeOffset = new Vector2(0f, 1024f);
        vendorBox.AddChild(buyingBox);
        sellingLabel = Glazier.Get().CreateLabel();
        sellingLabel.positionOffset_X = 5;
        sellingLabel.positionOffset_Y = 80;
        sellingLabel.positionScale_X = 0.5f;
        sellingLabel.sizeOffset_X = -40;
        sellingLabel.sizeOffset_Y = 30;
        sellingLabel.sizeScale_X = 0.5f;
        sellingLabel.fontSize = ESleekFontSize.Medium;
        sellingLabel.text = localization.format("Selling");
        vendorBox.AddChild(sellingLabel);
        sellingBox = Glazier.Get().CreateScrollView();
        sellingBox.positionOffset_X = 5;
        sellingBox.positionOffset_Y = 115;
        sellingBox.positionScale_X = 0.5f;
        sellingBox.sizeOffset_X = -10;
        sellingBox.sizeOffset_Y = -120;
        sellingBox.sizeScale_X = 0.5f;
        sellingBox.sizeScale_Y = 1f;
        sellingBox.scaleContentToWidth = true;
        sellingBox.contentSizeOffset = new Vector2(0f, 1024f);
        vendorBox.AddChild(sellingBox);
        experienceBox = Glazier.Get().CreateBox();
        experienceBox.positionOffset_Y = 10;
        experienceBox.positionScale_Y = 1f;
        experienceBox.sizeOffset_X = -5;
        experienceBox.sizeOffset_Y = 50;
        experienceBox.sizeScale_X = 0.5f;
        experienceBox.fontSize = ESleekFontSize.Medium;
        experienceBox.isVisible = false;
        vendorBox.AddChild(experienceBox);
        currencyBox = Glazier.Get().CreateBox();
        currencyBox.positionOffset_Y = 10;
        currencyBox.positionScale_Y = 1f;
        currencyBox.sizeOffset_X = -5;
        currencyBox.sizeOffset_Y = 50;
        currencyBox.sizeScale_X = 0.5f;
        currencyBox.isVisible = false;
        vendorBox.AddChild(currencyBox);
        currencyPanel = Glazier.Get().CreateFrame();
        currencyPanel.sizeScale_X = 1f;
        currencyPanel.sizeScale_Y = 1f;
        currencyBox.AddChild(currencyPanel);
        currencyLabel = Glazier.Get().CreateLabel();
        currencyLabel.positionOffset_X = -160;
        currencyLabel.positionScale_X = 1f;
        currencyLabel.sizeOffset_X = 150;
        currencyLabel.sizeScale_Y = 1f;
        currencyLabel.fontAlignment = TextAnchor.MiddleRight;
        currencyLabel.fontSize = ESleekFontSize.Medium;
        currencyBox.AddChild(currencyLabel);
        returnButton = Glazier.Get().CreateButton();
        returnButton.positionOffset_X = 5;
        returnButton.positionOffset_Y = 10;
        returnButton.positionScale_X = 0.5f;
        returnButton.positionScale_Y = 1f;
        returnButton.sizeOffset_X = -5;
        returnButton.sizeOffset_Y = 50;
        returnButton.sizeScale_X = 0.5f;
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.text = localization.format("Return");
        returnButton.tooltipText = localization.format("Return_Tooltip");
        returnButton.onClickedButton += onClickedReturnButton;
        vendorBox.AddChild(returnButton);
        PlayerInventory inventory = Player.player.inventory;
        inventory.onInventoryStateUpdated = (InventoryStateUpdated)Delegate.Combine(inventory.onInventoryStateUpdated, new InventoryStateUpdated(onInventoryStateUpdated));
        PlayerSkills skills = Player.player.skills;
        skills.onExperienceUpdated = (ExperienceUpdated)Delegate.Combine(skills.onExperienceUpdated, new ExperienceUpdated(onExperienceUpdated));
        PlayerSkills skills2 = Player.player.skills;
        skills2.onReputationUpdated = (ReputationUpdated)Delegate.Combine(skills2.onReputationUpdated, new ReputationUpdated(onReputationUpdated));
        PlayerQuests quests = Player.player.quests;
        quests.onFlagsUpdated = (FlagsUpdated)Delegate.Combine(quests.onFlagsUpdated, new FlagsUpdated(onFlagsUpdated));
        PlayerQuests quests2 = Player.player.quests;
        quests2.onFlagUpdated = (FlagUpdated)Delegate.Combine(quests2.onFlagUpdated, new FlagUpdated(onFlagUpdated));
        needsRefresh = true;
    }
}
