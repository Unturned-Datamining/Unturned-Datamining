using UnityEngine;

namespace SDG.Unturned;

public class SleekWebImage : SleekWrapper
{
    private ISleekImage internalImage;

    public SleekColor color
    {
        get
        {
            return internalImage.color;
        }
        set
        {
            internalImage.color = value;
        }
    }

    public void Refresh(string url, bool shouldCache = true)
    {
        Provider.refreshIcon(new Provider.IconQueryParams(url, OnImageReady, shouldCache));
    }

    public override void OnDestroy()
    {
        internalImage = null;
    }

    public void Clear()
    {
        internalImage.texture = null;
    }

    public SleekWebImage()
    {
        internalImage = Glazier.Get().CreateImage();
        internalImage.sizeScale_X = 1f;
        internalImage.sizeScale_Y = 1f;
        AddChild(internalImage);
    }

    private void OnImageReady(Texture2D icon, bool responsibleForDestroy)
    {
        if (internalImage != null)
        {
            internalImage.setTextureAndShouldDestroy(icon, responsibleForDestroy);
        }
        else if (responsibleForDestroy)
        {
            Object.Destroy(icon);
        }
    }
}
