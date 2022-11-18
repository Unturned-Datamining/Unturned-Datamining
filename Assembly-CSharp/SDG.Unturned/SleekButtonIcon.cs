using UnityEngine;

namespace SDG.Unturned;

public class SleekButtonIcon : SleekWrapper
{
    private ISleekButton button;

    private int iconSize;

    private bool iconScale;

    private ISleekImage iconImage;

    private ISleekLabel label;

    public Texture2D icon
    {
        set
        {
            iconImage.texture = value;
            if (iconSize == 0 && !iconScale && iconImage.texture != null)
            {
                iconImage.sizeOffset_X = iconImage.texture.width;
                iconImage.sizeOffset_Y = iconImage.texture.height;
                if (label != null)
                {
                    label.positionOffset_X = iconImage.sizeOffset_X + iconImage.positionOffset_X * 2;
                    label.sizeOffset_X = -label.positionOffset_X - 5;
                }
            }
        }
    }

    public string text
    {
        get
        {
            if (label == null)
            {
                return button.text;
            }
            return label.text;
        }
        set
        {
            if (label != null)
            {
                label.text = value;
            }
            else
            {
                button.text = value;
            }
        }
    }

    public string tooltip
    {
        get
        {
            return button.tooltipText;
        }
        set
        {
            button.tooltipText = value;
        }
    }

    public ESleekFontSize fontSize
    {
        get
        {
            return button.fontSize;
        }
        set
        {
            button.fontSize = value;
            if (label != null)
            {
                label.fontSize = value;
            }
        }
    }

    public ETextContrastContext shadowStyle
    {
        get
        {
            return button.shadowStyle;
        }
        set
        {
            button.shadowStyle = value;
            if (label != null)
            {
                label.shadowStyle = value;
            }
        }
    }

    public SleekColor backgroundColor
    {
        get
        {
            return button.backgroundColor;
        }
        set
        {
            button.backgroundColor = value;
        }
    }

    public SleekColor textColor
    {
        get
        {
            return button.textColor;
        }
        set
        {
            button.textColor = value;
            if (label != null)
            {
                label.textColor = value;
            }
        }
    }

    public bool enableRichText
    {
        get
        {
            return button.enableRichText;
        }
        set
        {
            button.enableRichText = value;
            if (label != null)
            {
                label.enableRichText = value;
            }
        }
    }

    public int iconPositionOffset
    {
        set
        {
            iconImage.positionOffset_X = value;
            iconImage.positionOffset_Y = value;
            if (label != null)
            {
                label.positionOffset_X = iconImage.sizeOffset_X + iconImage.positionOffset_X * 2;
                label.sizeOffset_X = -label.positionOffset_X - 5;
            }
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

    public bool isClickable
    {
        get
        {
            return button.isClickable;
        }
        set
        {
            button.isClickable = value;
        }
    }

    public event ClickedButton onClickedButton;

    public event ClickedButton onRightClickedButton;

    public SleekButtonIcon(Texture2D newIcon, int newSize, bool newScale)
    {
        iconSize = newSize;
        iconScale = newScale;
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.backgroundColor = ESleekTint.BACKGROUND;
        button.onClickedButton += onClickedInternalButton;
        button.onRightClickedButton += onRightClickedInternalButton;
        AddChild(button);
        iconImage = Glazier.Get().CreateImage();
        iconImage.texture = newIcon;
        iconPositionOffset = 5;
        if (iconScale)
        {
            iconImage.sizeOffset_X = -10;
            iconImage.sizeOffset_Y = -10;
            iconImage.sizeScale_X = 1f;
            iconImage.sizeScale_Y = 1f;
        }
        else
        {
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
        AddChild(iconImage);
        button.fontAlignment = TextAnchor.MiddleCenter;
        button.fontSize = ESleekFontSize.Default;
    }

    public SleekButtonIcon(Texture2D newIcon, int newSize)
        : this(newIcon, newSize, newScale: false)
    {
    }

    public SleekButtonIcon(Texture2D newIcon)
        : this(newIcon, 0, newScale: false)
    {
    }

    public SleekButtonIcon(Texture2D newIcon, bool newScale)
        : this(newIcon, 0, newScale)
    {
    }

    private void onClickedInternalButton(ISleekElement internalButton)
    {
        this.onClickedButton?.Invoke(this);
    }

    private void onRightClickedInternalButton(ISleekElement internalButton)
    {
        this.onRightClickedButton?.Invoke(this);
    }
}
