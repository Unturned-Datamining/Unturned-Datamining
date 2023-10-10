using UnityEngine;

namespace SDG.Unturned;

public class SleekSlot : SleekWrapper
{
    public SelectedItem onSelectedItem;

    public GrabbedItem onGrabbedItem;

    public PlacedItem onPlacedItem;

    private ISleekSprite image;

    private SleekItem _item;

    private byte _page;

    private bool _isItemEnabled = true;

    public SleekItem item => _item;

    public byte page => _page;

    public bool isItemEnabled
    {
        get
        {
            return _isItemEnabled;
        }
        set
        {
            _isItemEnabled = value;
            if (item != null)
            {
                item.setEnabled(value);
            }
        }
    }

    public bool isImageRaycastTarget
    {
        get
        {
            return image.IsRaycastTarget;
        }
        set
        {
            image.IsRaycastTarget = value;
        }
    }

    public void select()
    {
        onPlacedItem?.Invoke(page, 0, 0);
    }

    public void updateItem(ItemJar jar)
    {
        if (item != null)
        {
            item.updateItem(jar);
        }
    }

    public void applyItem(ItemJar jar)
    {
        if (item != null)
        {
            image.RemoveChild(item);
            _item = null;
        }
        if (jar != null)
        {
            _item = new SleekItem(jar);
            item.PositionOffset_X = -jar.size_x * 25;
            item.PositionOffset_Y = -jar.size_y * 25;
            item.PositionScale_X = 0.5f;
            item.PositionScale_Y = 0.5f;
            item.updateHotkey(page);
            item.onClickedItem = onClickedItem;
            item.onDraggedItem = onDraggedItem;
            item.setEnabled(_isItemEnabled);
            image.AddChild(item);
        }
    }

    private void onClickedItem(SleekItem item)
    {
        onSelectedItem?.Invoke(page, 0, 0);
    }

    private void onDraggedItem(SleekItem item)
    {
        onGrabbedItem?.Invoke(page, 0, 0, item);
    }

    public SleekSlot(byte newPage)
    {
        _page = newPage;
        base.SizeOffset_X = 250f;
        base.SizeOffset_Y = 150f;
        image = Glazier.Get().CreateSprite();
        image.SizeScale_X = 1f;
        image.SizeScale_Y = 1f;
        image.DrawMethod = ESleekSpriteType.Sliced;
        image.Sprite = PlayerDashboardInventoryUI.icons.load<Sprite>("Slot_Sprite");
        image.TintColor = ESleekTint.FOREGROUND;
        image.OnClicked += select;
        AddChild(image);
    }
}
