using UnityEngine;

namespace SDG.Unturned;

public class ExtraItemIconInfo
{
    public string extraPath;

    public void onItemIconReady(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        ReadWrite.writeBytes(extraPath + ".png", useCloud: false, usePath: false, bytes);
        Object.Destroy(texture);
        IconUtils.extraIcons.Remove(this);
    }
}
