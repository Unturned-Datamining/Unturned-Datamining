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
            return image.isRaycastTarget;
        }
        set
        {
            image.isRaycastTarget = value;
        }
    }

    public void select()
    {
        if (onPlacedItem != null)
        {
            onPlacedItem(page, 0, 0);
        }
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
            item.positionOffset_X = -jar.size_x * 25;
            item.positionOffset_Y = -jar.size_y * 25;
            item.positionScale_X = 0.5f;
            item.positionScale_Y = 0.5f;
            item.updateHotkey(page);
            item.onClickedItem = onClickedItem;
            item.onDraggedItem = onDraggedItem;
            item.setEnabled(_isItemEnabled);
            image.AddChild(item);
        }
    }

    private void onClickedItem(SleekItem item)
    {
        if (onSelectedItem != null)
        {
            onSelectedItem(page, 0, 0);
        }
    }

    private void onDraggedItem(SleekItem item)
    {
        if (onGrabbedItem != null)
        {
            onGrabbedItem(page, 0, 0, item);
        }
    }

    public SleekSlot(byte newPage)
    {
        _page = newPage;
        base.sizeOffset_X = 250;
        base.sizeOffset_Y = 150;
        image = Glazier.Get().CreateSprite();
        image.sizeScale_X = 1f;
        image.sizeScale_Y = 1f;
        image.drawMethod = ESleekSpriteType.Sliced;
        image.sprite = PlayerDashboardInventoryUI.icons.load<Sprite>("Slot_Sprite");
        image.color = ESleekTint.FOREGROUND;
        image.onImageClicked += select;
        AddChild(image);
    }
}
