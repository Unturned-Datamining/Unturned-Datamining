using UnityEngine;

namespace SDG.Unturned;

internal class GlazierTheme_uGUI
{
    public StaticResourceRef<Sprite> BoxSprite { get; private set; }

    public StaticResourceRef<Sprite> BoxHighlightedSprite { get; private set; }

    public Sprite BoxSelectedSprite => BoxHighlightedSprite;

    public StaticResourceRef<Sprite> BoxPressedSprite { get; private set; }

    public StaticResourceRef<Sprite> SliderBackgroundSprite { get; private set; }

    public StaticResourceRef<Sprite> ToggleForegroundSprite { get; private set; }

    public GlazierTheme_uGUI(string prefix)
    {
        BoxSprite = new StaticResourceRef<Sprite>(prefix + "/Box");
        BoxHighlightedSprite = new StaticResourceRef<Sprite>(prefix + "/Box_Highlighted");
        BoxPressedSprite = new StaticResourceRef<Sprite>(prefix + "/Box_Pressed");
        SliderBackgroundSprite = new StaticResourceRef<Sprite>(prefix + "/Slider_Background");
        ToggleForegroundSprite = new StaticResourceRef<Sprite>(prefix + "/Toggle_Foreground");
    }
}
