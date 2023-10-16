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
            num += item.jar.item.amount;
        }
        return doesLogicPass(num, amount);
    }

    public override void ApplyCondition(Player player)
    {
        if (!shouldReset)
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
            sleekBox.SizeOffset_Y = itemAsset.size_y * 50 + 10;
        }
        else
        {
            sleekBox.SizeOffset_Y = itemAsset.size_y * 25 + 10;
        }
        sleekBox.SizeScale_X = 1f;
        if (icon != null)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage(icon);
            sleekImage.PositionOffset_X = 5f;
            sleekImage.PositionOffset_Y = -10f;
            sleekImage.PositionScale_Y = 0.5f;
            sleekImage.SizeOffset_X = 20f;
            sleekImage.SizeOffset_Y = 20f;
            sleekBox.AddChild(sleekImage);
        }
        SleekItemIcon sleekItemIcon = new SleekItemIcon();
        if (icon != null)
        {
            sleekItemIcon.PositionOffset_X = 30f;
        }
        else
        {
            sleekItemIcon.PositionOffset_X = 5f;
        }
        sleekItemIcon.PositionOffset_Y = 5f;
        if (itemAsset.size_y == 1)
        {
            sleekItemIcon.SizeOffset_X = itemAsset.size_x * 50;
            sleekItemIcon.SizeOffset_Y = itemAsset.size_y * 50;
        }
        else
        {
            sleekItemIcon.SizeOffset_X = itemAsset.size_x * 25;
            sleekItemIcon.SizeOffset_Y = itemAsset.size_y * 25;
        }
        sleekBox.AddChild(sleekItemIcon);
        sleekItemIcon.Refresh(itemAsset.id, 100, itemAsset.getState(isFull: false), itemAsset, Mathf.RoundToInt(sleekItemIcon.SizeOffset_X), Mathf.RoundToInt(sleekItemIcon.SizeOffset_Y));
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        if (icon != null)
        {
            sleekLabel.PositionOffset_X = 35f + sleekItemIcon.SizeOffset_X;
            sleekLabel.SizeOffset_X = -40f - sleekItemIcon.SizeOffset_X;
        }
        else
        {
            sleekLabel.PositionOffset_X = 10f + sleekItemIcon.SizeOffset_X;
            sleekLabel.SizeOffset_X = -15f - sleekItemIcon.SizeOffset_X;
        }
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeScale_Y = 1f;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekLabel.AllowRichText = true;
        sleekLabel.Text = value;
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
