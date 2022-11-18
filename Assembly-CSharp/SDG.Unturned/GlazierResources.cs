using UnityEngine;

namespace SDG.Unturned;

internal static class GlazierResources
{
    public static StaticResourceRef<Texture2D> PixelTexture { get; private set; } = new StaticResourceRef<Texture2D>("Materials/Pixel");

}
