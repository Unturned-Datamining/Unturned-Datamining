using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class NPCItemCondition : NPCLogicCondition
{
    private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

    private static List<InventorySearch> search = new List<InventorySearch>();

    private static List<InventorySearch> applyConditionSearch = new List<InventorySearch>();

    public Guid itemGuid;

    [Obsolete]
    public ushort id { get; protected set; }

    public ushort amount { get; protected set; }

    public ItemAsset GetItemAsset()
    {
        return Assets.FindItemByGuidOrLegacyId<ItemAsset>(itemGuid, id);
    }

    public override bool isConditionMet(Player player)
    {
        ItemAsset itemAsset = GetItemAsset();
        if (itemAsset == null)
        {
            return false;
        }
        search.Clear();
        player.inventory.search(search, itemAsset.id, findEmpty: false, findHealthy: true);
        ushort num = 0;
        foreach (InventorySearch item in search)
        {
            num = (ushort)(num + item.jar.item.amount);
        }
        return doesLogicPass(num, amount);
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (!Provider.isServer || !shouldReset)
        {
            return;
        }
        ItemAsset itemAsset = GetItemAsset();
        if (itemAsset == null)
        {
            return;
        }
        applyConditionSearch.Clear();
        player.inventory.search(applyConditionSearch, itemAsset.id, findEmpty: false, findHealthy: true);
        applyConditionSearch.Sort(qualityAscendingComparator);
        uint num = amount;
        foreach (InventorySearch item in applyConditionSearch)
        {
            uint num2 = item.deleteAmount(player, num);
            num -= num2;
            if (num == 0)
            {
                break;
            }
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.format("Condition_Item");
        }
        ItemAsset itemAsset = GetItemAsset();
        if (itemAsset != null)
        {
            string arg = "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemAsset.rarity)) + ">" + itemAsset.itemName + "</color>";
            search.Clear();
            player.inventory.search(search, itemAsset.id, findEmpty: false, findHealthy: true);
            return string.Format(text, search.Count, amount, arg);
        }
        return string.Format(text, 0, amount, "?");
    }

    public override ISleekElement createUI(Player player, Texture2D icon)
    {
        string value = formatCondition(player);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        ItemAsset itemAsset = GetItemAsset();
        if (itemAsset == null)
        {
            return null;
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        if (itemAsset.size_y == 1)
        {
            sleekBox.sizeOffset_Y = itemAsset.size_y * 50 + 10;
        }
        else
        {
            sleekBox.sizeOffset_Y = itemAsset.size_y * 25 + 10;
        }
        sleekBox.sizeScale_X = 1f;
        if (icon != null)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage(icon);
            sleekImage.positionOffset_X = 5;
            sleekImage.positionOffset_Y = -10;
            sleekImage.positionScale_Y = 0.5f;
            sleekImage.sizeOffset_X = 20;
            sleekImage.sizeOffset_Y = 20;
            sleekBox.AddChild(sleekImage);
        }
        SleekItemIcon sleekItemIcon = new SleekItemIcon();
        if (icon != null)
        {
            sleekItemIcon.positionOffset_X = 30;
        }
        else
        {
            sleekItemIcon.positionOffset_X = 5;
        }
        sleekItemIcon.positionOffset_Y = 5;
        if (itemAsset.size_y == 1)
        {
            sleekItemIcon.sizeOffset_X = itemAsset.size_x * 50;
            sleekItemIcon.sizeOffset_Y = itemAsset.size_y * 50;
        }
        else
        {
            sleekItemIcon.sizeOffset_X = itemAsset.size_x * 25;
            sleekItemIcon.sizeOffset_Y = itemAsset.size_y * 25;
        }
        sleekBox.AddChild(sleekItemIcon);
        sleekItemIcon.Refresh(itemAsset.id, 100, itemAsset.getState(isFull: false), itemAsset, sleekItemIcon.sizeOffset_X, sleekItemIcon.sizeOffset_Y);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        if (icon != null)
        {
            sleekLabel.positionOffset_X = 35 + sleekItemIcon.sizeOffset_X;
            sleekLabel.sizeOffset_X = -40 - sleekItemIcon.sizeOffset_X;
        }
        else
        {
            sleekLabel.positionOffset_X = 10 + sleekItemIcon.sizeOffset_X;
            sleekLabel.sizeOffset_X = -15 - sleekItemIcon.sizeOffset_X;
        }
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.sizeScale_Y = 1f;
        sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
        sleekLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        sleekLabel.enableRichText = true;
        sleekLabel.text = value;
        sleekBox.AddChild(sleekLabel);
        return sleekBox;
    }

    public NPCItemCondition(Guid newItemGuid, ushort newID, ushort newAmount, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        itemGuid = newItemGuid;
        id = newID;
        amount = newAmount;
    }
}
