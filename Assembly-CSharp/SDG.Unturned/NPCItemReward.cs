using System;
using UnityEngine;

namespace SDG.Unturned;

public class NPCItemReward : INPCReward
{
    public Guid itemGuid;

    [Obsolete]
    public ushort id { get; protected set; }

    public byte amount { get; protected set; }

    public bool shouldAutoEquip { get; protected set; }

    public int sight { get; protected set; }

    public int tactical { get; protected set; }

    public int grip { get; protected set; }

    public int barrel { get; protected set; }

    public int magazine { get; protected set; }

    public int ammo { get; protected set; }

    public EItemOrigin origin { get; protected set; }

    public ItemAsset GetItemAsset()
    {
        return Assets.FindItemByGuidOrLegacyId<ItemAsset>(itemGuid, id);
    }

    public override void GrantReward(Player player)
    {
        ItemAsset itemAsset = GetItemAsset();
        if (itemAsset == null)
        {
            return;
        }
        for (byte b = 0; b < amount; b++)
        {
            Item item;
            if (sight > -1 || tactical > -1 || grip > -1 || barrel > -1 || magazine > -1 || ammo > -1)
            {
                if (itemAsset is ItemGunAsset itemGunAsset)
                {
                    ushort num = ((sight > -1) ? MathfEx.ClampToUShort(sight) : itemGunAsset.sightID);
                    ushort num2 = ((tactical > -1) ? MathfEx.ClampToUShort(tactical) : itemGunAsset.tacticalID);
                    ushort num3 = ((grip > -1) ? MathfEx.ClampToUShort(grip) : itemGunAsset.gripID);
                    ushort num4 = ((barrel > -1) ? MathfEx.ClampToUShort(barrel) : itemGunAsset.barrelID);
                    ushort num5 = ((magazine > -1) ? MathfEx.ClampToUShort(magazine) : itemGunAsset.getMagazineID());
                    byte b2 = ((ammo > -1) ? MathfEx.ClampToByte(ammo) : itemGunAsset.ammoMax);
                    byte[] state = itemGunAsset.getState(num, num2, num3, num4, num5, b2);
                    item = new Item(itemAsset.id, 1, 100, state);
                }
                else
                {
                    item = new Item(itemAsset.id, origin);
                }
            }
            else
            {
                item = new Item(itemAsset.id, origin);
            }
            player.inventory.forceAddItem(item, shouldAutoEquip, playEffect: false);
        }
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Item");
        }
        ItemAsset itemAsset = GetItemAsset();
        return Local.FormatText(arg1: (itemAsset == null) ? "?" : ("<color=" + Palette.hex(ItemTool.getRarityColorUI(itemAsset.rarity)) + ">" + itemAsset.itemName + "</color>"), text: text, arg0: amount);
    }

    public override ISleekElement createUI(Player player)
    {
        string value = formatReward(player);
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
        SleekItemIcon sleekItemIcon = new SleekItemIcon();
        sleekItemIcon.PositionOffset_X = 5f;
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
        sleekLabel.PositionOffset_X = 10f + sleekItemIcon.SizeOffset_X;
        sleekLabel.SizeOffset_X = -15f - sleekItemIcon.SizeOffset_X;
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

    public NPCItemReward(Guid newItemGuid, ushort newID, byte newAmount, bool newShouldAutoEquip, int newSight, int newTactical, int newGrip, int newBarrel, int newMagazine, int newAmmo, EItemOrigin origin, string newText)
        : base(newText)
    {
        itemGuid = newItemGuid;
        id = newID;
        amount = newAmount;
        shouldAutoEquip = newShouldAutoEquip;
        sight = newSight;
        tactical = newTactical;
        grip = newGrip;
        barrel = newBarrel;
        magazine = newMagazine;
        ammo = newAmmo;
        this.origin = origin;
    }
}
