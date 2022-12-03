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

    public ushort sight { get; protected set; }

    public ushort tactical { get; protected set; }

    public ushort grip { get; protected set; }

    public ushort barrel { get; protected set; }

    public ushort magazine { get; protected set; }

    public byte ammo { get; protected set; }

    public ItemAsset GetItemAsset()
    {
        return Assets.FindItemByGuidOrLegacyId<ItemAsset>(itemGuid, id);
    }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (!Provider.isServer)
        {
            return;
        }
        ItemAsset itemAsset = GetItemAsset();
        if (itemAsset == null)
        {
            return;
        }
        for (byte b = 0; b < amount; b = (byte)(b + 1))
        {
            Item item;
            if (sight > 0 || tactical > 0 || grip > 0 || barrel > 0 || magazine > 0)
            {
                ItemGunAsset itemGunAsset = itemAsset as ItemGunAsset;
                byte[] state = itemGunAsset.getState((sight > 0) ? sight : itemGunAsset.sightID, (tactical > 0) ? tactical : itemGunAsset.tacticalID, (grip > 0) ? grip : itemGunAsset.gripID, (barrel > 0) ? barrel : itemGunAsset.barrelID, (magazine > 0) ? magazine : itemGunAsset.getMagazineID(), (ammo > 0) ? ammo : itemGunAsset.ammoMax);
                item = new Item(itemAsset.id, 1, 100, state);
            }
            else
            {
                item = new Item(itemAsset.id, EItemOrigin.CRAFT);
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
        return string.Format(arg1: (itemAsset == null) ? "?" : ("<color=" + Palette.hex(ItemTool.getRarityColorUI(itemAsset.rarity)) + ">" + itemAsset.itemName + "</color>"), format: text, arg0: amount);
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
            sleekBox.sizeOffset_Y = itemAsset.size_y * 50 + 10;
        }
        else
        {
            sleekBox.sizeOffset_Y = itemAsset.size_y * 25 + 10;
        }
        sleekBox.sizeScale_X = 1f;
        SleekItemIcon sleekItemIcon = new SleekItemIcon();
        sleekItemIcon.positionOffset_X = 5;
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
        sleekLabel.positionOffset_X = 10 + sleekItemIcon.sizeOffset_X;
        sleekLabel.sizeOffset_X = -15 - sleekItemIcon.sizeOffset_X;
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

    public NPCItemReward(Guid newItemGuid, ushort newID, byte newAmount, bool newShouldAutoEquip, ushort newSight, ushort newTactical, ushort newGrip, ushort newBarrel, ushort newMagazine, byte newAmmo, string newText)
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
    }
}
