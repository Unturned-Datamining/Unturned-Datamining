using UnityEngine;

namespace SDG.Unturned;

public class BlueprintItemIconsInfo
{
    public Texture2D[] textures;

    public BlueprintItemIconsReady callback;

    private int index;

    public void onItemIconReady(Texture2D texture)
    {
        if (index < textures.Length)
        {
            textures[index] = texture;
            index++;
            if (index == textures.Length)
            {
                callback?.Invoke();
            }
        }
    }
}
