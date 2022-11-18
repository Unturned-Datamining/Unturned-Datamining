using UnityEngine;

namespace SDG.Unturned;

public class ItemDefIconInfo
{
    public string extraPath;

    private bool hasSmall;

    private bool hasLarge;

    public void onSmallItemIconReady(Texture2D texture)
    {
        hasSmall = true;
        complete();
    }

    public void onLargeItemIconReady(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        UnturnedLog.info(extraPath);
        ReadWrite.writeBytes(extraPath + ".png", useCloud: false, usePath: false, bytes);
        hasLarge = true;
        complete();
    }

    private void complete()
    {
        if (hasSmall && hasLarge)
        {
            IconUtils.icons.Remove(this);
        }
    }
}
