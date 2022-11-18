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
            return grid.isRaycastTarget;
        }
        set
        {
            grid.isRaycastTarget = value;
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
        horizontalScrollView.contentSizeOffset = new Vector2((float)(int)width * 50f, (float)(int)height * 50f);
        base.sizeOffset_Y = height * 50 + 30;
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
            if (items[i].positionOffset_X == num && items[i].positionOffset_Y == num2)
            {
                return i;
            }
        }
        return -1;
    }

    private SleekItem createElementForItem(ItemJar jar)
    {
        SleekItem sleekItem = new SleekItem(jar);
        sleekItem.positionOffset_X = jar.x * 50;
        sleekItem.positionOffset_Y = jar.y * 50;
        sleekItem.onClickedItem = onClickedItem;
        sleekItem.onDraggedItem = onDraggedItem;
        itemsPanel.AddChild(sleekItem);
        sleekItem.setEnabled(_areItemsEnabled);
        items.Add(sleekItem);
        return sleekItem;
    }

    private void onClickedItem(SleekItem item)
    {
        if (onSelectedItem != null)
        {
            onSelectedItem(page, (byte)(item.positionOffset_X / 50), (byte)(item.positionOffset_Y / 50));
        }
    }

    private void onDraggedItem(SleekItem item)
    {
        if (onGrabbedItem != null)
        {
            onGrabbedItem(page, (byte)(item.positionOffset_X / 50), (byte)(item.positionOffset_Y / 50), item);
        }
    }

    private void onClickedGrid()
    {
        Vector2 normalizedCursorPosition = grid.GetNormalizedCursorPosition();
        byte x = (byte)(normalizedCursorPosition.x * (float)(int)width);
        byte y = (byte)(normalizedCursorPosition.y * (float)(int)height);
        if (onPlacedItem != null)
        {
            onPlacedItem(page, x, y);
        }
    }

    public SleekItems(byte newPage)
    {
        _page = newPage;
        _items = new List<SleekItem>();
        pendingItems = new List<ItemJar>();
        base.sizeScale_X = 1f;
        horizontalScrollView = Glazier.Get().CreateScrollView();
        horizontalScrollView.sizeScale_X = 1f;
        horizontalScrollView.sizeScale_Y = 1f;
        horizontalScrollView.handleScrollWheel = false;
        AddChild(horizontalScrollView);
        grid = Glazier.Get().CreateSprite();
        grid.sizeScale_X = 1f;
        grid.sizeScale_Y = 1f;
        grid.sprite = PlayerDashboardInventoryUI.icons.load<Sprite>("Grid_Sprite");
        grid.onImageClicked += onClickedGrid;
        grid.color = ESleekTint.FOREGROUND;
        horizontalScrollView.AddChild(grid);
        itemsPanel = Glazier.Get().CreateFrame();
        itemsPanel.sizeScale_X = 1f;
        itemsPanel.sizeScale_Y = 1f;
        grid.AddChild(itemsPanel);
    }
}
