using TMPro;
using UnityEngine;

namespace SDG.Unturned;

internal static class GlazierResources_uGUI
{
    private static GlazierTheme_uGUI lightTheme = new GlazierTheme_uGUI("UI/Glazier_uGUI/LightTheme");

    private static GlazierTheme_uGUI darkTheme = new GlazierTheme_uGUI("UI/Glazier_uGUI/DarkTheme");

    public static StaticResourceRef<Material> FontMaterial_Default = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Default");

    public static StaticResourceRef<Material> FontMaterial_Outline = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Outline");

    public static StaticResourceRef<Material> FontMaterial_Shadow = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Shadow");

    public static StaticResourceRef<Material> FontMaterial_Tooltip = new StaticResourceRef<Material>("UI/Glazier_uGUI/Font_Tooltip");

    public static GlazierTheme_uGUI Theme
    {
        get
        {
            if (OptionsSettings.proUI)
            {
                return darkTheme;
            }
            return lightTheme;
        }
    }

    public static StaticResourceRef<Sprite> TooltipShadowSprite { get; private set; } = new StaticResourceRef<Sprite>("UI/Glazier_uGUI/TooltipShadow");


    public static StaticResourceRef<TMP_FontAsset> Font { get; private set; } = new StaticResourceRef<TMP_FontAsset>("UI/Glazier_uGUI/LiberationSans");

}
