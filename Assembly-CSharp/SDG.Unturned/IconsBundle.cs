using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// See Bundles.getIconsBundle.
/// </summary>
public struct IconsBundle
{
    private string path;

    /// <summary>
    /// In practice, T is a Texture2D or a Sprite.
    /// </summary>
    public T load<T>(string name) where T : Object
    {
        T result = null;
        if (Assets.coreMasterBundle != null)
        {
            return Assets.coreMasterBundle.LoadAsset<T>(path + "/" + name + ".png");
        }
        return result;
    }

    public IconsBundle(string path)
    {
        this.path = path;
    }
}
