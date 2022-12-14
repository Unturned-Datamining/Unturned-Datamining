using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerCrafting : PlayerCaller
{
    private static readonly byte SAVEDATA_VERSION = 1;

    private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

    private static InventorySearchAmountAscendingComparator amountAscendingComparator = new InventorySearchAmountAscendingComparator();

    [Obsolete("Use the static onCraftBlueprintRequested for ease-of-use instead.")]
    public PlayerCraftingRequestHandler onCraftingRequested;

    public static PlayerCraftingRequestHandler onCraftBlueprintRequested;

    public CraftingUpdated onCraftingUpdated;

    private static readonly ServerInstanceMethod<byte, byte, byte> SendStripAttachments = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerCrafting), "ReceiveStripAttachments");

    private static readonly ClientInstanceMethod SendRefreshCrafting = ClientInstanceMethod.Get(typeof(PlayerCrafting), "ReceiveRefreshCrafting");

    private static readonly ServerInstanceMethod<ushort, byte, bool> SendCraft = ServerInstanceMethod<ushort, byte, bool>.Get(typeof(PlayerCrafting), "ReceiveCraft");

    private List<IgnoredCraftingBlueprint> ignoredBlueprints = new List<IgnoredCraftingBlueprint>();

    public bool isBlueprintBlacklisted(Blueprint blueprint)
    {
        return Level.getAsset()?.isBlueprintBlacklisted(blueprint) ?? false;
    }

    private bool stripAttachments(byte page, ItemJar jar)
    {
        ItemAsset asset = jar.GetAsset();
        if (asset != null && asset.type == EItemType.GUN && jar.item.state != null && jar.item.state.Length == 18)
        {
            if (((ItemGunAsset)asset).hasSight)
            {
                ushort num = BitConverter.ToUInt16(jar.item.state, 0);
                if (num != 0 && num != ((ItemGunAsset)asset).sightID)
                {
                    base.player.inventory.forceAddItem(new Item(num, full: false, jar.item.state[13]), auto: true);
                    jar.item.state[0] = 0;
                    jar.item.state[1] = 0;
                    jar.item.state[13] = 0;
                }
            }
            if (((ItemGunAsset)asset).hasTactical)
            {
                ushort num2 = BitConverter.ToUInt16(jar.item.state, 2);
                if (num2 != 0)
                {
                    base.player.inventory.forceAddItem(new Item(num2, full: false, jar.item.state[14]), auto: true);
                    jar.item.state[2] = 0;
                    jar.item.state[3] = 0;
                    jar.item.state[14] = 0;
                }
            }
            if (((ItemGunAsset)asset).hasGrip)
            {
                ushort num3 = BitConverter.ToUInt16(jar.item.state, 4);
                if (num3 != 0)
                {
                    base.player.inventory.forceAddItem(new Item(num3, full: false, jar.item.state[15]), auto: true);
                    jar.item.state[4] = 0;
                    jar.item.state[5] = 0;
                    jar.item.state[15] = 0;
                }
            }
            if (((ItemGunAsset)asset).hasBarrel)
            {
                ushort num4 = BitConverter.ToUInt16(jar.item.state, 6);
                if (num4 != 0)
                {
                    base.player.inventory.forceAddItem(new Item(num4, full: false, jar.item.state[16]), auto: true);
                    jar.item.state[6] = 0;
                    jar.item.state[7] = 0;
                    jar.item.state[16] = 0;
                }
            }
            if (((ItemGunAsset)asset).allowMagazineChange)
            {
                ushort num5 = BitConverter.ToUInt16(jar.item.state, 8);
                if (num5 != 0 && jar.item.state[10] > 0)
                {
                    base.player.inventory.forceAddItem(new Item(num5, jar.item.state[10], jar.item.state[17]), auto: true);
                    jar.item.state[8] = 0;
                    jar.item.state[9] = 0;
                    jar.item.state[10] = 0;
                    jar.item.state[17] = 0;
                }
            }
            return true;
        }
        return false;
    }

    public void removeItem(byte page, ItemJar jar)
    {
        base.player.inventory.removeItem(page, base.player.inventory.getIndex(page, jar.x, jar.y));
        stripAttachments(page, jar);
    }

    [Obsolete]
    public void askStripAttachments(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveStripAttachments(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askStripAttachments")]
    public void ReceiveStripAttachments(byte page, byte x, byte y)
    {
        if (page < PlayerInventory.SLOTS || page >= PlayerInventory.PAGES - 1)
        {
            return;
        }
        if (base.player.equipment.checkSelection(page, x, y))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            if (item != null && stripAttachments(page, item))
            {
                base.player.inventory.sendUpdateInvState(page, x, y, item.item.state);
            }
        }
    }

    public void sendStripAttachments(byte page, byte x, byte y)
    {
        SendStripAttachments.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
    }

    [Obsolete]
    public void tellCraft(CSteamID steamID)
    {
        ReceiveRefreshCrafting();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellCraft")]
    public void ReceiveRefreshCrafting()
    {
        if (onCraftingUpdated != null)
        {
            onCraftingUpdated();
        }
    }

    public void ServerRefreshOwnerCrafting()
    {
        SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection());
    }

    [Obsolete]
    public void askCraft(CSteamID steamID, ushort id, byte index, bool force)
    {
        ReceiveCraft(id, index, force);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askCraft")]
    public void ReceiveCraft(ushort id, byte index, bool force)
    {
        if ((Level.info != null && Level.info.configData != null && !Level.info.configData.Allow_Crafting) || base.player.equipment.isBusy)
        {
            return;
        }
        bool shouldAllow = true;
        if (onCraftBlueprintRequested != null)
        {
            onCraftBlueprintRequested(this, ref id, ref index, ref shouldAllow);
        }
        else if (onCraftingRequested != null)
        {
            onCraftingRequested(this, ref id, ref index, ref shouldAllow);
        }
        if (!shouldAllow || !(Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset) || index >= itemAsset.blueprints.Count)
        {
            return;
        }
        Blueprint blueprint = itemAsset.blueprints[index];
        if (isBlueprintBlacklisted(blueprint) || (blueprint.skill == EBlueprintSkill.REPAIR && blueprint.level > Provider.modeConfigData.Gameplay.Repair_Level_Max) || (!string.IsNullOrEmpty(blueprint.map) && !blueprint.map.Equals(Level.info.name, StringComparison.InvariantCultureIgnoreCase)) || (blueprint.tool != 0 && base.player.inventory.has(blueprint.tool) == null))
        {
            return;
        }
        if (blueprint.skill != 0)
        {
            bool flag = PowerTool.checkFires(base.transform.position, 16f);
            if ((blueprint.skill == EBlueprintSkill.CRAFT && base.player.skills.skills[2][1].level < blueprint.level) || (blueprint.skill == EBlueprintSkill.COOK && (!flag || base.player.skills.skills[2][3].level < blueprint.level)) || (blueprint.skill == EBlueprintSkill.REPAIR && base.player.skills.skills[2][7].level < blueprint.level))
            {
                return;
            }
        }
        bool flag2 = false;
        for (int i = 0; i < 64; i++)
        {
            if (!blueprint.areConditionsMet(base.player))
            {
                break;
            }
            List<InventorySearch>[] array = new List<InventorySearch>[blueprint.supplies.Length];
            for (byte b = 0; b < blueprint.supplies.Length; b = (byte)(b + 1))
            {
                BlueprintSupply blueprintSupply = blueprint.supplies[b];
                List<InventorySearch> list = base.player.inventory.search(blueprintSupply.id, findEmpty: false, findHealthy: true);
                if (list.Count == 0)
                {
                    return;
                }
                ushort num = 0;
                foreach (InventorySearch item2 in list)
                {
                    num = (ushort)(num + item2.jar.item.amount);
                }
                if (num < blueprintSupply.amount && blueprint.type != EBlueprintType.AMMO)
                {
                    return;
                }
                if (blueprint.type == EBlueprintType.AMMO)
                {
                    list.Sort(amountAscendingComparator);
                }
                else
                {
                    list.Sort(qualityAscendingComparator);
                }
                array[b] = list;
            }
            if (blueprint.type == EBlueprintType.REPAIR)
            {
                List<InventorySearch> list2 = base.player.inventory.search(itemAsset.id, findEmpty: false, findHealthy: false);
                byte b2 = byte.MaxValue;
                byte b3 = byte.MaxValue;
                for (byte b4 = 0; b4 < list2.Count; b4 = (byte)(b4 + 1))
                {
                    if (list2[b4].jar.item.quality < b2)
                    {
                        b2 = list2[b4].jar.item.quality;
                        b3 = b4;
                    }
                }
                if (b3 == byte.MaxValue)
                {
                    break;
                }
                InventorySearch inventorySearch = list2[b3];
                if (base.player.equipment.checkSelection(inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y))
                {
                    base.player.equipment.dequip();
                }
                for (byte b5 = 0; b5 < array.Length; b5 = (byte)(b5 + 1))
                {
                    BlueprintSupply blueprintSupply2 = blueprint.supplies[b5];
                    List<InventorySearch> list3 = array[b5];
                    for (byte b6 = 0; b6 < blueprintSupply2.amount; b6 = (byte)(b6 + 1))
                    {
                        InventorySearch inventorySearch2 = list3[b6];
                        if (base.player.equipment.checkSelection(inventorySearch2.page, inventorySearch2.jar.x, inventorySearch2.jar.y))
                        {
                            base.player.equipment.dequip();
                        }
                        removeItem(inventorySearch2.page, inventorySearch2.jar);
                        if (inventorySearch2.page < PlayerInventory.SLOTS)
                        {
                            base.player.equipment.sendSlot(inventorySearch2.page);
                        }
                    }
                }
                base.player.inventory.sendUpdateQuality(inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y, 100);
                if (itemAsset.type == EItemType.REFILL && inventorySearch.jar.item.state.Length == 1 && inventorySearch.jar.item.state[0] == 3)
                {
                    inventorySearch.jar.item.state[0] = 1;
                    base.player.inventory.sendUpdateInvState(inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y, inventorySearch.jar.item.state);
                }
                SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection());
            }
            else if (blueprint.type == EBlueprintType.AMMO)
            {
                List<InventorySearch> list4 = base.player.inventory.search(itemAsset.id, findEmpty: true, findHealthy: true);
                int num2 = -1;
                byte b7 = byte.MaxValue;
                for (byte b8 = 0; b8 < list4.Count; b8 = (byte)(b8 + 1))
                {
                    if (list4[b8].jar.item.amount > num2 && list4[b8].jar.item.amount < itemAsset.amount)
                    {
                        num2 = list4[b8].jar.item.amount;
                        b7 = b8;
                    }
                }
                if (b7 == byte.MaxValue)
                {
                    break;
                }
                InventorySearch inventorySearch3 = list4[b7];
                int num3 = itemAsset.amount - num2;
                if (base.player.equipment.checkSelection(inventorySearch3.page, inventorySearch3.jar.x, inventorySearch3.jar.y))
                {
                    base.player.equipment.dequip();
                }
                List<InventorySearch> list5 = array[0];
                for (byte b9 = 0; b9 < list5.Count; b9 = (byte)(b9 + 1))
                {
                    InventorySearch inventorySearch4 = list5[b9];
                    if (inventorySearch4.jar != inventorySearch3.jar)
                    {
                        if (base.player.equipment.checkSelection(inventorySearch4.page, inventorySearch4.jar.x, inventorySearch4.jar.y))
                        {
                            base.player.equipment.dequip();
                        }
                        if (inventorySearch4.jar.item.amount > num3)
                        {
                            base.player.inventory.sendUpdateAmount(inventorySearch4.page, inventorySearch4.jar.x, inventorySearch4.jar.y, (byte)(inventorySearch4.jar.item.amount - num3));
                            num3 = 0;
                            break;
                        }
                        num3 -= inventorySearch4.jar.item.amount;
                        base.player.inventory.sendUpdateAmount(inventorySearch4.page, inventorySearch4.jar.x, inventorySearch4.jar.y, 0);
                        Asset asset = inventorySearch4.GetAsset();
                        if (asset == null || asset is ItemSupplyAsset || !(asset is ItemMagazineAsset itemMagazineAsset) || itemMagazineAsset.deleteEmpty)
                        {
                            removeItem(inventorySearch4.page, inventorySearch4.jar);
                            if (inventorySearch4.page < PlayerInventory.SLOTS)
                            {
                                base.player.equipment.sendSlot(inventorySearch4.page);
                            }
                        }
                        if (num3 == 0)
                        {
                            break;
                        }
                    }
                }
                base.player.inventory.sendUpdateAmount(inventorySearch3.page, inventorySearch3.jar.x, inventorySearch3.jar.y, (byte)(itemAsset.amount - num3));
                SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection());
            }
            else
            {
                for (byte b10 = 0; b10 < array.Length; b10 = (byte)(b10 + 1))
                {
                    BlueprintSupply blueprintSupply3 = blueprint.supplies[b10];
                    List<InventorySearch> list6 = array[b10];
                    for (byte b11 = 0; b11 < blueprintSupply3.amount; b11 = (byte)(b11 + 1))
                    {
                        InventorySearch inventorySearch5 = list6[b11];
                        if (base.player.equipment.checkSelection(inventorySearch5.page, inventorySearch5.jar.x, inventorySearch5.jar.y))
                        {
                            base.player.equipment.dequip();
                        }
                        removeItem(inventorySearch5.page, inventorySearch5.jar);
                        if (inventorySearch5.page < PlayerInventory.SLOTS)
                        {
                            base.player.equipment.sendSlot(inventorySearch5.page);
                        }
                    }
                }
                for (byte b12 = 0; b12 < blueprint.outputs.Length; b12 = (byte)(b12 + 1))
                {
                    BlueprintOutput blueprintOutput = blueprint.outputs[b12];
                    ItemAsset itemAsset2 = Assets.find(EAssetType.ITEM, blueprintOutput.id) as ItemAsset;
                    for (byte b13 = 0; b13 < blueprintOutput.amount; b13 = (byte)(b13 + 1))
                    {
                        if (blueprint.transferState)
                        {
                            Item item = new Item(blueprintOutput.id, array[0][0].jar.item.amount, array[0][0].jar.item.quality, array[0][0].jar.item.state);
                            if (itemAsset.type == EItemType.GUN && itemAsset2 != null && itemAsset2.type == EItemType.GUN && item.state.Length >= 12 && itemAsset2 is ItemGunAsset itemGunAsset)
                            {
                                item.state[11] = (byte)itemGunAsset.firemode;
                            }
                            base.player.inventory.forceAddItem(item, auto: true);
                        }
                        else
                        {
                            base.player.inventory.forceAddItem(new Item(blueprintOutput.id, blueprintOutput.origin), auto: true);
                        }
                    }
                }
                blueprint.applyConditions(base.player, shouldSend: true);
                blueprint.grantRewards(base.player, shouldSend: true);
                SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection());
            }
            if (!flag2)
            {
                flag2 = true;
                base.player.sendStat(EPlayerStat.FOUND_CRAFTS);
                EffectAsset effectAsset = blueprint.FindBuildEffectAsset();
                if (effectAsset != null)
                {
                    TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                    parameters.position = base.transform.position;
                    parameters.relevantDistance = EffectManager.SMALL;
                    EffectManager.triggerEffect(parameters);
                    if (Provider.isServer)
                    {
                        AlertTool.alert(base.transform.position, 8f);
                    }
                }
            }
            if (!force || blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO)
            {
                break;
            }
        }
    }

    public void sendCraft(ushort id, byte index, bool force)
    {
        SendCraft.Invoke(GetNetId(), ENetReliability.Unreliable, id, index, force);
    }

    public bool getIgnoringBlueprint(Blueprint blueprint)
    {
        if (blueprint == null)
        {
            return false;
        }
        foreach (IgnoredCraftingBlueprint ignoredBlueprint in ignoredBlueprints)
        {
            if (ignoredBlueprint.matchesBlueprint(blueprint))
            {
                return true;
            }
        }
        return false;
    }

    public void setIgnoringBlueprint(Blueprint blueprint, bool isIgnoring)
    {
        if (blueprint == null)
        {
            return;
        }
        for (int num = ignoredBlueprints.Count - 1; num >= 0; num--)
        {
            if (ignoredBlueprints[num].matchesBlueprint(blueprint))
            {
                if (!isIgnoring)
                {
                    ignoredBlueprints.RemoveAtFast(num);
                }
                return;
            }
        }
        IgnoredCraftingBlueprint ignoredCraftingBlueprint = new IgnoredCraftingBlueprint();
        ignoredCraftingBlueprint.itemId = blueprint.sourceItem.id;
        ignoredCraftingBlueprint.blueprintIndex = blueprint.id;
        ignoredBlueprints.Add(ignoredCraftingBlueprint);
    }

    internal void InitializePlayer()
    {
        if (base.channel.isOwner)
        {
            load();
        }
    }

    private void OnDestroy()
    {
        if (base.channel.isOwner)
        {
            save();
        }
    }

    private void load()
    {
        if (!ReadWrite.fileExists("/Cloud/Ignored_Blueprints.dat", useCloud: false))
        {
            return;
        }
        Block block = ReadWrite.readBlock("/Cloud/Ignored_Blueprints.dat", useCloud: false, 0);
        block.readByte();
        int a = block.readInt32();
        a = Mathf.Min(a, 10000);
        ignoredBlueprints.Capacity = ignoredBlueprints.Count + a;
        for (int i = 0; i < a; i++)
        {
            ushort num = block.readUInt16();
            byte blueprintIndex = block.readByte();
            if (num != 0)
            {
                IgnoredCraftingBlueprint ignoredCraftingBlueprint = new IgnoredCraftingBlueprint();
                ignoredCraftingBlueprint.itemId = num;
                ignoredCraftingBlueprint.blueprintIndex = blueprintIndex;
                ignoredBlueprints.Add(ignoredCraftingBlueprint);
            }
        }
    }

    private void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeInt32(ignoredBlueprints.Count);
        foreach (IgnoredCraftingBlueprint ignoredBlueprint in ignoredBlueprints)
        {
            block.writeUInt16(ignoredBlueprint.itemId);
            block.writeByte(ignoredBlueprint.blueprintIndex);
        }
        ReadWrite.writeBlock("/Cloud/Ignored_Blueprints.dat", useCloud: false, block);
    }
}
