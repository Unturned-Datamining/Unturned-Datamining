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
            iconImage.Texture = value;
            if (iconSize == 0 && iconImage.Texture != null)
            {
                iconImage.SizeOffset_X = iconImage.Texture.width;
                iconImage.SizeOffset_Y = iconImage.Texture.height;
                label.PositionOffset_X = iconImage.SizeOffset_X + iconImage.PositionOffset_X * 2f;
                label.SizeOffset_X = 0f - label.PositionOffset_X - 5f;
            }
        }
    }

    public string text
    {
        get
        {
            return label.Text;
        }
        set
        {
            label.Text = value;
        }
    }

    public string tooltip
    {
        get
        {
            return box.TooltipText;
        }
        set
        {
            box.TooltipText = value;
        }
    }

    public ESleekFontSize fontSize
    {
        get
        {
            return label.FontSize;
        }
        set
        {
            label.FontSize = value;
        }
    }

    public SleekColor iconColor
    {
        get
        {
            return iconImage.TintColor;
        }
        set
        {
            iconImage.TintColor = value;
        }
    }

    public SleekBoxIcon(Texture2D newIcon, int newSize)
    {
        iconSize = newSize;
        box = Glazier.Get().CreateBox();
        box.SizeScale_X = 1f;
        box.SizeScale_Y = 1f;
        AddChild(box);
        iconImage = Glazier.Get().CreateImage();
        iconImage.PositionOffset_X = 5f;
        iconImage.PositionOffset_Y = 5f;
        iconImage.Texture = newIcon;
        AddChild(iconImage);
        if (iconSize == 0)
        {
            if (iconImage.Texture != null)
            {
                iconImage.SizeOffset_X = iconImage.Texture.width;
                iconImage.SizeOffset_Y = iconImage.Texture.height;
            }
        }
        else
        {
            iconImage.SizeOffset_X = iconSize;
            iconImage.SizeOffset_Y = iconSize;
        }
        label = Glazier.Get().CreateLabel();
        label.SizeScale_X = 1f;
        label.SizeScale_Y = 1f;
        label.PositionOffset_X = iconImage.SizeOffset_X + iconImage.PositionOffset_X * 2f;
        label.SizeOffset_X = 0f - label.PositionOffset_X - 5f;
        AddChild(label);
    }

    public SleekBoxIcon(Texture2D newIcon)
        : this(newIcon, 0)
    {
    }
}
