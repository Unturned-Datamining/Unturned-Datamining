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
            foreground.SizeScale_X = state;
            if (suffix.Length == 0)
            {
                label.Text = Mathf.RoundToInt(foreground.SizeScale_X * 100f) + "%";
            }
        }
    }

    public int measure
    {
        set
        {
            if (suffix.Length != 0)
            {
                label.Text = value + suffix;
            }
        }
    }

    public Color color
    {
        get
        {
            return foreground.TintColor;
        }
        set
        {
            Color color = value;
            color.a = 0.5f;
            background.TintColor = color;
            foreground.TintColor = value;
        }
    }

    public SleekProgress(string newSuffix)
    {
        background = Glazier.Get().CreateImage();
        background.SizeScale_X = 1f;
        background.SizeScale_Y = 1f;
        background.Texture = (Texture2D)GlazierResources.PixelTexture;
        AddChild(background);
        foreground = Glazier.Get().CreateImage();
        foreground.SizeScale_X = 1f;
        foreground.SizeScale_Y = 1f;
        foreground.Texture = (Texture2D)GlazierResources.PixelTexture;
        AddChild(foreground);
        label = Glazier.Get().CreateLabel();
        label.SizeScale_X = 1f;
        label.PositionScale_Y = 0.5f;
        label.PositionOffset_Y = -15f;
        label.SizeOffset_Y = 30f;
        label.TextColor = Color.white;
        label.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(label);
        suffix = newSuffix;
    }
}
