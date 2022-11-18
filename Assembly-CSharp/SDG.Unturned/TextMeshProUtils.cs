using TMPro;

namespace SDG.Unturned;

public static class TextMeshProUtils
{
    public const string DefaultFontName = "LiberationSans SDF";

    public static StaticResourceRef<TMP_FontAsset> DefaultFont { get; private set; } = new StaticResourceRef<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");


    public static void FixupFont(TextMeshPro component)
    {
        if (component.font == null || component.font.name.Equals("LiberationSans SDF"))
        {
            component.font = DefaultFont;
        }
    }

    public static void FixupFont(TextMeshProUGUI component)
    {
        if (component.font == null || component.font.name.Equals("LiberationSans SDF"))
        {
            component.font = DefaultFont;
        }
    }

    public static void FixupFont(TMP_InputField component)
    {
        if (component.fontAsset == null || component.fontAsset.name.Equals("LiberationSans SDF"))
        {
            component.fontAsset = DefaultFont;
        }
    }
}
