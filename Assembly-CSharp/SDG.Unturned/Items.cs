using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class Items
{
    public ItemsResized onItemsResized;

    public ItemUpdated onItemUpdated;

    public ItemAdded onItemAdded;

    public ItemRemoved onItemRemoved;

    public ItemDiscarded onItemDiscarded;

    public StateUpdated onStateUpdated;

    private byte _page;

    private byte _width;

    private byte _height;

    private bool[,] slots;

    public byte page => _page;

    public byte width => _width;

    public byte height => _height;

    public List<ItemJar> items { get; protected set; }

    public void updateAmount(byte index, byte newAmount)
    {
        if (index >= 0 && index < items.Count)
        {
            items[index].item.amount = newAmount;
            if (onItemUpdated != null)
            {
                onItemUpdated(page, index, items[index]);
            }
            if (onStateUpdated != null)
            {
                onStateUpdated();
            }
        }
    }

    public void updateQuality(byte index, byte newQuality)
    {
        if (index >= 0 && index < items.Count)
        {
            items[index].item.quality = newQuality;
            if (onItemUpdated != null)
            {
                onItemUpdated(page, index, items[index]);
            }
            if (onStateUpdated != null)
            {
                onStateUpdated();
            }
        }
    }

    public void updateState(byte index, byte[] newState)
    {
        if (index >= 0 && index < items.Count)
        {
            items[index].item.state = newState;
            if (onItemUpdated != null)
            {
                onItemUpdated(page, index, items[index]);
            }
            if (onStateUpdated != null)
            {
                onStateUpdated();
            }
        }
    }

    public byte getItemCount()
    {
        return (byte)items.Count;
    }

    public bool containsItem(ItemJar jar)
    {
        return items.Contains(jar);
    }

    public ItemJar getItem(byte index)
    {
        if (index < 0 || index >= items.Count)
        {
            return null;
        }
        return items[index];
    }

    public byte getIndex(byte x, byte y)
    {
        if (page < PlayerInventory.SLOTS)
        {
            return 0;
        }
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            return byte.MaxValue;
        }
        for (byte b = 0; b < items.Count; b = (byte)(b + 1))
        {
            if (items[b].x == x && items[b].y == y)
            {
                return b;
            }
        }
        return byte.MaxValue;
    }

    public byte findIndex(byte x, byte y, out byte find_x, out byte find_y)
    {
        find_x = byte.MaxValue;
        find_y = byte.MaxValue;
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            return byte.MaxValue;
        }
        for (byte b = 0; b < items.Count; b = (byte)(b + 1))
        {
            if (items[b].x <= x && items[b].y <= y)
            {
                byte b2 = items[b].size_x;
                byte b3 = items[b].size_y;
                if ((int)items[b].rot % 2 == 1)
                {
                    b2 = items[b].size_y;
                    b3 = items[b].size_x;
                }
                if (items[b].x + b2 > x && items[b].y + b3 > y)
                {
                    find_x = items[b].x;
                    find_y = items[b].y;
                    return b;
                }
            }
        }
        return byte.MaxValue;
    }

    public List<InventorySearch> search(List<InventorySearch> search, EItemType type)
    {
        for (byte b = 0; b < items.Count; b = (byte)(b + 1))
        {
            ItemJar itemJar = items[b];
            if (itemJar.item.amount > 0)
            {
                ItemAsset asset = itemJar.GetAsset();
                if (asset != null && asset.type == type)
                {
                    search.Add(new InventorySearch(page, itemJar));
                }
            }
        }
        return search;
    }

    [Obsolete]
    public List<InventorySearch> search(List<InventorySearch> search, EItemType type, ushort caliber)
    {
        return this.search(search, type, caliber, allowZeroCaliber: true);
    }

    public List<InventorySearch> search(List<InventorySearch> search, EItemType type, ushort caliber, bool allowZeroCaliber)
    {
        for (byte b = 0; b < items.Count; b = (byte)(b + 1))
        {
            ItemJar itemJar = items[b];
            if (itemJar.item.amount > 0)
            {
                bool flag = false;
                for (int i = 0; i < search.Count; i++)
                {
                    if (search[i].page == page && search[i].jar.x == itemJar.x && search[i].jar.y == itemJar.y)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    ItemAsset asset = itemJar.GetAsset();
                    if (asset != null && asset.type == type)
                    {
                        if (((ItemCaliberAsset)asset).calibers.Length == 0)
                        {
                            if (allowZeroCaliber)
                            {
                                search.Add(new InventorySearch(page, itemJar));
                            }
                        }
                        else
                        {
                            for (byte b2 = 0; b2 < ((ItemCaliberAsset)asset).calibers.Length; b2 = (byte)(b2 + 1))
                            {
                                if (((ItemCaliberAsset)asset).calibers[b2] == caliber)
                                {
                                    search.Add(new InventorySearch(page, itemJar));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        return search;
    }

    public List<InventorySearch> search(List<InventorySearch> search, ushort id, bool findEmpty, bool findHealthy)
    {
        for (byte b = 0; b < items.Count; b = (byte)(b + 1))
        {
            ItemJar itemJar = items[b];
            if ((findEmpty || itemJar.item.amount > 0) && (findHealthy || itemJar.item.quality < 100) && itemJar.item.id == id)
            {
                search.Add(new InventorySearch(page, itemJar));
            }
        }
        return search;
    }

    public InventorySearch has(ushort id)
    {
        for (byte b = 0; b < items.Count; b = (byte)(b + 1))
        {
            ItemJar itemJar = items[b];
            if (itemJar.item.amount > 0 && itemJar.item.id == id)
            {
                return new InventorySearch(page, itemJar);
            }
        }
        return null;
    }

    public void loadItem(byte x, byte y, byte rot, Item item)
    {
        ItemJar itemJar = new ItemJar(x, y, rot, item);
        fillSlot(itemJar, isOccupied: true);
        items.Add(itemJar);
    }

    public void addItem(byte x, byte y, byte rot, Item item)
    {
        ItemJar itemJar = new ItemJar(x, y, rot, item);
        fillSlot(itemJar, isOccupied: true);
        items.Add(itemJar);
        try
        {
            if (onItemAdded != null)
            {
                onItemAdded(page, (byte)(items.Count - 1), itemJar);
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Caught exception during addItem (x: {x} y: {y} rot: {rot} item: {item?.id}):");
        }
        if (onStateUpdated != null)
        {
            onStateUpdated();
        }
    }

    public bool tryAddItem(Item item)
    {
        return tryAddItem(item, isStateUpdatable: true);
    }

    public bool tryAddItem(Item item, bool isStateUpdatable)
    {
        if (getItemCount() >= 200)
        {
            return false;
        }
        ItemJar itemJar = new ItemJar(item);
        if (!tryFindSpace(itemJar.size_x, itemJar.size_y, out var x, out var y, out var rot))
        {
            return false;
        }
        itemJar.x = x;
        itemJar.y = y;
        itemJar.rot = rot;
        fillSlot(itemJar, isOccupied: true);
        items.Add(itemJar);
        try
        {
            if (onItemAdded != null)
            {
                onItemAdded(page, (byte)(items.Count - 1), itemJar);
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Caught exception during tryAddItem ({item?.id}):");
        }
        if (isStateUpdatable && onStateUpdated != null)
        {
            onStateUpdated();
        }
        return true;
    }

    public void removeItem(byte index)
    {
        if (index >= 0 && index < items.Count)
        {
            fillSlot(items[index], isOccupied: false);
            if (onItemRemoved != null)
            {
                onItemRemoved(page, index, items[index]);
            }
            items.RemoveAt(index);
            if (onStateUpdated != null)
            {
                onStateUpdated();
            }
        }
    }

    public void clear()
    {
        items.Clear();
    }

    public void loadSize(byte newWidth, byte newHeight)
    {
        _width = newWidth;
        _height = newHeight;
        slots = new bool[width, height];
        for (byte b = 0; b < width; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < height; b2 = (byte)(b2 + 1))
            {
                slots[b, b2] = false;
            }
        }
        List<ItemJar> list = new List<ItemJar>();
        if (items != null)
        {
            for (byte b3 = 0; b3 < items.Count; b3 = (byte)(b3 + 1))
            {
                ItemJar itemJar = items[b3];
                byte b4 = itemJar.size_x;
                byte b5 = itemJar.size_y;
                if ((int)itemJar.rot % 2 == 1)
                {
                    b4 = itemJar.size_y;
                    b5 = itemJar.size_x;
                }
                if (width == 0 || height == 0 || (page >= PlayerInventory.SLOTS && (itemJar.x + b4 > width || itemJar.y + b5 > height)))
                {
                    if (onItemDiscarded != null)
                    {
                        onItemDiscarded(page, b3, itemJar);
                    }
                    if (onStateUpdated != null)
                    {
                        onStateUpdated();
                    }
                }
                else
                {
                    fillSlot(itemJar, isOccupied: true);
                    list.Add(itemJar);
                }
            }
        }
        items = list;
    }

    public void resize(byte newWidth, byte newHeight)
    {
        loadSize(newWidth, newHeight);
        if (onItemsResized != null)
        {
            onItemsResized(page, newWidth, newHeight);
        }
        if (onStateUpdated != null)
        {
            onStateUpdated();
        }
    }

    public bool checkSpaceEmpty(byte pos_x, byte pos_y, byte size_x, byte size_y, byte rot)
    {
        if (page < PlayerInventory.SLOTS)
        {
            return items.Count == 0;
        }
        if ((int)rot % 2 == 1)
        {
            byte num = size_x;
            size_x = size_y;
            size_y = num;
        }
        for (byte b = pos_x; b < pos_x + size_x; b = (byte)(b + 1))
        {
            for (byte b2 = pos_y; b2 < pos_y + size_y; b2 = (byte)(b2 + 1))
            {
                if (b >= width || b2 >= height)
                {
                    return false;
                }
                if (slots[b, b2])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool checkSpaceDrag(byte old_x, byte old_y, byte oldRot, byte new_x, byte new_y, byte newRot, byte size_x, byte size_y, bool checkSame)
    {
        if (page < PlayerInventory.SLOTS)
        {
            return items.Count == 0 || checkSame;
        }
        byte b = size_x;
        byte b2 = size_y;
        if ((int)oldRot % 2 == 1)
        {
            b = size_y;
            b2 = size_x;
        }
        byte b3 = size_x;
        byte b4 = size_y;
        if ((int)newRot % 2 == 1)
        {
            b3 = size_y;
            b4 = size_x;
        }
        for (byte b5 = new_x; b5 < new_x + b3; b5 = (byte)(b5 + 1))
        {
            for (byte b6 = new_y; b6 < new_y + b4; b6 = (byte)(b6 + 1))
            {
                if (b5 >= width || b6 >= height)
                {
                    return false;
                }
                if (slots[b5, b6])
                {
                    int num = b5 - old_x;
                    int num2 = b6 - old_y;
                    if (!checkSame || num < 0 || num2 < 0 || num >= b || num2 >= b2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public bool checkSpaceSwap(byte x, byte y, byte oldSize_X, byte oldSize_Y, byte oldRot, byte newSize_X, byte newSize_Y, byte newRot)
    {
        if (page < PlayerInventory.SLOTS)
        {
            return true;
        }
        if ((int)oldRot % 2 == 1)
        {
            byte num = oldSize_X;
            oldSize_X = oldSize_Y;
            oldSize_Y = num;
        }
        if ((int)newRot % 2 == 1)
        {
            byte num2 = newSize_X;
            newSize_X = newSize_Y;
            newSize_Y = num2;
        }
        for (byte b = x; b < x + newSize_X; b = (byte)(b + 1))
        {
            for (byte b2 = y; b2 < y + newSize_Y; b2 = (byte)(b2 + 1))
            {
                if (b >= width || b2 >= height)
                {
                    return false;
                }
                if (slots[b, b2])
                {
                    int num3 = b - x;
                    int num4 = b2 - y;
                    if (num3 < 0 || num4 < 0 || num3 >= oldSize_X || num4 >= oldSize_Y)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public bool tryFindSpace(byte size_x, byte size_y, out byte x, out byte y, out byte rot)
    {
        x = byte.MaxValue;
        y = byte.MaxValue;
        rot = 0;
        if (page < PlayerInventory.SLOTS)
        {
            x = 0;
            y = 0;
            rot = 0;
            return items.Count == 0;
        }
        for (byte b = 0; b < height - size_y + 1; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < width - size_x + 1; b2 = (byte)(b2 + 1))
            {
                bool flag = false;
                byte b3 = 0;
                while (b3 < size_y && !flag)
                {
                    for (byte b4 = 0; b4 < size_x; b4 = (byte)(b4 + 1))
                    {
                        if (slots[b2 + b4, b + b3])
                        {
                            flag = true;
                            break;
                        }
                        if (b4 == size_x - 1 && b3 == size_y - 1)
                        {
                            x = b2;
                            y = b;
                            rot = 0;
                            return true;
                        }
                    }
                    b3 = (byte)(b3 + 1);
                }
            }
        }
        for (byte b5 = 0; b5 < height - size_x + 1; b5 = (byte)(b5 + 1))
        {
            for (byte b6 = 0; b6 < width - size_y + 1; b6 = (byte)(b6 + 1))
            {
                bool flag2 = false;
                byte b7 = 0;
                while (b7 < size_x && !flag2)
                {
                    for (byte b8 = 0; b8 < size_y; b8 = (byte)(b8 + 1))
                    {
                        if (slots[b6 + b8, b5 + b7])
                        {
                            flag2 = true;
                            break;
                        }
                        if (b8 == size_y - 1 && b7 == size_x - 1)
                        {
                            x = b6;
                            y = b5;
                            rot = 1;
                            return true;
                        }
                    }
                    b7 = (byte)(b7 + 1);
                }
            }
        }
        return false;
    }

    private void fillSlot(ItemJar jar, bool isOccupied)
    {
        byte b = jar.size_x;
        byte b2 = jar.size_y;
        if ((int)jar.rot % 2 == 1)
        {
            b = jar.size_y;
            b2 = jar.size_x;
        }
        for (byte b3 = 0; b3 < b; b3 = (byte)(b3 + 1))
        {
            for (byte b4 = 0; b4 < b2; b4 = (byte)(b4 + 1))
            {
                if (jar.x + b3 < width && jar.y + b4 < height)
                {
                    slots[jar.x + b3, jar.y + b4] = isOccupied;
                }
            }
        }
    }

    public Items(byte newPage)
    {
        _page = newPage;
        items = new List<ItemJar>();
    }
}
