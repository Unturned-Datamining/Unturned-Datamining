using UnityEngine.UIElements;

namespace SDG.Unturned;

internal static class GlazierResources_UIToolkit
{
    public static ThemeStyleSheet Theme
    {
        get
        {
            if (OptionsSettings.proUI)
            {
                return DarkTheme;
            }
            return LightTheme;
        }
    }

    public static StaticResourceRef<ThemeStyleSheet> LightTheme { get; private set; } = new StaticResourceRef<ThemeStyleSheet>("UI/Glazier_UIToolkit/UnturnedLightTheme");


    public static StaticResourceRef<ThemeStyleSheet> DarkTheme { get; private set; } = new StaticResourceRef<ThemeStyleSheet>("UI/Glazier_UIToolkit/UnturnedDarkTheme");

}
