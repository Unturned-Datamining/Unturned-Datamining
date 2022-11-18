using UnityEngine;

namespace SDG.Unturned;

public class SleekProgress : SleekWrapper
{
    private ISleekImage background;

    private ISleekImage foreground;

    private ISleekLabel label;

    public string suffix;

    private float _state;

    public float state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = Mathf.Clamp01(value);
            foreground.sizeScale_X = state;
            if (suffix.Length == 0)
            {
                label.text = Mathf.RoundToInt(foreground.sizeScale_X * 100f) + "%";
            }
        }
    }

    public int measure
    {
        set
        {
            if (suffix.Length != 0)
            {
                label.text = value + suffix;
            }
        }
    }

    public Color color
    {
        get
        {
            return foreground.color;
        }
        set
        {
            Color color = value;
            color.a = 0.5f;
            background.color = color;
            foreground.color = value;
        }
    }

    public SleekProgress(string newSuffix)
    {
        background = Glazier.Get().CreateImage();
        background.sizeScale_X = 1f;
        background.sizeScale_Y = 1f;
        background.texture = (Texture2D)GlazierResources.PixelTexture;
        AddChild(background);
        foreground = Glazier.Get().CreateImage();
        foreground.sizeScale_X = 1f;
        foreground.sizeScale_Y = 1f;
        foreground.texture = (Texture2D)GlazierResources.PixelTexture;
        AddChild(foreground);
        label = Glazier.Get().CreateLabel();
        label.sizeScale_X = 1f;
        label.positionScale_Y = 0.5f;
        label.positionOffset_Y = -15;
        label.sizeOffset_Y = 30;
        label.textColor = Color.white;
        label.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        AddChild(label);
        suffix = newSuffix;
    }
}
