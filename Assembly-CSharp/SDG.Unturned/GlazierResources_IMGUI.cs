using UnityEngine;

namespace SDG.Unturned;

internal static class GlazierResources_IMGUI
{
    private static StaticResourceRef<GUISkin> lightTheme = new StaticResourceRef<GUISkin>("UI/Glazier_IMGUI/LightTheme");

    private static StaticResourceRef<GUISkin> darkTheme = new StaticResourceRef<GUISkin>("UI/Glazier_IMGUI/DarkTheme");

    public static GUISkin ActiveSkin => OptionsSettings.proUI ? darkTheme : lightTheme;
}
