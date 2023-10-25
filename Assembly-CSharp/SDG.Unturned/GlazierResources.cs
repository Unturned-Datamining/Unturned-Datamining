using UnityEngine;

namespace SDG.Unturned;

internal static class GlazierResources
{
    /// <summary>
    /// White 1x1 texture for solid colored images.
    /// uGUI empty image draws like this, but we need the texture for IMGUI backwards compatibility.
    /// </summary>
    public static StaticResourceRef<Texture2D> PixelTexture { get; private set; } = new StaticResourceRef<Texture2D>("Materials/Pixel");

}
