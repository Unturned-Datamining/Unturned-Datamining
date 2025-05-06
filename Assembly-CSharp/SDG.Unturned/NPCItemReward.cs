using System;
using UnityEngine;

namespace SDG.Unturned;

public class NPCItemReward : INPCReward
{
    private CachingBcAssetRef _itemAssetRef;

    public CachingBcAssetRef ItemAssetRef => _itemAssetRef;

    public byte amount { get; protected set; }

    public bool shouldAutoEquip { get; protected set; }

    public int sight { get; protected set; }

    public int tactical { get; protected set; }

    public int grip { get; protected set; }

    public int barrel { get; protected set; }

    public int magazine { get; protected set; }

    public int ammo { get; protected set; }

    public EItemOrigin origin { get; protected set; }

    [Obsolete]
    public Guid itemGuid => ItemAssetRef.Guid;

    [Obsolete]
    public ushort id => ItemAssetRef.LegacyId;

    public ItemAsset GetItemAsset()
    {
        return _itemAssetRef.Get<ItemAsset>();
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
                    ushort num5 = ((magazine > -1) ? MathfEx.ClampToUShort(magazine) : itemGunAsset.GetDefaultMagazineLegacyId());
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

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (!p.data.TryParseBcAssetRef("ID", EAssetType.ITEM, out _itemAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseUInt8("Amount", out var value))
        {
            amount = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Amount");
        }
        shouldAutoEquip = p.data.ParseBool("Auto_Equip");
        origin = p.data.ParseEnum("Origin", EItemOrigin.CRAFT);
        sight = p.data.ParseInt32("Sight", -1);
        tactical = p.data.ParseInt32("Tactical", -1);
        grip = p.data.ParseInt32("Grip", -1);
        barrel = p.data.ParseInt32("Barrel", -1);
        magazine = p.data.ParseInt32("Magazine", -1);
        ammo = p.data.ParseInt32("Ammo", -1);
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.ITEM, out _itemAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseUInt8(p.legacyPrefix + "_Amount", out var value))
        {
            amount = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Amount");
        }
        shouldAutoEquip = p.data.ParseBool(p.legacyPrefix + "_Auto_Equip");
        origin = p.data.ParseEnum(p.legacyPrefix + "_Origin", EItemOrigin.CRAFT);
        sight = p.data.ParseInt32(p.legacyPrefix + "_Sight", -1);
        tactical = p.data.ParseInt32(p.legacyPrefix + "_Tactical", -1);
        grip = p.data.ParseInt32(p.legacyPrefix + "_Grip", -1);
        barrel = p.data.ParseInt32(p.legacyPrefix + "_Barrel", -1);
        magazine = p.data.ParseInt32(p.legacyPrefix + "_Magazine", -1);
        ammo = p.data.ParseInt32(p.legacyPrefix + "_Ammo", -1);
    }

    public NPCItemReward()
    {
    }

    [Obsolete]
    public NPCItemReward(Guid newItemGuid, ushort newID, byte newAmount, bool newShouldAutoEquip, int newSight, int newTactical, int newGrip, int newBarrel, int newMagazine, int newAmmo, EItemOrigin origin, string newText)
        : base(newText)
    {
        _itemAssetRef = new CachingBcAssetRef(newItemGuid, EAssetType.ITEM, newID);
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
