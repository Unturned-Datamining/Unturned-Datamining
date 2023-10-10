using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class SleekItems : SleekWrapper
{
    public SelectedItem onSelectedItem;

    public GrabbedItem onGrabbedItem;

    public PlacedItem onPlacedItem;

    private ISleekElement itemsPanel;

    private ISleekSprite grid;

    private ISleekScrollView horizontalScrollView;

    private byte _page;

    private byte _width;

    private byte _height;

    private List<SleekItem> _items;

    private List<ItemJar> pendingItems;

    private bool _areItemsEnabled = true;

    public byte page => _page;

    public byte width => _width;

    public byte height => _height;

    public List<SleekItem> items => _items;

    public bool areItemsEnabled
    {
        get
        {
            return _areItemsEnabled;
        }
        set
        {
            _areItemsEnabled = value;
            foreach (SleekItem item in _items)
            {
                item.setEnabled(_areItemsEnabled);
            }
        }
    }

    public bool isGridRaycastTarget
    {
        get
        {
            return grid.IsRaycastTarget;
        }
        set
        {
            grid.IsRaycastTarget = value;
        }
    }

    public void resetHotkeyDisplay()
    {
        foreach (SleekItem item in _items)
        {
            if (item.hotkey != 255)
            {
                item.updateHotkey(byte.MaxValue);
            }
        }
    }

    public void updateHotkey(ItemJar jar, byte button)
    {
        int num = indexOfItemElement(jar);
        if (num >= 0)
        {
            items[num].updateHotkey(button);
            return;
        }
        int num2 = pendingItems.IndexOf(jar);
        if (num2 >= 0)
        {
            pendingItems.RemoveAtFast(num2);
            createElementForItem(jar).updateHotkey(button);
        }
    }

    public void resize(byte newWidth, byte newHeight)
    {
        _width = newWidth;
        _height = newHeight;
        horizontalScrollView.ContentSizeOffset = new Vector2((float)(int)width * 50f, (float)(int)height * 50f);
        base.SizeOffset_Y = height * 50 + 30;
        grid.TileRepeatHintForUITK = new Vector2Int(newWidth, newHeight);
    }

    public void clear()
    {
        items.Clear();
        itemsPanel.RemoveAllChildren();
        pendingItems.Clear();
    }

    public void updateItem(ItemJar jar)
    {
        int num = indexOfItemElement(jar);
        if (num >= 0)
        {
            items[num].updateItem(jar);
        }
    }

    public void addItem(ItemJar jar)
    {
        pendingItems.Add(jar);
    }

    public void removeItem(ItemJar jar)
    {
        int num = indexOfItemElement(jar);
        if (num >= 0)
        {
            itemsPanel.RemoveChild(items[num]);
            items.RemoveAtFast(num);
        }
        else
        {
            pendingItems.RemoveFast(jar);
        }
    }

    public override void OnUpdate()
    {
        int num = Mathf.Max(0, pendingItems.Count - 5);
        for (int num2 = pendingItems.Count - 1; num2 >= num; num2--)
        {
            ItemJar jar = pendingItems[num2];
            pendingItems.RemoveAt(num2);
            createElementForItem(jar);
        }
    }

    private int indexOfItemElement(ItemJar jar)
    {
        int num = jar.x * 50;
        int num2 = jar.y * 50;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].PositionOffset_X == (float)num && items[i].PositionOffset_Y == (float)num2)
            {
                return i;
            }
        }
        return -1;
    }

    private SleekItem createElementForItem(ItemJar jar)
    {
        SleekItem sleekItem = new SleekItem(jar);
        sleekItem.PositionOffset_X = jar.x * 50;
        sleekItem.PositionOffset_Y = jar.y * 50;
        sleekItem.onClickedItem = onClickedItem;
        sleekItem.onDraggedItem = onDraggedItem;
        itemsPanel.AddChild(sleekItem);
        sleekItem.setEnabled(_areItemsEnabled);
        items.Add(sleekItem);
        return sleekItem;
    }

    private void onClickedItem(SleekItem item)
    {
        onSelectedItem?.Invoke(page, (byte)(item.PositionOffset_X / 50f), (byte)(item.PositionOffset_Y / 50f));
    }

    private void onDraggedItem(SleekItem item)
    {
        onGrabbedItem?.Invoke(page, (byte)(item.PositionOffset_X / 50f), (byte)(item.PositionOffset_Y / 50f), item);
    }

    private void onClickedGrid()
    {
        Vector2 normalizedCursorPosition = grid.GetNormalizedCursorPosition();
        byte x = (byte)(normalizedCursorPosition.x * (float)(int)width);
        byte y = (byte)(normalizedCursorPosition.y * (float)(int)height);
        onPlacedItem?.Invoke(page, x, y);
    }

    public SleekItems(byte newPage)
    {
        _page = newPage;
        _items = new List<SleekItem>();
        pendingItems = new List<ItemJar>();
        base.SizeScale_X = 1f;
        horizontalScrollView = Glazier.Get().CreateScrollView();
        horizontalScrollView.SizeScale_X = 1f;
        horizontalScrollView.SizeScale_Y = 1f;
        horizontalScrollView.HandleScrollWheel = false;
        AddChild(horizontalScrollView);
        grid = Glazier.Get().CreateSprite();
        grid.SizeScale_X = 1f;
        grid.SizeScale_Y = 1f;
        grid.Sprite = PlayerDashboardInventoryUI.icons.load<Sprite>("Grid_Sprite");
        grid.OnClicked += onClickedGrid;
        grid.TintColor = ESleekTint.FOREGROUND;
        horizontalScrollView.AddChild(grid);
        itemsPanel = Glazier.Get().CreateFrame();
        itemsPanel.SizeScale_X = 1f;
        itemsPanel.SizeScale_Y = 1f;
        grid.AddChild(itemsPanel);
    }
}
