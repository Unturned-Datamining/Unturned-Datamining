using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

public struct SleekColor
{
    private ESleekTint tint;

    private Color customColor;

    public SleekColor(ESleekTint tint)
    {
        this.tint = tint;
        customColor = Color.white;
    }

    public SleekColor(ESleekTint tint, float alpha)
    {
        this.tint = tint;
        customColor = Color.white;
        customColor.a = alpha;
    }

    public SleekColor(Color customColor)
    {
        tint = ESleekTint.NONE;
        this.customColor = customColor;
    }

    public SleekColor(Color32 customColor)
    {
        tint = ESleekTint.NONE;
        this.customColor = customColor;
    }

    public static SleekColor BackgroundIfLight(Color customColor)
    {
        return new SleekColor(ESleekTint.BACKGROUND_IF_LIGHT, customColor);
    }

    public static SleekColor BackgroundIfLight(float alpha)
    {
        return new SleekColor(ESleekTint.BACKGROUND_IF_LIGHT, alpha);
    }

    public Color Get()
    {
        switch (tint)
        {
        default:
            return Color.white;
        case ESleekTint.NONE:
            return customColor;
        case ESleekTint.BACKGROUND:
        {
            Color backgroundColor2 = SleekCustomization.backgroundColor;
            backgroundColor2.a = customColor.a;
            return backgroundColor2;
        }
        case ESleekTint.FOREGROUND:
        {
            Color foregroundColor = SleekCustomization.foregroundColor;
            foregroundColor.a = customColor.a;
            return foregroundColor;
        }
        case ESleekTint.FONT:
        {
            Color fontColor = SleekCustomization.fontColor;
            fontColor.a = customColor.a;
            return fontColor;
        }
        case ESleekTint.RICH_TEXT_DEFAULT:
        {
            Color result = new Color32(180, 180, 180, byte.MaxValue);
            result.a = customColor.a;
            return result;
        }
        case ESleekTint.BACKGROUND_IF_LIGHT:
        {
            if (SleekCustomization.darkTheme)
            {
                return customColor;
            }
            Color backgroundColor = SleekCustomization.backgroundColor;
            backgroundColor.a = customColor.a;
            return backgroundColor;
        }
        case ESleekTint.BAD:
        {
            Color badColor = SleekCustomization.badColor;
            badColor.a = customColor.a;
            return badColor;
        }
        }
    }

    public StyleColor GetStyleColor()
    {
        return new StyleColor(Get());
    }

    public void SetAlpha(float alpha)
    {
        customColor.a = alpha;
    }

    public static implicit operator SleekColor(ESleekTint tint)
    {
        return new SleekColor(tint);
    }

    public static implicit operator SleekColor(Color customColor)
    {
        return new SleekColor(customColor);
    }

    public static implicit operator SleekColor(Color32 customColor)
    {
        return new SleekColor(customColor);
    }

    public static implicit operator Color(SleekColor color)
    {
        return color.Get();
    }

    public static implicit operator StyleColor(SleekColor color)
    {
        return color.GetStyleColor();
    }

    private SleekColor(ESleekTint tint, Color customColor)
    {
        this.tint = tint;
        this.customColor = customColor;
    }
}
