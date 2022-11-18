using UnityEngine;

namespace SDG.Unturned;

public class SleekItemIcon : SleekWrapper
{
    private ISleekImage internalImage;

    public byte rot
    {
        set
        {
            internalImage.angle = value * 90;
        }
    }

    public bool isAngled
    {
        set
        {
            internalImage.isAngled = value;
        }
    }

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

    public void Clear()
    {
        internalImage.texture = null;
    }

    public void Refresh(ushort id, byte quality, byte[] state)
    {
        ItemTool.getIcon(id, quality, state, OnIconReady);
    }

    public void Refresh(ushort id, byte quality, byte[] state, ItemAsset itemAsset)
    {
        ItemTool.getIcon(id, quality, state, itemAsset, OnIconReady);
    }

    public void Refresh(ushort id, byte quality, byte[] state, ItemAsset itemAsset, int widthOverride, int heightOverride)
    {
        ItemTool.getIcon(id, quality, state, itemAsset, widthOverride, heightOverride, OnIconReady);
    }

    public override void OnDestroy()
    {
        internalImage = null;
    }

    public SleekItemIcon()
    {
        internalImage = Glazier.Get().CreateImage();
        internalImage.sizeScale_X = 1f;
        internalImage.sizeScale_Y = 1f;
        AddChild(internalImage);
    }

    private void OnIconReady(Texture2D texture)
    {
        if (internalImage != null)
        {
            internalImage.texture = texture;
        }
    }
}
