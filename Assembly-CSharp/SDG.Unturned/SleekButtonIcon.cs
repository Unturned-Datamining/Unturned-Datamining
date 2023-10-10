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
            iconImage.Texture = value;
            if (iconSize == 0 && !iconScale && iconImage.Texture != null)
            {
                iconImage.SizeOffset_X = iconImage.Texture.width;
                iconImage.SizeOffset_Y = iconImage.Texture.height;
                if (label != null)
                {
                    label.PositionOffset_X = iconImage.SizeOffset_X + iconImage.PositionOffset_X * 2f;
                    label.SizeOffset_X = 0f - label.PositionOffset_X - 5f;
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
                return button.Text;
            }
            return label.Text;
        }
        set
        {
            if (label != null)
            {
                label.Text = value;
            }
            else
            {
                button.Text = value;
            }
        }
    }

    public string tooltip
    {
        get
        {
            return button.TooltipText;
        }
        set
        {
            button.TooltipText = value;
        }
    }

    public ESleekFontSize fontSize
    {
        get
        {
            return button.FontSize;
        }
        set
        {
            button.FontSize = value;
            if (label != null)
            {
                label.FontSize = value;
            }
        }
    }

    public ETextContrastContext shadowStyle
    {
        get
        {
            return button.TextContrastContext;
        }
        set
        {
            button.TextContrastContext = value;
            if (label != null)
            {
                label.TextContrastContext = value;
            }
        }
    }

    public SleekColor backgroundColor
    {
        get
        {
            return button.BackgroundColor;
        }
        set
        {
            button.BackgroundColor = value;
        }
    }

    public SleekColor textColor
    {
        get
        {
            return button.TextColor;
        }
        set
        {
            button.TextColor = value;
            if (label != null)
            {
                label.TextColor = value;
            }
        }
    }

    public bool enableRichText
    {
        get
        {
            return button.AllowRichText;
        }
        set
        {
            button.AllowRichText = value;
            if (label != null)
            {
                label.AllowRichText = value;
            }
        }
    }

    public int iconPositionOffset
    {
        set
        {
            iconImage.PositionOffset_X = value;
            iconImage.PositionOffset_Y = value;
            if (label != null)
            {
                label.PositionOffset_X = iconImage.SizeOffset_X + iconImage.PositionOffset_X * 2f;
                label.SizeOffset_X = 0f - label.PositionOffset_X - 5f;
            }
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

    public bool isClickable
    {
        get
        {
            return button.IsClickable;
        }
        set
        {
            button.IsClickable = value;
        }
    }

    public event ClickedButton onClickedButton;

    public event ClickedButton onRightClickedButton;

    public SleekButtonIcon(Texture2D newIcon, int newSize, bool newScale)
    {
        iconSize = newSize;
        iconScale = newScale;
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.BackgroundColor = ESleekTint.BACKGROUND;
        button.OnClicked += onClickedInternalButton;
        button.OnRightClicked += onRightClickedInternalButton;
        AddChild(button);
        iconImage = Glazier.Get().CreateImage();
        iconImage.Texture = newIcon;
        iconPositionOffset = 5;
        if (iconScale)
        {
            iconImage.SizeOffset_X = -10f;
            iconImage.SizeOffset_Y = -10f;
            iconImage.SizeScale_X = 1f;
            iconImage.SizeScale_Y = 1f;
        }
        else
        {
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
        AddChild(iconImage);
        button.TextAlignment = TextAnchor.MiddleCenter;
        button.FontSize = ESleekFontSize.Default;
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
