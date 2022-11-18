using UnityEngine;

namespace SDG.Unturned;

public class SleekBoxIcon : SleekWrapper
{
    private ISleekBox box;

    private ISleekImage iconImage;

    private int iconSize;

    private ISleekLabel label;

    public Texture2D icon
    {
        set
        {
            iconImage.texture = value;
            if (iconSize == 0 && iconImage.texture != null)
            {
                iconImage.sizeOffset_X = iconImage.texture.width;
                iconImage.sizeOffset_Y = iconImage.texture.height;
                label.positionOffset_X = iconImage.sizeOffset_X + iconImage.positionOffset_X * 2;
                label.sizeOffset_X = -label.positionOffset_X - 5;
            }
        }
    }

    public string text
    {
        get
        {
            return label.text;
        }
        set
        {
            label.text = value;
        }
    }

    public string tooltip
    {
        get
        {
            return box.tooltipText;
        }
        set
        {
            box.tooltipText = value;
        }
    }

    public ESleekFontSize fontSize
    {
        get
        {
            return label.fontSize;
        }
        set
        {
            label.fontSize = value;
        }
    }

    public SleekColor iconColor
    {
        get
        {
            return iconImage.color;
        }
        set
        {
            iconImage.color = value;
        }
    }

    public SleekBoxIcon(Texture2D newIcon, int newSize)
    {
        iconSize = newSize;
        box = Glazier.Get().CreateBox();
        box.sizeScale_X = 1f;
        box.sizeScale_Y = 1f;
        AddChild(box);
        iconImage = Glazier.Get().CreateImage();
        iconImage.positionOffset_X = 5;
        iconImage.positionOffset_Y = 5;
        iconImage.texture = newIcon;
        AddChild(iconImage);
        if (iconSize == 0)
        {
            if (iconImage.texture != null)
            {
                iconImage.sizeOffset_X = iconImage.texture.width;
                iconImage.sizeOffset_Y = iconImage.texture.height;
            }
        }
        else
        {
            iconImage.sizeOffset_X = iconSize;
            iconImage.sizeOffset_Y = iconSize;
        }
        label = Glazier.Get().CreateLabel();
        label.sizeScale_X = 1f;
        label.sizeScale_Y = 1f;
        label.positionOffset_X = iconImage.sizeOffset_X + iconImage.positionOffset_X * 2;
        label.sizeOffset_X = -label.positionOffset_X - 5;
        AddChild(label);
    }

    public SleekBoxIcon(Texture2D newIcon)
        : this(newIcon, 0)
    {
    }
}
