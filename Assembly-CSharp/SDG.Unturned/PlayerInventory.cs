using System;
using System.Collections.Generic;
using System.Diagnostics;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerInventory : PlayerCaller
{
    public static readonly ushort[] LOADOUT = new ushort[0];

    public static readonly ushort[] HORDE = new ushort[4] { 97, 98, 98, 98 };

    public static readonly ushort[][] SKILLSETS_SERVER = new ushort[11][]
    {
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0],
        new ushort[0]
    };

    public static readonly ushort[][] SKILLSETS_CLIENT = new ushort[11][]
    {
        new ushort[2] { 180, 214 },
        new ushort[3] { 233, 234, 241 },
        new ushort[3] { 223, 224, 225 },
        new ushort[2] { 1171, 1172 },
        new ushort[3] { 242, 243, 244 },
        new ushort[3] { 510, 511, 509 },
        new ushort[2] { 211, 213 },
        new ushort[3] { 232, 2, 240 },
        new ushort[3] { 230, 231, 239 },
        new ushort[2] { 1156, 1157 },
        new ushort[2] { 311, 312 }
    };

    public static readonly ushort[][] SKILLSETS_HERO = new ushort[11][]
    {
        new ushort[2] { 180, 214 },
        new ushort[4] { 233, 234, 241, 104 },
        new ushort[6] { 223, 224, 225, 10, 112, 99 },
        new ushort[6] { 1171, 1172, 1169, 334, 297, 1027 },
        new ushort[5] { 242, 243, 244, 101, 1034 },
        new ushort[3] { 510, 511, 509 },
        new ushort[3] { 211, 213, 16 },
        new ushort[4] { 232, 2, 240, 138 },
        new ushort[4] { 230, 231, 239, 137 },
        new ushort[5] { 1156, 1157, 434, 122, 1036 },
        new ushort[2] { 311, 312 }
    };

    public static readonly byte SAVEDATA_VERSION = 5;

    public static readonly byte SLOTS = 2;

    public static readonly byte PAGES = 9;

    public static readonly byte BACKPACK = 3;

    public static readonly byte VEST = 4;

    public static readonly byte SHIRT = 5;

    public static readonly byte PANTS = 6;

    public static readonly byte STORAGE = 7;

    public static readonly byte AREA = 8;

    public static ushort[] loadout = LOADOUT;

    public static ushort[][] skillsets = SKILLSETS_SERVER;

    protected int receivedUpdateIndex;

    public bool isStoring;

    public bool isStorageTrunk;

    public InteractableStorage storage;

    private bool ownerHasInventory;

    public InventoryResized onInventoryResized;

    public InventoryUpdated onInventoryUpdated;

    public InventoryAdded onInventoryAdded;

    public InventoryRemoved onInventoryRemoved;

    public InventoryStored onInventoryStored;

    public InventoryStateUpdated onInventoryStateUpdated;

    public DropItemRequestHandler onDropItemRequested;

    private static readonly ServerInstanceMethod<byte, byte, byte, byte, byte, byte, byte> SendDragItem = ServerInstanceMethod<byte, byte, byte, byte, byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveDragItem");

    private static readonly ServerInstanceMethod<byte, byte, byte, byte, byte, byte, byte, byte> SendSwapItem = ServerInstanceMethod<byte, byte, byte, byte, byte, byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveSwapItem");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendDropItem = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveDropItem");

    private static readonly ClientInstanceMethod<byte, byte, byte> SendUpdateAmount = ClientInstanceMethod<byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveUpdateAmount");

    private static readonly ClientInstanceMethod<byte, byte, byte> SendUpdateQuality = ClientInstanceMethod<byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveUpdateQuality");

    private static readonly ClientInstanceMethod<byte, byte, byte[]> SendUpdateInvState = ClientInstanceMethod<byte, byte, byte[]>.Get(typeof(PlayerInventory), "ReceiveUpdateInvState");

    private static readonly ClientInstanceMethod<byte, byte, byte, byte, ushort, byte, byte, byte[]> SendItemAdd = ClientInstanceMethod<byte, byte, byte, byte, ushort, byte, byte, byte[]>.Get(typeof(PlayerInventory), "ReceiveItemAdd");

    private static readonly ClientInstanceMethod<byte, byte, byte> SendItemRemove = ClientInstanceMethod<byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveItemRemove");

    private static readonly ClientInstanceMethod<byte, byte, byte> SendSize = ClientInstanceMethod<byte, byte, byte>.Get(typeof(PlayerInventory), "ReceiveSize");

    private static readonly ClientInstanceMethod SendStoraging = ClientInstanceMethod.Get(typeof(PlayerInventory), "ReceiveStoraging");

    private static readonly ClientInstanceMethod SendInventory = ClientInstanceMethod.Get(typeof(PlayerInventory), "ReceiveInventory");

    private bool wasLoadCalled;

    public Items[] items { get; private set; }

    public bool shouldInventoryStopGestureCloseStorage => !isStorageTrunk;

    public bool shouldInteractCloseStorage => !isStorageTrunk;

    public bool shouldStorageOpenDashboard => !isStorageTrunk;

    protected void incrementUpdateIndex()
    {
        receivedUpdateIndex++;
    }

    public bool doesSearchNeedRefresh(ref int index)
    {
        if (index == receivedUpdateIndex)
        {
            return false;
        }
        index = receivedUpdateIndex;
        return true;
    }

    public byte getWidth(byte page)
    {
        if (page < items.Length)
        {
            return items[page].width;
        }
        return 0;
    }

    public byte getHeight(byte page)
    {
        if (page < items.Length)
        {
            return items[page].height;
        }
        return 0;
    }

    public byte getItemCount(byte page)
    {
        if (page < items.Length)
        {
            return items[page].getItemCount();
        }
        return 0;
    }

    public ItemJar getItem(byte page, byte index)
    {
        if (page < items.Length)
        {
            return items[page].getItem(index);
        }
        return null;
    }

    public byte getIndex(byte page, byte x, byte y)
    {
        if (page < items.Length)
        {
            return items[page].getIndex(x, y);
        }
        return byte.MaxValue;
    }

    public byte findIndex(byte page, byte x, byte y, out byte find_x, out byte find_y)
    {
        find_x = byte.MaxValue;
        find_y = byte.MaxValue;
        return items[page].findIndex(x, y, out find_x, out find_y);
    }

    public void updateAmount(byte page, byte index, byte newAmount)
    {
        if (page < PAGES && items != null && items[page] != null)
        {
            items[page].updateAmount(index, newAmount);
        }
    }

    public void updateQuality(byte page, byte index, byte newQuality)
    {
        if (page < PAGES && items != null && items[page] != null)
        {
            items[page].updateQuality(index, newQuality);
            ItemJar item = items[page].getItem(index);
            if (item != null && base.player.equipment.checkSelection(page, item.x, item.y))
            {
                base.player.equipment.quality = newQuality;
            }
        }
    }

    public void updateState(byte page, byte index, byte[] newState)
    {
        if (page < PAGES && items != null && items[page] != null)
        {
            items[page].updateState(index, newState);
        }
    }

    public List<InventorySearch> search(EItemType type)
    {
        List<InventorySearch> result = new List<InventorySearch>();
        search(result, type);
        return result;
    }

    public void search(List<InventorySearch> search, EItemType type)
    {
        for (byte b = SLOTS; b < PAGES - 2; b = (byte)(b + 1))
        {
            items[b].search(search, type);
        }
    }

    [Obsolete]
    public List<InventorySearch> search(EItemType type, ushort[] calibers)
    {
        return search(type, calibers, allowZeroCaliber: true);
    }

    public List<InventorySearch> search(EItemType type, ushort[] calibers, bool allowZeroCaliber)
    {
        List<InventorySearch> result = new List<InventorySearch>();
        foreach (ushort caliber in calibers)
        {
            search(result, type, caliber, allowZeroCaliber);
        }
        return result;
    }

    [Obsolete]
    public void search(List<InventorySearch> search, EItemType type, ushort caliber)
    {
        this.search(search, type, caliber, allowZeroCaliber: true);
    }

    public void search(List<InventorySearch> search, EItemType type, ushort caliber, bool allowZeroCaliber)
    {
        for (byte b = SLOTS; b < PAGES - 2; b = (byte)(b + 1))
        {
            items[b].search(search, type, caliber, allowZeroCaliber);
        }
    }

    public List<InventorySearch> search(ushort id, bool findEmpty, bool findHealthy)
    {
        List<InventorySearch> result = new List<InventorySearch>();
        search(result, id, findEmpty, findHealthy);
        return result;
    }

    public void search(List<InventorySearch> search, ushort id, bool findEmpty, bool findHealthy)
    {
        for (byte b = SLOTS; b < PAGES - 2; b = (byte)(b + 1))
        {
            items[b].search(search, id, findEmpty, findHealthy);
        }
    }

    public List<InventorySearch> search(List<InventorySearch> search)
    {
        List<InventorySearch> list = new List<InventorySearch>();
        for (int i = 0; i < search.Count; i++)
        {
            InventorySearch inventorySearch = search[i];
            bool flag = true;
            for (int j = 0; j < list.Count; j++)
            {
                InventorySearch inventorySearch2 = list[j];
                if (inventorySearch2.jar.item.id == inventorySearch.jar.item.id && inventorySearch2.jar.item.amount == inventorySearch.jar.item.amount && inventorySearch2.jar.item.quality == inventorySearch.jar.item.quality)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                list.Add(inventorySearch);
            }
        }
        return list;
    }

    public InventorySearch has(ushort id)
    {
        for (byte b = 0; b < PAGES - 1; b = (byte)(b + 1))
        {
            InventorySearch inventorySearch = items[b].has(id);
            if (inventorySearch != null)
            {
                return inventorySearch;
            }
        }
        return null;
    }

    public bool tryAddItem(Item item, byte x, byte y, byte page, byte rot)
    {
        if (page >= PAGES - 1)
        {
            return false;
        }
        if (item == null)
        {
            return false;
        }
        ItemAsset asset = item.GetAsset();
        if (asset == null || asset.isPro)
        {
            return false;
        }
        if (page < SLOTS && !asset.slot.canEquipInPage(page))
        {
            return false;
        }
        if (page < SLOTS)
        {
            rot = 0;
        }
        if (x == byte.MaxValue && y == byte.MaxValue)
        {
            if (!items[page].tryAddItem(item))
            {
                return false;
            }
        }
        else
        {
            if (items[page].getItemCount() >= 200)
            {
                return false;
            }
            if (!items[page].checkSpaceEmpty(x, y, asset.size_x, asset.size_y, rot))
            {
                return false;
            }
            items[page].addItem(x, y, rot, item);
        }
        if (page < SLOTS)
        {
            base.player.equipment.sendSlot(page);
        }
        return true;
    }

    public bool tryAddItem(Item item, bool auto)
    {
        return tryAddItem(item, auto, playEffect: true);
    }

    public bool tryAddItem(Item item, bool auto, bool playEffect)
    {
        return tryAddItemAuto(item, auto, auto, auto, playEffect);
    }

    private bool tryAddItemEquip(Item item, byte page)
    {
        if (items[page].tryAddItem(item))
        {
            base.player.equipment.sendSlot(page);
            if (!base.player.equipment.HasValidUseable)
            {
                base.player.equipment.ServerEquip(page, 0, 0);
            }
            return true;
        }
        return false;
    }

    public bool tryAddItemAuto(Item item, bool autoEquipWeapon, bool autoEquipUseable, bool autoEquipClothing, bool playEffect)
    {
        if (item == null)
        {
            return false;
        }
        ItemAsset asset = item.GetAsset();
        if (asset == null || asset.isPro)
        {
            return false;
        }
        if (autoEquipWeapon && asset.canPlayerEquip)
        {
            if (asset.slot.canEquipAsSecondary() && tryAddItemEquip(item, 1))
            {
                return true;
            }
            if (asset.slot.canEquipAsPrimary() && tryAddItemEquip(item, 0))
            {
                return true;
            }
        }
        if (autoEquipClothing)
        {
            if (base.player.clothing.hatAsset == null && asset.type == EItemType.HAT)
            {
                base.player.clothing.askWearHat(item.id, item.quality, item.state, playEffect);
                return true;
            }
            if (base.player.clothing.shirtAsset == null && asset.type == EItemType.SHIRT)
            {
                base.player.clothing.askWearShirt(item.id, item.quality, item.state, playEffect);
                return true;
            }
            if (base.player.clothing.pantsAsset == null && asset.type == EItemType.PANTS)
            {
                base.player.clothing.askWearPants(item.id, item.quality, item.state, playEffect);
                return true;
            }
            if (base.player.clothing.backpackAsset == null && asset.type == EItemType.BACKPACK)
            {
                base.player.clothing.askWearBackpack(item.id, item.quality, item.state, playEffect);
                return true;
            }
            if (base.player.clothing.vestAsset == null && asset.type == EItemType.VEST)
            {
                base.player.clothing.askWearVest(item.id, item.quality, item.state, playEffect);
                return true;
            }
            if (base.player.clothing.maskAsset == null && asset.type == EItemType.MASK)
            {
                base.player.clothing.askWearMask(item.id, item.quality, item.state, playEffect);
                return true;
            }
            if (base.player.clothing.glassesAsset == null && asset.type == EItemType.GLASSES)
            {
                base.player.clothing.askWearGlasses(item.id, item.quality, item.state, playEffect);
                return true;
            }
        }
        for (byte b = SLOTS; b < PAGES - 2; b = (byte)(b + 1))
        {
            if (items[b].tryAddItem(item))
            {
                if (autoEquipUseable && !base.player.equipment.HasValidUseable && asset.slot.canEquipInPage(b) && asset.canPlayerEquip)
                {
                    ItemJar item2 = items[b].getItem((byte)(items[b].getItemCount() - 1));
                    base.player.equipment.ServerEquip(b, item2.x, item2.y);
                }
                return true;
            }
        }
        return false;
    }

    public void forceAddItem(Item item, bool auto)
    {
        forceAddItemAuto(item, auto, auto, auto);
    }

    public void forceAddItemAuto(Item item, bool autoEquipWeapon, bool autoEquipUseable, bool autoEquipClothing)
    {
        forceAddItemAuto(item, autoEquipWeapon, autoEquipUseable, autoEquipClothing, playEffect: true);
    }

    public void forceAddItem(Item item, bool auto, bool playEffect)
    {
        if (!tryAddItemAuto(item, auto, auto, auto, playEffect))
        {
            ItemManager.dropItem(item, base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
    }

    public void forceAddItemAuto(Item item, bool autoEquipWeapon, bool autoEquipUseable, bool autoEquipClothing, bool playEffect)
    {
        if (!tryAddItemAuto(item, autoEquipWeapon, autoEquipUseable, autoEquipClothing, playEffect))
        {
            ItemManager.dropItem(item, base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
    }

    public void replaceItems(byte page, Items replacement)
    {
        items[page] = replacement;
    }

    public void removeItem(byte page, byte index)
    {
        items[page].removeItem(index);
    }

    public bool checkSpaceEmpty(byte page, byte x, byte y, byte size_x, byte size_y, byte rot)
    {
        if (page < 0 || page >= PAGES)
        {
            return false;
        }
        return items[page].checkSpaceEmpty(x, y, size_x, size_y, rot);
    }

    public bool checkSpaceDrag(byte page, byte old_x, byte old_y, byte oldRot, byte new_x, byte new_y, byte newRot, byte size_x, byte size_y, bool checkSame)
    {
        if (page < 0 || page >= PAGES)
        {
            return false;
        }
        return items[page].checkSpaceDrag(old_x, old_y, oldRot, new_x, new_y, newRot, size_x, size_y, checkSame);
    }

    public bool checkSpaceSwap(byte page, byte x, byte y, byte oldSize_X, byte oldSize_Y, byte oldRot, byte newSize_X, byte newSize_Y, byte newRot)
    {
        if (page < 0 || page >= PAGES)
        {
            return false;
        }
        return items[page].checkSpaceSwap(x, y, oldSize_X, oldSize_Y, oldRot, newSize_X, newSize_Y, newRot);
    }

    public bool tryFindSpace(byte page, byte size_x, byte size_y, out byte x, out byte y, out byte rot)
    {
        x = 0;
        y = 0;
        rot = 0;
        if (page < 0 || page >= PAGES)
        {
            return false;
        }
        return items[page].tryFindSpace(size_x, size_y, out x, out y, out rot);
    }

    public bool tryFindSpace(byte size_x, byte size_y, out byte page, out byte x, out byte y, out byte rot)
    {
        x = 0;
        y = 0;
        rot = 0;
        for (page = SLOTS; page < PAGES - 1; page++)
        {
            if (items[page].tryFindSpace(size_x, size_y, out x, out y, out rot))
            {
                return true;
            }
        }
        return false;
    }

    [Obsolete]
    public void askDragItem(CSteamID steamID, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        ReceiveDragItem(page_0, x_0, y_0, page_1, x_1, y_1, rot_1);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askDragItem")]
    public void ReceiveDragItem(byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        if (base.player.equipment.checkSelection(page_0, x_0, y_0))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        else if (base.player.equipment.checkSelection(page_1, x_1, y_1))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if (page_0 < 0 || page_0 >= PAGES - 1 || items[page_0] == null)
        {
            return;
        }
        byte index = items[page_0].getIndex(x_0, y_0);
        if (index == byte.MaxValue || page_1 < 0 || page_1 >= PAGES - 1 || items[page_1] == null || getItemCount(page_1) >= 200)
        {
            return;
        }
        ItemJar item = items[page_0].getItem(index);
        if (item == null || !checkSpaceDrag(page_1, x_0, y_0, item.rot, x_1, y_1, rot_1, item.size_x, item.size_y, page_0 == page_1))
        {
            return;
        }
        ItemAsset asset = item.GetAsset();
        if (asset != null && (page_1 >= SLOTS || asset.slot.canEquipInPage(page_1)))
        {
            if (page_1 < SLOTS)
            {
                rot_1 = 0;
            }
            removeItem(page_0, index);
            items[page_1].addItem(x_1, y_1, rot_1, item.item);
            if (page_0 < SLOTS)
            {
                base.player.equipment.sendSlot(page_0);
            }
            if (page_1 < SLOTS)
            {
                base.player.equipment.sendSlot(page_1);
            }
        }
    }

    [Obsolete]
    public void askSwapItem(CSteamID steamID, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        ReceiveSwapItem(page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askSwapItem")]
    public void ReceiveSwapItem(byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        if (base.player.equipment.checkSelection(page_0, x_0, y_0))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        else if (base.player.equipment.checkSelection(page_1, x_1, y_1))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if ((page_0 == page_1 && x_0 == x_1 && y_0 == y_1 && rot_0 == rot_1) || page_0 < 0 || page_0 >= PAGES - 1 || items[page_0] == null)
        {
            return;
        }
        byte index = items[page_0].getIndex(x_0, y_0);
        if (index == byte.MaxValue || page_1 < 0 || page_1 >= PAGES - 1 || items[page_1] == null)
        {
            return;
        }
        byte b = items[page_1].getIndex(x_1, y_1);
        if (b == byte.MaxValue)
        {
            return;
        }
        ItemJar item = items[page_0].getItem(index);
        if (item == null)
        {
            return;
        }
        ItemJar item2 = items[page_1].getItem(b);
        if (item2 == null || item == item2 || !checkSpaceSwap(page_0, x_0, y_0, item.size_x, item.size_y, item.rot, item2.size_x, item2.size_y, rot_0) || !checkSpaceSwap(page_1, x_1, y_1, item2.size_x, item2.size_y, item2.rot, item.size_x, item.size_y, rot_1))
        {
            return;
        }
        ItemAsset asset = item.GetAsset();
        if (asset == null || (page_1 < SLOTS && !asset.slot.canEquipInPage(page_1)))
        {
            return;
        }
        ItemAsset asset2 = item2.GetAsset();
        if (asset2 != null && (page_0 >= SLOTS || asset2.slot.canEquipInPage(page_0)))
        {
            removeItem(page_0, index);
            if (page_0 == page_1 && b > index)
            {
                b = (byte)(b - 1);
            }
            removeItem(page_1, b);
            if (page_0 < SLOTS)
            {
                rot_0 = 0;
            }
            if (page_1 < SLOTS)
            {
                rot_1 = 0;
            }
            items[page_0].addItem(x_0, y_0, rot_0, item2.item);
            items[page_1].addItem(x_1, y_1, rot_1, item.item);
            if (page_0 < SLOTS)
            {
                base.player.equipment.sendSlot(page_0);
            }
            if (page_1 < SLOTS)
            {
                base.player.equipment.sendSlot(page_1);
            }
        }
    }

    public void sendDragItem(byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        SendDragItem.Invoke(GetNetId(), ENetReliability.Unreliable, page_0, x_0, y_0, page_1, x_1, y_1, rot_1);
    }

    public void sendSwapItem(byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        SendSwapItem.Invoke(GetNetId(), ENetReliability.Unreliable, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1);
    }

    [Obsolete]
    public void askDropItem(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveDropItem(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askDropItem")]
    public void ReceiveDropItem(byte page, byte x, byte y)
    {
        if (base.player.equipment.checkSelection(page, x, y))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if (page < 0 || page >= PAGES - 1 || items[page] == null || items == null)
        {
            return;
        }
        byte index = items[page].getIndex(x, y);
        if (index == byte.MaxValue)
        {
            return;
        }
        ItemJar item = items[page].getItem(index);
        if (item == null || item.item == null)
        {
            return;
        }
        ItemAsset asset = item.GetAsset();
        if (asset == null)
        {
            return;
        }
        bool shouldAllow = asset.allowManualDrop;
        onDropItemRequested?.Invoke(this, item.item, ref shouldAllow);
        if (shouldAllow)
        {
            ItemManager.dropItem(item.item, base.transform.position + base.transform.forward * 0.5f, playEffect: true, isDropped: true, wideSpread: false);
            removeItem(page, index);
            if (page < SLOTS)
            {
                base.player.equipment.sendSlot(page);
            }
        }
    }

    public void sendDropItem(byte page, byte x, byte y)
    {
        SendDropItem.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
    }

    [Obsolete]
    public void tellUpdateAmount(CSteamID steamID, byte page, byte index, byte amount)
    {
        ReceiveUpdateAmount(page, index, amount);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateAmount")]
    public void ReceiveUpdateAmount(byte page, byte index, byte amount)
    {
        updateAmount(page, index, amount);
    }

    [Obsolete]
    public void tellUpdateQuality(CSteamID steamID, byte page, byte index, byte quality)
    {
        ReceiveUpdateQuality(page, index, quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateQuality")]
    public void ReceiveUpdateQuality(byte page, byte index, byte quality)
    {
        updateQuality(page, index, quality);
    }

    [Obsolete]
    public void tellUpdateInvState(CSteamID steamID, byte page, byte index, byte[] state)
    {
        ReceiveUpdateInvState(page, index, state);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateInvState")]
    public void ReceiveUpdateInvState(byte page, byte index, byte[] state)
    {
        updateState(page, index, state);
    }

    [Obsolete]
    public void tellItemAdd(CSteamID steamID, byte page, byte x, byte y, byte rot, ushort id, byte amount, byte quality, byte[] state)
    {
        ReceiveItemAdd(page, x, y, rot, id, amount, quality, state);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellItemAdd")]
    public void ReceiveItemAdd(byte page, byte x, byte y, byte rot, ushort id, byte amount, byte quality, byte[] state)
    {
        if (page < PAGES && items != null && items[page] != null)
        {
            items[page].addItem(x, y, rot, new Item(id, amount, quality, state));
        }
    }

    [Obsolete]
    public void tellItemRemove(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveItemRemove(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellItemRemove")]
    public void ReceiveItemRemove(byte page, byte x, byte y)
    {
        if (page < PAGES && items != null && items[page] != null)
        {
            byte index = items[page].getIndex(x, y);
            if (index != byte.MaxValue)
            {
                items[page].removeItem(index);
            }
        }
    }

    [Obsolete]
    public void tellSize(CSteamID steamID, byte page, byte newWidth, byte newHeight)
    {
        ReceiveSize(page, newWidth, newHeight);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSize")]
    public void ReceiveSize(byte page, byte newWidth, byte newHeight)
    {
        if (page < PAGES && items != null && items[page] != null)
        {
            items[page].resize(newWidth, newHeight);
        }
    }

    [Obsolete]
    public void tellStoraging(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveStoraging(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadBit(out isStorageTrunk);
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        items[STORAGE].resize(value, value2);
        reader.ReadUInt8(out var value3);
        for (byte b = 0; b < value3; b = (byte)(b + 1))
        {
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            reader.ReadUInt8(out var value6);
            reader.ReadUInt16(out var value7);
            reader.ReadUInt8(out var value8);
            reader.ReadUInt8(out var value9);
            reader.ReadUInt8(out var value10);
            byte[] array = new byte[value10];
            reader.ReadBytes(array);
            items[STORAGE].addItem(value4, value5, value6, new Item(value7, value8, value9, array));
        }
        isStoring = items[STORAGE].height > 0;
        if (isStoring)
        {
            onInventoryStored?.Invoke();
        }
    }

    [Obsolete]
    public void tellInventory(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveInventory(in ClientInvocationContext context)
    {
        Player.isLoadingInventory = false;
        NetPakReader reader = context.reader;
        for (byte b = 0; b < PAGES - 2; b = (byte)(b + 1))
        {
            reader.ReadUInt8(out var value);
            reader.ReadUInt8(out var value2);
            items[b].resize(value, value2);
            reader.ReadUInt8(out var value3);
            for (byte b2 = 0; b2 < value3; b2 = (byte)(b2 + 1))
            {
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt8(out var value6);
                reader.ReadUInt16(out var value7);
                reader.ReadUInt8(out var value8);
                reader.ReadUInt8(out var value9);
                reader.ReadUInt8(out var value10);
                byte[] array = new byte[value10];
                reader.ReadBytes(array);
                items[b].addItem(value4, value5, value6, new Item(value7, value8, value9, array));
            }
        }
    }

    [Obsolete]
    public void askInventory(CSteamID steamID)
    {
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        if (base.channel.IsLocalPlayer)
        {
            Player.isLoadingInventory = false;
            for (byte b = 0; b < PAGES - 2; b = (byte)(b + 1))
            {
                onInventoryResized?.Invoke(b, items[b].width, items[b].height);
                for (byte b2 = 0; b2 < items[b].getItemCount(); b2 = (byte)(b2 + 1))
                {
                    ItemJar item = items[b].getItem(b2);
                    onItemAdded(b, b2, item);
                }
            }
        }
        else if (client == base.channel.owner)
        {
            SendInventory.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, delegate(NetPakWriter writer)
            {
                for (byte b3 = 0; b3 < PAGES - 2; b3 = (byte)(b3 + 1))
                {
                    writer.WriteUInt8(items[b3].width);
                    writer.WriteUInt8(items[b3].height);
                    writer.WriteUInt8(items[b3].getItemCount());
                    for (byte b4 = 0; b4 < items[b3].getItemCount(); b4 = (byte)(b4 + 1))
                    {
                        ItemJar item2 = items[b3].getItem(b4);
                        writer.WriteUInt8(item2.x);
                        writer.WriteUInt8(item2.y);
                        writer.WriteUInt8(item2.rot);
                        writer.WriteUInt16(item2.item.id);
                        writer.WriteUInt8(item2.item.amount);
                        writer.WriteUInt8(item2.item.quality);
                        writer.WriteUInt8((byte)item2.item.state.Length);
                        writer.WriteBytes(item2.item.state);
                    }
                }
            });
        }
        ownerHasInventory = true;
    }

    public void sendStorage()
    {
        if (base.channel.IsLocalPlayer)
        {
            onInventoryResized(STORAGE, items[STORAGE].width, items[STORAGE].height);
            if (items[STORAGE].height > 0)
            {
                onInventoryStored?.Invoke();
            }
            for (byte b = 0; b < items[STORAGE].getItemCount(); b = (byte)(b + 1))
            {
                ItemJar item = items[STORAGE].getItem(b);
                onItemAdded(STORAGE, b, item);
            }
            return;
        }
        SendStoraging.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.owner.transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteBit(isStorageTrunk);
            writer.WriteUInt8(items[STORAGE].width);
            writer.WriteUInt8(items[STORAGE].height);
            writer.WriteUInt8(items[STORAGE].getItemCount());
            for (byte b2 = 0; b2 < items[STORAGE].getItemCount(); b2 = (byte)(b2 + 1))
            {
                ItemJar item2 = items[STORAGE].getItem(b2);
                writer.WriteUInt8(item2.x);
                writer.WriteUInt8(item2.y);
                writer.WriteUInt8(item2.rot);
                writer.WriteUInt16(item2.item.id);
                writer.WriteUInt8(item2.item.amount);
                writer.WriteUInt8(item2.item.quality);
                writer.WriteUInt8((byte)item2.item.state.Length);
                writer.WriteBytes(item2.item.state);
            }
        });
    }

    public void updateItems(byte page, Items newItems)
    {
        if (items[page] != null)
        {
            Items obj = items[page];
            obj.onItemsResized = (ItemsResized)Delegate.Remove(obj.onItemsResized, new ItemsResized(onItemsResized));
            Items obj2 = items[page];
            obj2.onItemUpdated = (ItemUpdated)Delegate.Remove(obj2.onItemUpdated, new ItemUpdated(onItemUpdated));
            Items obj3 = items[page];
            obj3.onItemAdded = (ItemAdded)Delegate.Remove(obj3.onItemAdded, new ItemAdded(onItemAdded));
            Items obj4 = items[page];
            obj4.onItemRemoved = (ItemRemoved)Delegate.Remove(obj4.onItemRemoved, new ItemRemoved(onItemRemoved));
            Items obj5 = items[page];
            obj5.onStateUpdated = (StateUpdated)Delegate.Remove(obj5.onStateUpdated, new StateUpdated(onItemStateUpdated));
        }
        if (newItems != null)
        {
            items[page] = newItems;
            Items obj6 = items[page];
            obj6.onItemsResized = (ItemsResized)Delegate.Combine(obj6.onItemsResized, new ItemsResized(onItemsResized));
            Items obj7 = items[page];
            obj7.onItemUpdated = (ItemUpdated)Delegate.Combine(obj7.onItemUpdated, new ItemUpdated(onItemUpdated));
            Items obj8 = items[page];
            obj8.onItemAdded = (ItemAdded)Delegate.Combine(obj8.onItemAdded, new ItemAdded(onItemAdded));
            Items obj9 = items[page];
            obj9.onItemRemoved = (ItemRemoved)Delegate.Combine(obj9.onItemRemoved, new ItemRemoved(onItemRemoved));
            Items obj10 = items[page];
            obj10.onStateUpdated = (StateUpdated)Delegate.Combine(obj10.onStateUpdated, new StateUpdated(onItemStateUpdated));
        }
        else
        {
            items[page] = new Items(STORAGE);
            Items obj11 = items[page];
            obj11.onItemsResized = (ItemsResized)Delegate.Combine(obj11.onItemsResized, new ItemsResized(onItemsResized));
            Items obj12 = items[page];
            obj12.onItemUpdated = (ItemUpdated)Delegate.Combine(obj12.onItemUpdated, new ItemUpdated(onItemUpdated));
            Items obj13 = items[page];
            obj13.onItemAdded = (ItemAdded)Delegate.Combine(obj13.onItemAdded, new ItemAdded(onItemAdded));
            Items obj14 = items[page];
            obj14.onItemRemoved = (ItemRemoved)Delegate.Combine(obj14.onItemRemoved, new ItemRemoved(onItemRemoved));
            Items obj15 = items[page];
            obj15.onStateUpdated = (StateUpdated)Delegate.Combine(obj15.onStateUpdated, new StateUpdated(onItemStateUpdated));
            onInventoryResized?.Invoke(page, 0, 0);
        }
    }

    public void sendUpdateAmount(byte page, byte x, byte y, byte amount)
    {
        byte index = getIndex(page, x, y);
        updateAmount(page, index, amount);
        if (!base.channel.IsLocalPlayer && ownerHasInventory)
        {
            SendUpdateAmount.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), page, index, amount);
        }
    }

    public void sendUpdateQuality(byte page, byte x, byte y, byte quality)
    {
        byte index = getIndex(page, x, y);
        updateQuality(page, index, quality);
        if (!base.channel.IsLocalPlayer && ownerHasInventory)
        {
            SendUpdateQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), page, index, quality);
        }
    }

    public void sendUpdateInvState(byte page, byte x, byte y, byte[] state)
    {
        byte index = getIndex(page, x, y);
        updateState(page, index, state);
        if (!base.channel.IsLocalPlayer && ownerHasInventory)
        {
            SendUpdateInvState.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), page, index, state);
        }
    }

    private void sendItemAdd(byte page, ItemJar jar)
    {
        SendItemAdd.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), page, jar.x, jar.y, jar.rot, jar.item.id, jar.item.amount, jar.item.quality, jar.item.state);
    }

    private void sendItemRemove(byte page, ItemJar jar)
    {
        SendItemRemove.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), page, jar.x, jar.y);
    }

    private void bestowLoadout()
    {
        if (loadout != null && loadout.Length != 0)
        {
            for (int i = 0; i < loadout.Length; i++)
            {
                tryAddItem(new Item(loadout[i], EItemOrigin.ADMIN), auto: true, playEffect: false);
            }
        }
        else if (Level.info != null)
        {
            if (skillsets != null && skillsets[(byte)base.channel.owner.skillset] != null && skillsets[(byte)base.channel.owner.skillset].Length != 0)
            {
                for (int j = 0; j < skillsets[(byte)base.channel.owner.skillset].Length; j++)
                {
                    tryAddItem(new Item(skillsets[(byte)base.channel.owner.skillset][j], EItemOrigin.WORLD), auto: true, playEffect: false);
                }
            }
            else if (Level.info.type == ELevelType.HORDE)
            {
                for (int k = 0; k < HORDE.Length; k++)
                {
                    tryAddItem(new Item(HORDE[k], EItemOrigin.ADMIN), auto: true, playEffect: false);
                }
            }
        }
        if (Level.info == null)
        {
            return;
        }
        foreach (ArenaLoadout spawn_Loadout in Level.info.configData.Spawn_Loadouts)
        {
            for (ushort num = 0; num < spawn_Loadout.Amount; num = (ushort)(num + 1))
            {
                ushort num2 = SpawnTableTool.ResolveLegacyId(spawn_Loadout.Table_ID, EAssetType.ITEM, OnGetSpawnLoadoutErrorContext);
                if (num2 != 0)
                {
                    tryAddItemAuto(new Item(num2, full: true), autoEquipWeapon: true, autoEquipUseable: false, autoEquipClothing: true, playEffect: false);
                }
            }
        }
    }

    private string OnGetSpawnLoadoutErrorContext()
    {
        return "level config spawn loadout";
    }

    private void onShirtUpdated(ushort id, byte quality, byte[] state)
    {
        if (id != 0)
        {
            if (Assets.find(EAssetType.ITEM, id) is ItemBagAsset itemBagAsset)
            {
                items[SHIRT].resize(itemBagAsset.width, itemBagAsset.height);
            }
        }
        else
        {
            items[SHIRT].resize(0, 0);
        }
    }

    private void onPantsUpdated(ushort id, byte quality, byte[] state)
    {
        if (id != 0)
        {
            if (Assets.find(EAssetType.ITEM, id) is ItemBagAsset itemBagAsset)
            {
                items[PANTS].resize(itemBagAsset.width, itemBagAsset.height);
            }
        }
        else
        {
            items[PANTS].resize(0, 0);
        }
    }

    private void onBackpackUpdated(ushort id, byte quality, byte[] state)
    {
        if (id != 0)
        {
            if (Assets.find(EAssetType.ITEM, id) is ItemBagAsset itemBagAsset)
            {
                items[BACKPACK].resize(itemBagAsset.width, itemBagAsset.height);
            }
        }
        else
        {
            items[BACKPACK].resize(0, 0);
        }
    }

    private void onVestUpdated(ushort id, byte quality, byte[] state)
    {
        if (id != 0)
        {
            if (Assets.find(EAssetType.ITEM, id) is ItemBagAsset itemBagAsset)
            {
                items[VEST].resize(itemBagAsset.width, itemBagAsset.height);
            }
        }
        else
        {
            items[VEST].resize(0, 0);
        }
    }

    public void closeDistantStorage()
    {
        if (isStoring && !isStorageTrunk && !(storage == null) && storage.shouldCloseWhenOutsideRange)
        {
            Vector3 position = storage.transform.position;
            if ((base.transform.position - position).sqrMagnitude > 400f)
            {
                closeStorage();
            }
        }
    }

    public void openStorage(InteractableStorage newStorage)
    {
        if (isStoring)
        {
            closeStorage();
        }
        newStorage.isOpen = true;
        newStorage.opener = base.player;
        isStoring = true;
        isStorageTrunk = false;
        storage = newStorage;
        updateItems(STORAGE, storage.items);
        sendStorage();
    }

    public void openTrunk(Items trunkItems)
    {
        if (isStoring)
        {
            closeStorage();
        }
        isStoring = true;
        isStorageTrunk = true;
        storage = null;
        updateItems(STORAGE, trunkItems);
        sendStorage();
    }

    public void closeTrunk()
    {
        if (isStorageTrunk)
        {
            closeStorageAndNotifyClient();
        }
    }

    public void closeStorage()
    {
        if (!isStoring)
        {
            return;
        }
        isStoring = false;
        isStorageTrunk = false;
        if (storage != null)
        {
            if (Provider.isServer)
            {
                storage.isOpen = false;
                storage.opener = null;
            }
            storage = null;
        }
        updateItems(STORAGE, null);
    }

    public void closeStorageAndNotifyClient()
    {
        if (isStoring)
        {
            closeStorage();
            sendStorage();
        }
    }

    private void onLifeUpdated(bool isDead)
    {
        if ((Provider.isServer || base.channel.IsLocalPlayer) && isDead)
        {
            closeStorage();
        }
        if (!Provider.isServer)
        {
            return;
        }
        if (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Weapons_PvP : Provider.modeConfigData.Players.Lose_Weapons_PvE)
        {
            if (isDead)
            {
                items[0].resize(0, 0);
                items[1].resize(0, 0);
            }
            else
            {
                items[0].resize(1, 1);
                items[1].resize(1, 1);
            }
        }
        if (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Clothes_PvP : Provider.modeConfigData.Players.Lose_Clothes_PvE)
        {
            if (isDead)
            {
                for (byte b = SLOTS; b < PAGES - 2; b = (byte)(b + 1))
                {
                    items[b].resize(0, 0);
                }
            }
            else
            {
                items[2].resize(5, 3);
                bestowLoadout();
            }
        }
        else
        {
            if (!isDead)
            {
                return;
            }
            float num = (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Items_PvP : Provider.modeConfigData.Players.Lose_Items_PvE);
            for (byte b2 = SLOTS; b2 < PAGES - 2; b2 = (byte)(b2 + 1))
            {
                if (items[b2].getItemCount() > 0)
                {
                    for (int num2 = items[b2].getItemCount() - 1; num2 >= 0; num2--)
                    {
                        if (UnityEngine.Random.value < num)
                        {
                            ItemManager.dropItem(items[b2].getItem((byte)num2).item, base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
                            items[b2].removeItem((byte)num2);
                        }
                    }
                }
            }
        }
    }

    private void onItemsResized(byte page, byte newWidth, byte newHeight)
    {
        if (!base.channel.IsLocalPlayer && Provider.isServer && ownerHasInventory)
        {
            SendSize.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), page, newWidth, newHeight);
        }
        onInventoryResized?.Invoke(page, newWidth, newHeight);
        incrementUpdateIndex();
    }

    private void onItemUpdated(byte page, byte index, ItemJar jar)
    {
        onInventoryUpdated?.Invoke(page, index, jar);
        incrementUpdateIndex();
    }

    private void onItemAdded(byte page, byte index, ItemJar jar)
    {
        if (!base.channel.IsLocalPlayer && Provider.isServer && ownerHasInventory)
        {
            sendItemAdd(page, jar);
        }
        onInventoryAdded?.Invoke(page, index, jar);
        incrementUpdateIndex();
    }

    private void onItemRemoved(byte page, byte index, ItemJar jar)
    {
        if (Provider.isServer)
        {
            if (!base.channel.IsLocalPlayer && ownerHasInventory)
            {
                sendItemRemove(page, jar);
            }
            if (base.player.equipment.checkSelection(page, jar.x, jar.y))
            {
                base.player.equipment.dequip();
            }
        }
        onInventoryRemoved?.Invoke(page, index, jar);
        incrementUpdateIndex();
    }

    private void onItemDiscarded(byte page, byte index, ItemJar jar)
    {
        bool flag = true;
        if (base.player.life.isDead)
        {
            ItemAsset asset = jar.GetAsset();
            if (asset == null || !asset.shouldDropOnDeath)
            {
                flag = false;
            }
        }
        if (Provider.isServer)
        {
            if (!base.channel.IsLocalPlayer && ownerHasInventory)
            {
                sendItemRemove(page, jar);
            }
            if (base.player.equipment.checkSelection(page, jar.x, jar.y))
            {
                base.player.equipment.dequip();
            }
            onInventoryRemoved?.Invoke(page, index, jar);
            if (flag)
            {
                ItemManager.dropItem(jar.item, base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
        }
        incrementUpdateIndex();
    }

    private void onItemStateUpdated()
    {
        onInventoryStateUpdated?.Invoke();
        incrementUpdateIndex();
    }

    private void OnDestroy()
    {
        closeStorage();
    }

    internal void InitializePlayer()
    {
        items = new Items[PAGES];
        for (byte b = 0; b < PAGES - 1; b = (byte)(b + 1))
        {
            items[b] = new Items(b);
            Items obj = items[b];
            obj.onItemsResized = (ItemsResized)Delegate.Combine(obj.onItemsResized, new ItemsResized(onItemsResized));
            Items obj2 = items[b];
            obj2.onItemUpdated = (ItemUpdated)Delegate.Combine(obj2.onItemUpdated, new ItemUpdated(onItemUpdated));
            Items obj3 = items[b];
            obj3.onItemAdded = (ItemAdded)Delegate.Combine(obj3.onItemAdded, new ItemAdded(onItemAdded));
            Items obj4 = items[b];
            obj4.onItemRemoved = (ItemRemoved)Delegate.Combine(obj4.onItemRemoved, new ItemRemoved(onItemRemoved));
            Items obj5 = items[b];
            obj5.onStateUpdated = (StateUpdated)Delegate.Combine(obj5.onStateUpdated, new StateUpdated(onItemStateUpdated));
        }
        if (base.channel.IsLocalPlayer || Provider.isServer)
        {
            PlayerLife life = base.player.life;
            life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(onLifeUpdated));
        }
        if (Provider.isServer)
        {
            PlayerClothing clothing = base.player.clothing;
            clothing.onShirtUpdated = (ShirtUpdated)Delegate.Combine(clothing.onShirtUpdated, new ShirtUpdated(onShirtUpdated));
            PlayerClothing clothing2 = base.player.clothing;
            clothing2.onPantsUpdated = (PantsUpdated)Delegate.Combine(clothing2.onPantsUpdated, new PantsUpdated(onPantsUpdated));
            PlayerClothing clothing3 = base.player.clothing;
            clothing3.onBackpackUpdated = (BackpackUpdated)Delegate.Combine(clothing3.onBackpackUpdated, new BackpackUpdated(onBackpackUpdated));
            PlayerClothing clothing4 = base.player.clothing;
            clothing4.onVestUpdated = (VestUpdated)Delegate.Combine(clothing4.onVestUpdated, new VestUpdated(onVestUpdated));
            for (byte b2 = 0; b2 < PAGES - 1; b2 = (byte)(b2 + 1))
            {
                items[b2].onItemDiscarded = onItemDiscarded;
            }
            load();
        }
    }

    public void load()
    {
        wasLoadCalled = true;
        if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Inventory.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            Block block = PlayerSavedata.readBlock(base.channel.owner.playerID, "/Player/Inventory.dat", 0);
            byte b = block.readByte();
            if (b > 3)
            {
                for (byte b2 = 0; b2 < PAGES - 2; b2 = (byte)(b2 + 1))
                {
                    items[b2].loadSize(block.readByte(), block.readByte());
                    byte b3 = block.readByte();
                    for (byte b4 = 0; b4 < b3; b4 = (byte)(b4 + 1))
                    {
                        byte x = block.readByte();
                        byte y = block.readByte();
                        byte rot = 0;
                        if (b > 4)
                        {
                            rot = block.readByte();
                        }
                        ushort num = block.readUInt16();
                        byte newAmount = block.readByte();
                        byte newQuality = block.readByte();
                        byte[] newState = block.readByteArray();
                        if (Assets.find(EAssetType.ITEM, num) is ItemAsset)
                        {
                            items[b2].loadItem(x, y, rot, new Item(num, newAmount, newQuality, newState));
                        }
                    }
                }
            }
            else
            {
                items[0].loadSize(1, 1);
                items[1].loadSize(1, 1);
                items[2].loadSize(5, 3);
                items[BACKPACK].loadSize(0, 0);
                items[VEST].loadSize(0, 0);
                items[SHIRT].loadSize(0, 0);
                items[PANTS].loadSize(0, 0);
                items[STORAGE].loadSize(0, 0);
                bestowLoadout();
            }
        }
        else
        {
            items[0].loadSize(1, 1);
            items[1].loadSize(1, 1);
            items[2].loadSize(5, 3);
            items[BACKPACK].loadSize(0, 0);
            items[VEST].loadSize(0, 0);
            items[SHIRT].loadSize(0, 0);
            items[PANTS].loadSize(0, 0);
            items[STORAGE].loadSize(0, 0);
            bestowLoadout();
        }
    }

    public void save()
    {
        if (!wasLoadCalled)
        {
            return;
        }
        bool flag = (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Weapons_PvP : Provider.modeConfigData.Players.Lose_Weapons_PvE);
        bool flag2 = (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Clothes_PvP : Provider.modeConfigData.Players.Lose_Clothes_PvE);
        if (base.player.life.isDead && flag && flag2)
        {
            if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Inventory.dat"))
            {
                PlayerSavedata.deleteFile(base.channel.owner.playerID, "/Player/Inventory.dat");
            }
            return;
        }
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        for (byte b = 0; b < PAGES - 2; b = (byte)(b + 1))
        {
            bool flag3 = base.player.life.isDead && ((b >= SLOTS) ? flag2 : flag);
            byte value;
            byte value2;
            byte b2;
            if (items[b] == null || flag3)
            {
                value = 0;
                value2 = 0;
                b2 = 0;
            }
            else
            {
                value = items[b].width;
                value2 = items[b].height;
                b2 = items[b].getItemCount();
            }
            block.writeByte(value);
            block.writeByte(value2);
            block.writeByte(b2);
            for (byte b3 = 0; b3 < b2; b3 = (byte)(b3 + 1))
            {
                ItemJar item = items[b].getItem(b3);
                block.writeByte(item?.x ?? 0);
                block.writeByte(item?.y ?? 0);
                block.writeByte(item?.rot ?? 0);
                block.writeUInt16(item?.item.id ?? 0);
                block.writeByte(item?.item.amount ?? 0);
                block.writeByte(item?.item.quality ?? 0);
                block.writeByteArray((item != null) ? item.item.state : new byte[0]);
            }
        }
        PlayerSavedata.writeBlock(base.channel.owner.playerID, "/Player/Inventory.dat", block);
    }

    [Conditional("LOG_INVENTORY_RPC_FAILURES")]
    private void LogRPCFailure(string format, params object[] args)
    {
        UnturnedLog.warn(format, args);
    }
}
