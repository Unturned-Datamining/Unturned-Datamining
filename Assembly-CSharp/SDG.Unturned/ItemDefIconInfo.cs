using UnityEngine;

namespace SDG.Unturned;

public class ItemDefIconInfo
{
    /// <summary>
    /// Icon saved for community members in Extras folder.
    /// </summary>
    public string extraPath;

    /// <summary>
    /// Has the small icon been captured yet?
    /// </summary>
    private bool hasSmall;

    /// <summary>
    /// Has the large icon been captured yet?
    /// </summary>
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
